using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Projectiles
{
	public abstract class Projectile : INetObject<NetFields>
	{
		public const int travelTimeBeforeCollisionPossible = 100;

		public const int goblinsCurseIndex = 0;

		public const int flameBallIndex = 1;

		public const int fearBolt = 2;

		public const int shadowBall = 3;

		public const int bone = 4;

		public const int throwingKnife = 5;

		public const int snowBall = 6;

		public const int shamanBolt = 7;

		public const int frostBall = 8;

		public const int frozenBolt = 9;

		public const int fireball = 10;

		public const string projectileSheetName = "TileSheets\\Projectiles";

		public const int timePerTailUpdate = 50;

		public static int boundingBoxWidth = 21;

		public static int boundingBoxHeight = 21;

		public static Texture2D projectileSheet;

		protected readonly NetInt currentTileSheetIndex = new NetInt();

		protected readonly NetPosition position = new NetPosition();

		protected readonly NetInt tailLength = new NetInt();

		protected int tailCounter = 50;

		protected readonly NetInt bouncesLeft = new NetInt();

		protected int travelTime;

		protected float rotation;

		protected readonly NetFloat rotationVelocity = new NetFloat();

		protected readonly NetFloat xVelocity = new NetFloat();

		protected readonly NetFloat yVelocity = new NetFloat();

		private Queue<Vector2> tail = new Queue<Vector2>();

		public readonly NetInt maxTravelDistance = new NetInt(-1);

		public float travelDistance;

		protected readonly NetBool damagesMonsters = new NetBool();

		protected readonly NetBool spriteFromObjectSheet = new NetBool();

		protected readonly NetCharacterRef theOneWhoFiredMe = new NetCharacterRef();

		public readonly NetBool ignoreTravelGracePeriod = new NetBool(value: false);

		public readonly NetBool ignoreLocationCollision = new NetBool();

		public bool destroyMe;

		public bool IgnoreLocationCollision
		{
			get
			{
				return ignoreLocationCollision;
			}
			set
			{
				ignoreLocationCollision.Value = value;
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Projectile()
		{
			NetFields.AddFields(currentTileSheetIndex, position.NetFields, tailLength, bouncesLeft, rotationVelocity, xVelocity, yVelocity, damagesMonsters, spriteFromObjectSheet, theOneWhoFiredMe.NetFields, ignoreLocationCollision, maxTravelDistance, ignoreTravelGracePeriod);
		}

		private bool behaviorOnCollision(GameLocation location)
		{
			foreach (Vector2 tile in Utility.getListOfTileLocationsForBordersOfNonTileRectangle(getBoundingBox()))
			{
				foreach (Farmer player in location.farmers)
				{
					if (!damagesMonsters && player.GetBoundingBox().Intersects(getBoundingBox()))
					{
						behaviorOnCollisionWithPlayer(location, player);
						return true;
					}
				}
				if (location.terrainFeatures.ContainsKey(tile) && !location.terrainFeatures[tile].isPassable())
				{
					behaviorOnCollisionWithTerrainFeature(location.terrainFeatures[tile], tile, location);
					return true;
				}
				if ((bool)damagesMonsters)
				{
					NPC i = location.doesPositionCollideWithCharacter(getBoundingBox());
					if (i != null)
					{
						behaviorOnCollisionWithMonster(i, location);
						return true;
					}
				}
			}
			behaviorOnCollisionWithOther(location);
			return true;
		}

		public abstract void behaviorOnCollisionWithPlayer(GameLocation location, Farmer player);

		public abstract void behaviorOnCollisionWithTerrainFeature(TerrainFeature t, Vector2 tileLocation, GameLocation location);

		public abstract void behaviorOnCollisionWithMineWall(int tileX, int tileY);

		public abstract void behaviorOnCollisionWithOther(GameLocation location);

		public abstract void behaviorOnCollisionWithMonster(NPC n, GameLocation location);

		public virtual bool update(GameTime time, GameLocation location)
		{
			rotation += rotationVelocity;
			travelTime += time.ElapsedGameTime.Milliseconds;
			Vector2 old_position = position.Value;
			updatePosition(time);
			updateTail(time);
			travelDistance += (old_position - position.Value).Length();
			if (maxTravelDistance.Value >= 0 && travelDistance >= (float)(int)maxTravelDistance)
			{
				return true;
			}
			if (isColliding(location) && (travelTime > 100 || ignoreTravelGracePeriod.Value))
			{
				if ((int)bouncesLeft <= 0 || Game1.player.GetBoundingBox().Intersects(getBoundingBox()))
				{
					return behaviorOnCollision(location);
				}
				bouncesLeft.Value--;
				bool[] array = Utility.horizontalOrVerticalCollisionDirections(getBoundingBox(), theOneWhoFiredMe.Get(location), projectile: true);
				if (array[0])
				{
					xVelocity.Value = 0f - (float)xVelocity;
				}
				if (array[1])
				{
					yVelocity.Value = 0f - (float)yVelocity;
				}
			}
			return false;
		}

		private void updateTail(GameTime time)
		{
			tailCounter -= time.ElapsedGameTime.Milliseconds;
			if (tailCounter <= 0)
			{
				tailCounter = 50;
				tail.Enqueue(position);
				if (tail.Count > (int)tailLength)
				{
					tail.Dequeue();
				}
			}
		}

		public virtual bool isColliding(GameLocation location)
		{
			if (location.isTileOnMap(position.Value / 64f) && ((bool)ignoreLocationCollision || !location.isCollidingPosition(getBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: true, theOneWhoFiredMe.Get(location), pathfinding: false, projectile: true)) && ((bool)damagesMonsters || !Game1.player.GetBoundingBox().Intersects(getBoundingBox())))
			{
				if ((bool)damagesMonsters)
				{
					return location.doesPositionCollideWithCharacter(getBoundingBox()) != null;
				}
				return false;
			}
			return true;
		}

		public abstract void updatePosition(GameTime time);

		public virtual Rectangle getBoundingBox()
		{
			return new Rectangle((int)position.X + 32 - (boundingBoxWidth + (damagesMonsters ? 8 : 0)) / 2, (int)position.Y + 32 - (boundingBoxWidth + (damagesMonsters ? 8 : 0)) / 2, boundingBoxWidth + (damagesMonsters ? 8 : 0), boundingBoxWidth + (damagesMonsters ? 8 : 0));
		}

		public virtual void draw(SpriteBatch b)
		{
			b.Draw(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, currentTileSheetIndex, 16, 16), Color.White, rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, (position.Y + 96f) / 10000f);
			float scale = 4f;
			float alpha = 1f;
			for (int i = tail.Count - 1; i >= 0; i--)
			{
				b.Draw(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, Game1.GlobalToLocal(Game1.viewport, tail.ElementAt(i) + new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, currentTileSheetIndex, 16, 16), Color.White * alpha, rotation, new Vector2(8f, 8f), scale, SpriteEffects.None, (tail.ElementAt(i).Y + 96f) / 10000f);
				scale = 0.8f * (float)(4 - 4 / (i + 4));
				alpha -= 0.1f;
			}
		}
	}
}
