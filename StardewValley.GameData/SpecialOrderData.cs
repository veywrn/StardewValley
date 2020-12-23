using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace StardewValley.GameData
{
	public class SpecialOrderData
	{
		public string Name;

		public string Requester;

		public string Duration;

		[ContentSerializer(Optional = true)]
		public string Repeatable = "False";

		[ContentSerializer(Optional = true)]
		public string RequiredTags = "";

		[ContentSerializer(Optional = true)]
		public string OrderType = "";

		[ContentSerializer(Optional = true)]
		public string SpecialRule = "";

		public string Text;

		[ContentSerializer(Optional = true)]
		public string ItemToRemoveOnEnd;

		[ContentSerializer(Optional = true)]
		public string MailToRemoveOnEnd;

		[ContentSerializer(Optional = true)]
		public List<RandomizedElement> RandomizedElements;

		public List<SpecialOrderObjectiveData> Objectives;

		public List<SpecialOrderRewardData> Rewards;
	}
}
