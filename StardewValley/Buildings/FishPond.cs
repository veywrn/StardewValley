using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.FishPond;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Buildings
{
	public class FishPond : Building
	{
		public const int MAXIMUM_OCCUPANCY = 10;

		public static readonly float FISHING_MILLISECONDS = 1000f;

		public static readonly int HARVEST_BASE_EXP = 10;

		public static readonly float HARVEST_OUTPUT_EXP_MULTIPLIER = 0.04f;

		public static readonly int QUEST_BASE_EXP = 20;

		public static readonly float QUEST_SPAWNRATE_EXP_MULTIPIER = 5f;

		public const int NUMBER_OF_NETTING_STYLE_TYPES = 4;

		public readonly NetInt fishType = new NetInt(-1);

		public readonly NetInt lastUnlockedPopulationGate = new NetInt(0);

		public readonly NetBool hasCompletedRequest = new NetBool(value: false);

		public readonly NetRef<Object> sign = new NetRef<Object>();

		public readonly NetColor overrideWaterColor = new NetColor(Color.White);

		public readonly NetRef<Item> output = new NetRef<Item>();

		public readonly NetRef<Object> neededItem = new NetRef<Object>();

		public readonly NetIntDelta neededItemCount = new NetIntDelta(0);

		public readonly NetInt daysSinceSpawn = new NetInt(0);

		public readonly NetInt nettingStyle = new NetInt(0);

		public readonly NetInt seedOffset = new NetInt(0);

		public readonly NetBool hasSpawnedFish = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetMutex needsMutex = new NetMutex();

		[XmlIgnore]
		protected bool _hasAnimatedSpawnedFish;

		[XmlIgnore]
		protected float _delayUntilFishSilhouetteAdded;

		[XmlIgnore]
		protected int _numberOfFishToJump;

		[XmlIgnore]
		protected float _timeUntilFishHop;

		[XmlIgnore]
		protected Object _fishObject;

		[XmlIgnore]
		public List<PondFishSilhouette> _fishSilhouettes = new List<PondFishSilhouette>();

		[XmlIgnore]
		public List<JumpingFish> _jumpingFish = new List<JumpingFish>();

		[XmlIgnore]
		private readonly NetEvent0 animateHappyFishEvent = new NetEvent0();

		[XmlIgnore]
		public List<TemporaryAnimatedSprite> animations = new List<TemporaryAnimatedSprite>();

		[XmlIgnore]
		protected FishPondData _fishPondData;

		public int FishCount => currentOccupants.Value;

		public Item ItemWanted => null;

		public FishPond(BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
			UpdateMaximumOccupancy();
			fadeWhenPlayerIsBehind.Value = false;
			Reseed();
			_fishSilhouettes = new List<PondFishSilhouette>();
			_jumpingFish = new List<JumpingFish>();
		}

		public FishPond()
		{
			_fishSilhouettes = new List<PondFishSilhouette>();
			_jumpingFish = new List<JumpingFish>();
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(fishType, output, daysSinceSpawn, lastUnlockedPopulationGate, animateHappyFishEvent, hasCompletedRequest, neededItem, seedOffset, hasSpawnedFish, needsMutex.NetFields, neededItemCount, overrideWaterColor, sign, nettingStyle);
			animateHappyFishEvent.onEvent += AnimateHappyFish;
			fishType.fieldChangeVisibleEvent += OnFishTypeChanged;
		}

		public virtual void OnFishTypeChanged(NetInt field, int old_value, int new_value)
		{
			_fishSilhouettes.Clear();
			_jumpingFish.Clear();
			_fishObject = null;
		}

		public virtual void Reseed()
		{
			seedOffset.Value = DateTime.UtcNow.Millisecond;
		}

		public List<PondFishSilhouette> GetFishSilhouettes()
		{
			return _fishSilhouettes;
		}

		public void UpdateMaximumOccupancy()
		{
			GetFishPondData();
			if (_fishPondData == null)
			{
				return;
			}
			for (int i = 1; i <= 10; i++)
			{
				if (i <= lastUnlockedPopulationGate.Value)
				{
					maxOccupants.Set(i);
					continue;
				}
				if (_fishPondData.PopulationGates == null || !_fishPondData.PopulationGates.ContainsKey(i))
				{
					maxOccupants.Set(i);
					continue;
				}
				break;
			}
		}

		public FishPondData GetFishPondData()
		{
			if (fishType.Value <= 0)
			{
				return null;
			}
			List<FishPondData> list = Game1.content.Load<List<FishPondData>>("Data\\FishPondData");
			Object fish_item = GetFishObject();
			foreach (FishPondData data_entry in list)
			{
				bool fail = false;
				foreach (string required_tag in data_entry.RequiredTags)
				{
					if (!fish_item.HasContextTag(required_tag))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					_fishPondData = data_entry;
					if (_fishPondData.SpawnTime == -1)
					{
						int value = fish_item.Price;
						if (value <= 30)
						{
							_fishPondData.SpawnTime = 1;
						}
						else if (value <= 80)
						{
							_fishPondData.SpawnTime = 2;
						}
						else if (value <= 120)
						{
							_fishPondData.SpawnTime = 3;
						}
						else if (value <= 250)
						{
							_fishPondData.SpawnTime = 4;
						}
						else
						{
							_fishPondData.SpawnTime = 5;
						}
					}
					return _fishPondData;
				}
			}
			return null;
		}

		public Object GetFishProduce(Random random = null)
		{
			if (random == null)
			{
				random = Game1.random;
			}
			GetFishPondData();
			if (_fishPondData != null)
			{
				foreach (FishPondReward produced_item in _fishPondData.ProducedItems)
				{
					if (currentOccupants.Value >= produced_item.RequiredPopulation && !(random.NextDouble() > (double)produced_item.Chance))
					{
						Object o = new Object(produced_item.ItemID, random.Next(produced_item.MinQuantity, produced_item.MaxQuantity + 1));
						if ((int)o.parentSheetIndex == 812)
						{
							Color? c = TailoringMenu.GetDyeColor(GetFishObject());
							if (!c.HasValue)
							{
								c = Color.Orange;
							}
							if (fishType.Value == 698)
							{
								c = new Color(61, 55, 42);
							}
							o = new ColoredObject(produced_item.ItemID, random.Next(produced_item.MinQuantity, produced_item.MaxQuantity + 1), c.Value);
							o.name = Game1.objectInformation[fishType.Value].Split('/')[0] + " Roe";
							o.preserve.Value = Object.PreserveType.Roe;
							o.preservedParentSheetIndex.Value = fishType.Value;
							o.Price += Convert.ToInt32(Game1.objectInformation[fishType.Value].Split('/')[1]) / 2;
						}
						return o;
					}
				}
			}
			return null;
		}

		private Item CreateFishInstance()
		{
			return new Object(fishType, 1);
		}

		public override bool doAction(Vector2 tileLocation, Farmer who)
		{
			if ((int)daysOfConstructionLeft <= 0 && tileLocation.X >= (float)(int)tileX && tileLocation.X < (float)((int)tileX + (int)tilesWide) && tileLocation.Y >= (float)(int)tileY && tileLocation.Y < (float)((int)tileY + (int)tilesHigh))
			{
				if (who.isMoving())
				{
					Game1.haltAfterCheck = false;
				}
				if (who.ActiveObject != null && performActiveObjectDropInAction(who, probe: false))
				{
					return true;
				}
				if (output.Value != null)
				{
					Item item = output.Value;
					output.Value = null;
					if (who.addItemToInventoryBool(item))
					{
						Game1.playSound("coin");
						int bonusExperience = 0;
						if (item != null && item is Object)
						{
							bonusExperience = (int)((float)(item as Object).sellToStorePrice(-1L) * HARVEST_OUTPUT_EXP_MULTIPLIER);
						}
						who.gainExperience(1, bonusExperience + HARVEST_BASE_EXP);
					}
					else
					{
						output.Value = item;
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					}
					return true;
				}
				if (who.ActiveObject != null && HasUnresolvedNeeds() && !who.ActiveObject.bigCraftable.Value && who.ActiveObject.ParentSheetIndex == neededItem.Value.ParentSheetIndex)
				{
					if (neededItemCount.Value == 1)
					{
						showObjectThrownIntoPondAnimation(who, who.ActiveObject, delegate
						{
							if (neededItemCount.Value <= 0)
							{
								Game1.playSound("jingle1");
							}
						});
					}
					else
					{
						showObjectThrownIntoPondAnimation(who, who.ActiveObject);
					}
					who.reduceActiveItemByOne();
					if (who == Game1.player)
					{
						neededItemCount.Value--;
						if (neededItemCount.Value <= 0)
						{
							needsMutex.RequestLock(delegate
							{
								needsMutex.ReleaseLock();
								ResolveNeeds(who);
							});
							neededItemCount.Value = -1;
						}
					}
					if (neededItemCount.Value <= 0)
					{
						animateHappyFishEvent.Fire();
					}
					return true;
				}
				if (who.ActiveObject != null && (who.ActiveObject.Category == -4 || who.ActiveObject.ParentSheetIndex == 393 || who.ActiveObject.ParentSheetIndex == 397))
				{
					if (fishType.Value >= 0)
					{
						if (!isLegalFishForPonds(fishType))
						{
							string heldFishName3 = who.ActiveObject.DisplayName;
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:CantPutInPonds", heldFishName3.ToLower()));
							return true;
						}
						if (who.ActiveObject.ParentSheetIndex != (int)fishType)
						{
							string heldFishName2 = who.ActiveObject.DisplayName;
							if (who.ActiveObject.ParentSheetIndex == 393 || who.ActiveObject.ParentSheetIndex == 397)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishTypeCoral", heldFishName2));
							}
							else if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de)
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishType", heldFishName2, Game1.objectInformation[fishType].Split('/')[4]));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:WrongFishType", heldFishName2.ToLower(), Game1.objectInformation[fishType].Split('/')[4].ToLower()));
							}
							return true;
						}
						if ((int)currentOccupants >= (int)maxOccupants)
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:PondFull"));
							return true;
						}
						return addFishToPond(who, who.ActiveObject);
					}
					if (!isLegalFishForPonds(who.ActiveObject.ParentSheetIndex))
					{
						string heldFishName = who.ActiveObject.DisplayName;
						if (who.ActiveObject.HasContextTag("fish_legendary"))
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:CantPutInPonds", heldFishName));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:CantPutInPonds", heldFishName));
						}
						return true;
					}
					return addFishToPond(who, who.ActiveObject);
				}
				if ((int)fishType >= 0)
				{
					if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
					{
						Game1.playSound("bigSelect");
						Game1.activeClickableMenu = new PondQueryMenu(this);
						return true;
					}
				}
				else if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:NoFish"));
					return true;
				}
			}
			return base.doAction(tileLocation, who);
		}

		public void AnimateHappyFish()
		{
			_numberOfFishToJump = currentOccupants.Value;
			_timeUntilFishHop = 1f;
		}

		public Vector2 GetItemBucketTile()
		{
			return new Vector2((int)tileX + 4, (int)tileY + 4);
		}

		public Vector2 GetRequestTile()
		{
			return new Vector2((int)tileX + 2, (int)tileY + 2);
		}

		public Vector2 GetCenterTile()
		{
			return new Vector2((int)tileX + 2, (int)tileY + 2);
		}

		public void ResolveNeeds(Farmer who)
		{
			Reseed();
			hasCompletedRequest.Value = true;
			lastUnlockedPopulationGate.Value = maxOccupants.Value + 1;
			UpdateMaximumOccupancy();
			daysSinceSpawn.Value = 0;
			int bonusExperience = 0;
			FishPondData fishData = GetFishPondData();
			if (fishData != null)
			{
				bonusExperience = (int)((float)fishData.SpawnTime * QUEST_SPAWNRATE_EXP_MULTIPIER);
			}
			who.gainExperience(1, bonusExperience + QUEST_BASE_EXP);
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)seedOffset);
			Game1.showGlobalMessage(PondQueryMenu.getCompletedRequestString(this, GetFishObject(), r));
		}

		public override void resetLocalState()
		{
			base.resetLocalState();
			_jumpingFish.Clear();
			while (_fishSilhouettes.Count < currentOccupants.Value)
			{
				PondFishSilhouette silhouette = new PondFishSilhouette(this);
				_fishSilhouettes.Add(silhouette);
				silhouette.position = (GetCenterTile() + new Vector2(Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)((int)tilesWide - 2), Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)((int)tilesHigh - 2))) * 64f;
			}
		}

		private bool isLegalFishForPonds(int type)
		{
			List<FishPondData> fish_pond_data = Game1.content.Load<List<FishPondData>>("Data\\FishPondData");
			Object fish_item = new Object(type, 1);
			if (fish_item.HasContextTag("fish_legendary"))
			{
				return false;
			}
			foreach (FishPondData item in fish_pond_data)
			{
				bool fail = false;
				foreach (string required_tag in item.RequiredTags)
				{
					if (!fish_item.HasContextTag(required_tag))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					return true;
				}
			}
			return false;
		}

		private void showObjectThrownIntoPondAnimation(Farmer who, Object whichObject, DelayedAction.delayedBehavior callback = null)
		{
			who.faceGeneralDirection(GetCenterTile() * 64f + new Vector2(32f, 32f));
			if (who.FacingDirection == 1 || who.FacingDirection == 3)
			{
				float distance2 = Vector2.Distance(who.position, GetCenterTile() * 64f);
				float verticalDistance = GetCenterTile().Y * 64f + 32f - who.position.Y;
				distance2 -= 8f;
				float gravity = 0.0025f;
				float velocity = (float)((double)distance2 * Math.Sqrt(gravity / (2f * (distance2 + 96f))));
				float t2 = 2f * (velocity / gravity) + (float)((Math.Sqrt(velocity * velocity + 2f * gravity * 96f) - (double)velocity) / (double)gravity);
				t2 += verticalDistance;
				float xVelocityReduction = 0f;
				if (verticalDistance > 0f)
				{
					xVelocityReduction = verticalDistance / 832f;
					t2 += xVelocityReduction * 200f;
				}
				Game1.playSound("throwDownITem");
				List<TemporaryAnimatedSprite> fishTossSprites = new List<TemporaryAnimatedSprite>();
				fishTossSprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichObject.parentSheetIndex, 16, 16), who.position + new Vector2(0f, -64f), flipped: false, 0f, Color.White)
				{
					scale = 4f,
					layerDepth = 1f,
					totalNumberOfLoops = 1,
					interval = t2,
					motion = new Vector2((float)((who.FacingDirection != 3) ? 1 : (-1)) * (velocity - xVelocityReduction), (0f - velocity) * 3f / 2f),
					acceleration = new Vector2(0f, gravity),
					timeBasedMotion = true
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, GetCenterTile() * 64f, flicker: false, flipped: false)
				{
					delayBeforeAnimationStart = (int)t2,
					layerDepth = (((float)(int)tileY + 0.5f) * 64f + 2f) / 10000f
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, GetCenterTile() * 64f, flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
				{
					delayBeforeAnimationStart = (int)t2
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, GetCenterTile() * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
				{
					delayBeforeAnimationStart = (int)t2
				});
				fishTossSprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, GetCenterTile() * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
				{
					delayBeforeAnimationStart = (int)t2
				});
				if (who.IsLocalPlayer)
				{
					DelayedAction.playSoundAfterDelay("waterSlosh", (int)t2, who.currentLocation);
					if (callback != null)
					{
						DelayedAction.functionAfterDelay(callback, (int)t2);
					}
				}
				if (fishType.Value >= 0 && whichObject.ParentSheetIndex == fishType.Value)
				{
					_delayUntilFishSilhouetteAdded = t2 / 1000f;
				}
				Game1.multiplayer.broadcastSprites(who.currentLocation, fishTossSprites);
				return;
			}
			float distance3 = Vector2.Distance(who.position, GetCenterTile() * 64f);
			float height = Math.Abs(distance3);
			if (who.FacingDirection == 0)
			{
				distance3 = 0f - distance3;
				height += 64f;
			}
			float horizontalDistance = GetCenterTile().X * 64f - who.position.X;
			float gravity2 = 0.0025f;
			float velocity2 = (float)Math.Sqrt(2f * gravity2 * height);
			float t6 = (float)(Math.Sqrt(2f * (height - distance3) / gravity2) + (double)(velocity2 / gravity2));
			t6 *= 1.05f;
			t6 = ((who.FacingDirection != 0) ? (t6 * 2.5f) : (t6 * 0.7f));
			t6 -= Math.Abs(horizontalDistance) / ((who.FacingDirection == 0) ? 100f : 2f);
			Game1.playSound("throwDownITem");
			List<TemporaryAnimatedSprite> fishTossSprites2 = new List<TemporaryAnimatedSprite>();
			fishTossSprites2.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, whichObject.parentSheetIndex, 16, 16), who.position + new Vector2(0f, -64f), flipped: false, 0f, Color.White)
			{
				scale = 4f,
				layerDepth = 1f,
				totalNumberOfLoops = 1,
				interval = t6,
				motion = new Vector2(horizontalDistance / ((who.FacingDirection == 0) ? 900f : 1000f), 0f - velocity2),
				acceleration = new Vector2(0f, gravity2),
				timeBasedMotion = true
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite(28, 100f, 2, 1, GetCenterTile() * 64f, flicker: false, flipped: false)
			{
				delayBeforeAnimationStart = (int)t6,
				layerDepth = (((float)(int)tileY + 0.5f) * 64f + 2f) / 10000f
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 55f, 8, 0, GetCenterTile() * 64f, flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
			{
				delayBeforeAnimationStart = (int)t6
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 65f, 8, 0, GetCenterTile() * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
			{
				delayBeforeAnimationStart = (int)t6
			});
			fishTossSprites2.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 75f, 8, 0, GetCenterTile() * 64f + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 32)), flicker: false, Game1.random.NextDouble() < 0.5, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f)
			{
				delayBeforeAnimationStart = (int)t6
			});
			if (who.IsLocalPlayer)
			{
				DelayedAction.playSoundAfterDelay("waterSlosh", (int)t6, who.currentLocation);
				if (callback != null)
				{
					DelayedAction.functionAfterDelay(callback, (int)t6);
				}
			}
			if (fishType.Value >= 0 && whichObject.ParentSheetIndex == fishType.Value)
			{
				_delayUntilFishSilhouetteAdded = t6 / 1000f;
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, fishTossSprites2);
		}

		private bool addFishToPond(Farmer who, Object fish)
		{
			who.reduceActiveItemByOne();
			if ((int)currentOccupants == 0)
			{
				fishType.Value = fish.ParentSheetIndex;
				_fishPondData = null;
				UpdateMaximumOccupancy();
			}
			currentOccupants.Value++;
			showObjectThrownIntoPondAnimation(who, fish);
			return true;
		}

		public override void dayUpdate(int dayOfMonth)
		{
			hasSpawnedFish.Value = false;
			_hasAnimatedSpawnedFish = false;
			if (hasCompletedRequest.Value)
			{
				neededItem.Value = null;
				neededItemCount.Set(-1);
				hasCompletedRequest.Value = false;
			}
			GetFishPondData();
			if ((int)currentOccupants > 0)
			{
				Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)tileX * 1000 + (int)tileY * 2000);
				if (r.NextDouble() < (double)Utility.Lerp(0.15f, 0.95f, (float)(int)currentOccupants / 10f))
				{
					output.Value = GetFishProduce(r);
				}
				daysSinceSpawn.Value += 1;
				if (daysSinceSpawn.Value > GetFishPondData().SpawnTime)
				{
					daysSinceSpawn.Value = GetFishPondData().SpawnTime;
				}
				if (daysSinceSpawn.Value >= GetFishPondData().SpawnTime)
				{
					KeyValuePair<int, int> data = _GetNeededItemData();
					if (data.Key != -1)
					{
						if (currentOccupants.Value >= maxOccupants.Value && neededItem.Value == null)
						{
							neededItem.Value = new Object(data.Key, 1);
							neededItemCount.Set(data.Value);
						}
					}
					else
					{
						SpawnFish();
					}
				}
				if (currentOccupants.Value == 10 && (int)fishType == 717)
				{
					foreach (Farmer f in Game1.getAllFarmers())
					{
						if (!f.mailReceived.Contains("FullCrabPond"))
						{
							f.mailReceived.Add("FullCrabPond");
							f.activeDialogueEvents.Add("FullCrabPond", 14);
						}
					}
				}
				doFishSpecificWaterColoring();
			}
			base.dayUpdate(dayOfMonth);
		}

		private void doFishSpecificWaterColoring()
		{
			overrideWaterColor.Value = Color.White;
			if ((int)fishType == 162 && lastUnlockedPopulationGate.Value >= 2)
			{
				overrideWaterColor.Value = new Color(250, 30, 30);
			}
			else if ((int)fishType == 796 && (int)currentOccupants > 2)
			{
				overrideWaterColor.Value = new Color(60, 255, 60);
			}
			else if ((int)fishType == 795 && (int)currentOccupants > 2)
			{
				overrideWaterColor.Value = new Color(120, 20, 110);
			}
			else if ((int)fishType == 155 && (int)currentOccupants > 2)
			{
				overrideWaterColor.Value = new Color(150, 100, 200);
			}
		}

		public bool JumpFish()
		{
			PondFishSilhouette fish_silhouette2 = null;
			if (_fishSilhouettes.Count == 0)
			{
				return false;
			}
			fish_silhouette2 = Utility.GetRandom(_fishSilhouettes);
			_fishSilhouettes.Remove(fish_silhouette2);
			_jumpingFish.Add(new JumpingFish(this, fish_silhouette2.position, (GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f));
			return true;
		}

		public void SpawnFish()
		{
			if (currentOccupants.Value < maxOccupants.Value && currentOccupants.Value > 0)
			{
				hasSpawnedFish.Value = true;
				daysSinceSpawn.Value = 0;
				currentOccupants.Value += 1;
				if (currentOccupants.Value > maxOccupants.Value)
				{
					currentOccupants.Value = maxOccupants.Value;
				}
			}
		}

		public override bool performActiveObjectDropInAction(Farmer who, bool probe)
		{
			if (who.ActiveObject != null && (bool)who.ActiveObject.bigCraftable && who.ActiveObject.Name.Contains("Sign") && (sign.Value == null || who.ActiveObject.parentSheetIndex != sign.Value.parentSheetIndex))
			{
				if (probe)
				{
					return true;
				}
				Object oldSign = sign.Value;
				sign.Value = (Object)who.ActiveObject.getOne();
				who.reduceActiveItemByOne();
				if (oldSign != null)
				{
					Game1.createItemDebris(oldSign, new Vector2((float)(int)tileX + 0.5f, (int)tileY + (int)tilesHigh) * 64f, 3, who.currentLocation);
				}
				who.currentLocation.playSound("axe");
				return true;
			}
			return base.performActiveObjectDropInAction(who, probe);
		}

		public override void performToolAction(Tool t, int tileX, int tileY)
		{
			if (t != null && (t is Axe || t is Pickaxe) && sign.Value != null)
			{
				if (t.getLastFarmerToUse() != null)
				{
					Game1.createItemDebris((Object)sign, new Vector2((float)(int)base.tileX + 0.5f, (int)base.tileY + (int)tilesHigh) * 64f, 3, t.getLastFarmerToUse().currentLocation);
				}
				sign.Value = null;
				t.getLastFarmerToUse().currentLocation.playSound("hammer");
			}
			base.performToolAction(t, tileX, tileY);
		}

		public override void performActionOnConstruction(GameLocation location)
		{
			base.performActionOnConstruction(location);
			nettingStyle.Value = ((int)tileX / 3 + (int)tileY / 3) % 3;
		}

		public override void performActionOnBuildingPlacement()
		{
			base.performActionOnBuildingPlacement();
			nettingStyle.Value = ((int)tileX / 3 + (int)tileY / 3) % 3;
		}

		public bool HasUnresolvedNeeds()
		{
			if (neededItem.Value != null && _GetNeededItemData().Key != -1)
			{
				return !hasCompletedRequest.Value;
			}
			return false;
		}

		private KeyValuePair<int, int> _GetNeededItemData()
		{
			if (currentOccupants.Value < (int)maxOccupants)
			{
				return new KeyValuePair<int, int>(-1, 0);
			}
			GetFishPondData();
			if (_fishPondData.PopulationGates != null)
			{
				if (maxOccupants.Value + 1 <= lastUnlockedPopulationGate.Value)
				{
					return new KeyValuePair<int, int>(-1, 0);
				}
				if (_fishPondData.PopulationGates.ContainsKey(maxOccupants.Value + 1))
				{
					Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + (int)tileX * 1000 + (int)tileY * 2000);
					int item_index = -1;
					int item_count = 1;
					string[] split_data = Utility.GetRandom(_fishPondData.PopulationGates[maxOccupants.Value + 1], r).Split(' ');
					if (split_data.Length >= 1)
					{
						item_index = Convert.ToInt32(split_data[0]);
					}
					if (split_data.Length >= 3)
					{
						item_count = r.Next(Convert.ToInt32(split_data[1]), Convert.ToInt32(split_data[2]) + 1);
					}
					else if (split_data.Length >= 2)
					{
						item_count = Convert.ToInt32(split_data[1]);
					}
					return new KeyValuePair<int, int>(item_index, item_count);
				}
			}
			return new KeyValuePair<int, int>(-1, 0);
		}

		public void ClearPond()
		{
			_hasAnimatedSpawnedFish = false;
			hasSpawnedFish.Value = false;
			_fishSilhouettes.Clear();
			_jumpingFish.Clear();
			_fishObject = null;
			currentOccupants.Value = 0;
			daysSinceSpawn.Value = 0;
			neededItem.Value = null;
			neededItemCount.Value = -1;
			lastUnlockedPopulationGate.Value = 0;
			fishType.Value = -1;
			Reseed();
			overrideWaterColor.Value = Color.White;
		}

		public Object CatchFish()
		{
			if (currentOccupants.Value == 0)
			{
				return null;
			}
			currentOccupants.Value--;
			return (Object)CreateFishInstance();
		}

		public Object GetFishObject()
		{
			if (_fishObject == null)
			{
				_fishObject = new Object(fishType.Value, 1);
			}
			return _fishObject;
		}

		public override void Update(GameTime time)
		{
			needsMutex.Update(Game1.getFarm());
			animateHappyFishEvent.Poll();
			if (!_hasAnimatedSpawnedFish && hasSpawnedFish.Value && _numberOfFishToJump <= 0 && Utility.isOnScreen((GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f, 64))
			{
				_hasAnimatedSpawnedFish = true;
				if (fishType.Value != 393 && fishType.Value != 397)
				{
					_numberOfFishToJump = 1;
					_timeUntilFishHop = Utility.RandomFloat(2f, 5f);
				}
			}
			if (_delayUntilFishSilhouetteAdded > 0f)
			{
				_delayUntilFishSilhouetteAdded -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_delayUntilFishSilhouetteAdded < 0f)
				{
					_delayUntilFishSilhouetteAdded = 0f;
				}
			}
			if (_numberOfFishToJump > 0 && _timeUntilFishHop > 0f)
			{
				_timeUntilFishHop -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_timeUntilFishHop <= 0f && JumpFish())
				{
					_numberOfFishToJump--;
					_timeUntilFishHop = Utility.RandomFloat(0.15f, 0.25f);
				}
			}
			while (_fishSilhouettes.Count > currentOccupants.Value - _jumpingFish.Count)
			{
				_fishSilhouettes.RemoveAt(0);
			}
			if (_delayUntilFishSilhouetteAdded <= 0f)
			{
				while (_fishSilhouettes.Count < currentOccupants.Value - _jumpingFish.Count)
				{
					_fishSilhouettes.Add(new PondFishSilhouette(this));
				}
			}
			for (int i = 0; i < _fishSilhouettes.Count; i++)
			{
				_fishSilhouettes[i].Update((float)time.ElapsedGameTime.TotalSeconds);
			}
			for (int j = 0; j < _jumpingFish.Count; j++)
			{
				if (_jumpingFish[j].Update((float)time.ElapsedGameTime.TotalSeconds))
				{
					PondFishSilhouette new_silhouette = new PondFishSilhouette(this);
					new_silhouette.position = _jumpingFish[j].position;
					_fishSilhouettes.Add(new_silhouette);
					_jumpingFish.RemoveAt(j);
					j--;
				}
			}
			base.Update(time);
		}

		public override bool isTileFishable(Vector2 tile)
		{
			if ((int)daysOfConstructionLeft > 0)
			{
				return false;
			}
			if (tile.X > (float)(int)tileX && tile.X < (float)((int)tileX + (int)tilesWide - 1) && tile.Y > (float)(int)tileY)
			{
				return tile.Y < (float)((int)tileY + (int)tilesHigh - 1);
			}
			return false;
		}

		public override bool CanRefillWateringCan()
		{
			if ((int)daysOfConstructionLeft <= 0)
			{
				return true;
			}
			return false;
		}

		public override Rectangle getSourceRectForMenu()
		{
			return new Rectangle(0, 0, 80, 80);
		}

		public override void drawInMenu(SpriteBatch b, int x, int y)
		{
			y += 32;
			drawShadow(b, x, y);
			b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 80, 80, 80), new Color(60, 126, 150) * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
			for (int yWater = tileY; yWater < (int)tileY + 5; yWater++)
			{
				for (int xWater = tileX; xWater < (int)tileX + 4; xWater++)
				{
					bool num = yWater == (int)tileY + 4;
					bool topY = yWater == (int)tileY;
					if (num)
					{
						b.Draw(Game1.mouseCursors, new Vector2(x + xWater * 64 + 32, y + (yWater + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5), Game1.currentLocation.waterColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, new Vector2(x + xWater * 64 + 32, y + yWater * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f)), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((xWater + yWater) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - Game1.currentLocation.waterPosition)) : 0)), Game1.currentLocation.waterColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
					}
				}
			}
			b.Draw(texture.Value, new Vector2(x, y), new Rectangle(0, 0, 80, 80), color.Value * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
			b.Draw(texture.Value, new Vector2(x + 64, y + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0)), new Rectangle(16, 160, 48, 7), color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(texture.Value, new Vector2(x, y - 128), new Rectangle(80, 0, 80, 48), color.Value * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 1f);
		}

		public override void OnEndMove()
		{
			foreach (PondFishSilhouette fishSilhouette in _fishSilhouettes)
			{
				fishSilhouette.position = (GetCenterTile() + new Vector2(Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)((int)tilesWide - 2), Utility.Lerp(-0.5f, 0.5f, (float)Game1.random.NextDouble()) * (float)((int)tilesHigh - 2))) * 64f;
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (base.isMoving)
			{
				return;
			}
			if ((int)daysOfConstructionLeft > 0)
			{
				drawInConstruction(b);
				return;
			}
			for (int l = animations.Count - 1; l >= 0; l--)
			{
				animations[l].draw(b);
			}
			drawShadow(b);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), new Rectangle(0, 80, 80, 80), (overrideWaterColor.Equals(Color.White) ? new Color(60, 126, 150) : ((Color)overrideWaterColor)) * alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f - 3f) / 10000f);
			for (int y = tileY; y < (int)tileY + 5; y++)
			{
				for (int x = tileX; x < (int)tileX + 4; x++)
				{
					bool num = y == (int)tileY + 4;
					bool topY = y == (int)tileY;
					if (num)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, (y + 1) * 64 - (int)Game1.currentLocation.waterPosition - 32)), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)), 64, 32 + (int)Game1.currentLocation.waterPosition - 5), overrideWaterColor.Equals(Color.White) ? ((Color)Game1.currentLocation.waterColor) : (overrideWaterColor.Value * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f - 2f) / 10000f);
					}
					else
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32 - (int)((!topY) ? Game1.currentLocation.waterPosition : 0f))), new Rectangle(Game1.currentLocation.waterAnimationIndex * 64, 2064 + (((x + y) % 2 != 0) ? ((!Game1.currentLocation.waterTileFlip) ? 128 : 0) : (Game1.currentLocation.waterTileFlip ? 128 : 0)) + (topY ? ((int)Game1.currentLocation.waterPosition) : 0), 64, 64 + (topY ? ((int)(0f - Game1.currentLocation.waterPosition)) : 0)), overrideWaterColor.Equals(Color.White) ? ((Color)Game1.currentLocation.waterColor) : (overrideWaterColor.Value * 0.5f), 0f, Vector2.Zero, 1f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f - 2f) / 10000f);
					}
				}
			}
			if (overrideWaterColor.Value.Equals(Color.White))
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 64, (int)tileY * 64 + 44 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2500.0 < 1250.0) ? 4 : 0))), new Rectangle(16, 160, 48, 7), color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f);
			}
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), new Rectangle(0, 0, 80, 80), color.Value * alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, ((float)(int)tileY + 0.5f) * 64f / 10000f);
			if (nettingStyle.Value < 3)
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64 - 128)), new Rectangle(80, (int)nettingStyle * 48, 80, 48), color.Value * alpha, 0f, new Vector2(0f, 80f), 4f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 2f) / 10000f);
			}
			if (sign.Value != null)
			{
				b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 8, (int)tileY * 64 + (int)tilesHigh * 64 - 128 - 32)), Game1.getSourceRectForStandardTileSheet(Game1.bigCraftableSpriteSheet, sign.Value.parentSheetIndex, 16, 32), color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 2f) / 10000f);
				if (fishType.Value != -1)
				{
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 8 + 8 - 4, (int)tileY * 64 + (int)tilesHigh * 64 - 128 - 8 + 4)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, fishType.Value, 16, 16), Color.Black * 0.4f * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 3f) / 10000f);
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 8 + 8 - 1, (int)tileY * 64 + (int)tilesHigh * 64 - 128 - 8 + 1)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, fishType.Value, 16, 16), color.Value * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 4f) / 10000f);
					Utility.drawTinyDigits(currentOccupants.Value, b, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 32 + 8 + ((currentOccupants.Value < 10) ? 8 : 0), (int)tileY * 64 + (int)tilesHigh * 64 - 96)), 3f, (((float)(int)tileY + 0.5f) * 64f + 5f) / 10000f, Color.LightYellow * alpha);
				}
			}
			if (_fishObject != null && ((int)_fishObject.parentSheetIndex == 393 || (int)_fishObject.parentSheetIndex == 397))
			{
				for (int k = 0; k < (int)currentOccupants; k++)
				{
					Vector2 drawOffset = Vector2.Zero;
					int drawI = (k + seedOffset.Value) % 10;
					switch (drawI)
					{
					case 0:
						drawOffset = new Vector2(0f, 0f);
						break;
					case 1:
						drawOffset = new Vector2(48f, 32f);
						break;
					case 2:
						drawOffset = new Vector2(80f, 72f);
						break;
					case 3:
						drawOffset = new Vector2(140f, 28f);
						break;
					case 4:
						drawOffset = new Vector2(96f, 0f);
						break;
					case 5:
						drawOffset = new Vector2(0f, 96f);
						break;
					case 6:
						drawOffset = new Vector2(140f, 80f);
						break;
					case 7:
						drawOffset = new Vector2(64f, 120f);
						break;
					case 8:
						drawOffset = new Vector2(140f, 140f);
						break;
					case 9:
						drawOffset = new Vector2(0f, 150f);
						break;
					}
					b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 64 + 7, (int)tileY * 64 + 64 + 32) + drawOffset), Game1.shadowTexture.Bounds, color.Value * alpha, 0f, Vector2.Zero, 3f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f - 2f) / 10000f - 1.1E-05f);
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 64, (int)tileY * 64 + 64) + drawOffset), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, fishType.Value, 16, 16), color.Value * alpha * 0.75f, 0f, Vector2.Zero, 3f, (drawI % 3 == 0) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f - 2f) / 10000f - 1E-05f);
				}
			}
			else
			{
				for (int j = 0; j < _fishSilhouettes.Count; j++)
				{
					_fishSilhouettes[j].Draw(b);
				}
			}
			for (int i = 0; i < _jumpingFish.Count; i++)
			{
				_jumpingFish[i].Draw(b);
			}
			if (HasUnresolvedNeeds())
			{
				Vector2 drawn_position = GetRequestTile() * 64f;
				drawn_position += 64f * new Vector2(0.5f, 0.5f);
				float y_offset2 = 3f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				float bubble_layer_depth2 = (drawn_position.Y + 160f) / 10000f + 1E-06f;
				drawn_position.Y += y_offset2 - 32f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, drawn_position), new Rectangle(403, 496, 5, 14), Color.White * 0.75f, 0f, new Vector2(2f, 14f), 4f, SpriteEffects.None, bubble_layer_depth2);
			}
			if (output.Value != null)
			{
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64) + new Vector2(65f, 59f) * 4f), new Rectangle(0, 160, 15, 16), color.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (((float)(int)tileY + 0.5f) * 64f + 1f) / 10000f);
				Vector2 value = GetItemBucketTile() * 64f;
				float y_offset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
				Vector2 bubble_draw_position = value + new Vector2(0f, -2f) * 64f + new Vector2(0f, y_offset);
				Vector2 item_relative_to_bubble = new Vector2(40f, 36f);
				float bubble_layer_depth = (value.Y + 64f) / 10000f + 1E-06f;
				float item_layer_depth = (value.Y + 64f) / 10000f + 1E-05f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position), new Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, bubble_layer_depth);
				b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, output.Value.parentSheetIndex, 16, 16), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth);
				if (output.Value is ColoredObject)
				{
					b.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, bubble_draw_position + item_relative_to_bubble), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)output.Value.parentSheetIndex + 1, 16, 16), (output.Value as ColoredObject).color.Value * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, item_layer_depth + 1E-05f);
				}
			}
		}
	}
}
