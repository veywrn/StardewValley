using System.IO;

namespace Netcode
{
	public class NetEvent1<T> : AbstractNetEvent1<T> where T : NetEventArg, new()
	{
		protected override T readEventArg(BinaryReader reader, NetVersion version)
		{
			T arg = new T();
			arg.Read(reader);
			return arg;
		}

		protected override void writeEventArg(BinaryWriter writer, T eventArg)
		{
			eventArg.Write(writer);
		}
	}
}
