using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class MapPage : IClickableMenu
	{
		public const int region_desert = 1001;

		public const int region_farm = 1002;

		public const int region_backwoods = 1003;

		public const int region_busstop = 1004;

		public const int region_wizardtower = 1005;

		public const int region_marnieranch = 1006;

		public const int region_leahcottage = 1007;

		public const int region_samhouse = 1008;

		public const int region_haleyhouse = 1009;

		public const int region_townsquare = 1010;

		public const int region_harveyclinic = 1011;

		public const int region_generalstore = 1012;

		public const int region_blacksmith = 1013;

		public const int region_saloon = 1014;

		public const int region_manor = 1015;

		public const int region_museum = 1016;

		public const int region_elliottcabin = 1017;

		public const int region_sewer = 1018;

		public const int region_graveyard = 1019;

		public const int region_trailer = 1020;

		public const int region_alexhouse = 1021;

		public const int region_sciencehouse = 1022;

		public const int region_tent = 1023;

		public const int region_mines = 1024;

		public const int region_adventureguild = 1025;

		public const int region_quarry = 1026;

		public const int region_jojamart = 1027;

		public const int region_fishshop = 1028;

		public const int region_spa = 1029;

		public const int region_secretwoods = 1030;

		public const int region_ruinedhouse = 1031;

		public const int region_communitycenter = 1032;

		public const int region_sewerpipe = 1033;

		public const int region_railroad = 1034;

		public const int region_island = 1035;

		private string descriptionText = "";

		private string hoverText = "";

		private string playerLocationName;

		private Texture2D map;

		private int mapX;

		private int mapY;

		public List<ClickableComponent> points = new List<ClickableComponent>();

		private bool drawPamHouseUpgrade;

		private bool drawMovieTheaterJoja;

		private bool drawMovieTheater;

		private bool drawIsland;

		public MapPage(int x, int y, int width, int height)
			: base(x, y, width, height)
		{
			map = Game1.content.Load<Texture2D>("LooseSprites\\map");
			Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(map.Bounds.Width * 4, 720);
			drawPamHouseUpgrade = Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade");
			drawMovieTheaterJoja = Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja");
			drawMovieTheater = Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater");
			mapX = (int)center.X;
			mapY = (int)center.Y;
			points.Add(new ClickableComponent(new Rectangle(mapX, mapY, 292, 152), Game1.MasterPlayer.mailReceived.Contains("ccVault") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062") : "???")
			{
				myID = 1001,
				rightNeighborID = 1003,
				downNeighborID = 1030
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 324, mapY + 252, 188, 132), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.MasterPlayer.farmName))
			{
				myID = 1002,
				leftNeighborID = 1005,
				upNeighborID = 1003,
				rightNeighborID = 1004,
				downNeighborID = 1006
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 360, mapY + 96, 188, 132), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11065"))
			{
				myID = 1003,
				downNeighborID = 1002,
				leftNeighborID = 1001,
				rightNeighborID = 1022,
				upNeighborID = 1029
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 516, mapY + 224, 76, 100), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11066"))
			{
				myID = 1004,
				leftNeighborID = 1002,
				upNeighborID = 1003,
				downNeighborID = 1006,
				rightNeighborID = 1011
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 196, mapY + 352, 36, 76), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11067"))
			{
				myID = 1005,
				upNeighborID = 1001,
				downNeighborID = 1031,
				rightNeighborID = 1006,
				leftNeighborID = 1030
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 420, mapY + 392, 76, 40), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11068") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11069"))
			{
				myID = 1006,
				leftNeighborID = 1005,
				downNeighborID = 1007,
				upNeighborID = 1002,
				rightNeighborID = 1008
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 452, mapY + 436, 32, 24), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11070"))
			{
				myID = 1007,
				upNeighborID = 1006,
				downNeighborID = 1033,
				leftNeighborID = 1005,
				rightNeighborID = 1008
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 612, mapY + 396, 36, 52), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11071") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11072"))
			{
				myID = 1008,
				leftNeighborID = 1006,
				upNeighborID = 1010,
				rightNeighborID = 1009
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 652, mapY + 408, 40, 36), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11073") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11074"))
			{
				myID = 1009,
				leftNeighborID = 1008,
				upNeighborID = 1010,
				rightNeighborID = 1018
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 672, mapY + 340, 44, 60), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11075"))
			{
				myID = 1010,
				leftNeighborID = 1008,
				downNeighborID = 1009,
				rightNeighborID = 1014,
				upNeighborID = 1011
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 680, mapY + 304, 16, 32), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11077"))
			{
				myID = 1011,
				leftNeighborID = 1004,
				rightNeighborID = 1012,
				downNeighborID = 1010,
				upNeighborID = 1032
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 696, mapY + 296, 28, 40), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11078") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11079") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11080"))
			{
				myID = 1012,
				leftNeighborID = 1011,
				downNeighborID = 1014,
				rightNeighborID = 1021,
				upNeighborID = 1032
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 852, mapY + 388, 80, 36), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11081") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11082"))
			{
				myID = 1013,
				upNeighborID = 1027,
				rightNeighborID = 1016,
				downNeighborID = 1017,
				leftNeighborID = 1015
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 716, mapY + 352, 28, 40), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11083") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11084"))
			{
				myID = 1014,
				leftNeighborID = 1010,
				rightNeighborID = 1020,
				downNeighborID = 1019,
				upNeighborID = 1012
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 768, mapY + 388, 44, 56), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11085"))
			{
				myID = 1015,
				leftNeighborID = 1019,
				upNeighborID = 1020,
				rightNeighborID = 1013,
				downNeighborID = 1017
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 892, mapY + 416, 32, 28), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11086") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11087"))
			{
				myID = 1016,
				downNeighborID = 1017,
				leftNeighborID = 1013,
				upNeighborID = 1027,
				rightNeighborID = -1
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 824, mapY + 564, 28, 20), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11088"))
			{
				myID = 1017,
				downNeighborID = 1028,
				upNeighborID = 1015,
				rightNeighborID = -1
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 696, mapY + 448, 24, 20), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11089"))
			{
				myID = 1018,
				downNeighborID = 1017,
				rightNeighborID = 1019,
				upNeighborID = 1014,
				leftNeighborID = 1009
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 724, mapY + 424, 40, 32), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11090"))
			{
				myID = 1019,
				leftNeighborID = 1018,
				upNeighborID = 1014,
				rightNeighborID = 1015,
				downNeighborID = 1017
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 780, mapY + 360, 24, 20), Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade") ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouse") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouseHomeOf")) : Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11091"))
			{
				myID = 1020,
				upNeighborID = 1021,
				leftNeighborID = 1014,
				downNeighborID = 1015,
				rightNeighborID = 1027
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 748, mapY + 316, 36, 36), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11092") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11093"))
			{
				myID = 1021,
				rightNeighborID = 1027,
				downNeighborID = 1020,
				leftNeighborID = 1012,
				upNeighborID = 1032
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 732, mapY + 148, 48, 32), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11094") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11095") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11096"))
			{
				myID = 1022,
				downNeighborID = 1032,
				leftNeighborID = 1003,
				upNeighborID = 1034,
				rightNeighborID = 1023
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 784, mapY + 128, 12, 16), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11097"))
			{
				myID = 1023,
				leftNeighborID = 1034,
				downNeighborID = 1022,
				rightNeighborID = 1024
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 880, mapY + 96, 16, 24), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11098"))
			{
				myID = 1024,
				leftNeighborID = 1023,
				rightNeighborID = 1025,
				downNeighborID = 1027
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 900, mapY + 108, 32, 36), (Game1.stats.DaysPlayed >= 5) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11099") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11100")) : "???")
			{
				myID = 1025,
				leftNeighborID = 1024,
				rightNeighborID = 1026,
				downNeighborID = 1027
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 968, mapY + 116, 88, 76), Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11103") : "???")
			{
				myID = 1026,
				leftNeighborID = 1025,
				downNeighborID = 1027
			});
			string jojaHoverText2 = "";
			jojaHoverText2 = ((Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Map") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Hours")) : ((!Utility.HasAnyPlayerSeenEvent(191393)) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11105") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11106")) : Game1.content.LoadString("Strings\\StringsFromCSFiles:AbandonedJojaMart")));
			points.Add(new ClickableComponent(new Rectangle(mapX + 872, mapY + 280, 52, 52), jojaHoverText2)
			{
				myID = 1027,
				upNeighborID = 1025,
				leftNeighborID = 1021,
				downNeighborID = 1013
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 844, mapY + 608, 36, 40), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107") + Environment.NewLine + (Game1.player.mailReceived.Contains("willyHours") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11108_newHours") : Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11108")))
			{
				myID = 1028,
				upNeighborID = 1017,
				rightNeighborID = (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("Visited_Island") ? 1035 : (-1))
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 576, mapY + 60, 48, 36), Game1.isLocationAccessible("Railroad") ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11111")) : "???")
			{
				myID = 1029,
				rightNeighborID = 1034,
				downNeighborID = 1003,
				leftNeighborID = 1001
			});
			points.Add(new ClickableComponent(new Rectangle(mapX, mapY + 272, 196, 176), Game1.player.mailReceived.Contains("beenToWoods") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114") : "???")
			{
				myID = 1030,
				upNeighborID = 1001,
				rightNeighborID = 1005
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 260, mapY + 572, 20, 20), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11116"))
			{
				myID = 1031,
				rightNeighborID = 1033,
				upNeighborID = 1005
			});
			string ccText2 = "";
			ccText2 = ((!Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") || !Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117") : (Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Map") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MovieTheater_Hours")));
			points.Add(new ClickableComponent(new Rectangle(mapX + 692, mapY + 204, 44, 36), ccText2)
			{
				myID = 1032,
				downNeighborID = 1012,
				upNeighborID = 1022,
				leftNeighborID = 1004
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 380, mapY + 596, 24, 32), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11118"))
			{
				myID = 1033,
				leftNeighborID = 1031,
				rightNeighborID = 1017,
				upNeighborID = 1007
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 644, mapY + 64, 16, 8), Game1.isLocationAccessible("Railroad") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11119") : "???")
			{
				myID = 1034,
				leftNeighborID = 1029,
				rightNeighborID = 1023,
				downNeighborID = 1022
			});
			points.Add(new ClickableComponent(new Rectangle(mapX + 728, mapY + 652, 28, 28), Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11122")));
			if (Game1.MasterPlayer.hasOrWillReceiveMail("Visited_Island"))
			{
				drawIsland = true;
				points.Add(new ClickableComponent(new Rectangle(mapX + 1040, mapY + 600, 160, 120), Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName"))
				{
					myID = 1035,
					downNeighborID = -1,
					upNeighborID = 1013,
					leftNeighborID = 1028
				});
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(1002);
			snapCursorToCurrentSnappedComponent();
		}

		public Vector2 getPlayerMapPosition(Farmer player)
		{
			Vector2 playerMapPosition = new Vector2(-999f, -999f);
			if (player.currentLocation == null)
			{
				return playerMapPosition;
			}
			string replacedName = player.currentLocation.Name;
			if (replacedName.StartsWith("UndergroundMine") || replacedName == "Mine")
			{
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11098");
				if (player.currentLocation is MineShaft && (player.currentLocation as MineShaft).mineLevel > 120 && (player.currentLocation as MineShaft).mineLevel != 77377)
				{
					replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062");
				}
			}
			if (player.currentLocation is IslandLocation)
			{
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:IslandName");
			}
			switch (player.currentLocation.Name)
			{
			case "Woods":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11114");
				break;
			case "FishShop":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11107");
				break;
			case "Desert":
			case "SkullCave":
			case "Club":
			case "SandyHouse":
			case "SandyShop":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11062");
				break;
			case "AnimalShop":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11068");
				break;
			case "HarveyRoom":
			case "Hospital":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11076") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11077");
				break;
			case "SeedShop":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11078") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11079") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11080");
				break;
			case "ManorHouse":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11085");
				break;
			case "WizardHouse":
			case "WizardHouseBasement":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11067");
				break;
			case "BathHouse_Pool":
			case "BathHouse_Entry":
			case "BathHouse_MensLocker":
			case "BathHouse_WomensLocker":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11110") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11111");
				break;
			case "AdventureGuild":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11099");
				break;
			case "SebastianRoom":
			case "ScienceHouse":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11094") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11095");
				break;
			case "JoshHouse":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11092") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11093");
				break;
			case "ElliottHouse":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11088");
				break;
			case "ArchaeologyHouse":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11086");
				break;
			case "WitchWarpCave":
			case "Railroad":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11119");
				break;
			case "CommunityCenter":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11117");
				break;
			case "Trailer_Big":
				replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.PamHouse");
				break;
			case "Temp":
				if (player.currentLocation.Map.Id.Contains("Town"))
				{
					replacedName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
				}
				break;
			}
			foreach (ClickableComponent c in points)
			{
				string cNameNoSpaces = c.name.Replace(" ", "");
				int indexOfNewLine = c.name.IndexOf(Environment.NewLine);
				int indexOfNewLineNoSpaces = cNameNoSpaces.IndexOf(Environment.NewLine);
				string replacedNameSubstring = replacedName.Substring(0, replacedName.Contains(Environment.NewLine) ? replacedName.IndexOf(Environment.NewLine) : replacedName.Length);
				if (c.name.Equals(replacedName) || cNameNoSpaces.Equals(replacedName) || (c.name.Contains(Environment.NewLine) && (c.name.Substring(0, indexOfNewLine).Equals(replacedNameSubstring) || cNameNoSpaces.Substring(0, indexOfNewLineNoSpaces).Equals(replacedNameSubstring))))
				{
					playerMapPosition = new Vector2(c.bounds.Center.X, c.bounds.Center.Y);
					if (player.IsLocalPlayer)
					{
						playerLocationName = (c.name.Contains(Environment.NewLine) ? c.name.Substring(0, c.name.IndexOf(Environment.NewLine)) : c.name);
					}
					return playerMapPosition;
				}
			}
			int x = player.getTileX();
			int y = player.getTileY();
			switch ((string)player.currentLocation.name)
			{
			case "Saloon":
				if (player.IsLocalPlayer)
				{
					playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11172");
				}
				break;
			case "Beach":
				if (player.IsLocalPlayer)
				{
					playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11174");
				}
				playerMapPosition = new Vector2(mapX + 808, mapY + 564);
				break;
			case "Mountain":
				if (x < 38)
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11176");
					}
					playerMapPosition = new Vector2(mapX + 740, mapY + 144);
				}
				else if (x < 96)
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11177");
					}
					playerMapPosition = new Vector2(mapX + 880, mapY + 152);
				}
				else
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11178");
					}
					playerMapPosition = new Vector2(mapX + 1012, mapY + 160);
				}
				break;
			case "Tunnel":
			case "Backwoods":
				playerMapPosition = new Vector2(mapX + 436, mapY + 188);
				if (player.IsLocalPlayer)
				{
					playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11180");
				}
				break;
			case "FarmHouse":
			case "Barn":
			case "Big Barn":
			case "Deluxe Barn":
			case "Coop":
			case "Big Coop":
			case "Deluxe Coop":
			case "Cabin":
			case "Slime Hutch":
			case "Greenhouse":
			case "FarmCave":
			case "Shed":
			case "Big Shed":
			case "Farm":
				playerMapPosition = new Vector2(mapX + 384, mapY + 288);
				if (player.IsLocalPlayer)
				{
					playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", player.farmName);
				}
				break;
			case "Forest":
				if (y > 51)
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11186");
					}
					playerMapPosition = new Vector2(mapX + 280, mapY + 540);
				}
				else if (x < 58)
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11186");
					}
					playerMapPosition = new Vector2(mapX + 252, mapY + 416);
				}
				else
				{
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11188");
					}
					playerMapPosition = new Vector2(mapX + 436, mapY + 428);
				}
				break;
			case "Town":
				if (x > 84 && y < 68)
				{
					playerMapPosition = new Vector2(mapX + 900, mapY + 324);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (x > 80 && y >= 68)
				{
					playerMapPosition = new Vector2(mapX + 880, mapY + 432);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (y <= 42)
				{
					playerMapPosition = new Vector2(mapX + 712, mapY + 256);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (y > 42 && y < 76)
				{
					playerMapPosition = new Vector2(mapX + 700, mapY + 352);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else
				{
					playerMapPosition = new Vector2(mapX + 728, mapY + 436);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				break;
			case "Temp":
				if (!player.currentLocation.Map.Id.Contains("Town"))
				{
					break;
				}
				if (x > 84 && y < 68)
				{
					playerMapPosition = new Vector2(mapX + 900, mapY + 324);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (x > 80 && y >= 68)
				{
					playerMapPosition = new Vector2(mapX + 880, mapY + 432);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (y <= 42)
				{
					playerMapPosition = new Vector2(mapX + 712, mapY + 256);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else if (y > 42 && y < 76)
				{
					playerMapPosition = new Vector2(mapX + 700, mapY + 352);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				else
				{
					playerMapPosition = new Vector2(mapX + 728, mapY + 436);
					if (player.IsLocalPlayer)
					{
						playerLocationName = Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11190");
					}
				}
				break;
			}
			return playerMapPosition;
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
			if (Game1.options.doesInputListContain(Game1.options.mapButton, key))
			{
				exitThisMenu();
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableComponent c in points)
			{
				if (c.containsPoint(x, y))
				{
					string name = c.name;
					if (name == "Lonely Stone")
					{
						Game1.playSound("stoneCrack");
					}
				}
			}
			if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is GameMenu)
			{
				(Game1.activeClickableMenu as GameMenu).changeTab((Game1.activeClickableMenu as GameMenu).lastOpenedNonMapTab);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			descriptionText = "";
			hoverText = "";
			foreach (ClickableComponent c in points)
			{
				if (c.containsPoint(x, y))
				{
					hoverText = c.name;
					break;
				}
			}
		}

		protected virtual void drawMiniPortraits(SpriteBatch b)
		{
			Dictionary<Vector2, int> usedPositions = new Dictionary<Vector2, int>();
			foreach (Farmer player in Game1.getOnlineFarmers())
			{
				Vector2 pos = getPlayerMapPosition(player) - new Vector2(32f, 32f);
				int count = 0;
				usedPositions.TryGetValue(pos, out count);
				usedPositions[pos] = count + 1;
				pos += new Vector2(48 * (count % 2), 48 * (count / 2));
				player.FarmerRenderer.drawMiniPortrat(b, pos, 0.00011f, 4f, 2, player);
			}
		}

		public override void draw(SpriteBatch b)
		{
			float scroll_draw_y = yPositionOnScreen + height + 32 + 16;
			float scroll_draw_bottom = scroll_draw_y + 80f;
			if (scroll_draw_bottom > (float)Game1.uiViewport.Height)
			{
				scroll_draw_y -= scroll_draw_bottom - (float)Game1.uiViewport.Height;
			}
			int boxY = mapY - 96;
			int mY = mapY;
			Game1.drawDialogueBox(mapX - 32, boxY, (map.Bounds.Width + 16) * 4, 848, speaker: false, drawOnlyBox: true);
			b.Draw(map, new Vector2(mapX, mY), new Rectangle(0, 0, 300, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.86f);
			switch (Game1.whichFarm)
			{
			case 1:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(0, 180, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			case 2:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(131, 180, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			case 3:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(0, 241, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			case 4:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(131, 241, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			case 5:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(0, 302, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			case 6:
				b.Draw(map, new Vector2(mapX, mY + 172), new Rectangle(131, 302, 131, 61), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
				break;
			}
			if (drawPamHouseUpgrade)
			{
				b.Draw(map, new Vector2(mapX + 780, mapY + 348), new Rectangle(263, 181, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawMovieTheater)
			{
				b.Draw(map, new Vector2(mapX + 852, mapY + 280), new Rectangle(271, 181, 29, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawMovieTheaterJoja)
			{
				b.Draw(map, new Vector2(mapX + 684, mapY + 192), new Rectangle(276, 181, 13, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			if (drawIsland)
			{
				b.Draw(map, new Vector2(mapX + 1040, mapY + 600), new Rectangle(208, 363, 40, 30), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.861f);
			}
			drawMiniPortraits(b);
			if (playerLocationName != null)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, playerLocationName, xPositionOnScreen + width / 2, (int)scroll_draw_y);
			}
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
	}
}
