namespace StardewValley
{
	public class MailActivity : FarmActivity
	{
		public override bool AttemptActivity(Farm farm)
		{
			if (_character.getSpouse() != null)
			{
				activityPosition = Utility.PointToVector2(_character.getSpouse().getMailboxPosition());
			}
			else
			{
				activityPosition = Utility.PointToVector2(Game1.MasterPlayer.getMailboxPosition());
			}
			activityPosition.Y += 1f;
			activityDirection = 0;
			return true;
		}
	}
}
