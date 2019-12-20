using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	public class Polygon
	{
		public class Line
		{
			public Vector2 Start;

			public Vector2 End;

			public Line(Vector2 Start, Vector2 End)
			{
				this.Start = Start;
				this.End = End;
			}
		}

		public List<Line> lines = new List<Line>();

		public List<Line> Lines
		{
			get
			{
				return lines;
			}
			set
			{
				lines = value;
			}
		}

		public void addPoint(Vector2 point)
		{
			if (lines.Count > 0)
			{
				lines.Add(new Line(Lines[Lines.Count - 1].End, point));
			}
		}

		public bool containsPoint(Vector2 point)
		{
			foreach (Line line in Lines)
			{
				if (line.End.Equals(point))
				{
					return true;
				}
			}
			return false;
		}

		public static Polygon getGentlerBorderForLakes(Rectangle room, Random mineRandom)
		{
			return getGentlerBorderForLakes(room, mineRandom, Rectangle.Empty);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom)
		{
			return getEdgeBorder(room, mineRandom, new List<Rectangle>(), (room.Width - 2) / 2, (room.Height - 2) / 2);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom, List<Rectangle> smoothZone)
		{
			return getEdgeBorder(room, mineRandom, smoothZone, (room.Width - 2) / 2, (room.Height - 2) / 2);
		}

		public static Polygon getEdgeBorder(Rectangle room, Random mineRandom, List<Rectangle> smoothZone, int horizontalInwardLimit, int verticalInwardLimit)
		{
			if (smoothZone == null)
			{
				smoothZone = new List<Rectangle>();
			}
			int lakeAreaWidth = room.Width - 2;
			int lakeAreaHeight = room.Height - 2;
			int lakeXPosition = room.X + 1;
			int lakeYPosition = room.Y + 1;
			new Rectangle(lakeXPosition, lakeYPosition, lakeAreaWidth, lakeAreaHeight);
			Polygon lake = new Polygon();
			Vector2 lastPosition = new Vector2(mineRandom.Next(lakeXPosition + 5, lakeXPosition + 8), mineRandom.Next(lakeYPosition + 5, lakeYPosition + 8));
			lake.Lines.Add(new Line(lastPosition, new Vector2(lastPosition.X + 1f, lastPosition.Y)));
			lastPosition.X += 1f;
			int topWidth = lakeAreaWidth - 12;
			List<int> lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			int m = 0;
			while (m < topWidth)
			{
				int whichWayToGo = mineRandom.Next(3);
				if (lastDirection4.Last() != whichWayToGo && lastDirection4[lastDirection4.Count - 2] != lastDirection4.Last())
				{
					whichWayToGo = lastDirection4.Last();
				}
				if (whichWayToGo == 0 && lastPosition.Y > (float)lakeYPosition && !lastDirection4.Contains(1))
				{
					lastPosition.Y -= 1f;
					lastDirection4.Add(0);
				}
				else if (whichWayToGo == 1 && lastPosition.Y < (float)(lakeYPosition + verticalInwardLimit) && !lastDirection4.Contains(0))
				{
					lastPosition.Y += 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.X += 1f;
					m++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int rightWidth = lakeAreaHeight - 4 - (int)(lastPosition.Y - (float)room.Y);
			lastPosition.Y += 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int l = 0;
			while (l < rightWidth)
			{
				int whichWayToGo2 = mineRandom.Next(3);
				if (lastDirection4.Last() != whichWayToGo2 && lastDirection4[lastDirection4.Count - 2] != lastDirection4.Last())
				{
					whichWayToGo2 = lastDirection4.Last();
				}
				if (l > 4 && whichWayToGo2 == 0 && lastPosition.X < (float)(lakeXPosition + lakeAreaWidth - 1) && !lastDirection4.Contains(1) && !Utility.pointInRectangles(smoothZone, (int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X += 1f;
					lastDirection4.Add(0);
				}
				else if (l > 4 && whichWayToGo2 == 1 && lastPosition.X > (float)(lakeXPosition + lakeAreaWidth - horizontalInwardLimit + 1) && !lastDirection4.Contains(0) && !Utility.pointInRectangles(smoothZone, (int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X -= 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.Y += 1f;
					l++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int bottomWidth = (int)lastPosition.X - (int)lake.Lines[0].Start.X + 1;
			lastPosition.X -= 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int k = 0;
			while (k < bottomWidth)
			{
				int whichWayToGo3 = mineRandom.Next(3);
				if (lastDirection4.Last() != whichWayToGo3 && lastDirection4[lastDirection4.Count - 2] != lastDirection4.Last())
				{
					whichWayToGo3 = lastDirection4.Last();
				}
				if (k > 4 && whichWayToGo3 == 0 && lastPosition.Y > (float)(lakeYPosition + lakeAreaHeight - verticalInwardLimit) && !lastDirection4.Contains(1) && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f)))
				{
					lastPosition.Y -= 1f;
					lastDirection4.Add(0);
				}
				else if (k > 4 && whichWayToGo3 == 1 && lastPosition.Y < (float)(lakeYPosition + lakeAreaHeight) && !lastDirection4.Contains(0))
				{
					lastPosition.Y += 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.X -= 1f;
					k++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int leftWidth = (int)lastPosition.Y - (int)lake.Lines[0].Start.Y - 1;
			lastPosition.Y -= 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int j = 0;
			while (j < leftWidth)
			{
				int whichWayToGo4 = mineRandom.Next(3);
				if (lastDirection4.Last() != whichWayToGo4 && lastDirection4[lastDirection4.Count - 2] != lastDirection4.Last())
				{
					whichWayToGo4 = lastDirection4.Last();
				}
				if (j > 4 && whichWayToGo4 == 0 && lastPosition.X < (float)(int)lake.Lines[0].Start.X && !lastDirection4.Contains(1) && !lake.containsPoint(new Vector2(lastPosition.X + 1f, lastPosition.Y)) && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f)) && !Utility.pointInRectangles(smoothZone, (int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X += 1f;
					lastDirection4.Add(0);
				}
				else if (j > 4 && whichWayToGo4 == 1 && lastPosition.X > (float)(lakeXPosition + 1) && !lastDirection4.Contains(0) && !Utility.pointInRectangles(smoothZone, (int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X -= 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.Y -= 1f;
					j++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			if (lastPosition.X < (float)(int)lake.Lines[0].Start.X)
			{
				int leftover = (int)lake.Lines[0].Start.X + 1 - (int)lastPosition.X - 1;
				for (int i = 0; i < leftover; i++)
				{
					lastPosition.X += 1f;
					lake.addPoint(lastPosition);
				}
			}
			return lake;
		}

		public static Polygon getGentlerBorderForLakes(Rectangle room, Random mineRandom, Rectangle smoothZone)
		{
			int lakeAreaWidth = room.Width - 2;
			int lakeAreaHeight = room.Height - 2;
			int lakeXPosition = room.X + 1;
			int lakeYPosition = room.Y + 1;
			new Rectangle(lakeXPosition, lakeYPosition, lakeAreaWidth, lakeAreaHeight);
			Polygon lake = new Polygon();
			Vector2 lastPosition = new Vector2(mineRandom.Next(lakeXPosition + 5, lakeXPosition + 8), mineRandom.Next(lakeYPosition + 5, lakeYPosition + 8));
			lake.Lines.Add(new Line(lastPosition, new Vector2(lastPosition.X + 1f, lastPosition.Y)));
			lastPosition.X += 1f;
			int topWidth = lakeAreaWidth - 12;
			List<int> lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			int m = 0;
			while (m < topWidth)
			{
				int whichWayToGo = mineRandom.Next(3);
				if (whichWayToGo == 0 && lastPosition.Y > (float)lakeYPosition && !lastDirection4.Contains(1) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.Y -= 1f;
					lastDirection4.Add(0);
				}
				else if (whichWayToGo == 1 && lastPosition.Y < (float)(lakeYPosition + lakeAreaHeight / 2) && !lastDirection4.Contains(0) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.Y += 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.X += 1f;
					m++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int rightWidth = lakeAreaHeight - 4 - (int)(lastPosition.Y - (float)room.Y);
			lastPosition.Y += 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int l = 0;
			while (l < rightWidth)
			{
				int whichWayToGo2 = mineRandom.Next(3);
				if (whichWayToGo2 == 0 && lastPosition.X < (float)(lakeXPosition + lakeAreaWidth) && !lastDirection4.Contains(1) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X += 1f;
					lastDirection4.Add(0);
				}
				else if (whichWayToGo2 == 1 && lastPosition.X > (float)(lakeXPosition + lakeAreaWidth / 2 + 1) && !lastDirection4.Contains(0) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X -= 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.Y += 1f;
					l++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int bottomWidth = (int)lastPosition.X - (int)lake.Lines[0].Start.X + 1;
			lastPosition.X -= 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int k = 0;
			while (k < bottomWidth)
			{
				int whichWayToGo3 = mineRandom.Next(3);
				if (whichWayToGo3 == 0 && lastPosition.Y > (float)(lakeYPosition + lakeAreaHeight / 2) && !lastDirection4.Contains(1) && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f)) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.Y -= 1f;
					lastDirection4.Add(0);
				}
				else if (whichWayToGo3 == 1 && lastPosition.Y < (float)(lakeYPosition + lakeAreaHeight) && !lastDirection4.Contains(0) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.Y += 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.X -= 1f;
					k++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			int leftWidth = (int)lastPosition.Y - (int)lake.Lines[0].Start.Y - 1;
			lastPosition.Y -= 1f;
			lastDirection4 = new List<int>
			{
				2,
				2,
				2
			};
			lake.addPoint(lastPosition);
			int j = 0;
			while (j < leftWidth)
			{
				int whichWayToGo4 = mineRandom.Next(3);
				if (whichWayToGo4 == 0 && lastPosition.X < (float)(int)lake.Lines[0].Start.X && !lastDirection4.Contains(1) && !lake.containsPoint(new Vector2(lastPosition.X + 1f, lastPosition.Y)) && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f)) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X += 1f;
					lastDirection4.Add(0);
				}
				else if (whichWayToGo4 == 1 && lastPosition.X > (float)(lakeXPosition + 1) && !lastDirection4.Contains(0) && !smoothZone.Contains((int)lastPosition.X, (int)lastPosition.Y))
				{
					lastPosition.X -= 1f;
					lastDirection4.Add(1);
				}
				else
				{
					lastPosition.Y -= 1f;
					j++;
					lastDirection4.Add(2);
				}
				lastDirection4.RemoveAt(0);
				lake.addPoint(lastPosition);
			}
			if (lastPosition.X < (float)(int)lake.Lines[0].Start.X)
			{
				int leftover = (int)lake.Lines[0].Start.X + 1 - (int)lastPosition.X - 1;
				for (int i = 0; i < leftover; i++)
				{
					lastPosition.X += 1f;
					lake.addPoint(lastPosition);
				}
			}
			return lake;
		}

		public static Polygon getRandomBorderRoom(Rectangle room, Random mineRandom)
		{
			int lakeAreaWidth = room.Width - 2;
			int lakeAreaHeight = room.Height - 2;
			int lakeXPosition = room.X + 1;
			int lakeYPosition = room.Y + 1;
			new Rectangle(lakeXPosition, lakeYPosition, lakeAreaWidth, lakeAreaHeight);
			Polygon lake = new Polygon();
			Vector2 lastPosition = new Vector2(mineRandom.Next(lakeXPosition + 5, lakeXPosition + 8), mineRandom.Next(lakeYPosition + 5, lakeYPosition + 8));
			lake.Lines.Add(new Line(lastPosition, new Vector2(lastPosition.X + 1f, lastPosition.Y)));
			lastPosition.X += 1f;
			int topWidth = room.Right - (int)lastPosition.X - lakeAreaWidth / 8;
			int lastDirection4 = 2;
			int m = 0;
			while (m < topWidth)
			{
				int whichWayToGo = mineRandom.Next(3);
				if ((whichWayToGo == 0 && lastPosition.Y > (float)room.Y && lastDirection4 != 1) || (lastDirection4 == 2 && lastPosition.Y >= (float)(lakeYPosition + lakeAreaHeight / 2)))
				{
					lastPosition.Y -= 1f;
					lastDirection4 = 0;
				}
				else if ((whichWayToGo == 1 && lastPosition.Y < (float)(lakeYPosition + lakeAreaHeight / 2) && lastDirection4 != 0) || (lastDirection4 == 2 && lastPosition.Y <= (float)room.Y))
				{
					lastPosition.Y += 1f;
					lastDirection4 = 1;
				}
				else
				{
					lastPosition.X += 1f;
					m++;
					lastDirection4 = 2;
				}
				lake.addPoint(lastPosition);
			}
			int rightWidth = lakeAreaHeight - 4 - (int)(lastPosition.Y - (float)room.Y);
			lastPosition.Y += 1f;
			lastDirection4 = 2;
			lake.addPoint(lastPosition);
			int l = 0;
			while (l < rightWidth)
			{
				int whichWayToGo2 = mineRandom.Next(3);
				if ((whichWayToGo2 == 0 && lastPosition.X < (float)room.Right && lastDirection4 != 1) || (lastDirection4 == 2 && lastPosition.X <= (float)(lakeXPosition + lakeAreaWidth / 2 + 1)))
				{
					lastPosition.X += 1f;
					lastDirection4 = 0;
				}
				else if ((whichWayToGo2 == 1 && lastPosition.X > (float)(lakeXPosition + lakeAreaWidth / 2 + 1) && lastDirection4 != 0) || (lastDirection4 == 2 && lastPosition.X >= (float)room.Right))
				{
					lastPosition.X -= 1f;
					lastDirection4 = 1;
				}
				else
				{
					lastPosition.Y += 1f;
					l++;
					lastDirection4 = 2;
				}
				lake.addPoint(lastPosition);
			}
			int bottomWidth = (int)lastPosition.X - (int)lake.Lines[0].Start.X + lakeAreaWidth / 4;
			lastPosition.X -= 1f;
			lastDirection4 = 2;
			lake.addPoint(lastPosition);
			int k = 0;
			while (k < bottomWidth)
			{
				int whichWayToGo3 = mineRandom.Next(3);
				if ((whichWayToGo3 == 0 && lastPosition.Y > (float)(lakeYPosition + lakeAreaHeight / 2) && lastDirection4 != 1 && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f))) || (lastDirection4 == 2 && lastPosition.Y >= (float)room.Bottom))
				{
					lastPosition.Y -= 1f;
					lastDirection4 = 0;
				}
				else if ((whichWayToGo3 == 1 && lastPosition.Y < (float)room.Bottom && lastDirection4 != 0) || (lastDirection4 == 2 && lastPosition.Y <= (float)(lakeYPosition + lakeAreaHeight / 2)))
				{
					lastPosition.Y += 1f;
					lastDirection4 = 1;
				}
				else
				{
					lastPosition.X -= 1f;
					k++;
					lastDirection4 = 2;
				}
				lake.addPoint(lastPosition);
			}
			int leftWidth = (int)lastPosition.Y - (int)lake.Lines[0].Start.Y - 1;
			lastPosition.Y -= 1f;
			lastDirection4 = 2;
			lake.addPoint(lastPosition);
			int j = 0;
			while (j < leftWidth)
			{
				int whichWayToGo4 = mineRandom.Next(3);
				if ((whichWayToGo4 == 0 && lastPosition.X < (float)room.Center.X && !lake.containsPoint(new Vector2(lastPosition.X + 1f, lastPosition.Y)) && !lake.containsPoint(new Vector2(lastPosition.X, lastPosition.Y - 1f))) || (lastDirection4 == 2 && lastPosition.X <= (float)room.X))
				{
					lastPosition.X += 1f;
					lastDirection4 = 0;
				}
				else if ((whichWayToGo4 == 1 && lastPosition.X > (float)room.X && lastDirection4 != 0) || (lastDirection4 == 2 && lastPosition.X >= (float)room.Center.X))
				{
					lastPosition.X -= 1f;
					lastDirection4 = 1;
				}
				else
				{
					lastPosition.Y -= 1f;
					j++;
					lastDirection4 = 2;
				}
				lake.addPoint(lastPosition);
			}
			if (lastPosition.X < (float)(int)lake.Lines[0].Start.X)
			{
				int leftover = (int)lake.Lines[0].Start.X + 1 - (int)lastPosition.X - 1;
				for (int i = 0; i < leftover; i++)
				{
					lastPosition.X += 1f;
					lake.addPoint(lastPosition);
				}
			}
			return lake;
		}
	}
}
