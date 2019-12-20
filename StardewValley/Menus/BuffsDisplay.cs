using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class BuffsDisplay : IClickableMenu
	{
		public const int fullnessLength = 180000;

		public const int quenchedLength = 60000;

		private Dictionary<ClickableTextureComponent, Buff> buffs = new Dictionary<ClickableTextureComponent, Buff>();

		public Buff food;

		public Buff drink;

		public List<Buff> otherBuffs = new List<Buff>();

		public int fullnessLeft;

		public int quenchedLeft;

		public string hoverText = "";

		public BuffsDisplay()
		{
			updatePosition();
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		private void updatePosition()
		{
			Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
			int w = 288;
			int h = 64;
			int x = tsarea.Right - 320 - 32 - width;
			int y = tsarea.Top + 8;
			xPositionOnScreen = x;
			yPositionOnScreen = y;
			width = w;
			height = h;
			syncIcons();
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			foreach (KeyValuePair<ClickableTextureComponent, Buff> c in buffs)
			{
				if (c.Key.containsPoint(x, y))
				{
					hoverText = c.Key.hoverText + Environment.NewLine + c.Value.getTimeLeft();
					c.Key.scale = Math.Min(c.Key.baseScale + 0.1f, c.Key.scale + 0.02f);
					break;
				}
			}
		}

		public void arrangeTheseComponentsInThisRectangle(int rectangleX, int rectangleY, int rectangleWidthInComponentWidthUnits, int componentWidth, int componentHeight, int buffer, bool rightToLeft)
		{
			int x = 0;
			int y = 0;
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				ClickableTextureComponent c = buff.Key;
				if (rightToLeft)
				{
					c.bounds = new Rectangle(rectangleX + rectangleWidthInComponentWidthUnits * componentWidth - (x + 1) * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
				}
				else
				{
					c.bounds = new Rectangle(rectangleX + x * (componentWidth + buffer), rectangleY + y * (componentHeight + buffer), componentWidth, componentHeight);
				}
				x++;
				if (x > rectangleWidthInComponentWidthUnits)
				{
					y++;
					x %= rectangleWidthInComponentWidthUnits;
				}
			}
		}

		public void syncIcons()
		{
			buffs.Clear();
			if (food != null)
			{
				foreach (ClickableTextureComponent c3 in food.getClickableComponents())
				{
					buffs.Add(c3, food);
				}
			}
			if (drink != null)
			{
				foreach (ClickableTextureComponent c2 in drink.getClickableComponents())
				{
					buffs.Add(c2, drink);
				}
			}
			foreach (Buff b in otherBuffs)
			{
				foreach (ClickableTextureComponent c in b.getClickableComponents())
				{
					buffs.Add(c, b);
				}
			}
			arrangeTheseComponentsInThisRectangle(xPositionOnScreen, yPositionOnScreen, width / 64, 64, 64, 8, rightToLeft: true);
		}

		public bool hasBuff(int which)
		{
			foreach (Buff otherBuff in otherBuffs)
			{
				if (otherBuff.which == which)
				{
					return true;
				}
			}
			return false;
		}

		public bool tryToAddFoodBuff(Buff b, int duration)
		{
			if (b.total > 0 && fullnessLeft <= 0)
			{
				if (food != null)
				{
					food.removeBuff();
				}
				food = b;
				food.addBuff();
				syncIcons();
				return true;
			}
			return false;
		}

		public bool tryToAddDrinkBuff(Buff b)
		{
			if (b.source.Contains("Beer") || b.source.Contains("Wine") || b.source.Contains("Mead") || b.source.Contains("Pale Ale"))
			{
				addOtherBuff(new Buff(17));
			}
			else if (b.source.Equals("Oil of Garlic"))
			{
				addOtherBuff(new Buff(23));
			}
			else if (b.source.Equals("Life Elixir"))
			{
				Game1.player.health = Game1.player.maxHealth;
			}
			else if (b.source.Equals("Muscle Remedy"))
			{
				Game1.player.exhausted.Value = false;
			}
			if (b.total > 0 && quenchedLeft <= 0)
			{
				if (drink != null)
				{
					drink.removeBuff();
				}
				drink = b;
				drink.addBuff();
				syncIcons();
				return true;
			}
			return false;
		}

		public bool addOtherBuff(Buff buff)
		{
			if (buff.which != -1)
			{
				foreach (KeyValuePair<ClickableTextureComponent, Buff> kvp in buffs)
				{
					if (buff.which == kvp.Value.which)
					{
						kvp.Value.millisecondsDuration = buff.millisecondsDuration;
						kvp.Key.scale = kvp.Key.baseScale + 0.2f;
						return false;
					}
				}
			}
			otherBuffs.Add(buff);
			buff.addBuff();
			syncIcons();
			return true;
		}

		public new void update(GameTime time)
		{
			if (!Game1.wasMouseVisibleThisFrame)
			{
				hoverText = "";
			}
			if (food != null && food.update(time))
			{
				food.removeBuff();
				food = null;
				syncIcons();
			}
			if (drink != null && drink.update(time))
			{
				drink.removeBuff();
				drink = null;
				syncIcons();
			}
			for (int i = otherBuffs.Count - 1; i >= 0; i--)
			{
				if (otherBuffs[i].update(time))
				{
					otherBuffs[i].removeBuff();
					otherBuffs.RemoveAt(i);
					syncIcons();
				}
			}
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				ClickableTextureComponent c = buff.Key;
				c.scale = Math.Max(c.baseScale, c.scale - 0.01f);
			}
		}

		public void clearAllBuffs()
		{
			otherBuffs.Clear();
			if (food != null)
			{
				food.removeBuff();
				food = null;
			}
			if (drink != null)
			{
				drink.removeBuff();
				drink = null;
			}
			buffs.Clear();
		}

		public override void draw(SpriteBatch b)
		{
			updatePosition();
			foreach (KeyValuePair<ClickableTextureComponent, Buff> buff in buffs)
			{
				buff.Key.draw(b);
			}
			if (hoverText.Length != 0 && isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
	}
}
