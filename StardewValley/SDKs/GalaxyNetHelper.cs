using Galaxy.Api;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewValley.SDKs
{
	public class GalaxyNetHelper : SDKNetHelper
	{
		private class GameJoinRequestedListener : IGameJoinRequestedListener
		{
			private Action<GalaxyID, string> callback;

			public GameJoinRequestedListener(Action<GalaxyID, string> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerGameJoinRequested.GetListenerType(), this);
			}

			public override void OnGameJoinRequested(GalaxyID lobbyID, string result)
			{
				if (callback != null)
				{
					callback(lobbyID, result);
				}
			}
		}

		private class LobbyDataListener : ILobbyDataListener
		{
			private Action<GalaxyID, GalaxyID> callback;

			public LobbyDataListener(Action<GalaxyID, GalaxyID> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyData.GetListenerType(), this);
			}

			public override void OnLobbyDataUpdated(GalaxyID lobbyID, GalaxyID memberID)
			{
				if (callback != null)
				{
					callback(lobbyID, memberID);
				}
			}
		}

		private class RichPresenceListener : IRichPresenceListener
		{
			private Action<GalaxyID> callback;

			public RichPresenceListener(Action<GalaxyID> callback)
			{
				this.callback = callback;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerRichPresence.GetListenerType(), this);
			}

			public override void OnRichPresenceUpdated(GalaxyID userID)
			{
				if (callback != null)
				{
					callback(userID);
				}
			}
		}

		public const string GalaxyConnectionStringPrefix = "-connect-lobby-";

		public const string SteamConnectionStringPrefix = "+connect_lobby";

		protected GalaxyID lobbyRequested;

		private GameJoinRequestedListener lobbyJoinRequested;

		private LobbyDataListener lobbyDataListener;

		private RichPresenceListener richPresenceListener;

		private List<LobbyUpdateListener> lobbyUpdateListeners = new List<LobbyUpdateListener>();

		public GalaxyNetHelper()
		{
			lobbyRequested = getStartupLobby();
			lobbyJoinRequested = new GameJoinRequestedListener(onLobbyJoinRequested);
			lobbyDataListener = new LobbyDataListener(onLobbyDataUpdated);
			richPresenceListener = new RichPresenceListener(onRichPresenceUpdated);
			if (lobbyRequested != null)
			{
				Game1.multiplayer.inviteAccepted();
			}
		}

		public string GetUserID()
		{
			return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
		}

		private Client createClient(GalaxyID lobby)
		{
			return Game1.multiplayer.InitClient(new GalaxyNetClient(lobby));
		}

		public Client CreateClient(object lobby)
		{
			return createClient(new GalaxyID((ulong)lobby));
		}

		public Server CreateServer(IGameServer gameServer)
		{
			return Game1.multiplayer.InitServer(new GalaxyNetServer(gameServer));
		}

		protected GalaxyID parseConnectionString(string connectionString)
		{
			if (connectionString.StartsWith("-connect-lobby-"))
			{
				return new GalaxyID(Convert.ToUInt64(connectionString.Substring("-connect-lobby-".Length)));
			}
			if (connectionString.StartsWith("+connect_lobby "))
			{
				return new GalaxyID(Convert.ToUInt64(connectionString.Substring("+connect_lobby".Length + 1)));
			}
			return null;
		}

		protected virtual GalaxyID getStartupLobby()
		{
			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("-connect-lobby-"))
				{
					return parseConnectionString(args[i]);
				}
			}
			return null;
		}

		public Client GetRequestedClient()
		{
			if (lobbyRequested != null)
			{
				return createClient(lobbyRequested);
			}
			return null;
		}

		public void AddLobbyUpdateListener(LobbyUpdateListener listener)
		{
			lobbyUpdateListeners.Add(listener);
		}

		public void RemoveLobbyUpdateListener(LobbyUpdateListener listener)
		{
			lobbyUpdateListeners.Remove(listener);
		}

		public virtual void RequestFriendLobbyData()
		{
			uint count = GalaxyInstance.Friends().GetFriendCount();
			for (uint i = 0u; i < count; i++)
			{
				GalaxyID friend = GalaxyInstance.Friends().GetFriendByIndex(i);
				GalaxyInstance.Friends().RequestRichPresence(friend);
			}
		}

		private void onRichPresenceUpdated(GalaxyID userID)
		{
			GalaxyID lobby = parseConnectionString(GalaxyInstance.Friends().GetRichPresence("connect", userID));
			if (lobby != null)
			{
				GalaxyInstance.Matchmaking().RequestLobbyData(lobby);
			}
		}

		private void onLobbyDataUpdated(GalaxyID lobbyID, GalaxyID memberID)
		{
			foreach (LobbyUpdateListener lobbyUpdateListener in lobbyUpdateListeners)
			{
				lobbyUpdateListener.OnLobbyUpdate(lobbyID.ToUint64());
			}
		}

		public virtual string GetLobbyData(object lobby, string key)
		{
			return GalaxyInstance.Matchmaking().GetLobbyData(new GalaxyID((ulong)lobby), key);
		}

		public virtual string GetLobbyOwnerName(object lobbyId)
		{
			GalaxyID lobby = new GalaxyID((ulong)lobbyId);
			GalaxyID owner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobby);
			return GalaxyInstance.Friends().GetFriendPersonaName(owner);
		}

		private void onLobbyJoinRequested(GalaxyID userID, string connectionString)
		{
			lobbyRequested = parseConnectionString(connectionString);
			if (lobbyRequested != null)
			{
				Game1.multiplayer.inviteAccepted();
			}
		}

		public bool SupportsInviteCodes()
		{
			return true;
		}

		public object GetLobbyFromInviteCode(string inviteCode)
		{
			ulong decoded = 0uL;
			try
			{
				decoded = Base36.Decode(inviteCode);
			}
			catch (FormatException)
			{
			}
			if (decoded != 0L && decoded >> 56 == 0L)
			{
				return GalaxyID.FromRealID(GalaxyID.IDType.ID_TYPE_LOBBY, decoded).ToUint64();
			}
			return null;
		}

		public virtual void ShowInviteDialog(object lobby)
		{
			GalaxyInstance.Friends().ShowOverlayInviteDialog("-connect-lobby-" + Convert.ToString((ulong)lobby));
		}
	}
}
