using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewValley.Menus
{
	public class ChatBox : IClickableMenu
	{
		public const int chatMessage = 0;

		public const int errorMessage = 1;

		public const int userNotificationMessage = 2;

		public const int privateMessage = 3;

		public const int defaultMaxMessages = 10;

		public const int timeToDisplayMessages = 600;

		public const int chatboxWidth = 896;

		public const int chatboxHeight = 56;

		public ChatTextBox chatBox;

		private TextBoxEvent e;

		private TextBoxEvent e_backspace;

		private List<ChatMessage> messages = new List<ChatMessage>();

		private KeyboardState oldKBState;

		private List<string> cheatHistory = new List<string>();

		private int cheatHistoryPosition = -1;

		public int maxMessages = 10;

		public static Texture2D emojiTexture;

		private ClickableTextureComponent emojiMenuIcon;

		public EmojiMenu emojiMenu;

		public bool choosingEmoji;

		public bool enableCheats;

		private long lastReceivedPrivateMessagePlayerId;

		public ChatBox()
		{
			enableCheats = !Program.releaseBuild;
			Texture2D chatboxTexture = Game1.content.Load<Texture2D>("LooseSprites\\chatBox");
			chatBox = new ChatTextBox(chatboxTexture, null, Game1.smallFont, Color.White);
			e = textBoxEnter;
			chatBox.OnEnterPressed += e;
			Game1.keyboardDispatcher.Subscriber = chatBox;
			emojiTexture = Game1.content.Load<Texture2D>("LooseSprites\\emojis");
			emojiMenuIcon = new ClickableTextureComponent(new Rectangle(0, 0, 40, 36), emojiTexture, new Rectangle(0, 0, 9, 9), 4f);
			emojiMenu = new EmojiMenu(this, emojiTexture, chatboxTexture);
			updatePosition();
			chatBox.Selected = false;
		}

		private void updatePosition()
		{
			chatBox.Width = 896;
			chatBox.Height = 56;
			width = chatBox.Width;
			height = chatBox.Height;
			xPositionOnScreen = 0;
			yPositionOnScreen = Game1.viewport.Height - chatBox.Height;
			Utility.makeSafe(ref xPositionOnScreen, ref yPositionOnScreen, chatBox.Width, chatBox.Height);
			chatBox.X = xPositionOnScreen;
			chatBox.Y = yPositionOnScreen;
			emojiMenuIcon.bounds.Y = chatBox.Y + 8;
			emojiMenuIcon.bounds.X = chatBox.Width - emojiMenuIcon.bounds.Width - 8;
			if (emojiMenu != null)
			{
				emojiMenu.xPositionOnScreen = emojiMenuIcon.bounds.Center.X - 146;
				emojiMenu.yPositionOnScreen = emojiMenuIcon.bounds.Y - 248;
			}
		}

		public virtual void textBoxEnter(string text_to_send)
		{
			if (text_to_send.Length >= 1)
			{
				if (text_to_send[0] == '/' && text_to_send.Split(' ')[0].Length > 1)
				{
					runCommand(text_to_send.Substring(1));
					return;
				}
				text_to_send = Program.sdk.FilterDirtyWords(text_to_send);
				Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, text_to_send, Multiplayer.AllPlayers);
				receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, text_to_send);
			}
		}

		public virtual void textBoxEnter(TextBox sender)
		{
			if (sender is ChatTextBox)
			{
				ChatTextBox box = sender as ChatTextBox;
				if (box.finalText.Count > 0)
				{
					bool include_color_information = true;
					if (box.finalText[0].message != null && box.finalText[0].message.Length > 0 && box.finalText[0].message.ToString()[0] == '/' && box.finalText[0].message.Split(' ')[0].Length > 1)
					{
						include_color_information = false;
					}
					if (box.finalText.Count != 1 || ((box.finalText[0].message != null || box.finalText[0].emojiIndex != -1) && (box.finalText[0].message == null || box.finalText[0].message.Trim().Length != 0)))
					{
						string textToSend = ChatMessage.makeMessagePlaintext(box.finalText, include_color_information);
						textBoxEnter(textToSend);
					}
				}
				box.reset();
				cheatHistoryPosition = -1;
			}
			sender.Text = "";
			clickAway();
		}

		public virtual void addInfoMessage(string message)
		{
			receiveChatMessage(0L, 2, LocalizedContentManager.CurrentLanguageCode, message);
		}

		public virtual void globalInfoMessage(string messageKey, params string[] args)
		{
			if (Game1.IsMultiplayer)
			{
				Game1.multiplayer.globalChatInfoMessage(messageKey, args);
			}
			else
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_" + messageKey, args));
			}
		}

		public virtual void addErrorMessage(string message)
		{
			receiveChatMessage(0L, 1, LocalizedContentManager.CurrentLanguageCode, message);
		}

		public virtual void listPlayers(bool otherPlayersOnly = false)
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserList"));
			foreach (Farmer f in Game1.getOnlineFarmers())
			{
				if (!otherPlayersOnly || f.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
				{
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UserListUser", formattedUserNameLong(f)));
				}
			}
		}

		public virtual void showHelp()
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_Help"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpClear", "clear"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpList", "list"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColor", "color"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpColorList", "color-list"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpPause", "pause"));
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpResume", "resume"));
			if (Game1.IsMultiplayer)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpMessage", "message"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpReply", "reply"));
			}
			if (Game1.IsServer)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpKick", "kick"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpBan", "ban"));
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_HelpUnban", "unban"));
			}
		}

		protected virtual void runCommand(string command)
		{
			string[] split = command.Split(' ');
			switch (split[0])
			{
			case "qi":
				if (!Game1.player.mailReceived.Contains("QiChat1"))
				{
					Game1.player.mailReceived.Add("QiChat1");
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi1"), new Color(100, 50, 255));
				}
				else if (!Game1.player.mailReceived.Contains("QiChat2"))
				{
					Game1.player.mailReceived.Add("QiChat2");
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi2"), new Color(100, 50, 255));
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_Qi3"), Color.Yellow);
				}
				break;
			case "ape":
			case "concernedape":
			case "ConcernedApe":
			case "ca":
				if (!Game1.player.mailReceived.Contains("apeChat1"))
				{
					Game1.player.mailReceived.Add("apeChat1");
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe"), new Color(104, 214, 255));
				}
				else
				{
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApe2"), Color.Yellow);
				}
				break;
			case "dm":
			case "pm":
			case "message":
			case "whisper":
				sendPrivateMessage(command);
				break;
			case "reply":
			case "r":
				replyPrivateMessage(command);
				break;
			case "showmethemoney":
			case "imacheat":
			case "cheat":
			case "cheats":
			case "freegold":
			case "debug":
			case "rosebud":
				addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
				break;
			case "pause":
				if (!Game1.IsMasterGame)
				{
					addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
					break;
				}
				Game1.netWorldState.Value.IsPaused = !Game1.netWorldState.Value.IsPaused;
				if (Game1.netWorldState.Value.IsPaused)
				{
					globalInfoMessage("Paused");
				}
				else
				{
					globalInfoMessage("Resumed");
				}
				break;
			case "resume":
				if (!Game1.IsMasterGame)
				{
					addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_HostOnlyCommand"));
				}
				else if (Game1.netWorldState.Value.IsPaused)
				{
					Game1.netWorldState.Value.IsPaused = false;
					globalInfoMessage("Resumed");
				}
				break;
			case "color":
				if (split.Length > 1)
				{
					Game1.player.defaultChatColor = split[1];
				}
				break;
			case "color-list":
				addMessage("white, red, blue, green, jade, yellowgreen, pink, purple, yellow, orange, brown, gray, cream, salmon, peach, aqua, jungle, plum", Color.White);
				break;
			case "clear":
				messages.Clear();
				break;
			case "list":
			case "users":
			case "players":
				listPlayers();
				break;
			case "help":
			case "h":
				showHelp();
				break;
			case "kick":
				if (Game1.IsMultiplayer && Game1.IsServer)
				{
					kickPlayer(command);
				}
				break;
			case "ban":
				if (Game1.IsMultiplayer && Game1.IsServer)
				{
					banPlayer(command);
				}
				break;
			case "unban":
				if (Game1.IsServer)
				{
					unbanPlayer(command);
				}
				break;
			case "unbanAll":
			case "unbanall":
				if (Game1.IsServer)
				{
					if (Game1.bannedUsers.Count == 0)
					{
						addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
					}
					else
					{
						unbanAll();
					}
				}
				break;
			case "ping":
				if (Game1.IsMultiplayer)
				{
					StringBuilder sb = new StringBuilder();
					if (Game1.IsServer)
					{
						foreach (KeyValuePair<long, Farmer> farmer in Game1.otherFarmers)
						{
							sb.Clear();
							sb.AppendFormat("Ping({0}) {1}ms ", farmer.Value.Name, (int)Game1.server.getPingToClient(farmer.Key));
							addMessage(sb.ToString(), Color.White);
						}
						break;
					}
					sb.AppendFormat("Ping: {0}ms", (int)Game1.client.GetPingToHost());
					addMessage(sb.ToString(), Color.White);
				}
				break;
			case "mapscreenshot":
			{
				int scale = 25;
				string screenshot_name = null;
				if (split.Count() > 2 && !int.TryParse(split[2], out scale))
				{
					scale = 25;
				}
				if (split.Count() > 1)
				{
					screenshot_name = split[1];
				}
				if (scale <= 10)
				{
					scale = 10;
				}
				string result = Game1.game1.takeMapScreenshot((float)scale / 100f, screenshot_name);
				if (result != null)
				{
					addMessage("Wrote '" + result + "'.", Color.White);
				}
				else
				{
					addMessage("Failed.", Color.Red);
				}
				break;
			}
			case "mbp":
			case "movepermission":
			case "movebuildingpermission":
				if (!Game1.IsMasterGame)
				{
					break;
				}
				if (split.Count() > 1)
				{
					string toggle = split[1];
					if (toggle == "off")
					{
						Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
					}
					else if (toggle == "owned")
					{
						Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
					}
					else if (toggle == "on")
					{
						Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
					}
					addMessage("movebuildingpermission " + Game1.player.team.farmhandsCanMoveBuildings.Value, Color.White);
				}
				else
				{
					addMessage("off, owned, on", Color.White);
				}
				break;
			case "sleepannouncemode":
				if (!Game1.IsMasterGame)
				{
					break;
				}
				if (split.Count() > 1)
				{
					string toggle2 = split[1];
					if (toggle2 == "all")
					{
						Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.All;
					}
					else if (toggle2 == "first")
					{
						Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.First;
					}
					else if (toggle2 == "off")
					{
						Game1.player.team.sleepAnnounceMode.Value = FarmerTeam.SleepAnnounceModes.Off;
					}
				}
				Game1.multiplayer.globalChatInfoMessage("SleepAnnounceModeSet", Game1.content.LoadString("Strings\\UI:SleepAnnounceMode_" + Game1.player.team.sleepAnnounceMode.Value.ToString()));
				break;
			case "money":
				if (enableCheats)
				{
					cheat(command);
				}
				else
				{
					addMessage(Game1.content.LoadString("Strings\\UI:Chat_ConcernedApeNiceTry"), new Color(104, 214, 255));
				}
				break;
			case "e":
			case "emote":
			{
				if (!Game1.player.CanEmote())
				{
					break;
				}
				bool valid_emote = false;
				if (split.Count() > 1)
				{
					string emote_type2 = split[1];
					emote_type2 = emote_type2.Substring(0, Math.Min(emote_type2.Length, 16));
					emote_type2.Trim();
					emote_type2.ToLower();
					for (int j = 0; j < Farmer.EMOTES.Length; j++)
					{
						if (emote_type2 == Farmer.EMOTES[j].emoteString)
						{
							valid_emote = true;
							break;
						}
					}
					if (valid_emote)
					{
						Game1.player.netDoEmote(emote_type2);
					}
				}
				if (valid_emote)
				{
					break;
				}
				string emote_list = "";
				for (int i = 0; i < Farmer.EMOTES.Length; i++)
				{
					if (!Farmer.EMOTES[i].hidden)
					{
						emote_list += Farmer.EMOTES[i].emoteString;
						if (i < Farmer.EMOTES.Length - 1)
						{
							emote_list += ", ";
						}
					}
				}
				addMessage(emote_list, Color.White);
				break;
			}
			default:
				if (enableCheats || Game1.isRunningMacro)
				{
					cheat(command);
				}
				break;
			}
		}

		public virtual void cheat(string command)
		{
			Game1.debugOutput = null;
			addInfoMessage("/" + command);
			if (!Game1.isRunningMacro)
			{
				cheatHistory.Insert(0, "/" + command);
			}
			if (Game1.game1.parseDebugInput(command))
			{
				if (Game1.debugOutput != null && Game1.debugOutput != "")
				{
					addInfoMessage(Game1.debugOutput);
				}
			}
			else if (Game1.debugOutput != null && Game1.debugOutput != "")
			{
				addErrorMessage(Game1.debugOutput);
			}
			else
			{
				addErrorMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:ChatBox.cs.10261") + " " + command.Split(' ')[0]);
			}
		}

		private void replyPrivateMessage(string command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			if (lastReceivedPrivateMessagePlayerId == 0L)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerToReplyTo"));
				return;
			}
			bool playerOffline = !Game1.otherFarmers.ContainsKey(lastReceivedPrivateMessagePlayerId);
			if (!playerOffline)
			{
				playerOffline = !Game1.otherFarmers[lastReceivedPrivateMessagePlayerId].isActive();
			}
			if (playerOffline)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_CouldNotReply"));
				return;
			}
			string[] split = command.Split(' ');
			if (split.Length <= 1)
			{
				return;
			}
			string message2 = "";
			for (int i = 1; i < split.Length; i++)
			{
				message2 += split[i];
				if (i < split.Length - 1)
				{
					message2 += " ";
				}
			}
			message2 = Program.sdk.FilterDirtyWords(message2);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message2, lastReceivedPrivateMessagePlayerId);
			receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message2);
		}

		private void kickPlayer(string command)
		{
			int index = 0;
			Farmer farmer = findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
			if (farmer != null)
			{
				Game1.server.kick(farmer.UniqueMultiplayerID);
				return;
			}
			addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
			listPlayers(otherPlayersOnly: true);
		}

		private void banPlayer(string command)
		{
			int index = 0;
			Farmer farmer = findMatchingFarmer(command, ref index, allowMatchingByUserName: true);
			if (farmer != null)
			{
				string userId = Game1.server.ban(farmer.UniqueMultiplayerID);
				if (userId == null || !Game1.bannedUsers.ContainsKey(userId))
				{
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayerFailed"));
					return;
				}
				string userName = Game1.bannedUsers[userId];
				string userDisplay = (userName != null) ? (userName + " (" + userId + ")") : userId;
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayer", userDisplay));
			}
			else
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
				listPlayers(otherPlayersOnly: true);
			}
		}

		private void unbanAll()
		{
			addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedAllPlayers"));
			Game1.bannedUsers.Clear();
		}

		private void unbanPlayer(string command)
		{
			if (Game1.bannedUsers.Count == 0)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList_None"));
				return;
			}
			bool listUnbannablePlayers = false;
			string[] split = command.Split(' ');
			if (split.Length > 1)
			{
				string unbanId = split[1];
				string userId = null;
				if (Game1.bannedUsers.ContainsKey(unbanId))
				{
					userId = unbanId;
				}
				else
				{
					foreach (KeyValuePair<string, string> bannedUser2 in Game1.bannedUsers)
					{
						if (bannedUser2.Value == unbanId)
						{
							userId = bannedUser2.Key;
							break;
						}
					}
				}
				if (userId != null)
				{
					string userDisplay2 = (Game1.bannedUsers[userId] == null) ? userId : (Game1.bannedUsers[userId] + " (" + userId + ")");
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbannedPlayer", userDisplay2));
					Game1.bannedUsers.Remove(userId);
				}
				else
				{
					listUnbannablePlayers = true;
					addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_UnbanPlayer_NotFound"));
				}
			}
			else
			{
				listUnbannablePlayers = true;
			}
			if (listUnbannablePlayers)
			{
				addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_BannedPlayersList"));
				foreach (KeyValuePair<string, string> bannedUser in Game1.bannedUsers)
				{
					string userDisplay = "- " + bannedUser.Key;
					if (bannedUser.Value != null)
					{
						userDisplay = "- " + bannedUser.Value + " (" + bannedUser.Key + ")";
					}
					addInfoMessage(userDisplay);
				}
			}
		}

		private Farmer findMatchingFarmer(string command, ref int matchingIndex, bool allowMatchingByUserName = false)
		{
			string[] split = command.Split(' ');
			Farmer matchingFarmer3 = null;
			foreach (Farmer farmer in Game1.otherFarmers.Values)
			{
				string[] farmerNameSplit = farmer.displayName.Split(' ');
				bool isMatch2 = true;
				int j;
				for (j = 0; j < farmerNameSplit.Length; j++)
				{
					if (split.Length <= j + 1)
					{
						isMatch2 = false;
						break;
					}
					if (split[j + 1].ToLowerInvariant() != farmerNameSplit[j].ToLowerInvariant())
					{
						isMatch2 = false;
						break;
					}
				}
				if (isMatch2)
				{
					matchingFarmer3 = farmer;
					matchingIndex = j;
					return matchingFarmer3;
				}
				if (allowMatchingByUserName)
				{
					isMatch2 = true;
					string[] userNameSplit = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID).Split(' ');
					for (j = 0; j < userNameSplit.Length; j++)
					{
						if (split.Length <= j + 1)
						{
							isMatch2 = false;
							break;
						}
						if (split[j + 1].ToLowerInvariant() != userNameSplit[j].ToLowerInvariant())
						{
							isMatch2 = false;
							break;
						}
					}
					if (isMatch2)
					{
						matchingFarmer3 = farmer;
						matchingIndex = j;
						return matchingFarmer3;
					}
				}
			}
			return matchingFarmer3;
		}

		private void sendPrivateMessage(string command)
		{
			if (!Game1.IsMultiplayer)
			{
				return;
			}
			string[] split = command.Split(' ');
			int matchingIndex = 0;
			Farmer matchingFarmer = findMatchingFarmer(command, ref matchingIndex);
			if (matchingFarmer == null)
			{
				addErrorMessage(Game1.content.LoadString("Strings\\UI:Chat_NoPlayerWithThatName"));
				return;
			}
			string message2 = "";
			for (int i = matchingIndex + 1; i < split.Length; i++)
			{
				message2 += split[i];
				if (i < split.Length - 1)
				{
					message2 += " ";
				}
			}
			message2 = Program.sdk.FilterDirtyWords(message2);
			Game1.multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, message2, matchingFarmer.UniqueMultiplayerID);
			receiveChatMessage(Game1.player.UniqueMultiplayerID, 3, LocalizedContentManager.CurrentLanguageCode, message2);
		}

		public bool isActive()
		{
			return chatBox.Selected;
		}

		public void activate()
		{
			chatBox.Selected = true;
			setText("");
		}

		public override void clickAway()
		{
			base.clickAway();
			if (!choosingEmoji || !emojiMenu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Escape))
			{
				bool selected = chatBox.Selected;
				chatBox.Selected = false;
				choosingEmoji = false;
				setText("");
				cheatHistoryPosition = -1;
				if (selected)
				{
					Game1.oldKBState = Game1.GetKeyboardState();
				}
			}
		}

		public override bool isWithinBounds(int x, int y)
		{
			if (x - xPositionOnScreen >= width || x - xPositionOnScreen < 0 || y - yPositionOnScreen >= height || y - yPositionOnScreen < -getOldMessagesBoxHeight())
			{
				if (choosingEmoji)
				{
					return emojiMenu.isWithinBounds(x, y);
				}
				return false;
			}
			return true;
		}

		public virtual void setText(string text)
		{
			chatBox.setText(text);
		}

		public override void receiveKeyPress(Keys key)
		{
			switch (key)
			{
			case Keys.Up:
				if (cheatHistoryPosition < cheatHistory.Count - 1)
				{
					cheatHistoryPosition++;
					string cheat2 = cheatHistory[cheatHistoryPosition];
					chatBox.setText(cheat2);
				}
				break;
			case Keys.Down:
				if (cheatHistoryPosition > 0)
				{
					cheatHistoryPosition--;
					string cheat = cheatHistory[cheatHistoryPosition];
					chatBox.setText(cheat);
				}
				break;
			}
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				switch (b)
				{
				case Buttons.DPadUp:
					receiveKeyPress(Keys.Up);
					break;
				case Buttons.DPadDown:
					receiveKeyPress(Keys.Down);
					break;
				}
			}
		}

		public bool isHoveringOverEmojiUI(int x, int y)
		{
			if (!emojiMenuIcon.containsPoint(x, y))
			{
				if (choosingEmoji)
				{
					return emojiMenu.isWithinBounds(x, y);
				}
				return false;
			}
			return true;
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (emojiMenuIcon.containsPoint(x, y))
			{
				choosingEmoji = !choosingEmoji;
				Game1.playSound("shwip");
				emojiMenuIcon.scale = 4f;
				return;
			}
			if (choosingEmoji && emojiMenu.isWithinBounds(x, y))
			{
				emojiMenu.leftClick(x, y, this);
				return;
			}
			chatBox.Update();
			if (choosingEmoji)
			{
				choosingEmoji = false;
				emojiMenuIcon.scale = 4f;
			}
			if (isWithinBounds(x, y))
			{
				chatBox.Selected = true;
			}
		}

		public static string formattedUserName(Farmer farmer)
		{
			string name = farmer.Name;
			if (name == null || name.Trim() == "")
			{
				name = Game1.content.LoadString("Strings\\UI:Chat_PlayerJoinedNewName");
			}
			return name;
		}

		public static string formattedUserNameLong(Farmer farmer)
		{
			string name = formattedUserName(farmer);
			return Game1.content.LoadString("Strings\\UI:Chat_PlayerName", name, Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID));
		}

		private string formatMessage(long sourceFarmer, int chatKind, string message)
		{
			string userName = Game1.content.LoadString("Strings\\UI:Chat_UnknownUserName");
			if (sourceFarmer == Game1.player.UniqueMultiplayerID)
			{
				userName = formattedUserName(Game1.player);
			}
			if (Game1.otherFarmers.ContainsKey(sourceFarmer))
			{
				userName = formattedUserName(Game1.otherFarmers[sourceFarmer]);
			}
			switch (chatKind)
			{
			case 0:
				return Game1.content.LoadString("Strings\\UI:Chat_ChatMessageFormat", userName, message);
			case 2:
				return Game1.content.LoadString("Strings\\UI:Chat_UserNotificationMessageFormat", message);
			case 3:
				return Game1.content.LoadString("Strings\\UI:Chat_PrivateMessageFormat", userName, message);
			default:
				return Game1.content.LoadString("Strings\\UI:Chat_ErrorMessageFormat", message);
			}
		}

		protected virtual Color messageColor(int chatKind)
		{
			switch (chatKind)
			{
			case 0:
				return chatBox.TextColor;
			case 3:
				return Color.DarkCyan;
			case 2:
				return Color.Yellow;
			default:
				return Color.Red;
			}
		}

		public virtual void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
		{
			string text = formatMessage(sourceFarmer, chatKind, message);
			ChatMessage c = new ChatMessage();
			string s = Game1.parseText(text, chatBox.Font, chatBox.Width - 16);
			c.timeLeftToDisplay = 600;
			c.verticalSize = (int)chatBox.Font.MeasureString(s).Y + 4;
			c.color = messageColor(chatKind);
			c.language = language;
			c.parseMessageForEmoji(s);
			messages.Add(c);
			if (messages.Count > maxMessages)
			{
				messages.RemoveAt(0);
			}
			if (chatKind == 3 && sourceFarmer != Game1.player.UniqueMultiplayerID)
			{
				lastReceivedPrivateMessagePlayerId = sourceFarmer;
			}
		}

		public virtual void addMessage(string message, Color color)
		{
			ChatMessage c = new ChatMessage();
			string s = Game1.parseText(message, chatBox.Font, chatBox.Width - 8);
			c.timeLeftToDisplay = 600;
			c.verticalSize = (int)chatBox.Font.MeasureString(s).Y + 4;
			c.color = color;
			c.language = LocalizedContentManager.CurrentLanguageCode;
			c.parseMessageForEmoji(s);
			messages.Add(c);
			if (messages.Count > maxMessages)
			{
				messages.RemoveAt(0);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			emojiMenuIcon.tryHover(x, y, 1f);
			emojiMenuIcon.tryHover(x, y, 1f);
		}

		public override void update(GameTime time)
		{
			KeyboardState keyState = Game1.input.GetKeyboardState();
			Keys[] pressedKeys = keyState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				if (!oldKBState.IsKeyDown(key))
				{
					receiveKeyPress(key);
				}
			}
			oldKBState = keyState;
			for (int i = 0; i < messages.Count; i++)
			{
				if (messages[i].timeLeftToDisplay > 0)
				{
					messages[i].timeLeftToDisplay--;
				}
				if (messages[i].timeLeftToDisplay < 75)
				{
					messages[i].alpha = (float)messages[i].timeLeftToDisplay / 75f;
				}
			}
			if (chatBox.Selected)
			{
				foreach (ChatMessage message in messages)
				{
					message.alpha = 1f;
				}
			}
			emojiMenuIcon.tryHover(0, 0, 1f);
		}

		public override void receiveScrollWheelAction(int direction)
		{
			if (choosingEmoji)
			{
				emojiMenu.receiveScrollWheelAction(direction);
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			updatePosition();
		}

		public static SpriteFont messageFont(LocalizedContentManager.LanguageCode language)
		{
			return Game1.content.Load<SpriteFont>("Fonts\\SmallFont", language);
		}

		public int getOldMessagesBoxHeight()
		{
			int heightSoFar = 20;
			for (int i = messages.Count - 1; i >= 0; i--)
			{
				ChatMessage message = messages[i];
				if (chatBox.Selected || message.alpha > 0.01f)
				{
					heightSoFar += message.verticalSize;
				}
			}
			return heightSoFar;
		}

		public override void draw(SpriteBatch b)
		{
			int heightSoFar2 = 0;
			bool drawBG = false;
			for (int j = messages.Count - 1; j >= 0; j--)
			{
				ChatMessage message = messages[j];
				if (chatBox.Selected || message.alpha > 0.01f)
				{
					heightSoFar2 += message.verticalSize;
					drawBG = true;
				}
			}
			if (drawBG)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), xPositionOnScreen, yPositionOnScreen - heightSoFar2 - 20 + ((!chatBox.Selected) ? chatBox.Height : 0), chatBox.Width, heightSoFar2 + 20, Color.White, 4f, drawShadow: false);
			}
			heightSoFar2 = 0;
			for (int i = messages.Count - 1; i >= 0; i--)
			{
				ChatMessage message2 = messages[i];
				heightSoFar2 += message2.verticalSize;
				message2.draw(b, 12, yPositionOnScreen - heightSoFar2 - 8 + ((!chatBox.Selected) ? chatBox.Height : 0));
			}
			if (chatBox.Selected)
			{
				chatBox.Draw(b, drawShadow: false);
				emojiMenuIcon.draw(b, Color.White, 0.99f);
				if (choosingEmoji)
				{
					emojiMenu.draw(b);
				}
				if (isWithinBounds(Game1.getMouseX(), Game1.getMouseY()) && !Game1.options.hardwareCursor)
				{
					Game1.mouseCursor = (Game1.options.gamepadControls ? 44 : 0);
				}
			}
		}
	}
}
