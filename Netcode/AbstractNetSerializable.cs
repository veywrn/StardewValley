using System;
using System.IO;

namespace Netcode
{
	public abstract class AbstractNetSerializable : INetSerializable, INetObject<INetSerializable>
	{
		private uint dirtyTick = uint.MaxValue;

		private uint minNextDirtyTime;

		protected NetVersion ChangeVersion;

		public ushort DeltaAggregateTicks;

		private bool needsTick;

		private bool childNeedsTick;

		private INetSerializable parent;

		public uint DirtyTick
		{
			get
			{
				return dirtyTick;
			}
			set
			{
				if (value < dirtyTick)
				{
					SetDirtySooner(value);
				}
				else if (value > dirtyTick)
				{
					SetDirtyLater(value);
				}
			}
		}

		public virtual bool Dirty => dirtyTick != uint.MaxValue;

		public bool NeedsTick
		{
			get
			{
				return needsTick;
			}
			set
			{
				if (value != needsTick)
				{
					needsTick = value;
					if (value && Parent != null)
					{
						Parent.ChildNeedsTick = true;
					}
				}
			}
		}

		public bool ChildNeedsTick
		{
			get
			{
				return childNeedsTick;
			}
			set
			{
				if (value != childNeedsTick)
				{
					childNeedsTick = value;
					if (value && Parent != null)
					{
						Parent.ChildNeedsTick = true;
					}
				}
			}
		}

		public INetRoot Root
		{
			get;
			protected set;
		}

		public INetSerializable Parent
		{
			get
			{
				return parent;
			}
			set
			{
				SetParent(value);
			}
		}

		public INetSerializable NetFields => this;

		protected void SetDirtySooner(uint tick)
		{
			tick = Math.Max(tick, minNextDirtyTime);
			if (dirtyTick > tick)
			{
				dirtyTick = tick;
				if (Parent != null)
				{
					Parent.DirtyTick = Math.Min(Parent.DirtyTick, tick);
				}
				if (Root != null)
				{
					minNextDirtyTime = Root.Clock.GetLocalTick() + DeltaAggregateTicks;
					ChangeVersion.Set(Root.Clock.netVersion);
				}
				else
				{
					minNextDirtyTime = 0u;
					ChangeVersion.Clear();
				}
			}
		}

		protected void SetDirtyLater(uint tick)
		{
			if (dirtyTick < tick)
			{
				dirtyTick = tick;
				ForEachChild(delegate(INetSerializable child)
				{
					child.DirtyTick = Math.Max(child.DirtyTick, tick);
				});
				if (tick == uint.MaxValue)
				{
					CleanImpl();
				}
			}
		}

		protected virtual void CleanImpl()
		{
			if (Root == null)
			{
				minNextDirtyTime = 0u;
			}
			else
			{
				minNextDirtyTime = Root.Clock.GetLocalTick() + DeltaAggregateTicks;
			}
		}

		public void MarkDirty()
		{
			if (Root == null)
			{
				SetDirtySooner(0u);
			}
			else
			{
				SetDirtySooner(Root.Clock.GetLocalTick());
			}
		}

		public void MarkClean()
		{
			SetDirtyLater(uint.MaxValue);
		}

		protected virtual bool tickImpl()
		{
			return false;
		}

		public bool Tick()
		{
			if (needsTick)
			{
				needsTick = tickImpl();
			}
			if (childNeedsTick)
			{
				childNeedsTick = false;
				ForEachChild(delegate(INetSerializable child)
				{
					if (child.NeedsTick || child.ChildNeedsTick)
					{
						childNeedsTick |= child.Tick();
					}
				});
			}
			return childNeedsTick | needsTick;
		}

		public abstract void Read(BinaryReader reader, NetVersion version);

		public abstract void Write(BinaryWriter writer);

		public abstract void ReadFull(BinaryReader reader, NetVersion version);

		public abstract void WriteFull(BinaryWriter writer);

		protected uint GetLocalTick()
		{
			if (Root != null)
			{
				return Root.Clock.GetLocalTick();
			}
			return 0u;
		}

		protected NetTimestamp GetLocalTimestamp()
		{
			if (Root != null)
			{
				return Root.Clock.GetLocalTimestamp();
			}
			return default(NetTimestamp);
		}

		protected NetVersion GetLocalVersion()
		{
			if (Root != null)
			{
				return new NetVersion(Root.Clock.netVersion);
			}
			return default(NetVersion);
		}

		protected virtual void SetParent(INetSerializable parent)
		{
			this.parent = parent;
			if (parent != null)
			{
				Root = parent.Root;
				SetChildParents();
			}
			else
			{
				ClearChildParents();
			}
			MarkClean();
			ChangeVersion.Clear();
			minNextDirtyTime = 0u;
		}

		protected virtual void SetChildParents()
		{
			ForEachChild(delegate(INetSerializable child)
			{
				child.Parent = this;
			});
		}

		protected virtual void ClearChildParents()
		{
			ForEachChild(delegate(INetSerializable child)
			{
				if (child.Parent == this)
				{
					child.Parent = null;
				}
			});
		}

		protected virtual void ValidateChild(INetSerializable child)
		{
			if ((Parent != null || Root == this) && child.Parent != this)
			{
				throw new InvalidOperationException();
			}
		}

		protected virtual void ValidateChildren()
		{
			if (Parent != null || Root == this)
			{
				ForEachChild(ValidateChild);
			}
		}

		protected virtual void ForEachChild(Action<INetSerializable> childAction)
		{
		}
	}
}
