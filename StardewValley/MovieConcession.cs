using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Movies;
using System.Collections.Generic;

namespace StardewValley
{
	public class MovieConcession : ISalable
	{
		protected string _displayName = "";

		protected string _name = "";

		protected string _description = "";

		protected int _price;

		protected int _id;

		protected List<string> _tags;

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

		public MovieConcession(ConcessionItemData data)
		{
			_id = data.ID;
			_name = data.Name;
			_displayName = data.DisplayName;
			_description = data.Description;
			_price = data.Price;
			_tags = data.ItemTags;
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (_id != 590 && drawShadow)
			{
				spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
			}
			spriteBatch.Draw(Game1.concessionsSpriteSheet, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.concessionsSpriteSheet, _id, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
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
			return true;
		}

		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		public bool IsInfiniteStock()
		{
			return true;
		}

		public ISalable GetSalableInstance()
		{
			return this;
		}
	}
}
