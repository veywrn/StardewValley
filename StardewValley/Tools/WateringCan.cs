using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class WateringCan : Tool
	{
		[XmlElement("isBottomless")]
		public readonly NetBool isBottomless = new NetBool();

		[XmlIgnore]
		protected bool _emptyCanPlayed;

		public int waterCanMax = 40;

		private int waterLeft = 40;

		public int WaterLeft
		{
			get
			{
				return waterLeft;
			}
			set
			{
				waterLeft = value;
			}
		}

		public bool IsBottomless
		{
			get
			{
				return isBottomless;
			}
			set
			{
				isBottomless.Value = value;
			}
		}

		public WateringCan()
			: base("Watering Can", 0, 273, 296, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(isBottomless);
		}

		public override Item getOne()
		{
			WateringCan result = new WateringCan();
			result.UpgradeLevel = base.UpgradeLevel;
			CopyEnchantments(this, result);
			result._GetOneFrom(this);
			return result;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14324");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14325");
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location + (Game1.player.hasWateringCanEnchantment ? new Vector2(0f, -4f) : new Vector2(0f, -12f)), scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
			if (drawStackNumber != 0 && !Game1.player.hasWateringCanEnchantment)
			{
				spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(4f, 44f), new Rectangle(297, 420, 14, 5), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 0.0001f);
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X + 8, (int)location.Y + 64 - 16, (int)((float)waterLeft / (float)waterCanMax * 48f), 8), IsBottomless ? (Color.BlueViolet * 1f * transparency) : (Color.DodgerBlue * 0.7f * transparency));
			}
		}

		public override string getDescription()
		{
			return Game1.parseText(base.description + (Game1.player.hasWateringCanEnchantment ? (Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan_enchant")) : ""), Game1.smallFont, getDescriptionWidth());
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			power = who.toolPower;
			who.stopJittering();
			List<Vector2> tileLocations = tilesAffected(new Vector2(x / 64, y / 64), power, who);
			if (Game1.currentLocation.CanRefillWateringCanOnTile(x / 64, y / 64))
			{
				who.jitterStrength = 0.5f;
				switch ((int)upgradeLevel)
				{
				case 0:
					waterCanMax = 40;
					break;
				case 1:
					waterCanMax = 55;
					break;
				case 2:
					waterCanMax = 70;
					break;
				case 3:
					waterCanMax = 85;
					break;
				case 4:
					waterCanMax = 100;
					break;
				}
				waterLeft = waterCanMax;
				location.playSound("slosh");
				DelayedAction.playSoundAfterDelay("glug", 250, location);
			}
			else if (waterLeft > 0 || who.hasWateringCanEnchantment)
			{
				if (!isEfficient)
				{
					who.Stamina -= (float)(2 * (power + 1)) - (float)who.FarmingLevel * 0.1f;
				}
				int j = 0;
				foreach (Vector2 tileLocation in tileLocations)
				{
					if (location.terrainFeatures.ContainsKey(tileLocation))
					{
						location.terrainFeatures[tileLocation].performToolAction(this, 0, tileLocation, location);
					}
					if (location.objects.ContainsKey(tileLocation))
					{
						location.Objects[tileLocation].performToolAction(this, location);
					}
					location.performToolAction(this, (int)tileLocation.X, (int)tileLocation.Y);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(13, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 10, Game1.random.NextDouble() < 0.5, 70f, 0, 64, (tileLocation.Y * 64f + 32f) / 10000f - 0.01f)
					{
						delayBeforeAnimationStart = 200 + j * 10
					});
					j++;
				}
				if (!isBottomless)
				{
					waterLeft -= power + 1;
				}
				Vector2 basePosition = new Vector2(who.Position.X - 32f - 4f, who.Position.Y - 16f - 4f);
				switch (who.FacingDirection)
				{
				case 1:
					basePosition.X += 136f;
					break;
				case 2:
					basePosition.X += 72f;
					basePosition.Y += 44f;
					break;
				case 0:
					basePosition = Vector2.Zero;
					break;
				}
				if (!basePosition.Equals(Vector2.Zero))
				{
					for (int i = 0; i < 30; i++)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("", new Rectangle(0, 0, 1, 1), 999f, 1, 999, basePosition + new Vector2(Game1.random.Next(-3, 0) * 4, Game1.random.Next(2) * 4), flicker: false, flipped: false, (float)(who.GetBoundingBox().Bottom + 32) / 10000f, 0.04f, (Game1.random.NextDouble() < 0.5) ? Color.DeepSkyBlue : Color.LightBlue, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = i * 15,
							motion = new Vector2((float)Game1.random.Next(-10, 11) / 100f, 0.5f),
							acceleration = new Vector2(0f, 0.1f)
						});
					}
				}
			}
			else if (!_emptyCanPlayed)
			{
				_emptyCanPlayed = true;
				who.doEmote(4);
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:WateringCan.cs.14335"));
			}
		}

		public override bool CanUseOnStandingTile()
		{
			return true;
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			base.tickUpdate(time, who);
			if (who.IsLocalPlayer)
			{
				if (Game1.areAllOfTheseKeysUp(Game1.input.GetKeyboardState(), Game1.options.useToolButton) && Game1.input.GetMouseState().LeftButton == ButtonState.Released && Game1.input.GetGamePadState().IsButtonUp(Buttons.X))
				{
					_emptyCanPlayed = false;
				}
			}
			else
			{
				_emptyCanPlayed = false;
			}
		}
	}
}
