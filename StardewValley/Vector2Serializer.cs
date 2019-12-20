using Microsoft.Xna.Framework;
using System.Xml;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Vector2Serializer : XmlSerializer
	{
		private Vector2Reader _reader = new Vector2Reader();

		private Vector2Writer _writer = new Vector2Writer();

		protected override XmlSerializationReader CreateReader()
		{
			return _reader;
		}

		protected override XmlSerializationWriter CreateWriter()
		{
			return _writer;
		}

		public override bool CanDeserialize(XmlReader xmlReader)
		{
			return xmlReader.IsStartElement("Vector2");
		}

		protected override void Serialize(object o, XmlSerializationWriter writer)
		{
			_writer.WriteVector2((Vector2)o);
		}

		protected override object Deserialize(XmlSerializationReader reader)
		{
			return _reader.ReadVector2();
		}
	}
}
