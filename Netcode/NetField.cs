using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Netcode
{
	public abstract class NetField<T, TSelf> : NetFieldBase<T, TSelf>, IEnumerable<T>, IEnumerable where TSelf : NetField<T, TSelf>
	{
		private bool xmlInitialized;

		public NetField()
		{
		}

		public NetField(T value)
			: base(value)
		{
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Enumerable.Repeat(Get(), 1).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(T value)
		{
			if (xmlInitialized || base.Parent != null)
			{
				throw new InvalidOperationException(GetType().Name + " already has value " + ToString());
			}
			cleanSet(value);
			xmlInitialized = true;
		}
	}
}
