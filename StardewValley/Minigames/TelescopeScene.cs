using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Minigames
{
	public class TelescopeScene : IMinigame
	{
		public LocalizedContentManager temporaryContent;

		public Texture2D background;

		public Texture2D trees;

		public float yOffset;

		public GameLocation walkSpace;

		public TelescopeScene(NPC Maru)
		{
			temporaryContent = Game1.content.CreateTemporary();
			background = temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaru");
			trees = temporaryContent.Load<Texture2D>("LooseSprites\\nightSceneMaruTrees");
			walkSpace = new GameLocation(null, "walkSpace");
			walkSpace.map = new Map();
			walkSpace.map.AddLayer(new Layer("Back", walkSpace.map, new Size(30, 1), new Size(64)));
			Game1.currentLocation = walkSpace;
		}

		public bool overrideFreeMouseMovement()
		{
			return Game1.options.SnappyMenus;
		}

		public bool tick(GameTime time)
		{
			return false;
		}

		public void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public void leftClickHeld(int x, int y)
		{
		}

		public void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void releaseLeftClick(int x, int y)
		{
		}

		public void releaseRightClick(int x, int y)
		{
		}

		public void receiveKeyPress(Keys k)
		{
		}

		public void receiveKeyRelease(Keys k)
		{
		}

		public void draw(SpriteBatch b)
		{
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			b.Draw(background, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - background.Bounds.Width / 2 * 4, -(background.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height), background.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.001f);
			b.Draw(trees, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - trees.Bounds.Width / 2 * 4, -(trees.Bounds.Height * 4) + Game1.graphics.GraphicsDevice.Viewport.Height), trees.Bounds, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.End();
		}

		public void changeScreenSize()
		{
		}

		public void unload()
		{
			temporaryContent.Unload();
		}

		public void receiveEventPoke(int data)
		{
			throw new NotImplementedException();
		}

		public string minigameId()
		{
			return null;
		}

		public bool doMainGameUpdates()
		{
			return false;
		}

		public bool forceQuit()
		{
			return false;
		}
	}
}
