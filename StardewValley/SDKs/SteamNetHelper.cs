using Galaxy.Api;
using Steamworks;
using System;
using System.Collections.Generic;

namespace StardewValley.SDKs
{
	internal class SteamNetHelper : GalaxyNetHelper
	{
		private Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;

		private Callback<LobbyEnter_t> lobbyEnterCallback;

		private Callback<LobbyDataUpdate_t> lobbyDataUpdateCallback;

		private Dictionary<ulong, CSteamID> lobbyOwners = new Dictionary<ulong, CSteamID>();

		public SteamNetHelper()
		{
			gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(onGameLobbyJoinRequested);
			lobbyEnterCallback = Callback<LobbyEnter_t>.Create(onLobbyEnter);
			lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(onLobbyDataUpdate);
		}

		public override void RequestFriendLobbyData()
		{
			EFriendFlags flags = EFriendFlags.k_EFriendFlagImmediate;
			int count = SteamFriends.GetFriendCount(flags);
			for (int i = 0; i < count; i++)
			{
				if (SteamFriends.GetFriendGamePlayed(SteamFriends.GetFriendByIndex(i, flags), out FriendGameInfo_t gameInfo) && !(gameInfo.m_gameID.AppID() != SteamUtils.GetAppID()))
				{
					SteamMatchmaking.RequestLobbyData(gameInfo.m_steamIDLobby);
				}
			}
		}

		public override void ShowInviteDialog(object lobby)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(new CSteamID((ulong)lobby));
		}

		private void onLobbyDataUpdate(LobbyDataUpdate_t pCallback)
		{
			CSteamID steamLobby = new CSteamID(pCallback.m_ulSteamIDLobby);
			GalaxyID lobbyID = parseConnectionString(SteamMatchmaking.GetLobbyData(steamLobby, "connect"));
			lobbyOwners[lobbyID.ToUint64()] = SteamMatchmaking.GetLobbyOwner(steamLobby);
			if (lobbyID != null)
			{
				GalaxyInstance.Matchmaking().RequestLobbyData(lobbyID);
			}
		}

		protected override GalaxyID getStartupLobby()
		{
			string[] args = Environment.GetCommandLineArgs();
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == "+connect_lobby")
				{
					try
					{
						SteamMatchmaking.JoinLobby(new CSteamID(Convert.ToUInt64(args[i + 1])));
						return null;
					}
					catch (Exception)
					{
					}
				}
			}
			return null;
		}

		private void onGameLobbyJoinRequested(GameLobbyJoinRequested_t pCallback)
		{
			SteamMatchmaking.JoinLobby(pCallback.m_steamIDLobby);
		}

		private void onLobbyEnter(LobbyEnter_t pCallback)
		{
			CSteamID lobbyID = new CSteamID(pCallback.m_ulSteamIDLobby);
			if (!(SteamMatchmaking.GetLobbyOwner(lobbyID) == SteamUser.GetSteamID()))
			{
				lobbyRequested = parseConnectionString(SteamMatchmaking.GetLobbyData(lobbyID, "connect"));
				SteamMatchmaking.LeaveLobby(lobbyID);
				if (lobbyRequested != null)
				{
					Game1.multiplayer.inviteAccepted();
				}
			}
		}

		public override string GetLobbyOwnerName(object lobbyID)
		{
			if (lobbyOwners.ContainsKey((ulong)lobbyID))
			{
				return SteamFriends.GetFriendPersonaName(lobbyOwners[(ulong)lobbyID]);
			}
			return "";
		}
	}
}
