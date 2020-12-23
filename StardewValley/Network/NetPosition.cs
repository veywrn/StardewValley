using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network
{
	public sealed class NetPosition : NetPausableField<Vector2, NetVector2, NetVector2>
	{
		private const float SmoothingFudge = 0.8f;

		private const ushort DefaultDeltaAggregateTicks = 0;

		public bool ExtrapolationEnabled;

		public readonly NetBool moving = new NetBool().Interpolated(interpolate: false, wait: false);

		public float X
		{
			get
			{
				return Get().X;
			}
			set
			{
				Set(new Vector2(value, Y));
			}
		}

		public float Y
		{
			get
			{
				return Get().Y;
			}
			set
			{
				Set(new Vector2(X, value));
			}
		}

		public NetPosition()
			: base(new NetVector2().Interpolated(interpolate: true, wait: true))
		{
		}

		public NetPosition(NetVector2 field)
			: base(field)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(moving);
			base.NetFields.DeltaAggregateTicks = 0;
			Field.fieldChangeEvent += delegate
			{
				if (IsMaster())
				{
					moving.Value = true;
				}
			};
			moving.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
			{
				if (!IsMaster())
				{
					Field.ExtrapolationEnabled = (newValue && ExtrapolationEnabled);
				}
			};
		}

		protected bool IsMaster()
		{
			if (base.NetFields.Root != null)
			{
				return base.NetFields.Root.Clock.LocalId == 0;
			}
			return false;
		}

		public override Vector2 Get()
		{
			if (Game1.HostPaused)
			{
				Field.CancelInterpolation();
			}
			return base.Get();
		}

		public Vector2 CurrentInterpolationDirection()
		{
			if (base.Paused)
			{
				return Vector2.Zero;
			}
			return Field.CurrentInterpolationDirection();
		}

		public float CurrentInterpolationSpeed()
		{
			if (base.Paused)
			{
				return 0f;
			}
			return Field.CurrentInterpolationSpeed();
		}

		public void UpdateExtrapolation(float extrapolationSpeed)
		{
			base.NetFields.DeltaAggregateTicks = (ushort)((base.NetFields.Root != null) ? ((ushort)((float)base.NetFields.Root.Clock.InterpolationTicks * 0.8f)) : 0);
			ExtrapolationEnabled = true;
			Field.ExtrapolationSpeed = extrapolationSpeed;
			if (IsMaster())
			{
				moving.Value = false;
			}
		}
	}
}
