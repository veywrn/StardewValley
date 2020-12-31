using System.IO;

namespace Netcode
{
	public interface NetEventArg
	{
		void Read(BinaryReader reader);

		void Write(BinaryWriter writer);
	}
}
