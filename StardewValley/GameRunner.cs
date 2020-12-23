using Force.DeepCloner;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace StardewValley
{
	public class GameRunner : Game
	{
		public static GameRunner instance;

		public List<Game1> gameInstances = new List<Game1>();

		public List<Game1> gameInstancesToRemove = new List<Game1>();

		public Game1 gamePtr;

		public bool shouldLoadContent;

		protected bool _initialized;

		protected bool _windowSizeChanged;

		public List<int> startButtonState = new List<int>();

		public List<KeyValuePair<Game1, IEnumerator<int>>> activeNewDayProcesses = new List<KeyValuePair<Game1, IEnumerator<int>>>();

		public int nextInstanceId;

		public GameRunner()
		{
			Program.sdk.EarlyInitialize();
			if (!Program.releaseBuild)
			{
				base.InactiveSleepTime = new TimeSpan(0L);
			}
			Game1.graphics = new GraphicsDeviceManager(this);
			Game1.graphics.PreparingDeviceSettings += delegate(object sender, PreparingDeviceSettingsEventArgs args)
			{
				args.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
			};
			Game1.graphics.PreferredBackBufferWidth = 1280;
			Game1.graphics.PreferredBackBufferHeight = 720;
			base.Content.RootDirectory = "Content";
			LocalMultiplayer.Initialize();
			base.Window.AllowUserResizing = true;
			SubscribeClientSizeChange();
			base.Exiting += delegate(object sender, EventArgs args)
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.exitEvent(sender, args);
				});
				Process.GetCurrentProcess().Kill();
			};
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			LocalizedContentManager.OnLanguageChange += delegate
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.TranslateFields();
				});
			};
			DebugTools.GameConstructed(this);
		}

		protected override void OnActivated(object sender, EventArgs args)
		{
			ExecuteForInstances(delegate(Game1 instance)
			{
				instance.Instance_OnActivated(sender, args);
			});
		}

		public void SubscribeClientSizeChange()
		{
			base.Window.ClientSizeChanged += OnWindowSizeChange;
		}

		public void OnWindowSizeChange(object sender, EventArgs args)
		{
			base.Window.ClientSizeChanged -= OnWindowSizeChange;
			_windowSizeChanged = true;
		}

		protected override bool BeginDraw()
		{
			return base.BeginDraw();
		}

		protected override void BeginRun()
		{
			base.BeginRun();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		protected override void Draw(GameTime gameTime)
		{
			if (_windowSizeChanged)
			{
				ExecuteForInstances(delegate(Game1 instance)
				{
					instance.Window_ClientSizeChanged(null, null);
				});
				_windowSizeChanged = false;
				SubscribeClientSizeChange();
			}
			foreach (Game1 instance2 in gameInstances)
			{
				LoadInstance(instance2);
				Viewport old_viewport = base.GraphicsDevice.Viewport;
				Game1.graphics.GraphicsDevice.Viewport = new Viewport(0, 0, Math.Min(instance2.localMultiplayerWindow.Width, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferWidth), Math.Min(instance2.localMultiplayerWindow.Height, Game1.graphics.GraphicsDevice.PresentationParameters.BackBufferHeight));
				instance2.Instance_Draw(gameTime);
				base.GraphicsDevice.Viewport = old_viewport;
				SaveInstance(instance2);
			}
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				base.GraphicsDevice.Clear(Game1.bgColor);
				foreach (Game1 gameInstance in gameInstances)
				{
					gameInstance.isRenderingScreenBuffer = true;
					gameInstance.DrawSplitScreenWindow();
					gameInstance.isRenderingScreenBuffer = false;
				}
			}
			base.Draw(gameTime);
		}

		public int GetNewInstanceID()
		{
			return nextInstanceId++;
		}

		public virtual Game1 GetFirstInstanceAtThisLocation(GameLocation location, Func<Game1, bool> additional_check = null)
		{
			if (location == null)
			{
				return null;
			}
			Game1 old_game = Game1.game1;
			if (old_game != null)
			{
				SaveInstance(old_game);
			}
			foreach (Game1 instance in gameInstances)
			{
				if (instance.instanceGameLocation != null && instance.instanceGameLocation.Equals(location))
				{
					if (additional_check != null)
					{
						LoadInstance(instance);
						bool num = additional_check(instance);
						SaveInstance(instance);
						if (!num)
						{
							continue;
						}
					}
					if (old_game != null)
					{
						LoadInstance(old_game);
					}
					else
					{
						Game1.game1 = null;
					}
					return instance;
				}
			}
			if (old_game != null)
			{
				LoadInstance(old_game);
			}
			else
			{
				Game1.game1 = null;
			}
			return null;
		}

		protected override void EndDraw()
		{
			base.EndDraw();
		}

		protected override void EndRun()
		{
			base.EndRun();
		}

		protected override void Initialize()
		{
			DebugTools.BeforeGameInitialize(this);
			InitializeMainInstance();
			base.IsFixedTimeStep = true;
			base.Initialize();
			Game1.graphics.SynchronizeWithVerticalRetrace = true;
			Program.sdk.Initialize();
		}

		public int GetMaxSimultaneousPlayers()
		{
			return 4;
		}

		public void InitializeMainInstance()
		{
			gameInstances = new List<Game1>();
			AddGameInstance(PlayerIndex.One);
		}

		public virtual void ExecuteForInstances(Action<Game1> action)
		{
			Game1 old_game = Game1.game1;
			if (old_game != null)
			{
				SaveInstance(old_game);
			}
			foreach (Game1 instance in gameInstances)
			{
				LoadInstance(instance);
				action(instance);
				SaveInstance(instance);
			}
			if (old_game != null)
			{
				LoadInstance(old_game);
			}
			else
			{
				Game1.game1 = null;
			}
		}

		public virtual void RemoveGameInstance(Game1 instance)
		{
			if (gameInstances.Contains(instance) && !gameInstancesToRemove.Contains(instance))
			{
				gameInstancesToRemove.Add(instance);
			}
		}

		public virtual void AddGameInstance(PlayerIndex player_index)
		{
			Game1 old_game = Game1.game1;
			if (old_game != null)
			{
				SaveInstance(old_game, force: true);
			}
			if (gameInstances.Count > 0)
			{
				Game1 game = gameInstances[0];
				LoadInstance(game);
				Game1.StartLocalMultiplayerIfNecessary();
				SaveInstance(game, force: true);
			}
			Game1 new_instance2 = null;
			new_instance2 = ((gameInstances.Count != 0) ? CreateGameInstance(player_index, gameInstances.Count) : CreateGameInstance());
			gameInstances.Add(new_instance2);
			if (gamePtr == null)
			{
				gamePtr = new_instance2;
			}
			if (gameInstances.Count > 0)
			{
				new_instance2.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
				SetInstanceDefaults(new_instance2);
				LoadInstance(new_instance2);
			}
			Game1.game1 = new_instance2;
			new_instance2.Instance_Initialize();
			if (shouldLoadContent)
			{
				new_instance2.Instance_LoadContent();
			}
			SaveInstance(new_instance2);
			if (old_game != null)
			{
				LoadInstance(old_game);
			}
			else
			{
				Game1.game1 = null;
			}
			_windowSizeChanged = true;
		}

		public virtual Game1 CreateGameInstance(PlayerIndex player_index = PlayerIndex.One, int index = 0)
		{
			return new Game1(player_index, index);
		}

		public Game1 GetGamePtr()
		{
			return gamePtr;
		}

		protected override void LoadContent()
		{
			LoadInstance(gamePtr);
			gamePtr.Instance_LoadContent();
			SaveInstance(gamePtr);
			DebugTools.GameLoadContent(this);
			foreach (Game1 instance in gameInstances)
			{
				if (instance != gamePtr)
				{
					LoadInstance(instance);
					instance.Instance_LoadContent();
					SaveInstance(instance);
				}
			}
			shouldLoadContent = true;
			base.LoadContent();
		}

		protected override void UnloadContent()
		{
			gamePtr.Instance_UnloadContent();
			base.UnloadContent();
		}

		protected override void Update(GameTime gameTime)
		{
			for (int i = 0; i < activeNewDayProcesses.Count; i++)
			{
				KeyValuePair<Game1, IEnumerator<int>> active_new_days = activeNewDayProcesses[i];
				Game1 instance = activeNewDayProcesses[i].Key;
				LoadInstance(instance);
				if (!active_new_days.Value.MoveNext())
				{
					instance.isLocalMultiplayerNewDayActive = false;
					activeNewDayProcesses.RemoveAt(i);
					i--;
					Utility.CollectGarbage();
				}
				SaveInstance(instance);
			}
			while (startButtonState.Count < 4)
			{
				startButtonState.Add(-1);
			}
			for (PlayerIndex player_index = PlayerIndex.One; player_index <= PlayerIndex.Four; player_index++)
			{
				if (GamePad.GetState(player_index).IsButtonDown(Buttons.Start))
				{
					if (startButtonState[(int)player_index] >= 0)
					{
						startButtonState[(int)player_index]++;
					}
				}
				else if (startButtonState[(int)player_index] != 0)
				{
					startButtonState[(int)player_index] = 0;
				}
			}
			for (int j = 0; j < gameInstances.Count; j++)
			{
				Game1 instance2 = gameInstances[j];
				LoadInstance(instance2);
				if (j == 0)
				{
					PlayerIndex start_player_index = PlayerIndex.Two;
					if (instance2.instanceOptions.gamepadMode == Options.GamepadModes.ForceOff)
					{
						start_player_index = PlayerIndex.One;
					}
					for (PlayerIndex player_index2 = start_player_index; player_index2 <= PlayerIndex.Four; player_index2++)
					{
						bool fail = false;
						foreach (Game1 gameInstance in gameInstances)
						{
							if (gameInstance.instancePlayerOneIndex == player_index2)
							{
								fail = true;
								break;
							}
						}
						if (!fail && instance2.IsLocalCoopJoinable() && IsStartDown(player_index2) && instance2.ShowLocalCoopJoinMenu())
						{
							InvalidateStartPress(player_index2);
						}
					}
				}
				else
				{
					Game1.options.gamepadMode = Options.GamepadModes.ForceOn;
				}
				instance2.Instance_Update(gameTime);
				SaveInstance(instance2);
			}
			if (gameInstancesToRemove.Count > 0)
			{
				foreach (Game1 instance3 in gameInstancesToRemove)
				{
					LoadInstance(instance3);
					instance3.exitEvent(null, null);
					gameInstances.Remove(instance3);
					Game1.game1 = null;
				}
				for (int k = 0; k < gameInstances.Count; k++)
				{
					gameInstances[k].instanceIndex = k;
				}
				if (gameInstances.Count == 1)
				{
					Game1 game = gameInstances[0];
					LoadInstance(game, force: true);
					game.staticVarHolder = null;
					Game1.EndLocalMultiplayer();
				}
				bool controller_1_assigned = false;
				if (gameInstances.Count > 0)
				{
					foreach (Game1 gameInstance2 in gameInstances)
					{
						if (gameInstance2.instancePlayerOneIndex == PlayerIndex.One)
						{
							controller_1_assigned = true;
							break;
						}
					}
					if (!controller_1_assigned)
					{
						gameInstances[0].instancePlayerOneIndex = PlayerIndex.One;
					}
				}
				gameInstancesToRemove.Clear();
				_windowSizeChanged = true;
			}
			base.Update(gameTime);
		}

		public virtual void InvalidateStartPress(PlayerIndex index)
		{
			if (index >= PlayerIndex.One && (int)index < startButtonState.Count)
			{
				startButtonState[(int)index] = -1;
			}
		}

		public virtual bool IsStartDown(PlayerIndex index)
		{
			if (index >= PlayerIndex.One && (int)index < startButtonState.Count)
			{
				return startButtonState[(int)index] == 1;
			}
			return false;
		}

		private static void SetInstanceDefaults(InstanceGame instance)
		{
			for (int i = 0; i < LocalMultiplayer.staticDefaults.Count; i++)
			{
				object value = LocalMultiplayer.staticDefaults[i];
				if (value != null)
				{
					value = value.DeepClone();
				}
				LocalMultiplayer.staticFields[i].SetValue(null, value);
			}
			SaveInstance(instance);
		}

		public static void SaveInstance(InstanceGame instance, bool force = false)
		{
			if (force || LocalMultiplayer.IsLocalMultiplayer())
			{
				if (instance.staticVarHolder == null)
				{
					instance.staticVarHolder = Activator.CreateInstance(LocalMultiplayer.StaticVarHolderType);
				}
				LocalMultiplayer.StaticSave(instance.staticVarHolder);
			}
		}

		public static void LoadInstance(InstanceGame instance, bool force = false)
		{
			Game1.game1 = (instance as Game1);
			if ((force || LocalMultiplayer.IsLocalMultiplayer()) && instance.staticVarHolder != null)
			{
				LocalMultiplayer.StaticLoad(instance.staticVarHolder);
				if (Game1.player != null && (bool)Game1.player.isCustomized && Game1.splitscreenOptions.ContainsKey(Game1.player.UniqueMultiplayerID))
				{
					Game1.options = Game1.splitscreenOptions[Game1.player.UniqueMultiplayerID];
					Game1.options.lightingQuality = GameRunner.instance.gameInstances[0].instanceOptions.lightingQuality;
				}
			}
		}
	}
}
