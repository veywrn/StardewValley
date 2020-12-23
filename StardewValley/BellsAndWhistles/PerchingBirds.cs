using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.BellsAndWhistles
{
	public class PerchingBirds
	{
		public const int BIRD_STARTLE_DISTANCE = 200;

		[XmlIgnore]
		public List<Bird> _birds = new List<Bird>();

		[XmlIgnore]
		protected Point[] _birdLocations;

		protected Point[] _birdRoostLocations;

		[XmlIgnore]
		public Dictionary<Point, Bird> _birdPointOccupancy;

		public bool roosting;

		protected Texture2D _birdSheet;

		protected int _birdWidth;

		protected int _birdHeight;

		protected int _flapFrames = 2;

		protected Vector2 _birdOrigin;

		public int peckDuration = 5;

		public float birdSpeed = 5f;

		public PerchingBirds(Texture2D bird_texture, int flap_frames, int width, int height, Vector2 origin, Point[] perch_locations, Point[] roost_locations)
		{
			_birdSheet = bird_texture;
			_birdWidth = width;
			_birdHeight = height;
			_birdOrigin = origin;
			_flapFrames = flap_frames;
			_birdPointOccupancy = new Dictionary<Point, Bird>();
			_birdLocations = perch_locations;
			_birdRoostLocations = roost_locations;
			ResetLocalState();
		}

		public int GetBirdWidth()
		{
			return _birdWidth;
		}

		public int GetBirdHeight()
		{
			return _birdHeight;
		}

		public Vector2 GetBirdOrigin()
		{
			return _birdOrigin;
		}

		public Texture2D GetTexture()
		{
			return _birdSheet;
		}

		public Point GetFreeBirdPoint(Bird bird = null, int clearance = 200)
		{
			List<Point> points = new List<Point>();
			Point[] currentBirdLocationList = GetCurrentBirdLocationList();
			for (int i = 0; i < currentBirdLocationList.Length; i++)
			{
				Point point = currentBirdLocationList[i];
				if (_birdPointOccupancy[point] == null)
				{
					bool fail = false;
					if (bird != null)
					{
						foreach (Farmer farmer in Game1.currentLocation.farmers)
						{
							if (Utility.distance(farmer.position.X, (float)(point.X * 64) + 32f, farmer.position.Y, (float)(point.Y * 64) + 32f) < 200f)
							{
								fail = true;
							}
						}
					}
					if (!fail)
					{
						points.Add(point);
					}
				}
			}
			return Utility.GetRandom(points);
		}

		public void ReserveBirdPoint(Bird bird, Point point)
		{
			if (_birdPointOccupancy.ContainsKey(bird.endPosition))
			{
				_birdPointOccupancy[bird.endPosition] = null;
			}
			if (_birdPointOccupancy.ContainsKey(point))
			{
				_birdPointOccupancy[point] = bird;
			}
		}

		public bool ShouldBirdsRoost()
		{
			return roosting;
		}

		public Point[] GetCurrentBirdLocationList()
		{
			if (ShouldBirdsRoost())
			{
				return _birdRoostLocations;
			}
			return _birdLocations;
		}

		public virtual void Update(GameTime time)
		{
			for (int i = 0; i < _birds.Count; i++)
			{
				_birds[i].Update(time);
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			for (int i = 0; i < _birds.Count; i++)
			{
				_birds[i].Draw(b);
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}

		public virtual void ResetLocalState()
		{
			_birds.Clear();
			_birdPointOccupancy = new Dictionary<Point, Bird>();
			Point[] birdLocations = _birdLocations;
			foreach (Point point in birdLocations)
			{
				_birdPointOccupancy[point] = null;
			}
			birdLocations = _birdRoostLocations;
			foreach (Point point2 in birdLocations)
			{
				_birdPointOccupancy[point2] = null;
			}
		}

		public virtual void AddBird(int bird_type)
		{
			Bird bird = new Bird(GetFreeBirdPoint(), this, bird_type, _flapFrames);
			_birds.Add(bird);
			ReserveBirdPoint(bird, bird.endPosition);
		}
	}
}
