using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace Netcode
{
	public sealed class NetVector2 : NetField<Vector2, NetVector2>
	{
		public bool AxisAlignedMovement;

		public float ExtrapolationSpeed;

		public float MinDeltaForDirectionChange = 8f;

		public float MaxInterpolationDistance = 320f;

		private bool interpolateXFirst;

		private bool isExtrapolating;

		private bool isFixingExtrapolation;

		public float X
		{
			get
			{
				return base.Value.X;
			}
			set
			{
				Vector2 vector = base.value;
				if (vector.X != value)
				{
					Vector2 newValue = new Vector2(value, vector.Y);
					if (canShortcutSet())
					{
						base.value = newValue;
						return;
					}
					cleanSet(newValue);
					MarkDirty();
				}
			}
		}

		public float Y
		{
			get
			{
				return base.Value.Y;
			}
			set
			{
				Vector2 vector = base.value;
				if (vector.Y != value)
				{
					Vector2 newValue = new Vector2(vector.X, value);
					if (canShortcutSet())
					{
						base.value = newValue;
						return;
					}
					cleanSet(newValue);
					MarkDirty();
				}
			}
		}

		public NetVector2()
		{
		}

		public NetVector2(Vector2 value)
			: base(value)
		{
		}

		public void Set(float x, float y)
		{
			Set(new Vector2(x, y));
		}

		public override void Set(Vector2 newValue)
		{
			if (canShortcutSet())
			{
				value = newValue;
			}
			else if (newValue != value)
			{
				cleanSet(newValue);
				MarkDirty();
			}
		}

		public Vector2 InterpolationDelta()
		{
			if (base.NeedsTick)
			{
				return targetValue - previousValue;
			}
			return Vector2.Zero;
		}

		protected override bool setUpInterpolation(Vector2 oldValue, Vector2 newValue)
		{
			if ((newValue - oldValue).LengthSquared() >= MaxInterpolationDistance * MaxInterpolationDistance)
			{
				return false;
			}
			if (AxisAlignedMovement)
			{
				if (base.NeedsTick)
				{
					Vector2 delta2 = targetValue - previousValue;
					Vector2 absDelta2 = new Vector2(Math.Abs(delta2.X), Math.Abs(delta2.Y));
					if (interpolateXFirst)
					{
						interpolateXFirst = (InterpolationFactor() * (absDelta2.X + absDelta2.Y) < absDelta2.X);
					}
					else
					{
						interpolateXFirst = (InterpolationFactor() * (absDelta2.X + absDelta2.Y) > absDelta2.Y);
					}
				}
				else
				{
					Vector2 delta = newValue - oldValue;
					Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
					interpolateXFirst = (absDelta.X < absDelta.Y);
				}
			}
			return true;
		}

		public Vector2 CurrentInterpolationDirection()
		{
			if (AxisAlignedMovement)
			{
				float factor = InterpolationFactor();
				Vector2 delta2 = InterpolationDelta();
				float traveledLength = (Math.Abs(delta2.X) + Math.Abs(delta2.Y)) * factor;
				if (Math.Abs(delta2.X) < MinDeltaForDirectionChange && Math.Abs(delta2.Y) < MinDeltaForDirectionChange)
				{
					return Vector2.Zero;
				}
				if (Math.Abs(delta2.X) < MinDeltaForDirectionChange)
				{
					return new Vector2(0f, Math.Sign(delta2.Y));
				}
				if (Math.Abs(delta2.Y) < MinDeltaForDirectionChange)
				{
					return new Vector2(Math.Sign(delta2.X), 0f);
				}
				if (interpolateXFirst)
				{
					if (traveledLength > Math.Abs(delta2.X))
					{
						return new Vector2(0f, Math.Sign(delta2.Y));
					}
					return new Vector2(Math.Sign(delta2.X), 0f);
				}
				if (traveledLength > Math.Abs(delta2.Y))
				{
					return new Vector2(Math.Sign(delta2.X), 0f);
				}
				return new Vector2(0f, Math.Sign(delta2.Y));
			}
			Vector2 delta = InterpolationDelta();
			delta.Normalize();
			return delta;
		}

		public float CurrentInterpolationSpeed()
		{
			float distance = InterpolationDelta().Length();
			if (InterpolationTicks() == 0)
			{
				return distance;
			}
			if (InterpolationFactor() > 1f)
			{
				return ExtrapolationSpeed;
			}
			return distance / (float)InterpolationTicks();
		}

		protected override Vector2 interpolate(Vector2 startValue, Vector2 endValue, float factor)
		{
			if (AxisAlignedMovement && factor <= 1f && !isFixingExtrapolation)
			{
				isExtrapolating = false;
				Vector2 delta = InterpolationDelta();
				Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
				float traveledLength = (absDelta.X + absDelta.Y) * factor;
				float x2 = startValue.X;
				float y2 = startValue.Y;
				if (interpolateXFirst)
				{
					if (traveledLength > absDelta.X)
					{
						x2 = endValue.X;
						y2 = startValue.Y + (traveledLength - absDelta.X) * (float)Math.Sign(delta.Y);
					}
					else
					{
						x2 = startValue.X + traveledLength * (float)Math.Sign(delta.X);
						y2 = startValue.Y;
					}
				}
				else if (traveledLength > absDelta.Y)
				{
					y2 = endValue.Y;
					x2 = startValue.X + (traveledLength - absDelta.Y) * (float)Math.Sign(delta.X);
				}
				else
				{
					y2 = startValue.Y + traveledLength * (float)Math.Sign(delta.Y);
					x2 = startValue.X;
				}
				return new Vector2(x2, y2);
			}
			if (factor > 1f)
			{
				isExtrapolating = true;
				uint extrapolationTicks = (uint)((int)(base.Root.Clock.GetLocalTick() - interpolationStartTick) - InterpolationTicks());
				Vector2 direction = endValue - startValue;
				if (direction.LengthSquared() > ExtrapolationSpeed * ExtrapolationSpeed)
				{
					direction.Normalize();
					return endValue + direction * (float)(double)extrapolationTicks * ExtrapolationSpeed;
				}
			}
			isExtrapolating = false;
			return startValue + (endValue - startValue) * factor;
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			float newX = reader.ReadSingle();
			float newY = reader.ReadSingle();
			if (version.IsPriorityOver(ChangeVersion))
			{
				isFixingExtrapolation = isExtrapolating;
				setInterpolationTarget(new Vector2(newX, newY));
				isExtrapolating = false;
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Write(base.Value.X);
			writer.Write(base.Value.Y);
		}
	}
}
