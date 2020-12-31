using System.IO;

namespace Netcode
{
	public class NetEvent1Field<T, TField> : AbstractNetEvent1<T> where TField : NetField<T, TField>, new()
	{
		protected override T readEventArg(BinaryReader reader, NetVersion version)
		{
			TField val = new TField();
			val.ReadFull(reader, version);
			return val.Value;
		}

		protected override void writeEventArg(BinaryWriter writer, T eventArg)
		{
			TField val = new TField();
			val.Value = eventArg;
			val.WriteFull(writer);
		}
	}
}
