using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class NPCController
	{
		public delegate void endBehavior();

		public Character puppet;

		private bool loop;

		private bool destroyAtNextTurn;

		private List<Vector2> path;

		private Vector2 target;

		private int pathIndex;

		private int pauseTime = -1;

		private int speed;

		private endBehavior behaviorAtEnd;

		private int CurrentPathX
		{
			get
			{
				if (pathIndex >= path.Count)
				{
					return 0;
				}
				return (int)path[pathIndex].X;
			}
		}

		private int CurrentPathY
		{
			get
			{
				if (pathIndex >= path.Count)
				{
					return 0;
				}
				return (int)path[pathIndex].Y;
			}
		}

		private bool MovingHorizontally => CurrentPathX != 0;

		public NPCController(Character n, List<Vector2> path, bool loop, endBehavior endBehavior = null)
		{
			if (n != null)
			{
				speed = n.speed;
				this.loop = loop;
				puppet = n;
				this.path = path;
				setMoving(newTarget: true);
				behaviorAtEnd = endBehavior;
			}
		}

		public void destroyAtNextCrossroad()
		{
			destroyAtNextTurn = true;
		}

		private bool setMoving(bool newTarget)
		{
			if (puppet != null && pathIndex < path.Count)
			{
				int facingDirection2 = 2;
				if (CurrentPathX > 0)
				{
					facingDirection2 = 1;
				}
				else if (CurrentPathX < 0)
				{
					facingDirection2 = 3;
				}
				else if (CurrentPathY < 0)
				{
					facingDirection2 = 0;
				}
				else if (CurrentPathY > 0)
				{
					facingDirection2 = 2;
				}
				puppet.Halt();
				puppet.faceDirection(facingDirection2);
				if (CurrentPathX != 0 && CurrentPathY != 0)
				{
					pauseTime = CurrentPathY;
					facingDirection2 = CurrentPathX % 4;
					puppet.faceDirection(facingDirection2);
					return true;
				}
				puppet.setMovingInFacingDirection();
				if (newTarget)
				{
					target = new Vector2(puppet.Position.X + (float)(CurrentPathX * 64), puppet.Position.Y + (float)(CurrentPathY * 64));
				}
				return true;
			}
			return false;
		}

		public bool update(GameTime time, GameLocation location, List<NPCController> allControllers)
		{
			puppet.speed = speed;
			bool reachedMeYet = false;
			foreach (NPCController i in allControllers)
			{
				if (i.puppet != null)
				{
					if (i.puppet.Equals(puppet))
					{
						reachedMeYet = true;
					}
					if (i.puppet.FacingDirection == puppet.FacingDirection && !i.puppet.Equals(puppet) && i.puppet.GetBoundingBox().Intersects(puppet.nextPosition(puppet.FacingDirection)))
					{
						if (reachedMeYet)
						{
							break;
						}
						return false;
					}
				}
			}
			if (puppet is Farmer)
			{
				(puppet as Farmer).setRunning(isRunning: false, force: true);
				puppet.speed = 2;
				(puppet as Farmer).ignoreCollisions = true;
			}
			if (puppet is Farmer && Game1.CurrentEvent != null && Game1.CurrentEvent.farmer != puppet)
			{
				(puppet as Farmer).updateMovementAnimation(time);
			}
			puppet.MovePosition(time, Game1.viewport, location);
			if (pauseTime < 0 && !puppet.isMoving())
			{
				setMoving(newTarget: false);
			}
			if (pauseTime < 0 && Math.Abs(Vector2.Distance(puppet.Position, target)) <= (float)puppet.Speed)
			{
				pathIndex++;
				if (destroyAtNextTurn)
				{
					return true;
				}
				if (!setMoving(newTarget: true))
				{
					if (loop)
					{
						pathIndex = 0;
						setMoving(newTarget: true);
					}
					else if (Game1.currentMinigame == null)
					{
						if (behaviorAtEnd != null)
						{
							behaviorAtEnd();
						}
						return true;
					}
				}
			}
			else if (pauseTime >= 0)
			{
				pauseTime -= time.ElapsedGameTime.Milliseconds;
				if (pauseTime < 0)
				{
					pathIndex++;
					if (destroyAtNextTurn)
					{
						return true;
					}
					if (!setMoving(newTarget: true))
					{
						if (loop)
						{
							pathIndex = 0;
							setMoving(newTarget: true);
						}
						else if (Game1.currentMinigame == null)
						{
							if (behaviorAtEnd != null)
							{
								behaviorAtEnd();
							}
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
