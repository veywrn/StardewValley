using Microsoft.Xna.Framework.Input;

namespace StardewValley
{
	public class Response
	{
		public string responseKey;

		public string responseText;

		public Keys hotkey;

		public Response(string responseKey, string responseText)
		{
			this.responseKey = responseKey;
			this.responseText = responseText;
		}

		public Response SetHotKey(Keys key)
		{
			hotkey = key;
			return this;
		}
	}
}
