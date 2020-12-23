using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles
{
	public class ParrotUpgradePerch : INetObject<NetFields>
	{
		public enum UpgradeState
		{
			Idle,
			StartBuilding,
			Building,
			Complete
		}

		public class Parrot
		{
			public Vector2 position;

			public float height;

			protected ParrotUpgradePerch _perch;

			protected Vector2 targetPosition = Vector2.Zero;

			protected Vector2 startPosition = Vector2.Zero;

			public Texture2D texture;

			public bool bounced;

			public bool flipped;

			public bool isPerchedParrot;

			private int baseFrame;

			private int birdType;

			private int flapFrame;

			private float nextFlapTime;

			public float alpha;

			public float moveTime;

			public float moveDuration = 1f;

			public bool firstBounce;

			public bool flyAway;

			private bool soundBird;

			public Parrot(ParrotUpgradePerch perch, Vector2 start_position, bool soundBird = false)
			{
				this.soundBird = soundBird;
				_perch = perch;
				texture = perch.texture;
				position = (start_position + new Vector2(0.5f, 0.5f)) * 64f;
				startPosition = start_position;
				height = 64f;
				birdType = Game1.random.Next(0, 5);
				FindNewTarget();
				firstBounce = true;
			}

			public virtual void FindNewTarget()
			{
				isPerchedParrot = false;
				firstBounce = false;
				startPosition = position;
				moveTime = 0f;
				moveDuration = 1f;
				Microsoft.Xna.Framework.Rectangle rect = _perch.upgradeRect;
				if ((UpgradeState)_perch.currentState == UpgradeState.Complete)
				{
					flyAway = true;
					moveDuration = 5f;
					rect.Inflate(5, 0);
				}
				targetPosition = (Utility.getRandomPositionInThisRectangle(rect, Game1.random) + new Vector2(0.5f, 0.5f)) * 64f;
				Vector2 offset = Vector2.Zero;
				offset.X = targetPosition.X - position.X;
				offset.Y = targetPosition.Y - position.Y;
				if (offset.X < 0f)
				{
					flipped = false;
				}
				else if (offset.X > 0f)
				{
					flipped = true;
				}
				if (Math.Abs(offset.X) > Math.Abs(offset.Y))
				{
					baseFrame = 2;
				}
				else if (offset.Y > 0f)
				{
					baseFrame = 5;
				}
				else
				{
					baseFrame = 8;
				}
			}

			public virtual bool Update(GameTime time)
			{
				moveTime += (float)time.ElapsedGameTime.TotalSeconds;
				if (moveTime > moveDuration)
				{
					moveTime = moveDuration;
				}
				float t = moveTime / moveDuration;
				position.X = Utility.Lerp(startPosition.X, targetPosition.X, t);
				position.Y = Utility.Lerp(startPosition.Y, targetPosition.Y, t);
				if (isPerchedParrot)
				{
					height = EaseInOutQuad(t, 24f, -24f, 1f);
					firstBounce = false;
					birdType = 0;
				}
				else if (flyAway)
				{
					height = EaseInQuad(t, 0f, 1536f, 1f);
				}
				else if (firstBounce)
				{
					height = EaseInOutQuad(t, 64f, -64f, 1f);
				}
				else
				{
					float hop_height = 24f;
					if ((double)t <= 0.5)
					{
						height = EaseInOutQuad(t, 0f, hop_height, 0.5f);
					}
					else
					{
						height = EaseInQuad(t - 0.5f, hop_height, 0f - hop_height, 0.5f);
					}
				}
				if (t >= 1f)
				{
					if (flyAway)
					{
						return true;
					}
					FindNewTarget();
					if (!firstBounce && (string)_perch.upgradeName != "Turtle")
					{
						_perch.locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * (_perch.upgradeName.Contains("Volcano") ? 1 : Game1.random.Next(3)), 229, 16, 6), position, Game1.random.NextDouble() < 0.5, 0f, Color.White)
						{
							motion = new Vector2(Game1.random.Next(-2, 3), -12f),
							acceleration = new Vector2(0f, 0.25f),
							rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
							scale = 4f,
							animationLength = 1,
							totalNumberOfLoops = 1,
							interval = 1000 + Game1.random.Next(500),
							layerDepth = 1f,
							drawAboveAlwaysFront = true
						});
					}
				}
				if (firstBounce)
				{
					alpha = Utility.Clamp(t / 0.25f, 0f, 1f);
				}
				else if (flyAway)
				{
					float fade_duration = 0.1f;
					alpha = 1f - Utility.Clamp((t - (1f - fade_duration)) / fade_duration, 0f, 1f);
				}
				else
				{
					alpha = 1f;
				}
				if (nextFlapTime > 0f)
				{
					nextFlapTime -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (nextFlapTime <= 0f)
				{
					flapFrame = (flapFrame + 1) % 3;
					if (flyAway || firstBounce || height < 12f || t < 0.5f)
					{
						if (flapFrame == 0)
						{
							if ((string)_perch.upgradeName != "Hut")
							{
								if ((!flyAway || t < 0.5f) && soundBird)
								{
									_perch.locationRef.Value.localSound("batFlap");
								}
								if (!flyAway && !firstBounce)
								{
									if (_perch.upgradeName.Contains("Volcano"))
									{
										if (Game1.random.NextDouble() < 0.15000000596046448)
										{
											_perch.locationRef.Value.localSound("hammer");
										}
									}
									else if ((string)_perch.upgradeName == "Turtle")
									{
										if (Game1.random.NextDouble() < 0.15000000596046448)
										{
											_perch.locationRef.Value.localSound("hitEnemy");
										}
									}
									else
									{
										if (Game1.random.NextDouble() < 0.05000000074505806)
										{
											_perch.locationRef.Value.localSound("axe");
										}
										if (Game1.random.NextDouble() < 0.05000000074505806)
										{
											_perch.locationRef.Value.localSound("dirtyHit");
										}
										if (Game1.random.NextDouble() < 0.05000000074505806)
										{
											_perch.locationRef.Value.localSound("crafting");
										}
									}
								}
							}
							nextFlapTime = 0.1f;
						}
						else
						{
							nextFlapTime = 0.05f;
						}
					}
					else if (flapFrame == 0)
					{
						if (soundBird)
						{
							_perch.locationRef.Value.localSound("batFlap");
						}
						nextFlapTime = 0.3f;
					}
					else
					{
						nextFlapTime = 0.2f;
					}
				}
				return false;
			}

			private float EaseInOutQuad(float t, float b, float c, float d)
			{
				if ((t /= d / 2f) < 1f)
				{
					return c / 2f * t * t + b;
				}
				return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
			}

			private float EaseInQuad(float t, float b, float c, float d)
			{
				return c * (t /= d) * t + b;
			}

			private float EaseOutQuad(float t, float b, float c, float d)
			{
				return (0f - c) * (t /= d) * (t - 2f) + b;
			}

			public virtual void Draw(SpriteBatch b)
			{
				int drawn_frame = baseFrame + flapFrame;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(position - new Vector2(0f, height * 4f))), new Microsoft.Xna.Framework.Rectangle(drawn_frame * 24, birdType * 24, 24, 24), Color.White * alpha, 0f, new Vector2(12f, 18f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, position.Y / 10000f);
			}
		}

		public NetEvent0 animationEvent = new NetEvent0();

		public NetMutex upgradeMutex = new NetMutex();

		public NetPoint tilePosition = new NetPoint();

		public Texture2D texture;

		public NetRectangle upgradeRect = new NetRectangle();

		public List<Parrot> parrots = new List<Parrot>();

		public NetEvent0 upgradeCompleteEvent = new NetEvent0();

		public NetEnum<UpgradeState> currentState = new NetEnum<UpgradeState>(UpgradeState.Idle);

		public float stateTimer;

		public NetInt requiredNuts = new NetInt(0);

		public float squawkTime;

		public float timeUntilChomp;

		public float timeUntilSqwawk;

		public float shakeTime;

		public float costShakeTime;

		public const int PARROT_COUNT = 24;

		public bool parrotPresent = true;

		public bool isPlayerNearby;

		public NetString upgradeName = new NetString("");

		public NetString requiredMail = new NetString("");

		public float nextParrotSpawn;

		public NetLocationRef locationRef = new NetLocationRef();

		public Action onApplyUpgrade;

		public Func<bool> onUpdateCompletionStatus;

		protected bool _cachedAvailablity;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public ParrotUpgradePerch()
		{
			InitNetFields();
			texture = Game1.content.Load<Texture2D>("LooseSprites\\parrots");
		}

		public virtual void UpdateCompletionStatus()
		{
			if (onUpdateCompletionStatus != null && onUpdateCompletionStatus())
			{
				currentState.Value = UpgradeState.Complete;
			}
		}

		public virtual void InitNetFields()
		{
			NetFields.AddFields(tilePosition, upgradeRect, currentState, upgradeMutex.NetFields, animationEvent.NetFields, upgradeCompleteEvent.NetFields, locationRef.NetFields, requiredNuts, upgradeName, requiredMail);
			animationEvent.onEvent += PerformAnimation;
			upgradeCompleteEvent.onEvent += PerformCompleteAnimation;
		}

		public virtual void PerformCompleteAnimation()
		{
			if (upgradeName.Contains("Volcano"))
			{
				for (int j = 0; j < 16; j++)
				{
					locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-1, 2), -1f),
						scale = 1f,
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = j * 15
					});
					TemporaryAnimatedSprite t4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(65, 229, 16, 6), Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (upgradeRect.Bottom + 1) * 64
					};
					t4.reachedStopCoordinate = t4.bounce;
					locationRef.Value.TemporarySprites.Add(t4);
					t4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(65, 229, 16, 6), Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (upgradeRect.Bottom + 1) * 64
					};
					t4.reachedStopCoordinate = t4.bounce;
					locationRef.Value.TemporarySprites.Add(t4);
				}
				if (locationRef.Value == Game1.currentLocation)
				{
					Game1.flashAlpha = 1f;
					Game1.playSound("boulderBreak");
				}
			}
			else if ((string)upgradeName == "House")
			{
				for (int i = 0; i < 16; i++)
				{
					locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-1, 2), -1f),
						scale = 1f,
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = i * 15
					});
					TemporaryAnimatedSprite t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (upgradeRect.Bottom + 1) * 64
					};
					t2.reachedStopCoordinate = t2.bounce;
					locationRef.Value.TemporarySprites.Add(t2);
					t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(upgradeRect, Game1.random) * 64f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (upgradeRect.Bottom + 1) * 64
					};
					t2.reachedStopCoordinate = t2.bounce;
					locationRef.Value.TemporarySprites.Add(t2);
				}
				if (locationRef.Value == Game1.currentLocation)
				{
					Game1.flashAlpha = 1f;
					Game1.playSound("boulderBreak");
				}
			}
			else if (((string)upgradeName == "Resort" || (string)upgradeName == "Trader" || (string)upgradeName == "Obelisk") && locationRef.Value == Game1.currentLocation)
			{
				Game1.flashAlpha = 1f;
			}
			if (locationRef.Value == Game1.currentLocation && (string)upgradeName != "Hut")
			{
				DelayedAction.playSoundAfterDelay("secret1", 800);
			}
		}

		public ParrotUpgradePerch(GameLocation location, Point tile_position, Microsoft.Xna.Framework.Rectangle upgrade_rectangle, int required_nuts, Action apply_upgrade, Func<bool> update_completion_status, string upgrade_name = "", string required_mail = "")
			: this()
		{
			locationRef.Value = location;
			tilePosition.Value = tile_position;
			upgradeRect.Value = upgrade_rectangle;
			onApplyUpgrade = apply_upgrade;
			onUpdateCompletionStatus = update_completion_status;
			requiredNuts.Value = required_nuts;
			parrots = new List<Parrot>();
			UpdateCompletionStatus();
			upgradeName.Value = upgrade_name;
			if (required_mail != "")
			{
				requiredMail.Value = required_mail;
			}
		}

		public bool IsAtTile(int x, int y)
		{
			if (tilePosition.X == x && tilePosition.Y == y)
			{
				return currentState.Value == UpgradeState.Idle;
			}
			return false;
		}

		public virtual void PerformAnimation()
		{
			currentState.Value = UpgradeState.StartBuilding;
			stateTimer = 3f;
			if (Game1.currentLocation == locationRef.Value)
			{
				Game1.playSound("parrot_squawk");
				parrots.Clear();
				parrotPresent = true;
				Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), 2000f, 1, 0, (Utility.PointToVector2(tilePosition) + new Vector2(0.25f, -2.5f)) * 64f, flicker: false, flipped: false, (float)(tilePosition.Y * 64 + 1) / 10000f, 0f, Color.White, 3f, -0.015f, 0f, 0f)
				{
					motion = new Vector2(-0.1f, -7f),
					acceleration = new Vector2(0f, 0.25f),
					id = 98765f,
					drawAboveAlwaysFront = true
				});
				Game1.playSound("dwop");
				if (upgradeMutex.IsLockHeld())
				{
					Game1.player.freezePause = (((string)upgradeName != "Hut") ? 10000 : 3000);
				}
				timeUntilChomp = 1f;
				squawkTime = 1f;
			}
		}

		public virtual bool IsAvailable(bool use_cached_value = false)
		{
			if (use_cached_value && Game1.currentLocation == locationRef.Value)
			{
				return _cachedAvailablity;
			}
			if (requiredMail.Value == "")
			{
				return true;
			}
			string[] array = requiredMail.Value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string formatted_mail = array[i].Trim();
				if (!Game1.MasterPlayer.hasOrWillReceiveMail(formatted_mail))
				{
					return false;
				}
			}
			return true;
		}

		public virtual void StartAnimation()
		{
			animationEvent.Fire();
		}

		public bool CheckAction(Location tile_location, Farmer farmer)
		{
			if (IsAtTile(tile_location.X, tile_location.Y) && IsAvailable())
			{
				string request_text2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:UpgradePerch_" + upgradeName.Value);
				GameLocation location = locationRef.Value;
				if (request_text2 != null && location != null)
				{
					request_text2 = string.Format(request_text2, requiredNuts.Value);
					costShakeTime = 0.5f;
					squawkTime = 0.5f;
					shakeTime = 0.5f;
					if (locationRef.Value == Game1.currentLocation)
					{
						Game1.playSound("parrot_squawk");
					}
					if ((int)Game1.netWorldState.Value.GoldenWalnuts >= requiredNuts.Value)
					{
						location.createQuestionDialogue(request_text2, location.createYesNoResponses(), "UpgradePerch_" + upgradeName.Value);
					}
					else
					{
						Game1.drawDialogueNoTyping(request_text2);
					}
				}
				else if ((int)Game1.netWorldState.Value.GoldenWalnuts >= requiredNuts.Value)
				{
					AttemptConstruction();
				}
				else
				{
					ShowInsufficientNuts();
				}
				return true;
			}
			return false;
		}

		public virtual bool AnswerQuestion(Response answer)
		{
			if (Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey == "UpgradePerch_" + upgradeName.Value + "_Yes")
			{
				AttemptConstruction();
				return true;
			}
			return false;
		}

		public virtual void AttemptConstruction()
		{
			Game1.player.Halt();
			Game1.player.canMove = false;
			upgradeMutex.RequestLock(delegate
			{
				Game1.player.canMove = true;
				if ((int)Game1.netWorldState.Value.GoldenWalnuts >= requiredNuts.Value)
				{
					if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_UpgradePerch_" + upgradeName.Value) != null)
					{
						Game1.multiplayer.globalChatInfoMessage("UpgradePerch_" + upgradeName.Value, Game1.player.Name);
					}
					Game1.netWorldState.Value.GoldenWalnuts.Value -= requiredNuts.Value;
					StartAnimation();
				}
				else
				{
					ShowInsufficientNuts();
				}
			}, delegate
			{
				Game1.player.canMove = true;
			});
		}

		public virtual void ShowInsufficientNuts()
		{
			if (IsAvailable(use_cached_value: true))
			{
				costShakeTime = 0.5f;
				squawkTime = 0.5f;
				shakeTime = 0.5f;
				if (locationRef.Value == Game1.currentLocation)
				{
					Game1.playSound("parrot_squawk");
				}
			}
		}

		public virtual void ApplyUpgrade()
		{
			if (onApplyUpgrade != null)
			{
				onApplyUpgrade();
			}
		}

		public virtual void Cleanup()
		{
			if (upgradeMutex.IsLockHeld())
			{
				upgradeMutex.ReleaseLock();
			}
			if (isPlayerNearby)
			{
				Game1.specialCurrencyDisplay.ShowCurrency(null);
				isPlayerNearby = false;
			}
		}

		public virtual void ResetForPlayerEntry()
		{
			_cachedAvailablity = IsAvailable();
			parrotPresent = (currentState.Value == UpgradeState.Idle);
		}

		public virtual void UpdateEvenIfFarmerIsntHere(GameTime time)
		{
			animationEvent.Poll();
			upgradeCompleteEvent.Poll();
			upgradeMutex.Update(locationRef.Value);
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (stateTimer > 0f)
			{
				stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if ((UpgradeState)currentState == UpgradeState.StartBuilding && stateTimer <= 0f)
			{
				currentState.Value = UpgradeState.Building;
				stateTimer = 5f;
				if ((string)upgradeName == "Hut")
				{
					stateTimer = 0.1f;
				}
			}
			if ((UpgradeState)currentState == UpgradeState.Building && stateTimer <= 0f)
			{
				ApplyUpgrade();
				currentState.Value = UpgradeState.Complete;
				upgradeMutex.ReleaseLock();
				upgradeCompleteEvent.Fire();
			}
		}

		public virtual void Update(GameTime time)
		{
			if (squawkTime > 0f)
			{
				squawkTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (shakeTime > 0f)
			{
				shakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (costShakeTime > 0f)
			{
				costShakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (timeUntilChomp > 0f)
			{
				timeUntilChomp -= (float)time.ElapsedGameTime.TotalSeconds;
				if (timeUntilChomp <= 0f)
				{
					if (locationRef.Value == Game1.currentLocation)
					{
						Game1.playSound("eat");
					}
					timeUntilChomp = 0f;
					shakeTime = 0.25f;
					if (locationRef.Value.getTemporarySpriteByID(98765) != null)
					{
						for (int j = 0; j < 6; j++)
						{
							locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(9, 252, 3, 3), locationRef.Value.getTemporarySpriteByID(98765).position + new Vector2(8f, 8f) * 4f, Game1.random.NextDouble() < 0.5, 0f, Color.White)
							{
								motion = new Vector2(Game1.random.Next(-1, 2), -6f),
								acceleration = new Vector2(0f, 0.25f),
								rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
								scale = 4f,
								animationLength = 1,
								totalNumberOfLoops = 1,
								interval = 500 + Game1.random.Next(500),
								layerDepth = 1f,
								drawAboveAlwaysFront = true
							});
						}
					}
					locationRef.Value.removeTemporarySpritesWithID(98765f);
					timeUntilSqwawk = 1f;
				}
			}
			if (timeUntilSqwawk > 0f)
			{
				timeUntilSqwawk -= (float)time.ElapsedGameTime.TotalSeconds;
				if (timeUntilSqwawk <= 0f)
				{
					timeUntilSqwawk = 0f;
					if (locationRef.Value == Game1.currentLocation)
					{
						Game1.playSound("parrot_squawk");
					}
					squawkTime = 0.5f;
					shakeTime = 0.5f;
				}
			}
			if (parrotPresent && currentState.Value > UpgradeState.StartBuilding)
			{
				if (currentState.Value == UpgradeState.Building)
				{
					Parrot flying_parrot = new Parrot(this, Utility.PointToVector2(tilePosition));
					flying_parrot.isPerchedParrot = true;
					parrots.Add(flying_parrot);
				}
				parrotPresent = false;
			}
			if (IsAvailable(use_cached_value: true))
			{
				bool player_nearby = false;
				if (currentState.Value == UpgradeState.Idle && !upgradeMutex.IsLocked() && Math.Abs(Game1.player.getTileLocationPoint().X - tilePosition.X) <= 1 && Math.Abs(Game1.player.getTileLocationPoint().Y - tilePosition.Y) <= 1)
				{
					player_nearby = true;
				}
				if (player_nearby != isPlayerNearby)
				{
					isPlayerNearby = player_nearby;
					if (isPlayerNearby)
					{
						if (locationRef.Value == Game1.currentLocation)
						{
							Game1.playSound("parrot_squawk");
						}
						squawkTime = 0.5f;
						shakeTime = 0.5f;
						costShakeTime = 0.5f;
						Game1.specialCurrencyDisplay.ShowCurrency("walnuts");
					}
					else
					{
						Game1.specialCurrencyDisplay.ShowCurrency(null);
					}
				}
			}
			if ((UpgradeState)currentState == UpgradeState.Building && parrots.Count < 24)
			{
				if (nextParrotSpawn > 0f)
				{
					nextParrotSpawn -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (nextParrotSpawn <= 0f)
				{
					nextParrotSpawn = 0.05f;
					Microsoft.Xna.Framework.Rectangle spawn_rectangle = upgradeRect.Value;
					spawn_rectangle.Inflate(5, 0);
					parrots.Add(new Parrot(this, Utility.getRandomPositionInThisRectangle(spawn_rectangle, Game1.random), parrots.Count % 10 == 0));
				}
			}
			for (int i = 0; i < parrots.Count; i++)
			{
				if (parrots[i].Update(time))
				{
					parrots.RemoveAt(i);
					i--;
				}
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (IsAvailable(use_cached_value: true) && (parrotPresent || (string)upgradeName == "Hut"))
			{
				int frame = 0;
				Vector2 shake = Vector2.Zero;
				if (squawkTime > 0f)
				{
					frame = 1;
				}
				if (shakeTime > 0f)
				{
					shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
					shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
				}
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, (Utility.PointToVector2(tilePosition) + new Vector2(0.5f, -1f)) * 64f) + shake, new Microsoft.Xna.Framework.Rectangle(frame * 24, 0, 24, 24), Color.White, 0f, new Vector2(12f, 16f), 4f, SpriteEffects.None, (((float)tilePosition.Y + 1f) * 64f - 1f) / 10000f);
			}
		}

		public virtual void DrawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (parrotPresent && IsAvailable(use_cached_value: true) && isPlayerNearby)
			{
				Vector2 cost_shake = Vector2.Zero;
				if (costShakeTime > 0f)
				{
					cost_shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
					cost_shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
				}
				float yOffset2 = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				Vector2 draw_position = Utility.PointToVector2(tilePosition.Value);
				float draw_layer = draw_position.Y * 64f / 10000f;
				yOffset2 -= 72f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_position.X * 64f, draw_position.Y * 64f - 96f - 48f + yOffset2)) + cost_shake, new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
				Vector2 item_draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_position.X * 64f + 32f + 8f, draw_position.Y * 64f - 64f - 32f - 8f + yOffset2)) + cost_shake;
				b.Draw(Game1.objectSpriteSheet, item_draw_position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
				Utility.drawTinyDigits(requiredNuts.Value, b, item_draw_position + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(requiredNuts.Value, 3f)) + 3f - 32f, 16f), 3f, 1f, Color.White);
			}
			foreach (Parrot parrot in parrots)
			{
				parrot.Draw(b);
			}
		}
	}
}
