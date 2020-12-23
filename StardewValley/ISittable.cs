using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley
{
	public interface ISittable
	{
		bool IsSittingHere(Farmer who);

		bool HasSittingFarmers();

		void RemoveSittingFarmer(Farmer farmer);

		int GetSittingFarmerCount();

		List<Vector2> GetSeatPositions(bool ignore_offsets = false);

		Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false);

		Vector2? AddSittingFarmer(Farmer who);

		int GetSittingDirection();

		Rectangle GetSeatBounds();

		bool IsSeatHere(GameLocation location);
	}
}
