using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace StardewValley
{
	public class Multiplayer
	{
		public enum PartyWideMessageQueue
		{
			MailForTomorrow,
			SeenMail
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct FarmerRoots : IEnumerable<NetFarmerRoot>, IEnumerable
		{
			public struct Enumerator : IEnumerator<NetFarmerRoot>, IDisposable, IEnumerator
			{
				private Dictionary<long, NetRoot<Farmer>>.Enumerator _enumerator;

				private NetFarmerRoot _current;

				private int _step;

				private bool _done;

				public NetFarmerRoot Current => _current;

				object IEnumerator.Current
				{
					get
					{
						if (_done)
						{
							throw new InvalidOperationException();
						}
						return _current;
					}
				}

				public Enumerator(bool dummy)
				{
					_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
					_current = null;
					_step = 0;
					_done = false;
				}

				public bool MoveNext()
				{
					if (_step == 0)
					{
						_step++;
						if (Game1.serverHost != null)
						{
							_current = Game1.serverHost;
							return true;
						}
					}
					while (_enumerator.MoveNext())
					{
						NetRoot<Farmer> root = _enumerator.Current.Value;
						if (Game1.serverHost == null || root != Game1.serverHost)
						{
							_current = (root as NetFarmerRoot);
							return true;
						}
					}
					_done = true;
					_current = null;
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_enumerator = Game1.otherFarmers.Roots.GetEnumerator();
					_current = null;
					_step = 0;
					_done = false;
				}
			}

			public Enumerator GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}

			IEnumerator<NetFarmerRoot> IEnumerable<NetFarmerRoot>.GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return new Enumerator(dummy: true);
			}
		}

		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct ActiveLocations : IEnumerable<GameLocation>, IEnumerable
		{
			public struct Enumerator : IEnumerator<GameLocation>, IDisposable, IEnumerator
			{
				private NetCollection<Building>.Enumerator _enumerator;

				private GameLocation _current;

				private int _step;

				private bool _done;

				public GameLocation Current => _current;

				object IEnumerator.Current
				{
					get
					{
						if (_done)
						{
							throw new InvalidOperationException();
						}
						return _current;
					}
				}

				public bool MoveNext()
				{
					if (_step == 0)
					{
						_step++;
						if (Game1.currentLocation != null)
						{
							_current = Game1.currentLocation;
							return true;
						}
					}
					if (_step == 1)
					{
						_step++;
						Farm farm2 = Game1.getFarm();
						if (farm2 != null && farm2 != Game1.currentLocation)
						{
							_current = farm2;
							return true;
						}
					}
					if (_step == 2)
					{
						_step++;
						GameLocation farmhouse = Game1.getLocationFromName("FarmHouse");
						if (farmhouse != null && farmhouse != Game1.currentLocation)
						{
							_current = farmhouse;
							return true;
						}
					}
					if (_step == 3)
					{
						_step++;
						Farm farm = Game1.getFarm();
						_enumerator = farm.buildings.GetEnumerator();
					}
					while (_enumerator.MoveNext())
					{
						GameLocation location = _enumerator.Current.indoors.Value;
						if (location != null && location != Game1.currentLocation)
						{
							_current = location;
							return true;
						}
					}
					_done = true;
					_current = null;
					return false;
				}

				public void Dispose()
				{
				}

				void IEnumerator.Reset()
				{
					_current = null;
					_step = 0;
					_done = false;
				}
			}

			public Enumerator GetEnumerator()
			{
				return default(Enumerator);
			}

			IEnumerator<GameLocation> IEnumerable<GameLocation>.GetEnumerator()
			{
				return default(Enumerator);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return default(Enumerator);
			}
		}

		public enum DisconnectType
		{
			None,
			ClosedGame,
			ExitedToMainMenu,
			ExitedToMainMenu_FromFarmhandSelect,
			HostLeft,
			ServerOfflineMode,
			ServerFull,
			Kicked,
			ClientTimeout,
			LidgrenTimeout,
			GalaxyTimeout,
			Timeout_FarmhandSelection,
			LidgrenDisconnect_Unknown
		}

		public static readonly long AllPlayers = 0L;

		public const byte farmerDelta = 0;

		public const byte serverIntroduction = 1;

		public const byte playerIntroduction = 2;

		public const byte locationIntroduction = 3;

		public const byte forceEvent = 4;

		public const byte warpFarmer = 5;

		public const byte locationDelta = 6;

		public const byte locationSprites = 7;

		public const byte characterWarp = 8;

		public const byte availableFarmhands = 9;

		public const byte chatMessage = 10;

		public const byte connectionMessage = 11;

		public const byte worldDelta = 12;

		public const byte teamDelta = 13;

		public const byte newDaySync = 14;

		public const byte chatInfoMessage = 15;

		public const byte userNameUpdate = 16;

		public const byte farmerGainExperience = 17;

		public const byte serverToClientsMessage = 18;

		public const byte disconnecting = 19;

		public const byte sharedAchievement = 20;

		public const byte globalMessage = 21;

		public const byte partyWideMail = 22;

		public const byte forceKick = 23;

		public const byte removeLocationFromLookup = 24;

		public const byte farmerKilledMonster = 25;

		public const byte requestGrandpaReevaluation = 26;

		public int defaultInterpolationTicks = 15;

		public int farmerDeltaBroadcastPeriod = 3;

		public int locationDeltaBroadcastPeriod = 3;

		public int worldStateDeltaBroadcastPeriod = 3;

		public int playerLimit = 4;

		public static string kicked = "KICKED";

		public string protocolVersion = "1.4.0.1";

		public readonly NetLogger logging = new NetLogger();

		protected List<long> disconnectingFarmers = new List<long>();

		public ulong latestID;

		public const string MSG_START_FESTIVAL_EVENT = "festivalEvent";

		public const string MSG_END_FESTIVAL = "endFest";

		public const string MSG_TRAIN_APPROACH = "trainApproach";

		public const string MSG_PLACEHOLDER = "[replace me]";

		public virtual int MaxPlayers
		{
			get
			{
				if (Game1.server == null)
				{
					return 1;
				}
				return playerLimit;
			}
		}

		public virtual long getNewID()
		{
			ulong seqNum = ((latestID & 0xFF) + 1) & 0xFF;
			ulong nodeID3 = (ulong)Game1.player.UniqueMultiplayerID;
			nodeID3 = ((nodeID3 >> 32) ^ (nodeID3 & uint.MaxValue));
			nodeID3 = (((nodeID3 >> 16) ^ (nodeID3 & 0xFFFF)) & 0xFFFF);
			ulong timestamp = (ulong)(DateTime.Now.Ticks / 10000);
			latestID = ((timestamp << 24) | (nodeID3 << 8) | seqNum);
			return (long)latestID;
		}

		public virtual bool isDisconnecting(Farmer farmer)
		{
			return isDisconnecting(farmer.UniqueMultiplayerID);
		}

		public virtual bool isDisconnecting(long uid)
		{
			return disconnectingFarmers.Contains(uid);
		}

		public virtual bool isClientBroadcastType(byte messageType)
		{
			switch (messageType)
			{
			case 0:
			case 2:
			case 4:
			case 6:
			case 7:
			case 12:
			case 13:
			case 14:
			case 15:
			case 19:
			case 20:
			case 21:
			case 22:
			case 24:
			case 26:
				return true;
			default:
				return false;
			}
		}

		public virtual bool allowSyncDelay()
		{
			return Game1.newDaySync == null;
		}

		public virtual int interpolationTicks()
		{
			if (!allowSyncDelay())
			{
				return 0;
			}
			return defaultInterpolationTicks;
		}

		public virtual IEnumerable<NetFarmerRoot> farmerRoots()
		{
			if (Game1.serverHost != null)
			{
				yield return Game1.serverHost;
			}
			foreach (NetRoot<Farmer> farmerRoot in Game1.otherFarmers.Roots.Values)
			{
				if (Game1.serverHost == null || farmerRoot != Game1.serverHost)
				{
					yield return farmerRoot as NetFarmerRoot;
				}
			}
		}

		public virtual NetFarmerRoot farmerRoot(long id)
		{
			if (id == Game1.serverHost.Value.UniqueMultiplayerID)
			{
				return Game1.serverHost;
			}
			if (Game1.otherFarmers.ContainsKey(id))
			{
				return Game1.otherFarmers.Roots[id] as NetFarmerRoot;
			}
			return null;
		}

		public virtual void broadcastFarmerDeltas()
		{
			foreach (NetFarmerRoot farmerRoot in farmerRoots())
			{
				if (farmerRoot.Dirty && Game1.player.UniqueMultiplayerID == farmerRoot.Value.UniqueMultiplayerID)
				{
					broadcastFarmerDelta(farmerRoot.Value, writeObjectDeltaBytes(farmerRoot));
				}
			}
			if (Game1.player.teamRoot.Dirty)
			{
				broadcastTeamDelta(writeObjectDeltaBytes(Game1.player.teamRoot));
			}
		}

		protected virtual void broadcastTeamDelta(byte[] delta)
		{
			if (Game1.IsServer)
			{
				foreach (Farmer farmer in Game1.otherFarmers.Values)
				{
					if (farmer != Game1.player)
					{
						Game1.server.sendMessage(farmer.UniqueMultiplayerID, 13, Game1.player, delta);
					}
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(13, delta);
			}
		}

		protected virtual void broadcastFarmerDelta(Farmer farmer, byte[] delta)
		{
			foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
			{
				if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					v.Value.queueMessage(0, farmer, farmer.UniqueMultiplayerID, delta);
				}
			}
		}

		public void updateRoot<T>(T root) where T : INetRoot
		{
			foreach (long id in disconnectingFarmers)
			{
				root.Disconnect(id);
			}
			root.TickTree();
		}

		public virtual void updateRoots()
		{
			updateRoot(Game1.netWorldState);
			foreach (NetFarmerRoot farmerRoot in farmerRoots())
			{
				farmerRoot.Clock.InterpolationTicks = interpolationTicks();
				updateRoot(farmerRoot);
			}
			Game1.player.teamRoot.Clock.InterpolationTicks = interpolationTicks();
			updateRoot(Game1.player.teamRoot);
			if (Game1.IsClient)
			{
				foreach (GameLocation location in activeLocations())
				{
					if (location.Root != null && location.Root.Value == location)
					{
						location.Root.Clock.InterpolationTicks = interpolationTicks();
						updateRoot(location.Root);
					}
				}
				return;
			}
			foreach (GameLocation loc in Game1.locations)
			{
				if (loc.Root != null)
				{
					loc.Root.Clock.InterpolationTicks = interpolationTicks();
					updateRoot(loc.Root);
				}
			}
			foreach (MineShaft mine in MineShaft.activeMines)
			{
				if (mine.Root != null)
				{
					mine.Root.Clock.InterpolationTicks = interpolationTicks();
					updateRoot(mine.Root);
				}
			}
		}

		public virtual void broadcastLocationDeltas()
		{
			if (Game1.IsClient)
			{
				foreach (GameLocation location in activeLocations())
				{
					if (!(location.Root == null) && location.Root.Dirty)
					{
						broadcastLocationDelta(location);
					}
				}
				return;
			}
			foreach (GameLocation loc in Game1.locations)
			{
				if (loc.Root != null && loc.Root.Dirty)
				{
					broadcastLocationDelta(loc);
				}
			}
			MineShaft.ForEach(delegate(MineShaft mine)
			{
				if (mine.Root != null && mine.Root.Dirty)
				{
					broadcastLocationDelta(mine);
				}
			});
		}

		public virtual void broadcastLocationDelta(GameLocation loc)
		{
			if (!(loc.Root == null) && loc.Root.Dirty)
			{
				byte[] delta = writeObjectDeltaBytes(loc.Root);
				broadcastLocationBytes(loc, 6, delta);
			}
		}

		protected virtual void broadcastLocationBytes(GameLocation loc, byte messageType, byte[] bytes)
		{
			OutgoingMessage message = new OutgoingMessage(messageType, Game1.player, loc.isStructure.Value, loc.isStructure ? loc.uniqueName.Value : loc.name.Value, bytes);
			broadcastLocationMessage(loc, message);
		}

		protected virtual void broadcastLocationMessage(GameLocation loc, OutgoingMessage message)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(message);
				return;
			}
			Action<Farmer> tellFarmer = delegate(Farmer f)
			{
				if (f != Game1.player)
				{
					Game1.server.sendMessage(f.UniqueMultiplayerID, message);
				}
			};
			if (isAlwaysActiveLocation(loc))
			{
				foreach (Farmer farmer in Game1.otherFarmers.Values)
				{
					tellFarmer(farmer);
				}
				return;
			}
			foreach (Farmer f3 in loc.farmers)
			{
				tellFarmer(f3);
			}
			if (loc is BuildableGameLocation)
			{
				foreach (Building building in (loc as BuildableGameLocation).buildings)
				{
					if (building.indoors.Value != null)
					{
						foreach (Farmer f2 in building.indoors.Value.farmers)
						{
							tellFarmer(f2);
						}
					}
				}
			}
		}

		public virtual void broadcastSprites(GameLocation location, List<TemporaryAnimatedSprite> sprites)
		{
			broadcastSprites(location, sprites.ToArray());
		}

		public virtual void broadcastSprites(GameLocation location, params TemporaryAnimatedSprite[] sprites)
		{
			location.temporarySprites.AddRange(sprites);
			if (sprites.Length != 0 && Game1.IsMultiplayer)
			{
				using (MemoryStream stream = new MemoryStream())
				{
					using (BinaryWriter writer = createWriter(stream))
					{
						writer.Push("TemporaryAnimatedSprites");
						writer.Write(sprites.Length);
						for (int i = 0; i < sprites.Length; i++)
						{
							sprites[i].Write(writer, location);
						}
						writer.Pop();
					}
					broadcastLocationBytes(location, 7, stream.ToArray());
				}
			}
		}

		public virtual void broadcastWorldStateDeltas()
		{
			if (Game1.netWorldState.Dirty)
			{
				byte[] delta = writeObjectDeltaBytes(Game1.netWorldState);
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (v.Value != Game1.player)
					{
						v.Value.queueMessage(12, Game1.player, delta);
					}
				}
			}
		}

		public virtual void receiveWorldState(BinaryReader msg)
		{
			Game1.netWorldState.Clock.InterpolationTicks = 0;
			readObjectDelta(msg, Game1.netWorldState);
			Game1.netWorldState.TickTree();
			int origTime = Game1.timeOfDay;
			Game1.netWorldState.Value.WriteToGame1();
			if (!Game1.IsServer && origTime != Game1.timeOfDay && Game1.currentLocation != null && Game1.newDaySync == null)
			{
				Game1.performTenMinuteClockUpdate();
			}
		}

		public virtual void requestCharacterWarp(NPC character, GameLocation targetLocation, Vector2 position)
		{
			if (Game1.IsClient)
			{
				GameLocation loc = character.currentLocation;
				if (loc == null)
				{
					throw new ArgumentException("In warpCharacter, the character's currentLocation must not be null");
				}
				Guid characterGuid = loc.characters.GuidOf(character);
				if (characterGuid == Guid.Empty)
				{
					throw new ArgumentException("In warpCharacter, the character must be in its currentLocation");
				}
				OutgoingMessage message = new OutgoingMessage(8, Game1.player, loc.isStructure.Value, loc.isStructure ? loc.uniqueName.Value : loc.name.Value, characterGuid, targetLocation.isStructure.Value, targetLocation.isStructure ? targetLocation.uniqueName.Value : targetLocation.name.Value, position);
				Game1.serverHost.Value.queueMessage(message);
			}
		}

		public virtual NetRoot<GameLocation> locationRoot(GameLocation location)
		{
			if (location.Root == null && Game1.IsMasterGame)
			{
				new NetRoot<GameLocation>().Set(location);
				location.Root.Clock.InterpolationTicks = interpolationTicks();
				location.Root.MarkClean();
			}
			return location.Root;
		}

		public virtual void broadcastEvent(Event evt, GameLocation location, Vector2 positionBeforeEvent)
		{
			if (evt.id != -1)
			{
				object[] message = new object[5]
				{
					evt.id,
					(int)positionBeforeEvent.X,
					(int)positionBeforeEvent.Y,
					(byte)(location.isStructure ? 1 : 0),
					location.isStructure ? location.uniqueName.Value : location.Name
				};
				if (Game1.IsServer)
				{
					foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
					{
						if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
						{
							Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 4, Game1.player, message);
						}
					}
				}
				else if (Game1.IsClient)
				{
					Game1.client.sendMessage(4, message);
				}
			}
		}

		protected virtual void receiveRequestGrandpaReevaluation(IncomingMessage msg)
		{
			Game1.getFarm()?.requestGrandpaReevaluation();
		}

		protected virtual void receiveFarmerKilledMonster(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				string which = msg.Reader.ReadString();
				if (which != null)
				{
					Game1.stats.monsterKilled(which);
				}
			}
		}

		public virtual void broadcastRemoveLocationFromLookup(GameLocation location)
		{
			List<object> message = new List<object>();
			message.Add(location.NameOrUniqueName);
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 24, Game1.player, message.ToArray());
					}
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(24, message.ToArray());
			}
		}

		public virtual void broadcastPartyWideMail(string mail_key, PartyWideMessageQueue message_queue = PartyWideMessageQueue.MailForTomorrow, bool no_letter = false)
		{
			mail_key = mail_key.Trim();
			mail_key = mail_key.Replace(Environment.NewLine, "");
			List<object> message = new List<object>();
			message.Add(mail_key);
			message.Add((int)message_queue);
			message.Add(no_letter);
			_performPartyWideMail(mail_key, message_queue, no_letter);
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 22, Game1.player, message.ToArray());
					}
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(22, message.ToArray());
			}
		}

		public virtual void broadcastGrandpaReevaluation()
		{
			Game1.getFarm().requestGrandpaReevaluation();
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 26, Game1.player);
					}
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(26);
			}
		}

		public virtual void broadcastGlobalMessage(string localization_string_key, bool only_show_if_empty = false, params string[] substitutions)
		{
			if (!only_show_if_empty || Game1.hudMessages.Count == 0)
			{
				Game1.showGlobalMessage(Game1.content.LoadString(localization_string_key, substitutions));
			}
			List<object> message = new List<object>();
			message.Add(localization_string_key);
			message.Add(only_show_if_empty);
			message.Add(substitutions.Length);
			for (int i = 0; i < substitutions.Length; i++)
			{
				message.Add(substitutions[i]);
			}
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (v.Value.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 21, Game1.player, message.ToArray());
					}
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(21, message.ToArray());
			}
		}

		public virtual NetRoot<T> readObjectFull<T>(BinaryReader reader) where T : class, INetObject<INetSerializable>
		{
			NetRoot<T> netRoot = NetRoot<T>.Connect(reader);
			netRoot.Clock.InterpolationTicks = defaultInterpolationTicks;
			return netRoot;
		}

		protected virtual BinaryWriter createWriter(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			if (logging.IsLogging)
			{
				writer = new LoggingBinaryWriter(writer);
			}
			return writer;
		}

		public virtual void writeObjectFull<T>(BinaryWriter writer, NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			root.CreateConnectionPacket(writer, peer);
		}

		public virtual byte[] writeObjectFullBytes<T>(NetRoot<T> root, long? peer) where T : class, INetObject<INetSerializable>
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = createWriter(stream))
				{
					root.CreateConnectionPacket(writer, peer);
					return stream.ToArray();
				}
			}
		}

		public virtual void readObjectDelta<T>(BinaryReader reader, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Read(reader);
		}

		public virtual void writeObjectDelta<T>(BinaryWriter writer, NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			root.Write(writer);
		}

		public virtual byte[] writeObjectDeltaBytes<T>(NetRoot<T> root) where T : class, INetObject<INetSerializable>
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = createWriter(stream))
				{
					root.Write(writer);
					return stream.ToArray();
				}
			}
		}

		public virtual NetFarmerRoot readFarmer(BinaryReader reader)
		{
			NetFarmerRoot netFarmerRoot = new NetFarmerRoot();
			netFarmerRoot.ReadConnectionPacket(reader);
			netFarmerRoot.Clock.InterpolationTicks = defaultInterpolationTicks;
			return netFarmerRoot;
		}

		public virtual void addPlayer(NetFarmerRoot f)
		{
			long id = f.Value.UniqueMultiplayerID;
			f.Value.teamRoot = Game1.player.teamRoot;
			Game1.otherFarmers.Roots[id] = f;
			disconnectingFarmers.Remove(id);
			if (Game1.chatBox != null)
			{
				string farmerName = ChatBox.formattedUserNameLong(f.Value);
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerJoined", farmerName));
			}
		}

		public virtual void receivePlayerIntroduction(BinaryReader reader)
		{
			addPlayer(readFarmer(reader));
		}

		public virtual void broadcastPlayerIntroduction(NetFarmerRoot farmerRoot)
		{
			if (Game1.server != null)
			{
				foreach (KeyValuePair<long, Farmer> v in Game1.otherFarmers)
				{
					if (farmerRoot.Value.UniqueMultiplayerID != v.Value.UniqueMultiplayerID)
					{
						Game1.server.sendMessage(v.Value.UniqueMultiplayerID, 2, farmerRoot.Value, Game1.server.getUserName(farmerRoot.Value.UniqueMultiplayerID), writeObjectFullBytes(farmerRoot, v.Value.UniqueMultiplayerID));
					}
				}
			}
		}

		public virtual void broadcastUserName(long farmerId, string userName)
		{
			if (Game1.server == null)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					Farmer farmer = otherFarmer.Value;
					if (farmer.UniqueMultiplayerID != farmerId)
					{
						Game1.server.sendMessage(farmer.UniqueMultiplayerID, 16, Game1.serverHost.Value, farmerId, userName);
					}
				}
			}
		}

		public virtual string getUserName(long id)
		{
			if (id == Game1.player.UniqueMultiplayerID)
			{
				return Game1.content.LoadString("Strings\\UI:Chat_SelfPlayerID");
			}
			if (Game1.server != null)
			{
				return Game1.server.getUserName(id);
			}
			if (Game1.client != null)
			{
				return Game1.client.getUserName(id);
			}
			return "?";
		}

		public virtual void playerDisconnected(long id)
		{
			if (Game1.otherFarmers.ContainsKey(id) && !disconnectingFarmers.Contains(id))
			{
				NetFarmerRoot farmhand = Game1.otherFarmers.Roots[id] as NetFarmerRoot;
				if (farmhand.Value.mount != null && Game1.IsMasterGame)
				{
					farmhand.Value.mount.dismount();
				}
				if (Game1.IsMasterGame)
				{
					saveFarmhand(farmhand);
					farmhand.Value.handleDisconnect();
				}
				if (Game1.player.dancePartner.Value is Farmer && ((Farmer)Game1.player.dancePartner.Value).UniqueMultiplayerID == farmhand.Value.UniqueMultiplayerID)
				{
					Game1.player.dancePartner.Value = null;
				}
				if (Game1.chatBox != null)
				{
					Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_PlayerLeft", ChatBox.formattedUserNameLong(Game1.otherFarmers[id])));
				}
				disconnectingFarmers.Add(id);
			}
		}

		protected virtual void removeDisconnectedFarmers()
		{
			foreach (long id in disconnectingFarmers)
			{
				Game1.otherFarmers.Remove(id);
			}
			disconnectingFarmers.Clear();
		}

		public virtual void sendFarmhand()
		{
			(Game1.player.NetFields.Root as NetFarmerRoot).MarkReassigned();
		}

		protected virtual void saveFarmhand(NetFarmerRoot farmhand)
		{
			FarmHouse farmHouse = Utility.getHomeOfFarmer(farmhand);
			if (farmHouse is Cabin)
			{
				(farmHouse as Cabin).saveFarmhand(farmhand);
			}
		}

		public virtual void saveFarmhands()
		{
			if (Game1.IsMasterGame)
			{
				foreach (NetRoot<Farmer> farmer in Game1.otherFarmers.Roots.Values)
				{
					saveFarmhand(farmer as NetFarmerRoot);
				}
			}
		}

		public virtual void clientRemotelyDisconnected(DisconnectType disconnectType)
		{
			LogDisconnect(disconnectType);
			returnToMainMenu();
		}

		private void returnToMainMenu()
		{
			Game1.ExitToTitle(delegate
			{
				(Game1.activeClickableMenu as TitleMenu).skipToTitleButtons();
				TitleMenu.subMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\UI:Client_RemotelyDisconnected"), null)
				{
					okButton = 
					{
						visible = false
					}
				};
			});
		}

		public static bool ShouldLogDisconnect(DisconnectType disconnectType)
		{
			switch (disconnectType)
			{
			case DisconnectType.ClosedGame:
			case DisconnectType.ExitedToMainMenu:
			case DisconnectType.ExitedToMainMenu_FromFarmhandSelect:
			case DisconnectType.ServerOfflineMode:
			case DisconnectType.ServerFull:
				return false;
			default:
				return true;
			}
		}

		public static bool IsTimeout(DisconnectType disconnectType)
		{
			if ((uint)(disconnectType - 8) <= 2u)
			{
				return true;
			}
			return false;
		}

		public static void LogDisconnect(DisconnectType disconnectType)
		{
			if (ShouldLogDisconnect(disconnectType))
			{
				string message2 = "Disconnected at : " + DateTime.Now.ToLongTimeString() + " - " + disconnectType;
				if (Game1.client != null)
				{
					message2 = message2 + " Ping: " + Game1.client.GetPingToHost().ToString("0.#");
					message2 += ((Game1.client is LidgrenClient) ? " ip" : " friend/invite");
				}
				Program.WriteLog(Program.LogType.Disconnect, message2, append: true);
			}
			Console.WriteLine("Disconnected: " + disconnectType);
		}

		public virtual void sendSharedAchievementMessage(int achievement)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(20, achievement);
			}
			else if (Game1.IsServer)
			{
				foreach (long id in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(id, 20, Game1.player, achievement);
				}
			}
		}

		public virtual void sendServerToClientsMessage(string message)
		{
			if (Game1.IsServer)
			{
				foreach (KeyValuePair<long, Farmer> otherFarmer in Game1.otherFarmers)
				{
					otherFarmer.Value.queueMessage(18, Game1.player, message);
				}
			}
		}

		public virtual void sendChatMessage(LocalizedContentManager.LanguageCode language, string message, long recipientID)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(10, recipientID, language, message);
			}
			else if (Game1.IsServer)
			{
				if (recipientID == AllPlayers)
				{
					foreach (long id in Game1.otherFarmers.Keys)
					{
						Game1.server.sendMessage(id, 10, Game1.player, recipientID, language, message);
					}
				}
				else
				{
					Game1.server.sendMessage(recipientID, 10, Game1.player, recipientID, language, message);
				}
			}
		}

		public virtual void receiveChatMessage(Farmer sourceFarmer, long recipientID, LocalizedContentManager.LanguageCode language, string message)
		{
			if (Game1.chatBox != null)
			{
				int messageType = 0;
				if (recipientID != AllPlayers)
				{
					messageType = 3;
				}
				Game1.chatBox.receiveChatMessage(sourceFarmer.UniqueMultiplayerID, messageType, language, message);
			}
		}

		public virtual void globalChatInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsMultiplayer)
			{
				receiveChatInfoMessage(Game1.player, messageKey, args);
				sendChatInfoMessage(messageKey, args);
			}
		}

		protected virtual void sendChatInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsClient)
			{
				Game1.client.sendMessage(15, messageKey, args);
			}
			else if (Game1.IsServer)
			{
				foreach (long id in Game1.otherFarmers.Keys)
				{
					Game1.server.sendMessage(id, 15, Game1.player, messageKey, args);
				}
			}
		}

		protected virtual void receiveChatInfoMessage(Farmer sourceFarmer, string messageKey, string[] args)
		{
			if (Game1.chatBox != null)
			{
				try
				{
					string[] processedArgs = args.Select(delegate(string arg)
					{
						if (arg.StartsWith("achievement:"))
						{
							int key = Convert.ToInt32(arg.Substring("achievement:".Length));
							return Game1.content.Load<Dictionary<int, string>>("Data\\Achievements")[key].Split('^')[0];
						}
						return arg.StartsWith("object:") ? new Object(Convert.ToInt32(arg.Substring("object:".Length)), 1).DisplayName : arg;
					}).ToArray();
					ChatBox chatBox = Game1.chatBox;
					LocalizedContentManager content = Game1.content;
					string path = "Strings\\UI:Chat_" + messageKey;
					object[] substitutions = processedArgs;
					chatBox.addInfoMessage(content.LoadString(path, substitutions));
				}
				catch (ContentLoadException)
				{
				}
				catch (FormatException)
				{
				}
				catch (OverflowException)
				{
				}
				catch (KeyNotFoundException)
				{
				}
			}
		}

		public virtual void parseServerToClientsMessage(string message)
		{
			if (!Game1.IsClient)
			{
				return;
			}
			if (!(message == "festivalEvent"))
			{
				if (!(message == "endFest"))
				{
					if (message == "trainApproach")
					{
						GameLocation railroad = Game1.getLocationFromName("Railroad");
						if (railroad != null && railroad is Railroad)
						{
							((Railroad)railroad).PlayTrainApproach();
						}
					}
				}
				else if (Game1.CurrentEvent != null)
				{
					Game1.CurrentEvent.forceEndFestival(Game1.player);
				}
			}
			else if (Game1.currentLocation.currentEvent != null)
			{
				Game1.currentLocation.currentEvent.forceFestivalContinue();
			}
		}

		public virtual IEnumerable<GameLocation> activeLocations()
		{
			if (Game1.currentLocation != null)
			{
				yield return Game1.currentLocation;
			}
			Farm farm = Game1.getFarm();
			if (farm != null && farm != Game1.currentLocation)
			{
				yield return farm;
			}
			GameLocation farmhouse = Game1.getLocationFromName("FarmHouse");
			if (farmhouse != null && farmhouse != Game1.currentLocation)
			{
				yield return farmhouse;
			}
			foreach (Building building in farm.buildings)
			{
				if (building.indoors.Value != null && building.indoors.Value != Game1.currentLocation)
				{
					yield return building.indoors.Value;
				}
			}
		}

		public virtual bool isAlwaysActiveLocation(GameLocation location)
		{
			if (!(location.Name == "Farm") && !(location.Name == "FarmHouse"))
			{
				if (location.Root != null)
				{
					return location.Root.Value.Equals(Game1.getFarm());
				}
				return false;
			}
			return true;
		}

		protected virtual void readActiveLocation(IncomingMessage msg, bool forceCurrentLocation = false)
		{
			NetRoot<GameLocation> root = readObjectFull<GameLocation>(msg.Reader);
			if (isAlwaysActiveLocation(root.Value))
			{
				for (int i = 0; i < Game1.locations.Count; i++)
				{
					if (Game1.locations[i].Equals(root.Value))
					{
						Game1.locations[i] = root.Value;
						break;
					}
				}
			}
			if (!((Game1.locationRequest != null) | forceCurrentLocation))
			{
				return;
			}
			if (Game1.locationRequest != null)
			{
				Game1.currentLocation = Game1.findStructure(root.Value, Game1.locationRequest.Name);
				if (Game1.currentLocation == null)
				{
					Game1.currentLocation = root.Value;
				}
			}
			else if (forceCurrentLocation)
			{
				Game1.currentLocation = root.Value;
			}
			if (Game1.locationRequest != null)
			{
				Game1.locationRequest.Loaded(root.Value);
			}
			Game1.currentLocation.resetForPlayerEntry();
			Game1.player.currentLocation = Game1.currentLocation;
			if (Game1.locationRequest != null)
			{
				Game1.locationRequest.Warped(root.Value);
			}
			Game1.currentLocation.updateSeasonalTileSheets();
			if (Game1.isDebrisWeather)
			{
				Game1.populateDebrisWeatherArray();
			}
			Game1.locationRequest = null;
		}

		public virtual bool isActiveLocation(GameLocation location)
		{
			if (Game1.IsMasterGame)
			{
				return true;
			}
			if (Game1.currentLocation != null && Game1.currentLocation.Root != null && Game1.currentLocation.Root.Value == location.Root.Value)
			{
				return true;
			}
			if (isAlwaysActiveLocation(location))
			{
				return true;
			}
			return false;
		}

		protected virtual GameLocation readLocation(BinaryReader reader)
		{
			bool structure = reader.ReadByte() != 0;
			GameLocation location = Game1.getLocationFromName(reader.ReadString(), structure);
			if (location == null || locationRoot(location) == null)
			{
				return null;
			}
			if (!isActiveLocation(location))
			{
				return null;
			}
			return location;
		}

		protected virtual LocationRequest readLocationRequest(BinaryReader reader)
		{
			bool structure = reader.ReadByte() != 0;
			return Game1.getLocationRequest(reader.ReadString(), structure);
		}

		protected virtual void readWarp(BinaryReader reader, int tileX, int tileY, Action afterWarp)
		{
			LocationRequest request = readLocationRequest(reader);
			if (afterWarp != null)
			{
				request.OnWarp += afterWarp.Invoke;
			}
			Game1.warpFarmer(request, tileX, tileY, Game1.player.FacingDirection);
		}

		protected virtual NPC readNPC(BinaryReader reader)
		{
			GameLocation location = readLocation(reader);
			Guid guid = reader.ReadGuid();
			if (!location.characters.ContainsGuid(guid))
			{
				return null;
			}
			return location.characters[guid];
		}

		public virtual TemporaryAnimatedSprite[] readSprites(BinaryReader reader, GameLocation location)
		{
			int count = reader.ReadInt32();
			TemporaryAnimatedSprite[] result = new TemporaryAnimatedSprite[count];
			for (int i = 0; i < count; i++)
			{
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite();
				sprite.Read(reader, location);
				sprite.ticksBeforeAnimationStart += interpolationTicks();
				result[i] = sprite;
			}
			return result;
		}

		protected virtual void receiveTeamDelta(BinaryReader msg)
		{
			readObjectDelta(msg, Game1.player.teamRoot);
		}

		protected virtual void receiveNewDaySync(IncomingMessage msg)
		{
			if (Game1.newDaySync == null && msg.SourceFarmer == Game1.serverHost.Value)
			{
				Game1.NewDay(0f);
			}
			if (Game1.newDaySync != null)
			{
				Game1.newDaySync.receiveMessage(msg);
			}
		}

		protected virtual void receiveFarmerGainExperience(IncomingMessage msg)
		{
			if (msg.SourceFarmer == Game1.serverHost.Value)
			{
				int which = msg.Reader.ReadInt32();
				int howMuch = msg.Reader.ReadInt32();
				Game1.player.gainExperience(which, howMuch);
			}
		}

		protected virtual void receiveSharedAchievement(IncomingMessage msg)
		{
			Game1.getAchievement(msg.Reader.ReadInt32(), allowBroadcasting: false);
		}

		protected virtual void receiveRemoveLocationFromLookup(IncomingMessage msg)
		{
			Game1.removeLocationFromLocationLookup(msg.Reader.ReadString());
		}

		protected virtual void receivePartyWideMail(IncomingMessage msg)
		{
			string mail_key = msg.Reader.ReadString();
			PartyWideMessageQueue message_queue = (PartyWideMessageQueue)msg.Reader.ReadInt32();
			bool no_letter = msg.Reader.ReadBoolean();
			_performPartyWideMail(mail_key, message_queue, no_letter);
		}

		protected void _performPartyWideMail(string mail_key, PartyWideMessageQueue message_queue, bool no_letter)
		{
			if (message_queue == PartyWideMessageQueue.MailForTomorrow)
			{
				Game1.addMailForTomorrow(mail_key, no_letter);
			}
			if (no_letter)
			{
				mail_key += "%&NL&%";
			}
			if (message_queue == PartyWideMessageQueue.SeenMail && !Game1.player.mailReceived.Contains(mail_key))
			{
				Game1.player.mailReceived.Add(mail_key);
			}
			switch (message_queue)
			{
			case PartyWideMessageQueue.MailForTomorrow:
				mail_key = "%&MFT&%" + mail_key;
				break;
			case PartyWideMessageQueue.SeenMail:
				mail_key = "%&SM&%" + mail_key;
				break;
			}
			if (Game1.IsMasterGame && !Game1.player.team.broadcastedMail.Contains(mail_key))
			{
				Game1.player.team.broadcastedMail.Add(mail_key);
			}
		}

		protected void receiveForceKick()
		{
			if (!Game1.IsServer)
			{
				Disconnect(DisconnectType.Kicked);
				returnToMainMenu();
			}
		}

		protected virtual void receiveGlobalMessage(IncomingMessage msg)
		{
			string localization_string_key = msg.Reader.ReadString();
			if (!msg.Reader.ReadBoolean() || Game1.hudMessages.Count <= 0)
			{
				int count = msg.Reader.ReadInt32();
				object[] substitutions = new object[count];
				for (int i = 0; i < count; i++)
				{
					substitutions[i] = msg.Reader.ReadString();
				}
				Game1.showGlobalMessage(Game1.content.LoadString(localization_string_key, substitutions));
			}
		}

		public virtual void processIncomingMessage(IncomingMessage msg)
		{
			switch (msg.MessageType)
			{
			case 1:
			case 5:
			case 9:
			case 11:
			case 16:
				break;
			case 0:
			{
				long f = msg.Reader.ReadInt64();
				NetFarmerRoot farmer = farmerRoot(f);
				if (farmer != null)
				{
					readObjectDelta(msg.Reader, farmer);
				}
				break;
			}
			case 3:
				readActiveLocation(msg);
				break;
			case 6:
			{
				GameLocation location2 = readLocation(msg.Reader);
				if (location2 != null)
				{
					readObjectDelta(msg.Reader, location2.Root);
				}
				break;
			}
			case 7:
			{
				GameLocation location2 = readLocation(msg.Reader);
				location2?.temporarySprites.AddRange(readSprites(msg.Reader, location2));
				break;
			}
			case 8:
			{
				NPC character = readNPC(msg.Reader);
				GameLocation location2 = readLocation(msg.Reader);
				if (character != null && location2 != null)
				{
					Game1.warpCharacter(character, location2, msg.Reader.ReadVector2());
				}
				break;
			}
			case 4:
			{
				int eventId = msg.Reader.ReadInt32();
				int tileX = msg.Reader.ReadInt32();
				int tileY = msg.Reader.ReadInt32();
				Farmer farmerActor = (msg.SourceFarmer.NetFields.Root as NetRoot<Farmer>).Clone().Value;
				LocationRequest request = readLocationRequest(msg.Reader);
				request.OnWarp += delegate
				{
					farmerActor.currentLocation = Game1.currentLocation;
					farmerActor.completelyStopAnimatingOrDoingAction();
					farmerActor.UsingTool = false;
					farmerActor.items.Clear();
					farmerActor.hidden.Value = false;
					Event evt = Game1.currentLocation.findEventById(eventId, farmerActor);
					Game1.currentLocation.startEvent(evt);
					farmerActor.Position = Game1.player.Position;
					Game1.warpingForForcedRemoteEvent = false;
				};
				Action performForcedEvent = delegate
				{
					Game1.warpingForForcedRemoteEvent = true;
					Game1.warpFarmer(request, tileX, tileY, Game1.player.FacingDirection);
				};
				Game1.remoteEventQueue.Add(performForcedEvent);
				break;
			}
			case 10:
				receiveChatMessage(msg.SourceFarmer, msg.Reader.ReadInt64(), msg.Reader.ReadEnum<LocalizedContentManager.LanguageCode>(), msg.Reader.ReadString());
				break;
			case 15:
			{
				string messageKey = msg.Reader.ReadString();
				string[] args = new string[msg.Reader.ReadByte()];
				for (int i = 0; i < args.Length; i++)
				{
					args[i] = msg.Reader.ReadString();
				}
				receiveChatInfoMessage(msg.SourceFarmer, messageKey, args);
				break;
			}
			case 2:
				receivePlayerIntroduction(msg.Reader);
				break;
			case 12:
				receiveWorldState(msg.Reader);
				break;
			case 13:
				receiveTeamDelta(msg.Reader);
				break;
			case 14:
				receiveNewDaySync(msg);
				break;
			case 18:
				parseServerToClientsMessage(msg.Reader.ReadString());
				break;
			case 19:
				playerDisconnected(msg.SourceFarmer.UniqueMultiplayerID);
				break;
			case 17:
				receiveFarmerGainExperience(msg);
				break;
			case 25:
				receiveFarmerKilledMonster(msg);
				break;
			case 20:
				receiveSharedAchievement(msg);
				break;
			case 21:
				receiveGlobalMessage(msg);
				break;
			case 22:
				receivePartyWideMail(msg);
				break;
			case 23:
				receiveForceKick();
				break;
			case 24:
				receiveRemoveLocationFromLookup(msg);
				break;
			case 26:
				receiveRequestGrandpaReevaluation(msg);
				break;
			}
		}

		public virtual void StartServer()
		{
			Game1.server = new GameServer();
			Game1.server.startServer();
		}

		public virtual void Disconnect(DisconnectType disconnectType)
		{
			if (Game1.server != null)
			{
				Game1.server.stopServer();
				Game1.server = null;
				foreach (long id in Game1.otherFarmers.Keys)
				{
					playerDisconnected(id);
				}
			}
			if (Game1.client != null)
			{
				sendFarmhand();
				UpdateLate(forceSync: true);
				Game1.client.disconnect();
				Game1.client = null;
			}
			Game1.otherFarmers.Clear();
			LogDisconnect(disconnectType);
		}

		protected virtual void updatePendingConnections()
		{
			if (Game1.multiplayerMode == 2)
			{
				if (Game1.server == null && Game1.options.enableServer)
				{
					StartServer();
				}
			}
			else if (Game1.multiplayerMode == 1 && Game1.client != null && !Game1.client.readyToPlay)
			{
				Game1.client.receiveMessages();
			}
		}

		public void UpdateLoading()
		{
			updatePendingConnections();
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
		}

		public virtual void UpdateEarly()
		{
			updatePendingConnections();
			if (Game1.multiplayerMode == 2 && Game1.serverHost == null && Game1.options.enableServer)
			{
				Game1.server.initializeHost();
			}
			if (Game1.server != null)
			{
				Game1.server.receiveMessages();
			}
			else if (Game1.client != null)
			{
				Game1.client.receiveMessages();
			}
			updateRoots();
			if (Game1.CurrentEvent == null)
			{
				removeDisconnectedFarmers();
			}
		}

		public virtual void UpdateLate(bool forceSync = false)
		{
			if (Game1.multiplayerMode != 0)
			{
				if ((!allowSyncDelay() | forceSync) || Game1.ticks % farmerDeltaBroadcastPeriod == 0)
				{
					broadcastFarmerDeltas();
				}
				if ((!allowSyncDelay() | forceSync) || Game1.ticks % locationDeltaBroadcastPeriod == 0)
				{
					broadcastLocationDeltas();
				}
				if ((!allowSyncDelay() | forceSync) || Game1.ticks % worldStateDeltaBroadcastPeriod == 0)
				{
					broadcastWorldStateDeltas();
				}
			}
			if (Game1.server != null)
			{
				Game1.server.sendMessages();
			}
			if (Game1.client != null)
			{
				Game1.client.sendMessages();
			}
		}

		public virtual void inviteAccepted()
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				TitleMenu title = Game1.activeClickableMenu as TitleMenu;
				if (TitleMenu.subMenu == null)
				{
					title.performButtonAction("Invite");
				}
				else if (TitleMenu.subMenu is FarmhandMenu || TitleMenu.subMenu is CoopMenu)
				{
					TitleMenu.subMenu = new FarmhandMenu();
				}
			}
		}

		public virtual Client InitClient(Client client)
		{
			return client;
		}

		public virtual Server InitServer(Server server)
		{
			return server;
		}
	}
}
