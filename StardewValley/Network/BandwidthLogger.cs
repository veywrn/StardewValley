using System;
using System.Collections.Generic;

namespace StardewValley.Network
{
	public class BandwidthLogger
	{
		private long bitsDownSinceLastUpdate;

		private long bitsUpSinceLastUpdate;

		private DateTime lastUpdateTime = DateTime.Now;

		private double lastBitsDownPerSecond;

		private double lastBitsUpPerSecond;

		private double avgBitsUpPerSecond;

		private long bitsUpPerSecondCount;

		private double avgBitsDownPerSecond;

		private long bitsDownPerSecondCount;

		private long totalBitsDown;

		private long totalBitsUp;

		private double totalMs;

		private int queueCapacity = 100;

		private Queue<double> bitsUp = new Queue<double>();

		private Queue<double> bitsDown = new Queue<double>();

		public double AvgBitsDownPerSecond => avgBitsDownPerSecond;

		public double AvgBitsUpPerSecond => avgBitsUpPerSecond;

		public double BitsDownPerSecond => lastBitsDownPerSecond;

		public double BitsUpPerSecond => lastBitsUpPerSecond;

		public double TotalBitsDown => totalBitsDown;

		public double TotalBitsUp => totalBitsUp;

		public double TotalMs => totalMs;

		public Queue<double> LoggedAvgBitsUp => bitsUp;

		public Queue<double> LoggedAvgBitsDown => bitsDown;

		public void Update()
		{
			double msElapsed = (DateTime.Now - lastUpdateTime).TotalMilliseconds;
			if (msElapsed > 1000.0)
			{
				lastBitsDownPerSecond = (double)bitsDownSinceLastUpdate / msElapsed * 1000.0;
				lastBitsUpPerSecond = (double)bitsUpSinceLastUpdate / msElapsed * 1000.0;
				avgBitsDownPerSecond = (avgBitsDownPerSecond * (double)bitsDownPerSecondCount + lastBitsDownPerSecond) / (double)(++bitsDownPerSecondCount);
				avgBitsUpPerSecond = (avgBitsUpPerSecond * (double)bitsUpPerSecondCount + lastBitsUpPerSecond) / (double)(++bitsUpPerSecondCount);
				lastUpdateTime = DateTime.Now;
				bitsDownSinceLastUpdate = 0L;
				bitsUpSinceLastUpdate = 0L;
				totalMs += msElapsed;
				if (bitsUp.Count >= queueCapacity)
				{
					bitsUp.Dequeue();
				}
				if (bitsDown.Count >= queueCapacity)
				{
					bitsDown.Dequeue();
				}
				bitsUp.Enqueue(lastBitsUpPerSecond);
				bitsDown.Enqueue(lastBitsDownPerSecond);
			}
		}

		public void RecordBytesDown(long bytes)
		{
			bitsDownSinceLastUpdate += bytes * 8;
			totalBitsDown += bytes * 8;
		}

		public void RecordBytesUp(long bytes)
		{
			bitsUpSinceLastUpdate += bytes * 8;
			totalBitsUp += bytes * 8;
		}
	}
}
