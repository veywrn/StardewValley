using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;

namespace StardewValley.Objects
{
	public class CombinedRing : Ring
	{
		public NetList<Ring, NetRef<Ring>> combinedRings = new NetList<Ring, NetRef<Ring>>();

		public CombinedRing()
		{
			base.NetFields.AddField(combinedRings);
		}

		public CombinedRing(int parent_sheet_index)
			: base(880)
		{
			base.NetFields.AddField(combinedRings);
		}

		public virtual void UpdateDescription()
		{
			loadDisplayFields();
		}

		protected override bool loadDisplayFields()
		{
			displayName = "Combined Ring";
			description = "";
			foreach (Ring ring in combinedRings)
			{
				ring.getDescription();
				description = description + ring.description + "\n\n";
			}
			description = description.Trim();
			return true;
		}

		public override bool GetsEffectOfRing(int ring_index)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				if (combinedRing.GetsEffectOfRing(ring_index))
				{
					return true;
				}
			}
			return base.GetsEffectOfRing(ring_index);
		}

		public override Item getOne()
		{
			CombinedRing combinedRing = new CombinedRing(indexInTileSheet);
			combinedRing._GetOneFrom(this);
			return combinedRing;
		}

		public override void _GetOneFrom(Item source)
		{
			combinedRings.Clear();
			foreach (Ring combinedRing in (source as CombinedRing).combinedRings)
			{
				Ring ring = combinedRing.getOne() as Ring;
				combinedRings.Add(ring);
			}
			loadDisplayFields();
			base._GetOneFrom(source);
		}

		public override int GetEffectsOfRingMultiplier(int ring_index)
		{
			int count = 0;
			foreach (Ring ring in combinedRings)
			{
				count += ring.GetEffectsOfRingMultiplier(ring_index);
			}
			return count;
		}

		public override void onEquip(Farmer who, GameLocation location)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onEquip(who, location);
			}
			base.onEquip(who, location);
		}

		public override void onLeaveLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onLeaveLocation(who, environment);
			}
			base.onLeaveLocation(who, environment);
		}

		public override void onMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onMonsterSlay(m, location, who);
			}
			base.onMonsterSlay(m, location, who);
		}

		public override void onUnequip(Farmer who, GameLocation location)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onUnequip(who, location);
			}
			base.onUnequip(who, location);
		}

		public override void onNewLocation(Farmer who, GameLocation environment)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.onNewLocation(who, environment);
			}
			base.onNewLocation(who, environment);
		}

		public void FixCombinedRing()
		{
			if (base.ParentSheetIndex != 880)
			{
				string[] data = Game1.objectInformation[880].Split('/');
				base.Category = -96;
				Name = data[0];
				price.Value = Convert.ToInt32(data[1]);
				indexInTileSheet.Value = 880;
				base.ParentSheetIndex = indexInTileSheet;
				loadDisplayFields();
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (combinedRings.Count >= 2)
			{
				float oldScaleSize = scaleSize;
				scaleSize = 1f;
				location.Y -= (oldScaleSize - 1f) * 32f;
				Rectangle src = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, combinedRings[0].indexInTileSheet, 16, 16);
				src.X += 5;
				src.Y += 7;
				src.Width = 4;
				src.Height = 6;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-12f, 8f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				src.X++;
				src.Y += 4;
				src.Width = 3;
				src.Height = 1;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(51f, 51f) * scaleSize + new Vector2(-8f, 4f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				src = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, combinedRings[1].indexInTileSheet, 16, 16);
				src.X += 9;
				src.Y += 7;
				src.Width = 4;
				src.Height = 6;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 8f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				src.Y += 4;
				src.Width = 3;
				src.Height = 1;
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(51f, 51f) * scaleSize + new Vector2(4f, 4f) * scaleSize, src, color * transparency, 0f, new Vector2(1.5f, 2f) * 4f * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				Color? color3 = TailoringMenu.GetDyeColor(combinedRings[0]);
				Color? color2 = TailoringMenu.GetDyeColor(combinedRings[1]);
				Color color1noNull = Color.Red;
				Color color2noNull = Color.Blue;
				if (color3.HasValue)
				{
					color1noNull = color3.Value;
				}
				if (color2.HasValue)
				{
					color2noNull = color2.Value;
				}
				base.drawInMenu(spriteBatch, location + new Vector2(-5f, -1f), scaleSize, transparency, layerDepth, drawStackNumber, Utility.Get2PhaseColor(color1noNull, color2noNull), drawShadow);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(13f, 35f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 1125f) * transparency, -(float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(49f, 35f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 375f) * transparency, (float)Math.PI / 2f, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
				spriteBatch.Draw(Game1.objectSpriteSheet, location + new Vector2(31f, 53f) * scaleSize, new Rectangle(263, 579, 4, 2), Utility.Get2PhaseColor(color1noNull, color2noNull, 0, 1f, 750f) * transparency, (float)Math.PI, new Vector2(2f, 1.5f) * scaleSize, scaleSize * 4f, SpriteEffects.None, layerDepth);
			}
			else
			{
				base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
			}
		}

		public override void update(GameTime time, GameLocation environment, Farmer who)
		{
			foreach (Ring combinedRing in combinedRings)
			{
				combinedRing.update(time, environment, who);
			}
			base.update(time, environment, who);
		}
	}
}
