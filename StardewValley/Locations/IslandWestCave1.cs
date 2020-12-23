using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandWestCave1 : IslandLocation
	{
		public class CaveCrystal
		{
			public Vector2 tileLocation;

			public int id;

			public int pitch;

			public Color color;

			public Color currentColor;

			public float shakeTimer;

			public float glowTimer;

			public void update()
			{
				if (glowTimer > 0f)
				{
					glowTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					currentColor.R = (byte)Utility.Lerp((int)color.R, 255f, glowTimer / 1000f);
					currentColor.G = (byte)Utility.Lerp((int)color.G, 255f, glowTimer / 1000f);
					currentColor.B = (byte)Utility.Lerp((int)color.B, 255f, glowTimer / 1000f);
				}
				if (shakeTimer > 0f)
				{
					shakeTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				}
			}

			public void activate()
			{
				glowTimer = 1000f;
				shakeTimer = 100f;
				ICue cue = Game1.soundBank.GetCue("crystal");
				cue.SetVariable("Pitch", pitch);
				cue.Play();
				currentColor = color;
			}

			public void draw(SpriteBatch b)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(8f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle(188, 228, 52, 28), currentColor, 0f, new Vector2(52f, 28f) / 2f, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 64f - 8f) / 10000f);
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(tileLocation * 64f + new Vector2(0f, -52f) + new Vector2((shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0, (shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0)), new Microsoft.Xna.Framework.Rectangle(240, 227, 16, 29), currentColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tileLocation.Y * 64f + 64f - 4f) / 10000f);
			}
		}

		[XmlIgnore]
		protected List<CaveCrystal> crystals = new List<CaveCrystal>();

		public const int PHASE_INTRO = 0;

		public const int PHASE_PLAY_SEQUENCE = 1;

		public const int PHASE_WAIT_FOR_PLAYER_INPUT = 2;

		public const int PHASE_NOTHING = 3;

		public const int PHASE_SUCCESSFUL_SEQUENCE = 4;

		public const int PHASE_OUTRO = 5;

		[XmlElement("completed")]
		public NetBool completed = new NetBool();

		[XmlIgnore]
		public NetBool isActivated = new NetBool(value: false);

		[XmlIgnore]
		public NetFloat netPhaseTimer = new NetFloat();

		[XmlIgnore]
		public float localPhaseTimer;

		[XmlIgnore]
		public float betweenNotesTimer;

		[XmlIgnore]
		public int localPhase;

		[XmlIgnore]
		public NetInt netPhase = new NetInt(3);

		[XmlIgnore]
		public NetInt currentDifficulty = new NetInt(2);

		[XmlIgnore]
		public NetInt currentCrystalSequenceIndex = new NetInt(0);

		[XmlIgnore]
		public int currentPlaybackCrystalSequenceIndex;

		[XmlIgnore]
		public NetList<int, NetInt> currentCrystalSequence = new NetList<int, NetInt>();

		[XmlIgnore]
		public NetEvent1Field<int, NetInt> enterValueEvent = new NetEvent1Field<int, NetInt>();

		public IslandWestCave1()
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(netPhase, isActivated, currentDifficulty, currentCrystalSequenceIndex, currentCrystalSequence, enterValueEvent.NetFields, netPhaseTimer, completed);
			enterValueEvent.onEvent += enterValue;
			isActivated.fieldChangeVisibleEvent += onActivationChanged;
		}

		public IslandWestCave1(string map, string name)
			: base(map, name)
		{
		}

		public void onActivationChanged(NetBool field, bool old_value, bool new_value)
		{
			updateActivationVisuals();
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			resetPuzzle();
		}

		public void resetPuzzle()
		{
			isActivated.Value = false;
			updateActivationVisuals();
			netPhase.Value = 3;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (crystals.Count == 0)
			{
				crystals.Add(new CaveCrystal
				{
					tileLocation = new Vector2(3f, 4f),
					color = new Color(220, 0, 255),
					currentColor = new Color(220, 0, 255),
					id = 1,
					pitch = 0
				});
				crystals.Add(new CaveCrystal
				{
					tileLocation = new Vector2(4f, 6f),
					color = Color.Lime,
					currentColor = Color.Lime,
					id = 2,
					pitch = 700
				});
				crystals.Add(new CaveCrystal
				{
					tileLocation = new Vector2(6f, 7f),
					color = new Color(255, 50, 100),
					currentColor = new Color(255, 50, 100),
					id = 3,
					pitch = 1200
				});
				crystals.Add(new CaveCrystal
				{
					tileLocation = new Vector2(8f, 6f),
					color = new Color(0, 200, 255),
					currentColor = new Color(0, 200, 255),
					id = 4,
					pitch = 1400
				});
				crystals.Add(new CaveCrystal
				{
					tileLocation = new Vector2(9f, 4f),
					color = new Color(255, 180, 0),
					currentColor = new Color(255, 180, 0),
					id = 5,
					pitch = 1600
				});
			}
			updateActivationVisuals();
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && who.IsLocalPlayer)
			{
				string[] actionParams = action.Split(' ');
				if (actionParams[0] == "Crystal" && (netPhase.Value == 5 || netPhase.Value == 3 || netPhase.Value == 2))
				{
					int which = Convert.ToInt32(actionParams[1]);
					enterValueEvent.Fire(which);
					return true;
				}
				if (actionParams[0] == "CrystalCaveActivate" && !isActivated && !completed.Value)
				{
					isActivated.Value = true;
					Game1.playSound("openBox");
					updateActivationVisuals();
					netPhaseTimer.Value = 1200f;
					netPhase.Value = 0;
					currentDifficulty.Value = 2;
					return true;
				}
			}
			return base.performAction(action, who, tileLocation);
		}

		public virtual void updateActivationVisuals()
		{
			if (map != null && Game1.gameMode != 6 && Game1.currentLocation == this)
			{
				if (isActivated.Value || completed.Value)
				{
					Game1.currentLightSources.Add(new LightSource(1, new Vector2(6.5f, 1f) * 64f, 2f, Color.Black, 99, LightSource.LightContext.None, 0L));
					setMapTileIndex(6, 1, 33, "Buildings");
				}
				else
				{
					setMapTileIndex(6, 1, 31, "Buildings");
					Utility.removeLightSource(99);
				}
				if (completed.Value)
				{
					addCompletionTorches();
				}
			}
		}

		public virtual void enterValue(int which)
		{
			if (netPhase.Value == 2 && Game1.IsMasterGame && currentCrystalSequence.Count > (int)currentCrystalSequenceIndex)
			{
				if (currentCrystalSequence[currentCrystalSequenceIndex] != which - 1)
				{
					playSound("cancel");
					resetPuzzle();
					return;
				}
				currentCrystalSequenceIndex.Value++;
				if ((int)currentCrystalSequenceIndex >= currentCrystalSequence.Count)
				{
					DelayedAction.playSoundAfterDelay(((int)currentDifficulty == 7) ? "discoverMineral" : "newArtifact", 500, this);
					netPhaseTimer.Value = 2000f;
					netPhase.Value = 4;
				}
			}
			if (crystals.Count > which - 1)
			{
				crystals[which - 1].activate();
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			crystals.Clear();
			base.cleanupBeforePlayerExit();
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			enterValueEvent.Poll();
			if ((localPhase != 1 || currentPlaybackCrystalSequenceIndex < 0 || currentPlaybackCrystalSequenceIndex >= currentCrystalSequence.Count) && localPhase != netPhase.Value)
			{
				localPhaseTimer = netPhaseTimer.Value;
				localPhase = netPhase.Value;
				if (localPhase != 1)
				{
					currentPlaybackCrystalSequenceIndex = -1;
				}
				else
				{
					currentPlaybackCrystalSequenceIndex = 0;
				}
			}
			base.UpdateWhenCurrentLocation(time);
			foreach (CaveCrystal crystal in crystals)
			{
				crystal.update();
			}
			if (localPhaseTimer > 0f)
			{
				localPhaseTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (localPhaseTimer <= 0f)
				{
					switch (localPhase)
					{
					case 0:
					case 4:
					{
						currentPlaybackCrystalSequenceIndex = 0;
						if (Game1.IsMasterGame)
						{
							currentDifficulty.Value++;
							currentCrystalSequence.Clear();
							currentCrystalSequenceIndex.Value = 0;
							if ((int)currentDifficulty > 7)
							{
								netPhaseTimer.Value = 10f;
								netPhase.Value = 5;
								break;
							}
							for (int i = 0; i < (int)currentDifficulty; i++)
							{
								currentCrystalSequence.Add(Game1.random.Next(5));
							}
							netPhase.Value = 1;
						}
						int betweenNotesDivisor = currentDifficulty;
						if ((int)currentDifficulty > 5)
						{
							betweenNotesDivisor--;
						}
						betweenNotesTimer = 2000f / (float)betweenNotesDivisor;
						break;
					}
					case 5:
						if (Game1.currentLocation == this)
						{
							Game1.playSound("fireball");
							Utility.addSmokePuff(this, new Vector2(5f, 1f) * 64f);
							Utility.addSmokePuff(this, new Vector2(7f, 1f) * 64f);
						}
						if (Game1.IsMasterGame)
						{
							Game1.player.team.MarkCollectedNut("IslandWestCavePuzzle");
							Game1.createObjectDebris(73, 5, 1, this);
							Game1.createObjectDebris(73, 7, 1, this);
							Game1.createObjectDebris(73, 6, 1, this);
						}
						completed.Value = true;
						if (Game1.currentLocation == this)
						{
							addCompletionTorches();
						}
						break;
					}
				}
			}
			int num = localPhase;
			if (num != 1)
			{
				_ = 5;
				return;
			}
			betweenNotesTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (!(betweenNotesTimer <= 0f) || currentCrystalSequence.Count <= 0 || currentPlaybackCrystalSequenceIndex < 0)
			{
				return;
			}
			int which = currentCrystalSequence[currentPlaybackCrystalSequenceIndex];
			if (which < crystals.Count)
			{
				crystals[which].activate();
			}
			currentPlaybackCrystalSequenceIndex++;
			betweenNotesTimer = 1500f / (float)(int)currentDifficulty;
			if ((int)currentDifficulty > 7)
			{
				betweenNotesTimer = 100f;
			}
			if (currentPlaybackCrystalSequenceIndex < currentCrystalSequence.Count)
			{
				return;
			}
			currentPlaybackCrystalSequenceIndex = -1;
			if ((int)currentDifficulty > 7)
			{
				if (Game1.IsMasterGame)
				{
					netPhaseTimer.Value = 1000f;
					netPhase.Value = 5;
				}
			}
			else if (Game1.IsMasterGame)
			{
				netPhase.Value = 2;
				currentCrystalSequenceIndex.Value = 0;
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			if (l is IslandWestCave1)
			{
				IslandWestCave1 cave = l as IslandWestCave1;
				completed.Value = cave.completed.Value;
			}
		}

		public void addCompletionTorches()
		{
			if (completed.Value)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(5f, 1f) * 64f + new Vector2(0f, -20f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.013439999f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(7f, 1f) * 64f + new Vector2(8f, -20f), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					light = true,
					lightRadius = 2f,
					scale = 4f,
					layerDepth = 0.013439999f
				});
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (CaveCrystal crystal in crystals)
			{
				crystal.draw(b);
			}
		}
	}
}
