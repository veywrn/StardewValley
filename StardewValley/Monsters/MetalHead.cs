using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class MetalHead : Monster
	{
		[XmlElement("c")]
		public readonly NetColor c = new NetColor();

		public MetalHead()
		{
		}

		public MetalHead(Vector2 tileLocation, MineShaft mine)
			: this(tileLocation, mine.getMineArea())
		{
		}

		public MetalHead(string name, Vector2 tileLocation)
			: base(name, tileLocation)
		{
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
			c.Value = Color.White;
			base.IsWalkingTowardPlayer = true;
		}

		public MetalHead(Vector2 tileLocation, int mineArea)
			: base("Metal Head", tileLocation)
		{
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
			c.Value = Color.White;
			base.IsWalkingTowardPlayer = true;
			switch (mineArea)
			{
			case 0:
				c.Value = Color.White;
				break;
			case 40:
				c.Value = Color.Turquoise;
				base.Health *= 2;
				break;
			case 80:
				c.Value = Color.White;
				base.Health *= 3;
				break;
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(c);
			position.Field.AxisAlignedMovement = true;
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "clank");
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position, Color.DarkGray, 10, flipped: false, 70f));
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(-32f, 0f), Color.DarkGray, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 300
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(46, base.Position + new Vector2(32f, 0f), Color.DarkGray, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 600
			});
			base.currentLocation.localSound("monsterdead");
			Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.MediumPurple, 10)
			{
				holdLastFrame = true,
				alphaFade = 0.01f,
				interval = 70f
			}, base.currentLocation);
			base.localDeathAnimation();
		}

		public override void draw(SpriteBatch b)
		{
			if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 42f + yOffset), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3.5f + (float)scale + yOffset / 30f, SpriteEffects.None, (float)(getStandingY() - 1) / 10000f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 48 + yJumpOffset), Sprite.SourceRect, c, rotation, new Vector2(8f, 16f), Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
			}
		}

		public override void shedChunks(int number, float scale)
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, Sprite.getHeight() * 4, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, scale * 4f);
		}

		public override List<Item> getExtraDropItems()
		{
			List<Item> extraItems = new List<Item>();
			if ((Game1.stats.getMonstersKilled(name) + (int)Game1.uniqueIDForThisGame) % 100 == 0)
			{
				extraItems.Add(new Hat(51));
			}
			return extraItems;
		}

		protected override void updateMonsterSlaveAnimation(GameTime time)
		{
			if (isMoving())
			{
				if (FacingDirection == 0)
				{
					Sprite.AnimateUp(time);
				}
				else if (FacingDirection == 3)
				{
					Sprite.AnimateLeft(time);
				}
				else if (FacingDirection == 1)
				{
					Sprite.AnimateRight(time);
				}
				else if (FacingDirection == 2)
				{
					Sprite.AnimateDown(time);
				}
			}
			else
			{
				Sprite.StopAnimation();
			}
		}
	}
}
