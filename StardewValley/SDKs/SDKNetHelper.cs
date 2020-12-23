using StardewValley.Network;

namespace StardewValley.SDKs
{
	public interface SDKNetHelper
	{
		string GetUserID();

		Client CreateClient(object lobby);

		Client GetRequestedClient();

		Server CreateServer(IGameServer gameServer);

		void AddLobbyUpdateListener(LobbyUpdateListener listener);

		void RemoveLobbyUpdateListener(LobbyUpdateListener listener);

		void RequestFriendLobbyData();

		string GetLobbyData(object lobby, string key);

		string GetLobbyOwnerName(object lobby);

		bool SupportsInviteCodes();

		object GetLobbyFromInviteCode(string inviteCode);

		void ShowInviteDialog(object lobby);

		void MutePlayer(string userId, bool mute);

		bool IsPlayerMuted(string userId);

		void ShowProfile(string userId);
	}
}
