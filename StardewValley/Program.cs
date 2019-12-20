using StardewValley.SDKs;
using System;
using System.IO;

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

		public static SDKHelper sdk
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
			using (Game1 game = new Game1())
			{
				gamePtr = game;
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
			Game1.errorMessage = "Message: " + e.Message + Environment.NewLine + "InnerException: " + e.InnerException + Environment.NewLine + "Stack Trace: " + e.StackTrace;
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
