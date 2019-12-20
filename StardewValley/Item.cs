using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlInclude(typeof(Object))]
	[XmlInclude(typeof(Tool))]
	public abstract class Item : IComparable, INetObject<NetFields>, ISalable
	{
		public bool isLostItem;

		[XmlElement("specialVariable")]
		private readonly NetInt specialVariable = new NetInt();

		[XmlElement("category")]
		public readonly NetInt category = new NetInt();

		[XmlElement("hasBeenInInventory")]
		public readonly NetBool hasbeenInInventory = new NetBool();

		private HashSet<string> _contextTags;

		protected bool _contextTagsDirty;

		[XmlElement("name")]
		public readonly NetString netName = new NetString();

		[XmlElement("parentSheetIndex")]
		public readonly NetInt parentSheetIndex = new NetInt();

		public bool specialItem;

		public int SpecialVariable
		{
			get
			{
				return specialVariable;
			}
			set
			{
				specialVariable.Set(value);
			}
		}

		[XmlIgnore]
		public int Category
		{
			get
			{
				return category;
			}
			set
			{
				category.Set(value);
			}
		}

		[XmlIgnore]
		public bool HasBeenInInventory
		{
			get
			{
				return hasbeenInInventory;
			}
			set
			{
				hasbeenInInventory.Set(value);
			}
		}

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		[XmlIgnore]
		public int ParentSheetIndex
		{
			get
			{
				return parentSheetIndex;
			}
			set
			{
				parentSheetIndex.Value = value;
			}
		}

		public abstract string DisplayName
		{
			get;
			set;
		}

		public virtual string Name
		{
			get
			{
				return netName.Value;
			}
			set
			{
				netName.Value = value;
			}
		}

		public abstract int Stack
		{
			get;
			set;
		}

		public bool IsInfiniteStock()
		{
			if (isLostItem)
			{
				return true;
			}
			return false;
		}

		public List<string> GetContextTagList()
		{
			return GetContextTags().ToList();
		}

		public HashSet<string> GetContextTags()
		{
			if (_contextTags == null || _contextTagsDirty)
			{
				_GenerateContextTags();
			}
			return _contextTags;
		}

		public bool HasContextTag(string tag)
		{
			bool match = true;
			if (tag.Length > 1 && tag[0] == '!')
			{
				tag = tag.Substring(1);
				match = false;
			}
			return GetContextTags().Contains(tag) == match;
		}

		protected void _GenerateContextTags()
		{
			_contextTagsDirty = false;
			_contextTags = new HashSet<string>();
			string alternate_id2 = "id_" + Utility.getStandardDescriptionFromItem(this, 0, '_');
			alternate_id2 = alternate_id2.Substring(0, alternate_id2.Length - 2).ToLower();
			_contextTags.Add(alternate_id2);
			if (Name != null && Game1.objectContextTags.ContainsKey(Name))
			{
				string[] array = Game1.objectContextTags[Name].Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					string trimmed_tag2 = array[i].Trim();
					if (trimmed_tag2.Length > 0)
					{
						_contextTags.Add(trimmed_tag2);
					}
				}
			}
			else if (Name != null && Game1.objectContextTags.ContainsKey(alternate_id2))
			{
				string[] array = Game1.objectContextTags[alternate_id2].Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					string trimmed_tag = array[i].Trim();
					if (trimmed_tag.Length > 0)
					{
						_contextTags.Add(trimmed_tag);
					}
				}
			}
			_PopulateContextTags(_contextTags);
		}

		protected virtual void _PopulateContextTags(HashSet<string> tags)
		{
			if (Name != null)
			{
				tags.Add("item_" + SanitizeContextTag(Name));
			}
			switch (category.Value)
			{
			case -26:
				tags.Add("category_artisan_goods");
				break;
			case -21:
				tags.Add("category_bait");
				break;
			case -9:
				tags.Add("category_big_craftable");
				break;
			case -97:
				tags.Add("category_boots");
				break;
			case -100:
				tags.Add("category_clothing");
				break;
			case -7:
				tags.Add("category_cooking");
				break;
			case -8:
				tags.Add("category_crafting");
				break;
			case -5:
				tags.Add("category_egg");
				break;
			case -29:
				tags.Add("category_equipment");
				break;
			case -19:
				tags.Add("category_fertilizer");
				break;
			case -4:
				tags.Add("category_fish");
				_PopulateFishContextTags(tags);
				break;
			case -80:
				tags.Add("category_flowers");
				break;
			case -79:
				tags.Add("category_fruits");
				break;
			case -24:
				tags.Add("category_furniture");
				break;
			case -2:
				tags.Add("category_gem");
				break;
			case -81:
				tags.Add("category_greens");
				break;
			case -95:
				tags.Add("category_hat");
				break;
			case -25:
				tags.Add("category_ingredients");
				break;
			case -20:
				tags.Add("category_junk");
				break;
			case -14:
				tags.Add("category_meat");
				break;
			case -6:
				tags.Add("category_milk");
				break;
			case -12:
				tags.Add("category_minerals");
				break;
			case -28:
				tags.Add("category_monster_loot");
				break;
			case -96:
				tags.Add("category_ring");
				break;
			case -74:
				tags.Add("category_seeds");
				break;
			case -23:
				tags.Add("category_sell_at_fish_shop");
				break;
			case -27:
				tags.Add("category_syrup");
				break;
			case -22:
				tags.Add("category_tackle");
				break;
			case -99:
				tags.Add("category_tool");
				break;
			case -75:
				tags.Add("category_vegetable");
				break;
			case -98:
				tags.Add("category_weapon");
				break;
			case -17:
				tags.Add("category_sell_at_pierres");
				break;
			case -18:
				tags.Add("category_sell_at_pierres_and_marnies");
				break;
			case -15:
				tags.Add("category_metal_resources");
				break;
			case -16:
				tags.Add("category_building_resources");
				break;
			}
		}

		protected void _PopulateFishContextTags(HashSet<string> tags)
		{
			Dictionary<int, string> data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			if (!data.ContainsKey(ParentSheetIndex))
			{
				return;
			}
			string[] split = data[ParentSheetIndex].Split('/');
			if (split[1] == "trap")
			{
				tags.Add("fish_trap_location_" + split[4]);
				return;
			}
			tags.Add("fish_motion_" + split[2]);
			int difficulty = Convert.ToInt32(split[1]);
			if (difficulty <= 33)
			{
				tags.Add("fish_difficulty_easy");
			}
			else if (difficulty <= 66)
			{
				tags.Add("fish_difficulty_medium");
			}
			else if (difficulty <= 100)
			{
				tags.Add("fish_difficulty_hard");
			}
			else
			{
				tags.Add("fish_difficulty_extremely_hard");
			}
			tags.Add("fish_favor_weather_" + split[7]);
		}

		public string SanitizeContextTag(string tag)
		{
			return tag.Trim().ToLower().Replace(' ', '_')
				.Replace("'", "");
		}

		protected Item()
		{
			NetFields.AddFields(specialVariable, category, netName, parentSheetIndex, hasbeenInInventory);
			parentSheetIndex.Value = -1;
		}

		public virtual bool ShouldSerializeparentSheetIndex()
		{
			return parentSheetIndex.Value != -1;
		}

		public virtual void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, string overrideText)
		{
			if (!string.IsNullOrEmpty(overrideText) && overrideText != " ")
			{
				spriteBatch.DrawString(font, overrideText, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 2f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2(x + 16, y + 16 + 4) + new Vector2(0f, 2f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2(x + 16, y + 16 + 4) + new Vector2(2f, 0f), Game1.textShadowColor * alpha);
				spriteBatch.DrawString(font, overrideText, new Vector2(x + 16, y + 16 + 4), Game1.textColor * 0.9f * alpha);
				y += (int)font.MeasureString(overrideText).Y + 4;
			}
		}

		public virtual Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, string descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			return Point.Zero;
		}

		public abstract void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow);

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber)
		{
			drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, Color.White, drawShadow: true);
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth)
		{
			drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, StackDrawType.Draw, Color.White, drawShadow: true);
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize)
		{
			drawInMenu(spriteBatch, location, scaleSize, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
		}

		public abstract int maximumStackSize();

		public abstract int addToStack(Item stack);

		public abstract string getDescription();

		public abstract bool isPlaceable();

		public virtual int salePrice()
		{
			return -1;
		}

		public virtual bool canBeTrashed()
		{
			if (this is Tool && (!(this is MeleeWeapon) || (this as MeleeWeapon).isScythe()) && !(this is FishingRod) && !(this is Pan))
			{
				return this is Slingshot;
			}
			return true;
		}

		public virtual bool canBePlacedInWater()
		{
			return false;
		}

		public virtual bool actionWhenPurchased()
		{
			if (isLostItem)
			{
				Game1.player.itemsLostLastDeath.Clear();
				isLostItem = false;
				Game1.player.recoveredItem = this;
				Game1.player.mailReceived.Remove("MarlonRecovery");
				Game1.addMailForTomorrow("MarlonRecovery");
				Game1.playSound("newArtifact");
				Game1.exitActiveMenu();
				bool use_plural = Stack > 1;
				Game1.drawDialogue(Game1.getCharacterFromName("Marlon"), Game1.content.LoadString(use_plural ? "Strings\\StringsFromCSFiles:ItemRecovery_Engaged_Stack" : "Strings\\StringsFromCSFiles:ItemRecovery_Engaged", Lexicon.makePlural(DisplayName, !use_plural)));
				return true;
			}
			return false;
		}

		public virtual bool CanBuyItem(Farmer who)
		{
			return Game1.player.couldInventoryAcceptThisItem(this);
		}

		public virtual bool canBeDropped()
		{
			return true;
		}

		public virtual void actionWhenBeingHeld(Farmer who)
		{
		}

		public virtual void actionWhenStopBeingHeld(Farmer who)
		{
		}

		public int getRemainingStackSpace()
		{
			return maximumStackSize() - Stack;
		}

		public virtual int healthRecoveredOnConsumption()
		{
			return 0;
		}

		public virtual int staminaRecoveredOnConsumption()
		{
			return 0;
		}

		public virtual string getHoverBoxText(Item hoveredItem)
		{
			return null;
		}

		public virtual bool canBeGivenAsGift()
		{
			return false;
		}

		public virtual void drawAttachments(SpriteBatch b, int x, int y)
		{
		}

		public virtual bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			return false;
		}

		public virtual int attachmentSlots()
		{
			return 0;
		}

		public virtual string getCategoryName()
		{
			if (this is Boots)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Item.cs.3829");
			}
			return "";
		}

		public virtual Color getCategoryColor()
		{
			return Color.Black;
		}

		public virtual bool canStackWith(ISalable other)
		{
			if (other == null)
			{
				return false;
			}
			if ((other is Object && this is Object) || (other is ColoredObject && this is ColoredObject))
			{
				if (this is ColoredObject && other is ColoredObject && !(this as ColoredObject).color.Value.Equals((other as ColoredObject).color.Value))
				{
					return false;
				}
				if (maximumStackSize() > 1 && other.maximumStackSize() > 1 && (this as Object).ParentSheetIndex == (other as Object).ParentSheetIndex && (this as Object).bigCraftable.Value == (other as Object).bigCraftable.Value && (this as Object).quality.Value == (other as Object).quality.Value)
				{
					return Name.Equals(other.Name);
				}
				return false;
			}
			return false;
		}

		public virtual string checkForSpecialItemHoldUpMeessage()
		{
			return null;
		}

		public abstract Item getOne();

		public ISalable GetSalableInstance()
		{
			return getOne();
		}

		public virtual int CompareTo(object obj)
		{
			if (obj is Item)
			{
				if ((obj as Item).Category == Category)
				{
					if ((obj as Item).Name.Equals(Name) && (obj as Item).ParentSheetIndex == ParentSheetIndex)
					{
						return Stack - (obj as Item).Stack;
					}
					if (this is Object && obj is Object)
					{
						return string.Compare((this as Object).type + Name, (obj as Object).type + (obj as Item).Name);
					}
					return string.Compare(Name, (obj as Item).Name);
				}
				return (obj as Item).getCategorySortValue() - getCategorySortValue();
			}
			return 0;
		}

		public int getCategorySortValue()
		{
			if (Category == -100)
			{
				return -94;
			}
			return Category;
		}

		protected virtual int getDescriptionWidth()
		{
			int minimum_size = 272;
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr)
			{
				minimum_size = 384;
			}
			return Math.Max(minimum_size, (int)Game1.dialogueFont.MeasureString((DisplayName == null) ? "" : DisplayName).X);
		}

		public virtual void resetState()
		{
		}
	}
}
