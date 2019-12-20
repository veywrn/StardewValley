using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley
{
	public class SchedulePathDescription
	{
		public Stack<Point> route;

		public int facingDirection;

		public string endOfRouteBehavior;

		public string endOfRouteMessage;

		public SchedulePathDescription(Stack<Point> route, int facingDirection, string endBehavior, string endMessage)
		{
			endOfRouteMessage = endMessage;
			this.route = route;
			this.facingDirection = facingDirection;
			endOfRouteBehavior = endBehavior;
		}
	}
}
