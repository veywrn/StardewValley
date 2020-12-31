using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode
{
	public abstract class AbstractNetEvent1<T> : AbstractNetSerializable
	{
		private class EventRecording
		{
			public T arg;

			public uint timestamp;

			public EventRecording(T arg, uint timestamp)
			{
				this.arg = arg;
				this.timestamp = timestamp;
			}
		}

		public delegate void Event(T arg);

		public bool InterpolationWait = true;

		private List<EventRecording> outgoingEvents = new List<EventRecording>();

		private List<EventRecording> incomingEvents = new List<EventRecording>();

		public event Event onEvent;

		public AbstractNetEvent1()
		{
		}

		public bool HasPendingEvent(Predicate<T> match)
		{
			return incomingEvents.Exists((EventRecording e) => match(e.arg));
		}

		public void Clear()
		{
			outgoingEvents.Clear();
			incomingEvents.Clear();
		}

		public void Fire(T arg)
		{
			EventRecording recording = new EventRecording(arg, GetLocalTick());
			outgoingEvents.Add(recording);
			incomingEvents.Add(recording);
			MarkDirty();
			Poll();
		}

		public void Poll()
		{
			List<EventRecording> triggeredEvents = null;
			foreach (EventRecording e2 in incomingEvents)
			{
				if (base.Root != null && GetLocalTick() < e2.timestamp)
				{
					break;
				}
				if (triggeredEvents == null)
				{
					triggeredEvents = new List<EventRecording>();
				}
				triggeredEvents.Add(e2);
			}
			if (triggeredEvents != null && triggeredEvents.Count > 0)
			{
				incomingEvents.RemoveAll(triggeredEvents.Contains);
				if (this.onEvent != null)
				{
					foreach (EventRecording e in triggeredEvents)
					{
						this.onEvent(e.arg);
					}
				}
			}
		}

		protected abstract T readEventArg(BinaryReader reader, NetVersion version);

		protected abstract void writeEventArg(BinaryWriter writer, T arg);

		public override void Read(BinaryReader reader, NetVersion version)
		{
			uint count = reader.Read7BitEncoded();
			uint timestamp = GetLocalTick();
			if (InterpolationWait)
			{
				timestamp = (uint)((int)timestamp + base.Root.Clock.InterpolationTicks);
			}
			for (uint i = 0u; i < count; i++)
			{
				uint delay = reader.ReadUInt32();
				incomingEvents.Add(new EventRecording(readEventArg(reader, version), timestamp + delay));
			}
			ChangeVersion.Merge(version);
		}

		public override void ReadFull(BinaryReader reader, NetVersion version)
		{
			ChangeVersion.Merge(version);
		}

		public override void Write(BinaryWriter writer)
		{
			writer.Write7BitEncoded((uint)outgoingEvents.Count);
			if (outgoingEvents.Count > 0)
			{
				uint baseTime = outgoingEvents[0].timestamp;
				foreach (EventRecording e in outgoingEvents)
				{
					writer.Write(e.timestamp - baseTime);
					writeEventArg(writer, e.arg);
				}
			}
			outgoingEvents.Clear();
		}

		protected override void CleanImpl()
		{
			base.CleanImpl();
			outgoingEvents.Clear();
		}

		public override void WriteFull(BinaryWriter writer)
		{
		}
	}
}
