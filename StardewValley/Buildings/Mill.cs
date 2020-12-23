using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Buildings
{
	public class Mill : Building
	{
		[XmlElement("input")]
		public readonly NetRef<Chest> input = new NetRef<Chest>();

		[XmlElement("output")]
		public readonly NetRef<Chest> output = new NetRef<Chest>();

		private bool hasLoadedToday;

		private Rectangle baseSourceRect = new Rectangle(0, 0, 64, 128);

		public Mill(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
			input.Value = new Chest(playerChest: true);
			output.Value = new Chest(playerChest: true);
		}

		public Mill()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(input, output);
		}

		public override Rectangle getSourceRectForMenu()
		{
			return new Rectangle(0, 0, 64, texture.Value.Bounds.Height);
		}

		public override void load()
		{
			base.load();
		}

		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if ((int)daysOfConstructionLeft <= 0)
			{
				if (tileLocation.X == (float)((int)tileX + 1) && tileLocation.Y == (float)((int)tileY + 1))
				{
					if (who != null && who.ActiveObject != null)
					{
						bool millableItem = false;
						int num = who.ActiveObject.parentSheetIndex;
						if (num == 262 || num == 271 || num == 284)
						{
							millableItem = true;
						}
						if (!millableItem)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantMill"));
							return false;
						}
						Object item2 = who.ActiveObject;
						who.ActiveObject = null;
						item2 = (Object)Utility.addItemToThisInventoryList(item2, input.Value.items, 36);
						if (item2 != null)
						{
							who.ActiveObject = item2;
						}
						hasLoadedToday = true;
						Game1.playSound("Ship");
						if (who.ActiveObject != null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:MillFull"));
						}
					}
				}
				else if (tileLocation.X == (float)((int)tileX + 3) && tileLocation.Y == (float)((int)tileY + 1))
				{
					Utility.CollectSingleItemOrShowChestMenu(output.Value, this);
					return true;
				}
			}
			return base.doAction(tileLocation, who);
		}

		public override void dayUpdate(int dayOfMonth)
		{
			hasLoadedToday = false;
			for (int i = input.Value.items.Count - 1; i >= 0; i--)
			{
				if (input.Value.items[i] != null)
				{
					Item toAdd = null;
					switch ((int)input.Value.items[i].parentSheetIndex)
					{
					case 271:
						toAdd = new Object(423, input.Value.items[i].Stack);
						break;
					case 245:
					case 246:
					case 423:
						toAdd = input.Value.items[i];
						break;
					case 262:
						toAdd = new Object(246, input.Value.items[i].Stack);
						break;
					case 284:
						toAdd = new Object(245, 3 * input.Value.items[i].Stack);
						break;
					}
					if (toAdd != null && Utility.canItemBeAddedToThisInventoryList(toAdd, output.Value.items, 36))
					{
						input.Value.items[i] = Utility.addItemToThisInventoryList(toAdd, output.Value.items, 36);
					}
				}
			}
			base.dayUpdate(dayOfMonth);
		}

		public override List<Item> GetAdditionalItemsToCheckBeforeDemolish()
		{
			return new List<Item>(output.Value.items);
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			b.Draw(texture.Value, new Vector2(x, y), getSourceRectForMenu(), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.89f);
			b.Draw(texture.Value, new Vector2(x + 32, y + 4), new Rectangle(64 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800 / 89 * 32 % 160, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800 / 89 * 32 / 160 * 32, 32, 32), color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0.9f);
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
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), baseSourceRect, color.Value * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64) / 10000f);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 32, (int)tileY * 64 + (int)tilesHigh * 64 + 4)), new Rectangle(64 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800 / 89 * 32 % 160, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 800 / 89 * 32 / 160 * 32, 32, 32), color.Value * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f);
			if (hasLoadedToday)
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 52, (int)tileY * 64 + (int)tilesHigh * 64 + 276)), new Rectangle(64 + (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 700 / 100 * 21, 72, 21, 8), color.Value * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f);
			}
			if (output.Value.items.Count > 0 && output.Value.items[0] != null && ((int)output.Value.items[0].parentSheetIndex == 245 || (int)output.Value.items[0].parentSheetIndex == 246 || (int)output.Value.items[0].parentSheetIndex == 423))
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 192, (float)((int)tileY * 64 - 96) + yOffset)), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + 1) * 64) / 10000f + 1E-06f + (float)(int)tileX / 10000f);
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 192 + 32 + 4, (float)((int)tileY * 64 - 64 + 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, output.Value.items[0].parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (float)(((int)tileY + 1) * 64) / 10000f + 1E-05f + (float)(int)tileX / 10000f);
			}
		}
	}
}
