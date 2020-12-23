using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class SuspensionBridge
	{
		public Rectangle bridgeBounds;

		public List<Rectangle> bridgeEntrances = new List<Rectangle>();

		public List<Rectangle> bridgeSortRegions = new List<Rectangle>();

		public const float BRIDGE_SORT_OFFSET = 0.0256f;

		protected Texture2D _texture;

		public float shakeTime;

		public SuspensionBridge()
		{
			_texture = Game1.content.Load<Texture2D>("LooseSprites\\SuspensionBridge");
		}

		public SuspensionBridge(int tile_x, int tile_y)
			: this()
		{
			bridgeBounds = new Rectangle(tile_x * 64, tile_y * 64, 384, 64);
			bridgeEntrances.Add(new Rectangle((tile_x - 1) * 64, tile_y * 64, 64, 64));
			bridgeEntrances.Add(new Rectangle((tile_x + 6) * 64, tile_y * 64, 64, 64));
			bridgeSortRegions.Add(new Rectangle((tile_x - 1) * 64, (tile_y - 1) * 64, 128, 192));
			bridgeSortRegions.Add(new Rectangle((tile_x + 5) * 64, (tile_y - 1) * 64, 128, 192));
		}

		public virtual bool InEntranceArea(int x, int y)
		{
			foreach (Rectangle bridgeEntrance in bridgeEntrances)
			{
				if (bridgeEntrance.Contains(x, y))
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool InEntranceArea(Rectangle rectangle)
		{
			foreach (Rectangle bridgeEntrance in bridgeEntrances)
			{
				if (bridgeEntrance.Contains(rectangle))
				{
					return true;
				}
			}
			return false;
		}

		public virtual bool CheckPlacementPrevention(Vector2 tileLocation)
		{
			foreach (Rectangle bridgeEntrance in bridgeEntrances)
			{
				if (Utility.doesRectangleIntersectTile(bridgeEntrance, (int)tileLocation.X, (int)tileLocation.Y))
				{
					return true;
				}
			}
			return false;
		}

		public virtual void OnFootstep(Vector2 position)
		{
			if (bridgeBounds.Contains((int)position.X, (int)position.Y) && position.X > (float)(bridgeBounds.X + 64) && position.X < (float)(bridgeBounds.Right - 64))
			{
				shakeTime = 0.4f;
			}
		}

		public virtual void Update(GameTime time)
		{
			if (shakeTime > 0f)
			{
				shakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (shakeTime < 0f)
				{
					shakeTime = 0f;
				}
			}
			if (Game1.player.bridge == null && InEntranceArea(Game1.player.GetBoundingBox()))
			{
				Game1.player.bridge = this;
			}
			if (Game1.player.bridge == this)
			{
				Rectangle farmer_bounds = Game1.player.GetBoundingBox();
				if (farmer_bounds.Top >= bridgeBounds.Top && farmer_bounds.Bottom <= bridgeBounds.Bottom && (farmer_bounds.Intersects(bridgeBounds) || InEntranceArea(farmer_bounds)))
				{
					Game1.player.SetOnBridge(val: true);
				}
				else if (!InEntranceArea(Game1.player.GetBoundingBox()) && !farmer_bounds.Intersects(bridgeBounds))
				{
					Game1.player.SetOnBridge(val: false);
					Game1.player.bridge = null;
				}
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			b.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(bridgeBounds.X, bridgeBounds.Y - 128)), new Rectangle(0, 0, 96, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)bridgeBounds.Y / 10000f + 0.0256f);
			float[] shake_multipliers = new float[6]
			{
				0f,
				0.5f,
				1f,
				1f,
				0.5f,
				0f
			};
			for (int i = 0; i < 6; i++)
			{
				float shake = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 10.0 + (double)(i * 5)) * 1f * 4f * shake_multipliers[i] * (shakeTime / 0.4f);
				b.Draw(_texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(bridgeBounds.X + i * 64, (float)bridgeBounds.Y + shake)), new Rectangle(16 * i, 32, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)bridgeBounds.Y / 10000f + 0.0256f);
			}
		}
	}
}
