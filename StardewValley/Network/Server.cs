using System;

namespace StardewValley.Network
{
	public abstract class Server : IBandwidthMonitor
	{
		protected IGameServer gameServer;

		protected BandwidthLogger bandwidthLogger;

		public abstract int connectionsCount
		{
			get;
		}

		public bool LogBandwidth
		{
			get
			{
				return bandwidthLogger != null;
			}
			set
			{
				if (value)
				{
					bandwidthLogger = new BandwidthLogger();
				}
				else
				{
					bandwidthLogger = null;
				}
			}
		}

		public BandwidthLogger BandwidthLogger => bandwidthLogger;

		public Server(IGameServer gameServer)
		{
			this.gameServer = gameServer;
		}

		public abstract void initialize();

		public abstract void setPrivacy(ServerPrivacy privacy);

		public abstract void stopServer();

		public abstract void receiveMessages();

		public abstract void sendMessage(long peerId, OutgoingMessage message);

		public abstract bool connected();

		public virtual bool canAcceptIPConnections()
		{
			return false;
		}

		public virtual bool canOfferInvite()
		{
			return false;
		}

		public virtual void offerInvite()
		{
		}

		public virtual string getInviteCode()
		{
			return null;
		}

		public virtual string getUserId(long farmerId)
		{
			return null;
		}

		public virtual bool hasUserId(string userId)
		{
			return false;
		}

		public virtual float getPingToClient(long farmerId)
		{
			return 0f;
		}

		public virtual bool isConnectionActive(string connectionId)
		{
			throw new NotImplementedException();
		}

		public virtual void onConnect(string connectionId)
		{
			gameServer.onConnect(connectionId);
		}

		public virtual void onDisconnect(string connectionId)
		{
			gameServer.onDisconnect(connectionId);
		}

		public abstract string getUserName(long farmerId);

		public abstract void setLobbyData(string key, string value);

		public virtual void kick(long disconnectee)
		{
		}

		public virtual void playerDisconnected(long disconnectee)
		{
			gameServer.playerDisconnected(disconnectee);
		}
	}
}
