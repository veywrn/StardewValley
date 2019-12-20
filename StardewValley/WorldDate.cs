using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley
{
	public class WorldDate : INetObject<NetFields>
	{
		public const int MonthsPerYear = 4;

		public const int DaysPerMonth = 28;

		private readonly NetInt year = new NetInt(1);

		private readonly NetInt seasonIndex = new NetInt(0);

		private readonly NetInt dayOfMonth = new NetInt(1);

		public int Year
		{
			get
			{
				return year.Value;
			}
			set
			{
				year.Value = value;
			}
		}

		[XmlIgnore]
		public int SeasonIndex
		{
			get
			{
				return seasonIndex.Value;
			}
			private set
			{
				seasonIndex.Value = value;
			}
		}

		public int DayOfMonth
		{
			get
			{
				return dayOfMonth.Value;
			}
			set
			{
				dayOfMonth.Value = value;
			}
		}

		public DayOfWeek DayOfWeek => (DayOfWeek)(DayOfMonth % 7);

		public string Season
		{
			get
			{
				switch (SeasonIndex)
				{
				case 0:
					return "spring";
				case 1:
					return "summer";
				case 2:
					return "fall";
				case 3:
					return "winter";
				default:
					throw new ArgumentException(Convert.ToString(SeasonIndex));
				}
			}
			set
			{
				if (!(value == "spring"))
				{
					if (!(value == "summer"))
					{
						if (!(value == "fall"))
						{
							if (!(value == "winter"))
							{
								throw new ArgumentException(value);
							}
							SeasonIndex = 3;
						}
						else
						{
							SeasonIndex = 2;
						}
					}
					else
					{
						SeasonIndex = 1;
					}
				}
				else
				{
					SeasonIndex = 0;
				}
			}
		}

		public int TotalDays
		{
			get
			{
				return ((Year - 1) * 4 + SeasonIndex) * 28 + (DayOfMonth - 1);
			}
			set
			{
				int totalMonths = value / 28;
				DayOfMonth = value % 28 + 1;
				SeasonIndex = totalMonths % 4;
				Year = totalMonths / 4 + 1;
			}
		}

		public int TotalWeeks => TotalDays / 7;

		public int TotalSundayWeeks => (TotalDays + 1) / 7;

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public WorldDate()
		{
			NetFields.AddFields(year, seasonIndex, dayOfMonth);
		}

		public WorldDate(WorldDate other)
			: this()
		{
			Year = other.Year;
			SeasonIndex = other.SeasonIndex;
			DayOfMonth = other.DayOfMonth;
		}

		public WorldDate(int year, string season, int dayOfMonth)
			: this()
		{
			Year = year;
			Season = season;
			DayOfMonth = dayOfMonth;
		}

		public string Localize()
		{
			return Utility.getDateStringFor(DayOfMonth, SeasonIndex, Year);
		}

		public override string ToString()
		{
			return "Year " + Year + ", " + Season + " " + DayOfMonth + ", " + DayOfWeek;
		}

		public static bool operator ==(WorldDate a, WorldDate b)
		{
			return a?.TotalDays == b?.TotalDays;
		}

		public static bool operator !=(WorldDate a, WorldDate b)
		{
			return a?.TotalDays != b?.TotalDays;
		}

		public static bool operator <(WorldDate a, WorldDate b)
		{
			return a?.TotalDays < b?.TotalDays;
		}

		public static bool operator >(WorldDate a, WorldDate b)
		{
			return a?.TotalDays > b?.TotalDays;
		}

		public static bool operator <=(WorldDate a, WorldDate b)
		{
			return a?.TotalDays <= b?.TotalDays;
		}

		public static bool operator >=(WorldDate a, WorldDate b)
		{
			return a?.TotalDays >= b?.TotalDays;
		}
	}
}
