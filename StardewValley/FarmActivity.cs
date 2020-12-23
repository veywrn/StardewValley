using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class FarmActivity
	{
		protected NPC _character;

		public Vector2 activityPosition;

		public int activityDirection = 2;

		public float weight = 1f;

		protected float _age;

		protected bool _performingActivity;

		public virtual FarmActivity Initialize(NPC character, float activity_weight = 1f)
		{
			_character = character;
			weight = activity_weight;
			return this;
		}

		public virtual bool AttemptActivity(Farm farm)
		{
			if (_AttemptActivity(farm))
			{
				return true;
			}
			return false;
		}

		protected virtual bool _AttemptActivity(Farm farm)
		{
			return false;
		}

		public bool Update(GameTime time)
		{
			_age += (float)time.ElapsedGameTime.TotalSeconds;
			return _Update(time);
		}

		protected virtual bool _Update(GameTime time)
		{
			if (_age >= 10f)
			{
				return true;
			}
			return false;
		}

		public bool IsPerformingActivity()
		{
			return _performingActivity;
		}

		public void BeginActivity()
		{
			_character.faceDirection(activityDirection);
			_age = 0f;
			_performingActivity = true;
			_BeginActivity();
		}

		protected virtual void _BeginActivity()
		{
		}

		public void EndActivity()
		{
			_performingActivity = false;
			_EndActivity();
		}

		protected virtual void _EndActivity()
		{
		}

		public virtual bool IsTileBlockedFromSight(Vector2 tile)
		{
			return false;
		}

		public Rectangle GetFarmBounds(Farm farm)
		{
			return new Rectangle(0, 0, farm.map.Layers[0].LayerWidth, farm.map.Layers[0].LayerHeight);
		}

		public Object GetRandomObject(Farm farm, Func<Object, bool> validator = null)
		{
			List<Object> objects = new List<Object>();
			foreach (Vector2 position in farm.objects.Keys)
			{
				Object found_object = farm.objects[position];
				if (found_object != null && (validator == null || validator(found_object)))
				{
					objects.Add(found_object);
				}
			}
			return Utility.GetRandom(objects);
		}

		public TerrainFeature GetRandomTerrainFeature(Farm farm, Func<TerrainFeature, bool> validator = null)
		{
			List<TerrainFeature> objects = new List<TerrainFeature>();
			foreach (Vector2 position in farm.terrainFeatures.Keys)
			{
				TerrainFeature found_object = farm.terrainFeatures[position];
				if (found_object != null && (validator == null || validator(found_object)))
				{
					objects.Add(found_object);
				}
			}
			return Utility.GetRandom(objects);
		}

		public HoeDirt GetRandomCrop(Farm farm, Func<Crop, bool> validator = null)
		{
			List<HoeDirt> crops = new List<HoeDirt>();
			foreach (Vector2 position in farm.terrainFeatures.Keys)
			{
				HoeDirt hoe_dirt;
				if ((hoe_dirt = (farm.terrainFeatures[position] as HoeDirt)) != null && hoe_dirt.crop != null && (validator == null || validator(hoe_dirt.crop)))
				{
					crops.Add(hoe_dirt);
				}
			}
			return Utility.GetRandom(crops);
		}

		public Vector2 GetNearbyTile(Farm farm, Vector2 tile)
		{
			return Utility.getRandomAdjacentOpenTile(tile, farm);
		}
	}
}
