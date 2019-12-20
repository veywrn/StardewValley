namespace StardewValley.Menus
{
	public class ChatSnippet
	{
		public string message;

		public int emojiIndex = -1;

		public float myLength;

		public ChatSnippet(string message, LocalizedContentManager.LanguageCode language)
		{
			this.message = message;
			myLength = ChatBox.messageFont(language).MeasureString(message).X;
		}

		public ChatSnippet(int emojiIndex)
		{
			this.emojiIndex = emojiIndex;
			myLength = 40f;
		}
	}
}
