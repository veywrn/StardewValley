using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class Ring : Item
	{
		public const int ringLowerIndexRange = 516;

		public const int slimeCharmer = 520;

		public const int yobaRing = 524;

		public const int sturdyRing = 525;

		public const int burglarsRing = 526;

		public const int jukeboxRing = 528;

		public const int ringUpperIndexRange = 534;

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("indexInTileSheet")]
		public readonly NetInt indexInTileSheet = new NetInt();

		[XmlElement("uniqueID")]
		public readonly NetInt uniqueID = new NetInt();

		[XmlIgnore]
		public string description;

		[XmlIgnore]
		public string displayName;

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

		public Ring()
		{
			base.NetFields.AddFields(price, indexInTileSheet, uniqueID);
		}

		public Ring(int which)
			: this()
		{
			string[] data = Game1.objectInformation[which].Split('/');
			base.Category = -96;
			Name = data[0];
			price.Value = Convert.ToInt32(data[1]);
			indexInTileSheet.Value = which;
			base.ParentSheetIndex = indexInTileSheet;
			uniqueID.Value = Game1.year + Game1.dayOfMonth + Game1.timeOfDay + (int)indexInTileSheet + Game1.player.getTileX() + (int)Game1.stats.MonstersKilled + (int)Game1.stats.itemsCrafted;
			loadDisplayFields();
		}

		public void onEquip(Farmer who, GameLocation location)
		{
			switch ((int)indexInTileSheet)
			{
			case 516:
				location.sharedLights[(int)uniqueID + (int)who.UniqueMultiplayerID] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 5f, new Color(0, 50, 170), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				break;
			case 517:
				location.sharedLights[(int)uniqueID + (int)who.UniqueMultiplayerID] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				break;
			case 518:
				who.magneticRadius.Value += 64;
				break;
			case 519:
				who.magneticRadius.Value += 128;
				break;
			case 527:
				location.sharedLights[(int)uniqueID + (int)who.UniqueMultiplayerID] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				who.magneticRadius.Value += 128;
				who.attackIncreaseModifier += 0.1f;
				break;
			case 529:
				who.knockbackModifier += 0.1f;
				break;
			case 530:
				who.weaponPrecisionModifier += 0.1f;
				break;
			case 531:
				who.critChanceModifier += 0.1f;
				break;
			case 532:
				who.critPowerModifier += 0.1f;
				break;
			case 533:
				who.weaponSpeedModifier += 0.1f;
				break;
			case 534:
				who.attackIncreaseModifier += 0.1f;
				break;
			case 810:
				who.resilience += 5;
				break;
			}
		}

		public void onUnequip(Farmer who, GameLocation location)
		{
			switch ((int)indexInTileSheet)
			{
			case 516:
			case 517:
				location.removeLightSource((int)uniqueID + (int)who.UniqueMultiplayerID);
				break;
			case 518:
				who.magneticRadius.Value -= 64;
				break;
			case 519:
				who.magneticRadius.Value -= 128;
				break;
			case 527:
				who.magneticRadius.Value -= 128;
				location.removeLightSource((int)uniqueID + (int)who.UniqueMultiplayerID);
				who.attackIncreaseModifier -= 0.1f;
				break;
			case 529:
				who.knockbackModifier -= 0.1f;
				break;
			case 530:
				who.weaponPrecisionModifier -= 0.1f;
				break;
			case 531:
				who.critChanceModifier -= 0.1f;
				break;
			case 532:
				who.critPowerModifier -= 0.1f;
				break;
			case 533:
				who.weaponSpeedModifier -= 0.1f;
				break;
			case 534:
				who.attackIncreaseModifier -= 0.1f;
				break;
			case 810:
				who.resilience -= 5;
				break;
			}
		}

		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ring.cs.1");
		}

		public void onNewLocation(Farmer who, GameLocation environment)
		{
			switch ((int)indexInTileSheet)
			{
			case 516:
			case 517:
				onEquip(who, environment);
				break;
			case 527:
				environment.sharedLights[(int)uniqueID + (int)who.UniqueMultiplayerID] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, 0L);
				break;
			}
		}

		public void onLeaveLocation(Farmer who, GameLocation environment)
		{
			switch ((int)indexInTileSheet)
			{
			case 516:
			case 517:
				onUnequip(who, environment);
				break;
			case 527:
				environment.removeLightSource((int)uniqueID + (int)who.UniqueMultiplayerID);
				break;
			}
		}

		public override int salePrice()
		{
			return price;
		}

		public void onMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			switch ((int)indexInTileSheet)
			{
			case 521:
				if (Game1.random.NextDouble() < 0.1 + (double)((float)Game1.player.LuckLevel / 100f))
				{
					Game1.buffsDisplay.addOtherBuff(new Buff(20));
					Game1.playSound("warrior");
				}
				break;
			case 522:
				Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 2);
				break;
			case 523:
				Game1.buffsDisplay.addOtherBuff(new Buff(22));
				break;
			case 811:
				location.explode(m.getTileLocation(), 2, who, damageFarmers: false);
				break;
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		}

		public void update(GameTime time, GameLocation environment, Farmer who)
		{
			int num = indexInTileSheet;
			if ((uint)(num - 516) > 1u && num != 527)
			{
				_ = 528;
				return;
			}
			environment.repositionLightSource((int)uniqueID + (int)who.UniqueMultiplayerID, new Vector2(who.Position.X + 21f, who.Position.Y));
			if (!environment.isOutdoors && !(environment is MineShaft))
			{
				LightSource i = environment.getLightSource((int)uniqueID + (int)who.UniqueMultiplayerID);
				if (i != null)
				{
					i.radius.Value = 3f;
				}
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

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, string descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point dimensions = new Point(0, startingHeight);
			if ((int)parentSheetIndex == 810)
			{
				dimensions.X = (int)Math.Max(minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + (float)horizontalBuffer);
				dimensions.Y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, dimensions.X)).Y - 24;
			}
			return dimensions;
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, string overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
			y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;
			if ((int)parentSheetIndex == 810)
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 5), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
		}

		public override string getDescription()
		{
			if (description == null)
			{
				loadDisplayFields();
			}
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override Item getOne()
		{
			return new Ring(indexInTileSheet);
		}

		private bool loadDisplayFields()
		{
			if (Game1.objectInformation != null && indexInTileSheet != null)
			{
				string[] data = Game1.objectInformation[indexInTileSheet].Split('/');
				displayName = data[4];
				description = data[5];
				return true;
			}
			return false;
		}
	}
}
