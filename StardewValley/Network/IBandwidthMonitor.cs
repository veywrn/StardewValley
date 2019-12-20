namespace StardewValley.Network
{
	public interface IBandwidthMonitor
	{
		BandwidthLogger BandwidthLogger
		{
			get;
		}

		bool LogBandwidth
		{
			get;
			set;
		}
	}
}
