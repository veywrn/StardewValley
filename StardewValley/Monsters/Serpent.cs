using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewValley.Monsters
{
	public class Serpent : Monster
	{
		public const float rotationIncrement = (float)Math.PI / 64f;

		private int wasHitCounter;

		private float targetRotation;

		private bool turningRight;

		private readonly NetFarmerRef killer = new NetFarmerRef().Delayed(interpolationWait: false);

		public List<Vector3> segments = new List<Vector3>();

		public NetInt segmentCount = new NetInt(0);

		public Serpent()
		{
		}

		public Serpent(Vector2 position)
			: base("Serpent", position)
		{
			InitializeAttributes();
		}

		public Serpent(Vector2 position, string name)
			: base(name, position)
		{
			InitializeAttributes();
			if (name == "Royal Serpent")
			{
				segmentCount.Value = Game1.random.Next(3, 7);
				if (Game1.random.NextDouble() < 0.1)
				{
					segmentCount.Value = Game1.random.Next(5, 10);
				}
				else if (Game1.random.NextDouble() < 0.01)
				{
					segmentCount.Value *= 3;
				}
				reloadSprite();
				base.MaxHealth += segmentCount.Value * 50;
				base.Health = base.MaxHealth;
			}
		}

		public virtual void InitializeAttributes()
		{
			base.Slipperiness = 24 + Game1.random.Next(10);
			Halt();
			base.IsWalkingTowardPlayer = false;
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			base.Scale = 0.75f;
			base.HideShadow = true;
		}

		public bool IsRoyalSerpent()
		{
			return segmentCount.Value > 1;
		}

		public override bool TakesDamageFromHitbox(Rectangle area_of_effect)
		{
			if (base.TakesDamageFromHitbox(area_of_effect))
			{
				return true;
			}
			if (IsRoyalSerpent())
			{
				Rectangle bounding_box = GetBoundingBox();
				Vector2 offset = new Vector2((float)bounding_box.X - base.Position.X, (float)bounding_box.Y - base.Position.Y);
				foreach (Vector3 segment in segments)
				{
					bounding_box.X = (int)(segment.X + offset.X);
					bounding_box.Y = (int)(segment.Y + offset.Y);
					if (bounding_box.Intersects(area_of_effect))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override bool OverlapsFarmerForDamage(Farmer who)
		{
			if (base.OverlapsFarmerForDamage(who))
			{
				return true;
			}
			if (IsRoyalSerpent())
			{
				Rectangle farmer_box = who.GetBoundingBox();
				Rectangle bounding_box = GetBoundingBox();
				Vector2 offset = new Vector2((float)bounding_box.X - base.Position.X, (float)bounding_box.Y - base.Position.Y);
				foreach (Vector3 segment in segments)
				{
					bounding_box.X = (int)(segment.X + offset.X);
					bounding_box.Y = (int)(segment.Y + offset.Y);
					if (bounding_box.Intersects(farmer_box))
					{
						return true;
					}
				}
			}
			return false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(killer.NetFields, segmentCount);
			segmentCount.fieldChangeVisibleEvent += delegate(NetInt field, int old_value, int new_value)
			{
				if (new_value > 0)
				{
					reloadSprite();
				}
			};
		}

		public override void reloadSprite()
		{
			if (IsRoyalSerpent())
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Royal Serpent");
				base.Scale = 1f;
			}
			else
			{
				Sprite = new AnimatedSprite("Characters\\Monsters\\Serpent");
				base.Scale = 0.75f;
			}
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			base.HideShadow = true;
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
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				wasHitCounter = 500;
				base.currentLocation.playSound("serpentHit");
				if (base.Health <= 0)
				{
					killer.Value = who;
					deathAnimation();
				}
			}
			base.addedSpeed = Game1.random.Next(-1, 1);
			return actualDamage;
		}

		protected override void sharedDeathAnimation()
		{
		}

		protected override void localDeathAnimation()
		{
			if (killer.Value == null)
			{
				return;
			}
			Rectangle bb = GetBoundingBox();
			bb.Inflate(-bb.Width / 2 + 1, -bb.Height / 2 + 1);
			Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(bb.Center, 4f, killer.Value);
			int xTrajectory = -(int)velocityTowardPlayer.X;
			int yTrajectory = -(int)velocityTowardPlayer.Y;
			if (IsRoyalSerpent())
			{
				base.currentLocation.localSound("serpentDie");
				for (int i = -1; i < segments.Count; i++)
				{
					Vector2 segment_position2 = Vector2.Zero;
					Rectangle source_rect2 = new Rectangle(0, 0, 32, 32);
					float current_rotation = rotation;
					float color_fade = 0f;
					if (i == -1)
					{
						segment_position2 = base.Position;
						source_rect2 = new Rectangle(0, 64, 32, 32);
					}
					else
					{
						if (segments.Count <= 0 || i >= segments.Count)
						{
							break;
						}
						color_fade = (float)(i + 1) / (float)segments.Count;
						segment_position2 = new Vector2(segments[i].X, segments[i].Y);
						bb.X = (int)(segment_position2.X - (float)(bb.Width / 2));
						bb.Y = (int)(segment_position2.Y - (float)(bb.Height / 2));
						source_rect2 = new Rectangle(32, 64, 32, 32);
						if (i == segments.Count - 1)
						{
							source_rect2 = new Rectangle(64, 64, 32, 32);
						}
						current_rotation = segments[i].Z;
					}
					Color segment_color = default(Color);
					segment_color.R = (byte)Utility.Lerp(255f, 255f, color_fade);
					segment_color.G = (byte)Utility.Lerp(0f, 166f, color_fade);
					segment_color.B = (byte)Utility.Lerp(0f, 0f, color_fade);
					segment_color.A = byte.MaxValue;
					TemporaryAnimatedSprite current_sprite7 = null;
					current_sprite7 = new TemporaryAnimatedSprite(Sprite.textureName, source_rect2, 800f, 1, 0, segment_position2, flicker: false, flipped: false, 0.9f, 0.001f, segment_color, 4f * (float)scale, 0.01f, current_rotation + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
					{
						motion = new Vector2(xTrajectory, yTrajectory),
						layerDepth = 1f
					};
					current_sprite7.alphaFade = 0.025f;
					base.currentLocation.temporarySprites.Add(current_sprite7);
					current_sprite7 = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 50,
						motion = new Vector2(xTrajectory, yTrajectory),
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite7.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite7);
					current_sprite7 = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 100,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(xTrajectory, yTrajectory) * 0.8f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite7.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite7);
					current_sprite7 = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
					{
						delayBeforeAnimationStart = 150,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(xTrajectory, yTrajectory) * 0.6f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite7.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite7);
					current_sprite7 = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
					{
						delayBeforeAnimationStart = 200,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(xTrajectory, yTrajectory) * 0.4f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite7.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite7);
					current_sprite7 = new TemporaryAnimatedSprite(5, Utility.PointToVector2(bb.Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
					{
						delayBeforeAnimationStart = 250,
						startSound = "cowboy_monsterhit",
						motion = new Vector2(xTrajectory, yTrajectory) * 0.2f,
						layerDepth = 1f
					};
					if (i == -1)
					{
						current_sprite7.startSound = "cowboy_monsterhit";
					}
					base.currentLocation.temporarySprites.Add(current_sprite7);
				}
			}
			else
			{
				base.currentLocation.localSound("serpentDie");
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 64, 32, 32), 200f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f * (float)scale, 0.01f, rotation + (float)Math.PI, (float)((double)Game1.random.Next(3, 5) * Math.PI / 64.0))
				{
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(-32f, 0f), Color.LightGreen * 0.9f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 50,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory),
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(32f, 0f), Color.LightGreen * 0.8f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 100,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.8f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, -32f), Color.LightGreen * 0.7f, 10)
				{
					delayBeforeAnimationStart = 150,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.6f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center), Color.LightGreen * 0.6f, 10, flipped: false, 70f)
				{
					delayBeforeAnimationStart = 200,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.4f,
					layerDepth = 1f
				});
				base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(GetBoundingBox().Center) + new Vector2(0f, 32f), Color.LightGreen * 0.5f, 10)
				{
					delayBeforeAnimationStart = 250,
					startSound = "cowboy_monsterhit",
					motion = new Vector2(xTrajectory, yTrajectory) * 0.2f,
					layerDepth = 1f
				});
			}
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> items = new List<Item>();
			if (Game1.random.NextDouble() < 0.002)
			{
				items.Add(new Object(485, 1));
			}
			return items;
		}

		public override void drawAboveAllLayers(SpriteBatch b)
		{
			Vector2 last_position = base.Position;
			bool is_royal = IsRoyalSerpent();
			for (int i = -1; i < segmentCount.Value; i++)
			{
				Vector2 draw_position = Vector2.Zero;
				float current_rotation3 = 0f;
				float sort_offset2 = (float)(i + 1) * -0.25f / 10000f;
				float max_offset = (float)(int)segmentCount * -0.25f / 10000f - 5E-05f;
				if ((float)(getStandingY() - 1) / 10000f + max_offset < 0f)
				{
					sort_offset2 += 0f - ((float)(getStandingY() - 1) / 10000f + max_offset);
				}
				Rectangle draw_rect2 = Sprite.SourceRect;
				Vector2 shadow_position = base.Position;
				if (i == -1)
				{
					if (is_royal)
					{
						draw_rect2 = new Rectangle(0, 0, 32, 32);
					}
					draw_position = base.Position;
					current_rotation3 = rotation;
				}
				else
				{
					if (i >= segments.Count)
					{
						break;
					}
					Vector3 pos = segments[i];
					draw_position = new Vector2(pos.X, pos.Y);
					draw_rect2 = new Rectangle(32, 0, 32, 32);
					if (i == segments.Count - 1)
					{
						draw_rect2 = new Rectangle(64, 0, 32, 32);
					}
					current_rotation3 = pos.Z;
					shadow_position = (last_position + draw_position) / 2f;
				}
				if (Utility.isOnScreen(draw_position, 128))
				{
					Vector2 local_draw_position2 = Game1.GlobalToLocal(Game1.viewport, draw_position) + drawOffset + new Vector2(0f, yJumpOffset);
					Vector2 local_shadow_position = Game1.GlobalToLocal(Game1.viewport, shadow_position) + drawOffset + new Vector2(0f, yJumpOffset);
					b.Draw(Game1.shadowTexture, local_shadow_position + new Vector2(64f, GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f + sort_offset2);
					b.Draw(Sprite.Texture, local_draw_position2 + new Vector2(64f, GetBoundingBox().Height / 2), draw_rect2, Color.White, current_rotation3, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + sort_offset2)));
					if (isGlowing)
					{
						b.Draw(Sprite.Texture, local_draw_position2 + new Vector2(64f, GetBoundingBox().Height / 2), draw_rect2, glowingColor * glowingTransparency, current_rotation3, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + 0.0001f + sort_offset2)));
					}
					if (is_royal)
					{
						sort_offset2 += -5E-05f;
						current_rotation3 = 0f;
						draw_rect2 = new Rectangle(96, 0, 32, 32);
						local_draw_position2 = Game1.GlobalToLocal(Game1.viewport, last_position) + drawOffset + new Vector2(0f, yJumpOffset);
						if (i > 0)
						{
							b.Draw(Game1.shadowTexture, local_draw_position2 + new Vector2(64f, GetBoundingBox().Height), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f + sort_offset2);
						}
						b.Draw(Sprite.Texture, local_draw_position2 + new Vector2(64f, GetBoundingBox().Height / 2), draw_rect2, Color.White, current_rotation3, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + sort_offset2)));
						if (isGlowing)
						{
							b.Draw(Sprite.Texture, local_draw_position2 + new Vector2(64f, GetBoundingBox().Height / 2), draw_rect2, glowingColor * glowingTransparency, current_rotation3, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(getStandingY() + 8) / 10000f + 0.0001f + sort_offset2)));
						}
					}
				}
				last_position = draw_position;
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 96);
		}

		protected override void updateAnimation(GameTime time)
		{
			if (IsRoyalSerpent())
			{
				if (segments.Count < segmentCount.Value)
				{
					for (int i = 0; i < segmentCount.Value; i++)
					{
						Vector2 position = base.Position;
						segments.Add(new Vector3(position.X, position.Y, 0f));
					}
				}
				Vector2 last_position = base.Position;
				for (int j = 0; j < segments.Count; j++)
				{
					Vector2 current_position = new Vector2(segments[j].X, segments[j].Y);
					Vector2 offset = current_position - last_position;
					int segment_length = 64;
					int num = (int)offset.Length();
					offset.Normalize();
					if (num > segment_length)
					{
						current_position = offset * segment_length + last_position;
					}
					double angle = Math.Atan2(offset.Y, offset.X) - Math.PI / 2.0;
					segments[j] = new Vector3(current_position.X, current_position.Y, (float)angle);
					last_position = current_position;
				}
			}
			base.updateAnimation(time);
			if (wasHitCounter >= 0)
			{
				wasHitCounter -= time.ElapsedGameTime.Milliseconds;
			}
			if (!IsRoyalSerpent())
			{
				Sprite.Animate(time, 0, 9, 40f);
			}
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				float xSlope3 = -(base.Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X);
				float ySlope3 = base.Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y;
				float t = Math.Max(1f, Math.Abs(xSlope3) + Math.Abs(ySlope3));
				if (t < 64f)
				{
					xVelocity = Math.Max(-7f, Math.Min(7f, xVelocity * 1.1f));
					yVelocity = Math.Max(-7f, Math.Min(7f, yVelocity * 1.1f));
				}
				xSlope3 /= t;
				ySlope3 /= t;
				if (wasHitCounter <= 0)
				{
					targetRotation = (float)Math.Atan2(0f - ySlope3, xSlope3) - (float)Math.PI / 2f;
					if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextDouble() < 0.5)
					{
						turningRight = true;
					}
					else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
					{
						turningRight = false;
					}
					if (turningRight)
					{
						rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
					}
					else
					{
						rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
					}
					rotation %= (float)Math.PI * 2f;
					wasHitCounter = 5 + Game1.random.Next(-1, 2);
				}
				float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
				xSlope3 = (float)Math.Cos((double)rotation + Math.PI / 2.0);
				ySlope3 = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
				xVelocity += (0f - xSlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				yVelocity += (0f - ySlope3) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
				if (Math.Abs(xVelocity) > Math.Abs((0f - xSlope3) * 7f))
				{
					xVelocity -= (0f - xSlope3) * maxAccel / 6f;
				}
				if (Math.Abs(yVelocity) > Math.Abs((0f - ySlope3) * 7f))
				{
					yVelocity -= (0f - ySlope3) * maxAccel / 6f;
				}
			}
			resetAnimationSpeed();
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
			{
				base.Health = -500;
			}
			if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
			{
				base.Health = -500;
			}
			if (withinPlayerThreshold() && invincibleCountdown <= 0)
			{
				faceDirection(2);
			}
		}
	}
}
