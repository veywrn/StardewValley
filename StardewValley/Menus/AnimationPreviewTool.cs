using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StardewValley.Menus
{
	public class AnimationPreviewTool : IClickableMenu
	{
		public List<List<ClickableTextureComponent>> components;

		public Rectangle scrollView;

		public List<ClickableTextureComponent> animationButtons;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent hairLabel;

		public ClickableTextureComponent shirtLabel;

		public ClickableTextureComponent pantsLabel;

		public float scrollY;

		public AnimationPreviewTool()
			: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
		{
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			FieldInfo[] fields = typeof(FarmerSprite).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			animationButtons = new List<ClickableTextureComponent>();
			foreach (FieldInfo field in fields.Where((FieldInfo fi) => fi.IsLiteral && !fi.IsInitOnly))
			{
				ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 200, 48), null, default(Rectangle), 1f)
				{
					myID = (int)field.GetValue(null),
					name = field.Name
				};
				animationButtons.Add(component);
			}
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			components = new List<List<ClickableTextureComponent>>();
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
			{
				new ClickableTextureComponent("Hair Heading", new Rectangle(0, 0, 64, 16), "Hair", "", null, default(Rectangle), 1f)
			}));
			hairLabel = new ClickableTextureComponent("Hair Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
			{
				new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = -1
				},
				hairLabel,
				new ClickableTextureComponent("Hair Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 1
				}
			}));
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
			{
				new ClickableTextureComponent("Shirt Heading", new Rectangle(0, 0, 64, 16), "Shirt", "", null, default(Rectangle), 1f)
			}));
			shirtLabel = new ClickableTextureComponent("Shirt Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
			{
				new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = -1
				},
				shirtLabel,
				new ClickableTextureComponent("Shirt Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 1
				}
			}));
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
			{
				new ClickableTextureComponent("Pants Heading", new Rectangle(0, 0, 64, 16), "Pants", "", null, default(Rectangle), 1f)
			}));
			pantsLabel = new ClickableTextureComponent("Pants Label", new Rectangle(0, 0, 64, 64), "0", "", null, default(Rectangle), 1f);
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[3]
			{
				new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
				{
					myID = -1
				},
				pantsLabel,
				new ClickableTextureComponent("Pants Style", new Rectangle(0, 0, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
				{
					myID = 1
				}
			}));
			components.Add(new List<ClickableTextureComponent>(new ClickableTextureComponent[1]
			{
				new ClickableTextureComponent("Toggle Gender", new Rectangle(0, 0, 64, 64), "Toggle Gender", "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f)
			}));
			RepositionElements();
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			RepositionElements();
		}

		public void SwitchShirt(int direction)
		{
			Game1.player.changeShirt(Game1.player.shirt.Value + direction);
			UpdateLabels();
		}

		public void SwitchHair(int direction)
		{
			Game1.player.changeHairStyle(Game1.player.hair.Value + direction);
			UpdateLabels();
		}

		public void SwitchPants(int direction)
		{
			Game1.player.changePantStyle(Game1.player.pants.Value + direction);
			UpdateLabels();
		}

		private void RepositionElements()
		{
			scrollView = new Rectangle(xPositionOnScreen + 320, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder, 250, 500);
			if (scrollView.Left < Game1.graphics.GraphicsDevice.ScissorRectangle.Left)
			{
				int size_difference2 = Game1.graphics.GraphicsDevice.ScissorRectangle.Left - scrollView.Left;
				scrollView.X += size_difference2;
				scrollView.Width -= size_difference2;
			}
			if (scrollView.Right > Game1.graphics.GraphicsDevice.ScissorRectangle.Right)
			{
				int size_difference4 = scrollView.Right - Game1.graphics.GraphicsDevice.ScissorRectangle.Right;
				scrollView.X -= size_difference4;
				scrollView.Width -= size_difference4;
			}
			if (scrollView.Top < Game1.graphics.GraphicsDevice.ScissorRectangle.Top)
			{
				int size_difference3 = Game1.graphics.GraphicsDevice.ScissorRectangle.Top - scrollView.Top;
				scrollView.Y += size_difference3;
				scrollView.Width -= size_difference3;
			}
			if (scrollView.Bottom > Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom)
			{
				int size_difference = scrollView.Bottom - Game1.graphics.GraphicsDevice.ScissorRectangle.Bottom;
				scrollView.Y -= size_difference;
				scrollView.Width -= size_difference;
			}
			int component_y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 200;
			foreach (List<ClickableTextureComponent> component2 in components)
			{
				int component_x = xPositionOnScreen + 70;
				int max_height = 0;
				foreach (ClickableTextureComponent component in component2)
				{
					component.bounds.X = component_x;
					component.bounds.Y = component_y;
					component_x += component.bounds.Width + 8;
					max_height = Math.Max(component.bounds.Height, max_height);
				}
				component_y += max_height + 8;
			}
			RepositionScrollElements();
			UpdateLabels();
		}

		public void UpdateLabels()
		{
			pantsLabel.label = string.Concat(Game1.player.GetPantsIndex());
			shirtLabel.label = string.Concat(Game1.player.GetShirtIndex());
			hairLabel.label = string.Concat(Game1.player.getHair());
		}

		public void RepositionScrollElements()
		{
			int y_offset = (int)scrollY;
			if (scrollY > 0f)
			{
				scrollY = 0f;
			}
			foreach (ClickableTextureComponent component in animationButtons)
			{
				component.bounds.X = scrollView.X;
				component.bounds.Y = scrollView.Y + y_offset;
				component.bounds.Width = scrollView.Width;
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
			foreach (ClickableTextureComponent component2 in animationButtons)
			{
				if (component2.bounds.Contains(x, y) && scrollView.Contains(x, y))
				{
					if (component2.name.Contains("Left"))
					{
						Game1.player.faceDirection(3);
					}
					else if (component2.name.Contains("Right"))
					{
						Game1.player.faceDirection(1);
					}
					else if (component2.name.Contains("Up"))
					{
						Game1.player.faceDirection(0);
					}
					else
					{
						Game1.player.faceDirection(2);
					}
					Game1.player.completelyStopAnimatingOrDoingAction();
					Game1.player.animateOnce(component2.myID);
				}
			}
			foreach (List<ClickableTextureComponent> component3 in components)
			{
				foreach (ClickableTextureComponent component in component3)
				{
					if (component.containsPoint(x, y))
					{
						if (component.name == "Shirt Style")
						{
							SwitchShirt(component.myID);
						}
						else if (component.name == "Pants Style")
						{
							SwitchPants(component.myID);
						}
						else if (component.name == "Hair Style")
						{
							SwitchHair(component.myID);
						}
						else if (component.name == "Toggle Gender")
						{
							Game1.player.changeGender(!Game1.player.isMale.Value);
						}
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
		}

		public bool canLeaveMenu()
		{
			return true;
		}

		public override void draw(SpriteBatch b)
		{
			bool ignoreTitleSafe = false;
			ignoreTitleSafe = true;
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe);
			b.Draw(Game1.daybg, new Vector2(xPositionOnScreen + 64 + 42 - 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16), Color.White);
			Game1.player.FarmerRenderer.draw(b, Game1.player.FarmerSprite.CurrentAnimationFrame, Game1.player.FarmerSprite.CurrentFrame, Game1.player.FarmerSprite.SourceRect, new Vector2(xPositionOnScreen - 2 + 42 + 128 - 32, yPositionOnScreen + IClickableMenu.borderWidth - 16 + IClickableMenu.spaceToClearTopBorder + 32), Vector2.Zero, 0.8f, Color.White, 0f, 1f, Game1.player);
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			b.GraphicsDevice.ScissorRectangle = scrollView;
			foreach (ClickableTextureComponent component in animationButtons)
			{
				if (component.visible)
				{
					Game1.DrawBox(component.bounds.X, component.bounds.Y, component.bounds.Width, component.bounds.Height);
					Utility.drawTextWithShadow(b, component.name, Game1.smallFont, new Vector2(component.bounds.X, component.bounds.Y), Color.Black);
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			foreach (List<ClickableTextureComponent> component2 in components)
			{
				foreach (ClickableTextureComponent item in component2)
				{
					item.draw(b);
				}
			}
			okButton.draw(b);
			drawMouse(b);
		}

		public override void update(GameTime time)
		{
		}
	}
}
