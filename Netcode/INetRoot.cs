namespace Netcode
{
	public interface INetRoot
	{
		NetClock Clock
		{
			get;
		}

		void TickTree();

		void Disconnect(long connection);
	}
}
