using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace StardewValley.Menus
{
	public class StrengthGame : IClickableMenu
	{
		private float power;

		private float changeSpeed;

		private float endTimer;

		private float transparency = 1f;

		private Color barColor;

		private bool victorySound;

		private bool clicked;

		private bool showedResult;

		public StrengthGame()
			: base(2008, 3624, 20, 136)
		{
			power = 0f;
			changeSpeed = 3 + Game1.random.Next(2);
			barColor = Color.Red;
			Game1.playSound("cowboy_monsterhit");
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
			if (!clicked)
			{
				Game1.player.faceDirection(1);
				Game1.player.CurrentToolIndex = 107;
				Game1.player.FarmerSprite.animateOnce(168, 80f, 8);
				Game1.player.toolOverrideFunction = afterSwingAnimation;
				Game1.player.FarmerSprite.ignoreDefaultActionThisTime = false;
				clicked = true;
			}
			if (showedResult && Game1.dialogueTyping)
			{
				Game1.currentDialogueCharacterIndex = Game1.currentObjectDialogue.Peek().Length - 1;
			}
			if (showedResult && !Game1.dialogueTyping)
			{
				Game1.player.toolOverrideFunction = null;
				Game1.exitActiveMenu();
				Game1.afterDialogues = null;
				Game1.pressActionButton(Game1.oldKBState, Game1.oldMouseState, Game1.oldPadState);
			}
		}

		public void afterSwingAnimation(Farmer who)
		{
			if (!Game1.isFestival())
			{
				who.toolOverrideFunction = null;
				return;
			}
			changeSpeed = 0f;
			Game1.playSound("hammer");
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(46, new Vector2(30f, 56f) * 64f, Color.White));
			if (power >= 99f)
			{
				endTimer = 2000f;
			}
			else
			{
				endTimer = 1000f;
			}
		}

		public override void receiveKeyPress(Keys key)
		{
			base.receiveKeyPress(key);
		}

		public override void update(GameTime time)
		{
			base.update(time);
			if (changeSpeed == 0f)
			{
				endTimer -= time.ElapsedGameTime.Milliseconds;
				if (power >= 99f)
				{
					if (endTimer < 1500f)
					{
						if (!victorySound)
						{
							victorySound = true;
							Game1.playSound("getNewSpecialItem");
							barColor = Color.Orange;
						}
						if (!showedResult && Game1.random.NextDouble() < 0.08)
						{
							Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(10 + Game1.random.Next(2), new Vector2(31f, 55f) * 64f + new Vector2(Game1.random.Next(-64, 64), Game1.random.Next(-64, 64)), Color.Yellow)
							{
								layerDepth = 1f
							});
						}
					}
				}
				else
				{
					transparency = Math.Max(0f, transparency - 0.02f);
				}
				if (!(endTimer <= 0f) || showedResult)
				{
					return;
				}
				showedResult = true;
				if (power >= 99f)
				{
					Game1.player.festivalScore++;
					Game1.playSound("purchase");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11660"));
				}
				else if (power >= 2f)
				{
					string strengthLevel = "";
					switch ((int)power)
					{
					case 97:
					case 98:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11661");
						break;
					case 93:
					case 94:
					case 95:
					case 96:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11662");
						break;
					case 91:
					case 92:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11663");
						break;
					case 88:
					case 90:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11664");
						break;
					case 87:
					case 89:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11665");
						break;
					case 84:
					case 85:
					case 86:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11666");
						break;
					case 82:
					case 83:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11667");
						break;
					case 80:
					case 81:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11668");
						break;
					case 77:
					case 78:
					case 79:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11669");
						break;
					case 73:
					case 74:
					case 75:
					case 76:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11670");
						break;
					case 69:
					case 70:
					case 71:
					case 72:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11671");
						break;
					case 64:
					case 65:
					case 66:
					case 67:
					case 68:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11672");
						break;
					case 60:
					case 61:
					case 62:
					case 63:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11673");
						break;
					case 56:
					case 57:
					case 58:
					case 59:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11674");
						break;
					case 54:
					case 55:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11675");
						break;
					case 52:
					case 53:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11676");
						break;
					case 50:
					case 51:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11677");
						break;
					case 48:
					case 49:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11678");
						break;
					case 46:
					case 47:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11679");
						break;
					case 44:
					case 45:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11680");
						break;
					case 42:
					case 43:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11681");
						break;
					case 40:
					case 41:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11682");
						break;
					case 38:
					case 39:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11683");
						break;
					case 36:
					case 37:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11684");
						break;
					case 34:
					case 35:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11685");
						break;
					case 32:
					case 33:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11686");
						break;
					case 30:
					case 31:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11687");
						break;
					case 28:
					case 29:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11688");
						break;
					case 26:
					case 27:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11689");
						break;
					case 24:
					case 25:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11690");
						break;
					case 22:
					case 23:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11691");
						break;
					case 20:
					case 21:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11692");
						break;
					case 18:
					case 19:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11693");
						break;
					case 16:
					case 17:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11694");
						break;
					case 14:
					case 15:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11695");
						break;
					case 12:
					case 13:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11696");
						break;
					case 10:
					case 11:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11697");
						break;
					case 8:
					case 9:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11698");
						break;
					case 6:
					case 7:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11699");
						break;
					case 4:
					case 5:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11700");
						break;
					case 2:
					case 3:
						strengthLevel = Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11701");
						break;
					}
					Game1.playSound("dwop");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11703", strengthLevel));
				}
				else
				{
					Game1.player.festivalScore++;
					Game1.playSound("purchase");
					Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:StrengthGame.cs.11705")));
				}
				Game1.afterDialogues = base.exitThisMenuNoSound;
			}
			else
			{
				power += changeSpeed;
				if (power > 100f)
				{
					power = 100f;
					changeSpeed = 0f - changeSpeed;
				}
				else if (power < 0f)
				{
					power = 0f;
					changeSpeed = 0f - changeSpeed;
				}
			}
		}

		public override void performHoverAction(int x, int y)
		{
		}

		public override void draw(SpriteBatch b)
		{
			if (!Game1.dialogueUp)
			{
				b.Draw(Game1.staminaRect, Game1.GlobalToLocal(Game1.viewport, new Rectangle(xPositionOnScreen, (int)((float)yPositionOnScreen - power / 100f * (float)height), width, (int)(power / 100f * (float)height))), Game1.staminaRect.Bounds, barColor * transparency, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
			}
			if (Game1.player.FarmerSprite.isOnToolAnimation())
			{
				Game1.drawTool(Game1.player, Game1.player.CurrentToolIndex);
			}
		}

		public override void receiveRightClick(int x, int y, bool playSound = true)
		{
		}
	}
}
