using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class ColoredObject : Object
	{
		[XmlElement("color")]
		public readonly NetColor color = new NetColor();

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(color);
		}

		public ColoredObject()
		{
		}

		public ColoredObject(int parentSheetIndex, int stack, Color color)
			: base(parentSheetIndex, stack)
		{
			this.color.Value = color;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
		{
			if ((bool)isRecipe)
			{
				transparency = 0.5f;
				scaleSize *= 0.75f;
			}
			if ((bool)bigCraftable)
			{
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Object.getSourceRectForBigCraftable(parentSheetIndex), Color.White * transparency, 0f, new Vector2(32f, 64f) * scaleSize, (scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f), SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Object.getSourceRectForBigCraftable((int)parentSheetIndex + 1), color.Value * transparency, 0f, new Vector2(32f, 64f) * scaleSize, (scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f), SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
			}
			else
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, parentSheetIndex, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)parentSheetIndex + 1, 16, 16), color.Value * transparency, 0f, new Vector2(8f, 8f) * scaleSize, 4f * scaleSize, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
				if (((drawStackNumber == StackDrawType.Draw && maximumStackSize() > 1 && Stack > 1) || drawStackNumber == StackDrawType.Draw_OneInclusive) && (double)scaleSize > 0.3 && Stack != int.MaxValue)
				{
					Utility.drawTinyDigits(stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
				}
				if (drawStackNumber != 0 && (int)quality > 0)
				{
					float yOffset = ((int)quality < 2) ? 0f : (((float)Math.Cos((double)Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1f) * 0.05f);
					spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(12f, 52f + yOffset), new Rectangle(338 + ((int)quality - 1) * 8, 400, 8, 8), Color.White * transparency, 0f, new Vector2(4f, 4f), 3f * scaleSize * (1f + yOffset), SpriteEffects.None, layerDepth);
				}
			}
			if ((bool)isRecipe)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(16f, 16f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, Math.Min(1f, layerDepth + 0.0001f));
			}
		}

		public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
		{
			base.drawWhenHeld(spriteBatch, objectPosition, f);
			spriteBatch.Draw(Game1.objectSpriteSheet, objectPosition, GameLocation.getSourceRectForObject(f.ActiveObject.ParentSheetIndex + 1), color, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 4) / 10000f));
		}

		public override Item getOne()
		{
			ColoredObject coloredObject = new ColoredObject(parentSheetIndex, 1, color);
			coloredObject.Quality = quality;
			coloredObject.Price = price;
			coloredObject.HasBeenInInventory = base.HasBeenInInventory;
			coloredObject.HasBeenPickedUpByFarmer = hasBeenPickedUpByFarmer;
			coloredObject.SpecialVariable = base.SpecialVariable;
			coloredObject.preserve.Set(preserve.Value);
			coloredObject.preservedParentSheetIndex.Set(preservedParentSheetIndex.Value);
			coloredObject.Name = Name;
			return coloredObject;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			if ((bool)bigCraftable)
			{
				Vector2 scaleFactor = getScale();
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
				Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, Object.getSourceRectForBigCraftable(showNextIndex ? (base.ParentSheetIndex + 1) : base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, Object.getSourceRectForBigCraftable(base.ParentSheetIndex + 1), color, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				if (Name.Equals("Loom") && (int)minutesUntilReady > 0)
				{
					spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				}
			}
			else if (!Game1.eventUp || Game1.currentLocation.IsFarm)
			{
				if ((int)parentSheetIndex != 590)
				{
					spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-07f);
				}
				Texture2D objectSpriteSheet = Game1.objectSpriteSheet;
				Vector2 position2 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
				Rectangle? sourceRectangle = GameLocation.getSourceRectForObject(base.ParentSheetIndex);
				Color white = Color.White;
				Vector2 origin = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet, position2, sourceRectangle, white, 0f, origin, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
				Texture2D objectSpriteSheet2 = Game1.objectSpriteSheet;
				Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32));
				Rectangle? sourceRectangle2 = GameLocation.getSourceRectForObject(base.ParentSheetIndex + 1);
				Color obj = color;
				Vector2 origin2 = new Vector2(8f, 8f);
				_ = scale;
				spriteBatch.Draw(objectSpriteSheet2, position3, sourceRectangle2, obj, 0f, origin2, (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)getBoundingBox(new Vector2(x, y)).Bottom / 10000f);
			}
			if (Name != null && Name.Contains("Table") && heldObject.Value != null)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (bigCraftable ? 48 : 21))), GameLocation.getSourceRectForObject(heldObject.Value.ParentSheetIndex), Color.White, 0f, Vector2.Zero, 1f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(y * 64 + 64 + 1) / 10000f);
			}
		}
	}
}
