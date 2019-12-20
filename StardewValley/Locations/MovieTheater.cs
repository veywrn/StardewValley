using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Events;
using StardewValley.GameData.Movies;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;

namespace StardewValley.Locations
{
	public class MovieTheater : GameLocation
	{
		public enum MovieStates
		{
			Preshow,
			Show,
			PostShow
		}

		protected bool _startedMovie;

		protected bool _isJojaTheater;

		protected static Dictionary<string, MovieData> _movieData;

		protected static List<MovieCharacterReaction> _genericReactions;

		protected static List<ConcessionTaste> _concessionTastes;

		protected readonly NetStringDictionary<int, NetInt> _spawnedMoviePatrons = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<int, NetInt> _purchasedConcessions = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<int, NetInt> _playerInvitedPatrons = new NetStringDictionary<int, NetInt>();

		protected readonly NetStringDictionary<bool, NetBool> _characterGroupLookup = new NetStringDictionary<bool, NetBool>();

		protected Dictionary<int, List<Point>> _hangoutPoints;

		protected Dictionary<int, List<Point>> _availableHangoutPoints;

		protected int _maxHangoutGroups;

		protected int _movieStartTime = -1;

		[XmlElement("dayFirstEntered")]
		public readonly NetInt dayFirstEntered = new NetInt(-1);

		protected static Dictionary<int, MovieConcession> _concessions;

		public const int LOVE_MOVIE_FRIENDSHIP = 200;

		public const int LIKE_MOVIE_FRIENDSHIP = 100;

		public const int DISLIKE_MOVIE_FRIENDSHIP = 0;

		public const int BIRD_STARTLE_DISTANCE = 200;

		public const int LOVE_CONCESSION_FRIENDSHIP = 50;

		public const int LIKE_CONCESSION_FRIENDSHIP = 25;

		public const int DISLIKE_CONCESSION_FRIENDSHIP = 0;

		public const int OPEN_TIME = 900;

		public const int CLOSE_TIME = 2100;

		public int nextRepathTime;

		public int repathTimeInterval = 1000;

		[XmlIgnore]
		protected Dictionary<string, KeyValuePair<Point, int>> _destinationPositions = new Dictionary<string, KeyValuePair<Point, int>>();

		[XmlIgnore]
		public List<Bird> _birds = new List<Bird>();

		[XmlIgnore]
		public Point[] birdLocations = new Point[14]
		{
			new Point(19, 5),
			new Point(21, 4),
			new Point(16, 3),
			new Point(10, 13),
			new Point(2, 13),
			new Point(2, 6),
			new Point(9, 2),
			new Point(18, 12),
			new Point(21, 11),
			new Point(3, 11),
			new Point(4, 2),
			new Point(12, 12),
			new Point(11, 5),
			new Point(13, 13)
		};

		public Point[] birdRoostLocations = new Point[6]
		{
			new Point(19, 5),
			new Point(21, 4),
			new Point(16, 3),
			new Point(9, 2),
			new Point(21, 11),
			new Point(4, 2)
		};

		[XmlIgnore]
		public Dictionary<Point, Bird> _birdPointOccupancy;

		protected int _exitX;

		protected int _exitY;

		private NetEvent1<MovieViewerLockEvent> movieViewerLockEvent = new NetEvent1<MovieViewerLockEvent>();

		private NetEvent1<StartMovieEvent> startMovieEvent = new NetEvent1<StartMovieEvent>();

		private NetEvent1Field<long, NetLong> requestStartMovieEvent = new NetEvent1Field<long, NetLong>();

		private NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

		protected List<Farmer> _viewingFarmers = new List<Farmer>();

		protected List<List<Character>> _viewingGroups = new List<List<Character>>();

		protected List<List<Character>> _playerGroups = new List<List<Character>>();

		protected List<List<Character>> _npcGroups = new List<List<Character>>();

		protected static bool _hasRequestedMovieStart = false;

		protected static int _playerHangoutGroup = -1;

		protected int _farmerCount;

		protected NetInt _currentState = new NetInt();

		public static string[][][][] possibleNPCGroups = new string[7][][][]
		{
			new string[3][][]
			{
				new string[1][]
				{
					new string[1]
					{
						"Lewis"
					}
				},
				new string[3][]
				{
					new string[3]
					{
						"Jas",
						"Vincent",
						"Marnie"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[2]
					{
						"Penny",
						"Maru"
					}
				},
				new string[1][]
				{
					new string[2]
					{
						"Lewis",
						"Marnie"
					}
				}
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[1]
					{
						"Clint"
					},
					new string[2]
					{
						"Demetrius",
						"Robin"
					},
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Caroline",
						"Jodi"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[2][]
				{
					new string[1]
					{
						"Lewis"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				}
			},
			new string[3][][]
			{
				new string[2][]
				{
					new string[2]
					{
						"Evelyn",
						"George"
					},
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Sandy",
						"Emily"
					},
					new string[1]
					{
						"Elliot"
					}
				}
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[3]
					{
						"Alex",
						"Haley",
						"Emily"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Pierre",
						"Caroline"
					},
					new string[3]
					{
						"Shane",
						"Jas",
						"Marnie"
					}
				}
			},
			new string[3][][]
			{
				null,
				new string[3][]
				{
					new string[2]
					{
						"Haley",
						"Emily"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					},
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				}
			},
			new string[3][][]
			{
				new string[1][]
				{
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[3]
					{
						"Abigail",
						"Sebastian",
						"Sam"
					}
				},
				new string[2][]
				{
					new string[3]
					{
						"Harvey",
						"Maru",
						"Penny"
					},
					new string[1]
					{
						"Leah"
					}
				}
			},
			new string[3][][]
			{
				new string[3][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[3]
					{
						"George",
						"Evelyn",
						"Alex"
					},
					new string[1]
					{
						"Lewis"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Gus",
						"Willy"
					},
					new string[2]
					{
						"Maru",
						"Sebastian"
					}
				},
				new string[2][]
				{
					new string[2]
					{
						"Penny",
						"Pam"
					},
					new string[2]
					{
						"Sandy",
						"Emily"
					}
				}
			}
		};

		public MovieTheater()
		{
		}

		public static void AddMoviePoster(GameLocation location, float x, float y, int month_offset = 0)
		{
			WorldDate worldDate = new WorldDate(Game1.Date);
			worldDate.TotalDays += 28 * month_offset;
			MovieData data = GetMovieForDate(worldDate);
			if (data != null)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, data.SheetIndex * 128, 13, 19),
					sourceRectStartingPos = new Vector2(0f, data.SheetIndex * 128),
					animationLength = 1,
					totalNumberOfLoops = 9999,
					interval = 9999f,
					scale = 4f,
					position = new Vector2(x, y),
					layerDepth = 0.01f
				});
			}
		}

		public MovieTheater(string map, string name)
			: base(map, name)
		{
			_currentState.Set(0);
			GetMovieData();
			_InitializeMap();
			GetMovieReactions();
		}

		public static List<MovieCharacterReaction> GetMovieReactions()
		{
			if (_genericReactions == null)
			{
				_genericReactions = Game1.content.Load<List<MovieCharacterReaction>>("Data\\MoviesReactions");
			}
			return _genericReactions;
		}

		public static string GetConcessionTasteForCharacter(Character character, MovieConcession concession)
		{
			if (_concessionTastes == null)
			{
				_concessionTastes = Game1.content.Load<List<ConcessionTaste>>("Data\\ConcessionTastes");
			}
			ConcessionTaste universal_taste = null;
			foreach (ConcessionTaste taste2 in _concessionTastes)
			{
				if (taste2.Name == "*")
				{
					universal_taste = taste2;
					break;
				}
			}
			foreach (ConcessionTaste taste in _concessionTastes)
			{
				if (taste.Name == character.Name)
				{
					if (taste.LovedTags.Contains(concession.Name))
					{
						return "love";
					}
					if (taste.LikedTags.Contains(concession.Name))
					{
						return "like";
					}
					if (!taste.DislikedTags.Contains(concession.Name))
					{
						if (universal_taste != null)
						{
							if (universal_taste.LovedTags.Contains(concession.Name))
							{
								return "love";
							}
							if (universal_taste.LikedTags.Contains(concession.Name))
							{
								return "like";
							}
							if (universal_taste.DislikedTags.Contains(concession.Name))
							{
								return "dislike";
							}
						}
						if (concession.tags != null)
						{
							foreach (string tag in concession.tags)
							{
								if (taste.LovedTags.Contains(tag))
								{
									return "love";
								}
								if (taste.LikedTags.Contains(tag))
								{
									return "like";
								}
								if (taste.DislikedTags.Contains(tag))
								{
									return "dislike";
								}
								if (universal_taste != null)
								{
									if (universal_taste.LovedTags.Contains(tag))
									{
										return "love";
									}
									if (universal_taste.LikedTags.Contains(tag))
									{
										return "like";
									}
									if (universal_taste.DislikedTags.Contains(tag))
									{
										return "dislike";
									}
								}
							}
						}
						break;
					}
					return "dislike";
				}
			}
			return "like";
		}

		public static IEnumerable<string> GetPatronNames()
		{
			MovieTheater theater = Game1.getLocationFromName("MovieTheater") as MovieTheater;
			if (theater == null)
			{
				return null;
			}
			if (theater._spawnedMoviePatrons == null)
			{
				return null;
			}
			return theater._spawnedMoviePatrons.Keys;
		}

		protected void _InitializeMap()
		{
			_hangoutPoints = new Dictionary<int, List<Point>>();
			_maxHangoutGroups = 0;
			if (map.GetLayer("Paths") != null)
			{
				Layer paths_layer = map.GetLayer("Paths");
				for (int x = 0; x < paths_layer.LayerWidth; x++)
				{
					for (int y = 0; y < paths_layer.LayerHeight; y++)
					{
						if (paths_layer.Tiles[x, y] == null || paths_layer.Tiles[x, y].TileIndex != 7)
						{
							continue;
						}
						int hangout_group = -1;
						if (map.GetLayer("Paths").Tiles[x, y].Properties.ContainsKey("group") && int.TryParse(map.GetLayer("Paths").Tiles[x, y].Properties["group"], out hangout_group))
						{
							if (!_hangoutPoints.ContainsKey(hangout_group))
							{
								_hangoutPoints[hangout_group] = new List<Point>();
							}
							_hangoutPoints[hangout_group].Add(new Point(x, y));
							_maxHangoutGroups = Math.Max(_maxHangoutGroups, hangout_group);
						}
					}
				}
			}
			ResetTheater();
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(_spawnedMoviePatrons, _purchasedConcessions, _currentState, movieViewerLockEvent, requestStartMovieEvent, startMovieEvent, endMovieEvent, _playerInvitedPatrons, _characterGroupLookup, dayFirstEntered);
			movieViewerLockEvent.onEvent += OnMovieViewerLockEvent;
			requestStartMovieEvent.onEvent += OnRequestStartMovieEvent;
			startMovieEvent.onEvent += OnStartMovieEvent;
		}

		public void OnStartMovieEvent(StartMovieEvent e)
		{
			if (e.uid == Game1.player.UniqueMultiplayerID)
			{
				if (Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(Game1.player);
				}
				MovieTheaterScreeningEvent event_generator = new MovieTheaterScreeningEvent();
				Event viewing_event = event_generator.getMovieEvent(GetMovieForDate(Game1.Date).ID, e.playerGroups, e.npcGroups, GetConcessionsDictionary());
				Rumble.rumble(0.15f, 200f);
				Game1.player.completelyStopAnimatingOrDoingAction();
				playSoundAt("doorClose", Game1.player.getTileLocation());
				Game1.globalFadeToBlack(delegate
				{
					Game1.changeMusicTrack("none");
					startEvent(viewing_event);
				});
			}
		}

		public void OnRequestStartMovieEvent(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (_currentState.Value == 0)
			{
				if (Game1.player.team.movieMutex.IsLocked())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
				Game1.player.team.movieMutex.RequestLock();
				_playerGroups = new List<List<Character>>();
				_npcGroups = new List<List<Character>>();
				List<Character> patrons = new List<Character>();
				foreach (string patronName in GetPatronNames())
				{
					Character character3 = Game1.getCharacterFromName(patronName);
					patrons.Add(character3);
				}
				foreach (Farmer farmer in _viewingFarmers)
				{
					List<Character> farmer_group = new List<Character>();
					farmer_group.Add(farmer);
					for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
					{
						MovieInvitation invite = Game1.player.team.movieInvitations[i];
						if (invite.farmer == farmer && GetFirstInvitedPlayer(invite.invitedNPC) == farmer && patrons.Contains(invite.invitedNPC))
						{
							patrons.Remove(invite.invitedNPC);
							farmer_group.Add(invite.invitedNPC);
						}
					}
					_playerGroups.Add(farmer_group);
				}
				foreach (List<Character> playerGroup in _playerGroups)
				{
					foreach (Character character2 in playerGroup)
					{
						if (character2 is NPC)
						{
							(character2 as NPC).lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
						}
					}
				}
				_npcGroups.Add(new List<Character>(patrons));
				_PopulateNPCOnlyGroups(_playerGroups, _npcGroups);
				_viewingGroups = new List<List<Character>>();
				List<Character> player_invited_npcs = new List<Character>();
				foreach (List<Character> playerGroup2 in _playerGroups)
				{
					foreach (Character character in playerGroup2)
					{
						player_invited_npcs.Add(character);
					}
				}
				_viewingGroups.Add(player_invited_npcs);
				foreach (List<Character> characters in _npcGroups)
				{
					_viewingGroups.Add(new List<Character>(characters));
				}
				_currentState.Set(1);
			}
			startMovieEvent.Fire(new StartMovieEvent(uid, _playerGroups, _npcGroups));
		}

		public void OnMovieViewerLockEvent(MovieViewerLockEvent e)
		{
			_viewingFarmers = new List<Farmer>();
			_movieStartTime = e.movieStartTime;
			foreach (long uid in e.uids)
			{
				Farmer farmer = Game1.getFarmer(uid);
				if (farmer != null)
				{
					_viewingFarmers.Add(farmer);
				}
			}
			if (_viewingFarmers.Count > 0 && Game1.IsMultiplayer)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\UI:MovieStartRequest"));
			}
			if (Game1.player.team.movieMutex.IsLockHeld())
			{
				_ShowMovieStartReady();
			}
		}

		public void _ShowMovieStartReady()
		{
			if (!Game1.IsMultiplayer)
			{
				requestStartMovieEvent.Fire(Game1.player.UniqueMultiplayerID);
				return;
			}
			Game1.player.team.SetLocalRequiredFarmers("start_movie", _viewingFarmers);
			Game1.player.team.SetLocalReady("start_movie", ready: true);
			Game1.dialogueUp = false;
			_hasRequestedMovieStart = true;
			Game1.activeClickableMenu = new ReadyCheckDialog("start_movie", allowCancel: true, delegate(Farmer farmer)
			{
				if (_hasRequestedMovieStart)
				{
					_hasRequestedMovieStart = false;
					requestStartMovieEvent.Fire(farmer.UniqueMultiplayerID);
				}
			}, delegate(Farmer farmer)
			{
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(farmer);
				}
				if (Game1.player.team.movieMutex.IsLockHeld())
				{
					Game1.player.team.movieMutex.ReleaseLock();
				}
			});
		}

		public static Dictionary<string, MovieData> GetMovieData()
		{
			if (_movieData == null)
			{
				_movieData = Game1.content.Load<Dictionary<string, MovieData>>("Data\\Movies");
				foreach (KeyValuePair<string, MovieData> kvp in _movieData)
				{
					kvp.Value.ID = kvp.Key;
				}
			}
			return _movieData;
		}

		public NPC GetMoviePatron(string name)
		{
			for (int i = 0; i < characters.Count; i++)
			{
				if ((string)characters[i].name == name)
				{
					return characters[i];
				}
			}
			return null;
		}

		protected NPC AddMoviePatronNPC(string name, int x, int y, int facingDirection)
		{
			if (_spawnedMoviePatrons.ContainsKey(name))
			{
				return GetMoviePatron(name);
			}
			string spriteName = name.Equals("Krobus") ? "Krobus_Trenchcoat" : name;
			string portraitPath = "Portraits\\" + name;
			int height = (name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128;
			NPC i = new NPC(new AnimatedSprite("Characters\\" + spriteName, 0, 16, height / 4), new Vector2(x * 64, y * 64), base.Name, facingDirection, name, null, null, eventActor: true, portraitPath);
			i.eventActor = true;
			i.collidesWithOtherCharacters.Set(newValue: false);
			addCharacter(i);
			_spawnedMoviePatrons.Add(name, 1);
			GetDialogueForCharacter(i);
			return i;
		}

		public void RemoveAllPatrons()
		{
			if (_spawnedMoviePatrons == null)
			{
				return;
			}
			for (int i = 0; i < characters.Count; i++)
			{
				if (_spawnedMoviePatrons.ContainsKey(characters[i].Name))
				{
					characters.RemoveAt(i);
					i--;
				}
			}
			_spawnedMoviePatrons.Clear();
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if (_currentState.Value == 0)
			{
				MovieData movie = GetMovieForDate(Game1.Date);
				Game1.multiplayer.globalChatInfoMessage("MovieStart", movie.Title);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (!_isJojaTheater && Game1.MasterPlayer.mailReceived.Contains("ccMovieTheaterJoja"))
			{
				_isJojaTheater = true;
			}
			_birds.Clear();
			if (dayFirstEntered.Value == -1)
			{
				dayFirstEntered.Value = Game1.Date.TotalDays;
			}
			if (!_isJojaTheater)
			{
				_birdPointOccupancy = new Dictionary<Point, Bird>();
				Point[] array = birdLocations;
				foreach (Point point in array)
				{
					_birdPointOccupancy[point] = null;
				}
				array = birdRoostLocations;
				foreach (Point point2 in array)
				{
					_birdPointOccupancy[point2] = null;
				}
				for (int i = 0; i < Game1.random.Next(2, 5); i++)
				{
					int bird_type = Game1.random.Next(0, 4);
					if (Game1.currentSeason == "fall")
					{
						bird_type = 10;
					}
					Bird bird = new Bird(GetFreeBirdPoint(), this, bird_type);
					_birds.Add(bird);
					ReserveBirdPoint(bird, bird.endPosition);
				}
				if (Game1.timeOfDay > 2100 && Game1.random.NextDouble() < 0.5)
				{
					Bird bird2 = new Bird(GetFreeBirdPoint(), this, 11);
					_birds.Add(bird2);
					ReserveBirdPoint(bird2, bird2.endPosition);
				}
			}
			AddMoviePoster(this, 1104f, 292f);
			loadMap(mapPath, force_reload: true);
			if (_isJojaTheater)
			{
				string addOn = (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? "" : "_international";
				base.Map.TileSheets[0].ImageSource = "Maps\\MovieTheaterJoja_TileSheet" + addOn;
				base.Map.LoadTileSheets(Game1.mapDisplayDevice);
			}
			if (_currentState.Value == 0)
			{
				addRandomNPCs();
			}
			else if (_currentState.Value == 2)
			{
				Game1.changeMusicTrack("movieTheaterAfter");
				Game1.ambientLight = new Color(150, 170, 80);
				addSpecificRandomNPC(0);
			}
		}

		public void ReserveBirdPoint(Bird bird, Point point)
		{
			if (_birdPointOccupancy.ContainsKey(bird.endPosition))
			{
				_birdPointOccupancy[bird.endPosition] = null;
			}
			if (_birdPointOccupancy.ContainsKey(point))
			{
				_birdPointOccupancy[point] = bird;
			}
		}

		public Point[] GetCurrentBirdLocationList()
		{
			if (ShouldBirdsRoost())
			{
				return birdRoostLocations;
			}
			return birdLocations;
		}

		public bool ShouldBirdsRoost()
		{
			return _currentState.Value == 2;
		}

		public Point GetFreeBirdPoint(Bird bird = null, int clearance = 200)
		{
			List<Point> points = new List<Point>();
			Point[] currentBirdLocationList = GetCurrentBirdLocationList();
			for (int i = 0; i < currentBirdLocationList.Length; i++)
			{
				Point point = currentBirdLocationList[i];
				if (_birdPointOccupancy[point] == null)
				{
					bool fail = false;
					if (bird != null)
					{
						foreach (Farmer farmer in farmers)
						{
							if (Utility.distance(farmer.position.X, (float)(point.X * 64) + 32f, farmer.position.Y, (float)(point.Y * 64) + 32f) < 200f)
							{
								fail = true;
							}
						}
					}
					if (!fail)
					{
						points.Add(point);
					}
				}
			}
			return Utility.GetRandom(points);
		}

		private void addRandomNPCs()
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			critters = new List<Critter>();
			if (dayFirstEntered.Value == Game1.Date.TotalDays || r.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(0);
			}
			if (!_isJojaTheater && r.NextDouble() < 0.28)
			{
				addSpecificRandomNPC(4);
				addSpecificRandomNPC(11);
			}
			else if (_isJojaTheater && r.NextDouble() < 0.33)
			{
				addSpecificRandomNPC(13);
			}
			if (r.NextDouble() < 0.1)
			{
				addSpecificRandomNPC(9);
				addSpecificRandomNPC(7);
			}
			if (Game1.currentSeason.Equals("fall") && r.NextDouble() < 0.5)
			{
				addSpecificRandomNPC(1);
			}
			if (Game1.currentSeason.Equals("spring") && r.NextDouble() < 0.5)
			{
				addSpecificRandomNPC(3);
			}
			if (r.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(2);
			}
			if (r.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(6);
			}
			if (r.NextDouble() < 0.25)
			{
				addSpecificRandomNPC(8);
			}
			if (r.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(10);
			}
			if (r.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(12);
			}
			if (r.NextDouble() < 0.2)
			{
				addSpecificRandomNPC(5);
			}
			if (!_isJojaTheater)
			{
				if (r.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(13f, 7f)).setStayInbounds(stayInbounds: true));
				}
				if (r.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(4f, 8f)).setStayInbounds(stayInbounds: true));
				}
				if (r.NextDouble() < 0.75)
				{
					addCritter(new Butterfly(new Vector2(17f, 10f)).setStayInbounds(stayInbounds: true));
				}
			}
		}

		private void addSpecificRandomNPC(int whichRandomNPC)
		{
			Random r = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays + whichRandomNPC);
			switch (whichRandomNPC)
			{
			case 0:
				setMapTile(2, 9, 215, "Buildings", "MessageSpeech MovieTheater_CraneMan" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(2, 8, 199, "Front", null);
				break;
			case 1:
				setMapTile(19, 7, 216, "Buildings", "MessageSpeech MovieTheater_Welwick" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(19, 6, 200, "Front", null);
				break;
			case 2:
				setAnimatedMapTile(21, 7, new int[4]
				{
					217,
					217,
					217,
					218
				}, 700L, "Buildings", "MessageSpeech MovieTheater_ShortsMan" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(21, 6, new int[4]
				{
					201,
					201,
					201,
					202
				}, 700L, "Front", null);
				break;
			case 3:
				setMapTile(5, 9, 219, "Buildings", "MessageSpeech MovieTheater_Mother" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(6, 9, 220, "Buildings", "MessageSpeech MovieTheater_Child" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(5, 8, new int[6]
				{
					203,
					203,
					203,
					204,
					204,
					204
				}, 1000L, "Front", null);
				break;
			case 4:
				setMapTileIndex(20, 9, 222, "Front");
				setMapTileIndex(21, 9, 223, "Front");
				setMapTile(20, 10, 238, "Buildings", null);
				setMapTile(21, 10, 239, "Buildings", null);
				setMapTileIndex(20, 11, 254, "Buildings");
				setMapTileIndex(21, 11, 255, "Buildings");
				break;
			case 5:
				setAnimatedMapTile(10, 7, new int[4]
				{
					251,
					251,
					251,
					252
				}, 900L, "Buildings", "MessageSpeech MovieTheater_Lupini" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(10, 6, new int[4]
				{
					235,
					235,
					235,
					236
				}, 900L, "Front", null);
				break;
			case 6:
				setAnimatedMapTile(5, 7, new int[4]
				{
					249,
					249,
					249,
					250
				}, 600L, "Buildings", "MessageSpeech MovieTheater_ConcessionMan" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setAnimatedMapTile(5, 6, new int[4]
				{
					233,
					233,
					233,
					234
				}, 600L, "Front", null);
				break;
			case 7:
				setMapTile(1, 12, 248, "Buildings", "MessageSpeech MovieTheater_PurpleHairLady");
				setMapTile(1, 11, 232, "Front", null);
				break;
			case 8:
				setMapTile(3, 8, 247, "Buildings", "MessageSpeech MovieTheater_RedCapGuy" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(3, 7, 231, "Front", null);
				break;
			case 9:
				setMapTile(2, 11, 253, "Buildings", "MessageSpeech MovieTheater_Governor" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(2, 10, 237, "Front", null);
				break;
			case 10:
				setMapTile(9, 7, 221, "Buildings", "NPCSpeechMessageNoRadius Gunther MovieTheater_Gunther" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(9, 6, 205, "Front", null);
				break;
			case 11:
				setMapTile(19, 10, 208, "Buildings", "NPCSpeechMessageNoRadius Marlon MovieTheater_Marlon" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(19, 9, 192, "Front", null);
				break;
			case 12:
				setMapTile(12, 4, 209, "Buildings", "MessageSpeech MovieTheater_Marcello" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(12, 3, 193, "Front", null);
				break;
			case 13:
				setMapTile(17, 12, 241, "Buildings", "NPCSpeechMessageNoRadius Morris MovieTheater_Morris" + ((r.NextDouble() < 0.5) ? "2" : ""));
				setMapTile(17, 11, 225, "Front", null);
				break;
			}
		}

		public static MovieData GetMovieForDate(WorldDate date)
		{
			GetMovieData();
			string prefix = (date.Season + "_movie_").ToString();
			long day_theater_was_built = Game1.player.team.theaterBuildDate;
			long num = date.TotalDays;
			long current_year = num / 112 - day_theater_was_built / 112;
			if (num / 28 % 4 < day_theater_was_built / 28 % 4)
			{
				current_year--;
			}
			int last_found_year = 0;
			if (_movieData.ContainsKey(prefix + current_year))
			{
				return _movieData[prefix + current_year];
			}
			foreach (MovieData movie_data2 in _movieData.Values)
			{
				if (movie_data2 != null && movie_data2.ID.StartsWith(prefix))
				{
					string[] movie_id_parts = movie_data2.ID.Split('_');
					if (movie_id_parts.Length >= 3)
					{
						int movie_year = 0;
						if (int.TryParse(movie_id_parts[2], out movie_year) && movie_year > last_found_year)
						{
							last_found_year = movie_year;
						}
					}
				}
			}
			foreach (MovieData movie_data in _movieData.Values)
			{
				if (movie_data.ID == prefix + current_year % (last_found_year + 1))
				{
					return movie_data;
				}
			}
			return _movieData.Values.FirstOrDefault();
		}

		public override void DayUpdate(int dayOfMonth)
		{
			ResetTheater();
			_ResetHangoutPoints();
			base.DayUpdate(dayOfMonth);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (_farmerCount != farmers.Count)
			{
				_farmerCount = farmers.Count;
				if (Game1.activeClickableMenu is ReadyCheckDialog)
				{
					(Game1.activeClickableMenu as ReadyCheckDialog).closeDialog(Game1.player);
					if (Game1.player.team.movieMutex.IsLockHeld())
					{
						Game1.player.team.movieMutex.ReleaseLock();
					}
				}
			}
			for (int i = 0; i < _birds.Count; i++)
			{
				_birds[i].Update(time);
			}
			base.UpdateWhenCurrentLocation(time);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			for (int i = 0; i < _birds.Count; i++)
			{
				_birds[i].Draw(b);
			}
			base.drawAboveAlwaysFrontLayer(b);
		}

		public static bool Invite(Farmer farmer, NPC invited_npc)
		{
			if (farmer == null || invited_npc == null)
			{
				return false;
			}
			MovieInvitation invitation = new MovieInvitation();
			invitation.farmer = farmer;
			invitation.invitedNPC = invited_npc;
			farmer.team.movieInvitations.Add(invitation);
			return true;
		}

		public void ResetTheater()
		{
			_playerHangoutGroup = -1;
			RemoveAllPatrons();
			_playerGroups.Clear();
			_npcGroups.Clear();
			_viewingGroups.Clear();
			_viewingFarmers.Clear();
			_purchasedConcessions.Clear();
			_playerInvitedPatrons.Clear();
			_characterGroupLookup.Clear();
			_ResetHangoutPoints();
			Game1.player.team.movieMutex.ReleaseLock();
			_currentState.Set(0);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			movieViewerLockEvent.Poll();
			requestStartMovieEvent.Poll();
			startMovieEvent.Poll();
			endMovieEvent.Poll();
			if (!Game1.IsMasterGame)
			{
				return;
			}
			for (int j = 0; j < _viewingFarmers.Count; j++)
			{
				Farmer viewing_farmer = _viewingFarmers[j];
				if (!Game1.getOnlineFarmers().Contains(viewing_farmer))
				{
					_viewingFarmers.RemoveAt(j);
					j--;
				}
				else if (_currentState.Value == 2 && !farmers.Contains(viewing_farmer) && viewing_farmer.currentLocation != null && viewing_farmer.currentLocation.Name != "Temp")
				{
					_viewingFarmers.RemoveAt(j);
					j--;
				}
			}
			if (_currentState.Value != 0 && _viewingFarmers.Count == 0)
			{
				MovieData movie = GetMovieForDate(Game1.Date);
				Game1.multiplayer.globalChatInfoMessage("MovieEnd", movie.Title);
				ResetTheater();
			}
			if (Game1.player.team.movieInvitations != null && _playerInvitedPatrons.Count() < 4)
			{
				foreach (Farmer farmer in farmers)
				{
					for (int i = 0; i < Game1.player.team.movieInvitations.Count; i++)
					{
						MovieInvitation invite = Game1.player.team.movieInvitations[i];
						if (!invite.fulfilled && !_spawnedMoviePatrons.ContainsKey(invite.invitedNPC.displayName))
						{
							if (_playerHangoutGroup < 0)
							{
								_playerHangoutGroup = Game1.random.Next(_maxHangoutGroups);
							}
							int group = _playerHangoutGroup;
							if (invite.farmer == farmer && GetFirstInvitedPlayer(invite.invitedNPC) == farmer)
							{
								Point point = Utility.GetRandom(_availableHangoutPoints[group]);
								NPC character = AddMoviePatronNPC(invite.invitedNPC.name, 14, 15, 0);
								_playerInvitedPatrons.Add(character.name, 1);
								_availableHangoutPoints[group].Remove(point);
								int direction = 2;
								if (map.GetLayer("Paths").Tiles[point.X, point.Y].Properties != null && map.GetLayer("Paths").Tiles[point.X, point.Y].Properties.ContainsKey("direction"))
								{
									int.TryParse(map.GetLayer("Paths").Tiles[point.X, point.Y].Properties["direction"], out direction);
								}
								_destinationPositions[character.Name] = new KeyValuePair<Point, int>(point, direction);
								PathCharacterToLocation(character, point, direction);
								invite.fulfilled = true;
							}
						}
					}
				}
			}
		}

		public static MovieCharacterReaction GetReactionsForCharacter(NPC character)
		{
			if (character == null)
			{
				return null;
			}
			foreach (MovieCharacterReaction reactions in GetMovieReactions())
			{
				if (!(reactions.NPCName != character.Name))
				{
					return reactions;
				}
			}
			return null;
		}

		public override void checkForMusic(GameTime time)
		{
		}

		public static string GetResponseForMovie(NPC character)
		{
			string response = "like";
			MovieData movie = GetMovieForDate(Game1.Date);
			if (movie == null)
			{
				return null;
			}
			if (movie != null)
			{
				foreach (MovieCharacterReaction reactions in GetMovieReactions())
				{
					if (!(reactions.NPCName != character.Name))
					{
						foreach (MovieReaction tagged_reactions in reactions.Reactions)
						{
							if (tagged_reactions.ShouldApplyToMovie(movie, GetPatronNames()) && tagged_reactions.Response != null && tagged_reactions.Response.Length > 0)
							{
								response = tagged_reactions.Response;
								break;
							}
						}
					}
				}
				return response;
			}
			return response;
		}

		public Dialogue GetDialogueForCharacter(NPC character)
		{
			MovieData movie = GetMovieForDate(Game1.Date);
			if (movie != null)
			{
				foreach (MovieCharacterReaction reactions in _genericReactions)
				{
					if (!(reactions.NPCName != character.Name))
					{
						foreach (MovieReaction tagged_reactions in reactions.Reactions)
						{
							if (tagged_reactions.ShouldApplyToMovie(movie, GetPatronNames(), GetResponseForMovie(character)) && tagged_reactions.Response != null && tagged_reactions.Response.Length > 0 && tagged_reactions.SpecialResponses != null)
							{
								if (_currentState.Value == 0 && tagged_reactions.SpecialResponses.BeforeMovie != null)
								{
									return new Dialogue(FormatString(tagged_reactions.SpecialResponses.BeforeMovie.Text), character);
								}
								if (_currentState.Value == 1 && tagged_reactions.SpecialResponses.DuringMovie != null)
								{
									return new Dialogue(FormatString(tagged_reactions.SpecialResponses.DuringMovie.Text), character);
								}
								if (_currentState.Value != 2 || tagged_reactions.SpecialResponses.AfterMovie == null)
								{
									break;
								}
								return new Dialogue(FormatString(tagged_reactions.SpecialResponses.AfterMovie.Text), character);
							}
						}
						break;
					}
				}
			}
			return null;
		}

		public string FormatString(string text, params string[] args)
		{
			return string.Format(text, GetMovieForDate(Game1.Date).Title, Game1.player.displayName, args);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			PropertyValue action = null;
			map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size)?.Properties.TryGetValue("Action", out action);
			if (action != null)
			{
				return performAction(action, who, tileLocation);
			}
			foreach (NPC npc in characters)
			{
				if (npc != null && !npc.IsMonster && (!who.isRidingHorse() || !(npc is Horse)) && npc.GetBoundingBox().Intersects(tileRect))
				{
					if (!npc.isMoving())
					{
						if (_playerInvitedPatrons.ContainsKey(npc.Name))
						{
							npc.faceTowardFarmerForPeriod(5000, 4, faceAway: false, who);
							Dialogue dialogue = GetDialogueForCharacter(npc);
							if (dialogue != null)
							{
								npc.CurrentDialogue.Push(dialogue);
								Game1.drawDialogue(npc);
								npc.grantConversationFriendship(Game1.player);
							}
						}
						else if (_characterGroupLookup.ContainsKey(npc.Name))
						{
							if (!_characterGroupLookup[npc.Name])
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovieAlone", npc.Name));
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_AfterMovie", npc.Name));
							}
						}
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		protected void _PopulateNPCOnlyGroups(List<List<Character>> player_groups, List<List<Character>> groups)
		{
			HashSet<string> used_characters = new HashSet<string>();
			foreach (List<Character> player_group in player_groups)
			{
				foreach (Character character4 in player_group)
				{
					if (character4 is NPC)
					{
						used_characters.Add(character4.name);
					}
				}
			}
			foreach (List<Character> group in groups)
			{
				foreach (Character character3 in group)
				{
					if (character3 is NPC)
					{
						used_characters.Add(character3.name);
					}
				}
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
			int group_count = 0;
			for (int j = 0; j < 2; j++)
			{
				if (r.NextDouble() < 0.75)
				{
					group_count++;
				}
			}
			int time_of_day = 0;
			if (_movieStartTime >= 1200)
			{
				time_of_day = 1;
			}
			if (_movieStartTime >= 1800)
			{
				time_of_day = 2;
			}
			string[][] possible_npcs_for_this_day = possibleNPCGroups[(int)Game1.Date.DayOfWeek][time_of_day];
			if (possible_npcs_for_this_day == null)
			{
				return;
			}
			if (groups.Count > 0 && groups[0].Count == 0)
			{
				groups.RemoveAt(0);
			}
			for (int i = 0; i < group_count; i++)
			{
				if (groups.Count >= 2)
				{
					break;
				}
				int index = r.Next(possible_npcs_for_this_day.Length);
				bool valid = true;
				string[] array = possible_npcs_for_this_day[index];
				foreach (string character2 in array)
				{
					bool found_friendship = false;
					foreach (Farmer allFarmer in Game1.getAllFarmers())
					{
						if (allFarmer.friendshipData.ContainsKey(character2))
						{
							found_friendship = true;
							break;
						}
					}
					if (!found_friendship)
					{
						valid = false;
						break;
					}
					if (used_characters.Contains(character2))
					{
						valid = false;
						break;
					}
					if (GetResponseForMovie(Game1.getCharacterFromName(character2)) == "dislike" || GetResponseForMovie(Game1.getCharacterFromName(character2)) == "reject")
					{
						valid = false;
						break;
					}
				}
				if (valid)
				{
					List<Character> new_group = new List<Character>();
					array = possible_npcs_for_this_day[index];
					foreach (string character in array)
					{
						NPC patron = AddMoviePatronNPC(character, 1000, 1000, 2);
						new_group.Add(patron);
						used_characters.Add(character);
						_characterGroupLookup[character] = (possible_npcs_for_this_day[index].Length > 1);
					}
					groups.Add(new_group);
				}
			}
		}

		public Dictionary<Character, MovieConcession> GetConcessionsDictionary()
		{
			Dictionary<Character, MovieConcession> dictionary = new Dictionary<Character, MovieConcession>();
			foreach (string npc_name in _purchasedConcessions.Keys)
			{
				Character character = Game1.getCharacterFromName(npc_name);
				if (character != null && GetConcessions().ContainsKey(_purchasedConcessions[npc_name]))
				{
					dictionary[character] = GetConcessions()[_purchasedConcessions[npc_name]];
				}
			}
			return dictionary;
		}

		protected void _ResetHangoutPoints()
		{
			_destinationPositions.Clear();
			_availableHangoutPoints = new Dictionary<int, List<Point>>();
			foreach (int key in _hangoutPoints.Keys)
			{
				_availableHangoutPoints[key] = new List<Point>(_hangoutPoints[key]);
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			if (!Game1.eventUp)
			{
				Game1.changeMusicTrack("none");
			}
			base.cleanupBeforePlayerExit();
		}

		public void RequestEndMovie(long uid)
		{
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (_currentState.Value == 1)
			{
				_currentState.Set(2);
				for (int j = 0; j < _viewingGroups.Count; j++)
				{
					int index = Game1.random.Next(_viewingGroups.Count);
					List<Character> characters = _viewingGroups[j];
					_viewingGroups[j] = _viewingGroups[index];
					_viewingGroups[index] = characters;
				}
				_ResetHangoutPoints();
				int character_index = 0;
				for (int group = 0; group < _viewingGroups.Count; group++)
				{
					for (int i = 0; i < _viewingGroups[group].Count; i++)
					{
						if (!(_viewingGroups[group][i] is NPC))
						{
							continue;
						}
						NPC patron_character = GetMoviePatron(_viewingGroups[group][i].Name);
						if (patron_character != null)
						{
							patron_character.setTileLocation(new Vector2(14f, 4f + (float)character_index * 1f));
							Point point = Utility.GetRandom(_availableHangoutPoints[group]);
							int direction = 2;
							if (map.GetLayer("Paths").Tiles[point.X, point.Y].Properties.ContainsKey("direction"))
							{
								int.TryParse(map.GetLayer("Paths").Tiles[point.X, point.Y].Properties["direction"], out direction);
							}
							_destinationPositions[patron_character.Name] = new KeyValuePair<Point, int>(point, direction);
							PathCharacterToLocation(patron_character, point, direction);
							_availableHangoutPoints[group].Remove(point);
							character_index++;
						}
					}
				}
			}
			Game1.getFarmer(uid).team.endMovieEvent.Fire(uid);
		}

		public void PathCharacterToLocation(NPC character, Point point, int direction)
		{
			if (character.currentLocation == this)
			{
				PathFindController controller = new PathFindController(character, this, character.getTileLocationPoint(), direction);
				controller.pathToEndPoint = PathFindController.findPathForNPCSchedules(character.getTileLocationPoint(), point, this, 30000);
				character.temporaryController = controller;
				character.followSchedule = true;
				character.ignoreScheduleToday = true;
			}
		}

		public Dictionary<int, MovieConcession> GetConcessions()
		{
			if (_concessions == null)
			{
				_concessions = new Dictionary<int, MovieConcession>();
				List<ConcessionItemData> list = Game1.content.Load<List<ConcessionItemData>>("Data\\Concessions");
				new List<MovieConcession>();
				foreach (ConcessionItemData data in list)
				{
					_concessions[data.ID] = new MovieConcession(data);
				}
			}
			return _concessions;
		}

		public bool OnPurchaseConcession(ISalable salable, Farmer who, int amount)
		{
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
				{
					_purchasedConcessions[invitation.invitedNPC.Name] = (salable as MovieConcession).id;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", (salable as MovieConcession).DisplayName, invitation.invitedNPC.displayName));
					return true;
				}
			}
			return false;
		}

		public bool HasInvitedSomeone(Farmer who)
		{
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasPurchasedConcession(Farmer who)
		{
			if (!HasInvitedSomeone(who))
			{
				return false;
			}
			foreach (MovieInvitation invitation in who.team.movieInvitations)
			{
				if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
				{
					foreach (string key in _purchasedConcessions.Keys)
					{
						if (key == invitation.invitedNPC.Name && _spawnedMoviePatrons.ContainsKey(invitation.invitedNPC.Name))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public static Farmer GetFirstInvitedPlayer(NPC npc)
		{
			foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
			{
				if (invitation.invitedNPC.Name == npc.Name)
				{
					return invitation.farmer;
				}
			}
			return null;
		}

		public override void performTouchAction(string fullActionString, Vector2 playerStandingPosition)
		{
			string a = fullActionString.Split(' ')[0];
			if (a == "Theater_Exit")
			{
				_exitX = int.Parse(fullActionString.Split(' ')[1]) + Town.GetTheaterTileOffset().X;
				_exitY = int.Parse(fullActionString.Split(' ')[2]) + Town.GetTheaterTileOffset().Y;
				if ((int)Game1.player.lastSeenMovieWeek >= Game1.Date.TotalWeeks)
				{
					_Leave();
					return;
				}
				Game1.player.position.Y -= (Game1.player.Speed + Game1.player.addedSpeed) * 2;
				Game1.player.Halt();
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_LeavePrompt"), Game1.currentLocation.createYesNoResponses(), "LeaveMovie");
			}
			else
			{
				base.performTouchAction(fullActionString, playerStandingPosition);
			}
		}

		public List<MovieConcession> GetConcessionsForGuest(string npc_name)
		{
			List<MovieConcession> concessions = new List<MovieConcession>();
			List<MovieConcession> all_concessions = GetConcessions().Values.ToList();
			Random r = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			Utility.Shuffle(r, all_concessions);
			NPC npc = Game1.getCharacterFromName(npc_name);
			if (npc == null)
			{
				return concessions;
			}
			int num_loved = 1;
			int num_liked = 2;
			int num_disliked = 1;
			int min_concessions = 5;
			for (int j5 = 0; j5 < num_loved; j5++)
			{
				for (int j = 0; j < all_concessions.Count; j++)
				{
					MovieConcession concession = all_concessions[j];
					if (GetConcessionTasteForCharacter(npc, concession) == "love" && (!concession.Name.Equals("Stardrop Sorbet") || r.NextDouble() < 0.33))
					{
						concessions.Add(concession);
						all_concessions.RemoveAt(j);
						j--;
						break;
					}
				}
			}
			for (int j4 = 0; j4 < num_liked; j4++)
			{
				for (int l = 0; l < all_concessions.Count; l++)
				{
					MovieConcession concession2 = all_concessions[l];
					if (GetConcessionTasteForCharacter(npc, concession2) == "like")
					{
						concessions.Add(concession2);
						all_concessions.RemoveAt(l);
						l--;
						break;
					}
				}
			}
			for (int j3 = 0; j3 < num_disliked; j3++)
			{
				for (int n = 0; n < all_concessions.Count; n++)
				{
					MovieConcession concession3 = all_concessions[n];
					if (GetConcessionTasteForCharacter(npc, concession3) == "dislike")
					{
						concessions.Add(concession3);
						all_concessions.RemoveAt(n);
						n--;
						break;
					}
				}
			}
			for (int j2 = concessions.Count; j2 < min_concessions; j2++)
			{
				int i3 = 0;
				if (i3 < all_concessions.Count)
				{
					MovieConcession concession4 = all_concessions[i3];
					concessions.Add(concession4);
					all_concessions.RemoveAt(i3);
					i3--;
				}
			}
			if (_isJojaTheater && !concessions.Exists((MovieConcession x) => x.Name.Equals("JojaCorn")))
			{
				MovieConcession jojaCorn = all_concessions.Find((MovieConcession x) => x.Name.Equals("JojaCorn"));
				if (jojaCorn != null)
				{
					concessions.Add(jojaCorn);
				}
			}
			Utility.Shuffle(r, concessions);
			return concessions;
		}

		public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
		{
			if (questionAndAnswer == null)
			{
				return false;
			}
			if (!(questionAndAnswer == "LeaveMovie_Yes"))
			{
				if (questionAndAnswer == "Concession_Yes")
				{
					string npc_name = "";
					foreach (MovieInvitation invitation in Game1.player.team.movieInvitations)
					{
						if (invitation.farmer == Game1.player && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
						{
							npc_name = invitation.invitedNPC.Name;
						}
					}
					Game1.activeClickableMenu = new ShopMenu(((IEnumerable<ISalable>)GetConcessionsForGuest(npc_name)).ToList(), 0, "Concessions", OnPurchaseConcession);
					return true;
				}
				return base.answerDialogueAction(questionAndAnswer, questionParams);
			}
			_Leave();
			return true;
		}

		protected void _Leave()
		{
			Game1.player.completelyStopAnimatingOrDoingAction();
			Game1.warpFarmer("Town", _exitX, _exitY, 2);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action == "Concessions")
			{
				if (_currentState.Value > 0)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAfterMovie"));
					return true;
				}
				if (!HasInvitedSomeone(who))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionAlone"));
					return true;
				}
				if (HasPurchasedConcession(who))
				{
					foreach (MovieInvitation invitation in who.team.movieInvitations)
					{
						if (invitation.farmer == who && GetFirstInvitedPlayer(invitation.invitedNPC) == Game1.player)
						{
							foreach (string name in _purchasedConcessions.Keys)
							{
								if (name == invitation.invitedNPC.Name)
								{
									MovieConcession concession = GetConcessionsDictionary()[Game1.getCharacterFromName(name)];
									Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_ConcessionPurchased", concession.DisplayName, Game1.getCharacterFromName(name).displayName));
									return true;
								}
							}
						}
					}
					return true;
				}
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:MovieTheater_Concession"), Game1.currentLocation.createYesNoResponses(), "Concession");
			}
			else
			{
				if (action == "Theater_Doors")
				{
					if (_currentState.Value > 0)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Theater_MovieEndReEntry"));
						return true;
					}
					if (Game1.player.team.movieMutex.IsLocked())
					{
						_ShowMovieStartReady();
						return true;
					}
					Game1.player.team.movieMutex.RequestLock(delegate
					{
						List<Farmer> list = new List<Farmer>();
						foreach (Farmer current in farmers)
						{
							if (current.isActive() && current.currentLocation == this)
							{
								list.Add(current);
							}
						}
						movieViewerLockEvent.Fire(new MovieViewerLockEvent(list, Game1.timeOfDay));
					});
					return true;
				}
				if (action == "CraneGame")
				{
					if (getTileIndexAt(2, 9, "Buildings") == -1)
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CranePlay", 500), createYesNoResponses(), tryToStartCraneGame);
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:MovieTheater_CraneOccupied"));
					}
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		private void tryToStartCraneGame(Farmer who, string whichAnswer)
		{
			if (whichAnswer.ToLower() == "yes")
			{
				if (Game1.player.Money >= 500)
				{
					Game1.player.Money -= 500;
					Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.MiniGame);
					Game1.globalFadeToBlack(delegate
					{
						Game1.currentMinigame = new CraneGame();
					}, 0.008f);
				}
				else
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"));
				}
			}
		}

		public static void ClearCachedLocalizedData()
		{
			_concessions = null;
			_genericReactions = null;
			_movieData = null;
		}
	}
}
