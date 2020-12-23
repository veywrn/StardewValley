using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class PurchaseableKeyItem : ISalable
	{
		protected string _displayName = "";

		protected string _name = "";

		protected string _description = "";

		protected int _price;

		protected int _id;

		protected List<string> _tags;

		protected Action<Farmer> _onPurchase;

		public string DisplayName => _displayName;

		public int id => _id;

		public List<string> tags => _tags;

		public string Name => _name;

		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public PurchaseableKeyItem(string display_name, string display_description, int parent_sheet_index, Action<Farmer> on_purchase = null)
		{
			_id = parent_sheet_index;
			_name = display_name;
			_displayName = display_name;
			_description = display_description;
			_onPurchase = on_purchase;
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, _id, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
		}

		public bool ShouldDrawIcon()
		{
			return true;
		}

		public string getDescription()
		{
			return _description;
		}

		public int maximumStackSize()
		{
			return 1;
		}

		public int addToStack(Item stack)
		{
			return 1;
		}

		public bool canStackWith(ISalable other)
		{
			return false;
		}

		public int salePrice()
		{
			return _price;
		}

		public bool actionWhenPurchased()
		{
			if (_onPurchase != null)
			{
				_onPurchase(Game1.player);
			}
			return true;
		}

		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		public bool IsInfiniteStock()
		{
			return false;
		}

		public ISalable GetSalableInstance()
		{
			return this;
		}
	}
}
