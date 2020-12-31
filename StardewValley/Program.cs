using StardewValley.SDKs;
using System;
using System.IO;
using System.Text;

namespace StardewValley
{
	public static class Program
	{
		public enum LogType
		{
			Error,
			Disconnect
		}

		public const int build_steam = 0;

		public const int build_gog = 1;

		public const int build_rail = 2;

		public static bool GameTesterMode = false;

		public static bool releaseBuild = true;

		public const int buildType = 0;

		private static SDKHelper _sdk;

		public static Game1 gamePtr;

		public static bool handlingException;

		public static bool hasTriedToPrintLog;

		public static bool successfullyPrintedLog;

		internal static SDKHelper sdk
		{
			get
			{
				if (_sdk == null)
				{
					_sdk = new SteamHelper();
				}
				return _sdk;
			}
		}

		public static void Main(string[] args)
		{
			GameTesterMode = true;
			AppDomain.CurrentDomain.UnhandledException += handleException;
			using (GameRunner game = new GameRunner())
			{
				GameRunner.instance = game;
				game.Run();
			}
		}

		public static string WriteLog(LogType logType, string message, bool append = false)
		{
			string logDirectory2 = "ErrorLogs";
			string filename;
			if (logType != 0 && logType == LogType.Disconnect)
			{
				logDirectory2 = "DisconnectLogs";
				filename = ((Game1.player != null) ? Game1.player.Name : "NullPlayer") + "_" + DateTime.Now.Month + "-" + DateTime.Now.Day + ".txt";
			}
			else
			{
				logDirectory2 = "ErrorLogs";
				filename = ((Game1.player != null) ? Game1.player.Name : "NullPlayer") + "_" + Game1.uniqueIDForThisGame + "_" + ((Game1.player != null) ? ((int)Game1.player.millisecondsPlayed) : Game1.random.Next(999999)) + ".txt";
			}
			int folder = (Environment.OSVersion.Platform != PlatformID.Unix) ? 26 : 28;
			string fullFilePath = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder)folder), "StardewValley"), logDirectory2), filename);
			FileInfo info2 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath((Environment.SpecialFolder)folder), "StardewValley"), logDirectory2), "asdfasdf"));
			if (!info2.Directory.Exists)
			{
				info2.Directory.Create();
			}
			info2 = null;
			if (append)
			{
				if (!File.Exists(fullFilePath))
				{
					File.CreateText(fullFilePath);
				}
				try
				{
					File.AppendAllText(fullFilePath, message + Environment.NewLine);
					return fullFilePath;
				}
				catch (Exception)
				{
					return null;
				}
			}
			if (File.Exists(fullFilePath))
			{
				File.Delete(fullFilePath);
			}
			try
			{
				File.WriteAllText(fullFilePath, message);
				return fullFilePath;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static void handleException(object sender, UnhandledExceptionEventArgs args)
		{
			if (handlingException || !GameTesterMode)
			{
				return;
			}
			Game1.gameMode = 11;
			handlingException = true;
			Exception e = (Exception)args.ExceptionObject;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("Message: " + e.Message);
			sb.AppendLine("InnerException: " + e.InnerException);
			sb.AppendLine("Stack Trace: " + e.StackTrace);
			sb.AppendLine("");
			sb.AppendLine("Game Version: " + Game1.version);
			try
			{
				if (sdk != null)
				{
					sb.AppendLine("SDK Helper: " + sdk.GetType().Name);
				}
				sb.AppendLine("Game Language: " + LocalizedContentManager.CurrentLanguageCode.ToString());
				sb.AppendLine("GPU: " + Game1.graphics.GraphicsDevice.Adapter.Description);
				sb.AppendLine("OS: " + Environment.OSVersion.Platform + " " + Environment.OSVersion.VersionString);
				if (GameRunner.instance != null && GameRunner.instance.GetType().FullName.StartsWith("StardewModdingAPI."))
				{
					sb.AppendLine("Running SMAPI");
				}
				if (Game1.IsMultiplayer)
				{
					if (LocalMultiplayer.IsLocalMultiplayer())
					{
						sb.AppendLine("Multiplayer (Split Screen)");
					}
					else if (Game1.IsMasterGame)
					{
						sb.AppendLine("Multiplayer (Host)");
					}
					else
					{
						sb.AppendLine("Multiplayer (Client)");
					}
				}
				if (Game1.options.gamepadControls)
				{
					sb.AppendLine("Playing on Controller");
				}
				sb.AppendLine("In-game Date: " + Game1.currentSeason + " " + Game1.dayOfMonth + " Y" + Game1.year + " Time of Day: " + Game1.timeOfDay);
				sb.AppendLine("Game Location: " + ((Game1.currentLocation == null) ? "null" : Game1.currentLocation.NameOrUniqueName));
			}
			catch (Exception)
			{
			}
			Game1.errorMessage = sb.ToString();
			long targetTime = DateTime.Now.Ticks / 10000 + 25000;
			if (!hasTriedToPrintLog)
			{
				hasTriedToPrintLog = true;
				string successfulErrorPath = WriteLog(LogType.Error, Game1.errorMessage);
				if (successfulErrorPath != null)
				{
					successfullyPrintedLog = true;
					Game1.errorMessage = "(Error Report created at " + successfulErrorPath + ")" + Environment.NewLine + Game1.errorMessage;
				}
			}
			Game1.gameMode = 3;
		}
	}
}
