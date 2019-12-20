using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StardewValley.Menus
{
	public class EmoteSelector : IClickableMenu
	{
		public Rectangle scrollView;

		public List<ClickableTextureComponent> emoteButtons;

		public ClickableTextureComponent okButton;

		public float scrollY;

		public int emoteIndex;

		protected ClickableTextureComponent _selectedEmote;

		protected ClickableTextureComponent _hoveredEmote;

		protected Texture2D emoteTexture;

		public EmoteSelector(int emote_index, string selected_emote = "")
			: base(Game1.viewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 64)
		{
			emoteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
			Game1.playSound("shwip");
			emoteIndex = emote_index;
			Game1.player.faceDirection(2);
			Game1.player.FarmerSprite.StopAnimation();
			typeof(FarmerSprite).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
			emoteButtons = new List<ClickableTextureComponent>();
			currentlySnappedComponent = null;
			for (int i = 0; i < Farmer.EMOTES.Length; i++)
			{
				Farmer.EmoteType emote_type = Farmer.EMOTES[i];
				if (!emote_type.hidden || Game1.player.performedEmotes.ContainsKey(emote_type.emoteString))
				{
					ClickableTextureComponent component = new ClickableTextureComponent(new Rectangle(0, 0, 80, 68), emoteTexture, EmoteMenu.GetEmoteNonBubbleSpriteRect(i), 4f, drawShadow: true)
					{
						leftNeighborID = -99998,
						rightNeighborID = -99998,
						upNeighborID = -99998,
						downNeighborID = -99998,
						myID = i
					};
					component.label = emote_type.displayName;
					component.name = emote_type.emoteString;
					component.drawLabelWithShadow = true;
					component.hoverText = ((emote_type.animationFrames != null) ? "animated" : "");
					emoteButtons.Add(component);
					if (currentlySnappedComponent == null)
					{
						currentlySnappedComponent = component;
					}
					if (selected_emote != "" && selected_emote == component.name)
					{
						currentlySnappedComponent = component;
						_selectedEmote = component;
					}
				}
			}
			okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998,
				myID = 1000,
				drawShadow = true
			};
			RepositionElements();
			populateClickableComponentList();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				snapToDefaultClickableComponent();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			xPositionOnScreen = Game1.viewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
			RepositionElements();
		}

		public override void performHoverAction(int x, int y)
		{
			ClickableTextureComponent oldHovered = _hoveredEmote;
			_hoveredEmote = null;
			okButton.tryHover(x, y);
			foreach (ClickableTextureComponent component in emoteButtons)
			{
				int component_width = component.bounds.Width;
				component.bounds.Width = scrollView.Width / 3;
				component.tryHover(x, y);
				if (component != _selectedEmote && component.bounds.Contains(x, y) && scrollView.Contains(x, y))
				{
					_hoveredEmote = component;
				}
				component.bounds.Width = component_width;
			}
			if (_hoveredEmote != null && _hoveredEmote != oldHovered)
			{
				Game1.playSound("shiny4");
			}
		}

		private void RepositionElements()
		{
			scrollView = new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 4, width - 128, height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder - 64 + 8);
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
			int y_offset = (int)scrollY + 4;
			if (scrollY > 0f)
			{
				scrollY = 0f;
			}
			int x_offset = 8;
			foreach (ClickableTextureComponent component in emoteButtons)
			{
				component.bounds.X = scrollView.X + x_offset;
				component.bounds.Y = scrollView.Y + y_offset;
				if (component.bounds.Bottom > scrollView.Bottom)
				{
					y_offset = 4;
					x_offset += scrollView.Width / 3;
					component.bounds.X = scrollView.X + x_offset;
					component.bounds.Y = scrollView.Y + y_offset;
				}
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

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in emoteButtons)
			{
				int component_width = component.bounds.Width;
				component.bounds.Width = scrollView.Width / 3;
				if (component.bounds.Contains(x, y) && scrollView.Contains(x, y))
				{
					component.bounds.Width = component_width;
					if (emoteIndex < Game1.player.GetEmoteFavorites().Count)
					{
						Game1.player.GetEmoteFavorites()[emoteIndex] = component.name;
					}
					exitThisMenu(playSound: false);
					Game1.playSound("drumkit6");
					if (!Game1.options.gamepadControls)
					{
						Game1.emoteMenu = new EmoteMenu();
					}
					return;
				}
				component.bounds.Width = component_width;
			}
			if (okButton.containsPoint(x, y))
			{
				exitThisMenu();
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
			IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), xPositionOnScreen - 128 - 8, yPositionOnScreen + 128 - 8, 192, 164, Color.White, 1f, drawShadow: false);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe2);
			b.End();
			Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState
			{
				ScissorTestEnable = true
			});
			b.GraphicsDevice.ScissorRectangle = scrollView;
			foreach (ClickableTextureComponent component in emoteButtons)
			{
				if (component == currentlySnappedComponent && Game1.options.gamepadControls && component != _selectedEmote && component == _hoveredEmote)
				{
					IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(64, 320, 60, 60), component.bounds.X + 64 + 8, component.bounds.Y + 8, scrollView.Width / 3 - 64 - 16, component.bounds.Height - 16, Color.White, 1f, drawShadow: false);
					Utility.drawWithShadow(b, emoteTexture, component.getVector2() - new Vector2(4f, 4f), new Rectangle(83, 0, 18, 18), Color.White, 0f, Vector2.Zero, 4f);
				}
				component.draw(b, Color.White * ((component == _selectedEmote) ? 0.4f : 1f), 0.87f);
				if (component != _selectedEmote && component.hoverText != "" && Game1.currentGameTime.TotalGameTime.Milliseconds % 500 < 250)
				{
					b.Draw(component.texture, component.getVector2(), new Rectangle(component.sourceRect.X + 80, component.sourceRect.Y, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
				}
			}
			b.End();
			b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
			if (_selectedEmote != null)
			{
				for (int i = 0; i < 8; i++)
				{
					float radians = Utility.Lerp(0f, (float)Math.PI * 2f, (float)i / 8f);
					Vector2 pos = Vector2.Zero;
					pos.X = (int)((float)(xPositionOnScreen - 64 + (int)(Math.Cos(radians) * 12.0) * 4) - 3.5f);
					pos.Y = (int)((float)(yPositionOnScreen + 192 + (int)((0.0 - Math.Sin(radians)) * 12.0) * 4) - 3.5f);
					Utility.drawWithShadow(b, emoteTexture, pos, new Rectangle(64 + ((i == emoteIndex) ? 8 : 0), 48, 8, 8), Color.White, 0f, Vector2.Zero);
				}
			}
			okButton.draw(b);
			drawMouse(b);
		}

		public override void update(GameTime time)
		{
			base.update(time);
		}

		protected override void cleanupBeforeExit()
		{
			base.cleanupBeforeExit();
			Game1.player.noMovementPause = Math.Max(Game1.player.noMovementPause, 200);
		}
	}
}
