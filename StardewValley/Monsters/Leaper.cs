using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Locations;
using System;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	public class Leaper : Monster
	{
		public NetFloat leapDuration = new NetFloat(0.75f);

		public NetFloat leapProgress = new NetFloat(0f);

		public NetBool leaping = new NetBool(value: false);

		public NetVector2 leapStartPosition = new NetVector2();

		public NetVector2 leapEndPosition = new NetVector2();

		public float nextLeap;

		public Leaper()
		{
		}

		public Leaper(Vector2 position)
			: base("Spider", position)
		{
			forceOneTileWide.Value = true;
			base.IsWalkingTowardPlayer = false;
			nextLeap = Utility.RandomFloat(1f, 1.5f);
			isHardModeMonster.Value = true;
			reloadSprite();
		}

		public override int GetBaseDifficultyLevel()
		{
			return 1;
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(leapDuration, leapProgress, leapStartPosition, leapEndPosition, leaping);
			leapProgress.Interpolated(interpolate: true, wait: true);
			leaping.Interpolated(interpolate: true, wait: true);
			leaping.fieldChangeVisibleEvent += OnLeapingChanged;
		}

		public virtual void OnLeapingChanged(NetBool field, bool old_value, bool new_value)
		{
		}

		public override bool isInvincible()
		{
			if (leaping.Value)
			{
				return true;
			}
			return base.isInvincible();
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.localSound("monsterdead");
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.DarkRed, 10)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation);
		}

		protected override void sharedDeathAnimation()
		{
		}

		public override void defaultMovementBehavior(GameTime time)
		{
		}

		public override void noMovementProgressNearPlayerBehavior()
		{
		}

		public override void update(GameTime time, GameLocation location)
		{
			farmerPassesThrough = true;
			base.update(time, location);
			if (leaping.Value)
			{
				yJumpGravity = 0f;
				float progress = leapProgress.Value;
				if (!Game1.IsMasterGame)
				{
					float total_length = (leapStartPosition.Value - leapEndPosition).Length();
					progress = ((total_length != 0f) ? ((leapStartPosition.Value - base.Position).Length() / total_length) : 0f);
					if (progress < 0f)
					{
						progress = 0f;
					}
					if (progress > 1f)
					{
						progress = 1f;
					}
				}
				yJumpOffset = (int)(Math.Sin((double)progress * Math.PI) * -64.0 * 3.0);
			}
			else
			{
				yJumpOffset = 0;
			}
		}

		protected override void updateAnimation(GameTime time)
		{
			if ((bool)leaping)
			{
				Sprite.CurrentFrame = 2;
			}
			else
			{
				Sprite.Animate(time, 0, 2, 500f);
			}
			Sprite.UpdateSourceRect();
		}

		public virtual bool IsValidLandingTile(Vector2 tile, bool check_other_characters = false)
		{
			if (base.currentLocation is MineShaft && !(base.currentLocation as MineShaft).isTileOnClearAndSolidGround(tile))
			{
				return false;
			}
			if (base.currentLocation.isTileOccupied(tile, "", ignoreAllCharacters: true) || !base.currentLocation.isTileOnMap(tile) || !base.currentLocation.isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport))
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle my_bounding_box = GetBoundingBox();
			if (check_other_characters && base.currentLocation != null)
			{
				foreach (NPC character in base.currentLocation.characters)
				{
					if (character != this && character.GetBoundingBox().Intersects(my_bounding_box))
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (leaping.Value)
			{
				leapProgress.Value += (float)time.ElapsedGameTime.TotalSeconds / leapDuration.Value;
				if (leapProgress.Value >= 1f)
				{
					leapProgress.Value = 1f;
				}
				base.Position = new Vector2(Utility.Lerp(leapStartPosition.X, leapEndPosition.X, leapProgress), Utility.Lerp(leapStartPosition.Y, leapEndPosition.Y, leapProgress));
				if (leapProgress.Value == 1f)
				{
					leaping.Value = false;
					leapProgress.Value = 0f;
					if (!IsValidLandingTile(getTileLocation(), check_other_characters: true))
					{
						nextLeap = 0.1f;
					}
				}
				return;
			}
			if (nextLeap > 0f)
			{
				nextLeap -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (!(nextLeap <= 0f))
			{
				return;
			}
			Vector2? found_tile = null;
			Vector2 current_tile = getTileLocation();
			current_tile.X = (int)current_tile.X;
			current_tile.X = (int)current_tile.X;
			if (withinPlayerThreshold(5) && base.Player != null)
			{
				Vector2 target_tile = getTileLocation();
				if (Game1.random.NextDouble() < 0.60000002384185791)
				{
					nextLeap = Utility.RandomFloat(1.25f, 1.5f);
					target_tile = base.Player.getTileLocation();
					target_tile.X = (int)Math.Round(target_tile.X);
					target_tile.Y = (int)Math.Round(target_tile.Y);
					target_tile.X += Game1.random.Next(-1, 2);
					target_tile.Y += Game1.random.Next(-1, 2);
				}
				else
				{
					nextLeap = Utility.RandomFloat(0.1f, 0.2f);
					target_tile.X += Game1.random.Next(-1, 2);
					target_tile.Y += Game1.random.Next(-1, 2);
				}
				if (IsValidLandingTile(target_tile))
				{
					found_tile = target_tile;
				}
			}
			if (!found_tile.HasValue)
			{
				for (int i = 0; i < 8; i++)
				{
					Vector2 offset = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
					if (!(offset == Vector2.Zero))
					{
						Vector2 tile = current_tile + offset;
						if (IsValidLandingTile(tile))
						{
							nextLeap = Utility.RandomFloat(0.6f, 1.5f);
							found_tile = tile;
							break;
						}
					}
				}
			}
			if (found_tile.HasValue)
			{
				if (Utility.isOnScreen(base.Position, 128))
				{
					base.currentLocation.playSound("batFlap");
				}
				leapProgress.Value = 0f;
				leaping.Value = true;
				leapStartPosition.Value = base.Position;
				leapEndPosition.Value = found_tile.Value * 64f;
			}
			else
			{
				nextLeap = Utility.RandomFloat(0.25f, 0.5f);
			}
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, 64, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f);
		}
	}
}
