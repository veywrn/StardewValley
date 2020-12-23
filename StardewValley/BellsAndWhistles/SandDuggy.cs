using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.BellsAndWhistles
{
	public class SandDuggy : INetObject<NetFields>
	{
		public enum State
		{
			DigUp,
			Idle,
			DigDown
		}

		[XmlIgnore]
		public NetList<Point, NetPoint> holeLocations = new NetList<Point, NetPoint>();

		[XmlIgnore]
		public int frame;

		[XmlIgnore]
		public NetInt currentHoleIndex = new NetInt(0);

		[XmlIgnore]
		public int _localIndex;

		[XmlIgnore]
		public NetLocationRef locationRef = new NetLocationRef();

		[XmlIgnore]
		public State currentState;

		[XmlIgnore]
		public Texture2D texture;

		[XmlIgnore]
		public float nextFrameUpdate;

		[XmlElement("whacked")]
		public NetBool whacked = new NetBool(value: false);

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public SandDuggy()
		{
			InitNetFields();
		}

		public SandDuggy(GameLocation location, Point[] points)
			: this()
		{
			locationRef.Value = location;
			foreach (Point point in points)
			{
				holeLocations.Add(point);
			}
			currentHoleIndex.Value = FindRandomFreePoint();
		}

		public virtual int FindRandomFreePoint()
		{
			if (locationRef.Value == null)
			{
				return -1;
			}
			List<int> valid_hole_locations = new List<int>();
			for (int j = 0; j < holeLocations.Count; j++)
			{
				Point hole_location = holeLocations[j];
				if (!locationRef.Value.isObjectAtTile(hole_location.X, hole_location.Y) && !locationRef.Value.isTerrainFeatureAt(hole_location.X, hole_location.Y) && !locationRef.Value.terrainFeatures.ContainsKey(Utility.PointToVector2(hole_location)))
				{
					valid_hole_locations.Add(j);
				}
			}
			if (valid_hole_locations.Count == 1)
			{
				return valid_hole_locations[0];
			}
			for (int i = 0; i < valid_hole_locations.Count; i++)
			{
				int index = valid_hole_locations[i];
				Point hole_location2 = holeLocations[index];
				bool near_farmer = false;
				foreach (Farmer farmer in locationRef.Value.farmers)
				{
					if (NearFarmer(hole_location2, farmer))
					{
						near_farmer = true;
						break;
					}
				}
				if (near_farmer)
				{
					valid_hole_locations.RemoveAt(i);
					i--;
				}
			}
			if (valid_hole_locations.Count > 0)
			{
				return Utility.GetRandom(valid_hole_locations);
			}
			return -1;
		}

		public virtual void InitNetFields()
		{
			NetFields.AddFields(holeLocations, currentHoleIndex, locationRef.NetFields, whacked);
			whacked.fieldChangeVisibleEvent += OnWhackedChanged;
		}

		public virtual void OnWhackedChanged(NetBool field, bool old_value, bool new_value)
		{
			if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback() || !whacked.Value)
			{
				return;
			}
			if (Game1.IsMasterGame)
			{
				int index = currentHoleIndex.Value;
				if (index == -1)
				{
					index = 0;
				}
				Game1.player.team.MarkCollectedNut("SandDuggy");
				Game1.createItemDebris(new Object(73, 1), new Vector2(holeLocations[index].X, holeLocations[index].Y) * 64f, -1, locationRef.Value);
			}
			if (Game1.currentLocation == locationRef.Value)
			{
				AnimateWhacked();
			}
		}

		public virtual void AnimateWhacked()
		{
			if (Game1.currentLocation == locationRef.Value)
			{
				int index = currentHoleIndex.Value;
				if (index == -1)
				{
					index = 0;
				}
				Vector2 position = new Vector2(holeLocations[index].X, holeLocations[index].Y);
				int ground_position = (int)(position.Y * 64f - 32f);
				if (Utility.isOnScreen((position + new Vector2(0.5f, 0.5f)) * 64f, 64))
				{
					Game1.playSound("axchop");
					Game1.playSound("rockGolemHit");
				}
				TemporaryAnimatedSprite duggy_sprite = new TemporaryAnimatedSprite("LooseSprites/SandDuggy", new Rectangle(0, 48, 16, 48), new Vector2(position.X * 64f, position.Y * 64f - 32f), flipped: false, 0f, Color.White)
				{
					motion = new Vector2(2f, -3f),
					acceleration = new Vector2(0f, 0.25f),
					interval = 1000f,
					animationLength = 1,
					alphaFade = 0.02f,
					layerDepth = 0.07682f,
					scale = 4f,
					yStopCoordinate = ground_position
				};
				duggy_sprite.reachedStopCoordinate = delegate
				{
					duggy_sprite.motion.Y = -3f;
					duggy_sprite.acceleration.Y = 0.25f;
					duggy_sprite.yStopCoordinate = ground_position;
					duggy_sprite.flipped = !duggy_sprite.flipped;
				};
				Game1.currentLocation.temporarySprites.Add(duggy_sprite);
			}
		}

		public virtual void ResetForPlayerEntry()
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SandDuggy");
		}

		public virtual void PerformToolAction(Tool tool, int tile_x, int tile_y)
		{
			if (currentState == State.Idle && _localIndex >= 0)
			{
				Point point = holeLocations[_localIndex];
				if (point.X == tile_x && point.Y == tile_y)
				{
					whacked.Value = true;
				}
			}
		}

		public virtual bool NearFarmer(Point location, Farmer farmer)
		{
			if (Math.Abs(location.X - farmer.getTileX()) <= 2 && Math.Abs(location.Y - farmer.getTileY()) <= 2)
			{
				return true;
			}
			return false;
		}

		public virtual void Update(GameTime time)
		{
			if (whacked.Value)
			{
				return;
			}
			if (currentHoleIndex.Value >= 0)
			{
				Point synched_position = holeLocations[currentHoleIndex.Value];
				if (NearFarmer(synched_position, Game1.player) && FindRandomFreePoint() != (int)currentHoleIndex)
				{
					currentHoleIndex.Value = -1;
					DelayedAction.playSoundAfterDelay((Game1.random.NextDouble() < 0.1) ? "cowboy_gopher" : "tinyWhip", 200);
				}
			}
			nextFrameUpdate -= (float)time.ElapsedGameTime.TotalSeconds;
			if (currentHoleIndex.Value < 0 && Game1.IsMasterGame)
			{
				currentHoleIndex.Value = FindRandomFreePoint();
			}
			if (currentState == State.DigDown && frame == 0)
			{
				if (currentHoleIndex.Value >= 0)
				{
					currentState = State.DigUp;
				}
				_localIndex = currentHoleIndex.Value;
			}
			if ((int)currentHoleIndex == -1 || currentHoleIndex.Value != _localIndex)
			{
				currentState = State.DigDown;
			}
			if (!(nextFrameUpdate <= 0f))
			{
				return;
			}
			if (_localIndex >= 0)
			{
				if (currentState == State.DigDown)
				{
					frame--;
					if (frame <= 0)
					{
						frame = 0;
					}
				}
				else if (currentState == State.DigUp)
				{
					if (_localIndex >= 0)
					{
						frame++;
						if (frame >= 4)
						{
							currentState = State.Idle;
						}
					}
				}
				else if (currentState == State.Idle)
				{
					frame++;
					if (frame > 7)
					{
						frame = 4;
					}
				}
			}
			nextFrameUpdate = 0.075f;
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (!whacked.Value && _localIndex >= 0)
			{
				Point point = holeLocations[_localIndex];
				Vector2 draw_position = (new Vector2(point.X, point.Y) + new Vector2(0.5f, 0.5f)) * 64f;
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, draw_position), new Rectangle(frame % 4 * 16, frame / 4 * 24, 16, 24), Color.White, 0f, new Vector2(8f, 20f), 4f, SpriteEffects.None, draw_position.Y / 10000f);
			}
		}
	}
}
