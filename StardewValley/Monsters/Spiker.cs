using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Monsters
{
	public class Spiker : Monster
	{
		[XmlIgnore]
		public int targetDirection;

		[XmlIgnore]
		public NetBool moving = new NetBool(value: false);

		protected bool _localMoving;

		[XmlIgnore]
		public float nextMoveCheck;

		public Spiker()
		{
		}

		public Spiker(Vector2 position, int direction)
			: base("Spiker", position)
		{
			Sprite.SpriteWidth = 16;
			Sprite.SpriteHeight = 16;
			Sprite.UpdateSourceRect();
			targetDirection = direction;
			base.speed = 14;
			ignoreMovementAnimations = true;
			onCollision = collide;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(moving);
		}

		public static Vector3? GetSpawnPosition(GameLocation location, Point start_point)
		{
			_ = Vector2.Zero;
			int direction = (Game1.random.Next(0, 2) == 0) ? 1 : (-1);
			int vertical_distance = 0;
			Vector2 vertical_spawn_point = Vector2.Zero;
			int horizontal_distance = 0;
			Vector2 horizontal_spawn_point = Vector2.Zero;
			Point current_point = start_point;
			current_point.Y += direction;
			while (location.isTileOnMap(current_point.X, current_point.Y) && location.getTileIndexAt(current_point, "Buildings") < 0)
			{
				current_point.Y += direction;
				vertical_distance++;
			}
			current_point = start_point;
			current_point.Y -= direction;
			while (location.isTileOnMap(current_point.X, current_point.Y) && location.getTileIndexAt(current_point, "Buildings") < 0)
			{
				vertical_spawn_point.X = current_point.X;
				vertical_spawn_point.Y = current_point.Y;
				current_point.Y -= direction;
				vertical_distance++;
			}
			current_point = start_point;
			current_point.X += direction;
			while (location.isTileOnMap(current_point.X, current_point.Y) && location.getTileIndexAt(current_point, "Buildings") < 0)
			{
				current_point.X += direction;
				horizontal_distance++;
			}
			current_point = start_point;
			current_point.X -= direction;
			while (location.isTileOnMap(current_point.X, current_point.Y) && location.getTileIndexAt(current_point, "Buildings") < 0)
			{
				horizontal_spawn_point.X = current_point.X;
				horizontal_spawn_point.Y = current_point.Y;
				current_point.X -= direction;
				horizontal_distance++;
			}
			if (horizontal_distance < vertical_distance)
			{
				if (horizontal_distance <= 4)
				{
					return null;
				}
				return new Vector3(horizontal_spawn_point.X, horizontal_spawn_point.Y, (direction == 1) ? 1 : 3);
			}
			if (vertical_distance <= 4)
			{
				return null;
			}
			return new Vector3(vertical_spawn_point.X, vertical_spawn_point.Y, (direction == 1) ? 2 : 0);
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.update(time, location);
			if (moving.Value == _localMoving)
			{
				return;
			}
			_localMoving = moving.Value;
			if (_localMoving)
			{
				if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("parry");
				}
			}
			else if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
			{
				Game1.playSound("hammer");
			}
		}

		public override bool passThroughCharacters()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position), (float)GetBoundingBox().Center.Y / 10000f);
		}

		private void collide(GameLocation location)
		{
			Rectangle bb = nextPosition(FacingDirection);
			foreach (Farmer farmer in location.farmers)
			{
				if (farmer.GetBoundingBox().Intersects(bb))
				{
					return;
				}
			}
			if ((bool)moving)
			{
				moving.Value = false;
				targetDirection = (targetDirection + 2) % 4;
				nextMoveCheck = 0.75f;
			}
		}

		public override void updateMovement(GameLocation location, GameTime time)
		{
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			return -1;
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			if (nextMoveCheck > 0f)
			{
				nextMoveCheck -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (nextMoveCheck <= 0f)
			{
				nextMoveCheck = 0.25f;
				foreach (Farmer farmer in base.currentLocation.farmers)
				{
					if ((targetDirection == 0 || targetDirection == 2) && Math.Abs(farmer.getTileX() - getTileX()) <= 1)
					{
						if (targetDirection == 0 && farmer.Position.Y < base.Position.Y)
						{
							moving.Value = true;
							break;
						}
						if (targetDirection == 2 && farmer.Position.Y > base.Position.Y)
						{
							moving.Value = true;
							break;
						}
					}
					if ((targetDirection == 3 || targetDirection == 1) && Math.Abs(farmer.getTileY() - getTileY()) <= 1)
					{
						if (targetDirection == 3 && farmer.Position.X < base.Position.X)
						{
							moving.Value = true;
							break;
						}
						if (targetDirection == 1 && farmer.Position.X > base.Position.X)
						{
							moving.Value = true;
							break;
						}
					}
				}
			}
			moveUp = false;
			moveDown = false;
			moveLeft = false;
			moveRight = false;
			if (moving.Value)
			{
				if (targetDirection == 0)
				{
					moveUp = true;
				}
				if (targetDirection == 2)
				{
					moveDown = true;
				}
				else if (targetDirection == 3)
				{
					moveLeft = true;
				}
				else if (targetDirection == 1)
				{
					moveRight = true;
				}
				MovePosition(time, Game1.viewport, base.currentLocation);
			}
			faceDirection(2);
		}
	}
}
