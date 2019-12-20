using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.Quests;
using StardewValley.Tools;
using System;

namespace StardewValley
{
	public class Debris : INetObject<NetFields>
	{
		public enum DebrisType
		{
			CHUNKS,
			LETTERS,
			SQUARES,
			ARCHAEOLOGY,
			OBJECT,
			SPRITECHUNKS,
			RESOURCE,
			NUMBERS
		}

		public const int copperDebris = 0;

		public const int ironDebris = 2;

		public const int coalDebris = 4;

		public const int goldDebris = 6;

		public const int coinsDebris = 8;

		public const int iridiumDebris = 10;

		public const int woodDebris = 12;

		public const int stoneDebris = 14;

		public const int fuelDebris = 28;

		public const int quartzDebris = 30;

		public const int bigStoneDebris = 32;

		public const int bigWoodDebris = 34;

		public const int timesToBounce = 2;

		public const int minMoneyPerCoin = 10;

		public const int maxMoneyPerCoin = 40;

		public const float gravity = 0.4f;

		public const float timeToWaitBeforeRemoval = 600f;

		public const int marginForChunkPickup = 64;

		public const int white = 10000;

		public const int green = 100001;

		public const int blue = 100002;

		public const int red = 100003;

		public const int yellow = 100004;

		public const int black = 100005;

		public const int charcoal = 100007;

		public const int gray = 100006;

		private readonly NetObjectShrinkList<Chunk> chunks = new NetObjectShrinkList<Chunk>();

		public readonly NetInt chunkType = new NetInt();

		public readonly NetInt sizeOfSourceRectSquares = new NetInt(8);

		private readonly NetInt netItemQuality = new NetInt();

		private readonly NetInt netChunkFinalYLevel = new NetInt();

		private readonly NetInt netChunkFinalYTarget = new NetInt();

		public float timeSinceDoneBouncing;

		public readonly NetFloat scale = new NetFloat(1f).Interpolated(interpolate: true, wait: true);

		public bool chunksMoveTowardPlayer;

		public readonly NetLong DroppedByPlayerID = new NetLong().Interpolated(interpolate: false, wait: false);

		private bool movingUp;

		public readonly NetBool floppingFish = new NetBool();

		public bool isFishable;

		public bool movingFinalYLevel;

		public readonly NetEnum<DebrisType> debrisType = new NetEnum<DebrisType>(DebrisType.CHUNKS);

		public readonly NetString debrisMessage = new NetString("");

		public readonly NetColor nonSpriteChunkColor = new NetColor(Color.White);

		public readonly NetColor chunksColor = new NetColor();

		public readonly NetString spriteChunkSheetName = new NetString();

		private Texture2D _spriteChunkSheet;

		private readonly NetRef<Item> netItem = new NetRef<Item>();

		public Character toHover;

		public readonly NetFarmerRef player = new NetFarmerRef();

		private float relativeXPosition;

		public int itemQuality
		{
			get
			{
				return netItemQuality;
			}
			set
			{
				netItemQuality.Value = value;
			}
		}

		public int chunkFinalYLevel
		{
			get
			{
				return netChunkFinalYLevel;
			}
			set
			{
				netChunkFinalYLevel.Value = value;
			}
		}

		public int chunkFinalYTarget
		{
			get
			{
				return netChunkFinalYTarget;
			}
			set
			{
				netChunkFinalYTarget.Value = value;
			}
		}

		public Texture2D spriteChunkSheet
		{
			get
			{
				if (_spriteChunkSheet == null && spriteChunkSheetName != null)
				{
					_spriteChunkSheet = Game1.content.Load<Texture2D>(spriteChunkSheetName);
				}
				return _spriteChunkSheet;
			}
		}

		public Item item
		{
			get
			{
				return netItem;
			}
			set
			{
				netItem.Value = value;
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetObjectShrinkList<Chunk> Chunks => chunks;

		public Debris()
		{
			NetFields.AddFields(chunks, chunkType, sizeOfSourceRectSquares, netItemQuality, netChunkFinalYLevel, netChunkFinalYTarget, scale, floppingFish, debrisType, debrisMessage, nonSpriteChunkColor, chunksColor, spriteChunkSheetName, netItem, player.NetFields, DroppedByPlayerID);
		}

		public Debris(int objectIndex, Vector2 debrisOrigin, Vector2 playerPosition)
			: this(objectIndex, 1, debrisOrigin, playerPosition)
		{
			string type = "";
			if (((objectIndex > 0) ? Game1.objectInformation[objectIndex].Split('/')[3].Split(' ')[0] : "Crafting").Equals("Arch"))
			{
				debrisType.Value = DebrisType.ARCHAEOLOGY;
			}
			else
			{
				debrisType.Value = DebrisType.OBJECT;
			}
			if (objectIndex == 92)
			{
				debrisType.Value = DebrisType.RESOURCE;
			}
			if (Game1.player.speed >= 5)
			{
				for (int i = 0; i < chunks.Count; i++)
				{
					chunks[i].xVelocity.Value *= ((Game1.player.FacingDirection == 1 || Game1.player.FacingDirection == 3) ? 1 : 1);
				}
			}
			chunks[0].debrisType = objectIndex;
		}

		public Debris(int number, Vector2 debrisOrigin, Color messageColor, float scale, Character toHover)
			: this(-1, 1, debrisOrigin, Game1.player.Position)
		{
			chunkType.Value = number;
			debrisType.Value = DebrisType.NUMBERS;
			nonSpriteChunkColor.Value = messageColor;
			chunks[0].scale = scale;
			this.toHover = toHover;
			chunks[0].xVelocity.Value = Game1.random.Next(-1, 2);
		}

		public Debris(Item item, Vector2 debrisOrigin)
			: this(-2, 1, debrisOrigin, new Vector2(Game1.player.GetBoundingBox().Center.X, Game1.player.GetBoundingBox().Center.Y))
		{
			this.item = item;
			item.resetState();
		}

		public Debris(Item item, Vector2 debrisOrigin, Vector2 targetLocation)
			: this(-2, 1, debrisOrigin, targetLocation)
		{
			this.item = item;
			item.resetState();
		}

		public Debris(string message, int numberOfChunks, Vector2 debrisOrigin, Color messageColor, float scale, float rotation)
			: this(-1, numberOfChunks, debrisOrigin, Game1.player.Position)
		{
			debrisType.Value = DebrisType.LETTERS;
			debrisMessage.Value = message;
			nonSpriteChunkColor.Value = messageColor;
			chunks[0].rotation = rotation;
			chunks[0].scale = scale;
		}

		public Debris(string spriteSheet, int numberOfChunks, Vector2 debrisOrigin)
			: this(-1, numberOfChunks, debrisOrigin, Game1.player.Position)
		{
			debrisType.Value = DebrisType.SPRITECHUNKS;
			spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(0, 56);
				chunk.ySpriteSheet.Value = Game1.random.Next(0, 88);
				chunk.scale = 1f;
			}
		}

		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin)
			: this(-1, numberOfChunks, debrisOrigin, Game1.player.Position)
		{
			debrisType.Value = DebrisType.SPRITECHUNKS;
			spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
				chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
				chunk.scale = 1f;
			}
		}

		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, int sizeOfSourceRectSquares)
			: this(-1, numberOfChunks, debrisOrigin, Game1.player.Position)
		{
			this.sizeOfSourceRectSquares.Value = sizeOfSourceRectSquares;
			debrisType.Value = DebrisType.SPRITECHUNKS;
			spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.X;
				chunk.ySpriteSheet.Value = Game1.random.Next(2) * sizeOfSourceRectSquares + sourceRect.Y;
				chunk.rotationVelocity = ((Game1.random.NextDouble() < 0.5) ? ((float)(Math.PI / (double)Game1.random.Next(-32, -16))) : ((float)(Math.PI / (double)Game1.random.Next(16, 32))));
				chunk.xVelocity.Value *= 1.2f;
				chunk.yVelocity.Value *= 1.2f;
				chunk.scale = 4f;
			}
		}

		public Debris(string spriteSheet, Rectangle sourceRect, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel)
			: this(-1, numberOfChunks, debrisOrigin, playerPosition)
		{
			debrisType.Value = DebrisType.SPRITECHUNKS;
			spriteChunkSheetName.Value = spriteSheet;
			for (int i = 0; i < chunks.Count; i++)
			{
				Chunk chunk = chunks[i];
				chunk.xSpriteSheet.Value = Game1.random.Next(sourceRect.X, sourceRect.X + sourceRect.Width - 4);
				chunk.ySpriteSheet.Value = Game1.random.Next(sourceRect.Y, sourceRect.Y + sourceRect.Width - 4);
				chunk.scale = 1f;
			}
			chunkFinalYLevel = groundLevel;
		}

		public Debris(int type, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, int groundLevel, int color = -1)
			: this(-1, numberOfChunks, debrisOrigin, playerPosition)
		{
			debrisType.Value = DebrisType.CHUNKS;
			for (int i = 0; i < chunks.Count; i++)
			{
				chunks[i].debrisType = type;
			}
			chunkType.Value = type;
			chunksColor.Value = getColorForDebris((color == -1) ? type : color);
		}

		public Color getColorForDebris(int type)
		{
			switch (type)
			{
			case 12:
				return new Color(170, 106, 46);
			case 100001:
			case 100006:
				return Color.LightGreen;
			case 100003:
				return Color.Red;
			case 100004:
				return Color.Yellow;
			case 100005:
				return Color.Black;
			case 100007:
				return Color.DimGray;
			case 100002:
				return Color.LightBlue;
			default:
				return Color.White;
			}
		}

		public Debris(int debrisType, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition)
			: this(debrisType, numberOfChunks, debrisOrigin, playerPosition, 1f)
		{
		}

		public Debris(int debrisType, int numberOfChunks, Vector2 debrisOrigin, Vector2 playerPosition, float velocityMultiplyer)
			: this()
		{
			switch (debrisType)
			{
			case 0:
			case 378:
				debrisType = 378;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 2:
			case 380:
				debrisType = 380;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 6:
			case 384:
				debrisType = 384;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 10:
			case 386:
				debrisType = 386;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 4:
			case 382:
				debrisType = 382;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 12:
			case 388:
				debrisType = 388;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 14:
			case 390:
				debrisType = 390;
				this.debrisType.Value = DebrisType.RESOURCE;
				break;
			case 8:
				this.debrisType.Value = DebrisType.CHUNKS;
				break;
			default:
				this.debrisType.Value = DebrisType.OBJECT;
				break;
			}
			if (debrisType != -1)
			{
				playerPosition -= (playerPosition - debrisOrigin) * 2f;
			}
			_ = 154;
			chunkType.Value = debrisType;
			int minYVelocity3 = 2;
			int maxYVelocity3 = 10;
			int minXVelocity3 = -10;
			int maxXVelocity3 = 10;
			floppingFish.Value = (Game1.objectInformation.ContainsKey(debrisType) && Game1.objectInformation[debrisType].Split('/')[3].Contains("-4"));
			isFishable = (Game1.objectInformation.ContainsKey(debrisType) && Game1.objectInformation[debrisType].Split('/')[3].Contains("Fish"));
			if (playerPosition.Y >= debrisOrigin.Y - 32f && playerPosition.Y <= debrisOrigin.Y + 32f)
			{
				chunkFinalYLevel = (int)debrisOrigin.Y - 32;
				minYVelocity3 = 220;
				maxYVelocity3 = 250;
				if (playerPosition.X < debrisOrigin.X)
				{
					minXVelocity3 = 20;
					maxXVelocity3 = 140;
				}
				else
				{
					minXVelocity3 = -140;
					maxXVelocity3 = -20;
				}
			}
			else if (playerPosition.Y < debrisOrigin.Y - 32f)
			{
				chunkFinalYLevel = (int)debrisOrigin.Y + (int)(32f * velocityMultiplyer);
				minYVelocity3 = 150;
				maxYVelocity3 = 200;
				minXVelocity3 = -50;
				maxXVelocity3 = 50;
			}
			else
			{
				movingFinalYLevel = true;
				chunkFinalYLevel = (int)debrisOrigin.Y - 1;
				chunkFinalYTarget = (int)debrisOrigin.Y - (int)(96f * velocityMultiplyer);
				movingUp = true;
				minYVelocity3 = 350;
				maxYVelocity3 = 400;
				minXVelocity3 = -50;
				maxXVelocity3 = 50;
			}
			debrisOrigin.X -= 32f;
			debrisOrigin.Y -= 32f;
			minXVelocity3 = (int)((float)minXVelocity3 * velocityMultiplyer);
			maxXVelocity3 = (int)((float)maxXVelocity3 * velocityMultiplyer);
			minYVelocity3 = (int)((float)minYVelocity3 * velocityMultiplyer);
			maxYVelocity3 = (int)((float)maxYVelocity3 * velocityMultiplyer);
			for (int i = 0; i < numberOfChunks; i++)
			{
				chunks.Add(new Chunk(debrisOrigin, (float)Game1.recentMultiplayerRandom.Next(minXVelocity3, maxXVelocity3) / 40f, (float)Game1.recentMultiplayerRandom.Next(minYVelocity3, maxYVelocity3) / 40f, Game1.recentMultiplayerRandom.Next(debrisType, debrisType + 2)));
			}
		}

		private Vector2 approximatePosition()
		{
			Vector2 total = default(Vector2);
			foreach (Chunk chunk in Chunks)
			{
				total += chunk.position.Value;
			}
			return total / Chunks.Count;
		}

		private bool playerInRange(Vector2 position, Farmer farmer)
		{
			if (Math.Abs(position.X + 32f - (float)farmer.getStandingX()) <= (float)farmer.MagneticRadius)
			{
				return Math.Abs(position.Y + 32f - (float)farmer.getStandingY()) <= (float)farmer.MagneticRadius;
			}
			return false;
		}

		private Farmer findBestPlayer(GameLocation location)
		{
			Vector2 position = approximatePosition();
			float bestDistance = float.MaxValue;
			Farmer bestFarmer = null;
			foreach (Farmer farmer in location.farmers)
			{
				if ((farmer.UniqueMultiplayerID != (long)DroppedByPlayerID || bestFarmer == null) && playerInRange(position, farmer))
				{
					float distance = (farmer.Position - position).LengthSquared();
					if (distance < bestDistance || (bestFarmer != null && bestFarmer.UniqueMultiplayerID == (long)DroppedByPlayerID))
					{
						bestFarmer = farmer;
						bestDistance = distance;
					}
				}
			}
			return bestFarmer;
		}

		public bool shouldControlThis(GameLocation location)
		{
			if (!Game1.IsMasterGame)
			{
				return location?.isTemp() ?? false;
			}
			return true;
		}

		public bool updateChunks(GameTime time, GameLocation location)
		{
			if (chunks.Count == 0)
			{
				return true;
			}
			timeSinceDoneBouncing += time.ElapsedGameTime.Milliseconds;
			if (timeSinceDoneBouncing >= (floppingFish ? 2500f : (((DebrisType)debrisType == DebrisType.SPRITECHUNKS || (DebrisType)debrisType == DebrisType.NUMBERS) ? 1800f : 600f)))
			{
				if ((DebrisType)debrisType == DebrisType.LETTERS || (DebrisType)debrisType == DebrisType.NUMBERS || (DebrisType)debrisType == DebrisType.SQUARES || (DebrisType)debrisType == DebrisType.SPRITECHUNKS || ((DebrisType)debrisType == DebrisType.CHUNKS && chunks[0].debrisType - chunks[0].debrisType % 2 != 8))
				{
					return true;
				}
				if ((DebrisType)debrisType == DebrisType.ARCHAEOLOGY || (DebrisType)debrisType == DebrisType.OBJECT || (DebrisType)debrisType == DebrisType.RESOURCE || (DebrisType)debrisType == DebrisType.CHUNKS)
				{
					chunksMoveTowardPlayer = true;
				}
				timeSinceDoneBouncing = 0f;
			}
			if (location.farmers.Count == 0)
			{
				return false;
			}
			Vector2 position = approximatePosition();
			Farmer farmer = player.Value;
			if (chunksMoveTowardPlayer && shouldControlThis(location))
			{
				if (player.Value != null && (player.Value.currentLocation != location || !playerInRange(position, player.Value)))
				{
					player.Value = null;
					farmer = null;
				}
				if (farmer == null)
				{
					farmer = findBestPlayer(location);
				}
			}
			bool anyCouldMove = false;
			for (int i = chunks.Count - 1; i >= 0; i--)
			{
				Chunk chunk = chunks[i];
				chunk.position.UpdateExtrapolation(chunk.getSpeed());
				if (chunk.alpha > 0.1f && ((DebrisType)debrisType == DebrisType.SPRITECHUNKS || (DebrisType)debrisType == DebrisType.NUMBERS) && timeSinceDoneBouncing > 600f)
				{
					chunk.alpha = (1800f - timeSinceDoneBouncing) / 1000f;
				}
				if (chunk.position.X < -128f || chunk.position.Y < -64f || chunk.position.X >= (float)(location.map.DisplayWidth + 64) || chunk.position.Y >= (float)(location.map.DisplayHeight + 64))
				{
					chunks.RemoveAt(i);
					continue;
				}
				bool canMoveTowardPlayer = farmer != null;
				if (canMoveTowardPlayer)
				{
					switch (debrisType.Value)
					{
					case DebrisType.ARCHAEOLOGY:
					case DebrisType.OBJECT:
						if (item != null)
						{
							canMoveTowardPlayer = farmer.couldInventoryAcceptThisItem(item);
							break;
						}
						canMoveTowardPlayer = ((chunk.debrisType >= 0) ? farmer.couldInventoryAcceptThisObject(chunk.debrisType, 1, itemQuality) : farmer.couldInventoryAcceptThisItem(new Object(Vector2.Zero, chunk.debrisType * -1)));
						if (chunk.debrisType == 102 && (bool)farmer.hasMenuOpen)
						{
							canMoveTowardPlayer = false;
						}
						break;
					case DebrisType.RESOURCE:
						canMoveTowardPlayer = farmer.couldInventoryAcceptThisObject(chunk.debrisType - chunk.debrisType % 2, 1);
						break;
					default:
						canMoveTowardPlayer = true;
						break;
					}
					anyCouldMove |= canMoveTowardPlayer;
					if (canMoveTowardPlayer && shouldControlThis(location))
					{
						player.Value = farmer;
					}
				}
				if (((chunksMoveTowardPlayer || isFishable) & canMoveTowardPlayer) && player.Value != null)
				{
					if (!player.Value.IsLocalPlayer)
					{
						continue;
					}
					if (chunk.position.X < player.Value.Position.X - 12f)
					{
						chunk.xVelocity.Value = Math.Min((float)chunk.xVelocity + 0.8f, 8f);
					}
					else if (chunk.position.X > player.Value.Position.X + 12f)
					{
						chunk.xVelocity.Value = Math.Max((float)chunk.xVelocity - 0.8f, -8f);
					}
					if (chunk.position.Y + 32f < (float)(player.Value.getStandingY() - 12))
					{
						chunk.yVelocity.Value = Math.Max((float)chunk.yVelocity - 0.8f, -8f);
					}
					else if (chunk.position.Y + 32f > (float)(player.Value.getStandingY() + 12))
					{
						chunk.yVelocity.Value = Math.Min((float)chunk.yVelocity + 0.8f, 8f);
					}
					chunk.position.X += chunk.xVelocity;
					chunk.position.Y -= chunk.yVelocity;
					if (!(Math.Abs(chunk.position.X + 32f - (float)player.Value.getStandingX()) <= 64f) || !(Math.Abs(chunk.position.Y + 32f - (float)player.Value.getStandingY()) <= 64f))
					{
						continue;
					}
					int switcher = ((DebrisType)debrisType == DebrisType.ARCHAEOLOGY || (DebrisType)debrisType == DebrisType.OBJECT) ? chunk.debrisType : (chunk.debrisType - chunk.debrisType % 2);
					if ((DebrisType)debrisType == DebrisType.ARCHAEOLOGY)
					{
						Game1.farmerFindsArtifact(chunk.debrisType);
					}
					else if (item != null)
					{
						Item tmpItem = item;
						item = null;
						if (!player.Value.addItemToInventoryBool(tmpItem))
						{
							item = tmpItem;
							continue;
						}
					}
					else if ((DebrisType)debrisType != 0 || switcher != 8)
					{
						if (switcher <= -10000)
						{
							if (!player.Value.addItemToInventoryBool(new MeleeWeapon(switcher)))
							{
								continue;
							}
						}
						else if (switcher <= 0)
						{
							if (!player.Value.addItemToInventoryBool(new Object(Vector2.Zero, -switcher)))
							{
								continue;
							}
						}
						else if (!player.Value.addItemToInventoryBool((switcher == 93 || switcher == 94) ? new Torch(Vector2.Zero, 1, switcher) : new Object(Vector2.Zero, switcher, 1)
						{
							Quality = itemQuality
						}))
						{
							continue;
						}
					}
					if (Game1.debrisSoundInterval <= 0f)
					{
						Game1.debrisSoundInterval = 10f;
						location.localSound("coin");
					}
					chunks.RemoveAt(i);
					continue;
				}
				if ((DebrisType)debrisType == DebrisType.NUMBERS && toHover != null)
				{
					relativeXPosition += chunk.xVelocity;
					chunk.position.X = toHover.Position.X + 32f + relativeXPosition;
					chunk.scale = Math.Min(2f, Math.Max(1f, 0.9f + Math.Abs(chunk.position.Y - (float)chunkFinalYLevel) / 128f));
					chunkFinalYLevel = toHover.getStandingY() + 8;
					if (timeSinceDoneBouncing > 250f)
					{
						chunk.alpha = Math.Max(0f, chunk.alpha - 0.033f);
					}
					if (!(toHover is Farmer) && !nonSpriteChunkColor.Equals(Color.Yellow) && !nonSpriteChunkColor.Equals(Color.Green))
					{
						nonSpriteChunkColor.R = (byte)Math.Max(Math.Min(255, 200 + (int)chunkType), Math.Min(Math.Min(255, 220 + (int)chunkType), 400.0 * Math.Sin((double)timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)));
						nonSpriteChunkColor.G = (byte)Math.Max(150 - (int)chunkType, Math.Min(255 - (int)chunkType, (nonSpriteChunkColor.R > 220) ? (300.0 * Math.Sin((double)timeSinceDoneBouncing / (Math.PI * 256.0) + Math.PI / 12.0)) : 0.0));
						nonSpriteChunkColor.B = (byte)Math.Max(0, Math.Min(255, (nonSpriteChunkColor.G > 200) ? (nonSpriteChunkColor.G - 20) : 0));
					}
				}
				chunk.position.X += chunk.xVelocity;
				chunk.position.Y -= chunk.yVelocity;
				if (movingFinalYLevel)
				{
					chunkFinalYLevel -= (int)Math.Ceiling((float)chunk.yVelocity / 2f);
					if (chunkFinalYLevel <= chunkFinalYTarget)
					{
						chunkFinalYLevel = chunkFinalYTarget;
						movingFinalYLevel = false;
					}
				}
				if ((DebrisType)debrisType == DebrisType.SQUARES && chunk.position.Y < (float)(chunkFinalYLevel - 96) && Game1.random.NextDouble() < 0.1)
				{
					chunk.position.Y = chunkFinalYLevel - Game1.random.Next(1, 21);
					chunk.yVelocity.Value = (float)Game1.random.Next(30, 80) / 40f;
					chunk.position.X = Game1.random.Next((int)(chunk.position.X - chunk.position.X % 64f + 1f), (int)(chunk.position.X - chunk.position.X % 64f + 64f));
				}
				if ((DebrisType)debrisType != DebrisType.SQUARES && chunk.bounces <= (floppingFish ? 65 : 2))
				{
					chunk.yVelocity.Value -= 0.4f;
				}
				bool destroyThisChunk = false;
				if (chunk.position.Y >= (float)chunkFinalYLevel && (bool)chunk.hasPassedRestingLineOnce && chunk.bounces <= (floppingFish ? 65 : 2))
				{
					if ((DebrisType)debrisType != DebrisType.LETTERS && (DebrisType)debrisType != DebrisType.NUMBERS && (DebrisType)debrisType != DebrisType.SPRITECHUNKS && ((DebrisType)debrisType != 0 || chunk.debrisType - chunk.debrisType % 2 == 8) && shouldControlThis(location))
					{
						location.playSound("shiny4");
					}
					chunk.bounces++;
					if ((bool)floppingFish)
					{
						chunk.yVelocity.Value = Math.Abs(chunk.yVelocity) * ((movingUp && chunk.bounces < 2) ? 0.6f : 0.9f);
						chunk.xVelocity.Value = (float)Game1.random.Next(-250, 250) / 100f;
					}
					else
					{
						chunk.yVelocity.Value = Math.Abs((float)chunk.yVelocity * 2f / 3f);
						chunk.rotationVelocity = ((Game1.random.NextDouble() < 0.5) ? (chunk.rotationVelocity / 2f) : ((0f - chunk.rotationVelocity) * 2f / 3f));
						chunk.xVelocity.Value -= (float)chunk.xVelocity / 2f;
					}
					Vector2 chunkTile = new Vector2((int)((chunk.position.X + 32f) / 64f), (int)((chunk.position.Y + 32f) / 64f));
					if ((DebrisType)debrisType != DebrisType.LETTERS && (DebrisType)debrisType != DebrisType.SPRITECHUNKS && (DebrisType)debrisType != DebrisType.NUMBERS && location.doesTileSinkDebris((int)chunkTile.X, (int)chunkTile.Y, debrisType))
					{
						destroyThisChunk = location.sinkDebris(this, chunkTile, chunk.position);
					}
				}
				if ((!chunk.hitWall && location.Map.GetLayer("Buildings").Tiles[(int)((chunk.position.X + 32f) / 64f), (int)((chunk.position.Y + 32f) / 64f)] != null) || location.Map.GetLayer("Back").Tiles[(int)((chunk.position.X + 32f) / 64f), (int)((chunk.position.Y + 32f) / 64f)] == null)
				{
					chunk.xVelocity.Value = 0f - (float)chunk.xVelocity;
					chunk.hitWall = true;
				}
				if (chunk.position.Y < (float)chunkFinalYLevel)
				{
					chunk.hasPassedRestingLineOnce.Value = true;
				}
				if (chunk.bounces > (floppingFish ? 65 : 2))
				{
					chunk.yVelocity.Value = 0f;
					chunk.xVelocity.Value = 0f;
					chunk.rotationVelocity = 0f;
				}
				chunk.rotation += chunk.rotationVelocity;
				if (destroyThisChunk)
				{
					chunks.RemoveAt(i);
				}
			}
			if (!anyCouldMove && shouldControlThis(location))
			{
				player.Value = null;
			}
			if (chunks.Count == 0)
			{
				return true;
			}
			return false;
		}

		public static string getNameOfDebrisTypeFromIntId(int id)
		{
			switch (id)
			{
			case 0:
			case 1:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.621");
			case 2:
			case 3:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.622");
			case 4:
			case 5:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.623");
			case 6:
			case 7:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.624");
			case 8:
			case 9:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.625");
			case 10:
			case 11:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.626");
			case 12:
			case 13:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.627");
			case 14:
			case 15:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.628");
			case 28:
			case 29:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.629");
			case 30:
			case 31:
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.630");
			default:
				return "???";
			}
		}

		public static bool getDebris(int which, int howMuch)
		{
			switch (which)
			{
			case 0:
				Game1.player.CopperPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.621"), howMuch, add: true, Color.Sienna));
				if (howMuch > 0)
				{
					Game1.stats.CopperFound += (uint)howMuch;
				}
				break;
			case 2:
				Game1.player.IronPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.622"), howMuch, add: true, Color.LightSlateGray));
				if (howMuch > 0)
				{
					Game1.stats.IronFound += (uint)howMuch;
				}
				break;
			case 4:
				Game1.player.CoalPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.623"), howMuch, add: true, Color.DimGray));
				if (howMuch > 0)
				{
					Game1.stats.CoalFound += (uint)howMuch;
				}
				break;
			case 6:
				Game1.player.GoldPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.624"), howMuch, add: true, Color.Gold));
				if (howMuch > 0)
				{
					Game1.stats.GoldFound += (uint)howMuch;
				}
				break;
			case 10:
				Game1.player.IridiumPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.626"), howMuch, add: true, Color.Purple));
				if (howMuch > 0)
				{
					Game1.stats.IridiumFound += (uint)howMuch;
				}
				break;
			case 12:
				Game1.player.WoodPieces += howMuch;
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.627"), howMuch, add: true, Color.Tan));
				if (howMuch > 0)
				{
					Game1.stats.SticksChopped += (uint)howMuch;
				}
				break;
			case 28:
				Game1.player.fuelLantern(howMuch * 2);
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Debris.cs.629"), howMuch * 2, add: true, Color.Goldenrod));
				break;
			case 8:
			{
				int j = Game1.random.Next(10, 50) * howMuch;
				j -= j % 5;
				Game1.player.Money += j;
				if (howMuch > 0)
				{
					Game1.stats.CoinsFound += (uint)howMuch;
				}
				break;
			}
			default:
				return false;
			}
			if (Game1.questOfTheDay != null && (bool)Game1.questOfTheDay.accepted && !Game1.questOfTheDay.completed && Game1.questOfTheDay is ResourceCollectionQuest)
			{
				((ResourceCollectionQuest)Game1.questOfTheDay).checkIfComplete(null, which, howMuch);
			}
			return true;
		}
	}
}
