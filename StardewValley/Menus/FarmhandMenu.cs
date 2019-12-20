using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StardewValley.Menus
{
	public class FarmhandMenu : LoadGameMenu
	{
		protected class FarmhandSlot : SaveFileSlot
		{
			protected new FarmhandMenu menu;

			public FarmhandSlot(FarmhandMenu menu, Farmer farmer)
				: base(menu, farmer)
			{
				this.menu = menu;
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
					if (Game1.activeClickableMenu is TitleMenu)
					{
						Game1.gameMode = 6;
					}
				}
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
			return true;
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
				else
				{
					Game1.gameMode = 3;
				}
				UpdateButtons();
			}
			return false;
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
			new Task(delegate
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, updateLanguage: false);
				Game1.options = startupPreferences.clientOptions;
				Game1.initializeVolumeLevels();
			}).Start();
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
				return Game1.content.LoadString(client.connectionMessage);
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
			}
			base.Dispose(disposing);
		}
	}
}
