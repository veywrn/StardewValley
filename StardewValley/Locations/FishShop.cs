using StardewValley.BellsAndWhistles;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class FishShop : ShopLocation
	{
		public FishShop()
		{
		}

		public FishShop(string map, string name)
			: base(map, name)
		{
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
			case 4:
				response = ((!(Game1.random.NextDouble() < (double)(int)i.quality * 0.5 + 0.2)) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomNegativeFoodAdjective(n)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_1_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName, Lexicon.getRandomDeliciousAdjective(n)));
				break;
			case 1:
				response = (((int)i.quality != 0) ? ((!n.Name.Equals("Jodi")) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityHigh_Jodi_Willy", whatToCallPlayer, particle, i.DisplayName)) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_2_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName));
				break;
			case 2:
				if (n.Manners == 2)
				{
					if ((int)i.quality != 2)
					{
						response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityLow_Rude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomNegativeFoodAdjective(n), Lexicon.getRandomNegativeItemSlanderNoun());
					}
					else
					{
						Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_QualityHigh_Rude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2, Lexicon.getRandomSlightlyPositiveAdjectiveForEdibleNoun(n));
					}
				}
				else
				{
					Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_3_NonRude_Willy", whatToCallPlayer, particle, i.DisplayName, i.salePrice() / 2);
				}
				break;
			case 3:
				response = Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_4_Willy", whatToCallPlayer, particle, i.DisplayName);
				break;
			}
			string name = n.Name;
			if (name == "Willy")
			{
				response = (((int)i.quality != 0) ? Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityHigh_Willy", whatToCallPlayer, particle, i.DisplayName) : Game1.content.LoadString("Data\\ExtraDialogue:PurchasedItem_Pierre_QualityLow_Willy", whatToCallPlayer, particle, i.DisplayName));
			}
			return response;
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action == "WarpBoatTunnel")
			{
				if (Game1.player.mailReceived.Contains("willyBackRoomInvitation"))
				{
					Game1.warpFarmer("BoatTunnel", 6, 12, flip: false);
					playSound("doorClose");
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:LockedDoor"));
				}
			}
			return base.performAction(action, who, tileLocation);
		}
	}
}
