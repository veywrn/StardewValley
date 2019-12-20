using Netcode;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace StardewValley.Characters
{
	public class BotchedNetField<T, TNet> : IXmlSerializable where TNet : NetField<T, TNet>
	{
		[XmlIgnore]
		public TNet netField;

		public T Value
		{
			get
			{
				return netField.Value;
			}
			set
			{
				netField.Value = value;
			}
		}

		public BotchedNetField()
		{
		}

		public BotchedNetField(TNet net_field)
		{
			netField = net_field;
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteValue(netField.Value);
		}

		protected virtual object _ParseValue(XmlReader reader)
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (isEmptyElement)
			{
				return;
			}
			if (reader.Value != null && reader.Value != "")
			{
				netField.Value = (T)_ParseValue(reader);
			}
			else
			{
				XmlReader subreader = reader.ReadSubtree();
				if (subreader != null)
				{
					subreader.MoveToContent();
					subreader.Read();
					netField.Value = (T)_ParseValue(subreader);
					while (reader.NodeType != XmlNodeType.EndElement)
					{
						reader.Read();
					}
					reader.ReadEndElement();
				}
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				reader.Read();
			}
			reader.ReadEndElement();
		}
	}
}
