using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public class NetRoot<T> : NetRef<T>, INetRoot where T : class, INetObject<INetSerializable>
	{
		private Dictionary<long, int> connections = new Dictionary<long, int>();

		public NetClock Clock
		{
			get;
		} = new NetClock();


		public override bool Dirty => base.DirtyTick <= Clock.GetLocalTick();

		public NetRoot()
		{
			base.Root = this;
		}

		public NetRoot(T value)
			: this()
		{
			cleanSet(value);
		}

		public void TickTree()
		{
			Clock.Tick();
			Tick();
		}

		public override void Read(BinaryReader reader, NetVersion _)
		{
			NetVersion remoteVersion = default(NetVersion);
			remoteVersion.Read(reader);
			base.Read(reader, remoteVersion);
			Clock.netVersion.Merge(remoteVersion);
		}

		public void Read(BinaryReader reader)
		{
			NetVersion remoteVersion = default(NetVersion);
			remoteVersion.Read(reader);
			base.Read(reader, remoteVersion);
			Clock.netVersion.Merge(remoteVersion);
		}

		public override void Write(BinaryWriter writer)
		{
			Clock.netVersion.Write(writer);
			base.Write(writer);
			MarkClean();
		}

		public override void ReadFull(BinaryReader reader, NetVersion _)
		{
			base.ReadFull(reader, Clock.netVersion);
		}

		public static NetRoot<T> Connect(BinaryReader reader)
		{
			NetRoot<T> netRoot = new NetRoot<T>();
			netRoot.ReadConnectionPacket(reader);
			return netRoot;
		}

		public void ReadConnectionPacket(BinaryReader reader)
		{
			Clock.LocalId = reader.ReadByte();
			Clock.netVersion.Read(reader);
			base.ReadFull(reader, Clock.netVersion);
		}

		public void CreateConnectionPacket(BinaryWriter writer, long? connection)
		{
			int peerId;
			if (connection.HasValue && connections.ContainsKey(connection.Value))
			{
				peerId = connections[connection.Value];
			}
			else
			{
				peerId = Clock.AddNewPeer();
				if (connection.HasValue)
				{
					connections[connection.Value] = peerId;
				}
			}
			writer.Write((byte)peerId);
			Clock.netVersion.Write(writer);
			WriteFull(writer);
		}

		public void Disconnect(long connection)
		{
			if (connections.TryGetValue(connection, out int peerId))
			{
				Clock.RemovePeer(peerId);
			}
		}

		public virtual NetRoot<T> Clone()
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						WriteFull(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						NetRoot<T> netRoot = new NetRoot<T>();
						netRoot.Serializer = Serializer;
						netRoot.ReadFull(reader, Clock.netVersion);
						netRoot.reassigned.Set(default(NetVersion));
						netRoot.MarkClean();
						return netRoot;
					}
				}
			}
		}

		public void CloneInto(NetRef<T> netref)
		{
			NetRoot<T> netRoot = Clone();
			T copy = netRoot.Value;
			netRoot.Value = null;
			netref.Value = copy;
		}
	}
}
