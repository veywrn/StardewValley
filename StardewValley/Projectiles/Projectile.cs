using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

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

		public const int slash = 11;

		public const int arrowBolt = 12;

		public const int launchedSlime = 13;

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

		protected float? _rotation;

		[XmlIgnore]
		public float hostTimeUntilAttackable = -1f;

		public readonly NetFloat startingRotation = new NetFloat();

		protected readonly NetFloat rotationVelocity = new NetFloat();

		protected readonly NetFloat xVelocity = new NetFloat();

		protected readonly NetFloat yVelocity = new NetFloat();

		public readonly NetColor color = new NetColor(Color.White);

		private Queue<Vector2> tail = new Queue<Vector2>();

		public readonly NetInt maxTravelDistance = new NetInt(-1);

		public float travelDistance;

		public NetFloat height = new NetFloat(0f);

		protected readonly NetBool damagesMonsters = new NetBool();

		protected readonly NetBool spriteFromObjectSheet = new NetBool();

		protected readonly NetCharacterRef theOneWhoFiredMe = new NetCharacterRef();

		public readonly NetBool ignoreTravelGracePeriod = new NetBool(value: false);

		public readonly NetBool ignoreLocationCollision = new NetBool();

		public readonly NetBool ignoreMeleeAttacks = new NetBool(value: false);

		public bool destroyMe;

		public readonly NetFloat startingScale = new NetFloat(1f);

		[XmlIgnore]
		protected float? _localScale;

		public readonly NetFloat scaleGrow = new NetFloat(0f);

		public NetBool light = new NetBool();

		public bool hasLit;

		private int lightID;

		private float startingAlpha = 1f;

		protected float rotation
		{
			get
			{
				if (!_rotation.HasValue)
				{
					_rotation = startingRotation.Value;
				}
				return _rotation.Value;
			}
			set
			{
				_rotation = value;
			}
		}

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


		[XmlIgnore]
		public virtual float localScale
		{
			get
			{
				if (!_localScale.HasValue)
				{
					_localScale = startingScale.Value;
				}
				return _localScale.Value;
			}
			set
			{
				_localScale = value;
			}
		}

		public Projectile()
		{
			NetFields.AddFields(currentTileSheetIndex, position.NetFields, tailLength, bouncesLeft, rotationVelocity, startingRotation, xVelocity, yVelocity, damagesMonsters, spriteFromObjectSheet, theOneWhoFiredMe.NetFields, ignoreLocationCollision, maxTravelDistance, ignoreTravelGracePeriod, ignoreMeleeAttacks, height, startingScale, scaleGrow, color, light);
		}

		private bool behaviorOnCollision(GameLocation location)
		{
			if (hasLit)
			{
				Utility.removeLightSource(lightID);
			}
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
			if (Game1.IsMasterGame && hostTimeUntilAttackable > 0f)
			{
				hostTimeUntilAttackable -= (float)time.ElapsedGameTime.TotalSeconds;
				if (hostTimeUntilAttackable <= 0f)
				{
					ignoreMeleeAttacks.Value = false;
					hostTimeUntilAttackable = -1f;
				}
			}
			if ((bool)light)
			{
				if (!hasLit)
				{
					hasLit = true;
					lightID = Game1.random.Next(int.MinValue, int.MaxValue);
					Game1.currentLightSources.Add(new LightSource(4, position + new Vector2(32f, 32f), 1f, new Color(0, 65, 128), lightID, LightSource.LightContext.None, 0L));
				}
				else
				{
					_ = (Vector2)position;
					Utility.repositionLightSource(lightID, position + new Vector2(32f, 32f));
				}
			}
			rotation += rotationVelocity;
			travelTime += time.ElapsedGameTime.Milliseconds;
			if (scaleGrow.Value != 0f)
			{
				localScale += scaleGrow.Value;
			}
			Vector2 old_position = position.Value;
			updatePosition(time);
			updateTail(time);
			travelDistance += (old_position - position.Value).Length();
			if (maxTravelDistance.Value >= 0)
			{
				if (travelDistance > (float)((int)maxTravelDistance - 128))
				{
					startingAlpha = ((float)(int)maxTravelDistance - travelDistance) / 128f;
				}
				if (travelDistance >= (float)(int)maxTravelDistance)
				{
					if (hasLit)
					{
						Utility.removeLightSource(lightID);
					}
					return true;
				}
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
			Vector2 pos = position.Value;
			int damageSize2 = boundingBoxWidth + (damagesMonsters ? 8 : 0);
			float current_scale = localScale;
			damageSize2 = (int)((float)damageSize2 * current_scale);
			return new Rectangle((int)pos.X + 32 - damageSize2 / 2, (int)pos.Y + 32 - damageSize2 / 2, damageSize2, damageSize2);
		}

		public virtual void draw(SpriteBatch b)
		{
			float current_scale = 4f * localScale;
			float alpha = startingAlpha;
			b.Draw(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, 0f - (float)height) + new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, currentTileSheetIndex, 16, 16), color.Value * startingAlpha, rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (position.Y + 96f) / 10000f);
			if (height.Value > 0f)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(32f, 32f)), Game1.shadowTexture.Bounds, Color.White * alpha * 0.75f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 2f, SpriteEffects.None, (position.Y - 1f) / 10000f);
			}
			for (int i = tail.Count - 1; i >= 0; i--)
			{
				b.Draw(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, Game1.GlobalToLocal(Game1.viewport, Vector2.Lerp((i == tail.Count - 1) ? ((Vector2)position) : tail.ElementAt(i + 1), tail.ElementAt(i), (float)tailCounter / 50f) + new Vector2(0f, 0f - (float)height) + new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(spriteFromObjectSheet ? Game1.objectSpriteSheet : projectileSheet, currentTileSheetIndex, 16, 16), color.Value * alpha, rotation, new Vector2(8f, 8f), current_scale, SpriteEffects.None, (position.Y - (float)(tail.Count - i) + 96f) / 10000f);
				alpha -= 1f / (float)tail.Count;
				current_scale = 0.8f * (float)(4 - 4 / (i + 4));
			}
		}
	}
}
