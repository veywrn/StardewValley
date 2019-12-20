using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StardewValley.Menus
{
	public class LoadGameMenu : IClickableMenu, IDisposable
	{
		public abstract class MenuSlot : IDisposable
		{
			public int ActivateDelay;

			protected LoadGameMenu menu;

			public MenuSlot(LoadGameMenu menu)
			{
				this.menu = menu;
			}

			public abstract void Activate();

			public abstract void Draw(SpriteBatch b, int i);

			public virtual void Dispose()
			{
			}
		}

		public class SaveFileSlot : MenuSlot
		{
			public Farmer Farmer;

			public double redTimer;

			public int versionComparison;

			public SaveFileSlot(LoadGameMenu menu, Farmer farmer)
				: base(menu)
			{
				ActivateDelay = 2150;
				Farmer = farmer;
				versionComparison = Utility.CompareGameVersions(Game1.version, farmer.gameVersion, ignore_platform_specific: true);
			}

			public override void Activate()
			{
				SaveGame.Load(Farmer.slotName);
				Game1.exitActiveMenu();
			}

			protected virtual void drawSlotSaveNumber(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, menu.currentItemIndex + i + 1 + ".", menu.slotButtons[i].bounds.X + 28 + 32 - SpriteText.getWidthOfString(menu.currentItemIndex + i + 1 + ".") / 2, menu.slotButtons[i].bounds.Y + 36);
			}

			protected virtual string slotName()
			{
				return Farmer.Name;
			}

			protected virtual void drawSlotName(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, slotName(), menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36);
			}

			protected virtual void drawSlotShadow(SpriteBatch b, int i)
			{
				Vector2 offset = portraitOffset();
				b.Draw(Game1.shadowTexture, new Vector2((float)menu.slotButtons[i].bounds.X + offset.X + 32f, menu.slotButtons[i].bounds.Y + 128 + 16), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.8f);
			}

			protected virtual Vector2 portraitOffset()
			{
				return new Vector2(92f, 20f);
			}

			protected virtual void drawSlotFarmer(SpriteBatch b, int i)
			{
				Vector2 offset = portraitOffset();
				FarmerRenderer.isDrawingForUI = true;
				Farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, 0, secondaryArm: false, flip: false), 0, new Rectangle(0, 0, 16, 32), new Vector2((float)menu.slotButtons[i].bounds.X + offset.X, (float)menu.slotButtons[i].bounds.Y + offset.Y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Farmer);
				FarmerRenderer.isDrawingForUI = false;
			}

			protected virtual void drawSlotDate(SpriteBatch b, int i)
			{
				string dateStringForSaveGame = (!Farmer.dayOfMonthForSaveGame.HasValue || !Farmer.seasonForSaveGame.HasValue || !Farmer.yearForSaveGame.HasValue) ? Farmer.dateStringForSaveGame : Utility.getDateStringFor(Farmer.dayOfMonthForSaveGame.Value, Farmer.seasonForSaveGame.Value, Farmer.yearForSaveGame.Value);
				Utility.drawTextWithShadow(b, dateStringForSaveGame, Game1.dialogueFont, new Vector2(menu.slotButtons[i].bounds.X + 128 + 32, menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor);
			}

			protected virtual string slotSubName()
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11019", Farmer.farmName);
			}

			protected virtual void drawSlotSubName(SpriteBatch b, int i)
			{
				string subName = slotSubName();
				Utility.drawTextWithShadow(b, subName, Game1.dialogueFont, new Vector2((float)(menu.slotButtons[i].bounds.X + menu.width - 128) - Game1.dialogueFont.MeasureString(subName).X, menu.slotButtons[i].bounds.Y + 44), Game1.textColor);
			}

			protected virtual void drawSlotMoney(SpriteBatch b, int i)
			{
				string cashText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(Farmer.Money));
				if (Farmer.Money == 1 && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt)
				{
					cashText = cashText.Substring(0, cashText.Length - 1);
				}
				int moneyWidth = (int)Game1.dialogueFont.MeasureString(cashText).X;
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(menu.slotButtons[i].bounds.X + menu.width - 192 - 100 - moneyWidth, menu.slotButtons[i].bounds.Y + 64 + 44), new Rectangle(193, 373, 9, 9), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Vector2 position = new Vector2(menu.slotButtons[i].bounds.X + menu.width - 192 - 60 - moneyWidth, menu.slotButtons[i].bounds.Y + 64 + 44);
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					position.Y += 5f;
				}
				Utility.drawTextWithShadow(b, cashText, Game1.dialogueFont, position, Game1.textColor);
			}

			protected virtual void drawSlotTimer(SpriteBatch b, int i)
			{
				Vector2 position = new Vector2(menu.slotButtons[i].bounds.X + menu.width - 192 - 44, menu.slotButtons[i].bounds.Y + 64 + 36);
				Utility.drawWithShadow(b, Game1.mouseCursors, position, new Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				position = new Vector2(menu.slotButtons[i].bounds.X + menu.width - 192 - 4, menu.slotButtons[i].bounds.Y + 64 + 44);
				if (LocalizedContentManager.CurrentLanguageCode != 0)
				{
					position.Y += 5f;
				}
				Utility.drawTextWithShadow(b, Utility.getHoursMinutesStringFromMilliseconds(Farmer.millisecondsPlayed), Game1.dialogueFont, position, Game1.textColor);
			}

			public virtual void drawVersionMismatchSlot(SpriteBatch b, int i)
			{
				SpriteText.drawString(b, slotName(), menu.slotButtons[i].bounds.X + 128, menu.slotButtons[i].bounds.Y + 36);
				string farm_name = slotSubName();
				Utility.drawTextWithShadow(b, farm_name, Game1.dialogueFont, new Vector2((float)(menu.slotButtons[i].bounds.X + menu.width - 128) - Game1.dialogueFont.MeasureString(farm_name).X, menu.slotButtons[i].bounds.Y + 44), Game1.textColor);
				string game_version = Farmer.gameVersion;
				if (game_version == "-1")
				{
					game_version = "<1.4";
				}
				string mismatch_text = Game1.content.LoadString("Strings\\UI:VersionMismatch", game_version);
				Color text_color = Game1.textColor;
				if (Game1.currentGameTime.TotalGameTime.TotalSeconds < redTimer && (int)((redTimer - Game1.currentGameTime.TotalGameTime.TotalSeconds) / 0.25) % 2 == 1)
				{
					text_color = Color.Red;
				}
				Utility.drawTextWithShadow(b, mismatch_text, Game1.dialogueFont, new Vector2(menu.slotButtons[i].bounds.X + 128, menu.slotButtons[i].bounds.Y + 64 + 40), text_color);
			}

			public override void Draw(SpriteBatch b, int i)
			{
				drawSlotSaveNumber(b, i);
				if (versionComparison < 0)
				{
					drawVersionMismatchSlot(b, i);
					return;
				}
				drawSlotName(b, i);
				drawSlotShadow(b, i);
				drawSlotFarmer(b, i);
				drawSlotDate(b, i);
				drawSlotSubName(b, i);
				drawSlotMoney(b, i);
				drawSlotTimer(b, i);
			}

			public new void Dispose()
			{
				Farmer.unload();
			}
		}

		public const int region_upArrow = 800;

		public const int region_downArrow = 801;

		public const int region_okDelete = 802;

		public const int region_cancelDelete = 803;

		public const int region_slots = 900;

		public const int region_deleteButtons = 901;

		public const int region_navigationButtons = 902;

		public const int region_deleteConfirmations = 903;

		public const int itemsPerPage = 4;

		public List<ClickableComponent> slotButtons = new List<ClickableComponent>();

		public List<ClickableTextureComponent> deleteButtons = new List<ClickableTextureComponent>();

		protected int currentItemIndex;

		protected int timerToLoad;

		protected int selected = -1;

		protected int selectedForDelete = -1;

		public ClickableTextureComponent upArrow;

		public ClickableTextureComponent downArrow;

		public ClickableTextureComponent scrollBar;

		public ClickableTextureComponent okDeleteButton;

		public ClickableTextureComponent cancelDeleteButton;

		public ClickableComponent backButton;

		public bool scrolling;

		public bool deleteConfirmationScreen;

		protected List<MenuSlot> menuSlots = new List<MenuSlot>();

		private Rectangle scrollBarRunner;

		private string hoverText = "";

		protected bool loading;

		protected bool drawn;

		private bool deleting;

		private int _updatesSinceLastDeleteConfirmScreen;

		private Task<List<Farmer>> _initTask;

		private Task _deleteTask;

		private bool disposedValue;

		protected virtual List<MenuSlot> MenuSlots
		{
			get
			{
				return menuSlots;
			}
			set
			{
				menuSlots = value;
			}
		}

		public bool IsDoingTask()
		{
			if (_initTask == null && _deleteTask == null && !loading)
			{
				return deleting;
			}
			return true;
		}

		public override bool readyToClose()
		{
			if (!IsDoingTask())
			{
				return _updatesSinceLastDeleteConfirmScreen > 1;
			}
			return false;
		}

		public LoadGameMenu()
			: base(Game1.viewport.Width / 2 - (1100 + IClickableMenu.borderWidth * 2) / 2, Game1.viewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1100 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2)
		{
			backButton = new ClickableComponent(new Rectangle(Game1.viewport.Width + -198 - 48, Game1.viewport.Height - 81 - 24, 198, 81), "")
			{
				myID = 81114,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			upArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 16, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
			{
				myID = 800,
				downNeighborID = 801,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				region = 902
			};
			downArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
			{
				myID = 801,
				upNeighborID = 800,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = -99998,
				region = 902
			};
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
			okDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10992"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X - 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 802,
				rightNeighborID = 803,
				region = 903
			};
			cancelDeleteButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993"), new Rectangle((int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X + 64, (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 803,
				leftNeighborID = 802,
				region = 903
			};
			for (int i = 0; i < 4; i++)
			{
				slotButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * (height / 4), width - 32, height / 4 + 4), string.Concat(i))
				{
					myID = i,
					region = 900,
					downNeighborID = ((i < 3) ? (-99998) : (-7777)),
					upNeighborID = ((i > 0) ? (-99998) : (-7777)),
					rightNeighborID = -99998,
					fullyImmutable = true
				});
				if (hasDeleteButtons())
				{
					deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width - 64 - 4, yPositionOnScreen + 32 + 4 + i * (height / 4), 48, 48), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994"), Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f)
					{
						myID = i + 100,
						region = 901,
						leftNeighborID = -99998,
						downNeighborImmutable = true,
						downNeighborID = -99998,
						upNeighborImmutable = true,
						upNeighborID = ((i > 0) ? (-99998) : (-1)),
						rightNeighborID = -99998
					});
				}
			}
			startListPopulation();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			UpdateButtons();
		}

		protected virtual bool hasDeleteButtons()
		{
			return true;
		}

		protected virtual void startListPopulation()
		{
			_initTask = new Task<List<Farmer>>(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				return FindSaveGames();
			});
			_initTask.Start();
		}

		public virtual void UpdateButtons()
		{
			for (int i = 0; i < slotButtons.Count; i++)
			{
				ClickableTextureComponent delete_button = null;
				if (hasDeleteButtons() && i >= 0 && i < deleteButtons.Count)
				{
					delete_button = deleteButtons[i];
				}
				if (currentItemIndex + i < MenuSlots.Count)
				{
					slotButtons[i].visible = true;
					if (delete_button != null)
					{
						delete_button.visible = true;
					}
				}
				else
				{
					slotButtons[i].visible = false;
					if (delete_button != null)
					{
						delete_button.visible = false;
					}
				}
			}
		}

		protected virtual void addSaveFiles(List<Farmer> files)
		{
			MenuSlots.AddRange(((IEnumerable<Farmer>)files).Select((Func<Farmer, MenuSlot>)((Farmer file) => new SaveFileSlot(this, file))));
			UpdateButtons();
		}

		private static List<Farmer> FindSaveGames()
		{
			List<Farmer> results = new List<Farmer>();
			string pathToDirectory = Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"));
			if (Directory.Exists(pathToDirectory))
			{
				string[] directories = Directory.GetDirectories(pathToDirectory);
				foreach (string s in directories)
				{
					string pathToFile = Path.Combine(pathToDirectory, s, "SaveGameInfo");
					Farmer f = null;
					try
					{
						using (FileStream stream = File.OpenRead(pathToFile))
						{
							f = (Farmer)SaveGame.farmerSerializer.Deserialize(stream);
							SaveGame.loadDataToFarmer(f);
							f.slotName = s.Split(Path.DirectorySeparatorChar).Last();
							results.Add(f);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Exception occured trying to access file '{0}'", pathToFile);
						Console.WriteLine(ex.GetBaseException().ToString());
						f?.unload();
					}
				}
			}
			results.Sort();
			return results;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			if (b == Buttons.B && deleteConfirmationScreen)
			{
				deleteConfirmationScreen = false;
				selectedForDelete = -1;
				Game1.playSound("smallSelect");
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					currentlySnappedComponent = getComponentWithID(0);
					snapCursorToCurrentSnappedComponent();
				}
			}
		}

		public override void snapToDefaultClickableComponent()
		{
			if (deleteConfirmationScreen)
			{
				currentlySnappedComponent = getComponentWithID(803);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID(0);
			}
			snapCursorToCurrentSnappedComponent();
		}

		protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
		{
			if (direction == 2 && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
			{
				downArrowPressed();
				currentlySnappedComponent = getComponentWithID(3);
				snapCursorToCurrentSnappedComponent();
			}
			else if (direction == 0 && currentItemIndex > 0)
			{
				upArrowPressed();
				currentlySnappedComponent = getComponentWithID(0);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
		{
			xPositionOnScreen = (newBounds.Width - width) / 2;
			yPositionOnScreen = (newBounds.Height - (height + 32)) / 2;
			backButton.bounds.X = Game1.viewport.Width + -198 - 48;
			backButton.bounds.Y = Game1.viewport.Height - 81 - 24;
			upArrow.bounds.X = xPositionOnScreen + width + 16;
			upArrow.bounds.Y = yPositionOnScreen + 16;
			downArrow.bounds.X = xPositionOnScreen + width + 16;
			downArrow.bounds.Y = yPositionOnScreen + height - 64;
			scrollBar = new ClickableTextureComponent(new Rectangle(upArrow.bounds.X + 12, upArrow.bounds.Y + upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
			scrollBarRunner = new Rectangle(scrollBar.bounds.X, upArrow.bounds.Y + upArrow.bounds.Height + 4, scrollBar.bounds.Width, height - 64 - upArrow.bounds.Height - 28);
			okDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X - 64;
			okDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128;
			cancelDeleteButton.bounds.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).X + 64;
			cancelDeleteButton.bounds.Y = (int)Utility.getTopLeftPositionForCenteringOnScreen(64, 64).Y + 128;
			for (int j = 0; j < slotButtons.Count; j++)
			{
				slotButtons[j].bounds.X = xPositionOnScreen + 16;
				slotButtons[j].bounds.Y = yPositionOnScreen + 16 + j * (height / 4);
			}
			for (int i = 0; i < deleteButtons.Count; i++)
			{
				deleteButtons[i].bounds.X = xPositionOnScreen + width - 64 - 4;
				deleteButtons[i].bounds.Y = yPositionOnScreen + 32 + 4 + i * (height / 4);
			}
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				int id = (currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 81114;
				populateClickableComponentList();
				currentlySnappedComponent = getComponentWithID(id);
				snapCursorToCurrentSnappedComponent();
			}
		}

		public override void performHoverAction(int x, int y)
		{
			hoverText = "";
			base.performHoverAction(x, y);
			if (deleteConfirmationScreen)
			{
				okDeleteButton.tryHover(x, y);
				cancelDeleteButton.tryHover(x, y);
				if (okDeleteButton.containsPoint(x, y))
				{
					hoverText = "";
				}
				else if (cancelDeleteButton.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10993");
				}
				return;
			}
			upArrow.tryHover(x, y);
			downArrow.tryHover(x, y);
			scrollBar.tryHover(x, y);
			foreach (ClickableTextureComponent deleteButton in deleteButtons)
			{
				deleteButton.tryHover(x, y, 0.2f);
				if (deleteButton.containsPoint(x, y))
				{
					hoverText = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.10994");
					return;
				}
			}
			if (scrolling)
			{
				return;
			}
			for (int i = 0; i < slotButtons.Count; i++)
			{
				if (currentItemIndex + i < MenuSlots.Count && slotButtons[i].containsPoint(x, y))
				{
					if (slotButtons[i].scale == 1f)
					{
						Game1.playSound("Cowboy_gunshot");
					}
					slotButtons[i].scale = Math.Min(slotButtons[i].scale + 0.03f, 1.1f);
				}
				else
				{
					slotButtons[i].scale = Math.Max(1f, slotButtons[i].scale - 0.03f);
				}
			}
		}

		public override void leftClickHeld(int x, int y)
		{
			base.leftClickHeld(x, y);
			if (scrolling)
			{
				int y2 = scrollBar.bounds.Y;
				scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upArrow.bounds.Height + 20));
				float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
				currentItemIndex = Math.Min(MenuSlots.Count - 4, Math.Max(0, (int)((float)MenuSlots.Count * percentage)));
				setScrollBarToCurrentIndex();
				if (y2 != scrollBar.bounds.Y)
				{
					Game1.playSound("shiny4");
				}
			}
		}

		public override void releaseLeftClick(int x, int y)
		{
			base.releaseLeftClick(x, y);
			scrolling = false;
		}

		protected void setScrollBarToCurrentIndex()
		{
			if (MenuSlots.Count > 0)
			{
				scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, MenuSlots.Count - 4 + 1) * currentItemIndex + upArrow.bounds.Bottom + 4;
				if (currentItemIndex == MenuSlots.Count - 4)
				{
					scrollBar.bounds.Y = downArrow.bounds.Y - scrollBar.bounds.Height - 4;
				}
			}
			UpdateButtons();
		}

		public override void receiveScrollWheelAction(int direction)
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && currentItemIndex > 0)
			{
				upArrowPressed();
			}
			else if (direction < 0 && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
			{
				downArrowPressed();
			}
		}

		private void downArrowPressed()
		{
			downArrow.scale = downArrow.baseScale;
			currentItemIndex++;
			Game1.playSound("shwip");
			setScrollBarToCurrentIndex();
		}

		private void upArrowPressed()
		{
			upArrow.scale = upArrow.baseScale;
			currentItemIndex--;
			Game1.playSound("shwip");
			setScrollBarToCurrentIndex();
		}

		private void deleteFile(int which)
		{
			SaveFileSlot slot = MenuSlots[which] as SaveFileSlot;
			if (slot != null)
			{
				string filenameNoTmpString = slot.Farmer.slotName;
				string fullFilePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), filenameNoTmpString));
				Thread.Sleep(Game1.random.Next(1000, 5000));
				if (Directory.Exists(fullFilePath))
				{
					Directory.Delete(fullFilePath, recursive: true);
				}
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (timerToLoad > 0 || loading || deleting)
			{
				return;
			}
			if (deleteConfirmationScreen)
			{
				if (cancelDeleteButton.containsPoint(x, y))
				{
					deleteConfirmationScreen = false;
					selectedForDelete = -1;
					Game1.playSound("smallSelect");
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						currentlySnappedComponent = getComponentWithID(0);
						snapCursorToCurrentSnappedComponent();
					}
				}
				else if (okDeleteButton.containsPoint(x, y))
				{
					deleting = true;
					_deleteTask = new Task(delegate
					{
						Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
						deleteFile(selectedForDelete);
					});
					_deleteTask.Start();
					deleteConfirmationScreen = false;
					if (Game1.options.snappyMenus && Game1.options.gamepadControls)
					{
						currentlySnappedComponent = getComponentWithID(0);
						snapCursorToCurrentSnappedComponent();
					}
					Game1.playSound("trashcan");
				}
				return;
			}
			base.receiveLeftClick(x, y, playSound);
			if (downArrow.containsPoint(x, y) && currentItemIndex < Math.Max(0, MenuSlots.Count - 4))
			{
				downArrowPressed();
			}
			else if (upArrow.containsPoint(x, y) && currentItemIndex > 0)
			{
				upArrowPressed();
			}
			else if (scrollBar.containsPoint(x, y))
			{
				scrolling = true;
			}
			else if (!downArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
			{
				scrolling = true;
				leftClickHeld(x, y);
				releaseLeftClick(x, y);
			}
			if (selected == -1)
			{
				for (int i = 0; i < deleteButtons.Count; i++)
				{
					if (deleteButtons[i].containsPoint(x, y) && i < MenuSlots.Count && !deleteConfirmationScreen)
					{
						deleteConfirmationScreen = true;
						Game1.playSound("drumkit6");
						selectedForDelete = currentItemIndex + i;
						if (Game1.options.snappyMenus && Game1.options.gamepadControls)
						{
							currentlySnappedComponent = getComponentWithID(803);
							snapCursorToCurrentSnappedComponent();
						}
						return;
					}
				}
			}
			if (!deleteConfirmationScreen)
			{
				for (int j = 0; j < slotButtons.Count; j++)
				{
					if (!slotButtons[j].containsPoint(x, y) || j >= MenuSlots.Count)
					{
						continue;
					}
					SaveFileSlot menu_save_slot;
					if ((menu_save_slot = (MenuSlots[currentItemIndex + j] as SaveFileSlot)) != null && menu_save_slot.versionComparison < 0)
					{
						menu_save_slot.redTimer = Game1.currentGameTime.TotalGameTime.TotalSeconds + 1.0;
						Game1.playSound("cancel");
						continue;
					}
					Game1.playSound("select");
					timerToLoad = MenuSlots[currentItemIndex + j].ActivateDelay;
					if (timerToLoad > 0)
					{
						loading = true;
						selected = currentItemIndex + j;
					}
					else
					{
						MenuSlots[currentItemIndex + j].Activate();
					}
					return;
				}
			}
			currentItemIndex = Math.Max(0, Math.Min(MenuSlots.Count - 4, currentItemIndex));
		}

		protected virtual void saveFileScanComplete()
		{
		}

		protected virtual bool checkListPopulation()
		{
			if (!deleteConfirmationScreen)
			{
				_updatesSinceLastDeleteConfirmScreen++;
			}
			else
			{
				_updatesSinceLastDeleteConfirmScreen = 0;
			}
			if (_initTask != null)
			{
				if (_initTask.IsCanceled || _initTask.IsCompleted || _initTask.IsFaulted)
				{
					if (_initTask.IsCompleted)
					{
						addSaveFiles(_initTask.Result);
						saveFileScanComplete();
					}
					_initTask = null;
				}
				return true;
			}
			return false;
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (checkListPopulation())
			{
				return;
			}
			if (_deleteTask != null)
			{
				if (_deleteTask.IsCanceled || _deleteTask.IsCompleted || _deleteTask.IsFaulted)
				{
					if (!_deleteTask.IsCompleted)
					{
						selectedForDelete = -1;
					}
					_deleteTask = null;
					deleting = false;
				}
				return;
			}
			if (selectedForDelete != -1 && !deleteConfirmationScreen && !deleting)
			{
				SaveFileSlot slot = MenuSlots[selectedForDelete] as SaveFileSlot;
				if (slot != null)
				{
					slot.Farmer.unload();
					MenuSlots.RemoveAt(selectedForDelete);
					selectedForDelete = -1;
					slotButtons.Clear();
					deleteButtons.Clear();
					for (int i = 0; i < 4; i++)
					{
						slotButtons.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16, yPositionOnScreen + 16 + i * (height / 4), width - 32, height / 4 + 4), string.Concat(i)));
						if (hasDeleteButtons())
						{
							deleteButtons.Add(new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width - 64 - 4, yPositionOnScreen + 32 + 4 + i * (height / 4), 48, 48), "", "Delete File", Game1.mouseCursors, new Rectangle(322, 498, 12, 12), 3f));
						}
					}
				}
			}
			if (timerToLoad <= 0)
			{
				return;
			}
			timerToLoad -= time.ElapsedGameTime.Milliseconds;
			if (timerToLoad <= 0)
			{
				if (MenuSlots.Count > selected)
				{
					MenuSlots[selected].Activate();
				}
				else
				{
					Game1.ExitToTitle();
				}
			}
		}

		protected virtual string getStatusText()
		{
			if (_initTask != null)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_LookingForSavedGames");
			}
			if (deleting)
			{
				return Game1.content.LoadString("Strings\\UI:LoadGameMenu_Deleting");
			}
			if (MenuSlots.Count == 0)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11022");
			}
			return null;
		}

		protected virtual void drawExtra(SpriteBatch b)
		{
		}

		protected virtual void drawSlotBackground(SpriteBatch b, int i, MenuSlot slot)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), slotButtons[i].bounds.X, slotButtons[i].bounds.Y, slotButtons[i].bounds.Width, slotButtons[i].bounds.Height, ((currentItemIndex + i != selected || timerToLoad % 150 <= 75 || timerToLoad <= 1000) && (selected != -1 || !(slotButtons[i].scale > 1f) || scrolling || deleteConfirmationScreen)) ? Color.White : ((deleteButtons.Count > i && deleteButtons[i].containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY())) ? Color.White : Color.Wheat), 4f, drawShadow: false);
		}

		protected virtual void drawBefore(SpriteBatch b)
		{
		}

		protected virtual void drawStatusText(SpriteBatch b)
		{
			string text = getStatusText();
			if (text != null)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
			}
		}

		public override void draw(SpriteBatch b)
		{
			drawBefore(b);
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), xPositionOnScreen, yPositionOnScreen, width, height + 32, Color.White, 4f);
			if (selectedForDelete == -1 || !deleting || deleteConfirmationScreen)
			{
				for (int i = 0; i < slotButtons.Count; i++)
				{
					if (currentItemIndex + i < MenuSlots.Count)
					{
						drawSlotBackground(b, i, MenuSlots[currentItemIndex + i]);
						MenuSlots[currentItemIndex + i].Draw(b, i);
						if (deleteButtons.Count > i)
						{
							deleteButtons[i].draw(b, Color.White * 0.75f, 1f);
						}
					}
				}
			}
			drawStatusText(b);
			upArrow.draw(b);
			downArrow.draw(b);
			if (MenuSlots.Count > 4)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
				scrollBar.draw(b);
			}
			if (deleteConfirmationScreen && MenuSlots[selectedForDelete] is SaveFileSlot)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * 0.75f);
				string toDisplay = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11023", (MenuSlots[selectedForDelete] as SaveFileSlot).Farmer.Name);
				int middlePosX = okDeleteButton.bounds.X + (cancelDeleteButton.bounds.X - okDeleteButton.bounds.X) / 2 + okDeleteButton.bounds.Width / 2;
				SpriteText.drawString(b, toDisplay, middlePosX - SpriteText.getWidthOfString(toDisplay) / 2, (int)Utility.getTopLeftPositionForCenteringOnScreen(192, 64).Y, 9999, -1, 9999, 1f, 1f, junimoText: false, -1, "", 4);
				okDeleteButton.draw(b);
				cancelDeleteButton.draw(b);
			}
			base.draw(b);
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
			drawExtra(b);
			if (selected != -1 && timerToLoad < 1000)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.Black * (1f - (float)timerToLoad / 1000f));
			}
			drawn = true;
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposedValue)
			{
				return;
			}
			if (disposing)
			{
				if (MenuSlots != null)
				{
					foreach (MenuSlot menuSlot in MenuSlots)
					{
						menuSlot.Dispose();
					}
					MenuSlots.Clear();
					MenuSlots = null;
				}
				if (_initTask != null)
				{
					_initTask = null;
				}
				if (_deleteTask != null)
				{
					_deleteTask = null;
				}
			}
			disposedValue = true;
		}

		~LoadGameMenu()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
		{
			if (a.region == 901 && b.region != 901 && direction == 2 && b.myID != 81114)
			{
				return true;
			}
			if (a.region == 901 && direction == 3 && b.region != 900)
			{
				return false;
			}
			if (direction == 1 && a.region == 900 && hasDeleteButtons() && b.region != 901)
			{
				return false;
			}
			if (a.region != 903 && b.region == 903)
			{
				return false;
			}
			if ((direction == 0 || direction == 2) && a.myID == 81114 && b.region == 902)
			{
				return false;
			}
			return base.IsAutomaticSnapValid(direction, a, b);
		}

		protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
		{
			return false;
		}
	}
}
