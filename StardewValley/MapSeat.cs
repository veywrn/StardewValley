using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley
{
	public class MapSeat : INetObject<NetFields>, ISittable
	{
		[XmlIgnore]
		public static Texture2D mapChairTexture;

		[XmlIgnore]
		public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

		[XmlIgnore]
		public NetVector2 tilePosition = new NetVector2();

		[XmlIgnore]
		public NetVector2 size = new NetVector2();

		[XmlIgnore]
		public NetInt direction = new NetInt();

		[XmlIgnore]
		public NetVector2 drawTilePosition = new NetVector2(new Vector2(-1f, -1f));

		[XmlIgnore]
		public NetBool seasonal = new NetBool();

		[XmlIgnore]
		public NetString seatType = new NetString();

		[XmlIgnore]
		public NetString textureFile = new NetString(null);

		[XmlIgnore]
		public string _loadedTextureFile;

		[XmlIgnore]
		public Texture2D overlayTexture;

		[XmlIgnore]
		public int localSittingDirection = 2;

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public MapSeat()
		{
			NetFields.AddFields(sittingFarmers, tilePosition, size, direction, drawTilePosition, seasonal, seatType, textureFile);
		}

		public static MapSeat FromData(string data, int x, int y)
		{
			MapSeat instance = new MapSeat();
			try
			{
				string[] data_split = data.Split('/');
				instance.tilePosition.Set(new Vector2(x, y));
				instance.size.Set(new Vector2(int.Parse(data_split[0]), int.Parse(data_split[1])));
				instance.seatType.Value = data_split[3];
				if (data_split[2] == "right")
				{
					instance.direction.Value = 1;
				}
				else if (data_split[2] == "left")
				{
					instance.direction.Value = 3;
				}
				else if (data_split[2] == "down")
				{
					instance.direction.Value = 2;
				}
				else if (data_split[2] == "up")
				{
					instance.direction.Value = 0;
				}
				else if (data_split[2] == "opposite")
				{
					instance.direction.Value = -2;
				}
				instance.drawTilePosition.Set(new Vector2(int.Parse(data_split[4]), int.Parse(data_split[5])));
				instance.seasonal.Value = (data_split[6] == "true");
				if (data_split.Length <= 7)
				{
					instance.textureFile.Value = null;
					return instance;
				}
				instance.textureFile.Value = data_split[7];
				return instance;
			}
			catch (Exception)
			{
				return instance;
			}
		}

		public bool IsBlocked(GameLocation location)
		{
			Rectangle rect = GetSeatBounds();
			rect.X *= 64;
			rect.Y *= 64;
			rect.Width *= 64;
			rect.Height *= 64;
			Rectangle extended_rect = rect;
			if ((int)direction == 0)
			{
				extended_rect.Y -= 32;
				extended_rect.Height += 32;
			}
			else if ((int)direction == 2)
			{
				extended_rect.Height += 32;
			}
			if ((int)direction == 3)
			{
				extended_rect.X -= 32;
				extended_rect.Width += 32;
			}
			else if ((int)direction == 1)
			{
				extended_rect.Width += 32;
			}
			foreach (NPC character in location.characters)
			{
				Rectangle character_rect = character.GetBoundingBox();
				if (character_rect.Intersects(rect))
				{
					return true;
				}
				if (!character.isMovingOnPathFindPath.Value && character_rect.Intersects(extended_rect))
				{
					return true;
				}
			}
			return false;
		}

		public bool IsSittingHere(Farmer who)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return true;
			}
			return false;
		}

		public bool HasSittingFarmers()
		{
			return sittingFarmers.Count() > 0;
		}

		public List<Vector2> GetSeatPositions(bool ignore_offsets = false)
		{
			List<Vector2> seat_positions = new List<Vector2>();
			if (seatType.Value == "playground")
			{
				Vector2 seat = new Vector2(tilePosition.X + 0.75f, tilePosition.Y);
				if (!ignore_offsets)
				{
					seat.Y -= 0.1f;
				}
				seat_positions.Add(seat);
			}
			else if (seatType.Value == "ccdesk")
			{
				Vector2 seat2 = new Vector2(tilePosition.X + 0.5f, tilePosition.Y);
				if (!ignore_offsets)
				{
					seat2.Y -= 0.4f;
				}
				seat_positions.Add(seat2);
			}
			else
			{
				for (int x = 0; (float)x < size.X; x++)
				{
					for (int y = 0; (float)y < size.Y; y++)
					{
						Vector2 offset = new Vector2(0f, 0f);
						if (seatType.Value.StartsWith("bench"))
						{
							if (direction.Value == 2)
							{
								offset.Y += 0.25f;
							}
							else if ((direction.Value == 3 || direction.Value == 1) && y == 0)
							{
								offset.Y += 0.5f;
							}
						}
						if (seatType.Value.StartsWith("picnic"))
						{
							if (direction.Value == 2)
							{
								offset.Y -= 0.25f;
							}
							else if (direction.Value == 0)
							{
								offset.Y += 0.25f;
							}
						}
						if (seatType.Value.EndsWith("swings"))
						{
							offset.Y -= 0.5f;
						}
						if (seatType.Value.EndsWith("summitbench"))
						{
							offset.Y -= 0.2f;
						}
						if (seatType.Value.EndsWith("tall"))
						{
							offset.Y -= 0.3f;
						}
						if (seatType.Value.EndsWith("short"))
						{
							offset.Y += 0.3f;
						}
						if (ignore_offsets)
						{
							offset = Vector2.Zero;
						}
						seat_positions.Add(tilePosition.Value + new Vector2((float)x + offset.X, (float)y + offset.Y));
					}
				}
			}
			return seat_positions;
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (_loadedTextureFile != textureFile.Value)
			{
				_loadedTextureFile = textureFile.Value;
				try
				{
					overlayTexture = Game1.content.Load<Texture2D>(_loadedTextureFile);
				}
				catch (Exception)
				{
					overlayTexture = null;
				}
			}
			if (overlayTexture == null)
			{
				overlayTexture = mapChairTexture;
			}
			if (!(drawTilePosition.Value.X >= 0f) || !HasSittingFarmers())
			{
				return;
			}
			int extra_height = 0;
			if (seatType.Value.StartsWith("highback_chair") || seatType.Value.StartsWith("ccdesk"))
			{
				extra_height = 1;
			}
			Vector2 draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2(tilePosition.X * 64f, (tilePosition.Y - (float)extra_height) * 64f));
			float sort_layer = (float)(((double)((float)(int)tilePosition.Y + size.Y) + 0.1) * 64.0) / 10000f;
			Rectangle source_rect = new Rectangle((int)drawTilePosition.Value.X * 16, (int)(drawTilePosition.Value.Y - (float)extra_height) * 16, (int)size.Value.X * 16, (int)(size.Value.Y + (float)extra_height) * 16);
			if (seasonal.Value)
			{
				if (Game1.currentLocation.GetSeasonForLocation() == "summer")
				{
					source_rect.X += source_rect.Width;
				}
				else if (Game1.currentLocation.GetSeasonForLocation() == "fall")
				{
					source_rect.X += source_rect.Width * 2;
				}
				else if (Game1.currentLocation.GetSeasonForLocation() == "winter")
				{
					source_rect.X += source_rect.Width * 3;
				}
			}
			b.Draw(overlayTexture, draw_position, source_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
		}

		public bool OccupiesTile(int x, int y)
		{
			return GetSeatBounds().Contains(x, y);
		}

		public virtual Vector2? AddSittingFarmer(Farmer who)
		{
			if (who == Game1.player)
			{
				localSittingDirection = direction.Value;
				if (seatType.Value.StartsWith("stool"))
				{
					localSittingDirection = Game1.player.FacingDirection;
				}
				if (direction.Value == -2)
				{
					localSittingDirection = Utility.GetOppositeFacingDirection(Game1.player.FacingDirection);
				}
				if (seatType.Value.StartsWith("bathchair") && localSittingDirection == 0)
				{
					localSittingDirection = 2;
				}
			}
			List<Vector2> seat_positions = GetSeatPositions();
			int seat_index = -1;
			Vector2? sit_position = null;
			float distance = 96f;
			for (int i = 0; i < seat_positions.Count; i++)
			{
				if (!sittingFarmers.Values.Contains(i))
				{
					float curr_distance = ((seat_positions[i] + new Vector2(0.5f, 0.5f)) * 64f - who.getStandingPosition()).Length();
					if (curr_distance < distance)
					{
						distance = curr_distance;
						sit_position = seat_positions[i];
						seat_index = i;
					}
				}
			}
			if (sit_position.HasValue)
			{
				sittingFarmers[who.UniqueMultiplayerID] = seat_index;
			}
			return sit_position;
		}

		public bool IsSeatHere(GameLocation location)
		{
			return location.mapSeats.Contains(this);
		}

		public int GetSittingDirection()
		{
			return localSittingDirection;
		}

		public Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
		{
			if (sittingFarmers.ContainsKey(who.UniqueMultiplayerID))
			{
				return GetSeatPositions(ignore_offsets)[sittingFarmers[who.UniqueMultiplayerID]];
			}
			return null;
		}

		public virtual Rectangle GetSeatBounds()
		{
			if (seatType.Value == "chair" && (int)direction == 0)
			{
				new Rectangle((int)tilePosition.X, (int)tilePosition.Y + 1, (int)size.X, (int)size.Y - 1);
			}
			return new Rectangle((int)tilePosition.X, (int)tilePosition.Y, (int)size.X, (int)size.Y);
		}

		public virtual void RemoveSittingFarmer(Farmer farmer)
		{
			sittingFarmers.Remove(farmer.UniqueMultiplayerID);
		}

		public virtual int GetSittingFarmerCount()
		{
			return sittingFarmers.Count();
		}
	}
}
