using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Text;
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

		public const int protectiveRing = 861;

		public const int sapperRing = 862;

		public const int phoenixRing = 863;

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
		protected int? _lightSourceID;

		[XmlIgnore]
		public bool zeroStack;

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
				if (zeroStack)
				{
					return 0;
				}
				return 1;
			}
			set
			{
				if (value == 0)
				{
					zeroStack = true;
				}
				else
				{
					zeroStack = false;
				}
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

		public virtual void onDayUpdate(Farmer who, GameLocation location)
		{
			if (indexInTileSheet.Value == 859)
			{
				onEquip(who, location);
			}
		}

		public virtual void onEquip(Farmer who, GameLocation location)
		{
			if (_lightSourceID.HasValue)
			{
				location.removeLightSource(_lightSourceID.Value);
				_lightSourceID = null;
			}
			switch ((int)indexInTileSheet)
			{
			case 516:
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				while (location.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
				}
				location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 5f, new Color(0, 50, 170), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				break;
			case 517:
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				while (location.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
				}
				location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				break;
			case 518:
				who.magneticRadius.Value += 64;
				break;
			case 519:
				who.magneticRadius.Value += 128;
				break;
			case 888:
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				while (location.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
				}
				location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
				who.magneticRadius.Value += 128;
				break;
			case 527:
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				while (location.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
				}
				location.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 80, 0), (int)uniqueID + (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.UniqueMultiplayerID);
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
			case 859:
				who.addedLuckLevel.Value++;
				break;
			case 887:
				who.immunity += 4;
				break;
			}
		}

		public virtual void onUnequip(Farmer who, GameLocation location)
		{
			switch ((int)indexInTileSheet)
			{
			case 516:
			case 517:
				if (_lightSourceID.HasValue)
				{
					location.removeLightSource(_lightSourceID.Value);
					_lightSourceID = null;
				}
				break;
			case 518:
				who.magneticRadius.Value -= 64;
				break;
			case 519:
				who.magneticRadius.Value -= 128;
				break;
			case 888:
				if (_lightSourceID.HasValue)
				{
					location.removeLightSource(_lightSourceID.Value);
					_lightSourceID = null;
				}
				who.magneticRadius.Value -= 128;
				break;
			case 527:
				who.magneticRadius.Value -= 128;
				who.attackIncreaseModifier -= 0.1f;
				if (_lightSourceID.HasValue)
				{
					location.removeLightSource(_lightSourceID.Value);
					_lightSourceID = null;
				}
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
			case 859:
				who.addedLuckLevel.Value--;
				break;
			case 887:
				who.immunity -= 4;
				break;
			}
		}

		public override string getCategoryName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Ring.cs.1");
		}

		public virtual void onNewLocation(Farmer who, GameLocation environment)
		{
			if (_lightSourceID.HasValue)
			{
				environment.removeLightSource(_lightSourceID.Value);
				_lightSourceID = null;
			}
			switch ((int)indexInTileSheet)
			{
			case 516:
			case 517:
				onEquip(who, environment);
				break;
			case 527:
			case 888:
				_lightSourceID = (int)uniqueID + (int)who.UniqueMultiplayerID;
				while (environment.sharedLights.ContainsKey(_lightSourceID.Value))
				{
					_lightSourceID = _lightSourceID.Value + 1;
				}
				environment.sharedLights[_lightSourceID.Value] = new LightSource(1, new Vector2(who.Position.X + 21f, who.Position.Y + 64f), 10f, new Color(0, 30, 150), LightSource.LightContext.None, who.UniqueMultiplayerID);
				break;
			}
		}

		public virtual void onLeaveLocation(Farmer who, GameLocation environment)
		{
			int num = indexInTileSheet;
			if (((uint)(num - 516) <= 1u || num == 527 || num == 888) && _lightSourceID.HasValue)
			{
				environment.removeLightSource(_lightSourceID.Value);
				_lightSourceID = null;
			}
		}

		public override int salePrice()
		{
			return price;
		}

		public virtual void onMonsterSlay(Monster m, GameLocation location, Farmer who)
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
			case 862:
				Game1.player.Stamina = Math.Min(Game1.player.MaxStamina, Game1.player.Stamina + 4f);
				break;
			case 860:
				if (Game1.random.NextDouble() < 0.25)
				{
					m.objectsToDrop.Add(395);
				}
				else if (Game1.random.NextDouble() < 0.1)
				{
					m.objectsToDrop.Add(253);
				}
				break;
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, indexInTileSheet, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
		}

		public virtual void update(GameTime time, GameLocation environment, Farmer who)
		{
			if (_lightSourceID.HasValue)
			{
				Vector2 offset = Vector2.Zero;
				if (who.shouldShadowBeOffset)
				{
					offset += (Vector2)who.drawOffset;
				}
				environment.repositionLightSource(_lightSourceID.Value, new Vector2(who.Position.X + 21f, who.Position.Y) + offset);
				if (!environment.isOutdoors && !(environment is MineShaft) && !(environment is VolcanoDungeon))
				{
					LightSource i = environment.getLightSource(_lightSourceID.Value);
					if (i != null)
					{
						i.radius.Value = 3f;
					}
				}
			}
			int num = indexInTileSheet;
			_ = 528;
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int addToStack(Item stack)
		{
			return 1;
		}

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point dimensions = new Point(0, startingHeight);
			int extra_rows_needed = 0;
			if (GetsEffectOfRing(810))
			{
				extra_rows_needed++;
			}
			if (GetsEffectOfRing(887))
			{
				extra_rows_needed++;
			}
			if (GetsEffectOfRing(859))
			{
				extra_rows_needed++;
			}
			dimensions.X = (int)Math.Max(minWidth, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 9999)).X + (float)horizontalBuffer);
			dimensions.Y += extra_rows_needed * Math.Max((int)font.MeasureString("TT").Y, 48);
			return dimensions;
		}

		public virtual bool GetsEffectOfRing(int ring_index)
		{
			return (int)indexInTileSheet == ring_index;
		}

		public virtual int GetEffectsOfRingMultiplier(int ring_index)
		{
			if (GetsEffectOfRing(ring_index))
			{
				return 1;
			}
			return 0;
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
			y += (int)font.MeasureString(Game1.parseText(description, Game1.smallFont, getDescriptionWidth())).Y;
			if (GetsEffectOfRing(810))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", 5 * GetEffectsOfRingMultiplier(810)), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if (GetsEffectOfRing(887))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(150, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_ImmunityBonus", 4 * GetEffectsOfRingMultiplier(887)), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
			}
			if (GetsEffectOfRing(859))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(50, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, "+" + Game1.content.LoadString("Strings\\UI:ItemHover_Buff4", GetEffectsOfRingMultiplier(859)), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
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
			Ring ring = new Ring(indexInTileSheet);
			ring._GetOneFrom(this);
			return ring;
		}

		protected virtual bool loadDisplayFields()
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

		public virtual bool CanCombine(Ring ring)
		{
			if (ring is CombinedRing || this is CombinedRing)
			{
				return false;
			}
			if (base.ParentSheetIndex == ring.ParentSheetIndex)
			{
				return false;
			}
			return true;
		}

		public Ring Combine(Ring ring)
		{
			CombinedRing combinedRing = new CombinedRing(880);
			combinedRing.combinedRings.Add(getOne() as Ring);
			combinedRing.combinedRings.Add(ring.getOne() as Ring);
			combinedRing.UpdateDescription();
			combinedRing.uniqueID.Value = uniqueID.Value;
			return combinedRing;
		}
	}
}
