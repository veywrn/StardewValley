using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public struct InputButton
	{
		public Keys key;

		public bool mouseLeft;

		public bool mouseRight;

		public InputButton(Keys key)
		{
			this.key = key;
			mouseLeft = false;
			mouseRight = false;
		}

		public InputButton(bool mouseLeft)
		{
			key = Keys.None;
			this.mouseLeft = mouseLeft;
			mouseRight = !mouseLeft;
		}

		public override string ToString()
		{
			if (mouseLeft)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Left-Click");
			}
			if (mouseRight)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Right-Click");
			}
			switch (key)
			{
			case Keys.D1:
				return "1";
			case Keys.D2:
				return "2";
			case Keys.D3:
				return "3";
			case Keys.D4:
				return "4";
			case Keys.D5:
				return "5";
			case Keys.D6:
				return "6";
			case Keys.D7:
				return "7";
			case Keys.D8:
				return "8";
			case Keys.D9:
				return "9";
			case Keys.D0:
				return "0";
			default:
			{
				string retVal = key.ToString().Replace("Oem", "");
				if (Game1.content.LoadString("Strings\\StringsFromCSFiles:" + key.ToString().Replace("Oem", "")) != "Strings\\StringsFromCSFiles:" + key.ToString().Replace("Oem", ""))
				{
					retVal = Game1.content.LoadString("Strings\\StringsFromCSFiles:" + key.ToString().Replace("Oem", ""));
				}
				return retVal;
			}
			}
		}
	}
}
