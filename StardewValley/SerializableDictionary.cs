using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlRoot("dictionary")]
	public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		public struct ChangeArgs
		{
			public readonly ChangeType Type;

			public readonly TKey Key;

			public readonly TValue Value;

			public ChangeArgs(ChangeType type, TKey k, TValue v)
			{
				Type = type;
				Key = k;
				Value = v;
			}
		}

		public delegate void ChangeCallback(object sender, ChangeArgs args);

		private static XmlSerializer _keySerializer;

		private static XmlSerializer _valueSerializer;

		public event ChangeCallback CollectionChanged;

		static SerializableDictionary()
		{
			_keySerializer = SaveGame.GetSerializer(typeof(TKey));
			_valueSerializer = SaveGame.GetSerializer(typeof(TValue));
		}

		public new void Add(TKey key, TValue value)
		{
			base.Add(key, value);
			OnCollectionChanged(this, new ChangeArgs(ChangeType.Add, key, value));
		}

		public new bool Remove(TKey key)
		{
			if (TryGetValue(key, out TValue val))
			{
				base.Remove(key);
				OnCollectionChanged(this, new ChangeArgs(ChangeType.Remove, key, val));
				return true;
			}
			return false;
		}

		public new void Clear()
		{
			base.Clear();
			OnCollectionChanged(this, new ChangeArgs(ChangeType.Clear, default(TKey), default(TValue)));
		}

		private void OnCollectionChanged(object sender, ChangeArgs args)
		{
			this.CollectionChanged?.Invoke(sender ?? this, args);
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (!isEmptyElement)
			{
				while (reader.NodeType != XmlNodeType.EndElement)
				{
					reader.ReadStartElement("item");
					reader.ReadStartElement("key");
					TKey key = (TKey)_keySerializer.Deserialize(reader);
					reader.ReadEndElement();
					reader.ReadStartElement("value");
					TValue value = (TValue)_valueSerializer.Deserialize(reader);
					reader.ReadEndElement();
					base.Add(key, value);
					reader.ReadEndElement();
					reader.MoveToContent();
				}
				reader.ReadEndElement();
			}
		}

		public void WriteXml(XmlWriter writer)
		{
			foreach (TKey key in base.Keys)
			{
				writer.WriteStartElement("item");
				writer.WriteStartElement("key");
				_keySerializer.Serialize(writer, key);
				writer.WriteEndElement();
				writer.WriteStartElement("value");
				TValue value = base[key];
				_valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
		}
	}
}
