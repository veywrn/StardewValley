using Galaxy.Api;
using StardewValley.Network;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StardewValley.SDKs
{
	public class GalaxySocket
	{
		private class GalaxyLobbyCreatedListener : ILobbyCreatedListener
		{
			private Action<GalaxyID, LobbyCreateResult> callback;

			public GalaxyLobbyCreatedListener(Action<GalaxyID, LobbyCreateResult> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
			}

			public override void OnLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
			{
				callback(lobbyID, result);
			}

			public override void Dispose()
			{
				GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
				base.Dispose();
			}
		}

		private class GalaxyLobbyEnteredListener : ILobbyEnteredListener
		{
			private Action<GalaxyID, LobbyEnterResult> callback;

			public GalaxyLobbyEnteredListener(Action<GalaxyID, LobbyEnterResult> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
			}

			public override void OnLobbyEntered(GalaxyID lobbyID, LobbyEnterResult result)
			{
				callback(lobbyID, result);
			}

			public override void Dispose()
			{
				GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
				base.Dispose();
			}
		}

		private class GalaxyLobbyLeftListener : ILobbyLeftListener
		{
			private Action<GalaxyID, bool> callback;

			public GalaxyLobbyLeftListener(Action<GalaxyID, bool> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
			}

			public override void OnLobbyLeft(GalaxyID lobbyID, bool ioFailure)
			{
				callback(lobbyID, ioFailure);
			}

			public override void Dispose()
			{
				GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
				base.Dispose();
			}
		}

		public const long Timeout = 30000L;

		private const int SendMaxPacketSize = 1100;

		private const int ReceiveMaxPacketSize = 1300;

		private const long RecreateLobbyDelay = 20000L;

		private const long HeartbeatDelay = 15000L;

		private const byte HeartbeatMessage = byte.MaxValue;

		private GalaxyID selfId;

		private GalaxyID lobby;

		private GalaxyID lobbyOwner;

		private GalaxyLobbyEnteredListener galaxyLobbyEnterCallback;

		private GalaxyLobbyCreatedListener galaxyLobbyCreatedCallback;

		private GalaxyLobbyLeftListener galaxyLobbyLeftCallback;

		private string protocolVersion;

		private Dictionary<string, string> lobbyData = new Dictionary<string, string>();

		private ServerPrivacy privacy;

		private uint memberLimit;

		private long recreateTimer;

		private long heartbeatTimer;

		private Dictionary<ulong, GalaxyID> connections = new Dictionary<ulong, GalaxyID>();

		private HashSet<ulong> ghosts = new HashSet<ulong>();

		private Dictionary<ulong, MemoryStream> incompletePackets = new Dictionary<ulong, MemoryStream>();

		private Dictionary<ulong, long> lastMessageTime = new Dictionary<ulong, long>();

		private CSteamID? steamLobby;

		private Callback<LobbyEnter_t> steamLobbyEnterCallback;

		public int ConnectionCount => connections.Count;

		public IEnumerable<GalaxyID> Connections => connections.Values;

		public bool Connected => lobby != null;

		public GalaxyID LobbyOwner => lobbyOwner;

		public GalaxyID Lobby => lobby;

		public ulong? InviteDialogLobby
		{
			get
			{
				if (!steamLobby.HasValue)
				{
					return null;
				}
				return steamLobby.Value.m_SteamID;
			}
		}

		public GalaxySocket(string protocolVersion)
		{
			this.protocolVersion = protocolVersion;
			lobbyData["protocolVersion"] = protocolVersion;
			selfId = GalaxyInstance.User().GetGalaxyID();
			galaxyLobbyEnterCallback = new GalaxyLobbyEnteredListener(onGalaxyLobbyEnter);
			galaxyLobbyCreatedCallback = new GalaxyLobbyCreatedListener(onGalaxyLobbyCreated);
		}

		public string GetInviteCode()
		{
			if (lobby == null)
			{
				return null;
			}
			return Base36.Encode(lobby.GetRealID());
		}

		private string getConnectionString()
		{
			if (lobby == null)
			{
				return "";
			}
			return "-connect-lobby-" + lobby.ToUint64();
		}

		private long getTimeNow()
		{
			return DateTime.Now.Ticks / 10000;
		}

		public long GetPingWith(GalaxyID peer)
		{
			long time = 0L;
			lastMessageTime.TryGetValue(peer.ToUint64(), out time);
			if (time == 0L)
			{
				return 0L;
			}
			if (getTimeNow() - time > 30000)
			{
				return long.MaxValue;
			}
			return GalaxyInstance.Networking().GetPingWith(peer);
		}

		private LobbyType privacyToLobbyType(ServerPrivacy privacy)
		{
			switch (privacy)
			{
			case ServerPrivacy.InviteOnly:
				return LobbyType.LOBBY_TYPE_PRIVATE;
			case ServerPrivacy.FriendsOnly:
				return LobbyType.LOBBY_TYPE_FRIENDS_ONLY;
			case ServerPrivacy.Public:
				return LobbyType.LOBBY_TYPE_PUBLIC;
			default:
				throw new ArgumentException(Convert.ToString(privacy));
			}
		}

		private ELobbyType privacyToSteamLobbyType(ServerPrivacy privacy)
		{
			switch (privacy)
			{
			case ServerPrivacy.InviteOnly:
				return ELobbyType.k_ELobbyTypePrivate;
			case ServerPrivacy.FriendsOnly:
				return ELobbyType.k_ELobbyTypeFriendsOnly;
			case ServerPrivacy.Public:
				return ELobbyType.k_ELobbyTypePublic;
			default:
				throw new ArgumentException(Convert.ToString(privacy));
			}
		}

		public void SetPrivacy(ServerPrivacy privacy)
		{
			this.privacy = privacy;
			updateLobbyPrivacy();
		}

		public void CreateLobby(ServerPrivacy privacy, uint memberLimit)
		{
			this.privacy = privacy;
			this.memberLimit = memberLimit;
			lobbyOwner = selfId;
			tryCreateLobby();
		}

		private void tryCreateLobby()
		{
			Console.WriteLine("Creating lobby...");
			galaxyLobbyLeftCallback = new GalaxyLobbyLeftListener(onGalaxyLobbyLeft);
			GalaxyInstance.Matchmaking().CreateLobby(privacyToLobbyType(privacy), memberLimit, joinable: true, LobbyTopologyType.LOBBY_TOPOLOGY_TYPE_STAR);
			recreateTimer = 0L;
		}

		public void JoinLobby(GalaxyID lobbyId)
		{
			try
			{
				GalaxyInstance.Matchmaking().JoinLobby(lobbyId);
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}

		public void SetLobbyData(string key, string value)
		{
			lobbyData[key] = value;
			if (lobby != null)
			{
				GalaxyInstance.Matchmaking().SetLobbyData(lobby, key, value);
			}
		}

		private void updateLobbyPrivacy()
		{
			if (lobbyOwner != selfId)
			{
				return;
			}
			if (lobby != null)
			{
				GalaxyInstance.Matchmaking().SetLobbyType(lobby, privacyToLobbyType(privacy));
			}
			if (lobby == null)
			{
				if (steamLobby.HasValue)
				{
					SteamMatchmaking.LeaveLobby(steamLobby.Value);
				}
			}
			else if (!steamLobby.HasValue)
			{
				if (steamLobbyEnterCallback == null)
				{
					steamLobbyEnterCallback = Callback<LobbyEnter_t>.Create(onSteamLobbyEnter);
				}
				SteamMatchmaking.CreateLobby(privacyToSteamLobbyType(privacy), (int)memberLimit);
			}
			else
			{
				SteamMatchmaking.SetLobbyType(steamLobby.Value, privacyToSteamLobbyType(privacy));
				SteamMatchmaking.SetLobbyData(steamLobby.Value, "connect", getConnectionString());
			}
		}

		private void onGalaxyLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
		{
			if (result == LobbyCreateResult.LOBBY_CREATE_RESULT_ERROR)
			{
				Console.WriteLine("Failed to create lobby.");
				recreateTimer = getTimeNow() + 20000;
			}
		}

		private void onGalaxyLobbyLeft(GalaxyID lobbyID, bool ioFailure)
		{
			Console.WriteLine("Left lobby {0} - ioFailure: {1}", lobbyID.ToUint64(), ioFailure);
			lobby = null;
			recreateTimer = getTimeNow() + 20000;
		}

		private void onGalaxyLobbyEnter(GalaxyID lobbyID, LobbyEnterResult result)
		{
			if (result == LobbyEnterResult.LOBBY_ENTER_RESULT_SUCCESS)
			{
				Console.WriteLine("Lobby entered: {0}", lobbyID.ToUint64());
				lobby = lobbyID;
				lobbyOwner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobbyID);
				if (lobbyOwner == selfId)
				{
					foreach (KeyValuePair<string, string> pair in lobbyData)
					{
						GalaxyInstance.Matchmaking().SetLobbyData(lobby, pair.Key, pair.Value);
					}
					updateLobbyPrivacy();
				}
			}
		}

		private void onSteamLobbyEnter(LobbyEnter_t pCallback)
		{
			if (pCallback.m_EChatRoomEnterResponse == 1)
			{
				Console.WriteLine("Steam lobby entered: {0}", pCallback.m_ulSteamIDLobby);
				steamLobbyEnterCallback.Unregister();
				steamLobbyEnterCallback = null;
				steamLobby = new CSteamID(pCallback.m_ulSteamIDLobby);
				if (SteamMatchmaking.GetLobbyOwner(steamLobby.Value) == SteamUser.GetSteamID())
				{
					SteamMatchmaking.SetLobbyType(steamLobby.Value, privacyToSteamLobbyType(privacy));
					SteamMatchmaking.SetLobbyData(steamLobby.Value, "connect", getConnectionString());
				}
			}
		}

		public IEnumerable<GalaxyID> LobbyMembers()
		{
			if (lobby == null)
			{
				yield break;
			}
			uint i = 0u;
			while (i < GalaxyInstance.Matchmaking().GetNumLobbyMembers(lobby))
			{
				GalaxyID lobbyMember = GalaxyInstance.Matchmaking().GetLobbyMemberByIndex(lobby, i);
				if (!(lobbyMember == selfId) && !ghosts.Contains(lobbyMember.ToUint64()))
				{
					yield return lobbyMember;
				}
				uint num = i + 1;
				i = num;
			}
		}

		private bool lobbyContains(GalaxyID user)
		{
			foreach (GalaxyID lobbyMember in LobbyMembers())
			{
				if (user == lobbyMember || ghosts.Contains(lobbyMember.ToUint64()))
				{
					return true;
				}
			}
			return false;
		}

		private void close(GalaxyID peer)
		{
			connections.Remove(peer.ToUint64());
			incompletePackets.Remove(peer.ToUint64());
		}

		public void Kick(GalaxyID user)
		{
			ghosts.Add(user.ToUint64());
		}

		public void Close()
		{
			if (lobby != null)
			{
				while (ConnectionCount > 0)
				{
					close(Connections.First());
				}
				GalaxyInstance.Matchmaking().LeaveLobby(lobby);
				lobby = null;
			}
			updateLobbyPrivacy();
			galaxyLobbyEnterCallback.Dispose();
			galaxyLobbyCreatedCallback.Dispose();
			if (galaxyLobbyLeftCallback != null)
			{
				galaxyLobbyLeftCallback.Dispose();
			}
		}

		public void Receive(Action<GalaxyID> onConnection, Action<GalaxyID, Stream> onMessage, Action<GalaxyID> onDisconnect, Action<string> onError)
		{
			long timeNow = getTimeNow();
			if (lobby == null)
			{
				if (lobbyOwner == selfId && recreateTimer > 0 && recreateTimer <= timeNow)
				{
					recreateTimer = 0L;
					tryCreateLobby();
				}
				return;
			}
			string lobbyVersion = GalaxyInstance.Matchmaking().GetLobbyData(lobby, "protocolVersion");
			if (lobbyVersion != "" && lobbyVersion != protocolVersion)
			{
				onError("Strings\\UI:CoopMenu_FailedProtocolVersion");
				Close();
				return;
			}
			foreach (GalaxyID lobbyMember in LobbyMembers())
			{
				if (!connections.ContainsKey(lobbyMember.ToUint64()) && !ghosts.Contains(lobbyMember.ToUint64()))
				{
					connections.Add(lobbyMember.ToUint64(), lobbyMember);
					onConnection(lobbyMember);
				}
			}
			ghosts.IntersectWith(from peer in LobbyMembers()
				select peer.ToUint64());
			byte[] buffer = new byte[1300];
			uint packetSize = 1300u;
			GalaxyID sender = new GalaxyID();
			while (GalaxyInstance.Networking().ReadP2PPacket(buffer, (uint)buffer.Length, ref packetSize, ref sender))
			{
				lastMessageTime[sender.ToUint64()] = timeNow;
				if (!connections.ContainsKey(sender.ToUint64()) || buffer[0] == byte.MaxValue)
				{
					continue;
				}
				bool incomplete = buffer[0] == 1;
				MemoryStream messageData2 = new MemoryStream();
				messageData2.Write(buffer, 4, (int)(packetSize - 4));
				if (incompletePackets.ContainsKey(sender.ToUint64()))
				{
					messageData2.Position = 0L;
					messageData2.CopyTo(incompletePackets[sender.ToUint64()]);
					if (!incomplete)
					{
						messageData2 = incompletePackets[sender.ToUint64()];
						incompletePackets.Remove(sender.ToUint64());
						messageData2.Position = 0L;
						onMessage(sender, messageData2);
					}
				}
				else if (incomplete)
				{
					messageData2.Position = messageData2.Length;
					incompletePackets[sender.ToUint64()] = messageData2;
				}
				else
				{
					messageData2.Position = 0L;
					onMessage(sender, messageData2);
				}
			}
			List<GalaxyID> disconnectedPeers = new List<GalaxyID>();
			foreach (GalaxyID peer3 in connections.Values)
			{
				if (!lobbyContains(peer3) || ghosts.Contains(peer3.ToUint64()))
				{
					disconnectedPeers.Add(peer3);
				}
			}
			foreach (GalaxyID peer2 in disconnectedPeers)
			{
				onDisconnect(peer2);
				close(peer2);
			}
		}

		public void Heartbeat(IEnumerable<GalaxyID> peers)
		{
			long timeNow = getTimeNow();
			if (heartbeatTimer <= timeNow)
			{
				heartbeatTimer = timeNow + 15000;
				byte[] heartbeatPacket = new byte[1]
				{
					255
				};
				foreach (GalaxyID peer in peers)
				{
					GalaxyInstance.Networking().SendP2PPacket(peer, heartbeatPacket, (uint)heartbeatPacket.Length, P2PSendType.P2P_SEND_RELIABLE);
				}
			}
		}

		public void Send(GalaxyID peer, byte[] data)
		{
			if (!connections.ContainsKey(peer.ToUint64()))
			{
				return;
			}
			if (data.Length <= 1100)
			{
				byte[] packet2 = new byte[data.Length + 4];
				data.CopyTo(packet2, 4);
				GalaxyInstance.Networking().SendP2PPacket(peer, packet2, (uint)packet2.Length, P2PSendType.P2P_SEND_RELIABLE);
				return;
			}
			int chunkSize = 1096;
			int messageOffset = 0;
			byte[] packet = new byte[1100];
			packet[0] = 1;
			while (messageOffset < data.Length)
			{
				int thisChunkSize = chunkSize;
				if (messageOffset + chunkSize >= data.Length)
				{
					packet[0] = 0;
					thisChunkSize = data.Length - messageOffset;
				}
				Buffer.BlockCopy(data, messageOffset, packet, 4, thisChunkSize);
				messageOffset += thisChunkSize;
				GalaxyInstance.Networking().SendP2PPacket(peer, packet, (uint)(thisChunkSize + 4), P2PSendType.P2P_SEND_RELIABLE);
			}
		}

		public void Send(GalaxyID peer, OutgoingMessage message)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					message.Write(writer);
					stream.Seek(0L, SeekOrigin.Begin);
					Send(peer, stream.ToArray());
				}
			}
		}
	}
}
