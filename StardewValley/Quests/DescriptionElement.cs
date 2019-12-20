using Netcode;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class DescriptionElement : INetObject<NetFields>
	{
		public static XmlSerializer serializer = new XmlSerializer(typeof(DescriptionElement), new Type[3]
		{
			typeof(Monster),
			typeof(NPC),
			typeof(Object)
		});

		public string xmlKey;

		public List<object> param;

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public static implicit operator DescriptionElement(string key)
		{
			return new DescriptionElement(key);
		}

		public DescriptionElement()
		{
			xmlKey = string.Empty;
			param = new List<object>();
		}

		public DescriptionElement(string key)
		{
			xmlKey = key;
			param = new List<object>();
		}

		public DescriptionElement(string key, object param1)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
		}

		public DescriptionElement(string key, List<object> paramlist)
		{
			xmlKey = key;
			param = new List<object>();
			foreach (object o in paramlist)
			{
				param.Add(o);
			}
		}

		public DescriptionElement(string key, object param1, object param2)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
			param.Add(param2);
		}

		public DescriptionElement(string key, object param1, object param2, object param3)
		{
			xmlKey = key;
			param = new List<object>();
			param.Add(param1);
			param.Add(param2);
			param.Add(param3);
		}

		public string loadDescriptionElement()
		{
			DescriptionElement temp = new DescriptionElement(xmlKey, param);
			string returnString3 = "";
			for (int i = 0; i < temp.param.Count; i++)
			{
				if (temp.param[i] is DescriptionElement)
				{
					DescriptionElement d3 = temp.param[i] as DescriptionElement;
					temp.param[i] = d3.loadDescriptionElement();
				}
				if (temp.param[i] is Object)
				{
					Game1.objectInformation.TryGetValue((temp.param[i] as Object).parentSheetIndex, out string objectInformation);
					temp.param[i] = objectInformation.Split('/')[4];
				}
				if (temp.param[i] is Monster)
				{
					DescriptionElement d2;
					if ((temp.param[i] as Monster).name.Equals("Frost Jelly"))
					{
						d2 = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13772");
						temp.param[i] = d2.loadDescriptionElement();
					}
					else
					{
						d2 = new DescriptionElement("Data\\Monsters:" + (temp.param[i] as Monster).name);
						temp.param[i] = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? (d2.loadDescriptionElement().Split('/').Last() + "s") : d2.loadDescriptionElement().Split('/').Last());
					}
					temp.param[i] = d2.loadDescriptionElement().Split('/').Last();
				}
				if (temp.param[i] is NPC)
				{
					DescriptionElement d = new DescriptionElement("Data\\NPCDispositions:" + (temp.param[i] as NPC).name);
					temp.param[i] = d.loadDescriptionElement().Split('/').Last();
				}
			}
			if (temp.xmlKey == "")
			{
				return string.Empty;
			}
			switch (temp.param.Count)
			{
			default:
				returnString3 = Game1.content.LoadString(temp.xmlKey);
				if (xmlKey.Contains("Dialogue.cs.7") || xmlKey.Contains("Dialogue.cs.8"))
				{
					returnString3 = Game1.content.LoadString(temp.xmlKey).Replace("/", " ");
					returnString3 = ((returnString3[0] == ' ') ? returnString3.Substring(1) : returnString3);
				}
				break;
			case 1:
				returnString3 = Game1.content.LoadString(temp.xmlKey, temp.param[0]);
				break;
			case 2:
				returnString3 = Game1.content.LoadString(temp.xmlKey, temp.param[0], temp.param[1]);
				break;
			case 3:
				returnString3 = Game1.content.LoadString(temp.xmlKey, temp.param[0], temp.param[1], temp.param[2]);
				break;
			case 4:
				returnString3 = Game1.content.LoadString(temp.xmlKey, temp.param[0], temp.param[1], temp.param[2], temp.param[3]);
				break;
			}
			return returnString3;
		}
	}
}
