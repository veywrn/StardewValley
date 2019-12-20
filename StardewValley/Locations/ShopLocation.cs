using Netcode;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Locations
{
	public class ShopLocation : GameLocation
	{
		public const int maxItemsToSellFromPlayer = 11;

		public readonly NetObjectList<Item> itemsFromPlayerToSell = new NetObjectList<Item>();

		public readonly NetObjectList<Item> itemsToStartSellingTomorrow = new NetObjectList<Item>();

		public ShopLocation()
		{
		}

		public ShopLocation(string map, string name)
			: base(map, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(itemsFromPlayerToSell, itemsToStartSellingTomorrow);
		}

		public virtual string getPurchasedItemDialogueForNPC(Object i, NPC n)
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

		public override void DayUpdate(int dayOfMonth)
		{
			for (int i = itemsToStartSellingTomorrow.Count - 1; i >= 0; i--)
			{
				Item tomorrowItem = itemsToStartSellingTomorrow[i];
				if (itemsFromPlayerToSell.Count < 11)
				{
					bool stacked = false;
					foreach (Item item in itemsFromPlayerToSell)
					{
						if (item.Name.Equals(tomorrowItem.Name) && (item as Object).quality == (tomorrowItem as Object).quality)
						{
							item.Stack += tomorrowItem.Stack;
							stacked = true;
							break;
						}
					}
					itemsToStartSellingTomorrow.RemoveAt(i);
					if (!stacked)
					{
						itemsFromPlayerToSell.Add(tomorrowItem);
					}
				}
			}
			base.DayUpdate(dayOfMonth);
		}
	}
}
