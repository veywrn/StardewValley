using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley
{
	public interface ISalable
	{
		string DisplayName
		{
			get;
		}

		string Name
		{
			get;
		}

		int Stack
		{
			get;
			set;
		}

		void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow);

		string getDescription();

		int maximumStackSize();

		int addToStack(Item stack);

		int salePrice();

		bool actionWhenPurchased();

		bool canStackWith(ISalable other);

		bool CanBuyItem(Farmer farmer);

		bool IsInfiniteStock();

		ISalable GetSalableInstance();
	}
}
