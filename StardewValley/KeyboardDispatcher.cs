using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Threading;
using System.Windows;

namespace StardewValley
{
	public class KeyboardDispatcher
	{
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

		public KeyboardDispatcher(GameWindow window)
		{
			KeyboardInput.Initialize(window);
			KeyboardInput.CharEntered += EventInput_CharEntered;
			KeyboardInput.KeyDown += EventInput_KeyDown;
		}

		private void Event_KeyDown(object sender, Keys key)
		{
			if (_subscriber != null)
			{
				if (key == Keys.Back)
				{
					_subscriber.RecieveCommandInput('\b');
				}
				if (key == Keys.Enter)
				{
					_subscriber.RecieveCommandInput('\r');
				}
				if (key == Keys.Tab)
				{
					_subscriber.RecieveCommandInput('\t');
				}
				_subscriber.RecieveSpecialInput(key);
			}
		}

		private void EventInput_KeyDown(object sender, KeyEventArgs e)
		{
			if (_subscriber != null)
			{
				_subscriber.RecieveSpecialInput(e.KeyCode);
			}
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
					_subscriber.RecieveTextInput(_pasteResult);
				}
				else
				{
					_subscriber.RecieveCommandInput(e.Character);
				}
			}
			else
			{
				_subscriber.RecieveTextInput(e.Character);
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
