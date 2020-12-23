using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Menus
{
	public class EmoteMenu : IClickableMenu
	{
		public Texture2D menuBackgroundTexture;

		public List<string> emotes;

		protected Point _mouseStartPosition;

		public bool _hasSelectedEmote;

		protected List<ClickableTextureComponent> _emoteButtons;

		protected string _selectedEmote;

		protected int _selectedIndex = -1;

		protected int _oldSelection;

		protected int _selectedTime;

		protected float _alpha;

		protected int _menuCloseGracePeriod = -1;

		protected int _age;

		public bool gamepadMode;

		protected int _expandTime = 200;

		protected int _expandedButtonRadius = 24;

		protected int _buttonRadius;

		public Vector2 _oldDrawPosition;

		public EmoteMenu()
		{
			menuBackgroundTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\EmoteMenu");
			width = 256;
			height = 256;
			xPositionOnScreen = (int)((float)(Game1.viewport.Width / 2) - (float)width / 2f);
			yPositionOnScreen = (int)((float)(Game1.viewport.Height / 2) - (float)height / 2f);
			emotes = new List<string>();
			foreach (string emote_string in Game1.player.GetEmoteFavorites())
			{
				emotes.Add(emote_string);
			}
			_mouseStartPosition = Game1.getMousePosition(ui_scale: false);
			_alpha = 0f;
			_menuCloseGracePeriod = 300;
			_CreateEmoteButtons();
			_SnapToPlayerPosition();
		}

		protected void _CreateEmoteButtons()
		{
			_emoteButtons = new List<ClickableTextureComponent>();
			for (int j = 0; j < emotes.Count; j++)
			{
				int emote_index = -1;
				for (int i = 0; i < Farmer.EMOTES.Count(); i++)
				{
					if (Farmer.EMOTES[i].emoteString == emotes[j])
					{
						emote_index = i;
						break;
					}
				}
				ClickableTextureComponent emote_button = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), menuBackgroundTexture, GetEmoteNonBubbleSpriteRect(emote_index), 4f);
				_emoteButtons.Add(emote_button);
			}
			_RepositionButtons();
		}

		public static Rectangle GetEmoteSpriteRect(int emote_index)
		{
			if (emote_index <= 0)
			{
				return new Rectangle(48, 0, 16, 16);
			}
			return new Rectangle(emote_index % 4 * 16 + 48, emote_index / 4 * 16, 16, 16);
		}

		public static Rectangle GetEmoteNonBubbleSpriteRect(int emote_index)
		{
			return new Rectangle(emote_index % 4 * 16, emote_index / 4 * 16, 16, 16);
		}

		public override void applyMovementKey(int direction)
		{
		}

		protected override void cleanupBeforeExit()
		{
			if (Game1.emoteMenu != null)
			{
				Game1.emoteMenu = null;
			}
			Game1.oldMouseState = Game1.input.GetMouseState();
			base.cleanupBeforeExit();
		}

		public override void performHoverAction(int x, int y)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			if (gamepadMode)
			{
				return;
			}
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				if (_emoteButtons[i].containsPoint(x, y))
				{
					_selectedEmote = emotes[i];
					_selectedIndex = i;
					if (_selectedIndex != _oldSelection)
					{
						_selectedTime = 0;
					}
					return;
				}
			}
			_selectedEmote = null;
			_selectedIndex = -1;
		}

		protected void _RepositionButtons()
		{
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				ClickableTextureComponent emote_button = _emoteButtons[i];
				float radians = Utility.Lerp(0f, (float)Math.PI * 2f, (float)i / (float)_emoteButtons.Count);
				emote_button.bounds.X = (int)((float)(xPositionOnScreen + width / 2 + (int)(Math.Cos(radians) * (double)_buttonRadius) * 4) - (float)emote_button.bounds.Width / 2f);
				emote_button.bounds.Y = (int)((float)(yPositionOnScreen + height / 2 + (int)((0.0 - Math.Sin(radians)) * (double)_buttonRadius) * 4) - (float)emote_button.bounds.Height / 2f);
			}
		}

		protected void _SnapToPlayerPosition()
		{
			if (Game1.player != null)
			{
				Vector2 player_position = Game1.player.getLocalPosition(Game1.viewport) + new Vector2((float)(-width) / 2f, (float)(-height) / 2f);
				xPositionOnScreen = (int)player_position.X + 32;
				yPositionOnScreen = (int)player_position.Y - 64;
				if (xPositionOnScreen + width > Game1.viewport.Width)
				{
					xPositionOnScreen -= xPositionOnScreen + width - Game1.viewport.Width;
				}
				if (xPositionOnScreen < 0)
				{
					xPositionOnScreen -= xPositionOnScreen;
				}
				if (yPositionOnScreen + height > Game1.viewport.Height)
				{
					yPositionOnScreen -= yPositionOnScreen + height - Game1.viewport.Height;
				}
				if (yPositionOnScreen < 0)
				{
					yPositionOnScreen -= yPositionOnScreen;
				}
				_RepositionButtons();
			}
		}

		public override void update(GameTime time)
		{
			_age += time.ElapsedGameTime.Milliseconds;
			if (_age > _expandTime)
			{
				_age = _expandTime;
			}
			if (!gamepadMode && Game1.options.gamepadControls && (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f))
			{
				gamepadMode = true;
			}
			_alpha = (float)_age / (float)_expandTime;
			_buttonRadius = (int)((float)_age / (float)_expandTime * (float)_expandedButtonRadius);
			_SnapToPlayerPosition();
			Vector2 offset = default(Vector2);
			if (gamepadMode)
			{
				_mouseStartPosition = Game1.getMousePosition(ui_scale: false);
				if (Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.X) > 0.5f || Math.Abs(Game1.input.GetGamePadState().ThumbSticks.Right.Y) > 0.5f)
				{
					_hasSelectedEmote = true;
					offset = new Vector2(Game1.input.GetGamePadState().ThumbSticks.Right.X, Game1.input.GetGamePadState().ThumbSticks.Right.Y);
					offset.Y *= -1f;
					offset.Normalize();
					float highest_dot = -1f;
					for (int j = 0; j < _emoteButtons.Count; j++)
					{
						float dot = Vector2.Dot(value2: new Vector2((float)_emoteButtons[j].bounds.Center.X - ((float)xPositionOnScreen + (float)width / 2f), (float)_emoteButtons[j].bounds.Center.Y - ((float)yPositionOnScreen + (float)height / 2f)), value1: offset);
						if (dot > highest_dot)
						{
							highest_dot = dot;
							_selectedEmote = emotes[j];
							_selectedIndex = j;
						}
					}
					_menuCloseGracePeriod = 100;
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) && _selectedIndex >= 0)
					{
						Game1.activeClickableMenu = new EmoteSelector(_selectedIndex, emotes[_selectedIndex]);
						exitThisMenuNoSound();
						return;
					}
				}
				else
				{
					if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && _menuCloseGracePeriod < 100)
					{
						_menuCloseGracePeriod = 100;
					}
					if (_menuCloseGracePeriod >= 0)
					{
						_menuCloseGracePeriod -= time.ElapsedGameTime.Milliseconds;
					}
					if (_menuCloseGracePeriod <= 0 && !Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick))
					{
						ConfirmSelection();
					}
				}
			}
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				if (_emoteButtons[i].scale > 4f)
				{
					_emoteButtons[i].scale = Utility.MoveTowards(_emoteButtons[i].scale, 4f, (float)time.ElapsedGameTime.Milliseconds / 1000f * 10f);
				}
			}
			if (_selectedEmote != null && _selectedIndex > -1)
			{
				_emoteButtons[_selectedIndex].scale = 5f;
			}
			if (_oldSelection != _selectedIndex)
			{
				_oldSelection = _selectedIndex;
				_selectedTime = 0;
			}
			_selectedTime += time.ElapsedGameTime.Milliseconds;
			base.update(time);
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			for (int i = 0; i < _emoteButtons.Count; i++)
			{
				if (_emoteButtons[i].containsPoint(x, y) && Game1.activeClickableMenu == null)
				{
					Game1.activeClickableMenu = new EmoteSelector(i, emotes[i]);
					exitThisMenuNoSound();
					return;
				}
			}
			base.receiveLeftClick(x, y, playSound);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			x = (int)Utility.ModifyCoordinateFromUIScale(x);
			y = (int)Utility.ModifyCoordinateFromUIScale(y);
			ConfirmSelection();
			base.receiveLeftClick(x, y, playSound);
		}

		public void ConfirmSelection()
		{
			if (_selectedEmote != null)
			{
				Game1.chatBox.textBoxEnter("/emote " + _selectedEmote);
			}
			exitThisMenu(playSound: false);
		}

		public override void draw(SpriteBatch b)
		{
			Game1.StartWorldDrawInUI(b);
			Color background_color = Color.White;
			background_color.A = (byte)Utility.Lerp(0f, 255f, _alpha);
			foreach (ClickableTextureComponent emoteButton in _emoteButtons)
			{
				emoteButton.draw(b, background_color, 0.86f);
			}
			if (_selectedEmote != null)
			{
				Farmer.EmoteType[] eMOTES = Farmer.EMOTES;
				foreach (Farmer.EmoteType emote_type in eMOTES)
				{
					if (emote_type.emoteString == _selectedEmote)
					{
						SpriteText.drawStringWithScrollCenteredAt(b, emote_type.displayName, xPositionOnScreen + width / 2, yPositionOnScreen + height);
						break;
					}
				}
			}
			if (_selectedIndex >= 0 && _selectedTime >= 250)
			{
				Vector2 draw_position = Utility.PointToVector2(_emoteButtons[_selectedIndex].bounds.Center);
				draw_position.X += 16f;
				if (!gamepadMode)
				{
					draw_position = Utility.PointToVector2(Game1.getMousePosition(ui_scale: false)) + new Vector2(32f, 32f);
					b.Draw(menuBackgroundTexture, draw_position, new Rectangle(64, 0, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
				}
				else
				{
					b.Draw(Game1.controllerMaps, draw_position, Utility.controllerMapSourceRect(new Rectangle(625, 260, 28, 28)), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
				}
				draw_position.X += 32f;
				b.Draw(menuBackgroundTexture, draw_position, new Rectangle(64, 16, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.99f);
			}
			Game1.EndWorldDrawInUI(b);
		}
	}
}
