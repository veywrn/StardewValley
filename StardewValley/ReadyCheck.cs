using Netcode;
using StardewValley.Menus;
using StardewValley.Network;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	internal class ReadyCheck : INetObject<NetFields>
	{
		private readonly NetString name = new NetString();

		private readonly NetFarmerCollection readyPlayers = new NetFarmerCollection();

		private readonly NetFarmerCollection setPlayers = new NetFarmerCollection();

		private readonly NetFarmerCollection requiredPlayers = new NetFarmerCollection();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public string Name => name;

		public ReadyCheck()
		{
			NetFields.AddFields(name, readyPlayers.NetFields, setPlayers.NetFields);
		}

		public ReadyCheck(string name)
			: this()
		{
			this.name.Value = name;
		}

		public void SetRequiredFarmers(IEnumerable<Farmer> required_farmers)
		{
			requiredPlayers.Clear();
			if (required_farmers != null)
			{
				foreach (Farmer farmer in required_farmers)
				{
					requiredPlayers.Add(farmer);
				}
			}
		}

		private IEnumerable<Farmer> GetRequiredPlayers()
		{
			if (requiredPlayers.Count == 0)
			{
				if (setPlayers.Contains(Game1.player))
				{
					return readyPlayers.Intersect(Game1.getOnlineFarmers());
				}
				return Game1.getOnlineFarmers();
			}
			return requiredPlayers;
		}

		private bool containsAllPlayers(NetFarmerCollection farmerSet)
		{
			foreach (Farmer farmer in GetRequiredPlayers())
			{
				if (!farmerSet.Contains(farmer) && !Game1.multiplayer.isDisconnecting(farmer))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsOtherFarmerReady(Farmer farmer)
		{
			return setPlayers.Contains(farmer);
		}

		public bool IsCancelable()
		{
			return !setPlayers.Contains(Game1.player);
		}

		public bool IsReady()
		{
			return containsAllPlayers(setPlayers);
		}

		public int GetNumberReady()
		{
			return readyPlayers.Count;
		}

		public int GetNumberRequired()
		{
			int i = 0;
			foreach (Farmer requiredPlayer in GetRequiredPlayers())
			{
				_ = requiredPlayer;
				i++;
			}
			return i;
		}

		public void SetLocalReady(bool ready)
		{
			if (ready && !readyPlayers.Contains(Game1.player))
			{
				readyPlayers.Add(Game1.player);
			}
			else if (!ready && readyPlayers.Contains(Game1.player))
			{
				readyPlayers.Remove(Game1.player);
				setPlayers.Remove(Game1.player);
			}
		}

		public void Update()
		{
			if (readyPlayers.Contains(Game1.player) && !setPlayers.Contains(Game1.player) && !(Game1.activeClickableMenu is SaveGameMenu) && !(Game1.activeClickableMenu is ShippingMenu))
			{
				ReadyCheckDialog dialog = Game1.activeClickableMenu as ReadyCheckDialog;
				if (dialog == null || dialog.checkName != Name)
				{
					SetLocalReady(ready: false);
				}
			}
			if (requiredPlayers.RetainOnlinePlayers())
			{
				setPlayers.Remove(Game1.player);
			}
			if (readyPlayers.RetainOnlinePlayers())
			{
				setPlayers.Remove(Game1.player);
			}
			if (containsAllPlayers(readyPlayers))
			{
				if (!setPlayers.Contains(Game1.player))
				{
					setPlayers.Add(Game1.player);
				}
			}
			else if (setPlayers.Contains(Game1.player))
			{
				setPlayers.Remove(Game1.player);
			}
		}
	}
}
