using Netcode;
using StardewValley.Network;
using System.Threading;

namespace StardewValley
{
	public class NewDaySynchronizer : NetSynchronizer
	{
		public void start()
		{
			Game1.multiplayer.UpdateEarly();
			if (Game1.IsServer)
			{
				sendVar<NetBool, bool>("started", value: true);
			}
		}

		public bool readyForFinish()
		{
			Game1.player.team.SetLocalReady("wakeup", ready: true);
			Game1.player.team.Update();
			Game1.multiplayer.UpdateLate();
			Game1.multiplayer.UpdateEarly();
			return Game1.player.team.IsReady("wakeup");
		}

		public int numReadyForFinish()
		{
			return Game1.player.team.GetNumberReady("wakeup");
		}

		public bool readyForSave()
		{
			Game1.player.team.SetLocalReady("ready_for_save", ready: true);
			Game1.player.team.Update();
			Game1.multiplayer.UpdateLate();
			Game1.multiplayer.UpdateEarly();
			return Game1.player.team.IsReady("ready_for_save");
		}

		public int numReadyForSave()
		{
			return Game1.player.team.GetNumberReady("ready_for_save");
		}

		public void finish()
		{
			if (Game1.IsServer)
			{
				sendVar<NetBool, bool>("finished", value: true);
			}
			Game1.multiplayer.UpdateLate();
		}

		public bool hasFinished()
		{
			return hasVar("finished");
		}

		public void flagSaved()
		{
			if (Game1.IsServer)
			{
				sendVar<NetBool, bool>("saved", value: true);
			}
			Game1.multiplayer.UpdateLate();
		}

		public bool hasSaved()
		{
			return hasVar("saved");
		}

		public void waitForFinish()
		{
			if (Game1.IsClient)
			{
				waitForVar<NetBool, bool>("finished");
			}
		}

		public override void processMessages()
		{
			Game1.multiplayer.UpdateLate();
			Thread.Sleep(16);
			Program.sdk.Update();
			Game1.multiplayer.UpdateEarly();
		}

		protected override void sendMessage(params object[] data)
		{
			OutgoingMessage msg = new OutgoingMessage(14, Game1.player, data);
			if (Game1.IsServer)
			{
				foreach (Farmer f in Game1.otherFarmers.Values)
				{
					Game1.server.sendMessage(f.UniqueMultiplayerID, msg);
				}
			}
			else if (Game1.IsClient)
			{
				Game1.client.sendMessage(msg);
			}
		}
	}
}
