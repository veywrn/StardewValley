using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley.Objects
{
	[InstanceStatics]
	public class Phone : Object
	{
		public enum PhoneCalls
		{
			NONE = -1,
			Vincent,
			Lewis,
			Pierre,
			Foreign,
			Bear,
			Hat,
			Curse,
			Robo,
			MAX
		}

		public const int RING_DURATION = 600;

		public const int RING_CYCLE_TIME = 1800;

		public static Random r;

		protected static bool _phoneSoundPlayed = false;

		public static int ringingTimer;

		public static int whichPhoneCall = -1;

		public static long lastRunTick = -1L;

		public static long lastMinutesElapsedTick = -1L;

		public static int intervalsToRing = 0;

		public Phone()
		{
		}

		public Phone(Vector2 position)
			: base(position, 214)
		{
			Name = "Telephone";
			type.Value = "Crafting";
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (CanHearCall(whichPhoneCall) && whichPhoneCall >= 0)
			{
				int which_call = whichPhoneCall;
				if (_phoneSoundPlayed)
				{
					Game1.soundBank.GetCue("phone").Stop(AudioStopOptions.Immediate);
					_phoneSoundPlayed = false;
				}
				Game1.playSound("openBox");
				Game1.player.freezePause = 500;
				DelayedAction.functionAfterDelay(delegate
				{
					switch (which_call)
					{
					case 0:
						Game1.drawDialogue(Game1.getCharacterFromName("Vincent"), Game1.content.LoadString("Strings\\Characters:Phone_Ring_Vincent"));
						break;
					case 1:
						Game1.drawDialogue(Game1.getCharacterFromName("Lewis"), Game1.content.LoadString("Strings\\Characters:Phone_Ring_Lewis"));
						break;
					case 2:
						Game1.drawDialogue(Game1.getCharacterFromName("Pierre"), Game1.content.LoadString("Strings\\Characters:Phone_Ring_Pierre"));
						break;
					case 3:
						Game1.multipleDialogues(Game1.content.LoadString("Strings\\Characters:Phone_Ring_Foreign").Split('#'));
						break;
					case 4:
						Game1.drawDialogue(new NPC(new AnimatedSprite("Characters\\Bear", 0, 32, 32), Vector2.Zero, "", 0, "Bear", null, Game1.temporaryContent.Load<Texture2D>("Portraits\\Bear"), eventActor: false), Game1.content.LoadString("Strings\\Characters:Phone_Ring_Bear"));
						break;
					case 5:
						Game1.multipleDialogues(Game1.content.LoadString("Strings\\Characters:Phone_Ring_HatMouse").Split('#'));
						break;
					case 6:
						Game1.multipleDialogues(Game1.content.LoadString("Strings\\Characters:Phone_Ring_Cursed").Split('#'));
						break;
					case 7:
						Game1.multipleDialogues(Game1.content.LoadString("Strings\\Characters:Phone_Ring_RoboCaller").Split('#'));
						break;
					}
					Game1.player.callsReceived[which_call] = 1;
				}, 500);
			}
			else
			{
				Game1.game1.ShowTelephoneMenu();
			}
			ringingTimer = 0;
			whichPhoneCall = -1;
			return true;
		}

		public static bool CanHearCall(int which_phone_call)
		{
			if (which_phone_call == 4 && !Game1.player.eventsSeen.Contains(2120303))
			{
				return false;
			}
			if (which_phone_call == 6 && !Game1.player.mailReceived.Contains("cursed_doll"))
			{
				return false;
			}
			return true;
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			base.performRemoveAction(tileLocation, environment);
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (environment != Game1.currentLocation)
			{
				return;
			}
			if (Game1.ticks != lastRunTick)
			{
				if (Game1.eventUp)
				{
					return;
				}
				lastRunTick = Game1.ticks;
				if (whichPhoneCall >= 0 && CanHearCall(whichPhoneCall) && Game1.shouldTimePass())
				{
					if (ringingTimer == 0)
					{
						Game1.playSound("phone");
						_phoneSoundPlayed = true;
					}
					ringingTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
					if (ringingTimer >= 1800)
					{
						ringingTimer = 0;
						_phoneSoundPlayed = false;
					}
				}
			}
			base.updateWhenCurrentLocation(time, environment);
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
			_phoneSoundPlayed = false;
			ringingTimer = 0;
			whichPhoneCall = -1;
			intervalsToRing = 0;
		}

		public override bool minutesElapsed(int minutes, GameLocation environment)
		{
			if (!Game1.IsMasterGame)
			{
				return false;
			}
			if (lastMinutesElapsedTick != Game1.ticks)
			{
				lastMinutesElapsedTick = Game1.ticks;
				if (intervalsToRing == 0)
				{
					if (r == null)
					{
						r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed);
					}
					if (r.NextDouble() < 0.01)
					{
						int which_call = r.Next(8);
						if (Game1.timeOfDay < 1800 || which_call == 5)
						{
							intervalsToRing = 3;
							Game1.player.team.ringPhoneEvent.Fire(which_call);
						}
					}
				}
				else
				{
					intervalsToRing--;
					if (intervalsToRing <= 0)
					{
						Game1.player.team.ringPhoneEvent.Fire(-1);
					}
				}
			}
			return base.minutesElapsed(minutes, environment);
		}

		public static void Ring(int which_call)
		{
			if (which_call < 0)
			{
				whichPhoneCall = -1;
				ringingTimer = 0;
				if (_phoneSoundPlayed)
				{
					Game1.soundBank.GetCue("phone").Stop(AudioStopOptions.Immediate);
					_phoneSoundPlayed = false;
				}
			}
			else if (!Game1.player.callsReceived.ContainsKey(which_call))
			{
				whichPhoneCall = which_call;
				ringingTimer = 0;
				_phoneSoundPlayed = false;
			}
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			base.draw(spriteBatch, x, y, alpha);
			bool ringing = ringingTimer > 0 && ringingTimer < 600;
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)position.X + ((ringing || shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)position.Y + ((ringing || shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64, 128);
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 20) / 10000f) + (float)x * 1E-05f;
			spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, Object.getSourceRectForBigCraftable(base.ParentSheetIndex + 1), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
		}
	}
}
