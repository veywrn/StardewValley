using Microsoft.Xna.Framework;

namespace StardewValley.Menus
{
	public class PendingProposalDialog : ConfirmationDialog
	{
		public PendingProposalDialog()
			: base(Game1.content.LoadString("Strings\\UI:PendingProposal"), null)
		{
			okButton.visible = false;
			onCancel = cancelProposal;
			setCancelable(cancelable: true);
		}

		public void cancelProposal(Farmer who)
		{
			Proposal proposal = Game1.player.team.GetOutgoingProposal();
			if (proposal != null && proposal.receiver.Value != null && proposal.receiver.Value.isActive())
			{
				proposal.canceled.Value = true;
				message = Game1.content.LoadString("Strings\\UI:PendingProposal_Canceling");
				setCancelable(cancelable: false);
			}
		}

		public void setCancelable(bool cancelable)
		{
			cancelButton.visible = cancelable;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
		}

		public override bool readyToClose()
		{
			return false;
		}

		private bool consumesItem(ProposalType pt)
		{
			if (pt != 0)
			{
				return pt == ProposalType.Marriage;
			}
			return true;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			Proposal proposal = Game1.player.team.GetOutgoingProposal();
			if (proposal == null || proposal.receiver.Value == null || !proposal.receiver.Value.isActive())
			{
				Game1.player.team.RemoveOutgoingProposal();
				closeDialog(Game1.player);
			}
			else if (proposal.cancelConfirmed.Value && proposal.response.Value != ProposalResponse.Accepted)
			{
				Game1.player.team.RemoveOutgoingProposal();
				closeDialog(Game1.player);
			}
			else
			{
				if (proposal.response.Value == ProposalResponse.None)
				{
					return;
				}
				if (proposal.response.Value == ProposalResponse.Accepted)
				{
					if (consumesItem(proposal.proposalType))
					{
						Game1.player.reduceActiveItemByOne();
					}
					if (proposal.proposalType.Value == ProposalType.Dance)
					{
						Game1.player.dancePartner.Value = proposal.receiver.Value;
					}
					proposal.receiver.Value.doEmote(20);
				}
				Game1.player.team.RemoveOutgoingProposal();
				closeDialog(Game1.player);
				if (proposal.responseMessageKey.Value != null)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString(proposal.responseMessageKey.Value, proposal.receiver.Value.Name));
				}
			}
		}
	}
}
