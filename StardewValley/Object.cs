using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley
{
	[XmlInclude(typeof(Fence))]
	[XmlInclude(typeof(Torch))]
	[XmlInclude(typeof(SpecialItem))]
	[XmlInclude(typeof(Wallpaper))]
	[XmlInclude(typeof(Boots))]
	[XmlInclude(typeof(Hat))]
	[XmlInclude(typeof(ItemPedestal))]
	[XmlInclude(typeof(Clothing))]
	[XmlInclude(typeof(CombinedRing))]
	[XmlInclude(typeof(Ring))]
	[XmlInclude(typeof(TV))]
	[XmlInclude(typeof(CrabPot))]
	[XmlInclude(typeof(Chest))]
	[XmlInclude(typeof(Workbench))]
	[XmlInclude(typeof(MiniJukebox))]
	[XmlInclude(typeof(Phone))]
	[XmlInclude(typeof(StorageFurniture))]
	[XmlInclude(typeof(FishTankFurniture))]
	[XmlInclude(typeof(BedFurniture))]
	[XmlInclude(typeof(WoodChipper))]
	[XmlInclude(typeof(Cask))]
	[XmlInclude(typeof(SwitchFloor))]
	[XmlInclude(typeof(ColoredObject))]
	[XmlInclude(typeof(IndoorPot))]
	[XmlInclude(typeof(Sign))]
	public class Object : Item
	{
		public enum PreserveType
		{
			Wine,
			Jelly,
			Pickle,
			Juice,
			Roe,
			AgedRoe
		}

		public enum HoneyType
		{
			Wild = -1,
			Poppy = 376,
			Tulip = 591,
			SummerSpangle = 593,
			FairyRose = 595,
			BlueJazz = 597
		}

		public const int copperBar = 334;

		public const int ironBar = 335;

		public const int goldBar = 336;

		public const int iridiumBar = 337;

		public const int wood = 388;

		public const int stone = 390;

		public const int copper = 378;

		public const int iron = 380;

		public const int coal = 382;

		public const int gold = 384;

		public const int iridium = 386;

		public const int inedible = -300;

		public const int GreensCategory = -81;

		public const int GemCategory = -2;

		public const int VegetableCategory = -75;

		public const int FishCategory = -4;

		public const int EggCategory = -5;

		public const int MilkCategory = -6;

		public const int CookingCategory = -7;

		public const int CraftingCategory = -8;

		public const int BigCraftableCategory = -9;

		public const int FruitsCategory = -79;

		public const int SeedsCategory = -74;

		public const int mineralsCategory = -12;

		public const int flowersCategory = -80;

		public const int meatCategory = -14;

		public const int metalResources = -15;

		public const int buildingResources = -16;

		public const int sellAtPierres = -17;

		public const int sellAtPierresAndMarnies = -18;

		public const int fertilizerCategory = -19;

		public const int junkCategory = -20;

		public const int baitCategory = -21;

		public const int tackleCategory = -22;

		public const int sellAtFishShopCategory = -23;

		public const int furnitureCategory = -24;

		public const int ingredientsCategory = -25;

		public const int artisanGoodsCategory = -26;

		public const int syrupCategory = -27;

		public const int monsterLootCategory = -28;

		public const int equipmentCategory = -29;

		public const int clothingCategorySortValue = -94;

		public const int hatCategory = -95;

		public const int ringCategory = -96;

		public const int weaponCategory = -98;

		public const int bootsCategory = -97;

		public const int toolCategory = -99;

		public const int clothingCategory = -100;

		public const int objectInfoNameIndex = 0;

		public const int objectInfoPriceIndex = 1;

		public const int objectInfoEdibilityIndex = 2;

		public const int objectInfoTypeIndex = 3;

		public const int objectInfoDisplayNameIndex = 4;

		public const int objectInfoDescriptionIndex = 5;

		public const int objectInfoMiscIndex = 6;

		public const int objectInfoBuffTypesIndex = 7;

		public const int objectInfoBuffDurationIndex = 8;

		public const int WeedsIndex = 0;

		public const int StoneIndex = 2;

		public const int StickIndex = 4;

		public const int DryDirtTileIndex = 6;

		public const int WateredTileIndex = 7;

		public const int StumpTopLeftIndex = 8;

		public const int BoulderTopLeftIndex = 10;

		public const int StumpBottomLeftIndex = 12;

		public const int BoulderBottomLeftIndex = 14;

		public const int WildHorseradishIndex = 16;

		public const int TulipIndex = 18;

		public const int LeekIndex = 20;

		public const int DandelionIndex = 22;

		public const int ParsnipIndex = 24;

		public const int HandCursorIndex = 26;

		public const int WaterAnimationIndex = 28;

		public const int LumberIndex = 30;

		public const int mineStoneGrey1Index = 32;

		public const int mineStoneBlue1Index = 34;

		public const int mineStoneBlue2Index = 36;

		public const int mineStoneGrey2Index = 38;

		public const int mineStoneBrown1Index = 40;

		public const int mineStoneBrown2Index = 42;

		public const int mineStonePurpleIndex = 44;

		public const int mineStoneMysticIndex = 46;

		public const int mineStoneSnow1 = 48;

		public const int mineStoneSnow2 = 50;

		public const int mineStoneSnow3 = 52;

		public const int mineStonePurpleSnowIndex = 54;

		public const int mineStoneRed1Index = 56;

		public const int mineStoneRed2Index = 58;

		public const int emeraldIndex = 60;

		public const int aquamarineIndex = 62;

		public const int rubyIndex = 64;

		public const int amethystClusterIndex = 66;

		public const int topazIndex = 68;

		public const int sapphireIndex = 70;

		public const int diamondIndex = 72;

		public const int prismaticShardIndex = 74;

		public const int snowHoedDirtIndex = 76;

		public const int beachHoedDirtIndex = 77;

		public const int caveCarrotIndex = 78;

		public const int quartzIndex = 80;

		public const int bobberIndex = 133;

		public const int stardrop = 434;

		public const int spriteSheetTileSize = 16;

		public const int lowQuality = 0;

		public const int medQuality = 1;

		public const int highQuality = 2;

		public const int bestQuality = 4;

		public const int copperPerBar = 10;

		public const int ironPerBar = 10;

		public const int goldPerBar = 10;

		public const int iridiumPerBar = 10;

		public const float wobbleAmountWhenWorking = 10f;

		public const int fragility_Removable = 0;

		public const int fragility_Delicate = 1;

		public const int fragility_Indestructable = 2;

		[XmlElement("tileLocation")]
		public readonly NetVector2 tileLocation = new NetVector2();

		[XmlElement("owner")]
		public readonly NetLong owner = new NetLong();

		[XmlElement("type")]
		public readonly NetString type = new NetString();

		[XmlElement("canBeSetDown")]
		public readonly NetBool canBeSetDown = new NetBool(value: false);

		[XmlElement("canBeGrabbed")]
		public readonly NetBool canBeGrabbed = new NetBool(value: true);

		[XmlElement("isHoedirt")]
		public readonly NetBool isHoedirt = new NetBool(value: false);

		[XmlElement("isSpawnedObject")]
		public readonly NetBool isSpawnedObject = new NetBool(value: false);

		[XmlElement("questItem")]
		public readonly NetBool questItem = new NetBool(value: false);

		[XmlElement("questId")]
		public readonly NetInt questId = new NetInt(0);

		[XmlElement("isOn")]
		public readonly NetBool isOn = new NetBool(value: true);

		[XmlElement("fragility")]
		public readonly NetInt fragility = new NetInt(0);

		private bool isActive;

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("edibility")]
		public readonly NetInt edibility = new NetInt(-300);

		[XmlElement("stack")]
		public readonly NetInt stack = new NetInt(1);

		[XmlElement("quality")]
		public readonly NetInt quality = new NetInt(0);

		[XmlElement("bigCraftable")]
		public readonly NetBool bigCraftable = new NetBool();

		[XmlElement("setOutdoors")]
		public readonly NetBool setOutdoors = new NetBool();

		[XmlElement("setIndoors")]
		public readonly NetBool setIndoors = new NetBool();

		[XmlElement("readyForHarvest")]
		public readonly NetBool readyForHarvest = new NetBool();

		[XmlElement("showNextIndex")]
		public readonly NetBool showNextIndex = new NetBool();

		[XmlElement("flipped")]
		public readonly NetBool flipped = new NetBool();

		[XmlElement("hasBeenPickedUpByFarmer")]
		public readonly NetBool hasBeenPickedUpByFarmer = new NetBool();

		[XmlElement("isRecipe")]
		public readonly NetBool isRecipe = new NetBool();

		[XmlElement("isLamp")]
		public readonly NetBool isLamp = new NetBool();

		[XmlElement("heldObject")]
		public readonly NetRef<Object> heldObject = new NetRef<Object>();

		[XmlElement("minutesUntilReady")]
		public readonly NetIntDelta minutesUntilReady = new NetIntDelta();

		[XmlElement("boundingBox")]
		public readonly NetRectangle boundingBox = new NetRectangle();

		public Vector2 scale;

		[XmlElement("uses")]
		public readonly NetInt uses = new NetInt();

		[XmlIgnore]
		private readonly NetRef<LightSource> netLightSource = new NetRef<LightSource>();

		[XmlIgnore]
		public bool isTemporarilyInvisible;

		[XmlIgnore]
		protected NetBool _destroyOvernight = new NetBool(value: false);

		[XmlElement("orderData")]
		public readonly NetString orderData = new NetString();

		[XmlIgnore]
		public static Chest autoLoadChest;

		[XmlIgnore]
		public int shakeTimer;

		[XmlIgnore]
		public int lastNoteBlockSoundTime;

		[XmlIgnore]
		public ICue internalSound;

		[XmlElement("preserve")]
		public readonly NetNullableEnum<PreserveType> preserve = new NetNullableEnum<PreserveType>();

		[XmlElement("preservedParentSheetIndex")]
		public readonly NetInt preservedParentSheetIndex = new NetInt();

		[XmlElement("honeyType")]
		public readonly NetNullableEnum<HoneyType> honeyType = new NetNullableEnum<HoneyType>();

		[XmlIgnore]
		public string displayName;

		protected int health = 10;

		public bool destroyOvernight
		{
			get
			{
				return _destroyOvernight.Value;
			}
			set
			{
				_destroyOvernight.Value = value;
			}
		}

		[XmlIgnore]
		public LightSource lightSource
		{
			get
			{
				return netLightSource;
			}
			set
			{
				netLightSource.Value = value;
			}
		}

		[XmlIgnore]
		public Vector2 TileLocation
		{
			get
			{
				return tileLocation;
			}
			set
			{
				tileLocation.Value = value;
			}
		}

		[XmlIgnore]
		public string name
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

		[XmlIgnore]
		public override string DisplayName
		{
			get
			{
				if (Game1.objectInformation != null)
				{
					displayName = loadDisplayName();
					if (orderData.Value != null && orderData.Value == "QI_COOKING")
					{
						displayName = Game1.content.LoadString("Strings\\StringsFromCSFiles:Fresh_Prefix", displayName);
					}
				}
				return displayName + (isRecipe ? (((CraftingRecipe.craftingRecipes.ContainsKey(displayName) && CraftingRecipe.craftingRecipes[displayName].Split('/')[2].Split(' ').Count() > 1) ? (" x" + CraftingRecipe.craftingRecipes[displayName].Split('/')[2].Split(' ')[1]) : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12657")) : "");
			}
			set
			{
				displayName = value;
			}
		}

		[XmlIgnore]
		public override string Name
		{
			get
			{
				return name + (isRecipe ? " Recipe" : "");
			}
			set
			{
				name = value;
			}
		}

		[XmlIgnore]
		public string Type
		{
			get
			{
				return type;
			}
			set
			{
				type.Value = value;
			}
		}

		[XmlIgnore]
		public override int Stack
		{
			get
			{
				return Math.Max(0, stack);
			}
			set
			{
				stack.Value = Math.Min(Math.Max(0, value), (value == int.MaxValue) ? value : maximumStackSize());
			}
		}

		[XmlIgnore]
		public int Quality
		{
			get
			{
				return quality;
			}
			set
			{
				quality.Value = value;
			}
		}

		[XmlIgnore]
		public bool CanBeSetDown
		{
			get
			{
				return canBeSetDown;
			}
			set
			{
				canBeSetDown.Value = value;
			}
		}

		[XmlIgnore]
		public bool CanBeGrabbed
		{
			get
			{
				return canBeGrabbed;
			}
			set
			{
				canBeGrabbed.Value = value;
			}
		}

		[XmlIgnore]
		public bool HasBeenPickedUpByFarmer
		{
			get
			{
				return hasBeenPickedUpByFarmer;
			}
			set
			{
				hasBeenPickedUpByFarmer.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsHoeDirt => isHoedirt;

		[XmlIgnore]
		public bool IsOn
		{
			get
			{
				return isOn;
			}
			set
			{
				isOn.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsSpawnedObject
		{
			get
			{
				return isSpawnedObject;
			}
			set
			{
				isSpawnedObject.Value = value;
			}
		}

		[XmlIgnore]
		public bool IsRecipe
		{
			get
			{
				return isRecipe;
			}
			set
			{
				isRecipe.Value = value;
			}
		}

		[XmlIgnore]
		public bool Flipped
		{
			get
			{
				return flipped;
			}
			set
			{
				flipped.Value = value;
			}
		}

		[XmlIgnore]
		public int Price
		{
			get
			{
				return price;
			}
			set
			{
				price.Value = value;
			}
		}

		[XmlIgnore]
		public int Edibility
		{
			get
			{
				return edibility;
			}
			set
			{
				edibility.Value = value;
			}
		}

		[XmlIgnore]
		public int Fragility
		{
			get
			{
				return fragility;
			}
			set
			{
				fragility.Value = value;
			}
		}

		[XmlIgnore]
		public Vector2 Scale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
			}
		}

		[XmlIgnore]
		public int MinutesUntilReady
		{
			get
			{
				return minutesUntilReady;
			}
			set
			{
				minutesUntilReady.Value = value;
			}
		}

		protected virtual void initNetFields()
		{
			base.NetFields.AddFields(tileLocation, owner, type, canBeSetDown, canBeGrabbed, isHoedirt, isSpawnedObject, questItem, questId, isOn, fragility, price, edibility, stack, quality, uses, bigCraftable, setOutdoors, setIndoors, readyForHarvest, showNextIndex, flipped, hasBeenPickedUpByFarmer, isRecipe, isLamp, heldObject, minutesUntilReady, boundingBox, preserve, preservedParentSheetIndex, honeyType, netLightSource, orderData, _destroyOvernight);
		}

		public Object()
		{
			initNetFields();
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, bool isRecipe = false)
			: this()
		{
			this.isRecipe.Value = isRecipe;
			this.tileLocation.Value = tileLocation;
			base.ParentSheetIndex = parentSheetIndex;
			canBeSetDown.Value = true;
			bigCraftable.Value = true;
			Game1.bigCraftablesInformation.TryGetValue(parentSheetIndex, out string objectInformation);
			if (objectInformation != null)
			{
				string[] objectInfoArray = objectInformation.Split('/');
				name = objectInfoArray[0];
				price.Value = Convert.ToInt32(objectInfoArray[1]);
				edibility.Value = Convert.ToInt32(objectInfoArray[2]);
				string[] typeAndCategory = objectInfoArray[3].Split(' ');
				type.Value = typeAndCategory[0];
				if (typeAndCategory.Length > 1)
				{
					base.Category = Convert.ToInt32(typeAndCategory[1]);
				}
				setOutdoors.Value = Convert.ToBoolean(objectInfoArray[5]);
				setIndoors.Value = Convert.ToBoolean(objectInfoArray[6]);
				fragility.Value = Convert.ToInt32(objectInfoArray[7]);
				isLamp.Value = (objectInfoArray.Length > 8 && objectInfoArray[8].Equals("true"));
			}
			initializeLightSource(this.tileLocation);
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public Object(int parentSheetIndex, int initialStack, bool isRecipe = false, int price = -1, int quality = 0)
			: this(Vector2.Zero, parentSheetIndex, initialStack)
		{
			this.isRecipe.Value = isRecipe;
			if (price != -1)
			{
				this.price.Value = price;
			}
			this.quality.Value = quality;
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, int initialStack)
			: this(tileLocation, parentSheetIndex, null, canBeSetDown: true, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
		{
			stack.Value = initialStack;
		}

		public Object(Vector2 tileLocation, int parentSheetIndex, string Givenname, bool canBeSetDown, bool canBeGrabbed, bool isHoedirt, bool isSpawnedObject)
			: this()
		{
			this.tileLocation.Value = tileLocation;
			base.ParentSheetIndex = parentSheetIndex;
			Game1.objectInformation.TryGetValue(parentSheetIndex, out string objectInformation);
			try
			{
				if (objectInformation != null)
				{
					string[] objectInfoArray = objectInformation.Split('/');
					name = objectInfoArray[0];
					price.Value = Convert.ToInt32(objectInfoArray[1]);
					edibility.Value = Convert.ToInt32(objectInfoArray[2]);
					string[] typeAndCategory = objectInfoArray[3].Split(' ');
					type.Value = typeAndCategory[0];
					if (typeAndCategory.Length > 1)
					{
						base.Category = Convert.ToInt32(typeAndCategory[1]);
					}
				}
			}
			catch (Exception)
			{
			}
			if (name == null && Givenname != null)
			{
				name = Givenname;
			}
			else if (name == null)
			{
				name = "Error Item";
			}
			this.canBeSetDown.Value = canBeSetDown;
			this.canBeGrabbed.Value = canBeGrabbed;
			this.isHoedirt.Value = isHoedirt;
			this.isSpawnedObject.Value = isSpawnedObject;
			if (Game1.random.NextDouble() < 0.5 && parentSheetIndex > 52 && (parentSheetIndex < 8 || parentSheetIndex > 15) && (parentSheetIndex < 384 || parentSheetIndex > 391))
			{
				flipped.Value = true;
			}
			if (name.Contains("Block"))
			{
				scale = new Vector2(1f, 1f);
			}
			if (parentSheetIndex == 449 || name.Contains("Weed") || name.Contains("Twig"))
			{
				fragility.Value = 2;
			}
			else if (name.Contains("Fence"))
			{
				scale = new Vector2(10f, 0f);
				canBeSetDown = false;
			}
			else if (name.Contains("Stone"))
			{
				switch (parentSheetIndex)
				{
				case 8:
					minutesUntilReady.Value = 4;
					break;
				case 10:
					minutesUntilReady.Value = 8;
					break;
				case 12:
					minutesUntilReady.Value = 16;
					break;
				case 14:
					minutesUntilReady.Value = 12;
					break;
				case 25:
					minutesUntilReady.Value = 8;
					break;
				default:
					minutesUntilReady.Value = 1;
					break;
				}
			}
			if (parentSheetIndex >= 75 && parentSheetIndex <= 77)
			{
				isSpawnedObject = false;
			}
			initializeLightSource(this.tileLocation);
			if (base.Category == -22)
			{
				scale.Y = 1f;
			}
			boundingBox.Value = new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		protected override void _PopulateContextTags(HashSet<string> tags)
		{
			base._PopulateContextTags(tags);
			if (quality.Value == 0)
			{
				tags.Add("quality_none");
			}
			else if (quality.Value == 1)
			{
				tags.Add("quality_silver");
			}
			else if (quality.Value == 2)
			{
				tags.Add("quality_gold");
			}
			else if (quality.Value == 4)
			{
				tags.Add("quality_iridium");
			}
			if (orderData.Value == "QI_COOKING")
			{
				tags.Add("quality_qi");
			}
			if (preserve != null && preserve.Value.HasValue)
			{
				if (preserve.Value == PreserveType.Jelly)
				{
					tags.Add("jelly_item");
				}
				else if (preserve.Value == PreserveType.Juice)
				{
					tags.Add("juice_item");
				}
				else if (preserve.Value == PreserveType.Wine)
				{
					tags.Add("wine_item");
				}
				else if (preserve.Value == PreserveType.Pickle)
				{
					tags.Add("pickle_item");
				}
			}
			if (preservedParentSheetIndex.Value > 0)
			{
				tags.Add("preserve_sheet_index_" + preservedParentSheetIndex.Value);
			}
		}

		protected virtual string loadDisplayName()
		{
			if (preserve.Value.HasValue)
			{
				Game1.objectInformation.TryGetValue(preservedParentSheetIndex, out string objectInformation4);
				if (!string.IsNullOrEmpty(objectInformation4))
				{
					string preservedName = objectInformation4.Split('/')[4];
					switch (preserve.Value)
					{
					case PreserveType.Wine:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12730", preservedName);
					case PreserveType.Jelly:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12739", preservedName);
					case PreserveType.Pickle:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12735", preservedName);
					case PreserveType.Juice:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12726", preservedName);
					case PreserveType.Roe:
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Roe_DisplayName", preservedName);
					case PreserveType.AgedRoe:
						if (preservedParentSheetIndex.Value > 0)
						{
							return Game1.content.LoadString("Strings\\StringsFromCSFiles:AgedRoe_DisplayName", preservedName);
						}
						break;
					}
				}
			}
			else
			{
				if (name != null && name.Contains("Honey"))
				{
					_ = preservedParentSheetIndex.Value;
					if (preservedParentSheetIndex.Value == -1)
					{
						if (Name == "Honey")
						{
							Name = "Wild Honey";
						}
						return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12750");
					}
					if (preservedParentSheetIndex.Value == 0)
					{
						Game1.objectInformation.TryGetValue(parentSheetIndex, out string objectInformation3);
						if (!string.IsNullOrEmpty(objectInformation3))
						{
							return objectInformation3.Split('/')[4];
						}
					}
					string honeyName = Game1.objectInformation[preservedParentSheetIndex.Value].Split('/')[4];
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12760", honeyName);
				}
				if ((bool)bigCraftable)
				{
					Game1.bigCraftablesInformation.TryGetValue(parentSheetIndex, out string objectInformation2);
					if (!string.IsNullOrEmpty(objectInformation2))
					{
						string[] array = objectInformation2.Split('/');
						return array[array.Length - 1];
					}
				}
				else
				{
					Game1.objectInformation.TryGetValue(parentSheetIndex, out string objectInformation);
					if (!string.IsNullOrEmpty(objectInformation))
					{
						return objectInformation.Split('/')[4];
					}
				}
			}
			return name;
		}

		public Vector2 getLocalPosition(xTile.Dimensions.Rectangle viewport)
		{
			return new Vector2(tileLocation.X * 64f - (float)viewport.X, tileLocation.Y * 64f - (float)viewport.Y);
		}

		public static Microsoft.Xna.Framework.Rectangle getSourceRectForBigCraftable(int index)
		{
			return new Microsoft.Xna.Framework.Rectangle(index % (Game1.bigCraftableSpriteSheet.Width / 16) * 16, index * 16 / Game1.bigCraftableSpriteSheet.Width * 16 * 2, 16, 32);
		}

		public virtual bool performToolAction(Tool t, GameLocation location)
		{
			if (isTemporarilyInvisible)
			{
				return false;
			}
			if ((bool)bigCraftable && (int)parentSheetIndex == 165 && heldObject.Value != null && heldObject.Value is Chest && !(heldObject.Value as Chest).isEmpty())
			{
				(heldObject.Value as Chest).clearNulls();
				if (t != null && t.isHeavyHitter() && !(t is MeleeWeapon))
				{
					location.playSound("hammer");
					shakeTimer = 100;
				}
				return false;
			}
			if (t == null)
			{
				if (location.objects.ContainsKey(tileLocation) && location.objects[tileLocation].Equals(this))
				{
					if (location.farmers.Count > 0)
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
					}
					location.objects.Remove(tileLocation);
				}
				return false;
			}
			if (name.Equals("Stone") && t is Pickaxe)
			{
				int damage = 1;
				switch ((int)t.upgradeLevel)
				{
				case 1:
					damage = 2;
					break;
				case 2:
					damage = 3;
					break;
				case 3:
					damage = 4;
					break;
				case 4:
					damage = 5;
					break;
				}
				if (((int)parentSheetIndex == 12 && (int)t.upgradeLevel == 1) || (((int)parentSheetIndex == 12 || (int)parentSheetIndex == 14) && (int)t.upgradeLevel == 0))
				{
					damage = 0;
					location.playSound("crafting");
				}
				minutesUntilReady.Value -= damage;
				if ((int)minutesUntilReady <= 0)
				{
					return true;
				}
				location.playSound("hammer");
				shakeTimer = 100;
				return false;
			}
			if (name.Equals("Stone") && t is Pickaxe)
			{
				return false;
			}
			if (name.Equals("Boulder") && ((int)t.upgradeLevel != 4 || !(t is Pickaxe)))
			{
				if (t.isHeavyHitter())
				{
					location.playSound("hammer");
				}
				return false;
			}
			if (name.Contains("Weeds") && t.isHeavyHitter())
			{
				if (base.ParentSheetIndex != 319 && base.ParentSheetIndex != 320 && base.ParentSheetIndex != 321 && t.getLastFarmerToUse() != null)
				{
					foreach (BaseEnchantment enchantment in t.getLastFarmerToUse().enchantments)
					{
						enchantment.OnCutWeed(tileLocation, location, t.getLastFarmerToUse());
					}
				}
				cutWeed(t.getLastFarmerToUse(), location);
				return true;
			}
			if (name.Contains("Twig") && t is Axe)
			{
				fragility.Value = 2;
				location.playSound("axchop");
				t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(new Object(388, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
				return true;
			}
			if (name.Contains("SupplyCrate") && t.isHeavyHitter())
			{
				MinutesUntilReady -= (int)t.upgradeLevel + 1;
				if (MinutesUntilReady <= 0)
				{
					fragility.Value = 2;
					location.playSound("barrelBreak");
					Random r = new Random((int)((float)(double)Game1.uniqueIDForThisGame + tileLocation.X * 777f + tileLocation.Y * 7f));
					int houseLevel = t.getLastFarmerToUse().HouseUpgradeLevel;
					int x = (int)tileLocation.X;
					int y = (int)tileLocation.Y;
					switch (houseLevel)
					{
					case 0:
						switch (r.Next(6))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(371, x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(535, x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(241, x, y, r.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(395, x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(286, x, y, r.Next(3, 6), location);
							break;
						}
						break;
					case 1:
						switch (r.Next(9))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(371, x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(749, x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(253, x, y, r.Next(1, 3), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(237, x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(246, x, y, r.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(247, x, y, r.Next(2, 5), location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(245, x, y, r.Next(4, 8), location);
							break;
						case 8:
							Game1.createMultipleObjectDebris(287, x, y, r.Next(3, 6), location);
							break;
						}
						break;
					default:
						switch (r.Next(8))
						{
						case 0:
							Game1.createMultipleObjectDebris(770, x, y, r.Next(3, 6), location);
							break;
						case 1:
							Game1.createMultipleObjectDebris(920, x, y, r.Next(5, 8), location);
							break;
						case 2:
							Game1.createMultipleObjectDebris(749, x, y, r.Next(2, 5), location);
							break;
						case 3:
							Game1.createMultipleObjectDebris(253, x, y, r.Next(2, 4), location);
							break;
						case 4:
							Game1.createMultipleObjectDebris(r.Next(904, 906), x, y, r.Next(1, 3), location);
							break;
						case 5:
							Game1.createMultipleObjectDebris(246, x, y, r.Next(4, 8), location);
							Game1.createMultipleObjectDebris(247, x, y, r.Next(2, 5), location);
							Game1.createMultipleObjectDebris(245, x, y, r.Next(4, 8), location);
							break;
						case 6:
							Game1.createMultipleObjectDebris(275, x, y, 2, location);
							break;
						case 7:
							Game1.createMultipleObjectDebris(288, x, y, r.Next(3, 6), location);
							break;
						}
						break;
					}
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
					return true;
				}
				shakeTimer = 200;
				location.playSound("woodWhack");
				return false;
			}
			if ((int)parentSheetIndex == 590)
			{
				if (t is Hoe)
				{
					location.digUpArtifactSpot((int)tileLocation.X, (int)tileLocation.Y, t.getLastFarmerToUse());
					if (!location.terrainFeatures.ContainsKey(tileLocation))
					{
						location.makeHoeDirt(tileLocation, ignoreChecks: true);
					}
					location.playSound("hoeHit");
					if (location.objects.ContainsKey(tileLocation))
					{
						location.objects.Remove(tileLocation);
					}
				}
				return false;
			}
			if ((int)fragility == 2)
			{
				return false;
			}
			if (type != null && type.Equals("Crafting") && !(t is MeleeWeapon) && t.isHeavyHitter())
			{
				if (t is Hoe && IsSprinkler())
				{
					return false;
				}
				location.playSound("hammer");
				if ((int)fragility == 1)
				{
					Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
					Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(3, 6), resource: false);
					DelayedAction.functionAfterDelay(delegate
					{
						Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
						Game1.createRadialDebris(location, 14, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(2, 5), resource: false);
					}, 80);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f));
					performRemoveAction(tileLocation, location);
					if (location.objects.ContainsKey(tileLocation))
					{
						location.objects.Remove(tileLocation);
					}
					return false;
				}
				if (name.Contains("Tapper") && t.getLastFarmerToUse().currentLocation.terrainFeatures.ContainsKey(tileLocation) && t.getLastFarmerToUse().currentLocation.terrainFeatures[tileLocation] is Tree)
				{
					(t.getLastFarmerToUse().currentLocation.terrainFeatures[tileLocation] as Tree).tapped.Value = false;
				}
				if (Name == "Ostrich Incubator")
				{
					if (heldObject.Value != null)
					{
						base.ParentSheetIndex--;
						t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
						heldObject.Value = null;
						return true;
					}
					return true;
				}
				if ((bool)bigCraftable && base.ParentSheetIndex == 21 && heldObject.Value != null)
				{
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
					heldObject.Value = null;
				}
				if (IsSprinkler() && heldObject.Value != null)
				{
					if (heldObject.Value.heldObject.Value != null)
					{
						Chest chest = heldObject.Value.heldObject.Value as Chest;
						if (chest != null)
						{
							chest.GetMutex().RequestLock(delegate
							{
								List<Item> list = new List<Item>(chest.items);
								chest.items.Clear();
								foreach (Item current in list)
								{
									if (current != null)
									{
										t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(current, tileLocation.Value * 64f + new Vector2(32f, 32f)));
									}
								}
								Object value = heldObject.Value;
								heldObject.Value = null;
								t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(value, tileLocation.Value * 64f + new Vector2(32f, 32f)));
								chest.GetMutex().ReleaseLock();
							});
						}
						return false;
					}
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
					heldObject.Value = null;
					return false;
				}
				if (heldObject.Value != null && (bool)readyForHarvest)
				{
					t.getLastFarmerToUse().currentLocation.debris.Add(new Debris((Object)heldObject, tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
				if ((int)parentSheetIndex == 157)
				{
					base.ParentSheetIndex = 156;
					heldObject.Value = null;
					minutesUntilReady.Value = -1;
				}
				if (name.Contains("Seasonal"))
				{
					base.ParentSheetIndex -= base.ParentSheetIndex % 4;
				}
				return true;
			}
			return false;
		}

		protected virtual void cutWeed(Farmer who, GameLocation location = null)
		{
			if (location == null && who != null)
			{
				location = who.currentLocation;
			}
			Color c = Color.Green;
			string sound = "cut";
			int animation = 50;
			fragility.Value = 2;
			int toDrop = -1;
			if (Game1.random.NextDouble() < 0.5)
			{
				toDrop = 771;
			}
			else if (Game1.random.NextDouble() < 0.05)
			{
				toDrop = 770;
			}
			switch ((int)parentSheetIndex)
			{
			case 678:
				c = new Color(228, 109, 159);
				break;
			case 679:
				c = new Color(253, 191, 46);
				break;
			case 313:
			case 314:
			case 315:
				c = new Color(84, 101, 27);
				break;
			case 316:
			case 317:
			case 318:
				c = new Color(109, 49, 196);
				break;
			case 319:
				c = new Color(30, 216, 255);
				sound = "breakingGlass";
				animation = 47;
				location.playSound("drumkit2");
				toDrop = -1;
				break;
			case 320:
				c = new Color(175, 143, 255);
				sound = "breakingGlass";
				animation = 47;
				location.playSound("drumkit2");
				toDrop = -1;
				break;
			case 321:
				c = new Color(73, 255, 158);
				sound = "breakingGlass";
				animation = 47;
				location.playSound("drumkit2");
				toDrop = -1;
				break;
			case 792:
			case 793:
			case 794:
				toDrop = 770;
				break;
			case 882:
			case 883:
			case 884:
				c = new Color(30, 97, 68);
				if (Game1.MasterPlayer.hasOrWillReceiveMail("islandNorthCaveOpened") && Game1.random.NextDouble() < 0.1 && !Game1.MasterPlayer.hasOrWillReceiveMail("gotMummifiedFrog"))
				{
					Game1.addMailForTomorrow("gotMummifiedFrog", noLetter: true, sendToEveryone: true);
					toDrop = 828;
				}
				else if (Game1.random.NextDouble() < 0.01)
				{
					toDrop = 828;
				}
				else if (Game1.random.NextDouble() < 0.08)
				{
					toDrop = 831;
				}
				break;
			}
			if (sound.Equals("breakingGlass") && Game1.random.NextDouble() < 0.0025)
			{
				toDrop = 338;
			}
			location.playSound(sound);
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f, c));
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
			{
				scale = 0.75f,
				flipped = true
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
			{
				scale = 0.75f,
				delayBeforeAnimationStart = 50
			});
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(animation, tileLocation.Value * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), c * 0.75f)
			{
				scale = 0.75f,
				flipped = true,
				delayBeforeAnimationStart = 100
			});
			if (!sound.Equals("breakingGlass"))
			{
				if (Game1.random.NextDouble() < 1E-05)
				{
					location.debris.Add(new Debris(new Hat(40), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
				if (Game1.random.NextDouble() <= 0.01 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					location.debris.Add(new Debris(new Object(890, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
				}
			}
			if (toDrop != -1)
			{
				location.debris.Add(new Debris(new Object(toDrop, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				location.addJumperFrog(tileLocation);
			}
			if (who.currentLocation.HasUnlockedAreaSecretNotes(who) && Game1.random.NextDouble() < 0.009)
			{
				Object o = location.tryToCreateUnseenSecretNote(who);
				if (o != null)
				{
					Game1.createItemDebris(o, new Vector2(tileLocation.X + 0.5f, tileLocation.Y + 0.75f) * 64f, Game1.player.facingDirection, location);
				}
			}
		}

		public virtual bool isAnimalProduct()
		{
			if (base.Category != -18 && base.Category != -5 && base.Category != -6)
			{
				return (int)parentSheetIndex == 430;
			}
			return true;
		}

		public virtual bool onExplosion(Farmer who, GameLocation location)
		{
			if (who == null)
			{
				return false;
			}
			if (name.Contains("Weed"))
			{
				fragility.Value = 0;
				cutWeed(who, location);
				location.removeObject(tileLocation, showDestroyedObject: false);
			}
			if (name.Contains("Twig"))
			{
				fragility.Value = 0;
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 10), resource: false);
				location.debris.Add(new Debris(new Object(388, 1), tileLocation.Value * 64f + new Vector2(32f, 32f)));
			}
			if (name.Contains("Stone"))
			{
				fragility.Value = 0;
			}
			performRemoveAction(tileLocation, location);
			return true;
		}

		public virtual bool canBeShipped()
		{
			if (!bigCraftable && type != null && !type.Equals("Quest") && canBeTrashed() && !(this is Furniture))
			{
				return !(this is Wallpaper);
			}
			return false;
		}

		public virtual void ApplySprinkler(GameLocation location, Vector2 tile)
		{
			if (!(location.doesTileHavePropertyNoNull((int)tile.X, (int)tile.Y, "NoSprinklers", "Back") == "T") && location.terrainFeatures.ContainsKey(tile) && location.terrainFeatures[tile] is HoeDirt && (int)(location.terrainFeatures[tile] as HoeDirt).state != 2)
			{
				(location.terrainFeatures[tile] as HoeDirt).state.Value = 1;
			}
		}

		public virtual void ApplySprinklerAnimation(GameLocation location)
		{
			int radius = GetModifiedRadiusForSprinkler();
			if (radius >= 0)
			{
				switch (radius)
				{
				case 0:
				{
					int delay = Game1.random.Next(1000);
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, -48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						delayBeforeAnimationStart = delay,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = (float)Math.PI / 2f,
						delayBeforeAnimationStart = delay,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(0f, 48f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = (float)Math.PI,
						delayBeforeAnimationStart = delay,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite(29, tileLocation.Value * 64f + new Vector2(-48f, 0f), Color.White * 0.5f, 4, flipped: false, 60f, 100)
					{
						rotation = 4.712389f,
						delayBeforeAnimationStart = delay,
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					break;
				}
				case 1:
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1984, 192, 192), 60f, 3, 100, tileLocation.Value * 64f + new Vector2(-64f, -64f), flicker: false, flipped: false)
					{
						color = Color.White * 0.4f,
						delayBeforeAnimationStart = Game1.random.Next(1000),
						id = tileLocation.X * 4000f + tileLocation.Y
					});
					break;
				default:
				{
					float scale = (float)radius / 2f;
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2176, 320, 320), 60f, 4, 100, tileLocation.Value * 64f + new Vector2(32f, 32f) + new Vector2(-160f, -160f) * scale, flicker: false, flipped: false)
					{
						color = Color.White * 0.4f,
						delayBeforeAnimationStart = Game1.random.Next(1000),
						id = tileLocation.X * 4000f + tileLocation.Y,
						scale = scale
					});
					break;
				}
				}
			}
		}

		public virtual List<Vector2> GetSprinklerTiles()
		{
			int radius = GetModifiedRadiusForSprinkler();
			if (radius == 0)
			{
				return Utility.getAdjacentTileLocations(tileLocation);
			}
			if (radius > 0)
			{
				List<Vector2> tiles = new List<Vector2>();
				for (int i = (int)tileLocation.X - radius; (float)i <= tileLocation.X + (float)radius; i++)
				{
					for (int y = (int)tileLocation.Y - radius; (float)y <= tileLocation.Y + (float)radius; y++)
					{
						if (i != 0 || y != 0)
						{
							tiles.Add(new Vector2(i, y));
						}
					}
				}
				return tiles;
			}
			return new List<Vector2>();
		}

		public virtual bool IsInSprinklerRangeBroadphase(Vector2 target)
		{
			int radius = GetModifiedRadiusForSprinkler();
			if (radius == 0)
			{
				radius = 1;
			}
			if (Math.Abs(target.X - TileLocation.X) <= (float)radius)
			{
				return Math.Abs(target.Y - TileLocation.Y) <= (float)radius;
			}
			return false;
		}

		public virtual void DayUpdate(GameLocation location)
		{
			health = 10;
			if (IsSprinkler() && (!Game1.IsRainingHere(location) || !location.isOutdoors) && GetModifiedRadiusForSprinkler() >= 0)
			{
				location.postFarmEventOvernightActions.Add(delegate
				{
					if (!Game1.player.team.SpecialOrderRuleActive("NO_SPRINKLER"))
					{
						foreach (Vector2 current in GetSprinklerTiles())
						{
							ApplySprinkler(location, current);
						}
						ApplySprinklerAnimation(location);
					}
				});
			}
			if ((bool)bigCraftable)
			{
				switch ((int)parentSheetIndex)
				{
				case 231:
					if (!Game1.IsRainingHere(location) && location.IsOutdoors)
					{
						MinutesUntilReady -= 2400;
						if (MinutesUntilReady <= 0)
						{
							readyForHarvest.Value = true;
						}
					}
					break;
				case 246:
					heldObject.Value = new Object(395, 1);
					readyForHarvest.Value = true;
					break;
				case 272:
					if (location is AnimalHouse)
					{
						foreach (KeyValuePair<long, FarmAnimal> pair in (location as AnimalHouse).animals.Pairs)
						{
							pair.Value.pet(Game1.player, is_auto_pet: true);
						}
					}
					break;
				case 165:
					if (location != null && location is AnimalHouse)
					{
						foreach (KeyValuePair<long, FarmAnimal> kvp in (location as AnimalHouse).animals.Pairs)
						{
							if ((byte)kvp.Value.harvestType == 1 && (int)kvp.Value.currentProduce > 0 && (int)kvp.Value.currentProduce != 430 && heldObject.Value != null && heldObject.Value is Chest && (heldObject.Value as Chest).addItem(new Object(Vector2.Zero, kvp.Value.currentProduce.Value, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
							{
								Quality = kvp.Value.produceQuality
							}) == null)
							{
								Utility.RecordAnimalProduce(kvp.Value, kvp.Value.currentProduce);
								kvp.Value.currentProduce.Value = -1;
								if ((bool)kvp.Value.showDifferentTextureWhenReadyForHarvest)
								{
									kvp.Value.Sprite.LoadTexture("Animals\\Sheared" + kvp.Value.type.Value);
								}
								showNextIndex.Value = true;
							}
						}
					}
					break;
				case 157:
					if ((int)minutesUntilReady <= 0 && heldObject.Value != null && location.canSlimeHatchHere())
					{
						GreenSlime slime = null;
						Vector2 v = new Vector2((int)tileLocation.X, (int)tileLocation.Y + 1) * 64f;
						switch ((int)heldObject.Value.parentSheetIndex)
						{
						case 680:
							slime = new GreenSlime(v, 0);
							break;
						case 413:
							slime = new GreenSlime(v, 40);
							break;
						case 437:
							slime = new GreenSlime(v, 80);
							break;
						case 439:
							slime = new GreenSlime(v, 121);
							break;
						case 857:
							slime = new GreenSlime(v, 121);
							slime.makeTigerSlime();
							break;
						}
						if (slime != null)
						{
							Game1.showGlobalMessage(slime.cute ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12689") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12691"));
							Vector2 openSpot = Utility.recursiveFindOpenTileForCharacter(slime, location, tileLocation + new Vector2(0f, 1f), 10, allowOffMap: false);
							slime.setTilePosition((int)openSpot.X, (int)openSpot.Y);
							location.characters.Add(slime);
							heldObject.Value = null;
							base.ParentSheetIndex = 156;
							minutesUntilReady.Value = -1;
						}
					}
					break;
				case 10:
					if (location.GetSeasonForLocation().Equals("winter"))
					{
						heldObject.Value = null;
						readyForHarvest.Value = false;
						showNextIndex.Value = false;
						minutesUntilReady.Value = -1;
					}
					else if (heldObject.Value == null)
					{
						heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
					}
					break;
				case 108:
				case 109:
					base.ParentSheetIndex = 108;
					if (Game1.currentSeason.Equals("winter") || Game1.currentSeason.Equals("fall"))
					{
						base.ParentSheetIndex = 109;
					}
					break;
				case 117:
					heldObject.Value = new Object(167, 1);
					break;
				case 104:
					if (Game1.currentSeason.Equals("winter"))
					{
						minutesUntilReady.Value = 9999;
					}
					else
					{
						minutesUntilReady.Value = -1;
					}
					break;
				case 127:
				{
					NPC i = Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth);
					minutesUntilReady.Value = 1;
					if (i != null)
					{
						heldObject.Value = i.getFavoriteItem();
						break;
					}
					int index = 80;
					switch (Game1.random.Next(4))
					{
					case 0:
						index = 72;
						break;
					case 1:
						index = 337;
						break;
					case 2:
						index = 749;
						break;
					case 3:
						index = 336;
						break;
					}
					heldObject.Value = new Object(index, 1);
					break;
				}
				case 160:
					minutesUntilReady.Value = 1;
					heldObject.Value = new Object(386, Game1.random.Next(2, 9));
					break;
				case 280:
					minutesUntilReady.Value = 1;
					heldObject.Value = new Object(74, 1);
					break;
				case 164:
					if (!(location is Town))
					{
						break;
					}
					if (Game1.random.NextDouble() < 0.9)
					{
						if (Game1.getLocationFromName("ManorHouse").isTileLocationTotallyClearAndPlaceable(22, 6))
						{
							if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
							{
								Game1.mailbox.Add("lewisStatue");
							}
							rot();
							Game1.getLocationFromName("ManorHouse").objects.Add(new Vector2(22f, 6f), new Object(Vector2.Zero, 164));
						}
					}
					else if (Game1.getLocationFromName("AnimalShop").isTileLocationTotallyClearAndPlaceable(11, 6))
					{
						if (!Game1.player.hasOrWillReceiveMail("lewisStatue"))
						{
							Game1.mailbox.Add("lewisStatue");
						}
						rot();
						Game1.getLocationFromName("AnimalShop").objects.Add(new Vector2(11f, 6f), new Object(Vector2.Zero, 164));
					}
					break;
				case 128:
					if (heldObject.Value == null)
					{
						int whichMushroom2 = 404;
						whichMushroom2 = ((Game1.random.NextDouble() < 0.025) ? 422 : ((Game1.random.NextDouble() < 0.075) ? 281 : ((Game1.random.NextDouble() < 0.09) ? 257 : ((!(Game1.random.NextDouble() < 0.15)) ? 404 : 420))));
						heldObject.Value = new Object(whichMushroom2, 1);
						Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 2);
					}
					break;
				}
				if (name.Contains("Seasonal"))
				{
					int baseIndex = base.ParentSheetIndex - base.ParentSheetIndex % 4;
					base.ParentSheetIndex = baseIndex + Utility.getSeasonNumber(Game1.currentSeason);
				}
			}
			if ((bool)bigCraftable)
			{
				return;
			}
			string season = location.GetSeasonForLocation();
			switch ((int)parentSheetIndex)
			{
			case 746:
				if (season.Equals("winter"))
				{
					rot();
				}
				break;
			case 784:
			case 785:
				if (Game1.dayOfMonth == 1 && !season.Equals("spring") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex++;
				}
				break;
			case 674:
			case 675:
				if (Game1.dayOfMonth == 1 && season.Equals("summer") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex += 2;
				}
				break;
			case 676:
			case 677:
				if (Game1.dayOfMonth == 1 && season.Equals("fall") && (bool)location.isOutdoors)
				{
					base.ParentSheetIndex += 2;
				}
				break;
			}
		}

		public virtual void rot()
		{
			Random r = new Random(Game1.year * 999 + Game1.dayOfMonth + Utility.getSeasonNumber(Game1.currentSeason));
			base.ParentSheetIndex = r.Next(747, 749);
			price.Value = 0;
			quality.Value = 0;
			name = "Rotten Plant";
			displayName = null;
			lightSource = null;
			bigCraftable.Value = false;
		}

		public override void actionWhenBeingHeld(Farmer who)
		{
			if (Game1.eventUp && Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival)
			{
				if (lightSource != null && who.currentLocation != null && who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
				{
					who.currentLocation.removeLightSource((int)who.UniqueMultiplayerID);
				}
				base.actionWhenBeingHeld(who);
				return;
			}
			if (lightSource != null && (!bigCraftable || (bool)isLamp) && who.currentLocation != null)
			{
				if (!who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
				{
					who.currentLocation.sharedLights[(int)who.UniqueMultiplayerID] = new LightSource(lightSource.textureIndex, lightSource.position, lightSource.radius, lightSource.color, (int)who.UniqueMultiplayerID, LightSource.LightContext.None, who.uniqueMultiplayerID);
				}
				who.currentLocation.repositionLightSource((int)who.UniqueMultiplayerID, who.position + new Vector2(32f, -64f));
			}
			base.actionWhenBeingHeld(who);
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			if (lightSource != null && who.currentLocation != null && who.currentLocation.hasLightSource((int)who.UniqueMultiplayerID))
			{
				who.currentLocation.removeLightSource((int)who.UniqueMultiplayerID);
			}
			base.actionWhenStopBeingHeld(who);
		}

		public virtual void ConsumeInventoryItem(Farmer who, int parent_sheet_index, int amount)
		{
			IList<Item> items = who.Items;
			if (autoLoadChest != null)
			{
				items = autoLoadChest.items;
			}
			int i = items.Count - 1;
			while (true)
			{
				if (i >= 0)
				{
					if (Utility.IsNormalObjectAtParentSheetIndex(items[i], parent_sheet_index))
					{
						break;
					}
					i--;
					continue;
				}
				return;
			}
			items[i].Stack--;
			if (items[i].Stack <= 0)
			{
				if (who.ActiveObject == items[i])
				{
					who.ActiveObject = null;
				}
				items[i] = null;
			}
		}

		public virtual void ConsumeInventoryItem(Farmer who, Item drop_in, int amount)
		{
			drop_in.Stack -= amount;
			if (drop_in.Stack > 0)
			{
				return;
			}
			if (autoLoadChest != null)
			{
				bool found_item = false;
				for (int i = 0; i < autoLoadChest.items.Count; i++)
				{
					if (autoLoadChest.items[i] == drop_in)
					{
						autoLoadChest.items[i] = null;
						found_item = true;
						break;
					}
				}
				if (found_item)
				{
					autoLoadChest.clearNulls();
				}
			}
			else
			{
				who.removeItemFromInventory(drop_in);
			}
		}

		public virtual int GetTallyOfObject(Farmer who, int index, bool big_craftable)
		{
			if (autoLoadChest != null)
			{
				int tally = 0;
				{
					foreach (Item i in autoLoadChest.items)
					{
						if (i != null && i is Object && (i as Object).ParentSheetIndex == index && (bool)(i as Object).bigCraftable == big_craftable)
						{
							tally += i.Stack;
						}
					}
					return tally;
				}
			}
			return who.getTallyOfObject(index, big_craftable);
		}

		public virtual Object GetDeconstructorOutput(Item item)
		{
			if (!CraftingRecipe.craftingRecipes.ContainsKey(item.Name))
			{
				return null;
			}
			if (CraftingRecipe.craftingRecipes[item.Name].Split('/')[2].Split(' ').Count() > 1)
			{
				return null;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 710))
			{
				return new Object(334, 2);
			}
			string[] ingredients = CraftingRecipe.craftingRecipes[item.Name].Split('/')[0].Split(' ');
			List<Object> ingredient_objects = new List<Object>();
			for (int i = 0; i < ingredients.Count(); i += 2)
			{
				ingredient_objects.Add(new Object(Convert.ToInt32(ingredients[i]), Convert.ToInt32(ingredients[i + 1])));
			}
			if (ingredient_objects.Count == 0)
			{
				return null;
			}
			ingredient_objects.Sort((Object a, Object b) => a.sellToStorePrice(-1L) * a.Stack - b.sellToStorePrice(-1L) * b.Stack);
			return ingredient_objects.Last();
		}

		public virtual bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
		{
			if (isTemporarilyInvisible)
			{
				return false;
			}
			if (dropInItem is Object)
			{
				Object dropIn = dropInItem as Object;
				if (IsSprinkler() && heldObject.Value == null && (Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 915) || Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 913)))
				{
					if (probe)
					{
						return true;
					}
					if (who.currentLocation is MineShaft || (who.currentLocation is VolcanoDungeon && Utility.IsNormalObjectAtParentSheetIndex(dropInItem, 913)))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					Object attached_object = dropIn.getOne() as Object;
					if (Utility.IsNormalObjectAtParentSheetIndex(attached_object, 913) && attached_object.heldObject.Value == null)
					{
						Chest chest = new Chest();
						chest.SpecialChestType = Chest.SpecialChestTypes.Enricher;
						attached_object.heldObject.Value = chest;
					}
					who.currentLocation.playSound("axe");
					heldObject.Value = attached_object;
					minutesUntilReady.Value = -1;
					return true;
				}
				if (dropInItem is Wallpaper)
				{
					return false;
				}
				if (dropIn != null && (int)dropIn.parentSheetIndex == 872 && autoLoadChest == null)
				{
					if (Name == "Ostrich Incubator" || Name == "Slime Incubator" || Name == "Incubator")
					{
						return false;
					}
					if (MinutesUntilReady > 0)
					{
						if (probe)
						{
							return true;
						}
						Utility.addSprinklesToLocation(who.currentLocation, (int)tileLocation.X, (int)tileLocation.Y, 1, 2, 400, 40, Color.White);
						Game1.playSound("yoba");
						MinutesUntilReady = 10;
						who.reduceActiveItemByOne();
						DelayedAction.functionAfterDelay(delegate
						{
							minutesElapsed(10, who.currentLocation);
						}, 50);
					}
				}
				if (heldObject.Value != null && !name.Equals("Recycling Machine") && !name.Equals("Crystalarium"))
				{
					return false;
				}
				if (dropIn != null && (bool)dropIn.bigCraftable && !name.Equals("Deconstructor"))
				{
					return false;
				}
				if ((bool)bigCraftable && !probe && dropIn != null && heldObject.Value == null)
				{
					scale.X = 5f;
				}
				if (probe && MinutesUntilReady > 0)
				{
					return false;
				}
				if (name.Equals("Incubator"))
				{
					if (heldObject.Value == null && dropIn.ParentSheetIndex != 289 && (dropIn.Category == -5 || Utility.IsNormalObjectAtParentSheetIndex(dropIn, 107)))
					{
						heldObject.Value = new Object(dropIn.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 9000 * (((int)dropIn.parentSheetIndex != 107) ? 1 : 2);
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							if (dropIn.ParentSheetIndex == 180 || dropIn.ParentSheetIndex == 182 || dropIn.ParentSheetIndex == 305)
							{
								base.ParentSheetIndex += 2;
							}
							else
							{
								base.ParentSheetIndex++;
							}
							if (who != null && who.currentLocation != null && who.currentLocation is AnimalHouse)
							{
								(who.currentLocation as AnimalHouse).hasShownIncubatorBuildingFullMessage = false;
							}
						}
						return true;
					}
				}
				else if (name.Equals("Ostrich Incubator"))
				{
					if (heldObject.Value == null && (int)dropIn.parentSheetIndex == 289)
					{
						heldObject.Value = new Object(dropIn.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 15000;
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							base.ParentSheetIndex++;
							if (who != null && who.currentLocation != null && who.currentLocation is AnimalHouse)
							{
								(who.currentLocation as AnimalHouse).hasShownIncubatorBuildingFullMessage = false;
							}
						}
						return true;
					}
				}
				else if (name.Equals("Slime Incubator"))
				{
					if (heldObject.Value == null && dropIn.name.Contains("Slime Egg"))
					{
						heldObject.Value = new Object(dropIn.parentSheetIndex, 1);
						if (!probe)
						{
							who.currentLocation.playSound("coin");
							minutesUntilReady.Value = 4000;
							if (who.professions.Contains(2))
							{
								minutesUntilReady.Value /= 2;
							}
							base.ParentSheetIndex++;
						}
						return true;
					}
				}
				else if (name.Equals("Deconstructor"))
				{
					Object deconstructor_output = GetDeconstructorOutput(dropIn);
					if (deconstructor_output != null)
					{
						heldObject.Value = new Object(dropIn.parentSheetIndex, 1);
						if (!probe)
						{
							heldObject.Value = deconstructor_output;
							MinutesUntilReady = 60;
							Game1.playSound("furnace");
							return true;
						}
						return true;
					}
					if (!probe)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Deconstructor_fail"));
						}
						return false;
					}
				}
				else if (name.Equals("Bone Mill"))
				{
					int numItemsToTake = 0;
					switch ((int)dropIn.parentSheetIndex)
					{
					case 579:
					case 580:
					case 581:
					case 582:
					case 583:
					case 584:
					case 585:
					case 586:
					case 587:
					case 588:
					case 589:
					case 820:
					case 821:
					case 822:
					case 823:
					case 824:
					case 825:
					case 826:
					case 827:
					case 828:
						numItemsToTake = 1;
						break;
					case 881:
						numItemsToTake = 5;
						break;
					}
					if (numItemsToTake == 0)
					{
						return false;
					}
					if (probe)
					{
						return true;
					}
					if (dropIn.Stack < numItemsToTake)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:bonemill_5"));
						}
						return false;
					}
					int which = -1;
					int howMany = 1;
					switch (Game1.random.Next(4))
					{
					case 0:
						which = 466;
						howMany = 3;
						break;
					case 1:
						which = 465;
						howMany = 5;
						break;
					case 2:
						which = 369;
						howMany = 10;
						break;
					case 3:
						which = 805;
						howMany = 5;
						break;
					}
					if (Game1.random.NextDouble() < 0.1)
					{
						howMany *= 2;
					}
					heldObject.Value = new Object(which, howMany);
					if (!probe)
					{
						ConsumeInventoryItem(who, dropIn, numItemsToTake);
						minutesUntilReady.Value = 240;
						who.currentLocation.playSound("skeletonStep");
						DelayedAction.playSoundAfterDelay("skeletonHit", 150);
					}
				}
				else if (name.Equals("Keg"))
				{
					switch ((int)dropIn.parentSheetIndex)
					{
					case 262:
						heldObject.Value = new Object(Vector2.Zero, 346, "Beer", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Beer";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 1750;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 304:
						heldObject.Value = new Object(Vector2.Zero, 303, "Pale Ale", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Pale Ale";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 2250;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 815:
						heldObject.Value = new Object(Vector2.Zero, 614, "Green Tea", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Green Tea";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 180;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 433:
						if (dropIn.Stack < 5 && !probe)
						{
							if (autoLoadChest == null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12721"));
							}
							return false;
						}
						heldObject.Value = new Object(Vector2.Zero, 395, "Coffee", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Coffee";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							ConsumeInventoryItem(who, dropIn, 4);
							minutesUntilReady.Value = 120;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.DarkGray * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 340:
						heldObject.Value = new Object(Vector2.Zero, 459, "Mead", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							heldObject.Value.name = "Mead";
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 600;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					switch (dropIn.Category)
					{
					case -75:
						heldObject.Value = new Object(Vector2.Zero, 350, dropIn.Name + " Juice", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = (int)((double)dropIn.Price * 2.25);
						if (!probe)
						{
							heldObject.Value.name = dropIn.Name + " Juice";
							heldObject.Value.preserve.Value = PreserveType.Juice;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 6000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case -79:
						heldObject.Value = new Object(Vector2.Zero, 348, dropIn.Name + " Wine", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = dropIn.Price * 3;
						if (!probe)
						{
							heldObject.Value.name = dropIn.Name + " Wine";
							heldObject.Value.preserve.Value = PreserveType.Wine;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							who.currentLocation.playSound("bubbles");
							minutesUntilReady.Value = 10000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lavender * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
				}
				else if (name.Equals("Preserves Jar"))
				{
					switch (dropIn.Category)
					{
					case -75:
						heldObject.Value = new Object(Vector2.Zero, 342, "Pickled " + dropIn.Name, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + dropIn.Price * 2;
						if (!probe)
						{
							heldObject.Value.name = "Pickled " + dropIn.Name;
							heldObject.Value.preserve.Value = PreserveType.Pickle;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 4000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case -79:
						heldObject.Value = new Object(Vector2.Zero, 344, dropIn.Name + " Jelly", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + dropIn.Price * 2;
						if (!probe)
						{
							minutesUntilReady.Value = 4000;
							heldObject.Value.name = dropIn.Name + " Jelly";
							heldObject.Value.preserve.Value = PreserveType.Jelly;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					switch ((int)dropIn.parentSheetIndex)
					{
					case 829:
						heldObject.Value = new Object(Vector2.Zero, 342, "Pickled " + dropIn.Name, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						heldObject.Value.Price = 50 + dropIn.Price * 2;
						if (!probe)
						{
							heldObject.Value.name = "Pickled " + dropIn.Name;
							heldObject.Value.preserve.Value = PreserveType.Pickle;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.parentSheetIndex;
							who.currentLocation.playSound("Ship");
							minutesUntilReady.Value = 4000;
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.White * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 812:
					{
						if ((int)dropIn.preservedParentSheetIndex == 698)
						{
							heldObject.Value = new Object(445, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 6000;
								who.currentLocation.playSound("Ship");
								Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
								{
									alphaFade = 0.005f
								});
							}
							return true;
						}
						Object aged_roe2 = null;
						ColoredObject colored_object;
						aged_roe2 = (((colored_object = (dropIn as ColoredObject)) == null) ? new Object(447, 1) : new ColoredObject(447, 1, colored_object.color));
						heldObject.Value = aged_roe2;
						heldObject.Value.Price = dropIn.Price * 2;
						if (!probe)
						{
							minutesUntilReady.Value = 4000;
							heldObject.Value.name = "Aged " + dropIn.Name;
							heldObject.Value.preserve.Value = PreserveType.AgedRoe;
							heldObject.Value.Category = -26;
							heldObject.Value.preservedParentSheetIndex.Value = dropIn.preservedParentSheetIndex;
							who.currentLocation.playSound("Ship");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.LightBlue * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
					}
				}
				else if (name.Equals("Cheese Press"))
				{
					int stack2 = 1;
					switch (dropIn.ParentSheetIndex)
					{
					case 436:
						heldObject.Value = new Object(Vector2.Zero, 426, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = stack2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 438:
						heldObject.Value = new Object(Vector2.Zero, 426, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2,
							Stack = stack2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 184:
						heldObject.Value = new Object(Vector2.Zero, 424, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = stack2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 186:
						heldObject.Value = new Object(Vector2.Zero, 424, "Cheese (=)", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2,
							Stack = stack2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 200;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Mayonnaise Machine"))
				{
					switch (dropIn.ParentSheetIndex)
					{
					case 289:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
							heldObject.Value.Stack = 10;
							heldObject.Value.Quality = dropIn.Quality;
						}
						return true;
					case 174:
					case 182:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 176:
					case 180:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 442:
						heldObject.Value = new Object(Vector2.Zero, 307, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 305:
						heldObject.Value = new Object(Vector2.Zero, 308, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 107:
						heldObject.Value = new Object(Vector2.Zero, 807, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							who.currentLocation.playSound("Ship");
						}
						return true;
					case 928:
						heldObject.Value = new Object(Vector2.Zero, 306, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Quality = 2
						};
						if (!probe)
						{
							minutesUntilReady.Value = 180;
							heldObject.Value.Stack = 3;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Loom"))
				{
					float doubleChance = ((int)dropIn.quality == 0) ? 0f : (((int)dropIn.quality == 2) ? 0.25f : (((int)dropIn.quality == 4) ? 0.5f : 0.1f));
					int stack = (!(Game1.random.NextDouble() <= (double)doubleChance)) ? 1 : 2;
					int parentSheetIndex = dropIn.ParentSheetIndex;
					if (parentSheetIndex == 440)
					{
						heldObject.Value = new Object(Vector2.Zero, 428, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
						{
							Stack = stack
						};
						if (!probe)
						{
							minutesUntilReady.Value = 240;
							who.currentLocation.playSound("Ship");
						}
						return true;
					}
				}
				else if (name.Equals("Oil Maker"))
				{
					switch (dropIn.ParentSheetIndex)
					{
					case 270:
						heldObject.Value = new Object(Vector2.Zero, 247, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 1000;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 421:
						heldObject.Value = new Object(Vector2.Zero, 247, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 60;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 430:
						heldObject.Value = new Object(Vector2.Zero, 432, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						if (!probe)
						{
							minutesUntilReady.Value = 360;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					case 431:
						heldObject.Value = new Object(247, 1);
						if (!probe)
						{
							minutesUntilReady.Value = 3200;
							who.currentLocation.playSound("bubbles");
							who.currentLocation.playSound("sipTea");
							Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow * 0.75f, 1f, 0f, 0f, 0f)
							{
								alphaFade = 0.005f
							});
						}
						return true;
					}
				}
				else if (name.Equals("Seed Maker"))
				{
					if (dropIn != null && (int)dropIn.parentSheetIndex == 433)
					{
						return false;
					}
					if (dropIn != null && (int)dropIn.parentSheetIndex == 771)
					{
						return false;
					}
					Dictionary<int, string> dictionary = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Crops");
					bool found = false;
					int seed = -1;
					foreach (KeyValuePair<int, string> v in dictionary)
					{
						if (Convert.ToInt32(v.Value.Split('/')[3]) == dropIn.ParentSheetIndex)
						{
							found = true;
							seed = v.Key;
							break;
						}
					}
					if (found)
					{
						Random r2 = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)tileLocation.X + (int)tileLocation.Y * 77 + Game1.timeOfDay);
						heldObject.Value = new Object(seed, r2.Next(1, 4));
						if (!probe)
						{
							if (r2.NextDouble() < 0.005)
							{
								heldObject.Value = new Object(499, 1);
							}
							else if (r2.NextDouble() < 0.02)
							{
								heldObject.Value = new Object(770, r2.Next(1, 5));
							}
							minutesUntilReady.Value = 20;
							who.currentLocation.playSound("Ship");
							DelayedAction.playSoundAfterDelay("dirtyHit", 250);
						}
						return true;
					}
				}
				else if (name.Equals("Crystalarium"))
				{
					if ((dropIn.Category == -2 || dropIn.Category == -12) && dropIn.ParentSheetIndex != 74 && (heldObject.Value == null || heldObject.Value.ParentSheetIndex != dropIn.ParentSheetIndex) && (heldObject.Value == null || (int)minutesUntilReady > 0))
					{
						heldObject.Value = (Object)dropIn.getOne();
						if (!probe)
						{
							who.currentLocation.playSound("select");
							minutesUntilReady.Value = getMinutesForCrystalarium(dropIn.ParentSheetIndex);
						}
						return true;
					}
				}
				else if (name.Equals("Recycling Machine"))
				{
					if (dropIn.ParentSheetIndex >= 168 && dropIn.ParentSheetIndex <= 172 && heldObject.Value == null)
					{
						Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay + (int)tileLocation.X * 200 + (int)tileLocation.Y);
						switch (dropIn.ParentSheetIndex)
						{
						case 168:
							heldObject.Value = new Object((r.NextDouble() < 0.3) ? 382 : ((r.NextDouble() < 0.3) ? 380 : 390), r.Next(1, 4));
							break;
						case 169:
							heldObject.Value = new Object((r.NextDouble() < 0.25) ? 382 : 388, r.Next(1, 4));
							break;
						case 170:
							heldObject.Value = new Object(338, 1);
							break;
						case 171:
							heldObject.Value = new Object(338, 1);
							break;
						case 172:
							heldObject.Value = ((r.NextDouble() < 0.1) ? new Object(428, 1) : new Torch(Vector2.Zero, 3));
							break;
						}
						if (!probe)
						{
							who.currentLocation.playSound("trashcan");
							minutesUntilReady.Value = 60;
							Game1.stats.PiecesOfTrashRecycled++;
						}
						return true;
					}
				}
				else if (name.Equals("Furnace"))
				{
					if (who.IsLocalPlayer && GetTallyOfObject(who, 382, big_craftable: false) <= 0)
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12772"));
						}
						return false;
					}
					if (heldObject.Value == null)
					{
						if ((int)dropIn.stack < 5 && (int)dropIn.parentSheetIndex != 80 && (int)dropIn.parentSheetIndex != 82 && (int)dropIn.parentSheetIndex != 330 && (int)dropIn.parentSheetIndex != 458)
						{
							if (!probe && who.IsLocalPlayer && autoLoadChest == null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12777"));
							}
							return false;
						}
						int toRemove = 5;
						switch (dropIn.ParentSheetIndex)
						{
						case 378:
							heldObject.Value = new Object(Vector2.Zero, 334, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 30;
							}
							break;
						case 380:
							heldObject.Value = new Object(Vector2.Zero, 335, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 120;
							}
							break;
						case 384:
							heldObject.Value = new Object(Vector2.Zero, 336, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 300;
							}
							break;
						case 386:
							heldObject.Value = new Object(Vector2.Zero, 337, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 480;
							}
							break;
						case 80:
							heldObject.Value = new Object(Vector2.Zero, 338, "Refined Quartz", canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
							if (!probe)
							{
								minutesUntilReady.Value = 90;
								toRemove = 1;
							}
							break;
						case 82:
							heldObject.Value = new Object(338, 3);
							if (!probe)
							{
								minutesUntilReady.Value = 90;
								toRemove = 1;
							}
							break;
						case 458:
							heldObject.Value = new Object(277, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 10;
								toRemove = 1;
							}
							break;
						case 909:
							heldObject.Value = new Object(910, 1);
							if (!probe)
							{
								minutesUntilReady.Value = 560;
							}
							break;
						default:
							return false;
						}
						if (probe)
						{
							return true;
						}
						who.currentLocation.playSound("furnace");
						initializeLightSource(tileLocation);
						showNextIndex.Value = true;
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(30, tileLocation.Value * 64f + new Vector2(0f, -16f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
						{
							alphaFade = 0.005f
						});
						ConsumeInventoryItem(who, 382, 1);
						dropIn.Stack -= toRemove;
						if (dropIn.Stack <= 0)
						{
							return true;
						}
						return false;
					}
					if (probe)
					{
						return true;
					}
				}
				else if (name.Equals("Geode Crusher"))
				{
					if (who.IsLocalPlayer && GetTallyOfObject(who, 382, big_craftable: false) <= 0)
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12772"));
						}
						return false;
					}
					if (heldObject.Value == null)
					{
						if (!Utility.IsGeode(dropIn, disallow_special_geodes: true))
						{
							return false;
						}
						Object geode_item = (Object)Utility.getTreasureFromGeode(dropIn);
						if (geode_item == null)
						{
							return false;
						}
						heldObject.Value = geode_item;
						if (!probe)
						{
							Game1.stats.GeodesCracked++;
							minutesUntilReady.Value = 60;
						}
						if (probe)
						{
							return true;
						}
						showNextIndex.Value = true;
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(4f, -48f), 200);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(-16f, -56f), 300);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(16f, -52f), 400);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(32f, -56f), 200);
						Utility.addSmokePuff(who.currentLocation, tileLocation.Value * 64f + new Vector2(40f, -44f), 500);
						Game1.playSound("drumkit4");
						Game1.playSound("stoneCrack");
						DelayedAction.playSoundAfterDelay("steam", 200);
						ConsumeInventoryItem(who, 382, 1);
						dropIn.Stack--;
						if (dropIn.Stack <= 0)
						{
							return true;
						}
					}
					else if (probe)
					{
						return true;
					}
				}
				else if (name.Equals("Charcoal Kiln"))
				{
					if (who.IsLocalPlayer && ((int)dropIn.parentSheetIndex != 388 || dropIn.Stack < 10))
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12783"));
						}
						return false;
					}
					if (heldObject.Value == null && !probe && (int)dropIn.parentSheetIndex == 388 && dropIn.Stack >= 10)
					{
						ConsumeInventoryItem(who, dropIn, 10);
						who.currentLocation.playSound("openBox");
						DelayedAction.playSoundAfterDelay("fireball", 50);
						showNextIndex.Value = true;
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(27, tileLocation.Value * 64f + new Vector2(-16f, -128f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
						{
							alphaFade = 0.005f
						});
						heldObject.Value = new Object(382, 1);
						minutesUntilReady.Value = 30;
					}
					else if (heldObject.Value == null && probe && (int)dropIn.parentSheetIndex == 388 && dropIn.Stack >= 10)
					{
						heldObject.Value = new Object();
						return true;
					}
				}
				else if (name.Equals("Slime Egg-Press"))
				{
					if (who.IsLocalPlayer && ((int)dropIn.parentSheetIndex != 766 || dropIn.Stack < 100))
					{
						if (!probe && who.IsLocalPlayer && autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12787"));
						}
						return false;
					}
					if (heldObject.Value == null && !probe && (int)dropIn.parentSheetIndex == 766 && dropIn.Stack >= 100)
					{
						ConsumeInventoryItem(who, dropIn, 100);
						who.currentLocation.playSound("slimeHit");
						DelayedAction.playSoundAfterDelay("bubbles", 50);
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -160f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f)
						{
							alphaFade = 0.005f
						});
						int slime = 680;
						if (Game1.random.NextDouble() < 0.05)
						{
							slime = 439;
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							slime = 437;
						}
						else if (Game1.random.NextDouble() < 0.25)
						{
							slime = 413;
						}
						heldObject.Value = new Object(slime, 1);
						minutesUntilReady.Value = 1200;
					}
					else if (heldObject.Value == null && probe && (int)dropIn.parentSheetIndex == 766 && dropIn.Stack >= 100)
					{
						heldObject.Value = new Object();
						return true;
					}
				}
				else if (name.Contains("Feed Hopper") && dropIn.ParentSheetIndex == 178)
				{
					if (probe)
					{
						heldObject.Value = new Object();
						return true;
					}
					if (Utility.numSilos() <= 0)
					{
						if (autoLoadChest == null)
						{
							Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:NeedSilo"));
						}
						return false;
					}
					who.currentLocation.playSound("Ship");
					DelayedAction.playSoundAfterDelay("grassyStep", 100);
					if (dropIn.Stack == 0)
					{
						dropIn.Stack = 1;
					}
					int num = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
					int numLeft = (Game1.getLocationFromName("Farm") as Farm).tryToAddHay(dropIn.Stack);
					int now = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
					if (num <= 0 && now > 0)
					{
						showNextIndex.Value = true;
					}
					else if (now <= 0)
					{
						showNextIndex.Value = false;
					}
					dropIn.Stack = numLeft;
					if (numLeft <= 0)
					{
						return true;
					}
				}
				if (name.Contains("Table") && heldObject.Value == null && !dropIn.bigCraftable && !dropIn.Name.Contains("Table"))
				{
					heldObject.Value = (Object)dropIn.getOne();
					if (!probe)
					{
						who.currentLocation.playSound("woodyStep");
					}
					return true;
				}
				_ = heldObject.Value;
				return false;
			}
			return false;
		}

		public virtual void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			Object heldObject = this.heldObject.Get();
			if ((bool)readyForHarvest && heldObject == null)
			{
				readyForHarvest.Value = false;
			}
			LightSource lightSource = netLightSource.Get();
			if (lightSource != null && (bool)isOn && !environment.hasLightSource(lightSource.Identifier))
			{
				environment.sharedLights[lightSource.identifier] = lightSource.Clone();
			}
			if (heldObject != null)
			{
				if (heldObject.ParentSheetIndex == 913 && IsSprinkler() && heldObject.heldObject.Value is Chest)
				{
					Chest chest = heldObject.heldObject.Value as Chest;
					chest.mutex.Update(environment);
					if (Game1.activeClickableMenu == null && chest.GetMutex().IsLockHeld())
					{
						chest.GetMutex().ReleaseLock();
					}
				}
				lightSource = heldObject.netLightSource.Get();
				if (lightSource != null && !environment.hasLightSource(lightSource.Identifier))
				{
					environment.sharedLights[lightSource.identifier] = lightSource.Clone();
				}
			}
			if (shakeTimer > 0)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer <= 0)
				{
					health = 10;
				}
			}
			if (parentSheetIndex.Get() == 590 && Game1.random.NextDouble() < 0.01)
			{
				shakeTimer = 100;
			}
			if (bigCraftable.Get() && name.Equals("Slime Ball", StringComparison.Ordinal))
			{
				base.ParentSheetIndex = 56 + (int)(time.TotalGameTime.TotalMilliseconds % 600.0 / 100.0);
			}
		}

		public virtual void actionOnPlayerEntry()
		{
			isTemporarilyInvisible = false;
			health = 10;
			if (name != null && name.Contains("Feed Hopper"))
			{
				showNextIndex.Value = ((int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay > 0);
			}
		}

		public override bool canBeTrashed()
		{
			if ((bool)questItem)
			{
				return false;
			}
			if (!bigCraftable && (int)parentSheetIndex == 460)
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			return base.canBeTrashed();
		}

		public virtual bool isForage(GameLocation location)
		{
			if (base.Category != -79 && base.Category != -81 && base.Category != -80 && base.Category != -75 && !(location is Beach) && (int)parentSheetIndex != 430)
			{
				return base.Category == -23;
			}
			return true;
		}

		public virtual void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
		{
			if (name == null)
			{
				return;
			}
			int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
			if (this is Furniture && (int)(this as Furniture).furniture_type == 14 && (bool)(this as Furniture).isOn)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			}
			else if (this is Furniture && (int)(this as Furniture).furniture_type == 16 && (bool)(this as Furniture).isOn)
			{
				lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 1.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
			}
			else if ((bool)bigCraftable)
			{
				if (this is Torch && (bool)isOn)
				{
					float y_offset = -64f;
					if (Name.Contains("Campfire"))
					{
						y_offset = 32f;
					}
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + y_offset), 2.5f, new Color(0, 80, 160), identifier, LightSource.LightContext.None, 0L);
				}
				else if ((bool)isLamp)
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - 64f), 3f, new Color(0, 40, 80), identifier, LightSource.LightContext.None, 0L);
				}
				else if ((name.Equals("Furnace") && (int)minutesUntilReady > 0) || name.Equals("Bonfire"))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1.5f, Color.DarkCyan, identifier, LightSource.LightContext.None, 0L);
				}
				else if (name.Equals("Strange Capsule"))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f), 1f, Color.HotPink * 0.75f, identifier, LightSource.LightContext.None, 0L);
				}
			}
			else
			{
				if (!Utility.IsNormalObjectAtParentSheetIndex(this, parentSheetIndex) && !(this is Torch))
				{
					return;
				}
				if ((int)parentSheetIndex == 93 || (int)parentSheetIndex == 94 || (int)parentSheetIndex == 95)
				{
					Color c = Color.White;
					switch ((int)parentSheetIndex)
					{
					case 93:
						c = new Color(1, 1, 1) * 0.9f;
						break;
					case 94:
						c = Color.Yellow;
						break;
					case 95:
						c = new Color(70, 0, 150) * 0.9f;
						break;
					}
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 16f, tileLocation.Y * 64f + 16f), mineShaft ? 1.5f : 1.25f, c, identifier, LightSource.LightContext.None, 0L);
				}
				else if (Utility.IsNormalObjectAtParentSheetIndex(this, 746))
				{
					lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 48f), 0.5f, new Color(1, 1, 1) * 0.65f, identifier, LightSource.LightContext.None, 0L);
				}
			}
		}

		public virtual void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			if (lightSource != null)
			{
				environment.removeLightSource(lightSource.identifier);
				environment.removeLightSource((int)Game1.player.UniqueMultiplayerID);
			}
			if ((bool)bigCraftable)
			{
				if ((base.ParentSheetIndex == 105 || (int)parentSheetIndex == 264) && environment != null && environment.terrainFeatures != null && environment.terrainFeatures.ContainsKey(tileLocation) && environment.terrainFeatures[tileLocation] is Tree)
				{
					(environment.terrainFeatures[tileLocation] as Tree).tapped.Value = false;
				}
				if ((int)parentSheetIndex == 126 && (int)quality != 0)
				{
					Game1.createItemDebris(new Hat((int)quality - 1), tileLocation * 64f, (Game1.player.FacingDirection + 2) % 4);
				}
				quality.Value = 0;
			}
			if (name != null && name.Contains("Sprinkler"))
			{
				environment.removeTemporarySpritesWithID((int)tileLocation.X * 4000 + (int)tileLocation.Y);
			}
			if (name.Contains("Seasonal") && bigCraftable.Value)
			{
				base.ParentSheetIndex -= base.ParentSheetIndex % 4;
			}
		}

		public virtual void dropItem(GameLocation location, Vector2 origin, Vector2 destination)
		{
			if ((type.Equals("Crafting") || Type.Equals("interactive")) && (int)fragility != 2)
			{
				location.debris.Add(new Debris(bigCraftable ? (-base.ParentSheetIndex) : base.ParentSheetIndex, origin, destination));
			}
		}

		public virtual bool isPassable()
		{
			if (isTemporarilyInvisible)
			{
				return true;
			}
			if ((bool)bigCraftable)
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, parentSheetIndex))
			{
				switch ((int)parentSheetIndex)
				{
				case 93:
				case 286:
				case 287:
				case 288:
				case 293:
				case 297:
				case 328:
				case 329:
				case 331:
				case 333:
				case 401:
				case 405:
				case 407:
				case 409:
				case 411:
				case 415:
				case 590:
				case 840:
				case 841:
					return true;
				}
			}
			if (base.Category == -74 || base.Category == -19)
			{
				if (isSapling())
				{
					return false;
				}
				int num = parentSheetIndex;
				if ((uint)(num - 301) <= 1u || num == 473)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		public virtual void reloadSprite()
		{
			initializeLightSource(tileLocation);
		}

		public virtual void consumeRecipe(Farmer who)
		{
			if ((bool)isRecipe)
			{
				if (base.Category == -7)
				{
					who.cookingRecipes.Add(name, 0);
				}
				else
				{
					who.craftingRecipes.Add(name, 0);
				}
			}
		}

		public virtual Microsoft.Xna.Framework.Rectangle getBoundingBox(Vector2 tileLocation)
		{
			Microsoft.Xna.Framework.Rectangle boundingBox = this.boundingBox.Value;
			Microsoft.Xna.Framework.Rectangle newBounds = boundingBox;
			if ((this is Torch && !bigCraftable) || (int)parentSheetIndex == 590)
			{
				newBounds.X = (int)tileLocation.X * 64 + 24;
				newBounds.Y = (int)tileLocation.Y * 64 + 24;
			}
			else
			{
				newBounds.X = (int)tileLocation.X * 64;
				newBounds.Y = (int)tileLocation.Y * 64;
			}
			if (newBounds != boundingBox)
			{
				this.boundingBox.Set(boundingBox);
			}
			return newBounds;
		}

		public override bool canBeGivenAsGift()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			if (!bigCraftable && !(this is Furniture))
			{
				return !(this is Wallpaper);
			}
			return false;
		}

		public virtual bool performDropDownAction(Farmer who)
		{
			if (who == null)
			{
				who = Game1.getFarmer(owner);
			}
			if (name.Equals("Worm Bin"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(685, Game1.random.Next(2, 6));
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay);
				}
				return false;
			}
			if (name.Equals("Bee House"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
				}
				return false;
			}
			if (name.Equals("Solar Panel"))
			{
				if (heldObject.Value == null)
				{
					heldObject.Value = new Object(Vector2.Zero, 787, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = 16800;
				}
				return false;
			}
			if (name.Contains("Strange Capsule"))
			{
				minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 3);
			}
			else if (name.Contains("Feed Hopper"))
			{
				showNextIndex.Value = false;
				if ((int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay >= 0)
				{
					showNextIndex.Value = true;
				}
			}
			return false;
		}

		private void totemWarp(Farmer who)
		{
			for (int j = 0; j < 12; j++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
			}
			who.currentLocation.playSound("wand");
			Game1.displayFarmer = false;
			Game1.player.temporarilyInvincible = true;
			Game1.player.temporaryInvincibilityTimer = -2000;
			Game1.player.freezePause = 1000;
			Game1.flashAlpha = 1f;
			DelayedAction.fadeAfterDelay(totemWarpForReal, 1000);
			new Microsoft.Xna.Framework.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
			int i = 0;
			for (int x = who.getTileX() + 8; x >= who.getTileX() - 8; x--)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(6, new Vector2(x, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = i * 25,
					motion = new Vector2(-0.25f, 0f)
				});
				i++;
			}
		}

		private void totemWarpForReal()
		{
			switch ((int)parentSheetIndex)
			{
			case 688:
			{
				int warp_location_x = 48;
				int warp_location_y = 7;
				if (Game1.whichFarm == 5)
				{
					warp_location_x = 48;
					warp_location_y = 39;
				}
				else if (Game1.whichFarm == 6)
				{
					warp_location_x = 82;
					warp_location_y = 29;
				}
				Point warp_location = Game1.getFarm().GetMapPropertyPosition("WarpTotemEntry", warp_location_x, warp_location_y);
				Game1.warpFarmer("Farm", warp_location.X, warp_location.Y, flip: false);
				break;
			}
			case 689:
				Game1.warpFarmer("Mountain", 31, 20, flip: false);
				break;
			case 690:
				Game1.warpFarmer("Beach", 20, 4, flip: false);
				break;
			case 261:
				Game1.warpFarmer("Desert", 35, 43, flip: false);
				break;
			case 886:
				Game1.warpFarmer("IslandSouth", 11, 11, flip: false);
				break;
			}
			Game1.fadeToBlackAlpha = 0.99f;
			Game1.screenGlow = false;
			Game1.player.temporarilyInvincible = false;
			Game1.player.temporaryInvincibilityTimer = 0;
			Game1.displayFarmer = true;
		}

		public void MonsterMusk(Farmer who)
		{
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.StopAnimation();
			who.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
			{
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(104, 350, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(105, 350, secondaryArm: false, flip: false)
			});
			who.currentLocation.playSound("croak");
			Game1.buffsDisplay.addOtherBuff(new Buff(24));
		}

		public override string[] ModifyItemBuffs(string[] buffs)
		{
			if (buffs != null && base.Category == -7)
			{
				int buff_bonus = 0;
				if (Quality != 0)
				{
					buff_bonus = 1;
				}
				if (buff_bonus > 0)
				{
					int value = 0;
					for (int i = 0; i < buffs.Length; i++)
					{
						if (i != 9 && buffs[i] != "0" && int.TryParse(buffs[i], out value))
						{
							value += buff_bonus;
							buffs[i] = value.ToString();
						}
					}
				}
			}
			return base.ModifyItemBuffs(buffs);
		}

		private void rainTotem(Farmer who)
		{
			GameLocation.LocationContext location_context = Game1.currentLocation.GetLocationContext();
			if (location_context == GameLocation.LocationContext.Default)
			{
				if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
				{
					Game1.netWorldState.Value.WeatherForTomorrow = (Game1.weatherForTomorrow = 1);
					Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
				}
			}
			else
			{
				Game1.netWorldState.Value.GetWeatherForLocation(location_context).weatherForTomorrow.Value = 1;
				Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
			}
			Game1.screenGlow = false;
			who.currentLocation.playSound("thunder");
			who.canMove = false;
			Game1.screenGlowOnce(Color.SlateBlue, hold: false);
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[1]
			{
				new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
			});
			for (int i = 0; i < 6; i++)
			{
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
					delayBeforeAnimationStart = i * 200
				});
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
					delayBeforeAnimationStart = 100 + i * 200
				});
				Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 1045, 52, 33), 9999f, 1, 999, who.Position + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
				{
					motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
					delayBeforeAnimationStart = 200 + i * 200
				});
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
			{
				motion = new Vector2(0f, -7f),
				acceleration = new Vector2(0f, 0.1f),
				scaleChange = 0.015f,
				alpha = 1f,
				alphaFade = 0.0075f,
				shakeIntensity = 1f,
				initialPosition = Game1.player.Position + new Vector2(0f, -96f),
				xPeriodic = true,
				xPeriodicLoopTime = 1000f,
				xPeriodicRange = 4f,
				layerDepth = 1f
			});
			DelayedAction.playSoundAfterDelay("rainsound", 2000);
		}

		public virtual bool performUseAction(GameLocation location)
		{
			if (!Game1.player.canMove || isTemporarilyInvisible)
			{
				return false;
			}
			bool normal_gameplay = !Game1.eventUp && !Game1.isFestival() && !Game1.fadeToBlack && !Game1.player.swimming && !Game1.player.bathingClothes && !Game1.player.onBridge.Value;
			if (name != null && name.Contains("Totem"))
			{
				if (normal_gameplay)
				{
					switch ((int)parentSheetIndex)
					{
					case 681:
						rainTotem(Game1.player);
						return true;
					case 261:
					case 688:
					case 689:
					case 690:
					case 886:
					{
						Game1.player.jitterStrength = 1f;
						Color sprinkleColor = ((int)parentSheetIndex == 681) ? Color.SlateBlue : (((int)parentSheetIndex == 688) ? Color.LimeGreen : (((int)parentSheetIndex == 689) ? Color.OrangeRed : (((int)parentSheetIndex == 261) ? new Color(255, 200, 0) : Color.LightBlue)));
						location.playSound("warrior");
						Game1.player.faceDirection(2);
						Game1.player.CanMove = false;
						Game1.player.temporarilyInvincible = true;
						Game1.player.temporaryInvincibilityTimer = -4000;
						Game1.changeMusicTrack("none");
						if ((int)parentSheetIndex == 681)
						{
							Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
							{
								new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
								new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, rainTotem, behaviorAtEndOfFrame: true)
							});
						}
						else
						{
							Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
							{
								new FarmerSprite.AnimationFrame(57, 2000, secondaryArm: false, flip: false),
								new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, totemWarp, behaviorAtEndOfFrame: true)
							});
						}
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(0f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -1f),
							scaleChange = 0.01f,
							alpha = 1f,
							alphaFade = 0.0075f,
							shakeIntensity = 1f,
							initialPosition = Game1.player.Position + new Vector2(0f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 1f
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(-64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -0.5f),
							scaleChange = 0.005f,
							scale = 0.5f,
							alpha = 1f,
							alphaFade = 0.0075f,
							shakeIntensity = 1f,
							delayBeforeAnimationStart = 10,
							initialPosition = Game1.player.Position + new Vector2(-64f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 0.9999f
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 9999f, 1, 999, Game1.player.Position + new Vector2(64f, -96f), flicker: false, flipped: false, verticalFlipped: false, 0f)
						{
							motion = new Vector2(0f, -0.5f),
							scaleChange = 0.005f,
							scale = 0.5f,
							alpha = 1f,
							alphaFade = 0.0075f,
							delayBeforeAnimationStart = 20,
							shakeIntensity = 1f,
							initialPosition = Game1.player.Position + new Vector2(64f, -96f),
							xPeriodic = true,
							xPeriodicLoopTime = 1000f,
							xPeriodicRange = 4f,
							layerDepth = 0.9988f
						});
						Game1.screenGlowOnce(sprinkleColor, hold: false);
						Utility.addSprinklesToLocation(location, Game1.player.getTileX(), Game1.player.getTileY(), 16, 16, 1300, 20, Color.White, null, motionTowardCenter: true);
						return true;
					}
					}
				}
			}
			else
			{
				if (name != null && name.Contains("Secret Note"))
				{
					int which3 = (!name.Contains('#')) ? 1 : Convert.ToInt32(name.Split('#')[1]);
					if (!Game1.player.secretNotesSeen.Contains(which3))
					{
						Game1.player.secretNotesSeen.Add(which3);
						if (which3 == 23 && !Game1.player.eventsSeen.Contains(2120303))
						{
							Game1.player.addQuest(29);
						}
						else if (which3 == 10 && !Game1.player.mailReceived.Contains("qiCave"))
						{
							Game1.player.addQuest(30);
						}
					}
					Game1.activeClickableMenu = new LetterViewerMenu(which3);
					return true;
				}
				if (name != null && name.Contains("Journal Scrap"))
				{
					int which2 = (!name.Contains('#')) ? 1 : Convert.ToInt32(name.Split('#')[1]);
					which2 += GameLocation.JOURNAL_INDEX;
					if (!Game1.player.secretNotesSeen.Contains(which2))
					{
						Game1.player.secretNotesSeen.Add(which2);
					}
					Game1.activeClickableMenu = new LetterViewerMenu(which2);
					return true;
				}
			}
			if (base.ParentSheetIndex == 911)
			{
				if (!normal_gameplay)
				{
					return false;
				}
				switch (Utility.GetHorseWarpRestrictionsForFarmer(Game1.player))
				{
				case 1:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse"));
					break;
				case 2:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation"));
					break;
				case 3:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance"));
					break;
				case 4:
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InUse"));
					break;
				case 0:
				{
					Horse horse = null;
					foreach (NPC character in Game1.player.currentLocation.characters)
					{
						if (character is Horse)
						{
							Horse location_horse = character as Horse;
							if (location_horse.getOwner() == Game1.player)
							{
								horse = location_horse;
								break;
							}
						}
					}
					if (horse == null || Math.Abs(Game1.player.getTileX() - horse.getTileX()) > 1 || Math.Abs(Game1.player.getTileY() - horse.getTileY()) > 1)
					{
						Game1.player.faceDirection(2);
						Game1.soundBank.PlayCue("horse_flute");
						Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[6]
						{
							new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(100, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(98, 400, secondaryArm: true, flip: false),
							new FarmerSprite.AnimationFrame(99, 200, secondaryArm: true, flip: false)
						});
						Game1.player.freezePause = 1500;
						DelayedAction.functionAfterDelay(delegate
						{
							switch (Utility.GetHorseWarpRestrictionsForFarmer(Game1.player))
							{
							case 0:
								Game1.player.team.requestHorseWarpEvent.Fire(Game1.player.UniqueMultiplayerID);
								break;
							case 1:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse"));
								break;
							case 2:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation"));
								break;
							case 3:
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance"));
								break;
							}
						}, 1500);
					}
					stack.Value += 1;
					return true;
				}
				default:
					stack.Value += 1;
					return true;
				}
			}
			if (base.ParentSheetIndex == 879)
			{
				if (!normal_gameplay)
				{
					return false;
				}
				Game1.player.faceDirection(2);
				Game1.player.freezePause = 1750;
				Game1.player.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[2]
				{
					new FarmerSprite.AnimationFrame(57, 750, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame((short)Game1.player.FarmerSprite.CurrentFrame, 0, secondaryArm: false, flip: false, MonsterMusk, behaviorAtEndOfFrame: true)
				});
				for (int i = 0; i < 3; i++)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(16f, -64 + 32 * i), Color.Purple)
					{
						motion = new Vector2(Utility.RandomFloat(-1f, 1f), -0.5f),
						scaleChange = 0.005f,
						scale = 0.5f,
						alpha = 1f,
						alphaFade = 0.0075f,
						shakeIntensity = 1f,
						delayBeforeAnimationStart = 100 * i,
						layerDepth = 0.9999f,
						positionFollowsAttachedCharacter = true,
						attachedCharacter = Game1.player
					});
				}
				location.playSound("steam");
				return true;
			}
			_ = name;
			return false;
		}

		public override Color getCategoryColor()
		{
			if (this is Furniture)
			{
				return new Color(100, 25, 190);
			}
			if (type != null && type.Equals("Arch"))
			{
				return new Color(110, 0, 90);
			}
			switch (base.Category)
			{
			case -12:
			case -2:
				return new Color(110, 0, 90);
			case -75:
				return Color.Green;
			case -4:
				return Color.DarkBlue;
			case -7:
				return new Color(220, 60, 0);
			case -79:
				return Color.DeepPink;
			case -74:
				return Color.Brown;
			case -19:
				return Color.SlateGray;
			case -21:
				return Color.DarkRed;
			case -22:
				return Color.DarkCyan;
			case -24:
				return Color.Plum;
			case -20:
				return Color.DarkGray;
			case -27:
			case -26:
				return new Color(0, 155, 111);
			case -8:
				return new Color(148, 61, 40);
			case -18:
			case -14:
			case -6:
			case -5:
				return new Color(255, 0, 100);
			case -80:
				return new Color(219, 54, 211);
			case -28:
				return new Color(50, 10, 70);
			case -16:
			case -15:
				return new Color(64, 102, 114);
			case -81:
				return new Color(10, 130, 50);
			default:
				return Color.Black;
			}
		}

		public override string getCategoryName()
		{
			if (this is Furniture)
			{
				if ((this as Furniture).placementRestriction == 1)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Outdoors");
				}
				if ((this as Furniture).placementRestriction == 2)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture_Decoration");
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12847");
			}
			if (type.Value != null && type.Value.Equals("Arch"))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12849");
			}
			switch (base.Category)
			{
			case -12:
			case -2:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12850");
			case -75:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12851");
			case -4:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12852");
			case -25:
			case -7:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12853");
			case -79:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12854");
			case -74:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12855");
			case -19:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12856");
			case -21:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12857");
			case -22:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12858");
			case -24:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12859");
			case -20:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12860");
			case -27:
			case -26:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12862");
			case -8:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12863");
			case -18:
			case -14:
			case -6:
			case -5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12864");
			case -80:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12866");
			case -28:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12867");
			case -16:
			case -15:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12868");
			case -81:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12869");
			default:
				return "";
			}
		}

		public virtual bool isActionable(Farmer who)
		{
			if (!isTemporarilyInvisible)
			{
				return checkForAction(who, justCheckingForActivity: true);
			}
			return false;
		}

		public int getHealth()
		{
			return health;
		}

		public void setHealth(int health)
		{
			this.health = health;
		}

		protected virtual void grabItemFromAutoGrabber(Item item, Farmer who)
		{
			if (heldObject.Value != null && heldObject.Value is Chest)
			{
				if (who.couldInventoryAcceptThisItem(item))
				{
					(heldObject.Value as Chest).items.Remove(item);
					(heldObject.Value as Chest).clearNulls();
					Game1.activeClickableMenu = new ItemGrabMenu((heldObject.Value as Chest).items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, (heldObject.Value as Chest).grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this);
				}
				if ((heldObject.Value as Chest).isEmpty())
				{
					showNextIndex.Value = false;
				}
			}
		}

		public static bool HighlightFertilizers(Item i)
		{
			if (i is Object)
			{
				return (int)(i as Object).category == -19;
			}
			return false;
		}

		private void AttachToSprinklerAttachment(Item i, Farmer who)
		{
			if (i != null && i is Object && IsSprinkler() && heldObject.Value != null)
			{
				who.removeItemFromInventory(i);
				heldObject.Value.heldObject.Value = (i as Object);
				if (Game1.player.ActiveObject == null)
				{
					Game1.player.showNotCarrying();
					Game1.player.Halt();
				}
			}
		}

		public override int healthRecoveredOnConsumption()
		{
			if (Edibility < 0)
			{
				return 0;
			}
			if (base.ParentSheetIndex == 874)
			{
				return (int)((float)staminaRecoveredOnConsumption() * 0.68f);
			}
			return (int)((float)staminaRecoveredOnConsumption() * 0.45f);
		}

		public override int staminaRecoveredOnConsumption()
		{
			return (int)Math.Ceiling((double)Edibility * 2.5) + Quality * Edibility;
		}

		public virtual bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (isTemporarilyInvisible)
			{
				return true;
			}
			if (!justCheckingForActivity && who != null && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() - 1) && who.currentLocation.isObjectAtTile(who.getTileX(), who.getTileY() + 1) && who.currentLocation.isObjectAtTile(who.getTileX() + 1, who.getTileY()) && who.currentLocation.isObjectAtTile(who.getTileX() - 1, who.getTileY()) && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() - 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX(), who.getTileY() + 1).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() - 1, who.getTileY()).isPassable() && !who.currentLocation.getObjectAtTile(who.getTileX() + 1, who.getTileY()).isPassable())
			{
				performToolAction(null, who.currentLocation);
			}
			if ((bool)bigCraftable)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				switch ((int)parentSheetIndex)
				{
				case 231:
					if (readyForHarvest.Value && who.IsLocalPlayer)
					{
						Object item3 = heldObject.Value;
						heldObject.Value = null;
						if (!who.addItemToInventoryBool(item3))
						{
							heldObject.Value = item3;
							Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							return false;
						}
						heldObject.Value = new Object(Vector2.Zero, 787, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
						minutesUntilReady.Value = 16800;
						Game1.playSound("coin");
						readyForHarvest.Value = false;
						return true;
					}
					break;
				case 247:
					Game1.activeClickableMenu = new TailoringMenu();
					return true;
				case 165:
					if (heldObject.Value != null && heldObject.Value is Chest && !(heldObject.Value as Chest).isEmpty())
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						Game1.activeClickableMenu = new ItemGrabMenu((heldObject.Value as Chest).items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, (heldObject.Value as Chest).grabItemFromInventory, null, grabItemFromAutoGrabber, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this);
						return true;
					}
					break;
				case 239:
					shakeTimer = 500;
					who.currentLocation.localSound("DwarvishSentry");
					who.freezePause = 500;
					DelayedAction.functionAfterDelay(delegate
					{
						int totalCrops = Game1.getFarm().getTotalCrops();
						int totalOpenHoeDirt = Game1.getFarm().getTotalOpenHoeDirt();
						int totalCropsReadyForHarvest = Game1.getFarm().getTotalCropsReadyForHarvest();
						int totalUnwateredCrops = Game1.getFarm().getTotalUnwateredCrops();
						int totalGreenhouseCropsReadyForHarvest = Game1.getFarm().getTotalGreenhouseCropsReadyForHarvest();
						int totalForageItems = Game1.getFarm().getTotalForageItems();
						int numberOfMachinesReadyForHarvest = Game1.getFarm().getNumberOfMachinesReadyForHarvest();
						bool flag = Game1.getFarm().doesFarmCaveNeedHarvesting();
						Game1.multipleDialogues(new string[1]
						{
							Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_Intro", Game1.player.farmName.Value) + "^--------------^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_PiecesHay", (Game1.getLocationFromName("Farm") as Farm).piecesOfHay, Utility.numSilos() * 240) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalCrops", totalCrops) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest", totalCropsReadyForHarvest) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsUnwatered", totalUnwateredCrops) + "  ^" + ((totalGreenhouseCropsReadyForHarvest != -1) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_CropsReadyForHarvest_Greenhouse", totalGreenhouseCropsReadyForHarvest) + "  ^") : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalOpenHoeDirt", totalOpenHoeDirt) + "  ^" + ((Game1.whichFarm == 2 || Game1.whichFarm == 6) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_TotalForage", totalForageItems) + "  ^") : "") + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_MachinesReady", numberOfMachinesReadyForHarvest) + "  ^" + Game1.content.LoadString("Strings\\StringsFromCSFiles:FarmComputer_FarmCave", flag ? Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_Yes") : Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")) + "  "
						});
					}, 500);
					return true;
				case 238:
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					Vector2 obelisk = Vector2.Zero;
					Vector2 obelisk2 = Vector2.Zero;
					foreach (KeyValuePair<Vector2, Object> o in who.currentLocation.objects.Pairs)
					{
						if ((bool)o.Value.bigCraftable && o.Value.ParentSheetIndex == 238)
						{
							if (obelisk.Equals(Vector2.Zero))
							{
								obelisk = o.Key;
							}
							else if (obelisk2.Equals(Vector2.Zero))
							{
								obelisk2 = o.Key;
								break;
							}
						}
					}
					if (obelisk2.Equals(Vector2.Zero))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsPair"));
						return false;
					}
					Vector2 target = (!(Vector2.Distance(who.getTileLocation(), obelisk) > Vector2.Distance(who.getTileLocation(), obelisk2))) ? obelisk2 : obelisk;
					foreach (Vector2 v in new List<Vector2>
					{
						new Vector2(target.X, target.Y + 1f),
						new Vector2(target.X - 1f, target.Y),
						new Vector2(target.X + 1f, target.Y),
						new Vector2(target.X, target.Y - 1f)
					})
					{
						if (who.currentLocation.isTileLocationTotallyClearAndPlaceableIgnoreFloors(v))
						{
							for (int k = 0; k < 12; k++)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
							}
							who.currentLocation.playSound("wand");
							Game1.displayFarmer = false;
							Game1.player.freezePause = 800;
							Game1.flashAlpha = 1f;
							DelayedAction.fadeAfterDelay(delegate
							{
								who.setTileLocation(v);
								Game1.displayFarmer = true;
								Game1.globalFadeToClear();
							}, 800);
							new Microsoft.Xna.Framework.Rectangle(who.GetBoundingBox().X, who.GetBoundingBox().Y, 64, 64).Inflate(192, 192);
							int j = 0;
							for (int x = who.getTileX() + 8; x >= who.getTileX() - 8; x--)
							{
								who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, who.getTileY()) * 64f, Color.White, 8, flipped: false, 50f)
								{
									layerDepth = 1f,
									delayBeforeAnimationStart = j * 25,
									motion = new Vector2(-0.25f, 0f)
								});
								j++;
							}
							return true;
						}
					}
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MiniObelisk_NeedsSpace"));
					return false;
				}
				}
			}
			if (name.Equals("Prairie King Arcade System"))
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Game1.currentLocation.showPrairieKingMenu();
				return true;
			}
			if (name.Equals("Junimo Kart Arcade System"))
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				Response[] responses = new Response[3]
				{
					new Response("Progress", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12873")),
					new Response("Endless", Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12875")),
					new Response("Exit", Game1.content.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11738"))
				};
				who.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_Minecart_Menu"), responses, "MinecartGame");
				return true;
			}
			if (name.Equals("Staircase"))
			{
				if (who.currentLocation is MineShaft && (who.currentLocation as MineShaft).shouldCreateLadderOnThisLevel())
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					Game1.enterMine(Game1.CurrentMineLevel + 1);
					Game1.playSound("stairsdown");
				}
			}
			else
			{
				if (name.Equals("Slime Ball"))
				{
					if (justCheckingForActivity)
					{
						return true;
					}
					who.currentLocation.objects.Remove(tileLocation);
					DelayedAction.playSoundAfterDelay("slimedead", 40);
					DelayedAction.playSoundAfterDelay("slimeHit", 100);
					who.currentLocation.playSound("slimeHit");
					Random r = new Random((int)Game1.stats.daysPlayed + (int)Game1.uniqueIDForThisGame + (int)tileLocation.X * 77 + (int)tileLocation.Y * 777 + 2);
					Game1.createMultipleObjectDebris(766, (int)tileLocation.X, (int)tileLocation.Y, r.Next(10, 21), 1f + ((who.FacingDirection == 2) ? 0f : ((float)Game1.random.NextDouble())));
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, tileLocation.Value * 64f, Color.Lime, 10)
					{
						interval = 70f,
						holdLastFrame = true,
						alphaFade = 0.01f
					}, who.currentLocation);
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(-16f, 0f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 0,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(0f, 16f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 100,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(44, tileLocation.Value * 64f + new Vector2(16f, 0f), Color.Lime, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 200,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					while (r.NextDouble() < 0.33)
					{
						Game1.createObjectDebris(557, (int)tileLocation.X, (int)tileLocation.Y, who.UniqueMultiplayerID);
					}
					return true;
				}
				if (name.Equals("Furnace") && who.ActiveObject == null && !readyForHarvest)
				{
					if (heldObject.Value != null)
					{
						return true;
					}
				}
				else
				{
					if (name.Contains("Table"))
					{
						if (heldObject.Value != null)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							Object item2 = heldObject.Value;
							heldObject.Value = null;
							if (who.isMoving())
							{
								Game1.haltAfterCheck = false;
							}
							if (who.addItemToInventoryBool(item2))
							{
								Game1.playSound("coin");
							}
							else
							{
								heldObject.Value = item2;
							}
							return true;
						}
						if (name.Equals("Tile Table"))
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							base.ParentSheetIndex++;
							if ((int)parentSheetIndex == 322)
							{
								base.ParentSheetIndex -= 9;
								return false;
							}
							return true;
						}
						return false;
					}
					if (name.Contains("Stool"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						base.ParentSheetIndex++;
						if ((int)parentSheetIndex == 305)
						{
							base.ParentSheetIndex -= 9;
							return false;
						}
						return true;
					}
					if ((bool)bigCraftable && (name.Contains("Chair") || name.Contains("Painting") || name.Equals("House Plant")))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						base.ParentSheetIndex++;
						int total = -1;
						int baseIndex = -1;
						switch (name)
						{
						case "Red Chair":
							total = 4;
							baseIndex = 44;
							break;
						case "Patio Chair":
							total = 4;
							baseIndex = 52;
							break;
						case "Dark Chair":
							total = 4;
							baseIndex = 60;
							break;
						case "Wood Chair":
							total = 4;
							baseIndex = 24;
							break;
						case "House Plant":
							total = 8;
							baseIndex = 0;
							break;
						case "Painting":
							total = 8;
							baseIndex = 32;
							break;
						}
						if ((int)parentSheetIndex == baseIndex + total)
						{
							base.ParentSheetIndex -= total;
							return false;
						}
						return true;
					}
					if (name.Equals("Flute Block"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						preservedParentSheetIndex.Value = (preservedParentSheetIndex.Value + 100) % 2400;
						shakeTimer = 200;
						if (Game1.soundBank != null)
						{
							if (internalSound != null)
							{
								internalSound.Stop(AudioStopOptions.Immediate);
								internalSound = Game1.soundBank.GetCue("flute");
							}
							else
							{
								internalSound = Game1.soundBank.GetCue("flute");
							}
							internalSound.SetVariable("Pitch", preservedParentSheetIndex.Value);
							internalSound.Play();
						}
						scale.Y = 1.3f;
						shakeTimer = 200;
						return true;
					}
					if (name.Equals("Drum Block"))
					{
						if (justCheckingForActivity)
						{
							return true;
						}
						preservedParentSheetIndex.Value = (preservedParentSheetIndex.Value + 1) % 7;
						shakeTimer = 200;
						if (Game1.soundBank != null)
						{
							if (internalSound != null)
							{
								internalSound.Stop(AudioStopOptions.Immediate);
								internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
							}
							else
							{
								internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
							}
							internalSound.Play();
						}
						scale.Y = 1.3f;
						shakeTimer = 200;
						return true;
					}
					if (IsSprinkler())
					{
						if (heldObject.Value != null && heldObject.Value.ParentSheetIndex == 913)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
							{
								return false;
							}
							if (heldObject.Value.heldObject.Value is Chest)
							{
								Chest chest = heldObject.Value.heldObject.Value as Chest;
								chest.GetMutex().RequestLock(delegate
								{
									chest.ShowMenu();
								});
							}
						}
					}
					else
					{
						if (name.Contains("arecrow"))
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if ((int)parentSheetIndex == 126 && who.CurrentItem != null && who.CurrentItem is Hat)
							{
								shakeTimer = 100;
								if ((int)quality != 0)
								{
									Game1.createItemDebris(new Hat((int)quality - 1), tileLocation.Value * 64f, (who.FacingDirection + 2) % 4);
								}
								quality.Value = (int)(who.CurrentItem as Hat).which + 1;
								who.items[who.CurrentToolIndex] = null;
								who.currentLocation.playSound("dirtyHit");
								return true;
							}
							if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
							{
								return false;
							}
							shakeTimer = 100;
							if (base.SpecialVariable == 0)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12926"));
							}
							else
							{
								Game1.drawObjectDialogue((base.SpecialVariable == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12927") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12929", base.SpecialVariable));
							}
							return true;
						}
						if (name.Equals("Singing Stone"))
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if (Game1.soundBank != null)
							{
								ICue cue = Game1.soundBank.GetCue("crystal");
								int pitch2 = Game1.random.Next(2400);
								pitch2 -= pitch2 % 100;
								cue.SetVariable("Pitch", pitch2);
								shakeTimer = 100;
								cue.Play();
								return true;
							}
						}
						else if (name.Contains("Feed Hopper") && who.ActiveObject == null)
						{
							if (justCheckingForActivity)
							{
								return true;
							}
							if (who.freeSpotsInInventory() > 0)
							{
								int piecesHay = (Game1.getLocationFromName("Farm") as Farm).piecesOfHay;
								if (piecesHay > 0)
								{
									bool shouldReturn = false;
									if (who.currentLocation is AnimalHouse)
									{
										int piecesOfHayToRemove3 = Math.Min((who.currentLocation as AnimalHouse).animalsThatLiveHere.Count, piecesHay);
										piecesOfHayToRemove3 = Math.Max(1, piecesOfHayToRemove3);
										AnimalHouse i = who.currentLocation as AnimalHouse;
										int alreadyHay = i.numberOfObjectsWithName("Hay");
										piecesOfHayToRemove3 = Math.Min(piecesOfHayToRemove3, (int)i.animalLimit - alreadyHay);
										if (piecesOfHayToRemove3 != 0 && Game1.player.couldInventoryAcceptThisObject(178, piecesOfHayToRemove3))
										{
											(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value -= Math.Max(1, piecesOfHayToRemove3);
											who.addItemToInventoryBool(new Object(178, piecesOfHayToRemove3));
											Game1.playSound("shwip");
											shouldReturn = true;
										}
									}
									else if (Game1.player.couldInventoryAcceptThisObject(178, 1))
									{
										(Game1.getLocationFromName("Farm") as Farm).piecesOfHay.Value--;
										who.addItemToInventoryBool(new Object(178, 1));
										Game1.playSound("shwip");
									}
									if ((int)(Game1.getLocationFromName("Farm") as Farm).piecesOfHay <= 0)
									{
										showNextIndex.Value = false;
									}
									if (shouldReturn)
									{
										return true;
									}
								}
								else
								{
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12942"));
								}
							}
							else
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
							}
						}
					}
				}
			}
			Object objectThatWasHeld = heldObject.Value;
			if ((bool)readyForHarvest)
			{
				if (justCheckingForActivity)
				{
					return true;
				}
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				bool check_for_reload = false;
				if (name.Equals("Bee House"))
				{
					int honey_type = -1;
					string honeyName = "Wild";
					int honeyPriceAddition = 0;
					Crop c = Utility.findCloseFlower(who.currentLocation, tileLocation, 5, (Crop crop) => (!crop.forageCrop.Value) ? true : false);
					if (c != null)
					{
						honeyName = Game1.objectInformation[c.indexOfHarvest].Split('/')[0];
						honey_type = c.indexOfHarvest.Value;
						honeyPriceAddition = Convert.ToInt32(Game1.objectInformation[c.indexOfHarvest].Split('/')[1]) * 2;
					}
					if (heldObject.Value != null)
					{
						heldObject.Value.name = honeyName + " Honey";
						heldObject.Value.displayName = loadDisplayName();
						heldObject.Value.Price = Convert.ToInt32(Game1.objectInformation[340].Split('/')[1]) + honeyPriceAddition;
						heldObject.Value.preservedParentSheetIndex.Value = honey_type;
						if (Game1.GetSeasonForLocation(Game1.currentLocation).Equals("winter"))
						{
							heldObject.Value = null;
							readyForHarvest.Value = false;
							showNextIndex.Value = false;
							return false;
						}
						if (who.IsLocalPlayer)
						{
							Object item = heldObject.Value;
							heldObject.Value = null;
							if (!who.addItemToInventoryBool(item))
							{
								heldObject.Value = item;
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
								return false;
							}
						}
						Game1.playSound("coin");
						check_for_reload = true;
					}
				}
				else if (who.IsLocalPlayer)
				{
					heldObject.Value = null;
					if (!who.addItemToInventoryBool(objectThatWasHeld))
					{
						heldObject.Value = objectThatWasHeld;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
						return false;
					}
					Game1.playSound("coin");
					check_for_reload = true;
					switch (name)
					{
					case "Keg":
						Game1.stats.BeveragesMade++;
						break;
					case "Preserves Jar":
						Game1.stats.PreservesMade++;
						break;
					case "Cheese Press":
						if (objectThatWasHeld.ParentSheetIndex == 426)
						{
							Game1.stats.GoatCheeseMade++;
						}
						else
						{
							Game1.stats.CheeseMade++;
						}
						break;
					}
				}
				if (name.Equals("Crystalarium"))
				{
					minutesUntilReady.Value = getMinutesForCrystalarium(objectThatWasHeld.ParentSheetIndex);
					heldObject.Value = (Object)objectThatWasHeld.getOne();
				}
				else if (name.Contains("Tapper"))
				{
					if (who.currentLocation.terrainFeatures.ContainsKey(tileLocation) && who.currentLocation.terrainFeatures[tileLocation] is Tree)
					{
						(who.currentLocation.terrainFeatures[tileLocation] as Tree).UpdateTapperProduct(this, objectThatWasHeld);
					}
				}
				else
				{
					heldObject.Value = null;
				}
				readyForHarvest.Value = false;
				showNextIndex.Value = false;
				if (name.Equals("Bee House") && !Game1.GetSeasonForLocation(who.currentLocation).Equals("winter"))
				{
					heldObject.Value = new Object(Vector2.Zero, 340, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false);
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 4);
				}
				else if (name.Equals("Worm Bin"))
				{
					heldObject.Value = new Object(685, Game1.random.Next(2, 6));
					minutesUntilReady.Value = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, 1);
				}
				if (check_for_reload)
				{
					AttemptAutoLoad(who);
				}
				return true;
			}
			return false;
		}

		public virtual void AttemptAutoLoad(Farmer who)
		{
			Object source_object = null;
			if (who.currentLocation.objects.TryGetValue(new Vector2(TileLocation.X, TileLocation.Y - 1f), out source_object) && source_object != null && source_object is Chest)
			{
				Chest chest = source_object as Chest;
				if (chest.specialChestType.Value == Chest.SpecialChestTypes.AutoLoader)
				{
					chest.GetMutex().RequestLock(delegate
					{
						chest.GetMutex().ReleaseLock();
						Object value = heldObject.Value;
						heldObject.Value = null;
						foreach (Item current in chest.items)
						{
							autoLoadChest = chest;
							bool num = performObjectDropInAction(current, probe: true, who);
							heldObject.Value = value;
							if (num)
							{
								if (performObjectDropInAction(current, probe: false, who))
								{
									ConsumeInventoryItem(who, current, 1);
								}
								autoLoadChest = null;
								return;
							}
						}
						autoLoadChest = null;
						heldObject.Value = value;
					});
				}
			}
		}

		public virtual void farmerAdjacentAction(GameLocation location)
		{
			if (name == null || isTemporarilyInvisible)
			{
				return;
			}
			if (name.Equals("Flute Block") && (internalSound == null || ((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastNoteBlockSoundTime >= 1000 && !internalSound.IsPlaying)) && !Game1.dialogueUp)
			{
				if (Game1.soundBank != null)
				{
					internalSound = Game1.soundBank.GetCue("flute");
					internalSound.SetVariable("Pitch", preservedParentSheetIndex.Value);
					internalSound.Play();
				}
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
				if (location is IslandSouthEast)
				{
					(location as IslandSouthEast).OnFlutePlayed(preservedParentSheetIndex.Value);
				}
			}
			else if (name.Equals("Drum Block") && (internalSound == null || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds - (double)lastNoteBlockSoundTime >= 1000.0 && !internalSound.IsPlaying)) && !Game1.dialogueUp)
			{
				if (Game1.soundBank != null)
				{
					internalSound = Game1.soundBank.GetCue("drumkit" + preservedParentSheetIndex.Value);
					internalSound.Play();
				}
				scale.Y = 1.3f;
				shakeTimer = 200;
				lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			}
			else
			{
				if (!name.Equals("Obelisk"))
				{
					return;
				}
				scale.X += 1f;
				if (scale.X > 30f)
				{
					base.ParentSheetIndex = (((int)parentSheetIndex == 29) ? 30 : 29);
					scale.X = 0f;
					scale.Y += 2f;
				}
				if (!(scale.Y >= 20f) || !(Game1.random.NextDouble() < 0.0001) || location.characters.Count >= 4)
				{
					return;
				}
				Vector2 playerPos = Game1.player.getTileLocation();
				Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
				int num = 0;
				Vector2 v;
				while (true)
				{
					if (num < adjacentTilesOffsets.Length)
					{
						Vector2 offset = adjacentTilesOffsets[num];
						v = playerPos + offset;
						if (!location.isTileOccupied(v) && location.isTilePassable(new Location((int)v.X, (int)v.Y), Game1.viewport) && location.isCharacterAtTile(v) == null)
						{
							break;
						}
						num++;
						continue;
					}
					return;
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					location.characters.Add(new GreenSlime(v * new Vector2(64f, 64f)));
				}
				else if (Game1.random.NextDouble() < 0.5)
				{
					location.characters.Add(new ShadowGuy(v * new Vector2(64f, 64f)));
				}
				else
				{
					location.characters.Add(new ShadowGirl(v * new Vector2(64f, 64f)));
				}
				((Monster)location.characters[location.characters.Count - 1]).moveTowardPlayerThreshold.Value = 4;
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(352, 400f, 2, 1, v * new Vector2(64f, 64f), flicker: false, flipped: false));
				location.playSound("shadowpeep");
			}
		}

		public virtual void addWorkingAnimation(GameLocation environment)
		{
			if (environment == null || !environment.farmers.Any())
			{
				return;
			}
			string name = this.name;
			switch (name)
			{
			default:
				if (name == "Slime Egg-Press")
				{
					Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -160f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Lime, 1f, 0f, 0f, 0f)
					{
						alphaFade = 0.005f
					});
				}
				break;
			case "Keg":
			{
				Color c = Color.DarkGray;
				if (heldObject.Value.Name.Contains("Wine"))
				{
					c = Color.Lavender;
				}
				else if (heldObject.Value.Name.Contains("Juice"))
				{
					c = Color.White;
				}
				else if (heldObject.Value.name.Equals("Beer"))
				{
					c = Color.Yellow;
				}
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, c * 0.75f, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				environment.playSound("bubbles");
				break;
			}
			case "Preserves Jar":
			{
				Color c = Color.White;
				if (heldObject.Value.Name.Contains("Pickled"))
				{
					c = Color.White;
				}
				else if (heldObject.Value.Name.Contains("Jelly"))
				{
					c = Color.LightBlue;
				}
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, c * 0.75f, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				break;
			}
			case "Oil Maker":
				Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), 80f, 6, 999999, tileLocation.Value * 64f + new Vector2(0f, -128f), flicker: false, flipped: false, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f, 0f, Color.Yellow, 1f, 0f, 0f, 0f)
				{
					alphaFade = 0.005f
				});
				break;
			case "Furnace":
				if (Game1.random.NextDouble() < 0.5)
				{
					Game1.multiplayer.broadcastSprites(environment, new TemporaryAnimatedSprite(30, tileLocation.Value * 64f + new Vector2(0f, -16f), Color.White, 4, flipped: false, 50f, 10, 64, (tileLocation.Y + 1f) * 64f / 10000f + 0.0001f)
					{
						alphaFade = 0.005f,
						light = true,
						lightcolor = Color.Black
					});
					environment.playSound("fireball");
				}
				break;
			}
		}

		public virtual void onReadyForHarvest(GameLocation environment)
		{
		}

		public virtual bool minutesElapsed(int minutes, GameLocation environment)
		{
			if (heldObject.Value != null && !name.Contains("Table") && (!bigCraftable || (int)parentSheetIndex != 165))
			{
				if (name.Equals("Bee House") && !environment.IsOutdoors)
				{
					return false;
				}
				if (IsSprinkler())
				{
					return false;
				}
				if ((bool)bigCraftable && (int)parentSheetIndex == 231)
				{
					return false;
				}
				if (Game1.IsMasterGame)
				{
					minutesUntilReady.Value -= minutes;
				}
				if ((int)minutesUntilReady <= 0 && !name.Contains("Incubator"))
				{
					if (!readyForHarvest)
					{
						environment.playSound("dwop");
					}
					readyForHarvest.Value = true;
					minutesUntilReady.Value = 0;
					onReadyForHarvest(environment);
					showNextIndex.Value = false;
					if (name.Equals("Bee House") || name.Equals("Loom") || name.Equals("Mushroom Box"))
					{
						showNextIndex.Value = true;
					}
					if (lightSource != null)
					{
						environment.removeLightSource(lightSource.identifier);
						lightSource = null;
					}
				}
				if (!readyForHarvest && Game1.random.NextDouble() < 0.33)
				{
					addWorkingAnimation(environment);
				}
			}
			else if ((bool)bigCraftable)
			{
				switch ((int)parentSheetIndex)
				{
				case 29:
				case 30:
					showNextIndex.Value = ((int)parentSheetIndex == 29);
					scale.Y = Math.Max(0f, scale.Y -= minutes / 2 + 1);
					break;
				case 96:
				case 97:
					minutesUntilReady.Value -= minutes;
					showNextIndex.Value = ((int)parentSheetIndex == 96);
					if ((int)minutesUntilReady <= 0)
					{
						performRemoveAction(tileLocation, environment);
						environment.objects.Remove(tileLocation);
						environment.objects.Add(tileLocation, new Object(tileLocation, 98));
						if (!Game1.MasterPlayer.mailReceived.Contains("Capsule_Broken"))
						{
							Game1.MasterPlayer.mailReceived.Add("Capsule_Broken");
						}
					}
					break;
				case 141:
				case 142:
					showNextIndex.Value = ((int)parentSheetIndex == 141);
					break;
				case 83:
					showNextIndex.Value = false;
					environment.removeLightSource((int)(tileLocation.X * 797f + tileLocation.Y * 13f + 666f));
					break;
				}
			}
			return false;
		}

		public override string checkForSpecialItemHoldUpMeessage()
		{
			if (!bigCraftable)
			{
				if (type != null && type.Equals("Arch"))
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12993");
				}
				switch ((int)parentSheetIndex)
				{
				case 102:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12994");
				case 535:
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12995");
				}
			}
			else
			{
				int num = parentSheetIndex;
				if (num == 160)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12996");
				}
			}
			return base.checkForSpecialItemHoldUpMeessage();
		}

		public bool countsForShippedCollection()
		{
			if (type == null || type.Contains("Arch") || (bool)bigCraftable)
			{
				return false;
			}
			if ((int)parentSheetIndex == 433)
			{
				return true;
			}
			switch (base.Category)
			{
			case -74:
			case -29:
			case -24:
			case -22:
			case -21:
			case -20:
			case -19:
			case -14:
			case -12:
			case -8:
			case -7:
			case -2:
			case 0:
				return false;
			default:
				return isIndexOkForBasicShippedCategory(parentSheetIndex);
			}
		}

		public static bool isIndexOkForBasicShippedCategory(int index)
		{
			switch (index)
			{
			case 434:
				return false;
			case 889:
			case 928:
				return false;
			default:
				return true;
			}
		}

		public static bool isPotentialBasicShippedCategory(int index, string category)
		{
			int cat = 0;
			int.TryParse(category, out cat);
			if (index == 433)
			{
				return true;
			}
			switch (cat)
			{
			case -74:
			case -29:
			case -24:
			case -22:
			case -21:
			case -20:
			case -19:
			case -14:
			case -12:
			case -8:
			case -7:
			case -2:
				return false;
			case 0:
				return false;
			default:
				return isIndexOkForBasicShippedCategory(index);
			}
		}

		public Vector2 getScale()
		{
			if (base.Category == -22)
			{
				return Vector2.Zero;
			}
			if (!bigCraftable)
			{
				scale.Y = Math.Max(4f, scale.Y - 0.04f);
				return scale;
			}
			if ((heldObject.Value == null && (int)minutesUntilReady <= 0) || (bool)readyForHarvest || (int)parentSheetIndex == 10 || name.Contains("Table") || (int)parentSheetIndex == 105 || (int)parentSheetIndex == 264 || (int)parentSheetIndex == 165 || (int)parentSheetIndex == 231)
			{
				return Vector2.Zero;
			}
			if (name.Equals("Loom"))
			{
				scale.X = (float)((double)(scale.X + 0.04f) % (Math.PI * 2.0));
				return Vector2.Zero;
			}
			scale.X -= 0.1f;
			scale.Y += 0.1f;
			if (scale.X <= 0f)
			{
				scale.X = 10f;
			}
			if (scale.Y >= 10f)
			{
				scale.Y = 0f;
			}
			return new Vector2(Math.Abs(scale.X - 5f), Math.Abs(scale.Y - 5f));
		}

		public virtual void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			if ((bool)f.ActiveObject.bigCraftable)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, objectPosition, getSourceRectForBigCraftable(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
				return;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition, GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
			if (f.ActiveObject == null || !f.ActiveObject.Name.Contains("="))
			{
				return;
			}
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition + new Vector2(32f, 32f), GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex), Color.White, 0f, new Vector2(32f, 32f), 4f + Math.Abs(Game1.starCropShimmerPause) / 8f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 3) / 10000f));
			if (!(Math.Abs(Game1.starCropShimmerPause) <= 0.05f) || !(Game1.random.NextDouble() < 0.97))
			{
				Game1.starCropShimmerPause += 0.04f;
				if (Game1.starCropShimmerPause >= 0.8f)
				{
					Game1.starCropShimmerPause = -0.8f;
				}
			}
		}

		public virtual void drawPlacementBounds(SpriteBatch spriteBatch, GameLocation location)
		{
			if (!isPlaceable() || this is Wallpaper)
			{
				return;
			}
			int X = (int)Game1.GetPlacementGrabTile().X * 64;
			int Y = (int)Game1.GetPlacementGrabTile().Y * 64;
			Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
			if (Game1.isCheckingNonMousePlacement)
			{
				Vector2 nearbyValidPlacementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, X, Y);
				X = (int)nearbyValidPlacementPosition.X;
				Y = (int)nearbyValidPlacementPosition.Y;
			}
			if (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, X, Y))
			{
				return;
			}
			bool canPlaceHere = Utility.playerCanPlaceItemHere(location, this, X, Y, Game1.player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, X, Y) && Utility.withinRadiusOfPlayer(X, Y, 1, Game1.player));
			Game1.isCheckingNonMousePlacement = false;
			int width = 1;
			int height = 1;
			if (this is Furniture)
			{
				Furniture obj = this as Furniture;
				width = obj.getTilesWide();
				height = obj.getTilesHigh();
			}
			for (int x_offset = 0; x_offset < width; x_offset++)
			{
				for (int y_offset = 0; y_offset < height; y_offset++)
				{
					spriteBatch.Draw(Game1.mouseCursors, new Vector2((X / 64 + x_offset) * 64 - Game1.viewport.X, (Y / 64 + y_offset) * 64 - Game1.viewport.Y), new Microsoft.Xna.Framework.Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
				}
			}
			if ((bool)bigCraftable || this is Furniture || ((int)category != -74 && (int)category != -19))
			{
				draw(spriteBatch, X / 64, Y / 64, 0.5f);
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if ((bool)isRecipe)
			{
				transparency = 0.5f;
				scaleSize *= 0.75f;
			}
			bool shouldDrawStackNumber = ((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != int.MaxValue;
			if (IsRecipe)
			{
				shouldDrawStackNumber = false;
			}
			if ((bool)bigCraftable)
			{
				Microsoft.Xna.Framework.Rectangle sourceRect = getSourceRectForBigCraftable(parentSheetIndex);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(32f, 32f), sourceRect, color * transparency, 0f, new Vector2(8f, 16f), 4f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
				if (shouldDrawStackNumber)
				{
					Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
				}
			}
			else
			{
				if ((int)parentSheetIndex != 590 && drawShadow)
				{
					spriteBatch.Draw(Game1.shadowTexture, location + new Vector2(32f, 48f), Game1.shadowTexture.Bounds, color * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, layerDepth - 0.0001f);
				}
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2((int)(32f * scaleSize), (int)(32f * scaleSize)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), color * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
				if (shouldDrawStackNumber)
				{
					Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 1f), 3f * scaleSize, 1f, color);
				}
				if (drawStackNumber != 0 && (int)quality > 0)
				{
					Microsoft.Xna.Framework.Rectangle quality_rect = ((int)quality < 4) ? new Microsoft.Xna.Framework.Rectangle(338 + ((int)quality - 1) * 8, 400, 8, 8) : new Microsoft.Xna.Framework.Rectangle(346, 392, 8, 8);
					Texture2D quality_sheet = Game1.mouseCursors;
					float yOffset = ((int)quality < 4) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f);
					spriteBatch.Draw(quality_sheet, location + new Vector2(12f, 52f + yOffset), quality_rect, color * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + yOffset), SpriteEffects.None, layerDepth);
				}
				if (base.Category == -22 && uses.Value > 0)
				{
					float health = ((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses;
					spriteBatch.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle((int)location.X, (int)(location.Y + 56f * scaleSize), (int)(64f * scaleSize * health), (int)(8f * scaleSize)), Utility.getRedToGreenLerpColor(health));
				}
			}
			if ((bool)isRecipe)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), color, 0f, Vector2.Zero, 3f, SpriteEffects.None, layerDepth + 0.0001f);
			}
		}

		public virtual void drawAsProp(SpriteBatch b)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			int x = (int)tileLocation.X;
			int y = (int)tileLocation.Y;
			if ((bool)bigCraftable)
			{
				Vector2 scaleFactor = getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				b.Draw(destinationRectangle: new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f)), texture: Game1.bigCraftableSpriteSheet, sourceRectangle: getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), color: Color.White, rotation: 0f, origin: Vector2.Zero, effects: SpriteEffects.None, layerDepth: Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f) + (((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264) ? 0.0015f : 0f));
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f + 0.0001f));
				}
				return;
			}
			if ((int)parentSheetIndex != 590 && (int)parentSheetIndex != 742)
			{
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
			}
			Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
			Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
			Microsoft.Xna.Framework.Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
			Color white = Color.White;
			Vector2 origin = new Vector2(8f, 8f);
			_ = scale;
			b.Draw(objectSpriteSheet, position2, sourceRectangle, white, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
		}

		public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if ((bool)bigCraftable)
			{
				Vector2 scaleFactor = getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
				if (base.ParentSheetIndex == 105 || base.ParentSheetIndex == 264)
				{
					draw_layer = Math.Max(0f, (float)((y + 1) * 64 + 2) / 10000f) + (float)x / 1000000f;
				}
				if ((int)parentSheetIndex == 272)
				{
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, getSourceRectForBigCraftable(base.ParentSheetIndex + 1), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
					spriteBatch.Draw(Game1.bigCraftableSpriteSheet, position + new Vector2(8.5f, 12f) * 4f, getSourceRectForBigCraftable(base.ParentSheetIndex + 2), Color.White * alpha, (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * -1.5f, new Vector2(7.5f, 15.5f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
					return;
				}
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64) / 10000f + 0.0001f + (float)x * 1E-05f));
				}
				if ((bool)isLamp && Game1.isDarkOut())
				{
					spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x / 1000000f);
				}
				if ((int)parentSheetIndex == 126 && (int)quality != 0)
				{
					spriteBatch.Draw(FarmerRenderer.hatsTexture, position + new Vector2(-3f, -6f) * 4f, new Microsoft.Xna.Framework.Rectangle(((int)quality - 1) * 20 % FarmerRenderer.hatsTexture.Width, ((int)quality - 1) * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f);
				}
			}
			else if (!Game1.eventUp || (Game1.CurrentEvent != null && !Game1.CurrentEvent.isTileWalkedOn(x, y)))
			{
				if ((int)parentSheetIndex == 590)
				{
					Texture2D mouseCursors = Game1.mouseCursors;
					Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
					Microsoft.Xna.Framework.Rectangle? sourceRectangle = new Microsoft.Xna.Framework.Rectangle(368 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1200.0 <= 400.0) ? ((int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 400.0 / 100.0) * 16) : 0), 32, 16, 16);
					Color color = Color.White * alpha;
					Vector2 origin = new Vector2(8f, 8f);
					_ = scale;
					spriteBatch.Draw(mouseCursors, position2, sourceRectangle, color, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
					return;
				}
				if ((int)fragility != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
				}
				Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
				Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
				Microsoft.Xna.Framework.Rectangle? sourceRectangle2 = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
				Color color2 = Color.White * alpha;
				Vector2 origin2 = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet, position3, sourceRectangle2, color2, 0f, origin2, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
				if (heldObject.Value != null && IsSprinkler())
				{
					Vector2 offset = Vector2.Zero;
					if (heldObject.Value.ParentSheetIndex == 913)
					{
						offset = new Vector2(0f, -20f);
					}
					Texture2D objectSpriteSheet2 = Game1.objectSpriteSheet;
					Vector2 position4 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)) + offset);
					Microsoft.Xna.Framework.Rectangle? sourceRectangle3 = GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex + 1);
					Color color3 = Color.White * alpha;
					Vector2 origin3 = new Vector2(8f, 8f);
					_ = scale;
					spriteBatch.Draw(objectSpriteSheet2, position4, sourceRectangle3, color3, 0f, origin3, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(isPassable() ? getBoundingBox(new Vector2(x, y)).Top : getBoundingBox(new Vector2(x, y)).Bottom) / 10000f + 1E-05f);
				}
			}
			if (!readyForHarvest)
			{
				return;
			}
			float base_sort = (float)((y + 1) * 64) / 10000f + tileLocation.X / 50000f;
			if ((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264)
			{
				base_sort += 0.02f;
			}
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 8, (float)(y * 64 - 96 - 16) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort + 1E-06f);
			if (heldObject.Value != null)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, heldObject.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1E-05f);
				if (heldObject.Value is ColoredObject)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (float)(y * 64 - 64 - 8) + yOffset)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)heldObject.Value.parentSheetIndex + 1, 16, 16), (heldObject.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, base_sort + 1.1E-05f);
				}
			}
		}

		public virtual void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
		{
			if (isTemporarilyInvisible)
			{
				return;
			}
			if ((bool)bigCraftable)
			{
				Vector2 scaleFactor = getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile));
				Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(position) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White * alpha, scale.X, new Vector2(8f, 8f), 4f, SpriteEffects.None, layerDepth);
				}
				if ((bool)isLamp && Game1.isDarkOut())
				{
					spriteBatch.Draw(Game1.mouseCursors, position + new Vector2(-32f, -32f), new Microsoft.Xna.Framework.Rectangle(88, 1779, 32, 32), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
				}
			}
			else if (!Game1.eventUp || !Game1.CurrentEvent.isTileWalkedOn(xNonTile / 64, yNonTile / 64))
			{
				if ((int)parentSheetIndex != 590 && (int)fragility != 2)
				{
					spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32, yNonTile + 51 + 4)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, layerDepth - 1E-06f);
				}
				Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
				Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), yNonTile + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
				Microsoft.Xna.Framework.Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
				Color color = Color.White * alpha;
				Vector2 origin = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle, color, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
			}
		}

		private int getMinutesForCrystalarium(int whichGem)
		{
			switch (whichGem)
			{
			case 80:
				return 420;
			case 60:
				return 3000;
			case 68:
				return 1120;
			case 70:
				return 2400;
			case 64:
				return 3000;
			case 62:
				return 2240;
			case 66:
				return 1360;
			case 72:
				return 7200;
			case 82:
				return 1300;
			case 84:
				return 1120;
			case 86:
				return 800;
			default:
				return 5000;
			}
		}

		public override int maximumStackSize()
		{
			if (base.ParentSheetIndex == 911)
			{
				return 1;
			}
			if (base.Category == -22)
			{
				return 1;
			}
			return 999;
		}

		public override int addToStack(Item otherStack)
		{
			int maxStack = maximumStackSize();
			if (maxStack != 1)
			{
				stack.Value += otherStack.Stack;
				if (otherStack is Object)
				{
					Object otherObject = otherStack as Object;
					if (IsSpawnedObject && !otherObject.IsSpawnedObject)
					{
						IsSpawnedObject = false;
					}
				}
				if ((int)stack > maxStack)
				{
					int result = (int)stack - maxStack;
					stack.Value = maxStack;
					return result;
				}
				return 0;
			}
			return otherStack.Stack;
		}

		public virtual void hoverAction()
		{
		}

		public virtual bool clicked(Farmer who)
		{
			return false;
		}

		public override Item getOne()
		{
			if ((bool)bigCraftable)
			{
				int index = base.ParentSheetIndex;
				if (name.Contains("Seasonal"))
				{
					index = base.ParentSheetIndex - base.ParentSheetIndex % 4;
				}
				Object @object = new Object(tileLocation, index);
				@object.IsRecipe = isRecipe;
				@object.name = name;
				@object.DisplayName = DisplayName;
				@object.SpecialVariable = base.SpecialVariable;
				@object._GetOneFrom(this);
				return @object;
			}
			Object object2 = new Object(tileLocation, parentSheetIndex, 1);
			object2.Scale = scale;
			object2.Quality = quality;
			object2.IsSpawnedObject = isSpawnedObject;
			object2.IsRecipe = isRecipe;
			object2.Stack = 1;
			object2.SpecialVariable = base.SpecialVariable;
			object2.Price = price;
			object2.name = name;
			object2.DisplayName = DisplayName;
			object2.HasBeenInInventory = base.HasBeenInInventory;
			object2.HasBeenPickedUpByFarmer = HasBeenPickedUpByFarmer;
			object2.uses.Value = uses.Value;
			object2.questItem.Value = questItem;
			object2.questId.Value = questId;
			object2.preserve.Value = preserve.Value;
			object2.preservedParentSheetIndex.Value = preservedParentSheetIndex.Value;
			object2._GetOneFrom(this);
			return object2;
		}

		public override void _GetOneFrom(Item source)
		{
			orderData.Value = (source as Object).orderData.Value;
			base._GetOneFrom(source);
		}

		public override bool canBePlacedHere(GameLocation l, Vector2 tile)
		{
			if ((int)parentSheetIndex == 710)
			{
				if (CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y))
				{
					return true;
				}
				return false;
			}
			if (((int)parentSheetIndex == 105 || (int)parentSheetIndex == 264) && (bool)bigCraftable && l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is Tree && !l.objects.ContainsKey(tile))
			{
				return true;
			}
			if ((int)parentSheetIndex == 805 && l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is Tree)
			{
				return true;
			}
			if (name != null && name.Contains("Bomb") && (!l.isTileOccupiedForPlacement(tile, this) || l.isTileOccupiedByFarmer(tile) != null))
			{
				return true;
			}
			if (isWildTreeSeed(parentSheetIndex))
			{
				if (!l.isTileOccupiedForPlacement(tile, this))
				{
					return canPlaceWildTreeSeed(l, tile);
				}
				return false;
			}
			if (((int)category == -74 || (int)category == -19) && !l.isTileHoeDirt(tile) && !bigCraftable.Value)
			{
				switch ((int)parentSheetIndex)
				{
				case 69:
				case 292:
				case 309:
				case 310:
				case 311:
				case 628:
				case 629:
				case 630:
				case 631:
				case 632:
				case 633:
				case 835:
				case 891:
					if (!l.isTileOccupiedForPlacement(tile, this))
					{
						if (!l.CanPlantTreesHere(parentSheetIndex, (int)tile.X, (int)tile.Y))
						{
							return l.isOutdoors;
						}
						return true;
					}
					return false;
				case 251:
					if (!l.isTileOccupiedForPlacement(tile, this))
					{
						if (!l.isOutdoors)
						{
							if (l.IsGreenhouse)
							{
								return l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
							}
							return false;
						}
						return true;
					}
					return false;
				default:
					return false;
				}
			}
			if ((int)category == -19 && l.isTileHoeDirt(tile))
			{
				if ((int)parentSheetIndex == 805)
				{
					return false;
				}
				if (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is HoeDirt && (int)(l.terrainFeatures[tile] as HoeDirt).fertilizer != 0)
				{
					return false;
				}
				if (l.objects.ContainsKey(tile) && l.objects[tile] is IndoorPot && (int)(l.objects[tile] as IndoorPot).hoeDirt.Value.fertilizer != 0)
				{
					return false;
				}
			}
			if (l != null)
			{
				Vector2 nonTile = tile * 64f * 64f;
				nonTile.X += 32f;
				nonTile.Y += 32f;
				foreach (Furniture f in l.furniture)
				{
					if ((int)f.furniture_type == 11 && f.getBoundingBox(f.tileLocation).Contains((int)nonTile.X, (int)nonTile.Y) && f.heldObject.Value == null)
					{
						return true;
					}
					if (f.getBoundingBox(f.TileLocation).Intersects(new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64)) && !f.isPassable() && !f.AllowPlacementOnThisTile((int)tile.X, (int)tile.Y))
					{
						return false;
					}
				}
			}
			return !l.isTileOccupiedForPlacement(tile, this);
		}

		public override bool isPlaceable()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 681) || Utility.IsNormalObjectAtParentSheetIndex(this, 688) || Utility.IsNormalObjectAtParentSheetIndex(this, 689) || Utility.IsNormalObjectAtParentSheetIndex(this, 690) || Utility.IsNormalObjectAtParentSheetIndex(this, 261) || Utility.IsNormalObjectAtParentSheetIndex(this, 886))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 896))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 911))
			{
				return false;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 879))
			{
				return false;
			}
			_ = base.Category;
			if (type.Value != null && (base.Category == -8 || base.Category == -9 || type.Value.Equals("Crafting") || isSapling() || (int)parentSheetIndex == 710 || base.Category == -74 || base.Category == -19) && ((int)edibility < 0 || (int)parentSheetIndex == 292 || (int)parentSheetIndex == 891))
			{
				return true;
			}
			return false;
		}

		public bool IsConsideredReadyMachineForComputer()
		{
			if (bigCraftable.Value && heldObject.Value != null)
			{
				if (!(heldObject.Value is Chest))
				{
					return minutesUntilReady.Value <= 0;
				}
				if (!(heldObject.Value as Chest).isEmpty())
				{
					return true;
				}
			}
			return false;
		}

		public bool isSapling()
		{
			if (bigCraftable.Value)
			{
				return false;
			}
			if (!(GetType() == typeof(Object)))
			{
				return false;
			}
			if (name.Contains("Sapling"))
			{
				return true;
			}
			return false;
		}

		public static bool isWildTreeSeed(int index)
		{
			if (index != 309 && index != 310 && index != 311 && index != 292)
			{
				return index == 891;
			}
			return true;
		}

		private bool canPlaceWildTreeSeed(GameLocation location, Vector2 tile)
		{
			bool isTileDiggable = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Diggable", "Back") != null;
			string noSpawn = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "NoSpawn", "Back");
			bool cantSpawnHere = noSpawn != null && (noSpawn.Equals("Tree") || noSpawn.Equals("All") || noSpawn.Equals("True"));
			bool num = location is Farm || location.CanPlantTreesHere(parentSheetIndex, (int)tile.X, (int)tile.Y);
			bool isBlocked = location.objects.ContainsKey(tile) || (location.terrainFeatures.ContainsKey(tile) && !(location.terrainFeatures[tile] is HoeDirt));
			if ((num | isTileDiggable) && !cantSpawnHere)
			{
				return !isBlocked;
			}
			return false;
		}

		public virtual bool IsSprinkler()
		{
			if (GetBaseRadiusForSprinkler() >= 0)
			{
				return true;
			}
			return false;
		}

		public virtual int GetModifiedRadiusForSprinkler()
		{
			int radius = GetBaseRadiusForSprinkler();
			if (radius < 0)
			{
				return -1;
			}
			if (heldObject.Value != null && Utility.IsNormalObjectAtParentSheetIndex((Object)heldObject, 915))
			{
				radius++;
			}
			return radius;
		}

		public virtual int GetBaseRadiusForSprinkler()
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 599))
			{
				return 0;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 621))
			{
				return 1;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 645))
			{
				return 2;
			}
			return -1;
		}

		public virtual bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			Vector2 placementTile = new Vector2(x / 64, y / 64);
			health = 10;
			if (who != null)
			{
				owner.Value = who.UniqueMultiplayerID;
			}
			else
			{
				owner.Value = Game1.player.UniqueMultiplayerID;
			}
			if (!bigCraftable && !(this is Furniture))
			{
				if (IsSprinkler() && location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSprinklers", "Back") == "T")
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NoSprinklers"));
					return false;
				}
				switch (base.ParentSheetIndex)
				{
				case 926:
					if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Torch(placementTile, 278, bigCraftable: true)
					{
						Fragility = 1,
						destroyOvernight = true
					});
					Utility.addSmokePuff(location, new Vector2(x, y));
					Utility.addSmokePuff(location, new Vector2(x + 16, y + 16));
					Utility.addSmokePuff(location, new Vector2(x + 32, y));
					Utility.addSmokePuff(location, new Vector2(x + 48, y + 16));
					Utility.addSmokePuff(location, new Vector2(x + 32, y + 32));
					Game1.playSound("fireball");
					return true;
				case 292:
				case 309:
				case 310:
				case 311:
				case 891:
				{
					if (!canPlaceWildTreeSeed(location, placementTile))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021"));
						return false;
					}
					Vector2 v = default(Vector2);
					for (int l = x / 64 - 2; l <= x / 64 + 2; l++)
					{
						for (int k = y / 64 - 2; k <= y / 64 + 2; k++)
						{
							v.X = l;
							v.Y = k;
							if (location.terrainFeatures.ContainsKey(v) && location.terrainFeatures[v] is FruitTree)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060_Fruit"));
								return false;
							}
						}
					}
					int whichTree = 1;
					switch ((int)parentSheetIndex)
					{
					case 292:
						whichTree = 8;
						break;
					case 310:
						whichTree = 2;
						break;
					case 311:
						whichTree = 3;
						break;
					case 891:
						whichTree = 7;
						break;
					}
					location.terrainFeatures.Remove(placementTile);
					location.terrainFeatures.Add(placementTile, new Tree(whichTree, 0));
					location.playSound("dirtyHit");
					return true;
				}
				case 286:
				{
					foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
					{
						if (temporarySprite.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int idNum = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = idNum,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 3f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = idNum
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 287:
				{
					foreach (TemporaryAnimatedSprite temporarySprite2 in location.temporarySprites)
					{
						if (temporarySprite2.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int idNum = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = idNum,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = idNum
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 288:
				{
					foreach (TemporaryAnimatedSprite temporarySprite3 in location.temporarySprites)
					{
						if (temporarySprite3.position.Equals(placementTile * 64f))
						{
							return false;
						}
					}
					int idNum = Game1.random.Next();
					location.playSound("thudStep");
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, placementTile * 64f, flicker: true, flipped: false, location, who)
					{
						shakeIntensity = 0.5f,
						shakeIntensityChange = 0.002f,
						extraInfoForEndBehavior = idNum,
						endFunction = location.removeTemporarySpritesWithID
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f)
					{
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 100,
						id = idNum
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, placementTile * 64f + new Vector2(5f, 0f) * 4f, flicker: true, flipped: false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 200,
						id = idNum
					});
					location.netAudio.StartPlaying("fuse");
					return true;
				}
				case 297:
					if (location.objects.ContainsKey(placementTile) || location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Grass(1, 4));
					location.playSound("dirtyHit");
					return true;
				case 298:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 5, isGate: false));
					location.playSound("axe");
					return true;
				case 322:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 1, isGate: false));
					location.playSound("axe");
					return true;
				case 323:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 2, isGate: false));
					location.playSound("stoneStep");
					return true;
				case 324:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 3, isGate: false));
					location.playSound("hammer");
					return true;
				case 325:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Fence(placementTile, 4, isGate: true));
					location.playSound("axe");
					return true;
				case 328:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(0));
					location.playSound("axchop");
					return true;
				case 329:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(1));
					location.playSound("thudStep");
					return true;
				case 331:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(2));
					location.playSound("axchop");
					return true;
				case 333:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(3));
					location.playSound("thudStep");
					return true;
				case 401:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(4));
					location.playSound("thudStep");
					return true;
				case 293:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(10));
					location.playSound("thudStep");
					return true;
				case 405:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(6));
					location.playSound("woodyStep");
					return true;
				case 407:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(5));
					location.playSound("dirtyHit");
					return true;
				case 409:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(7));
					location.playSound("stoneStep");
					return true;
				case 415:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(9));
					location.playSound("stoneStep");
					return true;
				case 411:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(8));
					location.playSound("stoneStep");
					return true;
				case 840:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(11));
					location.playSound("stoneStep");
					return true;
				case 841:
					if (location.terrainFeatures.ContainsKey(placementTile))
					{
						return false;
					}
					location.terrainFeatures.Add(placementTile, new Flooring(12));
					location.playSound("stoneStep");
					return true;
				case 93:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.removeLightSource((int)(tileLocation.X * 2000f + tileLocation.Y));
					location.removeLightSource((int)(long)Game1.player.uniqueMultiplayerID);
					new Torch(placementTile, 1).placementAction(location, x, y, (who == null) ? Game1.player : who);
					return true;
				case 94:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					new Torch(placementTile, 1, 94).placementAction(location, x, y, who);
					return true;
				case 710:
					if (!CrabPot.IsValidCrabPotLocationTile(location, (int)placementTile.X, (int)placementTile.Y))
					{
						return false;
					}
					new CrabPot(placementTile).placementAction(location, x, y, who);
					return true;
				case 805:
					if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is Tree)
					{
						return (location.terrainFeatures[placementTile] as Tree).fertilize(location);
					}
					return false;
				}
			}
			else
			{
				switch (base.ParentSheetIndex)
				{
				case 37:
				case 38:
				case 39:
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					location.objects.Add(placementTile, new Sign(placementTile, base.ParentSheetIndex));
					location.playSound("axe");
					return true;
				case 62:
					location.objects.Add(placementTile, new IndoorPot(placementTile));
					break;
				case 71:
					if (location is MineShaft)
					{
						if ((location as MineShaft).shouldCreateLadderOnThisLevel() && (location as MineShaft).recursiveTryToCreateLadderDown(placementTile))
						{
							MineShaft.numberOfCraftedStairsUsedThisRun++;
							return true;
						}
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
					}
					return false;
				case 130:
				case 232:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50
					});
					location.playSound(((int)parentSheetIndex == 130) ? "axe" : "hammer");
					return true;
				case 163:
					location.objects.Add(placementTile, new Cask(placementTile));
					location.playSound("hammer");
					break;
				case 165:
				{
					Object autoGrabber = new Object(placementTile, 165);
					autoGrabber.heldObject.Value = new Chest();
					location.objects.Add(placementTile, autoGrabber);
					location.playSound("axe");
					return true;
				}
				case 208:
					location.objects.Add(placementTile, new Workbench(placementTile));
					location.playSound("axe");
					return true;
				case 209:
				{
					MiniJukebox mini_jukebox = this as MiniJukebox;
					if (mini_jukebox == null)
					{
						mini_jukebox = new MiniJukebox(placementTile);
					}
					location.objects.Add(placementTile, mini_jukebox);
					mini_jukebox.RegisterToLocation(location);
					location.playSound("hammer");
					return true;
				}
				case 211:
				{
					WoodChipper wood_chipper = this as WoodChipper;
					if (wood_chipper == null)
					{
						wood_chipper = new WoodChipper(placementTile);
					}
					wood_chipper.placementAction(location, x, y);
					location.objects.Add(placementTile, wood_chipper);
					location.playSound("hammer");
					return true;
				}
				case 214:
				{
					Phone phone = this as Phone;
					if (phone == null)
					{
						phone = new Phone(placementTile);
					}
					location.objects.Add(placementTile, phone);
					location.playSound("hammer");
					return true;
				}
				case 216:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					if (!(location is FarmHouse) && !(location is IslandFarmHouse))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					if (location is FarmHouse && (location as FarmHouse).upgradeLevel < 1)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
						return false;
					}
					Chest fridge = new Chest(216, placementTile, 217, 2)
					{
						shakeTimer = 50
					};
					fridge.fridge.Value = true;
					location.objects.Add(placementTile, fridge);
					location.playSound("hammer");
					return true;
				}
				case 143:
				case 144:
				case 145:
				case 146:
				case 147:
				case 148:
				case 149:
				case 150:
				case 151:
				{
					if (location.objects.ContainsKey(placementTile))
					{
						return false;
					}
					Torch torch = new Torch(placementTile, parentSheetIndex, bigCraftable: true);
					torch.shakeTimer = 25;
					torch.placementAction(location, x, y, who);
					return true;
				}
				case 105:
				case 264:
					if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is Tree)
					{
						Tree tree = location.terrainFeatures[placementTile] as Tree;
						if ((int)tree.growthStage >= 5 && !tree.stump && !location.objects.ContainsKey(placementTile))
						{
							Object tapper_instance = (Object)getOne();
							tapper_instance.heldObject.Value = null;
							tapper_instance.tileLocation.Value = placementTile;
							location.objects.Add(placementTile, tapper_instance);
							tree.tapped.Value = true;
							tree.UpdateTapperProduct(tapper_instance);
							location.playSound("axe");
							return true;
						}
					}
					return false;
				case 248:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.MiniShippingBin
					});
					location.playSound("axe");
					return true;
				case 238:
				{
					if (!(location is Farm))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceOnFarm"));
						return false;
					}
					Vector2 obelisk3 = Vector2.Zero;
					Vector2 obelisk2 = Vector2.Zero;
					foreach (KeyValuePair<Vector2, Object> o2 in location.objects.Pairs)
					{
						if ((bool)o2.Value.bigCraftable && o2.Value.ParentSheetIndex == 238)
						{
							if (obelisk3.Equals(Vector2.Zero))
							{
								obelisk3 = o2.Key;
							}
							else if (obelisk2.Equals(Vector2.Zero))
							{
								obelisk2 = o2.Key;
								break;
							}
						}
					}
					if (!obelisk3.Equals(Vector2.Zero) && !obelisk2.Equals(Vector2.Zero))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:OnlyPlaceTwo"));
						return false;
					}
					break;
				}
				case 254:
					if (!(location is AnimalHouse) || !(location as AnimalHouse).name.Contains("Barn"))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MustBePlacedInBarn"));
						return false;
					}
					break;
				case 256:
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					location.objects.Add(placementTile, new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.JunimoChest
					});
					location.playSound("axe");
					return true;
				case 275:
				{
					if (location.objects.ContainsKey(placementTile) || location is MineShaft || location is VolcanoDungeon)
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
						return false;
					}
					Chest chest = new Chest(playerChest: true, placementTile, parentSheetIndex)
					{
						shakeTimer = 50,
						SpecialChestType = Chest.SpecialChestTypes.AutoLoader
					};
					chest.lidFrameCount.Value = 2;
					location.objects.Add(placementTile, chest);
					location.playSound("axe");
					return true;
				}
				}
			}
			if (base.Category == -19 && location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is HoeDirt && (location.terrainFeatures[placementTile] as HoeDirt).crop != null && (base.ParentSheetIndex == 369 || base.ParentSheetIndex == 368) && (int)(location.terrainFeatures[placementTile] as HoeDirt).crop.currentPhase != 0)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
				return false;
			}
			if (isSapling())
			{
				if ((int)parentSheetIndex != 251)
				{
					Vector2 v2 = default(Vector2);
					for (int j = x / 64 - 2; j <= x / 64 + 2; j++)
					{
						for (int i = y / 64 - 2; i <= y / 64 + 2; i++)
						{
							v2.X = j;
							v2.Y = i;
							if (location.terrainFeatures.ContainsKey(v2) && (location.terrainFeatures[v2] is Tree || location.terrainFeatures[v2] is FruitTree))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13060"));
								return false;
							}
						}
					}
					if (FruitTree.IsGrowthBlocked(new Vector2(x / 64, y / 64), location))
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:FruitTree_PlacementWarning", DisplayName));
						return false;
					}
				}
				if (location.terrainFeatures.ContainsKey(placementTile))
				{
					if (!(location.terrainFeatures[placementTile] is HoeDirt) || (location.terrainFeatures[placementTile] as HoeDirt).crop != null)
					{
						return false;
					}
					location.terrainFeatures.Remove(placementTile);
				}
				if ((location is Farm && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Grass") || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Dirt")) && !location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "NoSpawn", "Back").Equals("Tree")) || (location.CanPlantTreesHere(parentSheetIndex, (int)placementTile.X, (int)placementTile.Y) && (location.doesTileHaveProperty((int)placementTile.X, (int)placementTile.Y, "Diggable", "Back") != null || location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone"))))
				{
					location.playSound("dirtyHit");
					DelayedAction.playSoundAfterDelay("coin", 100);
					if ((int)parentSheetIndex == 251)
					{
						location.terrainFeatures.Add(placementTile, new Bush(placementTile, 3, location));
						return true;
					}
					bool actAsGreenhouse = location.IsGreenhouse || (((int)parentSheetIndex == 69 || (int)parentSheetIndex == 835) && location is IslandWest);
					location.terrainFeatures.Add(placementTile, new FruitTree(parentSheetIndex)
					{
						GreenHouseTree = actAsGreenhouse,
						GreenHouseTileTree = location.doesTileHavePropertyNoNull((int)placementTile.X, (int)placementTile.Y, "Type", "Back").Equals("Stone")
					});
					return true;
				}
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13068"));
				return false;
			}
			if (base.Category == -74 || base.Category == -19)
			{
				if (location.terrainFeatures.ContainsKey(placementTile) && location.terrainFeatures[placementTile] is HoeDirt)
				{
					if (((HoeDirt)location.terrainFeatures[placementTile]).canPlantThisSeedHere(who.ActiveObject.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who.ActiveObject.Category == -19))
					{
						if (((HoeDirt)location.terrainFeatures[placementTile]).plant(who.ActiveObject.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, who.ActiveObject.Category == -19, location) && who.IsLocalPlayer)
						{
							if (base.Category == -74)
							{
								foreach (Object o in location.Objects.Values)
								{
									if (o.IsSprinkler() && o.heldObject.Value != null && o.heldObject.Value.ParentSheetIndex == 913 && o.IsInSprinklerRangeBroadphase(placementTile) && o.GetSprinklerTiles().Contains(placementTile))
									{
										Chest chest2 = o.heldObject.Value.heldObject.Value as Chest;
										if (chest2 != null && chest2.items.Count > 0 && chest2.items[0] != null && !chest2.GetMutex().IsLocked())
										{
											chest2.GetMutex().RequestLock(delegate
											{
												if (chest2.items.Count > 0 && chest2.items[0] != null)
												{
													Item item = chest2.items[0];
													if (item.Category == -19 && ((HoeDirt)location.terrainFeatures[placementTile]).plant(item.ParentSheetIndex, (int)placementTile.X, (int)placementTile.Y, who, isFertilizer: true, location))
													{
														item.Stack--;
														if (item.Stack <= 0)
														{
															chest2.items[0] = null;
														}
													}
												}
												chest2.GetMutex().ReleaseLock();
											});
											break;
										}
									}
								}
							}
							Game1.haltAfterCheck = false;
							return true;
						}
						return false;
					}
					return false;
				}
				return false;
			}
			if (!performDropDownAction(who))
			{
				Object toPlace = (Object)getOne();
				bool place_furniture_instance_instead = false;
				if (toPlace.GetType() == typeof(Furniture) && Furniture.GetFurnitureInstance(parentSheetIndex, new Vector2(x / 64, y / 64)).GetType() != toPlace.GetType())
				{
					toPlace = new StorageFurniture(parentSheetIndex, new Vector2(x / 64, y / 64));
					(toPlace as Furniture).currentRotation.Value = (this as Furniture).currentRotation.Value;
					(toPlace as Furniture).updateRotation();
					place_furniture_instance_instead = true;
				}
				toPlace.shakeTimer = 50;
				toPlace.tileLocation.Value = placementTile;
				toPlace.performDropDownAction(who);
				if (toPlace.name.Contains("Seasonal"))
				{
					int baseIndex = toPlace.ParentSheetIndex - toPlace.ParentSheetIndex % 4;
					toPlace.ParentSheetIndex = baseIndex + Utility.getSeasonNumber(Game1.currentSeason);
				}
				if (location.objects.ContainsKey(placementTile))
				{
					if (location.objects[placementTile].ParentSheetIndex != (int)parentSheetIndex)
					{
						Game1.createItemDebris(location.objects[placementTile], placementTile * 64f, Game1.random.Next(4));
						location.objects[placementTile] = toPlace;
					}
				}
				else if (toPlace is Furniture)
				{
					if (place_furniture_instance_instead)
					{
						location.furniture.Add(toPlace as Furniture);
					}
					else
					{
						location.furniture.Add(this as Furniture);
					}
				}
				else
				{
					location.objects.Add(placementTile, toPlace);
				}
				toPlace.initializeLightSource(placementTile);
			}
			location.playSound("woodyStep");
			return true;
		}

		public override bool actionWhenPurchased()
		{
			if (type.Value != null && type.Contains("Blueprint"))
			{
				string blueprintname = name.Substring(name.IndexOf(' ') + 1);
				if (!Game1.player.blueprints.Contains(name))
				{
					Game1.player.blueprints.Add(blueprintname);
				}
				return true;
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(this, 434))
			{
				if (!Game1.isFestival())
				{
					Game1.player.mailReceived.Add("CF_Sewer");
				}
				else
				{
					Game1.player.mailReceived.Add("CF_Fair");
				}
				Game1.exitActiveMenu();
				Game1.player.eatObject(this, overrideFullness: true);
			}
			if (base.actionWhenPurchased())
			{
				return true;
			}
			return isRecipe;
		}

		public override bool canBePlacedInWater()
		{
			return (int)parentSheetIndex == 710;
		}

		public virtual bool needsToBeDonated()
		{
			if (!bigCraftable && type != null && (type.Equals("Minerals") || type.Equals("Arch")))
			{
				return !(Game1.getLocationFromName("ArchaeologyHouse") as LibraryMuseum).museumAlreadyHasArtifact(parentSheetIndex);
			}
			return false;
		}

		public override string getDescription()
		{
			if ((bool)isRecipe)
			{
				if (base.Category == -7)
				{
					return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13073", loadDisplayName());
				}
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13074", loadDisplayName());
			}
			if (needsToBeDonated())
			{
				return Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13078"), Game1.smallFont, getDescriptionWidth());
			}
			if ((bool)bigCraftable && !Game1.bigCraftablesInformation.ContainsKey(parentSheetIndex))
			{
				return "";
			}
			return Game1.parseText(bigCraftable ? Game1.bigCraftablesInformation[parentSheetIndex].Split('/')[4] : (Game1.objectInformation.ContainsKey(parentSheetIndex) ? Game1.objectInformation[parentSheetIndex].Split('/')[5] : "???"), Game1.smallFont, getDescriptionWidth());
		}

		public virtual int sellToStorePrice(long specificPlayerID = -1L)
		{
			if (this is Fence)
			{
				return price;
			}
			if (base.Category == -22)
			{
				return (int)((float)(int)price * (1f + (float)(int)quality * 0.25f) * (((float)(FishingRod.maxTackleUses - uses.Value) + 0f) / (float)FishingRod.maxTackleUses));
			}
			float salePrice2 = (int)((float)(int)price * (1f + (float)Quality * 0.25f));
			salePrice2 = getPriceAfterMultipliers(salePrice2, specificPlayerID);
			if ((int)parentSheetIndex == 493)
			{
				salePrice2 /= 2f;
			}
			if (salePrice2 > 0f)
			{
				salePrice2 = Math.Max(1f, salePrice2 * Game1.MasterPlayer.difficultyModifier);
			}
			return (int)salePrice2;
		}

		public override int salePrice()
		{
			if (this is Fence)
			{
				return price;
			}
			if ((bool)isRecipe)
			{
				return (int)price * 10;
			}
			switch ((int)parentSheetIndex)
			{
			case 388:
				if (Game1.year <= 1)
				{
					return 10;
				}
				return 50;
			case 390:
				if (Game1.year <= 1)
				{
					return 20;
				}
				return 100;
			case 382:
				if (Game1.year <= 1)
				{
					return 120;
				}
				return 250;
			case 378:
				if (Game1.year <= 1)
				{
					return 80;
				}
				return 160;
			case 380:
				if (Game1.year <= 1)
				{
					return 150;
				}
				return 250;
			case 384:
				if (Game1.year <= 1)
				{
					return 350;
				}
				return 750;
			default:
			{
				float salePrice = (int)((float)((int)price * 2) * (1f + (float)(int)quality * 0.25f));
				if ((int)category == -74 || isSapling())
				{
					salePrice = (int)Math.Max(1f, salePrice * Game1.MasterPlayer.difficultyModifier);
				}
				return (int)salePrice;
			}
			}
		}

		private float getPriceAfterMultipliers(float startPrice, long specificPlayerID = -1L)
		{
			bool animalGood = false;
			if (name != null && (name.ToLower().Contains("mayonnaise") || name.ToLower().Contains("cheese") || name.ToLower().Contains("cloth") || name.ToLower().Contains("wool")))
			{
				animalGood = true;
			}
			float saleMultiplier = 1f;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (Game1.player.useSeparateWallets)
				{
					if (specificPlayerID == -1)
					{
						if (player.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || !player.isActive())
						{
							continue;
						}
					}
					else if (player.UniqueMultiplayerID != specificPlayerID)
					{
						continue;
					}
				}
				else if (!player.isActive())
				{
					continue;
				}
				float multiplier = 1f;
				if (player.professions.Contains(0) && (animalGood || base.Category == -5 || base.Category == -6 || base.Category == -18))
				{
					multiplier *= 1.2f;
				}
				if (player.professions.Contains(1) && (base.Category == -75 || base.Category == -80 || (base.Category == -79 && !isSpawnedObject)))
				{
					multiplier *= 1.1f;
				}
				if (player.professions.Contains(4) && base.Category == -26)
				{
					multiplier *= 1.4f;
				}
				if (player.professions.Contains(6) && base.Category == -4)
				{
					multiplier *= (player.professions.Contains(8) ? 1.5f : 1.25f);
				}
				if (player.professions.Contains(12) && (int)parentSheetIndex != 388)
				{
					_ = (int)parentSheetIndex;
					_ = 709;
				}
				if (player.professions.Contains(15) && base.Category == -27)
				{
					multiplier *= 1.25f;
				}
				if (player.professions.Contains(20) && (int)parentSheetIndex >= 334 && (int)parentSheetIndex <= 337)
				{
					multiplier *= 1.5f;
				}
				if (player.professions.Contains(23) && (base.Category == -2 || base.Category == -12))
				{
					multiplier *= 1.3f;
				}
				if (player.eventsSeen.Contains(2120303) && ((int)parentSheetIndex == 296 || (int)parentSheetIndex == 410))
				{
					multiplier *= 3f;
				}
				if (player.eventsSeen.Contains(3910979) && (int)parentSheetIndex == 399)
				{
					multiplier *= 5f;
				}
				saleMultiplier = Math.Max(saleMultiplier, multiplier);
			}
			return startPrice * saleMultiplier;
		}
	}
}
