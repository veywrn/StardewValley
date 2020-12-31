using System;
using System.IO;

namespace Netcode
{
	public abstract class NetFieldBase<T, TSelf> : AbstractNetSerializable, IEquatable<TSelf>, InterpolationCancellable where TSelf : NetFieldBase<T, TSelf>
	{
		public delegate void FieldChange(TSelf field, T oldValue, T newValue);

		[Flags]
		protected enum NetFieldBaseBool : byte
		{
			None = 0x0,
			InterpolationEnabled = 0x1,
			ExtrapolationEnabled = 0x2,
			InterpolationWait = 0x4,
			notifyOnTargetValueChange = 0x8
		}

		protected NetFieldBaseBool _bools;

		protected uint interpolationStartTick;

		protected T value;

		protected T previousValue;

		protected T targetValue;

		public bool InterpolationEnabled
		{
			get
			{
				return (_bools & NetFieldBaseBool.InterpolationEnabled) != 0;
			}
			set
			{
				if (value)
				{
					_bools |= NetFieldBaseBool.InterpolationEnabled;
				}
				else
				{
					_bools &= ~NetFieldBaseBool.InterpolationEnabled;
				}
			}
		}

		public bool ExtrapolationEnabled
		{
			get
			{
				return (_bools & NetFieldBaseBool.ExtrapolationEnabled) != 0;
			}
			set
			{
				if (value)
				{
					_bools |= NetFieldBaseBool.ExtrapolationEnabled;
				}
				else
				{
					_bools &= ~NetFieldBaseBool.ExtrapolationEnabled;
				}
			}
		}

		public bool InterpolationWait
		{
			get
			{
				return (_bools & NetFieldBaseBool.InterpolationWait) != 0;
			}
			set
			{
				if (value)
				{
					_bools |= NetFieldBaseBool.InterpolationWait;
				}
				else
				{
					_bools &= ~NetFieldBaseBool.InterpolationWait;
				}
			}
		}

		protected bool notifyOnTargetValueChange
		{
			get
			{
				return (_bools & NetFieldBaseBool.notifyOnTargetValueChange) != 0;
			}
			set
			{
				if (value)
				{
					_bools |= NetFieldBaseBool.notifyOnTargetValueChange;
				}
				else
				{
					_bools &= ~NetFieldBaseBool.notifyOnTargetValueChange;
				}
			}
		}

		public T TargetValue => targetValue;

		public T Value
		{
			get
			{
				return value;
			}
			set
			{
				Set(value);
			}
		}

		public event FieldChange fieldChangeEvent;

		public event FieldChange fieldChangeVisibleEvent;

		public NetFieldBase()
		{
			InterpolationWait = true;
			value = default(T);
			previousValue = default(T);
			targetValue = default(T);
		}

		public NetFieldBase(T value)
			: this()
		{
			cleanSet(value);
		}

		public TSelf Interpolated(bool interpolate, bool wait)
		{
			InterpolationEnabled = interpolate;
			InterpolationWait = wait;
			return (TSelf)this;
		}

		protected virtual int InterpolationTicks()
		{
			if (base.Root == null)
			{
				return 0;
			}
			return base.Root.Clock.InterpolationTicks;
		}

		protected float InterpolationFactor()
		{
			return (float)(double)(base.Root.Clock.GetLocalTick() - interpolationStartTick) / (float)InterpolationTicks();
		}

		public bool IsInterpolating()
		{
			if (InterpolationEnabled)
			{
				return base.NeedsTick;
			}
			return false;
		}

		public bool IsChanging()
		{
			return base.NeedsTick;
		}

		protected override bool tickImpl()
		{
			if (base.Root != null && InterpolationTicks() > 0)
			{
				float factor = InterpolationFactor();
				bool shouldExtrapolate = ExtrapolationEnabled && ChangeVersion[0] == base.Root.Clock.netVersion[0];
				if ((factor < 1f && InterpolationEnabled) || (shouldExtrapolate && factor < 3f))
				{
					value = interpolate(previousValue, targetValue, factor);
					return true;
				}
				if (factor < 1f && InterpolationWait)
				{
					value = previousValue;
					return true;
				}
			}
			T oldValue = previousValue;
			CancelInterpolation();
			if (this.fieldChangeVisibleEvent != null)
			{
				this.fieldChangeVisibleEvent((TSelf)this, oldValue, value);
			}
			return false;
		}

		public void CancelInterpolation()
		{
			if (base.NeedsTick)
			{
				value = targetValue;
				previousValue = default(T);
				base.NeedsTick = false;
			}
		}

		public T Get()
		{
			return value;
		}

		protected virtual T interpolate(T startValue, T endValue, float factor)
		{
			return startValue;
		}

		public abstract void Set(T newValue);

		protected bool canShortcutSet()
		{
			if (Dirty && this.fieldChangeEvent == null)
			{
				return this.fieldChangeVisibleEvent == null;
			}
			return false;
		}

		protected virtual void targetValueChanged(T oldValue, T newValue)
		{
		}

		protected void cleanSet(T newValue)
		{
			T oldValue = value;
			T oldTargetValue = targetValue;
			targetValue = newValue;
			value = newValue;
			previousValue = default(T);
			base.NeedsTick = false;
			if (notifyOnTargetValueChange)
			{
				targetValueChanged(oldTargetValue, newValue);
			}
			if (this.fieldChangeEvent != null)
			{
				this.fieldChangeEvent((TSelf)this, oldValue, newValue);
			}
			if (this.fieldChangeVisibleEvent != null)
			{
				this.fieldChangeVisibleEvent((TSelf)this, oldValue, newValue);
			}
		}

		protected virtual bool setUpInterpolation(T oldValue, T newValue)
		{
			return true;
		}

		protected void setInterpolationTarget(T newValue)
		{
			T oldValue = value;
			if (!InterpolationWait || base.Root == null || !setUpInterpolation(oldValue, newValue))
			{
				cleanSet(newValue);
				return;
			}
			T oldTargetValue = targetValue;
			previousValue = oldValue;
			base.NeedsTick = true;
			targetValue = newValue;
			interpolationStartTick = base.Root.Clock.GetLocalTick();
			if (notifyOnTargetValueChange)
			{
				targetValueChanged(oldTargetValue, newValue);
			}
			if (this.fieldChangeEvent != null)
			{
				this.fieldChangeEvent((TSelf)this, oldValue, newValue);
			}
		}

		protected abstract void ReadDelta(BinaryReader reader, NetVersion version);

		protected abstract void WriteDelta(BinaryWriter writer);

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			ReadDelta(reader, version);
			CancelInterpolation();
			ChangeVersion.Merge(version);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			WriteDelta(writer);
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			ReadDelta(reader, version);
			ChangeVersion.Merge(version);
		}

		public override void Write(BinaryWriter writer)
		{
			WriteDelta(writer);
		}

		public static implicit operator T(NetFieldBase<T, TSelf> netField)
		{
			if (netField == null)
			{
				return default(T);
			}
			return netField.value;
		}

		public override string ToString()
		{
			if (value != null)
			{
				return value.ToString();
			}
			return "null";
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TSelf) || !Equals(obj as TSelf))
			{
				return object.Equals(Value, obj);
			}
			return true;
		}

		public bool Equals(TSelf other)
		{
			return object.Equals(Value, other.Value);
		}

		public static bool operator ==(NetFieldBase<T, TSelf> self, TSelf other)
		{
			if ((object)self != other)
			{
				return object.Equals(self, other);
			}
			return true;
		}

		public static bool operator !=(NetFieldBase<T, TSelf> self, TSelf other)
		{
			if ((object)self != other)
			{
				return !object.Equals(self, other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((value != null) ? value.GetHashCode() : 0) ^ -858436897;
		}
	}
}
