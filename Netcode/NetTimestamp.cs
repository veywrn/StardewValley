using System.IO;

namespace Netcode
{
	public struct NetTimestamp
	{
		public int PeerId;

		public uint Tick;

		public void Read(BinaryReader reader)
		{
			PeerId = reader.ReadByte();
			Tick = reader.ReadUInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((byte)PeerId);
			writer.Write(Tick);
		}

		public override string ToString()
		{
			return "v" + Tick + "@" + PeerId;
		}
	}
}
