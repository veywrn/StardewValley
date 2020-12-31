using System.Collections.Generic;

namespace Netcode
{
	public abstract class NetFieldDictionary<TKey, TValue, TField, TSerialDict, TSelf> : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> where TField : NetField<TValue, TField>, new()where TSerialDict : IDictionary<TKey, TValue>, new()where TSelf : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>
	{
		public NetFieldDictionary()
		{
		}

		public NetFieldDictionary(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
			: base(pairs)
		{
		}

		protected override void setFieldValue(TField field, TKey key, TValue value)
		{
			field.Value = value;
		}

		protected override TValue getFieldValue(TField field)
		{
			return field.Value;
		}

		protected override TValue getFieldTargetValue(TField field)
		{
			return field.TargetValue;
		}
	}
}
