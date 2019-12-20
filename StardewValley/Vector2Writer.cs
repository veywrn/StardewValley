using Microsoft.Xna.Framework;
using System.Xml;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Vector2Writer : XmlSerializationWriter
	{
		public void WriteVector2(Vector2 vec)
		{
			XmlWriter writer = base.Writer;
			writer.WriteStartElement("Vector2");
			writer.WriteStartElement("X");
			writer.WriteValue(vec.X);
			writer.WriteEndElement();
			writer.WriteStartElement("Y");
			writer.WriteValue(vec.Y);
			writer.WriteEndElement();
			writer.WriteEndElement();
		}

		protected override void InitCallbacks()
		{
		}
	}
}
