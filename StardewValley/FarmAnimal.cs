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
		private readonly NetBool isEating = new NetBool();

		[XmlIgnore]
		private readonly NetEvent1Field<int, NetInt> doFarmerPushEvent = new NetEvent1Field<int, NetInt>();

		[XmlIgnore]
		private readonly NetEvent0 doBuildingPokeEvent = new NetEvent0();

		private string _displayHouse;

		private string _displayType;

		public static int NumPathfindingThisTick = 0;

		public static int MaxPathfindingPerTick = 1;

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
			base.initNetFields();
			base.NetFields.AddFields(defaultProduceIndex, deluxeProduceIndex, currentProduce, friendshipTowardFarmer, daysSinceLastFed, age, meatIndex, health, price, produceQuality, daysToLay, daysSinceLastLay, ageWhenMature, harvestType, happiness, fullness, happinessDrain, fullnessDrain, wasPet, showDifferentTextureWhenReadyForHarvest, allowReproduction, sound, type, buildingTypeILiveIn, toolUsedForHarvest, frontBackBoundingBox, sidewaysBoundingBox, frontBackSourceRect, sidewaysSourceRect, myID, ownerID, parentId, netHome.NetFields, homeLocation, moodMessage, isEating, doFarmerPushEvent, doBuildingPokeEvent);
			position.Field.AxisAlignedMovement = true;
			doFarmerPushEvent.onEvent += doFarmerPush;
			doBuildingPokeEvent.onEvent += doBuildingPoke;
		}

		public FarmAnimal(string type, long id, long ownerID)
			: base(null, new Vector2(64 * Game1.random.Next(2, 9), 64 * Game1.random.Next(4, 8)), 2, type)
		{
			this.ownerID.Value = ownerID;
			health.Value = 3;
			if (type.Contains("Chicken") && !type.Equals("Void Chicken"))
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
			return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(base.Position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
		}

		public Microsoft.Xna.Framework.Rectangle GetCursorPetBoundingBox()
		{
			if (type.Contains("Chicken"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)base.Position.Y - 16, 64, 68);
			}
			if (type.Contains("Cow"))
			{
				if (base.FacingDirection == 0 || base.FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 24f + 8f), (int)base.Position.Y, 68, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 4f), (int)(base.Position.Y + 24f - 8f), 112, 80);
			}
			if (type.Contains("Pig"))
			{
				if (base.FacingDirection == 0 || base.FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 24f), (int)base.Position.Y, 82, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 4f), (int)(base.Position.Y + 24f), 116, 72);
			}
			if (type.Contains("Duck"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y - 8f), 64, 60);
			}
			if (type.Contains("Rabbit"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)(base.Position.Y - 8f), 56, 56);
			}
			if (type.Contains("Dinosaur"))
			{
				return new Microsoft.Xna.Framework.Rectangle((int)base.Position.X, (int)base.Position.Y, 56, 52);
			}
			if (type.Contains("Sheep"))
			{
				if (base.FacingDirection == 0 || base.FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 24f + 8f), (int)base.Position.Y, 72, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 4f), (int)(base.Position.Y + 24f), 112, 72);
			}
			if (type.Contains("Goat"))
			{
				if (base.FacingDirection == 0 || base.FacingDirection == 2)
				{
					return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 40f) - 8, (int)base.Position.Y - 4, 64, 112);
				}
				return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + 4f), (int)(base.Position.Y + 24f) - 4, 112, 80);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(base.Position.Y + (float)(Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
		}

		public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
		{
			return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + (float)(Sprite.getWidth() * 4 / 2) - 32f + 8f), (int)(base.Position.Y + (float)(Sprite.getHeight() * 4) - 64f + 8f), 48, 48);
		}

		public void reload(Building home)
		{
			this.home = home;
			reloadData();
		}

		public void pet(Farmer who)
		{
			if (who.FarmerSprite.PauseForSingleAnimation)
			{
				return;
			}
			who.Halt();
			who.faceGeneralDirection(base.Position);
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
			if (!wasPet)
			{
				wasPet.Value = true;
				friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
				if ((who.professions.Contains(3) && !isCoopDweller()) || (who.professions.Contains(2) && isCoopDweller()))
				{
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 15);
					happiness.Value = (byte)Math.Min(255, (byte)happiness + Math.Max(5, 40 - (byte)happinessDrain));
				}
				doEmote(((int)moodMessage == 4) ? 12 : 20);
				happiness.Value = (byte)Math.Min(255, (byte)happiness + Math.Max(5, 40 - (byte)happinessDrain));
				makeSound();
				who.gainExperience(0, 5);
			}
			else if (who.ActiveObject == null || (int)who.ActiveObject.parentSheetIndex != 178)
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
				base.FacingDirection = Game1.random.Next(4);
				setMovingInFacingDirection();
			}
		}

		public void setRandomPosition(GameLocation location)
		{
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
		}

		public void dayUpdate(GameLocation environtment)
		{
			controller = null;
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
			if (!wasPet)
			{
				friendshipTowardFarmer.Value = Math.Max(0, (int)friendshipTowardFarmer - (10 - (int)friendshipTowardFarmer / 200));
				happiness.Value = (byte)Math.Max(0, (byte)happiness - (byte)happinessDrain * 5);
			}
			wasPet.Value = false;
			if (((byte)fullness < 200 || Game1.timeOfDay < 1700) && environtment is AnimalHouse)
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
					if (type.Value.Equals("Duck") && r.NextDouble() < (double)((float)(int)friendshipTowardFarmer + happinessModifier) / 5000.0 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() * 0.01)
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
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (isCoopDweller())
			{
				Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f)), isBaby() ? 3f : 4f);
			}
			Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f)), ((float)(GetBoundingBox().Center.Y + 4) + base.Position.X / 1000f) / 10000f, 0, 0, (hitGlowTimer > 0) ? Color.Red : Color.White, base.FacingDirection == 3, 4f);
			if (isEmoting)
			{
				Vector2 emotePosition = Game1.GlobalToLocal(Game1.viewport, base.Position + new Vector2(frontBackSourceRect.Width / 2 * 4 - 32, isCoopDweller() ? (-96) : (-64)));
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)GetBoundingBox().Bottom / 10000f);
			}
		}

		public virtual void updateWhenNotCurrentLocation(Building currentBuilding, GameTime time, GameLocation environment)
		{
			doFarmerPushEvent.Poll();
			doBuildingPokeEvent.Poll();
			if (!Game1.shouldTimePass())
			{
				return;
			}
			update(time, environment, myID, move: false);
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (currentBuilding != null && Game1.random.NextDouble() < 0.002 && (bool)currentBuilding.animalDoorOpen && Game1.timeOfDay < 1630 && !Game1.isRaining && !Game1.currentSeason.Equals("winter") && environment.farmers.Count == 0)
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
			behaviors(time, environment);
		}

		public static void behaviorAfterFindingGrassPatch(Character c, GameLocation environment)
		{
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
				isEating.Value = true;
				if (((Grass)environment.terrainFeatures[tilePosition]).reduceBy(isCoopDweller() ? 2 : 4, tilePosition, environment.Equals(Game1.currentLocation)))
				{
					environment.terrainFeatures.Remove(tilePosition);
				}
				Sprite.loop = false;
				fullness.Value = byte.MaxValue;
				if ((int)moodMessage != 5 && (int)moodMessage != 6 && !Game1.isRaining)
				{
					happiness.Value = byte.MaxValue;
					friendshipTowardFarmer.Value = Math.Min(1000, (int)friendshipTowardFarmer + 8);
				}
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
			if (home == null)
			{
				return false;
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
				if (location.IsOutdoors && (byte)fullness < 195 && Game1.random.NextDouble() < 0.002 && NumPathfindingThisTick < MaxPathfindingPerTick)
				{
					NumPathfindingThisTick++;
					controller = new PathFindController(this, location, grassEndPointFunction, -1, eraseOldPathController: false, behaviorAfterFindingGrassPatch, 200, Point.Zero);
				}
				if (Game1.timeOfDay >= 1700 && location.IsOutdoors && controller == null && Game1.random.NextDouble() < 0.002)
				{
					if (location.farmers.Count == 0)
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
						switch (base.FacingDirection)
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
			if (Utility.spawnObjectAround(Utility.getTranslatedVector2(getTileLocation(), base.FacingDirection, 1f), new Object(getTileLocation(), 430, 1), Game1.getFarm()))
			{
				Game1.stats.TrufflesFound++;
			}
			if (new Random((int)(long)myID / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay).NextDouble() > (double)(int)friendshipTowardFarmer / 1500.0)
			{
				currentProduce.Value = -1;
			}
		}

		public void hitWithWeapon(MeleeWeapon t)
		{
		}

		public void makeSound()
		{
			if (sound.Value != null && Game1.soundBank != null && base.currentLocation == Game1.currentLocation)
			{
				ICue cue = Game1.soundBank.GetCue(sound.Value);
				cue.SetVariable("Pitch", 1200 + Game1.random.Next(-200, 201));
				cue.Play();
			}
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
			if (Game1.timeOfDay >= 2000)
			{
				Sprite.currentFrame = (buildingTypeILiveIn.Contains("Coop") ? 16 : 12);
				Sprite.UpdateSourceRect();
				base.FacingDirection = 2;
				if (!isEmoting && Game1.random.NextDouble() < 0.002)
				{
					doEmote(24);
				}
			}
			else if (pauseTimer <= 0)
			{
				if (Game1.random.NextDouble() < 0.001 && (int)age >= (byte)ageWhenMature && Game1.gameMode == 3 && sound.Value != null && Utility.isOnScreen(base.Position, 192))
				{
					makeSound();
				}
				if (!Game1.IsClient && Game1.random.NextDouble() < 0.007 && uniqueFrameAccumulator == -1)
				{
					int newDirection = Game1.random.Next(5);
					if (newDirection != (base.FacingDirection + 2) % 4)
					{
						if (newDirection < 4)
						{
							int oldDirection = base.FacingDirection;
							faceDirection(newDirection);
							if (!location.isOutdoors && location.isCollidingPosition(nextPosition(newDirection), Game1.viewport, this))
							{
								faceDirection(oldDirection);
								return false;
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
				if (!Game1.IsClient && isMoving() && Game1.random.NextDouble() < 0.014 && uniqueFrameAccumulator == -1)
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.75)
					{
						uniqueFrameAccumulator = 0;
						if (buildingTypeILiveIn.Contains("Coop"))
						{
							switch (base.FacingDirection)
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
							switch (base.FacingDirection)
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
							switch (base.FacingDirection)
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
				}
				else if (!Game1.IsClient)
				{
					MovePosition(time, Game1.viewport, location);
				}
			}
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (pauseTimer > 0 || Game1.IsClient)
			{
				return;
			}
			if (moveUp)
			{
				if (!currentLocation.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
				{
					position.Y -= base.speed;
					Sprite.AnimateUp(time);
				}
				else
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6)
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
				else
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6)
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
				else
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6)
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
				else
				{
					Halt();
					Sprite.StopAnimation();
					if (Game1.random.NextDouble() < 0.6)
					{
						SetMovingRight(b: true);
					}
				}
				base.FacingDirection = 3;
				if (!isCoopDweller() && Sprite.currentFrame > 7)
				{
					Sprite.currentFrame = 4;
				}
			}
		}

		public override void animateInFacingDirection(GameTime time)
		{
			if (base.FacingDirection == 3)
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
