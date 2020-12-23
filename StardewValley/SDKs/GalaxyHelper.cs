using Galaxy.Api;
using System;

namespace StardewValley.SDKs
{
	public class GalaxyHelper : SDKHelper
	{
		public class AuthListener : IAuthListener
		{
			public Action OnSuccess;

			public Action<FailureReason> OnFailure;

			public Action OnLost;

			public AuthListener(Action success, Action<FailureReason> failure, Action lost)
			{
				OnSuccess = success;
				OnFailure = failure;
				OnLost = lost;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerAuth.GetListenerType(), this);
			}

			public override void OnAuthSuccess()
			{
				if (OnSuccess != null)
				{
					OnSuccess();
				}
			}

			public override void OnAuthFailure(FailureReason reason)
			{
				if (OnFailure != null)
				{
					OnFailure(reason);
				}
			}

			public override void OnAuthLost()
			{
				if (OnLost != null)
				{
					OnLost();
				}
			}
		}

		public class OperationalStateChangeListener : IOperationalStateChangeListener
		{
			public Action<uint> OnStateChanged;

			public OperationalStateChangeListener(Action<uint> stateChanged)
			{
				OnStateChanged = stateChanged;
				GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerOperationalStateChange.GetListenerType(), this);
			}

			public override void OnOperationalStateChanged(uint operationalState)
			{
				if (OnStateChanged != null)
				{
					OnStateChanged(operationalState);
				}
			}
		}

		public const string ClientID = "48767653913349277";

		public const string ClientSecret = "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a";

		public bool active;

		private AuthListener authListener;

		private OperationalStateChangeListener stateChangeListener;

		private GalaxyNetHelper networking;

		public string Name
		{
			get;
		} = "Galaxy";


		public bool ConnectionFinished
		{
			get;
			private set;
		}

		public int ConnectionProgress
		{
			get;
			private set;
		}

		public SDKNetHelper Networking => networking;

		public bool HasOverlay => false;

		public bool IsJapaneseRegionRelease => false;

		public bool IsEnterButtonAssignmentFlipped => false;

		public void EarlyInitialize()
		{
		}

		public void Initialize()
		{
			try
			{
				GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a"));
				authListener = new AuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
				stateChangeListener = new OperationalStateChangeListener(onGalaxyStateChange);
				GalaxyInstance.User().SignInGalaxy(requireOnline: true);
				active = true;
				ConnectionProgress++;
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
				ConnectionFinished = true;
			}
		}

		private void onGalaxyStateChange(uint operationalState)
		{
			if (networking == null)
			{
				if ((operationalState & 1) != 0)
				{
					Console.WriteLine("Galaxy signed in");
					ConnectionProgress++;
				}
				if ((operationalState & 2) != 0)
				{
					Console.WriteLine("Galaxy logged on");
					networking = new GalaxyNetHelper();
					ConnectionProgress++;
					ConnectionFinished = true;
				}
			}
		}

		private void onGalaxyAuthSuccess()
		{
			Console.WriteLine("Galaxy auth success");
			ConnectionProgress++;
		}

		private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
		{
			Console.WriteLine("Galaxy auth failure: " + reason);
			ConnectionFinished = true;
		}

		private void onGalaxyAuthLost()
		{
			Console.WriteLine("Galaxy auth lost");
			ConnectionFinished = true;
		}

		public void GetAchievement(string achieve)
		{
		}

		public void ResetAchievements()
		{
			if (active)
			{
				GalaxyInstance.Stats().ResetStatsAndAchievements();
			}
		}

		public void Update()
		{
			if (active)
			{
				GalaxyInstance.ProcessData();
			}
		}

		public void Shutdown()
		{
		}

		public void DebugInfo()
		{
		}

		public string FilterDirtyWords(string words)
		{
			return words;
		}
	}
}
