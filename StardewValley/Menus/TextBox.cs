using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.SDKs;
using System;

namespace StardewValley.Menus
{
	public class TextBox : IKeyboardSubscriber
	{
		protected Texture2D _textBoxTexture;

		protected Texture2D _caretTexture;

		protected SpriteFont _font;

		protected Color _textColor;

		public bool numbersOnly;

		public int textLimit = -1;

		public bool limitWidth = true;

		private string _text = "";

		private bool _showKeyboard;

		private bool _selected;

		public SpriteFont Font => _font;

		public Color TextColor => _textColor;

		public int X
		{
			get;
			set;
		}

		public int Y
		{
			get;
			set;
		}

		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		public bool PasswordBox
		{
			get;
			set;
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value;
				if (_text == null)
				{
					_text = "";
				}
				if (!(_text != ""))
				{
					return;
				}
				string filtered = "";
				for (int i = 0; i < value.Length; i++)
				{
					char c = value[i];
					if (_font.Characters.Contains(c))
					{
						filtered += c.ToString();
					}
				}
				if (limitWidth && _font.MeasureString(_text).X > (float)(Width - 21))
				{
					Text = _text.Substring(0, _text.Length - 1);
				}
			}
		}

		public string TitleText
		{
			get;
			set;
		}

		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				if (_selected == value)
				{
					return;
				}
				Console.WriteLine("TextBox.Selected is now '{0}'.", value);
				_selected = value;
				if (_selected)
				{
					Game1.keyboardDispatcher.Subscriber = this;
					_showKeyboard = true;
					return;
				}
				_showKeyboard = false;
				if (Program.sdk is SteamHelper && (Program.sdk as SteamHelper).active)
				{
					(Program.sdk as SteamHelper).CancelKeyboard();
				}
				if (Game1.keyboardDispatcher.Subscriber == this)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
			}
		}

		public event TextBoxEvent OnEnterPressed;

		public event TextBoxEvent OnTabPressed;

		public event TextBoxEvent OnBackspacePressed;

		public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
		{
			_textBoxTexture = textBoxTexture;
			if (textBoxTexture != null)
			{
				Width = textBoxTexture.Width;
				Height = textBoxTexture.Height;
			}
			_caretTexture = caretTexture;
			_font = font;
			_textColor = textColor;
		}

		public void SelectMe()
		{
			Selected = true;
		}

		public void Update()
		{
			Game1.input.GetMouseState();
			Point mousePoint = new Point(Game1.getMouseX(), Game1.getMouseY());
			if (new Rectangle(X, Y, Width, Height).Contains(mousePoint))
			{
				Selected = true;
			}
			else
			{
				Selected = false;
			}
			if (_showKeyboard)
			{
				if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
				{
					Game1.showTextEntry(this);
				}
				_showKeyboard = false;
			}
		}

		public virtual void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
		{
			bool caretVisible2 = true;
			caretVisible2 = ((!(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 < 500.0)) ? true : false);
			string toDraw = Text;
			if (PasswordBox)
			{
				toDraw = "";
				for (int i = 0; i < Text.Length; i++)
				{
					toDraw += "•";
				}
			}
			if (_textBoxTexture != null)
			{
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X, Y, 16, Height), new Rectangle(0, 0, 16, Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X + 16, Y, Width - 32, Height), new Rectangle(16, 0, 4, Height), Color.White);
				spriteBatch.Draw(_textBoxTexture, new Rectangle(X + Width - 16, Y, 16, Height), new Rectangle(_textBoxTexture.Bounds.Width - 16, 0, 16, Height), Color.White);
			}
			else
			{
				Game1.drawDialogueBox(X - 32, Y - 112 + 10, Width + 80, Height, speaker: false, drawOnlyBox: true);
			}
			Vector2 size = _font.MeasureString(toDraw);
			while (size.X > (float)Width)
			{
				toDraw = toDraw.Substring(1);
				size = _font.MeasureString(toDraw);
			}
			if (caretVisible2 && Selected)
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle(X + 16 + (int)size.X + 2, Y + 8, 4, 32), _textColor);
			}
			if (drawShadow)
			{
				Utility.drawTextWithShadow(spriteBatch, toDraw, _font, new Vector2(X + 16, Y + ((_textBoxTexture != null) ? 12 : 8)), _textColor);
			}
			else
			{
				spriteBatch.DrawString(_font, toDraw, new Vector2(X + 16, Y + ((_textBoxTexture != null) ? 12 : 8)), _textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
			}
		}

		public virtual void RecieveTextInput(char inputChar)
		{
			if (!Selected || (numbersOnly && !char.IsDigit(inputChar)) || (textLimit != -1 && Text.Length >= textLimit))
			{
				return;
			}
			if (Game1.gameMode != 3)
			{
				switch (inputChar)
				{
				case '"':
					return;
				case '+':
					Game1.playSound("slimeHit");
					break;
				case '*':
					Game1.playSound("hammer");
					break;
				case '=':
					Game1.playSound("coin");
					break;
				case '<':
					Game1.playSound("crystal");
					break;
				case '$':
					Game1.playSound("money");
					break;
				default:
					Game1.playSound("cowboy_monsterhit");
					break;
				}
			}
			Text += inputChar.ToString();
		}

		public virtual void RecieveTextInput(string text)
		{
			int dummy = -1;
			if (Selected && (!numbersOnly || int.TryParse(text, out dummy)) && (textLimit == -1 || Text.Length < textLimit))
			{
				Text += text;
			}
		}

		public virtual void RecieveCommandInput(char command)
		{
			if (!Selected)
			{
				return;
			}
			switch (command)
			{
			case '\b':
				if (Text.Length <= 0)
				{
					break;
				}
				if (this.OnBackspacePressed != null)
				{
					this.OnBackspacePressed(this);
					break;
				}
				Text = Text.Substring(0, Text.Length - 1);
				if (Game1.gameMode != 3)
				{
					Game1.playSound("tinyWhip");
				}
				break;
			case '\r':
				if (this.OnEnterPressed != null)
				{
					this.OnEnterPressed(this);
				}
				break;
			case '\t':
				if (this.OnTabPressed != null)
				{
					this.OnTabPressed(this);
				}
				break;
			}
		}

		public void RecieveSpecialInput(Keys key)
		{
		}

		public void Hover(int x, int y)
		{
			if (x > X && x < X + Width && y > Y && y < Y + Height)
			{
				Game1.SetFreeCursorDrag();
			}
		}
	}
}
