using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Sign : Object
	{
		public const int OBJECT = 1;

		public const int HAT = 2;

		public const int BIG_OBJECT = 3;

		public const int RING = 4;

		public const int FURNITURE = 5;

		[XmlElement("displayItem")]
		public readonly NetRef<Item> displayItem = new NetRef<Item>();

		[XmlElement("displayType")]
		public readonly NetInt displayType = new NetInt();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(displayItem, displayType);
		}

		public Sign()
		{
		}

		public Sign(Vector2 tile, int which)
			: base(tile, which)
		{
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return who.CurrentItem != null;
			}
			Item dropIn = who.CurrentItem;
			if (dropIn != null)
			{
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				displayItem.Value = dropIn.getOne();
				Game1.playSound("coin");
				displayType.Value = 1;
				if (displayItem.Value is Hat)
				{
					displayType.Value = 2;
				}
				else if (displayItem.Value is Ring)
				{
					displayType.Value = 4;
				}
				else if (displayItem.Value is Furniture)
				{
					displayType.Value = 5;
				}
				else if (displayItem.Value is Object)
				{
					displayType.Value = ((!(displayItem.Value as Object).bigCraftable) ? 1 : 3);
				}
				return true;
			}
			return false;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			if (displayItem.Value != null)
			{
				switch (displayType.Value)
				{
				case 1:
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 2f, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 2f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
					break;
				case 3:
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
					break;
				case 2:
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) + 1f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
					break;
				case 4:
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64) - 1f, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
					break;
				case 5:
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 8 - 1)), 0.75f, 0.45f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 1E-05f, StackDrawType.Hide, Color.Black, drawShadow: false);
					displayItem.Value.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 21 + 4 - 1)), 0.75f, 1f, Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f + 2E-05f, StackDrawType.Hide, Color.White, drawShadow: false);
					break;
				}
			}
		}
	}
}
