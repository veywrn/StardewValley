using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class OrderObjective : INetObject<NetFields>
	{
		[XmlIgnore]
		protected SpecialOrder _order;

		[XmlElement("currentCount")]
		public NetIntDelta currentCount = new NetIntDelta();

		[XmlElement("maxCount")]
		public NetInt maxCount = new NetInt(0);

		[XmlElement("description")]
		public NetString description = new NetString();

		[XmlIgnore]
		protected bool _complete;

		[XmlIgnore]
		protected bool _registered;

		[XmlElement("failOnCompletion")]
		public NetBool failOnCompletion = new NetBool(value: false);

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public OrderObjective()
		{
			InitializeNetFields();
		}

		public virtual void OnFail()
		{
		}

		public virtual void InitializeNetFields()
		{
			NetFields.AddFields(currentCount, maxCount, failOnCompletion, description);
			currentCount.fieldChangeVisibleEvent += OnCurrentCountChanged;
		}

		protected void OnCurrentCountChanged(NetIntDelta field, int oldValue, int newValue)
		{
			if (!Utility.ShouldIgnoreValueChangeCallback())
			{
				CheckCompletion();
			}
		}

		public void Register(SpecialOrder new_order)
		{
			_registered = true;
			_order = new_order;
			_Register();
			CheckCompletion(play_sound: false);
		}

		protected virtual void _Register()
		{
		}

		public virtual void Unregister()
		{
			_registered = false;
			_Unregister();
			_order = null;
		}

		protected virtual void _Unregister()
		{
		}

		public virtual bool ShouldShowProgress()
		{
			return true;
		}

		public int GetCount()
		{
			return currentCount;
		}

		public virtual void IncrementCount(int amount)
		{
			int new_value = GetCount() + amount;
			if (new_value < 0)
			{
				new_value = 0;
			}
			if (new_value > GetMaxCount())
			{
				new_value = GetMaxCount();
			}
			SetCount(new_value);
		}

		public virtual void SetCount(int new_count)
		{
			if (new_count > GetMaxCount())
			{
				new_count = GetMaxCount();
			}
			if (new_count != GetCount())
			{
				currentCount.Value = new_count;
			}
		}

		public int GetMaxCount()
		{
			return maxCount;
		}

		public virtual void OnCompletion()
		{
		}

		public virtual void CheckCompletion(bool play_sound = true)
		{
			if (!_registered)
			{
				return;
			}
			bool was_just_completed = false;
			if (GetCount() >= GetMaxCount() && CanComplete())
			{
				if (!_complete)
				{
					was_just_completed = true;
					OnCompletion();
				}
				_complete = true;
			}
			else if (CanUncomplete() && _complete)
			{
				_complete = false;
			}
			if (_order != null)
			{
				_order.CheckCompletion();
				if (was_just_completed && _order.questState.Value != SpecialOrder.QuestState.Complete && play_sound)
				{
					Game1.playSound("jingle1");
				}
			}
		}

		public virtual bool IsComplete()
		{
			return _complete;
		}

		public virtual bool CanUncomplete()
		{
			return false;
		}

		public virtual bool CanComplete()
		{
			return true;
		}

		public virtual string GetDescription()
		{
			GetMaxCount();
			_ = 1;
			return description;
		}

		public virtual void Load(SpecialOrder order, Dictionary<string, string> data)
		{
		}
	}
}
