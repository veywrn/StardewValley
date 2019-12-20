using Galaxy.Api;
using StardewValley.Network;
using System;
using System.IO;
using System.Linq;

namespace StardewValley.SDKs
{
	public class GalaxyNetClient : Client
	{
		private GalaxyID lobbyId;

		protected GalaxySocket client;

		private GalaxyID serverId;

		private float lastPingMs;

		public GalaxyNetClient(GalaxyID lobbyId)
		{
			this.lobbyId = lobbyId;
		}

		public override string getUserID()
		{
			return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
		}

		protected override string getHostUserName()
		{
			return GalaxyInstance.Friends().GetFriendPersonaName(serverId);
		}

		public override float GetPingToHost()
		{
			return lastPingMs;
		}

		protected override void connectImpl()
		{
			client = new GalaxySocket(Game1.multiplayer.protocolVersion);
			GalaxyInstance.User().GetGalaxyID();
			client.JoinLobby(lobbyId);
		}

		public override void disconnect(bool neatly = true)
		{
			if (client != null)
			{
				Console.WriteLine("Disconnecting from server {0}", lobbyId);
				client.Close();
				client = null;
				connectionMessage = null;
			}
		}

		protected override void receiveMessagesImpl()
		{
			if (client == null || !client.Connected)
			{
				return;
			}
			if (client.Connected && serverId == null)
			{
				serverId = client.LobbyOwner;
			}
			client.Receive(onReceiveConnection, onReceiveMessage, onReceiveDisconnect, onReceiveError);
			if (client != null)
			{
				client.Heartbeat(Enumerable.Repeat(serverId, 1));
				lastPingMs = client.GetPingWith(serverId);
				if (lastPingMs > 30000f)
				{
					timedOut = true;
					pendingDisconnect = Multiplayer.DisconnectType.GalaxyTimeout;
					disconnect();
				}
			}
		}

		protected virtual void onReceiveConnection(GalaxyID peer)
		{
		}

		protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
		{
			if (!(peer != serverId))
			{
				if (bandwidthLogger != null)
				{
					bandwidthLogger.RecordBytesDown(messageStream.Length);
				}
				using (IncomingMessage message = new IncomingMessage())
				{
					using (BinaryReader reader = new BinaryReader(messageStream))
					{
						message.Read(reader);
						processIncomingMessage(message);
					}
				}
			}
		}

		protected virtual void onReceiveDisconnect(GalaxyID peer)
		{
			if (peer != serverId)
			{
				Game1.multiplayer.playerDisconnected((long)peer.ToUint64());
				return;
			}
			timedOut = true;
			pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
		}

		protected virtual void onReceiveError(string messageKey)
		{
			connectionMessage = messageKey;
		}

		public override void sendMessage(OutgoingMessage message)
		{
			if (client != null && client.Connected && !(serverId == null))
			{
				if (bandwidthLogger != null)
				{
					using (MemoryStream stream = new MemoryStream())
					{
						using (BinaryWriter writer = new BinaryWriter(stream))
						{
							message.Write(writer);
							stream.Seek(0L, SeekOrigin.Begin);
							byte[] bytes = stream.ToArray();
							client.Send(serverId, bytes);
							bandwidthLogger.RecordBytesUp(bytes.Length);
						}
					}
				}
				else
				{
					client.Send(serverId, message);
				}
			}
		}
	}
}
