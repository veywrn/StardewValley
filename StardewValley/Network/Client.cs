using Netcode;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public abstract class Client : IBandwidthMonitor
	{
		public const int connectionTimeout = 45000;

		public bool hasHandshaked;

		public bool readyToPlay;

		public bool timedOut;

		public bool connectionStarted;

		public string serverName = "???";

		public string connectionMessage;

		public Multiplayer.DisconnectType pendingDisconnect;

		protected BandwidthLogger bandwidthLogger;

		protected long? timeoutTime;

		public List<Farmer> availableFarmhands;

		public Dictionary<long, string> userNames = new Dictionary<long, string>();

		public BandwidthLogger BandwidthLogger => bandwidthLogger;

		public bool LogBandwidth
		{
			get
			{
				return bandwidthLogger != null;
			}
			set
			{
				bandwidthLogger = (value ? new BandwidthLogger() : null);
			}
		}

		protected abstract void connectImpl();

		public abstract void disconnect(bool neatly = true);

		protected abstract void receiveMessagesImpl();

		public abstract void sendMessage(OutgoingMessage message);

		public abstract string getUserID();

		protected abstract string getHostUserName();

		public virtual float GetPingToHost()
		{
			return 0f;
		}

		public virtual string getUserName(long farmerId)
		{
			if (farmerId == Game1.serverHost.Value.UniqueMultiplayerID)
			{
				return getHostUserName();
			}
			if (userNames.ContainsKey(farmerId))
			{
				return userNames[farmerId];
			}
			return "?";
		}

		public virtual void connect()
		{
			Console.WriteLine("Starting client. Protocol version: 1.5");
			connectionMessage = null;
			if (!connectionStarted)
			{
				connectionStarted = true;
				connectImpl();
				timeoutTime = DateTime.Now.Ticks / 10000 + 45000;
			}
		}

		public virtual void receiveMessages()
		{
			receiveMessagesImpl();
			if (hasHandshaked)
			{
				timeoutTime = null;
			}
			if (timeoutTime.HasValue && DateTime.Now.Ticks / 10000 >= timeoutTime.Value)
			{
				pendingDisconnect = Multiplayer.DisconnectType.ClientTimeout;
				timedOut = true;
				disconnect(neatly: false);
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ClientTimeout);
			}
			if (bandwidthLogger != null)
			{
				bandwidthLogger.Update();
			}
		}

		protected virtual void processIncomingMessage(IncomingMessage message)
		{
			switch (message.MessageType)
			{
			case 2:
				userNames[message.FarmerID] = message.Reader.ReadString();
				Game1.multiplayer.processIncomingMessage(message);
				break;
			case 16:
				if (message.FarmerID == Game1.serverHost.Value.UniqueMultiplayerID)
				{
					receiveUserNameUpdate(message.Reader);
				}
				break;
			case 9:
				receiveAvailableFarmhands(message.Reader);
				break;
			case 1:
				receiveServerIntroduction(message.Reader);
				break;
			case 3:
				Game1.multiplayer.processIncomingMessage(message);
				break;
			case 11:
				connectionMessage = Game1.content.LoadString(message.Reader.ReadString());
				break;
			default:
				Game1.multiplayer.processIncomingMessage(message);
				break;
			}
		}

		protected virtual void receiveUserNameUpdate(BinaryReader msg)
		{
			long farmerId = msg.ReadInt64();
			string userName = msg.ReadString();
			userNames[farmerId] = userName;
		}

		protected virtual void receiveAvailableFarmhands(BinaryReader msg)
		{
			int year = msg.ReadInt32();
			int season = msg.ReadInt32();
			int dayOfMonth = msg.ReadInt32();
			int count = msg.ReadByte();
			availableFarmhands = new List<Farmer>();
			while (availableFarmhands.Count < count)
			{
				NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
				netFarmerRoot.ReadFull(msg, default(NetVersion));
				netFarmerRoot.MarkReassigned();
				netFarmerRoot.MarkClean();
				Farmer farmhand = netFarmerRoot.Value;
				availableFarmhands.Add(farmhand);
				farmhand.yearForSaveGame = year;
				farmhand.seasonForSaveGame = season;
				farmhand.dayOfMonthForSaveGame = dayOfMonth;
			}
			hasHandshaked = true;
			connectionMessage = null;
			if (!(Game1.activeClickableMenu is TitleMenu) && !(Game1.activeClickableMenu is FarmhandMenu))
			{
				using (List<Farmer>.Enumerator enumerator = availableFarmhands.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Game1.player = enumerator.Current;
						sendPlayerIntroduction();
						return;
					}
				}
				Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerFull);
			}
		}

		public virtual void sendPlayerIntroduction()
		{
			if (getUserID() != "")
			{
				string uid = getUserID();
				Console.WriteLine("sendPlayerIntroduction " + uid);
				Game1.player.userID.Value = uid;
			}
			(Game1.player.NetFields.Root as NetRoot<Farmer>).MarkClean();
			sendMessage(2, Game1.multiplayer.writeObjectFullBytes(Game1.player.NetFields.Root as NetFarmerRoot, null));
		}

		protected virtual void setUpGame()
		{
			Game1.flushLocationLookup();
			Game1.player.updateFriendshipGifts(Game1.Date);
			Game1.gameMode = 3;
			Game1.stats.checkForAchievements();
			Game1.multiplayerMode = 1;
			Game1.client = this;
			Game1.RefreshQuestOfTheDay();
			readyToPlay = true;
			BedFurniture.ApplyWakeUpPosition(Game1.player);
			Game1.fadeClear();
			Game1.currentLocation.resetForPlayerEntry();
			Game1.initializeVolumeLevels();
			Game1.addKentIfNecessary();
			Game1.addBirdieIfNecessary();
			Game1.addParrotBoyIfNecessary();
			if (Game1.MasterPlayer.eventsSeen.Contains(558291) && !Game1.player.songsHeard.Contains("grandpas_theme"))
			{
				Game1.player.songsHeard.Add("grandpas_theme");
			}
			Game1.AddModNPCs();
			Game1.exitActiveMenu();
			if (!Game1.player.isCustomized)
			{
				Game1.activeClickableMenu = new CharacterCustomization(CharacterCustomization.Source.NewFarmhand);
			}
			Game1.player.team.AddAnyBroadcastedMail();
			if (Game1.shouldPlayMorningSong(loading_game: true))
			{
				Game1.playMorningSong();
			}
			for (int i = 1; i < (int)Game1.netWorldState.Value.HighestPlayerLimit; i++)
			{
				if (Game1.getLocationFromName("Cellar" + (i + 1)) == null)
				{
					Game1.locations.Add(new Cellar("Maps\\Cellar", "Cellar" + (i + 1)));
				}
			}
			Game1.player.showToolUpgradeAvailability();
			Game1.dayTimeMoneyBox.questsDirty = true;
			Game1.player.ReequipEnchantments();
			Game1.player.isSitting.Value = false;
		}

		protected virtual void receiveServerIntroduction(BinaryReader msg)
		{
			Game1.otherFarmers.Roots[Game1.player.UniqueMultiplayerID] = (Game1.player.NetFields.Root as NetFarmerRoot);
			NetFarmerRoot f = Game1.multiplayer.readFarmer(msg);
			long id = f.Value.UniqueMultiplayerID;
			Game1.serverHost = f;
			Game1.serverHost.Value.teamRoot = Game1.multiplayer.readObjectFull<FarmerTeam>(msg);
			Game1.otherFarmers.Roots.Add(id, f);
			Game1.player.teamRoot = Game1.serverHost.Value.teamRoot;
			Game1.netWorldState = Game1.multiplayer.readObjectFull<IWorldState>(msg);
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			Game1.netWorldState.Value.WriteToGame1();
			setUpGame();
			if (Game1.chatBox != null)
			{
				Game1.chatBox.listPlayers();
			}
		}

		public virtual void sendMessages()
		{
			if (!(Game1.serverHost == null))
			{
				foreach (OutgoingMessage message in Game1.serverHost.Value.messageQueue)
				{
					sendMessage(message);
				}
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					otherFarmer.Value.messageQueue.Clear();
				}
			}
		}

		public virtual void sendMessage(byte which, params object[] data)
		{
			sendMessage(new OutgoingMessage(which, Game1.player, data));
		}
	}
}
