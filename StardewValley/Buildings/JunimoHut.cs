using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Buildings
{
	public class JunimoHut : Building
	{
		public const int cropHarvestRadius = 8;

		[XmlElement("output")]
		public readonly NetRef<Chest> output = new NetRef<Chest>();

		[XmlElement("noHarvest")]
		public readonly NetBool noHarvest = new NetBool();

		[XmlElement("wasLit")]
		public readonly NetBool wasLit = new NetBool(value: false);

		public Rectangle sourceRect;

		private int junimoSendOutTimer;

		[XmlIgnore]
		public List<JunimoHarvester> myJunimos = new List<JunimoHarvester>();

		[XmlIgnore]
		public Point lastKnownCropLocation = Point.Zero;

		private Rectangle lightInteriorRect = new Rectangle(195, 0, 18, 17);

		private Rectangle bagRect = new Rectangle(208, 51, 15, 13);

		public JunimoHut(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
			sourceRect = getSourceRectForMenu();
			output.Value = new Chest(playerChest: true);
		}

		public JunimoHut()
		{
			sourceRect = getSourceRectForMenu();
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(output, noHarvest, wasLit);
			wasLit.fieldChangeVisibleEvent += delegate
			{
				updateLightState();
			};
		}

		public override Rectangle getRectForAnimalDoor()
		{
			return new Rectangle((1 + (int)tileX) * 64, ((int)tileY + 1) * 64, 64, 64);
		}

		public override Rectangle getSourceRectForMenu()
		{
			return new Rectangle(Utility.getSeasonNumber(Game1.currentSeason) * 48, 0, 48, 64);
		}

		public override void load()
		{
			base.load();
			sourceRect = getSourceRectForMenu();
		}

		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			_ = (int)daysOfConstructionLeft;
			_ = 0;
			sourceRect = getSourceRectForMenu();
			myJunimos.Clear();
			wasLit.Value = false;
		}

		public void sendOutJunimos()
		{
			junimoSendOutTimer = 1000;
		}

		public override void performActionOnConstruction(GameLocation location)
		{
			base.performActionOnConstruction(location);
			sendOutJunimos();
		}

		public override void resetLocalState()
		{
			base.resetLocalState();
			updateLightState();
		}

		public void updateLightState()
		{
			if (Game1.currentLocation != Game1.getFarm())
			{
				return;
			}
			if (wasLit.Value)
			{
				if (Utility.getLightSource((int)tileX + (int)tileY * 777) == null)
				{
					Game1.currentLightSources.Add(new LightSource(4, new Vector2((int)tileX + 1, (int)tileY + 1) * 64f + new Vector2(32f, 32f), 0.5f, LightSource.LightContext.None, 0L)
					{
						Identifier = (int)tileX + (int)tileY * 777
					});
				}
				AmbientLocationSounds.addSound(new Vector2((int)tileX + 1, (int)tileY + 1), 1);
			}
			else
			{
				Utility.removeLightSource((int)tileX + (int)tileY * 777);
				AmbientLocationSounds.removeSound(new Vector2((int)tileX + 1, (int)tileY + 1));
			}
		}

		public int getUnusedJunimoNumber()
		{
			for (int i = 0; i < 3; i++)
			{
				if (i >= myJunimos.Count())
				{
					return i;
				}
				bool found = false;
				foreach (JunimoHarvester myJunimo in myJunimos)
				{
					if (myJunimo.whichJunimoFromThisHut == i)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					return i;
				}
			}
			return 2;
		}

		public override void Update(GameTime time)
		{
			base.Update(time);
			output.Value.mutex.Update(Game1.getFarm());
			if (output.Value.mutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				output.Value.mutex.ReleaseLock();
			}
			if (!Game1.IsMasterGame || junimoSendOutTimer <= 0)
			{
				return;
			}
			junimoSendOutTimer -= time.ElapsedGameTime.Milliseconds;
			if (junimoSendOutTimer <= 0 && myJunimos.Count() < 3 && !Game1.IsWinter && !Game1.isRaining && areThereMatureCropsWithinRadius() && Game1.farmEvent == null)
			{
				int junimoNumber = getUnusedJunimoNumber();
				bool isPrismatic = false;
				Color? gemColor = getGemColor(ref isPrismatic);
				JunimoHarvester i = new JunimoHarvester(new Vector2((int)tileX + 1, (int)tileY + 1) * 64f + new Vector2(0f, 32f), this, junimoNumber, gemColor);
				i.isPrismatic.Value = isPrismatic;
				Game1.getFarm().characters.Add(i);
				myJunimos.Add(i);
				junimoSendOutTimer = 1000;
				if (Utility.isOnScreen(Utility.Vector2ToPoint(new Vector2((int)tileX + 1, (int)tileY + 1)), 64, Game1.getFarm()))
				{
					try
					{
						Game1.getFarm().playSound("junimoMeep1");
					}
					catch (Exception)
					{
					}
				}
			}
		}

		private Color? getGemColor(ref bool isPrismatic)
		{
			List<Color> gemColors = new List<Color>();
			foreach (Item item in output.Value.items)
			{
				if (item != null && (item.Category == -12 || item.Category == -2))
				{
					Color? gemColor = TailoringMenu.GetDyeColor(item);
					if (item.Name == "Prismatic Shard")
					{
						isPrismatic = true;
					}
					if (gemColor.HasValue)
					{
						gemColors.Add(gemColor.Value);
					}
				}
			}
			if (gemColors.Count > 0)
			{
				return gemColors[Game1.random.Next(gemColors.Count)];
			}
			return null;
		}

		public bool areThereMatureCropsWithinRadius()
		{
			Farm f = Game1.getFarm();
			for (int x = (int)tileX + 1 - 8; x < (int)tileX + 2 + 8; x++)
			{
				for (int y = (int)tileY - 8 + 1; y < (int)tileY + 2 + 8; y++)
				{
					if (f.isCropAtTile(x, y) && (f.terrainFeatures[new Vector2(x, y)] as HoeDirt).readyForHarvest())
					{
						lastKnownCropLocation = new Point(x, y);
						return true;
					}
					if (f.terrainFeatures.ContainsKey(new Vector2(x, y)) && f.terrainFeatures[new Vector2(x, y)] is Bush && (int)(f.terrainFeatures[new Vector2(x, y)] as Bush).tileSheetOffset == 1)
					{
						lastKnownCropLocation = new Point(x, y);
						return true;
					}
				}
			}
			lastKnownCropLocation = Point.Zero;
			return false;
		}

		public override void performTenMinuteAction(int timeElapsed)
		{
			base.performTenMinuteAction(timeElapsed);
			for (int i = myJunimos.Count - 1; i >= 0; i--)
			{
				if (!Game1.getFarm().characters.Contains(myJunimos[i]))
				{
					myJunimos.RemoveAt(i);
				}
				else
				{
					myJunimos[i].pokeToHarvest();
				}
			}
			if (myJunimos.Count() < 3 && Game1.timeOfDay < 1900)
			{
				junimoSendOutTimer = 1;
			}
			if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && !Game1.IsWinter && Game1.random.NextDouble() < 0.2)
			{
				wasLit.Value = true;
			}
			else if (Game1.timeOfDay == 2400 && !Game1.IsWinter)
			{
				wasLit.Value = false;
			}
		}

		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if (tileLocation.X >= (float)(int)tileX && tileLocation.X < (float)((int)tileX + (int)tilesWide) && tileLocation.Y >= (float)(int)tileY && tileLocation.Y < (float)((int)tileY + (int)tilesHigh))
			{
				output.Value.mutex.RequestLock(delegate
				{
					Game1.activeClickableMenu = new ItemGrabMenu(output.Value.items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, output.Value.grabItemFromInventory, null, output.Value.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, 1, this);
				});
				return true;
			}
			return base.doAction(tileLocation, who);
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			drawShadow(b, x, y);
			b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 0, 48, 64), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
		}

		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0)
			{
				drawInConstruction(b);
				return;
			}
			drawShadow(b);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), sourceRect, color.Value * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64) / 10000f);
			bool containsOutput = false;
			foreach (Item item in output.Value.items)
			{
				if (item != null && item.Category != -12 && item.Category != -2)
				{
					containsOutput = true;
					break;
				}
			}
			if (containsOutput)
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 128 + 12, (int)tileY * 64 + (int)tilesHigh * 64 - 32)), bagRect, color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64 + 1) / 10000f);
			}
			if (Game1.timeOfDay >= 2000 && Game1.timeOfDay < 2400 && !Game1.IsWinter && wasLit.Value)
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 64, (int)tileY * 64 + (int)tilesHigh * 64 - 64)), lightInteriorRect, color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64 + 1) / 10000f);
			}
		}
	}
}
