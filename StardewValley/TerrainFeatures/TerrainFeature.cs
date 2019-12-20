using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	[XmlInclude(typeof(Grass))]
	[XmlInclude(typeof(Tree))]
	[XmlInclude(typeof(Quartz))]
	[XmlInclude(typeof(HoeDirt))]
	[XmlInclude(typeof(Flooring))]
	[XmlInclude(typeof(CosmeticPlant))]
	[XmlInclude(typeof(ResourceClump))]
	[XmlInclude(typeof(GiantCrop))]
	[XmlInclude(typeof(FruitTree))]
	[XmlInclude(typeof(Bush))]
	public abstract class TerrainFeature : INetObject<NetFields>
	{
		[XmlIgnore]
		public readonly bool NeedsTick;

		[XmlIgnore]
		public bool isTemporarilyInvisible;

		[XmlIgnore]
		protected bool _needsUpdate = true;

		[XmlIgnore]
		public GameLocation currentLocation;

		[XmlIgnore]
		public Vector2 currentTileLocation;

		[XmlIgnore]
		public bool NeedsUpdate
		{
			get
			{
				return _needsUpdate;
			}
			set
			{
				if (value != _needsUpdate)
				{
					_needsUpdate = value;
					if (currentLocation != null)
					{
						currentLocation.UpdateTerrainFeatureUpdateSubscription(this);
					}
				}
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		protected TerrainFeature(bool needsTick)
		{
			NeedsTick = needsTick;
		}

		public virtual Rectangle getBoundingBox(Vector2 tileLocation)
		{
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		}

		public virtual Rectangle getRenderBounds(Vector2 tileLocation)
		{
			return getBoundingBox(tileLocation);
		}

		public virtual void loadSprite()
		{
		}

		public virtual bool isPassable(Character c = null)
		{
			return isTemporarilyInvisible;
		}

		public virtual void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who, GameLocation location)
		{
		}

		public virtual bool performUseAction(Vector2 tileLocation, GameLocation location)
		{
			return false;
		}

		public virtual bool performToolAction(Tool t, int damage, Vector2 tileLocation, GameLocation location)
		{
			return false;
		}

		public virtual bool tickUpdate(GameTime time, Vector2 tileLocation, GameLocation location)
		{
			return false;
		}

		public virtual void dayUpdate(GameLocation environment, Vector2 tileLocation)
		{
		}

		public virtual bool seasonUpdate(bool onLoad)
		{
			return false;
		}

		public virtual bool isActionable()
		{
			return false;
		}

		public virtual void performPlayerEntryAction(Vector2 tileLocation)
		{
			isTemporarilyInvisible = false;
		}

		public virtual void draw(SpriteBatch spriteBatch, Vector2 tileLocation)
		{
		}

		public virtual bool forceDraw()
		{
			return false;
		}

		public virtual void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
		{
		}
	}
}
