using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	public class CraftingRecipe
	{
		public const int wild_seed_special_category = -777;

		public string name;

		public string DisplayName;

		public string description;

		public static Dictionary<string, string> craftingRecipes;

		public static Dictionary<string, string> cookingRecipes;

		public Dictionary<int, int> recipeList = new Dictionary<int, int>();

		public List<int> itemToProduce = new List<int>();

		public bool bigCraftable;

		public bool isCookingRecipe;

		public int timesCrafted;

		public int numberProducedPerCraft;

		public string itemType;

		public string ItemType
		{
			get
			{
				if (itemType != null && !(itemType == ""))
				{
					return itemType;
				}
				if (!bigCraftable)
				{
					return "O";
				}
				return "BO";
			}
		}

		public static void InitShared()
		{
			craftingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			cookingRecipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
		}

		public CraftingRecipe(string name)
			: this(name, cookingRecipes.ContainsKey(name))
		{
		}

		public CraftingRecipe(string name, bool isCookingRecipe)
		{
			this.isCookingRecipe = isCookingRecipe;
			this.name = name;
			string info = (isCookingRecipe && cookingRecipes.ContainsKey(name)) ? cookingRecipes[name] : (craftingRecipes.ContainsKey(name) ? craftingRecipes[name] : null);
			if (info == null)
			{
				this.name = "Torch";
				name = "Torch";
				info = craftingRecipes[name];
			}
			string[] infoSplit = info.Split('/');
			string[] ingredientsSplit = infoSplit[0].Split(' ');
			for (int j = 0; j < ingredientsSplit.Length; j += 2)
			{
				recipeList.Add(Convert.ToInt32(ingredientsSplit[j]), Convert.ToInt32(ingredientsSplit[j + 1]));
			}
			string[] itemToProduceList = infoSplit[2].Split(' ');
			for (int i = 0; i < itemToProduceList.Length; i += 2)
			{
				itemToProduce.Add(Convert.ToInt32(itemToProduceList[i]));
				numberProducedPerCraft = ((itemToProduceList.Length <= 1) ? 1 : Convert.ToInt32(itemToProduceList[i + 1]));
			}
			if (!isCookingRecipe)
			{
				if (infoSplit[3] == "true")
				{
					itemType = "BO";
					bigCraftable = true;
				}
				else if (infoSplit[3] == "false")
				{
					itemType = "O";
				}
				else
				{
					itemType = infoSplit[3];
				}
			}
			try
			{
				description = (bigCraftable ? Game1.bigCraftablesInformation[itemToProduce[0]].Split('/')[4] : Game1.objectInformation[itemToProduce[0]].Split('/')[5]);
			}
			catch (Exception)
			{
				description = "";
			}
			timesCrafted = (Game1.player.craftingRecipes.ContainsKey(name) ? Game1.player.craftingRecipes[name] : 0);
			if (name.Equals("Crab Pot") && Game1.player.professions.Contains(7))
			{
				recipeList = new Dictionary<int, int>();
				recipeList.Add(388, 25);
				recipeList.Add(334, 2);
			}
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				DisplayName = infoSplit[infoSplit.Length - 1];
			}
			else
			{
				DisplayName = name;
			}
		}

		public int getIndexOfMenuView()
		{
			if (itemToProduce.Count <= 0)
			{
				return -1;
			}
			return itemToProduce[0];
		}

		public virtual bool doesFarmerHaveIngredientsInInventory(IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<int, int> kvp in recipeList)
			{
				int required_count3 = kvp.Value;
				required_count3 -= Game1.player.getItemCount(kvp.Key, 5);
				if (required_count3 > 0)
				{
					if (extraToCheck != null)
					{
						required_count3 -= Game1.player.getItemCountInList(extraToCheck, kvp.Key, 5);
						if (required_count3 <= 0)
						{
							continue;
						}
					}
					return false;
				}
			}
			return true;
		}

		public virtual void drawMenuView(SpriteBatch b, int x, int y, float layerDepth = 0.88f, bool shadow = true)
		{
			if (bigCraftable)
			{
				Utility.drawWithShadow(b, Game1.bigCraftableSpriteSheet, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, getIndexOfMenuView(), 16, 32), Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
			}
			else
			{
				Utility.drawWithShadow(b, Game1.objectSpriteSheet, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getIndexOfMenuView(), 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, layerDepth);
			}
		}

		public virtual Item createItem()
		{
			int index = itemToProduce.ElementAt(Game1.random.Next(itemToProduce.Count));
			if (bigCraftable)
			{
				if (name.Equals("Chest"))
				{
					return new Chest(playerChest: true);
				}
				return new Object(Vector2.Zero, index);
			}
			if (name.Equals("Torch"))
			{
				return new Torch(Vector2.Zero, numberProducedPerCraft);
			}
			if ((index >= 516 && index <= 534) || index == 801)
			{
				return new Ring(index);
			}
			Item item = Utility.getItemFromStandardTextDescription(ItemType + " " + index + " " + numberProducedPerCraft, Game1.player);
			if (isCookingRecipe && item is Object && Game1.player.team.SpecialOrderRuleActive("QI_COOKING"))
			{
				(item as Object).orderData.Value = "QI_COOKING";
				item.MarkContextTagsDirty();
			}
			return item;
		}

		public static bool isThereSpecialIngredientRule(Object potentialIngredient, int requiredIngredient)
		{
			if (requiredIngredient == -777 && ((int)potentialIngredient.parentSheetIndex == 495 || (int)potentialIngredient.parentSheetIndex == 496 || (int)potentialIngredient.parentSheetIndex == 497 || (int)potentialIngredient.parentSheetIndex == 498))
			{
				return true;
			}
			return false;
		}

		public void consumeIngredients(List<Chest> additional_materials)
		{
			for (int k = recipeList.Count - 1; k >= 0; k--)
			{
				int required_count = recipeList[recipeList.Keys.ElementAt(k)];
				bool foundInBackpack = false;
				for (int j = Game1.player.items.Count - 1; j >= 0; j--)
				{
					if (Game1.player.items[j] != null && Game1.player.items[j] is Object && !(Game1.player.items[j] as Object).bigCraftable && ((int)((Object)Game1.player.items[j]).parentSheetIndex == recipeList.Keys.ElementAt(k) || ((Object)Game1.player.items[j]).Category == recipeList.Keys.ElementAt(k) || isThereSpecialIngredientRule((Object)Game1.player.items[j], recipeList.Keys.ElementAt(k))))
					{
						int toRemove = required_count;
						required_count -= Game1.player.items[j].Stack;
						Game1.player.items[j].Stack -= toRemove;
						if (Game1.player.items[j].Stack <= 0)
						{
							Game1.player.items[j] = null;
						}
						if (required_count <= 0)
						{
							foundInBackpack = true;
							break;
						}
					}
				}
				if (additional_materials != null && !foundInBackpack)
				{
					for (int c = 0; c < additional_materials.Count; c++)
					{
						Chest chest = additional_materials[c];
						if (chest == null)
						{
							continue;
						}
						bool removedItem = false;
						for (int i = chest.items.Count - 1; i >= 0; i--)
						{
							if (chest.items[i] != null && chest.items[i] is Object && ((int)((Object)chest.items[i]).parentSheetIndex == recipeList.Keys.ElementAt(k) || ((Object)chest.items[i]).Category == recipeList.Keys.ElementAt(k) || isThereSpecialIngredientRule((Object)chest.items[i], recipeList.Keys.ElementAt(k))))
							{
								int removed_count = Math.Min(required_count, chest.items[i].Stack);
								required_count -= removed_count;
								chest.items[i].Stack -= removed_count;
								if (chest.items[i].Stack <= 0)
								{
									chest.items[i] = null;
									removedItem = true;
								}
								if (required_count <= 0)
								{
									break;
								}
							}
						}
						if (removedItem)
						{
							chest.clearNulls();
						}
						if (required_count <= 0)
						{
							break;
						}
					}
				}
			}
		}

		public static bool DoesFarmerHaveAdditionalIngredientsInInventory(List<KeyValuePair<int, int>> additional_recipe_items, IList<Item> extraToCheck = null)
		{
			foreach (KeyValuePair<int, int> kvp in additional_recipe_items)
			{
				int required_count3 = kvp.Value;
				required_count3 -= Game1.player.getItemCount(kvp.Key, 5);
				if (required_count3 > 0)
				{
					if (extraToCheck != null)
					{
						required_count3 -= Game1.player.getItemCountInList(extraToCheck, kvp.Key, 5);
						if (required_count3 <= 0)
						{
							continue;
						}
					}
					return false;
				}
			}
			return true;
		}

		public static void ConsumeAdditionalIngredients(List<KeyValuePair<int, int>> additional_recipe_items, List<Chest> additional_materials)
		{
			for (int k = additional_recipe_items.Count - 1; k >= 0; k--)
			{
				int item_index = additional_recipe_items[k].Key;
				int required_count = additional_recipe_items[k].Value;
				bool foundInBackpack = false;
				for (int j = Game1.player.items.Count - 1; j >= 0; j--)
				{
					if (Game1.player.items[j] != null && Game1.player.items[j] is Object && !(Game1.player.items[j] as Object).bigCraftable && ((int)((Object)Game1.player.items[j]).parentSheetIndex == item_index || ((Object)Game1.player.items[j]).Category == item_index || isThereSpecialIngredientRule((Object)Game1.player.items[j], item_index)))
					{
						int toRemove = required_count;
						required_count -= Game1.player.items[j].Stack;
						Game1.player.items[j].Stack -= toRemove;
						if (Game1.player.items[j].Stack <= 0)
						{
							Game1.player.items[j] = null;
						}
						if (required_count <= 0)
						{
							foundInBackpack = true;
							break;
						}
					}
				}
				if (additional_materials != null && !foundInBackpack)
				{
					for (int c = 0; c < additional_materials.Count; c++)
					{
						Chest chest = additional_materials[c];
						if (chest == null)
						{
							continue;
						}
						bool removedItem = false;
						for (int i = chest.items.Count - 1; i >= 0; i--)
						{
							if (chest.items[i] != null && chest.items[i] is Object && ((int)((Object)chest.items[i]).parentSheetIndex == item_index || ((Object)chest.items[i]).Category == item_index || isThereSpecialIngredientRule((Object)chest.items[i], item_index)))
							{
								int removed_count = Math.Min(required_count, chest.items[i].Stack);
								required_count -= removed_count;
								chest.items[i].Stack -= removed_count;
								if (chest.items[i].Stack <= 0)
								{
									chest.items[i] = null;
									removedItem = true;
								}
								if (required_count <= 0)
								{
									break;
								}
							}
						}
						if (removedItem)
						{
							chest.clearNulls();
						}
						if (required_count <= 0)
						{
							break;
						}
					}
				}
			}
		}

		public virtual int getCraftableCount(IList<Chest> additional_material_chests)
		{
			List<Item> additional_items = new List<Item>();
			if (additional_material_chests != null)
			{
				for (int c = 0; c < additional_material_chests.Count; c++)
				{
					additional_items.AddRange(additional_material_chests[c].items);
				}
			}
			return getCraftableCount(additional_items);
		}

		public int getCraftableCount(IList<Item> additional_materials)
		{
			int craftable_count = -1;
			for (int j = recipeList.Count - 1; j >= 0; j--)
			{
				int ingredient_count = 0;
				int required_count = recipeList[recipeList.Keys.ElementAt(j)];
				for (int i = Game1.player.items.Count - 1; i >= 0; i--)
				{
					if (Game1.player.items[i] != null && Game1.player.items[i] is Object && !(Game1.player.items[i] as Object).bigCraftable && ((int)((Object)Game1.player.items[i]).parentSheetIndex == recipeList.Keys.ElementAt(j) || ((Object)Game1.player.items[i]).Category == recipeList.Keys.ElementAt(j) || isThereSpecialIngredientRule((Object)Game1.player.items[i], recipeList.Keys.ElementAt(j))))
					{
						ingredient_count += Game1.player.items[i].Stack;
					}
				}
				if (additional_materials != null)
				{
					for (int c = 0; c < additional_materials.Count; c++)
					{
						Item item = additional_materials[c];
						if (item != null && item is Object && ((Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex) && (int)((Object)item).parentSheetIndex == recipeList.Keys.ElementAt(j)) || ((Object)item).Category == recipeList.Keys.ElementAt(j) || isThereSpecialIngredientRule((Object)item, recipeList.Keys.ElementAt(j))))
						{
							ingredient_count += item.Stack;
						}
					}
				}
				int current_craftable_count = ingredient_count / required_count;
				if (current_craftable_count < craftable_count || craftable_count == -1)
				{
					craftable_count = current_craftable_count;
				}
			}
			return craftable_count;
		}

		public virtual string getCraftCountText()
		{
			if (isCookingRecipe)
			{
				if (Game1.player.recipesCooked.ContainsKey(getIndexOfMenuView()) && Game1.player.recipesCooked[getIndexOfMenuView()] > 0)
				{
					return Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", Game1.player.recipesCooked[getIndexOfMenuView()]);
				}
			}
			else if (Game1.player.craftingRecipes.ContainsKey(name) && Game1.player.craftingRecipes[name] > 0)
			{
				return Game1.content.LoadString("Strings\\UI:Crafting_NumberCrafted", Game1.player.craftingRecipes[name]);
			}
			return null;
		}

		public int getDescriptionHeight(int width)
		{
			return (int)(Game1.smallFont.MeasureString(Game1.parseText(description, Game1.smallFont, width)).Y + (float)(getNumberOfIngredients() * 36) + (float)(int)Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567")).Y + 21f);
		}

		public virtual void drawRecipeDescription(SpriteBatch b, Vector2 position, int width, IList<Item> additional_crafting_items)
		{
			int lineExpansion = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 8 : 0;
			b.Draw(Game1.staminaRect, new Rectangle((int)(position.X + 8f), (int)(position.Y + 32f + Game1.smallFont.MeasureString("Ing!").Y) - 4 - 2 - (int)((float)lineExpansion * 1.5f), width - 32, 2), Game1.textColor * 0.35f);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.567"), Game1.smallFont, position + new Vector2(8f, 28f), Game1.textColor * 0.75f);
			for (int i = 0; i < recipeList.Count; i++)
			{
				int required_count2 = recipeList.Values.ElementAt(i);
				int required_item = recipeList.Keys.ElementAt(i);
				int bag_count = Game1.player.getItemCount(required_item, 8);
				int containers_count = 0;
				required_count2 -= bag_count;
				if (additional_crafting_items != null)
				{
					containers_count = Game1.player.getItemCountInList(additional_crafting_items, required_item, 8);
					if (required_count2 > 0)
					{
						required_count2 -= containers_count;
					}
				}
				string ingredient_name_text = getNameFromIndex(recipeList.Keys.ElementAt(i));
				Color drawColor = (required_count2 <= 0) ? Game1.textColor : Color.Red;
				b.Draw(Game1.objectSpriteSheet, new Vector2(position.X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, getSpriteIndexFromRawIndex(recipeList.Keys.ElementAt(i)), 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.86f);
				Utility.drawTinyDigits(recipeList.Values.ElementAt(i), b, new Vector2(position.X + 32f - Game1.tinyFont.MeasureString(string.Concat(recipeList.Values.ElementAt(i))).X, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 21f), 2f, 0.87f, Color.AntiqueWhite);
				Vector2 text_draw_position = new Vector2(position.X + 32f + 8f, position.Y + 64f + (float)(i * 64 / 2) + (float)(i * 4) + 4f);
				Utility.drawTextWithShadow(b, ingredient_name_text, Game1.smallFont, text_draw_position, drawColor);
				if (Game1.options.showAdvancedCraftingInformation)
				{
					text_draw_position.X = position.X + (float)width - 40f;
					b.Draw(Game1.mouseCursors, new Rectangle((int)text_draw_position.X, (int)text_draw_position.Y + 2, 22, 26), new Rectangle(268, 1436, 11, 13), Color.White);
					Utility.drawTextWithShadow(b, string.Concat(bag_count + containers_count), Game1.smallFont, text_draw_position - new Vector2(Game1.smallFont.MeasureString(bag_count + containers_count + " ").X, 0f), drawColor);
				}
			}
			b.Draw(Game1.staminaRect, new Rectangle((int)position.X + 8, (int)position.Y + lineExpansion + 64 + 4 + recipeList.Count * 36, width - 32, 2), Game1.textColor * 0.35f);
			Utility.drawTextWithShadow(b, Game1.parseText(description, Game1.smallFont, width - 8), Game1.smallFont, position + new Vector2(0f, 76 + recipeList.Count * 36 + lineExpansion), Game1.textColor * 0.75f);
		}

		public virtual int getNumberOfIngredients()
		{
			return recipeList.Count;
		}

		public int getSpriteIndexFromRawIndex(int index)
		{
			switch (index)
			{
			case -1:
				return 20;
			case -2:
				return 80;
			case -3:
				return 24;
			case -4:
				return 145;
			case -5:
				return 176;
			case -6:
				return 184;
			case -777:
				return 495;
			default:
				return index;
			}
		}

		public string getNameFromIndex(int index)
		{
			if (index < 0)
			{
				switch (index)
				{
				case -1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.568");
				case -2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569");
				case -3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570");
				case -4:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571");
				case -5:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572");
				case -6:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573");
				case -777:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.574");
				default:
					return "???";
				}
			}
			string retString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
			if (Game1.objectInformation.ContainsKey(index))
			{
				retString = Game1.objectInformation[index].Split('/')[4];
			}
			return retString;
		}
	}
}
