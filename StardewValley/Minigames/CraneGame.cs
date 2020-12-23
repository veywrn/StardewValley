using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Minigames
{
	public class CraneGame : IMinigame
	{
		public enum GameButtons
		{
			Action,
			Tool,
			Confirm,
			Cancel,
			Run,
			Up,
			Left,
			Down,
			Right,
			MAX
		}

		public class GameLogic : CraneGameObject
		{
			[XmlType("CraneGame.GameStates")]
			public enum GameStates
			{
				Setup,
				Idle,
				MoveClawRight,
				WaitForMoveDown,
				MoveClawDown,
				ClawDescend,
				ClawAscend,
				ClawReturn,
				ClawRelease,
				ClawReset,
				EndGame
			}

			public List<Item> collectedItems;

			public const int CLAW_HEIGHT = 50;

			protected Claw _claw;

			public int maxLives = 3;

			public int lives = 3;

			public Vector2 _startPosition = new Vector2(24f, 56f);

			public Vector2 _dropPosition = new Vector2(32f, 56f);

			public Rectangle playArea = new Rectangle(16, 48, 272, 64);

			public Rectangle prizeChute = new Rectangle(16, 48, 32, 32);

			protected GameStates _currentState;

			protected int _stateTimer;

			public CraneGameObject moveRightIndicator;

			public CraneGameObject moveDownIndicator;

			public CraneGameObject creditsDisplay;

			public CraneGameObject timeDisplay1;

			public CraneGameObject timeDisplay2;

			public CraneGameObject sunShockedFace;

			public int currentTimer;

			public CraneGameObject joystick;

			public int[] conveyerBeltTiles = new int[68]
			{
				0,
				0,
				0,
				0,
				7,
				6,
				6,
				9,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				8,
				0,
				0,
				2,
				0,
				0,
				0,
				7,
				6,
				6,
				6,
				6,
				9,
				0,
				0,
				0,
				0,
				8,
				0,
				0,
				2,
				0,
				0,
				0,
				8,
				0,
				0,
				0,
				0,
				2,
				0,
				0,
				0,
				0,
				1,
				4,
				4,
				3,
				0,
				0,
				0,
				1,
				4,
				4,
				4,
				4,
				3
			};

			public int[] prizeMap = new int[68]
			{
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				1,
				0,
				2,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				0,
				1,
				0,
				0,
				1,
				0,
				0,
				0,
				0,
				1,
				0,
				2,
				0,
				3
			};

			public GameLogic(CraneGame game)
				: base(game)
			{
				_game.music = Game1.soundBank.GetCue("crane_game");
				_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
				_game.music.Play();
				_claw = new Claw(_game);
				_claw.position = _startPosition;
				_claw.zPosition = 50f;
				collectedItems = new List<Item>();
				SetState(GameStates.Setup);
				new Bush(_game, 55, 2, 3, 31, 111);
				new Bush(_game, 45, 2, 2, 112, 84);
				new Bush(_game, 45, 2, 2, 63, 63);
				new Bush(_game, 48, 1, 2, 56, 80);
				new Bush(_game, 48, 1, 2, 72, 80);
				new Bush(_game, 48, 1, 2, 56, 96);
				new Bush(_game, 48, 1, 2, 72, 96);
				new Bush(_game, 48, 1, 2, 56, 112);
				new Bush(_game, 48, 1, 2, 72, 112);
				new Bush(_game, 45, 2, 2, 159, 63);
				new Bush(_game, 48, 1, 2, 152, 80);
				new Bush(_game, 48, 1, 2, 168, 80);
				new Bush(_game, 48, 1, 2, 152, 96);
				new Bush(_game, 48, 1, 2, 168, 96);
				new Bush(_game, 48, 1, 2, 152, 112);
				new Bush(_game, 48, 1, 2, 168, 112);
				sunShockedFace = new CraneGameObject(_game);
				sunShockedFace.SetSpriteFromIndex(9);
				sunShockedFace.position = new Vector2(96f, 0f);
				sunShockedFace.spriteAnchor = Vector2.Zero;
				CraneGameObject craneGameObject = new CraneGameObject(_game);
				craneGameObject.position.X = 16f;
				craneGameObject.position.Y = 87f;
				craneGameObject.SetSpriteFromIndex(3);
				craneGameObject.spriteRect.Width = 32;
				craneGameObject.spriteAnchor = new Vector2(0f, 15f);
				joystick = new CraneGameObject(_game);
				joystick.position.X = 151f;
				joystick.position.Y = 134f;
				joystick.SetSpriteFromIndex(28);
				joystick.spriteRect.Width = 32;
				joystick.spriteRect.Height = 48;
				joystick.spriteAnchor = new Vector2(15f, 47f);
				lives = maxLives;
				moveRightIndicator = new CraneGameObject(_game);
				moveRightIndicator.position.X = 21f;
				moveRightIndicator.position.Y = 126f;
				moveRightIndicator.SetSpriteFromIndex(26);
				moveRightIndicator.spriteAnchor = Vector2.Zero;
				moveRightIndicator.visible = false;
				moveDownIndicator = new CraneGameObject(_game);
				moveDownIndicator.position.X = 49f;
				moveDownIndicator.position.Y = 126f;
				moveDownIndicator.SetSpriteFromIndex(27);
				moveDownIndicator.spriteAnchor = Vector2.Zero;
				moveDownIndicator.visible = false;
				creditsDisplay = new CraneGameObject(_game);
				creditsDisplay.SetSpriteFromIndex(70);
				creditsDisplay.position = new Vector2(234f, 125f);
				creditsDisplay.spriteAnchor = Vector2.Zero;
				timeDisplay1 = new CraneGameObject(_game);
				timeDisplay1.SetSpriteFromIndex(70);
				timeDisplay1.position = new Vector2(274f, 125f);
				timeDisplay1.spriteAnchor = Vector2.Zero;
				timeDisplay2 = new CraneGameObject(_game);
				timeDisplay2.SetSpriteFromIndex(70);
				timeDisplay2.position = new Vector2(285f, 125f);
				timeDisplay2.spriteAnchor = Vector2.Zero;
				int level_width = 17;
				for (int j = 0; j < conveyerBeltTiles.Length; j++)
				{
					if (conveyerBeltTiles[j] != 0)
					{
						int x = j % level_width + 1;
						int y = j / level_width + 3;
						if (conveyerBeltTiles[j] == 8)
						{
							new ConveyerBelt(_game, x, y, 0);
						}
						else if (conveyerBeltTiles[j] == 4)
						{
							new ConveyerBelt(_game, x, y, 3);
						}
						else if (conveyerBeltTiles[j] == 6)
						{
							new ConveyerBelt(_game, x, y, 1);
						}
						else if (conveyerBeltTiles[j] == 2)
						{
							new ConveyerBelt(_game, x, y, 2);
						}
						else if (conveyerBeltTiles[j] == 7)
						{
							new ConveyerBelt(_game, x, y, 1).SetSpriteFromCorner(240, 272);
						}
						else if (conveyerBeltTiles[j] == 9)
						{
							new ConveyerBelt(_game, x, y, 2).SetSpriteFromCorner(240, 240);
						}
						else if (conveyerBeltTiles[j] == 1)
						{
							new ConveyerBelt(_game, x, y, 0).SetSpriteFromCorner(240, 224);
						}
						else if (conveyerBeltTiles[j] == 3)
						{
							new ConveyerBelt(_game, x, y, 3).SetSpriteFromCorner(240, 256);
						}
					}
				}
				Dictionary<int, List<Item>> possible_items = new Dictionary<int, List<Item>>
				{
					[1] = new List<Item>
					{
						new Furniture(1760, Vector2.Zero),
						new Furniture(1761, Vector2.Zero),
						new Furniture(1762, Vector2.Zero),
						new Furniture(1763, Vector2.Zero),
						new Furniture(1764, Vector2.Zero),
						new Furniture(1365, Vector2.Zero)
					}
				};
				List<Item> item_list2 = new List<Item>();
				MovieData current_movie = MovieTheater.GetMovieForDate(Game1.Date);
				if (current_movie != null)
				{
					switch (current_movie.ID)
					{
					case "spring_movie_0":
						item_list2.Add(new Furniture(1952, Vector2.Zero));
						break;
					case "fall_movie_0":
						item_list2.Add(new Furniture(1953, Vector2.Zero));
						break;
					case "summer_movie_0":
						item_list2.Add(new Furniture(1954, Vector2.Zero));
						break;
					case "summer_movie_1":
						item_list2.Add(new Furniture(1955, Vector2.Zero));
						break;
					case "winter_movie_1":
						item_list2.Add(new Furniture(1956, Vector2.Zero));
						break;
					case "winter_movie_0":
						item_list2.Add(new Furniture(1957, Vector2.Zero));
						break;
					case "spring_movie_1":
						item_list2.Add(new Furniture(1958, Vector2.Zero));
						break;
					case "fall_movie_1":
						item_list2.Add(new Furniture(1959, Vector2.Zero));
						break;
					}
				}
				item_list2.Add(new Furniture(1669, Vector2.Zero));
				if (Game1.currentSeason == "spring")
				{
					item_list2.Add(new Furniture(1960, Vector2.Zero));
				}
				else if (Game1.currentSeason == "winter")
				{
					item_list2.Add(new Furniture(1961, Vector2.Zero));
				}
				else if (Game1.currentSeason == "summer")
				{
					item_list2.Add(new Furniture(1294, Vector2.Zero));
				}
				else if (Game1.currentSeason == "fall")
				{
					item_list2.Add(new Furniture(1918, Vector2.Zero));
				}
				item_list2.Add(new Object(Vector2.Zero, 2));
				possible_items[2] = item_list2;
				item_list2 = new List<Item>();
				if (Game1.currentSeason == "spring")
				{
					item_list2.Add(new Object(Vector2.Zero, 107));
					item_list2.Add(new Object(Vector2.Zero, 36));
					item_list2.Add(new Object(Vector2.Zero, 48));
					item_list2.Add(new Object(Vector2.Zero, 184));
					item_list2.Add(new Object(Vector2.Zero, 188));
					item_list2.Add(new Object(Vector2.Zero, 192));
					item_list2.Add(new Object(Vector2.Zero, 204));
				}
				else if (Game1.currentSeason == "winter")
				{
					item_list2.Add(new Furniture(1440, Vector2.Zero));
					item_list2.Add(new Object(Vector2.Zero, 44));
					item_list2.Add(new Object(Vector2.Zero, 40));
					item_list2.Add(new Object(Vector2.Zero, 41));
					item_list2.Add(new Object(Vector2.Zero, 43));
					item_list2.Add(new Object(Vector2.Zero, 42));
				}
				else if (Game1.currentSeason == "summer")
				{
					if (current_movie != null && current_movie.ID == "summer_movie_1")
					{
						item_list2.Add(new Furniture(1371, Vector2.Zero));
						item_list2.Add(new Furniture(1373, Vector2.Zero));
					}
					else
					{
						item_list2.Add(new Furniture(985, Vector2.Zero));
						item_list2.Add(new Furniture(984, Vector2.Zero));
					}
				}
				else if (Game1.currentSeason == "fall")
				{
					item_list2.Add(new Furniture(1917, Vector2.Zero));
					item_list2.Add(new Furniture(1307, Vector2.Zero));
					item_list2.Add(new Object(Vector2.Zero, 47));
					item_list2.Add(new Furniture(1471, Vector2.Zero));
					item_list2.Add(new Furniture(1375, Vector2.Zero));
				}
				possible_items[3] = item_list2;
				for (int i = 0; i < prizeMap.Length; i++)
				{
					if (prizeMap[i] == 0)
					{
						continue;
					}
					int x2 = i % level_width + 1;
					int y2 = i / level_width + 3;
					Item item = null;
					int prize_rarity = i;
					while (prize_rarity > 0 && item == null)
					{
						if (prizeMap[i] == 1)
						{
							item = Utility.GetRandom(possible_items[1]);
						}
						else if (prizeMap[i] == 2)
						{
							item = Utility.GetRandom(possible_items[2]);
						}
						else if (prizeMap[i] == 3)
						{
							item = Utility.GetRandom(possible_items[3]);
						}
						prize_rarity--;
					}
					new Prize(_game, item)
					{
						position = 
						{
							X = x2 * 16 + 8,
							Y = y2 * 16 + 8
						}
					};
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					Item item2 = null;
					Vector2 prizePosition = new Vector2(0f, 4f);
					switch (Game1.random.Next(4))
					{
					case 0:
						item2 = new Object(107, 1);
						break;
					case 1:
						item2 = new Object(749, 5);
						break;
					case 2:
						item2 = new Object(688, 5);
						break;
					case 3:
						item2 = new Object(288, 5);
						break;
					}
					new Prize(_game, item2)
					{
						position = 
						{
							X = prizePosition.X * 16f + 30f,
							Y = prizePosition.Y * 16f + 32f
						}
					};
				}
				else if (Game1.random.NextDouble() < 0.2)
				{
					new Prize(_game, new Object(809, 1))
					{
						position = 
						{
							X = 160f,
							Y = 58f
						}
					};
				}
				if (Game1.random.NextDouble() < 0.25)
				{
					new Prize(_game, new Furniture(986, Vector2.Zero))
					{
						position = new Vector2(263f, 56f),
						zPosition = 0f
					};
					new Prize(_game, new Furniture(986, Vector2.Zero))
					{
						position = new Vector2(215f, 56f),
						zPosition = 0f
					};
				}
				else
				{
					new Prize(_game, new Furniture(989, Vector2.Zero))
					{
						position = new Vector2(263f, 56f),
						zPosition = 0f
					};
					new Prize(_game, new Furniture(989, Vector2.Zero))
					{
						position = new Vector2(215f, 56f),
						zPosition = 0f
					};
				}
			}

			public GameStates GetCurrentState()
			{
				return _currentState;
			}

			public override void Update(GameTime time)
			{
				float desired_joystick_rotation = 0f;
				foreach (Shadow shadow in _game.GetObjectsOfType<Shadow>())
				{
					if (prizeChute.Contains(new Point((int)shadow.position.X, (int)shadow.position.Y)))
					{
						shadow.visible = false;
					}
					else
					{
						shadow.visible = true;
					}
				}
				int displayed_time = currentTimer / 60;
				if (_currentState == GameStates.Setup)
				{
					creditsDisplay.SetSpriteFromIndex(70);
				}
				else
				{
					creditsDisplay.SetSpriteFromIndex(70 + lives);
				}
				timeDisplay1.SetSpriteFromIndex(70 + displayed_time / 10);
				timeDisplay2.SetSpriteFromIndex(70 + displayed_time % 10);
				if (currentTimer < 0)
				{
					timeDisplay1.SetSpriteFromIndex(80);
					timeDisplay2.SetSpriteFromIndex(81);
				}
				if (_currentState == GameStates.Setup)
				{
					if (!_game.music.IsPlaying)
					{
						_game.music.Play();
					}
					_claw.openAngle = 40f;
					bool is_something_busy2 = false;
					foreach (Prize item2 in _game.GetObjectsOfType<Prize>())
					{
						if (!item2.CanBeGrabbed())
						{
							is_something_busy2 = true;
							break;
						}
					}
					if (!is_something_busy2)
					{
						if (_stateTimer >= 10)
						{
							SetState(GameStates.Idle);
						}
					}
					else
					{
						_stateTimer = 0;
					}
				}
				else if (_currentState == GameStates.Idle)
				{
					if (!_game.music.IsPlaying)
					{
						_game.music.Play();
					}
					if (_game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.Immediate);
						_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					currentTimer = 900;
					moveRightIndicator.visible = (Game1.ticks / 20 % 2 == 0);
					if (_game.IsButtonPressed(GameButtons.Tool) || _game.IsButtonPressed(GameButtons.Action) || _game.IsButtonPressed(GameButtons.Right))
					{
						Game1.playSound("bigSelect");
						SetState(GameStates.MoveClawRight);
					}
				}
				else if (_currentState == GameStates.MoveClawRight)
				{
					desired_joystick_rotation = 15f;
					if (_stateTimer < 15)
					{
						if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Action) && !_game.IsButtonDown(GameButtons.Right))
						{
							Game1.playSound("bigDeSelect");
							SetState(GameStates.Idle);
							return;
						}
					}
					else
					{
						if (_game.craneSound == null || !_game.craneSound.IsPlaying)
						{
							_game.craneSound = Game1.soundBank.GetCue("crane");
							_game.craneSound.Play();
							_game.craneSound.SetVariable("Pitch", 1200);
						}
						currentTimer--;
						if (currentTimer <= 0)
						{
							SetState(GameStates.ClawDescend);
							currentTimer = -1;
							if (_game.craneSound != null && !_game.craneSound.IsStopped)
							{
								_game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						moveRightIndicator.visible = true;
						if (_stateTimer > 10)
						{
							if (_stateTimer == 11)
							{
								_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
								_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
								_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
							}
							if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Right) && !_game.IsButtonDown(GameButtons.Action))
							{
								Game1.playSound("bigDeSelect");
								_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
								_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
								SetState(GameStates.WaitForMoveDown);
								moveRightIndicator.visible = false;
								if (_game.craneSound != null && !_game.craneSound.IsStopped)
								{
									_game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
							else
							{
								_claw.Move(0.5f, 0f);
								if (_claw.GetBounds().Right >= playArea.Right)
								{
									_claw.Move(-0.5f, 0f);
								}
							}
						}
					}
				}
				else if (_currentState == GameStates.WaitForMoveDown)
				{
					currentTimer--;
					if (currentTimer <= 0)
					{
						SetState(GameStates.ClawDescend);
						currentTimer = -1;
					}
					moveDownIndicator.visible = (Game1.ticks / 20 % 2 == 0);
					if (_game.IsButtonPressed(GameButtons.Tool) || _game.IsButtonPressed(GameButtons.Down) || _game.IsButtonPressed(GameButtons.Action))
					{
						Game1.playSound("bigSelect");
						SetState(GameStates.MoveClawDown);
					}
				}
				else if (_currentState == GameStates.MoveClawDown)
				{
					if (_game.craneSound == null || !_game.craneSound.IsPlaying)
					{
						_game.craneSound = Game1.soundBank.GetCue("crane");
						_game.craneSound.SetVariable("Pitch", 1200);
						_game.craneSound.Play();
					}
					currentTimer--;
					if (currentTimer <= 0)
					{
						SetState(GameStates.ClawDescend);
						currentTimer = -1;
						if (_game.craneSound != null && !_game.craneSound.IsStopped)
						{
							_game.craneSound.Stop(AudioStopOptions.Immediate);
						}
					}
					desired_joystick_rotation = -5f;
					moveDownIndicator.visible = true;
					if (_stateTimer > 10)
					{
						if (_stateTimer == 11)
						{
							_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
							_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
							_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
						}
						if (!_game.IsButtonDown(GameButtons.Tool) && !_game.IsButtonDown(GameButtons.Down) && !_game.IsButtonDown(GameButtons.Action))
						{
							Game1.playSound("bigDeSelect");
							_claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
							_claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
							moveDownIndicator.visible = false;
							SetState(GameStates.ClawDescend);
							if (_game.craneSound != null && !_game.craneSound.IsStopped)
							{
								_game.craneSound.Stop(AudioStopOptions.Immediate);
							}
						}
						else
						{
							_claw.Move(0f, 0.5f);
							if (_claw.GetBounds().Bottom >= playArea.Bottom)
							{
								_claw.Move(0f, -0.5f);
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawDescend)
				{
					if (_claw.openAngle < 40f)
					{
						_claw.openAngle += 1.5f;
						_stateTimer = 0;
					}
					else if (_stateTimer > 30)
					{
						if (_game.craneSound != null && _game.craneSound.IsPlaying)
						{
							_game.craneSound.SetVariable("Pitch", 2000);
						}
						else
						{
							_game.craneSound = Game1.soundBank.GetCue("crane");
							_game.craneSound.SetVariable("Pitch", 2000);
							_game.craneSound.Play();
						}
						if (_claw.zPosition > 0f)
						{
							_claw.zPosition -= 0.5f;
							if (_claw.zPosition <= 0f)
							{
								_claw.zPosition = 0f;
								SetState(GameStates.ClawAscend);
								if (_game.craneSound != null && !_game.craneSound.IsStopped)
								{
									_game.craneSound.Stop(AudioStopOptions.Immediate);
								}
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawAscend)
				{
					if (_claw.openAngle > 0f && _claw.GetGrabbedPrize() == null)
					{
						_claw.openAngle -= 1f;
						if (_claw.openAngle == 15f)
						{
							_claw.GrabObject();
							if (_claw.GetGrabbedPrize() != null)
							{
								Game1.playSound("FishHit");
								sunShockedFace.ApplyDrawEffect(new ShakeEffect(1f, 1f, 5));
								_game.freezeFrames = 60;
								if (_game.music.IsPlaying)
								{
									_game.music.Stop(AudioStopOptions.Immediate);
									_game.music = Game1.soundBank.GetCue("crane_game");
								}
							}
						}
						else if (_claw.openAngle == 0f && _claw.GetGrabbedPrize() == null)
						{
							if (lives == 1)
							{
								_game.music.Stop(AudioStopOptions.Immediate);
								Game1.playSound("fishEscape");
							}
							else
							{
								Game1.playSound("stoneStep");
							}
						}
						_stateTimer = 0;
					}
					else
					{
						if (_claw.GetGrabbedPrize() != null)
						{
							if (!_game.fastMusic.IsPlaying)
							{
								_game.fastMusic.Play();
							}
						}
						else if (_game.fastMusic.IsPlaying)
						{
							_game.fastMusic.Stop(AudioStopOptions.AsAuthored);
							_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
						}
						if (_claw.zPosition < 50f)
						{
							_claw.zPosition += 0.5f;
							if (_claw.zPosition >= 50f)
							{
								_claw.zPosition = 50f;
								SetState(GameStates.ClawReturn);
								if (_claw.GetGrabbedPrize() == null && lives == 1)
								{
									SetState(GameStates.EndGame);
								}
							}
						}
						_claw.CheckDropPrize();
					}
				}
				else if (_currentState == GameStates.ClawReturn)
				{
					if (_claw.GetGrabbedPrize() != null)
					{
						if (!_game.fastMusic.IsPlaying)
						{
							_game.fastMusic.Play();
						}
					}
					else if (_game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.AsAuthored);
						_game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
					}
					if (_stateTimer > 10)
					{
						if (_claw.position.Equals(_dropPosition))
						{
							SetState(GameStates.ClawRelease);
						}
						else
						{
							float move_speed2 = 0.5f;
							if (_claw.GetGrabbedPrize() == null)
							{
								move_speed2 = 0.75f;
							}
							if (_claw.position.X != _dropPosition.X)
							{
								_claw.position.X = Utility.MoveTowards(_claw.position.X, _dropPosition.X, move_speed2);
							}
							if (_claw.position.X != _dropPosition.Y)
							{
								_claw.position.Y = Utility.MoveTowards(_claw.position.Y, _dropPosition.Y, move_speed2);
							}
						}
					}
					_claw.CheckDropPrize();
				}
				else if (_currentState == GameStates.ClawRelease)
				{
					bool clawHadPrize = _claw.GetGrabbedPrize() != null;
					if (_stateTimer > 10)
					{
						_claw.ReleaseGrabbedObject();
						if (_claw.openAngle < 40f)
						{
							Claw claw = _claw;
							float openAngle = claw.openAngle;
							claw.openAngle = openAngle + 1f;
						}
						else
						{
							SetState(GameStates.ClawReset);
							if (!clawHadPrize)
							{
								Game1.playSound("button1");
								_claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
							}
						}
					}
				}
				else if (_currentState == GameStates.ClawReset)
				{
					if (_stateTimer > 50)
					{
						if (_claw.position.Equals(_startPosition))
						{
							lives--;
							if (lives <= 0)
							{
								SetState(GameStates.EndGame);
							}
							else
							{
								SetState(GameStates.Idle);
							}
						}
						else
						{
							float move_speed = 0.5f;
							if (_claw.position.X != _startPosition.X)
							{
								_claw.position.X = Utility.MoveTowards(_claw.position.X, _startPosition.X, move_speed);
							}
							if (_claw.position.X != _startPosition.Y)
							{
								_claw.position.Y = Utility.MoveTowards(_claw.position.Y, _startPosition.Y, move_speed);
							}
						}
					}
				}
				else if (_currentState == GameStates.EndGame)
				{
					if (_game.music.IsPlaying)
					{
						_game.music.Stop(AudioStopOptions.Immediate);
					}
					if (_game.fastMusic.IsPlaying)
					{
						_game.fastMusic.Stop(AudioStopOptions.Immediate);
					}
					bool is_something_busy = false;
					foreach (Prize item3 in _game.GetObjectsOfType<Prize>())
					{
						if (!item3.CanBeGrabbed())
						{
							is_something_busy = true;
							break;
						}
					}
					if (!is_something_busy && _stateTimer >= 20)
					{
						if (collectedItems.Count > 0)
						{
							List<Item> items = new List<Item>();
							foreach (Item item in collectedItems)
							{
								items.Add(item.getOne());
							}
							Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, _game);
						}
						_game.Quit();
					}
				}
				sunShockedFace.visible = (_claw.GetGrabbedPrize() != null);
				joystick.rotation = Utility.MoveTowards(joystick.rotation, desired_joystick_rotation, 2f);
				_stateTimer++;
			}

			public override void Draw(SpriteBatch b, float layer_depth)
			{
			}

			public void SetState(GameStates new_state)
			{
				_currentState = new_state;
				_stateTimer = 0;
			}
		}

		public class Trampoline : CraneGameObject
		{
			public Trampoline(CraneGame game, int x, int y)
				: base(game)
			{
				SetSpriteFromIndex(30);
				spriteRect.Width = 32;
				spriteRect.Height = 32;
				spriteAnchor.X = 15f;
				spriteAnchor.Y = 15f;
				position.X = x;
				position.Y = y;
			}
		}

		public class Shadow : CraneGameObject
		{
			public CraneGameObject _target;

			public Shadow(CraneGame game, CraneGameObject target)
				: base(game)
			{
				SetSpriteFromIndex(2);
				layerDepth = 900f;
				_target = target;
			}

			public override void Update(GameTime time)
			{
				if (_target != null)
				{
					position = _target.position;
				}
				if (_target is Prize && (_target as Prize).grabbed)
				{
					visible = false;
				}
				if (_target.IsDestroyed())
				{
					Destroy();
					return;
				}
				color.A = (byte)(Math.Min(1f, _target.zPosition / 50f) * 255f);
				scale = Utility.Lerp(1f, 0.5f, Math.Min(_target.zPosition / 100f, 1f)) * new Vector2(1f, 1f);
			}
		}

		public class Claw : CraneGameObject
		{
			protected CraneGameObject _leftArm;

			protected CraneGameObject _rightArm;

			protected Prize _grabbedPrize;

			protected Vector2 _prizePositionOffset;

			protected int _nextDropCheckTimer;

			protected int _dropChances;

			protected int _grabTime;

			public float openAngle
			{
				get
				{
					return _leftArm.rotation;
				}
				set
				{
					_leftArm.rotation = value;
				}
			}

			public Claw(CraneGame game)
				: base(game)
			{
				SetSpriteFromIndex();
				spriteAnchor = new Vector2(8f, 24f);
				_leftArm = new CraneGameObject(game);
				_leftArm.SetSpriteFromIndex(1);
				_leftArm.spriteAnchor = new Vector2(16f, 0f);
				_rightArm = new CraneGameObject(game);
				_rightArm.SetSpriteFromIndex(1);
				_rightArm.flipX = true;
				_rightArm.spriteAnchor = new Vector2(0f, 0f);
				new Shadow(_game, this);
			}

			public void CheckDropPrize()
			{
				if (_grabbedPrize == null)
				{
					return;
				}
				_nextDropCheckTimer--;
				if (_nextDropCheckTimer > 0)
				{
					return;
				}
				float drop_chance2 = _prizePositionOffset.Length() * 0.1f;
				drop_chance2 += zPosition * 0.001f;
				if (_grabbedPrize.isLargeItem)
				{
					drop_chance2 += 0.1f;
				}
				double roll = Game1.random.NextDouble();
				if (roll < (double)drop_chance2)
				{
					_dropChances--;
					if (_dropChances <= 0)
					{
						Game1.playSound("fishEscape");
						ReleaseGrabbedObject();
					}
					else
					{
						Game1.playSound("bob");
						_grabbedPrize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 50));
						_grabbedPrize.rotation += (float)Game1.random.NextDouble() * 10f;
					}
				}
				else if (roll < (double)drop_chance2)
				{
					Game1.playSound("dwop");
					_grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 50));
				}
				_nextDropCheckTimer = Game1.random.Next(50, 100);
			}

			public void ApplyDrawEffectToArms(DrawEffect new_effect)
			{
				_leftArm.ApplyDrawEffect(new_effect);
				_rightArm.ApplyDrawEffect(new_effect);
			}

			public void ReleaseGrabbedObject()
			{
				if (_grabbedPrize != null)
				{
					_grabbedPrize.grabbed = false;
					_grabbedPrize.OnDrop();
					_grabbedPrize = null;
				}
			}

			public void GrabObject()
			{
				Prize closest_prize = null;
				float closest_distance = 0f;
				foreach (Prize prize in _game.GetObjectsAtPoint<Prize>(position))
				{
					if (!prize.IsDestroyed() && prize.CanBeGrabbed())
					{
						float distance = (position - prize.position).LengthSquared();
						if (closest_prize == null || distance < closest_distance)
						{
							closest_distance = distance;
							closest_prize = prize;
						}
					}
				}
				if (closest_prize != null)
				{
					_grabbedPrize = closest_prize;
					_grabbedPrize.grabbed = true;
					_prizePositionOffset = _grabbedPrize.position - position;
					_nextDropCheckTimer = Game1.random.Next(50, 100);
					_dropChances = 3;
					Game1.playSound("pickUpItem");
					_grabTime = 0;
					_grabbedPrize.ApplyDrawEffect(new StretchEffect(0.95f, 1.1f));
					_grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 20));
				}
			}

			public override void Move(float x, float y)
			{
				base.Move(x, y);
			}

			public Prize GetGrabbedPrize()
			{
				return _grabbedPrize;
			}

			public override void Update(GameTime time)
			{
				_leftArm.position = position + new Vector2(0f, -16f);
				_rightArm.position = position + new Vector2(0f, -16f);
				_rightArm.rotation = 0f - _leftArm.rotation;
				_leftArm.layerDepth = (_rightArm.layerDepth = GetRendererLayerDepth() + 0.01f);
				_leftArm.zPosition = (_rightArm.zPosition = zPosition);
				if (_grabbedPrize != null)
				{
					_grabbedPrize.position = position + _prizePositionOffset * Utility.Lerp(1f, 0.25f, Math.Min(1f, (float)_grabTime / 200f));
					_grabbedPrize.zPosition = zPosition + _grabbedPrize.GetRestingZPosition();
				}
				_grabTime++;
			}

			public override void Destroy()
			{
				_leftArm.Destroy();
				_rightArm.Destroy();
				base.Destroy();
			}
		}

		public class ConveyerBelt : CraneGameObject
		{
			protected int _direction;

			protected Vector2 _spriteStartPosition;

			protected int _spriteOffset;

			public int GetDirection()
			{
				return _direction;
			}

			public ConveyerBelt(CraneGame game, int x, int y, int direction)
				: base(game)
			{
				position.X = x * 16;
				position.Y = y * 16;
				_direction = direction;
				spriteAnchor = Vector2.Zero;
				layerDepth = 1000f;
				if (_direction == 0)
				{
					SetSpriteFromIndex(5);
				}
				else if (_direction == 2)
				{
					SetSpriteFromIndex(10);
				}
				else if (_direction == 3)
				{
					SetSpriteFromIndex(15);
				}
				else if (_direction == 1)
				{
					SetSpriteFromIndex(20);
				}
				_spriteStartPosition = new Vector2(spriteRect.X, spriteRect.Y);
			}

			public void SetSpriteFromCorner(int x, int y)
			{
				spriteRect.X = x;
				spriteRect.Y = y;
				_spriteStartPosition = new Vector2(spriteRect.X, spriteRect.Y);
			}

			public override void Update(GameTime time)
			{
				int ticks_per_frame = 4;
				int frame_count = 4;
				spriteRect.X = (int)_spriteStartPosition.X + _spriteOffset / ticks_per_frame * 16;
				_spriteOffset++;
				if (_spriteOffset >= (frame_count - 1) * ticks_per_frame)
				{
					_spriteOffset = 0;
				}
			}
		}

		public class Bush : CraneGameObject
		{
			public Bush(CraneGame game, int tile_index, int tile_width, int tile_height, int x, int y)
				: base(game)
			{
				SetSpriteFromIndex(tile_index);
				spriteRect.Width = tile_width * 16;
				spriteRect.Height = tile_height * 16;
				spriteAnchor.X = (float)spriteRect.Width / 2f;
				spriteAnchor.Y = spriteRect.Height;
				if (tile_height > 16)
				{
					spriteAnchor.Y -= 8f;
				}
				else
				{
					spriteAnchor.Y -= 4f;
				}
				position.X = x;
				position.Y = y;
			}

			public override void Update(GameTime time)
			{
				rotation = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 0.0024999999441206455 + (double)position.Y + (double)(position.X * 2f)) * 2f;
			}
		}

		public class Prize : CraneGameObject
		{
			protected Vector2 _conveyerBeltMove;

			public bool grabbed;

			public float gravity;

			protected Vector2 _velocity = Vector2.Zero;

			protected Item _item;

			protected float _restingZPosition;

			protected float _angularSpeed;

			protected bool _isBeingCollected;

			public bool isLargeItem;

			public float GetRestingZPosition()
			{
				return _restingZPosition;
			}

			public Prize(CraneGame game, Item item)
				: base(game)
			{
				SetSpriteFromIndex(3);
				spriteAnchor = new Vector2(8f, 12f);
				_item = item;
				_UpdateItemSprite();
				new Shadow(_game, this);
			}

			public void OnDrop()
			{
				if (!isLargeItem)
				{
					_angularSpeed = Utility.Lerp(-5f, 5f, (float)Game1.random.NextDouble());
				}
				else
				{
					rotation = 0f;
				}
			}

			public void _UpdateItemSprite()
			{
				if (_item is Furniture)
				{
					texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\furniture");
					spriteRect = (_item as Furniture).defaultSourceRect.Value;
				}
				else if (_item is Object)
				{
					Object prize_object = _item as Object;
					if ((bool)prize_object.bigCraftable)
					{
						texture = Game1.bigCraftableSpriteSheet;
						spriteRect = Object.getSourceRectForBigCraftable(prize_object.ParentSheetIndex);
					}
					else
					{
						texture = Game1.objectSpriteSheet;
						spriteRect = GameLocation.getSourceRectForObject(prize_object.ParentSheetIndex);
					}
				}
				width = spriteRect.Width;
				height = spriteRect.Height;
				if (width > 16 || height > 16)
				{
					isLargeItem = true;
				}
				else
				{
					isLargeItem = false;
				}
				if (height <= 16)
				{
					spriteAnchor = new Vector2(width / 2, (float)height - 4f);
				}
				else
				{
					spriteAnchor = new Vector2(width / 2, (float)height - 8f);
				}
				_restingZPosition = 0f;
			}

			public bool CanBeGrabbed()
			{
				if (IsDestroyed())
				{
					return false;
				}
				if (_isBeingCollected)
				{
					return false;
				}
				if (zPosition != _restingZPosition)
				{
					return false;
				}
				return true;
			}

			public override void Update(GameTime time)
			{
				if (_isBeingCollected)
				{
					Vector4 color_vector = color.ToVector4();
					color_vector.X = Utility.MoveTowards(color_vector.X, 0f, 0.05f);
					color_vector.Y = Utility.MoveTowards(color_vector.Y, 0f, 0.05f);
					color_vector.Z = Utility.MoveTowards(color_vector.Z, 0f, 0.05f);
					color_vector.W = Utility.MoveTowards(color_vector.W, 0f, 0.05f);
					color = new Color(color_vector);
					scale.X = Utility.MoveTowards(scale.X, 0.5f, 0.05f);
					scale.Y = Utility.MoveTowards(scale.Y, 0.5f, 0.05f);
					if (color_vector.W == 0f)
					{
						Game1.playSound("Ship");
						Destroy();
					}
					position.Y += 0.5f;
				}
				else
				{
					if (grabbed)
					{
						return;
					}
					if (_velocity.X != 0f || _velocity.Y != 0f)
					{
						position.X += _velocity.X;
						if (!_game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)position.X, (int)position.Y)))
						{
							position.X -= _velocity.X;
							_velocity.X *= -1f;
						}
						position.Y += _velocity.Y;
						if (!_game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)position.X, (int)position.Y)))
						{
							position.Y -= _velocity.Y;
							_velocity.Y *= -1f;
						}
					}
					if (zPosition < _restingZPosition)
					{
						zPosition = _restingZPosition;
					}
					if (zPosition > _restingZPosition || _velocity != Vector2.Zero || gravity != 0f)
					{
						if (!isLargeItem)
						{
							rotation += _angularSpeed;
						}
						_conveyerBeltMove = Vector2.Zero;
						if (zPosition > _restingZPosition)
						{
							gravity += 0.1f;
						}
						zPosition -= gravity;
						if (!(zPosition < _restingZPosition))
						{
							return;
						}
						zPosition = _restingZPosition;
						if (!(gravity >= 0f))
						{
							return;
						}
						if (!isLargeItem)
						{
							_angularSpeed = Utility.Lerp(-10f, 10f, (float)Game1.random.NextDouble());
						}
						gravity = (0f - gravity) * 0.6f;
						if (_game.GetObjectsOfType<GameLogic>()[0].prizeChute.Contains(new Point((int)position.X, (int)position.Y)))
						{
							if (_game.GetObjectsOfType<GameLogic>()[0].GetCurrentState() != 0)
							{
								Game1.playSound("reward");
								_isBeingCollected = true;
								_game.GetObjectsOfType<GameLogic>()[0].collectedItems.Add(_item);
							}
							else
							{
								gravity = -2.5f;
								Vector2 offset3 = new Vector2(_game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, _game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(position.X, position.Y);
								offset3.Normalize();
								_velocity = offset3 * Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
							}
						}
						else if (_game.GetOverlaps<Trampoline>(this, 1).Count > 0)
						{
							Trampoline trampoline = _game.GetOverlaps<Trampoline>(this, 1)[0];
							Game1.playSound("axchop");
							trampoline.ApplyDrawEffect(new StretchEffect(0.75f, 0.75f, 5));
							trampoline.ApplyDrawEffect(new ShakeEffect(2f, 2f));
							ApplyDrawEffect(new ShakeEffect(2f, 2f));
							gravity = -2.5f;
							Vector2 offset2 = new Vector2(_game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, _game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(position.X, position.Y);
							offset2.Normalize();
							_velocity = offset2 * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
						}
						else if (Math.Abs(gravity) < 1.5f)
						{
							rotation = 0f;
							_velocity = Vector2.Zero;
							gravity = 0f;
						}
						else
						{
							bool bumped_object = false;
							foreach (Prize prize in _game.GetOverlaps<Prize>(this))
							{
								if (prize.gravity == 0f && prize.CanBeGrabbed())
								{
									Vector2 offset = position - prize.position;
									offset.Normalize();
									_velocity = offset * Utility.Lerp(0.25f, 1f, (float)Game1.random.NextDouble());
									if (!prize.isLargeItem || isLargeItem)
									{
										prize._velocity = -offset * Utility.Lerp(0.75f, 1.5f, (float)Game1.random.NextDouble());
										prize.gravity = gravity * 0.75f;
										prize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
									}
									bumped_object = true;
								}
							}
							ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
							if (!bumped_object)
							{
								float rad_angle = Utility.Lerp(0f, (float)Math.PI * 2f, (float)Game1.random.NextDouble());
								_velocity = new Vector2((float)Math.Sin(rad_angle), (float)Math.Cos(rad_angle)) * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
							}
						}
					}
					else if (_conveyerBeltMove.X == 0f && _conveyerBeltMove.Y == 0f)
					{
						List<ConveyerBelt> belts = _game.GetObjectsAtPoint<ConveyerBelt>(position, 1);
						if (belts.Count > 0)
						{
							switch (belts[0].GetDirection())
							{
							case 0:
								_conveyerBeltMove = new Vector2(0f, -16f);
								break;
							case 2:
								_conveyerBeltMove = new Vector2(0f, 16f);
								break;
							case 3:
								_conveyerBeltMove = new Vector2(-16f, 0f);
								break;
							case 1:
								_conveyerBeltMove = new Vector2(16f, 0f);
								break;
							}
						}
					}
					else
					{
						float move_speed = 0.3f;
						if (_conveyerBeltMove.X != 0f)
						{
							Move(move_speed * (float)Math.Sign(_conveyerBeltMove.X), 0f);
							_conveyerBeltMove.X = Utility.MoveTowards(_conveyerBeltMove.X, 0f, move_speed);
						}
						if (_conveyerBeltMove.Y != 0f)
						{
							Move(0f, move_speed * (float)Math.Sign(_conveyerBeltMove.Y));
							_conveyerBeltMove.Y = Utility.MoveTowards(_conveyerBeltMove.Y, 0f, move_speed);
						}
					}
				}
			}
		}

		public class CraneGameObject
		{
			protected CraneGame _game;

			public Vector2 position = Vector2.Zero;

			public float rotation;

			public Vector2 scale = new Vector2(1f, 1f);

			public bool flipX;

			public bool flipY;

			public Rectangle spriteRect;

			public Texture2D texture;

			public Vector2 spriteAnchor;

			public Color color = Color.White;

			public float layerDepth = -1f;

			public int width = 16;

			public int height = 16;

			public float zPosition;

			public bool visible = true;

			public List<DrawEffect> drawEffects;

			protected bool _destroyed;

			public CraneGameObject(CraneGame game)
			{
				_game = game;
				texture = _game.spriteSheet;
				spriteRect = new Rectangle(0, 0, 16, 16);
				spriteAnchor = new Vector2(8f, 8f);
				drawEffects = new List<DrawEffect>();
				_game.RegisterGameObject(this);
			}

			public void SetSpriteFromIndex(int index = 0)
			{
				spriteRect.X = 304 + index % 5 * 16;
				spriteRect.Y = index / 5 * 16;
			}

			public bool IsDestroyed()
			{
				return _destroyed;
			}

			public virtual void Destroy()
			{
				_destroyed = true;
				_game.UnregisterGameObject(this);
			}

			public virtual void Move(float x, float y)
			{
				position.X += x;
				position.Y += y;
			}

			public Rectangle GetBounds()
			{
				return new Rectangle((int)(position.X - spriteAnchor.X), (int)(position.Y - spriteAnchor.Y), width, height);
			}

			public virtual void Update(GameTime time)
			{
			}

			public float GetRendererLayerDepth()
			{
				float layer_depth = layerDepth;
				if (layer_depth < 0f)
				{
					layer_depth = (float)_game.gameHeight - position.Y;
				}
				return layer_depth;
			}

			public void ApplyDrawEffect(DrawEffect new_effect)
			{
				drawEffects.Add(new_effect);
			}

			public virtual void Draw(SpriteBatch b, float layer_depth)
			{
				if (!visible)
				{
					return;
				}
				SpriteEffects effects = SpriteEffects.None;
				if (flipX)
				{
					effects |= SpriteEffects.FlipHorizontally;
				}
				if (flipY)
				{
					effects |= SpriteEffects.FlipVertically;
				}
				float drawn_rotation = rotation;
				Vector2 drawn_scale = scale;
				Vector2 drawn_position = position - new Vector2(0f, zPosition);
				for (int i = 0; i < drawEffects.Count; i++)
				{
					if (drawEffects[i].Apply(ref drawn_position, ref drawn_rotation, ref drawn_scale))
					{
						drawEffects.RemoveAt(i);
						i--;
					}
				}
				b.Draw(texture, _game.upperLeft + drawn_position * 4f, spriteRect, color, drawn_rotation * ((float)Math.PI / 180f), spriteAnchor, 4f * drawn_scale, effects, layer_depth);
			}
		}

		public class SwayEffect : DrawEffect
		{
			public float swayMagnitude;

			public float swaySpeed;

			public int swayDuration = 1;

			public int age;

			public SwayEffect(float magnitude, float speed = 1f, int sway_duration = 10)
			{
				swayMagnitude = magnitude;
				swaySpeed = speed;
				swayDuration = sway_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > swayDuration)
				{
					return true;
				}
				float progress = (float)age / (float)swayDuration;
				rotation += (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000.0 * 360.0 * (double)swaySpeed * 0.01745329238474369) * (1f - progress) * swayMagnitude;
				age++;
				return false;
			}
		}

		public class ShakeEffect : DrawEffect
		{
			public Vector2 shakeAmount;

			public int shakeDuration = 1;

			public int age;

			public ShakeEffect(float shake_x, float shake_y, int shake_duration = 10)
			{
				shakeAmount = new Vector2(shake_x, shake_y);
				shakeDuration = shake_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > shakeDuration)
				{
					return true;
				}
				float progress = (float)age / (float)shakeDuration;
				Vector2 current_shake = new Vector2(Utility.Lerp(shakeAmount.X, 1f, progress), Utility.Lerp(shakeAmount.Y, 1f, progress));
				position += new Vector2((float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.X, (float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.Y);
				age++;
				return false;
			}
		}

		public class StretchEffect : DrawEffect
		{
			public Vector2 stretchScale;

			public int stretchDuration = 1;

			public int age;

			public StretchEffect(float x_scale, float y_scale, int stretch_duration = 10)
			{
				stretchScale = new Vector2(x_scale, y_scale);
				stretchDuration = stretch_duration;
				age = 0;
			}

			public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				if (age > stretchDuration)
				{
					return true;
				}
				float progress = (float)age / (float)stretchDuration;
				Vector2 current_scale = new Vector2(Utility.Lerp(stretchScale.X, 1f, progress), Utility.Lerp(stretchScale.Y, 1f, progress));
				scale *= current_scale;
				age++;
				return false;
			}
		}

		public class DrawEffect
		{
			public virtual bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
			{
				return true;
			}
		}

		public int gameWidth = 304;

		public int gameHeight = 150;

		protected LocalizedContentManager _content;

		public Texture2D spriteSheet;

		public Vector2 upperLeft;

		protected List<CraneGameObject> _gameObjects;

		protected Dictionary<GameButtons, int> _buttonStates;

		protected bool _shouldQuit;

		public Action onQuit;

		public ICue music;

		public ICue fastMusic;

		public Effect _effect;

		public int freezeFrames;

		public ICue craneSound;

		public List<Type> _gameObjectTypes;

		public Dictionary<Type, List<CraneGameObject>> _gameObjectsByType;

		public CraneGame()
		{
			Utility.farmerHeardSong("crane_game");
			Utility.farmerHeardSong("crane_game_fast");
			_effect = Game1.content.Load<Effect>("Effects\\ShadowRemove");
			_content = Game1.content.CreateTemporary();
			spriteSheet = _content.Load<Texture2D>("LooseSprites\\CraneGame");
			_buttonStates = new Dictionary<GameButtons, int>();
			_gameObjects = new List<CraneGameObject>();
			_gameObjectTypes = new List<Type>();
			_gameObjectsByType = new Dictionary<Type, List<CraneGameObject>>();
			changeScreenSize();
			new GameLogic(this);
			for (int i = 0; i < 9; i++)
			{
				_buttonStates[(GameButtons)i] = 0;
			}
		}

		public void Quit()
		{
			if (!_shouldQuit)
			{
				if (onQuit != null)
				{
					onQuit();
				}
				_shouldQuit = true;
			}
		}

		protected void _UpdateInput()
		{
			HashSet<InputButton> additional_keys = new HashSet<InputButton>();
			if (Game1.options.gamepadControls)
			{
				GamePadState pad_state = Game1.input.GetGamePadState();
				ButtonCollection.ButtonEnumerator enumerator = new ButtonCollection(ref pad_state).GetEnumerator();
				while (enumerator.MoveNext())
				{
					Keys key = Utility.mapGamePadButtonToKey(enumerator.Current);
					additional_keys.Add(new InputButton(key));
				}
			}
			if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
			{
				additional_keys.Add(new InputButton(mouseLeft: true));
			}
			else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
			{
				additional_keys.Add(new InputButton(mouseLeft: false));
			}
			_UpdateButtonState(GameButtons.Action, Game1.options.actionButton, additional_keys);
			_UpdateButtonState(GameButtons.Tool, Game1.options.useToolButton, additional_keys);
			_UpdateButtonState(GameButtons.Confirm, Game1.options.menuButton, additional_keys);
			_UpdateButtonState(GameButtons.Cancel, Game1.options.cancelButton, additional_keys);
			_UpdateButtonState(GameButtons.Run, Game1.options.runButton, additional_keys);
			_UpdateButtonState(GameButtons.Up, Game1.options.moveUpButton, additional_keys);
			_UpdateButtonState(GameButtons.Down, Game1.options.moveDownButton, additional_keys);
			_UpdateButtonState(GameButtons.Left, Game1.options.moveLeftButton, additional_keys);
			_UpdateButtonState(GameButtons.Right, Game1.options.moveRightButton, additional_keys);
		}

		public bool IsButtonPressed(GameButtons button)
		{
			return _buttonStates[button] == 1;
		}

		public bool IsButtonDown(GameButtons button)
		{
			return _buttonStates[button] > 0;
		}

		public bool IsButtonReleased(GameButtons button)
		{
			return _buttonStates[button] == -1;
		}

		public bool IsButtonDownRepeating(GameButtons button, int repeat_rate, int repeat_delay = 0)
		{
			int hold_time = _buttonStates[button];
			if (hold_time < 0)
			{
				hold_time = -1;
			}
			if (hold_time >= repeat_delay)
			{
				return (hold_time - repeat_delay) % repeat_rate == 0;
			}
			return false;
		}

		public int GetButtonHoldTime(GameButtons button)
		{
			return Math.Max(_buttonStates[button], 0);
		}

		protected void _UpdateButtonState(GameButtons button, InputButton[] keys, HashSet<InputButton> emulated_keys)
		{
			bool down = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), keys);
			for (int i = 0; i < keys.Length; i++)
			{
				if (emulated_keys.Contains(keys[i]))
				{
					down = true;
					break;
				}
			}
			if (_buttonStates[button] == -1)
			{
				_buttonStates[button] = 0;
			}
			if (down)
			{
				_buttonStates[button]++;
			}
			else if (_buttonStates[button] > 0)
			{
				_buttonStates[button] = -1;
			}
		}

		public T GetObjectAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
		{
			foreach (CraneGameObject game_object in _gameObjects)
			{
				if (game_object is T && game_object.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					return game_object as T;
				}
			}
			return null;
		}

		public List<T> GetObjectsAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGameObject game_object in _gameObjects)
			{
				if (game_object is T && game_object.GetBounds().Contains((int)point.X, (int)point.Y))
				{
					results.Add(game_object as T);
					if (max_count >= 0 && results.Count >= max_count)
					{
						return results;
					}
				}
			}
			return results;
		}

		public T GetObjectOfType<T>() where T : CraneGameObject
		{
			if (_gameObjectsByType.ContainsKey(typeof(T)))
			{
				List<CraneGameObject> game_objects = _gameObjectsByType[typeof(T)];
				if (game_objects.Count > 0)
				{
					return game_objects[0] as T;
				}
			}
			return null;
		}

		public List<T> GetObjectsOfType<T>() where T : CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGameObject game_object in _gameObjects)
			{
				if (game_object is T)
				{
					results.Add(game_object as T);
				}
			}
			return results;
		}

		public T GetOverlap<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject
		{
			foreach (CraneGameObject game_object in _gameObjects)
			{
				if (game_object is T && target.GetBounds().Intersects(game_object.GetBounds()) && target != game_object)
				{
					return game_object as T;
				}
			}
			return null;
		}

		public List<T> GetOverlaps<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject
		{
			List<T> results = new List<T>();
			foreach (CraneGameObject game_object in _gameObjects)
			{
				if (game_object is T && target.GetBounds().Intersects(game_object.GetBounds()) && target != game_object)
				{
					results.Add(game_object as T);
					if (max_count >= 0 && results.Count >= max_count)
					{
						return results;
					}
				}
			}
			return results;
		}

		public bool tick(GameTime time)
		{
			if (_shouldQuit)
			{
				if (music != null && music.IsPlaying)
				{
					music.Stop(AudioStopOptions.Immediate);
				}
				if (fastMusic != null && fastMusic.IsPlaying)
				{
					fastMusic.Stop(AudioStopOptions.Immediate);
				}
				if (craneSound != null && !craneSound.IsStopped)
				{
					craneSound.Stop(AudioStopOptions.Immediate);
				}
				return true;
			}
			if (freezeFrames > 0)
			{
				freezeFrames--;
			}
			else
			{
				_UpdateInput();
				for (int i = 0; i < _gameObjects.Count; i++)
				{
					if (_gameObjects[i] != null)
					{
						_gameObjects[i].Update(time);
					}
				}
			}
			if (IsButtonPressed(GameButtons.Confirm))
			{
				Quit();
				Game1.playSound("bigDeSelect");
				GameLogic logic = GetObjectOfType<GameLogic>();
				if (logic != null && logic.collectedItems.Count > 0)
				{
					List<Item> items = new List<Item>();
					foreach (Item item in logic.collectedItems)
					{
						items.Add(item.getOne());
					}
					Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
				}
			}
			return false;
		}

		public bool forceQuit()
		{
			Quit();
			GameLogic logic = GetObjectOfType<GameLogic>();
			if (logic != null)
			{
				new List<Item>();
				foreach (Item collectedItem in logic.collectedItems)
				{
					Utility.CollectOrDrop(collectedItem.getOne());
				}
			}
			return true;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void RegisterGameObject(CraneGameObject game_object)
		{
			if (!_gameObjectTypes.Contains(game_object.GetType()))
			{
				_gameObjectTypes.Add(game_object.GetType());
				_gameObjectsByType[game_object.GetType()] = new List<CraneGameObject>();
			}
			_gameObjectsByType[game_object.GetType()].Add(game_object);
			_gameObjects.Add(game_object);
		}

		public void UnregisterGameObject(CraneGameObject game_object)
		{
			_gameObjectsByType[game_object.GetType()].Remove(game_object);
			_gameObjects.Remove(game_object);
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, _effect);
			b.Draw(spriteSheet, upperLeft, new Rectangle(0, 0, gameWidth, gameHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Dictionary<CraneGameObject, float> depth_lookup = new Dictionary<CraneGameObject, float>();
			float lowest_depth = 0f;
			float highest_depth = 0f;
			for (int k = 0; k < _gameObjects.Count; k++)
			{
				if (_gameObjects[k] != null)
				{
					float depth = _gameObjects[k].GetRendererLayerDepth();
					depth_lookup[_gameObjects[k]] = depth;
					if (depth < lowest_depth)
					{
						lowest_depth = depth;
					}
					if (depth > highest_depth)
					{
						highest_depth = depth;
					}
				}
			}
			for (int j = 0; j < _gameObjectTypes.Count; j++)
			{
				Type type = _gameObjectTypes[j];
				for (int i = 0; i < _gameObjectsByType[type].Count; i++)
				{
					float drawn_depth = Utility.Lerp(0.1f, 0.9f, (depth_lookup[_gameObjectsByType[type][i]] - lowest_depth) / (highest_depth - lowest_depth));
					_gameObjectsByType[type][i].Draw(b, drawn_depth);
				}
			}
			b.End();
		}

		public void changeScreenSize()
		{
			float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
			Viewport vp = Game1.graphics.GraphicsDevice.Viewport;
			Rectangle cb = Game1.game1.Window.ClientBounds;
			Vector2 bb = new Vector2(Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight);
			float x = bb.X;
			Vector2 tmp = new Vector2(y: bb.Y / 2f, x: x / 2f) * pixel_zoom_adjustment;
			tmp.X -= gameWidth / 2 * 4;
			tmp.Y -= gameHeight / 2 * 4;
			if (upperLeft != tmp)
			{
				upperLeft = tmp;
				Console.WriteLine("CraneGame.changeScreenSize(); vp={0}, cb={1}, bb={2}, zl={3}, upperLeft={4}", vp, cb, bb, Game1.options.zoomLevel, upperLeft);
			}
		}

		public void unload()
		{
			Game1.stopMusicTrack(Game1.MusicContext.MiniGame);
			_content.Unload();
		}

		public void receiveEventPoke(int data)
		{
		}

		public string minigameId()
		{
			return "CraneGame";
		}
	}
}
