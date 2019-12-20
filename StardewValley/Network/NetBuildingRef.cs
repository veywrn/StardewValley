using Netcode;
using StardewValley.Buildings;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class NetBuildingRef : INetObject<NetFields>, IEnumerable<Building>, IEnumerable
	{
		private readonly NetString nameOfIndoors = new NetString();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Building Value
		{
			get
			{
				string nameOfIndoors = this.nameOfIndoors.Get();
				if (nameOfIndoors == null)
				{
					return null;
				}
				return Game1.getFarm().getBuildingByName(nameOfIndoors);
			}
			set
			{
				if (value == null)
				{
					nameOfIndoors.Value = null;
				}
				else
				{
					nameOfIndoors.Value = value.nameOfIndoors;
				}
			}
		}

		public NetBuildingRef()
		{
			NetFields.AddFields(nameOfIndoors);
		}

		public IEnumerator<Building> GetEnumerator()
		{
			yield return Value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public static implicit operator Building(NetBuildingRef buildingRef)
		{
			return buildingRef.Value;
		}
	}
}
