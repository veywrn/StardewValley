using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlInclude(typeof(MagnifyingGlass))]
	[XmlInclude(typeof(Shears))]
	[XmlInclude(typeof(MilkPail))]
	[XmlInclude(typeof(Axe))]
	[XmlInclude(typeof(Wand))]
	[XmlInclude(typeof(Hoe))]
	[XmlInclude(typeof(FishingRod))]
	[XmlInclude(typeof(MeleeWeapon))]
	[XmlInclude(typeof(Pan))]
	[XmlInclude(typeof(Pickaxe))]
	[XmlInclude(typeof(WateringCan))]
	[XmlInclude(typeof(Slingshot))]
	[XmlInclude(typeof(GenericTool))]
	public abstract class Tool : Item
	{
		public const int standardStaminaReduction = 2;

		public const int nonUpgradeable = -1;

		public const int stone = 0;

		public const int copper = 1;

		public const int steel = 2;

		public const int gold = 3;

		public const int iridium = 4;

		public const int parsnipSpriteIndex = 0;

		public const int hoeSpriteIndex = 21;

		public const int hammerSpriteIndex = 105;

		public const int axeSpriteIndex = 189;

		public const int wateringCanSpriteIndex = 273;

		public const int fishingRodSpriteIndex = 8;

		public const int batteredSwordSpriteIndex = 67;

		public const int axeMenuIndex = 215;

		public const int hoeMenuIndex = 47;

		public const int pickAxeMenuIndex = 131;

		public const int wateringCanMenuIndex = 296;

		public const int startOfNegativeWeaponIndex = -10000;

		public const string weaponsTextureName = "TileSheets\\weapons";

		public static Texture2D weaponsTexture;

		[XmlElement("initialParentTileIndex")]
		public readonly NetInt initialParentTileIndex = new NetInt();

		[XmlElement("currentParentTileIndex")]
		public readonly NetInt currentParentTileIndex = new NetInt();

		[XmlElement("indexOfMenuItemView")]
		public readonly NetInt indexOfMenuItemView = new NetInt();

		[XmlElement("stackable")]
		public readonly NetBool stackable = new NetBool();

		[XmlElement("instantUse")]
		public readonly NetBool instantUse = new NetBool();

		[XmlElement("isEfficient")]
		public readonly NetBool isEfficient = new NetBool();

		[XmlElement("animationSpeedModifier")]
		public readonly NetFloat animationSpeedModifier = new NetFloat(1f);

		[XmlIgnore]
		private string _description;

		public static Color copperColor = new Color(198, 108, 43);

		public static Color steelColor = new Color(197, 226, 222);

		public static Color goldColor = new Color(248, 255, 73);

		public static Color iridiumColor = new Color(144, 135, 181);

		[XmlElement("upgradeLevel")]
		public readonly NetInt upgradeLevel = new NetInt();

		[XmlElement("numAttachmentSlots")]
		public readonly NetInt numAttachmentSlots = new NetInt();

		protected Farmer lastUser;

		public readonly NetObjectArray<Object> attachments = new NetObjectArray<Object>();

		[XmlIgnore]
		protected string displayName;

		[XmlElement("enchantments")]
		public readonly NetList<BaseEnchantment, NetRef<BaseEnchantment>> enchantments = new NetList<BaseEnchantment, NetRef<BaseEnchantment>>();

		[XmlIgnore]
		public string description
		{
			get
			{
				if (_description == null)
				{
					_description = loadDescription();
				}
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		public string BaseName
		{
			get
			{
				return netName;
			}
			set
			{
				netName.Set(value);
			}
		}

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				displayName = loadDisplayName();
				switch ((int)upgradeLevel)
				{
				case 1:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14299", displayName);
				case 2:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14300", displayName);
				case 3:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14301", displayName);
				case 4:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14302", displayName);
				default:
					return displayName;
				}
			}
			set
			{
				displayName = value;
			}
		}

		public override string Name
		{
			get
			{
				switch ((int)upgradeLevel)
				{
				case 1:
					return "Copper " + BaseName;
				case 2:
					return "Steel " + BaseName;
				case 3:
					return "Gold " + BaseName;
				case 4:
					return "Iridium " + BaseName;
				default:
					return BaseName;
				}
			}
			set
			{
				BaseName = value;
			}
		}

		public override int Stack
		{
			get
			{
				if ((bool)stackable)
				{
					return ((Stackable)this).NumberInStack;
				}
				return 1;
			}
			set
			{
				if ((bool)stackable)
				{
					((Stackable)this).Stack = Math.Min(Math.Max(0, value), maximumStackSize());
				}
			}
		}

		public string Description => description;

		[XmlIgnore]
		public int CurrentParentTileIndex
		{
			get
			{
				return currentParentTileIndex;
			}
			set
			{
				currentParentTileIndex.Set(value);
			}
		}

		public int InitialParentTileIndex
		{
			get
			{
				return initialParentTileIndex;
			}
			set
			{
				initialParentTileIndex.Set(value);
			}
		}

		public int IndexOfMenuItemView
		{
			get
			{
				return indexOfMenuItemView;
			}
			set
			{
				indexOfMenuItemView.Set(value);
			}
		}

		[XmlIgnore]
		public int UpgradeLevel
		{
			get
			{
				return upgradeLevel;
			}
			set
			{
				upgradeLevel.Value = value;
				setNewTileIndexForUpgradeLevel();
			}
		}

		public bool InstantUse
		{
			get
			{
				return instantUse;
			}
			set
			{
				instantUse.Value = value;
			}
		}

		public bool IsEfficient
		{
			get
			{
				return isEfficient;
			}
			set
			{
				isEfficient.Value = value;
			}
		}

		public float AnimationSpeedModifier
		{
			get
			{
				return animationSpeedModifier;
			}
			set
			{
				animationSpeedModifier.Value = value;
			}
		}

		public bool Stackable
		{
			get
			{
				return stackable;
			}
			set
			{
				stackable.Value = value;
			}
		}

		public Tool()
		{
			initNetFields();
			base.Category = -99;
		}

		public Tool(string name, int upgradeLevel, int initialParentTileIndex, int indexOfMenuItemView, bool stackable, int numAttachmentSlots = 0)
			: this()
		{
			BaseName = name;
			this.initialParentTileIndex.Value = initialParentTileIndex;
			IndexOfMenuItemView = indexOfMenuItemView;
			Stackable = stackable;
			currentParentTileIndex.Value = initialParentTileIndex;
			this.numAttachmentSlots.Value = numAttachmentSlots;
			if (numAttachmentSlots > 0)
			{
				attachments.SetCount(numAttachmentSlots);
			}
			base.Category = -99;
		}

		protected virtual void initNetFields()
		{
			base.NetFields.AddFields(initialParentTileIndex, currentParentTileIndex, indexOfMenuItemView, stackable, instantUse, upgradeLevel, numAttachmentSlots, attachments, enchantments, isEfficient, animationSpeedModifier);
		}

		protected abstract string loadDisplayName();

		protected abstract string loadDescription();

		public override string getCategoryName()
		{
			if (this is MeleeWeapon && !(this as MeleeWeapon).isScythe(IndexOfMenuItemView))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14303", (this as MeleeWeapon).getItemLevel(), ((int)(this as MeleeWeapon).type == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14304") : (((int)(this as MeleeWeapon).type == 2) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14305") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14306")));
			}
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14307");
		}

		public override Color getCategoryColor()
		{
			return Color.DarkSlateGray;
		}

		public virtual void draw(SpriteBatch b)
		{
			if (lastUser != null && lastUser.toolPower > 0 && lastUser.canReleaseTool)
			{
				foreach (Vector2 v in tilesAffected(lastUser.GetToolLocation() / 64f, lastUser.toolPower, lastUser))
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((int)v.X * 64, (int)v.Y * 64)), new Rectangle(194, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
				}
			}
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, StringBuilder overrideText)
		{
			base.drawTooltip(spriteBatch, ref x, ref y, font, alpha, overrideText);
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.ShouldBeDisplayed())
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors2, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(127, 35, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, BaseEnchantment.hideEnchantmentName ? "???" : enchantment.GetDisplayName(), font, new Vector2(x + 16 + 52, y + 16 + 12), new Color(120, 0, 210) * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
		}

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, StringBuilder descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			Point dimensions = base.getExtraSpaceNeededForTooltipSpecialIcons(font, minWidth, horizontalBuffer, startingHeight, descriptionText, boldTitleText, moneyAmountToDisplayAtBottom);
			dimensions.Y = startingHeight;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.ShouldBeDisplayed())
				{
					dimensions.Y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
			return dimensions;
		}

		public virtual void tickUpdate(GameTime time, Farmer who)
		{
		}

		public bool isHeavyHitter()
		{
			if (!(this is MeleeWeapon) && !(this is Hoe) && !(this is Axe))
			{
				return this is Pickaxe;
			}
			return true;
		}

		public void Update(int direction, int farmerMotionFrame, Farmer who)
		{
			int offset = 0;
			if (this is WateringCan)
			{
				switch (direction)
				{
				case 0:
					offset = 4;
					break;
				case 1:
					offset = 2;
					break;
				case 2:
					offset = 0;
					break;
				case 3:
					offset = 2;
					break;
				}
			}
			else if (this is FishingRod)
			{
				switch (direction)
				{
				case 0:
					offset = 3;
					break;
				case 1:
					offset = 0;
					break;
				case 3:
					offset = 0;
					break;
				}
			}
			else
			{
				switch (direction)
				{
				case 0:
					offset = 3;
					break;
				case 1:
					offset = 2;
					break;
				case 3:
					offset = 2;
					break;
				}
			}
			if (!Name.Equals("Watering Can"))
			{
				if (farmerMotionFrame < 1)
				{
					CurrentParentTileIndex = InitialParentTileIndex;
				}
				else if (who.FacingDirection == 0 || (who.FacingDirection == 2 && farmerMotionFrame >= 2))
				{
					CurrentParentTileIndex = InitialParentTileIndex + 1;
				}
			}
			else if (farmerMotionFrame < 5 || direction == 0)
			{
				CurrentParentTileIndex = InitialParentTileIndex;
			}
			else
			{
				CurrentParentTileIndex = InitialParentTileIndex + 1;
			}
			CurrentParentTileIndex += offset;
		}

		public override int attachmentSlots()
		{
			return numAttachmentSlots;
		}

		public Farmer getLastFarmerToUse()
		{
			return lastUser;
		}

		public virtual void leftClick(Farmer who)
		{
		}

		public virtual void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			lastUser = who;
			Game1.recentMultiplayerRandom = new Random((short)Game1.random.Next(-32768, 32768));
			ToolFactory.getIndexFromTool(this);
			if (isHeavyHitter() && !(this is MeleeWeapon))
			{
				Rumble.rumble(0.1f + (float)(Game1.random.NextDouble() / 4.0), 100 + Game1.random.Next(50));
				location.damageMonster(new Rectangle(x - 32, y - 32, 64, 64), (int)upgradeLevel + 1, ((int)upgradeLevel + 1) * 3, isBomb: false, who);
			}
			if (this is MeleeWeapon && (!who.UsingTool || Game1.mouseClickPolling >= 50 || (int)(this as MeleeWeapon).type == 1 || (this as MeleeWeapon).InitialParentTileIndex == 47 || MeleeWeapon.timedHitTimer > 0 || who.FarmerSprite.currentAnimationIndex != 5 || !(who.FarmerSprite.timer < who.FarmerSprite.interval / 4f)))
			{
				if ((int)(this as MeleeWeapon).type == 2 && (this as MeleeWeapon).isOnSpecial)
				{
					(this as MeleeWeapon).triggerClubFunction(who);
				}
				else if (who.FarmerSprite.currentAnimationIndex > 0)
				{
					MeleeWeapon.timedHitTimer = 500;
				}
			}
		}

		public virtual void endUsing(GameLocation location, Farmer who)
		{
			who.stopJittering();
			who.canReleaseTool = false;
			int addedAnimationMultiplayer = (!(who.Stamina <= 0f)) ? 1 : 2;
			if (Game1.isAnyGamePadButtonBeingPressed() || !who.IsLocalPlayer)
			{
				who.lastClick = who.GetToolLocation();
			}
			if (Name.Equals("Seeds"))
			{
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(200, 150f, 4);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(204, 150f, 4);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(208, 150f, 4);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(212, 150f, 4);
					break;
				}
			}
			else if (this is WateringCan)
			{
				if ((this as WateringCan).WaterLeft > 0 && who.ShouldHandleAnimationSound())
				{
					who.currentLocation.localSound("wateringCan");
				}
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(164, 125f * (float)addedAnimationMultiplayer, 3);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(172, 125f * (float)addedAnimationMultiplayer, 3);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(180, 125f * (float)addedAnimationMultiplayer, 3);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(188, 125f * (float)addedAnimationMultiplayer, 3);
					break;
				}
			}
			else if (this is FishingRod && who.IsLocalPlayer && Game1.activeClickableMenu == null)
			{
				if (!(this as FishingRod).hit)
				{
					DoFunction(who.currentLocation, (int)who.lastClick.X, (int)who.lastClick.Y, 1, who);
				}
			}
			else if (!(this is MeleeWeapon) && !(this is Pan) && !(this is Shears) && !(this is MilkPail) && !(this is Slingshot))
			{
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(176, 60f * (float)addedAnimationMultiplayer, 8);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(168, 60f * (float)addedAnimationMultiplayer, 8);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(160, 60f * (float)addedAnimationMultiplayer, 8);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(184, 60f * (float)addedAnimationMultiplayer, 8);
					break;
				}
			}
		}

		public virtual bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			lastUser = who;
			if (!instantUse)
			{
				who.Halt();
				Update(who.FacingDirection, 0, who);
				if ((!(this is FishingRod) && (int)upgradeLevel <= 0 && !(this is MeleeWeapon)) || this is Pickaxe)
				{
					who.EndUsingTool();
					return true;
				}
			}
			if (Name.Equals("Wand"))
			{
				if (((Wand)this).charged)
				{
					Game1.toolAnimationDone(who);
					who.canReleaseTool = false;
					if (!who.IsLocalPlayer || !Game1.fadeToBlack)
					{
						who.CanMove = true;
						who.UsingTool = false;
					}
				}
				else
				{
					if (who.IsLocalPlayer)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3180")));
					}
					who.UsingTool = false;
					who.canReleaseTool = false;
				}
			}
			else if ((bool)instantUse)
			{
				Game1.toolAnimationDone(who);
				who.canReleaseTool = false;
				who.UsingTool = false;
			}
			else if (Name.Equals("Seeds"))
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.Sprite.currentFrame = 208;
					Update(0, 0, who);
					break;
				case 1:
					who.Sprite.currentFrame = 204;
					Update(1, 0, who);
					break;
				case 2:
					who.Sprite.currentFrame = 200;
					Update(2, 0, who);
					break;
				case 3:
					who.Sprite.currentFrame = 212;
					Update(3, 0, who);
					break;
				}
			}
			else if (this is WateringCan && location.CanRefillWateringCanOnTile((int)who.GetToolLocation().X / 64, (int)who.GetToolLocation().Y / 64))
			{
				switch (who.FacingDirection)
				{
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(166, 250f, 2);
					Update(2, 1, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(174, 250f, 2);
					Update(1, 0, who);
					break;
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(182, 250f, 2);
					Update(0, 1, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(190, 250f, 2);
					Update(3, 0, who);
					break;
				}
				who.canReleaseTool = false;
			}
			else if (this is WateringCan && ((WateringCan)this).WaterLeft <= 0)
			{
				Game1.toolAnimationDone(who);
				who.CanMove = true;
				who.canReleaseTool = false;
			}
			else if (this is WateringCan)
			{
				who.jitterStrength = 0.25f;
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(180);
					Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(172);
					Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(164);
					Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(188);
					Update(3, 0, who);
					break;
				}
			}
			else if (this is FishingRod)
			{
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(295, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(0, 0, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(296, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(1, 0, who);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(297, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(2, 0, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(298, 35f, 8, FishingRod.endOfAnimationBehavior);
					Update(3, 0, who);
					break;
				}
				who.canReleaseTool = false;
			}
			else if (this is MeleeWeapon)
			{
				((MeleeWeapon)this).setFarmerAnimating(who);
			}
			else
			{
				switch (who.FacingDirection)
				{
				case 0:
					who.FarmerSprite.setCurrentFrame(176);
					Update(0, 0, who);
					break;
				case 1:
					who.FarmerSprite.setCurrentFrame(168);
					Update(1, 0, who);
					break;
				case 2:
					who.FarmerSprite.setCurrentFrame(160);
					Update(2, 0, who);
					break;
				case 3:
					who.FarmerSprite.setCurrentFrame(184);
					Update(3, 0, who);
					break;
				}
			}
			return false;
		}

		public virtual bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			return false;
		}

		public override bool canBeDropped()
		{
			return false;
		}

		public virtual bool canThisBeAttached(Object o)
		{
			if (attachments != null)
			{
				for (int i = 0; i < attachments.Length; i++)
				{
					if (attachments[i] == null)
					{
						return true;
					}
				}
			}
			return false;
		}

		public virtual Object attach(Object o)
		{
			for (int i = 0; i < attachments.Length; i++)
			{
				if (attachments[i] == null)
				{
					attachments[i] = o;
					Game1.playSound("button1");
					return null;
				}
			}
			return o;
		}

		public void colorTool(int level)
		{
			int initialLocation = 0;
			int startPixel = 0;
			switch (BaseName.Split(' ').Last())
			{
			case "Hoe":
				initialLocation = 69129;
				startPixel = 65536;
				break;
			case "Pickaxe":
				initialLocation = 100749;
				startPixel = 98304;
				break;
			case "Axe":
				initialLocation = 134681;
				startPixel = 131072;
				break;
			case "Can":
				initialLocation = 168713;
				startPixel = 163840;
				break;
			}
			int red = 0;
			int green = 0;
			int blue = 0;
			switch (level)
			{
			case 1:
				red = 198;
				green = 108;
				blue = 43;
				break;
			case 2:
				red = 197;
				green = 226;
				blue = 222;
				break;
			case 3:
				red = 248;
				green = 255;
				blue = 73;
				break;
			case 4:
				red = 144;
				green = 135;
				blue = 181;
				break;
			}
			if (startPixel > 0 && level > 0)
			{
				if (BaseName.Contains("Can"))
				{
					ColorChanger.swapColor(Game1.toolSpriteSheet, initialLocation + 36, red * 5 / 4, green * 5 / 4, blue * 5 / 4, startPixel, startPixel + 32768);
				}
				ColorChanger.swapColor(Game1.toolSpriteSheet, initialLocation + 8, red, green, blue, startPixel, startPixel + 32768);
				ColorChanger.swapColor(Game1.toolSpriteSheet, initialLocation + 4, red * 3 / 4, green * 3 / 4, blue * 3 / 4, startPixel, startPixel + 32768);
				ColorChanger.swapColor(Game1.toolSpriteSheet, initialLocation, red * 3 / 8, green * 3 / 8, blue * 3 / 8, startPixel, startPixel + 32768);
			}
		}

		public virtual void actionWhenClaimed()
		{
			if (this is GenericTool)
			{
				int num = indexOfMenuItemView;
				if ((uint)(num - 13) <= 3u)
				{
					Game1.player.trashCanLevel++;
				}
			}
		}

		public override bool CanBuyItem(Farmer who)
		{
			if (Game1.player.toolBeingUpgraded.Value == null && (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan || (this is GenericTool && (int)indexOfMenuItemView >= 13 && (int)indexOfMenuItemView <= 16)))
			{
				return true;
			}
			return base.CanBuyItem(who);
		}

		public override bool actionWhenPurchased()
		{
			if (Game1.player.toolBeingUpgraded.Value == null)
			{
				if (this is Axe || this is Pickaxe || this is Hoe || this is WateringCan)
				{
					Tool t = Game1.player.getToolFromName(BaseName);
					t.UpgradeLevel++;
					Game1.player.removeItemFromInventory(t);
					Game1.player.toolBeingUpgraded.Value = t;
					Game1.player.daysLeftForToolUpgrade.Value = 2;
					Game1.playSound("parry");
					Game1.exitActiveMenu();
					Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
					return true;
				}
				if (this is GenericTool)
				{
					int num = indexOfMenuItemView;
					if ((uint)(num - 13) <= 3u)
					{
						Game1.player.toolBeingUpgraded.Value = this;
						Game1.player.daysLeftForToolUpgrade.Value = 2;
						Game1.playSound("parry");
						Game1.exitActiveMenu();
						Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Strings\\StringsFromCSFiles:Tool.cs.14317"));
						return true;
					}
				}
			}
			return base.actionWhenPurchased();
		}

		protected List<Vector2> tilesAffected(Vector2 tileLocation, int power, Farmer who)
		{
			power++;
			List<Vector2> tileLocations = new List<Vector2>();
			tileLocations.Add(tileLocation);
			Vector2 extremePowerPosition = Vector2.Zero;
			if (who.FacingDirection == 0)
			{
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y - 2f);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, -2f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, -3f));
						tileLocations.Add(tileLocation + new Vector2(0f, -4f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(1f, -2f));
						tileLocations.Add(tileLocation + new Vector2(1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -2f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int l = tileLocations.Count - 1; l >= 0; l--)
						{
							tileLocations.Add(tileLocations[l] + new Vector2(0f, -3f));
						}
					}
				}
			}
			else if (who.FacingDirection == 1)
			{
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X + 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(2f, 0f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(3f, 0f));
						tileLocations.Add(tileLocation + new Vector2(4f, 0f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(2f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(2f, 1f));
					}
					if (power >= 5)
					{
						for (int k = tileLocations.Count - 1; k >= 0; k--)
						{
							tileLocations.Add(tileLocations[k] + new Vector2(3f, 0f));
						}
					}
				}
			}
			else if (who.FacingDirection == 2)
			{
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X, tileLocation.Y + 2f);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 2f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(0f, 3f));
						tileLocations.Add(tileLocation + new Vector2(0f, 4f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(1f, 2f));
						tileLocations.Add(tileLocation + new Vector2(1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 2f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
					}
					if (power >= 5)
					{
						for (int j = tileLocations.Count - 1; j >= 0; j--)
						{
							tileLocations.Add(tileLocations[j] + new Vector2(0f, 3f));
						}
					}
				}
			}
			else if (who.FacingDirection == 3)
			{
				if (power >= 6)
				{
					extremePowerPosition = new Vector2(tileLocation.X - 2f, tileLocation.Y);
				}
				else
				{
					if (power >= 2)
					{
						tileLocations.Add(tileLocation + new Vector2(-1f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-2f, 0f));
					}
					if (power >= 3)
					{
						tileLocations.Add(tileLocation + new Vector2(-3f, 0f));
						tileLocations.Add(tileLocation + new Vector2(-4f, 0f));
					}
					if (power >= 4)
					{
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.RemoveAt(tileLocations.Count - 1);
						tileLocations.Add(tileLocation + new Vector2(0f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, -1f));
						tileLocations.Add(tileLocation + new Vector2(-2f, -1f));
						tileLocations.Add(tileLocation + new Vector2(0f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-1f, 1f));
						tileLocations.Add(tileLocation + new Vector2(-2f, 1f));
					}
					if (power >= 5)
					{
						for (int i = tileLocations.Count - 1; i >= 0; i--)
						{
							tileLocations.Add(tileLocations[i] + new Vector2(-3f, 0f));
						}
					}
				}
			}
			if (power >= 6)
			{
				tileLocations.Clear();
				for (int x = (int)extremePowerPosition.X - 2; (float)x <= extremePowerPosition.X + 2f; x++)
				{
					for (int y = (int)extremePowerPosition.Y - 2; (float)y <= extremePowerPosition.Y + 2f; y++)
					{
						tileLocations.Add(new Vector2(x, y));
					}
				}
			}
			return tileLocations;
		}

		public virtual bool doesShowTileLocationMarker()
		{
			return true;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			spriteBatch.Draw(Game1.toolSpriteSheet, location + new Vector2(32f, 32f), Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, IndexOfMenuItemView), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			if ((bool)stackable)
			{
				Game1.drawWithBorder(string.Concat(((Stackable)this).NumberInStack), Color.Black, Color.White, location + new Vector2(64f - Game1.dialogueFont.MeasureString(string.Concat(((Stackable)this).NumberInStack)).X, 64f - Game1.dialogueFont.MeasureString(string.Concat(((Stackable)this).NumberInStack)).Y * 3f / 4f), 0f, 0.5f, 1f);
			}
		}

		public override bool isPlaceable()
		{
			return false;
		}

		public override int maximumStackSize()
		{
			if ((bool)stackable)
			{
				return 99;
			}
			return -1;
		}

		public virtual void setNewTileIndexForUpgradeLevel()
		{
			if (this is MeleeWeapon || this is MagnifyingGlass || this is MilkPail || this is Shears || this is Pan || this is Slingshot || this is Wand)
			{
				return;
			}
			int toolTypeOffset2 = 21;
			if (this is FishingRod)
			{
				InitialParentTileIndex = 8 + (int)upgradeLevel;
				CurrentParentTileIndex = InitialParentTileIndex;
				IndexOfMenuItemView = InitialParentTileIndex;
				return;
			}
			if (this is Axe)
			{
				toolTypeOffset2 = 189;
			}
			else if (this is Hoe)
			{
				toolTypeOffset2 = 21;
			}
			else if (this is Pickaxe)
			{
				toolTypeOffset2 = 105;
			}
			else if (this is WateringCan)
			{
				toolTypeOffset2 = 273;
			}
			toolTypeOffset2 += (int)upgradeLevel * 7;
			if ((int)upgradeLevel > 2)
			{
				toolTypeOffset2 += 21;
			}
			InitialParentTileIndex = toolTypeOffset2;
			CurrentParentTileIndex = InitialParentTileIndex;
			IndexOfMenuItemView = InitialParentTileIndex + ((this is WateringCan) ? 2 : 5) + 21;
		}

		public override int addToStack(Item stack)
		{
			if ((bool)stackable)
			{
				((Stackable)this).NumberInStack += stack.Stack;
				if (((Stackable)this).NumberInStack > 99)
				{
					int result = ((Stackable)this).NumberInStack - 99;
					((Stackable)this).NumberInStack = 99;
					return result;
				}
				return 0;
			}
			return stack.Stack;
		}

		public override string getDescription()
		{
			return Game1.parseText(description, Game1.smallFont, getDescriptionWidth());
		}

		public virtual void ClearEnchantments()
		{
			for (int i = enchantments.Count - 1; i >= 0; i--)
			{
				enchantments[i].UnapplyTo(this);
			}
			enchantments.Clear();
		}

		public virtual int GetMaxForges()
		{
			return 0;
		}

		public int GetSecondaryEnchantmentCount()
		{
			int total = 0;
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment != null && enchantment.IsSecondaryEnchantment())
				{
					total++;
				}
			}
			return total;
		}

		public virtual bool CanAddEnchantment(BaseEnchantment enchantment)
		{
			if (!enchantment.IsForge() && !enchantment.IsSecondaryEnchantment())
			{
				return true;
			}
			if (GetTotalForgeLevels() >= GetMaxForges() && !enchantment.IsSecondaryEnchantment())
			{
				return false;
			}
			if (enchantment != null)
			{
				foreach (BaseEnchantment existing_enchantment in enchantments)
				{
					if (enchantment.GetType() == existing_enchantment.GetType())
					{
						if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
						{
							return true;
						}
						return false;
					}
				}
				return true;
			}
			return false;
		}

		public virtual void CopyEnchantments(Tool source, Tool destination)
		{
			foreach (BaseEnchantment enchantment in enchantments)
			{
				destination.enchantments.Add(enchantment.GetOne());
				enchantment.GetOne().ApplyTo(destination);
			}
		}

		public int GetTotalForgeLevels(bool for_unforge = false)
		{
			int total = 0;
			foreach (BaseEnchantment existing_enchantment in enchantments)
			{
				if (existing_enchantment is DiamondEnchantment)
				{
					if (for_unforge)
					{
						return total;
					}
				}
				else if (existing_enchantment.IsForge())
				{
					total += existing_enchantment.GetLevel();
				}
			}
			return total;
		}

		public virtual bool AddEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment != null)
			{
				if (this is MeleeWeapon && (enchantment.IsForge() || enchantment.IsSecondaryEnchantment()))
				{
					foreach (BaseEnchantment existing_enchantment in enchantments)
					{
						if (enchantment.GetType() == existing_enchantment.GetType())
						{
							if (existing_enchantment.GetMaximumLevel() < 0 || existing_enchantment.GetLevel() < existing_enchantment.GetMaximumLevel())
							{
								existing_enchantment.SetLevel(this, existing_enchantment.GetLevel() + 1);
								return true;
							}
							return false;
						}
					}
					enchantments.Add(enchantment);
					enchantment.ApplyTo(this, lastUser);
					return true;
				}
				for (int i = enchantments.Count - 1; i >= 0; i--)
				{
					if (!enchantments[i].IsForge() && !enchantments[i].IsSecondaryEnchantment())
					{
						enchantments.ElementAt(i).UnapplyTo(this);
						enchantments.RemoveAt(i);
					}
				}
				enchantments.Add(enchantment);
				enchantment.ApplyTo(this, lastUser);
				return true;
			}
			return false;
		}

		public bool hasEnchantmentOfType<T>()
		{
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment is T)
				{
					return true;
				}
			}
			return false;
		}

		public virtual void RemoveEnchantment(BaseEnchantment enchantment)
		{
			if (enchantment != null)
			{
				enchantments.Remove(enchantment);
				enchantment.UnapplyTo(this, lastUser);
			}
		}

		public override void actionWhenBeingHeld(Farmer who)
		{
			base.actionWhenBeingHeld(who);
			if (who.IsLocalPlayer)
			{
				foreach (BaseEnchantment enchantment in enchantments)
				{
					enchantment.OnEquip(who);
				}
			}
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			base.actionWhenStopBeingHeld(who);
			if (who.UsingTool)
			{
				who.UsingTool = false;
				if (who.FarmerSprite.PauseForSingleAnimation)
				{
					who.FarmerSprite.PauseForSingleAnimation = false;
				}
			}
			if (who.IsLocalPlayer)
			{
				foreach (BaseEnchantment enchantment in enchantments)
				{
					enchantment.OnUnequip(who);
				}
			}
		}

		public virtual bool CanUseOnStandingTile()
		{
			return false;
		}

		public virtual bool CanForge(Item item)
		{
			BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantment != null && CanAddEnchantment(enchantment))
			{
				return true;
			}
			return false;
		}

		public T GetEnchantmentOfType<T>() where T : BaseEnchantment
		{
			foreach (BaseEnchantment existing_enchantment in enchantments)
			{
				if (existing_enchantment.GetType() == typeof(T))
				{
					return existing_enchantment as T;
				}
			}
			return null;
		}

		public int GetEnchantmentLevel<T>() where T : BaseEnchantment
		{
			int total = 0;
			foreach (BaseEnchantment existing_enchantment in enchantments)
			{
				if (existing_enchantment.GetType() == typeof(T))
				{
					total += existing_enchantment.GetLevel();
				}
			}
			return total;
		}

		public virtual bool Forge(Item item, bool count_towards_stats = false)
		{
			BaseEnchantment enchantment = BaseEnchantment.GetEnchantmentFromItem(this, item);
			if (enchantment != null && AddEnchantment(enchantment))
			{
				if (enchantment is DiamondEnchantment)
				{
					int forges_left = GetMaxForges() - GetTotalForgeLevels();
					List<int> valid_forges = new List<int>();
					if (!hasEnchantmentOfType<EmeraldEnchantment>())
					{
						valid_forges.Add(0);
					}
					if (!hasEnchantmentOfType<AquamarineEnchantment>())
					{
						valid_forges.Add(1);
					}
					if (!hasEnchantmentOfType<RubyEnchantment>())
					{
						valid_forges.Add(2);
					}
					if (!hasEnchantmentOfType<AmethystEnchantment>())
					{
						valid_forges.Add(3);
					}
					if (!hasEnchantmentOfType<TopazEnchantment>())
					{
						valid_forges.Add(4);
					}
					if (!hasEnchantmentOfType<JadeEnchantment>())
					{
						valid_forges.Add(5);
					}
					for (int i = 0; i < forges_left; i++)
					{
						if (valid_forges.Count == 0)
						{
							break;
						}
						int index = Game1.random.Next(valid_forges.Count);
						int random_enchant = valid_forges[index];
						valid_forges.RemoveAt(index);
						switch (random_enchant)
						{
						case 0:
							AddEnchantment(new EmeraldEnchantment());
							break;
						case 1:
							AddEnchantment(new AquamarineEnchantment());
							break;
						case 2:
							AddEnchantment(new RubyEnchantment());
							break;
						case 3:
							AddEnchantment(new AmethystEnchantment());
							break;
						case 4:
							AddEnchantment(new TopazEnchantment());
							break;
						case 5:
							AddEnchantment(new JadeEnchantment());
							break;
						}
					}
				}
				else if (enchantment is GalaxySoulEnchantment && this is MeleeWeapon && (this as MeleeWeapon).isGalaxyWeapon() && (this as MeleeWeapon).GetEnchantmentLevel<GalaxySoulEnchantment>() >= 3)
				{
					int current_index = (this as MeleeWeapon).InitialParentTileIndex;
					int new_index = -1;
					switch (current_index)
					{
					case 4:
						new_index = 62;
						break;
					case 29:
						new_index = 63;
						break;
					case 23:
						new_index = 64;
						break;
					}
					if (new_index != -1)
					{
						(this as MeleeWeapon).transform(new_index);
						if (count_towards_stats)
						{
							DelayedAction.playSoundAfterDelay("discoverMineral", 400);
							Game1.multiplayer.globalChatInfoMessage("InfinityWeapon", Game1.player.name, DisplayName);
						}
					}
					GalaxySoulEnchantment enchant = GetEnchantmentOfType<GalaxySoulEnchantment>();
					if (enchant != null)
					{
						RemoveEnchantment(enchant);
					}
				}
				if (count_towards_stats && !enchantment.IsForge())
				{
					Game1.stats.incrementStat("timesEnchanted", 1);
				}
				return true;
			}
			return false;
		}
	}
}
