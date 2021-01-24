using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StardewValley.Menus
{
	public class FarmhandMenu : LoadGameMenu
	{
		public class FarmhandSlot : SaveFileSlot
		{
			protected new FarmhandMenu menu;

			protected bool _belongsToAnotherPlayer;

			public bool BelongsToAnotherPlayer()
			{
				if (Game1.game1 != null && !Game1.game1.IsMainInstance)
				{
					return false;
				}
				return _belongsToAnotherPlayer;
			}

			public FarmhandSlot(FarmhandMenu menu, Farmer farmer)
				: base(menu, farmer)
			{
				this.menu = menu;
				if (Program.sdk.Networking != null)
				{
					string local_user_id = Program.sdk.Networking.GetUserID();
					if (local_user_id != "" && farmer != null && farmer.userID.Value != "" && local_user_id != (string)farmer.userID)
					{
						_belongsToAnotherPlayer = true;
					}
				}
			}

			public override void Activate()
			{
				if (menu.client != null)
				{
					Game1.loadForNewGame();
					Game1.player = Farmer;
					menu.client.availableFarmhands = null;
					menu.client.sendPlayerIntroduction();
					menu.approvingFarmhand = true;
					menu.menuSlots.Clear();
					Game1.gameMode = 6;
				}
			}

			public override float getSlotAlpha()
			{
				if (BelongsToAnotherPlayer())
				{
					return 0.5f;
				}
				return base.getSlotAlpha();
			}

			protected override void drawSlotName(SpriteBatch b, int i)
			{
				if ((bool)Farmer.isCustomized)
				{
					base.drawSlotName(b, i);
					return;
				}
				string slotName = Game1.content.LoadString("Strings\\UI:CoopMenu_NewFarmhand");
				SpriteText.drawString(b, slotName, menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36);
			}

			protected override void drawSlotShadow(SpriteBatch b, int i)
			{
				if ((bool)Farmer.isCustomized)
				{
					base.drawSlotShadow(b, i);
				}
			}

			protected override void drawSlotFarmer(SpriteBatch b, int i)
			{
				if ((bool)Farmer.isCustomized)
				{
					base.drawSlotFarmer(b, i);
				}
			}

			protected override void drawSlotTimer(SpriteBatch b, int i)
			{
				if ((bool)Farmer.isCustomized)
				{
					base.drawSlotTimer(b, i);
				}
			}

			protected override void drawSlotMoney(SpriteBatch b, int i)
			{
			}
		}

		public bool gettingFarmhands;

		public bool approvingFarmhand;

		public Client client;

		public FarmhandMenu()
			: this(null)
		{
		}

		public FarmhandMenu(Client client)
		{
			if (client == null && Program.sdk.Networking != null)
			{
				client = Program.sdk.Networking.GetRequestedClient();
			}
			this.client = client;
			if (client != null)
			{
				gettingFarmhands = true;
			}
		}

		public override bool readyToClose()
		{
			return !loading;
		}

		protected override bool hasDeleteButtons()
		{
			return false;
		}

		protected override void startListPopulation()
		{
		}

		public override void UpdateButtons()
		{
			base.UpdateButtons();
			if (LocalMultiplayer.IsLocalMultiplayer() && !Game1.game1.IsMainInstance && backButton != null)
			{
				backButton.visible = false;
			}
		}

		protected override bool checkListPopulation()
		{
			if (client != null && (gettingFarmhands || approvingFarmhand) && (client.availableFarmhands != null || client.connectionMessage != null))
			{
				timerToLoad = 0;
				selected = -1;
				loading = false;
				gettingFarmhands = false;
				if (menuSlots == null)
				{
					menuSlots = new List<MenuSlot>();
				}
				else
				{
					menuSlots.Clear();
				}
				if (client.availableFarmhands == null)
				{
					approvingFarmhand = true;
				}
				else
				{
					approvingFarmhand = false;
					menuSlots.AddRange(client.availableFarmhands.Select((Farmer farmer) => new FarmhandSlot(this, farmer)));
				}
				if (Game1.activeClickableMenu is TitleMenu)
				{
					Game1.gameMode = 0;
				}
				else if (!Game1.game1.IsMainInstance)
				{
					Game1.gameMode = 0;
				}
				UpdateButtons();
				if (Game1.options.SnappyMenus)
				{
					populateClickableComponentList();
					snapToDefaultClickableComponent();
				}
			}
			return false;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B && readyToClose())
			{
				exitThisMenu();
			}
			base.receiveGamePadButton(b);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			for (int i = 0; i < slotButtons.Count; i++)
			{
				if (slotButtons[i].containsPoint(x, y) && i < MenuSlots.Count && MenuSlots[currentItemIndex + i] is FarmhandSlot && (MenuSlots[currentItemIndex + i] as FarmhandSlot).BelongsToAnotherPlayer())
				{
					Game1.playSound("cancel");
					return;
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			if (!(hoverText == ""))
			{
				return;
			}
			for (int i = 0; i < slotButtons.Count; i++)
			{
				if (currentItemIndex + i < MenuSlots.Count && slotButtons[i].containsPoint(x, y))
				{
					MenuSlot slot = MenuSlots[currentItemIndex + i];
					if (slot is FarmhandSlot && (slot as FarmhandSlot).BelongsToAnotherPlayer())
					{
						hoverText = Game1.content.LoadString("Strings\\UI:Farmhand_Locked");
					}
				}
			}
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (b != null && (b.myID == 800 || b.myID == 801) && menuSlots.Count <= 4)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		public override void update(GameTime time)
		{
			if (client != null)
			{
				if (!client.connectionStarted && drawn)
				{
					client.connect();
				}
				if (client.connectionStarted)
				{
					client.receiveMessages();
				}
				if (client.readyToPlay)
				{
					Game1.gameMode = 3;
					loadClientOptions();
					if (Game1.activeClickableMenu is FarmhandMenu || (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is FarmhandMenu))
					{
						Game1.exitActiveMenu();
					}
				}
				else if (client.timedOut)
				{
					if (approvingFarmhand)
					{
						Game1.multiplayer.clientRemotelyDisconnected(Multiplayer.IsTimeout(client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : client.pendingDisconnect);
					}
					else
					{
						menuSlots.RemoveAll((MenuSlot slot) => slot is FarmhandSlot);
					}
				}
			}
			base.update(time);
		}

		private void loadClientOptions()
		{
			Action action = delegate
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				if (Game1.game1.IsMainInstance)
				{
					Game1.options = startupPreferences.clientOptions;
				}
				else
				{
					Game1.options = new Options();
				}
				Game1.initializeVolumeLevels();
			};
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				Game1.currentSong = Game1.soundBank.GetCue("spring_day_ambient");
				action();
			}
			else
			{
				new Task(action).Start();
			}
		}

		protected override string getStatusText()
		{
			if (client == null)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_NoInvites");
			}
			if (client.timedOut)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			}
			if (client.connectionMessage != null)
			{
				return client.connectionMessage;
			}
			if (gettingFarmhands || approvingFarmhand)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_Connecting");
			}
			if (menuSlots.Count == 0)
			{
				return Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots");
			}
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			if (client != null && disposing && Game1.client != client)
			{
				Multiplayer.LogDisconnect(Multiplayer.IsTimeout(client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : Multiplayer.DisconnectType.ExitedToMainMenu_FromFarmhandSelect);
				client.disconnect();
				if (!Game1.game1.IsMainInstance)
				{
					GameRunner.instance.RemoveGameInstance(Game1.game1);
				}
			}
			base.Dispose(disposing);
		}
	}
}
