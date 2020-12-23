using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using xTile.Dimensions;

namespace StardewValley.Menus
{
	public class RenovateMenu : IClickableMenu
	{
		public const int region_okButton = 101;

		public const int region_randomButton = 103;

		public const int region_namingBox = 104;

		public static int menuHeight = 320;

		public static int menuWidth = 448;

		public ClickableTextureComponent okButton;

		public ClickableTextureComponent hovered;

		private bool freeze;

		protected HouseRenovation _renovation;

		protected string _oldLocation;

		protected Point _oldPosition;

		protected int _selectedIndex = -1;

		protected int _animatingIndex = -1;

		protected int _buildAnimationTimer;

		protected int _buildAnimationCount;

		public RenovateMenu(HouseRenovation renovation)
			: base(Game1.uiViewport.Width / 2 - menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - menuHeight - IClickableMenu.borderWidth * 2) / 4, menuWidth + IClickableMenu.borderWidth * 2, menuHeight + IClickableMenu.borderWidth)
		{
			height += 64;
			okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
			{
				myID = 101,
				upNeighborID = 103,
				leftNeighborID = 103
			};
			_renovation = renovation;
			menuHeight = 320;
			menuWidth = 448;
			if (Game1.options.SnappyMenus)
			{
				populateClickableComponentList();
				snapToDefaultClickableComponent();
			}
			SetupForRenovationPlacement();
		}

		public override bool shouldClampGamePadCursor()
		{
			return true;
		}

		public override void snapToDefaultClickableComponent()
		{
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}

		public void SetupForReturn()
		{
			freeze = true;
			LocationRequest locationRequest = Game1.getLocationRequest(_oldLocation);
			locationRequest.OnWarp += delegate
			{
				Console.WriteLine("Display farmer true");
				Game1.player.viewingLocation.Value = null;
				Game1.displayHUD = true;
				Game1.displayFarmer = true;
				freeze = false;
				Game1.viewportFreeze = false;
				FinalizeReturn();
			};
			Game1.warpFarmer(locationRequest, _oldPosition.X, _oldPosition.Y, Game1.player.facingDirection);
		}

		public void FinalizeReturn()
		{
			exitThisMenu(playSound: false);
			Game1.player.forceCanMove();
			freeze = false;
		}

		public void SetupForRenovationPlacement()
		{
			Game1.currentLocation.cleanupBeforePlayerExit();
			Game1.displayFarmer = false;
			_oldLocation = Game1.currentLocation.NameOrUniqueName;
			_oldPosition = Game1.player.getTileLocationPoint();
			Game1.currentLocation = _renovation.location;
			Game1.player.viewingLocation.Value = _renovation.location.NameOrUniqueName;
			Game1.currentLocation.resetForPlayerEntry();
			Game1.globalFadeToClear();
			freeze = false;
			okButton.bounds.X = Game1.uiViewport.Width - 128;
			okButton.bounds.Y = Game1.uiViewport.Height - 128;
			Game1.displayHUD = false;
			Game1.viewportFreeze = true;
			Vector2 center = default(Vector2);
			int count = 0;
			foreach (List<Microsoft.Xna.Framework.Rectangle> renovationBound in _renovation.renovationBounds)
			{
				foreach (Microsoft.Xna.Framework.Rectangle rectangle in renovationBound)
				{
					center.X += rectangle.Center.X;
					center.Y += rectangle.Center.Y;
					count++;
				}
			}
			if (count > 0)
			{
				center.X = (int)Math.Round(center.X / (float)count);
				center.Y = (int)Math.Round(center.Y / (float)count);
			}
			Game1.viewport.Location = new Location((int)((center.X + 0.5f) * 64f) - Game1.viewport.Width / 2, (int)((center.Y + 0.5f) * 64f) - Game1.viewport.Height / 2);
			Game1.panScreen(0, 0);
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
			{
				SetupForReturn();
				Game1.playSound("smallSelect");
				return;
			}
			Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
			for (int i = 0; i < _renovation.renovationBounds.Count; i++)
			{
				foreach (Microsoft.Xna.Framework.Rectangle item in _renovation.renovationBounds[i])
				{
					if (item.Contains((int)clickTile.X, (int)clickTile.Y))
					{
						CompleteRenovation(i);
						return;
					}
				}
			}
		}

		public virtual void AnimateRenovation()
		{
			if (_buildAnimationTimer == 0)
			{
				return;
			}
			_buildAnimationTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (_buildAnimationTimer > 0)
			{
				return;
			}
			if (_buildAnimationCount > 0)
			{
				_buildAnimationCount--;
				if (_renovation.animationType == HouseRenovation.AnimationType.Destroy)
				{
					_buildAnimationTimer = 50;
					for (int j = 0; j < 5; j++)
					{
						Microsoft.Xna.Framework.Rectangle rectangle2 = Utility.GetRandom(_renovation.renovationBounds[_animatingIndex]);
						int x2 = (int)Utility.RandomFloat((rectangle2.Left - 1) * 64, 64 * rectangle2.Right);
						int y2 = (int)Utility.RandomFloat((rectangle2.Top - 1) * 64, 64 * rectangle2.Bottom);
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x2, y2), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x2, y2), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(x2, y2), flipped: false, 0f, Color.White)
						{
							interval = 30f,
							totalNumberOfLoops = 99999,
							animationLength = 4,
							scale = 4f,
							alphaFade = 0.01f
						});
					}
				}
				else
				{
					_buildAnimationTimer = 500;
					Game1.playSound("axe");
					for (int i = 0; i < 20; i++)
					{
						Microsoft.Xna.Framework.Rectangle rectangle = Utility.GetRandom(_renovation.renovationBounds[_animatingIndex]);
						int x = (int)Utility.RandomFloat((rectangle.Left - 1) * 64, 64 * rectangle.Right);
						int y = (int)Utility.RandomFloat((rectangle.Top - 1) * 64, 64 * rectangle.Bottom);
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(x, y), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
						_renovation.location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90) - 64, 6, 1, new Vector2(x, y), flicker: false, (Game1.random.NextDouble() < 0.5) ? true : false));
					}
				}
			}
			else
			{
				_buildAnimationTimer = 0;
				SetupForReturn();
			}
		}

		public virtual void CompleteRenovation(int selected_index)
		{
			if (_renovation.validate == null || _renovation.validate(_renovation, selected_index))
			{
				freeze = true;
				if (_renovation.animationType == HouseRenovation.AnimationType.Destroy)
				{
					Game1.playSound("explosion");
					_buildAnimationCount = 10;
				}
				else
				{
					_buildAnimationCount = 3;
				}
				_buildAnimationTimer = -1;
				_animatingIndex = _selectedIndex;
				if (_renovation.onRenovation != null)
				{
					_renovation.onRenovation(_renovation, selected_index);
					Game1.player.renovateEvent.Fire(_renovation.location.NameOrUniqueName);
				}
				AnimateRenovation();
			}
		}

		public override bool overrideSnappyMenuCursorMovementBan()
		{
			return true;
		}

		public override void receiveGamePadButton(Buttons b)
		{
			base.receiveGamePadButton(b);
			if (b == Buttons.B && !Game1.globalFade)
			{
				SetupForReturn();
				Game1.playSound("smallSelect");
			}
		}

		public override bool readyToClose()
		{
			if (freeze)
			{
				return false;
			}
			return base.readyToClose();
		}

		public override void receiveKeyPress(Keys key)
		{
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (!Game1.globalFade)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
				{
					SetupForReturn();
				}
				else if (!Game1.options.SnappyMenus && !freeze)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
					}
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.globalFade)
			{
				if (readyToClose())
				{
					Game1.player.forceCanMove();
					Game1.exitActiveMenu();
					Game1.playSound("bigDeSelect");
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				base.receiveKeyPress(key);
			}
		}

		public override void update(GameTime time)
		{
			base.update(time);
			AnimateRenovation();
			int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
			if (!freeze)
			{
				if (mouseX - Game1.viewport.X < 64)
				{
					Game1.panScreen(-8, 0);
				}
				else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
				{
					Game1.panScreen(8, 0);
				}
				if (mouseY - Game1.viewport.Y < 64)
				{
					Game1.panScreen(0, -8);
				}
				else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
				{
					Game1.panScreen(0, 8);
				}
			}
			Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				receiveKeyPress(key);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}

		public override void performHoverAction(int x, int y)
		{
			hovered = null;
			if (Game1.globalFade || freeze)
			{
				return;
			}
			if (okButton != null)
			{
				if (okButton.containsPoint(x, y))
				{
					okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
				}
				else
				{
					okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
				}
			}
			Vector2 clickTile = new Vector2((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f, (Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f);
			_selectedIndex = -1;
			for (int i = 0; i < _renovation.renovationBounds.Count; i++)
			{
				foreach (Microsoft.Xna.Framework.Rectangle item in _renovation.renovationBounds[i])
				{
					if (item.Contains((int)clickTile.X, (int)clickTile.Y))
					{
						_selectedIndex = i;
						break;
					}
				}
			}
		}

		public static string getAnimalTitle(string name)
		{
			switch (name)
			{
			case "Chicken":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5922");
			case "Duck":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5937");
			case "Rabbit":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5945");
			case "Dairy Cow":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5927");
			case "Pig":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5948");
			case "Goat":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5933");
			case "Sheep":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5942");
			default:
				return "";
			}
		}

		public static string getAnimalDescription(string name)
		{
			switch (name)
			{
			case "Chicken":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11334") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Duck":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11337") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Rabbit":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11340") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11335");
			case "Dairy Cow":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11343") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Pig":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11346") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Goat":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11349") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			case "Sheep":
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11352") + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11344");
			default:
				return "";
			}
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.globalFade && !freeze)
			{
				Game1.StartWorldDrawInUI(b);
				for (int i = 0; i < _renovation.renovationBounds.Count; i++)
				{
					foreach (Microsoft.Xna.Framework.Rectangle rectangle in _renovation.renovationBounds[i])
					{
						for (int x = rectangle.Left; x < rectangle.Right; x++)
						{
							for (int y = rectangle.Top; y < rectangle.Bottom; y++)
							{
								int index = 0;
								if (i == _selectedIndex)
								{
									index = 1;
								}
								b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f), new Microsoft.Xna.Framework.Rectangle(194 + index * 16, 388, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.999f);
							}
						}
					}
				}
				Game1.EndWorldDrawInUI(b);
			}
			if (!Game1.globalFade && !freeze)
			{
				string s = _renovation.placementText;
				SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
			}
			if (!Game1.globalFade && !freeze && okButton != null)
			{
				okButton.draw(b);
			}
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}
	}
}
