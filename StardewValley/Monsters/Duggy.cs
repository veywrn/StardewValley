using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Monsters
{
	public class Duggy : Monster
	{
		private double chanceToDisappear = 0.03;

		public Duggy()
		{
			base.HideShadow = true;
		}

		public Duggy(Vector2 position)
			: base("Duggy", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.IsInvisible = true;
			base.DamageToFarmer = 0;
			Sprite.currentFrame = 0;
			base.HideShadow = true;
		}

		public Duggy(Vector2 position, bool magmaDuggy)
			: base("Magma Duggy", position)
		{
			base.IsWalkingTowardPlayer = false;
			base.IsInvisible = true;
			base.DamageToFarmer = 0;
			Sprite.currentFrame = 0;
			base.HideShadow = true;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			position.Field.Interpolated(interpolate: false, wait: true);
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				base.Health -= actualDamage;
				base.currentLocation.playSound("hitEnemy");
				if (base.Health <= 0)
				{
					deathAnimation();
				}
			}
			return actualDamage;
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

		public override void update(GameTime time, GameLocation location)
		{
			if (invincibleCountdown > 0)
			{
				glowingColor = Color.Cyan;
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (location.farmers.Any())
			{
				behaviorAtGameTick(time);
				if (base.Position.X < 0f || base.Position.X > (float)(location.map.GetLayer("Back").LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(location.map.GetLayer("Back").LayerHeight * 64))
				{
					location.characters.Remove(this);
				}
				updateGlow();
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2 + yJumpOffset), Sprite.SourceRect, Color.White, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, GetBoundingBox().Height / 2 + yJumpOffset), Sprite.SourceRect, glowingColor * glowingTransparency, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			isEmoting = false;
			Sprite.loop = false;
			Rectangle r = GetBoundingBox();
			if (Sprite.currentFrame < 4)
			{
				r.Inflate(128, 128);
				if (!base.IsInvisible || r.Contains(base.Player.getStandingX(), base.Player.getStandingY()))
				{
					if (base.IsInvisible)
					{
						if (base.currentLocation.map.GetLayer("Back").Tiles[(int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y].Properties.ContainsKey("NPCBarrier") || (!base.currentLocation.map.GetLayer("Back").Tiles[(int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y].TileIndexProperties.ContainsKey("Diggable") && base.currentLocation.map.GetLayer("Back").Tiles[(int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y].TileIndex != 0))
						{
							return;
						}
						base.Position = new Vector2(base.Player.Position.X, base.Player.Position.Y + (float)base.Player.Sprite.SpriteHeight - (float)Sprite.SpriteHeight);
						base.currentLocation.localSound("Duggy");
						base.Position = base.Player.getTileLocation() * 64f;
					}
					base.IsInvisible = false;
					Sprite.interval = 100f;
					Sprite.AnimateDown(time);
				}
			}
			if (Sprite.currentFrame >= 4 && Sprite.currentFrame < 8)
			{
				r.Inflate(-128, -128);
				base.currentLocation.isCollidingPosition(r, Game1.viewport, isFarmer: false, 8, glider: false, this);
				Sprite.AnimateRight(time);
				Sprite.interval = 220f;
				base.DamageToFarmer = 8;
			}
			if (Sprite.currentFrame >= 8)
			{
				Sprite.AnimateUp(time);
			}
			if (Sprite.currentFrame >= 10)
			{
				base.IsInvisible = true;
				Sprite.currentFrame = 0;
				Vector2 tile = getTileLocation();
				base.currentLocation.map.GetLayer("Back").Tiles[(int)tile.X, (int)tile.Y].TileIndex = 0;
				base.currentLocation.removeEverythingExceptCharactersFromThisTile((int)tile.X, (int)tile.Y);
				base.DamageToFarmer = 0;
			}
		}
	}
}
