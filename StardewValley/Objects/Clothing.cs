using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Clothing : Item
	{
		public enum ClothesType
		{
			SHIRT,
			PANTS,
			ACCESSORY
		}

		public const int SHIRT_SHEET_WIDTH = 128;

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheetMale = new NetInt();

		[XmlElement("indexInTileSheetFemale")]
		public readonly NetInt indexInTileSheetFemale = new NetInt();

		[XmlIgnore]
		public string description;

		[XmlIgnore]
		public string displayName;

		[XmlElement("clothesType")]
		public readonly NetInt clothesType = new NetInt();

		[XmlElement("dyeable")]
		public readonly NetBool dyeable = new NetBool(value: false);

		[XmlElement("clothesColor")]
		public readonly NetColor clothesColor = new NetColor(new Color(255, 255, 255));

		[XmlElement("otherData")]
		public readonly NetString otherData = new NetString("");

		protected List<string> _otherDataList;

		[XmlElement("isPrismatic")]
		public readonly NetBool isPrismatic = new NetBool(value: false);

		[XmlIgnore]
		protected bool _loadedData;

		protected static int _maxShirtValue = -1;

		protected static int _maxPantsValue = -1;

		public int Price
		{
			get
			{
				return price.Value;
			}
			set
			{
				price.Value = value;
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (!_loadedData)
				{
					LoadData();
				}
				return displayName;
			}
			set
			{
				displayName = value;
			}
		}

		[XmlIgnore]
		public override int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public Clothing()
		{
			base.Category = -100;
			base.NetFields.AddFields(price, indexInTileSheetMale, indexInTileSheetFemale, clothesType, dyeable, clothesColor, otherData, isPrismatic);
		}

		public static int GetMaxShirtValue()
		{
			if (_maxShirtValue < 0)
			{
				foreach (string data in Game1.clothingInformation.Values)
				{
					if (data != null)
					{
						string[] split = data.Split('/');
						if (split.Length >= 9 && !(split[8] != "Shirt"))
						{
							int value_male = int.Parse(split[3]);
							int value_female = int.Parse(split[4]);
							if (_maxShirtValue < value_male)
							{
								_maxShirtValue = value_male;
							}
							if (_maxShirtValue < value_female)
							{
								_maxShirtValue = value_female;
							}
						}
					}
				}
			}
			return _maxShirtValue;
		}

		public static int GetMaxPantsValue()
		{
			if (_maxPantsValue < 0)
			{
				foreach (string data in Game1.clothingInformation.Values)
				{
					if (data != null)
					{
						string[] split = data.Split('/');
						if (split.Length >= 9 && !(split[8] != "Pants"))
						{
							int value_male = int.Parse(split[3]);
							int value_female = int.Parse(split[4]);
							if (_maxPantsValue < value_male)
							{
								_maxPantsValue = value_male;
							}
							if (_maxPantsValue < value_male)
							{
								_maxPantsValue = value_female;
							}
						}
					}
				}
			}
			return _maxPantsValue;
		}

		public List<string> GetOtherData()
		{
			if (otherData.Value != null)
			{
				if (_otherDataList == null)
				{
					_otherDataList = new List<string>(otherData.Value.Split(','));
					for (int i = 0; i < _otherDataList.Count; i++)
					{
						if (_otherDataList[i].Trim() == "")
						{
							_otherDataList.RemoveAt(i);
							i--;
						}
					}
				}
				return _otherDataList;
			}
			return new List<string>();
		}

		public Clothing(int item_index)
			: this()
		{
			Name = "Clothing";
			base.Category = -100;
			base.ParentSheetIndex = item_index;
			LoadData(initialize_color: true);
		}

		public void LoadData(bool initialize_color = false)
		{
			if (_loadedData)
			{
				return;
			}
			int item_index = base.ParentSheetIndex;
			base.Category = -100;
			if (Game1.clothingInformation.ContainsKey(item_index))
			{
				string[] data2 = Game1.clothingInformation[item_index].Split('/');
				Name = data2[0];
				price.Value = Convert.ToInt32(data2[5]);
				indexInTileSheetMale.Value = Convert.ToInt32(data2[3]);
				indexInTileSheetFemale.Value = Convert.ToInt32(data2[4]);
				dyeable.Value = Convert.ToBoolean(data2[7]);
				if (initialize_color)
				{
					string[] rgb_string = data2[6].Split(' ');
					clothesColor.Value = new Color(Convert.ToInt32(rgb_string[0]), Convert.ToInt32(rgb_string[1]), Convert.ToInt32(rgb_string[2]));
				}
				displayName = data2[1];
				description = data2[2];
				switch (data2[8].ToLower().Trim())
				{
				case "pants":
					clothesType.Set(1);
					break;
				case "shirt":
					clothesType.Set(0);
					break;
				case "accessory":
					clothesType.Set(2);
					break;
				}
				if (data2.Length >= 10)
				{
					otherData.Set(data2[9]);
				}
				else
				{
					otherData.Set("");
				}
				if (GetOtherData().Contains("Prismatic"))
				{
					isPrismatic.Set(newValue: true);
				}
			}
			else
			{
				base.ParentSheetIndex = item_index;
				string[] data = Game1.clothingInformation[-1].Split('/');
				clothesType.Set(1);
				if (item_index >= 1000)
				{
					data = Game1.clothingInformation[-2].Split('/');
					clothesType.Set(0);
					item_index -= 1000;
				}
				if (initialize_color)
				{
					clothesColor.Set(new Color(1f, 1f, 1f));
				}
				if (clothesType.Value == 1)
				{
					dyeable.Set(newValue: true);
				}
				else
				{
					dyeable.Set(newValue: false);
				}
				displayName = data[1];
				description = data[2];
				indexInTileSheetMale.Set(item_index);
				indexInTileSheetFemale.Set(-1);
			}
			if (dyeable.Value)
			{
				description = description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Clothes_Dyeable");
			}
			_loadedData = true;
		}

		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:category_clothes");
		}

		public override int salePrice()
		{
			return price;
		}

		public void Dye(Color color, float strength = 0.5f)
		{
			if (dyeable.Value)
			{
				Color current_color = clothesColor.Value;
				clothesColor.Value = new Color(Utility.MoveTowards((float)(int)current_color.R / 255f, (float)(int)color.R / 255f, strength), Utility.MoveTowards((float)(int)current_color.G / 255f, (float)(int)color.G / 255f, strength), Utility.MoveTowards((float)(int)current_color.B / 255f, (float)(int)color.B / 255f, strength), Utility.MoveTowards((float)(int)current_color.A / 255f, (float)(int)color.A / 255f, strength));
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			Color clothes_color = clothesColor;
			if (isPrismatic.Value)
			{
				clothes_color = Utility.GetPrismaticColor();
			}
			if (clothesType.Value == 0)
			{
				float dye_portion_layer_offset = 1E-07f;
				if (layerDepth >= 1f - dye_portion_layer_offset)
				{
					layerDepth = 1f - dye_portion_layer_offset;
				}
				spriteBatch.Draw(FarmerRenderer.shirtsTexture, location + new Vector2(32f, 32f), new Rectangle(indexInTileSheetMale.Value * 8 % 128, indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8), color * transparency, 0f, new Vector2(4f, 4f), scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(FarmerRenderer.shirtsTexture, location + new Vector2(32f, 32f), new Rectangle(indexInTileSheetMale.Value * 8 % 128 + 128, indexInTileSheetMale.Value * 8 / 128 * 32, 8, 8), Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(4f, 4f), scaleSize * 4f, SpriteEffects.None, layerDepth + dye_portion_layer_offset);
			}
			else if (clothesType.Value == 1)
			{
				spriteBatch.Draw(FarmerRenderer.pantsTexture, location + new Vector2(32f, 32f), new Rectangle(192 * (indexInTileSheetMale.Value % (FarmerRenderer.pantsTexture.Width / 192)), 688 * (indexInTileSheetMale.Value / (FarmerRenderer.pantsTexture.Width / 192)) + 672, 16, 16), Utility.MultiplyColor(clothes_color, color) * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			}
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override string getDescription()
		{
			if (!_loadedData)
			{
				LoadData();
			}
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override Item getOne()
		{
			Clothing clothing = new Clothing(base.ParentSheetIndex);
			clothing.clothesColor.Value = clothesColor.Value;
			clothing._GetOneFrom(this);
			return clothing;
		}
	}
}
