using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	public class ReadyCheckDialog : ConfirmationDialog
	{
		public string checkName;

		private bool allowCancel;

		public ReadyCheckDialog(string checkName, bool allowCancel, behavior onConfirm = null, behavior onCancel = null)
			: base(Game1.content.LoadString("Strings\\UI:ReadyCheck", "N", "M"), onConfirm, onCancel)
		{
			this.checkName = checkName;
			this.allowCancel = allowCancel;
			okButton.visible = false;
			cancelButton.visible = isCancelable();
			updateMessage();
			exitFunction = delegate
			{
				closeDialog(Game1.player);
			};
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public bool isCancelable()
		{
			if (allowCancel)
			{
				return Game1.player.team.IsReadyCheckCancelable(checkName);
			}
			return false;
		}

		public override bool readyToClose()
		{
			return isCancelable();
		}

		public override void closeDialog(Farmer who)
		{
			base.closeDialog(who);
			if (isCancelable())
			{
				Game1.player.team.SetLocalReady(checkName, ready: false);
			}
		}

		private void updateMessage()
		{
			int readyNum = Game1.player.team.GetNumberReady(checkName);
			int requiredNum = Game1.player.team.GetNumberRequired(checkName);
			message = Game1.content.LoadString("Strings\\UI:ReadyCheck", readyNum, requiredNum);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			cancelButton.visible = isCancelable();
			updateMessage();
			Game1.player.team.SetLocalReady(checkName, ready: true);
			if (Game1.player.team.IsReady(checkName))
			{
				confirm();
			}
		}
	}
}
