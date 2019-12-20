using System;

namespace StardewValley
{
	public class CharacterEventArgs : EventArgs
	{
		private readonly char character;

		private readonly int lParam;

		public char Character => character;

		public int Param => lParam;

		public int RepeatCount => lParam & 0xFFFF;

		public bool ExtendedKey => (lParam & 0x1000000) > 0;

		public bool AltPressed => (lParam & 0x20000000) > 0;

		public bool PreviousState => (lParam & 0x40000000) > 0;

		public bool TransitionState => (lParam & int.MinValue) > 0;

		public CharacterEventArgs(char character, int lParam)
		{
			this.character = character;
			this.lParam = lParam;
		}
	}
}
