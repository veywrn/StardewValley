using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using xTile.Dimensions;

namespace StardewValley
{
	public class Utility
	{
		public static readonly char[] CharSpace = new char[1]
		{
			' '
		};

		public static Color[] PRISMATIC_COLORS = new Color[6]
		{
			Color.Red,
			new Color(255, 120, 0),
			new Color(255, 217, 0),
			Color.Lime,
			Color.Cyan,
			Color.Violet
		};

		public static List<VertexPositionColor[]> straightLineVertex = new List<VertexPositionColor[]>
		{
			new VertexPositionColor[2],
			new VertexPositionColor[2],
			new VertexPositionColor[2],
			new VertexPositionColor[2]
		};

		private static readonly ListPool<NPC> _pool = new ListPool<NPC>();

		public static readonly Vector2[] DirectionsTileVectors = new Vector2[4]
		{
			new Vector2(0f, -1f),
			new Vector2(1f, 0f),
			new Vector2(0f, 1f),
			new Vector2(-1f, 0f)
		};

		public static readonly RasterizerState ScissorEnabled = new RasterizerState
		{
			ScissorTestEnable = true
		};

		public static Microsoft.Xna.Framework.Rectangle controllerMapSourceRect(Microsoft.Xna.Framework.Rectangle xboxSourceRect)
		{
			return xboxSourceRect;
		}

		public static char getRandomSlotCharacter()
		{
			return getRandomSlotCharacter('o');
		}

		public static List<Vector2> removeDuplicates(List<Vector2> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = list.Count - 1; j >= 0; j--)
				{
					if (j != i && list[i].Equals(list[j]))
					{
						list.RemoveAt(j);
					}
				}
			}
			return list;
		}

		public static int GetHorseWarpRestrictionsForFarmer(Farmer who)
		{
			if (who.horseName.Value == null)
			{
				return 1;
			}
			GameLocation location = who.currentLocation;
			if (!location.IsOutdoors)
			{
				return 2;
			}
			Microsoft.Xna.Framework.Rectangle horse_check_rect = new Microsoft.Xna.Framework.Rectangle(who.getTileX() * 64, who.getTileY() * 64, 128, 64);
			if (location.isCollidingPosition(horse_check_rect, Game1.viewport, isFarmer: true, 0, glider: false, who))
			{
				return 3;
			}
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				if (farmer.mount != null && farmer.mount.getOwner() == who)
				{
					return 4;
				}
			}
			return 0;
		}

		public static Microsoft.Xna.Framework.Rectangle ConstrainScissorRectToScreen(Microsoft.Xna.Framework.Rectangle scissor_rect)
		{
			int amount_to_trim5 = 0;
			if (scissor_rect.Top < 0)
			{
				amount_to_trim5 = -scissor_rect.Top;
				scissor_rect.Height -= amount_to_trim5;
				scissor_rect.Y += amount_to_trim5;
			}
			if (scissor_rect.Bottom > Game1.viewport.Height)
			{
				amount_to_trim5 = scissor_rect.Bottom - Game1.viewport.Height;
				scissor_rect.Height -= amount_to_trim5;
			}
			if (scissor_rect.Left < 0)
			{
				amount_to_trim5 = -scissor_rect.Left;
				scissor_rect.Width -= amount_to_trim5;
				scissor_rect.X += amount_to_trim5;
			}
			if (scissor_rect.Right > Game1.viewport.Width)
			{
				amount_to_trim5 = scissor_rect.Right - Game1.viewport.Width;
				scissor_rect.Width -= amount_to_trim5;
			}
			return scissor_rect;
		}

		public static void RecordAnimalProduce(FarmAnimal animal, int produce)
		{
			if (animal.type.Contains("Cow"))
			{
				Game1.stats.CowMilkProduced++;
			}
			else if (animal.type.Contains("Sheep"))
			{
				Game1.stats.SheepWoolProduced++;
			}
			else if (animal.type.Contains("Goat"))
			{
				Game1.stats.GoatMilkProduced++;
			}
		}

		public static Point Vector2ToPoint(Vector2 v)
		{
			return new Point((int)v.X, (int)v.Y);
		}

		public static Vector2 PointToVector2(Point p)
		{
			return new Vector2(p.X, p.Y);
		}

		public static int getStartTimeOfFestival()
		{
			if (Game1.weatherIcon == 1)
			{
				return Convert.ToInt32(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1].Split(' ')[0]);
			}
			return -1;
		}

		public static bool doesMasterPlayerHaveMailReceivedButNotMailForTomorrow(string mailID)
		{
			if (Game1.MasterPlayer.mailReceived.Contains(mailID) || Game1.MasterPlayer.mailReceived.Contains(mailID + "%&NL&%"))
			{
				if (!Game1.MasterPlayer.mailForTomorrow.Contains(mailID))
				{
					return !Game1.MasterPlayer.mailForTomorrow.Contains(mailID + "%&NL&%");
				}
				return false;
			}
			return false;
		}

		public static bool isFestivalDay(int day, string season)
		{
			string s = season + day;
			return Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\FestivalDates").ContainsKey(s);
		}

		public static void ForAllLocations(Action<GameLocation> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				action(location);
				if (location is BuildableGameLocation)
				{
					foreach (Building building in (location as BuildableGameLocation).buildings)
					{
						if (building.indoors.Value != null)
						{
							action(building.indoors.Value);
						}
					}
				}
			}
		}

		public static int getNumObjectsOfIndexWithinRectangle(Microsoft.Xna.Framework.Rectangle r, int[] indexes, GameLocation location)
		{
			int count = 0;
			Vector2 v = Vector2.Zero;
			for (int y = r.Y; y < r.Bottom + 1; y++)
			{
				v.Y = y;
				for (int x = r.X; x < r.Right + 1; x++)
				{
					v.X = x;
					if (!location.objects.ContainsKey(v))
					{
						continue;
					}
					for (int i = 0; i < indexes.Length; i++)
					{
						if (indexes[i] == (int)location.objects[v].parentSheetIndex || indexes[i] == -1)
						{
							count++;
							break;
						}
					}
				}
			}
			return count;
		}

		public static string fuzzySearch(string query, List<string> word_bank)
		{
			foreach (string term4 in word_bank)
			{
				if (query.Trim() == term4.Trim())
				{
					return term4;
				}
			}
			foreach (string term3 in word_bank)
			{
				if (_formatForFuzzySearch(query) == _formatForFuzzySearch(term3))
				{
					return term3;
				}
			}
			foreach (string term2 in word_bank)
			{
				if (_formatForFuzzySearch(term2).StartsWith(_formatForFuzzySearch(query)))
				{
					return term2;
				}
			}
			foreach (string term in word_bank)
			{
				if (_formatForFuzzySearch(term).Contains(_formatForFuzzySearch(query)))
				{
					return term;
				}
			}
			return null;
		}

		protected static string _formatForFuzzySearch(string term)
		{
			return term.Trim().ToLowerInvariant().Replace(" ", "")
				.Replace("(", "")
				.Replace(")", "")
				.Replace("'", "")
				.Replace(".", "")
				.Replace("!", "")
				.Replace("?", "")
				.Replace("-", "");
		}

		public static Item fuzzyItemSearch(string query, int stack_count = 1)
		{
			Dictionary<string, string> items = new Dictionary<string, string>();
			foreach (int key7 in Game1.objectInformation.Keys)
			{
				string item_data7 = Game1.objectInformation[key7];
				if (item_data7 != null)
				{
					string[] data = item_data7.Split('/');
					string item_name7 = data[0];
					if (!(item_name7 == "Stone") || key7 == 390)
					{
						if (data[3] == "Ring")
						{
							items[item_name7] = "R " + key7 + " " + stack_count;
						}
						else
						{
							items[item_name7] = "O " + key7 + " " + stack_count;
						}
					}
				}
			}
			foreach (int key6 in Game1.bigCraftablesInformation.Keys)
			{
				string item_data6 = Game1.bigCraftablesInformation[key6];
				if (item_data6 != null)
				{
					string item_name6 = item_data6.Substring(0, item_data6.IndexOf('/'));
					items[item_name6] = "BO " + key6 + " " + stack_count;
				}
			}
			Dictionary<int, string> furniture_data = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			foreach (int key5 in furniture_data.Keys)
			{
				string item_data5 = furniture_data[key5];
				if (item_data5 != null)
				{
					string item_name5 = item_data5.Substring(0, item_data5.IndexOf('/'));
					items[item_name5] = "F " + key5 + " " + stack_count;
				}
			}
			Dictionary<int, string> weapon_data = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			foreach (int key4 in weapon_data.Keys)
			{
				string item_data4 = weapon_data[key4];
				if (item_data4 != null)
				{
					string item_name4 = item_data4.Substring(0, item_data4.IndexOf('/'));
					items[item_name4] = "W " + key4 + " " + stack_count;
				}
			}
			Dictionary<int, string> boot_data = Game1.content.Load<Dictionary<int, string>>("Data\\Boots");
			foreach (int key3 in boot_data.Keys)
			{
				string item_data3 = boot_data[key3];
				if (item_data3 != null)
				{
					string item_name3 = item_data3.Substring(0, item_data3.IndexOf('/'));
					items[item_name3] = "B " + key3 + " " + stack_count;
				}
			}
			Dictionary<int, string> hat_data = Game1.content.Load<Dictionary<int, string>>("Data\\hats");
			foreach (int key2 in hat_data.Keys)
			{
				string item_data2 = hat_data[key2];
				if (item_data2 != null)
				{
					string item_name2 = item_data2.Substring(0, item_data2.IndexOf('/'));
					items[item_name2] = "H " + key2 + " " + stack_count;
				}
			}
			foreach (int key in Game1.clothingInformation.Keys)
			{
				string item_data = Game1.clothingInformation[key];
				if (item_data != null)
				{
					string item_name = item_data.Substring(0, item_data.IndexOf('/'));
					items[item_name] = "C " + key + " " + stack_count;
				}
			}
			string result = fuzzySearch(query, items.Keys.ToList());
			if (result != null)
			{
				return getItemFromStandardTextDescription(items[result], null);
			}
			return null;
		}

		public static GameLocation fuzzyLocationSearch(string query)
		{
			Dictionary<string, GameLocation> name_bank = new Dictionary<string, GameLocation>();
			foreach (GameLocation location in Game1.locations)
			{
				name_bank[location.NameOrUniqueName] = location;
				if (location is BuildableGameLocation)
				{
					foreach (Building building in (location as BuildableGameLocation).buildings)
					{
						if (building.indoors.Value != null)
						{
							name_bank[building.indoors.Value.NameOrUniqueName] = building.indoors.Value;
						}
					}
				}
			}
			string location_name = fuzzySearch(query, name_bank.Keys.ToList());
			if (location_name != null)
			{
				return name_bank[location_name];
			}
			return null;
		}

		public static string AOrAn(string text)
		{
			if (text != null && text.Length > 0)
			{
				char letter = text.ToLowerInvariant()[0];
				if (letter == 'a' || letter == 'e' || letter == 'i' || letter == 'o' || letter == 'u')
				{
					if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
					{
						return "az";
					}
					return "an";
				}
			}
			return "a";
		}

		public static void getDefaultWarpLocation(string location_name, ref int x, ref int y)
		{
			switch (location_name)
			{
			case "LeoTreeHouse":
				x = 3;
				y = 4;
				break;
			case "QiNutRoom":
				x = 7;
				y = 7;
				break;
			case "Greenhouse":
				x = 9;
				y = 8;
				break;
			case "Caldera":
				x = 21;
				y = 30;
				break;
			case "IslandFarmCave":
				x = 4;
				y = 10;
				break;
			case "IslandShrine":
				x = 16;
				y = 28;
				break;
			case "IslandSouthEastCave":
				x = 2;
				y = 7;
				break;
			case "IslandFarmHouse":
				x = 14;
				y = 17;
				break;
			case "IslandFieldOffice":
				x = 8;
				y = 8;
				break;
			case "IslandHut":
				x = 7;
				y = 11;
				break;
			case "IslandWest":
				x = 77;
				y = 40;
				break;
			case "IslandNorth":
				x = 36;
				y = 89;
				break;
			case "IslandSouth":
				x = 21;
				y = 43;
				break;
			case "IslandEast":
				x = 21;
				y = 37;
				break;
			case "IslandSouthEast":
				x = 0;
				y = 28;
				break;
			case "IslandSecret":
				x = 80;
				y = 68;
				break;
			case "AbandonedJojaMart":
				x = 9;
				y = 12;
				break;
			case "AdventureGuild":
				x = 4;
				y = 13;
				break;
			case "AnimalShop":
				x = 12;
				y = 16;
				break;
			case "ArchaeologyHouse":
				x = 3;
				y = 10;
				break;
			case "Backwoods":
				x = 18;
				y = 18;
				break;
			case "Barn":
			case "Barn2":
			case "Barn3":
				x = 11;
				y = 13;
				break;
			case "BoatTunnel":
				x = 6;
				y = 11;
				break;
			case "BathHouse_Entry":
				x = 5;
				y = 8;
				break;
			case "BathHouse_MensLocker":
				x = 15;
				y = 16;
				break;
			case "BathHouse_Pool":
				x = 13;
				y = 5;
				break;
			case "BathHouse_WomensLocker":
				x = 2;
				y = 14;
				break;
			case "Beach":
				x = 39;
				y = 1;
				break;
			case "Blacksmith":
				x = 3;
				y = 15;
				break;
			case "BugLand":
				x = 14;
				y = 52;
				break;
			case "BusStop":
				x = 14;
				y = 23;
				break;
			case "Club":
				x = 8;
				y = 11;
				break;
			case "CommunityCenter":
				x = 32;
				y = 13;
				break;
			case "Coop":
			case "Coop2":
			case "Coop3":
				x = 2;
				y = 8;
				break;
			case "Desert":
				x = 35;
				y = 43;
				break;
			case "ElliottHouse":
				x = 3;
				y = 9;
				break;
			case "Farm":
				x = 64;
				y = 15;
				break;
			case "FishShop":
				x = 6;
				y = 6;
				break;
			case "Forest":
				x = 27;
				y = 12;
				break;
			case "HaleyHouse":
				x = 2;
				y = 23;
				break;
			case "HarveyRoom":
				x = 6;
				y = 11;
				break;
			case "Hospital":
				x = 10;
				y = 18;
				break;
			case "JojaMart":
				x = 13;
				y = 28;
				break;
			case "JoshHouse":
				x = 9;
				y = 20;
				break;
			case "LeahHouse":
				x = 7;
				y = 9;
				break;
			case "ManorHouse":
				x = 4;
				y = 10;
				break;
			case "MermaidHouse":
				x = 4;
				y = 9;
				break;
			case "Mine":
				x = 13;
				y = 10;
				break;
			case "Mountain":
				x = 40;
				y = 13;
				break;
			case "MovieTheater":
				x = 8;
				y = 9;
				break;
			case "Railroad":
				x = 29;
				y = 58;
				break;
			case "Saloon":
				x = 18;
				y = 20;
				break;
			case "SamHouse":
				x = 4;
				y = 15;
				break;
			case "SandyHouse":
				x = 2;
				y = 7;
				break;
			case "ScienceHouse":
				x = 8;
				y = 20;
				break;
			case "SeedShop":
				x = 4;
				y = 19;
				break;
			case "Sewer":
				x = 31;
				y = 18;
				break;
			case "SlimeHutch":
				x = 8;
				y = 18;
				break;
			case "SkullCave":
				x = 3;
				y = 4;
				break;
			case "Submarine":
				x = 14;
				y = 14;
				break;
			case "Sunroom":
				x = 5;
				y = 12;
				break;
			case "Tent":
				x = 2;
				y = 4;
				break;
			case "Town":
				x = 29;
				y = 67;
				break;
			case "Trailer":
				x = 12;
				y = 9;
				break;
			case "Trailer_Big":
				x = 13;
				y = 23;
				break;
			case "Tunnel":
				x = 17;
				y = 7;
				break;
			case "WitchHut":
				x = 7;
				y = 14;
				break;
			case "WitchSwamp":
				x = 20;
				y = 30;
				break;
			case "WitchWarpCave":
				x = 4;
				y = 8;
				break;
			case "WizardHouse":
				x = 6;
				y = 18;
				break;
			case "WizardHouseBasement":
				x = 4;
				y = 4;
				break;
			case "Woods":
				x = 8;
				y = 9;
				break;
			}
		}

		public static FarmAnimal fuzzyAnimalSearch(string query)
		{
			List<FarmAnimal> animals = new List<FarmAnimal>();
			foreach (GameLocation location in Game1.locations)
			{
				if (location is IAnimalLocation)
				{
					animals.AddRange((location as IAnimalLocation).Animals.Values);
				}
				if (location is BuildableGameLocation)
				{
					foreach (Building building in (location as BuildableGameLocation).buildings)
					{
						if (building.indoors.Value != null && building.indoors.Value is IAnimalLocation)
						{
							animals.AddRange((building.indoors.Value as IAnimalLocation).Animals.Values);
						}
					}
				}
			}
			Dictionary<string, FarmAnimal> name_bank = new Dictionary<string, FarmAnimal>();
			foreach (FarmAnimal animal in animals)
			{
				name_bank[animal.Name] = animal;
			}
			string character_name = fuzzySearch(query, name_bank.Keys.ToList());
			if (character_name != null)
			{
				return name_bank[character_name];
			}
			return null;
		}

		public static NPC fuzzyCharacterSearch(string query, bool must_be_villager = true)
		{
			List<NPC> list = new List<NPC>();
			getAllCharacters(list);
			Dictionary<string, NPC> name_bank = new Dictionary<string, NPC>();
			foreach (NPC character in list)
			{
				if (!must_be_villager || character.isVillager())
				{
					name_bank[character.Name] = character;
				}
			}
			string character_name = fuzzySearch(query, name_bank.Keys.ToList());
			if (character_name != null)
			{
				return name_bank[character_name];
			}
			return null;
		}

		public static Color GetPrismaticColor(int offset = 0, float speedMultiplier = 1f)
		{
			float interval = 1500f;
			int current_index = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval) + offset) % PRISMATIC_COLORS.Length;
			int next_index = (current_index + 1) % PRISMATIC_COLORS.Length;
			float position = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval % 1f;
			Color prismatic_color = default(Color);
			prismatic_color.R = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].R / 255f, (float)(int)PRISMATIC_COLORS[next_index].R / 255f, position) * 255f);
			prismatic_color.G = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].G / 255f, (float)(int)PRISMATIC_COLORS[next_index].G / 255f, position) * 255f);
			prismatic_color.B = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].B / 255f, (float)(int)PRISMATIC_COLORS[next_index].B / 255f, position) * 255f);
			prismatic_color.A = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].A / 255f, (float)(int)PRISMATIC_COLORS[next_index].A / 255f, position) * 255f);
			return prismatic_color;
		}

		public static Color Get2PhaseColor(Color color1, Color color2, int offset = 0, float speedMultiplier = 1f, float timeOffset = 0f)
		{
			float interval = 1500f;
			int num = ((int)((float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval) + offset) % 2;
			_ = (num + 1) % 2;
			float position = (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval % 1f;
			Color prismatic_color = default(Color);
			Color a = (num == 0) ? color1 : color2;
			Color b = (num == 0) ? color2 : color1;
			prismatic_color.R = (byte)(Lerp((float)(int)a.R / 255f, (float)(int)b.R / 255f, position) * 255f);
			prismatic_color.G = (byte)(Lerp((float)(int)a.G / 255f, (float)(int)b.G / 255f, position) * 255f);
			prismatic_color.B = (byte)(Lerp((float)(int)a.B / 255f, (float)(int)b.B / 255f, position) * 255f);
			prismatic_color.A = (byte)(Lerp((float)(int)a.A / 255f, (float)(int)b.A / 255f, position) * 255f);
			return prismatic_color;
		}

		public static bool IsNormalObjectAtParentSheetIndex(Item item, int index)
		{
			if (item == null)
			{
				return false;
			}
			if (item.GetType() != typeof(Object))
			{
				return false;
			}
			if ((item as Object).bigCraftable.Value)
			{
				return false;
			}
			if (item.ParentSheetIndex != index)
			{
				return false;
			}
			return true;
		}

		public static bool isObjectOffLimitsForSale(int index)
		{
			switch (index)
			{
			case 69:
			case 73:
			case 79:
			case 91:
			case 158:
			case 159:
			case 160:
			case 161:
			case 162:
			case 163:
			case 261:
			case 277:
			case 279:
			case 289:
			case 292:
			case 305:
			case 308:
			case 326:
			case 341:
			case 413:
			case 417:
			case 437:
			case 439:
			case 447:
			case 454:
			case 460:
			case 645:
			case 680:
			case 681:
			case 682:
			case 688:
			case 689:
			case 690:
			case 774:
			case 775:
			case 797:
			case 798:
			case 799:
			case 800:
			case 801:
			case 802:
			case 803:
			case 807:
			case 812:
				return true;
			default:
				return false;
			}
		}

		public static Microsoft.Xna.Framework.Rectangle xTileToMicrosoftRectangle(xTile.Dimensions.Rectangle rect)
		{
			return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static Microsoft.Xna.Framework.Rectangle getSafeArea()
		{
			Microsoft.Xna.Framework.Rectangle area = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
			if (Game1.game1.GraphicsDevice.GetRenderTargets().Length == 0)
			{
				float oneOverZoomLevel = 1f / Game1.options.zoomLevel;
				if (Game1.uiMode)
				{
					oneOverZoomLevel = 1f / Game1.options.uiScale;
				}
				area.X = (int)((float)area.X * oneOverZoomLevel);
				area.Y = (int)((float)area.Y * oneOverZoomLevel);
				area.Width = (int)((float)area.Width * oneOverZoomLevel);
				area.Height = (int)((float)area.Height * oneOverZoomLevel);
			}
			return area;
		}

		public static Vector2 makeSafe(Vector2 renderPos, Vector2 renderSize)
		{
			getSafeArea();
			int x = (int)renderPos.X;
			int y = (int)renderPos.Y;
			int w = (int)renderSize.X;
			int h = (int)renderSize.Y;
			makeSafe(ref x, ref y, w, h);
			return new Vector2(x, y);
		}

		public static void makeSafe(ref Vector2 position, int width, int height)
		{
			int x = (int)position.X;
			int y = (int)position.Y;
			makeSafe(ref x, ref y, width, height);
			position.X = x;
			position.Y = y;
		}

		public static void makeSafe(ref Microsoft.Xna.Framework.Rectangle bounds)
		{
			makeSafe(ref bounds.X, ref bounds.Y, bounds.Width, bounds.Height);
		}

		public static void makeSafe(ref int x, ref int y, int width, int height)
		{
			Microsoft.Xna.Framework.Rectangle area = getSafeArea();
			if (x < area.Left)
			{
				x = area.Left;
			}
			if (y < area.Top)
			{
				y = area.Top;
			}
			if (x + width > area.Right)
			{
				x = area.Right - width;
			}
			if (y + height > area.Bottom)
			{
				y = area.Bottom - height;
			}
		}

		internal static void makeSafeY(ref int y, int height)
		{
			Vector2 pos2 = new Vector2(0f, y);
			Vector2 size = new Vector2(0f, height);
			pos2 = makeSafe(pos2, size);
			y = (int)pos2.Y;
		}

		public static int makeSafeMarginX(int marginx)
		{
			Viewport vp = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle area = getSafeArea();
			if (area.Left > vp.Bounds.Left)
			{
				marginx = area.Left;
			}
			int i = area.Right - vp.Bounds.Right;
			if (i > marginx)
			{
				marginx = i;
			}
			return marginx;
		}

		public static int makeSafeMarginY(int marginy)
		{
			Viewport vp = Game1.game1.GraphicsDevice.Viewport;
			Microsoft.Xna.Framework.Rectangle area = getSafeArea();
			int j = area.Top - vp.Bounds.Top;
			if (j > marginy)
			{
				marginy = j;
			}
			j = vp.Bounds.Bottom - area.Bottom;
			if (j > marginy)
			{
				marginy = j;
			}
			return marginy;
		}

		public static bool onTravelingMerchantShopPurchase(ISalable item, Farmer farmer, int amount)
		{
			Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.TravelingMerchant, item, amount);
			return false;
		}

		private static Dictionary<ISalable, int[]> generateLocalTravelingMerchantStock(int seed)
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			HashSet<int> stockIndices = new HashSet<int>();
			Random r = new Random(seed);
			bool add_guaranteed_item = false;
			if (Game1.netWorldState.Value.VisitsUntilY1Guarantee == 0)
			{
				add_guaranteed_item = true;
			}
			for (int i = 0; i < 10; i++)
			{
				int index2 = r.Next(2, 790);
				while (true)
				{
					index2++;
					index2 %= 790;
					if (Game1.objectInformation.ContainsKey(index2) && !isObjectOffLimitsForSale(index2))
					{
						if (index2 == 266 || index2 == 485)
						{
							add_guaranteed_item = false;
						}
						string[] split = Game1.objectInformation[index2].Split('/');
						if (split[3].Contains('-') && Convert.ToInt32(split[1]) > 0 && !split[3].Contains("-13") && !split[3].Equals("Quest") && !split[0].Equals("Weeds") && !split[3].Contains("Minerals") && !split[3].Contains("Arch") && addToStock(stock, stockIndices, new Object(index2, 1), new int[2]
						{
							Math.Max(r.Next(1, 11) * 100, Convert.ToInt32(split[1]) * r.Next(3, 6)),
							(!(r.NextDouble() < 0.1)) ? 1 : 5
						}))
						{
							break;
						}
					}
				}
			}
			if (add_guaranteed_item)
			{
				string[] split2 = Game1.objectInformation[485].Split('/');
				addToStock(stock, stockIndices, new Object(485, 1), new int[2]
				{
					Math.Max(r.Next(1, 11) * 100, Convert.ToInt32(split2[1]) * r.Next(3, 6)),
					(!(r.NextDouble() < 0.1)) ? 1 : 5
				});
			}
			addToStock(stock, stockIndices, getRandomFurniture(r, null, 0, 1613), new int[2]
			{
				r.Next(1, 11) * 250,
				1
			});
			if (getSeasonNumber(Game1.currentSeason) < 2)
			{
				addToStock(stock, stockIndices, new Object(347, 1), new int[2]
				{
					1000,
					(!(r.NextDouble() < 0.1)) ? 1 : 5
				});
			}
			else if (r.NextDouble() < 0.4)
			{
				addToStock(stock, stockIndices, new Object(Vector2.Zero, 136), new int[2]
				{
					4000,
					1
				});
			}
			if (r.NextDouble() < 0.25)
			{
				addToStock(stock, stockIndices, new Object(433, 1), new int[2]
				{
					2500,
					1
				});
			}
			return stock;
		}

		public static Dictionary<ISalable, int[]> getTravelingMerchantStock(int seed)
		{
			Dictionary<ISalable, int[]> localStock = generateLocalTravelingMerchantStock(seed);
			Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.TravelingMerchant, localStock);
			if (Game1.IsMultiplayer && !Game1.player.craftingRecipes.ContainsKey("Wedding Ring"))
			{
				Object weddingRingRecipe = new Object(801, 1, isRecipe: true);
				localStock.Add(weddingRingRecipe, new int[2]
				{
					500,
					1
				});
			}
			return localStock;
		}

		private static bool addToStock(Dictionary<ISalable, int[]> stock, HashSet<int> stockIndices, Object objectToAdd, int[] listing)
		{
			int index = objectToAdd.ParentSheetIndex;
			if (!stockIndices.Contains(index))
			{
				stock.Add(objectToAdd, listing);
				stockIndices.Add(index);
				return true;
			}
			return false;
		}

		public static Dictionary<ISalable, int[]> getDwarfShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(773, 1), new int[2]
			{
				2000,
				2147483647
			});
			stock.Add(new Object(772, 1), new int[2]
			{
				3000,
				2147483647
			});
			stock.Add(new Object(286, 1), new int[2]
			{
				300,
				2147483647
			});
			stock.Add(new Object(287, 1), new int[2]
			{
				600,
				2147483647
			});
			stock.Add(new Object(288, 1), new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Object(243, 1), new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 138), new int[2]
			{
				2500,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 32), new int[2]
			{
				200,
				2147483647
			});
			if (!Game1.player.craftingRecipes.ContainsKey("Weathered Floor"))
			{
				stock.Add(new Object(331, 1, isRecipe: true), new int[2]
				{
					500,
					1
				});
			}
			return stock;
		}

		public static Dictionary<ISalable, int[]> getHospitalStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(349, 1), new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Object(351, 1), new int[2]
			{
				1000,
				2147483647
			});
			return stock;
		}

		public static int CompareGameVersions(string version, string other_version, bool ignore_platform_specific = false)
		{
			string[] split = version.Split('.');
			string[] other_split = other_version.Split('.');
			for (int i = 0; i < Math.Max(split.Length, other_split.Length); i++)
			{
				float version_number = 0f;
				float other_version_number = 0f;
				if (i < split.Length)
				{
					float.TryParse(split[i], out version_number);
				}
				if (i < other_split.Length)
				{
					float.TryParse(other_split[i], out other_version_number);
				}
				if (version_number != other_version_number || (i == 2 && ignore_platform_specific))
				{
					return version_number.CompareTo(other_version_number);
				}
			}
			return 0;
		}

		public static float getFarmerItemsShippedPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			int farmerShipped = 0;
			int total = 0;
			foreach (KeyValuePair<int, string> kvp in Game1.objectInformation)
			{
				string typeString = kvp.Value.Split('/')[3];
				if (!typeString.Contains("Arch") && !typeString.Contains("Fish") && !typeString.Contains("Mineral") && !typeString.Substring(typeString.Length - 3).Equals("-2") && !typeString.Contains("Cooking") && !typeString.Substring(typeString.Length - 3).Equals("-7") && Object.isPotentialBasicShippedCategory(kvp.Key, typeString.Substring(typeString.Length - 3)))
				{
					total++;
					if (who.basicShipped.ContainsKey(kvp.Key))
					{
						farmerShipped++;
					}
				}
			}
			return (float)farmerShipped / (float)total;
		}

		public static bool hasFarmerShippedAllItems()
		{
			return getFarmerItemsShippedPercent() >= 1f;
		}

		public static Dictionary<ISalable, int[]> getQiShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Furniture(1552, Vector2.Zero), new int[2]
			{
				5000,
				2147483647
			});
			stock.Add(new Furniture(1545, Vector2.Zero), new int[2]
			{
				4000,
				2147483647
			});
			stock.Add(new Furniture(1563, Vector2.Zero), new int[2]
			{
				4000,
				2147483647
			});
			stock.Add(new Furniture(1561, Vector2.Zero), new int[2]
			{
				3000,
				2147483647
			});
			stock.Add(new Hat(2), new int[2]
			{
				8000,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 126), new int[2]
			{
				10000,
				2147483647
			});
			stock.Add(new Object(298, 1), new int[2]
			{
				100,
				2147483647
			});
			stock.Add(new Object(703, 1), new int[2]
			{
				1000,
				2147483647
			});
			stock.Add(new Object(688, 1), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new BedFurniture(2192, Vector2.Zero), new int[2]
			{
				8000,
				2147483647
			});
			return stock;
		}

		public static Dictionary<ISalable, int[]> getJojaStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			if (Game1.MasterPlayer.eventsSeen.Contains(502261))
			{
				stock.Add(new Object(Vector2.Zero, 272), new int[2]
				{
					50000,
					2147483647
				});
			}
			stock.Add(new Object(Vector2.Zero, 167, int.MaxValue), new int[2]
			{
				75,
				2147483647
			});
			stock.Add(new Wallpaper(21)
			{
				Stack = int.MaxValue
			}, new int[2]
			{
				20,
				2147483647
			});
			stock.Add(new Furniture(1609, Vector2.Zero)
			{
				Stack = int.MaxValue
			}, new int[2]
			{
				500,
				2147483647
			});
			float priceMod2 = Game1.player.hasOrWillReceiveMail("JojaMember") ? 2f : 2.5f;
			priceMod2 *= Game1.MasterPlayer.difficultyModifier;
			if (Game1.currentSeason.Equals("spring"))
			{
				stock.Add(new Object(Vector2.Zero, 472, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[472].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 473, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[473].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 474, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[474].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 475, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[475].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 427, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[427].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 429, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[429].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 477, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[477].Split('/')[1]) * priceMod2),
					2147483647
				});
			}
			if (Game1.currentSeason.Equals("summer"))
			{
				stock.Add(new Object(Vector2.Zero, 480, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[480].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 482, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[482].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 483, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 484, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[484].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 479, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[479].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 302, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[302].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 453, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[453].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 455, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[455].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(431, int.MaxValue, isRecipe: false, 100), new int[2]
				{
					(int)(50f * priceMod2),
					2147483647
				});
			}
			if (Game1.currentSeason.Equals("fall"))
			{
				stock.Add(new Object(Vector2.Zero, 487, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[487].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 488, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[488].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 483, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[483].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 490, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[490].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 299, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[299].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 301, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[301].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 492, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[492].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 491, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[491].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 493, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[493].Split('/')[1]) * priceMod2),
					2147483647
				});
				stock.Add(new Object(431, int.MaxValue, isRecipe: false, 100), new int[2]
				{
					(int)(50f * priceMod2),
					2147483647
				});
				stock.Add(new Object(Vector2.Zero, 425, int.MaxValue), new int[2]
				{
					(int)((float)Convert.ToInt32(Game1.objectInformation[425].Split('/')[1]) * priceMod2),
					2147483647
				});
			}
			stock.Add(new Object(Vector2.Zero, 297, int.MaxValue), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[297].Split('/')[1]) * priceMod2),
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 245, int.MaxValue), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[245].Split('/')[1]) * priceMod2),
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 246, int.MaxValue), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[246].Split('/')[1]) * priceMod2),
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 423, int.MaxValue), new int[2]
			{
				(int)((float)Convert.ToInt32(Game1.objectInformation[423].Split('/')[1]) * priceMod2),
				2147483647
			});
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + 1);
			int whichWallpaper = r.Next(112);
			if (whichWallpaper == 21)
			{
				whichWallpaper = 22;
			}
			stock.Add(new Wallpaper(whichWallpaper)
			{
				Stack = int.MaxValue
			}, new int[2]
			{
				250,
				2147483647
			});
			stock.Add(new Wallpaper(r.Next(40), isFloor: true)
			{
				Stack = int.MaxValue
			}, new int[2]
			{
				250,
				2147483647
			});
			return stock;
		}

		public static Dictionary<ISalable, int[]> getHatStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Achievements");
			bool foundAll = true;
			foreach (KeyValuePair<int, string> v in dictionary)
			{
				if (Game1.player.achievements.Contains(v.Key))
				{
					stock.Add(new Hat(Convert.ToInt32(v.Value.Split('^')[4])), new int[2]
					{
						1000,
						2147483647
					});
				}
				else
				{
					foundAll = false;
				}
			}
			if (Game1.player.mailReceived.Contains("Egg Festival"))
			{
				stock.Add(new Hat(4), new int[2]
				{
					1000,
					2147483647
				});
			}
			if (Game1.player.mailReceived.Contains("Ice Festival"))
			{
				stock.Add(new Hat(17), new int[2]
				{
					1000,
					2147483647
				});
			}
			if (Game1.player.achievements.Contains(17))
			{
				stock.Add(new Hat(61), new int[2]
				{
					1000,
					2147483647
				});
			}
			if (foundAll)
			{
				stock.Add(new Hat(64), new int[2]
				{
					1000,
					2147483647
				});
			}
			return stock;
		}

		public static NPC getTodaysBirthdayNPC(string season, int day)
		{
			foreach (NPC i in getAllCharacters())
			{
				if (i.isBirthday(season, day))
				{
					return i;
				}
			}
			return null;
		}

		public static bool highlightEdibleItems(Item i)
		{
			if (i is Object)
			{
				return (int)(i as Object).edibility != -300;
			}
			return false;
		}

		public static T GetRandom<T>(List<T> list, Random random = null)
		{
			if (list == null || list.Count == 0)
			{
				return default(T);
			}
			if (random == null)
			{
				random = Game1.random;
			}
			return list[random.Next(list.Count)];
		}

		public static int getRandomSingleTileFurniture(Random r)
		{
			switch (r.Next(3))
			{
			case 0:
				return r.Next(10) * 3;
			case 1:
				return r.Next(1376, 1391);
			case 2:
				return r.Next(7) * 2 + 1391;
			default:
				return 0;
			}
		}

		public static void improveFriendshipWithEveryoneInRegion(Farmer who, int amount, int region)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC i in location.characters)
				{
					if (i.homeRegion == region && who.friendshipData.ContainsKey(i.Name))
					{
						who.changeFriendship(amount, i);
					}
				}
			}
		}

		public static Item getGiftFromNPC(NPC who)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + Game1.year + Game1.dayOfMonth + getSeasonNumber(Game1.currentSeason) + who.getTileX());
			List<Item> possibleObjects = new List<Item>();
			switch (who.Name)
			{
			case "Clint":
				possibleObjects.Add(new Object(337, 1));
				possibleObjects.Add(new Object(336, 5));
				possibleObjects.Add(new Object(r.Next(535, 538), 5));
				break;
			case "Marnie":
				possibleObjects.Add(new Object(176, 12));
				break;
			case "Robin":
				possibleObjects.Add(new Object(388, 99));
				possibleObjects.Add(new Object(390, 50));
				possibleObjects.Add(new Object(709, 25));
				break;
			case "Willy":
				possibleObjects.Add(new Object(690, 25));
				possibleObjects.Add(new Object(687, 1));
				possibleObjects.Add(new Object(703, 1));
				break;
			case "Evelyn":
				possibleObjects.Add(new Object(223, 1));
				break;
			default:
			{
				int age = who.Age;
				if (age == 2)
				{
					possibleObjects.Add(new Object(330, 1));
					possibleObjects.Add(new Object(103, 1));
					possibleObjects.Add(new Object(394, 1));
					possibleObjects.Add(new Object(r.Next(535, 538), 1));
					break;
				}
				possibleObjects.Add(new Object(608, 1));
				possibleObjects.Add(new Object(651, 1));
				possibleObjects.Add(new Object(611, 1));
				possibleObjects.Add(new Ring(517));
				possibleObjects.Add(new Object(466, 10));
				possibleObjects.Add(new Object(422, 1));
				possibleObjects.Add(new Object(392, 1));
				possibleObjects.Add(new Object(348, 1));
				possibleObjects.Add(new Object(346, 1));
				possibleObjects.Add(new Object(341, 1));
				possibleObjects.Add(new Object(221, 1));
				possibleObjects.Add(new Object(64, 1));
				possibleObjects.Add(new Object(60, 1));
				possibleObjects.Add(new Object(70, 1));
				break;
			}
			}
			return possibleObjects[r.Next(possibleObjects.Count)];
		}

		public static NPC getTopRomanticInterest(Farmer who)
		{
			NPC topSpot = null;
			int highestFriendPoints = -1;
			foreach (NPC i in getAllCharacters())
			{
				if (who.friendshipData.ContainsKey(i.Name) && (bool)i.datable && who.getFriendshipLevelForNPC(i.Name) > highestFriendPoints)
				{
					topSpot = i;
					highestFriendPoints = who.getFriendshipLevelForNPC(i.Name);
				}
			}
			return topSpot;
		}

		public static Color getRandomRainbowColor(Random r = null)
		{
			switch (r?.Next(8) ?? Game1.random.Next(8))
			{
			case 0:
				return Color.Red;
			case 1:
				return Color.Orange;
			case 2:
				return Color.Yellow;
			case 3:
				return Color.Lime;
			case 4:
				return Color.Cyan;
			case 5:
				return new Color(0, 100, 255);
			case 6:
				return new Color(152, 96, 255);
			case 7:
				return new Color(255, 100, 255);
			default:
				return Color.White;
			}
		}

		public static NPC getTopNonRomanticInterest(Farmer who)
		{
			NPC topSpot = null;
			int highestFriendPoints = -1;
			foreach (NPC i in getAllCharacters())
			{
				if (who.friendshipData.ContainsKey(i.Name) && !i.datable && who.getFriendshipLevelForNPC(i.Name) > highestFriendPoints)
				{
					topSpot = i;
					highestFriendPoints = who.getFriendshipLevelForNPC(i.Name);
				}
			}
			return topSpot;
		}

		public static int getHighestSkill(Farmer who)
		{
			int topSkillExperience = 0;
			int topSkill = 0;
			for (int i = 0; i < who.experiencePoints.Length; i++)
			{
				if (who.experiencePoints[i] > topSkillExperience)
				{
					topSkill = i;
				}
			}
			return topSkill;
		}

		public static int getNumberOfFriendsWithinThisRange(Farmer who, int minFriendshipPoints, int maxFriendshipPoints, bool romanceOnly = false)
		{
			int number = 0;
			foreach (NPC i in getAllCharacters())
			{
				int? level = who.tryGetFriendshipLevelForNPC(i.Name);
				if (level.HasValue && level.Value >= minFriendshipPoints && level.Value <= maxFriendshipPoints && (!romanceOnly || (bool)i.datable))
				{
					number++;
				}
			}
			return number;
		}

		public static bool highlightLuauSoupItems(Item i)
		{
			if (i is Object)
			{
				if (((int)(i as Object).edibility == -300 || (i as Object).Category == -7) && !IsNormalObjectAtParentSheetIndex(i, 789))
				{
					return IsNormalObjectAtParentSheetIndex(i, 71);
				}
				return true;
			}
			return false;
		}

		public static bool highlightEdibleNonCookingItems(Item i)
		{
			if (i is Object && (int)(i as Object).edibility != -300)
			{
				return (i as Object).Category != -7;
			}
			return false;
		}

		public static bool highlightSmallObjects(Item i)
		{
			if (i is Object)
			{
				return !(i as Object).bigCraftable;
			}
			return false;
		}

		public static bool highlightSantaObjects(Item i)
		{
			if (!i.canBeTrashed() || !i.canBeGivenAsGift())
			{
				return false;
			}
			return highlightSmallObjects(i);
		}

		public static bool highlightShippableObjects(Item i)
		{
			if (i is Object)
			{
				return (i as Object).canBeShipped();
			}
			return false;
		}

		public static Farmer getFarmerFromFarmerNumberString(string s, Farmer defaultFarmer)
		{
			if (s.Equals("farmer"))
			{
				return defaultFarmer;
			}
			if (s.StartsWith("farmer"))
			{
				return getFarmerFromFarmerNumber(Convert.ToInt32(s[s.Length - 1].ToString() ?? ""));
			}
			return defaultFarmer;
		}

		public static int getFarmerNumberFromFarmer(Farmer who)
		{
			for (int i = 1; i <= Game1.CurrentPlayerLimit; i++)
			{
				if (getFarmerFromFarmerNumber(i).UniqueMultiplayerID == who.UniqueMultiplayerID)
				{
					return i;
				}
			}
			return -1;
		}

		public static Farmer getFarmerFromFarmerNumber(int number)
		{
			if (!Game1.IsMultiplayer)
			{
				if (number == 1)
				{
					return Game1.player;
				}
				return null;
			}
			if (number <= 1 && Game1.serverHost != null)
			{
				return Game1.serverHost;
			}
			if (number <= Game1.numberOfPlayers())
			{
				return (from f in Game1.otherFarmers.Values
					where f != Game1.serverHost.Value
					orderby f.UniqueMultiplayerID
					select f).ElementAt(number - 2);
			}
			return null;
		}

		public static string getLoveInterest(string who)
		{
			switch (who)
			{
			case "Haley":
				return "Alex";
			case "Sam":
				return "Penny";
			case "Alex":
				return "Haley";
			case "Penny":
				return "Sam";
			case "Leah":
				return "Elliott";
			case "Harvey":
				return "Maru";
			case "Maru":
				return "Harvey";
			case "Elliott":
				return "Leah";
			case "Abigail":
				return "Sebastian";
			case "Sebastian":
				return "Abigail";
			case "Emily":
				return "Shane";
			case "Shane":
				return "Emily";
			default:
				return "";
			}
		}

		public static Dictionary<ISalable, int[]> getFishShopStock(Farmer who)
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(219, 1), new int[2]
			{
				250,
				2147483647
			});
			if ((int)Game1.player.fishingLevel >= 2)
			{
				stock.Add(new Object(685, 1), new int[2]
				{
					5,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 3)
			{
				stock.Add(new Object(710, 1), new int[2]
				{
					1500,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 6)
			{
				stock.Add(new Object(686, 1), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Object(694, 1), new int[2]
				{
					500,
					2147483647
				});
				stock.Add(new Object(692, 1), new int[2]
				{
					200,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 7)
			{
				stock.Add(new Object(693, 1), new int[2]
				{
					750,
					2147483647
				});
				stock.Add(new Object(695, 1), new int[2]
				{
					750,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 8)
			{
				stock.Add(new Object(691, 1), new int[2]
				{
					1000,
					2147483647
				});
				stock.Add(new Object(687, 1), new int[2]
				{
					1000,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 9)
			{
				stock.Add(new Object(703, 1), new int[2]
				{
					1000,
					2147483647
				});
			}
			stock.Add(new FishingRod(0), new int[2]
			{
				500,
				2147483647
			});
			stock.Add(new FishingRod(1), new int[2]
			{
				25,
				2147483647
			});
			if ((int)Game1.player.fishingLevel >= 2)
			{
				stock.Add(new FishingRod(2), new int[2]
				{
					1800,
					2147483647
				});
			}
			if ((int)Game1.player.fishingLevel >= 6)
			{
				stock.Add(new FishingRod(3), new int[2]
				{
					7500,
					2147483647
				});
			}
			if (Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
			{
				stock.Add(new Pan(), new int[2]
				{
					2500,
					2147483647
				});
			}
			stock.Add(new FishTankFurniture(2304, Vector2.Zero), new int[2]
			{
				2000,
				2147483647
			});
			stock.Add(new FishTankFurniture(2322, Vector2.Zero), new int[2]
			{
				500,
				2147483647
			});
			if (Game1.player.mailReceived.Contains("WillyTropicalFish"))
			{
				stock.Add(new FishTankFurniture(2312, Vector2.Zero), new int[2]
				{
					5000,
					2147483647
				});
			}
			stock.Add(new BedFurniture(2502, Vector2.Zero), new int[2]
			{
				25000,
				2147483647
			});
			GameLocation shop = Game1.getLocationFromName("FishShop");
			if (shop is ShopLocation)
			{
				foreach (Item i in (shop as ShopLocation).itemsFromPlayerToSell)
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
				return stock;
			}
			return stock;
		}

		public static string ParseGiftReveals(string str)
		{
			try
			{
				while (str.Contains("%revealtaste"))
				{
					int reveal_taste_location = str.IndexOf("%revealtaste");
					int token_start2 = reveal_taste_location + "%revealtaste".Length;
					int token_end = reveal_taste_location + 1;
					if (token_end >= str.Length)
					{
						token_end = str.Length - 1;
					}
					for (; token_end < str.Length && (str[token_end] < '0' || str[token_end] > '9'); token_end++)
					{
					}
					string character_name = str.Substring(token_start2, token_end - token_start2);
					token_start2 = token_end;
					for (; token_end < str.Length && str[token_end] >= '0' && str[token_end] <= '9'; token_end++)
					{
					}
					int item_index = int.Parse(str.Substring(token_start2, token_end - token_start2));
					str = str.Remove(reveal_taste_location, token_end - reveal_taste_location);
					NPC target = Game1.getCharacterFromName(character_name);
					if (target != null)
					{
						Game1.player.revealGiftTaste(target, item_index);
					}
				}
				return str;
			}
			catch (Exception)
			{
				return str;
			}
		}

		public static void Shuffle<T>(Random rng, List<T> list)
		{
			int j = list.Count;
			while (j > 1)
			{
				int i = rng.Next(j--);
				T temp = list[j];
				list[j] = list[i];
				list[i] = temp;
			}
		}

		public static void Shuffle<T>(Random rng, T[] array)
		{
			int j = array.Length;
			while (j > 1)
			{
				int i = rng.Next(j--);
				T temp = array[j];
				array[j] = array[i];
				array[i] = temp;
			}
		}

		public static int getSeasonNumber(string whichSeason)
		{
			if (whichSeason.Equals("spring", StringComparison.OrdinalIgnoreCase))
			{
				return 0;
			}
			if (whichSeason.Equals("summer", StringComparison.OrdinalIgnoreCase))
			{
				return 1;
			}
			if (whichSeason.Equals("autumn", StringComparison.OrdinalIgnoreCase) || whichSeason.Equals("fall", StringComparison.OrdinalIgnoreCase))
			{
				return 2;
			}
			if (whichSeason.Equals("winter", StringComparison.OrdinalIgnoreCase))
			{
				return 3;
			}
			return -1;
		}

		public static char getRandomSlotCharacter(char current)
		{
			char which = 'o';
			while (which == 'o' || which == current)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					which = '=';
					break;
				case 1:
					which = '\\';
					break;
				case 2:
					which = ']';
					break;
				case 3:
					which = '[';
					break;
				case 4:
					which = '<';
					break;
				case 5:
					which = '*';
					break;
				case 6:
					which = '$';
					break;
				case 7:
					which = '}';
					break;
				}
			}
			return which;
		}

		public static List<Vector2> getPositionsInClusterAroundThisTile(Vector2 startTile, int number)
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			List<Vector2> tiles = new List<Vector2>();
			Vector2 currentTile2 = startTile;
			openList.Enqueue(currentTile2);
			while (tiles.Count < number)
			{
				currentTile2 = openList.Dequeue();
				tiles.Add(currentTile2);
				if (!tiles.Contains(new Vector2(currentTile2.X + 1f, currentTile2.Y)))
				{
					openList.Enqueue(new Vector2(currentTile2.X + 1f, currentTile2.Y));
				}
				if (!tiles.Contains(new Vector2(currentTile2.X - 1f, currentTile2.Y)))
				{
					openList.Enqueue(new Vector2(currentTile2.X - 1f, currentTile2.Y));
				}
				if (!tiles.Contains(new Vector2(currentTile2.X, currentTile2.Y + 1f)))
				{
					openList.Enqueue(new Vector2(currentTile2.X, currentTile2.Y + 1f));
				}
				if (!tiles.Contains(new Vector2(currentTile2.X, currentTile2.Y - 1f)))
				{
					openList.Enqueue(new Vector2(currentTile2.X, currentTile2.Y - 1f));
				}
			}
			return tiles;
		}

		public static bool doesPointHaveLineOfSightInMine(GameLocation mine, Vector2 start, Vector2 end, int visionDistance)
		{
			if (Vector2.Distance(start, end) > (float)visionDistance)
			{
				return false;
			}
			foreach (Point p in GetPointsOnLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y))
			{
				if (mine.getTileIndexAt(p, "Buildings") != -1)
				{
					return false;
				}
			}
			return true;
		}

		public static void addSprinklesToLocation(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random r = new Random();
			int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
			for (int i = 0; i < numSprinkles; i++)
			{
				Vector2 currentSprinklePosition = getRandomPositionInThisRectangle(area, r) * 64f;
				l.temporarySprites.Add(new TemporaryAnimatedSprite(r.Next(10, 12), currentSprinklePosition, sprinkleColor, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					interval = 100f,
					startSound = sound,
					motion = (motionTowardCenter ? getVelocityTowardPoint(currentSprinklePosition, new Vector2(sourceXTile, sourceYTile) * 64f, Vector2.Distance(new Vector2(sourceXTile, sourceYTile) * 64f, currentSprinklePosition) / 64f) : Vector2.Zero),
					xStopCoordinate = sourceXTile,
					yStopCoordinate = sourceYTile
				});
			}
		}

		public static List<TemporaryAnimatedSprite> getStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
			Random r = new Random();
			int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
			List<TemporaryAnimatedSprite> tempSprites = new List<TemporaryAnimatedSprite>();
			for (int i = 0; i < numSprinkles; i++)
			{
				Vector2 currentSprinklePosition = getRandomPositionInThisRectangle(area, r) * 64f;
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", (r.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(359, 1437, 14, 14) : new Microsoft.Xna.Framework.Rectangle(377, 1438, 9, 9), currentSprinklePosition, flipped: false, 0.01f, sprinkleColor)
				{
					xPeriodic = true,
					xPeriodicLoopTime = r.Next(2000, 3000),
					xPeriodicRange = r.Next(-64, 64),
					motion = new Vector2(0f, -2f),
					rotationChange = (float)Math.PI / (float)r.Next(4, 64),
					delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
					layerDepth = 1f,
					scaleChange = 0.04f,
					scaleChangeChange = -0.0008f,
					scale = 4f
				});
			}
			return tempSprites;
		}

		public static void addStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
		{
			l.temporarySprites.AddRange(getStarsAndSpirals(l, sourceXTile, sourceYTile, tilesWide, tilesHigh, totalSprinkleDuration, millisecondsBetweenSprinkles, sprinkleColor, sound, motionTowardCenter));
		}

		public static Vector2 snapDrawPosition(Vector2 draw_position)
		{
			return new Vector2((int)draw_position.X, (int)draw_position.Y);
		}

		public static Vector2 clampToTile(Vector2 nonTileLocation)
		{
			nonTileLocation.X -= nonTileLocation.X % 64f;
			nonTileLocation.Y -= nonTileLocation.Y % 64f;
			return nonTileLocation;
		}

		public static float distance(float x1, float x2, float y1, float y2)
		{
			return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
		}

		public static void facePlayerEndBehavior(Character c, GameLocation location)
		{
			c.faceGeneralDirection(new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y), 0, opposite: false, useTileCalculations: false);
		}

		public static bool couldSeePlayerInPeripheralVision(Farmer player, Character c)
		{
			switch (c.FacingDirection)
			{
			case 0:
				if (player.GetBoundingBox().Center.Y < c.GetBoundingBox().Center.Y + 32)
				{
					return true;
				}
				break;
			case 1:
				if (player.GetBoundingBox().Center.X > c.GetBoundingBox().Center.X - 32)
				{
					return true;
				}
				break;
			case 2:
				if (player.GetBoundingBox().Center.Y > c.GetBoundingBox().Center.Y - 32)
				{
					return true;
				}
				break;
			case 3:
				if (player.GetBoundingBox().Center.X < c.GetBoundingBox().Center.X + 32)
				{
					return true;
				}
				break;
			}
			return false;
		}

		public static List<Microsoft.Xna.Framework.Rectangle> divideThisRectangleIntoQuarters(Microsoft.Xna.Framework.Rectangle rect)
		{
			return new List<Microsoft.Xna.Framework.Rectangle>
			{
				new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width / 2, rect.Height / 2),
				new Microsoft.Xna.Framework.Rectangle(rect.X + rect.Width / 2, rect.Y, rect.Width / 2, rect.Height / 2),
				new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2),
				new Microsoft.Xna.Framework.Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width / 2, rect.Height / 2)
			};
		}

		public static Item getUncommonItemForThisMineLevel(int level, Point location)
		{
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			List<int> possibleWeapons = new List<int>();
			int closest = -1;
			int closestLevel = -1;
			int guassianC = 12;
			Random r = new Random(location.X * 1000 + location.Y + (int)Game1.uniqueIDForThisGame + level);
			foreach (KeyValuePair<int, string> kvp in dictionary)
			{
				if (Game1.CurrentMineLevel >= Convert.ToInt32(kvp.Value.Split('/')[10]) && Convert.ToInt32(kvp.Value.Split('/')[9]) != -1)
				{
					int baseLevel = Convert.ToInt32(kvp.Value.Split('/')[9]);
					if (closest == -1 || closestLevel > Math.Abs(Game1.CurrentMineLevel - baseLevel))
					{
						closest = kvp.Key;
						closestLevel = Convert.ToInt32(kvp.Value.Split('/')[9]);
					}
					double gaussian = Math.Pow(Math.E, (0.0 - Math.Pow(Game1.CurrentMineLevel - baseLevel, 2.0)) / (double)(2 * (guassianC * guassianC)));
					if (r.NextDouble() < gaussian)
					{
						possibleWeapons.Add(kvp.Key);
					}
				}
			}
			possibleWeapons.Add(closest);
			return new MeleeWeapon(possibleWeapons.ElementAt(r.Next(possibleWeapons.Count)));
		}

		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
		{
			return GetPointsOnLine(x0, y0, x1, y1, ignoreSwap: false);
		}

		public static List<Vector2> getBorderOfThisRectangle(Microsoft.Xna.Framework.Rectangle r)
		{
			List<Vector2> border = new List<Vector2>();
			for (int l = r.X; l < r.Right; l++)
			{
				border.Add(new Vector2(l, r.Y));
			}
			for (int k = r.Y + 1; k < r.Bottom; k++)
			{
				border.Add(new Vector2(r.Right - 1, k));
			}
			for (int j = r.Right - 2; j >= r.X; j--)
			{
				border.Add(new Vector2(j, r.Bottom - 1));
			}
			for (int i = r.Bottom - 2; i >= r.Y + 1; i--)
			{
				border.Add(new Vector2(r.X, i));
			}
			return border;
		}

		public static Point getTranslatedPoint(Point p, int direction, int movementAmount)
		{
			switch (direction)
			{
			case 0:
				return new Point(p.X, p.Y - movementAmount);
			case 2:
				return new Point(p.X, p.Y + movementAmount);
			case 1:
				return new Point(p.X + movementAmount, p.Y);
			case 3:
				return new Point(p.X - movementAmount, p.Y);
			default:
				return p;
			}
		}

		public static Vector2 getTranslatedVector2(Vector2 p, int direction, float movementAmount)
		{
			switch (direction)
			{
			case 0:
				return new Vector2(p.X, p.Y - movementAmount);
			case 2:
				return new Vector2(p.X, p.Y + movementAmount);
			case 1:
				return new Vector2(p.X + movementAmount, p.Y);
			case 3:
				return new Vector2(p.X - movementAmount, p.Y);
			default:
				return p;
			}
		}

		public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1, bool ignoreSwap)
		{
			bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
			if (steep)
			{
				int t3 = x0;
				x0 = y0;
				y0 = t3;
				t3 = x1;
				x1 = y1;
				y1 = t3;
			}
			if (!ignoreSwap && x0 > x1)
			{
				int t2 = x0;
				x0 = x1;
				x1 = t2;
				t2 = y0;
				y0 = y1;
				y1 = t2;
			}
			int dx = x1 - x0;
			int dy = Math.Abs(y1 - y0);
			int error = dx / 2;
			int ystep = (y0 < y1) ? 1 : (-1);
			int y2 = y0;
			for (int x2 = x0; x2 <= x1; x2++)
			{
				yield return new Point(steep ? y2 : x2, steep ? x2 : y2);
				error -= dy;
				if (error < 0)
				{
					y2 += ystep;
					error += dx;
				}
			}
		}

		public static Vector2 getRandomAdjacentOpenTile(Vector2 tile, GameLocation location)
		{
			List<Vector2> i = getAdjacentTileLocations(tile);
			int iter = 0;
			int which = Game1.random.Next(i.Count);
			Vector2 v = i[which];
			for (; iter < 4; iter++)
			{
				if (!location.isTileOccupiedForPlacement(v) && location.isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport))
				{
					break;
				}
				which = (which + 1) % i.Count;
				v = i[which];
			}
			if (iter >= 4)
			{
				return Vector2.Zero;
			}
			return v;
		}

		public static int getObjectIndexFromSlotCharacter(char character)
		{
			switch (character)
			{
			case '=':
				return 72;
			case '\\':
				return 336;
			case ']':
				return 221;
			case '[':
				return 276;
			case '<':
				return 400;
			case '$':
				return 398;
			case '}':
				return 184;
			case '*':
				return 176;
			default:
				return 0;
			}
		}

		private static string farmerAccomplishments()
		{
			string accomplishments = Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5229") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5230");
			if (Game1.player.hasRustyKey)
			{
				accomplishments += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5235");
			}
			if (Game1.player.achievements.Contains(71))
			{
				accomplishments += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5236");
			}
			if (Game1.player.achievements.Contains(45))
			{
				accomplishments += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5237");
			}
			if (accomplishments.Length > 115)
			{
				accomplishments += "#$b#";
			}
			if (Game1.player.achievements.Contains(63))
			{
				accomplishments += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5239");
			}
			if (Game1.player.timesReachedMineBottom > 0)
			{
				accomplishments += Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5240");
			}
			return accomplishments + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5241", Game1.player.totalMoneyEarned - Game1.player.totalMoneyEarned % 1000u);
		}

		public static string getCreditsString()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5243") + Environment.NewLine + " " + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5244") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5245") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5246") + Environment.NewLine + Environment.NewLine + "-Eric Barone" + Environment.NewLine + " " + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5248") + Environment.NewLine + Environment.NewLine + "-Amber Hageman" + Environment.NewLine + "-Shane Waletzko" + Environment.NewLine + "-Fiddy, Nuns, Kappy &" + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5252") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5253") + Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5254");
		}

		public static string getStardewHeroCelebrationEventString(int finalFarmerScore)
		{
			string eventString = "";
			if (finalFarmerScore >= Game1.percentageToWinStardewHero)
			{
				return "title_day/-100 -100/farmer 18 20 1 rival 27 20 2" + getCelebrationPositionsForDatables(Game1.player.spouse) + ((Game1.player.spouse != null && !Game1.player.isEngaged()) ? (Game1.player.spouse + " 17 21 1 ") : "") + "Lewis 22 19 2 Marnie 21 22 0 Caroline 24 22 0 Pierre 25 22 0 Gus 26 22 0 Clint 26 23 0 Emily 25 23 0 Shane 27 23 0 " + ((Game1.player.friendshipData.ContainsKey("Sandy") && Game1.player.friendshipData["Sandy"].Points > 0) ? "Sandy 24 23 0 " : "") + "George 21 23 0 Evelyn 20 23 0 Pam 19 23 0 Jodi 27 24 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 26 24 0 " : "") + "Linus 24 24 0 Robin 21 24 0 Demetrius 20 24 0" + ((Game1.player.timesReachedMineBottom > 0) ? " Dwarf 19 24 0" : "") + "/addObject 18 19 " + Game1.random.Next(313, 320) + "/addObject 19 19 " + Game1.random.Next(313, 320) + "/addObject 20 19 " + Game1.random.Next(313, 320) + "/addObject 25 19 " + Game1.random.Next(313, 320) + "/addObject 26 19 " + Game1.random.Next(313, 320) + "/addObject 27 19 " + Game1.random.Next(313, 320) + "/addObject 23 19 468/viewport 22 20 true/pause 4000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5256") + "\"/pause 400/faceDirection Lewis 3/pause 500/faceDirection Lewis 1/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5259") + "\"/pause 200/showRivalFrame 16/pause 600/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5260") + "\"/pause 700/move Lewis 0 1 3/stopMusic/move Lewis -2 0 3/playMusic musicboxsong/faceDirection farmer 1/showRivalFrame 12/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5263", farmerAccomplishments()) + "\"/pause 800/move Lewis 5 0 1/showRivalFrame 12/playMusic rival/pause 500/speak Lewis \"" + (Game1.player.isMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5306") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5307")) + "\"/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5308") + "\"/move rival 0 1 2/showRivalFrame 17/pause 500/speak rival \"" + ((!Game1.player.isMale) ? ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5312") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5313")) : ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5310") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5311"))) + "\"/pause 600/emote farmer 40/showRivalFrame 16/pause 900/move rival 0 -1 2/showRivalFrame 16/move Lewis -3 0 2/stopMusic/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5314") + "\"/stopMusic/move Lewis 0 -1 2/pause 600/faceDirection Lewis 1/pause 600/faceDirection Lewis 3/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5315") + "\"/pause 300/move rival -2 0 2/showRivalFrame 16/pause 1500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5316") + "\"/pause 500/showRivalFrame 18/pause 400/playMusic happy/emote farmer 16/move farmer 5 0 2/move Lewis 0 1 1/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5317", finalFarmerScore) + "\"/speak Emily \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5318") + "\"/speak Gus \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5319") + "\"/speak Pierre \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5320") + "\"/showRivalFrame 12/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5321") + "\"/speed rival 4/move rival 6 0 0/faceDirection farmer 1 true/speed rival 4/move rival 0 -10 1/warp rival -100 -100/move farmer 0 1 2/emote farmer 20/fade/viewport -1000 -1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5322", getOtherFarmerNames()[0]) + "\"/end credits";
			}
			return "title_day/-100 -100/farmer 18 20 1 rival 27 20 2" + getCelebrationPositionsForDatables(Game1.player.spouse) + ((Game1.player.spouse != null && !Game1.player.isEngaged()) ? (Game1.player.spouse + " 17 21 1 ") : "") + "Lewis 22 19 2 Marnie 21 22 0 Caroline 24 22 0 Pierre 25 22 0 Gus 26 22 0 Clint 26 23 0 Emily 25 23 0 Shane 27 23 0 " + ((Game1.player.friendshipData.ContainsKey("Sandy") && Game1.player.friendshipData["Sandy"].Points > 0) ? "Sandy 24 23 0 " : "") + "George 21 23 0 Evelyn 20 23 0 Pam 19 23 0 Jodi 27 24 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 26 24 0 " : "") + "Linus 24 24 0 Robin 21 24 0 Demetrius 20 24 0" + ((Game1.player.timesReachedMineBottom > 0) ? " Dwarf 19 24 0" : "") + "/addObject 18 19 " + Game1.random.Next(313, 320) + "/addObject 19 19 " + Game1.random.Next(313, 320) + "/addObject 20 19 " + Game1.random.Next(313, 320) + "/addObject 25 19 " + Game1.random.Next(313, 320) + "/addObject 26 19 " + Game1.random.Next(313, 320) + "/addObject 27 19 " + Game1.random.Next(313, 320) + "/addObject 23 19 468/viewport 22 20 true/pause 4000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5256") + "\"/pause 400/faceDirection Lewis 3/pause 500/faceDirection Lewis 1/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5259") + "\"/pause 200/showRivalFrame 16/pause 600/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5260") + "\"/pause 700/move Lewis 0 1 3/stopMusic/move Lewis -2 0 3/playMusic musicboxsong/faceDirection farmer 1/showRivalFrame 12/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5263", farmerAccomplishments()) + "\"/pause 800/move Lewis 5 0 1/showRivalFrame 12/playMusic rival/pause 500/speak Lewis \"" + (Game1.player.isMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5306") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5307")) + "\"/pause 500/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5308") + "\"/move rival 0 1 2/showRivalFrame 17/pause 500/speak rival \"" + ((!Game1.player.isMale) ? ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5312") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5313")) : ((Game1.random.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5310") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5311"))) + "\"/pause 600/emote farmer 40/showRivalFrame 16/pause 900/move rival 0 -1 2/showRivalFrame 16/move Lewis -3 0 2/stopMusic/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5314") + "\"/stopMusic/move Lewis 0 -1 2/pause 600/faceDirection Lewis 1/pause 600/faceDirection Lewis 3/pause 600/faceDirection Lewis 2/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5315") + "\"/pause 300/move rival -2 0 2/showRivalFrame 16/pause 1500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5323") + "\"/pause 200/showFrame 32/move rival -2 0 2/showRivalFrame 19/pause 400/playSound death/emote farmer 28/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5324", Game1.percentageToWinStardewHero - finalFarmerScore) + "\"/speak rival \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5325") + "\"/pause 600/faceDirection Lewis 3/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5326") + "\"/speak Emily \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5327") + "\"/fade/viewport -1000 -1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5328", finalFarmerScore) + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5329") + "\"/end credits";
		}

		public static void CollectSingleItemOrShowChestMenu(Chest chest, object context = null)
		{
			int item_count = 0;
			Item item_to_grab = null;
			for (int i = 0; i < chest.items.Count; i++)
			{
				if (chest.items[i] != null)
				{
					item_count++;
					if (item_count == 1)
					{
						item_to_grab = chest.items[i];
					}
					if (item_count == 2)
					{
						item_to_grab = null;
						break;
					}
				}
			}
			if (item_count == 0)
			{
				return;
			}
			if (item_to_grab != null)
			{
				int old_stack_amount = item_to_grab.Stack;
				if (Game1.player.addItemToInventory(item_to_grab) == null)
				{
					Game1.playSound("coin");
					chest.items.Remove(item_to_grab);
					chest.clearNulls();
					return;
				}
				if (item_to_grab.Stack != old_stack_amount)
				{
					Game1.playSound("coin");
				}
			}
			Game1.activeClickableMenu = new ItemGrabMenu(chest.items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, context);
		}

		public static bool CollectOrDrop(Item item, int direction)
		{
			if (item != null)
			{
				item = Game1.player.addItemToInventory(item);
				if (item != null)
				{
					if (direction != -1)
					{
						Game1.createItemDebris(item, Game1.player.getStandingPosition(), direction);
					}
					else
					{
						Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
					}
					return false;
				}
				return true;
			}
			return true;
		}

		public static bool CollectOrDrop(Item item)
		{
			return CollectOrDrop(item, -1);
		}

		public static void perpareDayForStardewCelebration(int finalFarmerScore)
		{
			bool farmerWon = finalFarmerScore >= Game1.percentageToWinStardewHero;
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC i in location.characters)
				{
					string dialogue = "";
					if (farmerWon)
					{
						switch (Game1.random.Next(6))
						{
						case 0:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5348");
							break;
						case 1:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5349");
							break;
						case 2:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5350");
							break;
						case 3:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5351");
							break;
						case 4:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5352");
							break;
						case 5:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5353");
							break;
						}
						if (i.Name.Equals("Sebastian") || i.Name.Equals("Abigail"))
						{
							dialogue = (Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5356") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5357"));
						}
						else if (i.Name.Equals("George"))
						{
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5360");
						}
					}
					else
					{
						switch (Game1.random.Next(4))
						{
						case 0:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5361");
							break;
						case 1:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5362");
							break;
						case 2:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5363");
							break;
						case 3:
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5364");
							break;
						}
						if (i.Name.Equals("George"))
						{
							dialogue = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5360");
						}
					}
					i.CurrentDialogue.Push(new Dialogue(dialogue, i));
				}
			}
			if (farmerWon)
			{
				Game1.player.stardewHero = true;
			}
		}

		public static List<string> getExes(Farmer farmer)
		{
			List<string> exes = new List<string>();
			foreach (string key in farmer.friendshipData.Keys)
			{
				if (farmer.friendshipData[key].IsDivorced())
				{
					exes.Add(key);
				}
			}
			return exes;
		}

		public static string getCelebrationPositionsForDatables(List<string> people_to_exclude)
		{
			string positions = " ";
			if (!people_to_exclude.Contains("Sam"))
			{
				positions += "Sam 25 65 0 ";
			}
			if (!people_to_exclude.Contains("Sebastian"))
			{
				positions += "Sebastian 24 65 0 ";
			}
			if (!people_to_exclude.Contains("Alex"))
			{
				positions += "Alex 25 69 0 ";
			}
			if (!people_to_exclude.Contains("Harvey"))
			{
				positions += "Harvey 23 67 0 ";
			}
			if (!people_to_exclude.Contains("Elliott"))
			{
				positions += "Elliott 32 65 0 ";
			}
			if (!people_to_exclude.Contains("Haley"))
			{
				positions += "Haley 26 69 0 ";
			}
			if (!people_to_exclude.Contains("Penny"))
			{
				positions += "Penny 23 66 0 ";
			}
			if (!people_to_exclude.Contains("Maru"))
			{
				positions += "Maru 24 68 0 ";
			}
			if (!people_to_exclude.Contains("Leah"))
			{
				positions += "Leah 33 65 0 ";
			}
			if (!people_to_exclude.Contains("Abigail"))
			{
				positions += "Abigail 23 65 0 ";
			}
			return positions;
		}

		public static string getCelebrationPositionsForDatables(string personToExclude)
		{
			List<string> exclusion_list = new List<string>();
			if (personToExclude != null)
			{
				exclusion_list.Add(personToExclude);
			}
			return getCelebrationPositionsForDatables(exclusion_list);
		}

		public static void fixAllAnimals()
		{
			if (Game1.IsMasterGame)
			{
				Farm f = Game1.getFarm();
				foreach (Building b3 in f.buildings)
				{
					if (b3.indoors.Value != null && b3.indoors.Value is AnimalHouse)
					{
						foreach (long item in (b3.indoors.Value as AnimalHouse).animalsThatLiveHere)
						{
							FarmAnimal a4 = getAnimal(item);
							if (a4 != null)
							{
								a4.home = b3;
								a4.homeLocation.Value = new Vector2((int)b3.tileX, (int)b3.tileY);
							}
						}
					}
				}
				List<FarmAnimal> buggedAnimals = new List<FarmAnimal>();
				foreach (FarmAnimal a5 in f.getAllFarmAnimals())
				{
					if (a5.home == null)
					{
						buggedAnimals.Add(a5);
					}
				}
				foreach (FarmAnimal a6 in buggedAnimals)
				{
					foreach (Building b4 in f.buildings)
					{
						if (b4.indoors.Value != null && b4.indoors.Value is AnimalHouse)
						{
							for (int k = (b4.indoors.Value as AnimalHouse).animals.Count() - 1; k >= 0; k--)
							{
								if ((b4.indoors.Value as AnimalHouse).animals.Pairs.ElementAt(k).Value.Equals(a6))
								{
									(b4.indoors.Value as AnimalHouse).animals.Remove((b4.indoors.Value as AnimalHouse).animals.Pairs.ElementAt(k).Key);
								}
							}
						}
					}
					for (int j = f.animals.Count() - 1; j >= 0; j--)
					{
						if (f.animals.Pairs.ElementAt(j).Value.Equals(a6))
						{
							f.animals.Remove(f.animals.Pairs.ElementAt(j).Key);
						}
					}
				}
				foreach (Building b2 in f.buildings)
				{
					if (b2.indoors.Value != null && b2.indoors.Value is AnimalHouse)
					{
						for (int i = (b2.indoors.Value as AnimalHouse).animalsThatLiveHere.Count - 1; i >= 0; i--)
						{
							if (getAnimal((b2.indoors.Value as AnimalHouse).animalsThatLiveHere[i]).home != b2)
							{
								(b2.indoors.Value as AnimalHouse).animalsThatLiveHere.RemoveAt(i);
							}
						}
					}
				}
				foreach (FarmAnimal a3 in buggedAnimals)
				{
					foreach (Building b in f.buildings)
					{
						if (b.buildingType.Contains(a3.buildingTypeILiveIn) && b.indoors.Value != null && b.indoors.Value is AnimalHouse && !(b.indoors.Value as AnimalHouse).isFull())
						{
							a3.home = b;
							a3.homeLocation.Value = new Vector2((int)b.tileX, (int)b.tileY);
							a3.setRandomPosition(a3.home.indoors);
							(a3.home.indoors.Value as AnimalHouse).animals.Add(a3.myID, a3);
							(a3.home.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(a3.myID);
							break;
						}
					}
				}
				List<FarmAnimal> leftovers = new List<FarmAnimal>();
				foreach (FarmAnimal a2 in buggedAnimals)
				{
					if (a2.home == null)
					{
						leftovers.Add(a2);
					}
				}
				foreach (FarmAnimal a in leftovers)
				{
					a.Position = recursiveFindOpenTileForCharacter(a, f, new Vector2(40f, 40f), 200) * 64f;
					if (!f.animals.ContainsKey(a.myID))
					{
						f.animals.Add(a.myID, a);
					}
				}
			}
		}

		public static Event getWeddingEvent(Farmer farmer)
		{
			List<string> excluded_members = getExes(farmer);
			excluded_members.Add(farmer.spouse);
			return new Event("sweet/-1000 -100/farmer 27 63 2 spouse 28 63 2" + getCelebrationPositionsForDatables(excluded_members) + "Lewis 27 64 2 Marnie 26 65 0 Caroline 29 65 0 Pierre 30 65 0 Gus 31 65 0 Clint 31 66 0 " + ((farmer.spouse.Contains("Emily") || excluded_members.Contains("Emily")) ? "" : "Emily 30 66 0 ") + ((farmer.spouse.Contains("Shane") || excluded_members.Contains("Shane")) ? "" : "Shane 32 66 0 ") + ((farmer.friendshipData.ContainsKey("Sandy") && farmer.friendshipData["Sandy"].Points > 0) ? "Sandy 29 66 0 " : "") + "George 26 66 0 Evelyn 25 66 0 Pam 24 66 0 Jodi 32 67 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 31 67 0 " : "") + "otherFarmers 29 69 0 Linus 29 67 0 Robin 25 67 0 Demetrius 26 67 0 Vincent 26 68 3 Jas 25 68 1" + ((farmer.friendshipData.ContainsKey("Dwarf") && farmer.friendshipData["Dwarf"].Points > 0) ? " Dwarf 30 67 0" : "") + "/changeLocation Town/showFrame spouse 36/specificTemporarySprite wedding/viewport 27 64 true/pause 4000/speak Lewis \"" + (farmer.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5367", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5369", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName)) + "\"/faceDirection farmer 1/showFrame spouse 37/pause 500/faceDirection Lewis 0/pause 2000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5370") + "\"/move Lewis 0 1 0/playMusic none/pause 1000/showFrame Lewis 20/speak Lewis \"" + ((!farmer.IsMale) ? (isMale(farmer.spouse) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5377") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5375")) : (isMale(farmer.spouse) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5371") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5373"))) + "\"/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5379") + "\"/pause 1000/showFrame 101/showFrame spouse 38/specificTemporarySprite heart 28 62/playSound dwop/pause 2000/specificTemporarySprite wed/warp Marnie -2000 -2000/faceDirection farmer 2/showFrame spouse 36/faceDirection Pam 1 true/faceDirection Evelyn 3 true/faceDirection Pierre 3 true/faceDirection Caroline 1 true/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/move Lewis 0 3 3 true/move Caroline 0 -1 3 false/pause 4000/faceDirection farmer 1/showFrame spouse 37/globalFade/viewport -1000 -1000/pause 1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5381") + "\"/pause 500/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5383") + "\"/pause 4000/waitForOtherPlayers weddingEnd" + farmer.uniqueMultiplayerID + "/end wedding", -2, farmer);
		}

		public static Event getPlayerWeddingEvent(Farmer farmer, Farmer spouse)
		{
			List<string> excluded_members = getExes(farmer);
			excluded_members.AddRange(getExes(spouse));
			return new Event(("sweet/-1000 -100/farmer 27 63 2" + getCelebrationPositionsForDatables(excluded_members) + "Lewis 27 64 2 Marnie 26 65 0 Caroline 29 65 0 Pierre 30 65 0 Gus 31 65 0 Clint 31 66 0 " + (excluded_members.Contains("Emily") ? "" : "Emily 30 66 0 ") + (excluded_members.Contains("Shane") ? "" : "Shane 32 66 0 ") + ((farmer.friendshipData.ContainsKey("Sandy") && farmer.friendshipData["Sandy"].Points > 0) ? "Sandy 29 66 0 " : "") + "George 26 66 0 Evelyn 25 66 0 Pam 24 66 0 Jodi 32 67 0 " + ((Game1.getCharacterFromName("Kent") != null) ? "Kent 31 67 0 " : "") + "otherFarmers 29 69 0 Linus 29 67 0 Robin 25 67 0 Demetrius 26 67 0 Vincent 26 68 3 Jas 25 68 1" + ((farmer.friendshipData.ContainsKey("Dwarf") && farmer.friendshipData["Dwarf"].Points > 0) ? " Dwarf 30 67 0" : "") + "/changeLocation Town/faceDirection spouseFarmer 2/warp spouseFarmer 28 63/specificTemporarySprite wedding/viewport 27 64 true/pause 4000/speak Lewis \"" + (farmer.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5367", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5369", Game1.dayOfMonth, Game1.CurrentSeasonDisplayName)) + "\"/faceDirection farmer 1/faceDirection spouseFarmer 3/pause 500/faceDirection Lewis 0/pause 2000/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5370") + "\"/move Lewis 0 1 0/playMusic none/pause 1000/showFrame Lewis 20/speak Lewis \"" + ((!farmer.IsMale) ? (spouse.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5377") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5375")) : (spouse.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5371") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5373"))) + "\"/pause 500/speak Lewis \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5379") + "\"/pause 1000/showFrame 101/showFrame spouseFarmer 101/specificTemporarySprite heart 28 62/playSound dwop/pause 2000/specificTemporarySprite wed/warp Marnie -2000 -2000/faceDirection farmer 2/faceDirection spouseFarmer 2/faceDirection Pam 1 true/faceDirection Evelyn 3 true/faceDirection Pierre 3 true/faceDirection Caroline 1 true/animate Robin false true 500 20 21 20 22/animate Demetrius false true 500 24 25 24 26/move Lewis 0 3 3 true/move Caroline 0 -1 3 false/pause 4000/faceDirection farmer 1/showFrame spouseFarmer 3/globalFade/viewport -1000 -1000/pause 1000/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5381") + "\"/pause 500/message \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5383") + "\"/pause 4000/waitForOtherPlayers weddingEnd" + farmer.uniqueMultiplayerID + "/end wedding").Replace("spouseFarmer", "farmer" + getFarmerNumberFromFarmer(spouse)), -2, farmer);
		}

		public static void drawTinyDigits(int toDraw, SpriteBatch b, Vector2 position, float scale, float layerDepth, Color c)
		{
			int xPosition = 0;
			int currentValue = toDraw;
			int numDigits = 0;
			do
			{
				numDigits++;
			}
			while ((toDraw /= 10) >= 1);
			int digitStrip = (int)Math.Pow(10.0, numDigits - 1);
			bool significant = false;
			for (int i = 0; i < numDigits; i++)
			{
				int currentDigit = currentValue / digitStrip % 10;
				if (currentDigit > 0 || i == numDigits - 1)
				{
					significant = true;
				}
				if (significant)
				{
					b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, 0f), new Microsoft.Xna.Framework.Rectangle(368 + currentDigit * 5, 56, 5, 7), c, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
				}
				xPosition += (int)(5f * scale) - 1;
				digitStrip /= 10;
			}
		}

		public static int getWidthOfTinyDigitString(int toDraw, float scale)
		{
			int numDigits = 0;
			do
			{
				numDigits++;
			}
			while ((toDraw /= 10) >= 1);
			return (int)((float)(numDigits * 5) * scale);
		}

		public static bool isMale(string who)
		{
			switch (who)
			{
			case "Maru":
			case "Abigail":
			case "Leah":
			case "Haley":
			case "Penny":
			case "Sandy":
			case "Emily":
				return false;
			default:
				return true;
			}
		}

		public static int GetMaximumHeartsForCharacter(Character character)
		{
			if (character == null)
			{
				return 0;
			}
			int max_hearts = 10;
			if (character is NPC && (bool)((NPC)character).datable)
			{
				max_hearts = 8;
			}
			Friendship friendship = null;
			if (Game1.player.friendshipData.ContainsKey(character.Name))
			{
				friendship = Game1.player.friendshipData[character.Name];
			}
			if (friendship != null)
			{
				if (friendship.IsMarried())
				{
					max_hearts = 14;
				}
				else if (friendship.IsDating())
				{
					max_hearts = 10;
				}
			}
			return max_hearts;
		}

		public static bool doesItemWithThisIndexExistAnywhere(int index, bool bigCraftable = false)
		{
			bool item_found = false;
			iterateAllItems(delegate(Item item)
			{
				if (item is Object && (bool)(item as Object).bigCraftable == bigCraftable && (int)item.parentSheetIndex == index)
				{
					item_found = true;
				}
			});
			return item_found;
		}

		public static int getSwordUpgradeLevel()
		{
			foreach (Item t in Game1.player.items)
			{
				if (t != null && t is Sword)
				{
					return ((Tool)t).upgradeLevel;
				}
			}
			return 0;
		}

		public static bool tryToAddObjectToHome(Object o)
		{
			GameLocation home = Game1.getLocationFromName("FarmHouse");
			for (int x = home.map.GetLayer("Back").LayerWidth - 1; x >= 0; x--)
			{
				for (int y = home.map.GetLayer("Back").LayerHeight - 1; y >= 0; y--)
				{
					if (home.map.GetLayer("Back").Tiles[x, y] != null && home.dropObject(o, new Vector2(x * 64, y * 64), Game1.viewport, initialPlacement: false))
					{
						if (o.ParentSheetIndex == 468)
						{
							Object table = new Object(new Vector2(x, y), 308, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
							table.heldObject.Value = o;
							home.objects[new Vector2(x, y)] = table;
						}
						return true;
					}
				}
			}
			return false;
		}

		internal static void CollectGarbage(string filePath = "", int lineNumber = 0)
		{
			GC.Collect(0, GCCollectionMode.Forced);
		}

		public static string InvokeSimpleReturnTypeMethod(object toBeCalled, string methodName, object[] parameters)
		{
			Type calledType = toBeCalled.GetType();
			string s = "";
			try
			{
				return ((string)calledType.InvokeMember(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, toBeCalled, parameters)) ?? "";
			}
			catch (Exception e)
			{
				return Game1.parseText("Didn't work - " + e.Message);
			}
		}

		public static List<int> possibleCropsAtThisTime(string season, bool firstWeek)
		{
			List<int> firstWeekCrops = null;
			List<int> secondWeekCrops = null;
			if (season.Equals("spring"))
			{
				firstWeekCrops = new List<int>
				{
					24,
					192
				};
				if (Game1.year > 1)
				{
					firstWeekCrops.Add(250);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					firstWeekCrops.Add(248);
				}
				secondWeekCrops = new List<int>
				{
					190,
					188
				};
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					secondWeekCrops.Add(252);
				}
				secondWeekCrops.AddRange(firstWeekCrops);
			}
			else if (season.Equals("summer"))
			{
				firstWeekCrops = new List<int>
				{
					264,
					262,
					260
				};
				secondWeekCrops = new List<int>
				{
					254,
					256
				};
				if (Game1.year > 1)
				{
					firstWeekCrops.Add(266);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					secondWeekCrops.AddRange(new int[2]
					{
						258,
						268
					});
				}
				secondWeekCrops.AddRange(firstWeekCrops);
			}
			else if (season.Equals("fall"))
			{
				firstWeekCrops = new List<int>
				{
					272,
					278
				};
				secondWeekCrops = new List<int>
				{
					270,
					276,
					280
				};
				if (Game1.year > 1)
				{
					secondWeekCrops.Add(274);
				}
				if (doesAnyFarmerHaveMail("ccVault"))
				{
					firstWeekCrops.Add(284);
					secondWeekCrops.Add(282);
				}
				secondWeekCrops.AddRange(firstWeekCrops);
			}
			if (firstWeek)
			{
				return firstWeekCrops;
			}
			return secondWeekCrops;
		}

		public static int[] cropsOfTheWeek()
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)(Game1.stats.DaysPlayed / 29u));
			int[] cropsOfTheWeek = new int[4];
			List<int> firstWeekCrops = possibleCropsAtThisTime(Game1.currentSeason, firstWeek: true);
			List<int> secondWeekCrops = possibleCropsAtThisTime(Game1.currentSeason, firstWeek: false);
			if (firstWeekCrops != null)
			{
				cropsOfTheWeek[0] = firstWeekCrops.ElementAt(r.Next(firstWeekCrops.Count));
				for (int i = 1; i < 4; i++)
				{
					cropsOfTheWeek[i] = secondWeekCrops.ElementAt(r.Next(secondWeekCrops.Count));
					while (cropsOfTheWeek[i] == cropsOfTheWeek[i - 1])
					{
						cropsOfTheWeek[i] = secondWeekCrops.ElementAt(r.Next(secondWeekCrops.Count));
					}
				}
			}
			return cropsOfTheWeek;
		}

		public static float RandomFloat(float min, float max, Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			return Lerp(min, max, (float)random.NextDouble());
		}

		public static float Clamp(float value, float min, float max)
		{
			if (max < min)
			{
				float num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		public static Color MakeCompletelyOpaque(Color color)
		{
			if (color.A >= byte.MaxValue)
			{
				return color;
			}
			color.A = byte.MaxValue;
			return color;
		}

		public static int Clamp(int value, int min, int max)
		{
			if (max < min)
			{
				int num = min;
				min = max;
				max = num;
			}
			if (value < min)
			{
				value = min;
			}
			if (value > max)
			{
				value = max;
			}
			return value;
		}

		public static float Lerp(float a, float b, float t)
		{
			return a + t * (b - a);
		}

		public static float MoveTowards(float from, float to, float delta)
		{
			if (Math.Abs(to - from) <= delta)
			{
				return to;
			}
			return from + (float)Math.Sign(to - from) * delta;
		}

		public static Color MultiplyColor(Color a, Color b)
		{
			return new Color((float)(int)a.R / 255f * ((float)(int)b.R / 255f), (float)(int)a.G / 255f * ((float)(int)b.G / 255f), (float)(int)a.B / 255f * ((float)(int)b.B / 255f), (float)(int)a.A / 255f * ((float)(int)b.A / 255f));
		}

		public static int CalculateMinutesUntilMorning(int currentTime)
		{
			return CalculateMinutesUntilMorning(currentTime, 1);
		}

		public static int CalculateMinutesUntilMorning(int currentTime, int daysElapsed)
		{
			if (daysElapsed <= 0)
			{
				return 0;
			}
			daysElapsed--;
			return ConvertTimeToMinutes(2600) - ConvertTimeToMinutes(currentTime) + 400 + daysElapsed * 1600;
		}

		public static int CalculateMinutesBetweenTimes(int startTime, int endTime)
		{
			return ConvertTimeToMinutes(endTime) - ConvertTimeToMinutes(startTime);
		}

		public static int ModifyTime(int timestamp, int minutes_to_add)
		{
			timestamp = ConvertTimeToMinutes(timestamp);
			timestamp += minutes_to_add;
			return ConvertMinutesToTime(timestamp);
		}

		public static int ConvertMinutesToTime(int minutes)
		{
			return minutes / 60 * 100 + minutes % 60;
		}

		public static int ConvertTimeToMinutes(int time_stamp)
		{
			return time_stamp / 100 * 60 + time_stamp % 100;
		}

		public static int getSellToStorePriceOfItem(Item i, bool countStack = true)
		{
			if (i != null)
			{
				return ((i is Object) ? (i as Object).sellToStorePrice(-1L) : (i.salePrice() / 2)) * ((!countStack) ? 1 : i.Stack);
			}
			return 0;
		}

		public static bool HasAnyPlayerSeenSecretNote(int note_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.secretNotesSeen.Contains(note_number))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HasAnyPlayerSeenEvent(int event_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.eventsSeen.Contains(event_number))
				{
					return true;
				}
			}
			return false;
		}

		public static bool HaveAllPlayersSeenEvent(int event_number)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (!allFarmer.eventsSeen.Contains(event_number))
				{
					return false;
				}
			}
			return true;
		}

		public static List<string> GetAllPlayerUnlockedCookingRecipes()
		{
			List<string> unlocked_recipes = new List<string>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (string recipe in allFarmer.cookingRecipes.Keys)
				{
					if (!unlocked_recipes.Contains(recipe))
					{
						unlocked_recipes.Add(recipe);
					}
				}
			}
			return unlocked_recipes;
		}

		public static List<string> GetAllPlayerUnlockedCraftingRecipes()
		{
			List<string> unlocked_recipes = new List<string>();
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (string recipe in allFarmer.craftingRecipes.Keys)
				{
					if (!unlocked_recipes.Contains(recipe))
					{
						unlocked_recipes.Add(recipe);
					}
				}
			}
			return unlocked_recipes;
		}

		public static int GetAllPlayerFriendshipLevel(NPC npc)
		{
			int highest_friendship_points = -1;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.friendshipData.ContainsKey(npc.Name) && farmer.friendshipData[npc.Name].Points > highest_friendship_points)
				{
					highest_friendship_points = farmer.friendshipData[npc.Name].Points;
				}
			}
			return highest_friendship_points;
		}

		public static int GetAllPlayerDeepestMineLevel()
		{
			int highest_value = 0;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.deepestMineLevel > highest_value)
				{
					highest_value = farmer.deepestMineLevel;
				}
			}
			return highest_value;
		}

		public static int getRandomBasicSeasonalForageItem(string season, int randomSeedAddition = -1)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + randomSeedAddition);
			List<int> possibleItems = new List<int>();
			if (season.Equals("spring"))
			{
				possibleItems.AddRange(new int[4]
				{
					16,
					18,
					20,
					22
				});
			}
			else if (season.Equals("summer"))
			{
				possibleItems.AddRange(new int[3]
				{
					396,
					398,
					402
				});
			}
			else if (season.Equals("fall"))
			{
				possibleItems.AddRange(new int[4]
				{
					404,
					406,
					408,
					410
				});
			}
			else if (season.Equals("winter"))
			{
				possibleItems.AddRange(new int[4]
				{
					412,
					414,
					416,
					418
				});
			}
			return possibleItems.ElementAt(r.Next(possibleItems.Count));
		}

		public static int getRandomPureSeasonalItem(string season, int randomSeedAddition)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + randomSeedAddition);
			List<int> possibleItems = new List<int>();
			if (season.Equals("spring"))
			{
				possibleItems.AddRange(new int[15]
				{
					16,
					18,
					20,
					22,
					129,
					131,
					132,
					136,
					137,
					142,
					143,
					145,
					147,
					148,
					152
				});
			}
			else if (season.Equals("summer"))
			{
				possibleItems.AddRange(new int[16]
				{
					128,
					130,
					131,
					132,
					136,
					138,
					142,
					144,
					145,
					146,
					149,
					150,
					155,
					396,
					398,
					402
				});
			}
			else if (season.Equals("fall"))
			{
				possibleItems.AddRange(new int[17]
				{
					404,
					406,
					408,
					410,
					129,
					131,
					132,
					136,
					137,
					139,
					140,
					142,
					143,
					148,
					150,
					154,
					155
				});
			}
			else if (season.Equals("winter"))
			{
				possibleItems.AddRange(new int[17]
				{
					412,
					414,
					416,
					418,
					130,
					131,
					132,
					136,
					140,
					141,
					143,
					144,
					146,
					147,
					150,
					151,
					154
				});
			}
			return possibleItems.ElementAt(r.Next(possibleItems.Count));
		}

		public static int getRandomItemFromSeason(string season, int randomSeedAddition, bool forQuest, bool changeDaily = true)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)(changeDaily ? Game1.stats.DaysPlayed : 0) + randomSeedAddition);
			List<int> possibleItems = new List<int>
			{
				68,
				66,
				78,
				80,
				86,
				152,
				167,
				153,
				420
			};
			List<string> all_unlocked_crafting_recipes = new List<string>(Game1.player.craftingRecipes.Keys);
			List<string> all_unlocked_cooking_recipes = new List<string>(Game1.player.cookingRecipes.Keys);
			if (forQuest)
			{
				all_unlocked_crafting_recipes = GetAllPlayerUnlockedCraftingRecipes();
				all_unlocked_cooking_recipes = GetAllPlayerUnlockedCookingRecipes();
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 40 || GetAllPlayerDeepestMineLevel() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 40 || Game1.player.timesReachedMineBottom >= 1)))
			{
				possibleItems.AddRange(new int[5]
				{
					62,
					70,
					72,
					84,
					422
				});
			}
			if ((forQuest && (MineShaft.lowestLevelReached > 80 || GetAllPlayerDeepestMineLevel() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 80 || Game1.player.timesReachedMineBottom >= 1)))
			{
				possibleItems.AddRange(new int[3]
				{
					64,
					60,
					82
				});
			}
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				possibleItems.AddRange(new int[4]
				{
					88,
					90,
					164,
					165
				});
			}
			if (all_unlocked_crafting_recipes.Contains("Furnace"))
			{
				possibleItems.AddRange(new int[4]
				{
					334,
					335,
					336,
					338
				});
			}
			if (all_unlocked_crafting_recipes.Contains("Quartz Globe"))
			{
				possibleItems.Add(339);
			}
			if (season.Equals("spring"))
			{
				possibleItems.AddRange(new int[17]
				{
					16,
					18,
					20,
					22,
					129,
					131,
					132,
					136,
					137,
					142,
					143,
					145,
					147,
					148,
					152,
					167,
					267
				});
			}
			else if (season.Equals("summer"))
			{
				possibleItems.AddRange(new int[16]
				{
					128,
					130,
					132,
					136,
					138,
					142,
					144,
					145,
					146,
					149,
					150,
					155,
					396,
					398,
					402,
					267
				});
			}
			else if (season.Equals("fall"))
			{
				possibleItems.AddRange(new int[18]
				{
					404,
					406,
					408,
					410,
					129,
					131,
					132,
					136,
					137,
					139,
					140,
					142,
					143,
					148,
					150,
					154,
					155,
					269
				});
			}
			else if (season.Equals("winter"))
			{
				possibleItems.AddRange(new int[17]
				{
					412,
					414,
					416,
					418,
					130,
					131,
					132,
					136,
					140,
					141,
					144,
					146,
					147,
					150,
					151,
					154,
					269
				});
			}
			if (forQuest)
			{
				foreach (string s in all_unlocked_cooking_recipes)
				{
					if (!(r.NextDouble() < 0.4))
					{
						List<int> cropsAvailableNow = possibleCropsAtThisTime(Game1.currentSeason, (Game1.dayOfMonth <= 7) ? true : false);
						Dictionary<string, string> cookingRecipes = Game1.content.Load<Dictionary<string, string>>("Data//CookingRecipes");
						if (cookingRecipes.ContainsKey(s))
						{
							string[] ingredientsSplit = cookingRecipes[s].Split('/')[0].Split(' ');
							bool ingredientsAvailable = true;
							for (int i = 0; i < ingredientsSplit.Length; i++)
							{
								if (!possibleItems.Contains(Convert.ToInt32(ingredientsSplit[i])) && !isCategoryIngredientAvailable(Convert.ToInt32(ingredientsSplit[i])) && (cropsAvailableNow == null || !cropsAvailableNow.Contains(Convert.ToInt32(ingredientsSplit[i]))))
								{
									ingredientsAvailable = false;
									break;
								}
							}
							if (ingredientsAvailable)
							{
								possibleItems.Add(Convert.ToInt32(cookingRecipes[s].Split('/')[2]));
							}
						}
					}
				}
			}
			return possibleItems.ElementAt(r.Next(possibleItems.Count));
		}

		private static bool isCategoryIngredientAvailable(int category)
		{
			if (category < 0)
			{
				switch (category)
				{
				case -5:
					return false;
				case -6:
					return false;
				default:
					return true;
				}
			}
			return false;
		}

		public static int weatherDebrisOffsetForSeason(string season)
		{
			if (!(season == "spring"))
			{
				if (!(season == "summer"))
				{
					if (!(season == "fall"))
					{
						if (season == "winter")
						{
							return 20;
						}
						return 0;
					}
					return 18;
				}
				return 24;
			}
			return 16;
		}

		public static Color getSkyColorForSeason(string season)
		{
			if (!(season == "spring"))
			{
				if (!(season == "summer"))
				{
					if (!(season == "fall"))
					{
						if (season == "winter")
						{
							return new Color(165, 207, 255);
						}
						return new Color(92, 170, 255);
					}
					return new Color(255, 184, 151);
				}
				return new Color(24, 163, 255);
			}
			return new Color(92, 170, 255);
		}

		public static void farmerHeardSong(string trackName)
		{
			List<string> adjustedNames = new List<string>();
			if (trackName.Equals("EarthMine"))
			{
				if (!Game1.player.songsHeard.Contains("Crystal Bells"))
				{
					adjustedNames.Add("Crystal Bells");
				}
				if (!Game1.player.songsHeard.Contains("Cavern"))
				{
					adjustedNames.Add("Cavern");
				}
				if (!Game1.player.songsHeard.Contains("Secret Gnomes"))
				{
					adjustedNames.Add("Secret Gnomes");
				}
			}
			else if (trackName.Equals("FrostMine"))
			{
				if (!Game1.player.songsHeard.Contains("Cloth"))
				{
					adjustedNames.Add("Cloth");
				}
				if (!Game1.player.songsHeard.Contains("Icicles"))
				{
					adjustedNames.Add("Icicles");
				}
				if (!Game1.player.songsHeard.Contains("XOR"))
				{
					adjustedNames.Add("XOR");
				}
			}
			else if (trackName.Equals("LavaMine"))
			{
				if (!Game1.player.songsHeard.Contains("Of Dwarves"))
				{
					adjustedNames.Add("Of Dwarves");
				}
				if (!Game1.player.songsHeard.Contains("Near The Planet Core"))
				{
					adjustedNames.Add("Near The Planet Core");
				}
				if (!Game1.player.songsHeard.Contains("Overcast"))
				{
					adjustedNames.Add("Overcast");
				}
				if (!Game1.player.songsHeard.Contains("tribal"))
				{
					adjustedNames.Add("tribal");
				}
			}
			else if (trackName.Equals("VolcanoMines"))
			{
				if (!Game1.player.songsHeard.Contains("VolcanoMines1"))
				{
					adjustedNames.Add("VolcanoMines1");
				}
				if (!Game1.player.songsHeard.Contains("VolcanoMines2"))
				{
					adjustedNames.Add("VolcanoMines2");
				}
			}
			else if (!trackName.Equals("none") && !trackName.Equals("rain"))
			{
				adjustedNames.Add(trackName);
			}
			foreach (string s in adjustedNames)
			{
				if (!Game1.player.songsHeard.Contains(s))
				{
					Game1.player.songsHeard.Add(s);
				}
			}
		}

		public static float getMaxedFriendshipPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			float numMaxedFriends = 0f;
			Dictionary<string, string> disposition = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (KeyValuePair<string, Friendship> friend in who.friendshipData.Pairs)
			{
				if (disposition.ContainsKey(friend.Key))
				{
					Friendship friendship = friend.Value;
					bool isDatable = disposition[friend.Key].Split('/')[5] == "datable";
					if (friendship.Points >= 250 * (isDatable ? 8 : 10))
					{
						numMaxedFriends += 1f;
					}
				}
			}
			int totalNPCs = disposition.Count - 1;
			return numMaxedFriends / (float)totalNPCs;
		}

		public static float getCookedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> recipes = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
			float numberOfRecipesCooked = 0f;
			foreach (KeyValuePair<string, string> v in recipes)
			{
				if (who.cookingRecipes.ContainsKey(v.Key))
				{
					int recipe = Convert.ToInt32(v.Value.Split('/')[2].Split(' ')[0]);
					if (who.recipesCooked.ContainsKey(recipe))
					{
						numberOfRecipesCooked += 1f;
					}
				}
			}
			return numberOfRecipesCooked / (float)recipes.Count;
		}

		public static float getCraftedRecipesPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			Dictionary<string, string> recipes = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
			float numberOfRecipesMade = 0f;
			foreach (string s in recipes.Keys)
			{
				if (!(s == "Wedding Ring") && who.craftingRecipes.ContainsKey(s) && who.craftingRecipes[s] > 0)
				{
					numberOfRecipesMade += 1f;
				}
			}
			return numberOfRecipesMade / ((float)recipes.Count - 1f);
		}

		public static float getFishCaughtPercent(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			float differentKindsOfFishCaught = 0f;
			float totalKindsOfFish = 0f;
			foreach (KeyValuePair<int, string> v in Game1.objectInformation)
			{
				if (v.Value.Split('/')[3].Contains("Fish") && (v.Key < 167 || v.Key > 172) && (v.Key < 898 || v.Key > 902))
				{
					totalKindsOfFish += 1f;
					if (who.fishCaught.ContainsKey(v.Key))
					{
						differentKindsOfFishCaught += 1f;
					}
				}
			}
			return differentKindsOfFishCaught / totalKindsOfFish;
		}

		public static KeyValuePair<Farmer, bool> GetFarmCompletion(Func<Farmer, bool> check)
		{
			if (check(Game1.player))
			{
				return new KeyValuePair<Farmer, bool>(Game1.player, value: true);
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value && check(farmer))
				{
					return new KeyValuePair<Farmer, bool>(farmer, value: true);
				}
			}
			return new KeyValuePair<Farmer, bool>(Game1.player, value: false);
		}

		public static KeyValuePair<Farmer, float> GetFarmCompletion(Func<Farmer, float> check)
		{
			Farmer highest_farmer = Game1.player;
			float highest_value = check(Game1.player);
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer != Game1.player && farmer.isCustomized.Value)
				{
					float current_value = check(farmer);
					if (current_value > highest_value)
					{
						highest_farmer = farmer;
						highest_value = current_value;
					}
				}
			}
			return new KeyValuePair<Farmer, float>(highest_farmer, highest_value);
		}

		public static float percentGameComplete()
		{
			float total12 = 0f;
			float num = 0f + GetFarmCompletion((Farmer farmer) => getFarmerItemsShippedPercent(farmer)).Value * 15f;
			total12 += 15f;
			float num2 = num + Math.Min(numObelisksOnFarm(), 4f);
			total12 += 4f;
			float num3 = num2 + (float)(Game1.getFarm().isBuildingConstructed("Gold Clock") ? 10 : 0);
			total12 += 10f;
			float num4 = num3 + (float)(GetFarmCompletion((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value).Value ? 10 : 0);
			total12 += 10f;
			float NPCFriendPercent = GetFarmCompletion((Farmer farmer) => getMaxedFriendshipPercent(farmer)).Value;
			float num5 = num4 + NPCFriendPercent * 11f;
			total12 += 11f;
			float farmerLevelPercent = GetFarmCompletion((Farmer farmer) => Math.Min(farmer.Level, 25f) / 25f).Value;
			float num6 = num5 + farmerLevelPercent * 5f;
			total12 += 5f;
			float num7 = num6 + (float)(GetFarmCompletion((Farmer farmer) => foundAllStardrops(farmer)).Value ? 10 : 0);
			total12 += 10f;
			float num8 = num7 + GetFarmCompletion((Farmer farmer) => getCookedRecipesPercent(farmer)).Value * 10f;
			total12 += 10f;
			float num9 = num8 + GetFarmCompletion((Farmer farmer) => getCraftedRecipesPercent(farmer)).Value * 10f;
			total12 += 10f;
			float num10 = num9 + GetFarmCompletion((Farmer farmer) => getFishCaughtPercent(farmer)).Value * 10f;
			total12 += 10f;
			float totalNuts = 130f;
			float walnutsFound = Math.Min((int)Game1.netWorldState.Value.GoldenWalnutsFound, totalNuts);
			float num11 = num10 + walnutsFound / totalNuts * 5f;
			total12 += 5f;
			return num11 / total12;
		}

		public static int numObelisksOnFarm()
		{
			return (Game1.getFarm().isBuildingConstructed("Water Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Earth Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Desert Obelisk") ? 1 : 0) + (Game1.getFarm().isBuildingConstructed("Island Obelisk") ? 1 : 0);
		}

		public static bool IsDesertLocation(GameLocation location)
		{
			if (location.Name == "Desert" || location.Name == "SkullCave" || location.Name == "Club" || location.Name == "SandyHouse" || location.Name == "SandyShop")
			{
				return true;
			}
			return false;
		}

		public static List<string> getOtherFarmerNames()
		{
			List<string> otherFarmerNames = new List<string>();
			Random r = new Random((int)Game1.uniqueIDForThisGame);
			Random dayRandom = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			string[] maleNames = new string[33]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5499"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5500"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5501"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5502"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5503"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5504"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5505"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5506"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5507"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5508"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5509"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5510"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5511"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5512"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5513"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5514"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5515"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5516"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5517"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5518"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5519"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5520"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5521"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5522"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5523"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5524"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5525"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5526"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5527"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5528"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5529"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5530"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5531")
			};
			string[] femaleNames = new string[29]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5532"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5533"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5534"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5535"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5536"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5537"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5538"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5539"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5540"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5541"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5542"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5543"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5544"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5545"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5546"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5547"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5548"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5549"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5550"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5551"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5552"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5553"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5554"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5555"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5556"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5557"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5558"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5559"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5560")
			};
			string[] maletitles = new string[17]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5562"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5563"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5564"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5565"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5566"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5567"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5569"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5570"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5571"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5572"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5573"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5574"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5575"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5576"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5577")
			};
			string[] femaletitles = new string[12]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5562"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5573"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5581"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5582"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5583"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5585"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5586"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5587"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5588"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5589")
			};
			string[] maleGarbagetitles = new string[28]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5590"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5591"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5592"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5593"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5594"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5595"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5596"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5597"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5598"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5599"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5600"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5601"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5602"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5603"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5604"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5605"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5606"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5607"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5608"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5609"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5610"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5611"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5612"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5613"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5614"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5615"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5616"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5617")
			};
			string[] femaleGarbagetitles = new string[21]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5618"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5619"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5620"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5607"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5622"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5623"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5624"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5625"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5626"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5627"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5628"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5629"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5630"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5631"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5632"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5633"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5634"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5635"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5636"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5637"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5638")
			};
			string[] malegarbageNicknames = new string[9]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5639"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5640"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5641"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5642"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5643"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5644"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5645"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5646"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5647")
			};
			string[] maleRivalTitles = new string[4]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5569"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5651")
			};
			string[] femaleRivalTitles = new string[4]
			{
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5561"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5568"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5585"),
				Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5655")
			};
			string name6 = "";
			if (Game1.player.IsMale)
			{
				name6 = maleNames[r.Next(maleNames.Length)];
				for (int j = 0; j < 2; j++)
				{
					while (otherFarmerNames.Contains(name6) || Game1.player.Name.Equals(name6))
					{
						name6 = ((j != 0) ? maleNames[dayRandom.Next(maleNames.Length)] : maleNames[r.Next(maleNames.Length)]);
					}
					name6 = ((j != 0) ? (maletitles[dayRandom.Next(maletitles.Length)] + " " + name6) : (maleRivalTitles[r.Next(maleRivalTitles.Length)] + " " + name6));
					otherFarmerNames.Add(name6);
				}
			}
			else
			{
				name6 = femaleNames[r.Next(femaleNames.Length)];
				for (int i = 0; i < 2; i++)
				{
					while (otherFarmerNames.Contains(name6) || Game1.player.Name.Equals(name6))
					{
						name6 = ((i != 0) ? femaleNames[dayRandom.Next(femaleNames.Length)] : femaleNames[r.Next(femaleNames.Length)]);
					}
					name6 = ((i != 0) ? (femaletitles[dayRandom.Next(femaletitles.Length)] + " " + name6) : (femaleRivalTitles[r.Next(femaleRivalTitles.Length)] + " " + name6));
					otherFarmerNames.Add(name6);
				}
			}
			if (dayRandom.NextDouble() < 0.5)
			{
				name6 = maleNames[dayRandom.Next(maleNames.Length)];
				while (Game1.player.Name.Equals(name6))
				{
					name6 = maleNames[dayRandom.Next(maleNames.Length)];
				}
				name6 = ((!(dayRandom.NextDouble() < 0.5)) ? (name6 + " " + malegarbageNicknames[dayRandom.Next(malegarbageNicknames.Length)]) : (maleGarbagetitles[dayRandom.Next(maleGarbagetitles.Length)] + " " + name6));
			}
			else
			{
				name6 = femaleNames[dayRandom.Next(femaleNames.Length)];
				while (Game1.player.Name.Equals(name6))
				{
					name6 = femaleNames[dayRandom.Next(femaleNames.Length)];
				}
				name6 = femaleGarbagetitles[dayRandom.Next(femaleGarbagetitles.Length)] + " " + name6;
			}
			otherFarmerNames.Add(name6);
			return otherFarmerNames;
		}

		public static string getStardewHeroStandingsString()
		{
			string standings = "";
			Random r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			List<string> otherFarmers = getOtherFarmerNames();
			int[] otherFarmerScores = new int[otherFarmers.Count];
			otherFarmerScores[0] = (int)((float)(double)Game1.stats.DaysPlayed / 208f * (float)Game1.percentageToWinStardewHero);
			otherFarmerScores[1] = (int)((float)otherFarmerScores[0] * 0.75f + (float)r.Next(-5, 5));
			otherFarmerScores[2] = Math.Max(0, otherFarmerScores[1] / 2 + r.Next(-10, 0));
			if (Game1.stats.DaysPlayed < 30)
			{
				otherFarmerScores[0] += 3;
			}
			else if (Game1.stats.DaysPlayed < 60)
			{
				otherFarmerScores[0] += 7;
			}
			float farmerScore = percentGameComplete();
			bool farmerPlaced = false;
			for (int i = 0; i < 3; i++)
			{
				if (farmerScore > (float)otherFarmerScores[i] && !farmerPlaced)
				{
					farmerPlaced = true;
					standings = standings + Game1.player.getTitle() + " " + Game1.player.Name + " ....... " + farmerScore + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657") + Environment.NewLine;
				}
				standings = standings + otherFarmers[i] + " ....... " + otherFarmerScores[i] + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657") + Environment.NewLine;
			}
			if (!farmerPlaced)
			{
				standings = standings + Game1.player.getTitle() + " " + Game1.player.Name + " ....... " + farmerScore + Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5657");
			}
			return standings;
		}

		private static int cosmicFruitPercent()
		{
			return Math.Max(0, (Game1.player.MaxStamina - 120) / 20);
		}

		private static int minePercentage()
		{
			if (Game1.player.timesReachedMineBottom > 0)
			{
				return 4;
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				return 2;
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				return 1;
			}
			return 0;
		}

		private static int cookingPercent()
		{
			int recipesCooked = 0;
			foreach (string s in Game1.player.cookingRecipes.Keys)
			{
				if (Game1.player.cookingRecipes[s] > 0)
				{
					recipesCooked++;
				}
			}
			return (int)((float)(recipesCooked / Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes").Count) * 3f);
		}

		private static int craftingPercent()
		{
			int recipesCrafted = 0;
			foreach (string s in Game1.player.craftingRecipes.Keys)
			{
				if (Game1.player.craftingRecipes[s] > 0)
				{
					recipesCrafted++;
				}
			}
			return (int)((float)(recipesCrafted / Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes").Count) * 3f);
		}

		private static int achievementsPercent()
		{
			return (int)((float)(Game1.player.achievements.Count / Game1.content.Load<Dictionary<int, string>>("Data\\achievements").Count) * 15f);
		}

		private static int itemsShippedPercent()
		{
			return (int)((float)Game1.player.basicShipped.Count() / 92f * 5f);
		}

		private static int artifactsPercent()
		{
			return (int)((float)Game1.player.archaeologyFound.Count() / 32f * 3f);
		}

		private static int fishPercent()
		{
			return (int)((float)Game1.player.fishCaught.Count() / 42f * 3f);
		}

		private static int upgradePercent()
		{
			int upgradePercent4 = 0;
			foreach (Item t in Game1.player.items)
			{
				if (t != null && t is Tool && (t.Name.Contains("Hoe") || t.Name.Contains("Axe") || t.Name.Contains("Pickaxe") || t.Name.Contains("Can")) && (int)((Tool)t).upgradeLevel == 4)
				{
					upgradePercent4++;
				}
			}
			upgradePercent4 += Game1.player.HouseUpgradeLevel;
			upgradePercent4 += Game1.player.CoopUpgradeLevel;
			upgradePercent4 += Game1.player.BarnUpgradeLevel;
			if (Game1.player.hasGreenhouse)
			{
				upgradePercent4++;
			}
			return upgradePercent4;
		}

		private static int friendshipPercent()
		{
			int friendshipPoints = 0;
			foreach (string s in Game1.player.friendshipData.Keys)
			{
				friendshipPoints += Game1.player.friendshipData[s].Points;
			}
			return Math.Min(10, (int)((float)friendshipPoints / 70000f * 10f));
		}

		private static bool playerHasGalaxySword()
		{
			foreach (Item t in Game1.player.Items)
			{
				if (t != null && t is Sword && t.Name.Contains("Galaxy"))
				{
					return true;
				}
			}
			return false;
		}

		public static int getTrashReclamationPrice(Item i, Farmer f)
		{
			float sellPercentage = 0.15f * (float)f.trashCanLevel;
			if (i.canBeTrashed())
			{
				if (i is Wallpaper || i is Furniture)
				{
					return -1;
				}
				if (i is Object && !(i as Object).bigCraftable)
				{
					return (int)((float)i.Stack * ((float)(i as Object).sellToStorePrice(-1L) * sellPercentage));
				}
				if (i is MeleeWeapon || i is Ring || i is Boots)
				{
					return (int)((float)i.Stack * ((float)(i.salePrice() / 2) * sellPercentage));
				}
			}
			return -1;
		}

		public static Quest getQuestOfTheDay()
		{
			if (Game1.stats.DaysPlayed <= 1)
			{
				return null;
			}
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			Quest quest = null;
			double d = random.NextDouble();
			if (d < 0.08)
			{
				return new ResourceCollectionQuest();
			}
			if (d < 0.18 && MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5)
			{
				return new SlayMonsterQuest();
			}
			if (d < 0.53)
			{
				return null;
			}
			if (d < 0.6)
			{
				return new FishingQuest();
			}
			return new ItemDeliveryQuest();
		}

		public static Color getOppositeColor(Color color)
		{
			return new Color(255 - color.R, 255 - color.G, 255 - color.B);
		}

		public static void drawLightningBolt(Vector2 strikePosition, GameLocation l)
		{
			Microsoft.Xna.Framework.Rectangle lightningSourceRect = new Microsoft.Xna.Framework.Rectangle(644, 1078, 37, 57);
			Vector2 drawPosition = strikePosition + new Vector2(-lightningSourceRect.Width * 4 / 2, -lightningSourceRect.Height * 4);
			while (drawPosition.Y > (float)(-lightningSourceRect.Height * 4))
			{
				l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, flicker: false, Game1.random.NextDouble() < 0.5, (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 2f,
					delayBeforeAnimationStart = 200,
					lightcolor = Color.Black
				});
				drawPosition.Y -= lightningSourceRect.Height * 4;
			}
		}

		public static string getDateStringFor(int currentDay, int currentSeason, int currentYear)
		{
			if (currentDay <= 0)
			{
				currentDay += 28;
				currentSeason--;
				if (currentSeason < 0)
				{
					currentSeason = 3;
					currentYear--;
				}
			}
			else if (currentDay > 28)
			{
				currentDay -= 28;
				currentSeason++;
				if (currentSeason > 3)
				{
					currentSeason = 0;
					currentYear++;
				}
			}
			if (currentYear == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5677");
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5678", currentDay, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? getSeasonNameFromNumber(currentSeason).ToLower() : getSeasonNameFromNumber(currentSeason), currentYear);
		}

		public static string getDateString(int offset = 0)
		{
			int dayOfMonth = Game1.dayOfMonth;
			int currentSeason = getSeasonNumber(Game1.currentSeason);
			int currentYear = Game1.year;
			return getDateStringFor(dayOfMonth + offset, currentSeason, currentYear);
		}

		public static string getYesterdaysDate()
		{
			return getDateString(-1);
		}

		public static string getSeasonNameFromNumber(int number)
		{
			switch (number)
			{
			case 0:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5680");
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5681");
			case 2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5682");
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5683");
			default:
				return "";
			}
		}

		public static string getNumberEnding(int number)
		{
			if (number % 100 > 10 && number % 100 < 20)
			{
				return "th";
			}
			switch (number % 10)
			{
			case 0:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
				return "th";
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "";
			}
		}

		public static void killAllStaticLoopingSoundCues()
		{
			if (Game1.soundBank != null)
			{
				if (Intro.roadNoise != null)
				{
					Intro.roadNoise.Stop(AudioStopOptions.Immediate);
				}
				if (Fly.buzz != null)
				{
					Fly.buzz.Stop(AudioStopOptions.Immediate);
				}
				if (Railroad.trainLoop != null)
				{
					Railroad.trainLoop.Stop(AudioStopOptions.Immediate);
				}
				if (BobberBar.reelSound != null)
				{
					BobberBar.reelSound.Stop(AudioStopOptions.Immediate);
				}
				if (BobberBar.unReelSound != null)
				{
					BobberBar.unReelSound.Stop(AudioStopOptions.Immediate);
				}
				if (FishingRod.reelSound != null)
				{
					FishingRod.reelSound.Stop(AudioStopOptions.Immediate);
				}
			}
			Game1.locationCues.StopAll();
		}

		public static void consolidateStacks(IList<Item> objects)
		{
			for (int k = 0; k < objects.Count; k++)
			{
				if (objects[k] == null || !(objects[k] is Object))
				{
					continue;
				}
				Object o = objects[k] as Object;
				for (int i = k + 1; i < objects.Count; i++)
				{
					if (objects[i] != null && o.canStackWith(objects[i]))
					{
						o.Stack = objects[i].addToStack(o);
						if (o.Stack <= 0)
						{
							break;
						}
					}
				}
			}
			for (int j = objects.Count - 1; j >= 0; j--)
			{
				if (objects[j] != null && objects[j].Stack <= 0)
				{
					objects.RemoveAt(j);
				}
			}
		}

		public static void performLightningUpdate(int time_of_day)
		{
			Random random = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + time_of_day);
			if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
			{
				Farm.LightningStrikeEvent lightningEvent2 = new Farm.LightningStrikeEvent();
				lightningEvent2.bigFlash = true;
				Farm farm2 = Game1.getLocationFromName("Farm") as Farm;
				List<Vector2> lightningRods = new List<Vector2>();
				foreach (KeyValuePair<Vector2, Object> v2 in farm2.objects.Pairs)
				{
					if ((bool)v2.Value.bigCraftable && v2.Value.ParentSheetIndex == 9)
					{
						lightningRods.Add(v2.Key);
					}
				}
				if (lightningRods.Count > 0)
				{
					for (int i = 0; i < 2; i++)
					{
						Vector2 v = lightningRods.ElementAt(random.Next(lightningRods.Count));
						if (farm2.objects[v].heldObject.Value == null)
						{
							farm2.objects[v].heldObject.Value = new Object(787, 1);
							farm2.objects[v].minutesUntilReady.Value = CalculateMinutesUntilMorning(Game1.timeOfDay);
							farm2.objects[v].shakeTimer = 1000;
							lightningEvent2.createBolt = true;
							lightningEvent2.boltPosition = v * 64f + new Vector2(32f, 0f);
							farm2.lightningStrikeEvent.Fire(lightningEvent2);
							return;
						}
					}
				}
				if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
				{
					try
					{
						KeyValuePair<Vector2, TerrainFeature> c = farm2.terrainFeatures.Pairs.ElementAt(random.Next(farm2.terrainFeatures.Count()));
						if (!(c.Value is FruitTree))
						{
							bool num = c.Value is HoeDirt && (c.Value as HoeDirt).crop != null && !(c.Value as HoeDirt).crop.dead;
							if (c.Value.performToolAction(null, 50, c.Key, farm2))
							{
								lightningEvent2.destroyedTerrainFeature = true;
								lightningEvent2.createBolt = true;
								farm2.terrainFeatures.Remove(c.Key);
								lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, -128f);
							}
							if (num && c.Value is HoeDirt && (c.Value as HoeDirt).crop != null && (bool)(c.Value as HoeDirt).crop.dead)
							{
								lightningEvent2.createBolt = true;
								lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, 0f);
							}
						}
						else if (c.Value is FruitTree)
						{
							(c.Value as FruitTree).struckByLightningCountdown.Value = 4;
							(c.Value as FruitTree).shake(c.Key, doEvenIfStillShaking: true, farm2);
							lightningEvent2.createBolt = true;
							lightningEvent2.boltPosition = c.Key * 64f + new Vector2(32f, -128f);
						}
					}
					catch (Exception)
					{
					}
				}
				farm2.lightningStrikeEvent.Fire(lightningEvent2);
			}
			else if (random.NextDouble() < 0.1)
			{
				Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
				lightningEvent.smallFlash = true;
				Farm farm2 = Game1.getLocationFromName("Farm") as Farm;
				farm2.lightningStrikeEvent.Fire(lightningEvent);
			}
		}

		public static void overnightLightning()
		{
			if (Game1.IsMasterGame)
			{
				int numberOfLoops = (2300 - Game1.timeOfDay) / 100;
				for (int i = 1; i <= numberOfLoops; i++)
				{
					performLightningUpdate(Game1.timeOfDay + i * 100);
				}
			}
		}

		public static List<Vector2> getAdjacentTileLocations(Vector2 tileLocation)
		{
			return new List<Vector2>
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation
			};
		}

		public static List<Point> getAdjacentTilePoints(float xTile, float yTile)
		{
			List<Point> list = new List<Point>();
			int x = (int)xTile;
			int y = (int)yTile;
			list.Add(new Point(-1 + x, y));
			list.Add(new Point(1 + x, y));
			list.Add(new Point(x, 1 + y));
			list.Add(new Point(x, -1 + y));
			return list;
		}

		public static Vector2[] getAdjacentTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[4]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation
			};
		}

		public static Vector2[] getDiagonalTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[4]
			{
				new Vector2(-1f, -1f) + tileLocation,
				new Vector2(1f, 1f) + tileLocation,
				new Vector2(-1f, 1f) + tileLocation,
				new Vector2(1f, -1f) + tileLocation
			};
		}

		public static Vector2[] getSurroundingTileLocationsArray(Vector2 tileLocation)
		{
			return new Vector2[8]
			{
				new Vector2(-1f, 0f) + tileLocation,
				new Vector2(1f, 0f) + tileLocation,
				new Vector2(0f, 1f) + tileLocation,
				new Vector2(0f, -1f) + tileLocation,
				new Vector2(-1f, -1f) + tileLocation,
				new Vector2(1f, -1f) + tileLocation,
				new Vector2(1f, 1f) + tileLocation,
				new Vector2(-1f, 1f) + tileLocation
			};
		}

		public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation)
		{
			return findCloseFlower(location, startTileLocation, -1, null);
		}

		public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation, int range = -1, Func<Crop, bool> additional_check = null)
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			openList.Enqueue(startTileLocation);
			for (int attempts = 0; range >= 0 || (range < 0 && attempts <= 150); attempts++)
			{
				if (openList.Count <= 0)
				{
					break;
				}
				Vector2 currentTile = openList.Dequeue();
				if (location.terrainFeatures.ContainsKey(currentTile) && location.terrainFeatures[currentTile] is HoeDirt && (location.terrainFeatures[currentTile] as HoeDirt).crop != null && new Object((location.terrainFeatures[currentTile] as HoeDirt).crop.indexOfHarvest.Value, 1).Category == -80 && (int)(location.terrainFeatures[currentTile] as HoeDirt).crop.currentPhase >= (location.terrainFeatures[currentTile] as HoeDirt).crop.phaseDays.Count - 1 && !(location.terrainFeatures[currentTile] as HoeDirt).crop.dead && (additional_check == null || additional_check((location.terrainFeatures[currentTile] as HoeDirt).crop)))
				{
					return (location.terrainFeatures[currentTile] as HoeDirt).crop;
				}
				foreach (Vector2 v in getAdjacentTileLocations(currentTile))
				{
					if (!closedList.Contains(v) && (range < 0 || Math.Abs(v.X - startTileLocation.X) + Math.Abs(v.Y - startTileLocation.Y) <= (float)range))
					{
						openList.Enqueue(v);
					}
				}
				closedList.Add(currentTile);
			}
			return null;
		}

		public static Point findCloseMatureCrop(Vector2 startTileLocation)
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			Farm f = Game1.getLocationFromName("Farm") as Farm;
			openList.Enqueue(startTileLocation);
			for (int attempts = 0; attempts <= 40; attempts++)
			{
				if (openList.Count() <= 0)
				{
					break;
				}
				Vector2 currentTile = openList.Dequeue();
				if (f.terrainFeatures.ContainsKey(currentTile) && f.terrainFeatures[currentTile] is HoeDirt && (f.terrainFeatures[currentTile] as HoeDirt).crop != null && (f.terrainFeatures[currentTile] as HoeDirt).readyForHarvest())
				{
					return Vector2ToPoint(currentTile);
				}
				foreach (Vector2 v in getAdjacentTileLocations(currentTile))
				{
					if (!closedList.Contains(v))
					{
						openList.Enqueue(v);
					}
				}
				closedList.Add(currentTile);
			}
			return Point.Zero;
		}

		public static void recursiveFenceBuild(Vector2 position, int direction, GameLocation location, Random r)
		{
			if (!(r.NextDouble() < 0.04) && !location.objects.ContainsKey(position) && location.isTileLocationOpen(new Location((int)position.X, (int)position.Y)))
			{
				location.objects.Add(position, new Fence(position, 1, isGate: false));
				int directionToBuild = direction;
				if (r.NextDouble() < 0.16)
				{
					directionToBuild = r.Next(4);
				}
				if (directionToBuild == (direction + 2) % 4)
				{
					directionToBuild = (directionToBuild + 1) % 4;
				}
				switch (direction)
				{
				case 0:
					recursiveFenceBuild(position + new Vector2(0f, -1f), directionToBuild, location, r);
					break;
				case 1:
					recursiveFenceBuild(position + new Vector2(1f, 0f), directionToBuild, location, r);
					break;
				case 3:
					recursiveFenceBuild(position + new Vector2(-1f, 0f), directionToBuild, location, r);
					break;
				case 2:
					recursiveFenceBuild(position + new Vector2(0f, 1f), directionToBuild, location, r);
					break;
				}
			}
		}

		public static bool addAnimalToFarm(FarmAnimal animal)
		{
			if (animal == null || animal.Sprite == null)
			{
				return false;
			}
			foreach (Building b in ((Farm)Game1.getLocationFromName("Farm")).buildings)
			{
				if (b.buildingType.Contains(animal.buildingTypeILiveIn) && !((AnimalHouse)(GameLocation)b.indoors).isFull())
				{
					((AnimalHouse)(GameLocation)b.indoors).animals.Add(animal.myID, animal);
					((AnimalHouse)(GameLocation)b.indoors).animalsThatLiveHere.Add(animal.myID);
					animal.home = b;
					animal.setRandomPosition(b.indoors);
					return true;
				}
			}
			return false;
		}

		public static Item getItemFromStandardTextDescription(string description, Farmer who, char delimiter = ' ')
		{
			string[] array = description.Split(delimiter);
			string type = array[0];
			int index = Convert.ToInt32(array[1]);
			int stock = Convert.ToInt32(array[2]);
			Item item = null;
			switch (type)
			{
			case "Furniture":
			case "F":
				item = Furniture.GetFurnitureInstance(index, Vector2.Zero);
				break;
			case "Object":
			case "O":
				item = new Object(index, 1);
				break;
			case "BigObject":
			case "BO":
				item = new Object(Vector2.Zero, index);
				break;
			case "Ring":
			case "R":
				item = new Ring(index);
				break;
			case "Boot":
			case "B":
				item = new Boots(index);
				break;
			case "Weapon":
			case "W":
				item = new MeleeWeapon(index);
				break;
			case "Blueprint":
			case "BL":
				item = new Object(index, 1, isRecipe: true);
				break;
			case "Hat":
			case "H":
				item = new Hat(index);
				break;
			case "BigBlueprint":
			case "BBl":
			case "BBL":
				item = new Object(Vector2.Zero, index, isRecipe: true);
				break;
			case "C":
				item = new Clothing(index);
				break;
			}
			item.Stack = stock;
			if (who != null && item is Object && (bool)(item as Object).isRecipe && who.knowsRecipe(item.Name))
			{
				return null;
			}
			return item;
		}

		public static string getStandardDescriptionFromItem(Item item, int stack, char delimiter = ' ')
		{
			string identifier = "";
			int index = item.parentSheetIndex.Value;
			Boots boots;
			MeleeWeapon weapon;
			Hat hat;
			if (item is Furniture)
			{
				identifier = "F";
			}
			else if (item is Object)
			{
				Object obj = item as Object;
				identifier = (obj.bigCraftable.Value ? ((!obj.IsRecipe) ? "BO" : "BBL") : ((!obj.IsRecipe) ? "O" : "BL"));
			}
			else if (item is Ring)
			{
				identifier = "R";
			}
			else if ((boots = (item as Boots)) != null)
			{
				identifier = "B";
				index = boots.indexInTileSheet.Value;
			}
			else if ((weapon = (item as MeleeWeapon)) != null)
			{
				identifier = "W";
				index = weapon.CurrentParentTileIndex;
			}
			else if ((hat = (item as Hat)) != null)
			{
				identifier = "H";
				index = hat.which.Value;
			}
			else if (item is Clothing)
			{
				identifier = "C";
			}
			return identifier + delimiter.ToString() + index + delimiter.ToString() + stack;
		}

		public static List<TemporaryAnimatedSprite> sparkleWithinArea(Microsoft.Xna.Framework.Rectangle bounds, int numberOfSparkles, Color sparkleColor, int delayBetweenSparkles = 100, int delayBeforeStarting = 0, string sparkleSound = "")
		{
			return getTemporarySpritesWithinArea(new int[2]
			{
				10,
				11
			}, bounds, numberOfSparkles, sparkleColor, delayBetweenSparkles, delayBeforeStarting, sparkleSound);
		}

		public static List<TemporaryAnimatedSprite> getTemporarySpritesWithinArea(int[] temporarySpriteRowNumbers, Microsoft.Xna.Framework.Rectangle bounds, int numberOfsprites, Color color, int delayBetweenSprites = 100, int delayBeforeStarting = 0, string sound = "")
		{
			List<TemporaryAnimatedSprite> sparkles = new List<TemporaryAnimatedSprite>();
			for (int i = 0; i < numberOfsprites; i++)
			{
				sparkles.Add(new TemporaryAnimatedSprite(temporarySpriteRowNumbers[Game1.random.Next(temporarySpriteRowNumbers.Length)], new Vector2(Game1.random.Next(bounds.X, bounds.Right), Game1.random.Next(bounds.Y, bounds.Bottom)), color)
				{
					delayBeforeAnimationStart = delayBeforeStarting + delayBetweenSprites * i,
					startSound = ((sound.Length > 0) ? sound : null)
				});
			}
			return sparkles;
		}

		public static Vector2 getAwayFromPlayerTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Farmer who)
		{
			Vector2 offset = new Vector2(-(who.GetBoundingBox().Center.X - monsterBox.Center.X), who.GetBoundingBox().Center.Y - monsterBox.Center.Y);
			if (offset.Length() <= 0f)
			{
				if (who.FacingDirection == 3)
				{
					offset = new Vector2(-1f, 0f);
				}
				else if (who.FacingDirection == 1)
				{
					offset = new Vector2(1f, 0f);
				}
				else if (who.FacingDirection == 0)
				{
					offset = new Vector2(0f, 1f);
				}
				else if (who.FacingDirection == 2)
				{
					offset = new Vector2(0f, -1f);
				}
			}
			offset.Normalize();
			offset.X *= 50 + Game1.random.Next(-20, 20);
			offset.Y *= 50 + Game1.random.Next(-20, 20);
			return offset;
		}

		public static string getSongTitleFromCueName(string cueName)
		{
			switch (cueName.ToLower())
			{
			case "turn_off":
				return Game1.content.LoadString("Strings\\UI:Mini_JukeBox_Off");
			case "spring1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5718");
			case "spring2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5720");
			case "spring3":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5722");
			case "50s":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5724");
			case "ragtime":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_1");
			case "abigailflute":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5726");
			case "abigailfluteduet":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5728");
			case "aerobics":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5730");
			case "winter1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5732");
			case "winter2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5734");
			case "winter3":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5736");
			case "near the planet core":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5738");
			case "summer1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5740");
			case "summer2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5742");
			case "summer3":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5744");
			case "fall1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5746");
			case "fall2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5748");
			case "fall3":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5750");
			case "breezy":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5752");
			case "christmasTheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5754");
			case "cloth":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5756");
			case "cloudcountry":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5758");
			case "of dwarves":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5760");
			case "grandpas_theme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5762");
			case "flowerdance":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5764");
			case "fallfest":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5766");
			case "icicles":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5768");
			case "xor":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5770");
			case "wizardsong":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5772");
			case "woodstheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5774");
			case "wedding":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5776");
			case "tribal":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5778");
			case "tinymusicbox":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5780");
			case "ticktock":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5782");
			case "starshoot":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5784");
			case "springtown":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5786");
			case "spirits_eve":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5788");
			case "spacemusic":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5790");
			case "shimmeringbastion":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5792");
			case "heavy":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5794");
			case "settlingin":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5796");
			case "secret gnomes":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5798");
			case "sampractice":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5800");
			case "saloon1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5802");
			case "sadpiano":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5804");
			case "poppy":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5806");
			case "playful":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5808");
			case "overcast":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5810");
			case "moonlightjellies":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5812");
			case "marnieshop":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5814");
			case "marlonstheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5816");
			case "title_day":
			case "maintheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5819");
			case "title_night":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5821");
			case "librarytheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5823");
			case "junimostarsong":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5825");
			case "kindadumbautumn":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5827");
			case "jaunty":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5829");
			case "honkytonky":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5831");
			case "gusviolin":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5833");
			case "event1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5835");
			case "sweet":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_2");
			case "event2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5837");
			case "elliottpiano":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5839");
			case "echos":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5841");
			case "distantbanjo":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5843");
			case "desolate":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5845");
			case "crystal bells":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5847");
			case "cowboy_overworld":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5849");
			case "cowboy_outlawsong":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5851");
			case "cowboy_undead":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5853");
			case "cowboy_boss":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5855");
			case "cowboy_singing":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5857");
			case "cavern":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5859");
			case "wavy":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5861");
			case "jojaofficesoundscape":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5863");
			case "shanetheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_3");
			case "emilytheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_4");
			case "emilydance":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_5");
			case "emilydream":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_6");
			case "christmastheme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_7");
			case "musicboxsong":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_8");
			case "buglevelloop":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5864_9");
			case "night_market":
				return Game1.content.LoadString("Strings\\UI:Billboard_NightMarket");
			case "submarine_song":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Dialogue.cs.721");
			case "mermaidsong":
				return Game1.content.LoadString("strings\\StringsFromCSFiles:Dialogue.cs.718");
			case "sam_acoustic1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "sam_acoustic2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "harveys_theme_jazz":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "movie_classic":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "movie_nature":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "movie_wumbus":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "movietheater":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "movietheaterafter":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "crane_game":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "junimokart":
			case "junimokart_mushroommusic":
			case "junimokart_slimemusic":
			case "junimokart_whalemusic":
			case "junimokart_ghostmusic":
			case "crane_game_fast":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:" + cueName.ToLower());
			case "sunroom":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:SunRoom");
			case "random":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:JukeboxRandomTrack");
			case "islandmusic":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandMusic");
			case "fieldofficetentmusic":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:fieldOfficeTentMusic");
			case "volcanomines1":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:VolcanoMines1");
			case "volcanomines2":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:VolcanoMines2");
			case "caldera":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:caldera");
			case "frogcave":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:FrogCave");
			case "sad_kid":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:sad_kid");
			case "pirate_theme":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PIRATE_THEME");
			case "pirate_theme(muffled)":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PIRATE_THEME_MUFFLED");
			case "end_credits":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:EndCredits_SongName");
			default:
				return cueName;
			}
		}

		public static bool isOffScreenEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			if (!isOnScreen(new Vector2(currentNode.x * 64, currentNode.y * 64), 32))
			{
				return true;
			}
			return false;
		}

		public static Vector2 getAwayFromPositionTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Vector2 position)
		{
			float num = 0f - (position.X - (float)monsterBox.Center.X);
			float ySlope2 = position.Y - (float)monsterBox.Center.Y;
			float total = Math.Abs(num) + Math.Abs(ySlope2);
			if (total < 1f)
			{
				total = 5f;
			}
			float x = num / total * 20f;
			ySlope2 = ySlope2 / total * 20f;
			return new Vector2(x, ySlope2);
		}

		public static bool tileWithinRadiusOfPlayer(int xTile, int yTile, int tileRadius, Farmer f)
		{
			Point point = new Point(xTile, yTile);
			Vector2 playerTile = f.getTileLocation();
			if (Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius)
			{
				return Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
			}
			return false;
		}

		public static bool withinRadiusOfPlayer(int x, int y, int tileRadius, Farmer f)
		{
			Point point = new Point(x / 64, y / 64);
			Vector2 playerTile = f.getTileLocation();
			if (Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius)
			{
				return Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
			}
			return false;
		}

		public static bool isThereAnObjectHereWhichAcceptsThisItem(GameLocation location, Item item, int x, int y)
		{
			if (item is Tool)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2(x / 64, y / 64);
			if (location is BuildableGameLocation)
			{
				foreach (Building building in (location as BuildableGameLocation).buildings)
				{
					if (building.performActiveObjectDropInAction(Game1.player, probe: true))
					{
						return true;
					}
				}
			}
			if (location.Objects.ContainsKey(tileLocation) && location.objects[tileLocation].heldObject.Value == null)
			{
				location.objects[tileLocation].performObjectDropInAction((Object)item, probe: true, Game1.player);
				bool result = location.objects[tileLocation].heldObject.Value != null;
				location.objects[tileLocation].heldObject.Value = null;
				return result;
			}
			return false;
		}

		public static bool buyWallpaper()
		{
			if (Game1.player.Money >= Game1.wallpaperPrice)
			{
				Game1.updateWallpaperInFarmHouse(Game1.currentWallpaper);
				Game1.farmerWallpaper = Game1.currentWallpaper;
				Game1.player.Money -= Game1.wallpaperPrice;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5865"), Color.Green, 3500f));
				return true;
			}
			return false;
		}

		public static FarmAnimal getAnimal(long id)
		{
			if (Game1.getFarm().animals.ContainsKey(id))
			{
				return Game1.getFarm().animals[id];
			}
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.indoors.Value is AnimalHouse && (b.indoors.Value as AnimalHouse).animals.ContainsKey(id))
				{
					return (b.indoors.Value as AnimalHouse).animals[id];
				}
			}
			return null;
		}

		public static bool buyFloor()
		{
			if (Game1.player.Money >= Game1.floorPrice)
			{
				Game1.FarmerFloor = Game1.currentFloor;
				Game1.updateFloorInFarmHouse(Game1.currentFloor);
				Game1.player.Money -= Game1.floorPrice;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5868"), Color.Green, 3500f));
				return true;
			}
			return false;
		}

		public static int numSilos()
		{
			int num = 0;
			foreach (Building b in (Game1.getLocationFromName("Farm") as Farm).buildings)
			{
				if (b.buildingType.Equals("Silo") && (int)b.daysOfConstructionLeft <= 0)
				{
					num++;
				}
			}
			return num;
		}

		public static Dictionary<ISalable, int[]> getCarpenterStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			AddStock(stock, new Object(Vector2.Zero, 388, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 390, int.MaxValue));
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			AddStock(stock, new Furniture(1614, Vector2.Zero));
			AddStock(stock, new Furniture(1616, Vector2.Zero));
			switch (Game1.dayOfMonth % 7)
			{
			case 1:
				AddStock(stock, new Furniture(0, Vector2.Zero));
				AddStock(stock, new Furniture(192, Vector2.Zero));
				AddStock(stock, new Furniture(704, Vector2.Zero));
				AddStock(stock, new Furniture(1120, Vector2.Zero));
				AddStock(stock, new Furniture(1216, Vector2.Zero));
				AddStock(stock, new Furniture(1391, Vector2.Zero));
				break;
			case 2:
				AddStock(stock, new Furniture(3, Vector2.Zero));
				AddStock(stock, new Furniture(197, Vector2.Zero));
				AddStock(stock, new Furniture(709, Vector2.Zero));
				AddStock(stock, new Furniture(1122, Vector2.Zero));
				AddStock(stock, new Furniture(1218, Vector2.Zero));
				AddStock(stock, new Furniture(1393, Vector2.Zero));
				break;
			case 3:
				AddStock(stock, new Furniture(6, Vector2.Zero));
				AddStock(stock, new Furniture(202, Vector2.Zero));
				AddStock(stock, new Furniture(714, Vector2.Zero));
				AddStock(stock, new Furniture(1124, Vector2.Zero));
				AddStock(stock, new Furniture(1220, Vector2.Zero));
				AddStock(stock, new Furniture(1395, Vector2.Zero));
				break;
			case 4:
				AddStock(stock, getRandomFurniture(r, stock, 1296, 1391));
				AddStock(stock, getRandomFurniture(r, stock, 1296, 1391));
				break;
			case 5:
				AddStock(stock, getRandomFurniture(r, stock, 1443, 1450));
				AddStock(stock, getRandomFurniture(r, stock, 288, 313));
				break;
			case 6:
				AddStock(stock, getRandomFurniture(r, stock, 1565, 1607));
				AddStock(stock, getRandomFurniture(r, stock, 12, 129));
				break;
			case 0:
				AddStock(stock, getRandomFurniture(r, stock, 1296, 1391));
				AddStock(stock, getRandomFurniture(r, stock, 416, 537));
				break;
			}
			AddStock(stock, getRandomFurniture(r, stock));
			AddStock(stock, getRandomFurniture(r, stock));
			while (r.NextDouble() < 0.25)
			{
				AddStock(stock, getRandomFurniture(r, stock, 1673, 1815));
			}
			AddStock(stock, new Furniture(1402, Vector2.Zero));
			AddStock(stock, new Object(Vector2.Zero, 208)
			{
				Stack = int.MaxValue
			});
			if (Game1.currentSeason == "winter" || Game1.year >= 2)
			{
				AddStock(stock, new Object(Vector2.Zero, 211)
				{
					Stack = int.MaxValue
				});
			}
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(stock, new Object(Vector2.Zero, 216));
			}
			AddStock(stock, new Object(Vector2.Zero, 214));
			AddStock(stock, new TV(1466, Vector2.Zero));
			AddStock(stock, new TV(1680, Vector2.Zero));
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(stock, new TV(1468, Vector2.Zero));
			}
			if (getHomeOfFarmer(Game1.player).upgradeLevel > 0)
			{
				AddStock(stock, new Furniture(1226, Vector2.Zero));
			}
			AddStock(stock, new Object(Vector2.Zero, 200)
			{
				Stack = int.MaxValue
			});
			AddStock(stock, new Object(Vector2.Zero, 35)
			{
				Stack = int.MaxValue
			});
			AddStock(stock, new Object(Vector2.Zero, 46)
			{
				Stack = int.MaxValue
			});
			AddStock(stock, new Furniture(1792, Vector2.Zero));
			AddStock(stock, new Furniture(1794, Vector2.Zero));
			AddStock(stock, new Furniture(1798, Vector2.Zero));
			if (Game1.player.eventsSeen.Contains(1053978))
			{
				AddStock(stock, new BedFurniture(2186, Vector2.Zero));
			}
			AddStock(stock, new BedFurniture(2048, Vector2.Zero), 250);
			if (Game1.player.HouseUpgradeLevel > 0)
			{
				AddStock(stock, new BedFurniture(2052, Vector2.Zero), 1000);
			}
			if (Game1.player.HouseUpgradeLevel > 1)
			{
				AddStock(stock, new BedFurniture(2076, Vector2.Zero), 1000);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wooden Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 143, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Stone Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 144, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Barrel Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 150, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Stump Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 147, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Gold Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 145, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Carved Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 148, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Skull Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 149, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			else if (!Game1.player.craftingRecipes.ContainsKey("Marble Brazier"))
			{
				AddStock(stock, new Torch(Vector2.Zero, 151, bigCraftable: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wood Lamp-post"))
			{
				AddStock(stock, new Object(Vector2.Zero, 152, isRecipe: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Iron Lamp-post"))
			{
				AddStock(stock, new Object(Vector2.Zero, 153, isRecipe: true)
				{
					IsRecipe = true
				});
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Wood Floor"))
			{
				AddStock(stock, new Object(328, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Rustic Plank Floor"))
			{
				AddStock(stock, new Object(840, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stone Floor"))
			{
				AddStock(stock, new Object(329, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Brick Floor"))
			{
				AddStock(stock, new Object(293, 1, isRecipe: true), 250);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stone Walkway Floor"))
			{
				AddStock(stock, new Object(841, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Stepping Stone Path"))
			{
				AddStock(stock, new Object(415, 1, isRecipe: true), 50);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Straw Floor"))
			{
				AddStock(stock, new Object(401, 1, isRecipe: true), 100);
			}
			if (!Game1.player.craftingRecipes.ContainsKey("Crystal Path"))
			{
				AddStock(stock, new Object(409, 1, isRecipe: true), 100);
			}
			return stock;
		}

		private static bool isFurnitureOffLimitsForSale(int index)
		{
			switch (index)
			{
			case 131:
			case 134:
			case 984:
			case 985:
			case 986:
			case 989:
			case 1226:
			case 1298:
			case 1299:
			case 1300:
			case 1301:
			case 1302:
			case 1303:
			case 1304:
			case 1305:
			case 1306:
			case 1307:
			case 1308:
			case 1309:
			case 1371:
			case 1373:
			case 1375:
			case 1402:
			case 1466:
			case 1468:
			case 1471:
			case 1541:
			case 1545:
			case 1554:
			case 1669:
			case 1671:
			case 1680:
			case 1687:
			case 1692:
			case 1733:
			case 1760:
			case 1761:
			case 1762:
			case 1763:
			case 1764:
			case 1796:
			case 1798:
			case 1800:
			case 1802:
			case 1838:
			case 1840:
			case 1842:
			case 1844:
			case 1846:
			case 1848:
			case 1850:
			case 1852:
			case 1854:
			case 1900:
			case 1902:
			case 1907:
			case 1909:
			case 1914:
			case 1915:
			case 1916:
			case 1917:
			case 1918:
			case 1952:
			case 1953:
			case 1954:
			case 1955:
			case 1956:
			case 1957:
			case 1958:
			case 1959:
			case 1960:
			case 1961:
			case 1971:
			case 2186:
			case 2326:
			case 2329:
			case 2331:
			case 2332:
			case 2334:
			case 2393:
			case 2396:
			case 2400:
			case 2418:
			case 2419:
			case 2421:
			case 2423:
			case 2425:
			case 2426:
			case 2428:
			case 2496:
			case 2502:
			case 2508:
			case 2514:
			case 2624:
			case 2625:
			case 2626:
			case 2653:
			case 2732:
			case 2814:
				return true;
			default:
				return false;
			}
		}

		private static Furniture getRandomFurniture(Random r, Dictionary<ISalable, int[]> stock, int lowerIndexBound = 0, int upperIndexBound = 1462)
		{
			int index2 = -1;
			Dictionary<int, string> furnitureData = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			do
			{
				index2 = r.Next(lowerIndexBound, upperIndexBound);
				if (stock != null)
				{
					foreach (Item i in stock.Keys)
					{
						if (i is Furniture && (int)i.parentSheetIndex == index2)
						{
							index2 = -1;
						}
					}
				}
			}
			while (isFurnitureOffLimitsForSale(index2) || !furnitureData.ContainsKey(index2));
			Furniture furniture = new Furniture(index2, Vector2.Zero);
			furniture.stack.Value = int.MaxValue;
			return furniture;
		}

		public static Dictionary<ISalable, int[]> GetQiChallengeRewardStock(Farmer who)
		{
			Dictionary<ISalable, int[]> reward_stock = new Dictionary<ISalable, int[]>();
			Item i17 = null;
			int stock_count = int.MaxValue;
			i17 = new Object(Vector2.Zero, 256);
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				i17.Stack = 2;
				stock_count = 1;
			}
			reward_stock.Add(i17, new int[4]
			{
				0,
				stock_count,
				858,
				15 * i17.Stack
			});
			i17 = new Object(911, 1);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				50
			});
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
			{
				i17 = new Object(897, 1);
				(i17 as Object).questItem.Value = true;
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					50
				});
			}
			i17 = new Object(Vector2.Zero, 275);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				10
			});
			i17 = new Object(913, 4);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				20
			});
			i17 = new Object(915, 4);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				20
			});
			i17 = new Object(Vector2.Zero, 265);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				20
			});
			if (!who.HasTownKey)
			{
				PurchaseableKeyItem key = new PurchaseableKeyItem(Game1.content.LoadString("Strings\\StringsFromCSFiles:KeyToTheTown"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Key To The Town_desc"), 912, delegate(Farmer farmer)
				{
					farmer.HasTownKey = true;
				});
				reward_stock.Add(key, new int[4]
				{
					0,
					1,
					858,
					20
				});
			}
			i17 = new Object(896, 1);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				40
			});
			i17 = new Object(891, 1);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				5
			});
			i17 = new Object(908, 20);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				5
			});
			i17 = new Object(917, 10);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				10
			});
			i17 = new Hat(82);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				5
			});
			i17 = new FishTankFurniture(2400, Vector2.Zero);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				20
			});
			if (!who.craftingRecipes.ContainsKey("Heavy Tapper"))
			{
				i17 = new Object(Vector2.Zero, 264, isRecipe: true);
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					20
				});
			}
			if (!who.craftingRecipes.ContainsKey("Hyper Speed-Gro"))
			{
				i17 = new Object(918, 1, isRecipe: true);
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					30
				});
			}
			if (!who.craftingRecipes.ContainsKey("Deluxe Fertilizer"))
			{
				i17 = new Object(919, 1, isRecipe: true);
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					20
				});
			}
			if (!who.craftingRecipes.ContainsKey("Hopper"))
			{
				i17 = new Object(Vector2.Zero, 275, isRecipe: true);
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					50
				});
			}
			if (!who.craftingRecipes.ContainsKey("Magic Bait"))
			{
				i17 = new Object(908, 1, isRecipe: true);
				reward_stock.Add(i17, new int[4]
				{
					0,
					1,
					858,
					20
				});
			}
			if ((int)Game1.netWorldState.Value.GoldenWalnuts > 0 && Game1.player.hasOrWillReceiveMail("Island_FirstParrot") && Game1.player.hasOrWillReceiveMail("Island_Turtle") && Game1.player.hasOrWillReceiveMail("Island_UpgradeBridge") && Game1.player.hasOrWillReceiveMail("Island_UpgradeHouse") && Game1.player.hasOrWillReceiveMail("Island_UpgradeParrotPlatform") && Game1.player.hasOrWillReceiveMail("Island_Resort") && Game1.player.hasOrWillReceiveMail("Island_UpgradeTrader") && Game1.player.hasOrWillReceiveMail("Island_W_Obelisk") && Game1.player.hasOrWillReceiveMail("Island_UpgradeHouse_Mailbox") && Game1.player.hasOrWillReceiveMail("Island_VolcanoBridge") && Game1.player.hasOrWillReceiveMail("Island_VolcanoShortcutOut"))
			{
				i17 = new Object(858, 2);
				reward_stock.Add(i17, new int[4]
				{
					0,
					2147483647,
					73,
					1
				});
			}
			i17 = new BedFurniture(2514, Vector2.Zero);
			reward_stock.Add(i17, new int[4]
			{
				0,
				2147483647,
				858,
				50
			});
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				i17 = new Object(928, 1);
				reward_stock.Add(i17, new int[4]
				{
					0,
					2147483647,
					858,
					100
				});
			}
			return reward_stock;
		}

		public static Dictionary<ISalable, int[]> getAdventureRecoveryStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			foreach (Item i in Game1.player.itemsLostLastDeath)
			{
				if (i != null)
				{
					i.isLostItem = true;
					stock.Add(i, new int[2]
					{
						getSellToStorePriceOfItem(i),
						i.Stack
					});
				}
			}
			return stock;
		}

		public static Dictionary<ISalable, int[]> getAdventureShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			int infiniteStock = int.MaxValue;
			stock.Add(new MeleeWeapon(12), new int[2]
			{
				250,
				infiniteStock
			});
			if (MineShaft.lowestLevelReached >= 15)
			{
				stock.Add(new MeleeWeapon(17), new int[2]
				{
					500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 20)
			{
				stock.Add(new MeleeWeapon(1), new int[2]
				{
					750,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 25)
			{
				stock.Add(new MeleeWeapon(43), new int[2]
				{
					850,
					infiniteStock
				});
				stock.Add(new MeleeWeapon(44), new int[2]
				{
					1500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				stock.Add(new MeleeWeapon(27), new int[2]
				{
					2000,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 45)
			{
				stock.Add(new MeleeWeapon(10), new int[2]
				{
					2000,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 55)
			{
				stock.Add(new MeleeWeapon(7), new int[2]
				{
					4000,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 75)
			{
				stock.Add(new MeleeWeapon(5), new int[2]
				{
					6000,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 90)
			{
				stock.Add(new MeleeWeapon(50), new int[2]
				{
					9000,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 120)
			{
				stock.Add(new MeleeWeapon(9), new int[2]
				{
					25000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("galaxySword"))
			{
				stock.Add(new MeleeWeapon(4), new int[2]
				{
					50000,
					infiniteStock
				});
				stock.Add(new MeleeWeapon(23), new int[2]
				{
					35000,
					infiniteStock
				});
				stock.Add(new MeleeWeapon(29), new int[2]
				{
					75000,
					infiniteStock
				});
			}
			stock.Add(new Boots(504), new int[2]
			{
				500,
				infiniteStock
			});
			if (MineShaft.lowestLevelReached >= 10)
			{
				stock.Add(new Boots(506), new int[2]
				{
					500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 50)
			{
				stock.Add(new Boots(509), new int[2]
				{
					750,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 40)
			{
				stock.Add(new Boots(508), new int[2]
				{
					1250,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				stock.Add(new Boots(512), new int[2]
				{
					2000,
					infiniteStock
				});
				stock.Add(new Boots(511), new int[2]
				{
					2500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 110)
			{
				stock.Add(new Boots(514), new int[2]
				{
					5000,
					infiniteStock
				});
			}
			stock.Add(new Ring(529), new int[2]
			{
				1000,
				infiniteStock
			});
			stock.Add(new Ring(530), new int[2]
			{
				1000,
				infiniteStock
			});
			if (MineShaft.lowestLevelReached >= 40)
			{
				stock.Add(new Ring(531), new int[2]
				{
					2500,
					infiniteStock
				});
				stock.Add(new Ring(532), new int[2]
				{
					2500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 80)
			{
				stock.Add(new Ring(533), new int[2]
				{
					5000,
					infiniteStock
				});
				stock.Add(new Ring(534), new int[2]
				{
					5000,
					infiniteStock
				});
			}
			_ = MineShaft.lowestLevelReached;
			_ = 120;
			if (MineShaft.lowestLevelReached >= 40)
			{
				stock.Add(new Slingshot(32), new int[2]
				{
					500,
					infiniteStock
				});
			}
			if (MineShaft.lowestLevelReached >= 70)
			{
				stock.Add(new Slingshot(33), new int[2]
				{
					1000,
					infiniteStock
				});
			}
			if (Game1.player.craftingRecipes.ContainsKey("Explosive Ammo"))
			{
				stock.Add(new Object(441, int.MaxValue), new int[2]
				{
					300,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				stock.Add(new Ring(520), new int[2]
				{
					25000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				stock.Add(new Ring(523), new int[2]
				{
					25000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				stock.Add(new Ring(526), new int[2]
				{
					20000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				stock.Add(new Ring(522), new int[2]
				{
					15000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				stock.Add(new Ring(810), new int[2]
				{
					15000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				stock.Add(new Ring(811), new int[2]
				{
					30000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				stock.Add(new Hat(8), new int[2]
				{
					20000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				stock.Add(new Hat(27), new int[2]
				{
					20000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				stock.Add(new Hat(60), new int[2]
				{
					20000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				stock.Add(new Hat(50), new int[2]
				{
					20000,
					infiniteStock
				});
			}
			if (Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				stock.Add(new MeleeWeapon(13), new int[2]
				{
					10000,
					infiniteStock
				});
			}
			return stock;
		}

		public static void AddStock(Dictionary<ISalable, int[]> stock, Item obj, int buyPrice = -1, int limitedQuantity = -1)
		{
			int price = 2 * buyPrice;
			if (buyPrice == -1)
			{
				price = obj.salePrice();
			}
			int stack = int.MaxValue;
			if (obj is Object && (obj as Object).IsRecipe)
			{
				stack = 1;
			}
			else if (limitedQuantity != -1)
			{
				stack = limitedQuantity;
			}
			stock.Add(obj, new int[2]
			{
				price,
				stack
			});
		}

		public static Dictionary<ISalable, int[]> getSaloonStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			AddStock(stock, new Object(Vector2.Zero, 346, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 196, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 216, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 224, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 206, int.MaxValue));
			AddStock(stock, new Object(Vector2.Zero, 395, int.MaxValue));
			if (!Game1.player.cookingRecipes.ContainsKey("Hashbrowns"))
			{
				AddStock(stock, new Object(210, 1, isRecipe: true), 25);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Omelet"))
			{
				AddStock(stock, new Object(195, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Pancakes"))
			{
				AddStock(stock, new Object(211, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Bread"))
			{
				AddStock(stock, new Object(216, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Tortilla"))
			{
				AddStock(stock, new Object(229, 1, isRecipe: true), 50);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Pizza"))
			{
				AddStock(stock, new Object(206, 1, isRecipe: true), 75);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Maki Roll"))
			{
				AddStock(stock, new Object(228, 1, isRecipe: true), 150);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Cookies") && Game1.player.eventsSeen.Contains(19))
			{
				AddStock(stock, new Object(223, 1, isRecipe: true)
				{
					name = "Cookies"
				}, 150);
			}
			if (!Game1.player.cookingRecipes.ContainsKey("Triple Shot Espresso"))
			{
				AddStock(stock, new Object(253, 1, isRecipe: true), 2500);
			}
			if ((int)Game1.dishOfTheDay.stack > 0 && !getForbiddenDishesOfTheDay().Contains(Game1.dishOfTheDay.ParentSheetIndex))
			{
				AddStock(stock, Game1.dishOfTheDay.getOne() as Object, Game1.dishOfTheDay.Price, Game1.dishOfTheDay.stack);
			}
			Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(SynchronizedShopStock.SynchedShop.Saloon, stock);
			if (Game1.player.activeDialogueEvents.ContainsKey("willyCrabs"))
			{
				AddStock(stock, new Object(Vector2.Zero, 732, int.MaxValue));
			}
			return stock;
		}

		public static int[] getForbiddenDishesOfTheDay()
		{
			return new int[7]
			{
				346,
				196,
				216,
				224,
				206,
				395,
				217
			};
		}

		public static bool removeLightSource(int identifier)
		{
			bool removed = false;
			for (int i = Game1.currentLightSources.Count - 1; i >= 0; i--)
			{
				if ((int)Game1.currentLightSources.ElementAt(i).identifier == identifier)
				{
					Game1.currentLightSources.Remove(Game1.currentLightSources.ElementAt(i));
					removed = true;
				}
			}
			return removed;
		}

		public static Horse findHorseForPlayer(long uid)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					Horse horse = character as Horse;
					if (horse != null && (long)horse.ownerId == uid)
					{
						return horse;
					}
				}
			}
			return null;
		}

		public static Horse findHorse(Guid horseId)
		{
			foreach (GameLocation location in Game1.locations)
			{
				foreach (NPC character in location.characters)
				{
					Horse horse = character as Horse;
					if (horse != null && horse.HorseId == horseId)
					{
						return horse;
					}
				}
			}
			return null;
		}

		public static void addDirtPuffs(GameLocation location, int tileX, int tileY, int tilesWide, int tilesHigh, int number = 5)
		{
			for (int x = tileX; x < tileX + tilesWide; x++)
			{
				for (int y = tileY; y < tileY + tilesHigh; y++)
				{
					for (int i = 0; i < number; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 46 : 12, new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5)
						{
							delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
							motion = new Vector2(0f, -1f),
							interval = Game1.random.Next(50, 80)
						});
					}
					location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextDouble() < 0.5));
				}
			}
		}

		public static void addSmokePuff(GameLocation l, Vector2 v, int delay = 0, float baseScale = 2f, float scaleChange = 0.02f, float alpha = 0.75f, float alphaFade = 0.002f)
		{
			l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, alphaFade, Color.Gray)
			{
				alpha = alpha,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = baseScale,
				scaleChange = scaleChange,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
				delayBeforeAnimationStart = delay
			});
		}

		public static LightSource getLightSource(int identifier)
		{
			foreach (LightSource i in Game1.currentLightSources)
			{
				if ((int)i.identifier == identifier)
				{
					return i;
				}
			}
			return null;
		}

		public static Dictionary<ISalable, int[]> getAllWallpapersAndFloorsForFree()
		{
			Dictionary<ISalable, int[]> decors = new Dictionary<ISalable, int[]>();
			for (int j = 0; j < 112; j++)
			{
				decors.Add(new Wallpaper(j)
				{
					Stack = int.MaxValue
				}, new int[2]
				{
					0,
					2147483647
				});
			}
			for (int i = 0; i < 56; i++)
			{
				decors.Add(new Wallpaper(i, isFloor: true)
				{
					Stack = int.MaxValue
				}, new int[2]
				{
					0,
					2147483647
				});
			}
			return decors;
		}

		public static Dictionary<ISalable, int[]> getAllFurnituresForFree()
		{
			Dictionary<ISalable, int[]> decors = new Dictionary<ISalable, int[]>();
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture");
			List<Furniture> all_furnitures = new List<Furniture>();
			foreach (KeyValuePair<int, string> v in dictionary)
			{
				if (!isFurnitureOffLimitsForSale(v.Key))
				{
					all_furnitures.Add(Furniture.GetFurnitureInstance(v.Key));
				}
			}
			all_furnitures.Sort(SortAllFurnitures);
			foreach (Furniture f in all_furnitures)
			{
				decors.Add(f, new int[2]
				{
					0,
					2147483647
				});
			}
			decors.Add(new Furniture(1402, Vector2.Zero), new int[2]
			{
				0,
				2147483647
			});
			decors.Add(new TV(1680, Vector2.Zero), new int[2]
			{
				0,
				2147483647
			});
			decors.Add(new TV(1466, Vector2.Zero), new int[2]
			{
				0,
				2147483647
			});
			decors.Add(new TV(1468, Vector2.Zero), new int[2]
			{
				0,
				2147483647
			});
			return decors;
		}

		public static int SortAllFurnitures(Furniture a, Furniture b)
		{
			if (a.furniture_type != b.furniture_type)
			{
				return a.furniture_type.Value.CompareTo(b.furniture_type.Value);
			}
			if ((int)a.furniture_type == 12 && (int)b.furniture_type == 12)
			{
				bool num = a.Name.StartsWith("Floor Divider ");
				bool b_is_floor_divider = b.Name.StartsWith("Floor Divider ");
				if (num != b_is_floor_divider)
				{
					if (b_is_floor_divider)
					{
						return -1;
					}
					return 1;
				}
			}
			return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
		}

		public static bool doesAnyFarmerHaveOrWillReceiveMail(string id)
		{
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.hasOrWillReceiveMail(id))
				{
					return true;
				}
			}
			return false;
		}

		public static string loadStringShort(string fileWithinStringsFolder, string key)
		{
			return Game1.content.LoadString("Strings\\" + fileWithinStringsFolder + ":" + key);
		}

		public static string loadStringDataShort(string fileWithinStringsFolder, string key)
		{
			return Game1.content.LoadString("Data\\" + fileWithinStringsFolder + ":" + key);
		}

		public static bool doesAnyFarmerHaveMail(string id)
		{
			if (Game1.player.mailReceived.Contains(id))
			{
				return true;
			}
			foreach (Farmer value in Game1.otherFarmers.Values)
			{
				if (value.mailReceived.Contains(id))
				{
					return true;
				}
			}
			return false;
		}

		public static FarmEvent pickFarmEvent()
		{
			return Game1.hooks.OnUtility_PickFarmEvent(delegate
			{
				Random random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				if (Game1.weddingToday)
				{
					return null;
				}
				foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
				{
					Friendship spouseFriendship = onlineFarmer.GetSpouseFriendship();
					if (spouseFriendship != null && spouseFriendship.IsMarried() && spouseFriendship.WeddingDate == Game1.Date)
					{
						return null;
					}
				}
				if (Game1.stats.DaysPlayed == 31)
				{
					return new SoundInTheNightEvent(4);
				}
				if (Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved%&NL&%") || Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved"))
				{
					return new WorldChangeEvent(14);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
				{
					return new WorldChangeEvent(0);
				}
				if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
				{
					return new WorldChangeEvent(1);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
				{
					return new WorldChangeEvent(6);
				}
				if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
				{
					return new WorldChangeEvent(7);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
				{
					return new WorldChangeEvent(2);
				}
				if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
				{
					return new WorldChangeEvent(3);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
				{
					return new WorldChangeEvent(4);
				}
				if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
				{
					return new WorldChangeEvent(5);
				}
				if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
				{
					return new WorldChangeEvent(8);
				}
				if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
				{
					return new WorldChangeEvent(9);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
				{
					return new WorldChangeEvent(10);
				}
				if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(11);
				}
				if (Game1.MasterPlayer.eventsSeen.Contains(191393) && (Game1.isRaining || Game1.isLightning) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater"))
				{
					return new WorldChangeEvent(12);
				}
				if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor") && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
				{
					return new WorldChangeEvent(13);
				}
				if (random.NextDouble() < 0.01 && !Game1.currentSeason.Equals("winter"))
				{
					return new FairyEvent();
				}
				if (random.NextDouble() < 0.01)
				{
					return new WitchEvent();
				}
				if (random.NextDouble() < 0.01)
				{
					return new SoundInTheNightEvent(1);
				}
				if (random.NextDouble() < 0.008 && Game1.year > 1)
				{
					return new SoundInTheNightEvent(0);
				}
				return (random.NextDouble() < 0.008) ? new SoundInTheNightEvent(3) : null;
			});
		}

		public static FarmEvent pickPersonalFarmEvent()
		{
			Random r = new Random(((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2) ^ (470124797 + (int)Game1.player.UniqueMultiplayerID));
			if (Game1.weddingToday)
			{
				return null;
			}
			if (Game1.player.isMarried() && Game1.player.GetSpouseFriendship().DaysUntilBirthing <= 0 && Game1.player.GetSpouseFriendship().NextBirthingDate != null)
			{
				if (Game1.player.spouse != null)
				{
					return new BirthingEvent();
				}
				long spouseID2 = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				if (Game1.otherFarmers.ContainsKey(spouseID2))
				{
					return new PlayerCoupleBirthingEvent();
				}
			}
			else
			{
				if (Game1.player.isMarried() && Game1.player.spouse != null && Game1.getCharacterFromName(Game1.player.spouse).canGetPregnant() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation) && r.NextDouble() < 0.05)
				{
					return new QuestionEvent(1);
				}
				if (Game1.player.isMarried() && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.GetSpouseFriendship().NextBirthingDate == null && r.NextDouble() < 0.05)
				{
					long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
					if (Game1.otherFarmers.ContainsKey(spouseID))
					{
						Farmer spouse = Game1.otherFarmers[spouseID];
						if (spouse.currentLocation == Game1.player.currentLocation && (spouse.currentLocation == Game1.getLocationFromName(spouse.homeLocation) || spouse.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation)) && playersCanGetPregnantHere(spouse.currentLocation as FarmHouse))
						{
							return new QuestionEvent(3);
						}
					}
				}
			}
			if (r.NextDouble() < 0.5)
			{
				return new QuestionEvent(2);
			}
			return new SoundInTheNightEvent(2);
		}

		private static bool playersCanGetPregnantHere(FarmHouse farmHouse)
		{
			List<Child> kids = farmHouse.getChildren();
			if (farmHouse.cribStyle.Value <= 0)
			{
				return false;
			}
			if (farmHouse.getChildrenCount() < 2 && farmHouse.upgradeLevel >= 2 && kids.Count < 2)
			{
				if (kids.Count != 0)
				{
					return kids[0].Age > 2;
				}
				return true;
			}
			return false;
		}

		public static string capitalizeFirstLetter(string s)
		{
			if (s == null || s.Length < 1)
			{
				return "";
			}
			return s[0].ToString().ToUpper() + ((s.Length > 1) ? s.Substring(1) : "");
		}

		public static Dictionary<ISalable, int[]> getBlacksmithStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(Vector2.Zero, 378, int.MaxValue), new int[2]
			{
				(Game1.year > 1) ? 150 : 75,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 380, int.MaxValue), new int[2]
			{
				(Game1.year > 1) ? 250 : 150,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 382, int.MaxValue), new int[2]
			{
				(Game1.year > 1) ? 250 : 150,
				2147483647
			});
			stock.Add(new Object(Vector2.Zero, 384, int.MaxValue), new int[2]
			{
				(Game1.year > 1) ? 750 : 400,
				2147483647
			});
			return stock;
		}

		public static bool alreadyHasLightSourceWithThisID(int identifier)
		{
			foreach (LightSource currentLightSource in Game1.currentLightSources)
			{
				if ((int)currentLightSource.identifier == identifier)
				{
					return true;
				}
			}
			return false;
		}

		public static void repositionLightSource(int identifier, Vector2 position)
		{
			foreach (LightSource i in Game1.currentLightSources)
			{
				if ((int)i.identifier == identifier)
				{
					i.position.Value = position;
				}
			}
		}

		public static Dictionary<ISalable, int[]> getAnimalShopStock()
		{
			Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
			stock.Add(new Object(178, 1), new int[2]
			{
				50,
				2147483647
			});
			Object heater = new Object(Vector2.Zero, 104);
			heater.price.Value = 2000;
			heater.Stack = 1;
			stock.Add(heater, new int[2]
			{
				2000,
				2147483647
			});
			if (Game1.player.hasItemWithNameThatContains("Milk Pail") == null)
			{
				stock.Add(new MilkPail(), new int[2]
				{
					1000,
					1
				});
			}
			if (Game1.player.hasItemWithNameThatContains("Shears") == null)
			{
				stock.Add(new Shears(), new int[2]
				{
					1000,
					1
				});
			}
			if ((int)Game1.player.farmingLevel >= 10)
			{
				stock.Add(new Object(Vector2.Zero, 165), new int[2]
				{
					25000,
					2147483647
				});
			}
			stock.Add(new Object(Vector2.Zero, 45), new int[2]
			{
				250,
				2147483647
			});
			if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
			{
				stock.Add(new Object(928, 1), new int[2]
				{
					100000,
					2147483647
				});
			}
			return stock;
		}

		public static bool areThereAnyOtherAnimalsWithThisName(string name)
		{
			Farm f = Game1.getLocationFromName("Farm") as Farm;
			foreach (Building b in f.buildings)
			{
				if (b.indoors.Value is AnimalHouse)
				{
					foreach (FarmAnimal a2 in (b.indoors.Value as AnimalHouse).animals.Values)
					{
						if (a2.displayName != null && a2.displayName.Equals(name))
						{
							return true;
						}
					}
				}
			}
			foreach (FarmAnimal a in f.animals.Values)
			{
				if (a.displayName != null && a.displayName.Equals(name))
				{
					return true;
				}
			}
			return false;
		}

		public static string getNumberWithCommas(int number)
		{
			StringBuilder s = new StringBuilder(string.Concat(number));
			string comma = ",";
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
			{
				comma = ".";
			}
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru)
			{
				comma = " ";
			}
			for (int i = s.Length - 4; i >= 0; i -= 3)
			{
				s.Insert(i + 1, comma);
			}
			return s.ToString();
		}

		public static List<Object> getPurchaseAnimalStock()
		{
			List<Object> list = new List<Object>();
			Object o7 = new Object(100, 1, isRecipe: false, 400)
			{
				Name = "Chicken",
				Type = ((Game1.getFarm().isBuildingConstructed("Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop") || Game1.getFarm().isBuildingConstructed("Big Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5926")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 750)
			{
				Name = "Dairy Cow",
				Type = ((Game1.getFarm().isBuildingConstructed("Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn") || Game1.getFarm().isBuildingConstructed("Big Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5931")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 2000)
			{
				Name = "Goat",
				Type = ((Game1.getFarm().isBuildingConstructed("Big Barn") || Game1.getFarm().isBuildingConstructed("Deluxe Barn")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5936")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 600)
			{
				Name = "Duck",
				Type = ((Game1.getFarm().isBuildingConstructed("Big Coop") || Game1.getFarm().isBuildingConstructed("Deluxe Coop")) ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5940")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 4000)
			{
				Name = "Sheep",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Barn") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5944")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 4000)
			{
				Name = "Rabbit",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Coop") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5947")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945")
			};
			list.Add(o7);
			o7 = new Object(100, 1, isRecipe: false, 8000)
			{
				Name = "Pig",
				Type = (Game1.getFarm().isBuildingConstructed("Deluxe Barn") ? null : Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5950")),
				displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948")
			};
			list.Add(o7);
			return list;
		}

		public static void FixChildNameCollisions()
		{
			List<NPC> all_characters = new List<NPC>();
			getAllCharacters(all_characters);
			bool collision_found2 = false;
			Dictionary<string, string> dispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			foreach (NPC character in all_characters)
			{
				if (character is Child)
				{
					string old_character_name = character.Name;
					string character_name = character.Name;
					do
					{
						collision_found2 = false;
						if (dispositions.ContainsKey(character_name))
						{
							character_name += " ";
							collision_found2 = true;
						}
						else
						{
							foreach (NPC i in all_characters)
							{
								if (i != character && i.name.Equals(character_name))
								{
									character_name += " ";
									collision_found2 = true;
								}
							}
						}
					}
					while (collision_found2);
					if (character_name != character.Name)
					{
						character.Name = character_name;
						character.displayName = null;
						_ = character.displayName;
						foreach (Farmer farmer in Game1.getAllFarmers())
						{
							if (farmer.friendshipData != null && farmer.friendshipData.ContainsKey(old_character_name))
							{
								farmer.friendshipData[character_name] = farmer.friendshipData[old_character_name];
								farmer.friendshipData.Remove(old_character_name);
							}
						}
					}
				}
			}
		}

		public static List<Item> getShopStock(bool Pierres)
		{
			List<Item> stock = new List<Item>();
			if (Pierres)
			{
				if (Game1.currentSeason.Equals("spring"))
				{
					stock.Add(new Object(Vector2.Zero, 472, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 473, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 474, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 475, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 427, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 429, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 477, int.MaxValue));
					stock.Add(new Object(628, int.MaxValue, isRecipe: false, 1700));
					stock.Add(new Object(629, int.MaxValue, isRecipe: false, 1000));
					if (Game1.year > 1)
					{
						stock.Add(new Object(Vector2.Zero, 476, int.MaxValue));
					}
				}
				if (Game1.currentSeason.Equals("summer"))
				{
					stock.Add(new Object(Vector2.Zero, 480, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 482, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 483, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 484, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 479, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 302, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 453, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 455, int.MaxValue));
					stock.Add(new Object(630, int.MaxValue, isRecipe: false, 2000));
					stock.Add(new Object(631, int.MaxValue, isRecipe: false, 3000));
					if (Game1.year > 1)
					{
						stock.Add(new Object(Vector2.Zero, 485, int.MaxValue));
					}
				}
				if (Game1.currentSeason.Equals("fall"))
				{
					stock.Add(new Object(Vector2.Zero, 487, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 488, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 490, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 299, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 301, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 492, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 491, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 493, int.MaxValue));
					stock.Add(new Object(431, int.MaxValue, isRecipe: false, 100));
					stock.Add(new Object(Vector2.Zero, 425, int.MaxValue));
					stock.Add(new Object(632, int.MaxValue, isRecipe: false, 3000));
					stock.Add(new Object(633, int.MaxValue, isRecipe: false, 2000));
					if (Game1.year > 1)
					{
						stock.Add(new Object(Vector2.Zero, 489, int.MaxValue));
					}
				}
				stock.Add(new Object(Vector2.Zero, 297, int.MaxValue));
				stock.Add(new Object(Vector2.Zero, 245, int.MaxValue));
				stock.Add(new Object(Vector2.Zero, 246, int.MaxValue));
				stock.Add(new Object(Vector2.Zero, 423, int.MaxValue));
				Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
				stock.Add(new Wallpaper(r.Next(112))
				{
					Stack = int.MaxValue
				});
				stock.Add(new Wallpaper(r.Next(40), isFloor: true)
				{
					Stack = int.MaxValue
				});
				stock.Add(new Clothing(1000 + r.Next(128))
				{
					Stack = int.MaxValue,
					Price = 1000
				});
				if (Game1.player.achievements.Contains(38))
				{
					stock.Add(new Object(Vector2.Zero, 458, int.MaxValue));
				}
			}
			else
			{
				if (Game1.currentSeason.Equals("spring"))
				{
					stock.Add(new Object(Vector2.Zero, 478, int.MaxValue));
				}
				if (Game1.currentSeason.Equals("summer"))
				{
					stock.Add(new Object(Vector2.Zero, 486, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 481, int.MaxValue));
				}
				if (Game1.currentSeason.Equals("fall"))
				{
					stock.Add(new Object(Vector2.Zero, 493, int.MaxValue));
					stock.Add(new Object(Vector2.Zero, 494, int.MaxValue));
				}
				stock.Add(new Object(Vector2.Zero, 88, int.MaxValue));
				stock.Add(new Object(Vector2.Zero, 90, int.MaxValue));
			}
			return stock;
		}

		public static Vector2 getCornersOfThisRectangle(ref Microsoft.Xna.Framework.Rectangle r, int corner)
		{
			switch (corner)
			{
			default:
				return new Vector2(r.X, r.Y);
			case 1:
				return new Vector2(r.Right - 1, r.Y);
			case 2:
				return new Vector2(r.Right - 1, r.Bottom - 1);
			case 3:
				return new Vector2(r.X, r.Bottom - 1);
			}
		}

		private static int priceForToolUpgradeLevel(int level)
		{
			switch (level)
			{
			case 1:
				return 2000;
			case 2:
				return 5000;
			case 3:
				return 10000;
			case 4:
				return 25000;
			default:
				return 2000;
			}
		}

		private static int indexOfExtraMaterialForToolUpgrade(int level)
		{
			switch (level)
			{
			case 1:
				return 334;
			case 2:
				return 335;
			case 3:
				return 336;
			case 4:
				return 337;
			default:
				return 334;
			}
		}

		public static Dictionary<ISalable, int[]> getBlacksmithUpgradeStock(Farmer who)
		{
			Dictionary<ISalable, int[]> toolStock = new Dictionary<ISalable, int[]>();
			Tool axe = who.getToolFromName("Axe");
			Tool wateringCan = who.getToolFromName("Watering Can");
			Tool pickAxe = who.getToolFromName("Pickaxe");
			Tool hoe = who.getToolFromName("Hoe");
			if (axe != null && (int)axe.upgradeLevel < 4)
			{
				Tool shopAxe4 = new Axe();
				shopAxe4.UpgradeLevel = (int)axe.upgradeLevel + 1;
				toolStock.Add(shopAxe4, new int[3]
				{
					priceForToolUpgradeLevel(shopAxe4.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(shopAxe4.upgradeLevel)
				});
			}
			if (wateringCan != null && (int)wateringCan.upgradeLevel < 4)
			{
				Tool shopAxe3 = new WateringCan();
				shopAxe3.UpgradeLevel = (int)wateringCan.upgradeLevel + 1;
				toolStock.Add(shopAxe3, new int[3]
				{
					priceForToolUpgradeLevel(shopAxe3.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(shopAxe3.upgradeLevel)
				});
			}
			if (pickAxe != null && (int)pickAxe.upgradeLevel < 4)
			{
				Tool shopAxe2 = new Pickaxe();
				shopAxe2.UpgradeLevel = (int)pickAxe.upgradeLevel + 1;
				toolStock.Add(shopAxe2, new int[3]
				{
					priceForToolUpgradeLevel(shopAxe2.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(shopAxe2.upgradeLevel)
				});
			}
			if (hoe != null && (int)hoe.upgradeLevel < 4)
			{
				Tool shopAxe = new Hoe();
				shopAxe.UpgradeLevel = (int)hoe.upgradeLevel + 1;
				toolStock.Add(shopAxe, new int[3]
				{
					priceForToolUpgradeLevel(shopAxe.UpgradeLevel),
					1,
					indexOfExtraMaterialForToolUpgrade(shopAxe.upgradeLevel)
				});
			}
			if (who.trashCanLevel < 4)
			{
				string toolNameString = "";
				switch (who.trashCanLevel)
				{
				case 0:
					toolNameString = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14299", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 1:
					toolNameString = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14300", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 2:
					toolNameString = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14301", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				case 3:
					toolNameString = Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14302", Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan"));
					break;
				}
				Tool trashCan = new GenericTool(toolNameString, Game1.content.LoadString("Strings\\StringsFromCSFiles:TrashCan_Description", string.Concat((who.trashCanLevel + 1) * 15)), who.trashCanLevel + 1, 13 + who.trashCanLevel, 13 + who.trashCanLevel);
				toolStock.Add(trashCan, new int[3]
				{
					priceForToolUpgradeLevel(who.trashCanLevel + 1) / 2,
					1,
					indexOfExtraMaterialForToolUpgrade(who.trashCanLevel + 1)
				});
			}
			return toolStock;
		}

		public static Vector2 GetCurvePoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
		{
			float cx = 3f * (p1.X - p0.X);
			float cy = 3f * (p1.Y - p0.Y);
			float bx = 3f * (p2.X - p1.X) - cx;
			float by = 3f * (p2.Y - p1.Y) - cy;
			float num = p3.X - p0.X - cx - bx;
			float ay = p3.Y - p0.Y - cy - by;
			float Cube = t * t * t;
			float Square = t * t;
			float x = num * Cube + bx * Square + cx * t + p0.X;
			float resY = ay * Cube + by * Square + cy * t + p0.Y;
			return new Vector2(x, resY);
		}

		public static GameLocation getGameLocationOfCharacter(NPC n)
		{
			return n.currentLocation;
		}

		public static int[] parseStringToIntArray(string s, char delimiter = ' ')
		{
			string[] split = s.Split(delimiter);
			int[] result = new int[split.Length];
			for (int i = 0; i < split.Length; i++)
			{
				result[i] = Convert.ToInt32(split[i]);
			}
			return result;
		}

		public static void drawLineWithScreenCoordinates(int x1, int y1, int x2, int y2, SpriteBatch b, Color color1, float layerDepth = 1f)
		{
			Vector2 value = new Vector2(x2, y2);
			Vector2 start = new Vector2(x1, y1);
			Vector2 edge = value - start;
			float angle = (float)Math.Atan2(edge.Y, edge.X);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), 1), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
			b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y + 1, (int)edge.Length(), 1), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
		}

		public static string getRandomNonLoopingSong()
		{
			switch (Game1.random.Next(7))
			{
			case 0:
				return "springsongs";
			case 1:
				return "summersongs";
			case 2:
				return "fallsongs";
			case 3:
				return "wintersongs";
			case 4:
				return "EarthMine";
			case 5:
				return "FrostMine";
			case 6:
				return "LavaMine";
			default:
				return "fallsongs";
			}
		}

		public static Farmer isThereAFarmerWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation location)
		{
			foreach (Farmer f in location.farmers)
			{
				if (Math.Abs(tileLocation.X - f.getTileLocation().X) <= (float)tilesAway && Math.Abs(tileLocation.Y - f.getTileLocation().Y) <= (float)tilesAway)
				{
					return f;
				}
			}
			return null;
		}

		public static Character isThereAFarmerOrCharacterWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation environment)
		{
			foreach (NPC c in environment.characters)
			{
				if (Vector2.Distance(c.getTileLocation(), tileLocation) <= (float)tilesAway)
				{
					return c;
				}
			}
			return isThereAFarmerWithinDistance(tileLocation, tilesAway, environment);
		}

		public static Color getRedToGreenLerpColor(float power)
		{
			return new Color((int)((power <= 0.5f) ? 255f : ((1f - power) * 2f * 255f)), (int)Math.Min(255f, power * 2f * 255f), 0);
		}

		public static FarmHouse getHomeOfFarmer(Farmer who)
		{
			return Game1.getLocationFromName(who.homeLocation) as FarmHouse;
		}

		public static Vector2 getRandomPositionOnScreen()
		{
			return new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
		}

		public static Vector2 getRandomPositionOnScreenNotOnMap()
		{
			Vector2 output = Vector2.Zero;
			int tries;
			for (tries = 0; tries < 30; tries++)
			{
				if (!output.Equals(Vector2.Zero) && !Game1.currentLocation.isTileOnMap((output + new Vector2(Game1.viewport.X, Game1.viewport.Y)) / 64f))
				{
					break;
				}
				output = getRandomPositionOnScreen();
			}
			if (tries >= 30)
			{
				return new Vector2(-1000f, -1000f);
			}
			return output;
		}

		public static Microsoft.Xna.Framework.Rectangle getRectangleCenteredAt(Vector2 v, int size)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)v.X - size / 2, (int)v.Y - size / 2, size, size);
		}

		public static bool checkForCharacterInteractionAtTile(Vector2 tileLocation, Farmer who)
		{
			NPC character = Game1.currentLocation.isCharacterAtTile(tileLocation);
			if (character != null && !character.IsMonster && !character.IsInvisible)
			{
				if (Game1.currentLocation is MovieTheater)
				{
					Game1.mouseCursor = 4;
				}
				else if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && !who.isRidingHorse() && character.isVillager() && ((who.friendshipData.ContainsKey(character.Name) && who.friendshipData[character.Name].GiftsToday != 1) || Game1.NPCGiftTastes.ContainsKey(character.Name)) && !Game1.eventUp)
				{
					Game1.mouseCursor = 3;
				}
				else if (character.canTalk() && ((character.CurrentDialogue != null && character.CurrentDialogue.Count > 0) || (Game1.player.spouse != null && character.Name != null && character.Name == Game1.player.spouse && character.shouldSayMarriageDialogue.Value && character.currentMarriageDialogue != null && character.currentMarriageDialogue.Count > 0) || character.hasTemporaryMessageAvailable() || (who.hasClubCard && character.Name.Equals("Bouncer") && who.IsLocalPlayer) || (character.Name.Equals("Henchman") && character.currentLocation.Name.Equals("WitchSwamp") && !who.hasOrWillReceiveMail("henchmanGone"))) && !character.isOnSilentTemporaryMessage())
				{
					Game1.mouseCursor = 4;
				}
				if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence)
				{
					Game1.mouseCursor = 0;
				}
				Game1.currentLocation.checkForSpecialCharacterIconAtThisTile(tileLocation);
				if (Game1.mouseCursor == 3 || Game1.mouseCursor == 4)
				{
					if (tileWithinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, who))
					{
						Game1.mouseCursorTransparency = 1f;
					}
					else
					{
						Game1.mouseCursorTransparency = 0.5f;
					}
				}
				return true;
			}
			return false;
		}

		public static bool canGrabSomethingFromHere(int x, int y, Farmer who)
		{
			if (Game1.currentLocation == null)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2(x / 64, y / 64);
			if (Game1.currentLocation.isObjectAt(x, y))
			{
				Game1.currentLocation.getObjectAt(x, y).hoverAction();
			}
			if (checkForCharacterInteractionAtTile(tileLocation, who))
			{
				return false;
			}
			if (checkForCharacterInteractionAtTile(tileLocation + new Vector2(0f, 1f), who))
			{
				return false;
			}
			if (who.IsLocalPlayer)
			{
				if (who.onBridge.Value)
				{
					return false;
				}
				if (Game1.currentLocation != null)
				{
					foreach (Furniture f in Game1.currentLocation.furniture)
					{
						if (f.getBoundingBox(f.TileLocation).Contains(Vector2ToPoint(tileLocation * 64f)) && f.Name.Contains("Table") && f.heldObject.Value != null)
						{
							return true;
						}
					}
				}
				if (Game1.currentLocation.Objects.ContainsKey(tileLocation))
				{
					if ((bool)Game1.currentLocation.Objects[tileLocation].readyForHarvest || (bool)Game1.currentLocation.Objects[tileLocation].isSpawnedObject || (Game1.currentLocation.Objects[tileLocation] is IndoorPot && (Game1.currentLocation.Objects[tileLocation] as IndoorPot).hoeDirt.Value.readyForHarvest()))
					{
						Game1.mouseCursor = 6;
						if (!withinRadiusOfPlayer(x, y, 1, who))
						{
							Game1.mouseCursorTransparency = 0.5f;
							return false;
						}
						return true;
					}
				}
				else if (Game1.currentLocation.terrainFeatures.ContainsKey(tileLocation) && Game1.currentLocation.terrainFeatures[tileLocation] is HoeDirt && ((HoeDirt)Game1.currentLocation.terrainFeatures[tileLocation]).readyForHarvest())
				{
					Game1.mouseCursor = 6;
					if (!withinRadiusOfPlayer(x, y, 1, who))
					{
						Game1.mouseCursorTransparency = 0.5f;
						return false;
					}
					return true;
				}
			}
			return false;
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectWithinRectangularRegion(int regionX, int regionY, int regionWidth, int sourceIndex, int sourceWidth, int sourceHeight)
		{
			int sourceRectWidthsOfRegion = regionWidth / sourceWidth;
			return new Microsoft.Xna.Framework.Rectangle(regionX + sourceIndex % sourceRectWidthsOfRegion * sourceWidth, regionY + sourceIndex / sourceRectWidthsOfRegion * sourceHeight, sourceWidth, sourceHeight);
		}

		public static void drawWithShadow(SpriteBatch b, Texture2D texture, Vector2 position, Microsoft.Xna.Framework.Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale = -1f, bool flipped = false, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 0.35f)
		{
			if (scale == -1f)
			{
				scale = 4f;
			}
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = -4;
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = 4;
			}
			b.Draw(texture, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), sourceRect, Color.Black * shadowIntensity, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth - 0.0001f);
			b.Draw(texture, position, sourceRect, color, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}

		public static void drawTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? 2 : 3);
			}
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), new Color(221, 148, 84) * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawTextWithColoredShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, Color shadowColor, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, int numShadows = 3)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
			if (horizontalShadowOffset == -1)
			{
				horizontalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? (-2) : (-3));
			}
			if (verticalShadowOffset == -1)
			{
				verticalShadowOffset = ((font.Equals(Game1.smallFont) | longWords) ? 2 : 3);
			}
			if (text == null)
			{
				text = "";
			}
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
			if (numShadows == 2)
			{
				b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			}
			if (numShadows == 3)
			{
				b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		public static void drawBoldText(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int boldnessOffset = 1)
		{
			if (layerDepth == -1f)
			{
				layerDepth = position.Y / 10000f;
			}
			b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(boldnessOffset, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(boldnessOffset, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			b.DrawString(font, text, position + new Vector2(0f, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		}

		protected static bool _HasNonMousePlacementLeeway(int x, int y, Item item, Farmer f)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return false;
			}
			Point start_point = f.getTileLocationPoint();
			if (!withinRadiusOfPlayer(x, y, 2, f))
			{
				return false;
			}
			if (item is Object && ((Object)item).Category == -74)
			{
				return true;
			}
			foreach (Point p in GetPointsOnLine(start_point.X, start_point.Y, x / 64, y / 64))
			{
				if (!(p == start_point) && !item.canBePlacedHere(f.currentLocation, new Vector2(p.X, p.Y)))
				{
					return false;
				}
			}
			return true;
		}

		public static bool isPlacementForbiddenHere(GameLocation location)
		{
			if (location == null)
			{
				return true;
			}
			return isPlacementForbiddenHere(location.name);
		}

		public static bool isPlacementForbiddenHere(string location_name)
		{
			if (location_name == "AbandonedJojaMart")
			{
				return true;
			}
			if (location_name == "BeachNightMarket")
			{
				return true;
			}
			return false;
		}

		public static void transferPlacedObjectsFromOneLocationToAnother(GameLocation source, GameLocation destination, Vector2? overflow_chest_position = null, GameLocation overflow_chest_location = null)
		{
			if (source == null)
			{
				return;
			}
			List<Item> invalid_objects = new List<Item>();
			foreach (Vector2 position in new List<Vector2>(source.objects.Keys))
			{
				if (source.objects[position] != null)
				{
					Object source_object = source.objects[position];
					bool valid = true;
					if (destination == null)
					{
						valid = false;
					}
					if (valid && destination.objects.ContainsKey(position))
					{
						valid = false;
					}
					if (valid && !destination.isTileLocationTotallyClearAndPlaceable(position))
					{
						valid = false;
					}
					source.objects.Remove(position);
					if (valid && destination != null)
					{
						destination.objects[position] = source_object;
					}
					else
					{
						invalid_objects.Add(source_object);
						if (source_object is Chest)
						{
							Chest obj = source_object as Chest;
							List<Item> chest_items = new List<Item>(obj.items);
							obj.items.Clear();
							foreach (Item chest_item in chest_items)
							{
								if (chest_item != null)
								{
									invalid_objects.Add(chest_item);
								}
							}
						}
					}
				}
			}
			if (overflow_chest_position.HasValue)
			{
				if (overflow_chest_location != null)
				{
					createOverflowChest(overflow_chest_location, overflow_chest_position.Value, invalid_objects);
				}
				else if (destination != null)
				{
					createOverflowChest(destination, overflow_chest_position.Value, invalid_objects);
				}
			}
		}

		public static void createOverflowChest(GameLocation destination, Vector2 overflow_chest_location, List<Item> overflow_items)
		{
			List<Chest> chests = new List<Chest>();
			foreach (Item overflow_object in overflow_items)
			{
				if (chests.Count == 0)
				{
					chests.Add(new Chest(playerChest: true));
				}
				bool found_chest_to_stash_in = false;
				foreach (Chest item in chests)
				{
					if (item.addItem(overflow_object) == null)
					{
						found_chest_to_stash_in = true;
					}
				}
				if (!found_chest_to_stash_in)
				{
					Chest new_chest = new Chest(playerChest: true);
					new_chest.addItem(overflow_object);
					chests.Add(new_chest);
				}
			}
			for (int i = 0; i < chests.Count; i++)
			{
				Chest chest = chests[i];
				_placeOverflowChestInNearbySpace(destination, overflow_chest_location, chest);
			}
		}

		protected static void _placeOverflowChestInNearbySpace(GameLocation location, Vector2 tileLocation, Object o)
		{
			if (o == null || tileLocation.Equals(Vector2.Zero))
			{
				return;
			}
			int attempts = 0;
			Queue<Vector2> open_list = new Queue<Vector2>();
			HashSet<Vector2> closed_list = new HashSet<Vector2>();
			open_list.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			for (; attempts < 100; attempts++)
			{
				current = open_list.Dequeue();
				if (!location.isTileOccupiedForPlacement(current) && location.isTileLocationTotallyClearAndPlaceable(current) && !location.isOpenWater((int)current.X, (int)current.Y))
				{
					break;
				}
				closed_list.Add(current);
				foreach (Vector2 v in getAdjacentTileLocations(current))
				{
					if (!closed_list.Contains(v))
					{
						open_list.Enqueue(v);
					}
				}
			}
			if (!current.Equals(Vector2.Zero) && !location.isTileOccupiedForPlacement(current) && !location.isOpenWater((int)current.X, (int)current.Y) && location.isTileLocationTotallyClearAndPlaceable(current))
			{
				o.tileLocation.Value = current;
				location.objects.Add(current, o);
			}
		}

		public static bool isWithinTileWithLeeway(int x, int y, Item item, Farmer f)
		{
			if (!withinRadiusOfPlayer(x, y, 1, f))
			{
				return _HasNonMousePlacementLeeway(x, y, item, f);
			}
			return true;
		}

		public static bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f)
		{
			if (isPlacementForbiddenHere(location))
			{
				return false;
			}
			if (item == null || item is Tool || Game1.eventUp || (bool)f.bathingClothes || f.onBridge.Value)
			{
				return false;
			}
			if (isWithinTileWithLeeway(x, y, item, f) || (item is Wallpaper && location is DecoratableLocation) || (item is Furniture && location.CanPlaceThisFurnitureHere(item as Furniture)))
			{
				if (item is Furniture)
				{
					Furniture furniture = item as Furniture;
					if (!location.CanFreePlaceFurniture() && !furniture.IsCloseEnoughToFarmer(f, x / 64, y / 64))
					{
						return false;
					}
				}
				Vector2 tileLocation = new Vector2(x / 64, y / 64);
				Object tile_object = location.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y);
				if (tile_object != null && tile_object is Fence && (tile_object as Fence).CanRepairWithThisItem(item))
				{
					return true;
				}
				if (item.canBePlacedHere(location, tileLocation))
				{
					if (item is Wallpaper)
					{
						return true;
					}
					if (!((Object)item).isPassable())
					{
						foreach (Farmer farmer in location.farmers)
						{
							if (farmer.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64)))
							{
								return false;
							}
						}
					}
					if (itemCanBePlaced(location, tileLocation, item) || isViableSeedSpot(location, tileLocation, item))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static int GetDoubleWideVersionOfBed(int bed_index)
		{
			return bed_index + 4;
		}

		private static bool itemCanBePlaced(GameLocation location, Vector2 tileLocation, Item item)
		{
			if (location.isTilePlaceable(tileLocation, item) && item.isPlaceable() && (item.Category != -74 || (item is Object && (item as Object).isSapling())))
			{
				if (!((Object)item).isPassable())
				{
					return !new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64).Intersects(Game1.player.GetBoundingBox());
				}
				return true;
			}
			return false;
		}

		public static bool isViableSeedSpot(GameLocation location, Vector2 tileLocation, Item item)
		{
			if (((Object)item).Category == -74)
			{
				if ((!location.terrainFeatures.ContainsKey(tileLocation) || !(location.terrainFeatures[tileLocation] is HoeDirt) || !((HoeDirt)location.terrainFeatures[tileLocation]).canPlantThisSeedHere((item as Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y)) && (!location.objects.ContainsKey(tileLocation) || !(location.objects[tileLocation] is IndoorPot) || !(location.objects[tileLocation] as IndoorPot).hoeDirt.Value.canPlantThisSeedHere((item as Object).ParentSheetIndex, (int)tileLocation.X, (int)tileLocation.Y) || (item as Object).ParentSheetIndex == 499))
				{
					if (location.isTileHoeDirt(tileLocation) || !location.terrainFeatures.ContainsKey(tileLocation))
					{
						return Object.isWildTreeSeed(item.parentSheetIndex);
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public static int getDirectionFromChange(Vector2 current, Vector2 previous, bool yBias = false)
		{
			if (!yBias && current.X > previous.X)
			{
				return 1;
			}
			if (!yBias && current.X < previous.X)
			{
				return 3;
			}
			if (current.Y > previous.Y)
			{
				return 2;
			}
			if (current.Y < previous.Y)
			{
				return 0;
			}
			if (current.X > previous.X)
			{
				return 1;
			}
			if (current.X < previous.X)
			{
				return 3;
			}
			return -1;
		}

		public static bool doesRectangleIntersectTile(Microsoft.Xna.Framework.Rectangle r, int tileX, int tileY)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
			return r.Intersects(tileRect);
		}

		public static List<NPC> getPooledList()
		{
			lock (_pool)
			{
				return _pool.Get();
			}
		}

		public static bool IsHospitalVisitDay(string character_name)
		{
			try
			{
				Dictionary<string, string> schedule = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + character_name);
				string day_key = Game1.currentSeason + "_" + Game1.dayOfMonth;
				if (schedule.ContainsKey(day_key) && schedule[day_key].Contains("Hospital"))
				{
					return true;
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static void returnPooledList(List<NPC> list)
		{
			lock (_pool)
			{
				_pool.Return(list);
			}
		}

		public static List<NPC> getAllCharacters(List<NPC> list)
		{
			list.AddRange(Game1.currentLocation.characters);
			foreach (GameLocation i in Game1.locations)
			{
				if (!i.Equals(Game1.currentLocation))
				{
					list.AddRange(i.characters);
				}
			}
			Farm f = Game1.getFarm();
			if (f != null)
			{
				foreach (Building b in f.buildings)
				{
					if (b.indoors.Value != null)
					{
						foreach (NPC character in b.indoors.Value.characters)
						{
							character.currentLocation = b.indoors;
						}
						list.AddRange(b.indoors.Value.characters);
					}
				}
				return list;
			}
			return list;
		}

		public static DisposableList<NPC> getAllCharacters()
		{
			List<NPC> list;
			lock (_pool)
			{
				list = _pool.Get();
			}
			getAllCharacters(list);
			return new DisposableList<NPC>(list, _pool);
		}

		private static void _recursiveIterateItem(Item i, Action<Item> action)
		{
			if (i == null)
			{
				return;
			}
			if (i is Object)
			{
				Object o = i as Object;
				if (o is StorageFurniture)
				{
					foreach (Item item2 in (o as StorageFurniture).heldItems)
					{
						if (item2 != null)
						{
							_recursiveIterateItem(item2, action);
						}
					}
				}
				if (o is Chest)
				{
					foreach (Item item in (o as Chest).items)
					{
						if (item != null)
						{
							_recursiveIterateItem(item, action);
						}
					}
				}
				if (o.heldObject.Value != null)
				{
					_recursiveIterateItem((Object)o.heldObject, action);
				}
			}
			action(i);
		}

		protected static void _recursiveIterateLocation(GameLocation l, Action<Item> action)
		{
			if (l != null)
			{
				if (l != null)
				{
					foreach (Furniture item5 in l.furniture)
					{
						_recursiveIterateItem(item5, action);
					}
				}
				if (l is IslandFarmHouse)
				{
					foreach (Item item4 in (l as IslandFarmHouse).fridge.Value.items)
					{
						if (item4 != null)
						{
							_recursiveIterateItem(item4, action);
						}
					}
				}
				if (l is FarmHouse)
				{
					foreach (Item item3 in (l as FarmHouse).fridge.Value.items)
					{
						if (item3 != null)
						{
							_recursiveIterateItem(item3, action);
						}
					}
				}
				foreach (NPC character in l.characters)
				{
					if (character is Child && (character as Child).hat.Value != null)
					{
						_recursiveIterateItem((character as Child).hat.Value, action);
					}
					if (character is Horse && (character as Horse).hat.Value != null)
					{
						_recursiveIterateItem((character as Horse).hat.Value, action);
					}
				}
				if (l is BuildableGameLocation)
				{
					foreach (Building b in (l as BuildableGameLocation).buildings)
					{
						if (b.indoors.Value != null)
						{
							_recursiveIterateLocation(b.indoors.Value, action);
						}
						if (b is Mill)
						{
							foreach (Item item2 in (b as Mill).output.Value.items)
							{
								if (item2 != null)
								{
									_recursiveIterateItem(item2, action);
								}
							}
						}
						else if (b is JunimoHut)
						{
							foreach (Item item in (b as JunimoHut).output.Value.items)
							{
								if (item != null)
								{
									_recursiveIterateItem(item, action);
								}
							}
						}
					}
				}
				foreach (Object value in l.objects.Values)
				{
					_recursiveIterateItem(value, action);
				}
				foreach (Debris d in l.debris)
				{
					if (d.item != null)
					{
						_recursiveIterateItem(d.item, action);
					}
				}
			}
		}

		public static Item PerformSpecialItemPlaceReplacement(Item placedItem)
		{
			if (placedItem != null && placedItem is Pan)
			{
				return new Hat(71);
			}
			if (placedItem != null && placedItem is Object && (int)(placedItem as Object).parentSheetIndex == 71)
			{
				return new Clothing(15);
			}
			return placedItem;
		}

		public static Item PerformSpecialItemGrabReplacement(Item heldItem)
		{
			if (heldItem != null && heldItem is Clothing && (int)(heldItem as Clothing).parentSheetIndex == 15)
			{
				heldItem = new Object(71, 1);
				Object obj = heldItem as Object;
				obj.questItem.Value = true;
				obj.questId.Value = 102;
			}
			if (heldItem != null && heldItem is Hat && (int)(heldItem as Hat).which == 71)
			{
				heldItem = new Pan();
			}
			return heldItem;
		}

		public static void iterateAllItemsHere(GameLocation location, Action<Item> action)
		{
			_recursiveIterateLocation(location, action);
		}

		public static void iterateAllItems(Action<Item> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				_recursiveIterateLocation(location, action);
			}
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				foreach (Item item4 in farmer.Items)
				{
					_recursiveIterateItem(item4, action);
				}
				_recursiveIterateItem(farmer.shirtItem.Value, action);
				_recursiveIterateItem(farmer.pantsItem.Value, action);
				_recursiveIterateItem(farmer.boots.Value, action);
				_recursiveIterateItem(farmer.hat.Value, action);
				_recursiveIterateItem(farmer.leftRing.Value, action);
				_recursiveIterateItem(farmer.rightRing.Value, action);
			}
			foreach (Item item3 in Game1.player.team.returnedDonations)
			{
				if (item3 != null)
				{
					action(item3);
				}
			}
			foreach (Item item2 in Game1.player.team.junimoChest)
			{
				if (item2 != null)
				{
					action(item2);
				}
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (Item item in specialOrder.donatedItems)
				{
					if (item != null)
					{
						action(item);
					}
				}
			}
		}

		public static void iterateChestsAndStorage(Action<Item> action)
		{
			foreach (GameLocation i in Game1.locations)
			{
				foreach (Object o2 in i.objects.Values)
				{
					if (o2 is Chest)
					{
						foreach (Item item12 in (o2 as Chest).items)
						{
							if (item12 != null)
							{
								action(item12);
							}
						}
					}
					if (o2.heldObject.Value != null && o2.heldObject.Value is Chest)
					{
						foreach (Item item11 in (o2.heldObject.Value as Chest).items)
						{
							if (item11 != null)
							{
								action(item11);
							}
						}
					}
				}
				if (i is FarmHouse)
				{
					foreach (Item item10 in (i as FarmHouse).fridge.Value.items)
					{
						if (item10 != null)
						{
							action(item10);
						}
					}
				}
				if (i != null)
				{
					foreach (Furniture furniture2 in i.furniture)
					{
						if (furniture2 is StorageFurniture)
						{
							foreach (Item item9 in (furniture2 as StorageFurniture).heldItems)
							{
								if (item9 != null)
								{
									action(item9);
								}
							}
						}
					}
				}
				if (i is BuildableGameLocation)
				{
					foreach (Building b in (i as BuildableGameLocation).buildings)
					{
						if (b.indoors.Value != null)
						{
							foreach (Object o in b.indoors.Value.objects.Values)
							{
								if (o is Chest)
								{
									foreach (Item item8 in (o as Chest).items)
									{
										if (item8 != null)
										{
											action(item8);
										}
									}
								}
								if (o.heldObject.Value != null && o.heldObject.Value is Chest)
								{
									foreach (Item item7 in (o.heldObject.Value as Chest).items)
									{
										if (item7 != null)
										{
											action(item7);
										}
									}
								}
							}
							if (b.indoors.Value != null)
							{
								foreach (Furniture furniture in b.indoors.Value.furniture)
								{
									if (furniture is StorageFurniture)
									{
										foreach (Item item6 in (furniture as StorageFurniture).heldItems)
										{
											if (item6 != null)
											{
												action(item6);
											}
										}
									}
								}
							}
						}
						else if (b is Mill)
						{
							foreach (Item item5 in (b as Mill).output.Value.items)
							{
								if (item5 != null)
								{
									action(item5);
								}
							}
						}
						else if (b is JunimoHut)
						{
							foreach (Item item4 in (b as JunimoHut).output.Value.items)
							{
								if (item4 != null)
								{
									action(item4);
								}
							}
						}
					}
				}
			}
			foreach (Item item3 in Game1.player.team.returnedDonations)
			{
				if (item3 != null)
				{
					action(item3);
				}
			}
			foreach (Item item2 in Game1.player.team.junimoChest)
			{
				if (item2 != null)
				{
					action(item2);
				}
			}
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				foreach (Item item in specialOrder.donatedItems)
				{
					if (item != null)
					{
						action(item);
					}
				}
			}
		}

		public static Item removeItemFromInventory(int whichItemIndex, IList<Item> items)
		{
			if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
			{
				Item tmp = items[whichItemIndex];
				if (whichItemIndex == Game1.player.CurrentToolIndex && items.Equals(Game1.player.items))
				{
					tmp?.actionWhenStopBeingHeld(Game1.player);
				}
				items[whichItemIndex] = null;
				return tmp;
			}
			return null;
		}

		public static void iterateAllCrops(Action<Crop> action)
		{
			foreach (GameLocation location in Game1.locations)
			{
				_recursiveIterateLocationCrops(location, action);
			}
		}

		protected static void _recursiveIterateLocationCrops(GameLocation l, Action<Crop> action)
		{
			if (l != null)
			{
				if (l is BuildableGameLocation)
				{
					foreach (Building b in (l as BuildableGameLocation).buildings)
					{
						if (b.indoors.Value != null)
						{
							_recursiveIterateLocationCrops(b.indoors.Value, action);
						}
					}
				}
				foreach (TerrainFeature feature in l.terrainFeatures.Values)
				{
					if (feature is HoeDirt && (feature as HoeDirt).crop != null)
					{
						action((feature as HoeDirt).crop);
					}
				}
				foreach (Object o in l.objects.Values)
				{
					if (o is IndoorPot && (o as IndoorPot).hoeDirt.Value != null && (o as IndoorPot).hoeDirt.Value.crop != null)
					{
						action((o as IndoorPot).hoeDirt.Value.crop);
					}
				}
			}
		}

		public static void checkItemFirstInventoryAdd(Item item)
		{
			if (!(item is Object) || item.HasBeenInInventory)
			{
				return;
			}
			if (!(item is Furniture) && !(item as Object).bigCraftable && !(item as Object).hasBeenPickedUpByFarmer)
			{
				Game1.player.checkForQuestComplete(null, (item as Object).parentSheetIndex, (item as Object).stack, item, null, 9);
			}
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.onItemCollected != null)
					{
						order.onItemCollected(Game1.player, item);
					}
				}
			}
			item.HasBeenInInventory = true;
			(item as Object).hasBeenPickedUpByFarmer.Value = true;
			if ((bool)(item as Object).questItem)
			{
				if (IsNormalObjectAtParentSheetIndex(item, 875) && !Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop") && Game1.player.team.SpecialOrderActive("Wizard"))
				{
					Game1.addMailForTomorrow("ectoplasmDrop", noLetter: true, sendToEveryone: true);
				}
				else if (IsNormalObjectAtParentSheetIndex(item, 876) && !Game1.MasterPlayer.hasOrWillReceiveMail("prismaticJellyDrop") && Game1.player.team.SpecialOrderActive("Wizard2"))
				{
					Game1.addMailForTomorrow("prismaticJellyDrop", noLetter: true, sendToEveryone: true);
				}
				if (IsNormalObjectAtParentSheetIndex(item, 897) && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMissingStocklist"))
				{
					Game1.addMailForTomorrow("gotMissingStocklist", noLetter: true, sendToEveryone: true);
				}
				return;
			}
			if (item is Object && (item as Object).bigCraftable.Value && item.ParentSheetIndex == 256 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotFirstJunimoChest"))
			{
				Game1.addMailForTomorrow("gotFirstJunimoChest", noLetter: true, sendToEveryone: true);
			}
			if (IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				switch ((int)(item as Object).parentSheetIndex)
				{
				case 535:
					if (Game1.activeClickableMenu == null && !Game1.player.hasOrWillReceiveMail("geodeFound"))
					{
						Game1.player.mailReceived.Add("geodeFound");
						Game1.player.holdUpItemThenMessage(item);
					}
					break;
				case 378:
					if (!Game1.player.hasOrWillReceiveMail("copperFound"))
					{
						Game1.addMailForTomorrow("copperFound", noLetter: true);
					}
					break;
				case 428:
					if (!Game1.player.hasOrWillReceiveMail("clothFound"))
					{
						Game1.addMailForTomorrow("clothFound", noLetter: true);
					}
					break;
				case 102:
					Game1.stats.NotesFound++;
					break;
				case 390:
					Game1.stats.StoneGathered++;
					if (Game1.stats.StoneGathered >= 100 && !Game1.player.hasOrWillReceiveMail("robinWell"))
					{
						Game1.addMailForTomorrow("robinWell");
					}
					break;
				case 74:
					Game1.stats.PrismaticShardsFound++;
					break;
				case 72:
					Game1.stats.DiamondsFound++;
					break;
				}
			}
			else if (item is Object && (item as Object).bigCraftable.Value)
			{
				int parentSheetIndex = (item as Object).ParentSheetIndex;
				if (parentSheetIndex == 248)
				{
					Game1.netWorldState.Value.MiniShippingBinsObtained.Value = Game1.netWorldState.Value.MiniShippingBinsObtained.Value + 1;
				}
			}
			if (item is Object)
			{
				Game1.player.checkForQuestComplete(null, item.parentSheetIndex, item.Stack, item, "", 10);
			}
		}

		public static NPC getRandomTownNPC()
		{
			return getRandomTownNPC(Game1.random);
		}

		public static NPC getRandomTownNPC(Random r)
		{
			Dictionary<string, string> giftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			int index = r.Next(giftTastes.Count);
			NPC i = Game1.getCharacterFromName(giftTastes.ElementAt(index).Key);
			while (giftTastes.ElementAt(index).Key.Equals("Wizard") || giftTastes.ElementAt(index).Key.Equals("Krobus") || giftTastes.ElementAt(index).Key.Equals("Sandy") || giftTastes.ElementAt(index).Key.Equals("Dwarf") || giftTastes.ElementAt(index).Key.Equals("Marlon") || (giftTastes.ElementAt(index).Key.Equals("Leo") && !Game1.MasterPlayer.mailReceived.Contains("addedParrotBoy")) || i == null)
			{
				index = r.Next(giftTastes.Count);
				i = Game1.getCharacterFromName(giftTastes.ElementAt(index).Key);
			}
			return i;
		}

		public static NPC getTownNPCByGiftTasteIndex(int index)
		{
			Dictionary<string, string> giftTastes = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			NPC i = Game1.getCharacterFromName(giftTastes.ElementAt(index).Key);
			int add3 = index += 10;
			add3 %= 25;
			while (i == null)
			{
				i = Game1.getCharacterFromName(giftTastes.ElementAt(add3).Key);
				add3++;
				add3 %= 30;
			}
			return i;
		}

		public static bool foundAllStardrops(Farmer who = null)
		{
			if (who == null)
			{
				who = Game1.player;
			}
			if (who.hasOrWillReceiveMail("CF_Fair") && who.hasOrWillReceiveMail("CF_Fish") && who.hasOrWillReceiveMail("CF_Mines") && who.hasOrWillReceiveMail("CF_Sewer") && who.hasOrWillReceiveMail("museumComplete") && who.hasOrWillReceiveMail("CF_Spouse"))
			{
				return who.hasOrWillReceiveMail("CF_Statue");
			}
			return false;
		}

		public static int getGrandpaScore()
		{
			int points = 0;
			if (Game1.player.totalMoneyEarned >= 50000)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 100000)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 200000)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 300000)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 500000)
			{
				points++;
			}
			if (Game1.player.totalMoneyEarned >= 1000000)
			{
				points += 2;
			}
			if (Game1.player.achievements.Contains(5))
			{
				points++;
			}
			if (Game1.player.hasSkullKey)
			{
				points++;
			}
			bool num = Game1.isLocationAccessible("CommunityCenter");
			if (num || Game1.player.hasCompletedCommunityCenter())
			{
				points++;
			}
			if (num)
			{
				points += 2;
			}
			if (Game1.player.isMarried() && getHomeOfFarmer(Game1.player).upgradeLevel >= 2)
			{
				points++;
			}
			if (Game1.player.hasRustyKey)
			{
				points++;
			}
			if (Game1.player.achievements.Contains(26))
			{
				points++;
			}
			if (Game1.player.achievements.Contains(34))
			{
				points++;
			}
			int numberOfFriendsWithinThisRange = getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999);
			if (numberOfFriendsWithinThisRange >= 5)
			{
				points++;
			}
			if (numberOfFriendsWithinThisRange >= 10)
			{
				points++;
			}
			int level = Game1.player.Level;
			if (level >= 15)
			{
				points++;
			}
			if (level >= 25)
			{
				points++;
			}
			string petName = Game1.player.getPetName();
			if (petName != null)
			{
				Pet pet = Game1.getCharacterFromName<Pet>(petName, mustBeVillager: false);
				if (pet != null && (int)pet.friendshipTowardFarmer >= 999)
				{
					points++;
				}
			}
			return points;
		}

		public static int getGrandpaCandlesFromScore(int score)
		{
			if (score >= 12)
			{
				return 4;
			}
			if (score >= 8)
			{
				return 3;
			}
			if (score >= 4)
			{
				return 2;
			}
			return 1;
		}

		public static bool canItemBeAddedToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			if (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				return true;
			}
			int stack = i.Stack;
			foreach (Item it in list)
			{
				if (it == null)
				{
					return true;
				}
				if (it.canStackWith(i) && it.getRemainingStackSpace() > 0)
				{
					stack -= it.getRemainingStackSpace();
					if (stack <= 0)
					{
						return true;
					}
				}
			}
			return false;
		}

		public static Item addItemToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
		{
			if (i.Stack == 0)
			{
				i.Stack = 1;
			}
			foreach (Item it in list)
			{
				if (it != null && it.canStackWith(i) && it.getRemainingStackSpace() > 0)
				{
					i.Stack = it.addToStack(i);
					if (i.Stack <= 0)
					{
						return null;
					}
				}
			}
			for (int j = list.Count - 1; j >= 0; j--)
			{
				if (list[j] == null)
				{
					if (i.Stack <= i.maximumStackSize())
					{
						list[j] = i;
						return null;
					}
					list[j] = i.getOne();
					list[j].Stack = i.maximumStackSize();
					i.Stack -= i.maximumStackSize();
				}
			}
			while (listMaxSpace != -1 && list.Count < listMaxSpace)
			{
				if (i.Stack > i.maximumStackSize())
				{
					Item tmp = i.getOne();
					tmp.Stack = i.maximumStackSize();
					i.Stack -= i.maximumStackSize();
					list.Add(tmp);
					continue;
				}
				list.Add(i);
				return null;
			}
			return i;
		}

		public static Item addItemToInventory(Item item, int position, IList<Item> items, ItemGrabMenu.behaviorOnItemSelect onAddFunction = null)
		{
			if (items.Equals(Game1.player.items) && item is Object && (item as Object).specialItem)
			{
				if ((bool)(item as Object).bigCraftable)
				{
					if (!Game1.player.specialBigCraftables.Contains((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex)))
					{
						Game1.player.specialBigCraftables.Add((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex));
					}
				}
				else if (!Game1.player.specialItems.Contains((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex)))
				{
					Game1.player.specialItems.Add((item as Object).isRecipe ? (-(int)(item as Object).parentSheetIndex) : ((int)(item as Object).parentSheetIndex));
				}
			}
			if (position >= 0 && position < items.Count)
			{
				if (items[position] == null)
				{
					items[position] = item;
					checkItemFirstInventoryAdd(item);
					onAddFunction?.Invoke(item, null);
					return null;
				}
				if (items[position].maximumStackSize() != -1 && items[position].Name.Equals(item.Name) && (!(item is Object) || !(items[position] is Object) || ((item as Object).quality == (items[position] as Object).quality && (item as Object).parentSheetIndex == (items[position] as Object).parentSheetIndex)) && item.canStackWith(items[position]))
				{
					checkItemFirstInventoryAdd(item);
					int stackLeft = items[position].addToStack(item);
					if (stackLeft <= 0)
					{
						return null;
					}
					item.Stack = stackLeft;
					onAddFunction?.Invoke(item, null);
					return item;
				}
				Item tmp = items[position];
				if (position == Game1.player.CurrentToolIndex && items.Equals(Game1.player.items) && tmp != null)
				{
					tmp.actionWhenStopBeingHeld(Game1.player);
					item.actionWhenBeingHeld(Game1.player);
				}
				checkItemFirstInventoryAdd(item);
				items[position] = item;
				onAddFunction?.Invoke(item, null);
				return tmp;
			}
			return item;
		}

		public static bool spawnObjectAround(Vector2 tileLocation, Object o, GameLocation l, bool playSound = true, Action<Object> modifyObject = null)
		{
			if (o == null || l == null || tileLocation.Equals(Vector2.Zero))
			{
				return false;
			}
			int attempts = 0;
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			openList.Enqueue(tileLocation);
			Vector2 current = Vector2.Zero;
			for (; attempts < 100; attempts++)
			{
				current = openList.Dequeue();
				if (!l.isTileOccupiedForPlacement(current) && !l.isOpenWater((int)current.X, (int)current.Y))
				{
					break;
				}
				closedList.Add(current);
				foreach (Vector2 v in (from a in getAdjacentTileLocations(current)
					orderby Guid.NewGuid()
					select a).ToList())
				{
					if (!closedList.Contains(v))
					{
						openList.Enqueue(v);
					}
				}
			}
			o.isSpawnedObject.Value = true;
			o.canBeGrabbed.Value = true;
			o.tileLocation.Value = current;
			modifyObject?.Invoke(o);
			if (!current.Equals(Vector2.Zero) && !l.isTileOccupiedForPlacement(current) && !l.isOpenWater((int)current.X, (int)current.Y))
			{
				l.objects.Add(current, o);
				if (playSound)
				{
					l.playSound("coin");
				}
				if (l.Equals(Game1.currentLocation))
				{
					l.temporarySprites.Add(new TemporaryAnimatedSprite(5, current * 64f, Color.White));
				}
				return true;
			}
			return false;
		}

		public static bool IsGeode(Item item, bool disallow_special_geodes = false)
		{
			if (item == null)
			{
				return false;
			}
			if (!IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				return false;
			}
			int index = (item as Object).parentSheetIndex;
			if (index == 275 || index == 791)
			{
				return !disallow_special_geodes;
			}
			try
			{
				if (Game1.objectInformation.ContainsKey(index))
				{
					string[] misc_info = Game1.objectInformation[index].Split('/');
					if (misc_info.Length > 6)
					{
						string[] treasures = misc_info[6].Split(' ');
						if (treasures == null || treasures.Length == 0 || !int.TryParse(treasures[0], out int _))
						{
							return false;
						}
						return true;
					}
				}
			}
			catch (Exception)
			{
			}
			return false;
		}

		public static Item getTreasureFromGeode(Item geode)
		{
			bool is_geode = IsGeode(geode);
			if (is_geode)
			{
				try
				{
					Random r = new Random((int)Game1.stats.GeodesCracked + (int)Game1.uniqueIDForThisGame / 2);
					int prewarm_amount2 = r.Next(1, 10);
					for (int j = 0; j < prewarm_amount2; j++)
					{
						r.NextDouble();
					}
					prewarm_amount2 = r.Next(1, 10);
					for (int i = 0; i < prewarm_amount2; i++)
					{
						r.NextDouble();
					}
					int whichGeode = (geode as Object).parentSheetIndex;
					if (r.NextDouble() <= 0.1 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					{
						bool five = r.NextDouble() < 0.25;
						return new Object(890, (!five) ? 1 : 5);
					}
					if (whichGeode == 791)
					{
						if (r.NextDouble() < 0.05 && !Game1.player.hasOrWillReceiveMail("goldenCoconutHat"))
						{
							Game1.player.mailReceived.Add("goldenCoconutHat");
							return new Hat(75);
						}
						switch (r.Next(7))
						{
						case 0:
							return new Object(69, 1);
						case 1:
							return new Object(835, 1);
						case 2:
							return new Object(833, 5);
						case 3:
							return new Object(831, 5);
						case 4:
							return new Object(820, 1);
						case 5:
							return new Object(292, 1);
						case 6:
							return new Object(386, 5);
						}
					}
					else
					{
						if (whichGeode == 275 || !(r.NextDouble() < 0.5))
						{
							string[] treasures = Game1.objectInformation[whichGeode].Split('/')[6].Split(' ');
							int index = Convert.ToInt32(treasures[r.Next(treasures.Length)]);
							if (whichGeode == 749 && r.NextDouble() < 0.008 && (int)Game1.stats.GeodesCracked > 15)
							{
								return new Object(74, 1);
							}
							return new Object(index, 1);
						}
						int amount = r.Next(3) * 2 + 1;
						if (r.NextDouble() < 0.1)
						{
							amount = 10;
						}
						if (r.NextDouble() < 0.01)
						{
							amount = 20;
						}
						if (r.NextDouble() < 0.5)
						{
							switch (r.Next(4))
							{
							case 0:
							case 1:
								return new Object(390, amount);
							case 2:
								return new Object(330, 1);
							case 3:
							{
								int parentSheetIndex;
								switch (whichGeode)
								{
								case 749:
									return new Object(82 + r.Next(3) * 2, 1);
								default:
									parentSheetIndex = 82;
									break;
								case 536:
									parentSheetIndex = 84;
									break;
								case 535:
									parentSheetIndex = 86;
									break;
								}
								return new Object(parentSheetIndex, 1);
							}
							}
						}
						else
						{
							switch (whichGeode)
							{
							case 535:
								switch (r.Next(3))
								{
								case 0:
									return new Object(378, amount);
								case 1:
									return new Object((Game1.player.deepestMineLevel > 25) ? 380 : 378, amount);
								case 2:
									return new Object(382, amount);
								}
								break;
							case 536:
								switch (r.Next(4))
								{
								case 0:
									return new Object(378, amount);
								case 1:
									return new Object(380, amount);
								case 2:
									return new Object(382, amount);
								case 3:
									return new Object((Game1.player.deepestMineLevel > 75) ? 384 : 380, amount);
								}
								break;
							default:
								switch (r.Next(5))
								{
								case 0:
									return new Object(378, amount);
								case 1:
									return new Object(380, amount);
								case 2:
									return new Object(382, amount);
								case 3:
									return new Object(384, amount);
								case 4:
									return new Object(386, amount / 2 + 1);
								}
								break;
							}
						}
					}
					return new Object(Vector2.Zero, 390, 1);
				}
				catch (Exception)
				{
				}
			}
			if (is_geode)
			{
				return new Object(Vector2.Zero, 390, 1);
			}
			return null;
		}

		public static Vector2 snapToInt(Vector2 v)
		{
			v.X = (int)v.X;
			v.Y = (int)v.Y;
			return v;
		}

		public static Vector2 GetNearbyValidPlacementPosition(Farmer who, GameLocation location, Item item, int x, int y)
		{
			if (!Game1.isCheckingNonMousePlacement)
			{
				return new Vector2(x, y);
			}
			int item_width = 1;
			int item_length = 1;
			Point direction = default(Point);
			Microsoft.Xna.Framework.Rectangle bounding_box = new Microsoft.Xna.Framework.Rectangle(0, 0, item_width * 64, item_length * 64);
			if (item is Furniture)
			{
				Furniture furniture = item as Furniture;
				item_width = furniture.getTilesWide();
				item_length = furniture.getTilesHigh();
				bounding_box.Width = furniture.boundingBox.Value.Width;
				bounding_box.Height = furniture.boundingBox.Value.Height;
			}
			switch (who.FacingDirection)
			{
			case 0:
				direction.X = 0;
				direction.Y = -1;
				y -= (item_length - 1) * 64;
				break;
			case 2:
				direction.X = 0;
				direction.Y = 1;
				break;
			case 3:
				direction.X = -1;
				direction.Y = 0;
				x -= (item_width - 1) * 64;
				break;
			case 1:
				direction.X = 1;
				direction.Y = 0;
				break;
			}
			int scan_distance = 2;
			if (item is Object && (item as Object).isPassable() && ((item as Object).Category == -74 || (item as Object).isSapling() || (int)(item as Object).category == -19))
			{
				x = (int)who.GetToolLocation().X / 64 * 64;
				y = (int)who.GetToolLocation().Y / 64 * 64;
				direction.X = who.getTileX() - x / 64;
				direction.Y = who.getTileY() - y / 64;
				int magnitude = (int)Math.Sqrt(Math.Pow(direction.X, 2.0) + Math.Pow(direction.Y, 2.0));
				if (magnitude > 0)
				{
					direction.X /= magnitude;
					direction.Y /= magnitude;
				}
				scan_distance = magnitude + 1;
			}
			bool is_passable = item is Object && (item as Object).isPassable();
			x = x / 64 * 64;
			y = y / 64 * 64;
			for (int offset = 0; offset < scan_distance; offset++)
			{
				int checked_x = x + direction.X * offset * 64;
				int checked_y = y + direction.Y * offset * 64;
				bounding_box.X = checked_x;
				bounding_box.Y = checked_y;
				if ((!who.GetBoundingBox().Intersects(bounding_box) && !is_passable) || playerCanPlaceItemHere(location, item, checked_x, checked_y, who))
				{
					return new Vector2(checked_x, checked_y);
				}
			}
			return new Vector2(x, y);
		}

		public static bool tryToPlaceItem(GameLocation location, Item item, int x, int y)
		{
			if (item == null)
			{
				return false;
			}
			if (item is Tool)
			{
				return false;
			}
			Vector2 tileLocation = new Vector2(x / 64, y / 64);
			if (playerCanPlaceItemHere(location, item, x, y, Game1.player))
			{
				if (item is Furniture)
				{
					Game1.player.ActiveObject = null;
				}
				if (((Object)item).placementAction(location, x, y, Game1.player))
				{
					Game1.player.reduceActiveItemByOne();
				}
				else if (item is Furniture)
				{
					Game1.player.ActiveObject = (item as Furniture);
				}
				else if (item is Wallpaper)
				{
					return false;
				}
				return true;
			}
			if (isPlacementForbiddenHere(location) && item != null && item.isPlaceable())
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
			}
			else if (item is Furniture)
			{
				switch ((item as Furniture).GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
				{
				case 1:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12629"));
					break;
				case 2:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
					break;
				case 3:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12633"));
					break;
				case 4:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
					break;
				}
			}
			if (item.Category == -19 && location.terrainFeatures.ContainsKey(tileLocation) && location.terrainFeatures[tileLocation] is HoeDirt)
			{
				HoeDirt hoe_dirt = location.terrainFeatures[tileLocation] as HoeDirt;
				if ((int)(location.terrainFeatures[tileLocation] as HoeDirt).fertilizer != 0)
				{
					if ((location.terrainFeatures[tileLocation] as HoeDirt).fertilizer != item.parentSheetIndex)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
					}
					return false;
				}
				if (((int)item.parentSheetIndex == 368 || (int)item.parentSheetIndex == 368) && hoe_dirt.crop != null && (int)hoe_dirt.crop.currentPhase != 0)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
					return false;
				}
			}
			return false;
		}

		public static int showLanternBar()
		{
			foreach (Item t in Game1.player.Items)
			{
				if (t != null && t is Lantern && ((Lantern)t).on)
				{
					return ((Lantern)t).fuelLeft;
				}
			}
			return -1;
		}

		public static void plantCrops(GameLocation farm, int seedType, int x, int y, int width, int height, int daysOld)
		{
			for (int j = x; j < x + width; j++)
			{
				for (int i = y; i < y + height; i++)
				{
					Vector2 v = new Vector2(j, i);
					farm.makeHoeDirt(v);
					if (farm.terrainFeatures.ContainsKey(v) && farm.terrainFeatures[v] is HoeDirt)
					{
						((HoeDirt)farm.terrainFeatures[v]).crop = new Crop(seedType, x, y);
					}
				}
			}
		}

		public static bool pointInRectangles(List<Microsoft.Xna.Framework.Rectangle> rectangles, int x, int y)
		{
			foreach (Microsoft.Xna.Framework.Rectangle rectangle in rectangles)
			{
				if (rectangle.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public static Keys mapGamePadButtonToKey(Buttons b)
		{
			switch (b)
			{
			case Buttons.A:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton);
			case Buttons.X:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.useToolButton);
			case Buttons.B:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
			case Buttons.Back:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton);
			case Buttons.Start:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
			case Buttons.Y:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton);
			case Buttons.RightTrigger:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.toolSwapButton);
			case Buttons.LeftTrigger:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.toolSwapButton);
			case Buttons.DPadUp:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton);
			case Buttons.DPadRight:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton);
			case Buttons.DPadDown:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton);
			case Buttons.DPadLeft:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton);
			case Buttons.LeftThumbstickUp:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton);
			case Buttons.LeftThumbstickRight:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton);
			case Buttons.LeftThumbstickDown:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton);
			case Buttons.LeftThumbstickLeft:
				return Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton);
			default:
				return Keys.None;
			}
		}

		public static ButtonCollection getPressedButtons(GamePadState padState, GamePadState oldPadState)
		{
			return new ButtonCollection(ref padState, ref oldPadState);
		}

		public static bool thumbstickIsInDirection(int direction, GamePadState padState)
		{
			if (Game1.currentMinigame != null)
			{
				return true;
			}
			if (direction == 0 && Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y)
			{
				return true;
			}
			if (direction == 1 && padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			if (direction == 2 && Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			if (direction == 3 && Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y))
			{
				return true;
			}
			return false;
		}

		public static ButtonCollection getHeldButtons(GamePadState padState)
		{
			return new ButtonCollection(ref padState);
		}

		public static bool toggleMuteMusic()
		{
			if (Game1.soundBank != null)
			{
				if (Game1.options.musicVolumeLevel != 0f)
				{
					disableMusic();
					return true;
				}
				enableMusic();
			}
			return false;
		}

		public static void enableMusic()
		{
			if (Game1.soundBank != null)
			{
				Game1.options.musicVolumeLevel = 0.75f;
				Game1.musicCategory.SetVolume(0.75f);
				Game1.musicPlayerVolume = 0.75f;
				Game1.options.ambientVolumeLevel = 0.75f;
				Game1.ambientCategory.SetVolume(0.75f);
				Game1.ambientPlayerVolume = 0.75f;
			}
		}

		public static void disableMusic()
		{
			if (Game1.soundBank != null)
			{
				Game1.options.musicVolumeLevel = 0f;
				Game1.musicCategory.SetVolume(0f);
				Game1.options.ambientVolumeLevel = 0f;
				Game1.ambientCategory.SetVolume(0f);
				Game1.ambientPlayerVolume = 0f;
				Game1.musicPlayerVolume = 0f;
			}
		}

		public static Vector2 getVelocityTowardPlayer(Point startingPoint, float speed, Farmer f)
		{
			return getVelocityTowardPoint(startingPoint, new Vector2(f.GetBoundingBox().X, f.GetBoundingBox().Y), speed);
		}

		public static string getHoursMinutesStringFromMilliseconds(ulong milliseconds)
		{
			return milliseconds / 3600000uL + ":" + ((milliseconds % 3600000uL / 60000uL < 10) ? "0" : "") + milliseconds % 3600000uL / 60000uL;
		}

		public static string getMinutesSecondsStringFromMilliseconds(int milliseconds)
		{
			return milliseconds / 60000 + ":" + ((milliseconds % 60000 / 1000 < 10) ? "0" : "") + milliseconds % 60000 / 1000;
		}

		public static Vector2 getVelocityTowardPoint(Vector2 startingPoint, Vector2 endingPoint, float speed)
		{
			double xDif2 = endingPoint.X - startingPoint.X;
			double yDif2 = endingPoint.Y - startingPoint.Y;
			if (Math.Abs(xDif2) < 0.1 && Math.Abs(yDif2) < 0.1)
			{
				return new Vector2(0f, 0f);
			}
			double total = Math.Sqrt(Math.Pow(xDif2, 2.0) + Math.Pow(yDif2, 2.0));
			xDif2 /= total;
			yDif2 /= total;
			return new Vector2((float)(xDif2 * (double)speed), (float)(yDif2 * (double)speed));
		}

		public static Vector2 getVelocityTowardPoint(Point startingPoint, Vector2 endingPoint, float speed)
		{
			return getVelocityTowardPoint(new Vector2(startingPoint.X, startingPoint.Y), endingPoint, speed);
		}

		public static Vector2 getRandomPositionInThisRectangle(Microsoft.Xna.Framework.Rectangle r, Random random)
		{
			return new Vector2(random.Next(r.X, r.X + r.Width), random.Next(r.Y, r.Y + r.Height));
		}

		public static Vector2 getTopLeftPositionForCenteringOnScreen(xTile.Dimensions.Rectangle viewport, int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return new Vector2(viewport.Width / 2 - width / 2 + xOffset, viewport.Height / 2 - height / 2 + yOffset);
		}

		public static Vector2 getTopLeftPositionForCenteringOnScreen(int width, int height, int xOffset = 0, int yOffset = 0)
		{
			return getTopLeftPositionForCenteringOnScreen(Game1.uiViewport, width, height, xOffset, yOffset);
		}

		public static void recursiveFindPositionForCharacter(NPC c, GameLocation l, Vector2 tileLocation, int maxIterations)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			while (true)
			{
				if (iterations >= maxIterations || positionsToCheck.Count <= 0)
				{
					return;
				}
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(c.GetBoundingBox().Width / 2), currentPoint.Y * 64f - (float)c.GetBoundingBox().Height);
				if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, c, pathfinding: true))
				{
					break;
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 v in directionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
				iterations++;
			}
			if (!l.characters.Contains(c))
			{
				l.characters.Add(c);
				c.currentLocation = l;
			}
		}

		public static Vector2 recursiveFindOpenTileForCharacter(Character c, GameLocation l, Vector2 tileLocation, int maxIterations, bool allowOffMap = true)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			Vector2 originalPosition = c.Position;
			for (; iterations < maxIterations; iterations++)
			{
				if (positionsToCheck.Count <= 0)
				{
					break;
				}
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(c.GetBoundingBox().Width / 2), currentPoint.Y * 64f + 4f);
				if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, c is Farmer, 0, glider: false, c, pathfinding: true) && (allowOffMap || l.isTileOnMap(currentPoint)))
				{
					c.Position = originalPosition;
					return currentPoint;
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 v in directionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
			}
			c.Position = originalPosition;
			return Vector2.Zero;
		}

		public static List<Vector2> recursiveFindOpenTiles(GameLocation l, Vector2 tileLocation, int maxOpenTilesToFind = 24, int maxIterations = 50)
		{
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(tileLocation);
			List<Vector2> closedList = new List<Vector2>();
			List<Vector2> successList = new List<Vector2>();
			for (; iterations < maxIterations; iterations++)
			{
				if (positionsToCheck.Count <= 0)
				{
					break;
				}
				if (successList.Count >= maxOpenTilesToFind)
				{
					break;
				}
				Vector2 currentPoint = positionsToCheck.Dequeue();
				closedList.Add(currentPoint);
				if (l.isTileLocationTotallyClearAndPlaceable(currentPoint))
				{
					successList.Add(currentPoint);
				}
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				foreach (Vector2 v in directionsTileVectors)
				{
					if (!closedList.Contains(currentPoint + v))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
			}
			return successList;
		}

		public static void spreadAnimalsAround(Building b, Farm environment)
		{
			try
			{
			}
			catch (Exception)
			{
			}
		}

		public static void spreadAnimalsAround(Building b, Farm environment, List<FarmAnimal> animalsList)
		{
			if (b.indoors.Value == null || !(b.indoors.Value is AnimalHouse))
			{
				return;
			}
			Queue<FarmAnimal> animals = new Queue<FarmAnimal>(animalsList);
			int iterations = 0;
			Queue<Vector2> positionsToCheck = new Queue<Vector2>();
			positionsToCheck.Enqueue(new Vector2((int)b.tileX + b.animalDoor.X, (int)b.tileY + b.animalDoor.Y + 1));
			while (animals.Count > 0 && iterations < 40 && positionsToCheck.Count > 0)
			{
				Vector2 currentPoint = positionsToCheck.Dequeue();
				animals.Peek().Position = new Vector2(currentPoint.X * 64f + 32f - (float)(animals.Peek().GetBoundingBox().Width / 2), currentPoint.Y * 64f - 32f - (float)(animals.Peek().GetBoundingBox().Height / 2));
				if (!environment.isCollidingPosition(animals.Peek().GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, animals.Peek(), pathfinding: true))
				{
					FarmAnimal a = animals.Dequeue();
					environment.animals.Add(a.myID, a);
				}
				if (animals.Count > 0)
				{
					Vector2[] directionsTileVectors = DirectionsTileVectors;
					for (int i = 0; i < directionsTileVectors.Length; i++)
					{
						Vector2 v = directionsTileVectors[i];
						animals.Peek().Position = new Vector2((currentPoint.X + v.X) * 64f + 32f - (float)(animals.Peek().GetBoundingBox().Width / 2), (currentPoint.Y + v.Y) * 64f - 32f - (float)(animals.Peek().GetBoundingBox().Height / 2));
						if (!environment.isCollidingPosition(animals.Peek().GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, animals.Peek(), pathfinding: true))
						{
							positionsToCheck.Enqueue(currentPoint + v);
						}
					}
				}
				iterations++;
			}
		}

		public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, bool projectile = false)
		{
			return horizontalOrVerticalCollisionDirections(boundingBox, null, projectile);
		}

		public static Point findTile(GameLocation location, int tileIndex, string layer)
		{
			for (int y = 0; y < location.map.GetLayer(layer).LayerHeight; y++)
			{
				for (int x = 0; x < location.map.GetLayer(layer).LayerWidth; x++)
				{
					if (location.getTileIndexAt(x, y, layer) == tileIndex)
					{
						return new Point(x, y);
					}
				}
			}
			return new Point(-1, -1);
		}

		public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, Character c, bool projectile = false)
		{
			bool[] directions = new bool[2];
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
			rect.Width = 1;
			rect.X = boundingBox.Center.X;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
				{
					directions[1] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				directions[1] = true;
			}
			rect.Width = boundingBox.Width;
			rect.X = boundingBox.X;
			rect.Height = 1;
			rect.Y = boundingBox.Center.Y;
			if (c != null)
			{
				if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
				{
					directions[0] = true;
				}
			}
			else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				directions[0] = true;
			}
			return directions;
		}

		public static Color getBlendedColor(Color c1, Color c2)
		{
			return new Color((Game1.random.NextDouble() < 0.5) ? Math.Max(c1.R, c2.R) : ((c1.R + c2.R) / 2), (Game1.random.NextDouble() < 0.5) ? Math.Max(c1.G, c2.G) : ((c1.G + c2.G) / 2), (Game1.random.NextDouble() < 0.5) ? Math.Max(c1.B, c2.B) : ((c1.B + c2.B) / 2));
		}

		public static Character checkForCharacterWithinArea(Type kindOfCharacter, Vector2 positionToAvoid, GameLocation location, Microsoft.Xna.Framework.Rectangle area)
		{
			foreach (NPC i in location.characters)
			{
				if (i.GetType().Equals(kindOfCharacter) && i.GetBoundingBox().Intersects(area) && !i.Position.Equals(positionToAvoid))
				{
					return i;
				}
			}
			return null;
		}

		public static int getNumberOfCharactersInRadius(GameLocation l, Point position, int tileRadius)
		{
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(position.X - tileRadius * 64, position.Y - tileRadius * 64, (tileRadius * 2 + 1) * 64, (tileRadius * 2 + 1) * 64);
			int count = 0;
			foreach (NPC i in l.characters)
			{
				if (rect.Contains(Vector2ToPoint(i.Position)))
				{
					count++;
				}
			}
			return count;
		}

		public static List<Vector2> getListOfTileLocationsForBordersOfNonTileRectangle(Microsoft.Xna.Framework.Rectangle rectangle)
		{
			return new List<Vector2>
			{
				new Vector2(rectangle.Left / 64, rectangle.Top / 64),
				new Vector2(rectangle.Right / 64, rectangle.Top / 64),
				new Vector2(rectangle.Left / 64, rectangle.Bottom / 64),
				new Vector2(rectangle.Right / 64, rectangle.Bottom / 64),
				new Vector2(rectangle.Left / 64, rectangle.Center.Y / 64),
				new Vector2(rectangle.Right / 64, rectangle.Center.Y / 64),
				new Vector2(rectangle.Center.X / 64, rectangle.Bottom / 64),
				new Vector2(rectangle.Center.X / 64, rectangle.Top / 64),
				new Vector2(rectangle.Center.X / 64, rectangle.Center.Y / 64)
			};
		}

		public static void makeTemporarySpriteJuicier(TemporaryAnimatedSprite t, GameLocation l, int numAddOns = 4, int xRange = 64, int yRange = 64)
		{
			t.position.Y -= 8f;
			l.temporarySprites.Add(t);
			for (int i = 0; i < numAddOns; i++)
			{
				TemporaryAnimatedSprite clone = t.getClone();
				clone.delayBeforeAnimationStart = i * 100;
				clone.position += new Vector2(Game1.random.Next(-xRange / 2, xRange / 2 + 1), Game1.random.Next(-yRange / 2, yRange / 2 + 1));
				l.temporarySprites.Add(clone);
			}
		}

		public static void recursiveObjectPlacement(Object o, int tileX, int tileY, double growthRate, double decay, GameLocation location, string terrainToExclude = "", int objectIndexAddRange = 0, double failChance = 0.0, int objectIndeAddRangeMultiplier = 1)
		{
			if (!location.isTileLocationOpen(new Location(tileX, tileY)) || location.isTileOccupied(new Vector2(tileX, tileY)) || location.getTileIndexAt(tileX, tileY, "Back") == -1 || (!terrainToExclude.Equals("") && (location.doesTileHaveProperty(tileX, tileY, "Type", "Back") == null || location.doesTileHaveProperty(tileX, tileY, "Type", "Back").Equals(terrainToExclude))))
			{
				return;
			}
			Vector2 objectPos = new Vector2(tileX, tileY);
			if (Game1.random.NextDouble() > failChance * 2.0)
			{
				if (o is ColoredObject)
				{
					location.objects.Add(objectPos, new ColoredObject((int)o.parentSheetIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier, 1, (o as ColoredObject).color)
					{
						Fragility = o.fragility,
						MinutesUntilReady = o.minutesUntilReady,
						Name = o.name,
						CanBeSetDown = o.CanBeSetDown,
						CanBeGrabbed = o.CanBeGrabbed,
						IsSpawnedObject = o.IsSpawnedObject,
						TileLocation = objectPos,
						ColorSameIndexAsParentSheetIndex = (o as ColoredObject).ColorSameIndexAsParentSheetIndex
					});
				}
				else
				{
					location.objects.Add(objectPos, new Object(objectPos, (int)o.parentSheetIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier, o.name, o.canBeSetDown, o.canBeGrabbed, o.isHoedirt, o.isSpawnedObject)
					{
						Fragility = o.fragility,
						MinutesUntilReady = o.minutesUntilReady
					});
				}
			}
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX + 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX - 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX, tileY + 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveObjectPlacement(o, tileX, tileY - 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier);
			}
		}

		public static void recursiveFarmGrassPlacement(int tileX, int tileY, double growthRate, double decay, GameLocation farm)
		{
			if (farm.isTileLocationOpen(new Location(tileX, tileY)) && !farm.isTileOccupied(new Vector2(tileX, tileY)) && farm.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
			{
				Vector2 objectPos = new Vector2(tileX, tileY);
				if (Game1.random.NextDouble() < 0.05)
				{
					farm.objects.Add(new Vector2(tileX, tileY), new Object(new Vector2(tileX, tileY), (Game1.random.NextDouble() < 0.5) ? 674 : 675, 1));
				}
				else
				{
					farm.terrainFeatures.Add(objectPos, new Grass(1, 4 - (int)((1.0 - growthRate) * 4.0)));
				}
				growthRate -= decay;
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX + 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX - 1, tileY, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX, tileY + 1, growthRate, decay, farm);
				}
				if (Game1.random.NextDouble() < growthRate)
				{
					recursiveFarmGrassPlacement(tileX, tileY - 1, growthRate, decay, farm);
				}
			}
		}

		public static void recursiveTreePlacement(int tileX, int tileY, double growthRate, int growthStage, double skipChance, GameLocation l, Microsoft.Xna.Framework.Rectangle clearPatch, bool sparse)
		{
			if (clearPatch.Contains(tileX, tileY))
			{
				return;
			}
			Vector2 location = new Vector2(tileX, tileY);
			if (l.doesTileHaveProperty((int)location.X, (int)location.Y, "Diggable", "Back") == null || l.doesTileHaveProperty((int)location.X, (int)location.Y, "NoSpawn", "Back") != null || !l.isTileLocationOpen(new Location((int)location.X, (int)location.Y)) || l.isTileOccupied(location) || (sparse && (l.isTileOccupied(new Vector2(tileX, tileY + -1)) || l.isTileOccupied(new Vector2(tileX, tileY + 1)) || l.isTileOccupied(new Vector2(tileX + 1, tileY)) || l.isTileOccupied(new Vector2(tileX + -1, tileY)) || l.isTileOccupied(new Vector2(tileX + 1, tileY + 1)))))
			{
				return;
			}
			if (Game1.random.NextDouble() > skipChance)
			{
				if (sparse && location.X < 70f && (location.X < 48f || location.Y > 26f) && Game1.random.NextDouble() < 0.07)
				{
					(l as Farm).resourceClumps.Add(new ResourceClump((Game1.random.NextDouble() < 0.5) ? 672 : ((Game1.random.NextDouble() < 0.5) ? 600 : 602), 2, 2, location));
				}
				else
				{
					l.terrainFeatures.Add(location, new Tree(Game1.random.Next(1, 4), (growthStage < 5) ? Game1.random.Next(5) : 5));
				}
				growthRate -= 0.05;
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX + Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX - Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX, tileY + Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveTreePlacement(tileX, tileY - Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
			}
		}

		public static void recursiveRemoveTerrainFeatures(int tileX, int tileY, double growthRate, double decay, GameLocation l)
		{
			Vector2 location = new Vector2(tileX, tileY);
			l.terrainFeatures.Remove(location);
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX + 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX - 1, tileY, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX, tileY + 1, growthRate, decay, l);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveRemoveTerrainFeatures(tileX, tileY - 1, growthRate, decay, l);
			}
		}

		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration)
		{
			return generateNewFarm(skipFarmGeneration, loadForNewGame: true);
		}

		public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration, bool loadForNewGame)
		{
			Game1.fadeToBlack = false;
			Game1.fadeToBlackAlpha = 1f;
			Game1.debrisWeather.Clear();
			Game1.viewport.X = -9999;
			Game1.changeMusicTrack("none");
			if (loadForNewGame)
			{
				Game1.loadForNewGame();
			}
			Game1.currentLocation = Game1.getLocationFromName("Farmhouse");
			Game1.currentLocation.currentEvent = new Event("none/-600 -600/farmer 4 8 2/warp farmer 4 8/end beginGame");
			Game1.gameMode = 2;
			yield return 100;
		}

		public static bool isOnScreen(Vector2 positionNonTile, int acceptableDistanceFromScreen)
		{
			positionNonTile.X -= Game1.viewport.X;
			positionNonTile.Y -= Game1.viewport.Y;
			if (positionNonTile.X > (float)(-acceptableDistanceFromScreen) && positionNonTile.X < (float)(Game1.viewport.Width + acceptableDistanceFromScreen) && positionNonTile.Y > (float)(-acceptableDistanceFromScreen))
			{
				return positionNonTile.Y < (float)(Game1.viewport.Height + acceptableDistanceFromScreen);
			}
			return false;
		}

		public static bool isOnScreen(Point positionTile, int acceptableDistanceFromScreenNonTile, GameLocation location = null)
		{
			if (location != null && !location.Equals(Game1.currentLocation))
			{
				return false;
			}
			if (positionTile.X * 64 > Game1.viewport.X - acceptableDistanceFromScreenNonTile && positionTile.X * 64 < Game1.viewport.X + Game1.viewport.Width + acceptableDistanceFromScreenNonTile && positionTile.Y * 64 > Game1.viewport.Y - acceptableDistanceFromScreenNonTile)
			{
				return positionTile.Y * 64 < Game1.viewport.Y + Game1.viewport.Height + acceptableDistanceFromScreenNonTile;
			}
			return false;
		}

		public static void createPotteryTreasure(int tileX, int tileY)
		{
		}

		public static void clearObjectsInArea(Microsoft.Xna.Framework.Rectangle r, GameLocation l)
		{
			for (int x = r.Left; x < r.Right; x += 64)
			{
				for (int y = r.Top; y < r.Bottom; y += 64)
				{
					l.removeEverythingFromThisTile(x / 64, y / 64);
				}
			}
		}

		public static void trashItem(Item item)
		{
			if (item is Object && Game1.player.specialItems.Contains((item as Object).parentSheetIndex))
			{
				Game1.player.specialItems.Remove((item as Object).parentSheetIndex);
			}
			if (getTrashReclamationPrice(item, Game1.player) > 0)
			{
				Game1.player.Money += getTrashReclamationPrice(item, Game1.player);
			}
			Game1.playSound("trashcan");
		}

		public static FarmAnimal GetBestHarvestableFarmAnimal(IEnumerable<FarmAnimal> animals, Tool tool, Microsoft.Xna.Framework.Rectangle toolRect)
		{
			FarmAnimal fallbackAnimal = null;
			foreach (FarmAnimal animal in animals)
			{
				if (animal.GetHarvestBoundingBox().Intersects(toolRect))
				{
					if (animal.toolUsedForHarvest.Equals(tool.BaseName) && (int)animal.currentProduce > 0 && (int)animal.age >= (byte)animal.ageWhenMature)
					{
						return animal;
					}
					fallbackAnimal = animal;
				}
			}
			return fallbackAnimal;
		}

		public static void recolorDialogueAndMenu(string theme)
		{
			Color color37 = Color.White;
			Color color36 = Color.White;
			Color color35 = Color.White;
			Color color34 = Color.White;
			Color color33 = Color.White;
			Color color32 = Color.White;
			Color color31 = Color.White;
			Color color30 = Color.White;
			Color color29 = Color.White;
			switch (theme)
			{
			case "Earthy":
				color37 = new Color(44, 35, 0);
				color36 = new Color(115, 147, 102);
				color35 = new Color(91, 65, 0);
				color34 = new Color(122, 83, 0);
				color33 = new Color(179, 181, 125);
				color32 = new Color(144, 96, 0);
				color31 = new Color(234, 227, 190);
				color30 = new Color(255, 255, 227);
				color29 = new Color(193, 187, 156);
				break;
			case "Basic":
				color37 = new Color(47, 46, 36);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(220, 215, 194);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Outer Space":
				color37 = new Color(20, 20, 20);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(194, 189, 202);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Skyscape":
				color37 = new Color(15, 31, 57);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(206, 237, 254);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Ghosts N' Goblins":
				color37 = new Color(55, 0, 0);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(196, 197, 230);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Sweeties":
				color37 = new Color(120, 60, 60);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(255, 213, 227);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Biomes":
				color37 = new Color(17, 36, 0);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(192, 255, 183);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Sports":
				color37 = new Color(110, 45, 0);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(255, 214, 168);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Wasteland":
				color37 = new Color(14, 12, 10);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 15);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 15);
				color31 = new Color(185, 178, 165);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Bombs Away":
				color37 = new Color(50, 20, 0);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color33 = Color.Tan;
				color32 = new Color(color34.R + 30, color34.G + 30, color34.B + 30);
				color31 = new Color(192, 167, 143);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Polynomial":
				color37 = new Color(60, 60, 60);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 60);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 30);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 30);
				color32 = new Color(254, 254, 254);
				color33 = new Color(color34.R + 30, color34.G + 30, color34.B + 30);
				color31 = new Color(225, 225, 225);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			case "Duchess":
				color37 = new Color(69, 45, 0);
				color36 = new Color(color37.R + 60, color37.G + 60, color37.B + 30);
				color35 = new Color(color36.R + 30, color36.G + 30, color36.B + 20);
				color34 = new Color(color35.R + 30, color35.G + 30, color35.B + 20);
				color33 = new Color(color34.R + 15, color34.G + 15, color34.B + 10);
				color32 = new Color(color33.R + 15, color33.G + 15, color33.B + 10);
				color31 = new Color(227, 221, 174);
				color30 = new Color(Math.Min(255, color31.R + 30), Math.Min(255, color31.G + 30), Math.Min(255, color31.B + 30));
				color29 = new Color(color31.R - 30, color31.G - 30, color31.B - 30);
				break;
			}
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15633, color37.R, color37.G, color37.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15645, color32.R, color32.G, color32.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15649, color34.R, color34.G, color34.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15641, color34.R, color34.G, color34.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15637, color35.R, color35.G, color35.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 15666, color31.R, color31.G, color31.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 40577, color30.R, color30.G, color30.B);
			Game1.menuTexture = ColorChanger.swapColor(Game1.menuTexture, 40637, color29.R, color29.G, color29.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1760, color37.R, color37.G, color37.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1764, color35.R, color35.G, color35.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1768, color34.R, color34.G, color34.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1841, color32.R, color32.G, color32.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1792, color31.R, color31.G, color31.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1834, color30.R, color30.G, color30.B);
			Game1.toolIconBox = ColorChanger.swapColor(Game1.toolIconBox, 1773, color29.R, color29.G, color29.B);
		}

		public static long RandomLong(Random r = null)
		{
			if (r == null)
			{
				r = new Random();
			}
			byte[] bytes = new byte[8];
			r.NextBytes(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static ulong NewUniqueIdForThisGame()
		{
			DateTime epoc = new DateTime(2012, 6, 22);
			return (ulong)(long)(DateTime.UtcNow - epoc).TotalSeconds;
		}

		public static string FilterDirtyWords(string words)
		{
			return Program.sdk.FilterDirtyWords(words);
		}

		public static string FilterUserName(string name)
		{
			return name;
		}

		public static bool IsHorizontalDirection(int direction)
		{
			if (direction != 3)
			{
				return direction == 1;
			}
			return true;
		}

		public static bool IsVerticalDirection(int direction)
		{
			if (direction != 0)
			{
				return direction == 2;
			}
			return true;
		}

		public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int pixels)
		{
			rect.Height += 2 * pixels;
			rect.Width += 2 * pixels;
			rect.X -= pixels;
			rect.Y -= pixels;
			return rect;
		}

		public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int facingDirection, int pixels)
		{
			switch (facingDirection)
			{
			case 0:
				rect.Height += pixels;
				rect.Y -= pixels;
				break;
			case 1:
				rect.Width += pixels;
				break;
			case 2:
				rect.Height += pixels;
				break;
			case 3:
				rect.Width += pixels;
				rect.X -= pixels;
				break;
			}
			return rect;
		}

		public static int GetOppositeFacingDirection(int facingDirection)
		{
			switch (facingDirection)
			{
			case 0:
				return 2;
			case 1:
				return 3;
			case 2:
				return 0;
			case 3:
				return 1;
			default:
				return 0;
			}
		}

		public static void RGBtoHSL(int r, int g, int b, out double h, out double s, out double l)
		{
			double double_r = (double)r / 255.0;
			double double_g = (double)g / 255.0;
			double double_b = (double)b / 255.0;
			double max = double_r;
			if (max < double_g)
			{
				max = double_g;
			}
			if (max < double_b)
			{
				max = double_b;
			}
			double min = double_r;
			if (min > double_g)
			{
				min = double_g;
			}
			if (min > double_b)
			{
				min = double_b;
			}
			double diff = max - min;
			l = (max + min) / 2.0;
			if (Math.Abs(diff) < 1E-05)
			{
				s = 0.0;
				h = 0.0;
				return;
			}
			if (l <= 0.5)
			{
				s = diff / (max + min);
			}
			else
			{
				s = diff / (2.0 - max - min);
			}
			double r_dist = (max - double_r) / diff;
			double g_dist = (max - double_g) / diff;
			double b_dist = (max - double_b) / diff;
			if (double_r == max)
			{
				h = b_dist - g_dist;
			}
			else if (double_g == max)
			{
				h = 2.0 + r_dist - b_dist;
			}
			else
			{
				h = 4.0 + g_dist - r_dist;
			}
			h *= 60.0;
			if (h < 0.0)
			{
				h += 360.0;
			}
		}

		public static void HSLtoRGB(double h, double s, double l, out int r, out int g, out int b)
		{
			double p2 = (!(l <= 0.5)) ? (l + s - l * s) : (l * (1.0 + s));
			double p = 2.0 * l - p2;
			double double_r;
			double double_g;
			double double_b;
			if (s == 0.0)
			{
				double_r = l;
				double_g = l;
				double_b = l;
			}
			else
			{
				double_r = QQHtoRGB(p, p2, h + 120.0);
				double_g = QQHtoRGB(p, p2, h);
				double_b = QQHtoRGB(p, p2, h - 120.0);
			}
			r = (int)(double_r * 255.0);
			g = (int)(double_g * 255.0);
			b = (int)(double_b * 255.0);
		}

		private static double QQHtoRGB(double q1, double q2, double hue)
		{
			if (hue > 360.0)
			{
				hue -= 360.0;
			}
			else if (hue < 0.0)
			{
				hue += 360.0;
			}
			if (hue < 60.0)
			{
				return q1 + (q2 - q1) * hue / 60.0;
			}
			if (hue < 180.0)
			{
				return q2;
			}
			if (hue < 240.0)
			{
				return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
			}
			return q1;
		}

		public static float ModifyCoordinateFromUIScale(float coordinate)
		{
			return coordinate * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		public static Vector2 ModifyCoordinatesFromUIScale(Vector2 coordinates)
		{
			return coordinates * Game1.options.uiScale / Game1.options.zoomLevel;
		}

		public static float ModifyCoordinateForUIScale(float coordinate)
		{
			return coordinate / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		public static Vector2 ModifyCoordinatesForUIScale(Vector2 coordinates)
		{
			return coordinates / Game1.options.uiScale * Game1.options.zoomLevel;
		}

		public static bool ShouldIgnoreValueChangeCallback()
		{
			if (Game1.gameMode != 3)
			{
				return true;
			}
			if (Game1.client != null && !Game1.client.readyToPlay)
			{
				return true;
			}
			if (Game1.client != null && Game1.locationRequest != null)
			{
				return true;
			}
			return false;
		}
	}
}
