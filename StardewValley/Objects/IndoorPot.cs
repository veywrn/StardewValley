using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.TerrainFeatures;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class IndoorPot : Object
	{
		[XmlElement("hoeDirt")]
		public readonly NetRef<HoeDirt> hoeDirt = new NetRef<HoeDirt>();

		[XmlElement("bush")]
		public readonly NetRef<Bush> bush = new NetRef<Bush>();

		[XmlIgnore]
		private readonly NetBool bushLoadDirty = new NetBool(value: true);

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(hoeDirt, bush, bushLoadDirty);
		}

		public IndoorPot()
		{
		}

		public IndoorPot(Vector2 tileLocation)
			: base(tileLocation, 62)
		{
			hoeDirt.Value = new HoeDirt();
			if (Game1.isRaining && (bool)Game1.currentLocation.isOutdoors)
			{
				hoeDirt.Value.state.Value = 1;
			}
			showNextIndex.Value = ((int)hoeDirt.Value.state == 1);
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			hoeDirt.Value.dayUpdate(location, tileLocation);
			if (Game1.isRaining && (bool)location.isOutdoors)
			{
				hoeDirt.Value.state.Value = 1;
			}
			showNextIndex.Value = ((int)hoeDirt.Value.state == 1);
			if (heldObject.Value != null)
			{
				readyForHarvest.Value = true;
			}
			if (bush.Value != null)
			{
				bush.Value.dayUpdate(location);
			}
		}

		public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			if (who != null && dropInItem != null && bush.Value == null && hoeDirt.Value.canPlantThisSeedHere(dropInItem.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, dropInItem.Category == -19))
			{
				if ((int)dropInItem.parentSheetIndex == 805)
				{
					if (!probe)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					}
					return false;
				}
				if ((int)dropInItem.parentSheetIndex == 499)
				{
					if (!probe)
					{
						Game1.playSound("cancel");
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Objects:AncientFruitPot"));
					}
					return false;
				}
				if (!probe)
				{
					if (!hoeDirt.Value.plant(dropInItem.parentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y, who, dropInItem.Category == -19, who.currentLocation))
					{
						return false;
					}
				}
				else
				{
					heldObject.Value = new Object();
				}
				return true;
			}
			if (who != null && dropInItem != null && hoeDirt.Value.crop == null && bush.Value == null && dropInItem is Object && !(dropInItem as Object).bigCraftable && (int)dropInItem.parentSheetIndex == 251)
			{
				if (probe)
				{
					heldObject.Value = new Object();
				}
				else
				{
					bush.Value = new Bush(tileLocation, 3, who.currentLocation);
					if (!who.currentLocation.IsOutdoors)
					{
						bush.Value.greenhouseBush.Value = true;
						bush.Value.loadSprite();
						Game1.playSound("coin");
					}
				}
				return true;
			}
			return false;
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (t != null)
			{
				hoeDirt.Value.performToolAction(t, -1, tileLocation, location);
				if (bush.Value != null)
				{
					if (bush.Value.performToolAction(t, -1, tileLocation, location))
					{
						bush.Value = null;
					}
					return false;
				}
			}
			if ((int)hoeDirt.Value.state == 1)
			{
				showNextIndex.Value = true;
			}
			return base.performToolAction(t, location);
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (who != null)
			{
				if (justCheckingForActivity)
				{
					if (!hoeDirt.Value.readyForHarvest() && heldObject.Value == null)
					{
						if (bush.Value != null)
						{
							return bush.Value.inBloom(Game1.currentSeason, Game1.dayOfMonth);
						}
						return false;
					}
					return true;
				}
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				if (heldObject.Value != null)
				{
					bool num = who.addItemToInventoryBool(heldObject.Value);
					if (num)
					{
						heldObject.Value = null;
						readyForHarvest.Value = false;
						Game1.playSound("coin");
					}
					return num;
				}
				bool b = hoeDirt.Value.performUseAction(tileLocation, who.currentLocation);
				if (b)
				{
					if (hoeDirt.Value.crop == null)
					{
						hoeDirt.Value.fertilizer.Value = 0;
					}
					return b;
				}
				if (hoeDirt.Value.crop != null && (int)hoeDirt.Value.crop.currentPhase > 0 && hoeDirt.Value.getMaxShake() == 0f)
				{
					hoeDirt.Value.shake((float)Math.PI / 32f, (float)Math.PI / 50f, Game1.random.NextDouble() < 0.5);
					DelayedAction.playSoundAfterDelay("leafrustle", Game1.random.Next(100));
				}
				if (bush.Value != null)
				{
					bush.Value.performUseAction(tileLocation, who.currentLocation);
				}
			}
			return false;
		}

		public override void actionOnPlayerEntry()
		{
			base.actionOnPlayerEntry();
			if (hoeDirt.Value != null)
			{
				hoeDirt.Value.performPlayerEntryAction(tileLocation);
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			base.updateWhenCurrentLocation(time, environment);
			hoeDirt.Value.tickUpdate(time, tileLocation, environment);
			bush.Value?.tickUpdate(time, environment);
			if ((bool)bushLoadDirty)
			{
				bush.Value?.loadSprite();
				bushLoadDirty.Value = false;
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 scaleFactor = getScale();
			scaleFactor *= 4f;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, Object.getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (((int)parentSheetIndex == 105) ? 0.0035f : 0f) + (float)x * 1E-05f);
			if ((int)hoeDirt.Value.fertilizer != 0)
			{
				Rectangle fertilizer_rect = hoeDirt.Value.GetFertilizerSourceRect(hoeDirt.Value.fertilizer);
				fertilizer_rect.Width = 13;
				fertilizer_rect.Height = 13;
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 4f, tileLocation.Y * 64f - 12f)), fertilizer_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y + 0.65f) * 64f / 10000f + (float)x * 1E-05f);
			}
			if (hoeDirt.Value.crop != null)
			{
				hoeDirt.Value.crop.drawWithOffset(spriteBatch, tileLocation, ((int)hoeDirt.Value.state == 1 && (int)hoeDirt.Value.crop.currentPhase == 0 && !hoeDirt.Value.crop.raisedSeeds) ? (new Color(180, 100, 200) * 1f) : Color.White, hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
			}
			if (heldObject.Value != null)
			{
				heldObject.Value.draw(spriteBatch, x * 64, y * 64 - 48, (tileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, 1f);
			}
			if (bush.Value != null)
			{
				bush.Value.draw(spriteBatch, new Vector2(x, y), -24f);
			}
		}
	}
}
