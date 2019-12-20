using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewValley.Menus
{
	public class CoopMenu : LoadGameMenu
	{
		public enum Tab
		{
			JOIN_TAB,
			HOST_TAB
		}

		protected abstract class CoopMenuSlot : MenuSlot
		{
			protected new CoopMenu menu;

			public CoopMenuSlot(CoopMenu menu)
				: base(menu)
			{
				this.menu = menu;
			}
		}

		protected abstract class LabeledSlot : CoopMenuSlot
		{
			private string message;

			public LabeledSlot(CoopMenu menu, string message)
				: base(menu)
			{
				this.message = message;
			}

			public abstract override void Activate();

			public override void Draw(SpriteBatch b, int i)
			{
				int strWidth = SpriteText.getWidthOfString(message);
				int strHeight = SpriteText.getHeightOfString(message);
				Rectangle bounds = menu.slotButtons[i].bounds;
				int x = bounds.X + (bounds.Width - strWidth) / 2;
				int y = bounds.Y + (bounds.Height - strHeight) / 2;
				SpriteText.drawString(b, message, x, y);
			}
		}

		protected class LanSlot : LabeledSlot
		{
			public LanSlot(CoopMenu menu)
				: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_JoinLANGame"))
			{
			}

			public override void Activate()
			{
				menu.enterIPPressed();
			}
		}

		protected class InviteCodeSlot : LabeledSlot
		{
			public InviteCodeSlot(CoopMenu menu)
				: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode"))
			{
			}

			public override void Activate()
			{
				menu.enterInviteCodePressed();
			}
		}

		protected class HostNewFarmSlot : LabeledSlot
		{
			public HostNewFarmSlot(CoopMenu menu)
				: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_HostNewFarm"))
			{
				ActivateDelay = 2150;
			}

			public override void Activate()
			{
				Game1.resetPlayer();
				TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.HostNewFarm);
				Game1.changeMusicTrack("CloudCountry");
			}
		}

		protected class HostFileSlot : SaveFileSlot
		{
			protected new CoopMenu menu;

			public HostFileSlot(CoopMenu menu, Farmer farmer)
				: base(menu, farmer)
			{
				this.menu = menu;
			}

			public override void Activate()
			{
				Game1.multiplayerMode = 2;
				base.Activate();
			}

			protected override void drawSlotSaveNumber(SpriteBatch b, int i)
			{
			}

			protected override string slotName()
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_HostFile", Farmer.Name, Farmer.farmName.Value);
			}

			protected override string slotSubName()
			{
				return Farmer.Name;
			}

			protected override Vector2 portraitOffset()
			{
				return base.portraitOffset() - new Vector2(32f, 0f);
			}
		}

		protected class FriendFarmData
		{
			public object Lobby;

			public string OwnerName;

			public string FarmName;

			public int FarmType;

			public WorldDate Date;

			public bool PreviouslyJoined;

			public string ProtocolVersion;
		}

		protected class FriendFarmSlot : CoopMenuSlot
		{
			public FriendFarmData Farm;

			public FriendFarmSlot(CoopMenu menu, FriendFarmData farm)
				: base(menu)
			{
				Farm = farm;
			}

			public bool MatchAddress(object Lobby)
			{
				return object.Equals(Farm.Lobby, Lobby);
			}

			public void Update(FriendFarmData newData)
			{
				Farm = newData;
			}

			public override void Activate()
			{
				menu.setMenu(new FarmhandMenu(Program.sdk.Networking.CreateClient(Farm.Lobby)));
			}

			protected virtual string slotName()
			{
				string messageKey = Farm.PreviouslyJoined ? "Strings\\UI:CoopMenu_RevisitFriendFarm" : "Strings\\UI:CoopMenu_JoinFriendFarm";
				return Game1.content.LoadString(messageKey, Farm.FarmName);
			}

			protected virtual void drawSlotName(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, slotName(), menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36);
			}

			protected virtual void drawSlotDate(SpriteBatch b, int i)
			{
				Utility.drawTextWithShadow(b, Farm.Date.Localize(), Game1.dialogueFont, new Vector2(menu.slotButtons[i].bounds.X + 128 + 32, menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor);
			}

			protected virtual void drawSlotFarm(SpriteBatch b, int i)
			{
				Rectangle sourceRect = new Rectangle(22 * (Farm.FarmType % 5), 324 + 21 * (Farm.FarmType / 5), 22, 20);
				Texture2D texture = Game1.mouseCursors;
				Rectangle space = new Rectangle(menu.slotButtons[i].bounds.X, menu.slotButtons[i].bounds.Y, 160, menu.slotButtons[i].bounds.Height);
				Rectangle destRect = new Rectangle(space.X + (space.Width - sourceRect.Width * 4) / 2, space.Y + (space.Height - sourceRect.Height * 4) / 2, sourceRect.Width * 4, sourceRect.Height * 4);
				b.Draw(texture, destRect, sourceRect, Color.White);
			}

			protected virtual void drawSlotOwnerName(SpriteBatch b, int i)
			{
				Utility.drawTextWithShadow(b, Farm.OwnerName, Game1.dialogueFont, new Vector2((float)(menu.slotButtons[i].bounds.X + menu.width - 128) - Game1.dialogueFont.MeasureString(Farm.OwnerName).X, menu.slotButtons[i].bounds.Y + 44), Game1.textColor);
			}

			public override void Draw(SpriteBatch b, int i)
			{
				drawSlotName(b, i);
				drawSlotDate(b, i);
				drawSlotFarm(b, i);
				drawSlotOwnerName(b, i);
			}
		}

		private class LobbyUpdateCallback : LobbyUpdateListener
		{
			private Action<object> callback;

			public LobbyUpdateCallback(Action<object> callback)
			{
				this.callback = callback;
			}

			public void OnLobbyUpdate(object lobby)
			{
				if (callback != null)
				{
					callback(lobby);
				}
			}
		}

		public const int region_refresh = 810;

		public const int region_joinTab = 811;

		public const int region_hostTab = 812;

		public const int region_tabs = 1000;

		protected List<MenuSlot> hostSlots = new List<MenuSlot>();

		public ClickableComponent refreshButton;

		public ClickableComponent joinTab;

		public ClickableComponent hostTab;

		private LobbyUpdateListener lobbyUpdateListener;

		private Tab currentTab;

		private bool smallScreenFormat;

		private bool isSetUp;

		private int updateCounter;

		private double connectionFinishedTimer;

		private StringBuilder _stringBuilder = new StringBuilder();

		protected override List<MenuSlot> MenuSlots
		{
			get
			{
				if (currentTab == Tab.JOIN_TAB)
				{
					return menuSlots;
				}
				if (currentTab == Tab.HOST_TAB)
				{
					return hostSlots;
				}
				return null;
			}
			set
			{
				if (currentTab == Tab.JOIN_TAB)
				{
					menuSlots = value;
				}
				else if (currentTab == Tab.HOST_TAB)
				{
					hostSlots = value;
				}
			}
		}

		public override bool readyToClose()
		{
			if (isSetUp)
			{
				return base.readyToClose();
			}
			return true;
		}

		protected override bool hasDeleteButtons()
		{
			return false;
		}

		protected override void startListPopulation()
		{
			if (Program.sdk.ConnectionFinished)
			{
				connectionFinished();
			}
		}

		protected virtual void connectionFinished()
		{
			isSetUp = true;
			string label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			int width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			Vector2 pos3 = new Vector2(backButton.bounds.Right - width3, backButton.bounds.Y - 128);
			refreshButton = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, width3, 96), "", label3)
			{
				myID = 810,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = 81114
			};
			smallScreenFormat = (Game1.graphics.GraphicsDevice.Viewport.Height < 1080);
			label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			pos3 = (smallScreenFormat ? new Vector2(xPositionOnScreen, yPositionOnScreen) : new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen - 96));
			joinTab = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, width3, smallScreenFormat ? 72 : 64), "", label3)
			{
				myID = 811,
				downNeighborID = -99998,
				rightNeighborID = 812,
				region = 1000
			};
			label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			pos3 = (smallScreenFormat ? new Vector2(joinTab.bounds.Right + ((!smallScreenFormat) ? 4 : 0), yPositionOnScreen) : new Vector2(joinTab.bounds.Right + 4, yPositionOnScreen - 64));
			hostTab = new ClickableComponent(new Rectangle((int)pos3.X, (int)pos3.Y, width3, smallScreenFormat ? 72 : 64), "", label3)
			{
				myID = 812,
				downNeighborID = -99998,
				leftNeighborID = 811,
				rightNeighborID = 800,
				region = 1000
			};
			backButton.upNeighborID = 810;
			hostSlots.Add(new HostNewFarmSlot(this));
			menuSlots.Add(new LanSlot(this));
			if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
			{
				menuSlots.Add(new InviteCodeSlot(this));
			}
			base.startListPopulation();
			UpdateButtons();
			populateClickableComponentList();
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (!IsDoingTask())
			{
				if (b == Buttons.LeftTrigger && joinTab.visible)
				{
					SetTab(Tab.JOIN_TAB);
					setCurrentlySnappedComponentTo(joinTab.myID);
					snapCursorToCurrentSnappedComponent();
				}
				else if (b == Buttons.RightTrigger && hostTab.visible)
				{
					SetTab(Tab.HOST_TAB);
					setCurrentlySnappedComponentTo(hostTab.myID);
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void UpdateButtons()
		{
			base.UpdateButtons();
			foreach (ClickableComponent c in slotButtons)
			{
				if (c.myID == 0)
				{
					if (currentItemIndex == 0)
					{
						c.upNeighborID = 811;
					}
					else
					{
						c.upNeighborID = -7777;
					}
				}
			}
		}

		public override void update(GameTime time)
		{
			updateCounter++;
			if (!isSetUp)
			{
				if (Program.sdk.ConnectionFinished)
				{
					connectionFinishedTimer += time.ElapsedGameTime.TotalSeconds;
					if (connectionFinishedTimer >= 2.0)
					{
						connectionFinished();
					}
				}
			}
			else
			{
				base.update(time);
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			smallScreenFormat = (Game1.graphics.GraphicsDevice.Viewport.Height < 1080);
			string label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			int width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			Vector2 pos3 = smallScreenFormat ? new Vector2(xPositionOnScreen, yPositionOnScreen) : new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen - 96);
			joinTab.bounds.X = (int)pos3.X;
			joinTab.bounds.Y = (int)pos3.Y;
			label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			pos3 = (smallScreenFormat ? new Vector2(joinTab.bounds.Right + ((!smallScreenFormat) ? 4 : 0), yPositionOnScreen) : new Vector2(joinTab.bounds.Right + 4, yPositionOnScreen - 64));
			hostTab.bounds.X = (int)pos3.X;
			hostTab.bounds.Y = (int)pos3.Y;
			label3 = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			width3 = (int)Game1.dialogueFont.MeasureString(label3).X + 64;
			pos3 = new Vector2(backButton.bounds.Right - width3, backButton.bounds.Y - 128);
			refreshButton.bounds.X = (int)pos3.X;
			refreshButton.bounds.Y = (int)pos3.Y;
		}

		protected override void saveFileScanComplete()
		{
			if (Program.sdk.Networking != null)
			{
				lobbyUpdateListener = new LobbyUpdateCallback(onLobbyUpdate);
				Program.sdk.Networking.AddLobbyUpdateListener(lobbyUpdateListener);
				Program.sdk.Networking.RequestFriendLobbyData();
			}
		}

		protected virtual FriendFarmData readLobbyFarmData(object lobby)
		{
			FriendFarmData friendFarmData = new FriendFarmData();
			friendFarmData.Lobby = lobby;
			friendFarmData.Date = new WorldDate();
			friendFarmData.OwnerName = Program.sdk.Networking.GetLobbyOwnerName(lobby);
			friendFarmData.FarmName = Program.sdk.Networking.GetLobbyData(lobby, "farmName");
			friendFarmData.FarmType = Convert.ToInt32(Program.sdk.Networking.GetLobbyData(lobby, "farmType"));
			friendFarmData.Date.TotalDays = Convert.ToInt32(Program.sdk.Networking.GetLobbyData(lobby, "date"));
			friendFarmData.ProtocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
			return friendFarmData;
		}

		protected virtual bool checkFriendFarmCompatibility(FriendFarmData farm)
		{
			if (farm.FarmType < 0 || farm.FarmType > 5)
			{
				return false;
			}
			return farm.ProtocolVersion == Game1.multiplayer.protocolVersion;
		}

		protected virtual void onLobbyUpdate(object lobby)
		{
			try
			{
				Console.WriteLine("Receiving friend lobby data...");
				Console.WriteLine("Owner: " + Program.sdk.Networking.GetLobbyOwnerName(lobby));
				Console.WriteLine("farmName = " + Program.sdk.Networking.GetLobbyData(lobby, "farmName"));
				Console.WriteLine("farmType = " + Program.sdk.Networking.GetLobbyData(lobby, "farmType"));
				Console.WriteLine("date = " + Program.sdk.Networking.GetLobbyData(lobby, "date"));
				Console.WriteLine("protocolVersion = " + Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion"));
				Console.WriteLine("farmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "farmhands"));
				Console.WriteLine("newFarmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
				FriendFarmData farm = readLobbyFarmData(lobby);
				if (checkFriendFarmCompatibility(farm))
				{
					string selfID = Program.sdk.Networking.GetUserID();
					string farmhands = Program.sdk.Networking.GetLobbyData(lobby, "farmhands");
					bool newFarmhands = Convert.ToBoolean(Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
					if (!(farmhands == "") || newFarmhands)
					{
						string[] farmUsers = farmhands.Split(',');
						if (farmUsers.Contains(selfID) || newFarmhands)
						{
							farm.PreviouslyJoined = farmUsers.Contains(selfID);
							foreach (MenuSlot menuSlot in menuSlots)
							{
								FriendFarmSlot farmSlot = menuSlot as FriendFarmSlot;
								if (farmSlot != null && farmSlot.MatchAddress(lobby))
								{
									farmSlot.Update(farm);
									return;
								}
							}
							menuSlots.Add(new FriendFarmSlot(this, farm));
							UpdateButtons();
							populateClickableComponentList();
						}
					}
				}
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region == 1000 && (direction == 2 || direction == 0) && b.region == 1000)
			{
				return false;
			}
			if (a.myID == 810 && direction == 0 && b.region != 900)
			{
				return false;
			}
			if (a.myID == 810 && direction == 1 && b.myID == 81114)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		protected override void addSaveFiles(List<Farmer> files)
		{
			hostSlots.AddRange(files.Where((Farmer file) => file.slotCanHost).Select((Func<Farmer, MenuSlot>)((Farmer file) => new HostFileSlot(this, file))));
			UpdateButtons();
		}

		protected virtual void setMenu(IClickableMenu menu)
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				TitleMenu.subMenu = menu;
			}
			else
			{
				Game1.activeClickableMenu = menu;
			}
		}

		private void enterIPPressed()
		{
			string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterIP");
			setMenu(new TitleTextInputMenu(title, delegate(string address)
			{
				if (address == "")
				{
					address = "localhost";
				}
				setMenu(new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address))));
			}));
		}

		private void enterInviteCodePressed()
		{
			if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
			{
				string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode");
				setMenu(new TitleTextInputMenu(title, delegate(string code)
				{
					object lobbyFromInviteCode = Program.sdk.Networking.GetLobbyFromInviteCode(code);
					if (lobbyFromInviteCode != null)
					{
						Client client = Program.sdk.Networking.CreateClient(lobbyFromInviteCode);
						setMenu(new FarmhandMenu(client));
					}
				}));
			}
		}

		private bool tabClick(int x, int y)
		{
			if (joinTab.visible && joinTab.containsPoint(x, y))
			{
				SetTab(Tab.JOIN_TAB);
				return true;
			}
			if (hostTab.visible && hostTab.containsPoint(x, y))
			{
				SetTab(Tab.HOST_TAB);
				return true;
			}
			return false;
		}

		public virtual void SetTab(Tab new_tab)
		{
			if (currentTab == new_tab)
			{
				return;
			}
			currentTab = new_tab;
			if (!smallScreenFormat)
			{
				if (currentTab == Tab.HOST_TAB)
				{
					hostTab.bounds.Y = yPositionOnScreen - 96;
					joinTab.bounds.Y = yPositionOnScreen - 64;
				}
				else
				{
					hostTab.bounds.Y = yPositionOnScreen - 64;
					joinTab.bounds.Y = yPositionOnScreen - 96;
				}
			}
			Game1.playSound("smallSelect");
			UpdateButtons();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!isSetUp)
			{
				return;
			}
			if (refreshButton.visible && refreshButton.containsPoint(x, y))
			{
				Game1.playSound("bigDeSelect");
				setMenu(new CoopMenu());
			}
			else if (!smallScreenFormat || !tabClick(x, y))
			{
				base.receiveLeftClick(x, y, playSound);
				if (!smallScreenFormat && !loading)
				{
					tabClick(x, y);
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
			if (isSetUp)
			{
				if (refreshButton.visible && refreshButton.containsPoint(x, y))
				{
					refreshButton.scale = 1f;
				}
				else
				{
					refreshButton.scale = 0f;
				}
				if (smallScreenFormat && (hostTab.containsPoint(x, y) || joinTab.containsPoint(x, y)))
				{
					base.performHoverAction(-100, -100);
				}
				else
				{
					base.performHoverAction(x, y);
				}
			}
		}

		protected override string getStatusText()
		{
			return null;
		}

		private void drawTabs(SpriteBatch b)
		{
			if (isSetUp)
			{
				Color selectColor = smallScreenFormat ? Color.Orange : new Color(255, 255, 150);
				Color hoverColor = Color.Yellow;
				Color selectShadow = smallScreenFormat ? Color.DarkOrange : new Color(221, 148, 84);
				Color hoverShadow = Color.DarkGoldenrod;
				if (joinTab.visible)
				{
					bool colorSelect2 = currentTab == Tab.JOIN_TAB;
					bool colorHover2 = currentTab != 0 && joinTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), joinTab.bounds.X, joinTab.bounds.Y, joinTab.bounds.Width, joinTab.bounds.Height + ((!smallScreenFormat) ? 64 : 0), colorSelect2 ? selectColor : (colorHover2 ? hoverColor : Color.White), 1f, drawShadow: false);
					Utility.drawTextWithColoredShadow(b, joinTab.label, Game1.dialogueFont, new Vector2(joinTab.bounds.Center.X, joinTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(joinTab.label) / 2f, Game1.textColor, colorHover2 ? hoverShadow : (colorSelect2 ? selectShadow : new Color(221, 148, 84)), 1.01f);
				}
				if (hostTab.visible)
				{
					bool colorSelect = currentTab == Tab.HOST_TAB;
					bool colorHover = currentTab != Tab.HOST_TAB && hostTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), hostTab.bounds.X, hostTab.bounds.Y, hostTab.bounds.Width, hostTab.bounds.Height + ((!smallScreenFormat) ? 64 : 0), colorSelect ? selectColor : (colorHover ? hoverColor : Color.White), 1f, drawShadow: false);
					Utility.drawTextWithColoredShadow(b, hostTab.label, Game1.dialogueFont, new Vector2(hostTab.bounds.Center.X, hostTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(hostTab.label) / 2f, Game1.textColor, colorHover ? hoverShadow : (colorSelect ? selectShadow : new Color(221, 148, 84)), 1.01f);
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			base.snapToDefaultClickableComponent();
			if (currentlySnappedComponent == null)
			{
				currentlySnappedComponent = getComponentWithID(811);
				snapCursorToCurrentSnappedComponent();
			}
		}

		protected override void drawBefore(SpriteBatch b)
		{
			base.drawBefore(b);
			if (isSetUp && !smallScreenFormat)
			{
				drawTabs(b);
			}
		}

		protected override void drawExtra(SpriteBatch b)
		{
			base.drawExtra(b);
			if (isSetUp)
			{
				if (refreshButton.visible)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), refreshButton.bounds.X, refreshButton.bounds.Y, refreshButton.bounds.Width, refreshButton.bounds.Height, (refreshButton.scale > 0f) ? Color.Wheat : Color.White, 4f);
					Utility.drawTextWithShadow(b, refreshButton.label, Game1.dialogueFont, new Vector2(refreshButton.bounds.Center.X, refreshButton.bounds.Center.Y + 4) - Game1.dialogueFont.MeasureString(refreshButton.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f);
				}
				if (smallScreenFormat)
				{
					drawTabs(b);
				}
			}
		}

		protected override void drawStatusText(SpriteBatch b)
		{
			if (getStatusText() != null)
			{
				base.drawStatusText(b);
			}
			else if (!isSetUp)
			{
				int maxEllipsis = 1 + Program.sdk.ConnectionProgress;
				int ellipsisCount = updateCounter / 5 % maxEllipsis;
				string basicText = Game1.content.LoadString("Strings\\UI:CoopMenu_ConnectingOnlineServices");
				_stringBuilder.Clear();
				_stringBuilder.Append(basicText);
				for (int i = 0; i < ellipsisCount; i++)
				{
					_stringBuilder.Append(".");
				}
				string currentText = _stringBuilder.ToString();
				for (int j = ellipsisCount; j < maxEllipsis; j++)
				{
					_stringBuilder.Append(".");
				}
				int maxWidth = SpriteText.getWidthOfString(_stringBuilder.ToString());
				SpriteText.drawString(b, currentText, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - maxWidth / 2, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (lobbyUpdateListener != null && Program.sdk.Networking != null)
			{
				Program.sdk.Networking.RemoveLobbyUpdateListener(lobbyUpdateListener);
			}
			lobbyUpdateListener = null;
			base.Dispose(disposing);
		}
	}
}
