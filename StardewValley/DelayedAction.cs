using Microsoft.Xna.Framework;

namespace StardewValley
{
	public class DelayedAction
	{
		public delegate void delayedBehavior();

		public int timeUntilAction;

		public float floatData;

		public string stringData;

		public Point pointData;

		public NPC character;

		public GameLocation location;

		public delayedBehavior behavior;

		public Game1.afterFadeFunction afterFadeBehavior;

		public bool waitUntilMenusGone;

		public TemporaryAnimatedSprite temporarySpriteData;

		public DelayedAction(int timeUntilAction)
		{
			this.timeUntilAction = timeUntilAction;
		}

		public DelayedAction(int timeUntilAction, delayedBehavior behavior)
		{
			this.timeUntilAction = timeUntilAction;
			this.behavior = behavior;
		}

		public bool update(GameTime time)
		{
			if (!waitUntilMenusGone || Game1.activeClickableMenu == null)
			{
				timeUntilAction -= time.ElapsedGameTime.Milliseconds;
				if (timeUntilAction <= 0)
				{
					behavior();
				}
			}
			return timeUntilAction <= 0;
		}

		public static void warpAfterDelay(string nameToWarpTo, Point pointToWarp, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.warp;
			action.stringData = nameToWarpTo;
			action.pointData = pointToWarp;
			Game1.delayedActions.Add(action);
		}

		public static void addTemporarySpriteAfterDelay(TemporaryAnimatedSprite t, GameLocation l, int timer, bool waitUntilMenusGone = false)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.addTempSprite;
			action.temporarySpriteData = t;
			action.location = l;
			action.waitUntilMenusGone = waitUntilMenusGone;
			Game1.delayedActions.Add(action);
		}

		public static void playSoundAfterDelay(string soundName, int timer, GameLocation location = null, int pitch = -1)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.playSound;
			action.stringData = soundName;
			action.location = location;
			action.floatData = pitch;
			Game1.delayedActions.Add(action);
		}

		public static void removeTemporarySpriteAfterDelay(GameLocation location, float idOfTempSprite, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.removeTemporarySprite;
			action.location = location;
			action.floatData = idOfTempSprite;
			Game1.delayedActions.Add(action);
		}

		public static DelayedAction playMusicAfterDelay(string musicName, int timer, bool interruptable = true)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.changeMusicTrack;
			action.stringData = musicName;
			if (interruptable)
			{
				action.floatData = 1f;
			}
			else
			{
				action.floatData = 0f;
			}
			Game1.delayedActions.Add(action);
			return action;
		}

		public static void textAboveHeadAfterDelay(string text, NPC who, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.showTextAboveHead;
			action.stringData = text;
			action.character = who;
			Game1.delayedActions.Add(action);
		}

		public static void stopFarmerGlowing(int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.stopGlowing;
			Game1.delayedActions.Add(action);
		}

		public static void showDialogueAfterDelay(string dialogue, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.showDialogue;
			action.stringData = dialogue;
			Game1.delayedActions.Add(action);
		}

		public static void screenFlashAfterDelay(float intensity, int timer, string sound = "")
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.screenFlash;
			action.stringData = sound;
			action.floatData = intensity;
			Game1.delayedActions.Add(action);
		}

		public static void removeTileAfterDelay(int x, int y, int timer, GameLocation l, string whichLayer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.removeBuildingsTile;
			action.pointData = new Point(x, y);
			action.location = l;
			action.stringData = whichLayer;
			Game1.delayedActions.Add(action);
		}

		public static void fadeAfterDelay(Game1.afterFadeFunction behaviorAfterFade, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = action.doGlobalFade;
			action.afterFadeBehavior = behaviorAfterFade;
			Game1.delayedActions.Add(action);
		}

		public static void functionAfterDelay(delayedBehavior func, int timer)
		{
			DelayedAction action = new DelayedAction(timer);
			action.behavior = func;
			Game1.delayedActions.Add(action);
		}

		public void doGlobalFade()
		{
			Game1.globalFadeToBlack(afterFadeBehavior);
		}

		public void showTextAboveHead()
		{
			if (character != null && stringData != null)
			{
				character.showTextAboveHead(stringData);
			}
		}

		public void addTempSprite()
		{
			if (location != null && temporarySpriteData != null)
			{
				location.TemporarySprites.Add(temporarySpriteData);
			}
		}

		public void stopGlowing()
		{
			Game1.player.stopGlowing();
			Game1.player.stopJittering();
			Game1.screenGlowHold = false;
			if (Game1.isFestival() && Game1.currentSeason.Equals("fall"))
			{
				Game1.changeMusicTrack("fallFest");
			}
		}

		public void showDialogue()
		{
			Game1.drawObjectDialogue(stringData);
		}

		public void warp()
		{
			if (stringData != null)
			{
				_ = pointData;
				Game1.warpFarmer(stringData, pointData.X, pointData.Y, flip: false);
			}
		}

		public void removeBuildingsTile()
		{
			_ = pointData;
			if (location != null && stringData != null)
			{
				location.removeTile(pointData.X, pointData.Y, stringData);
			}
		}

		public void removeTemporarySprite()
		{
			if (location != null)
			{
				location.removeTemporarySpritesWithID(floatData);
			}
		}

		public void playSound()
		{
			if (stringData != null)
			{
				if (location == null)
				{
					Game1.playSound(stringData);
				}
				else if (floatData != -1f)
				{
					location.playSoundPitched(stringData, (int)floatData);
				}
				else
				{
					location.playSound(stringData);
				}
			}
		}

		public void changeMusicTrack()
		{
			if (stringData != null)
			{
				Game1.changeMusicTrack(stringData, floatData > 0f);
			}
		}

		public void screenFlash()
		{
			if (stringData != null && stringData.Length > 0)
			{
				Game1.playSound(stringData);
			}
			Game1.flashAlpha = floatData;
		}
	}
}
