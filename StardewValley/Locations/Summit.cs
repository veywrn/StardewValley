using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace StardewValley.Locations
{
	public class Summit : GameLocation
	{
		private ICue wind;

		private float windGust;

		private float globalWind = -0.25f;

		[XmlIgnore]
		public bool isShowingEndSlideshow;

		public Summit()
		{
		}

		public Summit(string map, string name)
			: base(map, name)
		{
		}

		public override void checkForMusic(GameTime time)
		{
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			if (Game1.random.NextDouble() < 0.005 || globalWind >= 1f || globalWind <= 0.35f)
			{
				if (globalWind < 0.35f)
				{
					windGust = (float)Game1.random.Next(3, 6) / 2000f;
				}
				else if (globalWind > 0.75f)
				{
					windGust = (float)(-Game1.random.Next(2, 6)) / 2000f;
				}
				else
				{
					windGust = (float)(((!(Game1.random.NextDouble() < 0.5)) ? 1 : (-1)) * Game1.random.Next(4, 6)) / 2000f;
				}
			}
			if (wind != null)
			{
				globalWind += windGust;
				globalWind = Utility.Clamp(globalWind, -0.5f, 1f);
				wind.SetVariable("Volume", Math.Abs(globalWind) * 60f);
				wind.SetVariable("Frequency", globalWind * 100f);
				wind.SetVariable("Pitch", 1200f + Math.Abs(globalWind) * 1200f);
			}
			base.UpdateWhenCurrentLocation(time);
			if (temporarySprites.Count == 0 && Game1.random.NextDouble() < ((Game1.timeOfDay < 1800) ? 0.0006 : ((Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20) ? 1.0 : 0.001)))
			{
				Rectangle sourceRect = Rectangle.Empty;
				Vector2 startingPosition = new Vector2(Game1.viewport.Width, Game1.random.Next(10, Game1.viewport.Height / 2));
				float speed = -4f;
				int loops = 200;
				float animationSpeed = 100f;
				if (Game1.timeOfDay < 1800)
				{
					if (Game1.currentSeason.Equals("spring") || Game1.currentSeason.Equals("fall"))
					{
						sourceRect = new Rectangle(640, 736, 16, 16);
						int rows = Game1.random.Next(1, 4);
						speed = -1f;
						for (int k = 0; k < rows; k++)
						{
							TemporaryAnimatedSprite bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, startingPosition + new Vector2((k + 1) * Game1.random.Next(15, 18), (k + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							bird2.motion = new Vector2(-1f, 0f);
							temporarySprites.Add(bird2);
							bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), 4, 200, startingPosition + new Vector2((k + 1) * Game1.random.Next(15, 18), (k + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							bird2.motion = new Vector2(-1f, 0f);
							temporarySprites.Add(bird2);
						}
					}
					else if (Game1.currentSeason.Equals("summer"))
					{
						sourceRect = new Rectangle(640, 752 + ((Game1.random.NextDouble() < 0.5) ? 16 : 0), 16, 16);
						speed = -0.5f;
						animationSpeed = 150f;
					}
					if (Game1.random.NextDouble() < 1.25)
					{
						TemporaryAnimatedSprite bird7 = null;
						switch (Game1.currentSeason)
						{
						case "spring":
							bird7 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(0, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							};
							break;
						case "summer":
							bird7 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(1, 165, 24, 21), Game1.random.Next(60, 80), 6, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							break;
						case "fall":
							bird7 = new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(0, 64, 32, 32), Game1.random.Next(60, 80), 5, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							};
							break;
						case "winter":
							bird7 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(104, 302, 26, 18), Game1.random.Next(80, 121), 4, 200, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f,
								pingPong = true
							};
							break;
						default:
							bird7 = new TemporaryAnimatedSprite();
							break;
						}
						bird7.motion = new Vector2(-3f, 0f);
						temporarySprites.Add(bird7);
					}
					else if (Game1.random.NextDouble() < 0.15 && Game1.stats.getStat("childrenTurnedToDoves") > 1)
					{
						for (int j = 0; j < Game1.stats.getStat("childrenTurnedToDoves"); j++)
						{
							sourceRect = Rectangle.Empty;
							TemporaryAnimatedSprite bird5 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 21), Game1.random.Next(80, 121), 6, 200, startingPosition + new Vector2((j + 1) * (Game1.random.Next(25, 27) * 4), Game1.random.Next(-32, 33) * 4), flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							bird5.motion = new Vector2(-3f, 0f);
							temporarySprites.Add(bird5);
						}
					}
					if (Game1.MasterPlayer.eventsSeen.Contains(571102) && Game1.random.NextDouble() < 0.1)
					{
						sourceRect = Rectangle.Empty;
						TemporaryAnimatedSprite bird4 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(222, 1890, 20, 9), 30f, 2, 99900, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 2f, 0f, 0f, 0f, local: true)
						{
							yPeriodic = true,
							yPeriodicLoopTime = 4000f,
							yPeriodicRange = 8f,
							layerDepth = 0f
						};
						bird4.motion = new Vector2(-3f, 0f);
						temporarySprites.Add(bird4);
					}
					if (Game1.MasterPlayer.eventsSeen.Contains(10) && Game1.random.NextDouble() < 0.05)
					{
						sourceRect = Rectangle.Empty;
						TemporaryAnimatedSprite bird3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(206, 1827, 15, 25), 30f, 4, 99900, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							rotation = -(float)Math.PI / 3f,
							layerDepth = 0f
						};
						bird3.motion = new Vector2(-4f, -0.5f);
						temporarySprites.Add(bird3);
					}
				}
				else if (Game1.timeOfDay >= 1900)
				{
					sourceRect = new Rectangle(640, 816, 16, 16);
					speed = -2f;
					loops = 0;
					startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
					if (Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20)
					{
						int numExtra = Game1.random.Next(3);
						for (int i = 0; i < numExtra; i++)
						{
							TemporaryAnimatedSprite t3 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(80, 121), Game1.currentSeason.Equals("winter") ? 2 : 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
							{
								layerDepth = 0f
							};
							t3.motion = new Vector2(speed, 0f);
							temporarySprites.Add(t3);
							startingPosition.X -= Game1.random.Next(64, Game1.viewport.Width);
							startingPosition.Y = Game1.random.Next(0, 200);
						}
					}
					else if (Game1.currentSeason.Equals("winter") && Game1.timeOfDay >= 1700 && Game1.random.NextDouble() < 0.1)
					{
						sourceRect = new Rectangle(640, 800, 32, 16);
						loops = 1000;
						startingPosition.X = Game1.viewport.Width;
					}
					else if (Game1.currentSeason.Equals("winter"))
					{
						sourceRect = Rectangle.Empty;
					}
				}
				if (Game1.timeOfDay >= 2200 && !Game1.currentSeason.Equals("winter") && Game1.currentSeason.Equals("summer") && Game1.dayOfMonth == 20 && Game1.random.NextDouble() < 0.05)
				{
					sourceRect = new Rectangle(640, 784, 16, 16);
					loops = 200;
					startingPosition.X = Game1.viewport.Width;
					speed = -3f;
				}
				if (!sourceRect.Equals(Rectangle.Empty) && Game1.viewport.X > -10000)
				{
					TemporaryAnimatedSprite t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, animationSpeed, Game1.currentSeason.Equals("winter") ? 2 : 4, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						layerDepth = 0f
					};
					t2.motion = new Vector2(speed, 0f);
					temporarySprites.Add(t2);
				}
			}
			if (Game1.viewport.X > -10000)
			{
				foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
				{
					temporarySprite.position.Y -= ((float)Game1.viewport.Y - Game1.previousViewportPosition.Y) / 8f;
					temporarySprite.drawAboveAlwaysFront = true;
				}
			}
			if (Game1.eventUp)
			{
				foreach (TemporaryAnimatedSprite t in temporarySprites)
				{
					if (t.attachedCharacter != null)
					{
						t.attachedCharacter.animateInFacingDirection(time);
					}
				}
			}
			else
			{
				isShowingEndSlideshow = false;
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			isShowingEndSlideshow = false;
			base.cleanupBeforePlayerExit();
			Game1.background = null;
			Game1.displayHUD = true;
			if (wind != null)
			{
				wind.Stop(AudioStopOptions.Immediate);
			}
		}

		protected override void resetLocalState()
		{
			isShowingEndSlideshow = false;
			isOutdoors.Value = false;
			base.resetLocalState();
			Game1.background = new Background();
			temporarySprites.Clear();
			Game1.displayHUD = false;
			Game1.changeMusicTrack("winter_day_ambient", track_interruptable: true, Game1.MusicContext.SubLocation);
			wind = Game1.soundBank.GetCue("wind");
			wind.Play();
			globalWind = 0f;
			windGust = 0.001f;
			if (!Game1.player.mailReceived.Contains("Summit_event") && Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && getSummitEvent() != "")
			{
				if (!Game1.player.songsHeard.Contains("end_credits"))
				{
					Game1.player.songsHeard.Add("end_credits");
				}
				Game1.player.mailReceived.Add("Summit_event");
				startEvent(new Event(getSummitEvent()));
			}
		}

		public string GetSummitDialogue(string file, string key)
		{
			string asset_path = "Data\\" + file + ":" + key;
			if (Game1.player.getSpouse() != null && Game1.player.getSpouse().Name == "Penny")
			{
				return Game1.content.LoadString(asset_path, "요");
			}
			return Game1.content.LoadString(asset_path, "");
		}

		private string getSummitEvent()
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				sb.Append("winter_day_ambient/-1000 -1000/farmer 9 23 0 ");
				if (Game1.player.isMarried() && Game1.player.getSpouse() != null && Game1.player.getSpouse().Name != "Krobus")
				{
					sb.Append(Game1.player.getSpouse().Name + " 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/move farmer 1 0 0/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Spouse") + "\"/viewport move 0 -1 4000/pause 5000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro2_Spouse" + (sayGrufferSummitIntro(Game1.player.getSpouse()) ? "_Gruff" : "")) + "\"/pause 400/emote farmer 56/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Spouse") + "\"/pause 2000/faceDirection " + Game1.player.getSpouse().Name + " 3/faceDirection farmer 1/pause 1000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Spouse") + "\"/pause 2000/faceDirection " + Game1.player.getSpouse().Name + " 0/faceDirection farmer 0/pause 2000/speak " + Game1.player.getSpouse().Name + " \"" + GetSummitDialogue("ExtraDialogue", ("SummitEvent_Dialogue3_" + Game1.player.getSpouse().Name) ?? "") + "\"/emote farmer 20/pause 500/faceDirection farmer 1/faceDirection " + Game1.player.getSpouse().Name + " 3/pause 1500/animate farmer false true 100 101/showKissFrame " + Game1.player.getSpouse().Name + "/playSound dwop/positionOffset farmer 8 0/positionOffset " + Game1.player.getSpouse().Name + " -4 0/specificTemporarySprite heart 11 12/pause 10");
				}
				else if (Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
				{
					sb.Append("Morris 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Morris") + "\"/viewport move 0 -1 4000/pause 5000/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Morris") + "\"/pause 2000/faceDirection Morris 3/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Morris") + "\"/pause 2000/faceDirection Morris 0/speak Morris \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Morris") + "\"/emote farmer 20/pause 10");
				}
				else
				{
					sb.Append("Lewis 11 13 0/skippable/viewport 10 17 clamp true/pause 2000/viewport move 0 -1 4000/move farmer 0 -10 0/pause 2000/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Intro_Lewis") + "\"/viewport move 0 -1 4000/pause 5000/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue1_Lewis") + "\"/pause 2000/faceDirection Lewis 3/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Dialogue2_Lewis") + "\"/pause 2000/faceDirection Lewis 0/speak Lewis \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_Outro_Lewis") + "\"/pause 10");
				}
				int pauseTime = 35000;
				if (Game1.player.mailReceived.Contains("Broken_Capsule"))
				{
					pauseTime += 8000;
				}
				if (Game1.player.totalMoneyEarned >= 100000000)
				{
					pauseTime += 8000;
				}
				if (Game1.year <= 2)
				{
					pauseTime += 8000;
				}
				sb.Append("/playMusic moonlightJellies/pause 2000/specificTemporarySprite krobusraven/viewport move 0 -1 12000/pause 10/pause " + pauseTime + "/pause 2000/playMusic none/viewport move 0 -1 5000/fade/playMusic end_credits/viewport -8000 -8000 true/removeTemporarySprites/specificTemporarySprite getEndSlideshow/pause 1000/playMusic none/pause 500");
				sb.Append("/playMusic grandpas_theme/pause 2000/fade/viewport -3000 -2000/specificTemporarySprite doneWithSlideShow/removeTemporarySprites/pause 3000/addTemporaryActor MrQi 16 32 -998 -1000 2 true/addTemporaryActor Grandpa 1 1 -100 -100 2 true/specificTemporarySprite grandpaSpirit/viewport -1000 -1000 true/pause 6000/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage2") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage3") + " \"/spriteText 3 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage4") + " \"/spriteText 7 \"" + GetSummitDialogue("ExtraDialogue", "SummitEvent_closingmessage5") + " \"/pause 400/playSound dwop/showFrame MrQi 1/pause 100/showFrame MrQi 2/pause 100/showFrame MrQi 3/pause 400/specificTemporarySprite grandpaThumbsUp/pause 10000/end");
			}
			catch (Exception)
			{
				return "";
			}
			return sb.ToString();
		}

		public string getEndSlideshow()
		{
			StringBuilder sb = new StringBuilder();
			Dictionary<string, string> charactersData = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
			int m = 0;
			foreach (KeyValuePair<string, string> v5 in charactersData)
			{
				try
				{
					if (!(v5.Key == "Marlon") && !(v5.Key == "Krobus") && !(v5.Key == "Dwarf") && !(v5.Key == "Sandy") && !(v5.Key == "Wizard"))
					{
						string texName = v5.Key;
						if (v5.Key == "Leo")
						{
							texName = "ParrotBoy";
						}
						base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\" + texName, new Rectangle(0, 96, 16, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m
						});
						m += 500;
					}
				}
				catch (Exception)
				{
				}
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 488, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 976, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Maps\\night_market_tilesheet_objects", new Rectangle(586, 119, 122, 28), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 1464, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 2000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(324, 1936, 12, 20), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + 192f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 14000,
				startSound = "dogWhining"
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(43, 80, 51, 56), 90f, 1, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-1f, -4f),
				delayBeforeAnimationStart = 27000,
				startSound = "trashbear",
				drawAboveAlwaysFront = true
			});
			sb.Append("pause 10/spriteText 5 \"" + Utility.loadStringShort("UI", "EndCredit_Neighbors") + " \"/pause 30000/");
			m += 4000;
			int oldTime4 = m;
			foreach (KeyValuePair<string, string> v4 in charactersData)
			{
				if (v4.Key == "Krobus" || v4.Key == "Dwarf" || v4.Key == "Sandy" || v4.Key == "Wizard")
				{
					int height2 = 32;
					if (v4.Key == "Krobus" || v4.Key == "Dwarf")
					{
						height2 = 24;
					}
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("Characters\\" + v4.Key, new Rectangle(0, height2 * 3, 16, height2), 120f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.4f + (float)((32 - height2) * 4)), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = m
					});
					m += 500;
				}
			}
			m += 5000;
			sb.Append("spriteText 4 \"" + Utility.loadStringShort("UI", "EndCredit_Animals") + " \"/pause " + (m - oldTime4 + 22000));
			oldTime4 = m;
			foreach (KeyValuePair<string, string> v3 in Game1.content.Load<Dictionary<string, string>>("Data\\FarmAnimals"))
			{
				if (!(v3.Key == "Hog") && !(v3.Key == "Brown Cow"))
				{
					int width = Convert.ToInt32(v3.Value.Split('/')[16]);
					int height = Convert.ToInt32(v3.Value.Split('/')[17]);
					int animalWidth2 = 0;
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + v3.Key, new Rectangle(0, height, width, height), 120f, 4, 999999, new Vector2(Game1.viewport.Width, (int)((float)Game1.viewport.Height * 0.5f - (float)(height * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = m
					});
					animalWidth2 += width * 4;
					int extra = (width > 16) ? 4 : 0;
					try
					{
						string babyString = "Baby" + v3.Key;
						if (v3.Key == "Duck")
						{
							babyString = "BabyWhite Chicken";
						}
						else if (v3.Key == "Dinosaur")
						{
							babyString = "Dinosaur";
						}
						Game1.content.Load<Texture2D>("Animals\\" + babyString);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + babyString, new Rectangle(0, height, width, height), 90f, 4, 999999, new Vector2(Game1.viewport.Width + (width + 2 + extra) * 4, (int)((float)Game1.viewport.Height * 0.5f - (float)(height * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\" + babyString, new Rectangle(0, height, width, height), 90f, 4, 999999, new Vector2(Game1.viewport.Width + (width + 2 + extra) * 2 * 4, (int)((float)Game1.viewport.Height * 0.5f - (float)(height * 4))), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m
						});
						animalWidth2 += (width + 2 + extra) * 4 * 2;
					}
					catch (Exception)
					{
					}
					base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(0, height, width, height), 120f, 1, 999999, new Vector2((float)(Game1.viewport.Width + animalWidth2 / 2) - Game1.dialogueFont.MeasureString(v3.Value.Split('/')[25]).X / 2f, (int)((float)Game1.viewport.Height * 0.5f + 12f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = m,
						text = v3.Value.Split('/')[25]
					});
					m += 2000 + extra * 300;
				}
			}
			if (Game1.player.catPerson)
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\cat" + ((Game1.player.whichPetBreed != 0) ? string.Concat(Game1.player.whichPetBreed) : ""), new Rectangle(0, 96, 32, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-4f, 0f),
					delayBeforeAnimationStart = 38000,
					startSound = "cat"
				});
			}
			else
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Animals\\dog" + ((Game1.player.whichPetBreed != 0) ? string.Concat(Game1.player.whichPetBreed) : ""), new Rectangle(0, 256, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-5f, 0f),
					delayBeforeAnimationStart = 38000,
					startSound = "dog_bark",
					pingPong = true
				});
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(64, 192, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 45000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 160, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 47000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 224, 32, 32), 90f, 6, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 128f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 48000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 320f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 49000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(32, 160, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 288f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 49500,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(34, 98, 32, 32), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 50000,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 32, 32, 32), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 50500,
				pingPong = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(256, 96, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 55600,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 128, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(48, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(96, 144, 16, 16), 90f, 3, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 57600,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(192, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 345.6f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 58000,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(128, 288, 16, 16), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 58300,
				pingPong = true,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 3000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 224, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 54000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\critters", new Rectangle(0, 240, 16, 16), 90f, 5, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 64f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 55000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(67, 190, 24, 51), 90f, 3, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, -4f),
				delayBeforeAnimationStart = 68000,
				rotation = -(float)Math.PI / 16f,
				pingPong = true,
				drawAboveAlwaysFront = true
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(0, 0, 57, 70), 150f, 2, 999999, new Vector2(Game1.viewport.Width / 2, Game1.viewport.Height), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, -4f),
				delayBeforeAnimationStart = 69000,
				rotation = -(float)Math.PI / 16f,
				drawAboveAlwaysFront = true
			});
			sb.Append("/spriteText 1 \"" + Utility.loadStringShort("UI", "EndCredit_Fish") + " \"/pause " + (m - oldTime4 + 18000));
			m += 6000;
			oldTime4 = m;
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 70000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 86000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 91000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 78, 28, 38), 250f, 2, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 152f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 102000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(257, 98, 182, 18), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 72f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 75000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 82000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 80000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", new Rectangle(0, 287, 47, 14), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 56f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 84000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(132, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 81500,
				yPeriodic = true,
				yPeriodicRange = 21f,
				yPeriodicLoopTime = 5000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(140, 20, 8, 8), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 48f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 83500,
				yPeriodic = true,
				yPeriodicRange = 21f,
				yPeriodicLoopTime = 5000f
			});
			Dictionary<int, string> dictionary = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			Dictionary<int, string> aquariumData = Game1.content.Load<Dictionary<int, string>>("Data\\AquariumFish");
			int stack = 0;
			foreach (KeyValuePair<int, string> v2 in dictionary)
			{
				try
				{
					int aquariumSpot = Convert.ToInt32(aquariumData[v2.Key].Split('/')[0]);
					Rectangle source = new Rectangle(24 * aquariumSpot % 480, 24 * aquariumSpot / 480 * 48, 24, 24);
					float textWidth = Game1.dialogueFont.MeasureString(Game1.IsEnglish() ? v2.Value.Split('/')[0] : v2.Value.Split('/')[13]).X;
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", source, 9999f, 1, 999999, new Vector2(Game1.viewport.Width + 192, (int)((float)Game1.viewport.Height * 0.53f - (float)(stack * 64) * 2f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = m,
						yPeriodic = true,
						yPeriodicLoopTime = Game1.random.Next(1500, 2100),
						yPeriodicRange = 4f
					});
					base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\AquariumFish", source, 9999f, 1, 999999, new Vector2((float)(Game1.viewport.Width + 192 + 48) - textWidth / 2f, (int)((float)Game1.viewport.Height * 0.53f - (float)(stack * 64) * 2f + 64f + 16f)), flicker: false, flipped: true, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
					{
						motion = new Vector2(-3f, 0f),
						delayBeforeAnimationStart = m,
						text = (Game1.IsEnglish() ? v2.Value.Split('/')[0] : v2.Value.Split('/')[13])
					});
					stack++;
					if (stack == 4)
					{
						m += 2000;
						stack = 0;
					}
				}
				catch (Exception)
				{
				}
			}
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-6f, 0f),
				delayBeforeAnimationStart = 123000,
				rotationChange = -0.1f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\projectiles", new Rectangle(64, 0, 16, 16), 909f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 339.2f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-6f, 0f),
				delayBeforeAnimationStart = 123300,
				rotationChange = -0.1f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 2564, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 640, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 5128, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(0, 1452, 300, 69), 900f, 1, 999999, new Vector2(Game1.viewport.Width + 7692, (float)Game1.viewport.Height * 0.5f - 392f), flicker: false, flipped: false, 0.2f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 108000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 110000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 115000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 118000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 121000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(32, 320, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 124000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(31, 58, 67, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 127000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 98, 32, 23), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 92f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 132000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(49, 131, 47, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 137000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 113000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 116000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 119000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 60, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 126000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 129000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 100, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 134000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 120, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 139000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(0, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 142000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\upperCavePlants", new Rectangle(96, 0, 48, 21), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 84f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 146000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 90f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 352f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 145000,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 2500f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\temporary_sprites_1", new Rectangle(2, 123, 19, 24), 100f, 4, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 358.4f), flicker: false, flipped: true, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 142500,
				yPeriodic = true,
				yPeriodicRange = 8f,
				yPeriodicLoopTime = 2000f
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 149000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(65, 0, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 151000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(96, 90, 31, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 154000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\bushes", new Rectangle(0, 176, 104, 29), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 116f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 156000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 0, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 155000
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 20, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 152500
			});
			base.TemporarySprites.Add(new TemporaryAnimatedSprite("TerrainFeatures\\grass", new Rectangle(0, 40, 44, 13), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f + 240f - 52f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-3f, 0f),
				delayBeforeAnimationStart = 158000
			});
			if (Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Value.ToLower().Equals("concernedape"))
			{
				base.TemporarySprites.Add(new TemporaryAnimatedSprite("Minigames\\Clouds", new Rectangle(210, 842, 138, 130), 900f, 1, 999999, new Vector2(Game1.viewport.Width, (float)Game1.viewport.Height * 0.5f - 240f), flicker: false, flipped: false, 0.7f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					motion = new Vector2(-3f, 0f),
					delayBeforeAnimationStart = 160500,
					startSound = "discoverMineral"
				});
			}
			sb.Append("/spriteText 2 \"" + Utility.loadStringShort("UI", "EndCredit_Monsters") + " \"/pause " + (m - oldTime4 + 19000));
			m += 6000;
			oldTime4 = m;
			foreach (KeyValuePair<string, string> v in Game1.content.Load<Dictionary<string, string>>("Data\\Monsters"))
			{
				if (!(v.Key == "Fireball") && !(v.Key == "Skeleton Warrior"))
				{
					int spriteHeight6 = 16;
					int spriteWidth2 = 16;
					int spriteStartIndex3 = 0;
					int animationLength = 4;
					bool pingpong = false;
					int yOffset = 0;
					Character attachedCharacter6 = null;
					if (v.Key.Contains("Bat") || v.Key.Contains("Ghost"))
					{
						spriteHeight6 = 24;
					}
					switch (v.Key)
					{
					case "Grub":
					case "Rock Crab":
					case "Lava Crab":
					case "Iridium Crab":
					case "Stone Golem":
					case "Fly":
					case "Duggy":
					case "Magma Duggy":
					case "Wilderness Golem":
						spriteHeight6 = 24;
						spriteStartIndex3 = 4;
						break;
					case "False Magma Cap":
					case "Dust Spirit":
						spriteHeight6 = 24;
						spriteStartIndex3 = 0;
						break;
					case "Pepper Rex":
						spriteWidth2 = 32;
						spriteHeight6 = 32;
						break;
					case "Lava Lurk":
						spriteStartIndex3 = 4;
						pingpong = true;
						break;
					case "Magma Sparker":
					case "Magma Sprite":
						animationLength = 7;
						spriteStartIndex3 = 7;
						break;
					case "Big Slime":
						spriteHeight6 = 32;
						spriteWidth2 = 32;
						yOffset = 64;
						attachedCharacter6 = new BigSlime(Vector2.Zero, 0);
						break;
					case "Blue Squid":
						spriteWidth2 = 24;
						spriteHeight6 = 24;
						animationLength = 5;
						break;
					case "Spider":
						spriteWidth2 = 32;
						spriteHeight6 = 32;
						animationLength = 2;
						break;
					case "Serpent":
						spriteWidth2 = 32;
						spriteHeight6 = 32;
						animationLength = 5;
						break;
					case "Spiker":
					case "Carbon Ghost":
					case "Ghost":
					case "Dwarvish Sentry":
					case "Putrid Ghost":
						animationLength = 1;
						break;
					case "Skeleton":
					case "Mummy":
					case "Skeleton Mage":
						spriteHeight6 = 32;
						spriteStartIndex3 = 4;
						break;
					case "Shadow Guy":
					{
						spriteHeight6 = 32;
						spriteStartIndex3 = 4;
						Texture2D t5 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Brute");
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t5.Width, spriteWidth2 * spriteStartIndex3 / t5.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t5
						});
						t5 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Shaman");
						spriteHeight6 = 24;
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t5.Width, spriteWidth2 * spriteStartIndex3 / t5.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t5
						});
						t5 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Shadow Sniper");
						spriteHeight6 = 32;
						spriteWidth2 = 32;
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t5.Width, spriteWidth2 * spriteStartIndex3 / t5.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t5
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t5.Width, spriteWidth2 * spriteStartIndex3 / t5.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 128 + spriteWidth2 * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							text = Utility.loadStringShort("UI", "EndCredit_ShadowPeople")
						});
						m += 1500;
						continue;
					}
					case "Bat":
					{
						Texture2D t8 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Frost Bat");
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t8.Width, spriteWidth2 * spriteStartIndex3 / t8.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t8
						});
						t8 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Lava Bat");
						spriteHeight6 = 24;
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t8.Width, spriteWidth2 * spriteStartIndex3 / t8.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t8
						});
						t8 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Iridium Bat");
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t8.Width, spriteWidth2 * spriteStartIndex3 / t8.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = t8
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t8.Width, spriteWidth2 * spriteStartIndex3 / t8.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 128 + spriteWidth2 * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							text = Utility.loadStringShort("UI", "EndCredit_Bats")
						});
						m += 1500;
						continue;
					}
					case "Green Slime":
					{
						Texture2D t9 = null;
						if (attachedCharacter6 == null)
						{
							t9 = Game1.content.Load<Texture2D>("Characters\\Monsters\\Green Slime");
						}
						spriteHeight6 = 32;
						spriteStartIndex3 = 4;
						attachedCharacter6 = new GreenSlime(Vector2.Zero, 0);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192 - 64, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = null
						});
						attachedCharacter6 = new GreenSlime(Vector2.Zero, 41);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 96f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = null
						});
						attachedCharacter6 = new GreenSlime(Vector2.Zero, 81);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 288f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = null
						});
						attachedCharacter6 = new GreenSlime(Vector2.Zero, 121);
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 240f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = null
						});
						attachedCharacter6 = new GreenSlime(Vector2.Zero, 0);
						(attachedCharacter6 as GreenSlime).makeTigerSlime();
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)Game1.viewport.Width + 144f - 64f, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4 * 2) + 32f), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = null
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t9.Width, spriteWidth2 * spriteStartIndex3 / t9.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 192 + spriteWidth2 * 4 / 2) - Game1.dialogueFont.MeasureString(v.Value.Split('/')[14]).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							text = Utility.loadStringShort("UI", "EndCredit_Slimes")
						});
						m += 1500;
						continue;
					}
					case "Shadow Shaman":
					case "Shadow Sniper":
					case "Shadow Brute":
					case "Cat":
					case "Frog":
					case "Crow":
					case "Frost Jelly":
					case "Sludge":
					case "Iridium Slime":
					case "Tiger Slime":
					case "Lava Bat":
					case "Iridium Bat":
					case "Frost Bat":
					case "Royal Serpent":
						continue;
					}
					try
					{
						Texture2D t2 = null;
						t2 = ((attachedCharacter6 != null) ? attachedCharacter6.Sprite.Texture : Game1.content.Load<Texture2D>("Characters\\Monsters\\" + v.Key));
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t2.Width, spriteWidth2 * spriteStartIndex3 / t2.Width * spriteHeight6 + 1, spriteWidth2, spriteHeight6 - 1), 100f, animationLength, 999999, new Vector2(Game1.viewport.Width + 192, (float)Game1.viewport.Height * 0.5f - (float)(spriteHeight6 * 4) - 16f + (float)yOffset), flicker: false, flipped: true, 0.9f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							yPeriodic = (animationLength == 1),
							yPeriodicRange = 16f,
							yPeriodicLoopTime = 3000f,
							attachedCharacter = attachedCharacter6,
							texture = ((attachedCharacter6 == null) ? t2 : null),
							pingPong = pingpong
						});
						base.TemporarySprites.Add(new TemporaryAnimatedSprite(null, new Rectangle(spriteWidth2 * spriteStartIndex3 % t2.Width, spriteWidth2 * spriteStartIndex3 / t2.Width * spriteHeight6, spriteWidth2, spriteHeight6), 100f, animationLength, 999999, new Vector2((float)(Game1.viewport.Width + 192 + spriteWidth2 * 4 / 2) - Game1.dialogueFont.MeasureString(Game1.parseText(v.Value.Split('/')[14], Game1.dialogueFont, 256)).X / 2f, (float)Game1.viewport.Height * 0.5f), flicker: false, flipped: false, 0.9f, 0f, Color.White, 1f, 0f, 0f, 0f, local: true)
						{
							motion = new Vector2(-3f, 0f),
							delayBeforeAnimationStart = m,
							text = Game1.parseText(v.Value.Split('/')[14], Game1.dialogueFont, 256)
						});
						m += 1500;
					}
					catch (Exception)
					{
					}
				}
			}
			return sb.ToString();
		}

		private bool sayGrufferSummitIntro(NPC spouse)
		{
			switch ((string)spouse.name)
			{
			case "Harvey":
			case "Elliott":
				return false;
			case "Abigail":
			case "Maru":
				return true;
			default:
				return spouse.Gender == 0;
			}
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			if (critters != null && Game1.farmEvent == null)
			{
				for (int i = 0; i < critters.Count; i++)
				{
					critters[i].drawAboveFrontLayer(b);
				}
			}
			foreach (NPC character in characters)
			{
				character.drawAboveAlwaysFrontLayer(b);
			}
			foreach (Projectile projectile in projectiles)
			{
				projectile.draw(b);
			}
			if (Game1.eventUp)
			{
				if (isShowingEndSlideshow)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 400f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 412f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 432f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 468f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f - 536f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 240f), Game1.viewport.Width, 8), Utility.GetPrismaticColor());
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 256f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.8f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 276f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.6f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 312f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.4f);
					b.Draw(Game1.staminaRect, new Rectangle(0, (int)((float)Game1.viewport.Height * 0.5f + 380f), Game1.viewport.Width, 4), Utility.GetPrismaticColor() * 0.2f);
				}
				foreach (TemporaryAnimatedSprite s in base.TemporarySprites)
				{
					if (s.drawAboveAlwaysFront)
					{
						s.draw(b);
					}
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
		}
	}
}
