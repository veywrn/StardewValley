using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode
{
	public abstract class SerializationFacade<SerialT> : IEnumerable<SerialT>, IEnumerable
	{
		public SerializationFacade()
		{
		}

		protected abstract SerialT Serialize();

		protected abstract void Deserialize(SerialT serialValue);

		public IEnumerator<SerialT> GetEnumerator()
		{
			return Enumerable.Repeat(Serialize(), 1).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(SerialT value)
		{
			Deserialize(value);
		}
	}
}
