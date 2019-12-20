using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	public interface IAnimalLocation
	{
		NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals
		{
			get;
		}

		bool CheckPetAnimal(Vector2 position, Farmer who);

		bool CheckPetAnimal(Rectangle rect, Farmer who);

		bool CheckInspectAnimal(Vector2 position, Farmer who);

		bool CheckInspectAnimal(Rectangle rect, Farmer who);
	}
}
