using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class TextEntryMenu : IClickableMenu
	{
		public const int borderSpace = 4;

		public const int buttonSize = 16;

		public const int windowWidth = 168;

		public const int windowHeight = 88;

		public string[][] letterMaps = new string[3][]
		{
			new string[4]
			{
				"1234567890",
				"qwertyuiop",
				"asdfghjkl'",
				"zxcvbnm,.?"
			},
			new string[4]
			{
				"!@#$%^&*()",
				"QWERTYUIOP",
				"ASDFGHJKL\"",
				"ZXCVBNM,.?"
			},
			new string[4]
			{
				"&%#|~$£~/\\",
				"-+=<>:;'\"`",
				"()[]{}.^°ñ",
				"áéíóúü¡!¿?"
			}
		};

		public List<ClickableTextureComponent> keys = new List<ClickableTextureComponent>();

		public ClickableTextureComponent backspaceButton;

		public ClickableTextureComponent spaceButton;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent upperCaseButton;

		public ClickableTextureComponent symbolsButton;

		protected int _lettersPerRow;

		protected TextBox _target;

		public int _currentKeyboard;

		public override void receiveGamePadButton(Buttons b)
		{
			switch (b)
			{
			case Buttons.Y:
				OnSpaceBar();
				break;
			case Buttons.X:
				OnBackSpace();
				break;
			case Buttons.B:
				Close();
				break;
			case Buttons.Start:
				OnSubmit();
				break;
			default:
				base.receiveGamePadButton(b);
				break;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			if (key == Keys.Escape)
			{
				Close();
			}
			base.receiveKeyPress(key);
		}

		public TextEntryMenu(TextBox target)
			: base((int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).Y, 672, 352)
		{
			_target = target;
			_lettersPerRow = letterMaps[0][0].Length;
			for (int j = 0; j < letterMaps[0].Length; j++)
			{
				for (int i = 0; i < _lettersPerRow; i++)
				{
					ClickableTextureComponent key_component = new ClickableTextureComponent(new Rectangle(0, 0, 1024, 1024), Game1.mouseCursors2, new Rectangle(32, 176, 16, 16), 4f)
					{
						myID = j * _lettersPerRow + i,
						leftNeighborID = -99998,
						rightNeighborID = -99998,
						upNeighborID = -99998,
						downNeighborID = -99998
					};
					if (j == letterMaps[0].Length - 1)
					{
						if (i >= 2 && i <= _lettersPerRow - 4)
						{
							key_component.downNeighborID = 99991;
							key_component.downNeighborImmutable = true;
						}
						if (i >= _lettersPerRow - 3 && i <= _lettersPerRow - 2)
						{
							key_component.downNeighborID = 99990;
							key_component.downNeighborImmutable = true;
						}
					}
					keys.Add(key_component);
				}
			}
			backspaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 128, 64), Game1.mouseCursors2, new Rectangle(32, 144, 32, 16), 4f)
			{
				myID = 99990,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			spaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 320, 64), Game1.mouseCursors2, new Rectangle(0, 160, 80, 16), 4f)
			{
				myID = 99991,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			okButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 144, 16, 16), 4f)
			{
				myID = 99992,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			upperCaseButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 144, 16, 16), 4f)
			{
				myID = 99993,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			symbolsButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 144, 16, 16), 4f)
			{
				myID = 99994,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998
			};
			ShowKeyboard(0, play_sound: false);
			RepositionElements();
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				snapToDefaultClickableComponent();
			}
			Game1.playSound("bigSelect");
		}

		public override bool readyToClose()
		{
			return false;
		}

		public void ShowKeyboard(int index, bool play_sound = true)
		{
			_currentKeyboard = index;
			int key_index = 0;
			string[] array = letterMaps[index];
			foreach (string key_map in array)
			{
				foreach (char key_character in key_map)
				{
					keys[key_index].name = (key_character.ToString() ?? "");
					key_index++;
				}
			}
			upperCaseButton.sourceRect = new Rectangle(0, 144, 16, 16);
			symbolsButton.sourceRect = new Rectangle(16, 144, 16, 16);
			if (_currentKeyboard == 1)
			{
				upperCaseButton.sourceRect = new Rectangle(0, 176, 16, 16);
			}
			else if (_currentKeyboard == 2)
			{
				symbolsButton.sourceRect = new Rectangle(16, 176, 16, 16);
			}
			if (play_sound)
			{
				Game1.playSound("button1");
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(_lettersPerRow);
			snapCursorToCurrentSnappedComponent();
		}

		public void RepositionElements()
		{
			xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).X;
			yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 256).Y;
			for (int y = 0; y < keys.Count / _lettersPerRow; y++)
			{
				for (int x = 0; x < _lettersPerRow; x++)
				{
					keys[x + y * _lettersPerRow].bounds = new Rectangle(xPositionOnScreen + 16 + x * 16 * 4, yPositionOnScreen + 16 + y * 16 * 4, 64, 64);
				}
			}
			upperCaseButton.bounds = new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + 256, upperCaseButton.bounds.Width, upperCaseButton.bounds.Height);
			symbolsButton.bounds = new Rectangle(xPositionOnScreen + 16 + 64, yPositionOnScreen + 16 + 256, symbolsButton.bounds.Width, symbolsButton.bounds.Height);
			backspaceButton.bounds = new Rectangle(xPositionOnScreen + 16 + 448, yPositionOnScreen + 16 + 256, backspaceButton.bounds.Width, backspaceButton.bounds.Height);
			spaceButton.bounds = new Rectangle(xPositionOnScreen + 16 + 128, yPositionOnScreen + 16 + 256, spaceButton.bounds.Width, spaceButton.bounds.Height);
			okButton.bounds = new Rectangle(xPositionOnScreen + 16 + 576, yPositionOnScreen + 16 + 256, okButton.bounds.Width, okButton.bounds.Height);
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			base.gameWindowSizeChanged(oldBounds, newBounds);
			RepositionElements();
		}

		public override void performHoverAction(int x, int y)
		{
			base.performHoverAction(x, y);
			foreach (ClickableTextureComponent key in keys)
			{
				key.tryHover(x, y);
			}
			spaceButton.tryHover(x, y);
			backspaceButton.tryHover(x, y);
			okButton.tryHover(x, y);
			symbolsButton.tryHover(x, y);
			upperCaseButton.tryHover(x, y);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			foreach (ClickableTextureComponent component in keys)
			{
				if (component.containsPoint(x, y))
				{
					OnLetter(component.name);
				}
			}
			if (okButton.containsPoint(x, y))
			{
				OnSubmit();
				return;
			}
			if (spaceButton.containsPoint(x, y))
			{
				OnSpaceBar();
			}
			if (upperCaseButton.containsPoint(x, y))
			{
				if (_currentKeyboard != 1)
				{
					ShowKeyboard(1);
				}
				else
				{
					ShowKeyboard(0);
				}
			}
			if (symbolsButton.containsPoint(x, y))
			{
				if (_currentKeyboard != 2)
				{
					ShowKeyboard(2);
				}
				else
				{
					ShowKeyboard(0);
				}
			}
			if (backspaceButton.containsPoint(x, y))
			{
				OnBackSpace();
			}
		}

		public void OnSubmit()
		{
			_target.RecieveCommandInput('\r');
			Close();
		}

		public void OnSpaceBar()
		{
			_target.RecieveTextInput(' ');
		}

		public void OnBackSpace()
		{
			_target.RecieveCommandInput('\b');
		}

		public void OnLetter(string letter)
		{
			if (letter.Length > 0)
			{
				_target.RecieveTextInput(letter[0]);
			}
		}

		public void Close()
		{
			Game1.playSound("bigDeSelect");
			Game1.closeTextEntry();
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
			Game1.DrawBox(xPositionOnScreen, yPositionOnScreen, width, height);
			foreach (ClickableTextureComponent key in keys)
			{
				key.draw(b);
				Vector2 size = Game1.dialogueFont.MeasureString(key.name);
				b.DrawString(Game1.dialogueFont, key.name, Utility.snapDrawPosition(new Vector2((float)key.bounds.Center.X - size.X / 2f, (float)key.bounds.Center.Y - size.Y / 2f)), Color.Black);
			}
			backspaceButton.draw(b);
			okButton.draw(b);
			spaceButton.draw(b);
			symbolsButton.draw(b);
			upperCaseButton.draw(b);
			if (_target != null)
			{
				int x = _target.X;
				int y = _target.Y;
				_target.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(_target.Width, _target.Height * 4).X;
				_target.Y = yPositionOnScreen - 96;
				_target.Draw(b);
				_target.X = x;
				_target.Y = y;
			}
			base.draw(b);
			drawMouse(b, ignore_transparency: true);
		}

		public override void update(GameTime time)
		{
			if (_target == null || !_target.Selected)
			{
				Close();
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftStick) && !Game1.oldPadState.IsButtonDown(Buttons.LeftStick))
			{
				if (_currentKeyboard != 1)
				{
					ShowKeyboard(1);
				}
				else
				{
					ShowKeyboard(0);
				}
			}
			if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && !Game1.oldPadState.IsButtonDown(Buttons.RightStick))
			{
				if (_currentKeyboard != 2)
				{
					ShowKeyboard(2);
				}
				else
				{
					ShowKeyboard(0);
				}
			}
		}
	}
}
