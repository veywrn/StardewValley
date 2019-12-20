using System;
using System.Linq;

namespace StardewValley.BellsAndWhistles
{
	public class Lexicon
	{
		public static string getRandomNegativeItemSlanderNoun()
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomNegativeItemNoun").Split('#');
			return choices[r.Next(choices.Length)];
		}

		public static string getProperArticleForWord(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return "";
			}
			string article = "a";
			if (word != null && word.Length > 0)
			{
				switch (word.ToLower()[0])
				{
				case 'a':
					article += "n";
					break;
				case 'e':
					article += "n";
					break;
				case 'i':
					article += "n";
					break;
				case 'o':
					article += "n";
					break;
				case 'u':
					article += "n";
					break;
				}
			}
			return article;
		}

		public static string capitalize(string text)
		{
			if (text == null || text == "" || LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return text;
			}
			int positionOfFirstCapitalizableCharacter = 0;
			for (int i = 0; i < text.Length; i++)
			{
				if (char.IsLetter(text[i]))
				{
					positionOfFirstCapitalizableCharacter = i;
					break;
				}
			}
			if (positionOfFirstCapitalizableCharacter == 0)
			{
				return text.First().ToString().ToUpper() + text.Substring(1);
			}
			return text.Substring(0, positionOfFirstCapitalizableCharacter) + text[positionOfFirstCapitalizableCharacter].ToString().ToUpper() + text.Substring(positionOfFirstCapitalizableCharacter + 1);
		}

		public static string makePlural(string word, bool ignore = false)
		{
			if (ignore || LocalizedContentManager.CurrentLanguageCode != 0 || word == null)
			{
				return word;
			}
			switch (word)
			{
			case "Rice Pudding":
				return "bowls of Rice Pudding";
			case "Algae Soup":
				return "bowls of Algae Soup";
			case "Coal":
				return "lumps of Coal";
			case "Salt":
				return "pieces of Salt";
			case "Jelly":
				return "Jellies";
			case "Driftwood":
			case "Mixed Seeds":
			case "Crab Cakes":
			case "Largemouth Bass":
			case "Sandfish":
			case "Carp":
			case "Bream":
			case "Chub":
			case "Ghostfish":
			case "Pickles":
				return word;
			default:
				if (word.Last() == 's' || word.Last() == 'z' || word.Last() == 'x' || (word.Length > 2 && word.Substring(word.Length - 2) == "sh") || (word.Length > 2 && word.Substring(word.Length - 2) == "ch"))
				{
					return word + "es";
				}
				return word + "s";
			}
		}

		public static string prependArticle(string word)
		{
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return word;
			}
			return getProperArticleForWord(word) + " " + word;
		}

		public static string getRandomPositiveAdjectiveForEventOrPerson(NPC n = null)
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = (n != null && n.Age != 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_Child").Split('#') : ((n != null && n.Gender == 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultMale").Split('#') : ((n == null || n.Gender != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_PlaceOrEvent").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomPositiveAdjective_AdultFemale").Split('#')));
			return choices[r.Next(choices.Length)];
		}

		public static string getRandomNegativeAdjectiveForEventOrPerson(NPC n = null)
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = (n != null && n.Age != 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_Child").Split('#') : ((n != null && n.Gender == 0) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_AdultMale").Split('#') : ((n == null || n.Gender != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_PlaceOrEvent").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomNegativeAdjective_AdultFemale").Split('#')));
			return choices[r.Next(choices.Length)];
		}

		public static string getRandomDeliciousAdjective(NPC n = null)
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = (n == null || n.Age != 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomDeliciousAdjective_Child").Split('#');
			return choices[r.Next(choices.Length)];
		}

		public static string getRandomNegativeFoodAdjective(NPC n = null)
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = (n != null && n.Age == 2) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Child").Split('#') : ((n == null || n.Manners != 1) ? Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective").Split('#') : Game1.content.LoadString("Strings\\Lexicon:RandomNegativeFoodAdjective_Polite").Split('#'));
			return choices[r.Next(choices.Length)];
		}

		public static string getRandomSlightlyPositiveAdjectiveForEdibleNoun(NPC n = null)
		{
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			string[] choices = Game1.content.LoadString("Strings\\Lexicon:RandomSlightlyPositiveFoodAdjective").Split('#');
			return choices[r.Next(choices.Length)];
		}

		public static string getGenderedChildTerm(bool isMale)
		{
			if (isMale)
			{
				return Game1.content.LoadString("Strings\\Lexicon:ChildTerm_Male");
			}
			return Game1.content.LoadString("Strings\\Lexicon:ChildTerm_Female");
		}

		public static string getPronoun(bool isMale)
		{
			if (isMale)
			{
				return Game1.content.LoadString("Strings\\Lexicon:Pronoun_Male");
			}
			return Game1.content.LoadString("Strings\\Lexicon:Pronoun_Female");
		}
	}
}
