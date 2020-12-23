using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace StardewValley.Locations
{
	public class SeedShop : ShopLocation
	{
		protected bool _stockListGranted;

		public SeedShop()
		{
		}

		public SeedShop(string map, string name)
			: base(map, name)
		{
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if ((int)Game1.player.maxItems == 12)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(255, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
			}
			else if ((int)Game1.player.maxItems < 36)
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2(456f, 1088f)), new Rectangle(267, 1436, 12, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(452, 1184, 112, 20)), new Rectangle(258, 1449, 1, 1), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1232f);
			}
		}

		public override string getPurchasedItemDialogueForNPC(Object i, NPC n)
		{
			string response = "...";
			string[] split = Game1.content.LoadString("Strings\\Lexicon:GenericPlayerTerm").Split('^');
			string genderName = split[0];
			if (split.Length > 1 && !Game1.player.isMale)
			{
				genderName = split[1];
			}
			string whatToCallPlayer = (Game1.random.NextDouble() < (double)(Game1.player.getFriendshipLevelForNPC(n.Name) / 1250)) ? Game1.player.Name : genderName;
			if (n.Age != 0)
			{
				whatToCallPlayer = Game1.player.Name;
			}
			string particle = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? Lexicon.getProperArticleForWord(i.name) : "";
			if ((i.Category == -4 || i.Category == -75 || i.Category == -79) && Game1.random.NextDouble() < 0.5)
			{
				particle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SeedShop.cs.9701");
			}
			int whichDialogue = Game1.random.Next(5);
			if (n.Manners == 2)
			{
				whichDialogue = 2;
			}
			switch (whichDialogue)
			{
			case 0:
				response = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityLow", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityHigh", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
				break;
			case 1:
				response = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi", whatToCallPlayer, particle, i.DisplayName)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityLow", whatToCallPlayer, particle, i.DisplayName));
				break;
			case 2:
				if (n.Manners == 2)
				{
					if ((int)i.quality != 2)
					{
						response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun());
					}
					else
					{
						Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n));
					}
				}
				else
				{
					Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_NonRude", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2);
				}
				break;
			case 3:
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_4", whatToCallPlayer, particle, i.DisplayName);
				break;
			case 4:
				if (i.Category == -75 || i.Category == -79)
				{
					response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_VegetableOrFruit", whatToCallPlayer, particle, i.DisplayName);
				}
				else if (i.Category == -7)
				{
					string adjective = Lexicon.getRandomPositiveAdjectiveForEventOrPerson(n);
					response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Cooking", whatToCallPlayer, particle, i.DisplayName, Lexicon.getProperArticleForWord(adjective), adjective);
				}
				else
				{
					response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_5_Foraged", whatToCallPlayer, particle, i.DisplayName);
				}
				break;
			}
			if (n.Age == 1 && Game1.random.NextDouble() < 0.6)
			{
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Teen", whatToCallPlayer, particle, i.DisplayName);
			}
			switch (n.Name)
			{
			case "Abigail":
				response = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Abigail_QualityLow", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeItemSlanderNoun()));
				break;
			case "Caroline":
				response = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Caroline_QualityLow", whatToCallPlayer, particle, i.DisplayName));
				break;
			case "Pierre":
				response = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow", whatToCallPlayer, particle, i.DisplayName));
				break;
			case "Haley":
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Haley", whatToCallPlayer, particle, i.DisplayName);
				break;
			case "Elliott":
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Elliott", whatToCallPlayer, particle, i.DisplayName);
				break;
			case "Alex":
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Alex", whatToCallPlayer, particle, i.DisplayName);
				break;
			case "Leah":
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Leah", whatToCallPlayer, particle, i.DisplayName);
				break;
			}
			return response;
		}

		private void addStock(Dictionary<ISalable, int[]> stock, int parentSheetIndex, int buyPrice = -1, string item_season = null)
		{
			float price_multiplier = 2f;
			int price2 = buyPrice;
			Object obj = new Object(Vector2.Zero, parentSheetIndex, 1);
			if (buyPrice == -1)
			{
				price2 = obj.salePrice();
				price_multiplier = 1f;
			}
			if (item_season != null && item_season != Game1.currentSeason)
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist"))
				{
					return;
				}
				price_multiplier *= 1.5f;
			}
			price2 = (int)((float)price2 * price_multiplier);
			if (item_season != null)
			{
				foreach (KeyValuePair<ISalable, int[]> item in stock)
				{
					if (item.Key != null && item.Key is Object)
					{
						Object existing_item = item.Key as Object;
						if (Utility.IsNormalObjectAtParentSheetIndex(existing_item, parentSheetIndex))
						{
							if (item.Value.Length != 0 && price2 < item.Value[0])
							{
								item.Value[0] = price2;
								stock[existing_item] = item.Value;
							}
							return;
						}
					}
				}
			}
			stock.Add(obj, new int[2]
			{
				price2,
				2147483647
			});
		}

		public Dictionary<ISalable, int[]> shopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			addStock(stock, 472, -1, "spring");
			addStock(stock, 473, -1, "spring");
			addStock(stock, 474, -1, "spring");
			addStock(stock, 475, -1, "spring");
			addStock(stock, 427, -1, "spring");
			addStock(stock, 477, -1, "spring");
			addStock(stock, 429, -1, "spring");
			if (Game1.year > 1)
			{
				addStock(stock, 476, -1, "spring");
				addStock(stock, 273, -1, "spring");
			}
			addStock(stock, 479, -1, "summer");
			addStock(stock, 480, -1, "summer");
			addStock(stock, 481, -1, "summer");
			addStock(stock, 482, -1, "summer");
			addStock(stock, 483, -1, "summer");
			addStock(stock, 484, -1, "summer");
			addStock(stock, 453, -1, "summer");
			addStock(stock, 455, -1, "summer");
			addStock(stock, 302, -1, "summer");
			addStock(stock, 487, -1, "summer");
			addStock(stock, 431, 100, "summer");
			if (Game1.year > 1)
			{
				addStock(stock, 485, -1, "summer");
			}
			addStock(stock, 490, -1, "fall");
			addStock(stock, 487, -1, "fall");
			addStock(stock, 488, -1, "fall");
			addStock(stock, 491, -1, "fall");
			addStock(stock, 492, -1, "fall");
			addStock(stock, 493, -1, "fall");
			addStock(stock, 483, -1, "fall");
			addStock(stock, 431, 100, "fall");
			addStock(stock, 425, -1, "fall");
			addStock(stock, 299, -1, "fall");
			addStock(stock, 301, -1, "fall");
			if (Game1.year > 1)
			{
				addStock(stock, 489, -1, "fall");
			}
			addStock(stock, 297);
			if (!Game1.player.craftingRecipes.ContainsKey("Grass Starter"))
			{
				stock.Add(new Object(297, 1, isRecipe: true), new int[2]
				{
					1000,
					1
				});
			}
			addStock(stock, 245);
			addStock(stock, 246);
			addStock(stock, 423);
			addStock(stock, 247);
			addStock(stock, 419);
			if ((int)Game1.stats.DaysPlayed >= 15)
			{
				addStock(stock, 368, 50);
				addStock(stock, 370, 50);
				addStock(stock, 465, 50);
			}
			if (Game1.year > 1)
			{
				addStock(stock, 369, 75);
				addStock(stock, 371, 75);
				addStock(stock, 466, 75);
			}
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			int wp = r.Next(112);
			if (wp == 21)
			{
				wp = 36;
			}
			Wallpaper wallpaper2 = new Wallpaper(wp);
			stock.Add(wallpaper2, new int[2]
			{
				wallpaper2.salePrice(),
				2147483647
			});
			wallpaper2 = new Wallpaper(r.Next(56), isFloor: true);
			stock.Add(wallpaper2, new int[2]
			{
				wallpaper2.salePrice(),
				2147483647
			});
			Furniture furniture = new Furniture(1308, Vector2.Zero);
			stock.Add(furniture, new int[2]
			{
				furniture.salePrice(),
				2147483647
			});
			addStock(stock, 628, 1700);
			addStock(stock, 629, 1000);
			addStock(stock, 630, 2000);
			addStock(stock, 631, 3000);
			addStock(stock, 632, 3000);
			addStock(stock, 633, 2000);
			foreach (Item i in itemsFromPlayerToSell)
			{
				if (i.Stack > 0)
				{
					int price = i.salePrice();
					if (i is Object)
					{
						price = (i as Object).sellToStorePrice(-1L);
					}
					stock.Add(i, new int[2]
					{
						price,
						i.Stack
					});
				}
			}
			if (Game1.player.hasAFriendWithHeartLevel(8, datablesOnly: true))
			{
				addStock(stock, 458);
			}
			return stock;
		}
	}
}
