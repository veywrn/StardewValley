using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Cask : Object
	{
		public const int defaultDaysToMature = 56;

		[XmlElement("agingRate")]
		public readonly NetFloat agingRate = new NetFloat();

		[XmlElement("daysToMature")]
		public readonly NetFloat daysToMature = new NetFloat();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(agingRate, daysToMature);
		}

		public Cask()
		{
		}

		public Cask(Vector2 v)
			: base(v, 163)
		{
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
			{
				if (heldObject.Value != null)
				{
					Game1.createItemDebris(heldObject.Value, tileLocation.Value * 64f, -1);
				}
				location.playSound("woodWhack");
				if (heldObject.Value == null)
				{
					return true;
				}
				heldObject.Value = null;
				readyForHarvest.Value = false;
				minutesUntilReady.Value = -1;
				return false;
			}
			return base.performToolAction(t, location);
		}

		public virtual bool IsValidCaskLocation(GameLocation location)
		{
			if (location is Cellar)
			{
				return true;
			}
			if (location.getMapProperty("CanCaskHere") != "")
			{
				return true;
			}
			return false;
		}

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			if (dropIn != null && dropIn is Object && (bool)(dropIn as Object).bigCraftable)
			{
				return false;
			}
			if (heldObject.Value != null)
			{
				if ((int)dropIn.parentSheetIndex == 872)
				{
					if (probe)
					{
						return true;
					}
					if (heldObject.Value == null)
					{
						return false;
					}
					if (heldObject.Value.Quality == 4)
					{
						return false;
					}
					Utility.addSprinklesToLocation(who.currentLocation, (int)tileLocation.X, (int)tileLocation.Y, 1, 2, 400, 40, Color.White);
					Game1.playSound("yoba");
					daysToMature.Value = GetDaysForQuality(GetNextQuality(heldObject.Value.Quality));
					checkForMaturity();
					return true;
				}
				return false;
			}
			if (!probe && (who == null || !IsValidCaskLocation(who.currentLocation)))
			{
				if (Object.autoLoadChest == null)
				{
					Game1.showRedMessageUsingLoadString("Strings\\Objects:CaskNoCellar");
				}
				return false;
			}
			if ((int)quality >= 4)
			{
				return false;
			}
			Object dropped_in_object;
			if ((dropped_in_object = (dropIn as Object)) != null && dropped_in_object.Quality >= 4)
			{
				return false;
			}
			float multiplier2 = 1f;
			multiplier2 = GetAgingMultiplierForItem(dropIn);
			if (multiplier2 > 0f)
			{
				heldObject.Value = (dropIn.getOne() as Object);
				if (!probe)
				{
					agingRate.Value = multiplier2;
					minutesUntilReady.Value = 999999;
					daysToMature.Value = GetDaysForQuality(heldObject.Value.Quality);
					if (heldObject.Value.Quality == 4)
					{
						minutesUntilReady.Value = 1;
					}
					who.currentLocation.playSound("Ship");
					who.currentLocation.playSound("bubbles");
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
					{
						alphaFade = 0.005f
					});
				}
				return true;
			}
			return false;
		}

		public virtual float GetAgingMultiplierForItem(Item item)
		{
			if (item == null)
			{
				return 0f;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				switch ((int)item.parentSheetIndex)
				{
				case 426:
					return 4f;
				case 424:
					return 4f;
				case 348:
					return 1f;
				case 459:
					return 2f;
				case 303:
					return 1.66f;
				case 346:
					return 2f;
				}
			}
			return 0f;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			return base.checkForAction(who, justCheckingForActivity);
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			if (heldObject.Value != null)
			{
				minutesUntilReady.Value = 999999;
				daysToMature.Value -= agingRate;
				checkForMaturity();
			}
		}

		public float GetDaysForQuality(int quality)
		{
			switch (quality)
			{
			case 4:
				return 0f;
			case 2:
				return 28f;
			case 1:
				return 42f;
			default:
				return 56f;
			}
		}

		public int GetNextQuality(int quality)
		{
			switch (quality)
			{
			case 4:
				return 4;
			case 2:
				return 4;
			case 1:
				return 2;
			default:
				return 1;
			}
		}

		public void checkForMaturity()
		{
			if ((float)daysToMature <= GetDaysForQuality(GetNextQuality(heldObject.Value.quality.Value)))
			{
				heldObject.Value.quality.Value = GetNextQuality(heldObject.Value.quality.Value);
				if (heldObject.Value.Quality == 4)
				{
					minutesUntilReady.Value = 1;
				}
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (heldObject.Value != null && (int)heldObject.Value.quality > 0)
			{
				Vector2 scaleFactor = ((int)minutesUntilReady > 0) ? new Vector2(Math.Abs(scale.X - 5f), Math.Abs(scale.Y - 5f)) : Vector2.Zero;
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				Rectangle destination = new Rectangle((int)(position.X + 32f - 8f - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y + 64f + 8f - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(16f + scaleFactor.X), (int)(16f + scaleFactor.Y / 2f));
				spriteBatch.Draw(Game1.mouseCursors, destination, ((int)heldObject.Value.quality < 4) ? new Rectangle(338 + ((int)heldObject.Value.quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8), Color.White * 0.95f, 0f, Vector2.Zero, SpriteEffects.None, (float)((y + 1) * 64) / 10000f);
			}
		}
	}
}
