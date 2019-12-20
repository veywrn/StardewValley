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

		public override bool performObjectDropInAction(Item dropIn, bool probe, Farmer who)
		{
			if (dropIn != null && dropIn is Object && (bool)(dropIn as Object).bigCraftable)
			{
				return false;
			}
			if (heldObject.Value != null)
			{
				return false;
			}
			if (!probe && (who == null || !(who.currentLocation is Cellar)))
			{
				Game1.showRedMessageUsingLoadString("Strings\\Objects:CaskNoCellar");
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
			bool goodItem = false;
			float multiplier = 1f;
			switch ((int)dropIn.parentSheetIndex)
			{
			case 426:
				goodItem = true;
				multiplier = 4f;
				break;
			case 424:
				goodItem = true;
				multiplier = 4f;
				break;
			case 348:
				goodItem = true;
				multiplier = 1f;
				break;
			case 459:
				goodItem = true;
				multiplier = 2f;
				break;
			case 303:
				goodItem = true;
				multiplier = 1.66f;
				break;
			case 346:
				goodItem = true;
				multiplier = 2f;
				break;
			}
			if (goodItem)
			{
				heldObject.Value = (dropIn.getOne() as Object);
				if (!probe)
				{
					agingRate.Value = multiplier;
					daysToMature.Value = 56f;
					minutesUntilReady.Value = 999999;
					if ((int)heldObject.Value.quality == 1)
					{
						daysToMature.Value = 42f;
					}
					else if ((int)heldObject.Value.quality == 2)
					{
						daysToMature.Value = 28f;
					}
					else if ((int)heldObject.Value.quality == 4)
					{
						daysToMature.Value = 0f;
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

		public void checkForMaturity()
		{
			if ((float)daysToMature <= 0f)
			{
				minutesUntilReady.Value = 1;
				heldObject.Value.quality.Value = 4;
			}
			else if ((float)daysToMature <= 28f)
			{
				heldObject.Value.quality.Value = 2;
			}
			else if ((float)daysToMature <= 42f)
			{
				heldObject.Value.quality.Value = 1;
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
