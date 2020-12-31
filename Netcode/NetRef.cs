namespace Netcode
{
	public class NetRef<T> : NetExtendableRef<T, NetRef<T>> where T : class, INetObject<INetSerializable>
	{
		public NetRef()
		{
		}

		public NetRef(T value)
			: base(value)
		{
		}
	}
}
