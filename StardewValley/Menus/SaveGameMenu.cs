using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace StardewValley.Menus
{
	public class SaveGameMenu : IClickableMenu, IDisposable
	{
		private Stopwatch stopwatch;

		private IEnumerator<int> loader;

		private int completePause = -1;

		public bool quit;

		public bool hasDrawn;

		private SparklingText saveText;

		private int margin = 500;

		private StringBuilder _stringBuilder = new StringBuilder();

		private float _ellipsisDelay = 0.5f;

		private int _ellipsisCount;

		protected bool _hasSentFarmhandData;

		public SaveGameMenu()
		{
			saveText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGameMenu.cs.11378"), Color.LimeGreen, Color.Black * 0.001f, rainbow: false, 0.1, 1500, 32);
			_hasSentFarmhandData = false;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public void complete()
		{
			Game1.playSound("money");
			completePause = 1500;
			loader = null;
			Game1.game1.IsSaving = false;
			if (Game1.IsMasterGame && Game1.newDaySync != null && !Game1.newDaySync.hasSaved())
			{
				Game1.newDaySync.flagSaved();
			}
		}

		public override bool readyToClose()
		{
			return false;
		}

		public override void update(GameTime time)
		{
			if (quit)
			{
				if (Game1.activeClickableMenu.Equals(this) && Game1.PollForEndOfNewDaySync())
				{
					Game1.exitActiveMenu();
				}
				return;
			}
			base.update(time);
			if (Game1.client != null && Game1.client.timedOut)
			{
				quit = true;
				if (Game1.activeClickableMenu.Equals(this))
				{
					Game1.exitActiveMenu();
				}
				return;
			}
			_ellipsisDelay -= (float)time.ElapsedGameTime.TotalSeconds;
			if (_ellipsisDelay <= 0f)
			{
				_ellipsisDelay += 0.75f;
				_ellipsisCount++;
				if (_ellipsisCount > 3)
				{
					_ellipsisCount = 1;
				}
			}
			if (loader != null)
			{
				loader.MoveNext();
				if (loader.Current >= 100)
				{
					margin -= time.ElapsedGameTime.Milliseconds;
					if (margin <= 0)
					{
						complete();
					}
				}
			}
			else if (hasDrawn && completePause == -1)
			{
				if (Game1.IsMasterGame)
				{
					if (Game1.saveOnNewDay)
					{
						Game1.player.team.endOfNightStatus.UpdateState("ready");
						if (Game1.newDaySync.readyForSave())
						{
							Game1.multiplayer.saveFarmhands();
							Game1.game1.IsSaving = true;
							loader = SaveGame.Save();
						}
					}
					else
					{
						margin = -1;
						if (Game1.newDaySync.readyForSave())
						{
							Game1.game1.IsSaving = true;
							complete();
						}
					}
				}
				else
				{
					if (!_hasSentFarmhandData)
					{
						_hasSentFarmhandData = true;
						Game1.multiplayer.sendFarmhand();
					}
					Game1.multiplayer.UpdateLate();
					Program.sdk.Update();
					Game1.multiplayer.UpdateEarly();
					Game1.newDaySync.readyForSave();
					Game1.player.team.endOfNightStatus.UpdateState("ready");
					if (Game1.newDaySync.hasSaved())
					{
						saveClientOptions();
						complete();
					}
				}
			}
			if (completePause >= 0)
			{
				completePause -= time.ElapsedGameTime.Milliseconds;
				saveText.update(time);
				if (completePause < 0)
				{
					quit = true;
					completePause = -9999;
				}
			}
		}

		private static void saveClientOptions()
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false);
			startupPreferences.clientOptions = Game1.options;
			startupPreferences.savePreferences(async: false);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Vector2 txtpos2 = new Vector2(64f, Game1.viewport.Height - 64);
			Vector2 txtsize = new Vector2(64f, 64f);
			txtpos2 = Utility.makeSafe(txtpos2, txtsize);
			bool draw_ready_status = false;
			if (completePause >= 0)
			{
				if (Game1.saveOnNewDay)
				{
					saveText.draw(b, txtpos2);
				}
			}
			else if (margin < 0 || Game1.IsClient)
			{
				if (Game1.IsMultiplayer)
				{
					_stringBuilder.Clear();
					_stringBuilder.Append(Game1.content.LoadString("Strings\\UI:ReadyCheck", Game1.newDaySync.numReadyForSave(), Game1.getOnlineFarmers().Count()));
					for (int i = 0; i < _ellipsisCount; i++)
					{
						_stringBuilder.Append(".");
					}
					b.DrawString(Game1.dialogueFont, _stringBuilder, txtpos2, Color.White);
					draw_ready_status = true;
				}
			}
			else if (!Game1.IsMultiplayer)
			{
				_stringBuilder.Clear();
				_stringBuilder.Append(Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGameMenu.cs.11381"));
				for (int k = 0; k < _ellipsisCount; k++)
				{
					_stringBuilder.Append(".");
				}
				b.DrawString(Game1.dialogueFont, _stringBuilder, txtpos2, Color.White);
			}
			else
			{
				_stringBuilder.Clear();
				_stringBuilder.Append(Game1.content.LoadString("Strings\\UI:ReadyCheck", Game1.newDaySync.numReadyForSave(), Game1.getOnlineFarmers().Count()));
				for (int j = 0; j < _ellipsisCount; j++)
				{
					_stringBuilder.Append(".");
				}
				b.DrawString(Game1.dialogueFont, _stringBuilder, txtpos2, Color.White);
				draw_ready_status = true;
			}
			if (completePause > 0)
			{
				draw_ready_status = false;
			}
			if (Game1.newDaySync != null && Game1.newDaySync.hasSaved())
			{
				draw_ready_status = false;
			}
			if (Game1.IsMultiplayer && draw_ready_status && Game1.options.showMPEndOfNightReadyStatus)
			{
				Game1.player.team.endOfNightStatus.Draw(b, txtpos2 + new Vector2(0f, -32f), 4f, 0.99f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
			}
			hasDrawn = true;
		}

		public void Dispose()
		{
			Game1.game1.IsSaving = false;
		}
	}
}
