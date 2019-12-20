using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;

namespace StardewValley.BellsAndWhistles
{
	public class Squirrel : Critter
	{
		private int nextNibbleTimer = 1000;

		private int treeRunTimer;

		private int characterCheckTimer = 200;

		private bool running;

		private Tree climbed;

		private Vector2 treeTile;

		public Squirrel(Vector2 position, bool flip)
		{
			base.position = position * 64f;
			base.flip = flip;
			baseFrame = 60;
			sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 32, 32);
			sprite.loop = false;
			startingPosition = position;
		}

		private void doneNibbling(Farmer who)
		{
			nextNibbleTimer = Game1.random.Next(2000);
		}

		public override void draw(SpriteBatch b)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64 + ((treeRunTimer > 0) ? (flip ? 224 : (-16)) : 0), -64f + yJumpOffset + yOffset + (float)((treeRunTimer > 0) ? ((!flip) ? 128 : 0) : 0))), (position.Y + 64f + (float)((treeRunTimer > 0) ? 128 : 0)) / 10000f + position.X / 1000000f, 0, 0, Color.White, flip, 4f, (treeRunTimer > 0) ? ((float)((double)(flip ? 1 : (-1)) * Math.PI / 2.0)) : 0f);
			if (treeRunTimer <= 0)
			{
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, 60f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 16f), SpriteEffects.None, (position.Y - 1f) / 10000f);
			}
		}

		public override bool update(GameTime time, GameLocation environment)
		{
			nextNibbleTimer -= time.ElapsedGameTime.Milliseconds;
			if (sprite.CurrentAnimation == null && nextNibbleTimer <= 0)
			{
				int nibbles = Game1.random.Next(2, 8);
				List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
				for (int i = 0; i < nibbles; i++)
				{
					anim.Add(new FarmerSprite.AnimationFrame(baseFrame, 200));
					anim.Add(new FarmerSprite.AnimationFrame(baseFrame + 1, 200));
				}
				anim.Add(new FarmerSprite.AnimationFrame(baseFrame, 200, secondaryArm: false, flip: false, doneNibbling));
				sprite.setCurrentAnimation(anim);
			}
			characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
			if (characterCheckTimer <= 0 && !running)
			{
				if (Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 12, environment) != null)
				{
					running = true;
					sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(baseFrame + 2, 50),
						new FarmerSprite.AnimationFrame(baseFrame + 3, 50),
						new FarmerSprite.AnimationFrame(baseFrame + 4, 50),
						new FarmerSprite.AnimationFrame(baseFrame + 5, 120),
						new FarmerSprite.AnimationFrame(baseFrame + 6, 80),
						new FarmerSprite.AnimationFrame(baseFrame + 7, 50)
					});
					sprite.loop = true;
				}
				characterCheckTimer = 200;
			}
			if (running)
			{
				if (treeRunTimer > 0)
				{
					position.Y -= 4f;
				}
				else
				{
					position.X += (flip ? (-4) : 4);
				}
			}
			if (running && characterCheckTimer <= 0 && treeRunTimer <= 0)
			{
				characterCheckTimer = 100;
				Vector2 v2 = new Vector2((int)(position.X / 64f), (int)position.Y / 64);
				if (environment.terrainFeatures.ContainsKey(v2) && environment.terrainFeatures[v2] is Tree)
				{
					treeRunTimer = 700;
					climbed = (environment.terrainFeatures[v2] as Tree);
					treeTile = v2;
					position = v2 * 64f;
					return false;
				}
				v2 = new Vector2((int)((position.X + 64f + 1f) / 64f), (int)position.Y / 64);
			}
			if (treeRunTimer > 0)
			{
				treeRunTimer -= time.ElapsedGameTime.Milliseconds;
				if (treeRunTimer <= 0)
				{
					climbed.performUseAction(treeTile, environment);
					return true;
				}
			}
			return base.update(time, environment);
		}
	}
}
