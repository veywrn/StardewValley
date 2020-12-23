using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.GameData.Crafting;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class TailorRecipeListTool : IClickableMenu
	{
		public Rectangle scrollView;

		public List<ClickableTextureComponent> recipeComponents;

		public ClickableTextureComponent okButton;

		public float scrollY;

		public Dictionary<string, KeyValuePair<Item, Item>> _recipeLookup;

		public Item hoveredItem;

		public string metadata = "";

		public Dictionary<string, string> _recipeMetadata;

		public Dictionary<string, Color> _recipeColors;

		public TailorRecipeListTool()
			: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
		{
			TailoringMenu tailoring_menu = new TailoringMenu();
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			recipeComponents = new List<ClickableTextureComponent>();
			_recipeLookup = new Dictionary<string, KeyValuePair<Item, Item>>();
			_recipeMetadata = new Dictionary<string, string>();
			_recipeColors = new Dictionary<string, Color>();
			Item cloth = new Object(Vector2.Zero, 428, 1);
			foreach (int id in Game1.objectInformation.Keys)
			{
				Item key = new Object(Vector2.Zero, id, 1);
				if (!key.Name.Contains("Seeds") && !key.Name.Contains("Floor") && !key.Name.Equals("Stone") && !key.Name.Contains("Weeds") && !key.Name.Equals("Lumber") && !key.Name.Contains("Fence") && !key.Name.Equals("Gate") && !key.Name.Contains("Starter") && !key.Name.Contains("Twig") && !key.Name.Equals("Secret Note") && !key.Name.Contains("Guide") && !key.Name.Contains("Path") && !key.Name.Contains("Ring") && (int)key.category != -22 && !key.Name.Contains("Sapling"))
				{
					Item value = tailoring_menu.CraftItem(cloth, key);
					TailorItemRecipe recipe = tailoring_menu.GetRecipeForItems(cloth, key);
					KeyValuePair<Item, Item> kvp = new KeyValuePair<Item, Item>(key, value);
					_recipeLookup[Utility.getStandardDescriptionFromItem(key, 1)] = kvp;
					string metadata = "";
					Color? dye_color = TailoringMenu.GetDyeColor(key);
					if (dye_color.HasValue)
					{
						_recipeColors[Utility.getStandardDescriptionFromItem(key, 1)] = dye_color.Value;
					}
					if (recipe != null)
					{
						metadata = "clothes id: " + recipe.CraftedItemID + " from ";
						foreach (string context_tag in recipe.SecondItemTags)
						{
							metadata = metadata + context_tag + " ";
						}
						metadata.Trim();
					}
					_recipeMetadata[Utility.getStandardDescriptionFromItem(key, 1)] = metadata;
					ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, default(Rectangle), 1f)
					{
						myID = 0,
						name = Utility.getStandardDescriptionFromItem(key, 1),
						label = key.DisplayName
					};
					recipeComponents.Add(component);
				}
			}
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			RepositionElements();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			RepositionElements();
		}

		private void RepositionElements()
		{
			scrollView = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, width - IClickableMenu.borderWidth, 500);
			if (scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
			{
				int size_difference2 = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - scrollView.Left;
				scrollView.X += size_difference2;
				scrollView.Width -= size_difference2;
			}
			if (scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
			{
				int size_difference3 = scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
				scrollView.X -= size_difference3;
				scrollView.Width -= size_difference3;
			}
			if (scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
			{
				int size_difference4 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - scrollView.Top;
				scrollView.Y += size_difference4;
				scrollView.Width -= size_difference4;
			}
			if (scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
			{
				int size_difference = scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
				scrollView.Y -= size_difference;
				scrollView.Width -= size_difference;
			}
			RepositionScrollElements();
		}

		public void RepositionScrollElements()
		{
			int y_offset = (int)scrollY;
			if (scrollY > 0f)
			{
				scrollY = 0f;
			}
			foreach (ClickableTextureComponent component in recipeComponents)
			{
				component.bounds.X = scrollView.X;
				component.bounds.Y = scrollView.Y + y_offset;
				y_offset += component.bounds.Height;
				if (scrollView.Intersects(component.bounds))
				{
					component.visible = true;
				}
				else
				{
					component.visible = false;
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			snapCursorToCurrentSnappedComponent();
		}

		public override void gamePadButtonHeld(Buttons b)
		{
			base.gamePadButtonHeld(b);
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			_ = currentlySnappedComponent;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in recipeComponents)
			{
				if (component.bounds.Contains(x, y) && scrollView.Contains(x, y))
				{
					try
					{
						int index = Convert.ToInt32(_recipeMetadata[component.name].Split(' ')[2]);
						if (index >= 2000)
						{
							Game1.player.addItemToInventoryBool(new Hat(index - 2000));
						}
						else
						{
							Clothing o = new Clothing(index);
							if (_recipeColors.ContainsKey(component.name))
							{
								o.Dye(_recipeColors[component.name], 1f);
							}
							Game1.player.addItemToInventoryBool(o);
						}
					}
					catch (Exception)
					{
					}
				}
			}
			if (okButton.containsPoint(x, y))
			{
				exitThisMenu();
			}
		}

		public override void leftClickHeld(int x, int y)
		{
		}

		public override void releaseLeftClick(int x, int y)
		{
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void receiveKeyPress(Keys key)
		{
		}

		public override void receiveScrollWheelAction(int direction)
		{
			scrollY += direction;
			RepositionScrollElements();
			base.receiveScrollWheelAction(direction);
		}

		public override void performHoverAction(int x, int y)
		{
			hoveredItem = null;
			metadata = "";
			foreach (ClickableTextureComponent component in recipeComponents)
			{
				if (component.containsPoint(x, y))
				{
					hoveredItem = _recipeLookup[component.name].Value;
					metadata = _recipeMetadata[component.name];
				}
			}
		}

		public bool canLeaveMenu()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe2 = false;
			ignoreTitleSafe2 = true;
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe2);
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			b.GraphicsDevice.ScissorRectangle = scrollView;
			foreach (ClickableTextureComponent component in recipeComponents)
			{
				if (component.visible)
				{
					drawHorizontalPartition(b, component.bounds.Bottom - 32, small: true);
					KeyValuePair<Item, Item> kvp = _recipeLookup[component.name];
					component.draw(b);
					kvp.Key.drawInMenu(b, new Vector2(component.bounds.X, component.bounds.Y), 1f);
					if (_recipeColors.ContainsKey(component.name))
					{
						int size = 24;
						b.Draw(Game1.staminaRect, new Rectangle(scrollView.Left + scrollView.Width / 2 - size / 2, component.bounds.Center.Y - size / 2, size, size), _recipeColors[component.name]);
					}
					if (kvp.Value != null)
					{
						kvp.Value.drawInMenu(b, new Vector2(scrollView.Left + scrollView.Width - 128, component.bounds.Y), 1f);
					}
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			okButton.draw(b);
			drawMouse(b);
			if (hoveredItem != null)
			{
				Utility.drawTextWithShadow(b, metadata, Game1.smallFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth, yPositionOnScreen + height - 64), Color.Black);
				if (!Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
				}
			}
		}

		public override void update(GameTime time)
		{
		}
	}
}
