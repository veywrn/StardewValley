using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Friendship : INetObject<NetFields>
	{
		private readonly NetInt points = new NetInt();

		private readonly NetInt giftsThisWeek = new NetInt();

		private readonly NetInt giftsToday = new NetInt();

		private readonly NetRef<WorldDate> lastGiftDate = new NetRef<WorldDate>();

		private readonly NetBool talkedToToday = new NetBool();

		private readonly NetBool proposalRejected = new NetBool();

		private readonly NetRef<WorldDate> weddingDate = new NetRef<WorldDate>();

		private readonly NetRef<WorldDate> nextBirthingDate = new NetRef<WorldDate>();

		private readonly NetEnum<FriendshipStatus> status = new NetEnum<FriendshipStatus>(FriendshipStatus.Friendly);

		private readonly NetLong proposer = new NetLong();

		private readonly NetBool roommateMarriage = new NetBool(value: false);

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public int Points
		{
			get
			{
				return points.Value;
			}
			set
			{
				points.Value = value;
			}
		}

		public int GiftsThisWeek
		{
			get
			{
				return giftsThisWeek.Value;
			}
			set
			{
				giftsThisWeek.Value = value;
			}
		}

		public int GiftsToday
		{
			get
			{
				return giftsToday.Value;
			}
			set
			{
				giftsToday.Value = value;
			}
		}

		public WorldDate LastGiftDate
		{
			get
			{
				return lastGiftDate.Value;
			}
			set
			{
				lastGiftDate.Value = value;
			}
		}

		public bool TalkedToToday
		{
			get
			{
				return talkedToToday.Value;
			}
			set
			{
				talkedToToday.Value = value;
			}
		}

		public bool ProposalRejected
		{
			get
			{
				return proposalRejected.Value;
			}
			set
			{
				proposalRejected.Value = value;
			}
		}

		public WorldDate WeddingDate
		{
			get
			{
				return weddingDate.Value;
			}
			set
			{
				weddingDate.Value = value;
			}
		}

		public WorldDate NextBirthingDate
		{
			get
			{
				return nextBirthingDate.Value;
			}
			set
			{
				nextBirthingDate.Value = value;
			}
		}

		public FriendshipStatus Status
		{
			get
			{
				return status.Value;
			}
			set
			{
				status.Value = value;
			}
		}

		public long Proposer
		{
			get
			{
				return proposer.Value;
			}
			set
			{
				proposer.Value = value;
			}
		}

		public bool RoommateMarriage
		{
			get
			{
				return roommateMarriage.Value;
			}
			set
			{
				roommateMarriage.Value = value;
			}
		}

		public int DaysMarried
		{
			get
			{
				if (WeddingDate == null || WeddingDate.TotalDays > Game1.Date.TotalDays)
				{
					return 0;
				}
				return Game1.Date.TotalDays - WeddingDate.TotalDays;
			}
		}

		public int CountdownToWedding
		{
			get
			{
				if (WeddingDate == null || WeddingDate.TotalDays < Game1.Date.TotalDays)
				{
					return 0;
				}
				return WeddingDate.TotalDays - Game1.Date.TotalDays;
			}
		}

		public int DaysUntilBirthing
		{
			get
			{
				if (NextBirthingDate == null)
				{
					return -1;
				}
				return Math.Max(0, NextBirthingDate.TotalDays - Game1.Date.TotalDays);
			}
		}

		public Friendship()
		{
			NetFields.AddFields(points, giftsThisWeek, giftsToday, lastGiftDate, talkedToToday, proposalRejected, weddingDate, nextBirthingDate, status, proposer, roommateMarriage);
		}

		public Friendship(int startingPoints)
			: this()
		{
			Points = startingPoints;
		}

		public void Clear()
		{
			points.Value = 0;
			giftsThisWeek.Value = 0;
			giftsToday.Value = 0;
			lastGiftDate.Value = null;
			talkedToToday.Value = false;
			proposalRejected.Value = false;
			roommateMarriage.Value = false;
			weddingDate.Value = null;
			nextBirthingDate.Value = null;
			status.Value = FriendshipStatus.Friendly;
			proposer.Value = 0L;
		}

		public bool IsDating()
		{
			if (Status != FriendshipStatus.Dating && Status != FriendshipStatus.Engaged)
			{
				return Status == FriendshipStatus.Married;
			}
			return true;
		}

		public bool IsEngaged()
		{
			return Status == FriendshipStatus.Engaged;
		}

		public bool IsMarried()
		{
			return Status == FriendshipStatus.Married;
		}

		public bool IsDivorced()
		{
			return Status == FriendshipStatus.Divorced;
		}

		public bool IsRoommate()
		{
			if (IsMarried())
			{
				return roommateMarriage.Value;
			}
			return false;
		}
	}
}
