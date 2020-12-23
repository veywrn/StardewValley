using System.Collections.Generic;

namespace StardewValley.Quests
{
	public interface IQuest
	{
		string GetName();

		string GetDescription();

		List<string> GetObjectiveDescriptions();

		bool CanBeCancelled();

		void MarkAsViewed();

		bool ShouldDisplayAsNew();

		bool ShouldDisplayAsComplete();

		bool IsTimedQuest();

		int GetDaysLeft();

		bool IsHidden();

		bool HasReward();

		bool HasMoneyReward();

		int GetMoneyReward();

		void OnMoneyRewardClaimed();

		bool OnLeaveQuestPage();
	}
}
