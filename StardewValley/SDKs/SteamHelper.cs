using Galaxy.Api;
using StardewValley.Menus;
using Steamworks;
using System;

namespace StardewValley.SDKs
{
	public class SteamHelper : SDKHelper
	{
		private Callback<GameOverlayActivated_t> gameOverlayActivated;

		private CallResult<EncryptedAppTicketResponse_t> encryptedAppTicketResponse;

		private Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

		private GalaxyHelper.AuthListener galaxyAuthListener;

		private GalaxyHelper.OperationalStateChangeListener galaxyStateChangeListener;

		public bool active;

		private SDKNetHelper networking;

		private TextBox _keyboardTextBox;

		public SDKNetHelper Networking => networking;

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

		public string Name
		{
			get;
		} = "Steam";


		public void EarlyInitialize()
		{
		}

		public void Initialize()
		{
			try
			{
				active = SteamAPI.Init();
				Console.WriteLine("Steam logged on: " + SteamUser.BLoggedOn().ToString());
				if (active)
				{
					Console.WriteLine("Initializing GalaxySDK");
					GalaxyInstance.InitLocal("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a", ".");
					encryptedAppTicketResponse = CallResult<EncryptedAppTicketResponse_t>.Create(onEncryptedAppTicketResponse);
					galaxyAuthListener = new GalaxyHelper.AuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
					galaxyStateChangeListener = new GalaxyHelper.OperationalStateChangeListener(onGalaxyStateChange);
					Console.WriteLine("Requesting Steam app ticket");
					SteamAPICall_t handle = SteamUser.RequestEncryptedAppTicket(new byte[0], 0);
					encryptedAppTicketResponse.Set(handle);
					ConnectionProgress++;
				}
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
				active = false;
				ConnectionFinished = true;
			}
			if (active)
			{
				gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(onGameOverlayActivated);
				gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnKeyboardDismissed);
			}
		}

		public void CancelKeyboard()
		{
			_keyboardTextBox = null;
		}

		public void ShowKeyboard(TextBox text_box)
		{
			_keyboardTextBox = text_box;
			SteamUtils.ShowGamepadTextInput(text_box.PasswordBox ? EGamepadTextInputMode.k_EGamepadTextInputModePassword : EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, "", (uint)((text_box.textLimit < 0) ? 100 : text_box.textLimit), text_box.Text);
		}

		public void OnKeyboardDismissed(GamepadTextInputDismissed_t callback)
		{
			if (_keyboardTextBox == null)
			{
				return;
			}
			if (!callback.m_bSubmitted)
			{
				_keyboardTextBox = null;
				return;
			}
			string entered_text = "";
			uint length = SteamUtils.GetEnteredGamepadTextLength();
			if (!SteamUtils.GetEnteredGamepadTextInput(out entered_text, length))
			{
				_keyboardTextBox = null;
				return;
			}
			_keyboardTextBox.RecieveTextInput(entered_text);
			_keyboardTextBox = null;
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
					networking = new SteamNetHelper();
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

		private void onEncryptedAppTicketResponse(EncryptedAppTicketResponse_t response, bool ioFailure)
		{
			if (response.m_eResult == EResult.k_EResultOK)
			{
				byte[] ticket = new byte[1024];
				SteamUser.GetEncryptedAppTicket(ticket, 1024, out uint ticketSize);
				Console.WriteLine("Signing into GalaxySDK");
				GalaxyInstance.User().SignIn(ticket, ticketSize, SteamFriends.GetPersonaName());
				ConnectionProgress++;
			}
			else
			{
				Console.WriteLine("Failed to retrieve encrypted app ticket: " + response.m_eResult + ", " + ioFailure.ToString());
				ConnectionFinished = true;
			}
		}

		private void onGameOverlayActivated(GameOverlayActivated_t pCallback)
		{
			if (active)
			{
				if (pCallback.m_bActive != 0)
				{
					Game1.paused = ((!Game1.IsMultiplayer) ? true : false);
				}
				else
				{
					Game1.paused = false;
				}
			}
		}

		public void GetAchievement(string achieve)
		{
			if (active && SteamAPI.IsSteamRunning())
			{
				if (achieve.Equals("0"))
				{
					achieve = "a0";
				}
				try
				{
					SteamUserStats.SetAchievement(achieve);
					SteamUserStats.StoreStats();
				}
				catch (Exception)
				{
				}
			}
		}

		public void ResetAchievements()
		{
			if (active && SteamAPI.IsSteamRunning())
			{
				try
				{
					SteamUserStats.ResetAllStats(bAchievementsToo: true);
				}
				catch (Exception)
				{
				}
			}
		}

		public void Update()
		{
			if (active)
			{
				SteamAPI.RunCallbacks();
				GalaxyInstance.ProcessData();
			}
		}

		public void Shutdown()
		{
			SteamAPI.Shutdown();
		}

		public void DebugInfo()
		{
			if (SteamAPI.IsSteamRunning())
			{
				Game1.debugOutput = "steam is running";
				if (SteamUser.BLoggedOn())
				{
					Game1.debugOutput += ", user logged on";
				}
			}
			else
			{
				Game1.debugOutput = "steam is not running";
				SteamAPI.Init();
			}
		}

		public string FilterDirtyWords(string words)
		{
			return words;
		}
	}
}
