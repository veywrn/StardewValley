using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using xTile.Dimensions;

namespace StardewValley
{
	public class Event
	{
		protected static Dictionary<string, MethodInfo> _commandLookup;

		[InstancedStatic]
		protected static object[] _eventCommandArgs = new object[3];

		public const int weddingEventId = -2;

		private const float timeBetweenSpeech = 500f;

		private const float viewportMoveSpeed = 3f;

		public const string festivalTextureName = "Maps\\Festivals";

		public bool simultaneousCommand;

		public string[] eventCommands;

		public int currentCommand;

		public int farmerAddedSpeed;

		public int int_useMeForAnything;

		public int int_useMeForAnything2;

		public List<NPC> actors = new List<NPC>();

		public List<Object> props = new List<Object>();

		public List<Prop> festivalProps = new List<Prop>();

		public string messageToScreen;

		public string playerControlSequenceID;

		public string spriteTextToDraw;

		public bool showActiveObject;

		public bool continueAfterMove;

		public bool specialEventVariable1;

		public bool forked;

		public bool wasBloomDay;

		public bool wasBloomVisible;

		public bool eventSwitched;

		public bool isFestival;

		public bool showGroundObjects = true;

		public bool isWedding;

		public bool doingSecretSanta;

		public bool showWorldCharacters;

		public bool isMemory;

		public bool ignoreObjectCollisions = true;

		protected bool _playerControlSequence;

		private Dictionary<string, Vector3> actorPositionsAfterMove;

		private float timeAccumulator;

		private float viewportXAccumulator;

		private float viewportYAccumulator;

		public float float_useMeForAnything;

		private Vector3 viewportTarget;

		private Color previousAmbientLight;

		private BloomSettings previousBloomSettings;

		public List<NPC> npcsWithUniquePortraits = new List<NPC>();

		public LocationRequest exitLocation;

		public ICustomEventScript currentCustomEventScript;

		public List<Farmer> farmerActors = new List<Farmer>();

		private HashSet<long> festivalWinners = new HashSet<long>();

		public Action onEventFinished;

		protected bool _repeatingLocationSpecificCommand;

		private readonly LocalizedContentManager festivalContent = Game1.content.CreateTemporary();

		private GameLocation temporaryLocation;

		public Point playerControlTargetTile;

		private Texture2D _festivalTexture;

		public List<NPCController> npcControllers;

		public NPC secretSantaRecipient;

		public NPC mySecretSanta;

		public bool skippable;

		public int id;

		public List<Vector2> characterWalkLocations = new List<Vector2>();

		public bool ignoreTileOffsets;

		public Vector2 eventPositionTileOffset = Vector2.Zero;

		[NonInstancedStatic]
		public static HashSet<string> invalidFestivals = new HashSet<string>();

		private Dictionary<string, string> festivalData;

		private int oldShirt;

		private Color oldPants;

		private bool drawTool;

		public bool skipped;

		private bool waitingForMenuClose;

		private int oldTime;

		public List<TemporaryAnimatedSprite> underwaterSprites;

		public List<TemporaryAnimatedSprite> aboveMapSprites;

		private NPC festivalHost;

		private string hostMessage;

		public int festivalTimer;

		public int grangeScore = -1000;

		public bool grangeJudged;

		private int previousFacingDirection = -1;

		public Dictionary<string, Dictionary<ISalable, int[]>> festivalShops;

		private int previousAnswerChoice = -1;

		private bool startSecretSantaAfterDialogue;

		public bool specialEventVariable2;

		private List<Farmer> winners;

		public bool playerControlSequence
		{
			get
			{
				return _playerControlSequence;
			}
			set
			{
				if (_playerControlSequence != value)
				{
					_playerControlSequence = value;
					if (!_playerControlSequence)
					{
						OnPlayerControlSequenceEnd(playerControlSequenceID);
					}
				}
			}
		}

		public Farmer farmer
		{
			get
			{
				if (farmerActors.Count <= 0)
				{
					return Game1.player;
				}
				return farmerActors[0];
			}
		}

		public Texture2D festivalTexture
		{
			get
			{
				if (_festivalTexture == null)
				{
					_festivalTexture = festivalContent.Load<Texture2D>("Maps\\Festivals");
				}
				return _festivalTexture;
			}
		}

		public int CurrentCommand
		{
			get
			{
				return currentCommand;
			}
			set
			{
				currentCommand = value;
			}
		}

		public string FestivalName
		{
			get
			{
				if (festivalData == null)
				{
					return "";
				}
				return festivalData["name"];
			}
		}

		public virtual void setupEventCommands()
		{
			if (_commandLookup == null)
			{
				_commandLookup = new Dictionary<string, MethodInfo>(StringComparer.InvariantCultureIgnoreCase);
				MethodInfo[] event_command_methods = (from method_info in typeof(Event).GetMethods()
					where method_info.Name.StartsWith("command_")
					select method_info).ToArray();
				MethodInfo[] array = event_command_methods;
				foreach (MethodInfo method in array)
				{
					_commandLookup.Add(method.Name.Substring("command_".Length), method);
				}
				Console.WriteLine("setupEventCommands() registered '{0}' methods", event_command_methods.Length);
			}
		}

		public virtual void tryEventCommand(GameLocation location, GameTime time, string[] split)
		{
			_eventCommandArgs[0] = location;
			_eventCommandArgs[1] = time;
			_eventCommandArgs[2] = split;
			if (split.Length != 0)
			{
				if (_commandLookup.TryGetValue(split[0], out MethodInfo method_info))
				{
					try
					{
						method_info.Invoke(this, _eventCommandArgs);
					}
					catch (TargetInvocationException e)
					{
						LogErrorAndHalt(e.InnerException);
					}
				}
				else
				{
					Console.WriteLine("ERROR: Invalid command: " + split[0]);
				}
			}
		}

		public virtual void command_ignoreEventTileOffset(GameLocation location, GameTime time, string[] split)
		{
			ignoreTileOffsets = true;
			CurrentCommand++;
		}

		public virtual void command_move(GameLocation location, GameTime time, string[] split)
		{
			for (int i = 1; i < split.Length && split.Length - i >= 3; i += 4)
			{
				if (split[i].Contains("farmer") && !actorPositionsAfterMove.ContainsKey(split[i]))
				{
					Farmer f = getFarmerFromFarmerNumberString(split[i], farmer);
					if (f != null)
					{
						f.canOnlyWalk = false;
						f.setRunning(isRunning: false, force: true);
						f.canOnlyWalk = true;
						f.convertEventMotionCommandToMovement(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])));
						actorPositionsAfterMove.Add(split[i], getPositionAfterMove(farmer, Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2]), Convert.ToInt32(split[i + 3])));
					}
				}
				else
				{
					NPC j = getActorByName(split[i]);
					string name = split[i].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[i];
					if (!actorPositionsAfterMove.ContainsKey(name))
					{
						j.convertEventMotionCommandToMovement(new Vector2(Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])));
						actorPositionsAfterMove.Add(name, getPositionAfterMove(j, Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2]), Convert.ToInt32(split[i + 3])));
					}
				}
			}
			if (split.Last().Equals("true"))
			{
				continueAfterMove = true;
				CurrentCommand++;
			}
			else if (split.Last().Equals("false"))
			{
				continueAfterMove = false;
				if (split.Length == 2 && actorPositionsAfterMove.Count == 0)
				{
					CurrentCommand++;
				}
			}
		}

		public virtual void command_speak(GameLocation location, GameTime time, string[] split)
		{
			if (skipped || Game1.dialogueUp)
			{
				return;
			}
			timeAccumulator += time.ElapsedGameTime.Milliseconds;
			if (timeAccumulator < 500f)
			{
				return;
			}
			timeAccumulator = 0f;
			NPC i = getActorByName(split[1]);
			if (i == null)
			{
				Game1.getCharacterFromName(split[1].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[1]);
			}
			if (i == null)
			{
				Game1.eventFinished();
				return;
			}
			int firstQuoteIndex = eventCommands[currentCommand].IndexOf('"');
			if (firstQuoteIndex > 0)
			{
				int lastQuoteIndex = eventCommands[CurrentCommand].Substring(firstQuoteIndex + 1).LastIndexOf('"');
				Game1.player.checkForQuestComplete(i, -1, -1, null, null, 5);
				if (Game1.NPCGiftTastes.ContainsKey(split[1]) && !Game1.player.friendshipData.ContainsKey(split[1]))
				{
					Game1.player.friendshipData.Add(split[1], new Friendship(0));
				}
				if (lastQuoteIndex > 0)
				{
					i.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(firstQuoteIndex + 1, lastQuoteIndex), i));
				}
				else
				{
					i.CurrentDialogue.Push(new Dialogue("...", i));
				}
			}
			else
			{
				i.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(split[2]), i));
			}
			Game1.drawDialogue(i);
		}

		public virtual void command_beginSimultaneousCommand(GameLocation location, GameTime time, string[] split)
		{
			simultaneousCommand = true;
			CurrentCommand++;
		}

		public virtual void command_endSimultaneousCommand(GameLocation location, GameTime time, string[] split)
		{
			simultaneousCommand = false;
			CurrentCommand++;
		}

		public virtual void command_minedeath(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
			int moneyToLose4 = r.Next(Game1.player.Money / 20, Game1.player.Money / 4);
			moneyToLose4 = Math.Min(moneyToLose4, 5000);
			moneyToLose4 -= (int)((double)Game1.player.LuckLevel * 0.01 * (double)moneyToLose4);
			moneyToLose4 -= moneyToLose4 % 100;
			int numberOfItemsLost = 0;
			double itemLossRate = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
			Game1.player.itemsLostLastDeath.Clear();
			for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
			{
				if (Game1.player.Items[i] != null && (!(Game1.player.Items[i] is Tool) || (Game1.player.Items[i] is MeleeWeapon && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[i].canBeTrashed() && !(Game1.player.Items[i] is Ring) && r.NextDouble() < itemLossRate)
				{
					Item item = Game1.player.Items[i];
					Game1.player.Items[i] = null;
					numberOfItemsLost++;
					Game1.player.itemsLostLastDeath.Add(item);
				}
			}
			Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
			Game1.player.Money = Math.Max(0, Game1.player.Money - moneyToLose4);
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1057") + " " + ((moneyToLose4 <= 0) ? "" : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1058", moneyToLose4)) + ((numberOfItemsLost <= 0) ? ((moneyToLose4 <= 0) ? "" : ".") : ((moneyToLose4 <= 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1060") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1063") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))))));
			List<string> tmp = eventCommands.ToList();
			tmp.Insert(CurrentCommand + 1, "showItemsLost");
			eventCommands = tmp.ToArray();
		}

		public virtual void command_hospitaldeath(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + Game1.timeOfDay);
			int numberOfItemsLost = 0;
			double itemLossRate = 0.25 - (double)Game1.player.LuckLevel * 0.05 - Game1.player.DailyLuck;
			Game1.player.itemsLostLastDeath.Clear();
			for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
			{
				if (Game1.player.Items[i] != null && (!(Game1.player.Items[i] is Tool) || (Game1.player.Items[i] is MeleeWeapon && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 47 && (Game1.player.Items[i] as MeleeWeapon).InitialParentTileIndex != 4)) && Game1.player.Items[i].canBeTrashed() && !(Game1.player.Items[i] is Ring) && r.NextDouble() < itemLossRate)
				{
					Item item = Game1.player.Items[i];
					Game1.player.Items[i] = null;
					numberOfItemsLost++;
					Game1.player.itemsLostLastDeath.Add(item);
				}
			}
			Game1.player.Stamina = Math.Min(Game1.player.Stamina, 2f);
			int moneyToLose = Math.Min(1000, Game1.player.Money);
			Game1.player.Money -= moneyToLose;
			Game1.drawObjectDialogue(((moneyToLose > 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1068", moneyToLose) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1070")) + ((numberOfItemsLost > 0) ? (Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1071") + ((numberOfItemsLost == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1061") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1062", numberOfItemsLost))) : ""));
			List<string> tmp = eventCommands.ToList();
			tmp.Insert(CurrentCommand + 1, "showItemsLost");
			eventCommands = tmp.ToArray();
		}

		public virtual void command_showItemsLost(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new ItemListMenu(Game1.content.LoadString("Strings\\UI:ItemList_ItemsLost"), Game1.player.itemsLostLastDeath.ToList());
			}
		}

		public virtual void command_end(GameLocation location, GameTime time, string[] split)
		{
			endBehaviors(split, location);
		}

		public virtual void command_locationSpecificCommand(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 1)
			{
				if (location.RunLocationSpecificEventCommand(this, split[1], !_repeatingLocationSpecificCommand, split.Skip(2).ToArray()))
				{
					_repeatingLocationSpecificCommand = false;
					CurrentCommand++;
				}
				else
				{
					_repeatingLocationSpecificCommand = true;
				}
			}
		}

		public virtual void command_unskippable(GameLocation location, GameTime time, string[] split)
		{
			skippable = false;
			CurrentCommand++;
		}

		public virtual void command_skippable(GameLocation location, GameTime time, string[] split)
		{
			skippable = true;
			CurrentCommand++;
		}

		public virtual void command_emote(GameLocation location, GameTime time, string[] split)
		{
			bool nextCommandImmediate = split.Length > 3;
			if (split[1].Contains("farmer"))
			{
				if (getFarmerFromFarmerNumberString(split[1], farmer) != null)
				{
					farmer.doEmote(Convert.ToInt32(split[2]), !nextCommandImmediate);
				}
			}
			else
			{
				NPC i = getActorByName(split[1]);
				if (!i.isEmoting)
				{
					i.doEmote(Convert.ToInt32(split[2]), !nextCommandImmediate);
				}
			}
			if (nextCommandImmediate)
			{
				CurrentCommand++;
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_stopMusic(GameLocation location, GameTime time, string[] split)
		{
			Game1.changeMusicTrack("none", track_interruptable: false, Game1.MusicContext.Event);
			CurrentCommand++;
		}

		public virtual void command_playSound(GameLocation location, GameTime time, string[] split)
		{
			Game1.playSound(split[1]);
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_tossConcession(GameLocation location, GameTime time, string[] split)
		{
			NPC actor = getActorByName(split[1]);
			int concession_id = int.Parse(split[2]);
			Game1.playSound("dwop");
			location.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = Game1.concessionsSpriteSheet,
				sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.concessionsSpriteSheet, concession_id, 16, 16),
				animationLength = 1,
				totalNumberOfLoops = 1,
				motion = new Vector2(0f, -6f),
				acceleration = new Vector2(0f, 0.2f),
				interval = 1000f,
				scale = 4f,
				position = OffsetPosition(new Vector2(actor.Position.X, actor.Position.Y - 96f)),
				layerDepth = (float)actor.getStandingY() / 10000f
			});
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_pause(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = Convert.ToInt32(split[1]);
			}
		}

		public virtual void command_resetVariable(GameLocation location, GameTime time, string[] split)
		{
			specialEventVariable1 = false;
			currentCommand++;
		}

		public virtual void command_faceDirection(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.FarmerSprite.StopAnimation();
					f.completelyStopAnimatingOrDoingAction();
					f.faceDirection(Convert.ToInt32(split[2]));
				}
			}
			else if (split[1].Contains("spouse"))
			{
				if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && getActorByName(Game1.player.spouse) != null && !Game1.player.isRoommate(Game1.player.spouse))
				{
					getActorByName(Game1.player.spouse).faceDirection(Convert.ToInt32(split[2]));
				}
			}
			else
			{
				getActorByName(split[1])?.faceDirection(Convert.ToInt32(split[2]));
			}
			if (split.Length == 3 && Game1.pauseTime <= 0f)
			{
				Game1.pauseTime = 500f;
			}
			else if (split.Length > 3)
			{
				CurrentCommand++;
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_warp(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.setTileLocation(OffsetTile(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]))));
					f.position.Y -= 16f;
					if (farmerActors.Contains(f))
					{
						f.completelyStopAnimatingOrDoingAction();
					}
				}
			}
			else if (split[1].Contains("spouse"))
			{
				if (Game1.player.spouse != null && Game1.player.spouse.Length > 0 && getActorByName(Game1.player.spouse) != null && !Game1.player.isRoommate(Game1.player.spouse))
				{
					if (npcControllers != null)
					{
						for (int j = npcControllers.Count - 1; j >= 0; j--)
						{
							if (npcControllers[j].puppet.Name.Equals(Game1.player.spouse))
							{
								npcControllers.RemoveAt(j);
							}
						}
					}
					getActorByName(Game1.player.spouse).Position = OffsetPosition(new Vector2(Convert.ToInt32(split[2]) * 64, Convert.ToInt32(split[3]) * 64));
				}
			}
			else
			{
				NPC i = getActorByName(split[1]);
				if (i != null)
				{
					i.position.X = OffsetPositionX(Convert.ToInt32(split[2]) * 64 + 4);
					i.position.Y = OffsetPositionY(Convert.ToInt32(split[3]) * 64);
				}
			}
			CurrentCommand++;
			if (split.Length > 4)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_speed(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmerAddedSpeed = Convert.ToInt32(split[2]);
			}
			else
			{
				getActorByName(split[1]).speed = Convert.ToInt32(split[2]);
			}
			CurrentCommand++;
		}

		public virtual void command_stopAdvancedMoves(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 1)
			{
				if (split[1].Equals("next"))
				{
					foreach (NPCController npcController in npcControllers)
					{
						npcController.destroyAtNextCrossroad();
					}
				}
			}
			else
			{
				npcControllers.Clear();
			}
			CurrentCommand++;
		}

		public virtual void command_doAction(GameLocation location, GameTime time, string[] split)
		{
			location.checkAction(new Location(OffsetTileX(Convert.ToInt32(split[1])), OffsetTileY(Convert.ToInt32(split[2]))), Game1.viewport, farmer);
			CurrentCommand++;
		}

		public virtual void command_removeTile(GameLocation location, GameTime time, string[] split)
		{
			location.removeTile(OffsetTileX(Convert.ToInt32(split[1])), OffsetTileY(Convert.ToInt32(split[2])), split[3]);
			CurrentCommand++;
		}

		public virtual void command_textAboveHead(GameLocation location, GameTime time, string[] split)
		{
			NPC i = getActorByName(split[1]);
			if (i != null)
			{
				int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int lastQuoteIndex = eventCommands[CurrentCommand].Substring(firstQuoteIndex).LastIndexOf('"');
				i.showTextAboveHead(eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex));
			}
			CurrentCommand++;
		}

		public virtual void command_showFrame(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 2 && !split[2].Equals("flip") && !split[1].Contains("farmer"))
			{
				NPC i = getActorByName(split[1]);
				if (i != null)
				{
					int frame = Convert.ToInt32(split[2]);
					if (split[1].Equals("spouse") && i.Gender == 0 && frame >= 36 && frame <= 38)
					{
						frame += 12;
					}
					i.Sprite.CurrentFrame = frame;
				}
			}
			else
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (split.Length == 2)
				{
					f = farmer;
				}
				if (f != null)
				{
					if (split.Length > 2)
					{
						split[1] = split[2];
					}
					List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
					animationFrames.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[1]), 100, secondaryArm: false, split.Length > 2));
					f.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
					f.FarmerSprite.loop = true;
					f.FarmerSprite.loopThisAnimation = true;
					f.FarmerSprite.PauseForSingleAnimation = true;
					f.Sprite.currentFrame = Convert.ToInt32(split[1]);
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_farmerAnimation(GameLocation location, GameTime time, string[] split)
		{
			farmer.FarmerSprite.setCurrentSingleAnimation(Convert.ToInt32(split[1]));
			farmer.FarmerSprite.PauseForSingleAnimation = true;
			CurrentCommand++;
		}

		public virtual void command_ignoreMovementAnimation(GameLocation location, GameTime time, string[] split)
		{
			bool ignore = true;
			if (split.Length > 2)
			{
				split[2].Equals("true");
			}
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.ignoreMovementAnimation = ignore;
				}
			}
			else
			{
				NPC i = getActorByName(split[1].Replace('_', ' '));
				if (i != null)
				{
					i.ignoreMovementAnimation = ignore;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_animate(GameLocation location, GameTime time, string[] split)
		{
			int interval = Convert.ToInt32(split[4]);
			bool flip = split[2].Equals("true");
			bool loop = split[3].Equals("true");
			List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>();
			for (int j = 5; j < split.Length; j++)
			{
				animationFrames.Add(new FarmerSprite.AnimationFrame(Convert.ToInt32(split[j]), interval, secondaryArm: false, flip));
			}
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
					f.FarmerSprite.loop = true;
					f.FarmerSprite.loopThisAnimation = loop;
					f.FarmerSprite.PauseForSingleAnimation = true;
				}
			}
			else
			{
				NPC i = getActorByName(split[1].Replace('_', ' '));
				if (i != null)
				{
					i.Sprite.setCurrentAnimation(animationFrames);
					i.Sprite.loop = loop;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_stopAnimation(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.completelyStopAnimatingOrDoingAction();
					f.Halt();
					f.FarmerSprite.CurrentAnimation = null;
					switch (f.FacingDirection)
					{
					case 0:
						f.FarmerSprite.setCurrentSingleFrame(12, 32000);
						break;
					case 1:
						f.FarmerSprite.setCurrentSingleFrame(6, 32000);
						break;
					case 2:
						f.FarmerSprite.setCurrentSingleFrame(0, 32000);
						break;
					case 3:
						f.FarmerSprite.setCurrentSingleFrame(6, 32000, secondaryArm: false, flip: true);
						break;
					}
				}
			}
			else
			{
				NPC i = getActorByName(split[1]);
				if (i != null)
				{
					i.Sprite.StopAnimation();
					if (split.Length > 2)
					{
						i.Sprite.currentFrame = Convert.ToInt32(split[2]);
						i.Sprite.UpdateSourceRect();
					}
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_showRivalFrame(GameLocation location, GameTime time, string[] split)
		{
			getActorByName("rival").Sprite.currentFrame = Convert.ToInt32(split[1]);
			CurrentCommand++;
		}

		public virtual void command_weddingSprite(GameLocation location, GameTime time, string[] split)
		{
			getActorByName("WeddingOutfits").Sprite.currentFrame = Convert.ToInt32(split[1]);
			CurrentCommand++;
		}

		public virtual void command_changeLocation(GameLocation location, GameTime time, string[] split)
		{
			changeLocation(split[1], farmer.getTileX(), farmer.getTileY(), -1, delegate
			{
				CurrentCommand++;
			});
		}

		public virtual void command_halt(GameLocation location, GameTime time, string[] split)
		{
			foreach (NPC actor in actors)
			{
				actor.Halt();
			}
			farmer.Halt();
			CurrentCommand++;
			continueAfterMove = false;
			actorPositionsAfterMove.Clear();
		}

		public virtual void command_message(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.dialogueUp && Game1.activeClickableMenu == null)
			{
				int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int lastQuoteIndex = eventCommands[CurrentCommand].LastIndexOf('"');
				if (lastQuoteIndex > 0 && lastQuoteIndex > firstQuoteIndex)
				{
					Game1.drawDialogueNoTyping(Game1.parseText(eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex - firstQuoteIndex)));
				}
				else
				{
					Game1.drawDialogueNoTyping("...");
				}
			}
		}

		public virtual void command_addCookingRecipe(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.cookingRecipes.Add(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1), 0);
			CurrentCommand++;
		}

		public virtual void command_itemAboveHead(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 1 && split[1].Equals("pan"))
			{
				farmer.holdUpItemThenMessage(new Pan());
			}
			else if (split.Length > 1 && split[1].Equals("hero"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 116));
			}
			else if (split.Length > 1 && split[1].Equals("sculpture"))
			{
				farmer.holdUpItemThenMessage(new Furniture(1306, Vector2.Zero));
			}
			else if (split.Length > 1 && split[1].Equals("samBoombox"))
			{
				farmer.holdUpItemThenMessage(new Furniture(1309, Vector2.Zero));
			}
			else if (split.Length > 1 && split[1].Equals("joja"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 117));
			}
			else if (split.Length > 1 && split[1].Equals("slimeEgg"))
			{
				farmer.holdUpItemThenMessage(new Object(680, 1));
			}
			else if (split.Length > 1 && split[1].Equals("rod"))
			{
				farmer.holdUpItemThenMessage(new FishingRod());
			}
			else if (split.Length > 1 && split[1].Equals("sword"))
			{
				farmer.holdUpItemThenMessage(new MeleeWeapon(0));
			}
			else if (split.Length > 1 && split[1].Equals("ore"))
			{
				farmer.holdUpItemThenMessage(new Object(378, 1), showMessage: false);
			}
			else if (split.Length > 1 && split[1].Equals("pot"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 62), showMessage: false);
			}
			else if (split.Length > 1 && split[1].Equals("jukebox"))
			{
				farmer.holdUpItemThenMessage(new Object(Vector2.Zero, 209), showMessage: false);
			}
			else
			{
				farmer.holdUpItemThenMessage(null, showMessage: false);
			}
			CurrentCommand++;
		}

		public virtual void command_addCraftingRecipe(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.player.craftingRecipes.ContainsKey(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1)))
			{
				Game1.player.craftingRecipes.Add(eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf(' ') + 1), 0);
			}
			CurrentCommand++;
		}

		public virtual void command_hostMail(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.IsMasterGame && !Game1.player.hasOrWillReceiveMail(split[1]))
			{
				Game1.addMailForTomorrow(split[1]);
			}
			CurrentCommand++;
		}

		public virtual void command_mail(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.player.hasOrWillReceiveMail(split[1]))
			{
				Game1.addMailForTomorrow(split[1]);
			}
			CurrentCommand++;
		}

		public virtual void command_shake(GameLocation location, GameTime time, string[] split)
		{
			getActorByName(split[1]).shake(Convert.ToInt32(split[2]));
			CurrentCommand++;
		}

		public virtual void command_temporarySprite(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[3]), OffsetPosition(new Vector2(Convert.ToInt32(split[1]) * 64, Convert.ToInt32(split[2]) * 64)), Color.White, Convert.ToInt32(split[4]), split.Length > 6 && split[6] == "true", (split.Length > 5) ? ((float)Convert.ToInt32(split[5])) : 300f, 0, 64, (split.Length > 7) ? ((float)Convert.ToDouble(split[7])) : (-1f)));
			CurrentCommand++;
		}

		public virtual void command_removeTemporarySprites(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Clear();
			CurrentCommand++;
		}

		public virtual void command_null(GameLocation location, GameTime time, string[] split)
		{
		}

		public virtual void command_specificTemporarySprite(GameLocation location, GameTime time, string[] split)
		{
			addSpecificTemporarySprite(split[1], location, split);
			CurrentCommand++;
		}

		public virtual void command_playMusic(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("samBand"))
			{
				if (Game1.player.DialogueQuestionsAnswered.Contains(78))
				{
					Game1.changeMusicTrack("shimmeringbastion", track_interruptable: false, Game1.MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains(79))
				{
					Game1.changeMusicTrack("honkytonky", track_interruptable: false, Game1.MusicContext.Event);
				}
				else if (Game1.player.DialogueQuestionsAnswered.Contains(77))
				{
					Game1.changeMusicTrack("heavy", track_interruptable: false, Game1.MusicContext.Event);
				}
				else
				{
					Game1.changeMusicTrack("poppy", track_interruptable: false, Game1.MusicContext.Event);
				}
			}
			else if (Game1.options.musicVolumeLevel > 0f)
			{
				StringBuilder b = new StringBuilder(split[1]);
				for (int i = 2; i < split.Length; i++)
				{
					b.Append(" " + split[i]);
				}
				Game1.changeMusicTrack(b.ToString(), track_interruptable: false, Game1.MusicContext.Event);
			}
			CurrentCommand++;
		}

		public virtual void command_nameSelect(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.nameSelectUp)
			{
				Game1.showNameSelectScreen(split[1]);
			}
		}

		public virtual void command_makeInvisible(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() == 3)
			{
				int x = OffsetTileX(Convert.ToInt32(split[1]));
				int y2 = OffsetTileY(Convert.ToInt32(split[2]));
				Object o4 = null;
				o4 = location.getObjectAtTile(x, y2);
				if (o4 != null)
				{
					o4.isTemporarilyInvisible = true;
				}
			}
			else
			{
				int x2 = OffsetTileX(Convert.ToInt32(split[1]));
				int y = OffsetTileY(Convert.ToInt32(split[2]));
				int width = Convert.ToInt32(split[3]);
				int height = Convert.ToInt32(split[4]);
				Object o2 = null;
				for (int j = x2; j < x2 + width; j++)
				{
					for (int i = y; i < y + height; i++)
					{
						o2 = location.getObjectAtTile(j, i);
						if (o2 != null)
						{
							o2.isTemporarilyInvisible = true;
						}
						else if (location.terrainFeatures.ContainsKey(new Vector2(j, i)))
						{
							location.terrainFeatures[new Vector2(j, i)].isTemporarilyInvisible = true;
						}
					}
				}
			}
			CurrentCommand++;
		}

		public virtual void command_addObject(GameLocation location, GameTime time, string[] split)
		{
			float layerDepth = (float)(OffsetTileY(Convert.ToInt32(split[2])) * 64) / 10000f;
			if (split.Length > 4)
			{
				layerDepth = Convert.ToSingle(split[4]);
			}
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[3]), 9999f, 1, 9999, OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f), flicker: false, flipped: false)
			{
				layerDepth = layerDepth
			});
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addBigProp(GameLocation location, GameTime time, string[] split)
		{
			props.Add(new Object(OffsetTile(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]))), Convert.ToInt32(split[3])));
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addFloorProp(GameLocation location, GameTime time, string[] split)
		{
			command_addProp(location, time, split);
		}

		public virtual void command_addProp(GameLocation location, GameTime time, string[] split)
		{
			int tileX = OffsetTileX(Convert.ToInt32(split[2]));
			int tileY = OffsetTileY(Convert.ToInt32(split[3]));
			int index = Convert.ToInt32(split[1]);
			int drawWidth = (split.Length <= 4) ? 1 : Convert.ToInt32(split[4]);
			int drawHeight = (split.Length <= 5) ? 1 : Convert.ToInt32(split[5]);
			int boundingHeight = (split.Length > 6) ? Convert.ToInt32(split[6]) : drawHeight;
			bool solid = !split[0].Contains("Floor");
			festivalProps.Add(new Prop(festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, tileY, solid));
			if (split.Length > 7)
			{
				int tilesHorizontal = Convert.ToInt32(split[7]);
				for (int x = tileX + tilesHorizontal; x != tileX; x -= Math.Sign(tilesHorizontal))
				{
					festivalProps.Add(new Prop(festivalTexture, index, drawWidth, boundingHeight, drawHeight, x, tileY, solid));
				}
			}
			if (split.Length > 8)
			{
				int tilesVertical = Convert.ToInt32(split[8]);
				for (int y = tileY + tilesVertical; y != tileY; y -= Math.Sign(tilesVertical))
				{
					festivalProps.Add(new Prop(festivalTexture, index, drawWidth, boundingHeight, drawHeight, tileX, y, solid));
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_addToTable(GameLocation location, GameTime time, string[] split)
		{
			if (location is FarmHouse)
			{
				(location as FarmHouse).furniture[0].heldObject.Value = new Object(Vector2.Zero, Convert.ToInt32(split[3]), 1);
			}
			else
			{
				location.objects[OffsetTile(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])))].heldObject.Value = new Object(Vector2.Zero, Convert.ToInt32(split[3]), 1);
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_removeObject(GameLocation location, GameTime time, string[] split)
		{
			Vector2 position = OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f);
			for (int i = location.temporarySprites.Count - 1; i >= 0; i--)
			{
				if (location.temporarySprites[i].position.Equals(position))
				{
					location.temporarySprites.RemoveAt(i);
					break;
				}
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_glow(GameLocation location, GameTime time, string[] split)
		{
			bool hold = false;
			if (split.Length > 4 && split[4].Equals("true"))
			{
				hold = true;
			}
			Game1.screenGlowOnce(new Color(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), Convert.ToInt32(split[3])), hold);
			CurrentCommand++;
		}

		public virtual void command_stopGlowing(GameLocation location, GameTime time, string[] split)
		{
			Game1.screenGlowUp = false;
			Game1.screenGlowHold = false;
			CurrentCommand++;
		}

		public virtual void command_addQuest(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.addQuest(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_removeQuest(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.removeQuest(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_awardFestivalPrize(GameLocation location, GameTime time, string[] split)
		{
			if (festivalWinners.Contains(Game1.player.UniqueMultiplayerID))
			{
				string a = festivalData["file"];
				if (!(a == "spring13"))
				{
					if (!(a == "winter8"))
					{
						return;
					}
					if (!Game1.player.mailReceived.Contains("Ice Festival"))
					{
						if (Game1.activeClickableMenu == null)
						{
							Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>
							{
								new Hat(17),
								new Object(687, 1),
								new Object(691, 1),
								new Object(703, 1)
							}, this).setEssential(essential: true);
						}
						Game1.player.mailReceived.Add("Ice Festival");
						CurrentCommand++;
					}
					else
					{
						Game1.player.Money += 2000;
						Game1.playSound("money");
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1164"));
						CurrentCommand += 2;
					}
				}
				else if (!Game1.player.mailReceived.Contains("Egg Festival"))
				{
					if (Game1.activeClickableMenu == null)
					{
						Game1.player.addItemByMenuIfNecessary(new Hat(4));
					}
					Game1.player.mailReceived.Add("Egg Festival");
					CurrentCommand++;
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
				}
				else
				{
					Game1.player.Money += 1000;
					Game1.playSound("money");
					CurrentCommand += 2;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1159"));
				}
			}
			else if (split.Length > 1)
			{
				switch (split[1].ToLower())
				{
				case "birdiereward":
					new List<Item>();
					Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
					{
						Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
					}
					CurrentCommand++;
					CurrentCommand++;
					break;
				case "memento":
				{
					Object o = new Object(864, 1)
					{
						specialItem = true
					};
					o.questItem.Value = true;
					Game1.player.addItemByMenuIfNecessary(o);
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				}
				case "emilyclothes":
				{
					Clothing pants = new Clothing(8);
					pants.Dye(new Color(0, 143, 239), 1f);
					Game1.player.addItemsByMenuIfNecessary(new List<Item>
					{
						new Boots(804),
						new Hat(41),
						new Clothing(1127),
						pants
					});
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				}
				case "qimilk":
					if (!Game1.player.mailReceived.Contains("qiCave"))
					{
						Game1.player.maxHealth += 25;
						Game1.player.mailReceived.Add("qiCave");
					}
					CurrentCommand++;
					break;
				case "pan":
					Game1.player.addItemByMenuIfNecessary(new Pan());
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "sculpture":
					Game1.player.addItemByMenuIfNecessary(new Furniture(1306, Vector2.Zero));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "samboombox":
					Game1.player.addItemByMenuIfNecessary(new Furniture(1309, Vector2.Zero));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "marniepainting":
					Game1.player.addItemByMenuIfNecessary(new Furniture(1802, Vector2.Zero));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "rod":
					Game1.player.addItemByMenuIfNecessary(new FishingRod());
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "pot":
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 62));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "jukebox":
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 209));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "sword":
					Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(0));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "hero":
					Game1.getSteamAchievement("Achievement_LocalLegend");
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 116));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "joja":
					Game1.getSteamAchievement("Achievement_Joja");
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 117));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				case "slimeegg":
					Game1.player.addItemByMenuIfNecessary(new Object(680, 1));
					if (Game1.activeClickableMenu == null)
					{
						CurrentCommand++;
					}
					CurrentCommand++;
					break;
				}
			}
			else
			{
				CurrentCommand += 2;
			}
		}

		public virtual void command_attachCharacterToTempSprite(GameLocation location, GameTime time, string[] split)
		{
			TemporaryAnimatedSprite t = location.temporarySprites.Last();
			if (t != null)
			{
				t.attachedCharacter = getActorByName(split[1]);
			}
		}

		public virtual void command_fork(GameLocation location, GameTime time, string[] split)
		{
			if (split.Length > 2)
			{
				if (Game1.player.mailReceived.Contains(split[1]) || (int.TryParse(split[1], out int i) && Game1.player.dialogueQuestionsAnswered.Contains(i)))
				{
					string[] newCommands2 = eventCommands = ((split.Length <= 3) ? Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[2]].Split('/') : Game1.content.LoadString(split[2]).Split('/'));
					CurrentCommand = 0;
					forked = !forked;
				}
				else
				{
					CurrentCommand++;
				}
			}
			else if (specialEventVariable1)
			{
				string[] newCommands = eventCommands = (isFestival ? festivalData[split[1]].Split('/') : Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[1]].Split('/'));
				CurrentCommand = 0;
				forked = !forked;
			}
			else
			{
				CurrentCommand++;
			}
		}

		public virtual void command_switchEvent(GameLocation location, GameTime time, string[] split)
		{
			string[] newCommands = eventCommands = ((!isFestival) ? Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.currentLocation.Name)[split[1]].Split('/') : festivalData[split[1]].Split('/'));
			CurrentCommand = 0;
			eventSwitched = true;
		}

		public virtual void command_globalFade(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.globalFade)
			{
				if (split.Length > 2)
				{
					Game1.globalFadeToBlack(null, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
					CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToBlack(incrementCommandAfterFade, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
				}
			}
		}

		public virtual void command_globalFadeToClear(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.globalFade)
			{
				if (split.Length > 2)
				{
					Game1.globalFadeToClear(null, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
					CurrentCommand++;
				}
				else
				{
					Game1.globalFadeToClear(incrementCommandAfterFade, (split.Length > 1) ? ((float)Convert.ToDouble(split[1])) : 0.007f);
				}
			}
		}

		public virtual void command_cutscene(GameLocation location, GameTime time, string[] split)
		{
			if (currentCustomEventScript != null)
			{
				if (currentCustomEventScript.update(time, this))
				{
					currentCustomEventScript = null;
					CurrentCommand++;
				}
			}
			else
			{
				if (Game1.currentMinigame != null)
				{
					return;
				}
				switch (split[1])
				{
				case "greenTea":
					currentCustomEventScript = new EventScript_GreenTea(new Vector2(-64000f, -64000f), this);
					break;
				case "linusMoneyGone":
					foreach (TemporaryAnimatedSprite temporarySprite in location.temporarySprites)
					{
						temporarySprite.alphaFade = 0.01f;
						temporarySprite.motion = new Vector2(0f, -1f);
					}
					CurrentCommand++;
					return;
				case "marucomet":
					Game1.currentMinigame = new MaruComet();
					break;
				case "AbigailGame":
					Game1.currentMinigame = new AbigailGame(playingWithAbby: true);
					break;
				case "robot":
					Game1.currentMinigame = new RobotBlastoff();
					break;
				case "haleyCows":
					Game1.currentMinigame = new HaleyCowPictures();
					break;
				case "boardGame":
					Game1.currentMinigame = new FantasyBoardGame();
					CurrentCommand++;
					break;
				case "plane":
					Game1.currentMinigame = new PlaneFlyBy();
					break;
				case "balloonDepart":
				{
					TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(1);
					temporarySpriteByID.attachedCharacter = farmer;
					temporarySpriteByID.motion = new Vector2(0f, -2f);
					TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(2);
					temporarySpriteByID2.attachedCharacter = getActorByName("Harvey");
					temporarySpriteByID2.motion = new Vector2(0f, -2f);
					location.getTemporarySpriteByID(3).scaleChange = -0.01f;
					CurrentCommand++;
					return;
				}
				case "clearTempSprites":
					location.temporarySprites.Clear();
					CurrentCommand++;
					break;
				case "balloonChangeMap":
					eventPositionTileOffset = Vector2.Zero;
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(-23f, 0f) * 4f), flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						yStopCoordinate = (int)OffsetPositionY(576f),
						reachedStopCoordinate = balloonInSky,
						attachedCharacter = farmer,
						id = 1f
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, OffsetPosition(new Vector2(22f, 36f) * 64f + new Vector2(0f, 134f) * 4f), flicker: false, flipped: false, 0.2625f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						id = 2f,
						attachedCharacter = getActorByName("Harvey")
					});
					CurrentCommand++;
					break;
				case "bandFork":
				{
					int whichBand = 76;
					if (Game1.player.dialogueQuestionsAnswered.Contains(77))
					{
						whichBand = 77;
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains(78))
					{
						whichBand = 78;
					}
					else if (Game1.player.dialogueQuestionsAnswered.Contains(79))
					{
						whichBand = 79;
					}
					answerDialogue("bandFork", whichBand);
					CurrentCommand++;
					return;
				}
				case "eggHuntWinner":
					eggHuntWinner();
					CurrentCommand++;
					return;
				case "governorTaste":
					governorTaste();
					currentCommand++;
					return;
				case "addSecretSantaItem":
				{
					Item o = Utility.getGiftFromNPC(mySecretSanta);
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(o);
					currentCommand++;
					return;
				}
				case "iceFishingWinner":
					iceFishingWinner();
					currentCommand++;
					return;
				case "iceFishingWinnerMP":
					iceFishingWinnerMP();
					currentCommand++;
					return;
				}
				Game1.globalFadeToClear(null, 0.01f);
			}
		}

		public virtual void command_grabObject(GameLocation location, GameTime time, string[] split)
		{
			farmer.grabObject(new Object(Vector2.Zero, Convert.ToInt32(split[1]), null, canBeSetDown: false, canBeGrabbed: true, isHoedirt: false, isSpawnedObject: false));
			showActiveObject = true;
			CurrentCommand++;
		}

		public virtual void command_addTool(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("Sword"))
			{
				if (!Game1.player.addItemToInventoryBool(new Sword("Battered Sword", 67)))
				{
					Game1.player.addItemToInventoryBool(new Sword("Battered Sword", 67));
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1209")));
				}
				else
				{
					for (int i = 0; i < Game1.player.Items.Count(); i++)
					{
						if (Game1.player.Items[i] != null && Game1.player.Items[i] is Tool && Game1.player.Items[i].Name.Contains("Sword"))
						{
							Game1.player.CurrentToolIndex = i;
							Game1.switchToolAnimation();
							break;
						}
					}
				}
			}
			else if (split[1].Equals("Wand") && !Game1.player.addItemToInventoryBool(new Wand()))
			{
				Game1.player.addItemToInventoryBool(new Wand());
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1212")));
			}
			CurrentCommand++;
		}

		public virtual void command_waitForTempSprite(GameLocation location, GameTime time, string[] split)
		{
			int whichSprite = int.Parse(split[1]);
			if (Game1.currentLocation.getTemporarySpriteByID(whichSprite) != null)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_waitForKey(GameLocation location, GameTime time, string[] split)
		{
			string whichKey = split[1];
			KeyboardState currentState = Game1.GetKeyboardState();
			bool keyWasPressed = false;
			if (!farmer.UsingTool && !Game1.pickingTool)
			{
				Keys[] pressedKeys = currentState.GetPressedKeys();
				foreach (Keys i in pressedKeys)
				{
					if (Enum.GetName(i.GetType(), i).Equals(whichKey.ToUpper()))
					{
						keyWasPressed = true;
						switch (i)
						{
						case Keys.Z:
							Game1.pressSwitchToolButton();
							break;
						case Keys.C:
							Game1.pressUseToolButton();
							farmer.EndUsingTool();
							break;
						case Keys.S:
							Game1.pressAddItemToInventoryButton();
							showActiveObject = false;
							farmer.showNotCarrying();
							break;
						}
						break;
					}
				}
			}
			int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
			int lastQuoteIndex = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
			messageToScreen = eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex);
			if (keyWasPressed)
			{
				messageToScreen = null;
				CurrentCommand++;
			}
		}

		public virtual void command_cave(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Response[] responses = new Response[2]
				{
					new Response("Mushrooms", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1220")),
					new Response("Bats", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1222"))
				};
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1223"), responses, "cave");
				Game1.dialogueTyping = false;
			}
		}

		public virtual void command_updateMinigame(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.currentMinigame != null)
			{
				Game1.currentMinigame.receiveEventPoke(Convert.ToInt32(split[1]));
			}
			CurrentCommand++;
		}

		public virtual void command_startJittering(GameLocation location, GameTime time, string[] split)
		{
			farmer.jitterStrength = 1f;
			CurrentCommand++;
		}

		public virtual void command_money(GameLocation location, GameTime time, string[] split)
		{
			farmer.Money += Convert.ToInt32(split[1]);
			if (farmer.Money < 0)
			{
				farmer.Money = 0;
			}
			CurrentCommand++;
		}

		public virtual void command_stopJittering(GameLocation location, GameTime time, string[] split)
		{
			farmer.stopJittering();
			CurrentCommand++;
		}

		public virtual void command_addLantern(GameLocation location, GameTime time, string[] split)
		{
			location.TemporarySprites.Add(new TemporaryAnimatedSprite(Convert.ToInt32(split[1]), 999999f, 1, 0, OffsetPosition(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3])) * 64f), flicker: false, flipped: false)
			{
				light = true,
				lightRadius = Convert.ToInt32(split[4])
			});
			CurrentCommand++;
		}

		public virtual void command_rustyKey(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.hasRustyKey = true;
			CurrentCommand++;
		}

		public virtual void command_swimming(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmer.bathingClothes.Value = true;
				farmer.swimming.Value = true;
			}
			else
			{
				getActorByName(split[1]).swimming.Value = true;
			}
			CurrentCommand++;
		}

		public virtual void command_stopSwimming(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("farmer"))
			{
				farmer.bathingClothes.Value = (location is BathHousePool);
				farmer.swimming.Value = false;
			}
			else
			{
				getActorByName(split[1]).swimming.Value = false;
			}
			CurrentCommand++;
		}

		public virtual void command_tutorialMenu(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new TutorialMenu();
			}
		}

		public virtual void command_animalNaming(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.activeClickableMenu = new NamingMenu(delegate(string animal_name)
				{
					(Game1.currentLocation as AnimalHouse).addNewHatchedAnimal(animal_name);
					CurrentCommand++;
				}, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"));
			}
		}

		public virtual void command_splitSpeak(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.dialogueUp)
			{
				return;
			}
			timeAccumulator += time.ElapsedGameTime.Milliseconds;
			if (!(timeAccumulator < 500f))
			{
				timeAccumulator = 0f;
				int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
				int lastQuoteIndex = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
				string[] speakSplit = eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex).Split('~');
				NPC i = getActorByName(split[1]);
				if (i == null)
				{
					i = Game1.getCharacterFromName(split[1].Equals("rival") ? Utility.getOtherFarmerNames()[0] : split[1]);
				}
				if (i == null || previousAnswerChoice < 0 || previousAnswerChoice >= speakSplit.Length)
				{
					CurrentCommand++;
					return;
				}
				i.CurrentDialogue.Push(new Dialogue(speakSplit[previousAnswerChoice], i));
				Game1.drawDialogue(i);
			}
		}

		public virtual void command_catQuestion(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1241") + (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1242") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1243")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1244"), Game1.currentLocation.createYesNoResponses(), "pet");
			}
		}

		public virtual void command_ambientLight(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 4)
			{
				int r = Game1.ambientLight.R;
				int g = Game1.ambientLight.G;
				int b = Game1.ambientLight.B;
				float_useMeForAnything += time.ElapsedGameTime.Milliseconds;
				if (float_useMeForAnything > 10f)
				{
					bool success = true;
					if (r != Convert.ToInt32(split[1]))
					{
						r += Math.Sign(Convert.ToInt32(split[1]) - r);
						success = false;
					}
					if (g != Convert.ToInt32(split[2]))
					{
						g += Math.Sign(Convert.ToInt32(split[2]) - g);
						success = false;
					}
					if (b != Convert.ToInt32(split[3]))
					{
						b += Math.Sign(Convert.ToInt32(split[3]) - b);
						success = false;
					}
					float_useMeForAnything = 0f;
					Game1.ambientLight = new Color(r, g, b);
					if (success)
					{
						CurrentCommand++;
					}
				}
			}
			else
			{
				Game1.ambientLight = new Color(Convert.ToInt32(split[1]), Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
				CurrentCommand++;
			}
		}

		public virtual void command_bgColor(GameLocation location, GameTime time, string[] split)
		{
			Game1.setBGColor(Convert.ToByte(split[1]), Convert.ToByte(split[2]), Convert.ToByte(split[3]));
			CurrentCommand++;
		}

		public virtual void command_bloom(GameLocation location, GameTime time, string[] split)
		{
			Game1.bloom.Settings = new BloomSettings("eventBloom", (float)Convert.ToDouble(split[1]) / 10f, (float)Convert.ToDouble(split[2]) / 10f, (float)Convert.ToDouble(split[3]) / 10f, (float)Convert.ToDouble(split[4]) / 10f, (float)Convert.ToDouble(split[5]) / 10f, (float)Convert.ToDouble(split[6]) / 10f, split.Length > 7);
			Game1.bloom.reload();
			Game1.bloomDay = true;
			Game1.bloom.Visible = true;
			CurrentCommand++;
		}

		public virtual void command_elliottbooktalk(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.dialogueUp)
			{
				string speech2 = "";
				speech2 = (Game1.player.dialogueQuestionsAnswered.Contains(958699) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1257") : (Game1.player.dialogueQuestionsAnswered.Contains(958700) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1258") : ((!Game1.player.dialogueQuestionsAnswered.Contains(9586701)) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1260") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1259"))));
				NPC i = getActorByName("Elliott");
				if (i == null)
				{
					i = Game1.getCharacterFromName("Elliott");
				}
				i.CurrentDialogue.Push(new Dialogue(speech2, i));
				Game1.drawDialogue(i);
			}
		}

		public virtual void command_removeItem(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.removeFirstOfThisItemFromInventory(Convert.ToInt32(split[1]));
			CurrentCommand++;
		}

		public virtual void command_friendship(GameLocation location, GameTime time, string[] split)
		{
			NPC character = Game1.getCharacterFromName(split[1]);
			if (character != null)
			{
				Game1.player.changeFriendship(Convert.ToInt32(split[2]), character);
			}
			CurrentCommand++;
		}

		public virtual void command_setRunning(GameLocation location, GameTime time, string[] split)
		{
			farmer.setRunning(isRunning: true);
			CurrentCommand++;
		}

		public virtual void command_extendSourceRect(GameLocation location, GameTime time, string[] split)
		{
			if (split[2].Equals("reset"))
			{
				getActorByName(split[1]).reloadSprite();
				getActorByName(split[1]).Sprite.SpriteWidth = 16;
				getActorByName(split[1]).Sprite.SpriteHeight = 32;
				getActorByName(split[1]).HideShadow = false;
			}
			else
			{
				getActorByName(split[1]).extendSourceRect(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), split.Length <= 4);
			}
			CurrentCommand++;
		}

		public virtual void command_waitForOtherPlayers(GameLocation location, GameTime time, string[] split)
		{
			if (Game1.IsMultiplayer)
			{
				Game1.player.team.SetLocalReady(split[1], ready: true);
				if (Game1.player.team.IsReady(split[1]))
				{
					if (Game1.activeClickableMenu is ReadyCheckDialog)
					{
						Game1.exitActiveMenu();
					}
					CurrentCommand++;
				}
				else if (Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new ReadyCheckDialog(split[1], allowCancel: false);
				}
			}
			else
			{
				CurrentCommand++;
			}
		}

		public virtual void command_requestMovieEnd(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.team.requestMovieEndEvent.Fire(Game1.player.UniqueMultiplayerID);
		}

		public virtual void command_restoreStashedItem(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.TemporaryItem = null;
			CurrentCommand++;
		}

		public virtual void command_advancedMove(GameLocation location, GameTime time, string[] split)
		{
			setUpAdvancedMove(split);
			CurrentCommand++;
		}

		public virtual void command_stopRunning(GameLocation location, GameTime time, string[] split)
		{
			farmer.setRunning(isRunning: false);
			CurrentCommand++;
		}

		public virtual void command_eyes(GameLocation location, GameTime time, string[] split)
		{
			farmer.currentEyes = Convert.ToInt32(split[1]);
			farmer.blinkTimer = Convert.ToInt32(split[2]);
			CurrentCommand++;
		}

		public virtual void command_addMailReceived(GameLocation location, GameTime time, string[] split)
		{
			Game1.player.mailReceived.Add(split[1]);
			CurrentCommand++;
		}

		public virtual void command_addWorldState(GameLocation location, GameTime time, string[] split)
		{
			Game1.worldStateIDs.Add(split[1]);
			Game1.netWorldState.Value.addWorldStateID(split[1]);
			CurrentCommand++;
		}

		public virtual void command_fade(GameLocation location, GameTime time, string[] split)
		{
			if (split.Count() > 1 && split[1].Equals("unfade"))
			{
				Game1.fadeIn = false;
				Game1.fadeToBlack = false;
				CurrentCommand++;
				return;
			}
			Game1.fadeToBlack = true;
			Game1.fadeIn = true;
			if (Game1.fadeToBlackAlpha >= 0.97f)
			{
				if (split.Length == 1)
				{
					Game1.fadeIn = false;
				}
				CurrentCommand++;
			}
		}

		public virtual void command_changeMapTile(GameLocation location, GameTime time, string[] split)
		{
			string whichLayer = split[1];
			int tileX = OffsetTileX(Convert.ToInt32(split[2]));
			int tileY = OffsetTileY(Convert.ToInt32(split[3]));
			int newTileIndex = Convert.ToInt32(split[4]);
			location.map.GetLayer(whichLayer).Tiles[tileX, tileY].TileIndex = newTileIndex;
			CurrentCommand++;
		}

		public virtual void command_changeSprite(GameLocation location, GameTime time, string[] split)
		{
			getActorByName(split[1]).Sprite.LoadTexture("Characters\\" + NPC.getTextureNameForCharacter(split[1]) + "_" + split[2]);
			CurrentCommand++;
		}

		public virtual void command_waitForAllStationary(GameLocation location, GameTime time, string[] split)
		{
			bool fail = false;
			if (npcControllers != null && npcControllers.Count > 0)
			{
				fail = true;
			}
			if (!fail)
			{
				foreach (NPC actor in actors)
				{
					if (actor.isMoving())
					{
						fail = true;
						break;
					}
				}
			}
			if (!fail)
			{
				foreach (Farmer farmerActor in farmerActors)
				{
					if (farmerActor.isMoving())
					{
						fail = true;
						break;
					}
				}
			}
			if (!fail)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_proceedPosition(GameLocation location, GameTime time, string[] split)
		{
			continueAfterMove = true;
			try
			{
				Character character = getCharacterByName(split[1]);
				if (!character.isMoving() || (npcControllers != null && npcControllers.Count == 0))
				{
					character.Halt();
					CurrentCommand++;
				}
			}
			catch (Exception)
			{
				CurrentCommand++;
			}
		}

		public virtual void command_changePortrait(GameLocation location, GameTime time, string[] split)
		{
			NPC i = getActorByName(split[1]);
			if (i == null)
			{
				i = Game1.getCharacterFromName(split[1]);
			}
			i.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + split[1] + "_" + split[2]);
			i.uniquePortraitActive = true;
			npcsWithUniquePortraits.Add(i);
			CurrentCommand++;
		}

		public virtual void command_changeYSourceRectOffset(GameLocation location, GameTime time, string[] split)
		{
			NPC i = getActorByName(split[1]);
			if (i != null)
			{
				i.ySourceRectOffset = Convert.ToInt32(split[2]);
			}
			CurrentCommand++;
		}

		public virtual void command_changeName(GameLocation location, GameTime time, string[] split)
		{
			NPC i = getActorByName(split[1]);
			if (i != null)
			{
				i.displayName = split[2].Replace('_', ' ');
			}
			CurrentCommand++;
		}

		public virtual void command_playFramesAhead(GameLocation location, GameTime time, string[] split)
		{
			int framesToSkip = Convert.ToInt32(split[1]);
			CurrentCommand++;
			for (int i = 0; i < framesToSkip; i++)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_showKissFrame(GameLocation location, GameTime time, string[] split)
		{
			bool facingRight = true;
			NPC actor = getActorByName(split[1]);
			bool flip = split.Count() > 2 && Convert.ToBoolean(split[2]);
			int spouseFrame = 28;
			switch (actor.Name)
			{
			case "Maru":
				spouseFrame = 28;
				facingRight = false;
				break;
			case "Harvey":
				spouseFrame = 31;
				facingRight = false;
				break;
			case "Leah":
				spouseFrame = 25;
				facingRight = true;
				break;
			case "Elliott":
				spouseFrame = 35;
				facingRight = false;
				break;
			case "Sebastian":
				spouseFrame = 40;
				facingRight = false;
				break;
			case "Abigail":
				spouseFrame = 33;
				facingRight = false;
				break;
			case "Penny":
				spouseFrame = 35;
				facingRight = true;
				break;
			case "Alex":
				spouseFrame = 42;
				facingRight = true;
				break;
			case "Sam":
				spouseFrame = 36;
				facingRight = true;
				break;
			case "Shane":
				spouseFrame = 34;
				facingRight = false;
				break;
			case "Emily":
				spouseFrame = 33;
				facingRight = false;
				break;
			case "Krobus":
				spouseFrame = 16;
				facingRight = true;
				break;
			}
			if (flip)
			{
				facingRight = !facingRight;
			}
			actor.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(spouseFrame, 1000, secondaryArm: false, facingRight)
			});
			CurrentCommand++;
		}

		public virtual void command_addTemporaryActor(GameLocation location, GameTime time, string[] split)
		{
			string textureLocation = "Characters\\";
			bool has_valid_type_key = true;
			if (split.Length > 8 && split[8].ToLower().Equals("animal"))
			{
				textureLocation = "Animals\\";
			}
			else if (split.Length > 8 && split[8].ToLower().Equals("monster"))
			{
				textureLocation = "Characters\\Monsters\\";
			}
			else if (split.Length <= 8 || !split[8].ToLower().Equals("character"))
			{
				has_valid_type_key = false;
			}
			NPC i = new NPC(new AnimatedSprite(festivalContent, textureLocation + split[1].Replace('_', ' '), 0, Convert.ToInt32(split[2]), Convert.ToInt32(split[3])), OffsetPosition(new Vector2(Convert.ToInt32(split[4]), Convert.ToInt32(split[5])) * 64f), Convert.ToInt32(split[6]), split[1].Replace('_', ' '), festivalContent);
			if (split.Length > 7)
			{
				i.Breather = Convert.ToBoolean(split[7]);
			}
			if (!has_valid_type_key && split.Length > 8)
			{
				i.displayName = split[8].Replace('_', ' ');
			}
			if (isFestival)
			{
				try
				{
					i.CurrentDialogue.Push(new Dialogue(festivalData[i.Name], i));
				}
				catch (Exception)
				{
				}
			}
			if (textureLocation.Contains("Animals") && split.Length > 9)
			{
				i.Name = split[9];
			}
			if (i.Sprite.SpriteWidth >= 32)
			{
				i.HideShadow = true;
			}
			i.eventActor = true;
			actors.Add(i);
			CurrentCommand++;
		}

		public virtual void command_changeToTemporaryMap(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("Town"))
			{
				temporaryLocation = new Town("Maps\\" + split[1], "Temp");
			}
			else
			{
				temporaryLocation = new GameLocation("Maps\\" + split[1], "Temp");
			}
			temporaryLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
			Event e = Game1.currentLocation.currentEvent;
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.currentLocation.currentEvent = null;
			Game1.currentLightSources.Clear();
			Game1.currentLocation = temporaryLocation;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.currentLocation.currentEvent = e;
			CurrentCommand++;
			Game1.player.currentLocation = Game1.currentLocation;
			farmer.currentLocation = Game1.currentLocation;
			if (split.Length < 3)
			{
				Game1.panScreen(0, 0);
			}
		}

		public virtual void command_positionOffset(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.position.X += Convert.ToInt32(split[2]);
					f.position.Y += Convert.ToInt32(split[3]);
				}
			}
			else
			{
				NPC i = getActorByName(split[1]);
				if (i != null)
				{
					i.position.X += Convert.ToInt32(split[2]);
					i.position.Y += Convert.ToInt32(split[3]);
				}
			}
			CurrentCommand++;
			if (split.Length > 4)
			{
				checkForNextCommand(location, time);
			}
		}

		public virtual void command_question(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string[] questionAndAnswersSplit = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)].Split('"')[1].Split('#');
				string question = questionAndAnswersSplit[0];
				Response[] answers = new Response[questionAndAnswersSplit.Length - 1];
				for (int i = 1; i < questionAndAnswersSplit.Length; i++)
				{
					answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswersSplit[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, answers, split[1]);
			}
		}

		public virtual void command_quickQuestion(GameLocation location, GameTime time, string[] split)
		{
			if (!Game1.isQuestion && Game1.activeClickableMenu == null)
			{
				string obj = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)];
				string[] questionAndAnswerSplit = obj.Substring(obj.IndexOf(' ') + 1).Split(new string[1]
				{
					"(break)"
				}, StringSplitOptions.None)[0].Split('#');
				string question = questionAndAnswerSplit[0];
				Response[] answers = new Response[questionAndAnswerSplit.Length - 1];
				for (int i = 1; i < questionAndAnswerSplit.Length; i++)
				{
					answers[i - 1] = new Response((i - 1).ToString(), questionAndAnswerSplit[i]);
				}
				Game1.currentLocation.createQuestionDialogue(question, answers, "quickQuestion");
			}
		}

		public virtual void command_drawOffset(GameLocation location, GameTime time, string[] split)
		{
			int x_offset = Convert.ToInt32(split[2]);
			float y_offset = Convert.ToInt32(split[3]);
			Character character2 = null;
			character2 = (Character)((!split[1].Equals("farmer")) ? ((object)getActorByName(split[1])) : ((object)farmer));
			character2.drawOffset.Value = new Vector2(x_offset, y_offset) * 4f;
			CurrentCommand++;
		}

		public virtual void command_hideShadow(GameLocation location, GameTime time, string[] split)
		{
			bool hide_shadow = split[2].Equals("true");
			getActorByName(split[1]).HideShadow = hide_shadow;
			CurrentCommand++;
		}

		public virtual void command_animateHeight(GameLocation location, GameTime time, string[] split)
		{
			int? height = null;
			float? jump_gravity = null;
			float? jump_velocity = null;
			if (split[2] != "keep")
			{
				height = Convert.ToInt32(split[2]);
			}
			if (split[3] != "keep")
			{
				jump_gravity = (float)Convert.ToDouble(split[3]);
			}
			if (split[4] != "keep")
			{
				jump_velocity = Convert.ToInt32(split[4]);
			}
			Character character2 = null;
			character2 = (Character)((!split[1].Equals("farmer")) ? ((object)getActorByName(split[1])) : ((object)farmer));
			if (height.HasValue)
			{
				character2.yJumpOffset = -height.Value;
			}
			if (jump_gravity.HasValue)
			{
				character2.yJumpGravity = jump_gravity.Value;
			}
			if (jump_velocity.HasValue)
			{
				character2.yJumpVelocity = jump_velocity.Value;
			}
			CurrentCommand++;
		}

		public virtual void command_jump(GameLocation location, GameTime time, string[] split)
		{
			float jumpV = (split.Length > 2) ? ((float)Convert.ToDouble(split[2])) : 8f;
			if (split[1].Equals("farmer"))
			{
				farmer.jump(jumpV);
			}
			else
			{
				getActorByName(split[1]).jump(jumpV);
			}
			CurrentCommand++;
			checkForNextCommand(location, time);
		}

		public virtual void command_farmerEat(GameLocation location, GameTime time, string[] split)
		{
			Object toEat = new Object(Convert.ToInt32(split[1]), 1);
			farmer.eatObject(toEat, overrideFullness: true);
			CurrentCommand++;
		}

		public virtual void command_spriteText(GameLocation location, GameTime time, string[] split)
		{
			int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
			int lastQuoteIndex = eventCommands[CurrentCommand].LastIndexOf('"');
			int_useMeForAnything2 = Convert.ToInt32(split[1]);
			if (lastQuoteIndex <= 0 || lastQuoteIndex <= firstQuoteIndex)
			{
				return;
			}
			string text = eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex - firstQuoteIndex);
			float_useMeForAnything += time.ElapsedGameTime.Milliseconds;
			if (float_useMeForAnything > 80f)
			{
				if (int_useMeForAnything >= text.Length - 1)
				{
					if (float_useMeForAnything >= 2500f)
					{
						int_useMeForAnything = 0;
						float_useMeForAnything = 0f;
						spriteTextToDraw = "";
						CurrentCommand++;
					}
				}
				else
				{
					int_useMeForAnything++;
					float_useMeForAnything = 0f;
					Game1.playSound("dialogueCharacter");
				}
			}
			spriteTextToDraw = text;
		}

		public virtual void command_ignoreCollisions(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				if (f != null)
				{
					f.ignoreCollisions = true;
				}
			}
			else
			{
				NPC i = getActorByName(split[1]);
				if (i != null)
				{
					i.isCharging = true;
				}
			}
			CurrentCommand++;
		}

		public virtual void command_screenFlash(GameLocation location, GameTime time, string[] split)
		{
			Game1.flashAlpha = (float)Convert.ToDouble(split[1]);
			CurrentCommand++;
		}

		public virtual void command_grandpaCandles(GameLocation location, GameTime time, string[] split)
		{
			int candles = Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore());
			Game1.getFarm().grandpaScore.Value = candles;
			for (int i = 0; i < candles; i++)
			{
				DelayedAction.playSoundAfterDelay("fireball", 100 * i);
			}
			Game1.getFarm().addGrandpaCandles();
			CurrentCommand++;
		}

		public virtual void command_grandpaEvaluation2(GameLocation location, GameTime time, string[] split)
		{
			switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
			{
			case 1:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1306") + "\"";
				break;
			case 2:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1307") + "\"";
				break;
			case 3:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1308") + "\"";
				break;
			case 4:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1309") + "\"";
				break;
			}
			Game1.player.eventsSeen.Remove(2146991);
		}

		public virtual void command_grandpaEvaluation(GameLocation location, GameTime time, string[] split)
		{
			switch (Utility.getGrandpaCandlesFromScore(Utility.getGrandpaScore()))
			{
			case 1:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1315") + "\"";
				break;
			case 2:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1316") + "\"";
				break;
			case 3:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1317") + "\"";
				break;
			case 4:
				eventCommands[currentCommand] = "speak Grandpa \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1318") + "\"";
				break;
			}
		}

		public virtual void command_loadActors(GameLocation location, GameTime time, string[] split)
		{
			if (temporaryLocation != null && temporaryLocation.map.GetLayer(split[1]) != null)
			{
				actors.Clear();
				if (npcControllers != null)
				{
					npcControllers.Clear();
				}
				Dictionary<string, string> NPCData = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				List<string> npc_names = new List<string>();
				for (int x2 = 0; x2 < temporaryLocation.map.GetLayer(split[1]).LayerWidth; x2++)
				{
					for (int y = 0; y < temporaryLocation.map.GetLayer(split[1]).LayerHeight; y++)
					{
						if (temporaryLocation.map.GetLayer(split[1]).Tiles[x2, y] != null)
						{
							int actorIndex = temporaryLocation.map.GetLayer(split[1]).Tiles[x2, y].TileIndex / 4;
							int actorFacingDirection = temporaryLocation.map.GetLayer(split[1]).Tiles[x2, y].TileIndex % 4;
							string actorName = NPCData.ElementAt(actorIndex).Key;
							if (actorName != null && Game1.getCharacterFromName(actorName) != null && (!(actorName == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved")))
							{
								addActor(actorName, x2, y, actorFacingDirection, temporaryLocation);
								npc_names.Add(actorName);
							}
						}
					}
				}
				if (festivalData != null)
				{
					string key_name = split[1] + "_additionalCharacters";
					if (festivalData.ContainsKey(key_name))
					{
						string[] array = festivalData[key_name].Split('/');
						foreach (string additional_character_data in array)
						{
							if (string.IsNullOrEmpty(additional_character_data))
							{
								continue;
							}
							string[] additional_character_split = additional_character_data.Split(' ');
							if (additional_character_split.Length < 4)
							{
								continue;
							}
							bool fail = false;
							int x = 0;
							int y2 = 0;
							int direction = 2;
							if (!fail && !int.TryParse(additional_character_split[1], out x))
							{
								fail = true;
							}
							if (!fail && !int.TryParse(additional_character_split[2], out y2))
							{
								fail = true;
							}
							if (!fail)
							{
								string direction_string2 = additional_character_split[3];
								direction_string2 = direction_string2.ToLowerInvariant();
								if (direction_string2 == "up")
								{
									direction = 0;
								}
								else if (direction_string2 == "down")
								{
									direction = 2;
								}
								else if (direction_string2 == "left")
								{
									direction = 3;
								}
								else if (direction_string2 == "right")
								{
									direction = 1;
								}
								else if (!int.TryParse(direction_string2, out direction))
								{
									fail = true;
								}
							}
							if (fail)
							{
								Console.WriteLine("Warning: Failed to load additional festival character: " + additional_character_data);
								continue;
							}
							string actor_name = additional_character_split[0];
							if (actor_name != null && Game1.getCharacterFromName(actor_name) != null)
							{
								if (!(actor_name == "Leo") || Game1.MasterPlayer.mailReceived.Contains("leoMoved"))
								{
									addActor(actor_name, x, y2, direction, temporaryLocation);
									npc_names.Add(actor_name);
								}
							}
							else
							{
								Console.WriteLine("Warning: Invalid additional festival character name: " + actor_name);
							}
						}
					}
				}
				if (split[1] == "Set-Up")
				{
					foreach (string npc_name in npc_names)
					{
						NPC npc2 = Game1.getCharacterFromName(npc_name);
						if (npc2.isMarried() && npc2.getSpouse() != null && npc2.getSpouse().getChildren().Count > 0)
						{
							Farmer spouse = Game1.player;
							if (npc2.getSpouse() != null)
							{
								spouse = npc2.getSpouse();
							}
							List<Child> children = spouse.getChildren();
							npc2 = (getCharacterByName(npc_name) as NPC);
							for (int child_index = 0; child_index < children.Count; child_index++)
							{
								Child child = children[child_index];
								if (child.Age >= 3)
								{
									Child child_actor = new Child(child.Name, child.Gender == 0, child.darkSkinned, spouse);
									child_actor.NetFields.CopyFrom(child.NetFields);
									child_actor.Halt();
									new Point((int)npc2.Position.X / 64, (int)npc2.Position.Y / 64);
									Point[] direction_offsets2 = null;
									switch (npc2.FacingDirection)
									{
									case 0:
										direction_offsets2 = new Point[4]
										{
											new Point(0, 1),
											new Point(-1, 0),
											new Point(1, 0),
											new Point(0, -1)
										};
										break;
									case 2:
										direction_offsets2 = new Point[4]
										{
											new Point(0, -1),
											new Point(1, 0),
											new Point(-1, 0),
											new Point(0, 1)
										};
										break;
									case 3:
										direction_offsets2 = new Point[4]
										{
											new Point(1, 0),
											new Point(0, -1),
											new Point(0, 1),
											new Point(-1, 0)
										};
										break;
									case 1:
										direction_offsets2 = new Point[4]
										{
											new Point(-1, 0),
											new Point(0, 1),
											new Point(0, -1),
											new Point(1, 0)
										};
										break;
									default:
										direction_offsets2 = new Point[4]
										{
											new Point(-1, 0),
											new Point(1, 0),
											new Point(0, -1),
											new Point(0, 1)
										};
										break;
									}
									Point spawn_point = new Point(npc2.getTileX(), npc2.getTileY());
									List<Point> points_to_check = new List<Point>();
									new List<Point>();
									Point[] array2 = direction_offsets2;
									for (int j = 0; j < array2.Length; j++)
									{
										Point offset = array2[j];
										points_to_check.Add(new Point(spawn_point.X + offset.X, spawn_point.Y + offset.Y));
									}
									Func<Point, bool> is_walkable_tile_check = (Point point) => temporaryLocation.isTilePassable(new Location(point.X, point.Y), Game1.viewport) ? true : false;
									Func<Point, bool> has_clearance_check = delegate(Point point)
									{
										int num = 1;
										for (int k = point.X - num; k <= point.X + num; k++)
										{
											for (int l = point.Y - num; l <= point.Y + num; l++)
											{
												if (temporaryLocation.isTileOccupiedForPlacement(new Vector2(k, l)))
												{
													return false;
												}
												foreach (NPC current in actors)
												{
													if (!(current is Child) && current.getTileX() == k && current.getTileY() == l)
													{
														return false;
													}
												}
											}
										}
										return true;
									};
									bool found_spawn = false;
									for (int iteration = 0; iteration < 5; iteration++)
									{
										if (found_spawn)
										{
											break;
										}
										int current_check_count = points_to_check.Count;
										for (int i = 0; i < current_check_count; i++)
										{
											Point current_point = points_to_check[0];
											points_to_check.RemoveAt(0);
											if (is_walkable_tile_check(current_point))
											{
												if (has_clearance_check(current_point))
												{
													found_spawn = true;
													spawn_point = current_point;
													break;
												}
												array2 = direction_offsets2;
												for (int j = 0; j < array2.Length; j++)
												{
													Point offset2 = array2[j];
													points_to_check.Add(new Point(current_point.X + offset2.X, current_point.Y + offset2.Y));
												}
											}
										}
									}
									if (found_spawn)
									{
										child_actor.setTilePosition(spawn_point.X, spawn_point.Y);
										child_actor.DefaultPosition = npc2.DefaultPosition;
										child_actor.faceDirection(npc2.FacingDirection);
										child_actor.eventActor = true;
										child_actor.lastCrossroad = new Microsoft.Xna.Framework.Rectangle(spawn_point.X * 64, spawn_point.Y * 64, 64, 64);
										child_actor.squareMovementFacingPreference = -1;
										child_actor.walkInSquare(3, 3, 2000);
										child_actor.controller = null;
										child_actor.temporaryController = null;
										actors.Add(child_actor);
									}
								}
							}
						}
					}
				}
			}
			CurrentCommand++;
		}

		public virtual void command_playerControl(GameLocation location, GameTime time, string[] split)
		{
			if (!playerControlSequence)
			{
				setUpPlayerControlSequence(split[1]);
			}
		}

		public virtual void command_removeSprite(GameLocation location, GameTime time, string[] split)
		{
			Vector2 tile = OffsetPosition(new Vector2(Convert.ToInt32(split[1]), Convert.ToInt32(split[2])) * 64f);
			for (int i = Game1.currentLocation.temporarySprites.Count - 1; i >= 0; i--)
			{
				if (Game1.currentLocation.temporarySprites[i].position.Equals(tile))
				{
					Game1.currentLocation.temporarySprites.RemoveAt(i);
				}
			}
			CurrentCommand++;
		}

		public virtual void command_viewport(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("move"))
			{
				viewportTarget = new Vector3(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]), Convert.ToInt32(split[4]));
			}
			else
			{
				if (aboveMapSprites != null && Convert.ToInt32(split[1]) < 0)
				{
					aboveMapSprites.Clear();
					aboveMapSprites = null;
				}
				Game1.viewportFreeze = true;
				int target_tile_x = OffsetTileX(Convert.ToInt32(split[1]));
				int target_tile_y = OffsetTileY(Convert.ToInt32(split[2]));
				if (id == 2146991)
				{
					Point grandpaShrinePosition = Game1.getFarm().GetGrandpaShrinePosition();
					target_tile_x = grandpaShrinePosition.X;
					target_tile_y = grandpaShrinePosition.Y;
				}
				Game1.viewport.X = target_tile_x * 64 + 32 - Game1.viewport.Width / 2;
				Game1.viewport.Y = target_tile_y * 64 + 32 - Game1.viewport.Height / 2;
				if (Game1.viewport.X > 0 && Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
				{
					Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
				}
				if (Game1.viewport.Y > 0 && Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
				{
					Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
				}
				if (split.Length > 3 && split[3].Equals("true"))
				{
					Game1.fadeScreenToBlack();
					Game1.fadeToBlackAlpha = 1f;
					Game1.nonWarpFade = true;
				}
				else if (split.Length > 3 && split[3].Equals("clamp"))
				{
					if (Game1.currentLocation.map.DisplayWidth >= Game1.viewport.Width)
					{
						if (Game1.viewport.X + Game1.viewport.Width > Game1.currentLocation.Map.DisplayWidth)
						{
							Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
						}
						if (Game1.viewport.X < 0)
						{
							Game1.viewport.X = 0;
						}
					}
					else
					{
						Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth / 2 - Game1.viewport.Width / 2;
					}
					if (Game1.currentLocation.map.DisplayHeight >= Game1.viewport.Height)
					{
						if (Game1.viewport.Y + Game1.viewport.Height > Game1.currentLocation.Map.DisplayHeight)
						{
							Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height;
						}
					}
					else
					{
						Game1.viewport.Y = Game1.currentLocation.Map.DisplayHeight / 2 - Game1.viewport.Height / 2;
					}
					if (Game1.viewport.Y < 0)
					{
						Game1.viewport.Y = 0;
					}
					if (split.Length > 4 && split[4].Equals("true"))
					{
						Game1.fadeScreenToBlack();
						Game1.fadeToBlackAlpha = 1f;
						Game1.nonWarpFade = true;
					}
				}
				if (split.Length > 4 && split[4].Equals("unfreeze"))
				{
					Game1.viewportFreeze = false;
				}
				if (Game1.gameMode == 2)
				{
					Game1.viewport.X = Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width;
				}
			}
			CurrentCommand++;
		}

		public virtual void command_broadcastEvent(GameLocation location, GameTime time, string[] split)
		{
			if (farmer == Game1.player)
			{
				bool use_local_farmer = false;
				if (split.Length > 1 && split[1] == "local")
				{
					use_local_farmer = true;
				}
				if (id == 558291 || id == 558292)
				{
					use_local_farmer = true;
				}
				Game1.multiplayer.broadcastEvent(this, Game1.currentLocation, Game1.player.positionBeforeEvent, use_local_farmer);
			}
			CurrentCommand++;
		}

		public virtual void command_addConversationTopic(GameLocation location, GameTime time, string[] split)
		{
			if (isMemory)
			{
				CurrentCommand++;
				return;
			}
			if (!Game1.player.activeDialogueEvents.ContainsKey(split[1]))
			{
				Game1.player.activeDialogueEvents.Add(split[1], (split.Count() > 2) ? Convert.ToInt32(split[2]) : 4);
			}
			CurrentCommand++;
		}

		public virtual void command_dump(GameLocation location, GameTime time, string[] split)
		{
			if (split[1].Equals("girls"))
			{
				Game1.player.activeDialogueEvents.Add("dumped_Girls", 7);
				Game1.player.activeDialogueEvents.Add("secondChance_Girls", 14);
			}
			else
			{
				Game1.player.activeDialogueEvents.Add("dumped_Guys", 7);
				Game1.player.activeDialogueEvents.Add("secondChance_Guys", 14);
			}
			CurrentCommand++;
		}

		public Event(string eventString, int eventID = -1, Farmer farmerActor = null)
			: this()
		{
			id = eventID;
			eventCommands = eventString.Split('/');
			actorPositionsAfterMove = new Dictionary<string, Vector3>();
			previousAmbientLight = Game1.ambientLight;
			wasBloomDay = Game1.bloomDay;
			wasBloomVisible = (Game1.bloom != null && Game1.bloom.Visible);
			if (wasBloomDay)
			{
				previousBloomSettings = Game1.bloom.Settings;
			}
			if (farmerActor != null)
			{
				farmerActors.Add(farmerActor);
			}
			farmer.canOnlyWalk = true;
			farmer.showNotCarrying();
			drawTool = false;
			if (eventID == -2)
			{
				isWedding = true;
			}
		}

		public Event()
		{
			setupEventCommands();
		}

		public bool tryToLoadFestival(string festival)
		{
			if (invalidFestivals.Contains(festival))
			{
				return false;
			}
			Game1.player.festivalScore = 0;
			try
			{
				festivalData = festivalContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + festival);
				festivalData["file"] = festival;
			}
			catch (Exception)
			{
				invalidFestivals.Add(festival);
				return false;
			}
			string locationName = festivalData["conditions"].Split('/')[0];
			int startTime = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[0]);
			int endTime = Convert.ToInt32(festivalData["conditions"].Split('/')[1].Split(' ')[1]);
			if (!locationName.Equals(Game1.currentLocation.Name) || Game1.timeOfDay < startTime || Game1.timeOfDay >= endTime)
			{
				return false;
			}
			int year_count;
			for (year_count = 1; festivalData.ContainsKey("set-up_y" + year_count + 1); year_count++)
			{
			}
			int setup_variant = Game1.year % year_count;
			if (setup_variant == 0)
			{
				setup_variant = year_count;
			}
			eventCommands = festivalData["set-up"].Split('/');
			if (setup_variant > 1)
			{
				List<string> event_commands = new List<string>(eventCommands);
				event_commands.AddRange(festivalData["set-up_y" + setup_variant].Split('/'));
				eventCommands = event_commands.ToArray();
			}
			actorPositionsAfterMove = new Dictionary<string, Vector3>();
			previousAmbientLight = Game1.ambientLight;
			_ = wasBloomDay;
			isFestival = true;
			Game1.setRichPresence("festival", festival);
			return true;
		}

		public string GetFestivalDataForYear(string key)
		{
			int years;
			for (years = 1; festivalData.ContainsKey(key + "_y" + (years + 1)); years++)
			{
			}
			int selected_year = Game1.year % years;
			if (selected_year == 0)
			{
				selected_year = years;
			}
			if (selected_year > 1)
			{
				return festivalData[key + "_y" + selected_year];
			}
			return festivalData[key];
		}

		public void setExitLocation(string location, int x, int y)
		{
			if (Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == "")
			{
				exitLocation = Game1.getLocationRequest(location);
				Game1.player.positionBeforeEvent = new Vector2(x, y);
			}
		}

		public void endBehaviors(string[] split, GameLocation location)
		{
			if (Game1.getMusicTrackName().Contains(Game1.currentSeason) && !eventCommands[0].Equals("continue"))
			{
				Game1.stopMusicTrack(Game1.MusicContext.Default);
			}
			if (split != null && split.Length > 1)
			{
				switch (split[1])
				{
				case "Leo":
					if (!isMemory)
					{
						Game1.addMailForTomorrow("leoMoved", noLetter: true, sendToEveryone: true);
						Game1.player.team.requestLeoMove.Fire();
					}
					break;
				case "bed":
					Game1.player.Position = Game1.player.mostRecentBed + new Vector2(0f, 64f);
					break;
				case "newDay":
					Game1.player.faceDirection(2);
					setExitLocation(Game1.player.homeLocation, (int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
					if (!Game1.IsMultiplayer)
					{
						exitLocation.OnWarp += delegate
						{
							Game1.NewDay(0f);
							Game1.player.currentLocation.lastTouchActionLocation = new Vector2((int)Game1.player.mostRecentBed.X / 64, (int)Game1.player.mostRecentBed.Y / 64);
						};
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					if ((bool)Game1.player.bathingClothes)
					{
						Game1.player.changeOutOfSwimSuit();
					}
					Game1.player.swimming.Value = false;
					Game1.player.CanMove = false;
					Game1.changeMusicTrack("none");
					break;
				case "busIntro":
					Game1.currentMinigame = new Intro(4);
					break;
				case "invisibleWarpOut":
					Game1.getCharacterFromName(split[2]).IsInvisible = true;
					setExitLocation(location.warps[0].TargetName, location.warps[0].TargetX, location.warps[0].TargetY);
					Game1.fadeScreenToBlack();
					Game1.eventOver = true;
					CurrentCommand += 2;
					Game1.screenGlowHold = false;
					break;
				case "invisible":
					if (!isMemory)
					{
						Game1.getCharacterFromName(split[2]).IsInvisible = true;
					}
					break;
				case "warpOut":
				{
					int whichWarp2 = 0;
					if (location is BathHousePool && Game1.player.IsMale)
					{
						whichWarp2 = 1;
					}
					setExitLocation(location.warps[whichWarp2].TargetName, location.warps[whichWarp2].TargetX, location.warps[whichWarp2].TargetY);
					Game1.eventOver = true;
					CurrentCommand += 2;
					Game1.screenGlowHold = false;
					break;
				}
				case "dialogueWarpOut":
				{
					int whichWarp2 = 0;
					if (location is BathHousePool && Game1.player.IsMale)
					{
						whichWarp2 = 1;
					}
					setExitLocation(location.warps[whichWarp2].TargetName, location.warps[whichWarp2].TargetX, location.warps[whichWarp2].TargetY);
					NPC i = Game1.getCharacterFromName(split[2]);
					int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
					int lastQuoteIndex = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
					i.CurrentDialogue.Clear();
					i.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex), i));
					Game1.eventOver = true;
					CurrentCommand += 2;
					Game1.screenGlowHold = false;
					break;
				}
				case "Maru1":
					Game1.getCharacterFromName("Demetrius").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1018"));
					Game1.getCharacterFromName("Maru").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1020"));
					setExitLocation(location.warps[0].TargetName, location.warps[0].TargetX, location.warps[0].TargetY);
					Game1.fadeScreenToBlack();
					Game1.eventOver = true;
					CurrentCommand += 2;
					break;
				case "wedding":
				{
					if (farmer.IsMale)
					{
						farmer.changeShirt(-1);
						farmer.changePants(oldPants);
						farmer.changePantStyle(-1);
						Game1.getCharacterFromName("Lewis").CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1025"), Game1.getCharacterFromName("Lewis")));
					}
					FarmHouse homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
					Point porch = homeOfFarmer.getPorchStandingSpot();
					if (homeOfFarmer is Cabin)
					{
						setExitLocation("Farm", porch.X + 1, porch.Y);
					}
					else
					{
						setExitLocation("Farm", porch.X - 1, porch.Y);
					}
					if (!Game1.IsMasterGame)
					{
						break;
					}
					NPC spouse = Game1.getCharacterFromName(farmer.spouse);
					if (spouse != null)
					{
						spouse.Schedule = null;
						spouse.ignoreScheduleToday = true;
						spouse.shouldPlaySpousePatioAnimation.Value = false;
						spouse.controller = null;
						spouse.temporaryController = null;
						spouse.currentMarriageDialogue.Clear();
						Game1.warpCharacter(spouse, "Farm", Utility.getHomeOfFarmer(farmer).getPorchStandingSpot());
						spouse.faceDirection(2);
						if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\StringsFromCSFiles:" + spouse.Name + "_AfterWedding") != null)
						{
							spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", spouse.Name + "_AfterWedding", false);
						}
						else
						{
							spouse.addMarriageDialogue("Strings\\StringsFromCSFiles", "Game1.cs.2782", false);
						}
					}
					break;
				}
				case "dialogue":
				{
					NPC i = Game1.getCharacterFromName(split[2]);
					int firstQuoteIndex = eventCommands[CurrentCommand].IndexOf('"') + 1;
					int lastQuoteIndex = eventCommands[CurrentCommand].Substring(eventCommands[CurrentCommand].IndexOf('"') + 1).IndexOf('"');
					if (i != null)
					{
						i.shouldSayMarriageDialogue.Value = false;
						i.currentMarriageDialogue.Clear();
						i.CurrentDialogue.Clear();
						i.CurrentDialogue.Push(new Dialogue(eventCommands[CurrentCommand].Substring(firstQuoteIndex, lastQuoteIndex), i));
					}
					break;
				}
				case "beginGame":
					Game1.gameMode = 3;
					setExitLocation("FarmHouse", 9, 9);
					Game1.NewDay(1000f);
					exitEvent();
					Game1.eventFinished();
					return;
				case "credits":
					Game1.debrisWeather.Clear();
					Game1.isDebrisWeather = false;
					Game1.changeMusicTrack("wedding", track_interruptable: false, Game1.MusicContext.Event);
					Game1.gameMode = 10;
					CurrentCommand += 2;
					break;
				case "position":
					if (Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == "")
					{
						Game1.player.positionBeforeEvent = new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]));
					}
					break;
				case "islandDepart":
				{
					Game1.player.orientationBeforeEvent = 2;
					if (Game1.whereIsTodaysFest != null && Game1.whereIsTodaysFest == "Beach")
					{
						Game1.player.orientationBeforeEvent = 0;
						setExitLocation("Town", 54, 109);
					}
					else if (Game1.whereIsTodaysFest != null && Game1.whereIsTodaysFest == "Town")
					{
						Game1.player.orientationBeforeEvent = 3;
						setExitLocation("BusStop", 33, 23);
					}
					else
					{
						setExitLocation("BoatTunnel", 6, 9);
					}
					GameLocation left_location = Game1.currentLocation;
					exitLocation.OnLoad += delegate
					{
						foreach (NPC actor in actors)
						{
							actor.shouldShadowBeOffset = true;
							actor.drawOffset.Y = 0f;
						}
						foreach (Farmer farmerActor in farmerActors)
						{
							farmerActor.shouldShadowBeOffset = true;
							farmerActor.drawOffset.Y = 0f;
						}
						Game1.player.drawOffset.Value = Vector2.Zero;
						Game1.player.shouldShadowBeOffset = false;
						if (left_location is IslandSouth)
						{
							(left_location as IslandSouth).ResetBoat();
						}
					};
					break;
				}
				case "tunnelDepart":
					if (Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
					{
						Game1.warpFarmer("IslandSouth", 21, 43, 0);
					}
					break;
				}
			}
			exitEvent();
		}

		public void exitEvent()
		{
			if (id != -1 && !Game1.player.eventsSeen.Contains(id))
			{
				Game1.player.eventsSeen.Add(id);
			}
			if (id == 1039573)
			{
				Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			}
			Game1.stopMusicTrack(Game1.MusicContext.Event);
			Game1.player.ignoreCollisions = false;
			Game1.player.canOnlyWalk = false;
			Game1.nonWarpFade = true;
			if (!Game1.fadeIn || Game1.fadeToBlackAlpha >= 1f)
			{
				Game1.fadeScreenToBlack();
			}
			Game1.eventOver = true;
			Game1.fadeToBlack = true;
			Game1.setBGColor(5, 3, 4);
			CurrentCommand += 2;
			Game1.screenGlowHold = false;
			if (isFestival)
			{
				Game1.timeOfDayAfterFade = 2200;
				_ = festivalData["file"];
				if (festivalData != null && (festivalData["file"].Equals("summer28") || festivalData["file"].Equals("fall27")))
				{
					Game1.timeOfDayAfterFade = 2400;
				}
				int timePass = Utility.CalculateMinutesBetweenTimes(Game1.timeOfDay, Game1.timeOfDayAfterFade);
				if (Game1.IsMasterGame)
				{
					Point house_entry = Game1.getFarm().GetMainFarmHouseEntry();
					setExitLocation("Farm", house_entry.X, house_entry.Y);
				}
				else
				{
					Point porchSpot = Utility.getHomeOfFarmer(Game1.player).getPorchStandingSpot();
					setExitLocation("Farm", porchSpot.X, porchSpot.Y);
				}
				Game1.player.toolOverrideFunction = null;
				isFestival = false;
				foreach (NPC k in actors)
				{
					if (k != null)
					{
						resetDialogueIfNecessary(k);
					}
				}
				if (Game1.IsMasterGame)
				{
					foreach (NPC j in Utility.getAllCharacters())
					{
						if (j.isVillager())
						{
							if (j.getSpouse() != null)
							{
								Farmer spouse_farmer = j.getSpouse();
								if (spouse_farmer.isMarried())
								{
									j.controller = null;
									j.temporaryController = null;
									FarmHouse home_location = Utility.getHomeOfFarmer(spouse_farmer);
									j.Halt();
									Game1.warpCharacter(j, home_location, Utility.PointToVector2(home_location.getSpouseBedSpot(spouse_farmer.spouse)));
									if (home_location.GetSpouseBed() != null)
									{
										FarmHouse.spouseSleepEndFunction(j, Utility.getHomeOfFarmer(spouse_farmer));
									}
									j.ignoreScheduleToday = true;
									if (Game1.timeOfDayAfterFade >= 1800)
									{
										j.currentMarriageDialogue.Clear();
										j.checkForMarriageDialogue(1800, Utility.getHomeOfFarmer(spouse_farmer));
									}
									else if (Game1.timeOfDayAfterFade >= 1100)
									{
										j.currentMarriageDialogue.Clear();
										j.checkForMarriageDialogue(1100, Utility.getHomeOfFarmer(spouse_farmer));
									}
									continue;
								}
							}
							if (j.currentLocation != null && j.defaultMap.Value != null)
							{
								j.doingEndOfRouteAnimation.Value = false;
								j.nextEndOfRouteMessage = null;
								j.endOfRouteMessage.Value = null;
								j.controller = null;
								j.temporaryController = null;
								j.Halt();
								Game1.warpCharacter(j, j.defaultMap, j.DefaultPosition / 64f);
								j.ignoreScheduleToday = true;
							}
						}
					}
				}
				foreach (GameLocation i in Game1.locations)
				{
					foreach (Vector2 position in new List<Vector2>(i.objects.Keys))
					{
						if (i.objects[position].minutesElapsed(timePass, i))
						{
							i.objects.Remove(position);
						}
					}
					if (i is Farm)
					{
						(i as Farm).timeUpdate(timePass);
					}
				}
				Game1.player.freezePause = 1500;
			}
			else
			{
				Game1.player.forceCanMove();
			}
		}

		public void resetDialogueIfNecessary(NPC n)
		{
			if (!Game1.player.hasTalkedToFriendToday(n.Name))
			{
				n.resetCurrentDialogue();
			}
			else if (n.CurrentDialogue != null)
			{
				n.CurrentDialogue.Clear();
			}
		}

		public void incrementCommandAfterFade()
		{
			CurrentCommand++;
			Game1.globalFade = false;
		}

		public void cleanup()
		{
			Game1.ambientLight = previousAmbientLight;
			if (Game1.bloom != null)
			{
				Game1.bloom.Settings = previousBloomSettings;
				Game1.bloom.Visible = wasBloomVisible;
				Game1.bloom.reload();
			}
			foreach (NPC i in npcsWithUniquePortraits)
			{
				i.Portrait = Game1.content.Load<Texture2D>("Portraits\\" + i.Name);
				i.uniquePortraitActive = false;
			}
			if (_festivalTexture != null)
			{
				_festivalTexture = null;
			}
			festivalContent.Unload();
		}

		private void changeLocation(string locationName, int x, int y, int direction = -1, Action onComplete = null)
		{
			if (direction == -1)
			{
				direction = Game1.player.FacingDirection;
			}
			Event e = Game1.currentLocation.currentEvent;
			Game1.currentLocation.currentEvent = null;
			LocationRequest locationRequest = Game1.getLocationRequest(locationName);
			locationRequest.OnLoad += delegate
			{
				if (!e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
				temporaryLocation = null;
				if (onComplete != null)
				{
					onComplete();
				}
			};
			locationRequest.OnWarp += delegate
			{
				farmer.currentLocation = Game1.currentLocation;
				if (e.isFestival)
				{
					Game1.currentLocation.currentEvent = e;
				}
			};
			Game1.warpFarmer(locationRequest, x, y, farmer.FacingDirection);
		}

		public void LogErrorAndHalt(Exception e)
		{
			Game1.chatBox.addErrorMessage("Event script error: " + e.Message);
			if (eventCommands != null && eventCommands.Length != 0 && CurrentCommand < eventCommands.Length)
			{
				Game1.chatBox.addErrorMessage("On line #" + CurrentCommand + ": " + eventCommands[CurrentCommand]);
				skipEvent();
			}
		}

		public void checkForNextCommand(GameLocation location, GameTime time)
		{
			try
			{
				_checkForNextCommand(location, time);
			}
			catch (Exception e)
			{
				LogErrorAndHalt(e);
			}
		}

		protected void _checkForNextCommand(GameLocation location, GameTime time)
		{
			if (skipped || Game1.farmEvent != null)
			{
				return;
			}
			foreach (NPC j in actors)
			{
				j.update(time, Game1.currentLocation);
				if (j.Sprite.CurrentAnimation != null)
				{
					j.Sprite.animateOnce(time);
				}
			}
			if (aboveMapSprites != null)
			{
				for (int k = aboveMapSprites.Count - 1; k >= 0; k--)
				{
					if (aboveMapSprites[k].update(time))
					{
						aboveMapSprites.RemoveAt(k);
					}
				}
			}
			if (underwaterSprites != null)
			{
				foreach (TemporaryAnimatedSprite underwaterSprite in underwaterSprites)
				{
					underwaterSprite.update(time);
				}
			}
			if (!playerControlSequence)
			{
				farmer.setRunning(isRunning: false);
			}
			if (npcControllers != null)
			{
				for (int l = npcControllers.Count - 1; l >= 0; l--)
				{
					npcControllers[l].puppet.isCharging = !isFestival;
					if (npcControllers[l].update(time, location, npcControllers))
					{
						npcControllers.RemoveAt(l);
					}
				}
			}
			if (isFestival)
			{
				festivalUpdate(time);
			}
			string[] split = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)].Split(' ');
			if (temporaryLocation != null && !Game1.currentLocation.Equals(temporaryLocation))
			{
				temporaryLocation.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush: true);
			}
			if (split.Length != 0 && split[0].StartsWith("--"))
			{
				CurrentCommand++;
				return;
			}
			if (CurrentCommand == 0 && !forked && !eventSwitched)
			{
				farmer.speed = 2;
				farmer.running = false;
				Game1.eventOver = false;
				if (eventCommands.Length > 3 && eventCommands[3] == "ignoreEventTileOffset")
				{
					ignoreTileOffsets = true;
				}
				if ((!eventCommands[0].Equals("none") || !Game1.isRaining) && !eventCommands[0].Equals("continue") && !eventCommands[0].Contains("pause"))
				{
					Game1.changeMusicTrack(eventCommands[0], track_interruptable: false, Game1.MusicContext.Event);
				}
				if (location is Farm && Convert.ToInt32(eventCommands[1].Split(' ')[0]) >= -1000 && id != -2 && !ignoreTileOffsets)
				{
					Point p = Farm.getFrontDoorPositionForFarmer(farmer);
					p.X *= 64;
					p.Y *= 64;
					Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.graphics.GraphicsDevice.Viewport.Width)) : (p.X - Game1.graphics.GraphicsDevice.Viewport.Width / 2));
					Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.graphics.GraphicsDevice.Viewport.Height)) : (p.Y - Game1.graphics.GraphicsDevice.Viewport.Height / 2));
				}
				else if (!eventCommands[1].Equals("follow"))
				{
					try
					{
						string[] viewportSplit = eventCommands[1].Split(' ');
						Game1.viewportFreeze = true;
						int centerX = OffsetTileX(Convert.ToInt32(viewportSplit[0])) * 64 + 32;
						int centerY = OffsetTileY(Convert.ToInt32(viewportSplit[1])) * 64 + 32;
						if (viewportSplit[0][0] == '-')
						{
							Game1.viewport.X = centerX;
							Game1.viewport.Y = centerY;
						}
						else
						{
							Game1.viewport.X = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerX - Game1.viewport.Width / 2, Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width)) : (centerX - Game1.viewport.Width / 2));
							Game1.viewport.Y = (Game1.currentLocation.IsOutdoors ? Math.Max(0, Math.Min(centerY - Game1.viewport.Height / 2, Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height)) : (centerY - Game1.viewport.Height / 2));
						}
						if (centerX > 0 && Game1.graphics.GraphicsDevice.Viewport.Width > Game1.currentLocation.Map.DisplayWidth)
						{
							Game1.viewport.X = (Game1.currentLocation.Map.DisplayWidth - Game1.viewport.Width) / 2;
						}
						if (centerY > 0 && Game1.graphics.GraphicsDevice.Viewport.Height > Game1.currentLocation.Map.DisplayHeight)
						{
							Game1.viewport.Y = (Game1.currentLocation.Map.DisplayHeight - Game1.viewport.Height) / 2;
						}
					}
					catch (Exception)
					{
						forked = true;
						return;
					}
				}
				setUpCharacters(eventCommands[2], location);
				trySpecialSetUp(location);
				populateWalkLocationsList();
				CurrentCommand = 3;
				return;
			}
			if (!Game1.fadeToBlack || actorPositionsAfterMove.Count > 0 || CurrentCommand > 3 || forked)
			{
				if (eventCommands.Length <= CurrentCommand)
				{
					return;
				}
				_ = viewportTarget;
				if (!viewportTarget.Equals(Vector3.Zero))
				{
					int playerSpeed = farmer.speed;
					farmer.speed = (int)viewportTarget.X;
					Game1.viewport.X += (int)viewportTarget.X;
					if (viewportTarget.X != 0f)
					{
						Game1.updateRainDropPositionForPlayerMovement((!(viewportTarget.X < 0f)) ? 1 : 3, overrideConstraints: true, Math.Abs(viewportTarget.X + (float)((farmer.isMoving() && farmer.FacingDirection == 3) ? (-farmer.speed) : ((farmer.isMoving() && farmer.FacingDirection == 1) ? farmer.speed : 0))));
					}
					Game1.viewport.Y += (int)viewportTarget.Y;
					farmer.speed = (int)viewportTarget.Y;
					if (viewportTarget.Y != 0f)
					{
						Game1.updateRainDropPositionForPlayerMovement((!(viewportTarget.Y < 0f)) ? 2 : 0, overrideConstraints: true, Math.Abs(viewportTarget.Y - (float)((farmer.isMoving() && farmer.FacingDirection == 0) ? (-farmer.speed) : ((farmer.isMoving() && farmer.FacingDirection == 2) ? farmer.speed : 0))));
					}
					farmer.speed = playerSpeed;
					viewportTarget.Z -= time.ElapsedGameTime.Milliseconds;
					if (viewportTarget.Z <= 0f)
					{
						viewportTarget = Vector3.Zero;
					}
				}
				if (actorPositionsAfterMove.Count > 0)
				{
					string[] array = actorPositionsAfterMove.Keys.ToArray();
					foreach (string s in array)
					{
						Microsoft.Xna.Framework.Rectangle targetTile = new Microsoft.Xna.Framework.Rectangle((int)actorPositionsAfterMove[s].X * 64, (int)actorPositionsAfterMove[s].Y * 64, 64, 64);
						targetTile.Inflate(-4, 0);
						if (getActorByName(s) != null && getActorByName(s).GetBoundingBox().Width > 64)
						{
							targetTile.Inflate(4, 0);
							targetTile.Width = getActorByName(s).GetBoundingBox().Width + 4;
							targetTile.Height = getActorByName(s).GetBoundingBox().Height + 4;
							targetTile.X += 8;
							targetTile.Y += 16;
						}
						if (s.Contains("farmer"))
						{
							Farmer f = getFarmerFromFarmerNumberString(s, farmer);
							if (f != null && targetTile.Contains(f.GetBoundingBox()) && (((float)(f.GetBoundingBox().Y - targetTile.Top) <= 16f + f.getMovementSpeed() && f.FacingDirection != 2) || ((float)(targetTile.Bottom - f.GetBoundingBox().Bottom) <= 16f + f.getMovementSpeed() && f.FacingDirection == 2)))
							{
								f.showNotCarrying();
								f.Halt();
								f.faceDirection((int)actorPositionsAfterMove[s].Z);
								f.FarmerSprite.StopAnimation();
								f.Halt();
								actorPositionsAfterMove.Remove(s);
							}
							else if (f != null)
							{
								f.canOnlyWalk = false;
								f.setRunning(isRunning: false, force: true);
								f.canOnlyWalk = true;
								f.lastPosition = farmer.Position;
								f.MovePosition(time, Game1.viewport, location);
							}
						}
						else
						{
							foreach (NPC i in actors)
							{
								Microsoft.Xna.Framework.Rectangle r = i.GetBoundingBox();
								if (i.Name.Equals(s) && targetTile.Contains(r) && i.GetBoundingBox().Y - targetTile.Top <= 16)
								{
									i.Halt();
									i.faceDirection((int)actorPositionsAfterMove[s].Z);
									actorPositionsAfterMove.Remove(s);
									break;
								}
								if (i.Name.Equals(s))
								{
									if (i is Monster)
									{
										i.MovePosition(time, Game1.viewport, location);
									}
									else
									{
										i.MovePosition(time, Game1.viewport, null);
									}
									break;
								}
							}
						}
					}
					if (actorPositionsAfterMove.Count == 0)
					{
						if (continueAfterMove)
						{
							continueAfterMove = false;
						}
						else
						{
							CurrentCommand++;
						}
					}
					if (!continueAfterMove)
					{
						return;
					}
				}
			}
			tryEventCommand(location, time, split);
		}

		public bool isTileWalkedOn(int x, int y)
		{
			return characterWalkLocations.Contains(new Vector2(x, y));
		}

		private void populateWalkLocationsList()
		{
			Vector2 pos = farmer.getTileLocation();
			characterWalkLocations.Add(pos);
			for (int k = 2; k < eventCommands.Length; k++)
			{
				string[] split = eventCommands[k].Split(' ');
				string a = split[0];
				if (a == "move" && split[1].Equals("farmer"))
				{
					for (int x = 0; x < Math.Abs(Convert.ToInt32(split[2])); x++)
					{
						pos.X += Math.Sign(Convert.ToInt32(split[2]));
						characterWalkLocations.Add(pos);
					}
					for (int y = 0; y < Math.Abs(Convert.ToInt32(split[3])); y++)
					{
						pos.Y += Math.Sign(Convert.ToInt32(split[3]));
						characterWalkLocations.Add(pos);
					}
				}
			}
			foreach (NPC j in actors)
			{
				pos = j.getTileLocation();
				characterWalkLocations.Add(pos);
				for (int i = 2; i < eventCommands.Length; i++)
				{
					string[] split2 = eventCommands[i].Split(' ');
					string a = split2[0];
					if (a == "move" && split2[1].Equals(j.Name))
					{
						for (int x2 = 0; x2 < Math.Abs(Convert.ToInt32(split2[2])); x2++)
						{
							pos.X += Math.Sign(Convert.ToInt32(split2[2]));
							characterWalkLocations.Add(pos);
						}
						for (int y2 = 0; y2 < Math.Abs(Convert.ToInt32(split2[3])); y2++)
						{
							pos.Y += Math.Sign(Convert.ToInt32(split2[3]));
							characterWalkLocations.Add(pos);
						}
					}
				}
			}
		}

		public NPC getActorByName(string name)
		{
			if (name.Equals("rival"))
			{
				name = Utility.getOtherFarmerNames()[0];
			}
			if (name.Equals("spouse"))
			{
				name = farmer.spouse;
			}
			foreach (NPC i in actors)
			{
				if (i.Name.Equals(name))
				{
					return i;
				}
			}
			return null;
		}

		public void applyToAllFarmersByFarmerString(string farmer_string, Action<Farmer> function)
		{
			List<Farmer> farmers = new List<Farmer>();
			if (farmer_string.Equals("farmer"))
			{
				farmers.Add(this.farmer);
			}
			else if (farmer_string.StartsWith("farmer"))
			{
				farmers.Add(getFarmerFromFarmerNumberString(farmer_string, this.farmer));
			}
			foreach (Farmer farmer in farmers)
			{
				bool handled = false;
				foreach (Farmer fake_farmer in farmerActors)
				{
					if (fake_farmer.UniqueMultiplayerID == farmer.UniqueMultiplayerID)
					{
						handled = true;
						function(fake_farmer);
						break;
					}
				}
				if (!handled)
				{
					function(farmer);
				}
			}
		}

		private void addActor(string name, int x, int y, int facingDirection, GameLocation location)
		{
			string spriteName = NPC.getTextureNameForCharacter(name);
			if (name.Equals("Krobus_Trenchcoat"))
			{
				name = "Krobus";
			}
			Texture2D portrait = null;
			try
			{
				portrait = Game1.content.Load<Texture2D>("Portraits\\" + (spriteName.Equals("WeddingOutfits") ? farmer.spouse : spriteName));
			}
			catch (Exception)
			{
			}
			int height = (name.Contains("Dwarf") || name.Equals("Krobus")) ? 96 : 128;
			NPC i = new NPC(new AnimatedSprite("Characters\\" + spriteName, 0, 16, height / 4), new Vector2(x * 64, y * 64), location.Name, facingDirection, name.Contains("Rival") ? Utility.getOtherFarmerNames()[0] : name, null, portrait, eventActor: true);
			i.eventActor = true;
			if (isFestival)
			{
				try
				{
					i.setNewDialogue(GetFestivalDataForYear(i.Name));
				}
				catch (Exception)
				{
				}
			}
			if (i.name.Equals("MrQi"))
			{
				i.displayName = Game1.content.LoadString("Strings\\NPCNames:MisterQi");
			}
			i.eventActor = true;
			actors.Add(i);
		}

		public Farmer getFarmerFromFarmerNumberString(string name, Farmer defaultFarmer)
		{
			Farmer actualFarmer = Utility.getFarmerFromFarmerNumberString(name, defaultFarmer);
			if (actualFarmer == null)
			{
				return null;
			}
			foreach (Farmer farmer in farmerActors)
			{
				if (actualFarmer.UniqueMultiplayerID == farmer.UniqueMultiplayerID)
				{
					return farmer;
				}
			}
			return actualFarmer;
		}

		public Character getCharacterByName(string name)
		{
			if (name.Equals("rival"))
			{
				name = Utility.getOtherFarmerNames()[0];
			}
			if (name.Contains("farmer"))
			{
				return getFarmerFromFarmerNumberString(name, farmer);
			}
			foreach (NPC i in actors)
			{
				if (i.Name.Equals(name))
				{
					return i;
				}
			}
			return null;
		}

		public Vector3 getPositionAfterMove(Character c, int xMove, int yMove, int facingDirection)
		{
			Vector2 tileLocation = c.getTileLocation();
			return new Vector3(tileLocation.X + (float)xMove, tileLocation.Y + (float)yMove, facingDirection);
		}

		private void trySpecialSetUp(GameLocation location)
		{
			switch (id)
			{
			case 739330:
				if (!Game1.player.friendshipData.ContainsKey("Willy"))
				{
					Game1.player.friendshipData.Add("Willy", new Friendship(0));
				}
				Game1.player.checkForQuestComplete(Game1.getCharacterFromName("Willy"), -1, -1, null, null, 5);
				break;
			case 9333220:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					farmer.Position = new Vector2(1920f, 400f);
					getActorByName("Sebastian").setTilePosition(31, 6);
				}
				break;
			case 4324303:
			{
				if (!(location is FarmHouse))
				{
					break;
				}
				Point bed_spot = (location as FarmHouse).GetPlayerBedSpot();
				bed_spot.X--;
				farmer.Position = new Vector2(bed_spot.X * 64, bed_spot.Y * 64 + 16);
				getActorByName("Penny").setTilePosition(bed_spot.X - 1, bed_spot.Y);
				Microsoft.Xna.Framework.Rectangle room = new Microsoft.Xna.Framework.Rectangle(23, 12, 10, 10);
				if ((location as FarmHouse).upgradeLevel == 1)
				{
					room = new Microsoft.Xna.Framework.Rectangle(20, 3, 8, 7);
				}
				Point room_center = room.Center;
				if (!room.Contains(Game1.player.getTileLocationPoint()))
				{
					List<string> commands = new List<string>(eventCommands);
					int command_index12 = 56;
					commands.Insert(command_index12, "globalFade 0.03");
					command_index12++;
					commands.Insert(command_index12, "beginSimultaneousCommand");
					command_index12++;
					commands.Insert(command_index12, "viewport " + room_center.X + " " + room_center.Y);
					command_index12++;
					commands.Insert(command_index12, "globalFadeToClear 0.03");
					command_index12++;
					commands.Insert(command_index12, "endSimultaneousCommand");
					command_index12++;
					commands.Insert(command_index12, "pause 2000");
					command_index12++;
					commands.Insert(command_index12, "globalFade 0.03");
					command_index12++;
					commands.Insert(command_index12, "beginSimultaneousCommand");
					command_index12++;
					commands.Insert(command_index12, "viewport " + Game1.player.getTileX() + " " + Game1.player.getTileY());
					command_index12++;
					commands.Insert(command_index12, "globalFadeToClear 0.03");
					command_index12++;
					commands.Insert(command_index12, "endSimultaneousCommand");
					command_index12++;
					eventCommands = commands.ToArray();
				}
				for (int i = 0; i < eventCommands.Length; i++)
				{
					if (eventCommands[i].StartsWith("makeInvisible"))
					{
						string[] split = eventCommands[i].Split(' ');
						split[1] = string.Concat(int.Parse(split[1]) - 26 + bed_spot.X);
						split[2] = string.Concat(int.Parse(split[2]) - 13 + bed_spot.Y);
						if (location.getObjectAtTile(int.Parse(split[1]), int.Parse(split[2])) == (location as FarmHouse).GetPlayerBed())
						{
							eventCommands[i] = "makeInvisible -1000 -1000";
						}
						else
						{
							eventCommands[i] = string.Join(" ", split);
						}
					}
				}
				break;
			}
			case 4325434:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					farmer.Position = new Vector2(512f, 336f);
					getActorByName("Penny").setTilePosition(5, 5);
				}
				break;
			case 3912132:
			{
				if (!(location is FarmHouse))
				{
					break;
				}
				Point bed_spot2 = (location as FarmHouse).GetPlayerBedSpot();
				bed_spot2.X--;
				if (!location.isTileLocationTotallyClearAndPlaceable(Utility.PointToVector2(bed_spot2) + new Vector2(-2f, 0f)))
				{
					bed_spot2.X++;
				}
				farmer.setTileLocation(Utility.PointToVector2(bed_spot2));
				getActorByName("Elliott").setTileLocation(Utility.PointToVector2(bed_spot2) + new Vector2(-2f, 0f));
				for (int j = 0; j < eventCommands.Length; j++)
				{
					if (eventCommands[j].StartsWith("makeInvisible"))
					{
						string[] split2 = eventCommands[j].Split(' ');
						split2[1] = string.Concat(int.Parse(split2[1]) - 26 + bed_spot2.X);
						split2[2] = string.Concat(int.Parse(split2[2]) - 13 + bed_spot2.Y);
						if (location.getObjectAtTile(int.Parse(split2[1]), int.Parse(split2[2])) == (location as FarmHouse).GetPlayerBed())
						{
							eventCommands[j] = "makeInvisible -1000 -1000";
						}
						else
						{
							eventCommands[j] = string.Join(" ", split2);
						}
					}
				}
				break;
			}
			case 8675611:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					getActorByName("Haley").setTilePosition(4, 5);
					farmer.Position = new Vector2(320f, 336f);
				}
				break;
			case 3917601:
				if (location is DecoratableLocation)
				{
					foreach (Furniture f in (location as DecoratableLocation).furniture)
					{
						if ((int)f.furniture_type == 14 && location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(f.TileLocation + new Vector2(0f, 1f)) && location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(f.TileLocation + new Vector2(1f, 1f)))
						{
							getActorByName("Emily").setTilePosition((int)f.TileLocation.X, (int)f.TileLocation.Y + 1);
							farmer.Position = new Vector2((f.TileLocation.X + 1f) * 64f, (f.tileLocation.Y + 1f) * 64f + 16f);
							f.isOn.Value = true;
							f.setFireplace(location, playSound: false);
							return;
						}
					}
					if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
					{
						getActorByName("Emily").setTilePosition(4, 5);
						farmer.Position = new Vector2(320f, 336f);
					}
				}
				break;
			case 3917666:
				if (location is FarmHouse && (location as FarmHouse).upgradeLevel == 1)
				{
					getActorByName("Maru").setTilePosition(4, 5);
					farmer.Position = new Vector2(320f, 336f);
				}
				break;
			}
		}

		private void setUpCharacters(string description, GameLocation location)
		{
			farmer.Halt();
			if ((Game1.player.locationBeforeForcedEvent.Value == null || Game1.player.locationBeforeForcedEvent.Value == "") && !isMemory)
			{
				Game1.player.positionBeforeEvent = Game1.player.getTileLocation();
				Game1.player.orientationBeforeEvent = Game1.player.FacingDirection;
			}
			string[] split = description.Split(' ');
			for (int j = 0; j < split.Length; j += 4)
			{
				if (split[j + 1].Equals("-1") && !split[j].Equals("farmer"))
				{
					foreach (NPC i in location.getCharacters())
					{
						if (i.Name.Equals(split[j]))
						{
							actors.Add(i);
						}
					}
				}
				else if (!split[j].Equals("farmer"))
				{
					if (split[j].Equals("otherFarmers"))
					{
						int x2 = OffsetTileX(Convert.ToInt32(split[j + 1]));
						int y2 = OffsetTileY(Convert.ToInt32(split[j + 2]));
						int direction2 = Convert.ToInt32(split[j + 3]);
						foreach (Farmer f2 in Game1.getOnlineFarmers())
						{
							if (f2.UniqueMultiplayerID != farmer.UniqueMultiplayerID)
							{
								Farmer fake2 = f2.CreateFakeEventFarmer();
								fake2.completelyStopAnimatingOrDoingAction();
								fake2.hidden.Value = false;
								fake2.faceDirection(direction2);
								fake2.setTileLocation(new Vector2(x2, y2));
								fake2.currentLocation = Game1.currentLocation;
								x2++;
								farmerActors.Add(fake2);
							}
						}
						continue;
					}
					if (split[j].Contains("farmer"))
					{
						int x = OffsetTileX(Convert.ToInt32(split[j + 1]));
						int y = OffsetTileY(Convert.ToInt32(split[j + 2]));
						int direction = Convert.ToInt32(split[j + 3]);
						Farmer f = Utility.getFarmerFromFarmerNumber(Convert.ToInt32(split[j].Last().ToString() ?? ""));
						if (f != null)
						{
							Farmer fake = f.CreateFakeEventFarmer();
							fake.completelyStopAnimatingOrDoingAction();
							fake.hidden.Value = false;
							fake.faceDirection(direction);
							fake.setTileLocation(new Vector2(x, y));
							fake.currentLocation = Game1.currentLocation;
							fake.isFakeEventActor = true;
							farmerActors.Add(fake);
						}
						continue;
					}
					string name = split[j];
					if (split[j].Equals("spouse"))
					{
						name = farmer.spouse;
					}
					if (split[j].Equals("rival"))
					{
						name = (farmer.IsMale ? "maleRival" : "femaleRival");
					}
					if (split[j].Equals("cat"))
					{
						actors.Add(new Cat(OffsetTileX(Convert.ToInt32(split[j + 1])), OffsetTileY(Convert.ToInt32(split[j + 2])), Game1.player.whichPetBreed));
						actors.Last().Name = "Cat";
						actors.Last().position.X -= 32f;
						continue;
					}
					if (split[j].Equals("dog"))
					{
						actors.Add(new Dog(OffsetTileX(Convert.ToInt32(split[j + 1])), OffsetTileY(Convert.ToInt32(split[j + 2])), Game1.player.whichPetBreed));
						actors.Last().Name = "Dog";
						actors.Last().position.X -= 42f;
						continue;
					}
					if (split[j].Equals("golem"))
					{
						actors.Add(new NPC(new AnimatedSprite("Characters\\Monsters\\Wilderness Golem", 0, 16, 24), OffsetPosition(new Vector2(Convert.ToInt32(split[j + 1]), Convert.ToInt32(split[j + 2])) * 64f), 0, "Golem"));
						continue;
					}
					if (split[j].Equals("Junimo"))
					{
						actors.Add(new Junimo(OffsetPosition(new Vector2(Convert.ToInt32(split[j + 1]) * 64, Convert.ToInt32(split[j + 2]) * 64 - 32)), Game1.currentLocation.Name.Equals("AbandonedJojaMart") ? 6 : (-1))
						{
							Name = "Junimo",
							EventActor = true
						});
						continue;
					}
					int xPos = OffsetTileX(Convert.ToInt32(split[j + 1]));
					int yPos = OffsetTileY(Convert.ToInt32(split[j + 2]));
					int facingDir = Convert.ToInt32(split[j + 3]);
					if (location is Farm && id != -2 && !ignoreTileOffsets)
					{
						xPos = Farm.getFrontDoorPositionForFarmer(farmer).X;
						yPos = Farm.getFrontDoorPositionForFarmer(farmer).Y + 2;
						facingDir = 0;
					}
					addActor(name, xPos, yPos, facingDir, location);
				}
				else if (!split[j + 1].Equals("-1"))
				{
					farmer.position.X = OffsetPositionX(Convert.ToInt32(split[j + 1]) * 64);
					farmer.position.Y = OffsetPositionY(Convert.ToInt32(split[j + 2]) * 64 + 16);
					farmer.faceDirection(Convert.ToInt32(split[j + 3]));
					if (location is Farm && id != -2 && !ignoreTileOffsets)
					{
						farmer.position.X = Farm.getFrontDoorPositionForFarmer(farmer).X * 64;
						farmer.position.Y = (Farm.getFrontDoorPositionForFarmer(farmer).Y + 1) * 64;
						farmer.faceDirection(2);
					}
					farmer.FarmerSprite.StopAnimation();
				}
			}
		}

		private void beakerSmashEndFunction(int extraInfo)
		{
			Game1.playSound("breakingGlass");
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.LightBlue, 10));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(400, 3008, 64, 64), 99999f, 2, 0, new Vector2(9f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.LightBlue, 1f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 700
			});
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(9f, 16f) * 64f, Color.White * 0.75f, 10)
			{
				motion = new Vector2(0f, -1f)
			});
		}

		private void eggSmashEndFunction(int extraInfo)
		{
			Game1.playSound("slimedead");
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(47, new Vector2(9f, 16f) * 64f, Color.White, 10));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite(177, 99999f, 9999, 0, new Vector2(6f, 5f) * 64f, flicker: false, flipped: false)
			{
				layerDepth = 1E-06f
			});
		}

		private void balloonInSky(int extraInfo)
		{
			TemporaryAnimatedSprite t2 = Game1.currentLocation.getTemporarySpriteByID(2);
			if (t2 != null)
			{
				t2.motion = Vector2.Zero;
			}
			t2 = Game1.currentLocation.getTemporarySpriteByID(1);
			if (t2 != null)
			{
				t2.motion = Vector2.Zero;
			}
		}

		private void marcelloBalloonLand(int extraInfo)
		{
			Game1.playSound("thudStep");
			Game1.playSound("dirtyHit");
			TemporaryAnimatedSprite t2 = Game1.currentLocation.getTemporarySpriteByID(2);
			if (t2 != null)
			{
				t2.motion = Vector2.Zero;
			}
			t2 = Game1.currentLocation.getTemporarySpriteByID(3);
			if (t2 != null)
			{
				t2.scaleChange = 0f;
			}
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(25f, 39f) + eventPositionTileOffset) * 64f + new Vector2(-32f, 32f), flicker: false, flipped: true, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
			Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), 120f, 8, 1, (new Vector2(27f, 39f) + eventPositionTileOffset) * 64f + new Vector2(0f, 48f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
			{
				delayBeforeAnimationStart = 300
			});
			CurrentCommand++;
		}

		private void samPreOllie(int extraInfo)
		{
			getActorByName("Sam").Sprite.currentFrame = 27;
			farmer.faceDirection(0);
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.xStopCoordinate = 1408;
			temporarySpriteByID.reachedStopCoordinate = samOllie;
			temporarySpriteByID.motion = new Vector2(2f, 0f);
		}

		private void samOllie(int extraInfo)
		{
			Game1.playSound("crafting");
			getActorByName("Sam").Sprite.currentFrame = 26;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.motion.Y = -9f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 530f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.endFunction = samGrind;
			temporarySpriteByID.destroyable = false;
		}

		private void samGrind(int extraInfo)
		{
			Game1.playSound("hammer");
			getActorByName("Sam").Sprite.currentFrame = 28;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 9999;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.xStopCoordinate = 1664;
			temporarySpriteByID.yStopCoordinate = -1;
			temporarySpriteByID.reachedStopCoordinate = samDropOff;
		}

		private void samDropOff(int extraInfo)
		{
			NPC actorByName = getActorByName("Sam");
			actorByName.Sprite.currentFrame = 31;
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			temporarySpriteByID.currentNumberOfLoops = 9999;
			temporarySpriteByID.totalNumberOfLoops = 0;
			temporarySpriteByID.motion.Y = 0f;
			temporarySpriteByID.motion.X = 2f;
			temporarySpriteByID.acceleration = new Vector2(0f, 0.4f);
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 99999f;
			temporarySpriteByID.yStopCoordinate = 5760;
			temporarySpriteByID.reachedStopCoordinate = samGround;
			temporarySpriteByID.endFunction = null;
			actorByName.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(29, 100),
				new FarmerSprite.AnimationFrame(30, 100),
				new FarmerSprite.AnimationFrame(31, 100),
				new FarmerSprite.AnimationFrame(32, 100)
			});
			actorByName.Sprite.loop = false;
		}

		private void samGround(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(92473);
			Game1.playSound("thudStep");
			temporarySpriteByID.attachedCharacter = null;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.totalNumberOfLoops = -1;
			temporarySpriteByID.interval = 0f;
			temporarySpriteByID.destroyable = true;
			CurrentCommand++;
		}

		private void catchFootball(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("fishSlap");
			temporarySpriteByID.motion = new Vector2(2f, -8f);
			temporarySpriteByID.rotationChange = (float)Math.PI / 24f;
			temporarySpriteByID.reachedStopCoordinate = footballLand;
			temporarySpriteByID.yStopCoordinate = 1088;
			farmer.jump();
		}

		private void footballLand(int extraInfo)
		{
			TemporaryAnimatedSprite temporarySpriteByID = Game1.currentLocation.getTemporarySpriteByID(56232);
			Game1.playSound("sandyStep");
			temporarySpriteByID.motion = new Vector2(0f, 0f);
			temporarySpriteByID.rotationChange = 0f;
			temporarySpriteByID.reachedStopCoordinate = null;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.interval = 999999f;
			CurrentCommand++;
		}

		private void parrotSplat(int extraInfo)
		{
			Game1.playSound("drumkit0");
			DelayedAction.playSoundAfterDelay("drumkit5", 100);
			Game1.playSound("slimeHit");
			foreach (TemporaryAnimatedSprite aboveMapSprite in aboveMapSprites)
			{
				aboveMapSprite.alpha = 0f;
			}
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 2f, (float)Math.PI / 64f)
			{
				motion = new Vector2(2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI / 4f, (float)Math.PI / 64f)
			{
				motion = new Vector2(-2f, -1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, (float)Math.PI, (float)Math.PI / 64f)
			{
				motion = new Vector2(1f, 1f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(174, 168, 4, 11), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.02f, 0.01f, Color.White, 4f, 0f, 0f, (float)Math.PI / 64f)
			{
				motion = new Vector2(-2f, -2f),
				acceleration = new Vector2(0f, 0.1f)
			});
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(148, 165, 25, 23), 99999f, 1, 99999, new Vector2(1504f, 5568f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				id = 666f
			});
			CurrentCommand++;
		}

		public virtual Vector2 OffsetPosition(Vector2 original)
		{
			return new Vector2(OffsetPositionX(original.X), OffsetPositionY(original.Y));
		}

		public virtual Vector2 OffsetTile(Vector2 original)
		{
			return new Vector2(OffsetTileX((int)original.X), OffsetTileY((int)original.Y));
		}

		public virtual float OffsetPositionX(float original)
		{
			if (original < 0f || ignoreTileOffsets)
			{
				return original;
			}
			return original + eventPositionTileOffset.X * 64f;
		}

		public virtual float OffsetPositionY(float original)
		{
			if (original < 0f || ignoreTileOffsets)
			{
				return original;
			}
			return original + eventPositionTileOffset.Y * 64f;
		}

		public virtual int OffsetTileX(int original)
		{
			if (original < 0 || ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + eventPositionTileOffset.X);
		}

		public virtual int OffsetTileY(int original)
		{
			if (original < 0 || ignoreTileOffsets)
			{
				return original;
			}
			return (int)((float)original + eventPositionTileOffset.Y);
		}

		private void addSpecificTemporarySprite(string key, GameLocation location, string[] split)
		{
			switch (key)
			{
			case "LeoWillyFishing":
			{
				for (int i = 0; i < 20; i++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite(0, new Vector2(42.5f, 38f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), Color.White * 0.7f)
					{
						layerDepth = (float)(1280 + i) / 10000f,
						delayBeforeAnimationStart = i * 150
					});
				}
				break;
			}
			case "LeoLinusCooking":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(240, 128, 16, 16), 9999f, 1, 1, new Vector2(29f, 8.5f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					layerDepth = 1f
				});
				for (int smokePuffs = 0; smokePuffs < 10; smokePuffs++)
				{
					Utility.addSmokePuff(location, new Vector2(29.5f, 8.6f) * 64f, smokePuffs * 500);
				}
				break;
			}
			case "BoatParrotLeave":
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite2 = aboveMapSprites.First();
				temporaryAnimatedSprite2.motion = new Vector2(4f, -6f);
				temporaryAnimatedSprite2.sourceRect.X = 48;
				temporaryAnimatedSprite2.sourceRectStartingPos.X = 48f;
				temporaryAnimatedSprite2.animationLength = 3;
				temporaryAnimatedSprite2.pingPong = true;
				break;
			}
			case "BoatParrotSquawkStop":
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite = aboveMapSprites.First();
				temporaryAnimatedSprite.sourceRect.X = 0;
				temporaryAnimatedSprite.sourceRectStartingPos.X = 0f;
				break;
			}
			case "BoatParrotSquawk":
			{
				TemporaryAnimatedSprite temporaryAnimatedSprite3 = aboveMapSprites.First();
				temporaryAnimatedSprite3.sourceRect.X = 24;
				temporaryAnimatedSprite3.sourceRectStartingPos.X = 24f;
				Game1.playSound("parrot_squawk");
				break;
			}
			case "BoatParrot":
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(48, 0, 24, 24), 100f, 3, 99999, new Vector2(Game1.viewport.X - 64, 2112f), flicker: false, flipped: true, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f,
					motion = new Vector2(6f, 1f),
					delayBeforeAnimationStart = 0,
					pingPong = true,
					xStopCoordinate = 1040,
					reachedStopCoordinate = delegate
					{
						TemporaryAnimatedSprite temporaryAnimatedSprite4 = aboveMapSprites.First();
						if (temporaryAnimatedSprite4 != null)
						{
							temporaryAnimatedSprite4.motion = new Vector2(0f, 2f);
							temporaryAnimatedSprite4.yStopCoordinate = 2336;
							temporaryAnimatedSprite4.reachedStopCoordinate = delegate
							{
								TemporaryAnimatedSprite temporaryAnimatedSprite5 = aboveMapSprites.First();
								temporaryAnimatedSprite5.animationLength = 1;
								temporaryAnimatedSprite5.pingPong = false;
								temporaryAnimatedSprite5.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24);
								temporaryAnimatedSprite5.sourceRectStartingPos = Vector2.Zero;
							};
						}
					}
				});
				break;
			case "islandFishSplash":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 544, 16, 16), 100000f, 1, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 9999f,
					motion = new Vector2(-2f, -8f),
					acceleration = new Vector2(0f, 0.2f),
					flipped = true,
					rotationChange = -0.02f,
					yStopCoordinate = 5952,
					layerDepth = 0.99f,
					reachedStopCoordinate = delegate
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, location.getTemporarySpriteByID(9999).position, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							layerDepth = 1f
						});
						location.removeTemporarySpritesWithID(9999);
						Game1.playSound("waterSlosh");
					}
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(48, 16, 16, 16), 100f, 5, 1, new Vector2(81f, 92f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					layerDepth = 1f
				});
				break;
			case "georgeLeekGift":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(17f, 19f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f,
					paused = false,
					holdLastFrame = true
				});
				break;
			case "staticSprite":
				location.temporarySprites.Add(new TemporaryAnimatedSprite(split[2], new Microsoft.Xna.Framework.Rectangle(Convert.ToInt32(split[3]), Convert.ToInt32(split[4]), Convert.ToInt32(split[5]), Convert.ToInt32(split[6])), new Vector2((float)Convert.ToDouble(split[7]), (float)Convert.ToDouble(split[8])) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					scale = 4f,
					layerDepth = ((split.Length > 10) ? ((float)Convert.ToDouble(split[10])) : 1f),
					id = ((split.Length > 9) ? Convert.ToInt32(split[9]) : 999)
				});
				break;
			case "WillyWad":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors2"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(192, 61, 32, 32),
					sourceRectStartingPos = new Vector2(192f, 61f),
					animationLength = 2,
					totalNumberOfLoops = 99999,
					interval = 400f,
					scale = 4f,
					position = new Vector2(50f, 23f) * 64f,
					layerDepth = 0.1536f,
					id = 996f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3328f, 1728f), Color.White, 10, flipped: false, 80f, 999999));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3264f, 1792f), Color.White, 10, flipped: false, 70f, 999999));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(51, new Vector2(3392f, 1792f), Color.White, 10, flipped: false, 85f, 999999));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(53f, 24f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(54f, 23f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "parrotHutSquawk":
				(location as IslandHut).parrotUpgradePerches[0].timeUntilSqwawk = 1f;
				break;
			case "parrotPerchHut":
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(0, 0, 24, 24), new Vector2(7f, 4f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					scale = 4f,
					layerDepth = 1f,
					id = 999f
				});
				break;
			case "trashBearTown":
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(46, 80, 46, 56), new Vector2(43f, 64f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					motion = new Vector2(4f, 0f),
					scale = 4f,
					layerDepth = 1f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 32f,
					id = 777f,
					xStopCoordinate = 3392,
					reachedStopCoordinate = delegate
					{
						aboveMapSprites.First().xStopCoordinate = -1;
						aboveMapSprites.First().motion = new Vector2(4f, 0f);
						location.ApplyMapOverride("Town-TrashGone", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(57, 68, 17, 5));
						location.ApplyMapOverride("Town-DogHouse", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(51, 65, 5, 6));
						Game1.flashAlpha = 0.75f;
						Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
						location.playSound("yoba");
						TemporaryAnimatedSprite temporaryAnimatedSprite6 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), new Vector2(3456f, 4160f), flipped: false, 0f, Color.White)
						{
							yStopCoordinate = 4372,
							motion = new Vector2(-0.5f, -10f),
							acceleration = new Vector2(0f, 0.25f),
							scale = 4f,
							alphaFade = 0f,
							extraInfoForEndBehavior = -777
						};
						temporaryAnimatedSprite6.reachedStopCoordinate = temporaryAnimatedSprite6.bounce;
						temporaryAnimatedSprite6.initialPosition.Y = 4372f;
						aboveMapSprites.Add(temporaryAnimatedSprite6);
						aboveMapSprites.AddRange(Utility.getStarsAndSpirals(location, 54, 69, 6, 5, 1000, 10, Color.Lime));
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 1f,
							delayBeforeAnimationStart = 3000,
							startSound = "dogWhining"
						});
					}
				});
				break;
			case "trashBearUmbrella1":
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(0, 80, 46, 56), new Vector2(102f, 94.5f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					motion = new Vector2(0f, -9f),
					acceleration = new Vector2(0f, 0.4f),
					scale = 4f,
					layerDepth = 1f,
					id = 777f,
					yStopCoordinate = 6144,
					reachedStopCoordinate = delegate(int param)
					{
						location.getTemporarySpriteByID(777).yStopCoordinate = -1;
						location.getTemporarySpriteByID(777).motion = new Vector2(0f, (float)param * 0.75f);
						location.getTemporarySpriteByID(777).acceleration = new Vector2(0.04f, -0.19f);
						location.getTemporarySpriteByID(777).accelerationChange = new Vector2(0f, 0.0015f);
						location.getTemporarySpriteByID(777).sourceRect.X += 46;
						location.playSound("batFlap");
						location.playSound("tinyWhip");
					}
				});
				break;
			case "trashBearMagic":
				Utility.addStarsAndSpirals(location, 95, 103, 24, 12, 2000, 10, Color.Lime);
				(location as Forest).removeSewerTrash();
				Game1.flashAlpha = 0.75f;
				Game1.screenGlowOnce(Color.Lime, hold: false, 0.25f, 1f);
				break;
			case "trashBearPrelude":
				Utility.addStarsAndSpirals(location, 95, 106, 23, 4, 10000, 275, Color.Lime);
				break;
			case "krobusBeach":
			{
				for (int j = 0; j < 8; j++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 150f, 4, 0, new Vector2(84f + ((j % 2 == 0) ? 0.25f : (-0.05f)), 41f) * 64f, flicker: false, Game1.random.NextDouble() < 0.5, 0.001f, 0.02f, Color.White, 0.75f, 0.003f, 0f, 0f)
					{
						delayBeforeAnimationStart = 500 + j * 1000,
						startSound = "waterSlosh"
					});
				}
				underwaterSprites = new List<TemporaryAnimatedSprite>();
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2688,
					delayBeforeAnimationStart = 0,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(82f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 3008,
					delayBeforeAnimationStart = 2000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2688,
					delayBeforeAnimationStart = 150,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(88f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 3008,
					delayBeforeAnimationStart = 2000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(90f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2816,
					delayBeforeAnimationStart = 300,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(79f, 52f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2816,
					delayBeforeAnimationStart = 1000,
					pingPong = true
				});
				break;
			}
			case "coldstarMiracle":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(400, 704, 90, 61),
					sourceRectStartingPos = new Vector2(400f, 704f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 99999f,
					alpha = 0.01f,
					alphaFade = -0.01f,
					scale = 4f,
					position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
					layerDepth = 0.8535f,
					id = 989f
				});
				break;
			case "sunroom":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(304, 486, 24, 26),
					sourceRectStartingPos = new Vector2(304f, 486f),
					animationLength = 1,
					totalNumberOfLoops = 997,
					interval = 99999f,
					scale = 4f,
					position = new Vector2(4f, 8f) * 64f + new Vector2(8f, -8f) * 4f,
					layerDepth = 0.0512f,
					id = 996f
				});
				location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(stayInbounds: true));
				while (Game1.random.NextDouble() < 0.5)
				{
					location.addCritter(new Butterfly(location.getRandomTile()).setStayInbounds(stayInbounds: true));
				}
				break;
			case "sauceGood":
				Utility.addSprinklesToLocation(location, OffsetTileX(64), OffsetTileY(16), 3, 1, 800, 200, Color.White);
				break;
			case "sauceFire":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
					animationLength = 4,
					sourceRectStartingPos = new Vector2(276f, 1985f),
					interval = 100f,
					totalNumberOfLoops = 5,
					position = OffsetPosition(new Vector2(64f, 16f) * 64f + new Vector2(3f, -4f) * 4f),
					scale = 4f,
					layerDepth = 1f
				});
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				for (int k = 0; k < 8; k++)
				{
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), OffsetPosition(new Vector2(64f, 16f) * 64f) + new Vector2(Game1.random.Next(-16, 32), 0f), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = k * 25
					});
				}
				break;
			}
			case "evilRabbit":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(264, 209, 19, 16),
					sourceRectStartingPos = new Vector2(264f, 209f),
					animationLength = 1,
					totalNumberOfLoops = 999,
					interval = 999f,
					scale = 4f,
					position = new Vector2(4f, 1f) * 64f + new Vector2(38f, 23f) * 4f,
					layerDepth = 1f,
					motion = new Vector2(-2f, -2f),
					acceleration = new Vector2(0f, 0.1f),
					yStopCoordinate = 204,
					xStopCoordinate = 316,
					flipped = true,
					id = 778f
				});
				break;
			case "shakeBushStop":
				location.getTemporarySpriteByID(777).shakeIntensity = 0f;
				break;
			case "shakeBush":
				location.getTemporarySpriteByID(777).shakeIntensity = 1f;
				break;
			case "movieBush":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\bushes"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(65, 58, 30, 35),
					sourceRectStartingPos = new Vector2(65f, 58f),
					animationLength = 1,
					totalNumberOfLoops = 999,
					interval = 999f,
					scale = 4f,
					position = new Vector2(4f, 1f) * 64f + new Vector2(33f, 13f) * 4f,
					layerDepth = 0.99f,
					id = 777f
				});
				break;
			case "woodswalker":
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(448, 419, 16, 21),
					sourceRectStartingPos = new Vector2(448f, 419f),
					animationLength = 4,
					totalNumberOfLoops = 7,
					interval = 150f,
					scale = 4f,
					position = new Vector2(4f, 1f) * 64f + new Vector2(5f, 22f) * 4f,
					shakeIntensity = 1f,
					motion = new Vector2(1f, 0f),
					xStopCoordinate = 576,
					layerDepth = 1f,
					id = 996f
				});
				break;
			case "movieFrame":
			{
				int movieIndex_2 = Convert.ToInt32(split[2]);
				int frame = Convert.ToInt32(split[3]);
				int duration = Convert.ToInt32(split[4]);
				int y_offset_2 = movieIndex_2 * 128 + frame / 5 * 64;
				int x_offset_2 = frame % 5 * 96;
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(16 + x_offset_2, y_offset_2, 90, 61),
					sourceRectStartingPos = new Vector2(16 + x_offset_2, y_offset_2),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = duration,
					scale = 4f,
					position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
					shakeIntensity = 0.25f,
					layerDepth = 0.85f + (float)(movieIndex_2 * frame) / 10000f,
					id = 997f
				});
				break;
			}
			case "movieTheater_screen":
			{
				int movie_index = int.Parse(split[2]);
				int screen_index = int.Parse(split[3]);
				bool shake = bool.Parse(split[4]);
				int y_offset = movie_index * 128 + screen_index / 5 * 64;
				int x_offset = screen_index % 5 * 96;
				location.removeTemporarySpritesWithIDLocal(998f);
				if (screen_index >= 0)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Movies"),
						sourceRect = new Microsoft.Xna.Framework.Rectangle(16 + x_offset, y_offset, 90, 61),
						sourceRectStartingPos = new Vector2(16 + x_offset, y_offset),
						animationLength = 1,
						totalNumberOfLoops = 9999,
						interval = 5000f,
						scale = 4f,
						position = new Vector2(4f, 1f) * 64f + new Vector2(3f, 7f) * 4f,
						shakeIntensity = (shake ? 1f : 0f),
						layerDepth = 0.1f + (float)(movie_index * screen_index) / 10000f,
						id = 998f
					});
				}
				break;
			}
			case "movieTheater_setup":
				Game1.currentLightSources.Add(new LightSource(7, new Vector2(192f, 64f) + new Vector2(64f, 80f) * 4f, 4f, LightSource.LightContext.None, 0L));
				location.temporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("Maps\\MovieTheaterScreen_TileSheet"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(224, 0, 96, 96),
					sourceRectStartingPos = new Vector2(224f, 0f),
					animationLength = 1,
					interval = 5000f,
					totalNumberOfLoops = 9999,
					scale = 4f,
					position = new Vector2(4f, 5f) * 64f,
					layerDepth = 1f,
					id = 999f,
					delayBeforeAnimationStart = 7950
				});
				break;
			case "junimoSpotlight":
				actors.First().drawOnTop = true;
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
					sourceRect = new Microsoft.Xna.Framework.Rectangle(316, 123, 67, 43),
					sourceRectStartingPos = new Vector2(316f, 123f),
					animationLength = 1,
					interval = 5000f,
					totalNumberOfLoops = 9999,
					scale = 4f,
					position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 268, 172, 0, -20),
					layerDepth = 0.0001f,
					local = true,
					id = 999f
				});
				break;
			case "missingJunimoStars":
			{
				location.removeTemporarySpritesWithID(999);
				Texture2D tempTxture98 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				for (int l = 0; l < 48; l++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite
					{
						texture = tempTxture98,
						sourceRect = new Microsoft.Xna.Framework.Rectangle(477, 306, 28, 28),
						sourceRectStartingPos = new Vector2(477f, 306f),
						animationLength = 1,
						interval = 5000f,
						totalNumberOfLoops = 10,
						scale = Game1.random.Next(1, 5),
						position = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 84, 84) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)),
						rotationChange = (float)Math.PI / (float)Game1.random.Next(16, 128),
						motion = new Vector2((float)Game1.random.Next(-30, 40) / 10f, (float)Game1.random.Next(20, 90) * -0.1f),
						acceleration = new Vector2(0f, 0.05f),
						local = true,
						layerDepth = (float)l / 100f,
						color = ((Game1.random.NextDouble() < 0.5) ? Color.White : Utility.getRandomRainbowColor())
					});
				}
				break;
			}
			case "frogJump":
			{
				TemporaryAnimatedSprite temporarySpriteByID3 = location.getTemporarySpriteByID(777);
				temporarySpriteByID3.motion = new Vector2(-2f, 0f);
				temporarySpriteByID3.animationLength = 4;
				temporarySpriteByID3.interval = 150f;
				break;
			}
			case "sebastianFrogHouse":
			{
				Vector2 spot = new Vector2(((location as FarmHouse).upgradeLevel == 1) ? 30 : 36, ((location as FarmHouse).upgradeLevel == 1) ? 7 : 16);
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(641f, 1534f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2(0f, -5f) * 4f,
					scale = 4f,
					layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
				});
				Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = crittersText2,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(0f, 224f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = spot * 64f + new Vector2(25f, 2f) * 4f,
					scale = 4f,
					flipped = true,
					layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
					id = 777f
				});
				break;
			}
			case "sebastianFrog":
			{
				Texture2D crittersText3 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = crittersText3,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 224, 16, 16),
					animationLength = 4,
					sourceRectStartingPos = new Vector2(0f, 224f),
					interval = 120f,
					totalNumberOfLoops = 9999,
					position = new Vector2(45f, 36f) * 64f,
					scale = 4f,
					layerDepth = 0.00064f,
					motion = new Vector2(2f, 0f),
					xStopCoordinate = 3136,
					id = 777f,
					reachedStopCoordinate = delegate
					{
						int num = CurrentCommand;
						CurrentCommand = num + 1;
						location.removeTemporarySpritesWithID(777);
					}
				});
				break;
			}
			case "haleyCakeWalk":
			{
				Texture2D tempTxture99 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture99,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 400, 144, 112),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(0f, 400f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(26f, 65f) * 64f,
					scale = 4f,
					layerDepth = 0.00064f
				});
				break;
			}
			case "harveyDinnerSet":
			{
				Vector2 centerPoint = new Vector2(5f, 16f);
				if (location is DecoratableLocation)
				{
					foreach (Furniture f in (location as DecoratableLocation).furniture)
					{
						if ((int)f.furniture_type == 14 && location.getTileIndexAt((int)f.tileLocation.X, (int)f.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)f.tileLocation.X + 1, (int)f.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)f.tileLocation.X + 2, (int)f.tileLocation.Y + 1, "Buildings") == -1 && location.getTileIndexAt((int)f.tileLocation.X - 1, (int)f.tileLocation.Y + 1, "Buildings") == -1)
						{
							centerPoint = new Vector2((int)f.TileLocation.X, (int)f.TileLocation.Y + 1);
							f.isOn.Value = true;
							f.setFireplace(location, playSound: false);
							break;
						}
					}
				}
				location.TemporarySprites.Clear();
				getActorByName("Harvey").setTilePosition((int)centerPoint.X + 2, (int)centerPoint.Y);
				getActorByName("Harvey").Position = new Vector2(getActorByName("Harvey").Position.X - 32f, getActorByName("Harvey").Position.Y);
				farmer.Position = new Vector2(centerPoint.X * 64f - 32f, centerPoint.Y * 64f + 32f);
				Object o5 = null;
				o5 = location.getObjectAtTile((int)centerPoint.X, (int)centerPoint.Y);
				if (o5 != null)
				{
					o5.isTemporarilyInvisible = true;
				}
				o5 = location.getObjectAtTile((int)centerPoint.X + 1, (int)centerPoint.Y);
				if (o5 != null)
				{
					o5.isTemporarilyInvisible = true;
				}
				o5 = location.getObjectAtTile((int)centerPoint.X - 1, (int)centerPoint.Y);
				if (o5 != null)
				{
					o5.isTemporarilyInvisible = true;
				}
				o5 = location.getObjectAtTile((int)centerPoint.X + 2, (int)centerPoint.Y);
				if (o5 != null)
				{
					o5.isTemporarilyInvisible = true;
				}
				Texture2D tempTxture100 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture100,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(385, 423, 48, 32),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(385f, 423f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = centerPoint * 64f + new Vector2(-8f, -16f) * 4f,
					scale = 4f,
					layerDepth = (centerPoint.Y + 0.2f) * 64f / 10000f,
					light = true,
					lightRadius = 4f,
					lightcolor = Color.Black
				});
				List<string> tmp = eventCommands.ToList();
				tmp.Insert(CurrentCommand + 1, "viewport " + (int)centerPoint.X + " " + (int)centerPoint.Y + " true");
				eventCommands = tmp.ToArray();
				break;
			}
			case "harveyKitchenFlame":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11),
					animationLength = 4,
					sourceRectStartingPos = new Vector2(276f, 1985f),
					interval = 100f,
					totalNumberOfLoops = 6,
					position = new Vector2(7f, 12f) * 64f + new Vector2(8f, 5f) * 4f,
					scale = 4f,
					layerDepth = 0.09184f
				});
				break;
			case "harveyKitchenSetup":
			{
				Texture2D tempTxture101 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture101,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(379, 251, 31, 13),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(379f, 251f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(7f, 12f) * 64f + new Vector2(-2f, 6f) * 4f,
					scale = 4f,
					layerDepth = 0.091520004f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture101,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(391, 235, 5, 13),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(391f, 235f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(6f, 12f) * 64f + new Vector2(8f, 4f) * 4f,
					scale = 4f,
					layerDepth = 0.091520004f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture101,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(399, 229, 11, 21),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(399f, 229f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(4f, 12f) * 64f + new Vector2(8f, -5f) * 4f,
					scale = 4f,
					layerDepth = 0.091520004f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(6f, 12f) * 64f + new Vector2(0f, -5f) * 4f, Color.White, 10)
				{
					totalNumberOfLoops = 999,
					layerDepth = 0.0921599939f
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(27, new Vector2(6f, 12f) * 64f + new Vector2(24f, -5f) * 4f, Color.White, 10)
				{
					totalNumberOfLoops = 999,
					flipped = true,
					delayBeforeAnimationStart = 400,
					layerDepth = 0.0921599939f
				});
				break;
			}
			case "golemDie":
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite(46, new Vector2(40f, 11f) * 64f, Color.DarkGray, 10));
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, new Vector2(40f, 11f) * 64f, Color.LimeGreen, 10), location, 2);
				Texture2D tempTxture102 = Game1.temporaryContent.Load<Texture2D>("Characters\\Monsters\\Wilderness Golem");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture102,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 16, 24),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(0f, 0f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(40f, 11f) * 64f + new Vector2(2f, -8f) * 4f,
					scale = 4f,
					layerDepth = 0.01f,
					rotation = (float)Math.PI / 2f,
					motion = new Vector2(0f, 4f),
					yStopCoordinate = 832
				});
				break;
			}
			case "farmerHoldPainting":
			{
				Texture2D tempTxture103 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.getTemporarySpriteByID(888).sourceRect.X += 15;
				location.getTemporarySpriteByID(888).sourceRectStartingPos.X += 15f;
				location.removeTemporarySpritesWithID(444);
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture103,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(476, 394, 25, 22),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(476f, 394f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(75f, 40f) * 64f + new Vector2(-4f, -33f) * 4f,
					scale = 4f,
					layerDepth = 1f,
					id = 777f
				});
				break;
			}
			case "leahStopHoldingPainting":
				location.getTemporarySpriteByID(999).sourceRect.X -= 15;
				location.getTemporarySpriteByID(999).sourceRectStartingPos.X -= 15f;
				location.removeTemporarySpritesWithIDLocal(777f);
				Game1.playSound("thudStep");
				break;
			case "leahHoldPainting":
			{
				Texture2D tempTxture104 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.getTemporarySpriteByID(999).sourceRect.X += 15;
				location.getTemporarySpriteByID(999).sourceRectStartingPos.X += 15f;
				int whichPainting = (!Game1.netWorldState.Value.hasWorldStateID("m_painting0")) ? (Game1.netWorldState.Value.hasWorldStateID("m_painting1") ? 1 : 2) : 0;
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture104,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(400 + whichPainting * 25, 394, 25, 23),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(400 + whichPainting * 25, 394f),
					interval = 5000f,
					totalNumberOfLoops = 9999,
					position = new Vector2(73f, 38f) * 64f + new Vector2(-2f, -16f) * 4f,
					scale = 4f,
					layerDepth = 1f,
					id = 777f
				});
				break;
			}
			case "leahPaintingSetup":
			{
				Texture2D tempTxture105 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture105,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(368f, 393f),
					interval = 5000f,
					totalNumberOfLoops = 99999,
					position = new Vector2(72f, 38f) * 64f + new Vector2(3f, -13f) * 4f,
					scale = 4f,
					layerDepth = 0.1f,
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture105,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 393, 15, 28),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(368f, 393f),
					interval = 5000f,
					totalNumberOfLoops = 99999,
					position = new Vector2(74f, 40f) * 64f + new Vector2(3f, -17f) * 4f,
					scale = 4f,
					layerDepth = 0.1f,
					id = 888f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture105,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(369, 424, 11, 15),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(369f, 424f),
					interval = 9999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(75f, 40f) * 64f + new Vector2(-2f, -11f) * 4f,
					scale = 4f,
					layerDepth = 0.01f,
					id = 444f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 1822, 32, 34),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(96f, 1822f),
					interval = 5000f,
					totalNumberOfLoops = 99999,
					position = new Vector2(79f, 36f) * 64f,
					scale = 4f,
					layerDepth = 0.1f
				});
				break;
			}
			case "junimoShow":
			{
				Texture2D tempTxture106 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture106,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 350, 19, 14),
					animationLength = 6,
					sourceRectStartingPos = new Vector2(393f, 350f),
					interval = 90f,
					totalNumberOfLoops = 86,
					position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
					scale = 4f,
					layerDepth = 0.95f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture106,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 364, 19, 14),
					animationLength = 4,
					sourceRectStartingPos = new Vector2(393f, 364f),
					interval = 90f,
					totalNumberOfLoops = 31,
					position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
					scale = 4f,
					layerDepth = 0.97f,
					delayBeforeAnimationStart = 11034
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture106,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(393, 378, 19, 14),
					animationLength = 6,
					sourceRectStartingPos = new Vector2(393f, 378f),
					interval = 90f,
					totalNumberOfLoops = 21,
					position = new Vector2(37f, 14f) * 64f + new Vector2(7f, -2f) * 4f,
					scale = 4f,
					layerDepth = 1f,
					delayBeforeAnimationStart = 22069
				});
				break;
			}
			case "samTV":
			{
				Texture2D tempTxture107 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture107,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 350, 25, 29),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(368f, 350f),
					interval = 5000f,
					totalNumberOfLoops = 99999,
					position = new Vector2(37f, 14f) * 64f + new Vector2(4f, -12f) * 4f,
					scale = 4f,
					layerDepth = 0.9f
				});
				break;
			}
			case "gridballGameTV":
			{
				Texture2D tempTxture108 = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxture108,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(368, 336, 19, 14),
					animationLength = 7,
					sourceRectStartingPos = new Vector2(368f, 336f),
					interval = 5000f,
					totalNumberOfLoops = 99999,
					position = new Vector2(34f, 3f) * 64f + new Vector2(7f, 13f) * 4f,
					scale = 4f,
					layerDepth = 1f
				});
				break;
			}
			case "shaneSaloonCola":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.mouseCursors,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(552f, 1862f),
					interval = 999999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(32f, 17f) * 64f + new Vector2(10f, 3f) * 4f,
					scale = 4f,
					layerDepth = 1E-07f
				});
				break;
			case "luauShorts":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(336, 512, 16, 16), 9999f, 1, 99999, new Vector2(35f, 10f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, -8f),
					acceleration = new Vector2(0f, 0.25f),
					yStopCoordinate = 704,
					xStopCoordinate = 2112
				});
				break;
			case "qiCave":
			{
				Texture2D tempTxt = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(415, 216, 96, 89),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(415f, 216f),
					interval = 999999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(2f, 2f) * 64f + new Vector2(112f, 25f) * 4f,
					scale = 4f,
					layerDepth = 1E-07f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(370, 272, 107, 64),
					animationLength = 1,
					sourceRectStartingPos = new Vector2(370f, 216f),
					interval = 999999f,
					totalNumberOfLoops = 99999,
					position = new Vector2(2f, 2f) * 64f + new Vector2(67f, 81f) * 4f,
					scale = 4f,
					layerDepth = 1.1E-07f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = Game1.objectSpriteSheet,
					sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16),
					sourceRectStartingPos = new Vector2(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).X, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 803, 16, 16).Y),
					animationLength = 1,
					interval = 999999f,
					id = 803f,
					totalNumberOfLoops = 99999,
					position = new Vector2(13f, 7f) * 64f + new Vector2(1f, 9f) * 4f,
					scale = 4f,
					layerDepth = 2.1E-06f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(8f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 90f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(5f, 7f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 120f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(7f, 10f) * 64f,
					scale = 4f,
					layerDepth = 1f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 80f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(15f, 7f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(12f, 11f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 105f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(16f, 10f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTxt,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(432, 171, 16, 30),
					animationLength = 5,
					sourceRectStartingPos = new Vector2(432f, 171f),
					pingPong = true,
					interval = 85f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(3f, 9f) * 64f,
					scale = 4f
				});
				break;
			}
			case "removeSprite":
				if (split != null && split.Count() > 2)
				{
					location.removeTemporarySpritesWithID(Convert.ToInt32(split[2]));
				}
				break;
			case "willyCrabExperiment":
			{
				Texture2D tempTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 250f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(2f, 4f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 200f,
					totalNumberOfLoops = 99999,
					id = 1f,
					initialPosition = new Vector2(2f, 6f) * 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 8000f,
					yPeriodicRange = 32f,
					position = new Vector2(2f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(1f, 5.75f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 100f,
					totalNumberOfLoops = 99999,
					id = 11f,
					position = new Vector2(5f, 3f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 140f,
					totalNumberOfLoops = 99999,
					id = 22f,
					position = new Vector2(4f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 140f,
					totalNumberOfLoops = 99999,
					id = 22f,
					position = new Vector2(8.5f, 5f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 170f,
					totalNumberOfLoops = 99999,
					id = 222f,
					position = new Vector2(6f, 3.25f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 190f,
					totalNumberOfLoops = 99999,
					id = 222f,
					position = new Vector2(6f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 150f,
					totalNumberOfLoops = 99999,
					id = 222f,
					position = new Vector2(7f, 4f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 200f,
					totalNumberOfLoops = 99999,
					id = 2f,
					position = new Vector2(4f, 7f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 127, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 127f),
					pingPong = true,
					interval = 180f,
					totalNumberOfLoops = 99999,
					id = 3f,
					position = new Vector2(8f, 6f) * 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 10000f,
					yPeriodicRange = 32f,
					initialPosition = new Vector2(8f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 220f,
					totalNumberOfLoops = 99999,
					id = 33f,
					position = new Vector2(9f, 6f) * 64f,
					scale = 4f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTexture,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(259, 146, 18, 18),
					animationLength = 3,
					sourceRectStartingPos = new Vector2(259f, 146f),
					pingPong = true,
					interval = 150f,
					totalNumberOfLoops = 99999,
					id = 33f,
					position = new Vector2(10f, 5f) * 64f,
					scale = 4f
				});
				break;
			}
			case "springOnionRemove":
				location.removeTemporarySpritesWithID(777);
				break;
			case "springOnionPeel":
			{
				TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
				temporarySpriteByID.sourceRectStartingPos = new Vector2(144f, 327f);
				temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 327, 112, 112);
				break;
			}
			case "springOnionDemo":
			{
				Texture2D tempTex = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
				location.TemporarySprites.Add(new TemporaryAnimatedSprite
				{
					texture = tempTex,
					sourceRect = new Microsoft.Xna.Framework.Rectangle(144, 215, 112, 112),
					animationLength = 2,
					sourceRectStartingPos = new Vector2(144f, 215f),
					interval = 200f,
					totalNumberOfLoops = 99999,
					id = 777f,
					position = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 264, Game1.graphics.GraphicsDevice.Viewport.Height / 3 - 264),
					local = true,
					scale = 4f,
					destroyable = false,
					overrideLocationDestroy = true
				});
				break;
			}
			case "springOnion":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(1, 129, 16, 16), 200f, 8, 999999, new Vector2(84f, 39f) * 64f, flicker: false, flipped: false, 0.4736f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "pamYobaStatue":
				location.objects.Remove(new Vector2(26f, 9f));
				location.objects.Add(new Vector2(26f, 9f), new Object(Vector2.Zero, 34));
				Game1.getLocationFromName("Trailer_Big").objects.Remove(new Vector2(26f, 9f));
				Game1.getLocationFromName("Trailer_Big").objects.Add(new Vector2(26f, 9f), new Object(Vector2.Zero, 34));
				break;
			case "arcaneBook":
			{
				for (int n = 0; n < 16; n++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(128f, 792f) + new Vector2(Game1.random.Next(32), Game1.random.Next(32) - n * 4), flipped: false, 0f, Color.White)
					{
						interval = 50f,
						totalNumberOfLoops = 99999,
						animationLength = 7,
						layerDepth = 1f,
						scale = 4f,
						alphaFade = 0.008f,
						motion = new Vector2(0f, -0.5f)
					});
				}
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 18), new Vector2(160f, 800f), flipped: false, 0f, Color.White)
				{
					interval = 25f,
					totalNumberOfLoops = 99999,
					animationLength = 3,
					layerDepth = 1f,
					scale = 1f,
					scaleChange = 1f,
					scaleChangeChange = -0.05f,
					alpha = 0.65f,
					alphaFade = 0.005f,
					motion = new Vector2(-8f, -8f),
					acceleration = new Vector2(0.4f, 0.4f)
				});
				for (int m = 0; m < 16; m++)
				{
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2f, 12f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0.002f, Color.Gray)
					{
						alpha = 0.75f,
						motion = new Vector2(1f, -1f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
						interval = 99999f,
						layerDepth = 0.0384f + (float)Game1.random.Next(100) / 10000f,
						scale = 3f,
						scaleChange = 0.01f,
						rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
						delayBeforeAnimationStart = m * 25
					});
				}
				location.setMapTileIndex(2, 12, 2143, "Front", 1);
				break;
			}
			case "stopShakeTent":
				location.getTemporarySpriteByID(999).shakeIntensity = 0f;
				break;
			case "shakeTent":
				location.getTemporarySpriteByID(999).shakeIntensity = 1f;
				break;
			case "EmilyCamping":
				showGroundObjects = false;
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1578, 59, 53), 999999f, 1, 99999, new Vector2(26f, 9f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0788f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(675, 1299, 29, 24), 999999f, 1, 99999, new Vector2(27f, 14f) * 64f, flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 99f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(27f, 14f) * 64f + new Vector2(8f, 4f) * 4f, flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightID = 666,
					id = 666f,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.01f
				});
				Game1.currentLightSources.Add(new LightSource(4, new Vector2(27f, 14f) * 64f, 2f, LightSource.LightContext.None, 0L));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(585, 1846, 26, 22), 999999f, 1, 99999, new Vector2(25f, 12f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 0.001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 96f
				});
				AmbientLocationSounds.addSound(new Vector2(27f, 14f), 1);
				break;
			case "curtainOpen":
				location.getTemporarySpriteByID(999).sourceRect.X = 672;
				Game1.playSound("shwip");
				break;
			case "curtainClose":
				location.getTemporarySpriteByID(999).sourceRect.X = 644;
				Game1.playSound("shwip");
				break;
			case "ClothingTherapy":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(644, 1405, 28, 46), 999999f, 1, 99999, new Vector2(5f, 6f) * 64f + new Vector2(-32f, -144f), flicker: false, flipped: false, 0.0424f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "EmilySongBackLights":
			{
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				for (int lightcolumns = 0; lightcolumns < 5; lightcolumns++)
				{
					for (int yPos = 0; yPos < Game1.graphics.GraphicsDevice.Viewport.Height + 48; yPos += 48)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(681, 1890, 18, 12), 42241f, 1, 1, new Vector2((lightcolumns + 1) * Game1.graphics.GraphicsDevice.Viewport.Width / 5 - Game1.graphics.GraphicsDevice.Viewport.Width / 7, -24 + yPos), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 1760f,
							xPeriodicRange = 128 + yPos / 12 * 4,
							delayBeforeAnimationStart = lightcolumns * 100 + yPos / 4,
							local = true
						});
					}
				}
				for (int numFlyers = 0; numFlyers < 27; numFlyers++)
				{
					int flyerNumber = 0;
					int yPos2 = Game1.random.Next(64, Game1.graphics.GraphicsDevice.Viewport.Height - 64);
					int loopTime = Game1.random.Next(800, 2000);
					int loopRange = Game1.random.Next(32, 64);
					bool pulse = Game1.random.NextDouble() < 0.25;
					int speed = Game1.random.Next(-6, -3);
					for (int tails = 0; tails < 8; tails++)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(616 + flyerNumber * 10, 1891, 10, 10), 42241f, 1, 1, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, yPos2), flicker: false, flipped: false, 0.01f, 0f, Color.White * (1f - (float)tails * 0.11f), 4f, 0f, 0f, 0f)
						{
							yPeriodic = true,
							motion = new Vector2(speed, 0f),
							yPeriodicLoopTime = loopTime,
							pulse = pulse,
							pulseTime = 440f,
							pulseAmount = 1.5f,
							yPeriodicRange = loopRange,
							delayBeforeAnimationStart = 14000 + numFlyers * 900 + tails * 100,
							local = true
						});
					}
				}
				for (int numRainbows2 = 0; numRainbows2 < 15; numRainbows2++)
				{
					int it = 0;
					int yPos3 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width - 128);
					for (int xPos = Game1.graphics.GraphicsDevice.Viewport.Height; xPos >= -64; xPos -= 48)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(yPos3, xPos), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, -(float)Math.PI / 2f, 0f)
						{
							delayBeforeAnimationStart = 27500 + numRainbows2 * 880 + it * 25,
							local = true
						});
						it++;
					}
				}
				for (int numRainbows = 0; numRainbows < 120; numRainbows++)
				{
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(626 + numRainbows / 28 * 10, 1891, 10, 10), 2000f, 1, 1, new Vector2(Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Width), Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height)), flicker: false, flipped: false, 0.01f, 0f, Color.White, 0.1f, 0f, 0f, 0f)
					{
						motion = new Vector2(0f, -2f),
						alphaFade = 0.002f,
						scaleChange = 0.5f,
						scaleChangeChange = -0.0085f,
						delayBeforeAnimationStart = 27500 + numRainbows * 110,
						local = true
					});
				}
				break;
			}
			case "EmilyBoomBoxStart":
				location.getTemporarySpriteByID(999).pulse = true;
				location.getTemporarySpriteByID(999).pulseTime = 420f;
				break;
			case "EmilyBoomBoxStop":
				location.getTemporarySpriteByID(999).pulse = false;
				location.getTemporarySpriteByID(999).scale = 4f;
				break;
			case "EmilyBoomBox":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(586, 1871, 24, 14), 99999f, 1, 99999, new Vector2(15f, 4f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "parrotGone":
				location.removeTemporarySpritesWithID(666);
				break;
			case "parrotSlide":
				location.getTemporarySpriteByID(666).yStopCoordinate = 5632;
				location.getTemporarySpriteByID(666).motion.X = 0f;
				location.getTemporarySpriteByID(666).motion.Y = 1f;
				break;
			case "parrotSplat":
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.viewport.X + Game1.graphics.GraphicsDevice.Viewport.Width, Game1.viewport.Y + 64), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f,
					motion = new Vector2(-2f, 4f),
					acceleration = new Vector2(-0.1f, 0f),
					delayBeforeAnimationStart = 0,
					yStopCoordinate = 5568,
					xStopCoordinate = 1504,
					reachedStopCoordinate = parrotSplat
				});
				break;
			case "parrots1":
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 256f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 32f,
					delayBeforeAnimationStart = 0,
					local = true
				});
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 32f,
					delayBeforeAnimationStart = 600,
					local = true
				});
				aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 165, 24, 22), 100f, 6, 9999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 320f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 32f,
					delayBeforeAnimationStart = 1200,
					local = true
				});
				break;
			case "EmilySign":
			{
				int iter2 = 0;
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				for (int numRainbows3 = 0; numRainbows3 < 10; numRainbows3++)
				{
					iter2 = 0;
					int yPos4 = Game1.random.Next(Game1.graphics.GraphicsDevice.Viewport.Height - 128);
					for (int xPos2 = Game1.graphics.GraphicsDevice.Viewport.Width; xPos2 >= -64; xPos2 -= 48)
					{
						aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(597, 1888, 16, 16), 99999f, 1, 99999, new Vector2(xPos2, yPos4), flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
						{
							delayBeforeAnimationStart = numRainbows3 * 600 + iter2 * 25,
							startSound = ((iter2 == 0) ? "dwoop" : null),
							local = true
						});
						iter2++;
					}
				}
				break;
			}
			case "EmilySleeping":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(574, 1892, 11, 11), 1000f, 2, 99999, new Vector2(20f, 3f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "shaneHospital":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 10), 99999f, 1, 99999, new Vector2(20f, 3f) * 64f + new Vector2(16f, 12f), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "shaneCliffs":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(83f, 98f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(84f, 99f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(82f, 98f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(83f, 99f) * 64f + new Vector2(-8f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "shaneCliffProps":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(549, 1891, 19, 12), 99999f, 1, 99999, new Vector2(104f, 96f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				break;
			case "shaneThrowCan":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(542, 1893, 4, 6), 99999f, 1, 99999, new Vector2(103f, 95f) * 64f + new Vector2(0f, 4f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -4f),
					acceleration = new Vector2(0f, 0.25f),
					rotationChange = (float)Math.PI / 128f
				});
				Game1.playSound("shwip");
				break;
			case "jasGift":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), 100f, 6, 1, new Vector2(22f, 16f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f,
					paused = true,
					holdLastFrame = true
				});
				break;
			case "jasGiftOpen":
				location.getTemporarySpriteByID(999).paused = false;
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(537, 1850, 11, 10), 1500f, 1, 1, new Vector2(23f, 16f) * 64f + new Vector2(16f, -48f), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.25f),
					delayBeforeAnimationStart = 500,
					yStopCoordinate = 928
				});
				location.temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle(1440, 992, 128, 64), 5, Color.White, 300));
				break;
			case "waterShaneDone":
				farmer.completelyStopAnimatingOrDoingAction();
				farmer.TemporaryItem = null;
				drawTool = false;
				location.removeTemporarySpritesWithID(999);
				break;
			case "waterShane":
				drawTool = true;
				farmer.TemporaryItem = new WateringCan();
				farmer.CurrentTool.Update(1, 0, farmer);
				farmer.FarmerSprite.animateOnce(new FarmerSprite.AnimationFrame[4]
				{
					new FarmerSprite.AnimationFrame(58, 0, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(58, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect),
					new FarmerSprite.AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true),
					new FarmerSprite.AnimationFrame(45, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true)
				});
				break;
			case "shanePassedOut":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(533, 1864, 19, 27), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 999f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(552, 1862, 31, 21), 99999f, 1, 99999, new Vector2(25f, 7f) * 64f + new Vector2(-16f, 0f), flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "morrisFlying":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(105, 1318, 13, 31), 9999f, 1, 99999, new Vector2(32f, 13f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(4f, -8f),
					rotationChange = (float)Math.PI / 16f,
					shakeIntensity = 1f
				});
				break;
			case "grandpaNight":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 1f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.Cyan, 4f, 0f, 0f, 0f, local: true)
				{
					alpha = 0.01f,
					alphaFade = -0.002f,
					local = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1453, 639, 176), 9999f, 1, 999999, new Vector2(0f, 768f), flicker: false, flipped: true, 0.9f, 0f, Color.Blue, 4f, 0f, 0f, 0f, local: true)
				{
					alpha = 0.01f,
					alphaFade = -0.002f,
					local = true
				});
				break;
			case "doneWithSlideShow":
				(location as Summit).isShowingEndSlideshow = false;
				break;
			case "getEndSlideshow":
			{
				Summit summit = location as Summit;
				string[] s = summit.getEndSlideshow().Split('/');
				List<string> commandsList = eventCommands.ToList();
				commandsList.InsertRange(CurrentCommand + 1, s);
				eventCommands = commandsList.ToArray();
				summit.isShowingEndSlideshow = true;
				break;
			}
			case "krobusraven":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 0, 32, 32), 100f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					pingPong = true,
					motion = new Vector2(-2f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 3000f,
					yPeriodicRange = 16f,
					startSound = "shadowpeep"
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 32, 32, 32), 30f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2800f,
					yPeriodicRange = 16f,
					delayBeforeAnimationStart = 8000
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(0, 64, 32, 39), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					pingPong = true,
					motion = new Vector2(-4f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 16f,
					delayBeforeAnimationStart = 15000,
					startSound = "fireball"
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-4f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2200f,
					yPeriodicRange = 32f,
					local = true,
					delayBeforeAnimationStart = 20000
				});
				for (int i3 = 0; i3 < 12; i3++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(16, 594, 16, 12), 100f, 2, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = Game1.random.Next(1500, 2000),
						yPeriodicRange = 32f,
						local = true,
						delayBeforeAnimationStart = 24000 + i3 * 200,
						startSound = ((i3 == 0) ? "yoba" : null)
					});
				}
				int whenToStart2 = 0;
				if (Game1.player.mailReceived.Contains("Capsule_Broken"))
				{
					for (int i2 = 0; i2 < 3; i2++)
					{
						location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(639, 785, 16, 16), 100f, 4, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, (float)Game1.viewport.Height * 0.33f + (float)Game1.random.Next(-128, 128)), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(-2f, 0f),
							yPeriodic = true,
							yPeriodicLoopTime = Game1.random.Next(1500, 2000),
							yPeriodicRange = 16f,
							local = true,
							delayBeforeAnimationStart = 30000 + i2 * 500,
							startSound = ((i2 == 0) ? "UFO" : null)
						});
					}
					whenToStart2 += 5000;
				}
				if (Game1.year <= 1)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 8f,
						delayBeforeAnimationStart = 30000 + whenToStart2
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(2, 129, 120, 27), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 8f,
						startSound = "discoverMineral",
						delayBeforeAnimationStart = 30000 + whenToStart2
					});
					whenToStart2 += 5000;
				}
				else if (Game1.year <= 2)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(150, 259, 9, 9), 10f, 4, 9999999, new Vector2(Game1.viewport.Width + 4, (float)Game1.viewport.Height * 0.33f + 44f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 8f,
						delayBeforeAnimationStart = 30000 + whenToStart2
					});
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(1, 104, 100, 24), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 8f,
						startSound = "newArtifact",
						delayBeforeAnimationStart = 30000 + whenToStart2
					});
					whenToStart2 += 5000;
				}
				if (Game1.MasterPlayer.totalMoneyEarned >= 100000000)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\KrobusRaven", new Microsoft.Xna.Framework.Rectangle(125, 108, 34, 50), 1090f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.33f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-2f, 0f),
						yPeriodic = true,
						yPeriodicLoopTime = 3000f,
						yPeriodicRange = 8f,
						startSound = "discoverMineral",
						delayBeforeAnimationStart = 30000 + whenToStart2
					});
					whenToStart2 += 5000;
				}
				break;
			}
			case "grandpaThumbsUp":
			{
				TemporaryAnimatedSprite temporarySpriteByID2 = location.getTemporarySpriteByID(77777);
				temporarySpriteByID2.texture = Game1.mouseCursors2;
				temporarySpriteByID2.sourceRect = new Microsoft.Xna.Framework.Rectangle(186, 265, 22, 34);
				temporarySpriteByID2.sourceRectStartingPos = new Vector2(186f, 265f);
				temporarySpriteByID2.yPeriodic = true;
				temporarySpriteByID2.yPeriodicLoopTime = 1000f;
				temporarySpriteByID2.yPeriodicRange = 16f;
				temporarySpriteByID2.xPeriodicLoopTime = 2500f;
				temporarySpriteByID2.xPeriodicRange = 16f;
				temporarySpriteByID2.initialPosition = temporarySpriteByID2.position;
				break;
			}
			case "grandpaSpirit":
			{
				TemporaryAnimatedSprite p = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(555, 1956, 18, 35), 9999f, 1, 99999, new Vector2(-1000f, -1010f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					yStopCoordinate = -64128,
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					motion = new Vector2(0f, 1f),
					overrideLocationDestroy = true,
					id = 77777f
				};
				location.temporarySprites.Add(p);
				for (int i4 = 0; i4 < 19; i4++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(10, new Vector2(32f, 32f), Color.White)
					{
						parentSprite = p,
						delayBeforeAnimationStart = (i4 + 1) * 500,
						overrideLocationDestroy = true,
						scale = 1f,
						alpha = 1f
					});
				}
				break;
			}
			case "farmerForestVision":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(393, 1973, 1, 1), 9999f, 1, 999999, new Vector2(0f, 0f) * 64f, flicker: false, flipped: false, 0.9f, 0f, Color.LimeGreen * 0.85f, Game1.viewport.Width * 2, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.002f,
					id = 1f
				});
				Game1.player.mailReceived.Add("canReadJunimoText");
				int x = -64;
				int y = -64;
				int index = 0;
				int yIndex = 0;
				while (y < Game1.viewport.Height + 128)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(367 + ((index % 2 == 0) ? 8 : 0), 1969, 8, 8), 9999f, 1, 999999, new Vector2(x, y), flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						alpha = 0f,
						alphaFade = -0.0015f,
						xPeriodic = true,
						xPeriodicLoopTime = 4000f,
						xPeriodicRange = 64f,
						yPeriodic = true,
						yPeriodicLoopTime = 5000f,
						yPeriodicRange = 96f,
						rotationChange = (float)Game1.random.Next(-1, 2) * (float)Math.PI / 256f,
						id = 1f,
						delayBeforeAnimationStart = 20 * index
					});
					x += 128;
					if (x > Game1.viewport.Width + 64)
					{
						yIndex++;
						x = ((yIndex % 2 == 0) ? (-64) : 64);
						y += 128;
					}
					index++;
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 2 - 100, Game1.viewport.Height / 2 - 240), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 6000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 4 - 100, Game1.viewport.Height / 4 - 120), flicker: false, flipped: false, 0.99f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 9000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 3 / 4, Game1.viewport.Height / 3 - 120), flicker: false, flipped: false, 0.98f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 12000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 3 - 60, Game1.viewport.Height * 3 / 4 - 120), flicker: false, flipped: false, 0.97f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 15000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height * 2 / 3 - 120), flicker: false, flipped: false, 0.96f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 18000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width / 8, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.95f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 19500,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(648, 895, 51, 101), 9999f, 1, 999999, new Vector2(Game1.viewport.Width * 2 / 3, Game1.viewport.Height / 5 - 120), flicker: false, flipped: false, 0.94f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					alpha = 0f,
					alphaFade = -0.001f,
					id = 1f,
					delayBeforeAnimationStart = 21000,
					scaleChange = 0.004f,
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 64f,
					yPeriodic = true,
					yPeriodicLoopTime = 5000f,
					yPeriodicRange = 32f
				});
				break;
			}
			case "wizardWarp":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(8f, 16f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(2f, -2f),
					acceleration = new Vector2(0.1f, 0f),
					scaleChange = -0.02f,
					alphaFade = 0.001f
				});
				break;
			case "witchFlyby":
				Game1.screenOverlayTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1886, 35, 29), 9999f, 1, 999999, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width, 192f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-4f, 0f),
					acceleration = new Vector2(-0.025f, 0f),
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 64f,
					local = true
				});
				break;
			case "wizardWarp2":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(387, 1965, 16, 31), 9999f, 1, 999999, new Vector2(54f, 34f) * 64f + new Vector2(0f, 4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-1f, 2f),
					acceleration = new Vector2(-0.1f, 0.2f),
					scaleChange = 0.03f,
					alphaFade = 0.001f
				});
				break;
			case "junimoCageGone":
				location.removeTemporarySpritesWithID(1);
				break;
			case "junimoCageGone2":
				location.removeTemporarySpritesWithID(1);
				Game1.viewportFreeze = true;
				Game1.viewport.X = -1000;
				Game1.viewport.Y = -1000;
				break;
			case "junimoCage":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(325, 1977, 18, 19), 60f, 3, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black,
					id = 1f,
					shakeIntensity = 0f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.5f,
					lightcolor = Color.Black,
					id = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 24f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 24f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, -4f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.5f,
					lightcolor = Color.Black,
					id = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = -24f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 24f,
					delayBeforeAnimationStart = 250
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(0f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.5f,
					lightcolor = Color.Black,
					id = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = -24f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 24f,
					delayBeforeAnimationStart = 450
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(379, 1991, 5, 5), 9999f, 1, 999999, new Vector2(10f, 17f) * 64f + new Vector2(72f, 52f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.5f,
					lightcolor = Color.Black,
					id = 1f,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 24f,
					yPeriodic = true,
					yPeriodicLoopTime = 2000f,
					yPeriodicRange = 24f,
					delayBeforeAnimationStart = 650
				});
				break;
			case "WizardPromise":
				Utility.addSprinklesToLocation(location, 16, 15, 9, 9, 2000, 50, Color.White);
				break;
			case "wizardSewerMagic":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(15f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black,
					alphaFade = 0.005f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 20, new Vector2(17f, 13f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 1f,
					lightcolor = Color.Black,
					alphaFade = 0.005f
				});
				break;
			case "linusCampfire":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), 50f, 4, 99999, new Vector2(29f, 9f) * 64f + new Vector2(8f, 0f), flicker: false, flipped: false, 0.0576f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 3f,
					lightcolor = Color.Black
				});
				break;
			case "linusLights":
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(55f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(60f, 62f) * 64f, 2f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(57f, 60f) * 64f, 3f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(57f, 60f) * 64f, 2f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(47f, 70f) * 64f, 2f, LightSource.LightContext.None, 0L));
				Game1.currentLightSources.Add(new LightSource(2, new Vector2(52f, 63f) * 64f, 2f, LightSource.LightContext.None, 0L));
				break;
			case "wed":
			{
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				Game1.flashAlpha = 1f;
				for (int i5 = 0; i5 < 150; i5++)
				{
					Vector2 position = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.random.Next(Game1.viewport.Height));
					int scale = Game1.random.Next(2, 5);
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(424, 1266, 8, 8), 60f + (float)Game1.random.Next(-10, 10), 7, 999999, position, flicker: false, flipped: false, 0.99f, 0f, Color.White, scale, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.1625f, -0.25f) * scale
					});
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(26f, 64f) * 64f, flicker: false, flipped: false, 0.416f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				Game1.changeMusicTrack("wedding", track_interruptable: false, Game1.MusicContext.Event);
				Game1.musicPlayerVolume = 0f;
				break;
			}
			case "wedding":
				if (farmer.IsMale)
				{
					oldShirt = farmer.shirt;
					farmer.changeShirt(10);
					oldPants = farmer.pantsColor;
					farmer.changePantStyle(0);
					farmer.changePants(new Color(49, 49, 49));
				}
				foreach (Farmer farmerActor in farmerActors)
				{
					if (farmerActor.IsMale)
					{
						farmerActor.changeShirt(10);
						farmerActor.changePants(new Color(49, 49, 49));
						farmerActor.changePantStyle(0);
					}
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1196, 98, 54), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, -64f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(540, 1250, 98, 25), 99999f, 1, 99999, new Vector2(25f, 60f) * 64f + new Vector2(0f, 54f) * 4f + new Vector2(0f, -64f), flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 62f) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(24f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(527, 1249, 12, 25), 99999f, 1, 99999, new Vector2(32f, 69f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "jojaCeremony":
			{
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				for (int i6 = 0; i6 < 16; i6++)
				{
					Vector2 position2 = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + i6 * 64);
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position2, flicker: false, flipped: false, 0.99f, 0f, Color.DeepSkyBlue, 4f, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.25f, -1.5f),
						acceleration = new Vector2(0f, -0.001f)
					});
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position2 + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.25f, -1.5f),
						acceleration = new Vector2(0f, -0.001f)
					});
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1363, 114, 58), 99999f, 1, 99999, new Vector2(50f, 20f) * 64f, flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(48f, 20f) * 64f, flicker: false, flipped: false, 0.157200009f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 200f, 3, 99999, new Vector2(49f, 20f) * 64f, flicker: false, flipped: false, 0.157200009f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 210f, 3, 99999, new Vector2(62f, 20f) * 64f, flicker: false, flipped: false, 0.157200009f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(595, 1387, 14, 34), 190f, 3, 99999, new Vector2(60f, 20f) * 64f, flicker: false, flipped: false, 0.157200009f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				break;
			}
			case "ccCelebration":
			{
				aboveMapSprites = new List<TemporaryAnimatedSprite>();
				for (int i7 = 0; i7 < 32; i7++)
				{
					Vector2 position3 = new Vector2(Game1.random.Next(Game1.viewport.Width - 128), Game1.viewport.Height + i7 * 64);
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(534, 1413, 11, 16), 99999f, 1, 99999, position3, flicker: false, flipped: false, 1f, 0f, Utility.getRandomRainbowColor(), 4f, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.25f, -1.5f),
						acceleration = new Vector2(0f, -0.001f)
					});
					aboveMapSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(545, 1413, 11, 34), 99999f, 1, 99999, position3 + new Vector2(0f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						local = true,
						motion = new Vector2(0.25f, -1.5f),
						acceleration = new Vector2(0f, -0.001f)
					});
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(558, 1425, 20, 26), 400f, 3, 99999, new Vector2(53f, 21f) * 64f, flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					pingPong = true
				});
				break;
			}
			case "dickBag":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(528, 1435, 16, 16), 99999f, 1, 99999, new Vector2(48f, 7f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "dickGlitter":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 200
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 300
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 100
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1435, 16, 16), 100f, 6, 99999, new Vector2(47f, 8f) * 64f + new Vector2(16f, 16f), flicker: false, flipped: false, 1f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 400
				});
				break;
			case "iceFishingCatch":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(68f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 510f, 3, 99999, new Vector2(74f, 30f) * 64f, flicker: false, flipped: false, 0.1984f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 490f, 3, 99999, new Vector2(67f, 36f) * 64f, flicker: false, flipped: false, 0.2368f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(160, 368, 16, 32), 500f, 3, 99999, new Vector2(76f, 35f) * 64f, flicker: false, flipped: false, 0.2304f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "secretGift":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(288, 1231, 16, 16), new Vector2(30f, 70f) * 64f + new Vector2(0f, -21f), flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 999999f,
					id = 666f,
					scale = 4f
				});
				break;
			case "secretGiftOpen":
			{
				TemporaryAnimatedSprite t = location.getTemporarySpriteByID(666);
				if (t != null)
				{
					t.animationLength = 6;
					t.interval = 100f;
					t.totalNumberOfLoops = 1;
					t.timer = 0f;
					t.holdLastFrame = true;
				}
				break;
			}
			case "moonlightJellies":
				if (npcControllers != null)
				{
					npcControllers.Clear();
				}
				underwaterSprites = new List<TemporaryAnimatedSprite>();
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(26f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 10000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(29f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(31f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2624,
					delayBeforeAnimationStart = 12000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1728,
					delayBeforeAnimationStart = 14000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1856,
					delayBeforeAnimationStart = 19500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2048,
					delayBeforeAnimationStart = 20300,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(17f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2496,
					delayBeforeAnimationStart = 21500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(16f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2816,
					delayBeforeAnimationStart = 22400,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(12f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2688,
					delayBeforeAnimationStart = 23200,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(9f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2752,
					delayBeforeAnimationStart = 24000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(18f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1920,
					delayBeforeAnimationStart = 24600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 25600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2496,
					delayBeforeAnimationStart = 26900,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(21f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2176,
					delayBeforeAnimationStart = 28000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(20f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2240,
					delayBeforeAnimationStart = 28500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(22f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2304,
					delayBeforeAnimationStart = 28500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(33f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2752,
					delayBeforeAnimationStart = 29000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(36f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2752,
					delayBeforeAnimationStart = 30000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 32, 16, 16), 250f, 3, 9999, new Vector2(28f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-0.5f, -0.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 4000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 2f,
					xStopCoordinate = 1216,
					yStopCoordinate = 2432,
					delayBeforeAnimationStart = 32000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(40f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 10000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(42f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2752,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(43f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2624,
					delayBeforeAnimationStart = 12000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(45f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2496,
					delayBeforeAnimationStart = 14000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(46f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1856,
					delayBeforeAnimationStart = 19500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(48f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2240,
					delayBeforeAnimationStart = 20300,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(49f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 21500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1920,
					delayBeforeAnimationStart = 22400,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2112,
					delayBeforeAnimationStart = 23200,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2432,
					delayBeforeAnimationStart = 24000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(53f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2240,
					delayBeforeAnimationStart = 24600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(54f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1920,
					delayBeforeAnimationStart = 25600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(55f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 26900,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(4f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 1920,
					delayBeforeAnimationStart = 24000,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(5f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2560,
					delayBeforeAnimationStart = 24600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(3f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2176,
					delayBeforeAnimationStart = 25600,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(6f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2368,
					delayBeforeAnimationStart = 26900,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(256, 16, 16, 16), 250f, 3, 9999, new Vector2(8f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = 3000f,
					xPeriodicRange = 16f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2688,
					delayBeforeAnimationStart = 26900,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(50f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2688,
					delayBeforeAnimationStart = 28500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(51f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2752,
					delayBeforeAnimationStart = 28500,
					pingPong = true
				});
				underwaterSprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(304, 16, 16, 16), 200f, 3, 9999, new Vector2(52f, 49f) * 64f, flicker: false, flipped: false, 0.1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -1.5f),
					xPeriodic = true,
					xPeriodicLoopTime = 2500f,
					xPeriodicRange = 10f,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 1f,
					yStopCoordinate = 2816,
					delayBeforeAnimationStart = 29000,
					pingPong = true
				});
				break;
			case "candleBoatMove":
				location.getTemporarySpriteByID(1).motion = new Vector2(0f, 2f);
				break;
			case "candleBoat":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(240, 112, 16, 32), 1000f, 2, 99999, new Vector2(22f, 36f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 1f,
					light = true,
					lightRadius = 2f,
					lightcolor = Color.Black
				});
				break;
			case "linusMoney":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1002f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 10,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1003f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 100,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-999f, -1000f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 200,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1004f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 300,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-1001f, -998f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 400,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -999f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 500,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-998f, -1002f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 600,
					overrideLocationDestroy = true
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(397, 1941, 19, 20), 9999f, 1, 99999, new Vector2(-997f, -1001f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					startSound = "money",
					delayBeforeAnimationStart = 700,
					overrideLocationDestroy = true
				});
				break;
			case "joshDinner":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(649, 9999f, 1, 9999, new Vector2(6f, 4f) * 64f + new Vector2(8f, 32f), flicker: false, flipped: false)
				{
					layerDepth = 0.0256f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(664, 9999f, 1, 9999, new Vector2(8f, 4f) * 64f + new Vector2(-8f, 32f), flicker: false, flipped: false)
				{
					layerDepth = 0.0256f
				});
				break;
			case "alexDiningDog":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(7f, 2f) * 64f + new Vector2(2f, -8f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 1f
				});
				break;
			case "JoshMom":
			{
				TemporaryAnimatedSprite parent = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(416, 1931, 58, 65), 750f, 2, 99999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					alpha = 0.6f,
					local = true,
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 32f,
					motion = new Vector2(0f, -1.25f),
					initialPosition = new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height)
				};
				location.temporarySprites.Add(parent);
				for (int i8 = 0; i8 < 19; i8++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(516, 1916, 7, 10), 99999f, 1, 99999, new Vector2(64f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						alphaFade = 0.01f,
						local = true,
						motion = new Vector2(-1f, -1f),
						parentSprite = parent,
						delayBeforeAnimationStart = (i8 + 1) * 1000
					});
				}
				break;
			}
			case "joshSteak":
				location.temporarySprites.Clear();
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1936, 12, 20), 80f, 4, 99999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 1f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(497, 1918, 11, 11), 999f, 1, 9999, new Vector2(50f, 68f) * 64f + new Vector2(32f, -8f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "joshDog":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1916, 12, 20), 500f, 6, 9999, new Vector2(53f, 67f) * 64f + new Vector2(3f, 3f) * 4f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 1f
				});
				break;
			case "joshFootball":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1916, 14, 8), 40f, 6, 9999, new Vector2(25f, 16f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					rotation = -(float)Math.PI / 4f,
					rotationChange = (float)Math.PI / 200f,
					motion = new Vector2(6f, -4f),
					acceleration = new Vector2(0f, 0.2f),
					xStopCoordinate = 1856,
					reachedStopCoordinate = catchFootball,
					layerDepth = 1f,
					id = 56232f
				});
				break;
			case "skateboardFly":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1875, 16, 6), 9999f, 1, 999, new Vector2(26f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					rotationChange = (float)Math.PI / 24f,
					motion = new Vector2(-8f, -10f),
					acceleration = new Vector2(0.02f, 0.3f),
					yStopCoordinate = 5824,
					xStopCoordinate = 1024,
					layerDepth = 1f
				});
				break;
			case "samSkate1":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0), 9999f, 1, 999, new Vector2(12f, 90f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(4f, 0f),
					acceleration = new Vector2(-0.008f, 0f),
					xStopCoordinate = 1344,
					reachedStopCoordinate = samPreOllie,
					attachedCharacter = getActorByName("Sam"),
					id = 92473f
				});
				break;
			case "beachStuff":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1887, 47, 29), 9999f, 1, 999, new Vector2(44f, 21f) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "dropEgg":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(176, 800f, 1, 0, new Vector2(6f, 4f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
				{
					rotationChange = (float)Math.PI / 24f,
					motion = new Vector2(0f, -7f),
					acceleration = new Vector2(0f, 0.3f),
					endFunction = eggSmashEndFunction,
					layerDepth = 1f
				});
				break;
			case "balloonBirds":
			{
				int positionOffset = 0;
				if (split != null && split.Length > 2)
				{
					positionOffset = Convert.ToInt32(split[2]);
				}
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 12) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1500
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 13) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1250
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 14) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1100
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, positionOffset + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1000
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 16) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1080
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 17) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1300
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 18) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 1450
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 15) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-4f, 0f),
					delayBeforeAnimationStart = 5450
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 10) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 500
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 11) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 250
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 12) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 100
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(45f, positionOffset + 13) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f)
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(46f, positionOffset + 14) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 80
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(47f, positionOffset + 15) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 300
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(48f, positionOffset + 16) * 64f, flicker: false, flipped: false, 0f, 0f, Color.White, 2f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f),
					delayBeforeAnimationStart = 450
				});
				break;
			}
			case "marcelloLand":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1183, 84, 160), 10000f, 1, 99999, (new Vector2(25f, 19f) + eventPositionTileOffset) * 64f + new Vector2(-23f, 0f) * 4f, flicker: false, flipped: false, 2E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, 2f),
					yStopCoordinate = (41 + (int)eventPositionTileOffset.Y) * 64 - 640,
					reachedStopCoordinate = marcelloBalloonLand,
					attachedCharacter = getActorByName("Marcello"),
					id = 1f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(84, 1205, 38, 26), 10000f, 1, 99999, (new Vector2(25f, 19f) + eventPositionTileOffset) * 64f + new Vector2(0f, 134f) * 4f, flicker: false, flipped: false, (41f + eventPositionTileOffset.Y) * 64f / 10000f + 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, 2f),
					id = 2f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(24, 1343, 36, 19), 7000f, 1, 99999, (new Vector2(25f, 40f) + eventPositionTileOffset) * 64f, flicker: false, flipped: false, 1E-05f, 0f, Color.White, 0f, 0f, 0f, 0f)
				{
					scaleChange = 0.01f,
					id = 3f
				});
				break;
			case "elliottBoat":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(461, 1843, 32, 51), 1000f, 2, 9999, new Vector2(15f, 26f) * 64f + new Vector2(-28f, 0f), flicker: false, flipped: false, 0.1664f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "sebastianRide":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1843, 14, 9), 40f, 4, 999, new Vector2(19f, 8f) * 64f + new Vector2(0f, 28f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(-2f, 0f)
				});
				break;
			case "sebastianOnBike":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 80f, 8, 9999, new Vector2(19f, 27f) * 64f + new Vector2(32f, -16f), flicker: false, flipped: true, 0.1792f, 0f, Color.White, 1f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(405, 1854, 47, 33), 9999f, 1, 999, new Vector2(17f, 27f) * 64f + new Vector2(0f, -8f), flicker: false, flipped: false, 0.1792f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "umbrella":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(324, 1843, 27, 23), 80f, 3, 9999, new Vector2(12f, 39f) * 64f + new Vector2(-20f, -104f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "sebastianGarage":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1843, 48, 42), 9999f, 1, 999, new Vector2(17f, 23f) * 64f + new Vector2(0f, 8f), flicker: false, flipped: false, 0.1472f, 0f, Color.White, 4f, 0f, 0f, 0f));
				getActorByName("Sebastian").HideShadow = true;
				break;
			case "leahLaptop":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(130, 1849, 19, 19), 9999f, 1, 999, new Vector2(12f, 10f) * 64f + new Vector2(0f, 24f), flicker: false, flipped: false, 0.1856f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "leahPicnic":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(96, 1808, 32, 48), 9999f, 1, 999, new Vector2(75f, 37f) * 64f, flicker: false, flipped: false, 0.2496f, 0f, Color.White, 4f, 0f, 0f, 0f));
				NPC n3 = new NPC(new AnimatedSprite(festivalContent, "Characters\\" + (farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(-100f, -100f) * 64f, 2, "LeahEx");
				actors.Add(n3);
				break;
			}
			case "leahShow":
			{
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(29f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.377500027f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(112, 656, 16, 64), 9999f, 1, 999, new Vector2(29f, 56f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 59f) * 64f - new Vector2(0f, 16f), flicker: false, flipped: false, 0.377500027f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(128, 688, 16, 32), 9999f, 1, 999, new Vector2(33f, 58f) * 64f, flicker: false, flipped: false, 0.3776f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(160, 656, 32, 64), 9999f, 1, 999, new Vector2(29f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(144, 688, 16, 32), 9999f, 1, 999, new Vector2(34f, 63f) * 64f, flicker: false, flipped: false, 0.4031f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(113, 592, 16, 64), 100f, 4, 99999, new Vector2(34f, 60f) * 64f, flicker: false, flipped: false, 0.4032f, 0f, Color.White, 4f, 0f, 0f, 0f));
				NPC n3 = new NPC(new AnimatedSprite(festivalContent, "Characters\\" + (farmer.IsMale ? "LeahExMale" : "LeahExFemale"), 0, 16, 32), new Vector2(46f, 57f) * 64f, 2, "LeahEx");
				actors.Add(n3);
				break;
			}
			case "leahTree":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(744, 999999f, 1, 0, new Vector2(42f, 8f) * 64f, flicker: false, flipped: false));
				break;
			case "haleyRoomDark":
				Game1.currentLightSources.Clear();
				Game1.ambientLight = new Color(200, 200, 100);
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(743, 999999f, 1, 0, new Vector2(4f, 1f) * 64f, flicker: false, flipped: false)
				{
					light = true,
					lightcolor = new Color(0, 255, 255),
					lightRadius = 2f
				});
				break;
			case "pennyCook":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
				{
					layerDepth = 1f,
					animationLength = 6,
					interval = 75f,
					motion = new Vector2(0f, -0.5f)
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(16f, 0f), flipped: false, 0f, Color.White)
				{
					layerDepth = 0.1f,
					animationLength = 6,
					interval = 75f,
					motion = new Vector2(0f, -0.5f),
					delayBeforeAnimationStart = 500
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f + new Vector2(-16f, 0f), flipped: false, 0f, Color.White)
				{
					layerDepth = 1f,
					animationLength = 6,
					interval = 75f,
					motion = new Vector2(0f, -0.5f),
					delayBeforeAnimationStart = 750
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(256, 1856, 64, 128), new Vector2(10f, 6f) * 64f, flipped: false, 0f, Color.White)
				{
					layerDepth = 0.1f,
					animationLength = 6,
					interval = 75f,
					motion = new Vector2(0f, -0.5f),
					delayBeforeAnimationStart = 1000
				});
				break;
			case "pennyFieldTrip":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 1813, 86, 54), 999999f, 1, 0, new Vector2(68f, 44f) * 64f, flicker: false, flipped: false, 0.0001f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "pennyMess":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(739, 999999f, 1, 0, new Vector2(10f, 5f) * 64f, flicker: false, flipped: false));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(740, 999999f, 1, 0, new Vector2(15f, 5f) * 64f, flicker: false, flipped: false));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(741, 999999f, 1, 0, new Vector2(16f, 6f) * 64f, flicker: false, flipped: false));
				break;
			case "heart":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, OffsetPosition(new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3]))) * 64f + new Vector2(-16f, -16f), flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					motion = new Vector2(0f, -0.5f),
					alphaFade = 0.01f
				});
				break;
			case "robot":
			{
				TemporaryAnimatedSprite parent2 = new TemporaryAnimatedSprite(getActorByName("robot").Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(35, 42, 35, 42), 50f, 1, 9999, new Vector2(13f, 27f) * 64f - new Vector2(0f, 32f), flicker: false, flipped: false, 0.98f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					acceleration = new Vector2(0f, -0.01f),
					accelerationChange = new Vector2(0f, -0.0001f)
				};
				location.temporarySprites.Add(parent2);
				for (int i11 = 0; i11 < 420; i11++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 64, 320, 64, 64), new Vector2(Game1.random.Next(96), 136f), flipped: false, 0.01f, Color.White * 0.75f)
					{
						layerDepth = 1f,
						delayBeforeAnimationStart = i11 * 10,
						animationLength = 1,
						currentNumberOfLoops = 0,
						interval = 9999f,
						motion = new Vector2(Game1.random.Next(-100, 100) / (i11 + 20), 0.25f + (float)i11 / 100f),
						parentSprite = parent2
					});
				}
				break;
			}
			case "maruTrapdoor":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1632, 16, 32), 150f, 4, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(688, 1632, 16, 32), 99999f, 1, 0, new Vector2(1f, 5f) * 64f, flicker: false, flipped: false, 0.99f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					delayBeforeAnimationStart = 500
				});
				break;
			case "maruElectrocution":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(432, 1664, 16, 32), 40f, 1, 20, new Vector2(7f, 5f) * 64f - new Vector2(-4f, 8f), flicker: true, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "maruTelescope":
			{
				for (int i12 = 0; i12 < 9; i12++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(256, 1680, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 28), Game1.random.Next(1, 20)) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 8000 + i12 * Game1.random.Next(2000),
						motion = new Vector2(4f, 4f)
					});
				}
				break;
			}
			case "maruBeaker":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(738, 1380f, 1, 0, new Vector2(9f, 14f) * 64f + new Vector2(0f, 32f), flicker: false, flipped: false)
				{
					rotationChange = (float)Math.PI / 24f,
					motion = new Vector2(0f, -7f),
					acceleration = new Vector2(0f, 0.2f),
					endFunction = beakerSmashEndFunction,
					layerDepth = 1f
				});
				break;
			case "abbyOuijaCandles":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(5f, 9f) * 64f, flicker: false, flipped: false)
				{
					light = true,
					lightRadius = 1f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(737, 999999f, 1, 0, new Vector2(7f, 8f) * 64f, flicker: false, flipped: false)
				{
					light = true,
					lightRadius = 1f
				});
				break;
			case "abbyManyBats":
			{
				for (int i9 = 0; i9 < 100; i9++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
					{
						xPeriodic = true,
						xPeriodicLoopTime = Game1.random.Next(1500, 2500),
						xPeriodicRange = Game1.random.Next(64, 192),
						motion = new Vector2(Game1.random.Next(-2, 3), Game1.random.Next(-8, -4)),
						delayBeforeAnimationStart = i9 * 30,
						startSound = ((i9 % 10 == 0 || Game1.random.NextDouble() < 0.1) ? "batScreech" : null)
					});
				}
				for (int i10 = 0; i10 < 100; i10++)
				{
					location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
					{
						motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-8, -4)),
						delayBeforeAnimationStart = 10 + i10 * 30
					});
				}
				break;
			}
			case "abbyOneBat":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(640, 1664, 16, 16), 80f, 4, 9999, new Vector2(23f, 9f) * 64f, flicker: false, flipped: false, 1f, 0.003f, Color.White, 4f, 0f, 0f, 0f)
				{
					xPeriodic = true,
					xPeriodicLoopTime = 2000f,
					xPeriodicRange = 128f,
					motion = new Vector2(0f, -8f)
				});
				break;
			case "swordswipe":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(Convert.ToInt32(split[2]), Convert.ToInt32(split[3])) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
				break;
			case "abbyOuija":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 960, 128, 128), 60f, 4, 0, new Vector2(6f, 9f) * 64f, flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f));
				break;
			case "abbyvideoscreen":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(167, 1714, 19, 14), 100f, 3, 9999, new Vector2(2f, 3f) * 64f + new Vector2(7f, 12f) * 4f, flicker: false, flipped: false, 0.0002f, 0f, Color.White, 4f, 0f, 0f, 0f));
				break;
			case "abbyGraveyard":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(736, 999999f, 1, 0, new Vector2(48f, 86f) * 64f, flicker: false, flipped: false));
				break;
			case "abbyAtLake":
				location.TemporarySprites.Add(new TemporaryAnimatedSprite(735, 999999f, 1, 0, new Vector2(48f, 30f) * 64f, flicker: false, flipped: false)
				{
					light = true,
					lightRadius = 2f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2000f,
					yPeriodicLoopTime = 1600f,
					xPeriodicRange = 32f,
					yPeriodicRange = 21f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 1000f,
					yPeriodicLoopTime = 1600f,
					xPeriodicRange = 16f,
					yPeriodicRange = 21f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2400f,
					yPeriodicLoopTime = 2800f,
					xPeriodicRange = 21f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(48f, 30f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2000f,
					yPeriodicLoopTime = 2400f,
					xPeriodicRange = 16f,
					yPeriodicRange = 16f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2000f,
					yPeriodicLoopTime = 2600f,
					xPeriodicRange = 21f,
					yPeriodicRange = 48f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2000f,
					yPeriodicLoopTime = 2600f,
					xPeriodicRange = 32f,
					yPeriodicRange = 21f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 4000f,
					yPeriodicLoopTime = 5000f,
					xPeriodicRange = 42f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(66f, 34f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 4000f,
					yPeriodicLoopTime = 5500f,
					xPeriodicRange = 32f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2400f,
					yPeriodicLoopTime = 3600f,
					xPeriodicRange = 32f,
					yPeriodicRange = 21f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2500f,
					yPeriodicLoopTime = 3600f,
					xPeriodicRange = 42f,
					yPeriodicRange = 51f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 4500f,
					yPeriodicLoopTime = 3000f,
					xPeriodicRange = 21f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(69f, 28f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 5000f,
					yPeriodicLoopTime = 4500f,
					xPeriodicRange = 64f,
					yPeriodicRange = 48f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(-32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2000f,
					yPeriodicLoopTime = 3000f,
					xPeriodicRange = 32f,
					yPeriodicRange = 21f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 0f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 2900f,
					yPeriodicLoopTime = 3200f,
					xPeriodicRange = 21f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(32f, 32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 4200f,
					yPeriodicLoopTime = 3300f,
					xPeriodicRange = 16f,
					yPeriodicRange = 32f
				});
				location.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(232, 328, 4, 4), 9999999f, 1, 0, new Vector2(72f, 33f) * 64f + new Vector2(0f, -32f), flicker: false, flipped: false, 1f, 0f, Color.White, 1f, 0f, 0f, 0f)
				{
					lightcolor = Color.Orange,
					light = true,
					lightRadius = 0.2f,
					xPeriodic = true,
					yPeriodic = true,
					xPeriodicLoopTime = 5100f,
					yPeriodicLoopTime = 4000f,
					xPeriodicRange = 32f,
					yPeriodicRange = 16f
				});
				break;
			}
		}

		private Microsoft.Xna.Framework.Rectangle skipBounds()
		{
			int scale = 4;
			int width = 22 * scale;
			Microsoft.Xna.Framework.Rectangle skipBounds = new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width - width - 8, Game1.viewport.Height - 64, width, 15 * scale);
			Utility.makeSafe(ref skipBounds);
			return skipBounds;
		}

		public void receiveMouseClick(int x, int y)
		{
			if (!skipped && skippable && skipBounds().Contains(x, y))
			{
				skipped = true;
				skipEvent();
				Game1.freezeControls = false;
			}
		}

		public void skipEvent()
		{
			if (playerControlSequence)
			{
				EndPlayerControlSequence();
			}
			Game1.playSound("drumkit6");
			actorPositionsAfterMove.Clear();
			foreach (NPC i in actors)
			{
				bool ignore_stop_animation = i.Sprite.ignoreStopAnimation;
				i.Sprite.ignoreStopAnimation = true;
				i.Halt();
				i.Sprite.ignoreStopAnimation = ignore_stop_animation;
				resetDialogueIfNecessary(i);
			}
			farmer.Halt();
			farmer.ignoreCollisions = false;
			Game1.exitActiveMenu();
			Game1.dialogueUp = false;
			Game1.dialogueTyping = false;
			Game1.pauseTime = 0f;
			switch (id)
			{
			case -157039427:
				endBehaviors(new string[2]
				{
					"end",
					"islandDepart"
				}, Game1.currentLocation);
				break;
			case -888999:
			{
				Object o = new Object(864, 1)
				{
					specialItem = true
				};
				o.questItem.Value = true;
				Game1.player.addItemByMenuIfNecessary(o);
				Game1.player.addQuest(130);
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			}
			case -666777:
				new List<Item>();
				Game1.player.team.RequestLimitedNutDrops("Birdie", null, 0, 0, 5, 5);
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("gotBirdieReward"))
				{
					Game1.addMailForTomorrow("gotBirdieReward", noLetter: true, sendToEveryone: true);
				}
				if (!Game1.player.craftingRecipes.ContainsKey("Fairy Dust"))
				{
					Game1.player.craftingRecipes.Add("Fairy Dust", 0);
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 6497428:
				endBehaviors(new string[2]
				{
					"end",
					"Leo"
				}, Game1.currentLocation);
				break;
			case -78765:
				endBehaviors(new string[2]
				{
					"end",
					"tunnelDepart"
				}, Game1.currentLocation);
				break;
			case 690006:
				if (Game1.player.hasItemWithNameThatContains("Green Slime Egg") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(680, 1));
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 191393:
				if (Game1.player.hasItemWithNameThatContains("Stardew Hero Trophy") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 116));
				}
				endBehaviors(new string[4]
				{
					"end",
					"position",
					"52",
					"20"
				}, Game1.currentLocation);
				break;
			case 2123343:
				endBehaviors(new string[2]
				{
					"end",
					"newDay"
				}, Game1.currentLocation);
				break;
			case 404798:
				if (Game1.player.hasItemWithNameThatContains("Copper Pan") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Pan());
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 26:
				if (!Game1.player.craftingRecipes.ContainsKey("Wild Bait"))
				{
					Game1.player.craftingRecipes.Add("Wild Bait", 0);
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 611173:
				if (!Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgrade") && !Game1.player.activeDialogueEvents.ContainsKey("pamHouseUpgradeAnonymous"))
				{
					Game1.player.activeDialogueEvents.Add("pamHouseUpgrade", 4);
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 3091462:
				if (Game1.player.hasItemWithNameThatContains("My First Painting") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Furniture(1802, Vector2.Zero));
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 3918602:
				if (Game1.player.hasItemWithNameThatContains("Sam's Boombox") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Furniture(1309, Vector2.Zero));
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 19:
				if (!Game1.player.cookingRecipes.ContainsKey("Cookies"))
				{
					Game1.player.cookingRecipes.Add("Cookies", 0);
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 992553:
				if (!Game1.player.craftingRecipes.ContainsKey("Furnace"))
				{
					Game1.player.craftingRecipes.Add("Furnace", 0);
				}
				if (!Game1.player.hasQuest(11))
				{
					Game1.player.addQuest(11);
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 900553:
				if (!Game1.player.craftingRecipes.ContainsKey("Garden Pot"))
				{
					Game1.player.craftingRecipes.Add("Garden Pot", 0);
				}
				if (Game1.player.hasItemWithNameThatContains("Garden Pot") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 62));
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 980558:
				if (!Game1.player.craftingRecipes.ContainsKey("Mini-Jukebox"))
				{
					Game1.player.craftingRecipes.Add("Mini-Jukebox", 0);
				}
				if (Game1.player.hasItemWithNameThatContains("Mini-Jukebox") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 209));
				}
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			case 60367:
				endBehaviors(new string[2]
				{
					"end",
					"beginGame"
				}, Game1.currentLocation);
				break;
			case 739330:
				if (Game1.player.hasItemWithNameThatContains("Bamboo Pole") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new FishingRod());
				}
				endBehaviors(new string[4]
				{
					"end",
					"position",
					"43",
					"36"
				}, Game1.currentLocation);
				break;
			case 112:
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				Game1.player.mailReceived.Add("canReadJunimoText");
				break;
			case 558292:
				Game1.player.eventsSeen.Remove(2146991);
				endBehaviors(new string[2]
				{
					"end",
					"bed"
				}, Game1.currentLocation);
				break;
			case 100162:
				if (Game1.player.hasItemWithNameThatContains("Rusty Sword") == null)
				{
					Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(0));
				}
				Game1.player.Position = new Vector2(-9999f, -99999f);
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			default:
				endBehaviors(new string[1]
				{
					"end"
				}, Game1.currentLocation);
				break;
			}
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void receiveActionPress(int xTile, int yTile)
		{
			if (xTile != playerControlTargetTile.X || yTile != playerControlTargetTile.Y)
			{
				return;
			}
			string a = playerControlSequenceID;
			if (!(a == "haleyBeach"))
			{
				if (a == "haleyBeach2")
				{
					EndPlayerControlSequence();
					CurrentCommand++;
				}
			}
			else
			{
				props.Clear();
				Game1.playSound("coin");
				playerControlTargetTile = new Point(35, 11);
				playerControlSequenceID = "haleyBeach2";
			}
		}

		public void startSecretSantaEvent()
		{
			playerControlSequence = false;
			playerControlSequenceID = null;
			eventCommands = festivalData["secretSanta"].Split('/');
			doingSecretSanta = true;
			setUpSecretSantaCommands();
			currentCommand = 0;
		}

		public void festivalUpdate(GameTime time)
		{
			Game1.player.team.festivalScoreStatus.UpdateState(string.Concat(Game1.player.festivalScore));
			if (festivalTimer > 0)
			{
				oldTime = festivalTimer;
				festivalTimer -= time.ElapsedGameTime.Milliseconds;
				string a = playerControlSequenceID;
				if (a == "iceFishing")
				{
					if (!Game1.player.UsingTool)
					{
						Game1.player.forceCanMove();
					}
					if (oldTime % 500 < festivalTimer % 500)
					{
						NPC temp3 = getActorByName("Pam");
						temp3.Sprite.sourceRect.Offset(temp3.Sprite.SourceRect.Width, 0);
						if (temp3.Sprite.sourceRect.X >= temp3.Sprite.Texture.Width)
						{
							temp3.Sprite.sourceRect.Offset(-temp3.Sprite.Texture.Width, 0);
						}
						temp3 = getActorByName("Elliott");
						temp3.Sprite.sourceRect.Offset(temp3.Sprite.SourceRect.Width, 0);
						if (temp3.Sprite.sourceRect.X >= temp3.Sprite.Texture.Width)
						{
							temp3.Sprite.sourceRect.Offset(-temp3.Sprite.Texture.Width, 0);
						}
						temp3 = getActorByName("Willy");
						temp3.Sprite.sourceRect.Offset(temp3.Sprite.SourceRect.Width, 0);
						if (temp3.Sprite.sourceRect.X >= temp3.Sprite.Texture.Width)
						{
							temp3.Sprite.sourceRect.Offset(-temp3.Sprite.Texture.Width, 0);
						}
					}
					if (oldTime % 29900 < festivalTimer % 29900)
					{
						getActorByName("Willy").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Willy").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 45900 < festivalTimer % 45900)
					{
						getActorByName("Pam").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Pam").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
					if (oldTime % 59900 < festivalTimer % 59900)
					{
						getActorByName("Elliott").shake(500);
						Game1.playSound("dwop");
						temporaryLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\Festivals", new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), getActorByName("Elliott").Position + new Vector2(0f, -96f), flipped: false, 0.015f, Color.White)
						{
							layerDepth = 1f,
							scale = 4f,
							interval = 9999f,
							motion = new Vector2(0f, -1f)
						});
					}
				}
				if (festivalTimer <= 0)
				{
					Game1.player.Halt();
					a = playerControlSequenceID;
					if (!(a == "eggHunt"))
					{
						if (a == "iceFishing")
						{
							EndPlayerControlSequence();
							eventCommands = festivalData["afterIceFishing"].Split('/');
							currentCommand = 0;
							if (Game1.activeClickableMenu != null)
							{
								Game1.activeClickableMenu.emergencyShutDown();
							}
							Game1.activeClickableMenu = null;
							if (Game1.player.UsingTool && Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod)
							{
								(Game1.player.CurrentTool as FishingRod).doneFishing(Game1.player);
							}
							Game1.screenOverlayTempSprites.Clear();
							Game1.player.forceCanMove();
						}
					}
					else
					{
						EndPlayerControlSequence();
						eventCommands = festivalData["afterEggHunt"].Split('/');
						currentCommand = 0;
					}
				}
			}
			if (startSecretSantaAfterDialogue && !Game1.dialogueUp)
			{
				Game1.globalFadeToBlack(startSecretSantaEvent, 0.01f);
				startSecretSantaAfterDialogue = false;
			}
			Game1.player.festivalScore = Math.Min(Game1.player.festivalScore, 9999);
			if (waitingForMenuClose && Game1.activeClickableMenu == null)
			{
				string a = festivalData["file"];
				_ = (a == "fall16");
				waitingForMenuClose = false;
			}
		}

		private void setUpSecretSantaCommands()
		{
			int secretSantaX = 0;
			int secretSantaY2 = 0;
			try
			{
				secretSantaX = getActorByName(mySecretSanta.Name).getTileX();
				secretSantaY2 = getActorByName(mySecretSanta.Name).getTileY();
			}
			catch (Exception)
			{
				mySecretSanta = getActorByName("Lewis");
				secretSantaX = getActorByName(mySecretSanta.Name).getTileX();
				secretSantaY2 = getActorByName(mySecretSanta.Name).getTileY();
			}
			string dialogue3 = "";
			string dialogue2 = "";
			switch (mySecretSanta.Age)
			{
			case 2:
				dialogue3 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1497");
				dialogue2 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1498");
				break;
			case 0:
			case 1:
				switch (mySecretSanta.Manners)
				{
				case 0:
				case 1:
					dialogue3 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1499");
					dialogue2 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1500");
					break;
				case 2:
					dialogue3 = Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1501");
					dialogue2 = (mySecretSanta.Name.Equals("George") ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1503") : Game1.LoadStringByGender(mySecretSanta.gender, "Strings\\StringsFromCSFiles:Event.cs.1504"));
					break;
				}
				break;
			}
			for (int i = 0; i < eventCommands.Length; i++)
			{
				eventCommands[i] = eventCommands[i].Replace("secretSanta", mySecretSanta.Name);
				eventCommands[i] = eventCommands[i].Replace("warpX", string.Concat(secretSantaX));
				eventCommands[i] = eventCommands[i].Replace("warpY", string.Concat(secretSantaY2));
				eventCommands[i] = eventCommands[i].Replace("dialogue1", dialogue3);
				eventCommands[i] = eventCommands[i].Replace("dialogue2", dialogue2);
			}
		}

		public void drawFarmers(SpriteBatch b)
		{
			foreach (Farmer farmerActor in farmerActors)
			{
				farmerActor.draw(b);
			}
		}

		public virtual bool ShouldHideCharacter(NPC n)
		{
			if (n is Child && doingSecretSanta)
			{
				return true;
			}
			return false;
		}

		public void draw(SpriteBatch b)
		{
			if (currentCustomEventScript != null)
			{
				currentCustomEventScript.draw(b);
				return;
			}
			foreach (NPC j in actors)
			{
				if (!ShouldHideCharacter(j))
				{
					j.Name.Equals("Marcello");
					if (j.ySourceRectOffset == 0)
					{
						j.draw(b);
					}
					else
					{
						j.draw(b, j.ySourceRectOffset);
					}
				}
			}
			foreach (Object prop in props)
			{
				prop.drawAsProp(b);
			}
			foreach (Prop festivalProp in festivalProps)
			{
				festivalProp.draw(b);
			}
			if (isFestival)
			{
				string a = festivalData["file"];
				if (a == "fall16")
				{
					Vector2 start = Game1.GlobalToLocal(Game1.viewport, new Vector2(37f, 56f) * 64f);
					start.X += 4f;
					int xCutoff = (int)start.X + 168;
					start.Y += 8f;
					for (int i = 0; i < Game1.player.team.grangeDisplay.Count; i++)
					{
						if (Game1.player.team.grangeDisplay[i] != null)
						{
							start.Y += 42f;
							start.X += 4f;
							b.Draw(Game1.shadowTexture, start, Game1.shadowTexture.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
							start.Y -= 42f;
							start.X -= 4f;
							Game1.player.team.grangeDisplay[i].drawInMenu(b, start, 1f, 1f, (float)i / 1000f + 0.001f, StackDrawType.Hide);
						}
						start.X += 60f;
						if (start.X >= (float)xCutoff)
						{
							start.X = xCutoff - 168;
							start.Y += 64f;
						}
					}
				}
			}
			if (drawTool)
			{
				Game1.drawTool(farmer);
			}
		}

		public void drawUnderWater(SpriteBatch b)
		{
			if (underwaterSprites != null)
			{
				foreach (TemporaryAnimatedSprite underwaterSprite in underwaterSprites)
				{
					underwaterSprite.draw(b);
				}
			}
		}

		public void drawAfterMap(SpriteBatch b)
		{
			if (aboveMapSprites != null)
			{
				foreach (TemporaryAnimatedSprite aboveMapSprite in aboveMapSprites)
				{
					aboveMapSprite.draw(b);
				}
			}
			if (!Game1.game1.takingMapScreenshot && playerControlSequenceID != null)
			{
				switch (playerControlSequenceID)
				{
				case "eggHunt":
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(32, 32, 224, 160), Color.Black * 0.5f);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1514", festivalTimer / 1000), Color.Black, Color.Yellow, new Vector2(64f, 64f), 0f, 1f, 1f, tiny: false);
					Game1.drawWithBorder(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1515", Game1.player.festivalScore), Color.Black, Color.Pink, new Vector2(64f, 128f), 0f, 1f, 1f, tiny: false);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				case "fair":
					b.End();
					Game1.PushUIMode();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 64), Color.Black * 0.75f);
					b.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Microsoft.Xna.Framework.Rectangle(338, 400, 8, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.drawWithBorder(string.Concat(Game1.player.festivalScore), Color.Black, Color.White, new Vector2(72f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
					if (Game1.activeClickableMenu == null)
					{
						Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.dayTimeMoneyBox.xPositionOnScreen, 4);
					}
					b.End();
					Game1.PopUIMode();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				case "iceFishing":
					b.End();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle(16, 16, 128 + ((Game1.player.festivalScore > 999) ? 16 : 0), 128), Color.Black * 0.75f);
					b.Draw(festivalTexture, new Vector2(32f, 16f), new Microsoft.Xna.Framework.Rectangle(112, 432, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					Game1.drawWithBorder(string.Concat(Game1.player.festivalScore), Color.Black, Color.White, new Vector2(96f, 21 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en) ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8))), 0f, 1f, 1f, tiny: false);
					Game1.drawWithBorder(Utility.getMinutesSecondsStringFromMilliseconds(festivalTimer), Color.Black, Color.White, new Vector2(32f, 93f), 0f, 1f, 1f, tiny: false);
					b.End();
					b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
					if (Game1.IsMultiplayer)
					{
						Game1.player.team.festivalScoreStatus.Draw(b, new Vector2(32f, Game1.viewport.Height - 32), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
					}
					break;
				}
			}
			if (spriteTextToDraw != null && spriteTextToDraw.Length > 0)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, spriteTextToDraw, Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 192, int_useMeForAnything, -1, 999999, 1f, 1f, junimoText: false, int_useMeForAnything2);
			}
			foreach (NPC actor in actors)
			{
				actor.drawAboveAlwaysFrontLayer(b);
			}
			if (skippable && !Game1.options.SnappyMenus && !Game1.game1.takingMapScreenshot)
			{
				Microsoft.Xna.Framework.Rectangle skipBounds = this.skipBounds();
				Color renderCol = Color.White;
				if (skipBounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()))
				{
					renderCol *= 0.5f;
				}
				b.Draw(sourceRectangle: new Microsoft.Xna.Framework.Rectangle(205, 406, 22, 15), texture: Game1.mouseCursors, position: Utility.PointToVector2(skipBounds.Location), color: renderCol, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: 0.92f);
			}
			if (currentCustomEventScript != null)
			{
				currentCustomEventScript.drawAboveAlwaysFront(b);
			}
		}

		public void EndPlayerControlSequence()
		{
			playerControlSequence = false;
			playerControlSequenceID = null;
		}

		public void OnPlayerControlSequenceEnd(string id)
		{
			Game1.player.CanMove = false;
			Game1.player.Halt();
		}

		public void setUpPlayerControlSequence(string id)
		{
			playerControlSequenceID = id;
			playerControlSequence = true;
			Game1.player.CanMove = true;
			Game1.viewportFreeze = false;
			Game1.forceSnapOnNextViewportUpdate = true;
			Game1.globalFade = false;
			doingSecretSanta = false;
			switch (id)
			{
			case "haleyBeach":
				playerControlTargetTile = new Point(53, 8);
				props.Add(new Object(new Vector2(53f, 8f), 742, 1)
				{
					Flipped = false
				});
				Game1.player.canOnlyWalk = false;
				break;
			case "eggFestival":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1521");
				break;
			case "flowerFestival":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1524");
				if (NetWorldState.checkAnywhereForWorldStateID("trashBearDone"))
				{
					Game1.currentLocation.setMapTileIndex(62, 29, -1, "Buildings");
					Game1.currentLocation.setMapTileIndex(64, 29, -1, "Buildings");
					Game1.currentLocation.setMapTileIndex(72, 49, -1, "Buildings");
				}
				break;
			case "luau":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1527");
				break;
			case "jellies":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1531");
				break;
			case "boatRide":
				Game1.viewportFreeze = true;
				Game1.currentViewportTarget = Utility.PointToVector2(Game1.viewportCenter);
				currentCommand++;
				break;
			case "parrotRide":
				Game1.player.canOnlyWalk = false;
				currentCommand++;
				break;
			case "fair":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1535");
				break;
			case "eggHunt":
			{
				for (int x = 0; x < Game1.currentLocation.map.GetLayer("Paths").LayerWidth; x++)
				{
					for (int y = 0; y < Game1.currentLocation.map.GetLayer("Paths").LayerHeight; y++)
					{
						if (Game1.currentLocation.map.GetLayer("Paths").Tiles[x, y] != null)
						{
							festivalProps.Add(new Prop(festivalTexture, Game1.currentLocation.map.GetLayer("Paths").Tiles[x, y].TileIndex, 1, 1, 1, x, y));
						}
					}
				}
				festivalTimer = 52000;
				currentCommand++;
				break;
			}
			case "halloween":
				temporaryLocation.objects.Add(new Vector2(33f, 13f), new Chest(0, new List<Item>
				{
					new Object(373, 1)
				}, new Vector2(33f, 13f)));
				break;
			case "christmas":
			{
				Random r = new Random((int)(Game1.uniqueIDForThisGame / 2uL) ^ Game1.year ^ (int)Game1.player.UniqueMultiplayerID);
				secretSantaRecipient = Utility.getRandomTownNPC(r);
				while (mySecretSanta == null || mySecretSanta.Equals(secretSantaRecipient) || mySecretSanta.isDivorcedFrom(farmer))
				{
					mySecretSanta = Utility.getRandomTownNPC(r);
				}
				Game1.debugOutput = "Secret Santa Recipient: " + secretSantaRecipient.Name + "  My Secret Santa: " + mySecretSanta.Name;
				break;
			}
			case "iceFestival":
				festivalHost = getActorByName("Lewis");
				hostMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1548");
				break;
			case "iceFishing":
				festivalTimer = 120000;
				farmer.festivalScore = 0;
				farmer.CurrentToolIndex = 0;
				farmer.TemporaryItem = new FishingRod();
				(farmer.CurrentTool as FishingRod).attachments[1] = new Object(687, 1);
				farmer.CurrentToolIndex = 0;
				break;
			}
		}

		public bool canMoveAfterDialogue()
		{
			if (playerControlSequenceID != null && playerControlSequenceID.Equals("eggHunt"))
			{
				Game1.player.canMove = true;
				CurrentCommand++;
			}
			return playerControlSequence;
		}

		public void forceFestivalContinue()
		{
			if (festivalData["file"].Equals("fall16"))
			{
				initiateGrangeJudging();
				return;
			}
			Game1.dialogueUp = false;
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu.emergencyShutDown();
			}
			Game1.exitActiveMenu();
			string[] newCommands = eventCommands = GetFestivalDataForYear("mainEvent").Split('/');
			CurrentCommand = 0;
			eventSwitched = true;
			playerControlSequence = false;
			setUpFestivalMainEvent();
			Game1.player.Halt();
		}

		public bool isSpecificFestival(string festivalID)
		{
			if (isFestival)
			{
				return festivalData["file"].Equals(festivalID);
			}
			return false;
		}

		public void setUpFestivalMainEvent()
		{
			if (!isSpecificFestival("spring24"))
			{
				return;
			}
			List<NetDancePartner> females = new List<NetDancePartner>();
			List<NetDancePartner> males = new List<NetDancePartner>();
			List<string> leftoverFemales = new List<string>
			{
				"Abigail",
				"Penny",
				"Leah",
				"Maru",
				"Haley",
				"Emily"
			};
			List<string> leftoverMales = new List<string>
			{
				"Sebastian",
				"Sam",
				"Elliott",
				"Harvey",
				"Alex",
				"Shane"
			};
			List<Farmer> farmers = (from f in Game1.getOnlineFarmers()
				orderby f.UniqueMultiplayerID
				select f).ToList();
			while (farmers.Count > 0)
			{
				Farmer f2 = farmers[0];
				farmers.RemoveAt(0);
				if (Game1.multiplayer.isDisconnecting(f2) || f2.dancePartner.Value == null)
				{
					continue;
				}
				if (f2.dancePartner.GetGender() == 1)
				{
					females.Add(f2.dancePartner);
					if (f2.dancePartner.IsVillager())
					{
						leftoverFemales.Remove(f2.dancePartner.TryGetVillager().Name);
					}
					males.Add(new NetDancePartner(f2));
				}
				else
				{
					males.Add(f2.dancePartner);
					if (f2.dancePartner.IsVillager())
					{
						leftoverMales.Remove(f2.dancePartner.TryGetVillager().Name);
					}
					females.Add(new NetDancePartner(f2));
				}
				if (f2.dancePartner.IsFarmer())
				{
					farmers.Remove(f2.dancePartner.TryGetFarmer());
				}
			}
			while (females.Count < 6)
			{
				string female = leftoverFemales.Last();
				if (leftoverMales.Contains(Utility.getLoveInterest(female)))
				{
					females.Add(new NetDancePartner(female));
					males.Add(new NetDancePartner(Utility.getLoveInterest(female)));
				}
				leftoverFemales.Remove(female);
			}
			string rawFestivalData8 = GetFestivalDataForYear("mainEvent");
			for (int i = 1; i <= 6; i++)
			{
				string female2 = (!females[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(females[i - 1].TryGetFarmer())) : females[i - 1].TryGetVillager().Name;
				string male = (!males[i - 1].IsVillager()) ? ("farmer" + Utility.getFarmerNumberFromFarmer(males[i - 1].TryGetFarmer())) : males[i - 1].TryGetVillager().Name;
				rawFestivalData8 = rawFestivalData8.Replace("Girl" + i, female2);
				rawFestivalData8 = rawFestivalData8.Replace("Guy" + i, male);
			}
			Regex regex = new Regex("showFrame (?<farmerName>farmer\\d) 44");
			Regex showFrameGirl = new Regex("showFrame (?<farmerName>farmer\\d) 40");
			Regex animation1Guy = new Regex("animate (?<farmerName>farmer\\d) false true 600 44 45");
			Regex animation1Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 43 41 43 42");
			Regex animation2Guy = new Regex("animate (?<farmerName>farmer\\d) false true 300 46 47");
			Regex animation2Girl = new Regex("animate (?<farmerName>farmer\\d) false true 600 46 47");
			rawFestivalData8 = regex.Replace(rawFestivalData8, "showFrame $1 12/faceDirection $1 0");
			rawFestivalData8 = showFrameGirl.Replace(rawFestivalData8, "showFrame $1 0/faceDirection $1 2");
			rawFestivalData8 = animation1Guy.Replace(rawFestivalData8, "animate $1 false true 600 12 13 12 14");
			rawFestivalData8 = animation1Girl.Replace(rawFestivalData8, "animate $1 false true 596 4 0");
			rawFestivalData8 = animation2Guy.Replace(rawFestivalData8, "animate $1 false true 150 12 13 12 14");
			rawFestivalData8 = animation2Girl.Replace(rawFestivalData8, "animate $1 false true 600 0 3");
			string[] newCommands = eventCommands = rawFestivalData8.Split('/');
		}

		private void judgeGrange()
		{
			int pointsEarned3 = 14;
			Dictionary<int, bool> categoriesRepresented = new Dictionary<int, bool>();
			int nullsCount = 0;
			bool purpleShorts = false;
			foreach (Item i in Game1.player.team.grangeDisplay)
			{
				if (i != null && i is Object)
				{
					if (IsItemMayorShorts(i as Object))
					{
						purpleShorts = true;
					}
					pointsEarned3 += (i as Object).Quality + 1;
					int num = (i as Object).sellToStorePrice(-1L);
					if (num >= 20)
					{
						pointsEarned3++;
					}
					if (num >= 90)
					{
						pointsEarned3++;
					}
					if (num >= 200)
					{
						pointsEarned3++;
					}
					if (num >= 300 && (i as Object).Quality < 2)
					{
						pointsEarned3++;
					}
					if (num >= 400 && (i as Object).Quality < 1)
					{
						pointsEarned3++;
					}
					switch ((i as Object).Category)
					{
					case -75:
						categoriesRepresented[-75] = true;
						break;
					case -79:
						categoriesRepresented[-79] = true;
						break;
					case -18:
					case -14:
					case -6:
					case -5:
						categoriesRepresented[-5] = true;
						break;
					case -12:
					case -2:
						categoriesRepresented[-12] = true;
						break;
					case -4:
						categoriesRepresented[-4] = true;
						break;
					case -81:
					case -80:
					case -27:
						categoriesRepresented[-81] = true;
						break;
					case -7:
						categoriesRepresented[-7] = true;
						break;
					case -26:
						categoriesRepresented[-26] = true;
						break;
					}
				}
				else if (i == null)
				{
					nullsCount++;
				}
			}
			pointsEarned3 += Math.Min(30, categoriesRepresented.Count * 5);
			int displayFilledPoints = 9 - 2 * nullsCount;
			pointsEarned3 = (grangeScore = pointsEarned3 + displayFilledPoints);
			if (purpleShorts)
			{
				grangeScore = -666;
			}
		}

		private void lewisDoneJudgingGrange()
		{
			if (Game1.activeClickableMenu == null)
			{
				Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1584")));
				Game1.player.Halt();
			}
			interpretGrangeResults();
		}

		public void interpretGrangeResults()
		{
			List<Character> winners = new List<Character>();
			winners.Add(getActorByName("Pierre"));
			winners.Add(getActorByName("Marnie"));
			winners.Add(getActorByName("Willy"));
			if (grangeScore >= 90)
			{
				winners.Insert(0, Game1.player);
			}
			else if (grangeScore >= 75)
			{
				winners.Insert(1, Game1.player);
			}
			else if (grangeScore >= 60)
			{
				winners.Insert(2, Game1.player);
			}
			else
			{
				winners.Add(Game1.player);
			}
			if (winners[0] is NPC && winners[0].Name.Equals("Pierre"))
			{
				getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1591"));
			}
			else
			{
				getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1593"));
			}
			getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1595"));
			getActorByName("Willy").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1597"));
			if (grangeScore == -666)
			{
				getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1600"));
			}
			grangeJudged = true;
		}

		private void initiateGrangeJudging()
		{
			judgeGrange();
			hostMessage = null;
			setUpAdvancedMove("advancedMove Lewis False 2 0 0 7 8 0 4 3000 3 0 4 3000 3 0 4 3000 3 0 4 3000 -14 0 2 1000".Split(' '), lewisDoneJudgingGrange);
			getActorByName("Lewis").CurrentDialogue.Clear();
			setUpAdvancedMove("advancedMove Marnie False 0 1 4 1000".Split(' '));
			getActorByName("Marnie").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1602"));
			getActorByName("Pierre").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1604"));
			getActorByName("Willy").setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1606"));
		}

		public void answerDialogueQuestion(NPC who, string answerKey)
		{
			if (!isFestival)
			{
				return;
			}
			if (!(answerKey == "yes"))
			{
				if (answerKey == "no" || !(answerKey == "danceAsk"))
				{
					return;
				}
				if (Game1.player.spouse != null && who.Name.Equals(Game1.player.spouse))
				{
					Game1.player.dancePartner.Value = who;
					switch (Game1.player.spouse)
					{
					case "Abigail":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1613"));
						break;
					case "Penny":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1615"));
						break;
					case "Maru":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1617"));
						break;
					case "Leah":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1619"));
						break;
					case "Haley":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1621"));
						break;
					case "Sebastian":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1623"));
						break;
					case "Sam":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1625"));
						break;
					case "Harvey":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1627"));
						break;
					case "Elliott":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1629"));
						break;
					case "Alex":
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1631"));
						break;
					default:
						who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1632"));
						break;
					}
					foreach (NPC j in actors)
					{
						if (j.CurrentDialogue != null && j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
						{
							j.CurrentDialogue.Clear();
						}
					}
				}
				else if (!who.HasPartnerForDance && Game1.player.getFriendshipLevelForNPC(who.Name) >= 1000 && !who.isMarried())
				{
					string accept = "";
					switch (who.Gender)
					{
					case 0:
						accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1633");
						break;
					case 1:
						accept = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1634");
						break;
					}
					try
					{
						Game1.player.changeFriendship(250, Game1.getCharacterFromName(who.Name));
					}
					catch (Exception)
					{
					}
					Game1.player.dancePartner.Value = who;
					who.setNewDialogue(accept);
					foreach (NPC i in actors)
					{
						if (i.CurrentDialogue != null && i.CurrentDialogue.Count > 0 && i.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
						{
							i.CurrentDialogue.Clear();
						}
					}
				}
				else if (who.HasPartnerForDance)
				{
					who.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1635"));
				}
				else
				{
					try
					{
						who.setNewDialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + who.Name)["danceRejection"]);
					}
					catch (Exception)
					{
						return;
					}
				}
				Game1.drawDialogue(who);
				who.immediateSpeak = true;
				who.facePlayer(Game1.player);
				who.Halt();
			}
			else if (festivalData["file"].Equals("fall16"))
			{
				initiateGrangeJudging();
				if (Game1.IsServer)
				{
					Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
				}
			}
			else
			{
				string[] newCommands = eventCommands = GetFestivalDataForYear("mainEvent").Split('/');
				CurrentCommand = 0;
				eventSwitched = true;
				playerControlSequence = false;
				setUpFestivalMainEvent();
				if (Game1.IsServer)
				{
					Game1.multiplayer.sendServerToClientsMessage("festivalEvent");
				}
			}
		}

		public void addItemToGrangeDisplay(Item i, int position, bool force)
		{
			while (Game1.player.team.grangeDisplay.Count < 9)
			{
				Game1.player.team.grangeDisplay.Add(null);
			}
			if (position >= 0 && position < Game1.player.team.grangeDisplay.Count && (Game1.player.team.grangeDisplay[position] == null || force))
			{
				Game1.player.team.grangeDisplay[position] = i;
			}
		}

		private bool onGrangeChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
		{
			if (!onRemoval)
			{
				if (i.Stack > 1 || (i.Stack == 1 && old != null && old.Stack == 1 && i.canStackWith(old)))
				{
					if (old != null && i != null && old.canStackWith(i))
					{
						container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
						container.heldItem = old;
						return false;
					}
					if (old != null)
					{
						Utility.addItemToInventory(old, position, container.ItemsToGrabMenu.actualInventory);
						container.heldItem = i;
						return false;
					}
					int allButOne = i.Stack - 1;
					Item reject = i.getOne();
					reject.Stack = allButOne;
					container.heldItem = reject;
					i.Stack = 1;
				}
			}
			else if (old != null && old.Stack > 1 && !old.Equals(i))
			{
				return false;
			}
			addItemToGrangeDisplay((onRemoval && (old == null || old.Equals(i))) ? null : i, position, force: true);
			return true;
		}

		public bool canPlayerUseTool()
		{
			if (festivalData != null && festivalData.ContainsKey("file") && festivalData["file"].Equals("winter8") && festivalTimer > 0 && !Game1.player.UsingTool)
			{
				previousFacingDirection = Game1.player.FacingDirection;
				return true;
			}
			return false;
		}

		public bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (isFestival)
			{
				if (temporaryLocation != null && temporaryLocation.objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
				{
					temporaryLocation.objects[new Vector2(tileLocation.X, tileLocation.Y)].checkForAction(who);
				}
				string tileAction2 = null;
				string tileSheetID2 = "";
				int tileIndex2 = -1;
				tileIndex2 = Game1.currentLocation.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings");
				tileAction2 = Game1.currentLocation.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
				tileSheetID2 = Game1.currentLocation.getTileSheetIDAt(tileLocation.X, tileLocation.Y, "Buildings");
				if (Game1.currentSeason == "winter" && Game1.dayOfMonth == 8 && tileSheetID2 == "fest" && (tileIndex2 == 1009 || tileIndex2 == 1010 || tileIndex2 == 1012 || tileIndex2 == 1013))
				{
					Game1.playSound("pig");
					return true;
				}
				bool success = true;
				switch (tileIndex2)
				{
				case 175:
				case 176:
					if (tileSheetID2 == "untitled tile sheet" && who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.player.eatObject(new Object(241, 1), overrideFullness: true);
					}
					break;
				case 308:
				case 309:
				{
					Response[] colors = new Response[3]
					{
						new Response("Orange", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1645")),
						new Response("Green", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1647")),
						new Response("I", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1650"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1652")), colors, "wheelBet");
					}
					break;
				}
				case 87:
				case 88:
				{
					Response[] responses4 = new Response[2]
					{
						new Response("Buy", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1654")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1656"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1659"), responses4, "StarTokenShop");
					}
					break;
				}
				case 501:
				case 502:
				{
					Response[] responses = new Response[2]
					{
						new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1666"), responses, "slingshotGame");
					}
					break;
				}
				case 510:
				case 511:
					if (!who.IsLocalPlayer || !festivalData["file"].Equals("fall16"))
					{
						break;
					}
					if (festivalShops == null)
					{
						festivalShops = new Dictionary<string, Dictionary<ISalable, int[]>>();
					}
					if (!festivalShops.ContainsKey("starTokenShop"))
					{
						Dictionary<ISalable, int[]> stock2 = new Dictionary<ISalable, int[]>();
						stock2.Add(new Furniture(1307, Vector2.Zero), new int[2]
						{
							100,
							1
						});
						stock2.Add(new Hat(19), new int[2]
						{
							500,
							1
						});
						stock2.Add(new Object(Vector2.Zero, 110), new int[2]
						{
							800,
							1
						});
						if (!Game1.player.mailReceived.Contains("CF_Fair"))
						{
							stock2.Add(new Object(434, 1), new int[2]
							{
								2000,
								1
							});
						}
						stock2.Add(new Furniture(2488, Vector2.Zero), new int[2]
						{
							500,
							1
						});
						switch (new Random((int)Game1.uniqueIDForThisGame + Game1.year + 19).Next(5))
						{
						case 0:
							stock2.Add(new Object(251, 1), new int[2]
							{
								400,
								2
							});
							break;
						case 1:
							stock2.Add(new Object(215, 1), new int[2]
							{
								250,
								2
							});
							break;
						case 2:
							stock2.Add(new Ring(888), new int[2]
							{
								1000,
								1
							});
							break;
						case 3:
							stock2.Add(new Object(178, 100), new int[2]
							{
								500,
								1
							});
							break;
						case 4:
							stock2.Add(new Object(770, 24), new int[2]
							{
								1000,
								1
							});
							break;
						}
						festivalShops.Add("starTokenShop", stock2);
					}
					Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1672"), Game1.currentLocation.createYesNoResponses(), "starTokenShop");
					break;
				case 349:
				case 350:
				case 351:
					if (festivalData["file"].Equals("fall16"))
					{
						Game1.player.team.grangeMutex.RequestLock(delegate
						{
							while (Game1.player.team.grangeDisplay.Count < 9)
							{
								Game1.player.team.grangeDisplay.Add(null);
							}
							Game1.activeClickableMenu = new StorageContainer(Game1.player.team.grangeDisplay.ToList(), 9, 3, onGrangeChange, Utility.highlightSmallObjects);
							waitingForMenuClose = true;
						});
					}
					break;
				case 503:
				case 504:
				{
					Response[] responses2 = new Response[2]
					{
						new Response("Play", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1662")),
						new Response("Leave", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1663"))
					};
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1681"), responses2, "fishingGame");
					}
					break;
				}
				case 540:
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						if (who.getTileX() == 29)
						{
							Game1.activeClickableMenu = new StrengthGame();
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1684")));
						}
					}
					break;
				case 505:
				case 506:
					if (who.IsLocalPlayer && festivalData["file"].Equals("fall16"))
					{
						if (who.Money >= 100 && !who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Response[] responses3 = new Response[2]
							{
								new Response("Read", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1688")),
								new Response("No", Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1690"))
							};
							Game1.currentLocation.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1691")), responses3, "fortuneTeller");
						}
						else if (who.mailReceived.Contains("fortuneTeller" + Game1.year))
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1694")));
						}
						else
						{
							Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1695")));
						}
						who.Halt();
					}
					break;
				default:
					success = false;
					break;
				}
				if (success)
				{
					return true;
				}
				if (tileAction2 != null)
				{
					try
					{
						string[] split = tileAction2.Split(' ');
						string text = split[0];
						switch (text)
						{
						default:
							if (text == "LuauSoup" && !specialEventVariable2)
							{
								Game1.activeClickableMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightLuauSoupItems, clickToAddItemToLuauSoup, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1719"), null, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
							}
							break;
						case "Shop":
						{
							if (!who.IsLocalPlayer)
							{
								return false;
							}
							if (festivalShops == null)
							{
								festivalShops = new Dictionary<string, Dictionary<ISalable, int[]>>();
							}
							Dictionary<ISalable, int[]> stockList2 = null;
							if (!festivalShops.ContainsKey(split[1]))
							{
								_ = split[1];
								string[] inventoryList = festivalData[split[1]].Split(' ');
								stockList2 = new Dictionary<ISalable, int[]>();
								int infiniteStock = int.MaxValue;
								for (int k = 0; k < inventoryList.Length; k += 4)
								{
									string type = inventoryList[k];
									_ = inventoryList[k + 1];
									int index = Convert.ToInt32(inventoryList[k + 1]);
									int price = Convert.ToInt32(inventoryList[k + 2]);
									int stock = Convert.ToInt32(inventoryList[k + 3]);
									Item item = null;
									switch (type)
									{
									case "Object":
									case "O":
									{
										int stack = (stock <= 0) ? 1 : stock;
										item = new Object(index, stack);
										break;
									}
									case "BigObject":
									case "BO":
										item = new Object(Vector2.Zero, index);
										break;
									case "Ring":
									case "R":
										item = new Ring(index);
										break;
									case "Boot":
									case "B":
										item = new Boots(index);
										break;
									case "Weapon":
									case "W":
										item = new MeleeWeapon(index);
										break;
									case "Blueprint":
									case "BL":
										item = new Object(index, 1, isRecipe: true);
										break;
									case "Hat":
									case "H":
										item = new Hat(index);
										break;
									case "BigBlueprint":
									case "BBl":
									case "BBL":
										item = new Object(Vector2.Zero, index, isRecipe: true);
										break;
									case "F":
										item = Furniture.GetFurnitureInstance(index);
										break;
									}
									if ((!(item is Object) || !(item as Object).isRecipe || !who.knowsRecipe(item.Name)) && item != null)
									{
										stockList2.Add(item, new int[2]
										{
											price,
											(stock <= 0) ? infiniteStock : stock
										});
									}
								}
								festivalShops.Add(split[1], stockList2);
							}
							else
							{
								stockList2 = festivalShops[split[1]];
							}
							if (stockList2 != null && stockList2.Count > 0)
							{
								Game1.activeClickableMenu = new ShopMenu(stockList2);
							}
							else
							{
								Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1714"));
							}
							break;
						}
						case "Message":
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromMaps:" + split[1].Replace("\"", "")));
							break;
						case "Dialogue":
							Game1.drawObjectDialogue(Game1.currentLocation.actionParamsToString(split).Replace("#", " "));
							break;
						}
					}
					catch (Exception)
					{
					}
				}
				else if (isFestival)
				{
					if (who.IsLocalPlayer)
					{
						foreach (NPC j in actors)
						{
							if (j.getTileX() == tileLocation.X && j.getTileY() == tileLocation.Y && j is Child)
							{
								(j as Child).checkAction(who, temporaryLocation);
								return true;
							}
							if (j.getTileX() == tileLocation.X && j.getTileY() == tileLocation.Y && (j.CurrentDialogue.Count() >= 1 || (j.CurrentDialogue.Count() > 0 && !j.CurrentDialogue.Peek().isOnFinalDialogue()) || j.Equals(festivalHost) || ((bool)j.datable && festivalData["file"].Equals("spring24")) || (secretSantaRecipient != null && j.Name.Equals(secretSantaRecipient.Name))))
							{
								bool divorced = who.friendshipData.ContainsKey(j.Name) && who.friendshipData[j.Name].IsDivorced();
								if ((grangeScore > -100 || grangeScore == -666) && j.Equals(festivalHost) && grangeJudged)
								{
									string message2 = "";
									if (grangeScore >= 90)
									{
										Game1.playSound("reward");
										message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1723", grangeScore);
										Game1.player.festivalScore += 1000;
									}
									else if (grangeScore >= 75)
									{
										Game1.playSound("reward");
										message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1726", grangeScore);
										Game1.player.festivalScore += 500;
									}
									else if (grangeScore >= 60)
									{
										Game1.playSound("newArtifact");
										message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1729", grangeScore);
										Game1.player.festivalScore += 250;
									}
									else if (grangeScore == -666)
									{
										Game1.playSound("secret1");
										message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1730");
										Game1.player.festivalScore += 750;
									}
									else
									{
										Game1.playSound("newArtifact");
										message2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1732", grangeScore);
										Game1.player.festivalScore += 50;
									}
									grangeScore = -100;
									j.setNewDialogue(message2);
								}
								else if ((Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && j.Equals(festivalHost) && (j.CurrentDialogue.Count() == 0 || j.CurrentDialogue.Peek().isOnFinalDialogue()) && hostMessage != null)
								{
									j.setNewDialogue(hostMessage);
								}
								else if ((Game1.serverHost == null || Game1.player.Equals(Game1.serverHost.Value)) && j.Equals(festivalHost) && (j.CurrentDialogue.Count == 0 || j.CurrentDialogue.Peek().isOnFinalDialogue()) && hostMessage != null)
								{
									j.setNewDialogue(hostMessage);
								}
								if (isSpecificFestival("spring24") && !divorced && ((bool)j.datable || (who.spouse != null && j.Name.Equals(who.spouse))))
								{
									j.grantConversationFriendship(who);
									if (who.dancePartner.Value == null)
									{
										if (j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
										{
											j.CurrentDialogue.Clear();
										}
										if (j.CurrentDialogue.Count == 0)
										{
											j.CurrentDialogue.Push(new Dialogue("...", j));
											if (j.name.Equals(who.spouse))
											{
												j.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1736", j.displayName), add: true);
											}
											else
											{
												j.setNewDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1738", j.displayName), add: true);
											}
										}
										else if (j.CurrentDialogue.Peek().isOnFinalDialogue())
										{
											Dialogue d = j.CurrentDialogue.Peek();
											Game1.drawDialogue(j);
											j.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
											who.Halt();
											j.CurrentDialogue = new Stack<Dialogue>();
											j.CurrentDialogue.Push(new Dialogue("...", j));
											j.CurrentDialogue.Push(d);
											return true;
										}
									}
									else if (j.CurrentDialogue.Count > 0 && j.CurrentDialogue.Peek().getCurrentDialogue().Equals("..."))
									{
										j.CurrentDialogue.Clear();
									}
								}
								if (!divorced && secretSantaRecipient != null && j.Name.Equals(secretSantaRecipient.Name))
								{
									j.grantConversationFriendship(who);
									Game1.currentLocation.createQuestionDialogue(Game1.parseText(((int)secretSantaRecipient.gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1740", secretSantaRecipient.displayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1741", secretSantaRecipient.displayName)), Game1.currentLocation.createYesNoResponses(), "secretSanta");
									who.Halt();
									return true;
								}
								if (j.CurrentDialogue.Count == 0)
								{
									return true;
								}
								if (who.spouse != null && j.Name.Equals(who.spouse) && !festivalData["file"].Equals("spring24") && festivalData.ContainsKey(j.Name + "_spouse"))
								{
									j.CurrentDialogue.Clear();
									j.CurrentDialogue.Push(new Dialogue(GetFestivalDataForYear(j.Name + "_spouse"), j));
								}
								if (divorced)
								{
									j.CurrentDialogue.Clear();
									j.CurrentDialogue.Push(new Dialogue(Game1.content.Load<Dictionary<string, string>>("Characters\\Dialogue\\" + j.Name)["divorced"], j));
								}
								j.grantConversationFriendship(who);
								Game1.drawDialogue(j);
								j.faceTowardFarmerForPeriod(3000, 2, faceAway: false, who);
								who.Halt();
								return true;
							}
						}
					}
					if (festivalData != null && festivalData["file"].Equals("spring13"))
					{
						Microsoft.Xna.Framework.Rectangle tile = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
						for (int i = festivalProps.Count - 1; i >= 0; i--)
						{
							if (festivalProps[i].isColliding(tile))
							{
								who.festivalScore++;
								festivalProps.RemoveAt(i);
								who.team.FestivalPropsRemoved(tile);
								if (who.IsLocalPlayer)
								{
									Game1.playSound("coin");
								}
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		public void removeFestivalProps(Microsoft.Xna.Framework.Rectangle rect)
		{
			for (int i = festivalProps.Count - 1; i >= 0; i--)
			{
				if (festivalProps[i].isColliding(rect))
				{
					festivalProps.RemoveAt(i);
				}
			}
		}

		public void checkForSpecialCharacterIconAtThisTile(Vector2 tileLocation)
		{
			if (isFestival && festivalHost != null && festivalHost.getTileLocation().Equals(tileLocation))
			{
				Game1.mouseCursor = 4;
			}
		}

		public void forceEndFestival(Farmer who)
		{
			Game1.currentMinigame = null;
			Game1.exitActiveMenu();
			Game1.player.Halt();
			endBehaviors(null, Game1.currentLocation);
			if (Game1.IsServer)
			{
				Game1.multiplayer.sendServerToClientsMessage("endFest");
			}
			Game1.changeMusicTrack("none");
		}

		public bool checkForCollision(Microsoft.Xna.Framework.Rectangle position, Farmer who)
		{
			foreach (NPC i in actors)
			{
				if (i.GetBoundingBox().Intersects(position) && !farmer.temporarilyInvincible && farmer.TemporaryPassableTiles.IsEmpty() && !i.IsInvisible && !who.GetBoundingBox().Intersects(i.GetBoundingBox()) && !i.farmerPassesThrough)
				{
					return true;
				}
			}
			if (position.X < 0 || position.Y < 0 || position.X >= Game1.currentLocation.map.Layers[0].DisplayWidth || position.Y >= Game1.currentLocation.map.Layers[0].DisplayHeight)
			{
				if (who.IsLocalPlayer && isFestival)
				{
					who.Halt();
					who.Position = who.lastPosition;
					if (!Game1.IsMultiplayer && Game1.activeClickableMenu == null)
					{
						Game1.activeClickableMenu = new ConfirmationDialog(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1758", FestivalName), forceEndFestival);
					}
					else if (Game1.activeClickableMenu == null)
					{
						Game1.player.team.SetLocalReady("festivalEnd", ready: true);
						Game1.activeClickableMenu = new ReadyCheckDialog("festivalEnd", allowCancel: true, forceEndFestival);
					}
				}
				return true;
			}
			foreach (Object prop in props)
			{
				if (prop.getBoundingBox(prop.tileLocation).Intersects(position))
				{
					return true;
				}
			}
			if (temporaryLocation != null)
			{
				foreach (Object value in temporaryLocation.objects.Values)
				{
					if (value.getBoundingBox(value.tileLocation).Intersects(position))
					{
						return true;
					}
				}
			}
			foreach (Prop festivalProp in festivalProps)
			{
				if (festivalProp.isColliding(position))
				{
					return true;
				}
			}
			return false;
		}

		public void answerDialogue(string questionKey, int answerChoice)
		{
			previousAnswerChoice = answerChoice;
			if (questionKey.Contains("fork"))
			{
				int forkAnswer = Convert.ToInt32(questionKey.Replace("fork", ""));
				if (answerChoice == forkAnswer)
				{
					specialEventVariable1 = !specialEventVariable1;
				}
				return;
			}
			if (questionKey.Contains("quickQuestion"))
			{
				string obj = eventCommands[Math.Min(eventCommands.Length - 1, CurrentCommand)];
				string[] newCommands = obj.Substring(obj.IndexOf(' ') + 1).Split(new string[1]
				{
					"(break)"
				}, StringSplitOptions.None)[1 + answerChoice].Split('\\');
				List<string> tmp = eventCommands.ToList();
				tmp.InsertRange(CurrentCommand + 1, newCommands);
				eventCommands = tmp.ToArray();
				return;
			}
			switch (questionKey)
			{
			case "shaneCliffs":
				switch (answerChoice)
				{
				case 0:
					eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1760") + "\"";
					break;
				case 1:
					eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1761") + "\"";
					break;
				case 2:
					eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1763") + "\"";
					break;
				case 3:
					eventCommands[currentCommand + 2] = "speak Shane \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1764") + "\"";
					break;
				}
				break;
			case "shaneLoan":
				if (answerChoice != 0)
				{
					_ = 1;
					break;
				}
				specialEventVariable1 = true;
				eventCommands[currentCommand + 1] = "fork giveShaneLoan";
				Game1.player.Money -= 3000;
				break;
			case "haleyDarkRoom":
				switch (answerChoice)
				{
				case 2:
					break;
				case 0:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork decorate";
					break;
				case 1:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork leave";
					break;
				}
				break;
			case "chooseCharacter":
				switch (answerChoice)
				{
				case 2:
					break;
				case 0:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork warrior";
					break;
				case 1:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork healer";
					break;
				}
				break;
			case "bandFork":
				switch (answerChoice)
				{
				case 76:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork poppy";
					break;
				case 77:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork heavy";
					break;
				case 78:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork techno";
					break;
				case 79:
					specialEventVariable1 = true;
					eventCommands[currentCommand + 1] = "fork honkytonk";
					break;
				}
				break;
			case "StarTokenShop":
				if (answerChoice == 0)
				{
					Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1774"), buyStarTokens, 50, 0, 999);
				}
				break;
			case "wheelBet":
				specialEventVariable2 = (answerChoice == 1);
				if (answerChoice != 2)
				{
					Game1.activeClickableMenu = new NumberSelectionMenu(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1776"), betStarTokens, -1, 1, Game1.player.festivalScore, Math.Min(1, Game1.player.festivalScore));
				}
				break;
			case "fortuneTeller":
				if (answerChoice == 0)
				{
					Game1.globalFadeToBlack(readFortune);
					Game1.player.Money -= 100;
					Game1.player.mailReceived.Add("fortuneTeller" + Game1.year);
				}
				break;
			case "slingshotGame":
				if (answerChoice == 0 && Game1.player.Money >= 50)
				{
					Game1.globalFadeToBlack(TargetGame.startMe, 0.01f);
					Game1.player.Money -= 50;
				}
				else if (answerChoice == 0 && Game1.player.Money < 50)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
				}
				break;
			case "fishingGame":
				if (answerChoice == 0 && Game1.player.Money >= 50)
				{
					Game1.globalFadeToBlack(FishingGame.startMe, 0.01f);
					Game1.player.Money -= 50;
				}
				else if (answerChoice == 0 && Game1.player.Money < 50)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1780"));
				}
				break;
			case "starTokenShop":
				if (answerChoice == 0)
				{
					if (festivalShops["starTokenShop"].Count == 0)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1785")));
					}
					else
					{
						Game1.activeClickableMenu = new ShopMenu(festivalShops["starTokenShop"], 1);
					}
				}
				break;
			case "secretSanta":
				if (answerChoice == 0)
				{
					Game1.activeClickableMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightSantaObjects, chooseSecretSantaGift, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1788", secretSantaRecipient.displayName), null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
				}
				break;
			case "cave":
				if (answerChoice == 0)
				{
					Game1.MasterPlayer.caveChoice.Value = 2;
					(Game1.getLocationFromName("FarmCave") as FarmCave).setUpMushroomHouse();
				}
				else
				{
					Game1.MasterPlayer.caveChoice.Value = 1;
				}
				break;
			case "pet":
				if (answerChoice == 0)
				{
					Game1.activeClickableMenu = new NamingMenu(namePet, Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1236"), (!Game1.player.IsMale) ? (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1796") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1797")) : (Game1.player.catPerson ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1794") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1795")));
					break;
				}
				Game1.player.mailReceived.Add("rejectedPet");
				eventCommands = new string[2];
				eventCommands[1] = "end";
				eventCommands[0] = "speak Marnie \"" + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1798") + "\"";
				currentCommand = 0;
				eventSwitched = true;
				specialEventVariable1 = true;
				break;
			}
		}

		private void namePet(string name)
		{
			Pet p = (!Game1.player.catPerson) ? ((Pet)new Dog(68, 13, Game1.player.whichPetBreed)) : ((Pet)new Cat(68, 13, Game1.player.whichPetBreed));
			p.warpToFarmHouse(Game1.player);
			p.Name = name;
			p.displayName = p.name;
			Game1.exitActiveMenu();
			CurrentCommand++;
		}

		public void chooseSecretSantaGift(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			if (i is Object)
			{
				if (i.Stack > 1)
				{
					i.Stack--;
					who.addItemToInventory(i);
				}
				Game1.exitActiveMenu();
				NPC recipient = getActorByName(secretSantaRecipient.Name);
				recipient.faceTowardFarmerForPeriod(15000, 5, faceAway: false, who);
				recipient.receiveGift(i as Object, who, updateGiftLimitInfo: false, 5f, showResponse: false);
				recipient.CurrentDialogue.Clear();
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					recipient.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1801", i.DisplayName), recipient));
				}
				else
				{
					recipient.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1801", i.DisplayName, Lexicon.getProperArticleForWord(i.DisplayName)), recipient));
				}
				Game1.drawDialogue(recipient);
				secretSantaRecipient = null;
				startSecretSantaAfterDialogue = true;
				who.Halt();
				who.completelyStopAnimatingOrDoingAction();
				who.faceGeneralDirection(recipient.Position, 0, opposite: false, useTileCalculations: false);
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1803"));
			}
		}

		public void perfectFishing()
		{
			if (isFestival && Game1.currentMinigame != null && festivalData["file"].Equals("fall16"))
			{
				(Game1.currentMinigame as FishingGame).perfections++;
			}
		}

		public void caughtFish(int whichFish, int size, Farmer who)
		{
			if (!isFestival)
			{
				return;
			}
			if (whichFish != -1 && Game1.currentMinigame != null && festivalData["file"].Equals("fall16"))
			{
				(Game1.currentMinigame as FishingGame).score += ((size <= 0) ? 1 : (size + 5));
				if (size > 0)
				{
					(Game1.currentMinigame as FishingGame).fishCaught++;
				}
				Game1.player.FarmerSprite.PauseForSingleAnimation = false;
				Game1.player.FarmerSprite.StopAnimation();
			}
			else if (whichFish != -1 && festivalData["file"].Equals("winter8"))
			{
				if (size > 0 && who.getTileX() < 79 && who.getTileY() < 43)
				{
					who.festivalScore++;
					Game1.playSound("newArtifact");
				}
				who.forceCanMove();
				if (previousFacingDirection != -1)
				{
					who.faceDirection(previousFacingDirection);
				}
			}
		}

		public void readFortune()
		{
			Game1.globalFade = true;
			Game1.fadeToBlackAlpha = 1f;
			NPC topRomance = Utility.getTopRomanticInterest(Game1.player);
			NPC topFriend = Utility.getTopNonRomanticInterest(Game1.player);
			int topSkill = Utility.getHighestSkill(Game1.player);
			string[] fortune = new string[5];
			if (topFriend != null && Game1.player.getFriendshipLevelForNPC(topFriend.Name) > 100)
			{
				if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topFriend.Name) - 100, Game1.player.getFriendshipLevelForNPC(topFriend.Name)) > 3 && Game1.random.NextDouble() < 0.5)
				{
					fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1810");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1811", topFriend.displayName);
						break;
					case 1:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1813", topFriend.displayName) + ((topFriend.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1815") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1816"));
						break;
					case 2:
						fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1818", topFriend.displayName);
						break;
					case 3:
						fortune[0] = ((topFriend.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1820") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1821")) + Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1823", topFriend.displayName);
						break;
					}
				}
			}
			else
			{
				fortune[0] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1825");
			}
			if (topRomance != null && Game1.player.getFriendshipLevelForNPC(topRomance.Name) > 250)
			{
				if (Utility.getNumberOfFriendsWithinThisRange(Game1.player, Game1.player.getFriendshipLevelForNPC(topRomance.Name) - 100, Game1.player.getFriendshipLevelForNPC(topRomance.Name), romanceOnly: true) > 2)
				{
					fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1826");
				}
				else
				{
					switch (Game1.random.Next(4))
					{
					case 0:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomance.displayName);
						break;
					case 1:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1827", topRomance.displayName);
						break;
					case 2:
						fortune[1] = ((topRomance.Gender != 0) ? ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1833") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1834")) : ((topRomance.SocialAnxiety == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1831") : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1832"))) + " " + ((topRomance.Gender == 0) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1837", topRomance.displayName[0]) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1838", topRomance.displayName[0]));
						break;
					case 3:
						fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1843", topRomance.displayName);
						break;
					}
				}
			}
			else
			{
				fortune[1] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1845");
			}
			switch (topSkill)
			{
			case 0:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1846");
				break;
			case 3:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1847");
				break;
			case 4:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1848");
				break;
			case 1:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1849");
				break;
			case 2:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1850");
				break;
			case 5:
				fortune[2] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1851");
				break;
			}
			fortune[3] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1852");
			fortune[4] = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1853");
			Game1.multipleDialogues(fortune);
			Game1.afterDialogues = fadeClearAndviewportUnfreeze;
			Game1.viewportFreeze = true;
			Game1.viewport.X = -9999;
		}

		public void fadeClearAndviewportUnfreeze()
		{
			Game1.fadeClear();
			Game1.viewportFreeze = false;
		}

		public void betStarTokens(int value, int price, Farmer who)
		{
			if (value <= who.festivalScore)
			{
				Game1.playSound("smallSelect");
				Game1.activeClickableMenu = new WheelSpinGame(value);
			}
		}

		public void buyStarTokens(int value, int price, Farmer who)
		{
			if (value > 0 && value * price <= who.Money)
			{
				who.Money -= price * value;
				who.festivalScore += value;
				Game1.playSound("purchase");
				Game1.exitActiveMenu();
			}
		}

		public void clickToAddItemToLuauSoup(Item i, Farmer who)
		{
			addItemToLuauSoup(i, who);
		}

		public void setUpAdvancedMove(string[] split, NPCController.endBehavior endBehavior = null)
		{
			if (npcControllers == null)
			{
				npcControllers = new List<NPCController>();
			}
			List<Vector2> path = new List<Vector2>();
			for (int j = 3; j < split.Length; j += 2)
			{
				path.Add(new Vector2(Convert.ToInt32(split[j]), Convert.ToInt32(split[j + 1])));
			}
			if (split[1].Contains("farmer"))
			{
				Farmer f = getFarmerFromFarmerNumberString(split[1], farmer);
				npcControllers.Add(new NPCController(f, path, Convert.ToBoolean(split[2]), endBehavior));
				return;
			}
			NPC i = getActorByName(split[1].Replace('_', ' '));
			if (i != null)
			{
				npcControllers.Add(new NPCController(i, path, Convert.ToBoolean(split[2]), endBehavior));
			}
		}

		public static bool IsItemMayorShorts(Item i)
		{
			if (!Utility.IsNormalObjectAtParentSheetIndex(i, 789))
			{
				return Utility.IsNormalObjectAtParentSheetIndex(i, 71);
			}
			return true;
		}

		public void addItemToLuauSoup(Item i, Farmer who)
		{
			if (i == null)
			{
				return;
			}
			who.team.luauIngredients.Add(i.getOne());
			if (who.IsLocalPlayer)
			{
				specialEventVariable2 = true;
				bool is_shorts = IsItemMayorShorts(i);
				if (i != null && i.Stack > 1 && !is_shorts)
				{
					i.Stack--;
					who.addItemToInventory(i);
				}
				else if (is_shorts)
				{
					who.addItemToInventory(i);
				}
				Game1.exitActiveMenu();
				Game1.playSound("dropItemInWater");
				if (i != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1857", i.DisplayName));
				}
				string qualityString = "";
				if (i is Object)
				{
					qualityString = (((i as Object).Quality == 1) ? " ([51])" : (((i as Object).Quality == 2) ? " ([52])" : (((i as Object).Quality == 4) ? " ([53])" : "")));
				}
				Game1.multiplayer.globalChatInfoMessage("LuauSoup", Game1.player.Name, i.DisplayName + qualityString);
			}
		}

		private void governorTaste()
		{
			int likeLevel = 5;
			foreach (Item luauIngredient in Game1.player.team.luauIngredients)
			{
				Object o = luauIngredient as Object;
				int itemLevel = 5;
				if (IsItemMayorShorts(o))
				{
					likeLevel = 6;
					break;
				}
				if ((o.Quality >= 2 && (int)o.price >= 160) || (o.Quality == 1 && (int)o.price >= 300 && (int)o.edibility > 10))
				{
					itemLevel = 4;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 120, 2);
				}
				else if ((int)o.edibility >= 20 || (int)o.price >= 100 || ((int)o.price >= 70 && o.Quality >= 1))
				{
					itemLevel = 3;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, 60, 2);
				}
				else if (((int)o.price > 20 && (int)o.edibility >= 10) || ((int)o.price >= 40 && (int)o.edibility >= 5))
				{
					itemLevel = 2;
				}
				else if ((int)o.edibility >= 0)
				{
					itemLevel = 1;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -50, 2);
				}
				if ((int)o.edibility > -300 && (int)o.edibility < 0)
				{
					itemLevel = 0;
					Utility.improveFriendshipWithEveryoneInRegion(Game1.player, -100, 2);
				}
				if (itemLevel < likeLevel)
				{
					likeLevel = itemLevel;
				}
			}
			if (likeLevel != 6 && Game1.player.team.luauIngredients.Count < Game1.numberOfPlayers())
			{
				likeLevel = 5;
			}
			eventCommands[CurrentCommand + 1] = "switchEvent governorReaction" + likeLevel;
		}

		private void eggHuntWinner()
		{
			int numberOfEggsToWin = 12;
			switch (Game1.numberOfPlayers())
			{
			case 1:
				numberOfEggsToWin = 9;
				break;
			case 2:
				numberOfEggsToWin = 6;
				break;
			case 3:
				numberOfEggsToWin = 5;
				break;
			case 4:
				numberOfEggsToWin = 4;
				break;
			}
			List<Farmer> winners = new List<Farmer>();
			_ = Game1.player;
			int mostEggsScore = Game1.player.festivalScore;
			foreach (Farmer temp2 in Game1.getOnlineFarmers())
			{
				if (temp2.festivalScore > mostEggsScore)
				{
					mostEggsScore = temp2.festivalScore;
				}
			}
			foreach (Farmer temp in Game1.getOnlineFarmers())
			{
				if (temp.festivalScore == mostEggsScore)
				{
					winners.Add(temp);
					festivalWinners.Add(temp.UniqueMultiplayerID);
				}
			}
			string winnerDialogue2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1862");
			if (mostEggsScore >= numberOfEggsToWin)
			{
				if (winners.Count == 1)
				{
					winnerDialogue2 = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? ("¡" + winners[0].displayName + "!") : (winners[0].displayName + "!"));
				}
				else
				{
					winnerDialogue2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int i = 0; i < winners.Count; i++)
					{
						if (i == winners.Count() - 1)
						{
							winnerDialogue2 += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						winnerDialogue2 = winnerDialogue2 + " " + winners[i].displayName;
						if (i < winners.Count - 1)
						{
							winnerDialogue2 += ",";
						}
					}
					winnerDialogue2 += "!";
				}
				specialEventVariable1 = false;
			}
			else
			{
				specialEventVariable1 = true;
			}
			getActorByName("Lewis").CurrentDialogue.Push(new Dialogue(winnerDialogue2, getActorByName("Lewis")));
			Game1.drawDialogue(getActorByName("Lewis"));
		}

		private void iceFishingWinner()
		{
			int numberOfFishToWin = 5;
			winners = new List<Farmer>();
			_ = Game1.player;
			int mostFishScore = Game1.player.festivalScore;
			for (int k = 1; k <= Game1.numberOfPlayers(); k++)
			{
				Farmer temp = Utility.getFarmerFromFarmerNumber(k);
				if (temp != null && temp.festivalScore > mostFishScore)
				{
					mostFishScore = temp.festivalScore;
				}
			}
			for (int j = 1; j <= Game1.numberOfPlayers(); j++)
			{
				Farmer temp2 = Utility.getFarmerFromFarmerNumber(j);
				if (temp2 != null && temp2.festivalScore == mostFishScore)
				{
					winners.Add(temp2);
					festivalWinners.Add(temp2.UniqueMultiplayerID);
				}
			}
			string winnerDialogue2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1871");
			if (mostFishScore >= numberOfFishToWin)
			{
				if (winners.Count == 1)
				{
					winnerDialogue2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1872", winners[0].displayName, winners[0].festivalScore);
				}
				else
				{
					winnerDialogue2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1864");
					for (int i = 0; i < winners.Count; i++)
					{
						if (i == winners.Count() - 1)
						{
							winnerDialogue2 += Game1.content.LoadString("Strings\\StringsFromCSFiles:Event.cs.1865");
						}
						winnerDialogue2 = winnerDialogue2 + " " + winners[i].displayName;
						if (i < winners.Count - 1)
						{
							winnerDialogue2 += ",";
						}
					}
					winnerDialogue2 += "!";
				}
				specialEventVariable1 = false;
			}
			else
			{
				specialEventVariable1 = true;
			}
			getActorByName("Lewis").CurrentDialogue.Push(new Dialogue(winnerDialogue2, getActorByName("Lewis")));
			Game1.drawDialogue(getActorByName("Lewis"));
		}

		private void iceFishingWinnerMP()
		{
			specialEventVariable1 = !winners.Contains(Game1.player);
		}
	}
}
