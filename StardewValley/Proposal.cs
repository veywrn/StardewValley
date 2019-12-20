using Netcode;
using StardewValley.Network;

namespace StardewValley
{
	public class Proposal : INetObject<NetFields>
	{
		public readonly NetFarmerRef sender = new NetFarmerRef();

		public readonly NetFarmerRef receiver = new NetFarmerRef();

		public readonly NetEnum<ProposalType> proposalType = new NetEnum<ProposalType>(ProposalType.Gift);

		public readonly NetEnum<ProposalResponse> response = new NetEnum<ProposalResponse>(ProposalResponse.None);

		public readonly NetString responseMessageKey = new NetString();

		public readonly NetRef<Item> gift = new NetRef<Item>();

		public readonly NetBool canceled = new NetBool();

		public readonly NetBool cancelConfirmed = new NetBool();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Proposal()
		{
			NetFields.AddFields(sender.NetFields, receiver.NetFields, proposalType, response, responseMessageKey, gift, canceled, cancelConfirmed);
		}
	}
}
