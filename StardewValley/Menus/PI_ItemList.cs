using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class PI_ItemList : ProfileItem
	{
		protected List<Item> _items;

		protected List<ClickableTextureComponent> _components;

		protected float _height;

		protected List<Vector2> _emptyBoxPositions;

		public PI_ItemList(ProfileMenu context, string name, List<Item> values)
			: base(context, name)
		{
			_items = values;
			_components = new List<ClickableTextureComponent>();
			_height = 0f;
			_emptyBoxPositions = new List<Vector2>();
			_UpdateIcons();
		}

		public override void Unload()
		{
			base.Unload();
			_ClearItems();
		}

		protected void _ClearItems()
		{
			for (int i = 0; i < _components.Count; i++)
			{
				_context.UnregisterClickable(_components[i]);
			}
			_components.Clear();
		}

		protected void _UpdateIcons()
		{
			_ClearItems();
			Vector2 draw_position = new Vector2(0f, 0f);
			for (int i = 0; i < _items.Count; i++)
			{
				Item item = _items[i];
				ClickableTextureComponent component = new ClickableTextureComponent(item.DisplayName, new Rectangle((int)draw_position.X, (int)draw_position.Y, 32, 32), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, item.parentSheetIndex, 16, 16), 2f)
				{
					myID = 0,
					name = item.DisplayName,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					region = 502
				};
				_components.Add(component);
				_context.RegisterClickable(component);
			}
		}

		public override float HandleLayout(float draw_y, Rectangle content_rectangle, int index)
		{
			_emptyBoxPositions.Clear();
			draw_y = base.HandleLayout(draw_y, content_rectangle, index);
			int draw_x = 0;
			int lowest_drawn_position = (int)draw_y;
			Point padding = new Point(4, 4);
			for (int i = 0; i < _components.Count; i++)
			{
				ClickableTextureComponent component = _components[i];
				if (draw_x + component.bounds.Width + padding.Y > content_rectangle.Width)
				{
					draw_x = 0;
					draw_y += (float)(component.bounds.Height + padding.Y);
				}
				component.bounds.X = content_rectangle.Left + draw_x;
				component.bounds.Y = (int)draw_y;
				draw_x += component.bounds.Width + padding.X;
				lowest_drawn_position = Math.Max((int)draw_y + component.bounds.Height, lowest_drawn_position);
			}
			for (; draw_x + 32 + padding.X <= content_rectangle.Width; draw_x += 32 + padding.X)
			{
				_emptyBoxPositions.Add(new Vector2(content_rectangle.Left + draw_x, draw_y));
			}
			return lowest_drawn_position + 8;
		}

		public override void DrawItem(SpriteBatch b)
		{
			for (int j = 0; j < _components.Count; j++)
			{
				ClickableTextureComponent component = _components[j];
				b.Draw(Game1.menuTexture, new Rectangle(component.bounds.X, component.bounds.Y, 32, 32), new Rectangle(64, 128, 64, 64), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				b.Draw(Game1.menuTexture, new Rectangle(component.bounds.X, component.bounds.Y, 32, 32), new Rectangle(128, 128, 64, 64), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				_components[j].draw(b, Color.White, 4.1E-05f);
				if (Game1.player.hasItemInInventory(_items[j].parentSheetIndex, 1))
				{
					b.Draw(Game1.mouseCursors, new Rectangle(_components[j].bounds.X + 32 - 11, _components[j].bounds.Y + 32 - 13, 11, 13), new Rectangle(268, 1436, 11, 13), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4E-05f);
				}
			}
			for (int i = 0; i < _emptyBoxPositions.Count; i++)
			{
				b.Draw(Game1.menuTexture, new Rectangle((int)_emptyBoxPositions[i].X, (int)_emptyBoxPositions[i].Y, 32, 32), new Rectangle(64, 896, 64, 64), Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
				b.Draw(Game1.menuTexture, new Rectangle((int)_emptyBoxPositions[i].X, (int)_emptyBoxPositions[i].Y, 32, 32), new Rectangle(128, 128, 64, 64), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 4.3E-05f);
			}
		}

		public override void performHover(int x, int y)
		{
			for (int i = 0; i < _components.Count; i++)
			{
				if (_components[i].bounds.Contains(new Point(x, y)))
				{
					_context.hoveredItem = _items[i];
				}
			}
		}

		public override bool ShouldDraw()
		{
			return _items.Count > 0;
		}
	}
}
