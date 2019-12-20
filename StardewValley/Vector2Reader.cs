using Microsoft.Xna.Framework;
using System.Xml;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Vector2Reader : XmlSerializationReader
	{
		public Vector2 ReadVector2()
		{
			XmlReader reader = base.Reader;
			reader.ReadStartElement("Vector2");
			reader.ReadStartElement("X");
			float x = reader.ReadContentAsFloat();
			reader.ReadEndElement();
			reader.ReadStartElement("Y");
			float y = reader.ReadContentAsFloat();
			reader.ReadEndElement();
			reader.ReadEndElement();
			return new Vector2(x, y);
		}

		protected override void InitCallbacks()
		{
		}

		protected override void InitIDs()
		{
		}
	}
}
