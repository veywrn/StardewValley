using Lidgren.Network;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StardewValley.Network
{
	public class GameServer : IGameServer, IBandwidthMonitor
	{
		protected List<Server> servers = new List<Server>();

		private Dictionary<Action, Func<bool>> pendingGameAvailableActions = new Dictionary<Action, Func<bool>>();

		protected Dictionary<string, Action> _pendingFarmhandSelections = new Dictionary<string, Action>();

		private List<Action> completedPendingActions = new List<Action>();

		private List<string> bannedUsers = new List<string>();

		protected bool _wasConnected;

		protected bool _isLocalMultiplayerInitiatedServer;

		public int connectionsCount => servers.Sum((Server s) => s.connectionsCount);

		public BandwidthLogger BandwidthLogger
		{
			get
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.BandwidthLogger;
					}
				}
				return null;
			}
		}

		public bool LogBandwidth
		{
			get
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						return server.LogBandwidth;
					}
				}
				return false;
			}
			set
			{
				foreach (Server server in servers)
				{
					if (server.connectionsCount > 0)
					{
						server.LogBandwidth = value;
						break;
					}
				}
			}
		}

		public GameServer(bool local_multiplayer = false)
		{
			servers.Add(Game1.multiplayer.InitServer(new LidgrenServer(this)));
			_isLocalMultiplayerInitiatedServer = local_multiplayer;
			if (!_isLocalMultiplayerInitiatedServer && Program.sdk.Networking != null)
			{
				servers.Add(Program.sdk.Networking.CreateServer(this));
			}
		}

		public bool isConnectionActive(string connectionId)
		{
			foreach (Server server in servers)
			{
				if (server.isConnectionActive(connectionId))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void onConnect(string connectionID)
		{
			UpdateLocalOnlyFlag();
		}

		public virtual void onDisconnect(string connectionID)
		{
			if (_pendingFarmhandSelections.ContainsKey(connectionID))
			{
				Console.WriteLine("Removed pending farmhand selection for invalidated connection " + connectionID);
				if (pendingGameAvailableActions.ContainsKey(_pendingFarmhandSelections[connectionID]))
				{
					pendingGameAvailableActions.Remove(_pendingFarmhandSelections[connectionID]);
				}
				_pendingFarmhandSelections.Remove(connectionID);
			}
			UpdateLocalOnlyFlag();
		}

		public bool IsLocalMultiplayerInitiatedServer()
		{
			return _isLocalMultiplayerInitiatedServer;
		}

		public virtual void UpdateLocalOnlyFlag()
		{
			if (!Game1.game1.IsMainInstance)
			{
				return;
			}
			bool local_only = true;
			HashSet<long> local_clients = new HashSet<long>();
			GameRunner.instance.ExecuteForInstances(delegate
			{
				Client client = Game1.client;
				if (client == null && Game1.activeClickableMenu is FarmhandMenu)
				{
					client = (Game1.activeClickableMenu as FarmhandMenu).client;
				}
				if (client is LidgrenClient)
				{
					local_clients.Add((client as LidgrenClient).client.UniqueIdentifier);
				}
			});
			foreach (Server server in servers)
			{
				if (server is LidgrenServer)
				{
					foreach (NetConnection connection in (server as LidgrenServer).server.Connections)
					{
						if (!local_clients.Contains(connection.RemoteUniqueIdentifier))
						{
							local_only = false;
							break;
						}
					}
				}
				else if (server.connectionsCount > 0)
				{
					local_only = false;
					break;
				}
				if (!local_only)
				{
					break;
				}
			}
			if (Game1.hasLocalClientsOnly != local_only)
			{
				Game1.hasLocalClientsOnly = local_only;
				if (Game1.hasLocalClientsOnly)
				{
					Console.WriteLine("Game has only local clients.");
				}
				else
				{
					Console.WriteLine("Game has remote clients.");
				}
			}
		}

		public string getInviteCode()
		{
			foreach (Server server in servers)
			{
				string code = server.getInviteCode();
				if (code != null)
				{
					return code;
				}
			}
			return null;
		}

		public string getUserName(long farmerId)
		{
			foreach (Server server in servers)
			{
				string name = server.getUserName(farmerId);
				if (name != null)
				{
					return name;
				}
			}
			return null;
		}

		public float getPingToClient(long farmerId)
		{
			foreach (Server server in servers)
			{
				if (server.getPingToClient(farmerId) != -1f)
				{
					return server.getPingToClient(farmerId);
				}
			}
			return -1f;
		}

		protected void initialize()
		{
			foreach (Server server in servers)
			{
				server.initialize();
			}
			whenGameAvailable(updateLobbyData);
		}

		public void setPrivacy(ServerPrivacy privacy)
		{
			foreach (Server server in servers)
			{
				server.setPrivacy(privacy);
			}
		}

		public void stopServer()
		{
			if (Game1.chatBox != null)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_DisablingServer"));
			}
			foreach (Server server in servers)
			{
				server.stopServer();
			}
		}

		public void receiveMessages()
		{
			foreach (Server server in servers)
			{
				server.receiveMessages();
			}
			completedPendingActions.Clear();
			foreach (Action action2 in pendingGameAvailableActions.Keys)
			{
				if (pendingGameAvailableActions[action2]())
				{
					action2();
					completedPendingActions.Add(action2);
				}
			}
			foreach (Action action in completedPendingActions)
			{
				pendingGameAvailableActions.Remove(action);
			}
			completedPendingActions.Clear();
			if (Game1.chatBox == null)
			{
				return;
			}
			bool any_server_connected = anyServerConnected();
			if (_wasConnected != any_server_connected)
			{
				_wasConnected = any_server_connected;
				if (_wasConnected)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_StartingServer"));
				}
			}
		}

		public void sendMessage(long peerId, OutgoingMessage message)
		{
			foreach (Server server in servers)
			{
				server.sendMessage(peerId, message);
			}
		}

		public bool canAcceptIPConnections()
		{
			return servers.Select((Server s) => s.canAcceptIPConnections()).Aggregate(seed: false, (bool a, bool b) => a | b);
		}

		public bool canOfferInvite()
		{
			return servers.Select((Server s) => s.canOfferInvite()).Aggregate(seed: false, (bool a, bool b) => a | b);
		}

		public void offerInvite()
		{
			foreach (Server s in servers)
			{
				if (s.canOfferInvite())
				{
					s.offerInvite();
				}
			}
		}

		public bool anyServerConnected()
		{
			foreach (Server server in servers)
			{
				if (server.connected())
				{
					return true;
				}
			}
			return false;
		}

		public bool connected()
		{
			foreach (Server server in servers)
			{
				if (!server.connected())
				{
					return false;
				}
			}
			return true;
		}

		public void sendMessage(long peerId, byte messageType, Farmer sourceFarmer, params object[] data)
		{
			sendMessage(peerId, new OutgoingMessage(messageType, sourceFarmer, data));
		}

		public void sendMessages()
		{
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				foreach (OutgoingMessage message in farmer.messageQueue)
				{
					sendMessage(farmer.UniqueMultiplayerID, message);
				}
				farmer.messageQueue.Clear();
			}
		}

		public void startServer()
		{
			_wasConnected = false;
			Console.WriteLine("Starting server. Protocol version: 1.5.4");
			initialize();
			if (Game1.netWorldState == null)
			{
				Game1.netWorldState = new NetRoot<IWorldState>(new NetWorldState());
			}
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			Game1.netWorldState.Value.UpdateFromGame1();
		}

		public void initializeHost()
		{
			if (Game1.serverHost == null)
			{
				Game1.serverHost = new NetFarmerRoot();
			}
			Game1.serverHost.Value = Game1.player;
			Game1.serverHost.MarkClean();
			Game1.serverHost.Clock.InterpolationTicks = Game1.multiplayer.defaultInterpolationTicks;
		}

		public void sendServerIntroduction(long peer)
		{
			sendMessage(peer, new OutgoingMessage(1, Game1.serverHost.Value, Game1.multiplayer.writeObjectFullBytes(Game1.serverHost, peer), Game1.multiplayer.writeObjectFullBytes(Game1.player.teamRoot, peer), Game1.multiplayer.writeObjectFullBytes(Game1.netWorldState, peer)));
			foreach (KeyValuePair<long, NetRoot<Farmer>> r in Game1.otherFarmers.Roots)
			{
				if (r.Key != Game1.player.UniqueMultiplayerID && r.Key != peer)
				{
					sendMessage(peer, new OutgoingMessage(2, r.Value.Value, getUserName(r.Value.Value.UniqueMultiplayerID), Game1.multiplayer.writeObjectFullBytes(r.Value, peer)));
				}
			}
		}

		public void kick(long disconnectee)
		{
			foreach (Server server in servers)
			{
				server.kick(disconnectee);
			}
		}

		public string ban(long farmerId)
		{
			string userId = null;
			foreach (Server server in servers)
			{
				userId = server.getUserId(farmerId);
				if (userId != null)
				{
					break;
				}
			}
			if (userId != null && !Game1.bannedUsers.ContainsKey(userId))
			{
				string userName = Game1.multiplayer.getUserName(farmerId);
				if (userName == "" || userName == userId)
				{
					userName = null;
				}
				Game1.bannedUsers.Add(userId, userName);
				kick(farmerId);
				return userId;
			}
			return null;
		}

		public void playerDisconnected(long disconnectee)
		{
			Farmer disconnectedFarmer = null;
			Game1.otherFarmers.TryGetValue(disconnectee, out disconnectedFarmer);
			Game1.multiplayer.playerDisconnected(disconnectee);
			if (disconnectedFarmer != null)
			{
				OutgoingMessage message = new OutgoingMessage(19, disconnectedFarmer);
				foreach (long peer in Game1.otherFarmers.Keys)
				{
					if (peer != disconnectee)
					{
						sendMessage(peer, message);
					}
				}
			}
		}

		public bool isGameAvailable()
		{
			bool inIntro = Game1.currentMinigame is Intro || Game1.Date.DayOfMonth == 0;
			bool isWedding = Game1.CurrentEvent != null && Game1.CurrentEvent.isWedding;
			bool isSleeping = Game1.newDaySync != null && !Game1.newDaySync.hasFinished();
			bool isDemolishing = Game1.player.team.demolishLock.IsLocked();
			if (!Game1.isFestival() && !isWedding && !inIntro && !isSleeping && !isDemolishing)
			{
				return Game1.weddingsToday.Count == 0;
			}
			return false;
		}

		public bool whenGameAvailable(Action action, Func<bool> customAvailabilityCheck = null)
		{
			Func<bool> availabilityCheck = (customAvailabilityCheck != null) ? customAvailabilityCheck : new Func<bool>(isGameAvailable);
			if (availabilityCheck())
			{
				action();
				return true;
			}
			pendingGameAvailableActions.Add(action, availabilityCheck);
			return false;
		}

		private void rejectFarmhandRequest(string userID, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage)
		{
			sendAvailableFarmhands(userID, sendMessage);
			Console.WriteLine("Rejected request for farmhand " + ((farmer.Value != null) ? farmer.Value.UniqueMultiplayerID.ToString() : "???"));
		}

		private IEnumerable<Cabin> cabins()
		{
			if (Game1.getFarm() != null)
			{
				foreach (Building building in Game1.getFarm().buildings)
				{
					if ((int)building.daysOfConstructionLeft <= 0 && building.indoors.Value is Cabin)
					{
						yield return building.indoors.Value as Cabin;
					}
				}
			}
		}

		public bool isUserBanned(string userID)
		{
			return Game1.bannedUsers.ContainsKey(userID);
		}

		private bool authCheck(string userID, Farmer farmhand)
		{
			if (!Game1.options.enableFarmhandCreation && !IsLocalMultiplayerInitiatedServer() && !farmhand.isCustomized)
			{
				return false;
			}
			if (!(userID == "") && !(farmhand.userID.Value == ""))
			{
				return farmhand.userID.Value == userID;
			}
			return true;
		}

		private Cabin findCabin(Farmer farmhand)
		{
			foreach (Cabin cabin in cabins())
			{
				if (cabin.getFarmhand().Value.UniqueMultiplayerID == farmhand.UniqueMultiplayerID)
				{
					return cabin;
				}
			}
			return null;
		}

		private Farmer findOriginalFarmhand(Farmer farmhand)
		{
			return findCabin(farmhand)?.getFarmhand().Value;
		}

		public void checkFarmhandRequest(string userID, string connectionID, NetFarmerRoot farmer, Action<OutgoingMessage> sendMessage, Action approve)
		{
			if (farmer.Value == null)
			{
				rejectFarmhandRequest(userID, farmer, sendMessage);
				return;
			}
			long id = farmer.Value.UniqueMultiplayerID;
			Action check = delegate
			{
				if (_pendingFarmhandSelections.ContainsKey(connectionID))
				{
					_pendingFarmhandSelections.Remove(connectionID);
				}
				Farmer farmer2 = findOriginalFarmhand(farmer.Value);
				if (!isConnectionActive(connectionID))
				{
					Console.WriteLine("Rejected request for connection ID " + connectionID + ": Connection not active.");
				}
				else if (farmer2 == null)
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": doesn't exist");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else if (!authCheck(userID, farmer2))
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": authorization failure");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else if ((Game1.otherFarmers.ContainsKey(id) && !Game1.multiplayer.isDisconnecting(id)) || Game1.serverHost.Value.UniqueMultiplayerID == id)
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": already in use");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else if (findCabin(farmer.Value).isInventoryOpen())
				{
					Console.WriteLine("Rejected request for farmhand " + id + ": inventory in use");
					rejectFarmhandRequest(userID, farmer, sendMessage);
				}
				else
				{
					Console.WriteLine("Approved request for farmhand " + id);
					approve();
					Game1.updateCellarAssignments();
					Game1.multiplayer.addPlayer(farmer);
					Game1.multiplayer.broadcastPlayerIntroduction(farmer);
					sendLocation(id, Game1.getFarm());
					sendLocation(id, Game1.getLocationFromName("FarmHouse"));
					sendLocation(id, Game1.getLocationFromName("Greenhouse"));
					if (farmer.Value.lastSleepLocation != null)
					{
						GameLocation locationFromName = Game1.getLocationFromName(farmer.Value.lastSleepLocation);
						if (locationFromName != null && Game1.isLocationAccessible(locationFromName.Name) && !Game1.multiplayer.isAlwaysActiveLocation(locationFromName))
						{
							sendLocation(id, locationFromName, force_current: true);
						}
					}
					sendServerIntroduction(id);
					updateLobbyData();
				}
			};
			if (!whenGameAvailable(check))
			{
				_pendingFarmhandSelections[connectionID] = check;
				Console.WriteLine("Postponing request for farmhand " + id + " from connection: " + connectionID);
				sendMessage(new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostAvailability"));
			}
		}

		public void sendAvailableFarmhands(string userID, Action<OutgoingMessage> sendMessage)
		{
			List<NetRef<Farmer>> availableFarmhands = new List<NetRef<Farmer>>();
			Game1.getFarm();
			foreach (Cabin cabin in cabins())
			{
				NetRef<Farmer> farmhand2 = cabin.getFarmhand();
				if ((!farmhand2.Value.isActive() || Game1.multiplayer.isDisconnecting(farmhand2.Value.UniqueMultiplayerID)) && !cabin.isInventoryOpen())
				{
					availableFarmhands.Add(farmhand2);
				}
			}
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					writer.Write(Game1.year);
					writer.Write(Utility.getSeasonNumber(Game1.currentSeason));
					writer.Write(Game1.dayOfMonth);
					writer.Write((byte)availableFarmhands.Count);
					foreach (NetRef<Farmer> farmhand in availableFarmhands)
					{
						try
						{
							farmhand.Serializer = SaveGame.farmerSerializer;
							farmhand.WriteFull(writer);
						}
						finally
						{
							farmhand.Serializer = null;
						}
					}
					stream.Seek(0L, SeekOrigin.Begin);
					sendMessage(new OutgoingMessage(9, Game1.player, stream.ToArray()));
				}
			}
		}

		private void sendLocation(long peer, GameLocation location, bool force_current = false)
		{
			sendMessage(peer, 3, Game1.serverHost.Value, force_current, Game1.multiplayer.writeObjectFullBytes(Game1.multiplayer.locationRoot(location), peer));
		}

		private void warpFarmer(Farmer farmer, short x, short y, string name, bool isStructure)
		{
			GameLocation location = Game1.getLocationFromName(name, isStructure);
			if (Game1.IsMasterGame)
			{
				location.hostSetup();
			}
			farmer.currentLocation = location;
			farmer.Position = new Vector2(x * 64, y * 64 - (farmer.Sprite.getHeight() - 32) + 16);
			sendLocation(farmer.UniqueMultiplayerID, location);
		}

		public void processIncomingMessage(IncomingMessage message)
		{
			switch (message.MessageType)
			{
			case 5:
			{
				short x = message.Reader.ReadInt16();
				short y = message.Reader.ReadInt16();
				string name = message.Reader.ReadString();
				bool isStructure = message.Reader.ReadByte() == 1;
				warpFarmer(message.SourceFarmer, x, y, name, isStructure);
				break;
			}
			case 2:
				message.Reader.ReadString();
				Game1.multiplayer.processIncomingMessage(message);
				break;
			case 10:
			{
				long recipient = message.Reader.ReadInt64();
				message.Reader.BaseStream.Position -= 8L;
				if (recipient == Multiplayer.AllPlayers || recipient == Game1.player.UniqueMultiplayerID)
				{
					Game1.multiplayer.processIncomingMessage(message);
				}
				rebroadcastClientMessage(message, recipient);
				break;
			}
			default:
				Game1.multiplayer.processIncomingMessage(message);
				break;
			}
			if (Game1.multiplayer.isClientBroadcastType(message.MessageType))
			{
				rebroadcastClientMessage(message, Multiplayer.AllPlayers);
			}
		}

		private void rebroadcastClientMessage(IncomingMessage message, long peerID)
		{
			OutgoingMessage outMessage = new OutgoingMessage(message);
			foreach (long peer in Game1.otherFarmers.Keys)
			{
				if (peer != message.FarmerID && (peerID == Multiplayer.AllPlayers || peer == peerID))
				{
					sendMessage(peer, outMessage);
				}
			}
		}

		private void setLobbyData(string key, string value)
		{
			foreach (Server server in servers)
			{
				server.setLobbyData(key, value);
			}
		}

		private bool unclaimedFarmhandsExist()
		{
			foreach (Cabin cabin in cabins())
			{
				if (cabin.farmhand.Value == null)
				{
					return true;
				}
				if (cabin.farmhand.Value.userID.Value == "")
				{
					return true;
				}
			}
			return false;
		}

		public void updateLobbyData()
		{
			setLobbyData("farmName", Game1.player.farmName.Value);
			setLobbyData("farmType", Convert.ToString(Game1.whichFarm));
			WorldDate date = new WorldDate(Game1.year, Game1.currentSeason, Game1.dayOfMonth);
			setLobbyData("date", Convert.ToString(date.TotalDays));
			IEnumerable<string> farmhandUserIds = from farmhand in Game1.getAllFarmhands()
				select farmhand.userID.Value;
			setLobbyData("farmhands", string.Join(",", farmhandUserIds.Where((string user) => user != "")));
			setLobbyData("newFarmhands", Convert.ToString(Game1.options.enableFarmhandCreation && unclaimedFarmhandsExist()));
		}
	}
}
