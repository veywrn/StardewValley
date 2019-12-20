using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Owl : Critter
	{
		public Owl()
		{
		}

		public Owl(Vector2 position)
		{
			baseFrame = 83;
			base.position = position;
			sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 32, 32);
			startingPosition = position;
			sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(83, 100),
				new FarmerSprite.AnimationFrame(84, 100),
				new FarmerSprite.AnimationFrame(85, 100),
				new FarmerSprite.AnimationFrame(86, 100)
			});
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			Vector2 parallax = new Vector2((float)Game1.viewport.X - Game1.previousViewportPosition.X, (float)Game1.viewport.Y - Game1.previousViewportPosition.Y) * 0.15f;
			position.Y += (float)time.ElapsedGameTime.TotalMilliseconds * 0.2f;
			position.X += (float)time.ElapsedGameTime.TotalMilliseconds * 0.05f;
			position -= parallax;
			return base.update(time, environment);
		}

		public override void draw(SpriteBatch b)
		{
		}

		public override void drawAboveFrontLayer(SpriteBatch b)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f + position.X / 100000f, 0, 0, Color.MediumBlue, flip, 4f);
		}
	}
}
