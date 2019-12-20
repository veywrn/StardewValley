using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley.Util
{
	public class EventTest
	{
		private int currentEventIndex;

		private int currentLocationIndex;

		private int aButtonTimer;

		private List<string> specificEventsToDo = new List<string>();

		private bool doingSpecifics;

		public EventTest(string startingLocationName = "", int startingEventIndex = 0)
		{
			currentLocationIndex = 0;
			if (startingLocationName.Length > 0)
			{
				for (int i = 0; i < Game1.locations.Count(); i++)
				{
					if (Game1.locations[i].Name.Equals(startingLocationName))
					{
						currentLocationIndex = i;
						break;
					}
				}
			}
			currentEventIndex = startingEventIndex;
		}

		public EventTest(string[] whichEvents)
		{
			for (int i = 1; i < whichEvents.Count(); i += 2)
			{
				specificEventsToDo.Add(whichEvents[i] + " " + whichEvents[i + 1]);
			}
			doingSpecifics = true;
			currentLocationIndex = -1;
		}

		public void update()
		{
			if (!Game1.eventUp && !Game1.fadeToBlack)
			{
				if (currentLocationIndex >= Game1.locations.Count)
				{
					return;
				}
				if (doingSpecifics && currentLocationIndex == -1)
				{
					if (specificEventsToDo.Count == 0)
					{
						return;
					}
					for (int j = 0; j < Game1.locations.Count(); j++)
					{
						if (!Game1.locations[j].Name.Equals(specificEventsToDo.Last().Split(' ')[0]))
						{
							continue;
						}
						currentLocationIndex = j;
						Dictionary<string, string> events = Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + Game1.locations[j].Name);
						for (int l = 0; l < events.Count; l++)
						{
							int result = -1;
							if (int.TryParse(events.ElementAt(l).Key.Split('/')[0], out result) && result == Convert.ToInt32(specificEventsToDo.Last().Split(' ')[1]))
							{
								currentEventIndex = l;
								break;
							}
						}
						specificEventsToDo.Remove(specificEventsToDo.Last());
						break;
					}
				}
				GameLocation k = Game1.locations[currentLocationIndex];
				if (k.currentEvent != null)
				{
					return;
				}
				string locationName = k.name;
				string a = locationName;
				if (a == "Pool")
				{
					locationName = "BathHouse_Pool";
				}
				bool exists = true;
				try
				{
					Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName);
				}
				catch (Exception)
				{
					exists = false;
				}
				if (exists && currentEventIndex < Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).Count && Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(currentEventIndex).Key.Contains('/') && !Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(currentEventIndex).Value.Equals("null"))
				{
					if (Game1.currentLocation.Name.Equals(locationName))
					{
						Game1.eventUp = true;
						Game1.currentLocation.currentEvent = new Event(Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(currentEventIndex).Value);
					}
					else
					{
						LocationRequest locationRequest = Game1.getLocationRequest(locationName);
						int i = currentEventIndex;
						locationRequest.OnLoad += delegate
						{
							Game1.currentLocation.currentEvent = new Event(Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).ElementAt(i).Value);
						};
						Game1.warpFarmer(locationRequest, 8, 8, Game1.player.FacingDirection);
					}
				}
				currentEventIndex++;
				if (!exists || currentEventIndex >= Game1.content.Load<Dictionary<string, string>>("Data\\Events\\" + locationName).Count)
				{
					currentEventIndex = 0;
					currentLocationIndex++;
				}
				if (doingSpecifics)
				{
					currentLocationIndex = -1;
				}
				return;
			}
			aButtonTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (aButtonTimer < 0)
			{
				aButtonTimer = 100;
				if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
				{
					(Game1.activeClickableMenu as DialogueBox).performHoverAction(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
					(Game1.activeClickableMenu as DialogueBox).receiveLeftClick(Game1.graphics.GraphicsDevice.Viewport.Width / 2, Game1.graphics.GraphicsDevice.Viewport.Height - 64 - Game1.random.Next(300));
				}
			}
		}
	}
}
