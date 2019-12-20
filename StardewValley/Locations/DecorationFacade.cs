using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Locations
{
	public class DecorationFacade : SerializationCollectionFacade<int>
	{
		public delegate void ChangeEvent(int whichRoom, int which);

		public readonly NetIntDictionary<int, NetInt> Field = new NetIntDictionary<int, NetInt>();

		private List<Action> pendingChanges = new List<Action>();

		public int this[int whichRoom]
		{
			get
			{
				if (Field.ContainsKey(whichRoom))
				{
					return Field[whichRoom];
				}
				return 0;
			}
			set
			{
				Field[whichRoom] = value;
			}
		}

		public int Count
		{
			get
			{
				if (Field.Count() == 0)
				{
					return 0;
				}
				return Field.Keys.Max() + 1;
			}
		}

		public event ChangeEvent OnChange;

		public DecorationFacade()
		{
			Field.InterpolationWait = false;
			Field.OnValueAdded += delegate(int whichRoom, int which)
			{
				DecorationFacade decorationFacade = this;
				Field.InterpolationWait = false;
				Field.FieldDict[whichRoom].fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
				{
					decorationFacade.changed(whichRoom, newValue);
				};
				changed(whichRoom, which);
			};
		}

		private void changed(int whichRoom, int which)
		{
			pendingChanges.Add(delegate
			{
				if (this.OnChange != null)
				{
					this.OnChange(whichRoom, which);
				}
			});
		}

		protected override List<int> Serialize()
		{
			List<int> result = new List<int>();
			while (result.Count < Count)
			{
				result.Add(0);
			}
			foreach (KeyValuePair<int, int> pair in Field.Pairs)
			{
				result[pair.Key] = pair.Value;
			}
			return result;
		}

		protected override void DeserializeAdd(int serialValue)
		{
			Field[Count] = serialValue;
		}

		public void Set(DecorationFacade other)
		{
			Field.Set(other.Field.Pairs);
		}

		public void SetCountAtLeast(int targetCount)
		{
			while (Count < targetCount)
			{
				this[Count] = 0;
			}
		}

		public void Update()
		{
			foreach (Action pendingChange in pendingChanges)
			{
				pendingChange();
			}
			pendingChanges.Clear();
		}
	}
}
