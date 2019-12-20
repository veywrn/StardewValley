namespace StardewValley
{
	public class NPCDialogueResponse : Response
	{
		public int friendshipChange;

		public int id;

		public NPCDialogueResponse(int id, int friendshipChange, string keyToNPCresponse, string responseText)
			: base(keyToNPCresponse, responseText)
		{
			this.friendshipChange = friendshipChange;
			this.id = id;
		}
	}
}
