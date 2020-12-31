using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles
{
	public class ParrotPlatform
	{
		public enum TakeoffState
		{
			Idle,
			Boarding,
			BeginFlying,
			Liftoff,
			Flying,
			Finished
		}

		public class Parrot
		{
			public Vector2 position;

			public Vector2 anchorPosition;

			public Texture2D texture;

			protected ParrotPlatform _platform;

			protected bool facingRight;

			protected bool facingUp;

			public const int START_HEIGHT = 21;

			public const int END_HEIGHT = 64;

			public float height = 21f;

			public bool flapping;

			public float nextFlap;

			public float slack;

			public Vector2[] points = new Vector2[4];

			public float swayOffset;

			public float liftSpeed;

			public float squawkTime;

			public Parrot(ParrotPlatform platform, int x, int y, bool facing_right, bool facing_up)
			{
				_platform = platform;
				texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
				position = new Vector2(x, y);
				anchorPosition = position;
				facingRight = facing_right;
				facingUp = facing_up;
				swayOffset = Utility.RandomFloat(0f, 100f);
			}

			public virtual void UpdateLine(Vector2 start, Vector2 end)
			{
				float sag = Utility.Lerp(15f, 0f, (height - 21f) / 43f);
				for (int i = 0; i < points.Length; i++)
				{
					Vector2 point = new Vector2(Utility.Lerp(start.X, end.X, (float)i / (float)(points.Length - 1)), Utility.Lerp(start.Y, end.Y, (float)i / (float)(points.Length - 1)));
					point.Y -= ((float)Math.Pow(2f * ((float)i / (float)(points.Length - 1)) - 1f, 2.0) - 1f) * sag;
					points[i] = point;
				}
			}

			public virtual void Update(GameTime time)
			{
				if (squawkTime > 0f)
				{
					squawkTime -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (_platform.takeoffState < TakeoffState.BeginFlying)
				{
					return;
				}
				nextFlap -= (float)time.ElapsedGameTime.TotalSeconds;
				if (nextFlap <= 0f)
				{
					flapping = !flapping;
					if (flapping)
					{
						Game1.playSound("batFlap");
						nextFlap = Utility.RandomFloat(0.025f, 0.1f);
					}
					else
					{
						nextFlap = Utility.RandomFloat(0.075f, 0.15f);
					}
				}
				if (height < 64f)
				{
					height += liftSpeed;
					liftSpeed += 0.025f;
					if (facingRight)
					{
						position.X += 0.15f;
					}
					else
					{
						position.X -= 0.15f;
					}
					if (facingUp)
					{
						position.Y -= 0.15f;
					}
					else
					{
						position.Y += 0.15f;
					}
				}
			}

			public virtual void Draw(SpriteBatch b)
			{
				Vector2 draw_position = _platform.GetDrawPosition() + position * 4f;
				float radius = Utility.Lerp(0f, 2f, (height - 21f) / 43f);
				Vector2 draw_offset = new Vector2((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)swayOffset) * radius, (float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalSeconds * 16.0 + (double)swayOffset) * radius);
				if (_platform.takeoffState <= TakeoffState.Boarding)
				{
					int base_frame2 = 0;
					if (squawkTime > 0f)
					{
						draw_offset.X += Utility.RandomFloat(-0.15f, 0.15f) * 4f;
						draw_offset.Y += Utility.RandomFloat(-0.15f, 0.15f) * 4f;
						base_frame2 = 1;
					}
					b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle(base_frame2 * 24, 0, 24, 24), Color.White, 0f, new Vector2(12f, 19f), 4f, facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 192f) / 10000f);
					return;
				}
				int frame_off = flapping ? 1 : 0;
				if (flapping && nextFlap <= 0.05f)
				{
					frame_off = 2;
				}
				int base_frame = 5;
				if (facingUp)
				{
					base_frame = 8;
				}
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle((base_frame + frame_off) * 24, 0, 24, 24), Color.White, 0f, new Vector2(12f, 19f), 4f, facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 128f) / 10000f);
				Vector2 anchor_draw_position = _platform.position + anchorPosition * 4f;
				Vector2 drawPosition = _platform.GetDrawPosition();
				Vector2 start = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (anchorPosition - new Vector2(0f, 21f)) * 4f));
				Vector2 end = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (position - new Vector2(0f, height) + draw_offset) * 4f));
				UpdateLine(start + new Vector2(2f, 0f), end);
				if (points == null)
				{
					return;
				}
				Vector2? last_position = null;
				float sort_step = 1E-06f;
				float sort_offset = 0f;
				float sort_layer = (anchor_draw_position.Y + 0.05f) / 10000f;
				Vector2[] array = points;
				foreach (Vector2 current_point in array)
				{
					b.Draw(_platform.texture, current_point, new Microsoft.Xna.Framework.Rectangle(16, 68, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, sort_layer + sort_offset);
					sort_offset += sort_step;
					if (last_position.HasValue)
					{
						Vector2 offset = current_point - last_position.Value;
						int distance = (int)Math.Ceiling(offset.Length() / 4f);
						float rotation = 0f - (float)Math.Atan2(offset.X, offset.Y) + (float)Math.PI / 2f;
						b.Draw(_platform.texture, last_position.Value, new Microsoft.Xna.Framework.Rectangle(0, 68, 16, 16), Color.White, rotation, new Vector2(0f, 8f), new Vector2((float)(4 * distance) / 16f, 4f), SpriteEffects.None, sort_layer + sort_offset);
						sort_offset += sort_step;
					}
					last_position = current_point;
				}
			}
		}

		[XmlIgnore]
		[InstancedStatic]
		public static ParrotPlatform activePlatform;

		[XmlIgnore]
		public Vector2 position;

		[XmlIgnore]
		public Texture2D texture;

		[XmlIgnore]
		public List<Parrot> parrots = new List<Parrot>();

		[XmlIgnore]
		public float height;

		[XmlIgnore]
		protected Event _takeoffEvent;

		[XmlIgnore]
		public TakeoffState takeoffState;

		[XmlIgnore]
		public float stateTimer;

		[XmlIgnore]
		public float liftSpeed;

		[XmlIgnore]
		protected bool _onActivationTile;

		public Vector2 shake = Vector2.Zero;

		public string currentLocationKey = "";

		public KeyValuePair<string, KeyValuePair<string, Point>> currentDestination;

		public static List<KeyValuePair<string, KeyValuePair<string, Point>>> GetDestinations(bool only_show_accessible = true)
		{
			List<KeyValuePair<string, KeyValuePair<string, Point>>> destinations = new List<KeyValuePair<string, KeyValuePair<string, Point>>>();
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Volcano", new KeyValuePair<string, Point>("IslandNorth", new Point(60, 17))));
			if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeBridge") || !only_show_accessible)
			{
				destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Archaeology", new KeyValuePair<string, Point>("IslandNorth", new Point(5, 49))));
			}
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Farm", new KeyValuePair<string, Point>("IslandWest", new Point(74, 10))));
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Forest", new KeyValuePair<string, Point>("IslandEast", new Point(28, 29))));
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Docks", new KeyValuePair<string, Point>("IslandSouth", new Point(6, 32))));
			return destinations;
		}

		public static List<ParrotPlatform> CreateParrotPlatformsForArea(GameLocation location)
		{
			List<ParrotPlatform> parrot_platforms = new List<ParrotPlatform>();
			foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in GetDestinations(only_show_accessible: false))
			{
				if (location.Name == destination.Value.Key)
				{
					parrot_platforms.Add(new ParrotPlatform(destination.Value.Value.X - 1, destination.Value.Value.Y - 2, destination.Key));
				}
			}
			return parrot_platforms;
		}

		public ParrotPlatform()
		{
			texture = Game1.content.Load<Texture2D>("LooseSprites\\ParrotPlatform");
		}

		public ParrotPlatform(int tile_x, int tile_y, string key)
			: this()
		{
			currentLocationKey = key;
			position = new Vector2(tile_x * 64, tile_y * 64);
			parrots.Add(new Parrot(this, 15, 20, facing_right: false, facing_up: false));
			parrots.Add(new Parrot(this, 33, 20, facing_right: true, facing_up: false));
		}

		public virtual void StartDeparture()
		{
			takeoffState = TakeoffState.Boarding;
			Game1.playSound("parrot");
			foreach (Parrot parrot in parrots)
			{
				parrot.squawkTime = 0.25f;
			}
			stateTimer = 0.5f;
			Game1.player.shouldShadowBeOffset = true;
			xTile.Dimensions.Rectangle viewport = Game1.viewport;
			StringBuilder event_string2 = new StringBuilder();
			Vector2 farmer_position = Game1.player.Position;
			event_string2.Append("/0 0/farmer " + Game1.player.getTileX() + " " + Game1.player.getTileY() + " " + Game1.player.facingDirection);
			event_string2.Append("/playerControl parrotRide");
			_takeoffEvent = new Event(event_string2.ToString())
			{
				showWorldCharacters = true,
				showGroundObjects = true
			};
			event_string2 = null;
			Game1.currentLocation.currentEvent = _takeoffEvent;
			_takeoffEvent.checkForNextCommand(Game1.player.currentLocation, Game1.currentGameTime);
			Game1.player.Position = farmer_position;
			Game1.eventUp = true;
			Game1.viewport = viewport;
			foreach (Parrot parrot2 in parrots)
			{
				parrot2.height = 21f;
				parrot2.position = parrot2.anchorPosition;
			}
		}

		public virtual void Update(GameTime time)
		{
			if (takeoffState == TakeoffState.Idle && !Game1.player.IsBusyDoingSomething())
			{
				bool on_activation_tile = new Microsoft.Xna.Framework.Rectangle((int)position.X / 64, (int)position.Y / 64, 3, 1).Contains(Game1.player.getTileLocationPoint());
				if (_onActivationTile != on_activation_tile)
				{
					_onActivationTile = on_activation_tile;
					if (_onActivationTile && Game1.netWorldState.Value.ParrotPlatformsUnlocked.Value)
					{
						Activate();
					}
				}
			}
			shake = Vector2.Zero;
			if (takeoffState == TakeoffState.Liftoff)
			{
				shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
				shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
			}
			if (stateTimer > 0f)
			{
				stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (takeoffState == TakeoffState.Boarding && stateTimer <= 0f)
			{
				takeoffState = TakeoffState.BeginFlying;
				Game1.playSound("dwoop");
			}
			if (takeoffState == TakeoffState.BeginFlying && parrots[0].height >= 64f && stateTimer <= 0f)
			{
				takeoffState = TakeoffState.Liftoff;
				stateTimer = 0.5f;
				Game1.playSound("treethud");
			}
			if (takeoffState == TakeoffState.Liftoff && stateTimer <= 0f)
			{
				takeoffState = TakeoffState.Flying;
			}
			if (takeoffState >= TakeoffState.Flying && parrots[0].height >= 64f)
			{
				height += liftSpeed;
				liftSpeed += 0.025f;
				Game1.player.drawOffset.Value = new Vector2(0f, (0f - height) * 4f);
				if (height >= 128f && takeoffState != TakeoffState.Finished)
				{
					takeoffState = TakeoffState.Finished;
					_takeoffEvent.command_end(Game1.currentLocation, Game1.currentGameTime, new string[0]);
					_takeoffEvent = null;
					LocationRequest locationRequest = Game1.getLocationRequest(currentDestination.Value.Key);
					locationRequest.OnWarp += delegate
					{
						takeoffState = TakeoffState.Idle;
						Game1.player.shouldShadowBeOffset = false;
						Game1.player.drawOffset.Value = Vector2.Zero;
					};
					Game1.warpFarmer(locationRequest, currentDestination.Value.Value.X, currentDestination.Value.Value.Y, 2);
				}
			}
			foreach (Parrot parrot in parrots)
			{
				parrot.Update(time);
			}
		}

		public virtual void Activate()
		{
			List<Response> responses = new List<Response>();
			foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in GetDestinations())
			{
				if (destination.Key != currentLocationKey)
				{
					responses.Add(new Response("Go" + destination.Key, Game1.content.LoadString("Strings\\UI:ParrotPlatform_" + destination.Key)));
				}
			}
			responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
			Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:ParrotPlatform_Question"), responses.ToArray(), "ParrotPlatform");
			activePlatform = this;
		}

		public virtual bool AnswerQuestion(Response answer)
		{
			if (this == activePlatform)
			{
				if (Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.afterQuestion == null && (Game1.currentLocation.lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey).StartsWith("ParrotPlatform_Go"))
				{
					string destination_key = answer.responseKey.Substring(2);
					foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in GetDestinations())
					{
						if (destination.Key == destination_key)
						{
							currentDestination = destination;
							break;
						}
					}
					StartDeparture();
					return true;
				}
				activePlatform = null;
			}
			return false;
		}

		public virtual void Cleanup()
		{
			activePlatform = null;
		}

		public virtual bool CheckCollisions(Microsoft.Xna.Framework.Rectangle rectangle)
		{
			int wall_width = 16;
			if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, 192, wall_width)))
			{
				return true;
			}
			if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y + 128 - wall_width, 64, wall_width)))
			{
				return true;
			}
			if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X + 128, (int)position.Y + 128 - wall_width, 64, wall_width)))
			{
				return true;
			}
			if (takeoffState > TakeoffState.Idle && rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X + 64, (int)position.Y + 128 - wall_width, 64, wall_width)))
			{
				return true;
			}
			if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, wall_width, 128)))
			{
				return true;
			}
			if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)position.X + 192 - wall_width, (int)position.Y, wall_width, 128)))
			{
				return true;
			}
			return false;
		}

		public virtual bool OccupiesTile(Vector2 tile_pos)
		{
			if (tile_pos.X >= position.X / 64f && tile_pos.X < position.X / 64f + 3f && tile_pos.Y >= position.Y / 64f && tile_pos.Y < position.Y / 64f + 2f)
			{
				return true;
			}
			return false;
		}

		public virtual Vector2 GetDrawPosition()
		{
			return position - new Vector2(0f, 128f + height * 4f) + shake;
		}

		public virtual void Draw(SpriteBatch b)
		{
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position - new Vector2(0f, 128f) + new Vector2(-2f, 38f) * 4f + new Vector2(48f, 32f) * 4f / 2f), new Microsoft.Xna.Framework.Rectangle(48, 73, 48, 32), Color.White, 0f, new Vector2(48f, 32f) / 2f, 4f * (1f - Math.Min(1f, height / 480f)), SpriteEffects.None, 0f);
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 68), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, position.Y / 10000f);
			b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle(48, 0, 48, 68), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (position.Y + 128f) / 10000f);
			if (Game1.netWorldState.Value.ParrotPlatformsUnlocked.Value)
			{
				foreach (Parrot parrot in parrots)
				{
					parrot.Draw(b);
				}
			}
		}
	}
}
