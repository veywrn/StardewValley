using Netcode;

namespace StardewValley.Network
{
	public class NetLocationRef : INetObject<NetFields>
	{
		private readonly NetString locationName = new NetString();

		private readonly NetBool isStructure = new NetBool();

		public GameLocation Value
		{
			get
			{
				return Get();
			}
			set
			{
				Set(value);
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public NetLocationRef()
		{
			NetFields.AddFields(locationName, isStructure);
		}

		public NetLocationRef(GameLocation value)
			: this()
		{
			Set(value);
		}

		public bool IsChanging()
		{
			if (!locationName.IsChanging())
			{
				return isStructure.IsChanging();
			}
			return true;
		}

		public void Update()
		{
		}

		public GameLocation Get()
		{
			return Game1.getLocationFromName(locationName, isStructure);
		}

		public void Set(GameLocation location)
		{
			if (location == null)
			{
				isStructure.Value = false;
				locationName.Value = "";
			}
			else
			{
				isStructure.Value = location.isStructure;
				locationName.Value = (location.isStructure ? ((string)location.uniqueName) : location.Name);
			}
		}

		public static implicit operator GameLocation(NetLocationRef locationRef)
		{
			return locationRef.Value;
		}
	}
}
