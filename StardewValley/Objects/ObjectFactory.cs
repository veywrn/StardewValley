using Microsoft.Xna.Framework;
using StardewValley.Tools;
using System;

namespace StardewValley.Objects
{
	public class ObjectFactory
	{
		public const byte regularObject = 0;

		public const byte bigCraftable = 1;

		public const byte weapon = 2;

		public const byte specialItem = 3;

		public const byte regularObjectRecipe = 4;

		public const byte bigCraftableRecipe = 5;

		public static ItemDescription getDescriptionFromItem(Item i)
		{
			if (i is Object && (bool)(i as Object).bigCraftable)
			{
				return new ItemDescription(1, (i as Object).ParentSheetIndex, i.Stack);
			}
			if (i is Object)
			{
				return new ItemDescription(0, (i as Object).ParentSheetIndex, i.Stack);
			}
			if (i is MeleeWeapon)
			{
				return new ItemDescription(2, (i as MeleeWeapon).CurrentParentTileIndex, i.Stack);
			}
			throw new Exception("ItemFactory trying to create item description from unknown item");
		}

		public static Item getItemFromDescription(byte type, int index, int stack)
		{
			switch (type)
			{
			case 0:
				return new Object(Vector2.Zero, index, stack);
			case 4:
				return new Object(index, stack, isRecipe: true);
			case 1:
				return new Object(Vector2.Zero, index);
			case 5:
				return new Object(Vector2.Zero, index, isRecipe: true);
			case 2:
				return new MeleeWeapon(index);
			default:
				throw new Exception("ItemFactory trying to create item from unknown description");
			}
		}
	}
}
