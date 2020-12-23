using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Monsters
{
	public class Monster : NPC
	{
		protected delegate void collisionBehavior(GameLocation location);

		public const int defaultInvincibleCountdown = 450;

		public const int seekPlayerIterationLimit = 80;

		[XmlElement("damageToFarmer")]
		public readonly NetInt damageToFarmer = new NetInt();

		[XmlElement("health")]
		public readonly NetInt health = new NetInt();

		[XmlElement("maxHealth")]
		public readonly NetInt maxHealth = new NetInt();

		[XmlElement("coinsToDrop")]
		public readonly NetInt coinsToDrop = new NetInt();

		[XmlElement("durationOfRandomMovements")]
		public readonly NetInt durationOfRandomMovements = new NetInt();

		[XmlElement("resilience")]
		public readonly NetInt resilience = new NetInt();

		[XmlElement("slipperiness")]
		public readonly NetInt slipperiness = new NetInt(2);

		[XmlElement("experienceGained")]
		public readonly NetInt experienceGained = new NetInt();

		[XmlElement("jitteriness")]
		public readonly NetDouble jitteriness = new NetDouble();

		[XmlElement("missChance")]
		public readonly NetDouble missChance = new NetDouble();

		[XmlElement("isGlider")]
		public readonly NetBool isGlider = new NetBool();

		[XmlElement("mineMonster")]
		public readonly NetBool mineMonster = new NetBool();

		[XmlElement("hasSpecialItem")]
		public readonly NetBool hasSpecialItem = new NetBool();

		[XmlIgnore]
		public readonly NetFloat synchedRotation = new NetFloat().Interpolated(interpolate: true, wait: true);

		public readonly NetIntList objectsToDrop = new NetIntList();

		protected int skipHorizontal;

		protected int invincibleCountdown;

		[XmlIgnore]
		private bool skipHorizontalUp;

		protected readonly NetInt defaultAnimationInterval = new NetInt(175);

		public int stunTime;

		[XmlElement("initializedForLocation")]
		public bool initializedForLocation;

		[XmlIgnore]
		public readonly NetBool netFocusedOnFarmers = new NetBool();

		[XmlIgnore]
		public readonly NetBool netWildernessFarmMonster = new NetBool();

		private readonly NetEvent1<ParryEventArgs> parryEvent = new NetEvent1<ParryEventArgs>();

		private readonly NetEvent1Field<Vector2, NetVector2> trajectoryEvent = new NetEvent1Field<Vector2, NetVector2>();

		[XmlIgnore]
		private readonly NetEvent0 deathAnimEvent = new NetEvent0();

		[XmlElement("ignoreDamageLOS")]
		public readonly NetBool ignoreDamageLOS = new NetBool();

		protected collisionBehavior onCollision;

		[XmlElement("isHardModeMonster")]
		public NetBool isHardModeMonster = new NetBool(value: false);

		private int slideAnimationTimer;

		[XmlIgnore]
		public Farmer Player => findPlayer();

		[XmlIgnore]
		public int DamageToFarmer
		{
			get
			{
				return damageToFarmer;
			}
			set
			{
				damageToFarmer.Value = value;
			}
		}

		[XmlIgnore]
		public int Health
		{
			get
			{
				return health;
			}
			set
			{
				health.Value = value;
			}
		}

		[XmlIgnore]
		public int MaxHealth
		{
			get
			{
				return maxHealth;
			}
			set
			{
				maxHealth.Value = value;
			}
		}

		[XmlIgnore]
		public int ExperienceGained
		{
			get
			{
				return experienceGained;
			}
			set
			{
				experienceGained.Value = value;
			}
		}

		[XmlIgnore]
		public int Slipperiness
		{
			get
			{
				return slipperiness;
			}
			set
			{
				slipperiness.Value = value;
			}
		}

		[XmlIgnore]
		public bool focusedOnFarmers
		{
			get
			{
				return netFocusedOnFarmers;
			}
			set
			{
				netFocusedOnFarmers.Value = value;
			}
		}

		[XmlIgnore]
		public bool wildernessFarmMonster
		{
			get
			{
				return netWildernessFarmMonster;
			}
			set
			{
				netWildernessFarmMonster.Value = value;
			}
		}

		public override bool IsMonster => true;

		public Monster()
		{
		}

		public Monster(string name, Vector2 position)
			: this(name, position, 2)
		{
			base.Breather = false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(damageToFarmer, health, maxHealth, coinsToDrop, durationOfRandomMovements, resilience, slipperiness, experienceGained, jitteriness, missChance, isGlider, mineMonster, hasSpecialItem, objectsToDrop, defaultAnimationInterval, netFocusedOnFarmers, netWildernessFarmMonster, deathAnimEvent, parryEvent, trajectoryEvent, ignoreDamageLOS, synchedRotation, isHardModeMonster);
			position.Field.AxisAlignedMovement = false;
			parryEvent.onEvent += handleParried;
			parryEvent.InterpolationWait = false;
			deathAnimEvent.onEvent += localDeathAnimation;
			trajectoryEvent.onEvent += doSetTrajectory;
			trajectoryEvent.InterpolationWait = false;
		}

		protected override Farmer findPlayer()
		{
			if (base.currentLocation == null)
			{
				return Game1.player;
			}
			Farmer bestFarmer = Game1.player;
			double bestPriority = double.MaxValue;
			foreach (Farmer f in base.currentLocation.farmers)
			{
				if (!f.hidden)
				{
					double priority = findPlayerPriority(f);
					if (priority < bestPriority)
					{
						bestPriority = priority;
						bestFarmer = f;
					}
				}
			}
			return bestFarmer;
		}

		protected virtual double findPlayerPriority(Farmer f)
		{
			return (f.Position - base.Position).LengthSquared();
		}

		public virtual void onDealContactDamage(Farmer who)
		{
		}

		public virtual List<Item> getExtraDropItems()
		{
			return new List<Item>();
		}

		public override bool withinPlayerThreshold()
		{
			if (!focusedOnFarmers)
			{
				return withinPlayerThreshold(moveTowardPlayerThreshold);
			}
			return true;
		}

		public Monster(string name, Vector2 position, int facingDir)
			: base(new AnimatedSprite("Characters\\Monsters\\" + name), position, facingDir, name)
		{
			parseMonsterInfo(name);
			base.Breather = false;
		}

		public virtual void drawAboveAllLayers(SpriteBatch b)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!isGlider)
			{
				base.draw(b);
			}
		}

		public virtual bool isInvincible()
		{
			return invincibleCountdown > 0;
		}

		public void setInvincibleCountdown(int time)
		{
			invincibleCountdown = time;
			startGlowing(new Color(255, 0, 0), border: false, 0.25f);
			glowingTransparency = 1f;
		}

		protected int maxTimesReachedMineBottom()
		{
			int result = 0;
			foreach (Farmer farmer in Game1.getOnlineFarmers())
			{
				result = Math.Max(result, farmer.timesReachedMineBottom);
			}
			return result;
		}

		public virtual Debris ModifyMonsterLoot(Debris debris)
		{
			return debris;
		}

		public virtual int GetBaseDifficultyLevel()
		{
			return 0;
		}

		public virtual void BuffForAdditionalDifficulty(int additional_difficulty)
		{
			int target3 = 0;
			if (DamageToFarmer != 0)
			{
				DamageToFarmer = (int)((float)DamageToFarmer * (1f + (float)additional_difficulty * 0.25f));
				target3 = 20 + (additional_difficulty - 1) * 20;
				if (DamageToFarmer < target3)
				{
					DamageToFarmer = (int)Utility.Lerp(DamageToFarmer, target3, 0.5f);
				}
			}
			MaxHealth = (int)((float)MaxHealth * (1f + (float)additional_difficulty * 0.5f));
			target3 = 500 + (additional_difficulty - 1) * 300;
			if (MaxHealth < target3)
			{
				MaxHealth = (int)Utility.Lerp(MaxHealth, target3, 0.5f);
			}
			Health = MaxHealth;
			resilience.Value += additional_difficulty * resilience.Value;
			isHardModeMonster.Value = true;
		}

		protected void parseMonsterInfo(string name)
		{
			string[] monsterInfo = Game1.content.Load<Dictionary<string, string>>("Data\\Monsters")[name].Split('/');
			Health = Convert.ToInt32(monsterInfo[0]);
			MaxHealth = Health;
			DamageToFarmer = Convert.ToInt32(monsterInfo[1]);
			coinsToDrop.Value = Game1.random.Next(Convert.ToInt32(monsterInfo[2]), Convert.ToInt32(monsterInfo[3]) + 1);
			isGlider.Value = Convert.ToBoolean(monsterInfo[4]);
			durationOfRandomMovements.Value = Convert.ToInt32(monsterInfo[5]);
			string[] objectsSplit = monsterInfo[6].Split(' ');
			objectsToDrop.Clear();
			for (int i = 0; i < objectsSplit.Length; i += 2)
			{
				if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
				{
					objectsToDrop.Add(Convert.ToInt32(objectsSplit[i]));
				}
			}
			resilience.Value = Convert.ToInt32(monsterInfo[7]);
			jitteriness.Value = Convert.ToDouble(monsterInfo[8]);
			base.willDestroyObjectsUnderfoot = false;
			moveTowardPlayer(Convert.ToInt32(monsterInfo[9]));
			base.speed = Convert.ToInt32(monsterInfo[10]);
			missChance.Value = Convert.ToDouble(monsterInfo[11]);
			mineMonster.Value = Convert.ToBoolean(monsterInfo[12]);
			if (maxTimesReachedMineBottom() >= 1 && (bool)mineMonster)
			{
				resilience.Value += resilience.Value / 2;
				missChance.Value *= 2.0;
				Health += Game1.random.Next(0, Health);
				DamageToFarmer += Game1.random.Next(0, DamageToFarmer / 2);
				coinsToDrop.Value += Game1.random.Next(0, (int)coinsToDrop + 1);
			}
			try
			{
				ExperienceGained = Convert.ToInt32(monsterInfo[13]);
			}
			catch (Exception)
			{
				ExperienceGained = 1;
			}
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				base.displayName = monsterInfo[monsterInfo.Length - 1];
			}
		}

		public virtual void InitializeForLocation(GameLocation location)
		{
			if (initializedForLocation)
			{
				return;
			}
			if ((bool)mineMonster && maxTimesReachedMineBottom() >= 1)
			{
				double additional_chance = 0.0;
				if (location is MineShaft)
				{
					additional_chance = (double)(location as MineShaft).GetAdditionalDifficulty() * 0.001;
				}
				if (Game1.random.NextDouble() < 0.001 + additional_chance)
				{
					objectsToDrop.Add((Game1.random.NextDouble() < 0.5) ? 72 : 74);
				}
			}
			if (Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS") && Game1.random.NextDouble() < (((string)name == "Dust Spirit") ? 0.02 : 0.05))
			{
				objectsToDrop.Add(890);
			}
			initializedForLocation = true;
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name, 0, 16, 16);
		}

		public virtual void shedChunks(int number)
		{
			shedChunks(number, 0.75f);
		}

		public virtual void shedChunks(int number, float scale)
		{
			if (Sprite.Texture.Height > Sprite.getHeight() * 4)
			{
				Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, Sprite.getHeight() * 4 + 16, 16, 16), 8, GetBoundingBox().Center.X, GetBoundingBox().Center.Y, number, (int)getTileLocation().Y, Color.White, 4f * scale);
			}
		}

		public void deathAnimation()
		{
			sharedDeathAnimation();
			deathAnimEvent.Fire();
		}

		protected virtual void sharedDeathAnimation()
		{
			shedChunks(Game1.random.Next(4, 9), 0.75f);
		}

		protected virtual void localDeathAnimation()
		{
		}

		public void parried(int damage, Farmer who)
		{
			parryEvent.Fire(new ParryEventArgs(damage, who));
		}

		private void handleParried(ParryEventArgs args)
		{
			int damage = args.damage;
			Farmer who = args.who;
			if (Game1.IsMasterGame)
			{
				float oldXVel = xVelocity;
				float oldYVel = yVelocity;
				if (xVelocity != 0f || yVelocity != 0f)
				{
					base.currentLocation.damageMonster(GetBoundingBox(), damage / 2, damage / 2 + 1, isBomb: false, 0f, 0, 0f, 0f, triggerMonsterInvincibleTimer: false, who);
				}
				xVelocity = 0f - oldXVel;
				yVelocity = 0f - oldYVel;
				xVelocity *= (isGlider ? 2f : 3.5f);
				yVelocity *= (isGlider ? 2f : 3.5f);
			}
			setInvincibleCountdown(450);
		}

		public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
		}

		public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			slideAnimationTimer = 0;
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				Health -= actualDamage;
				base.currentLocation.playSound(hitSound);
				setTrajectory(xTrajectory / 3, yTrajectory / 3);
				if (Health <= 0)
				{
					deathAnimation();
				}
			}
			return actualDamage;
		}

		public override void setTrajectory(Vector2 trajectory)
		{
			trajectoryEvent.Fire(trajectory);
		}

		private void doSetTrajectory(Vector2 trajectory)
		{
			if (Game1.IsMasterGame)
			{
				if (Math.Abs(trajectory.X) > Math.Abs(xVelocity))
				{
					xVelocity = trajectory.X;
				}
				if (Math.Abs(trajectory.Y) > Math.Abs(yVelocity))
				{
					yVelocity = trajectory.Y;
				}
			}
		}

		public virtual void behaviorAtGameTick(GameTime time)
		{
			if (timeBeforeAIMovementAgain > 0f)
			{
				timeBeforeAIMovementAgain -= time.ElapsedGameTime.Milliseconds;
			}
			if (!Player.isRafting || !withinPlayerThreshold(4))
			{
				return;
			}
			if (Math.Abs(Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y) > 192)
			{
				if (Player.GetBoundingBox().Center.X - GetBoundingBox().Center.X > 0)
				{
					SetMovingLeft(b: true);
				}
				else
				{
					SetMovingRight(b: true);
				}
			}
			else if (Player.GetBoundingBox().Center.Y - GetBoundingBox().Center.Y > 0)
			{
				SetMovingUp(b: true);
			}
			else
			{
				SetMovingDown(b: true);
			}
			MovePosition(time, Game1.viewport, base.currentLocation);
		}

		public virtual bool passThroughCharacters()
		{
			return false;
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (Game1.IsMasterGame && !initializedForLocation && location != null)
			{
				InitializeForLocation(location);
				initializedForLocation = true;
			}
			parryEvent.Poll();
			trajectoryEvent.Poll();
			deathAnimEvent.Poll();
			position.UpdateExtrapolation(base.speed + base.addedSpeed);
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (!location.farmers.Any())
			{
				return;
			}
			if (!Player.isRafting || !withinPlayerThreshold(4))
			{
				base.update(time, location);
			}
			if (Game1.IsMasterGame)
			{
				if (stunTime <= 0)
				{
					behaviorAtGameTick(time);
				}
				else
				{
					stunTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
					if (stunTime < 0)
					{
						stunTime = 0;
					}
				}
			}
			updateAnimation(time);
			if (Game1.IsMasterGame)
			{
				synchedRotation.Value = rotation;
			}
			else
			{
				rotation = synchedRotation.Value;
			}
			if (controller != null && withinPlayerThreshold(3))
			{
				controller = null;
			}
			if (!isGlider && (base.Position.X < 0f || base.Position.X > (float)(location.Map.GetLayer("Back").LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(location.map.GetLayer("Back").LayerHeight * 64)))
			{
				location.characters.Remove(this);
			}
			else if ((bool)isGlider && base.Position.X < -2000f)
			{
				Health = -500;
			}
		}

		protected void resetAnimationSpeed()
		{
			if (!ignoreMovementAnimations)
			{
				Sprite.interval = (float)(int)defaultAnimationInterval - (float)(base.speed + base.addedSpeed - 2) * 20f;
			}
		}

		protected virtual void updateAnimation(GameTime time)
		{
			if (!Game1.IsMasterGame)
			{
				updateMonsterSlaveAnimation(time);
			}
			resetAnimationSpeed();
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		protected virtual void updateMonsterSlaveAnimation(GameTime time)
		{
			Sprite.animateOnce(time);
		}

		private bool doHorizontalMovement(GameLocation location)
		{
			bool wasAbleToMoveHorizontally = false;
			if (base.Position.X > Player.Position.X + 8f || (skipHorizontal > 0 && Player.getStandingX() < getStandingX() - 8))
			{
				SetMovingOnlyLeft();
				if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					wasAbleToMoveHorizontally = true;
				}
				else
				{
					faceDirection(3);
					if ((int)durationOfRandomMovements > 0 && Game1.random.NextDouble() < (double)jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							tryToMoveInDirection(2, isFarmer: false, DamageToFarmer, isGlider);
						}
						else
						{
							tryToMoveInDirection(0, isFarmer: false, DamageToFarmer, isGlider);
						}
						timeBeforeAIMovementAgain = (int)durationOfRandomMovements;
					}
				}
			}
			else if (base.Position.X < Player.Position.X - 8f)
			{
				SetMovingOnlyRight();
				if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					wasAbleToMoveHorizontally = true;
				}
				else
				{
					faceDirection(1);
					if ((int)durationOfRandomMovements > 0 && Game1.random.NextDouble() < (double)jitteriness)
					{
						if (Game1.random.NextDouble() < 0.5)
						{
							tryToMoveInDirection(2, isFarmer: false, DamageToFarmer, isGlider);
						}
						else
						{
							tryToMoveInDirection(0, isFarmer: false, DamageToFarmer, isGlider);
						}
						timeBeforeAIMovementAgain = (int)durationOfRandomMovements;
					}
				}
			}
			else
			{
				faceGeneralDirection(Player.getStandingPosition());
				setMovingInFacingDirection();
				skipHorizontal = 500;
			}
			return wasAbleToMoveHorizontally;
		}

		public virtual bool ShouldActuallyMoveAwayFromPlayer()
		{
			return false;
		}

		private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (who.Position.X > base.Position.X + 16f)
			{
				if (ShouldActuallyMoveAwayFromPlayer())
				{
					SetMovingOnlyLeft();
				}
				else
				{
					SetMovingOnlyRight();
				}
				setMoving = true;
				if (!location.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					success = true;
				}
				else
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (success || !(who.Position.X < base.Position.X - 16f))
			{
				return;
			}
			if (ShouldActuallyMoveAwayFromPlayer())
			{
				SetMovingOnlyRight();
			}
			else
			{
				SetMovingOnlyLeft();
			}
			setMoving = true;
			if (!location.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
			{
				success = true;
				return;
			}
			MovePosition(Game1.currentGameTime, Game1.viewport, location);
			if (!base.Position.Equals(lastPosition))
			{
				scootSuccess = true;
			}
		}

		private void checkVerticalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
		{
			if (!success && who.Position.Y < base.Position.Y - 16f)
			{
				if (ShouldActuallyMoveAwayFromPlayer())
				{
					SetMovingOnlyDown();
				}
				else
				{
					SetMovingOnlyUp();
				}
				setMoving = true;
				if (!location.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
				{
					success = true;
				}
				else
				{
					MovePosition(Game1.currentGameTime, Game1.viewport, location);
					if (!base.Position.Equals(lastPosition))
					{
						scootSuccess = true;
					}
				}
			}
			if (success || !(who.Position.Y > base.Position.Y + 16f))
			{
				return;
			}
			if (ShouldActuallyMoveAwayFromPlayer())
			{
				SetMovingOnlyUp();
			}
			else
			{
				SetMovingOnlyDown();
			}
			setMoving = true;
			if (!location.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, DamageToFarmer, isGlider, this))
			{
				success = true;
				return;
			}
			MovePosition(Game1.currentGameTime, Game1.viewport, location);
			if (!base.Position.Equals(lastPosition))
			{
				scootSuccess = true;
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
			if (base.IsWalkingTowardPlayer)
			{
				if (((int)moveTowardPlayerThreshold == -1 || withinPlayerThreshold()) && timeBeforeAIMovementAgain <= 0f && IsMonster && !isGlider && location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y] != null && !location.map.GetLayer("Back").Tiles[(int)Player.getTileLocation().X, (int)Player.getTileLocation().Y].Properties.ContainsKey("NPCBarrier"))
				{
					if (skipHorizontal <= 0)
					{
						if (lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
						{
							switch (FacingDirection)
							{
							case 1:
							case 3:
								if (Game1.random.NextDouble() < 0.5)
								{
									SetMovingOnlyUp();
								}
								else
								{
									SetMovingOnlyDown();
								}
								break;
							case 0:
							case 2:
								if (Game1.random.NextDouble() < 0.5)
								{
									SetMovingOnlyRight();
								}
								else
								{
									SetMovingOnlyLeft();
								}
								break;
							}
							skipHorizontal = 700;
							return;
						}
						bool success = false;
						bool setMoving = false;
						bool scootSuccess = false;
						if (lastPosition.X == base.Position.X)
						{
							checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
							checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
						}
						else
						{
							checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
							checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, Player, location);
						}
						if (!success && !setMoving)
						{
							Halt();
							faceGeneralDirection(Player.getStandingPosition());
						}
						if (success)
						{
							skipHorizontal = 500;
						}
						if (scootSuccess)
						{
							return;
						}
					}
					else
					{
						skipHorizontal -= time.ElapsedGameTime.Milliseconds;
					}
				}
			}
			else
			{
				defaultMovementBehavior(time);
			}
			MovePosition(time, Game1.viewport, location);
			if (base.Position.Equals(lastPosition) && base.IsWalkingTowardPlayer && withinPlayerThreshold())
			{
				noMovementProgressNearPlayerBehavior();
			}
		}

		public virtual void noMovementProgressNearPlayerBehavior()
		{
			Halt();
			faceGeneralDirection(Player.getStandingPosition());
		}

		public virtual void defaultMovementBehavior(GameTime time)
		{
			if (Game1.random.NextDouble() < (double)jitteriness * 1.8 && skipHorizontal <= 0)
			{
				switch (Game1.random.Next(6))
				{
				case 0:
					SetMovingOnlyUp();
					break;
				case 1:
					SetMovingOnlyRight();
					break;
				case 2:
					SetMovingOnlyDown();
					break;
				case 3:
					SetMovingOnlyLeft();
					break;
				default:
					Halt();
					break;
				}
			}
		}

		public virtual bool TakesDamageFromHitbox(Microsoft.Xna.Framework.Rectangle area_of_effect)
		{
			return GetBoundingBox().Intersects(area_of_effect);
		}

		public virtual bool OverlapsFarmerForDamage(Farmer who)
		{
			return GetBoundingBox().Intersects(who.GetBoundingBox());
		}

		public override void Halt()
		{
			int old_speed = base.speed;
			base.Halt();
			base.speed = old_speed;
		}

		public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
		{
			if (stunTime > 0)
			{
				return;
			}
			lastPosition = base.Position;
			if (xVelocity != 0f || yVelocity != 0f)
			{
				if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity))
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				Microsoft.Xna.Framework.Rectangle nextPosition = GetBoundingBox();
				int start_x = nextPosition.X;
				int start_y = nextPosition.Y;
				int end_x = nextPosition.X + (int)xVelocity;
				int end_y = nextPosition.Y - (int)yVelocity;
				int steps = 1;
				bool found_collision = false;
				bool is_grounded_glider = false;
				if (this is SquidKid)
				{
					is_grounded_glider = true;
				}
				if (!isGlider.Value | is_grounded_glider)
				{
					if (nextPosition.Width > 0 && Math.Abs((int)xVelocity) > nextPosition.Width)
					{
						steps = (int)Math.Max(steps, Math.Ceiling((float)Math.Abs((int)xVelocity) / (float)nextPosition.Width));
					}
					if (nextPosition.Height > 0 && Math.Abs((int)yVelocity) > nextPosition.Height)
					{
						steps = (int)Math.Max(steps, Math.Ceiling((float)Math.Abs((int)yVelocity) / (float)nextPosition.Height));
					}
				}
				for (int i = 1; i <= steps; i++)
				{
					nextPosition.X = (int)Utility.Lerp(start_x, end_x, (float)i / (float)steps);
					nextPosition.Y = (int)Utility.Lerp(start_y, end_y, (float)i / (float)steps);
					bool is_glider = isGlider;
					if (is_grounded_glider)
					{
						is_glider = false;
					}
					if (currentLocation != null && currentLocation.isCollidingPosition(nextPosition, viewport, isFarmer: false, DamageToFarmer, is_glider, this))
					{
						found_collision = true;
						break;
					}
				}
				if (!found_collision)
				{
					position.X += xVelocity;
					position.Y -= yVelocity;
					if (Slipperiness < 1000)
					{
						xVelocity -= xVelocity / (float)Slipperiness;
						yVelocity -= yVelocity / (float)Slipperiness;
						if (Math.Abs(xVelocity) <= 0.05f)
						{
							xVelocity = 0f;
						}
						if (Math.Abs(yVelocity) <= 0.05f)
						{
							yVelocity = 0f;
						}
					}
					if (!isGlider && invincibleCountdown > 0)
					{
						slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
						if (slideAnimationTimer < 0 && (Math.Abs(xVelocity) >= 3f || Math.Abs(yVelocity) >= 3f))
						{
							slideAnimationTimer = 100 - (int)(Math.Abs(xVelocity) * 2f + Math.Abs(yVelocity) * 2f);
							Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, getStandingPosition() + new Vector2(-32f, -32f), Color.White * 0.75f, 8, Game1.random.NextDouble() < 0.5, 20f)
							{
								scale = 0.75f
							});
						}
					}
				}
				else if ((bool)isGlider || Slipperiness >= 8)
				{
					if ((bool)isGlider)
					{
						bool[] array = Utility.horizontalOrVerticalCollisionDirections(nextPosition, this);
						if (array[0])
						{
							xVelocity = 0f - xVelocity;
							position.X += Math.Sign(xVelocity);
							rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
						}
						if (array[1])
						{
							yVelocity = 0f - yVelocity;
							position.Y -= Math.Sign(yVelocity);
							rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
						}
					}
					if (Slipperiness < 1000)
					{
						xVelocity -= xVelocity / (float)Slipperiness / 4f;
						yVelocity -= yVelocity / (float)Slipperiness / 4f;
						if (Math.Abs(xVelocity) <= 0.05f)
						{
							xVelocity = 0f;
						}
						if (Math.Abs(yVelocity) <= 0.051f)
						{
							yVelocity = 0f;
						}
					}
				}
				else
				{
					xVelocity -= xVelocity / (float)Slipperiness;
					yVelocity -= yVelocity / (float)Slipperiness;
					if (Math.Abs(xVelocity) <= 0.05f)
					{
						xVelocity = 0f;
					}
					if (Math.Abs(yVelocity) <= 0.05f)
					{
						yVelocity = 0f;
					}
				}
				if ((bool)isGlider)
				{
					return;
				}
			}
			if (moveUp)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.Y -= base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateUp(time);
					}
					FacingDirection = 0;
					faceDirection(0);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp = this.nextPosition(0);
					tmp.Width /= 4;
					bool leftCorner2 = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					tmp.X += tmp.Width * 3;
					bool rightCorner2 = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2(getStandingX() / 64, getStandingY() / 64 - 1);
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.Y -= base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveRight)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.X += base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateRight(time);
					}
					FacingDirection = 1;
					faceDirection(1);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp2 = this.nextPosition(1);
					tmp2.Height /= 4;
					bool topCorner2 = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					tmp2.Y += tmp2.Height * 3;
					bool bottomCorner2 = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2(getStandingX() / 64 + 1, getStandingY() / 64);
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.X += base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveDown)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.Y += base.speed + base.addedSpeed;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateDown(time);
					}
					FacingDirection = 2;
					faceDirection(2);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp3 = this.nextPosition(2);
					tmp3.Width /= 4;
					bool leftCorner = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					tmp3.X += tmp3.Width * 3;
					bool rightCorner = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2(getStandingX() / 64, getStandingY() / 64 + 1);
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.Y += base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (moveLeft)
			{
				if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, DamageToFarmer, isGlider, this)) || isCharging)
				{
					position.X -= base.speed + base.addedSpeed;
					FacingDirection = 3;
					if (!ignoreMovementAnimations)
					{
						Sprite.AnimateLeft(time);
					}
					faceDirection(3);
				}
				else
				{
					Microsoft.Xna.Framework.Rectangle tmp4 = this.nextPosition(3);
					tmp4.Height /= 4;
					bool topCorner = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					tmp4.Y += tmp4.Height * 3;
					bool bottomCorner = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: false, DamageToFarmer, isGlider, this);
					if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, DamageToFarmer, isGlider, this))
					{
						position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
					}
					if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
					{
						Halt();
					}
					else if (base.willDestroyObjectsUnderfoot)
					{
						new Vector2(getStandingX() / 64 - 1, getStandingY() / 64);
						if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), showDestroyedObject: true))
						{
							currentLocation.playSound("stoneCrack");
							position.X -= base.speed + base.addedSpeed;
						}
						else
						{
							blockedInterval += time.ElapsedGameTime.Milliseconds;
						}
					}
					if (onCollision != null)
					{
						onCollision(currentLocation);
					}
				}
			}
			else if (!ignoreMovementAnimations)
			{
				if (moveUp)
				{
					Sprite.AnimateUp(time);
				}
				else if (moveRight)
				{
					Sprite.AnimateRight(time);
				}
				else if (moveDown)
				{
					Sprite.AnimateDown(time);
				}
				else if (moveLeft)
				{
					Sprite.AnimateLeft(time);
				}
			}
			if ((blockedInterval < 3000 || !((float)blockedInterval <= 3750f)) && blockedInterval >= 5000)
			{
				base.speed = 4;
				isCharging = true;
				blockedInterval = 0;
			}
			if (DamageToFarmer <= 0 || !(Game1.random.NextDouble() < 0.00033333333333333332))
			{
				return;
			}
			if (base.Name.Equals("Shadow Guy") && Game1.random.NextDouble() < 0.3)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					currentLocation.playSound("grunt");
				}
				else
				{
					currentLocation.playSound("shadowpeep");
				}
			}
			else if (!base.Name.Equals("Shadow Girl"))
			{
				if (base.Name.Equals("Ghost"))
				{
					currentLocation.playSound("ghost");
				}
				else if (!base.Name.Contains("Slime"))
				{
					base.Name.Contains("Jelly");
				}
			}
		}
	}
}
