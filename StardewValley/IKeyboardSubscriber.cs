using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public interface IKeyboardSubscriber
	{
		bool Selected
		{
			get;
			set;
		}

		void RecieveTextInput(char inputChar);

		void RecieveTextInput(string text);

		void RecieveCommandInput(char command);

		void RecieveSpecialInput(Keys key);
	}
}
