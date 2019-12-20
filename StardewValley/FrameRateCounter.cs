using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;

public class FrameRateCounter : DrawableGameComponent
{
	private LocalizedContentManager content;

	private SpriteBatch spriteBatch;

	private int frameRate;

	private int frameCounter;

	private TimeSpan elapsedTime = TimeSpan.Zero;

	public FrameRateCounter(Game game)
		: base(game)
	{
		content = new LocalizedContentManager(game.Services, base.Game.Content.RootDirectory);
	}

	protected override void LoadContent()
	{
		spriteBatch = new SpriteBatch(base.GraphicsDevice);
	}

	protected override void UnloadContent()
	{
		content.Unload();
	}

	public override void Update(GameTime gameTime)
	{
		elapsedTime += gameTime.ElapsedGameTime;
		if (elapsedTime > TimeSpan.FromSeconds(1.0))
		{
			elapsedTime -= TimeSpan.FromSeconds(1.0);
			frameRate = frameCounter;
			frameCounter = 0;
		}
	}

	public override void Draw(GameTime gameTime)
	{
		frameCounter++;
		string fps = $"fps: {frameRate}";
		spriteBatch.Begin();
		spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(33f, 33f), Color.Black);
		spriteBatch.DrawString(Game1.dialogueFont, fps, new Vector2(32f, 32f), Color.White);
		spriteBatch.End();
	}
}
