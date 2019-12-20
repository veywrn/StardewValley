using System;

namespace StardewValley
{
	public struct FarmerPair
	{
		public long Farmer1;

		public long Farmer2;

		public static FarmerPair MakePair(long f1, long f2)
		{
			FarmerPair pair = default(FarmerPair);
			pair.Farmer1 = Math.Min(f1, f2);
			pair.Farmer2 = Math.Max(f1, f2);
			return pair;
		}

		public bool Contains(long f)
		{
			if (Farmer1 != f)
			{
				return Farmer2 == f;
			}
			return true;
		}

		public long GetOther(long f)
		{
			if (Farmer1 == f)
			{
				return Farmer2;
			}
			return Farmer1;
		}

		public bool Equals(FarmerPair other)
		{
			if (Farmer1 == other.Farmer1)
			{
				return Farmer2 == other.Farmer2;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is FarmerPair)
			{
				return Equals((FarmerPair)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Farmer1.GetHashCode() ^ (Farmer2.GetHashCode() << 16);
		}
	}
}
