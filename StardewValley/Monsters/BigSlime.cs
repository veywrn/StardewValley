using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class BigSlime : Monster
	{
		[XmlElement("c")]
		public readonly NetColor c = new NetColor();

		[XmlElement("heldObject")]
		public readonly NetRef<Object> heldObject = new NetRef<Object>();

		private float heldObjectBobTimer;

		public BigSlime()
		{
		}

		public BigSlime(Vector2 position, MineShaft mine)
			: this(position, mine.getMineArea())
		{
			Sprite.ignoreStopAnimation = true;
			ignoreMovementAnimations = true;
			base.HideShadow = true;
		}

		public BigSlime(Vector2 position, int mineArea)
			: base("Big Slime", position)
		{
			ignoreMovementAnimations = true;
			Sprite.ignoreStopAnimation = true;
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			Sprite.UpdateSourceRect();
			Sprite.framesPerAnimation = 8;
			c.Value = Color.White;
			switch (mineArea)
			{
			case 0:
			case 10:
				c.Value = Color.Lime;
				break;
			case 40:
				c.Value = Color.Turquoise;
				base.Health *= 2;
				base.ExperienceGained *= 2;
				break;
			case 80:
				c.Value = Color.Red;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
				base.ExperienceGained *= 3;
				break;
			case 121:
				c.Value = Color.BlueViolet;
				base.Health *= 4;
				base.DamageToFarmer *= 3;
				base.ExperienceGained *= 3;
				break;
			}
			int r2 = c.R;
			int g2 = c.G;
			int b2 = c.B;
			r2 += Game1.random.Next(-20, 21);
			g2 += Game1.random.Next(-20, 21);
			b2 += Game1.random.Next(-20, 21);
			c.R = (byte)Math.Max(Math.Min(255, r2), 0);
			c.G = (byte)Math.Max(Math.Min(255, g2), 0);
			c.B = (byte)Math.Max(Math.Min(255, b2), 0);
			c.Value *= (float)Game1.random.Next(7, 11) / 10f;
			Sprite.interval = 300f;
			base.HideShadow = true;
			if (Game1.random.NextDouble() < 0.01 && mineArea >= 40)
			{
				heldObject.Value = new Object(221, 1);
			}
			if (Game1.mine != null && Game1.mine.GetAdditionalDifficulty() > 0)
			{
				if (Game1.random.NextDouble() < 0.1)
				{
					heldObject.Value = new Object(858, 1);
				}
				else if (Game1.random.NextDouble() < 0.005)
				{
					heldObject.Value = new Object(896, 1);
				}
			}
			if (Game1.random.NextDouble() < 0.5 && Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD"))
			{
				heldObject.Value = new Object(930, 1);
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(c, heldObject);
		}

		public override void reloadSprite()
		{
			base.reloadSprite();
			Sprite.SpriteWidth = 32;
			Sprite.SpriteHeight = 32;
			Sprite.interval = 300f;
			Sprite.ignoreStopAnimation = true;
			ignoreMovementAnimations = true;
			base.HideShadow = true;
			Sprite.UpdateSourceRect();
			Sprite.framesPerAnimation = 8;
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
				base.Slipperiness = 3;
				base.Health -= actualDamage;
				setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("hitEnemy");
				base.IsWalkingTowardPlayer = true;
				if (base.Health <= 0)
				{
					deathAnimation();
					Game1.stats.SlimesKilled++;
					if (Game1.gameMode == 3 && Game1.random.NextDouble() < 0.75)
					{
						int toCreate = Game1.random.Next(2, 5);
						for (int i = 0; i < toCreate; i++)
						{
							base.currentLocation.characters.Add(new GreenSlime(base.Position, Game1.CurrentMineLevel));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-2, 3), yTrajectory / 8 + Game1.random.Next(-2, 3));
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].moveTowardPlayer(4);
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
							base.currentLocation.characters[base.currentLocation.characters.Count - 1].currentLocation = base.currentLocation;
						}
					}
				}
			}
			return actualDamage;
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, c, 10, flipped: false, 70f));
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(-32f, 0f), c, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 100
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(32f, 0f), c, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 200
			});
			base.currentLocation.localSound("slimedead");
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -32f), c, 10)
			{
				delayBeforeAnimationStart = 300
			});
		}

		protected override void updateAnimation(GameTime time)
		{
			int currentIndex = Sprite.currentFrame;
			Sprite.AnimateDown(time);
			if (isMoving())
			{
				Sprite.interval = 100f;
				heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * ((float)Math.PI / 400f);
			}
			else
			{
				Sprite.interval = 200f;
				heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * ((float)Math.PI / 800f);
			}
			if (Utility.isOnScreen(base.Position, 128) && Sprite.currentFrame == 0 && currentIndex == 7)
			{
				base.currentLocation.localSound("slimeHit");
			}
		}

		public override List<Item> getExtraDropItems()
		{
			if (heldObject.Value != null)
			{
				return new List<Item>
				{
					heldObject.Value
				};
			}
			return base.getExtraDropItems();
		}

		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				if (heldObject.Value != null)
				{
					heldObject.Value.drawInMenu(b, getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(getStandingY() - 1) / 10000f, StackDrawType.Hide, Color.White, drawShadow: false);
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, c, rotation, new Vector2(16f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				if (isGlowing)
				{
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + yJumpOffset), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, scale), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f + 0.001f)));
				}
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 8, (int)position.Y, Sprite.SpriteWidth * 4 * 3 / 4, 64);
		}

		public override void shedChunks(int number, float scale)
		{
		}
	}
}
