using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Boots : Item
	{
		[XmlElement("defenseBonus")]
		public readonly NetInt defenseBonus = new NetInt();

		[XmlElement("immunityBonus")]
		public readonly NetInt immunityBonus = new NetInt();

		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheet = new NetInt();

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("indexInColorSheet")]
		public readonly NetInt indexInColorSheet = new NetInt();

		[XmlElement("appliedBootSheetIndex")]
		public readonly NetInt appliedBootSheetIndex = new NetInt(-1);

		[XmlIgnore]
		public string displayName;

		[XmlIgnore]
		public string description;

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (displayName == null)
				{
					loadDisplayFields();
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

		public Boots()
		{
			base.NetFields.AddFields(defenseBonus, immunityBonus, indexInTileSheet, price, indexInColorSheet);
			base.Category = -97;
		}

		public Boots(int which)
			: this()
		{
			indexInTileSheet.Value = which;
			reloadData();
			base.Category = -97;
		}

		public virtual void reloadData()
		{
			string[] data = Game1.content.Load<Dictionary<int, string>>("Data\\Boots")[indexInTileSheet.Value].Split('/');
			Name = data[0];
			price.Value = Convert.ToInt32(data[2]);
			defenseBonus.Value = Convert.ToInt32(data[3]);
			immunityBonus.Value = Convert.ToInt32(data[4]);
			indexInColorSheet.Value = Convert.ToInt32(data[5]);
		}

		public void applyStats(Boots applied_boots)
		{
			reloadData();
			if (defenseBonus.Value == (int)applied_boots.defenseBonus && immunityBonus.Value == (int)applied_boots.immunityBonus)
			{
				appliedBootSheetIndex.Value = -1;
			}
			else
			{
				appliedBootSheetIndex.Value = applied_boots.getStatsIndex();
			}
			defenseBonus.Value = applied_boots.defenseBonus.Value;
			immunityBonus.Value = applied_boots.immunityBonus.Value;
			price.Value = applied_boots.price.Value;
			loadDisplayFields();
		}

		public virtual int getStatsIndex()
		{
			if (appliedBootSheetIndex.Value >= 0)
			{
				return appliedBootSheetIndex.Value;
			}
			return indexInTileSheet.Value;
		}

		public override int salePrice()
		{
			return (int)defenseBonus * 100 + (int)immunityBonus * 100;
		}

		public void onEquip()
		{
			Game1.player.resilience += defenseBonus;
			Game1.player.immunity += immunityBonus;
			Game1.player.changeShoeColor(indexInColorSheet);
		}

		public void onUnequip()
		{
			Game1.player.resilience -= defenseBonus;
			Game1.player.immunity -= immunityBonus;
			Game1.player.changeShoeColor(12);
		}

		public int getNumberOfDescriptionCategories()
		{
			if ((int)immunityBonus > 0 && (int)defenseBonus > 0)
			{
				return 2;
			}
			return 1;
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, string overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
			y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;
			if ((int)defenseBonus > 0)
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", defenseBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if ((int)immunityBonus > 0)
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", immunityBonus), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, string descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			int maxStat = 9999;
			Point dimensions = new Point(0, startingHeight);
			dimensions.Y -= (int)font.MeasureString(descriptionText).Y;
			dimensions.Y += (int)((float)(getNumberOfDescriptionCategories() * 4 * 12) + font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y);
			dimensions.X = (int)Math.Max(minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", maxStat)).X + (float)horizontalBuffer));
			return dimensions;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet.Value, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12501");
		}

		public override string getDescription()
		{
			if (description == null)
			{
				loadDisplayFields();
			}
			return Game1.parseText(description + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:Boots.cs.12500", (int)immunityBonus + (int)defenseBonus), Game1.smallFont, getDescriptionWidth());
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override Item getOne()
		{
			Boots boots = new Boots(indexInTileSheet);
			boots.appliedBootSheetIndex.Value = appliedBootSheetIndex.Value;
			boots.indexInColorSheet.Value = indexInColorSheet.Value;
			boots.defenseBonus.Value = defenseBonus.Value;
			boots.immunityBonus.Value = immunityBonus.Value;
			boots.loadDisplayFields();
			return boots;
		}

		private bool loadDisplayFields()
		{
			if (indexInTileSheet != null)
			{
				string[] data = Game1.content.Load<Dictionary<int, string>>("Data\\Boots")[indexInTileSheet].Split('/');
				displayName = Name;
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					displayName = data[data.Length - 1];
				}
				if (appliedBootSheetIndex.Value >= 0)
				{
					displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:CustomizedBootItemName", DisplayName);
				}
				description = data[1];
				return true;
			}
			return false;
		}
	}
}
