using System;
using System.IO;
using System.Xml.Serialization;

namespace Netcode
{
	public abstract class NetRefBase<T, TSelf> : NetField<T, TSelf> where T : class where TSelf : NetRefBase<T, TSelf>
	{
		private enum RefDeltaType : byte
		{
			ChildDelta,
			Reassigned
		}

		public delegate void ConflictResolveEvent(T rejected, T accepted);

		public XmlSerializer Serializer;

		private RefDeltaType deltaType;

		protected NetVersion reassigned;

		public event ConflictResolveEvent OnConflictResolve;

		public NetRefBase()
		{
		}

		public NetRefBase(T value)
			: this()
		{
			cleanSet(value);
		}

		protected override void SetParent(INetSerializable parent)
		{
			if (parent == null || parent.Root != base.Root)
			{
				reassigned.Clear();
			}
			base.SetParent(parent);
		}

		protected override void CleanImpl()
		{
			base.CleanImpl();
			deltaType = RefDeltaType.ChildDelta;
		}

		public void MarkReassigned()
		{
			deltaType = RefDeltaType.Reassigned;
			if (base.Root != null)
			{
				reassigned.Set(base.Root.Clock.netVersion);
			}
			MarkDirty();
		}

		public override void Set(T newValue)
		{
			if (newValue != base.Value)
			{
				deltaType = RefDeltaType.Reassigned;
				if (base.Root != null)
				{
					reassigned.Set(base.Root.Clock.netVersion);
				}
				cleanSet(newValue);
				MarkDirty();
			}
		}

		private T createType(Type type)
		{
			if (type == null)
			{
				return null;
			}
			return (T)Activator.CreateInstance(type);
		}

		protected T ReadType(BinaryReader reader)
		{
			return createType(reader.ReadType());
		}

		protected void WriteType(BinaryWriter writer)
		{
			writer.WriteTypeOf(targetValue);
		}

		private void serialize(BinaryWriter writer, XmlSerializer serializer = null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				(serializer ?? Serializer).Serialize(stream, targetValue);
				stream.Seek(0L, SeekOrigin.Begin);
				writer.Write((int)stream.Length);
				writer.Write(stream.ToArray());
			}
		}

		private T deserialize(BinaryReader reader, XmlSerializer serializer = null)
		{
			int length = reader.ReadInt32();
			using (MemoryStream stream = new MemoryStream(reader.ReadBytes(length)))
			{
				return (T)(serializer ?? Serializer).Deserialize(stream);
			}
		}

		protected abstract void ReadValueFull(T value, BinaryReader reader, NetVersion version);

		protected abstract void ReadValueDelta(BinaryReader reader, NetVersion version);

		protected abstract void WriteValueFull(BinaryWriter writer);

		protected abstract void WriteValueDelta(BinaryWriter writer);

		private void writeBaseValue(BinaryWriter writer)
		{
			if (Serializer != null)
			{
				serialize(writer);
			}
			else
			{
				WriteType(writer);
			}
		}

		private T readBaseValue(BinaryReader reader, NetVersion version)
		{
			if (Serializer != null)
			{
				return deserialize(reader);
			}
			return ReadType(reader);
		}

		protected override void ReadDelta(BinaryReader reader, NetVersion version)
		{
			if (reader.ReadByte() == 1)
			{
				reader.ReadSkippable(delegate
				{
					NetVersion other = default(NetVersion);
					other.Read(reader);
					T val = readBaseValue(reader, version);
					if (val != null)
					{
						ReadValueFull(val, reader, version);
					}
					if (other.IsIndependent(reassigned))
					{
						if (!other.IsPriorityOver(reassigned))
						{
							if (this.OnConflictResolve != null)
							{
								this.OnConflictResolve(val, targetValue);
							}
							return;
						}
						if (this.OnConflictResolve != null)
						{
							this.OnConflictResolve(targetValue, val);
						}
					}
					else if (!other.IsPriorityOver(reassigned))
					{
						return;
					}
					reassigned.Set(other);
					setInterpolationTarget(val);
				});
			}
			else
			{
				reader.ReadSkippable(delegate
				{
					if (version.IsPrecededBy(reassigned) && targetValue != null)
					{
						ReadValueDelta(reader, version);
					}
				});
			}
		}

		protected override void WriteDelta(BinaryWriter writer)
		{
			writer.Push((targetValue != null) ? targetValue.GetType().Name : "null");
			writer.Write((byte)deltaType);
			if (deltaType == RefDeltaType.Reassigned)
			{
				writer.WriteSkippable(delegate
				{
					reassigned.Write(writer);
					writeBaseValue(writer);
					if (targetValue != null)
					{
						WriteValueFull(writer);
					}
				});
			}
			else
			{
				writer.WriteSkippable(delegate
				{
					if (targetValue != null)
					{
						WriteValueDelta(writer);
					}
				});
			}
			deltaType = RefDeltaType.ChildDelta;
			writer.Pop();
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			reassigned.Read(reader);
			T remoteValue = readBaseValue(reader, version);
			if (remoteValue != null)
			{
				ReadValueFull(remoteValue, reader, version);
			}
			cleanSet(remoteValue);
			ChangeVersion.Merge(version);
		}

		public override void WriteFull(BinaryWriter writer)
		{
			writer.Push((targetValue != null) ? targetValue.GetType().Name : "null");
			reassigned.Write(writer);
			writeBaseValue(writer);
			if (targetValue != null)
			{
				WriteValueFull(writer);
			}
			writer.Pop();
		}
	}
}
