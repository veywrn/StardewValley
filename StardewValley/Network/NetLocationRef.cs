using Netcode;

namespace StardewValley.Network
{
	public class NetLocationRef : INetObject<NetFields>
	{
		private readonly NetString locationName = new NetString();

		private readonly NetBool isStructure = new NetBool();

		protected GameLocation _gameLocation;

		protected bool _dirty = true;

		protected bool _usedLocalLocation;

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
			locationName.fieldChangeVisibleEvent += OnLocationNameChanged;
			isStructure.fieldChangeVisibleEvent += OnStructureValueChanged;
		}

		public virtual void OnLocationNameChanged(NetString field, string old_value, string new_value)
		{
			_dirty = true;
		}

		public virtual void OnStructureValueChanged(NetBool field, bool old_value, bool new_value)
		{
			_dirty = true;
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
			ApplyChangesIfDirty();
		}

		public void ApplyChangesIfDirty()
		{
			if (_usedLocalLocation && _gameLocation != Game1.currentLocation)
			{
				_dirty = true;
				_usedLocalLocation = false;
			}
			if (_dirty)
			{
				_gameLocation = Game1.getLocationFromName(locationName, isStructure);
				_dirty = false;
			}
			if (!_usedLocalLocation && _gameLocation != Game1.currentLocation && IsCurrentlyViewedLocation())
			{
				_usedLocalLocation = true;
				_gameLocation = Game1.currentLocation;
			}
		}

		public GameLocation Get()
		{
			ApplyChangesIfDirty();
			return _gameLocation;
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
			if (IsCurrentlyViewedLocation())
			{
				_usedLocalLocation = true;
				_gameLocation = Game1.currentLocation;
			}
			else
			{
				_gameLocation = location;
			}
			if (_gameLocation != null && _gameLocation.isTemp())
			{
				_gameLocation = null;
			}
			_dirty = false;
		}

		public bool IsCurrentlyViewedLocation()
		{
			if (Game1.currentLocation != null && locationName.Value == Game1.currentLocation.NameOrUniqueName)
			{
				return true;
			}
			return false;
		}

		public static implicit operator GameLocation(NetLocationRef locationRef)
		{
			return locationRef.Value;
		}
	}
}
