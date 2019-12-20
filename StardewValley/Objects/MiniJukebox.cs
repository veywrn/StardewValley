using Microsoft.Xna.Framework;
using StardewValley.Locations;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Objects
{
	public class MiniJukebox : Object
	{
		private bool showNote;

		protected override void initNetFields()
		{
			base.initNetFields();
		}

		public MiniJukebox()
		{
		}

		public MiniJukebox(Vector2 position)
			: base(position, 209)
		{
			Name = "Mini-Jukebox";
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
			if (!who.currentLocation.IsFarm && !who.currentLocation.IsGreenhouse && !(who.currentLocation is Cellar))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_NotFarmPlay"));
			}
			else if (who.currentLocation.IsOutdoors && Game1.isRaining)
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Mini_JukeBox_OutdoorRainy"));
			}
			else
			{
				List<string> list = Game1.player.songsHeard.Distinct().ToList();
				list.Insert(0, "turn_off");
				Game1.activeClickableMenu = new ChooseFromListMenu(list, OnSongChosen, isJukebox: true, who.currentLocation.miniJukeboxTrack.Value);
			}
			return true;
		}

		public void RegisterToLocation(GameLocation location)
		{
			location?.OnMiniJukeboxAdded();
		}

		public override void performRemoveAction(Vector2 tileLocation, GameLocation environment)
		{
			environment?.OnMiniJukeboxRemoved();
			base.performRemoveAction(tileLocation, environment);
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (environment != null && environment.IsMiniJukeboxPlaying())
			{
				showNextIndex.Value = true;
				if (showNote)
				{
					showNote = false;
					for (int i = 0; i < 4; i++)
					{
						environment.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(516, 1916, 7, 10), 9999f, 1, 1, tileLocation.Value * 64f + new Vector2(Game1.random.Next(48), -80f), flicker: false, flipped: false, (tileLocation.Value.Y + 1f) * 64f / 10000f, 0.01f, Color.White, 4f, 0f, 0f, 0f)
						{
							xPeriodic = true,
							xPeriodicLoopTime = 1200f,
							xPeriodicRange = 8f,
							motion = new Vector2((float)Game1.random.Next(-10, 10) / 100f, -1f),
							delayBeforeAnimationStart = 1200 + 300 * i
						});
					}
				}
			}
			else
			{
				showNextIndex.Value = false;
			}
			base.updateWhenCurrentLocation(time, environment);
		}

		public void OnSongChosen(string selection)
		{
			if (Game1.player.currentLocation == null)
			{
				return;
			}
			if (selection == "turn_off")
			{
				Game1.player.currentLocation.miniJukeboxTrack.Value = "";
				return;
			}
			if (selection != (string)Game1.player.currentLocation.miniJukeboxTrack)
			{
				showNote = true;
				shakeTimer = 1000;
			}
			Game1.player.currentLocation.miniJukeboxTrack.Value = selection;
		}
	}
}
