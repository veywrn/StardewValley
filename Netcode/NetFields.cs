using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public class NetFields : AbstractNetSerializable
	{
		private List<INetSerializable> fields = new List<INetSerializable>();

		public void AddFields(params INetSerializable[] fields)
		{
			foreach (INetSerializable field in fields)
			{
				AddField(field);
			}
		}

		public void CancelInterpolation()
		{
			foreach (INetSerializable field in fields)
			{
				if (field is InterpolationCancellable)
				{
					(field as InterpolationCancellable).CancelInterpolation();
				}
			}
		}

		public void AddField(INetSerializable field)
		{
			if (field.Parent != null)
			{
				throw new InvalidOperationException("Attempt to add a field to more than one tree");
			}
			if (base.Parent != null)
			{
				throw new InvalidOperationException("Cannot add new fields once this NetFields is part of a tree");
			}
			fields.Add(field);
		}

		public override void Read(BinaryReader reader, NetVersion version)
		{
			BitArray dirtyBits = reader.ReadBitArray();
			if (fields.Count != dirtyBits.Length)
			{
				throw new InvalidOperationException();
			}
			for (int i = 0; i < fields.Count; i++)
			{
				if (dirtyBits[i])
				{
					fields[i].Read(reader, version);
				}
			}
		}

		public override void Write(BinaryWriter writer)
		{
			BitArray dirtyBits = new BitArray(fields.Count);
			for (int j = 0; j < fields.Count; j++)
			{
				dirtyBits[j] = fields[j].Dirty;
			}
			writer.WriteBitArray(dirtyBits);
			for (int i = 0; i < fields.Count; i++)
			{
				if (dirtyBits[i])
				{
					INetSerializable netSerializable = fields[i];
					writer.Push(Convert.ToString(i));
					netSerializable.Write(writer);
					writer.Pop();
				}
			}
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			foreach (INetSerializable field in fields)
			{
				field.ReadFull(reader, version);
			}
		}

		public override void WriteFull(BinaryWriter writer)
		{
			for (int i = 0; i < fields.Count; i++)
			{
				INetSerializable netSerializable = fields[i];
				writer.Push(Convert.ToString(i));
				netSerializable.WriteFull(writer);
				writer.Pop();
			}
		}

		public virtual void CopyFrom(NetFields source)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					using (BinaryReader reader = new BinaryReader(stream))
					{
						source.WriteFull(writer);
						stream.Seek(0L, SeekOrigin.Begin);
						if (base.Root == null)
						{
							ReadFull(reader, new NetClock().netVersion);
						}
						else
						{
							ReadFull(reader, base.Root.Clock.netVersion);
						}
						MarkClean();
					}
				}
			}
		}

		protected override void ForEachChild(Action<INetSerializable> childAction)
		{
			foreach (INetSerializable field in fields)
			{
				childAction(field);
			}
		}
	}
}
