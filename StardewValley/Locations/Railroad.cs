using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using System;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class Railroad : GameLocation
	{
		public const int trainSoundDelay = 15000;

		[XmlIgnore]
		public readonly NetRef<Train> train = new NetRef<Train>();

		[XmlElement("hasTrainPassed")]
		private readonly NetBool hasTrainPassed = new NetBool(value: false);

		private int trainTime = -1;

		[XmlIgnore]
		public readonly NetInt trainTimer = new NetInt(0);

		public static ICue trainLoop;

		[XmlElement("witchStatueGone")]
		public readonly NetBool witchStatueGone = new NetBool(value: false);

		public Railroad()
		{
		}

		public Railroad(string map, string name)
			: base(map, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(train, hasTrainPassed, witchStatueGone, trainTimer);
			witchStatueGone.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
			{
				if (!oldValue && newValue && base.Map != null)
				{
					DelayedAction.removeTileAfterDelay(54, 35, 2000, this, "Buildings");
					DelayedAction.removeTileAfterDelay(54, 34, 2000, this, "Front");
				}
			};
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (Game1.getMusicTrackName().ToLower().Contains("ambient"))
			{
				Game1.changeMusicTrack("none");
			}
			if ((bool)witchStatueGone || Game1.MasterPlayer.mailReceived.Contains("witchStatueGone"))
			{
				removeTile(54, 35, "Buildings");
				removeTile(54, 34, "Front");
			}
			if (!Game1.IsWinter)
			{
				AmbientLocationSounds.addSound(new Vector2(15f, 56f), 0);
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			if (trainLoop != null)
			{
				trainLoop.Stop(AudioStopOptions.Immediate);
			}
			trainLoop = null;
		}

		public override void checkForMusic(GameTime time)
		{
			if (Game1.timeOfDay < 1800 && !Game1.isRaining && !Game1.eventUp)
			{
				string currentSeason = Game1.currentSeason;
				if (currentSeason == "summer" || currentSeason == "fall" || currentSeason == "spring")
				{
					Game1.changeMusicTrack(Game1.currentSeason + "_day_ambient");
				}
			}
			else if (Game1.timeOfDay >= 2000 && !Game1.isRaining && !Game1.eventUp)
			{
				string currentSeason = Game1.currentSeason;
				if (currentSeason == "summer" || currentSeason == "fall" || currentSeason == "spring")
				{
					Game1.changeMusicTrack("spring_night_ambient");
				}
			}
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (who.secretNotesSeen.Contains(16) && xLocation == 12 && yLocation == 38 && !who.mailReceived.Contains("SecretNote16_done"))
			{
				who.mailReceived.Add("SecretNote16_done");
				Game1.createObjectDebris(166, xLocation, yLocation, who.UniqueMultiplayerID, this);
				return "";
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (map.GetLayer("Buildings").Tiles[tileLocation] != null)
			{
				int tileIndex = map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
				if (tileIndex == 287)
				{
					if (Game1.player.hasDarkTalisman)
					{
						Game1.player.freezePause = 7000;
						playSound("fireball");
						DelayedAction.playSoundAfterDelay("secret1", 2000);
						DelayedAction.removeTemporarySpriteAfterDelay(this, 9999f, 2000);
						witchStatueGone.Value = true;
						who.mailReceived.Add("witchStatueGone");
						for (int i = 0; i < 22; i++)
						{
							DelayedAction.playSoundAfterDelay("batFlap", 2220 + 240 * i);
						}
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(576, 271, 28, 31), 60f, 3, 999, new Vector2(54f, 34f) * 64f + new Vector2(-2f, 1f) * 4f, flicker: false, flipped: false, 0.2176f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 8000f,
							xPeriodicRange = 384f,
							motion = new Vector2(-2f, 0f),
							acceleration = new Vector2(0f, -0.015f),
							pingPong = true,
							delayBeforeAnimationStart = 2000
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 50f, 7, 999, new Vector2(54f, 34f) * 64f + new Vector2(7f, 11f) * 4f, flicker: false, flipped: false, 0.2177f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 8000f,
							xPeriodicRange = 384f,
							motion = new Vector2(-2f, 0f),
							acceleration = new Vector2(0f, -0.015f),
							delayBeforeAnimationStart = 2000
						});
						Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(0, 499, 10, 11), 35.715f, 7, 8, new Vector2(54f, 34f) * 64f + new Vector2(3f, 10f) * 4f, flicker: false, flipped: false, 0.2305f, 0f, Color.White, 4f, 0f, 0f, 0f)
						{
							id = 9999f
						});
					}
					else
					{
						Game1.drawObjectDialogue("???");
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			base.DayUpdate(dayOfMonth);
			hasTrainPassed.Value = false;
			trainTime = -1;
			Random r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed);
			if (r.NextDouble() < 0.2 && Game1.isLocationAccessible("Railroad"))
			{
				trainTime = r.Next(900, 1800);
				trainTime -= trainTime % 10;
			}
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
		{
			if (!Game1.eventUp && train.Value != null && train.Value.getBoundingBox().Intersects(position))
			{
				return true;
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
		}

		public void setTrainComing(int delay)
		{
			trainTimer.Value = delay;
			if (Game1.IsMasterGame)
			{
				PlayTrainApproach();
				Game1.multiplayer.sendServerToClientsMessage("trainApproach");
			}
		}

		public void PlayTrainApproach()
		{
			if ((bool)Game1.currentLocation.isOutdoors && !Game1.isFestival())
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:Railroad_TrainComing"));
				if (Game1.soundBank != null)
				{
					ICue cue = Game1.soundBank.GetCue("distantTrain");
					cue.SetVariable("Volume", 100f);
					cue.Play();
				}
			}
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.player.secretNotesSeen.Contains(GameLocation.NECKLACE_SECRET_NOTE_INDEX) && !Game1.player.hasOrWillReceiveMail(GameLocation.CAROLINES_NECKLACE_MAIL))
			{
				Game1.player.mailForTomorrow.Add(GameLocation.CAROLINES_NECKLACE_MAIL + "%&NL&%");
				Object result = new Object(GameLocation.CAROLINES_NECKLACE_ITEM, 1);
				Game1.player.addQuest(128);
				Game1.player.addQuest(129);
				return result;
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		public override bool isTileFishable(int tileX, int tileY)
		{
			if (Game1.currentSeason.Equals("winter"))
			{
				return false;
			}
			return base.isTileFishable(tileX, tileY);
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
			if (train.Value != null && train.Value.Update(time, this) && Game1.IsMasterGame)
			{
				train.Value = null;
			}
			if (!Game1.IsMasterGame)
			{
				return;
			}
			if (Game1.timeOfDay == trainTime - trainTime % 10 && (int)trainTimer == 0 && !Game1.isFestival() && train.Value == null)
			{
				setTrainComing(15000);
			}
			if ((int)trainTimer > 0)
			{
				trainTimer.Value -= time.ElapsedGameTime.Milliseconds;
				if ((int)trainTimer <= 0)
				{
					train.Value = new Train();
					playSound("trainWhistle");
				}
				if ((int)trainTimer < 3500 && Game1.currentLocation == this && Game1.soundBank != null && (trainLoop == null || !trainLoop.IsPlaying))
				{
					trainLoop = Game1.soundBank.GetCue("trainLoop");
					trainLoop.SetVariable("Volume", 0f);
					trainLoop.Play();
				}
			}
			if (train.Value != null)
			{
				if (Game1.currentLocation == this && Game1.soundBank != null && (trainLoop == null || !trainLoop.IsPlaying))
				{
					trainLoop = Game1.soundBank.GetCue("trainLoop");
					trainLoop.SetVariable("Volume", 0f);
					trainLoop.Play();
				}
				if (trainLoop != null && trainLoop.GetVariable("Volume") < 100f)
				{
					trainLoop.SetVariable("Volume", trainLoop.GetVariable("Volume") + 0.5f);
				}
			}
			else if (trainLoop != null && (int)trainTimer <= 0)
			{
				trainLoop.SetVariable("Volume", trainLoop.GetVariable("Volume") - 0.15f);
				if (trainLoop.GetVariable("Volume") <= 0f)
				{
					trainLoop.Stop(AudioStopOptions.Immediate);
					trainLoop = null;
				}
			}
			else if ((int)trainTimer > 0 && trainLoop != null)
			{
				trainLoop.SetVariable("Volume", trainLoop.GetVariable("Volume") + 0.15f);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (train.Value != null && !Game1.eventUp)
			{
				train.Value.draw(b);
			}
		}
	}
}
