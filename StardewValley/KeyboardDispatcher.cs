using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;

namespace StardewValley
{
	public class KeyboardDispatcher
	{
		protected string _enteredText;

		protected List<char> _commandInputs = new List<char>();

		protected List<Keys> _keysDown = new List<Keys>();

		protected List<char> _charsEntered = new List<char>();

		protected GameWindow _window;

		private IKeyboardSubscriber _subscriber;

		private string _pasteResult = "";

		public IKeyboardSubscriber Subscriber
		{
			get
			{
				return _subscriber;
			}
			set
			{
				if (_subscriber != value)
				{
					if (_subscriber != null)
					{
						_subscriber.Selected = false;
					}
					_subscriber = value;
					if (_subscriber != null)
					{
						_subscriber.Selected = true;
					}
				}
			}
		}

		public void Cleanup()
		{
			KeyboardInput.CharEntered -= EventInput_CharEntered;
			KeyboardInput.KeyDown -= EventInput_KeyDown;
			_window = null;
		}

		public KeyboardDispatcher(GameWindow window)
		{
			_commandInputs = new List<char>();
			_keysDown = new List<Keys>();
			_charsEntered = new List<char>();
			_window = window;
			if (Game1.game1.IsMainInstance)
			{
				KeyboardInput.Initialize(window);
			}
			KeyboardInput.CharEntered += EventInput_CharEntered;
			KeyboardInput.KeyDown += EventInput_KeyDown;
		}

		private void Event_KeyDown(object sender, Keys key)
		{
			if (_subscriber != null)
			{
				if (key == Keys.Back)
				{
					_commandInputs.Add('\b');
				}
				if (key == Keys.Enter)
				{
					_commandInputs.Add('\r');
				}
				if (key == Keys.Tab)
				{
					_commandInputs.Add('\t');
				}
				_keysDown.Add(key);
			}
		}

		private void EventInput_KeyDown(object sender, KeyEventArgs e)
		{
			_keysDown.Add(e.KeyCode);
		}

		private void EventInput_CharEntered(object sender, CharacterEventArgs e)
		{
			if (_subscriber == null)
			{
				return;
			}
			if (char.IsControl(e.Character))
			{
				if (e.Character == '\u0016')
				{
					Thread thread = new Thread(PasteThread);
					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();
					thread.Join();
					_enteredText = _pasteResult;
				}
				else
				{
					_commandInputs.Add(e.Character);
				}
			}
			else
			{
				_charsEntered.Add(e.Character);
			}
		}

		public bool ShouldSuppress()
		{
			return false;
		}

		public void Discard()
		{
			_enteredText = null;
			_charsEntered.Clear();
			_commandInputs.Clear();
			_keysDown.Clear();
		}

		public void Poll()
		{
			if (_enteredText != null)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					_subscriber.RecieveTextInput(_enteredText);
				}
				_enteredText = null;
			}
			if (_charsEntered.Count > 0)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					foreach (char key3 in _charsEntered)
					{
						_subscriber.RecieveTextInput(key3);
					}
				}
				_charsEntered.Clear();
			}
			if (_commandInputs.Count > 0)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					foreach (char key2 in _commandInputs)
					{
						_subscriber.RecieveCommandInput(key2);
					}
				}
				_commandInputs.Clear();
			}
			if (_keysDown.Count > 0)
			{
				if (_subscriber != null && !ShouldSuppress())
				{
					foreach (Keys key in _keysDown)
					{
						_subscriber.RecieveSpecialInput(key);
					}
				}
				_keysDown.Clear();
			}
		}

		[STAThread]
		private void PasteThread()
		{
			if (Clipboard.ContainsText())
			{
				_pasteResult = Clipboard.GetText();
			}
			else
			{
				_pasteResult = "";
			}
		}
	}
}
