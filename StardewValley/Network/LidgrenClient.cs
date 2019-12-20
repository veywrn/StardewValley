using Lidgren.Network;
using System;
using System.IO;

namespace StardewValley.Network
{
	public class LidgrenClient : Client
	{
		private string address;

		private NetClient client;

		private bool serverDiscovered;

		private int maxRetryAttempts;

		private int retryMs = 10000;

		private double lastAttemptMs;

		private int retryAttempts;

		private float lastLatencyMs;

		public LidgrenClient(string address)
		{
			this.address = address;
		}

		public override string getUserID()
		{
			return "";
		}

		public override float GetPingToHost()
		{
			return lastLatencyMs / 2f;
		}

		protected override string getHostUserName()
		{
			return client.ServerConnection.RemoteEndPoint.Address.ToString();
		}

		protected override void connectImpl()
		{
			NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
			config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
			config.ConnectionTimeout = 30f;
			config.PingInterval = 5f;
			config.MaximumTransmissionUnit = 1200;
			client = new NetClient(config);
			client.Start();
			attemptConnection();
		}

		private void attemptConnection()
		{
			int port = 24642;
			if (address.Contains(":"))
			{
				string[] split = address.Split(':');
				address = split[0];
				port = Convert.ToInt32(split[1]);
			}
			client.DiscoverKnownPeer(address, port);
			lastAttemptMs = DateTime.Now.TimeOfDay.TotalMilliseconds;
		}

		public override void disconnect(bool neatly = true)
		{
			if (client.ConnectionStatus != NetConnectionStatus.Disconnected && client.ConnectionStatus != NetConnectionStatus.Disconnecting)
			{
				if (neatly)
				{
					sendMessage(new OutgoingMessage(19, Game1.player));
				}
				client.FlushSendQueue();
				client.Disconnect("");
				client.FlushSendQueue();
			}
			connectionMessage = null;
		}

		protected virtual bool validateProtocol(string version)
		{
			return version == Game1.multiplayer.protocolVersion;
		}

		protected override void receiveMessagesImpl()
		{
			if (client != null && !serverDiscovered && DateTime.Now.TimeOfDay.TotalMilliseconds >= lastAttemptMs + (double)retryMs && retryAttempts < maxRetryAttempts)
			{
				attemptConnection();
				retryAttempts++;
			}
			NetIncomingMessage inc;
			while ((inc = client.ReadMessage()) != null)
			{
				switch (inc.MessageType)
				{
				case NetIncomingMessageType.ConnectionLatencyUpdated:
					readLatency(inc);
					break;
				case NetIncomingMessageType.DiscoveryResponse:
					if (!serverDiscovered)
					{
						Console.WriteLine("Found server at " + inc.SenderEndPoint);
						string protocolVersion = inc.ReadString();
						if (validateProtocol(protocolVersion))
						{
							serverName = inc.ReadString();
							receiveHandshake(inc);
							serverDiscovered = true;
						}
						else
						{
							connectionMessage = "Strings\\UI:CoopMenu_FailedProtocolVersion";
							client.Disconnect("");
						}
					}
					break;
				case NetIncomingMessageType.Data:
					parseDataMessageFromServer(inc);
					break;
				case NetIncomingMessageType.DebugMessage:
				case NetIncomingMessageType.WarningMessage:
				case NetIncomingMessageType.ErrorMessage:
				{
					string message = inc.ReadString();
					Console.WriteLine("{0}: {1}", inc.MessageType, message);
					Game1.debugOutput = message;
					break;
				}
				case NetIncomingMessageType.StatusChanged:
					statusChanged(inc);
					break;
				}
			}
			if (client.ServerConnection != null && DateTime.Now.Second % 2 == 0)
			{
				Game1.debugOutput = "Ping: " + client.ServerConnection.AverageRoundtripTime * 1000f + "ms";
			}
		}

		private void readLatency(NetIncomingMessage msg)
		{
			lastLatencyMs = msg.ReadFloat() * 1000f;
		}

		private void receiveHandshake(NetIncomingMessage msg)
		{
			client.Connect(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
		}

		private void statusChanged(NetIncomingMessage message)
		{
			NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
			if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
			{
				string byeMessage = message.ReadString();
				clientRemotelyDisconnected(status, byeMessage);
			}
		}

		private void clientRemotelyDisconnected(NetConnectionStatus status, string message)
		{
			timedOut = true;
			if (status == NetConnectionStatus.Disconnected)
			{
				if (message == Multiplayer.kicked)
				{
					pendingDisconnect = Multiplayer.DisconnectType.Kicked;
				}
				else
				{
					pendingDisconnect = Multiplayer.DisconnectType.LidgrenTimeout;
				}
			}
			else
			{
				pendingDisconnect = Multiplayer.DisconnectType.LidgrenDisconnect_Unknown;
			}
		}

		public override void sendMessage(OutgoingMessage message)
		{
			NetOutgoingMessage sendMsg = client.CreateMessage();
			using (NetBufferWriteStream stream = new NetBufferWriteStream(sendMsg))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					message.Write(writer);
				}
			}
			client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesUp(sendMsg.LengthBytes);
			}
		}

		private void parseDataMessageFromServer(NetIncomingMessage dataMsg)
		{
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesDown(dataMsg.LengthBytes);
			}
			using (IncomingMessage message = new IncomingMessage())
			{
				using (NetBufferReadStream stream = new NetBufferReadStream(dataMsg))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						while (dataMsg.LengthBits - dataMsg.Position >= 8)
						{
							message.Read(reader);
							processIncomingMessage(message);
						}
					}
				}
			}
		}
	}
}
