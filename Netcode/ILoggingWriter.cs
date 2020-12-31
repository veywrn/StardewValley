namespace Netcode
{
	public interface ILoggingWriter
	{
		void Push(string name);

		void Pop();
	}
}
