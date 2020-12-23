using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
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
	public class FarmAnimal : Character
	{
		public const byte eatGrassBehavior = 0;

		public const short newHome = 0;

		public const short happy = 1;

		public const short neutral = 2;

		public const short unhappy = 3;

		public const short hungry = 4;

		public const short disturbedByDog = 5;

		public const short leftOutAtNight = 6;

		public const int hitsTillDead = 3;

		public const double chancePerUpdateToChangeDirection = 0.007;

		public const byte fullnessValueOfGrass = 60;

		public const int noWarpTimerTime = 3000;

		public new const double chanceForSound = 0.002;

		public const double chanceToGoOutside = 0.002;

		public const int uniqueDownFrame = 16;

		public const int uniqueRightFrame = 18;

		public const int uniqueUpFrame = 20;

		public const int uniqueLeftFrame = 22;

		public const int pushAccumulatorTimeTillPush = 40;

		public const int timePerUniqueFrame = 500;

		public const byte layHarvestType = 0;

		public const byte grabHarvestType = 1;

		public NetBool isSwimming = new NetBool();

		[XmlIgnore]
		public Vector2 hopOffset = new Vector2(0f, 0f);

		[XmlElement("defaultProduceIndex")]
		public readonly NetInt defaultProduceIndex = new NetInt();

		[XmlElement("deluxeProduceIndex")]
		public readonly NetInt deluxeProduceIndex = new NetInt();

		[XmlElement("currentProduce")]
		public readonly NetInt currentProduce = new NetInt();

		[XmlElement("friendshipTowardFarmer")]
		public readonly NetInt friendshipTowardFarmer = new NetInt();

		[XmlElement("daysSinceLastFed")]
		public readonly NetInt daysSinceLastFed = new NetInt();

		public int pushAccumulator;

		public int uniqueFrameAccumulator = -1;

		[XmlElement("age")]
		public readonly NetInt age = new NetInt();

		[XmlElement("daysOwned")]
		public readonly NetInt daysOwned = new NetInt(-1);

		[XmlElement("meatIndex")]
		public readonly NetInt meatIndex = new NetInt();

		[XmlElement("health")]
		public readonly NetInt health = new NetInt();

		[XmlElement("price")]
		public readonly NetInt price = new NetInt();

		[XmlElement("produceQuality")]
		public readonly NetInt produceQuality = new NetInt();

		[XmlElement("daysToLay")]
		public readonly NetByte daysToLay = new NetByte();

		[XmlElement("daysSinceLastLay")]
		public readonly NetByte daysSinceLastLay = new NetByte();

		[XmlElement("ageWhenMature")]
		public readonly NetByte ageWhenMature = new NetByte();

		[XmlElement("harvestType")]
		public readonly NetByte harvestType = new NetByte();

		[XmlElement("happiness")]
		public readonly NetByte happiness = new NetByte();

		[XmlElement("fullness")]
		public readonly NetByte fullness = new NetByte();

		[XmlElement("happinessDrain")]
		public readonly NetByte happinessDrain = new NetByte();

		[XmlElement("fullnessDrain")]
		public readonly NetByte fullnessDrain = new NetByte();

		[XmlElement("wasAutoPet")]
		public readonly NetBool wasAutoPet = new NetBool();

		[XmlElement("wasPet")]
		public readonly NetBool wasPet = new NetBool();

		[XmlElement("showDifferentTextureWhenReadyForHarvest")]
		public readonly NetBool showDifferentTextureWhenReadyForHarvest = new NetBool();

		[XmlElement("allowReproduction")]
		public readonly NetBool allowReproduction = new NetBool(value: true);

		[XmlElement("sound")]
		public readonly NetString sound = new NetString();

		[XmlElement("type")]
		public readonly NetString type = new NetString();

		[XmlElement("buildingTypeILiveIn")]
		public readonly NetString buildingTypeILiveIn = new NetString();

		[XmlElement("toolUsedForHarvest")]
		public readonly NetString toolUsedForHarvest = new NetString();

		[XmlElement("frontBackBoundingBox")]
		public readonly NetRectangle frontBackBoundingBox = new NetRectangle();

		[XmlElement("sidewaysBoundingBox")]
		public readonly NetRectangle sidewaysBoundingBox = new NetRectangle();

		[XmlElement("frontBackSourceRect")]
		public readonly NetRectangle frontBackSourceRect = new NetRectangle();

		[XmlElement("sidewaysSourceRect")]
		public readonly NetRectangle sidewaysSourceRect = new NetRectangle();

		[XmlElement("myID")]
		public readonly NetLong myID = new NetLong();

		[XmlElement("ownerID")]
		public readonly NetLong ownerID = new NetLong();

		[XmlElement("parentId")]
		public readonly NetLong parentId = new NetLong(-1L);

		[XmlIgnore]
		private readonly NetBuildingRef netHome = new NetBuildingRef();

		[XmlElement("homeLocation")]
		public readonly NetVector2 homeLocation = new NetVector2();

		[XmlIgnore]
		public int noWarpTimer;

		[XmlIgnore]
		public int hitGlowTimer;

		[XmlIgnore]
		public int pauseTimer;

		[XmlElement("moodMessage")]
		public readonly NetInt moodMessage = new NetInt();

		[XmlElement("isEating")]
		public readonly NetBool isEating = new NetBool();

		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> doFarmerPushEvent = new NetEvent1Field<int, NetInt>();

		[XmlIgnore]
		private readonly NetEvent0 doBuildingPokeEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 doDiveEvent = new NetEvent0();

		private string _displayHouse;

		private string _displayType;

		public static int NumPathfindingThisTick = 0;

		public static int MaxPathfindingPerTick = 1;

		[XmlIgnore]
		public int nextRipple;

		[XmlIgnore]
		public int nextFollowDirectionChange;

		protected FarmAnimal _followTarget;

		protected Point? _followTargetPosition;

		protected float _nextFollowTargetScan = 1f;

		[XmlIgnore]
		public int bobOffset;

		[XmlIgnore]
		protected Vector2 _swimmingVelocity = Vector2.Zero;

		[XmlIgnore]
		public static HashSet<Grass> reservedGrass = new HashSet<Grass>();

		[XmlIgnore]
		public Grass foundGrass;

		[XmlIgnore]
		public Building home
		{
			get
			{
				return netHome.Value;
			}
			set
			{
				netHome.Value = value;
			}
		}

		[XmlIgnore]
		public string displayHouse
		{
			get
			{
				if (_displayHouse == null)
				{
					Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue(type, out string rawData);
					_displayHouse = buildingTypeILiveIn;
					if (rawData != null && LocalizedContentManager.CurrentLanguageCode != 0)
					{
						_displayHouse = rawData.Split('/')[26];
					}
				}
				return _displayHouse;
			}
			set
			{
				_displayHouse = value;
			}
		}

		[XmlIgnore]
		public string displayType
		{
			get
			{
				if (_displayType == null)
				{
					Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue(type, out string rawData);
					if (rawData != null)
					{
						_displayType = rawData.Split('/')[25];
					}
				}
				return _displayType;
			}
			set
			{
				_displayType = value;
			}
		}

		public FarmAnimal()
		{
		}

		protected override void initNetFields()
		{
			bobOffset = Game1.random.Next(0, 1000);
			base.initNetFields();
			base.NetFields.AddFields(defaultProduceIndex, deluxeProduceIndex, currentProduce, friendshipTowardFarmer, daysSinceLastFed, age, meatIndex, health, price, produceQuality, daysToLay, daysSinceLastLay, ageWhenMature, harvestType, happiness, fullness, happinessDrain, fullnessDrain, wasPet, wasAutoPet, showDifferentTextureWhenReadyForHarvest, allowReproduction, sound, type, buildingTypeILiveIn, toolUsedForHarvest, frontBackBoundingBox, sidewaysBoundingBox, frontBackSourceRect, sidewaysSourceRect, myID, ownerID, parentId, netHome.NetFields, homeLocation, moodMessage, isEating, doFarmerPushEvent, doBuildingPokeEvent, isSwimming, doDiveEvent.NetFields, daysOwned);
			position.Field.AxisAlignedMovement = true;
			doFarmerPushEvent.onEvent += doFarmerPush;
			doBuildingPokeEvent.onEvent += doBuildingPoke;
			doDiveEvent.onEvent += doDive;
			isSwimming.fieldChangeVisibleEvent += delegate
			{
				if (isSwimming.Value)
				{
					position.Field.AxisAlignedMovement = false;
				}
				else
				{
					position.Field.AxisAlignedMovement = true;
				}
			};
		}

		public FarmAnimal(string type, long id, long ownerID)
			: base(null, new Vector2(64 * Game1.random.Next(2, 9), 64 * Game1.random.Next(4, 8)), 2, type)
		{
			this.ownerID.Value = ownerID;
			health.Value = 3;
			if (type.Contains("Chicken") && !type.Equals("Void Chicken") && !type.Equals("Golden Chicken"))
			{
				type = ((Game1.random.NextDouble() < 0.5 || type.Contains("Brown")) ? "Brown Chicken" : "White Chicken");
				if (Game1.player.eventsSeen.Contains(3900074) && Game1.random.NextDouble() < 0.25)
				{
					type = "Blue Chicken";
				}
			}
			if (type.Contains("Cow"))
			{
				type = ((!type.Contains("White") && (Game1.random.NextDouble() < 0.5 || type.Contains("Brown"))) ? "Brown Cow" : "White Cow");
			}
			myID.Value = id;
			this.type.Value = type;
			base.Name = Dialogue.randomName();
			base.displayName = name;
			happiness.Value = byte.MaxValue;
			fullness.Value = byte.MaxValue;
			_nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
			reloadData();
		}

		public virtual void reloadData()
		{
			Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals").TryGetValue(type.Value, out string rawData);
			if (rawData != null)
			{
				string[] split = rawData.Split('/');
				daysToLay.Value = Convert.ToByte(split[0]);
				ageWhenMature.Value = Convert.ToByte(split[1]);
				defaultProduceIndex.Value = Convert.ToInt32(split[2]);
				deluxeProduceIndex.Value = Convert.ToInt32(split[3]);
				sound.Value = (split[4].Equals("none") ? null : split[4]);
				frontBackBoundingBox.Value = new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(split[5]), Convert.ToInt32(split[6]), Convert.ToInt32(split[7]), Convert.ToInt32(split[8]));
				sidewaysBoundingBox.Value = new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(split[9]), Convert.ToInt32(split[10]), Convert.ToInt32(split[11]), Convert.ToInt32(split[12]));
				harvestType.Value = Convert.ToByte(split[13]);
				showDifferentTextureWhenReadyForHarvest.Value = Convert.ToBoolean(split[14]);
				buildingTypeILiveIn.Value = split[15];
				int sourceWidth = Convert.ToInt32(split[16]);
				string textureName = type;
				if ((int)age < (byte)ageWhenMature)
				{
					textureName = "Baby" + (type.Value.Equals("Duck") ? "White Chicken" : type.Value);
				}
				else if ((bool)showDifferentTextureWhenReadyForHarvest && (int)currentProduce <= 0)
				{
					textureName = "Sheared" + type.Value;
				}
				Sprite = new AnimatedSprite("Animals\\" + textureName, 0, sourceWidth, Convert.ToInt32(split[17]));
				frontBackSourceRect.Value = new Microsoft.Xna.Framework.Rectangle(0, 0, Convert.ToInt32(split[16]), Convert.ToInt32(split[17]));
				sidewaysSourceRect.Value = new Microsoft.Xna.Framework.Rectangle(0, 0, Convert.ToInt32(split[18]), Convert.ToInt32(split[19]));
				fullnessDrain.Value = Convert.ToByte(split[20]);
				happinessDrain.Value = Convert.ToByte(split[21]);
				toolUsedForHarvest.Value = ((split[22].Length > 0) ? split[22] : "");
				meatIndex.Value = Convert.ToInt32(split[23]);
				price.Value = Convert.ToInt32(split[24]);
				if (!isCoopDweller())
				{
					Sprite.textureUsesFlippedRightForLeft = true;
				}
			}
		}

		public string shortDisplayType()
		{
			switch (LocalizedContentManager.CurrentLanguageCode)
			{
			case LocalizedContentManager.LanguageCode.en:
				return displayType.Split(' ').Last();
			case LocalizedContentManager.LanguageCode.ja:
				if (!displayType.Contains("トリ"))
				{
					if (!displayType.Contains("ウシ"))
					{
						if (!displayType.Contains("ブタ"))
						{
							return displayType;
						}
						return "ブタ";
					}
					return "ウシ";
				}
				return "トリ";
			case LocalizedContentManager.LanguageCode.ru:
				if (!displayType.ToLower().Contains("курица"))
				{
					if (!displayType.ToLower().Contains("корова"))
					{
						return displayType;
					}
					return "Корова";
				}
				return "Курица";
			case LocalizedContentManager.LanguageCode.zh:
				if (!displayType.Contains("鸡"))
				{
					if (!displayType.Contains("牛"))
					{
						if (!displayType.Contains("猪"))
						{
							return displayType;
						}
						return "猪";
					}
					return "牛";
				}
				return "鸡";
			case LocalizedContentManager.LanguageCode.pt:
			case LocalizedContentManager.LanguageCode.es:
				return displayType.Split(' ').First();
			case LocalizedContentManager.LanguageCode.de:
				return displayType.Split(' ').Last().Split('-')
					.Last();
			default:
				return displayType;
			}
		}

		public bool isCoopDweller()
		{
			if (home != null)
			{
				return home is Coop;
			}
			return false;
		}

		public Microsoft.Xna.Framework.Rectangle GetHarvestBoundingBox()
		{
			Vector2 position = base.Position;
			return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
		}

		public Microsoft.Xna.Framework.Rectangle GetCursorPetBoundingBox()
		{
			Vector2 position = base.Position;
			if (type.Contains("Chicken"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y - 16, 64, 68);
			}
			if (type.Contains("Cow"))
			{
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 24f + 8f), (int)position.Y, 68, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 4f), (int)(position.Y + 24f - 8f), 112, 80);
			}
			if (type.Contains("Pig"))
			{
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 24f), (int)position.Y, 82, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 4f), (int)(position.Y + 24f), 116, 72);
			}
			if (type.Contains("Duck"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)(position.Y - 8f), 64, 60);
			}
			if (type.Contains("Rabbit"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)(position.Y - 8f), 56, 56);
			}
			if (type.Contains("Dinosaur"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, 56, 52);
			}
			if (type.Contains("Sheep"))
			{
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 24f + 8f), (int)position.Y, 72, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 4f), (int)(position.Y + 24f), 112, 72);
			}
			if (type.Contains("Goat"))
			{
				if (FacingDirection == 0 || FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 40f) - 8, (int)position.Y - 4, 64, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(position.X + 4f), (int)(position.Y + 24f) - 4, 112, 80);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
		}

		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 8f), (int)(position.Y + (float)(Sprite.getHeight() * 4) - 64f + 8f), 48, 48);
		}

		public void reload(Building home)
		{
			this.home = home;
			reloadData();
		}

		public int GetDaysOwned()
		{
			if (daysOwned.Value < 0)
			{
				daysOwned.Value = age.Value;
			}
			return daysOwned.Value;
		}

		public void pet(Farmer who, bool is_auto_pet = false)
		{
			if (!is_auto_pet)
			{
				if (who.FarmerSprite.PauseForSingleAnimation)
				{
					return;
				}
				who.Halt();
				who.faceGeneralDirection(base.Position, 0, opposite: false, useTileCalculations: false);
				if (Game1.timeOfDay >= 1900 && !isMoving())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\FarmAnimals:TryingToSleep", base.displayName));
					return;
				}
				Halt();
				Sprite.StopAnimation();
				uniqueFrameAccumulator = -1;
				switch (Game1.player.FacingDirection)
				{
				case 0:
					Sprite.currentFrame = 0;
					break;
				case 1:
					Sprite.currentFrame = 12;
					break;
				case 2:
					Sprite.currentFrame = 8;
					break;
				case 3:
					Sprite.currentFrame = 4;
					break;
				}
			}
			else if (wasAutoPet.Value)
			{
				return;
			}
			if (!wasPet)
			{
				if (!is_auto_pet)
				{
					wasPet.Value = true;
				}
				int auto_pet_reduction = 7;
				if (wasAutoPet.Value)
				{
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + auto_pet_reduction);
				}
				else if (is_auto_pet)
				{
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + (15 - auto_pet_reduction));
				}
				else
				{
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
				}
				if (is_auto_pet)
				{
					wasAutoPet.Value = true;
				}
				if (!is_auto_pet)
				{
					if ((who.professions.Contains(3) && !isCoopDweller()) || (who.professions.Contains(2) && isCoopDweller()))
					{
						friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
						happiness.Value = (byte)Math.Min(255, (byte)happiness + Math.Max(5, 40 - (byte)happinessDrain));
					}
					int emote_index = 20;
					if (wasAutoPet.Value)
					{
						emote_index = 32;
					}
					doEmote(((int)moodMessage == 4) ? 12 : emote_index);
				}
				happiness.Value = (byte)Math.Min(255, (byte)happiness + Math.Max(5, 40 - (byte)happinessDrain));
				if (!is_auto_pet)
				{
					makeSound();
					who.gainExperience(0, 5);
				}
			}
			else if (!is_auto_pet && (who.ActiveObject == null || (int)who.ActiveObject.parentSheetIndex != 178))
			{
				Game1.activeClickableMenu = new AnimalQueryMenu(this);
			}
			if (type.Value.Equals("Sheep") && (int)friendshipTowardFarmer >= 900)
			{
				daysToLay.Value = 2;
			}
		}

		public void farmerPushing()
		{
			pushAccumulator++;
			if (pushAccumulator > 40)
			{
				doFarmerPushEvent.Fire(Game1.player.FacingDirection);
				Microsoft.Xna.Framework.Rectangle bounds2 = GetBoundingBox();
				bounds2 = Utility.ExpandRectangle(bounds2, Utility.GetOppositeFacingDirection(Game1.player.FacingDirection), 6);
				Game1.player.TemporaryPassableTiles.Add(bounds2);
				pushAccumulator = 0;
			}
		}

		public virtual void doDive()
		{
			yJumpVelocity = 8f;
			yJumpOffset = 1;
		}

		private void doFarmerPush(int direction)
		{
			if (Game1.IsMasterGame)
			{
				switch (direction)
				{
				case 0:
					Halt();
					SetMovingUp(b: true);
					break;
				case 1:
					Halt();
					SetMovingRight(b: true);
					break;
				case 2:
					Halt();
					SetMovingDown(b: true);
					break;
				case 3:
					Halt();
					SetMovingLeft(b: true);
					break;
				}
			}
		}

		public void Poke()
		{
			doBuildingPokeEvent.Fire();
		}

		private void doBuildingPoke()
		{
			if (Game1.IsMasterGame)
			{
				FacingDirection = Game1.random.Next(4);
				setMovingInFacingDirection();
			}
		}

		public void setRandomPosition(GameLocation location)
		{
			StopAllActions();
			string[] array = location.getMapProperty("ProduceArea").Split(' ');
			int produceX = Convert.ToInt32(array[0]);
			int produceY = Convert.ToInt32(array[1]);
			int produceWidth = Convert.ToInt32(array[2]);
			int produceHeight = Convert.ToInt32(array[3]);
			base.Position = new Vector2(Game1.random.Next(produceX, produceX + produceWidth) * 64, Game1.random.Next(produceY, produceY + produceHeight) * 64);
			int tries = 0;
			while (base.Position.Equals(Vector2.Zero) || location.Objects.ContainsKey(base.Position) || location.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				base.Position = new Vector2(Game1.random.Next(produceX, produceX + produceWidth), Game1.random.Next(produceY, produceY + produceHeight)) * 64f;
				tries++;
				if (tries > 64)
				{
					break;
				}
			}
			SleepIfNecessary();
		}

		public virtual void StopAllActions()
		{
			foundGrass = null;
			controller = null;
			isSwimming.Value = false;
			hopOffset = Vector2.Zero;
			_followTarget = null;
			_followTargetPosition = null;
			Halt();
			Sprite.StopAnimation();
			Sprite.UpdateSourceRect();
		}

		public void dayUpdate(GameLocation environtment)
		{
			if (daysOwned.Value < 0)
			{
				daysOwned.Value = age.Value;
			}
			StopAllActions();
			health.Value = 3;
			bool wasLeftOutLastNight = false;
			if (home != null && !(home.indoors.Value as AnimalHouse).animals.ContainsKey(myID) && environtment is Farm)
			{
				if ((bool)home.animalDoorOpen)
				{
					(environtment as Farm).animals.Remove(myID);
					(home.indoors.Value as AnimalHouse).animals.Add(myID, this);
					if (Game1.timeOfDay > 1800 && controller == null)
					{
						happiness.Value /= 2;
					}
					environtment = home.indoors;
					setRandomPosition(environtment);
					return;
				}
				moodMessage.Value = 6;
				wasLeftOutLastNight = true;
				happiness.Value /= 2;
			}
			daysSinceLastLay.Value++;
			if (!wasPet.Value && !wasAutoPet.Value)
			{
				friendshipTowardFarmer.Value = Math.Max(0, (int)friendshipTowardFarmer - (10 - (int)friendshipTowardFarmer / 200));
				happiness.Value = (byte)Math.Max(0, (byte)happiness - (byte)happinessDrain * 5);
			}
			wasPet.Value = false;
			wasAutoPet.Value = false;
			daysOwned.Value++;
			if ((byte)fullness < 200 && environtment is AnimalHouse)
			{
				for (int i = environtment.objects.Count() - 1; i >= 0; i--)
				{
					if (environtment.objects.Pairs.ElementAt(i).Value.Name.Equals("Hay"))
					{
						environtment.objects.Remove(environtment.objects.Pairs.ElementAt(i).Key);
						fullness.Value = byte.MaxValue;
						break;
					}
				}
			}
			Random r = new Random((int)(long)myID / 2 + (int)Game1.stats.DaysPlayed);
			if ((byte)fullness > 200 || r.NextDouble() < (double)((byte)fullness - 30) / 170.0)
			{
				age.Value++;
				if ((int)age == (byte)ageWhenMature)
				{
					Sprite.LoadTexture("Animals\\" + type.Value);
					if (type.Value.Contains("Sheep"))
					{
						currentProduce.Value = defaultProduceIndex;
					}
					daysSinceLastLay.Value = 99;
				}
				happiness.Value = (byte)Math.Min(255, (byte)happiness + (byte)happinessDrain * 2);
			}
			if (fullness.Value < 200)
			{
				happiness.Value = (byte)Math.Max(0, (byte)happiness - 100);
				friendshipTowardFarmer.Value = Math.Max(0, (int)friendshipTowardFarmer - 20);
			}
			bool produceToday = (byte)daysSinceLastLay >= (byte)daysToLay - ((type.Value.Equals("Sheep") && Game1.getFarmer(ownerID).professions.Contains(3)) ? 1 : 0) && r.NextDouble() < (double)(int)(byte)fullness / 200.0 && r.NextDouble() < (double)(int)(byte)happiness / 70.0;
			int whichProduce;
			if (!produceToday || (int)age < (byte)ageWhenMature)
			{
				whichProduce = -1;
			}
			else
			{
				whichProduce = defaultProduceIndex;
				if (r.NextDouble() < (double)(int)(byte)happiness / 150.0)
				{
					float happinessModifier = ((byte)happiness > 200) ? ((float)(int)(byte)happiness * 1.5f) : ((float)(((byte)happiness <= 100) ? ((byte)happiness - 100) : 0));
					if (type.Value.Equals("Duck") && r.NextDouble() < (double)((float)(int)friendshipTowardFarmer + happinessModifier) / 4750.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.01)
					{
						whichProduce = deluxeProduceIndex;
					}
					else if (type.Value.Equals("Rabbit") && r.NextDouble() < (double)((float)(int)friendshipTowardFarmer + happinessModifier) / 5000.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.02)
					{
						whichProduce = deluxeProduceIndex;
					}
					daysSinceLastLay.Value = 0;
					switch (whichProduce)
					{
					case 176:
						Game1.stats.ChickenEggsLayed++;
						break;
					case 180:
						Game1.stats.ChickenEggsLayed++;
						break;
					case 442:
						Game1.stats.DuckEggsLayed++;
						break;
					case 440:
						Game1.stats.RabbitWoolProduced++;
						break;
					}
					if (r.NextDouble() < (double)((float)(int)friendshipTowardFarmer + happinessModifier) / 1200.0 && !type.Value.Equals("Duck") && !type.Value.Equals("Rabbit") && (int)deluxeProduceIndex != -1 && (int)friendshipTowardFarmer >= 200)
					{
						whichProduce = deluxeProduceIndex;
					}
					double chanceForQuality = (float)(int)friendshipTowardFarmer / 1000f - (1f - (float)(int)(byte)happiness / 225f);
					if ((!isCoopDweller() && Game1.getFarmer(ownerID).professions.Contains(3)) || (isCoopDweller() && Game1.getFarmer(ownerID).professions.Contains(2)))
					{
						chanceForQuality += 0.33;
					}
					if (chanceForQuality >= 0.95 && r.NextDouble() < chanceForQuality / 2.0)
					{
						produceQuality.Value = 4;
					}
					else if (r.NextDouble() < chanceForQuality / 2.0)
					{
						produceQuality.Value = 2;
					}
					else if (r.NextDouble() < chanceForQuality)
					{
						produceQuality.Value = 1;
					}
					else
					{
						produceQuality.Value = 0;
					}
				}
			}
			if ((byte)harvestType == 1 && produceToday)
			{
				currentProduce.Value = whichProduce;
				whichProduce = -1;
			}
			if (whichProduce != -1 && home != null)
			{
				bool spawn_object = true;
				foreach (Object location_object in home.indoors.Value.objects.Values)
				{
					if ((bool)location_object.bigCraftable && (int)location_object.parentSheetIndex == 165 && location_object.heldObject.Value != null && (location_object.heldObject.Value as Chest).addItem(new Object(Vector2.Zero, whichProduce, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false)
					{
						Quality = produceQuality
					}) == null)
					{
						location_object.showNextIndex.Value = true;
						spawn_object = false;
						break;
					}
				}
				if (spawn_object && !home.indoors.Value.Objects.ContainsKey(getTileLocation()))
				{
					home.indoors.Value.Objects.Add(getTileLocation(), new Object(Vector2.Zero, whichProduce, null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: true)
					{
						Quality = produceQuality
					});
				}
			}
			if (!wasLeftOutLastNight)
			{
				if ((byte)fullness < 30)
				{
					moodMessage.Value = 4;
				}
				else if ((byte)happiness < 30)
				{
					moodMessage.Value = 3;
				}
				else if ((byte)happiness < 200)
				{
					moodMessage.Value = 2;
				}
				else
				{
					moodMessage.Value = 1;
				}
			}
			if (Game1.timeOfDay < 1700)
			{
				fullness.Value = (byte)Math.Max(0, (byte)fullness - (byte)fullnessDrain * (1700 - Game1.timeOfDay) / 100);
			}
			fullness.Value = 0;
			if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason))
			{
				fullness.Value = 250;
			}
			reload(home);
		}

		public int getSellPrice()
		{
			double adjustedFriendship = (double)(int)friendshipTowardFarmer / 1000.0 + 0.3;
			return (int)((double)(int)price * adjustedFriendship);
		}

		public bool isMale()
		{
			switch (type.Value)
			{
			case "Rabbit":
				return (long)myID % 2 == 0;
			case "Truffle Pig":
			case "Hog":
			case "Pig":
				return (long)myID % 2 == 0;
			default:
				return false;
			}
		}

		public string getMoodMessage()
		{
			if ((byte)harvestType == 2)
			{
				base.Name = "It";
			}
			string gender = isMale() ? "Male" : "Female";
			switch (moodMessage.Value)
			{
			case 0:
				if ((long)parentId != -1)
				{
					return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Baby_" + gender, base.displayName);
				}
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Adult_" + gender + "_" + (Game1.dayOfMonth % 2 + 1), base.displayName);
			case 6:
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_LeftOutsideAtNight_" + gender, base.displayName);
			case 5:
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_DisturbedByDog_" + gender, base.displayName);
			case 4:
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_" + (((Game1.dayOfMonth + (long)myID) % 2 == 0L) ? "Hungry1" : "Hungry2"), base.displayName);
			default:
				if ((byte)happiness < 30)
				{
					moodMessage.Value = 3;
				}
				else if ((byte)happiness < 200)
				{
					moodMessage.Value = 2;
				}
				else
				{
					moodMessage.Value = 1;
				}
				switch ((int)moodMessage)
				{
				case 3:
					return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Sad", base.displayName);
				case 2:
					return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Fine", base.displayName);
				case 1:
					return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Happy", base.displayName);
				default:
					return "";
				}
			}
		}

		public bool isBaby()
		{
			return (int)age < (byte)ageWhenMature;
		}

		public void warpHome(Farm f, FarmAnimal a)
		{
			if (home != null)
			{
				(home.indoors.Value as AnimalHouse).animals.Add(myID, this);
				f.animals.Remove(myID);
				controller = null;
				setRandomPosition(home.indoors);
				home.currentOccupants.Value++;
				isSwimming.Value = false;
				hopOffset = Vector2.Zero;
				_followTarget = null;
				_followTargetPosition = null;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (isCoopDweller())
			{
				if (IsActuallySwimming())
				{
					Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f)), isBaby() ? 2.5f : 3.5f, 0.5f);
				}
				else
				{
					Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f)), isBaby() ? 3f : 4f);
				}
			}
			Vector2 offset = default(Vector2);
			int bob_amount = 0;
			if (IsActuallySwimming())
			{
				bob_amount = (int)((Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)bobOffset) + 0.5) * 3.0);
				offset.Y += bob_amount;
			}
			offset.Y += yJumpOffset;
			float layer_depth = ((float)(GetBoundingBox().Center.Y + 4) + base.Position.X / 20000f) / 10000f;
			Sprite.draw(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f) + offset)), layer_depth, 0, 0, (hitGlowTimer > 0) ? Color.Red : Color.White, FacingDirection == 3, 4f);
			if (isEmoting)
			{
				Vector2 emotePosition = Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(frontBackSourceRect.Width / 2 * 4 - 32, isCoopDweller() ? (-96) : (-64)) + offset);
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)GetBoundingBox().Bottom / 10000f);
			}
		}

		public virtual void updateWhenNotCurrentLocation(Building currentBuilding, GameTime time, GameLocation environment)
		{
			doFarmerPushEvent.Poll();
			doBuildingPokeEvent.Poll();
			doDiveEvent.Poll();
			if (!Game1.shouldTimePass())
			{
				return;
			}
			update(time, environment, myID, move: false);
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (hopOffset != Vector2.Zero)
			{
				HandleHop();
				return;
			}
			if (currentBuilding != null && Game1.random.NextDouble() < 0.002 && (bool)currentBuilding.animalDoorOpen && Game1.timeOfDay < 1630 && !Game1.isRaining && !Game1.currentSeason.Equals("winter") && !environment.farmers.Any())
			{
				Farm farm = (Farm)Game1.getLocationFromName("Farm");
				if (farm.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(((int)currentBuilding.tileX + currentBuilding.animalDoor.X) * 64 + 2, ((int)currentBuilding.tileY + currentBuilding.animalDoor.Y) * 64 + 2, (isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false) || farm.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(((int)currentBuilding.tileX + currentBuilding.animalDoor.X) * 64 + 2, ((int)currentBuilding.tileY + currentBuilding.animalDoor.Y + 1) * 64 + 2, (isCoopDweller() ? 64 : 128) - 4, 60), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
				{
					return;
				}
				if (farm.animals.ContainsKey(myID))
				{
					for (int i = farm.animals.Count() - 1; i >= 0; i--)
					{
						if (farm.animals.Pairs.ElementAt(i).Key.Equals(myID))
						{
							farm.animals.Remove(myID);
							break;
						}
					}
				}
				(currentBuilding.indoors.Value as AnimalHouse).animals.Remove(myID);
				farm.animals.Add(myID, this);
				faceDirection(2);
				SetMovingDown(b: true);
				base.Position = new Vector2(currentBuilding.getRectForAnimalDoor().X, ((int)currentBuilding.tileY + currentBuilding.animalDoor.Y) * 64 - (Sprite.getHeight() * 4 - GetBoundingBox().Height) + 32);
				if (NumPathfindingThisTick < MaxPathfindingPerTick)
				{
					NumPathfindingThisTick++;
					controller = new PathFindController(this, farm, grassEndPointFunction, Game1.random.Next(4), eraseOldPathController: false, behaviorAfterFindingGrassPatch, 200, Point.Zero);
				}
				if (controller == null || controller.pathToEndPoint == null || controller.pathToEndPoint.Count < 3)
				{
					SetMovingDown(b: true);
					controller = null;
				}
				else
				{
					faceDirection(2);
					base.Position = new Vector2(controller.pathToEndPoint.Peek().X * 64, controller.pathToEndPoint.Peek().Y * 64 - (Sprite.getHeight() * 4 - GetBoundingBox().Height) + 16);
					if (!isCoopDweller())
					{
						position.X -= 32f;
					}
				}
				noWarpTimer = 3000;
				currentBuilding.currentOccupants.Value--;
				if (Utility.isOnScreen(getTileLocationPoint(), 192, farm))
				{
					farm.localSound("sandyStep");
				}
				if (environment.isTileOccupiedByFarmer(getTileLocation()) != null)
				{
					environment.isTileOccupiedByFarmer(getTileLocation()).TemporaryPassableTiles.Add(GetBoundingBox());
				}
			}
			UpdateRandomMovements();
			behaviors(time, environment);
		}

		public static void behaviorAfterFindingGrassPatch(Character c, GameLocation environment)
		{
			Vector2 tilePosition = new Vector2(c.GetBoundingBox().Center.X / 64, c.GetBoundingBox().Center.Y / 64);
			if (environment.terrainFeatures.ContainsKey(tilePosition))
			{
				TerrainFeature feature = environment.terrainFeatures[tilePosition];
				if (feature is Grass && reservedGrass.Contains(feature as Grass))
				{
					reservedGrass.Remove(feature as Grass);
				}
			}
			if ((byte)((FarmAnimal)c).fullness < byte.MaxValue)
			{
				((FarmAnimal)c).eatGrass(environment);
			}
		}

		public static bool animalDoorEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
		{
			Vector2 tileLocation = new Vector2(currentPoint.x, currentPoint.y);
			foreach (Building building in ((Farm)location).buildings)
			{
				if (building.animalDoor.X >= 0 && (float)(building.animalDoor.X + (int)building.tileX) == tileLocation.X && (float)(building.animalDoor.Y + (int)building.tileY) == tileLocation.Y && building.buildingType.Value.Contains(((FarmAnimal)c).buildingTypeILiveIn) && (int)building.currentOccupants < (int)building.maxOccupants)
				{
					building.currentOccupants.Value++;
					location.playSound("dwop");
					return true;
				}
			}
			return false;
		}

		public static bool grassEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
		{
			Vector2 tileLocation = new Vector2(currentPoint.x, currentPoint.y);
			if (location.terrainFeatures.TryGetValue(tileLocation, out TerrainFeature t) && t is Grass)
			{
				if (reservedGrass.Contains(t))
				{
					return false;
				}
				reservedGrass.Add(t as Grass);
				if (c is FarmAnimal)
				{
					(c as FarmAnimal).foundGrass = (t as Grass);
				}
				return true;
			}
			return false;
		}

		public virtual void updatePerTenMinutes(int timeOfDay, GameLocation environment)
		{
			if (timeOfDay >= 1800)
			{
				if ((environment.IsOutdoors && timeOfDay > 1900) || (!environment.IsOutdoors && (byte)happiness > 150 && Game1.currentSeason.Equals("winter")) || ((bool)environment.isOutdoors && Game1.isRaining) || ((bool)environment.isOutdoors && Game1.currentSeason.Equals("winter")))
				{
					happiness.Value = (byte)Math.Min(255, Math.Max(0, (byte)happiness - ((environment.numberOfObjectsWithName("Heater") > 0 && Game1.currentSeason.Equals("winter")) ? (-(byte)happinessDrain) : ((byte)happinessDrain))));
				}
				else if (environment.IsOutdoors)
				{
					happiness.Value = (byte)Math.Min(255, (byte)happiness + (byte)happinessDrain);
				}
			}
			if (environment.isTileOccupiedByFarmer(getTileLocation()) != null)
			{
				environment.isTileOccupiedByFarmer(getTileLocation()).TemporaryPassableTiles.Add(GetBoundingBox());
			}
		}

		public void eatGrass(GameLocation environment)
		{
			Vector2 tilePosition = new Vector2(GetBoundingBox().Center.X / 64, GetBoundingBox().Center.Y / 64);
			if (environment.terrainFeatures.ContainsKey(tilePosition) && environment.terrainFeatures[tilePosition] is Grass)
			{
				TerrainFeature feature = environment.terrainFeatures[tilePosition];
				if (reservedGrass.Contains(feature as Grass))
				{
					reservedGrass.Remove(feature as Grass);
				}
				if (foundGrass != null && reservedGrass.Contains(foundGrass))
				{
					reservedGrass.Remove(foundGrass);
				}
				foundGrass = null;
				Eat(environment);
			}
		}

		public virtual void Eat(GameLocation location)
		{
			Vector2 tilePosition = new Vector2(GetBoundingBox().Center.X / 64, GetBoundingBox().Center.Y / 64);
			isEating.Value = true;
			if (location.terrainFeatures.ContainsKey(tilePosition) && location.terrainFeatures[tilePosition] is Grass && ((Grass)location.terrainFeatures[tilePosition]).reduceBy(isCoopDweller() ? 2 : 4, tilePosition, location.Equals(Game1.currentLocation)))
			{
				location.terrainFeatures.Remove(tilePosition);
			}
			Sprite.loop = false;
			fullness.Value = byte.MaxValue;
			if ((int)moodMessage != 5 && (int)moodMessage != 6 && !Game1.isRaining)
			{
				happiness.Value = byte.MaxValue;
				friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 8);
			}
		}

		public override void performBehavior(byte which)
		{
			if (which == 0)
			{
				eatGrass(Game1.currentLocation);
			}
		}

		private bool behaviors(GameTime time, GameLocation location)
		{
			Building home = this.home;
			if (home == null)
			{
				return false;
			}
			if (Game1.IsMasterGame && isBaby() && CanFollowAdult())
			{
				_nextFollowTargetScan -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_nextFollowTargetScan < 0f)
				{
					_nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
					if (controller != null || !(location is Farm))
					{
						_followTarget = null;
						_followTargetPosition = null;
					}
					else
					{
						if (_followTarget != null)
						{
							if (!GetFollowRange(_followTarget).Contains(_followTargetPosition.Value))
							{
								GetNewFollowPosition();
							}
							return false;
						}
						if (location is Farm)
						{
							foreach (FarmAnimal animal in (location as Farm).animals.Values)
							{
								if (!animal.isBaby() && animal.type.Value == type.Value && GetFollowRange(animal, 4).Contains(Utility.Vector2ToPoint(getStandingPosition())))
								{
									_followTarget = animal;
									GetNewFollowPosition();
									return false;
								}
							}
						}
					}
				}
			}
			if ((bool)isEating)
			{
				if (home != null && home.getRectForAnimalDoor().Intersects(GetBoundingBox()))
				{
					behaviorAfterFindingGrassPatch(this, location);
					isEating.Value = false;
					Halt();
					return false;
				}
				if (buildingTypeILiveIn.Contains("Barn"))
				{
					Sprite.Animate(time, 16, 4, 100f);
					if (Sprite.currentFrame >= 20)
					{
						isEating.Value = false;
						Sprite.loop = true;
						Sprite.currentFrame = 0;
						faceDirection(2);
					}
				}
				else
				{
					Sprite.Animate(time, 24, 4, 100f);
					if (Sprite.currentFrame >= 28)
					{
						isEating.Value = false;
						Sprite.loop = true;
						Sprite.currentFrame = 0;
						faceDirection(2);
					}
				}
				return true;
			}
			if (!Game1.IsClient)
			{
				if (controller != null)
				{
					return true;
				}
				if (!isSwimming.Value && location.IsOutdoors && (byte)fullness < 195 && Game1.random.NextDouble() < 0.002 && NumPathfindingThisTick < MaxPathfindingPerTick)
				{
					NumPathfindingThisTick++;
					controller = new PathFindController(this, location, grassEndPointFunction, -1, eraseOldPathController: false, behaviorAfterFindingGrassPatch, 200, Point.Zero);
					_followTarget = null;
					_followTargetPosition = null;
				}
				if (Game1.timeOfDay >= 1700 && location.IsOutdoors && controller == null && Game1.random.NextDouble() < 0.002)
				{
					if (!location.farmers.Any())
					{
						(location as Farm).animals.Remove(myID);
						(home.indoors.Value as AnimalHouse).animals.Add(myID, this);
						setRandomPosition(home.indoors);
						faceDirection(Game1.random.Next(4));
						controller = null;
						return true;
					}
					if (NumPathfindingThisTick < MaxPathfindingPerTick)
					{
						NumPathfindingThisTick++;
						controller = new PathFindController(this, location, PathFindController.isAtEndPoint, 0, eraseOldPathController: false, null, 200, new Point((int)home.tileX + home.animalDoor.X, (int)home.tileY + home.animalDoor.Y));
						_followTarget = null;
						_followTargetPosition = null;
					}
				}
				if (location.IsOutdoors && !Game1.isRaining && !Game1.currentSeason.Equals("winter") && (int)currentProduce != -1 && (int)age >= (byte)ageWhenMature && type.Value.Contains("Pig") && Game1.random.NextDouble() < 0.0002)
				{
					Microsoft.Xna.Framework.Rectangle rect = GetBoundingBox();
					for (int i = 0; i < 4; i++)
					{
						Vector2 v = Utility.getCornersOfThisRectangle(ref rect, i);
						Vector2 vec = new Vector2((int)(v.X / 64f), (int)(v.Y / 64f));
						if (location.terrainFeatures.ContainsKey(vec) || location.objects.ContainsKey(vec))
						{
							return false;
						}
					}
					if (Game1.player.currentLocation.Equals(location))
					{
						DelayedAction.playSoundAfterDelay("dirtyHit", 450);
						DelayedAction.playSoundAfterDelay("dirtyHit", 900);
						DelayedAction.playSoundAfterDelay("dirtyHit", 1350);
					}
					if (location.Equals(Game1.currentLocation))
					{
						switch (FacingDirection)
						{
						case 2:
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(1, 250),
								new FarmerSprite.AnimationFrame(3, 250),
								new FarmerSprite.AnimationFrame(1, 250),
								new FarmerSprite.AnimationFrame(3, 250),
								new FarmerSprite.AnimationFrame(1, 250),
								new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false, findTruffle)
							});
							break;
						case 1:
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(5, 250),
								new FarmerSprite.AnimationFrame(7, 250),
								new FarmerSprite.AnimationFrame(5, 250),
								new FarmerSprite.AnimationFrame(7, 250),
								new FarmerSprite.AnimationFrame(5, 250),
								new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: false, findTruffle)
							});
							break;
						case 0:
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(9, 250),
								new FarmerSprite.AnimationFrame(11, 250),
								new FarmerSprite.AnimationFrame(9, 250),
								new FarmerSprite.AnimationFrame(11, 250),
								new FarmerSprite.AnimationFrame(9, 250),
								new FarmerSprite.AnimationFrame(11, 250, secondaryArm: false, flip: false, findTruffle)
							});
							break;
						case 3:
							Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
							{
								new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
								new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
								new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
								new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
								new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
								new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true, findTruffle)
							});
							break;
						}
						Sprite.loop = false;
					}
					else
					{
						findTruffle(Game1.player);
					}
				}
			}
			return false;
		}

		private void findTruffle(Farmer who)
		{
			if (Utility.spawnObjectAround(Utility.getTranslatedVector2(getTileLocation(), FacingDirection, 1f), new Object(getTileLocation(), 430, 1), Game1.getFarm()))
			{
				Game1.stats.TrufflesFound++;
			}
			if (new Random((int)(long)myID / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay).NextDouble() > (double)(int)friendshipTowardFarmer / 1500.0)
			{
				currentProduce.Value = -1;
			}
		}

		public static Microsoft.Xna.Framework.Rectangle GetFollowRange(FarmAnimal animal, int distance = 2)
		{
			Vector2 standing_position = animal.getStandingPosition();
			return new Microsoft.Xna.Framework.Rectangle((int)(standing_position.X - (float)(distance * 64)), (int)(standing_position.Y - (float)(distance * 64)), distance * 64 * 2, 64 * distance * 2);
		}

		public virtual void GetNewFollowPosition()
		{
			if (_followTarget == null)
			{
				_followTargetPosition = null;
			}
			else if (_followTarget.isMoving() && _followTarget.IsActuallySwimming())
			{
				_followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(GetFollowRange(_followTarget, 1), Game1.random));
			}
			else
			{
				_followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(GetFollowRange(_followTarget), Game1.random));
			}
		}

		public void hitWithWeapon(MeleeWeapon t)
		{
		}

		public void makeSound()
		{
			if (sound.Value != null && Game1.soundBank != null && base.currentLocation == Game1.currentLocation && !Game1.options.muteAnimalSounds)
			{
				ICue cue = Game1.soundBank.GetCue(sound.Value);
				cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
				cue.Play();
			}
		}

		public virtual bool CanHavePregnancy()
		{
			if (isCoopDweller())
			{
				return false;
			}
			if (type.Value == "Ostrich")
			{
				return false;
			}
			return true;
		}

		public virtual bool SleepIfNecessary()
		{
			if (Game1.timeOfDay >= 2000)
			{
				isSwimming.Value = false;
				hopOffset = Vector2.Zero;
				_followTarget = null;
				_followTargetPosition = null;
				if (isMoving())
				{
					Halt();
				}
				Sprite.currentFrame = (buildingTypeILiveIn.Contains("Coop") ? 16 : 12);
				FacingDirection = 2;
				Sprite.UpdateSourceRect();
				return true;
			}
			return false;
		}

		public override bool isMoving()
		{
			if (_swimmingVelocity != Vector2.Zero)
			{
				return true;
			}
			if (!IsActuallySwimming() && uniqueFrameAccumulator != -1)
			{
				return false;
			}
			return base.isMoving();
		}

		public virtual bool updateWhenCurrentLocation(GameTime time, GameLocation location)
		{
			if (!Game1.shouldTimePass())
			{
				return false;
			}
			if (health.Value <= 0)
			{
				return true;
			}
			doBuildingPokeEvent.Poll();
			doDiveEvent.Poll();
			if (IsActuallySwimming())
			{
				int time_multiplier = 1;
				if (isMoving())
				{
					time_multiplier = 4;
				}
				nextRipple -= (int)time.ElapsedGameTime.TotalMilliseconds * time_multiplier;
				if (nextRipple <= 0)
				{
					nextRipple = 2000;
					float scale = 1f;
					if (isBaby())
					{
						scale = 0.65f;
					}
					float x_offset = base.Position.X - (float)getStandingX();
					TemporaryAnimatedSprite ripple = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), isMoving() ? 75f : 150f, 8, 0, new Vector2((float)getStandingX() + x_offset * scale, (float)getStandingY() - 32f * scale), flicker: false, Game1.random.NextDouble() < 0.5, 0.01f, 0.01f, Color.White * 0.75f, scale, 0f, 0f, 0f);
					Vector2 offset = Utility.PointToVector2(Utility.getTranslatedPoint(default(Point), FacingDirection, -1));
					ripple.motion = offset * 0.25f;
					location.TemporarySprites.Add(ripple);
				}
			}
			if (hitGlowTimer > 0)
			{
				hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (Sprite.CurrentAnimation != null)
			{
				if (Sprite.animateOnce(time))
				{
					Sprite.CurrentAnimation = null;
				}
				return false;
			}
			update(time, location, myID, move: false);
			if (hopOffset != Vector2.Zero)
			{
				Sprite.UpdateSourceRect();
				HandleHop();
				return false;
			}
			if (Game1.IsMasterGame && behaviors(time, location))
			{
				return false;
			}
			if (Sprite.CurrentAnimation != null)
			{
				return false;
			}
			if (controller != null && controller.timerSinceLastCheckPoint > 10000)
			{
				controller = null;
				Halt();
			}
			if (location is Farm && noWarpTimer <= 0)
			{
				Building home = netHome.Value;
				if (home != null && Game1.IsMasterGame && home.getRectForAnimalDoor().Contains(GetBoundingBox().Center.X, GetBoundingBox().Top))
				{
					if (Utility.isOnScreen(getTileLocationPoint(), 192, location))
					{
						location.localSound("dwoop");
					}
					((Farm)location).animals.Remove(myID);
					(home.indoors.Value as AnimalHouse).animals[myID] = this;
					setRandomPosition(home.indoors);
					faceDirection(Game1.random.Next(4));
					controller = null;
					return true;
				}
			}
			noWarpTimer = Math.Max(0, noWarpTimer - time.ElapsedGameTime.Milliseconds);
			if (pauseTimer > 0)
			{
				pauseTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (SleepIfNecessary())
			{
				if (!isEmoting && Game1.random.NextDouble() < 0.002)
				{
					doEmote(24);
				}
			}
			else if (pauseTimer <= 0 && Game1.random.NextDouble() < 0.001 && (int)age >= (byte)ageWhenMature && Game1.gameMode == 3 && sound.Value != null && Utility.isOnScreen(base.Position, 192))
			{
				makeSound();
			}
			UpdateRandomMovements();
			if (uniqueFrameAccumulator != -1 && _followTarget != null && !GetFollowRange(_followTarget, 1).Contains(Utility.Vector2ToPoint(getStandingPosition())))
			{
				uniqueFrameAccumulator = -1;
			}
			if (uniqueFrameAccumulator != -1 && !Game1.IsClient)
			{
				uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
				if (uniqueFrameAccumulator > 500)
				{
					if (buildingTypeILiveIn.Contains("Coop"))
					{
						Sprite.currentFrame = Sprite.currentFrame + 1 - Sprite.currentFrame % 2 * 2;
					}
					else if (Sprite.currentFrame > 12)
					{
						Sprite.currentFrame = (Sprite.currentFrame - 13) * 4;
					}
					else
					{
						switch (FacingDirection)
						{
						case 0:
							Sprite.currentFrame = 15;
							break;
						case 1:
							Sprite.currentFrame = 14;
							break;
						case 2:
							Sprite.currentFrame = 13;
							break;
						case 3:
							Sprite.currentFrame = 14;
							break;
						}
					}
					uniqueFrameAccumulator = 0;
					if (Game1.random.NextDouble() < 0.4)
					{
						uniqueFrameAccumulator = -1;
					}
				}
				if (IsActuallySwimming())
				{
					MovePosition(time, Game1.viewport, location);
				}
			}
			else if (!Game1.IsClient)
			{
				MovePosition(time, Game1.viewport, location);
			}
			if (IsActuallySwimming())
			{
				Sprite.UpdateSourceRect();
				Microsoft.Xna.Framework.Rectangle source_rect = Sprite.SourceRect;
				source_rect.Offset(new Point(0, 112));
				Sprite.SourceRect = source_rect;
			}
			return false;
		}

		public virtual void UpdateRandomMovements()
		{
			if (Game1.timeOfDay >= 2000 || pauseTimer > 0)
			{
				return;
			}
			if (fullness.Value < byte.MaxValue && IsActuallySwimming() && Game1.random.NextDouble() < 0.002 && !isEating.Value)
			{
				Eat(base.currentLocation);
			}
			if (!Game1.IsClient && Game1.random.NextDouble() < 0.007 && uniqueFrameAccumulator == -1)
			{
				int newDirection = Game1.random.Next(5);
				if (newDirection != (FacingDirection + 2) % 4 || IsActuallySwimming())
				{
					if (newDirection < 4)
					{
						int oldDirection = FacingDirection;
						faceDirection(newDirection);
						if (!base.currentLocation.isOutdoors && base.currentLocation.isCollidingPosition(nextPosition(newDirection), Game1.viewport, this))
						{
							faceDirection(oldDirection);
							return;
						}
					}
					switch (newDirection)
					{
					case 0:
						SetMovingUp(b: true);
						break;
					case 1:
						SetMovingRight(b: true);
						break;
					case 2:
						SetMovingDown(b: true);
						break;
					case 3:
						SetMovingLeft(b: true);
						break;
					default:
						Halt();
						Sprite.StopAnimation();
						break;
					}
				}
				else if (noWarpTimer <= 0)
				{
					Halt();
					Sprite.StopAnimation();
				}
			}
			if (Game1.IsClient || !isMoving() || !(Game1.random.NextDouble() < 0.014) || uniqueFrameAccumulator != -1)
			{
				return;
			}
			Halt();
			Sprite.StopAnimation();
			if (Game1.random.NextDouble() < 0.75)
			{
				uniqueFrameAccumulator = 0;
				if (buildingTypeILiveIn.Contains("Coop"))
				{
					switch (FacingDirection)
					{
					case 0:
						Sprite.currentFrame = 20;
						break;
					case 1:
						Sprite.currentFrame = 18;
						break;
					case 2:
						Sprite.currentFrame = 16;
						break;
					case 3:
						Sprite.currentFrame = 22;
						break;
					}
				}
				else if (buildingTypeILiveIn.Contains("Barn"))
				{
					switch (FacingDirection)
					{
					case 0:
						Sprite.currentFrame = 15;
						break;
					case 1:
						Sprite.currentFrame = 14;
						break;
					case 2:
						Sprite.currentFrame = 13;
						break;
					case 3:
						Sprite.currentFrame = 14;
						break;
					}
				}
			}
			Sprite.UpdateSourceRect();
		}

		public virtual bool CanSwim()
		{
			if (type.Value == "Duck")
			{
				return true;
			}
			return false;
		}

		public virtual bool CanFollowAdult()
		{
			if (!isCoopDweller())
			{
				return false;
			}
			if (isBaby())
			{
				if (type.Value == "Duck")
				{
					return true;
				}
				if (type.Contains("Chicken"))
				{
					return true;
				}
			}
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public virtual void HandleHop()
		{
			int hop_speed = 4;
			if (hopOffset != Vector2.Zero)
			{
				if (hopOffset.X != 0f)
				{
					int move_amount2 = (int)Math.Min(hop_speed, Math.Abs(hopOffset.X));
					base.Position += new Vector2(move_amount2 * Math.Sign(hopOffset.X), 0f);
					hopOffset.X = Utility.MoveTowards(hopOffset.X, 0f, move_amount2);
				}
				if (hopOffset.Y != 0f)
				{
					int move_amount = (int)Math.Min(hop_speed, Math.Abs(hopOffset.Y));
					base.Position += new Vector2(0f, move_amount * Math.Sign(hopOffset.Y));
					hopOffset.Y = Utility.MoveTowards(hopOffset.Y, 0f, move_amount);
				}
				if (hopOffset == Vector2.Zero && isSwimming.Value)
				{
					Splash();
					_swimmingVelocity = Utility.getTranslatedVector2(Vector2.Zero, FacingDirection, base.speed);
					base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
				}
			}
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (pauseTimer > 0 || Game1.IsClient)
			{
				return;
			}
			Location next_tile = nextPositionTile();
			if (!currentLocation.isTileOnMap(new Vector2(next_tile.X, next_tile.Y)))
			{
				facingDirection.Value = Utility.GetOppositeFacingDirection(facingDirection);
				moveUp = (facingDirection.Value == 0);
				moveLeft = (facingDirection.Value == 3);
				moveDown = (facingDirection.Value == 2);
				moveRight = (facingDirection.Value == 1);
				_followTarget = null;
				_followTargetPosition = null;
				_swimmingVelocity = Vector2.Zero;
				return;
			}
			if (_followTarget != null && (_followTarget.currentLocation != currentLocation || (int)_followTarget.health <= 0))
			{
				_followTarget = null;
				_followTargetPosition = null;
			}
			if (_followTargetPosition.HasValue)
			{
				Vector2 standing_position = getStandingPosition();
				Vector2 offset = standing_position - Utility.PointToVector2(_followTargetPosition.Value);
				if (Math.Abs(offset.X) <= 64f || Math.Abs(offset.Y) <= 64f)
				{
					moveDown = false;
					moveUp = false;
					moveLeft = false;
					moveRight = false;
					GetNewFollowPosition();
				}
				else if (nextFollowDirectionChange >= 0)
				{
					nextFollowDirectionChange -= (int)time.ElapsedGameTime.TotalMilliseconds;
				}
				else
				{
					if (IsActuallySwimming())
					{
						nextFollowDirectionChange = 100;
					}
					else
					{
						nextFollowDirectionChange = 500;
					}
					moveDown = false;
					moveUp = false;
					moveLeft = false;
					moveRight = false;
					if (Math.Abs(standing_position.X - (float)_followTargetPosition.Value.X) < Math.Abs(standing_position.Y - (float)_followTargetPosition.Value.Y))
					{
						if (standing_position.Y > (float)_followTargetPosition.Value.Y)
						{
							moveUp = true;
						}
						else if (standing_position.Y < (float)_followTargetPosition.Value.Y)
						{
							moveDown = true;
						}
					}
					else if (standing_position.X < (float)_followTargetPosition.Value.X)
					{
						moveRight = true;
					}
					else if (standing_position.X > (float)_followTargetPosition.Value.X)
					{
						moveLeft = true;
					}
				}
			}
			if (IsActuallySwimming())
			{
				Vector2 desired_movement = default(Vector2);
				if (!isEating.Value)
				{
					if (moveUp)
					{
						desired_movement.Y = -base.speed;
					}
					else if (moveDown)
					{
						desired_movement.Y = base.speed;
					}
					if (moveLeft)
					{
						desired_movement.X = -base.speed;
					}
					else if (moveRight)
					{
						desired_movement.X = base.speed;
					}
				}
				_swimmingVelocity = new Vector2(Utility.MoveTowards(_swimmingVelocity.X, desired_movement.X, 0.025f), Utility.MoveTowards(_swimmingVelocity.Y, desired_movement.Y, 0.025f));
				Vector2 old_position = base.Position;
				base.Position += _swimmingVelocity;
				Microsoft.Xna.Framework.Rectangle next_bounds = GetBoundingBox();
				base.Position = old_position;
				int moving_direction = -1;
				if (!currentLocation.isCollidingPosition(next_bounds, Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
				{
					base.Position += _swimmingVelocity;
					if (Math.Abs(_swimmingVelocity.X) > Math.Abs(_swimmingVelocity.Y))
					{
						if (_swimmingVelocity.X < 0f)
						{
							moving_direction = 3;
						}
						else if (_swimmingVelocity.X > 0f)
						{
							moving_direction = 1;
						}
					}
					else if (_swimmingVelocity.Y < 0f)
					{
						moving_direction = 0;
					}
					else if (_swimmingVelocity.Y > 0f)
					{
						moving_direction = 2;
					}
					switch (moving_direction)
					{
					case 0:
						Sprite.AnimateUp(time);
						faceDirection(0);
						break;
					case 3:
						Sprite.AnimateRight(time);
						FacingDirection = 3;
						break;
					case 1:
						Sprite.AnimateRight(time);
						faceDirection(1);
						break;
					case 2:
						Sprite.AnimateDown(time);
						faceDirection(2);
						break;
					}
				}
				else if (!HandleCollision(next_bounds))
				{
					Halt();
					Sprite.StopAnimation();
					_swimmingVelocity *= -1f;
				}
			}
			else if (moveUp)
			{
				if (!currentLocation.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
				{
					position.Y -= base.speed;
					Sprite.AnimateUp(time);
				}
				else if (!HandleCollision(nextPosition(0)))
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
					{
						SetMovingDown(b: true);
					}
				}
				faceDirection(0);
			}
			else if (moveRight)
			{
				if (!currentLocation.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, 0, glider: false, this))
				{
					position.X += base.speed;
					Sprite.AnimateRight(time);
				}
				else if (!HandleCollision(nextPosition(1)))
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
					{
						SetMovingLeft(b: true);
					}
				}
				faceDirection(1);
			}
			else if (moveDown)
			{
				if (!currentLocation.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, 0, glider: false, this))
				{
					position.Y += base.speed;
					Sprite.AnimateDown(time);
				}
				else if (!HandleCollision(nextPosition(2)))
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
					{
						SetMovingUp(b: true);
					}
				}
				faceDirection(2);
			}
			else
			{
				if (!moveLeft)
				{
					return;
				}
				if (!currentLocation.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, 0, glider: false, this))
				{
					position.X -= base.speed;
					Sprite.AnimateRight(time);
				}
				else if (!HandleCollision(nextPosition(3)))
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6 || IsActuallySwimming())
					{
						SetMovingRight(b: true);
					}
				}
				FacingDirection = 3;
				if (!isCoopDweller() && Sprite.currentFrame > 7)
				{
					Sprite.currentFrame = 4;
				}
			}
		}

		public virtual bool HandleCollision(Microsoft.Xna.Framework.Rectangle next_position)
		{
			if (_followTarget != null)
			{
				_followTarget = null;
				_followTargetPosition = null;
			}
			if (base.currentLocation is Farm && CanSwim() && (isSwimming.Value || controller == null) && wasPet.Value && hopOffset == Vector2.Zero)
			{
				base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
				Microsoft.Xna.Framework.Rectangle current_position = GetBoundingBox();
				Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, FacingDirection, 1f);
				if (offset != Vector2.Zero)
				{
					Point hop_over_tile = getTileLocationPoint();
					hop_over_tile.X += (int)offset.X;
					hop_over_tile.Y += (int)offset.Y;
					offset *= 128f;
					Microsoft.Xna.Framework.Rectangle hop_destination = current_position;
					hop_destination.Offset(Utility.Vector2ToPoint(offset));
					Point hop_tile = new Point(hop_destination.X / 64, hop_destination.Y / 64);
					if (base.currentLocation.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Water", "Back") != null && base.currentLocation.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !base.currentLocation.isCollidingPosition(hop_destination, Game1.viewport, isFarmer: false, 0, glider: false, this) && base.currentLocation.isOpenWater(hop_tile.X, hop_tile.Y) != isSwimming.Value)
					{
						isSwimming.Value = !isSwimming.Value;
						if (!isSwimming.Value)
						{
							Splash();
						}
						hopOffset = offset;
						pauseTimer = 0;
						doDiveEvent.Fire();
					}
					return true;
				}
			}
			return false;
		}

		public virtual bool IsActuallySwimming()
		{
			if (isSwimming.Value)
			{
				return hopOffset == Vector2.Zero;
			}
			return false;
		}

		public virtual void Splash()
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 192, base.currentLocation))
			{
				base.currentLocation.playSound("dropItemInWater");
			}
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(28, 100f, 2, 1, getStandingPosition() + new Vector2(-0.5f, -0.5f) * 64f, flicker: false, flipped: false)
			{
				delayBeforeAnimationStart = 0,
				layerDepth = (float)getStandingY() / 10000f
			});
		}

		public override void animateInFacingDirection(GameTime time)
		{
			if (FacingDirection == 3)
			{
				Sprite.AnimateRight(time);
			}
			else
			{
				base.animateInFacingDirection(time);
			}
		}
	}
}
