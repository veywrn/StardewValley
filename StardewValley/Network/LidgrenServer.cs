using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public class LidgrenServer : Server
	{
		public const int defaultPort = 24642;

		protected NetServer server;

		private HashSet<NetConnection> introductionsSent = new HashSet<NetConnection>();

		protected Bimap<long, NetConnection> peers = new Bimap<long, NetConnection>();

		public override int connectionsCount
		{
			get
			{
				if (server == null)
				{
					return 0;
				}
				return server.ConnectionsCount;
			}
		}

		public LidgrenServer(IGameServer gameServer)
			: base(gameServer)
		{
		}

		public override bool isConnectionActive(string connectionID)
		{
			foreach (NetConnection connection in server.Connections)
			{
				if (getConnectionId(connection) == connectionID && connection.Status == NetConnectionStatus.Connected)
				{
					return true;
				}
			}
			return false;
		}

		public override string getUserId(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return peers[farmerId].RemoteEndPoint.Address.ToString();
		}

		public override bool hasUserId(string userId)
		{
			foreach (NetConnection rightValue in peers.RightValues)
			{
				if (rightValue.RemoteEndPoint.Address.ToString().Equals(userId))
				{
					return true;
				}
			}
			return false;
		}

		public override string getUserName(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return null;
			}
			return peers[farmerId].RemoteEndPoint.Address.ToString();
		}

		public override float getPingToClient(long farmerId)
		{
			if (!peers.ContainsLeft(farmerId))
			{
				return -1f;
			}
			return peers[farmerId].AverageRoundtripTime / 2f * 1000f;
		}

		public override void setPrivacy(ServerPrivacy privacy)
		{
		}

		public override bool canAcceptIPConnections()
		{
			return true;
		}

		public override bool connected()
		{
			return server != null;
		}

		public override void initialize()
		{
			Console.WriteLine("Starting LAN server");
			NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
			config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
			config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
			config.Port = 24642;
			config.ConnectionTimeout = 30f;
			config.PingInterval = 5f;
			config.MaximumConnections = Game1.multiplayer.playerLimit * 2;
			config.MaximumTransmissionUnit = 1200;
			server = new NetServer(config);
			server.Start();
		}

		public override void stopServer()
		{
			Console.WriteLine("Stopping LAN server");
			server.Shutdown("Server shutting down...");
			server.FlushSendQueue();
			introductionsSent.Clear();
			peers.Clear();
		}

		public override void receiveMessages()
		{
			NetIncomingMessage inc;
			while ((inc = server.ReadMessage()) != null)
			{
				if (bandwidthLogger != null)
				{
					bandwidthLogger.RecordBytesDown(inc.LengthBytes);
				}
				switch (inc.MessageType)
				{
				case NetIncomingMessageType.DiscoveryRequest:
					if (Game1.options.ipConnectionsEnabled && !gameServer.isUserBanned(inc.SenderEndPoint.Address.ToString()))
					{
						sendVersionInfo(inc);
					}
					break;
				case NetIncomingMessageType.ConnectionApproval:
					if (Game1.options.ipConnectionsEnabled)
					{
						inc.SenderConnection.Approve();
					}
					else
					{
						inc.SenderConnection.Deny();
					}
					break;
				case NetIncomingMessageType.Data:
					parseDataMessageFromClient(inc);
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
				default:
					Game1.debugOutput = inc.ToString();
					break;
				}
				server.Recycle(inc);
			}
			foreach (NetConnection conn in server.Connections)
			{
				if (conn.Status == NetConnectionStatus.Connected && !introductionsSent.Contains(conn))
				{
					if (!gameServer.whenGameAvailable(delegate
					{
						gameServer.sendAvailableFarmhands("", delegate(OutgoingMessage msg)
						{
							sendMessage(conn, msg);
						});
					}, () => Game1.gameMode != 6))
					{
						Console.WriteLine("Postponing introduction message");
						sendMessage(conn, new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostLoad"));
					}
					introductionsSent.Add(conn);
				}
			}
			if (bandwidthLogger != null)
			{
				bandwidthLogger.Update();
			}
		}

		private void sendVersionInfo(NetIncomingMessage message)
		{
			NetOutgoingMessage response = server.CreateMessage();
			response.Write(Game1.multiplayer.protocolVersion);
			response.Write("StardewValley");
			server.SendDiscoveryResponse(response, message.SenderEndPoint);
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesUp(response.LengthBytes);
			}
		}

		private void statusChanged(NetIncomingMessage message)
		{
			switch (message.ReadByte())
			{
			case 5:
				onConnect(getConnectionId(message.SenderConnection));
				break;
			case 6:
			case 7:
				onDisconnect(getConnectionId(message.SenderConnection));
				if (peers.ContainsRight(message.SenderConnection))
				{
					playerDisconnected(peers[message.SenderConnection]);
				}
				break;
			}
		}

		public override void kick(long disconnectee)
		{
			base.kick(disconnectee);
			if (peers.ContainsLeft(disconnectee))
			{
				peers[disconnectee].Disconnect(Multiplayer.kicked);
				server.FlushSendQueue();
				playerDisconnected(disconnectee);
			}
		}

		public override void playerDisconnected(long disconnectee)
		{
			base.playerDisconnected(disconnectee);
			introductionsSent.Remove(peers[disconnectee]);
			peers.RemoveLeft(disconnectee);
		}

		protected virtual void parseDataMessageFromClient(NetIncomingMessage dataMsg)
		{
			NetConnection peer = dataMsg.SenderConnection;
			using (IncomingMessage message = new IncomingMessage())
			{
				using (NetBufferReadStream stream = new NetBufferReadStream(dataMsg))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						while (dataMsg.LengthBits - dataMsg.Position >= 8)
						{
							message.Read(reader);
							if (peers.ContainsLeft(message.FarmerID) && peers[message.FarmerID] == peer)
							{
								gameServer.processIncomingMessage(message);
							}
							else if (message.MessageType == 2)
							{
								NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
								gameServer.checkFarmhandRequest("", getConnectionId(dataMsg.SenderConnection), farmer, delegate(OutgoingMessage msg)
								{
									sendMessage(peer, msg);
								}, delegate
								{
									peers[farmer.Value.UniqueMultiplayerID] = peer;
								});
							}
						}
					}
				}
			}
		}

		public string getConnectionId(NetConnection connection)
		{
			return "L_" + connection.RemoteUniqueIdentifier.ToString();
		}

		public override void sendMessage(long peerId, OutgoingMessage message)
		{
			if (peers.ContainsLeft(peerId))
			{
				sendMessage(peers[peerId], message);
			}
		}

		protected virtual void sendMessage(NetConnection connection, OutgoingMessage message)
		{
			NetOutgoingMessage msg = server.CreateMessage();
			using (NetBufferWriteStream stream = new NetBufferWriteStream(msg))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					message.Write(writer);
				}
			}
			server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
			if (bandwidthLogger != null)
			{
				bandwidthLogger.RecordBytesUp(msg.LengthBytes);
			}
		}

		public override void setLobbyData(string key, string value)
		{
		}
	}
}
