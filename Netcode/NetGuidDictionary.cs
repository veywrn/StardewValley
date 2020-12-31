using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public class NetGuidDictionary<T, TField> : NetFieldDictionary<Guid, T, TField, Dictionary<Guid, T>, NetGuidDictionary<T, TField>> where TField : NetField<T, TField>, new()
	{
		public NetGuidDictionary()
		{
		}

		public NetGuidDictionary(IEnumerable<KeyValuePair<Guid, T>> pairs)
			: base(pairs)
		{
		}

		protected override Guid ReadKey(BinaryReader reader)
		{
			return reader.ReadGuid();
		}

		protected override void WriteKey(BinaryWriter writer, Guid key)
		{
			writer.WriteGuid(key);
		}
	}
}
