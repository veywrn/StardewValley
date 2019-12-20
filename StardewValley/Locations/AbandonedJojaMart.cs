using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Locations
{
	public class AbandonedJojaMart : GameLocation
	{
		[XmlIgnore]
		private readonly NetEvent0 restoreAreaCutsceneEvent = new NetEvent0();

		[XmlIgnore]
		public NetMutex bundleMutex = new NetMutex();

		public AbandonedJojaMart()
		{
		}

		public AbandonedJojaMart(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(restoreAreaCutsceneEvent);
			restoreAreaCutsceneEvent.onEvent += doRestoreAreaCutscene;
			base.NetFields.AddField(bundleMutex.NetFields);
		}

		public void checkBundle()
		{
			bundleMutex.RequestLock(delegate
			{
				Dictionary<int, bool[]> bundlesComplete = (Game1.getLocationFromName("CommunityCenter") as CommunityCenter).bundlesDict();
				Game1.activeClickableMenu = new JunimoNoteMenu(6, bundlesComplete);
			});
		}

		public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
		{
			base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
			bundleMutex.Update(this);
			if (bundleMutex.IsLockHeld() && Game1.activeClickableMenu == null)
			{
				bundleMutex.ReleaseLock();
			}
			restoreAreaCutsceneEvent.Poll();
		}

		public void restoreAreaCutscene()
		{
			restoreAreaCutsceneEvent.Fire();
		}

		private void doRestoreAreaCutscene()
		{
			if (Game1.currentLocation == this)
			{
				Game1.player.freezePause = 1000;
				DelayedAction.removeTileAfterDelay(8, 8, 100, Game1.currentLocation, "Buildings");
				Game1.getLocationFromName("AbandonedJojaMart").startEvent(new Event(Game1.content.Load<Dictionary<string, string>>("Data\\Events\\AbandonedJojaMart")["missingBundleComplete"], 192393));
			}
			Game1.addMailForTomorrow("ccMovieTheater", noLetter: true, sendToEveryone: true);
			if (Game1.player.team.theaterBuildDate.Value < 0)
			{
				Game1.player.team.theaterBuildDate.Set(Game1.Date.TotalDays + 1);
			}
		}
	}
}
