namespace StardewValley.Util
{
	public class ToolSpamInputSimulator : IInputSimulator
	{
		private bool pressedLastFrame;

		public void SimulateInput(ref bool actionButtonPressed, ref bool switchToolButtonPressed, ref bool useToolButtonPressed, ref bool useToolButtonReleased, ref bool addItemToInventoryButtonPressed, ref bool cancelButtonPressed, ref bool moveUpPressed, ref bool moveRightPressed, ref bool moveLeftPressed, ref bool moveDownPressed, ref bool moveUpReleased, ref bool moveRightReleased, ref bool moveLeftReleased, ref bool moveDownReleased, ref bool moveUpHeld, ref bool moveRightHeld, ref bool moveLeftHeld, ref bool moveDownHeld)
		{
			useToolButtonPressed = pressedLastFrame;
			useToolButtonReleased = !pressedLastFrame;
			pressedLastFrame = !pressedLastFrame;
		}
	}
}
