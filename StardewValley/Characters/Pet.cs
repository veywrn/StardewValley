using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Characters
{
	public class Pet : NPC
	{
		public const int bedTime = 2000;

		public const int maxFriendship = 1000;

		public const int behavior_Walk = 0;

		public const int behavior_Sleep = 1;

		public const int behavior_SitDown = 2;

		public const int frame_basicSit = 18;

		[XmlElement("whichBreed")]
		public readonly NetInt whichBreed = new NetInt();

		private readonly NetInt netCurrentBehavior = new NetInt();

		[XmlIgnore]
		public readonly NetEvent1Field<string, NetString> petAnimationEvent = new NetEvent1Field<string, NetString>();

		[XmlIgnore]
		protected int _currentBehavior = -1;

		[XmlIgnore]
		protected int _lastDirection = -1;

		[XmlElement("lastPetDay")]
		public NetLongDictionary<int, NetInt> lastPetDay = new NetLongDictionary<int, NetInt>();

		[XmlElement("grantedFriendshipForPet")]
		public NetBool grantedFriendshipForPet = new NetBool(value: false);

		[XmlElement("friendshipTowardFarmer")]
		public NetInt friendshipTowardFarmer = new NetInt(0);

		public NetBool isSleepingOnFarmerBed = new NetBool(value: false);

		[XmlIgnore]
		public readonly NetMutex mutex = new NetMutex();

		private int pushingTimer;

		public int CurrentBehavior
		{
			get
			{
				return netCurrentBehavior.Value;
			}
			set
			{
				if (netCurrentBehavior.Value != value)
				{
					netCurrentBehavior.Value = value;
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(netCurrentBehavior, whichBreed, friendshipTowardFarmer, grantedFriendshipForPet, mutex.NetFields, lastPetDay, petAnimationEvent, isSleepingOnFarmerBed);
			name.FilterStringEvent += Utility.FilterDirtyWords;
			petAnimationEvent.onEvent += OnPetAnimationEvent;
			friendshipTowardFarmer.fieldChangeVisibleEvent += delegate
			{
				GrantLoveMailIfNecessary();
			};
			isSleepingOnFarmerBed.fieldChangeVisibleEvent += delegate
			{
				UpdateSleepingOnBed();
			};
			whichBreed.fieldChangeVisibleEvent += delegate
			{
				reloadBreedSprite();
			};
		}

		protected void _FlipFrames()
		{
		}

		public virtual void OnPetAnimationEvent(string animation_event)
		{
		}

		public override void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
		{
			base.behaviorOnFarmerLocationEntry(location, who);
			if (location is Farm && Game1.timeOfDay >= 2000 && !location.farmers.Any())
			{
				if (CurrentBehavior != 1 || base.currentLocation is Farm)
				{
					Game1.player.team.requestPetWarpHomeEvent.Fire(Game1.player.UniqueMultiplayerID);
				}
			}
			else if (Game1.timeOfDay < 2000 && Game1.random.NextDouble() < 0.5 && _currentBehavior != 1)
			{
				CurrentBehavior = 1;
				_OnNewBehavior();
				Sprite.UpdateSourceRect();
			}
			UpdateSleepingOnBed();
		}

		public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
		{
			base.behaviorOnLocalFarmerLocationEntry(location);
			netCurrentBehavior.CancelInterpolation();
			if (netCurrentBehavior.Value == 1)
			{
				position.NetFields.CancelInterpolation();
				if (_currentBehavior != 1)
				{
					_OnNewBehavior();
					Sprite.UpdateSourceRect();
				}
			}
			UpdateSleepingOnBed();
		}

		public override bool canTalk()
		{
			return false;
		}

		public virtual string getPetTextureName()
		{
			return "Animals\\dog" + ((whichBreed.Value == 0) ? "" : string.Concat(whichBreed.Value));
		}

		public void reloadBreedSprite()
		{
			if (Sprite != null)
			{
				Sprite.LoadTexture(getPetTextureName());
			}
		}

		public override void reloadSprite()
		{
			reloadBreedSprite();
			base.DefaultPosition = new Vector2(54f, 8f) * 64f;
			base.HideShadow = true;
			base.Breather = false;
			setAtFarmPosition();
		}

		public void warpToFarmHouse(Farmer who)
		{
			isSleepingOnFarmerBed.Value = false;
			FarmHouse farmHouse = Utility.getHomeOfFarmer(who);
			Vector2 sleepTile3 = Vector2.Zero;
			int tries = 0;
			sleepTile3 = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
			List<Furniture> rugs = new List<Furniture>();
			foreach (Furniture house_furniture in farmHouse.furniture)
			{
				if ((int)house_furniture.furniture_type == 12)
				{
					rugs.Add(house_furniture);
				}
			}
			BedFurniture player_bed = farmHouse.GetPlayerBed();
			if (player_bed != null && !Game1.newDay && Game1.timeOfDay >= 2000 && ((this is Cat && Game1.random.NextDouble() <= 0.75) || Game1.random.NextDouble() <= 0.05000000074505806))
			{
				sleepTile3 = Utility.PointToVector2(player_bed.GetBedSpot()) + new Vector2(-1f, 0f);
				Game1.warpCharacter(this, farmHouse, sleepTile3);
				base.NetFields.CancelInterpolation();
				CurrentBehavior = 1;
				isSleepingOnFarmerBed.Value = true;
				foreach (Furniture furniture in farmHouse.furniture)
				{
					if (furniture is BedFurniture && furniture.getBoundingBox(furniture.TileLocation).Intersects(GetBoundingBox()))
					{
						(furniture as BedFurniture).ReserveForNPC();
						break;
					}
				}
				UpdateSleepingOnBed();
				_OnNewBehavior();
				Sprite.UpdateSourceRect();
				return;
			}
			if (Game1.random.NextDouble() <= 0.30000001192092896)
			{
				sleepTile3 = Utility.PointToVector2(farmHouse.getBedSpot()) + new Vector2(0f, 2f);
			}
			else if (Game1.random.NextDouble() <= 0.5)
			{
				Furniture rug = Utility.GetRandom(rugs, Game1.random);
				if (rug != null)
				{
					sleepTile3 = new Vector2(rug.boundingBox.Left / 64, rug.boundingBox.Center.Y / 64);
				}
			}
			for (; tries < 50; tries++)
			{
				if (farmHouse.canPetWarpHere(sleepTile3) && farmHouse.isTileLocationTotallyClearAndPlaceable(sleepTile3) && farmHouse.isTileLocationTotallyClearAndPlaceable(sleepTile3 + new Vector2(1f, 0f)) && !farmHouse.isTileOnWall((int)sleepTile3.X, (int)sleepTile3.Y))
				{
					break;
				}
				sleepTile3 = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
			}
			if (tries < 50)
			{
				Game1.warpCharacter(this, farmHouse, sleepTile3);
				CurrentBehavior = 1;
			}
			else
			{
				faceDirection(2);
				Game1.warpCharacter(this, "Farm", (Game1.getLocationFromName("Farm") as Farm).GetPetStartLocation());
			}
			UpdateSleepingOnBed();
			_OnNewBehavior();
			Sprite.UpdateSourceRect();
		}

		public virtual void UpdateSleepingOnBed()
		{
			drawOnTop = false;
			collidesWithOtherCharacters.Value = !isSleepingOnFarmerBed.Value;
			farmerPassesThrough = isSleepingOnFarmerBed.Value;
		}

		public override void dayUpdate(int dayOfMonth)
		{
			isSleepingOnFarmerBed.Value = false;
			UpdateSleepingOnBed();
			base.DefaultPosition = new Vector2(54f, 8f) * 64f;
			Sprite.loop = false;
			base.Breather = false;
			if (Game1.isRaining)
			{
				CurrentBehavior = 2;
				warpToFarmHouse(Game1.player);
			}
			else if (base.currentLocation is FarmHouse)
			{
				setAtFarmPosition();
			}
			if (base.currentLocation is Farm && Game1.IsMasterGame)
			{
				if ((base.currentLocation as Farm).petBowlWatered.Value)
				{
					friendshipTowardFarmer.Set(Math.Min(1000, friendshipTowardFarmer.Value + 6));
				}
				(base.currentLocation as Farm).petBowlWatered.Set(newValue: false);
			}
			Halt();
			CurrentBehavior = 1;
			grantedFriendshipForPet.Set(newValue: false);
			_OnNewBehavior();
			Sprite.UpdateSourceRect();
		}

		public void GrantLoveMailIfNecessary()
		{
			if (friendshipTowardFarmer.Value >= 1000)
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer != null && !farmer.mailReceived.Contains("petLoveMessage"))
					{
						if (farmer == Game1.player)
						{
							Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", base.displayName));
						}
						farmer.mailReceived.Add("petLoveMessage");
					}
				}
			}
		}

		public void setAtFarmPosition()
		{
			if (Game1.IsMasterGame)
			{
				_ = base.currentLocation;
				if (!Game1.isRaining)
				{
					faceDirection(2);
					Game1.warpCharacter(this, "Farm", (Game1.getLocationFromName("Farm") as Farm).GetPetStartLocation());
				}
				else
				{
					warpToFarmHouse(Game1.MasterPlayer);
				}
			}
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (!lastPetDay.ContainsKey(who.UniqueMultiplayerID))
			{
				lastPetDay.Add(who.UniqueMultiplayerID, -1);
			}
			if (lastPetDay[who.UniqueMultiplayerID] != Game1.Date.TotalDays)
			{
				lastPetDay[who.UniqueMultiplayerID] = Game1.Date.TotalDays;
				mutex.RequestLock(delegate
				{
					if (!grantedFriendshipForPet.Value)
					{
						grantedFriendshipForPet.Set(newValue: true);
						friendshipTowardFarmer.Set(Math.Min(1000, (int)friendshipTowardFarmer + 12));
					}
					mutex.ReleaseLock();
				});
				doEmote(20);
				playContentSound();
				return true;
			}
			return false;
		}

		public virtual void playContentSound()
		{
		}

		public void hold(Farmer who)
		{
			flip = Sprite.CurrentAnimation.Last().flip;
			Sprite.CurrentFrame = Sprite.CurrentAnimation.Last().frame;
			Sprite.CurrentAnimation = null;
			Sprite.loop = false;
		}

		public override void behaviorOnFarmerPushing()
		{
			if (!(this is Dog) || (this as Dog).CurrentBehavior != 51)
			{
				pushingTimer += 2;
				if (pushingTimer > 100)
				{
					Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), Game1.player);
					setTrajectory((int)trajectory.X / 2, (int)trajectory.Y / 2);
					pushingTimer = 0;
					Halt();
					facePlayer(Game1.player);
					FacingDirection += 2;
					FacingDirection %= 4;
					faceDirection(FacingDirection);
					CurrentBehavior = 0;
				}
			}
		}

		public override void update(GameTime time, GameLocation location, long id, bool move)
		{
			base.update(time, location, id, move);
			pushingTimer = Math.Max(0, pushingTimer - 1);
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			petAnimationEvent.Poll();
			if (isSleepingOnFarmerBed.Value && CurrentBehavior != 1 && Game1.IsMasterGame)
			{
				isSleepingOnFarmerBed.Value = false;
				UpdateSleepingOnBed();
			}
			if (base.currentLocation == null)
			{
				base.currentLocation = location;
			}
			mutex.Update(location);
			if (!Game1.eventUp)
			{
				if (_currentBehavior != CurrentBehavior)
				{
					_OnNewBehavior();
				}
				RunState(time);
				if (Game1.IsMasterGame && Sprite.CurrentAnimation == null)
				{
					MovePosition(time, Game1.viewport, location);
				}
				flip = false;
				if (FacingDirection == 3 && Sprite.CurrentFrame >= 16)
				{
					flip = true;
				}
			}
		}

		public virtual void RunState(GameTime time)
		{
			if (_currentBehavior == 0 && Game1.IsMasterGame && base.currentLocation.isCollidingPosition(nextPosition(FacingDirection), Game1.viewport, this))
			{
				int new_direction = Game1.random.Next(0, 4);
				if (!base.currentLocation.isCollidingPosition(nextPosition(FacingDirection), Game1.viewport, this))
				{
					faceDirection(new_direction);
				}
			}
			if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && Sprite.CurrentAnimation == null && xVelocity == 0f && yVelocity == 0f)
			{
				CurrentBehavior = 1;
			}
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
			if (Sprite.CurrentAnimation != null)
			{
				Sprite.animateOnce(time);
			}
			else if (CurrentBehavior == 0)
			{
				Sprite.faceDirection(FacingDirection);
				if (isMoving())
				{
					animateInFacingDirection(time);
				}
				else
				{
					Sprite.StopAnimation();
				}
			}
		}

		protected void _OnNewBehavior()
		{
			_currentBehavior = CurrentBehavior;
			Halt();
			Sprite.CurrentAnimation = null;
			OnNewBehavior();
		}

		public virtual void OnNewBehavior()
		{
			Sprite.loop = false;
			Sprite.CurrentAnimation = null;
			switch (CurrentBehavior)
			{
			case 1:
			{
				Sprite.loop = true;
				bool local_sleep_flip2 = false;
				local_sleep_flip2 = (Game1.random.NextDouble() < 0.5);
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 1000, secondaryArm: false, local_sleep_flip2),
					new FarmerSprite.AnimationFrame(29, 1000, secondaryArm: false, local_sleep_flip2)
				});
				break;
			}
			case 2:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(17, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(18, 100, secondaryArm: false, flip: false, hold)
				});
				break;
			case 0:
				if (Game1.IsMasterGame)
				{
					Halt();
					faceDirection(Game1.random.Next(4));
					setMovingInFacingDirection();
				}
				Sprite.loop = true;
				break;
			}
		}

		public override Rectangle GetBoundingBox()
		{
			Vector2 position = base.Position;
			return new Rectangle((int)position.X + 16, (int)position.Y + 16, Sprite.SpriteWidth * 4 * 3 / 4, 32);
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, GetBoundingBox().Height / 2) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, Color.White, rotation, new Vector2(Sprite.SpriteWidth / 2, (float)Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, scale) * 4f, (flip || (Sprite.CurrentAnimation != null && Sprite.CurrentAnimation[Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, isSleepingOnFarmerBed.Value ? (((float)getStandingY() + 112f) / 10000f) : ((float)getStandingY() / 10000f)));
			if (base.IsEmoting)
			{
				Vector2 emotePosition = getLocalPosition(Game1.viewport);
				emotePosition.X += 32f;
				emotePosition.Y -= 96 + ((this is Dog) ? 16 : 0);
				b.Draw(Game1.emoteSpriteSheet, emotePosition, new Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)getStandingY() / 10000f + 0.0001f);
			}
		}

		public override bool withinPlayerThreshold(int threshold)
		{
			if (base.currentLocation != null && !base.currentLocation.farmers.Any())
			{
				return false;
			}
			Vector2 tileLocationOfMonster = getTileLocation();
			foreach (Farmer farmer in base.currentLocation.farmers)
			{
				Vector2 tileLocationOfPlayer = farmer.getTileLocation();
				if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
				{
					return true;
				}
			}
			return false;
		}
	}
}
