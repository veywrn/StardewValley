using System.Threading;
using System.Windows;

namespace StardewValley
{
	public class DesktopClipboard
	{
		public static bool _availabilityChecked;

		public static bool _isAvailable;

		public static bool IsAvailable
		{
			get
			{
				if (!_availabilityChecked)
				{
					_availabilityChecked = true;
					string temp = "";
					if (GetText(ref temp))
					{
						_isAvailable = true;
					}
					else
					{
						_isAvailable = false;
					}
				}
				return _isAvailable;
			}
		}

		public static bool GetText(ref string output)
		{
			if (Clipboard.ContainsText())
			{
				bool success = false;
				string clipboard_text = "";
				Thread thread = new Thread((ThreadStart)delegate
				{
					for (int i = 0; i < 10; i++)
					{
						try
						{
							clipboard_text = Clipboard.GetText();
							success = true;
							return;
						}
						catch
						{
						}
					}
				});
				thread.SetApartmentState(ApartmentState.STA);
				thread.Start();
				thread.Join();
				output = clipboard_text;
			}
			else
			{
				output = "";
			}
			return true;
		}

		public static bool SetText(string text)
		{
			bool success = false;
			Thread thread = new Thread((ThreadStart)delegate
			{
				for (int i = 0; i < 10; i++)
				{
					try
					{
						Clipboard.SetText(text);
						success = true;
						return;
					}
					catch
					{
					}
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.Start();
			thread.Join();
			return success;
		}
	}
}
