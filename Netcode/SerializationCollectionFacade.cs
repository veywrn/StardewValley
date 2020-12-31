using System.Collections;
using System.Collections.Generic;

namespace Netcode
{
	public abstract class SerializationCollectionFacade<SerialT> : IEnumerable<SerialT>, IEnumerable
	{
		public SerializationCollectionFacade()
		{
		}

		protected abstract List<SerialT> Serialize();

		protected abstract void DeserializeAdd(SerialT serialElem);

		public IEnumerator<SerialT> GetEnumerator()
		{
			return Serialize().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(SerialT value)
		{
			DeserializeAdd(value);
		}
	}
}
