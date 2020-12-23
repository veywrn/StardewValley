using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class IslandSecret : IslandLocation
	{
		[XmlIgnore]
		public List<SuspensionBridge> suspensionBridges = new List<SuspensionBridge>();

		[XmlElement("addedSlimesToday")]
		private readonly NetBool addedSlimesToday = new NetBool();

		public IslandSecret()
		{
		}

		public IslandSecret(string map, string name)
			: base(map, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(addedSlimesToday);
		}

		protected override void resetSharedState()
		{
			base.resetSharedState();
			if ((bool)addedSlimesToday)
			{
				return;
			}
			addedSlimesToday.Value = true;
			Random rand = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame + 12);
			Microsoft.Xna.Framework.Rectangle spawnArea = new Microsoft.Xna.Framework.Rectangle(13, 15, 7, 6);
			for (int tries = 5; tries > 0; tries--)
			{
				Vector2 tile = Utility.getRandomPositionInThisRectangle(spawnArea, rand);
				if (isTileLocationTotallyClearAndPlaceable(tile))
				{
					GreenSlime i = new GreenSlime(tile * 64f, 9999899);
					characters.Add(i);
				}
			}
			if (rand.NextDouble() < 0.5 && isTileLocationTotallyClearAndPlaceable(new Vector2(17f, 18f)))
			{
				objects.Add(new Vector2(17f, 18f), new Object(new Vector2(17f, 18f), 56));
			}
			GreenSlime slime2 = new GreenSlime(new Vector2(42f, 34f) * 64f);
			slime2.makeTigerSlime();
			characters.Add(slime2);
			slime2 = new GreenSlime(new Vector2(38f, 33f) * 64f);
			slime2.makeTigerSlime();
			characters.Add(slime2);
		}

		public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
		{
			if (xLocation == 82 && yLocation == 83 && who.secretNotesSeen.Contains(1002))
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("Island_Secret_BuriedTreasureNut"))
				{
					Game1.createItemDebris(new Object(73, 1), new Vector2(xLocation, yLocation) * 64f, 1);
					Game1.addMailForTomorrow("Island_Secret_BuriedTreasureNut", noLetter: true, sendToEveryone: true);
				}
				if (!Game1.player.hasOrWillReceiveMail("Island_Secret_BuriedTreasure"))
				{
					Game1.createItemDebris(new Object(166, 1), new Vector2(xLocation, yLocation) * 64f, 1);
					Game1.addMailForTomorrow("Island_Secret_BuriedTreasure", noLetter: true);
				}
			}
			return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			suspensionBridges.Clear();
			suspensionBridges.Add(new SuspensionBridge(46, 44));
			suspensionBridges.Add(new SuspensionBridge(47, 34));
			NPC i = getCharacterFromName("Birdie");
			if (i != null)
			{
				if (i.Sprite.SourceRect.Width < 32)
				{
					i.extendSourceRect(16, 0);
				}
				i.Sprite.SpriteWidth = 32;
				i.Sprite.ignoreSourceRectUpdates = false;
				i.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(8, 1000, 0, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(9, 1000, 0, secondaryArm: false, flip: false)
				});
				i.Sprite.loop = true;
				i.HideShadow = true;
				i.IsInvisible = Game1.IsRainingHere(this);
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			if (getCharacterFromName("Birdie") != null && !getCharacterFromName("Birdie").IsInvisible && getCharacterFromName("Birdie").getTileLocation().Equals(new Vector2(tileLocation.X, tileLocation.Y)))
			{
				if (!who.mailReceived.Contains("birdieQuestBegun"))
				{
					Game1.globalFadeToBlack(delegate
					{
						startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro")));
					});
					who.mailReceived.Add("birdieQuestBegun");
				}
				else if (!who.mailReceived.Contains("birdieQuestFinished") && who.ActiveObject != null && (int)who.ActiveObject.parentSheetIndex == 870)
				{
					Game1.globalFadeToBlack(delegate
					{
						startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished")));
						who.ActiveObject = null;
					});
					who.mailReceived.Add("birdieQuestFinished");
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override void DayUpdate(int dayOfMonth)
		{
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i] is Monster)
				{
					characters.RemoveAt(i);
				}
			}
			addedSlimesToday.Value = false;
			base.DayUpdate(dayOfMonth);
		}

		public override bool performAction(string action, Farmer who, Location tileLocation)
		{
			if (action != null && action == "BananaShrine" && who.CurrentItem != null && who.CurrentItem is Object && (int)who.CurrentItem.parentSheetIndex == 91 && getTemporarySpriteByID(777) == null)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", new Microsoft.Xna.Framework.Rectangle(304, 48, 16, 16), new Vector2(tileLocation.X, tileLocation.Y - 1) * 64f, flipped: false, 0f, Color.White)
				{
					id = 888f,
					scale = 4f,
					layerDepth = ((float)tileLocation.Y + 1.2f) * 64f / 10000f
				});
				temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32), 400f, 2, 999, new Vector2(15.5f, 20f) * 64f, flicker: false, flipped: false, 0.128f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					id = 777f,
					yStopCoordinate = 1561,
					motion = new Vector2(0f, 2f),
					reachedStopCoordinate = gorillaReachedShrine,
					delayBeforeAnimationStart = 1000
				});
				playSound("coin");
				DelayedAction.playSoundAfterDelay("grassyStep", 1400);
				DelayedAction.playSoundAfterDelay("grassyStep", 1800);
				DelayedAction.playSoundAfterDelay("grassyStep", 2200);
				DelayedAction.playSoundAfterDelay("grassyStep", 2600);
				DelayedAction.playSoundAfterDelay("grassyStep", 3000);
				who.reduceActiveItemByOne();
				Game1.changeMusicTrack("none");
				DelayedAction.playSoundAfterDelay("gorilla_intro", 2000);
			}
			return base.performAction(action, who, tileLocation);
		}

		private void gorillaReachedShrine(int extra)
		{
			TemporaryAnimatedSprite temporarySpriteByID = getTemporarySpriteByID(777);
			temporarySpriteByID.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			temporarySpriteByID.sourceRectStartingPos = Utility.PointToVector2(temporarySpriteByID.sourceRect.Location);
			temporarySpriteByID.currentNumberOfLoops = 0;
			temporarySpriteByID.totalNumberOfLoops = 1;
			temporarySpriteByID.interval = 1000f;
			temporarySpriteByID.timer = 0f;
			temporarySpriteByID.motion = Vector2.Zero;
			temporarySpriteByID.animationLength = 1;
			temporarySpriteByID.endFunction = gorillaGrabBanana;
		}

		private void gorillaGrabBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = getTemporarySpriteByID(777);
			removeTemporarySpritesWithID(888);
			playSound("slimeHit");
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(96, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.animationLength = 1;
			gorilla.endFunction = gorillaEatBanana;
			temporarySprites.Add(gorilla);
		}

		private void gorillaEatBanana(int extra)
		{
			TemporaryAnimatedSprite gorilla = getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(128, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 5;
			gorilla.interval = 300f;
			gorilla.timer = 0f;
			gorilla.animationLength = 2;
			gorilla.endFunction = gorillaAfterEat;
			playSound("eat");
			DelayedAction.playSoundAfterDelay("eat", 600);
			DelayedAction.playSoundAfterDelay("eat", 1200);
			DelayedAction.playSoundAfterDelay("eat", 1800);
			DelayedAction.playSoundAfterDelay("eat", 2400);
			temporarySprites.Add(gorilla);
		}

		private void gorillaAfterEat(int extra)
		{
			TemporaryAnimatedSprite gorilla = getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = gorillaSpawnNut;
			gorilla.shakeIntensity = 1f;
			gorilla.shakeIntensityChange = -0.01f;
			temporarySprites.Add(gorilla);
		}

		private void gorillaSpawnNut(int extra)
		{
			TemporaryAnimatedSprite gorilla = getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 1;
			gorilla.interval = 1000f;
			gorilla.shakeIntensity = 2f;
			gorilla.shakeIntensityChange = -0.01f;
			playSound("grunt");
			Game1.createItemDebris(new Object(73, 1), new Vector2(16.5f, 25f) * 64f, 0, this, 1280);
			gorilla.timer = 0f;
			gorilla.motion = Vector2.Zero;
			gorilla.animationLength = 1;
			gorilla.endFunction = gorillaReturn;
			temporarySprites.Add(gorilla);
		}

		private void gorillaReturn(int extra)
		{
			TemporaryAnimatedSprite gorilla = getTemporarySpriteByID(777);
			gorilla.sourceRect = new Microsoft.Xna.Framework.Rectangle(32, 352, 32, 32);
			gorilla.sourceRectStartingPos = Utility.PointToVector2(gorilla.sourceRect.Location);
			gorilla.currentNumberOfLoops = 0;
			gorilla.totalNumberOfLoops = 6;
			gorilla.interval = 200f;
			gorilla.timer = 0f;
			gorilla.motion = new Vector2(0f, -3f);
			gorilla.animationLength = 2;
			gorilla.yStopCoordinate = 1280;
			gorilla.reachedStopCoordinate = delegate
			{
				removeTemporarySpritesWithID(777);
			};
			temporarySprites.Add(gorilla);
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.playMorningSong();
			}, 3000);
		}

		public override void SetBuriedNutLocations()
		{
			buriedNutPoints.Add(new Point(23, 47));
			buriedNutPoints.Add(new Point(61, 21));
			base.SetBuriedNutLocations();
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				suspensionBridge.Update(time);
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				suspensionBridge.Draw(b);
			}
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (SuspensionBridge suspensionBridge in suspensionBridges)
			{
				if (suspensionBridge.CheckPlacementPrevention(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}
	}
}
