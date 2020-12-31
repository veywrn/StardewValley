namespace Netcode
{
	public interface INetObject<out T> where T : INetSerializable
	{
		T NetFields
		{
			get;
		}
	}
}
