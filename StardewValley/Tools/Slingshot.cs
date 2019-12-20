using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class Slingshot : Tool
	{
		public const int basicDamage = 5;

		public const int basicSlingshot = 32;

		public const int masterSlingshot = 33;

		public const int galaxySlingshot = 34;

		public const int drawBackSoundThreshold = 8;

		[XmlIgnore]
		public int recentClickX;

		[XmlIgnore]
		public int recentClickY;

		[XmlIgnore]
		public int lastClickX;

		[XmlIgnore]
		public int lastClickY;

		[XmlIgnore]
		public int mouseDragAmount;

		private bool canPlaySound;

		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		[XmlIgnore]
		public readonly NetPoint aimPos = new NetPoint().Interpolated(interpolate: true, wait: true);

		public Slingshot()
		{
			base.InitialParentTileIndex = 32;
			base.CurrentParentTileIndex = base.InitialParentTileIndex;
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			string[] split = Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/');
			base.BaseName = split[0];
			numAttachmentSlots.Value = 1;
			attachments.SetCount(1);
		}

		public override Item getOne()
		{
			return new Slingshot();
		}

		protected override string loadDisplayName()
		{
			string[] split = Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/');
			if (LocalizedContentManager.CurrentLanguageCode != 0)
			{
				return split[split.Length - 1];
			}
			return Name;
		}

		protected override string loadDescription()
		{
			return Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/')[1];
		}

		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		public Slingshot(int which = 32)
		{
			base.InitialParentTileIndex = which;
			base.CurrentParentTileIndex = base.InitialParentTileIndex;
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			string[] split = Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/');
			base.BaseName = split[0];
			numAttachmentSlots.Value = 1;
			attachments.SetCount(1);
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(finishEvent, aimPos);
			finishEvent.onEvent += doFinish;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.IndexOfMenuItemView = base.InitialParentTileIndex;
			if (attachments[0] != null)
			{
				updateAimPos();
				int mouseX = aimPos.X;
				int mouseY = aimPos.Y;
				int num = Math.Min(20, (int)Vector2.Distance(new Vector2(who.getStandingX(), who.getStandingY() - 64), new Vector2(mouseX, mouseY)) / 20);
				Vector2 v = Utility.getVelocityTowardPoint(new Point(who.getStandingX(), who.getStandingY() + 64), new Vector2(mouseX, mouseY + 64), (float)(15 + Game1.random.Next(4, 6)) * (1f + who.weaponSpeedModifier));
				if (num > 4 && !canPlaySound)
				{
					Object ammunition = (Object)attachments[0].getOne();
					attachments[0].Stack--;
					if (attachments[0].Stack <= 0)
					{
						attachments[0] = null;
					}
					int damage = 1;
					BasicProjectile.onCollisionBehavior collisionBehavior = null;
					string collisionSound = "hammer";
					float damageMod = 1f;
					if (base.InitialParentTileIndex == 33)
					{
						damageMod = 2f;
					}
					else if (base.InitialParentTileIndex == 34)
					{
						damageMod = 4f;
					}
					switch (ammunition.ParentSheetIndex)
					{
					case 388:
						damage = 2;
						ammunition.ParentSheetIndex++;
						break;
					case 390:
						damage = 5;
						ammunition.ParentSheetIndex++;
						break;
					case 378:
						damage = 10;
						ammunition.ParentSheetIndex++;
						break;
					case 380:
						damage = 20;
						ammunition.ParentSheetIndex++;
						break;
					case 384:
						damage = 30;
						ammunition.ParentSheetIndex++;
						break;
					case 382:
						damage = 15;
						ammunition.ParentSheetIndex++;
						break;
					case 386:
						damage = 50;
						ammunition.ParentSheetIndex++;
						break;
					case 441:
						damage = 20;
						collisionBehavior = BasicProjectile.explodeOnImpact;
						collisionSound = "explosion";
						break;
					}
					int category = ammunition.Category;
					if (category == -5)
					{
						collisionSound = "slimedead";
					}
					location.projectiles.Add(new BasicProjectile((int)(damageMod * (float)(damage + Game1.random.Next(-(damage / 2), damage + 2)) * (1f + who.attackIncreaseModifier)), ammunition.ParentSheetIndex, 0, 0, (float)(Math.PI / (double)(64f + (float)Game1.random.Next(-63, 64))), 0f - v.X, 0f - v.Y, new Vector2(who.getStandingX() - 16, who.getStandingY() - 64 - 8), collisionSound, "", explode: false, damagesMonsters: true, location, who, spriteFromObjectSheet: true, collisionBehavior)
					{
						IgnoreLocationCollision = (Game1.currentLocation.currentEvent != null)
					});
				}
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14254"));
			}
			canPlaySound = true;
			finish();
		}

		public void finish()
		{
			finishEvent.Fire();
		}

		private void doFinish()
		{
			lastUser.usingSlingshot = false;
			lastUser.canReleaseTool = true;
			lastUser.UsingTool = false;
			lastUser.canMove = true;
			lastUser.Halt();
		}

		public override bool canThisBeAttached(Object o)
		{
			if (o == null || (!o.bigCraftable && (((int)o.parentSheetIndex >= 378 && (int)o.parentSheetIndex <= 390) || o.Category == -5 || o.Category == -79 || o.Category == -75 || (int)o.parentSheetIndex == 441)))
			{
				return true;
			}
			return false;
		}

		public override Object attach(Object o)
		{
			Object result = attachments[0];
			attachments[0] = o;
			Game1.playSound("button1");
			return result;
		}

		public override string getHoverBoxText(Item hoveredItem)
		{
			if (hoveredItem != null && hoveredItem is Object && canThisBeAttached(hoveredItem as Object))
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14256", DisplayName, hoveredItem.DisplayName);
			}
			if (hoveredItem == null && attachments != null && attachments[0] != null)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:Slingshot.cs.14258", attachments[0].DisplayName);
			}
			return null;
		}

		public override bool onRelease(GameLocation location, int x, int y, Farmer who)
		{
			DoFunction(location, x, y, 1, who);
			return true;
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			who.usingSlingshot = true;
			who.canReleaseTool = false;
			mouseDragAmount = 0;
			int offset = (who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0);
			who.FarmerSprite.setCurrentFrame(42 + offset);
			if (!who.IsLocalPlayer)
			{
				return true;
			}
			double mouseX3 = Game1.getOldMouseX() + Game1.viewport.X - who.getStandingX();
			double mouseY3 = Game1.getOldMouseY() + Game1.viewport.Y - who.getStandingY();
			if (Math.Abs(mouseX3) > Math.Abs(mouseY3))
			{
				mouseX3 /= Math.Abs(mouseX3);
				mouseY3 = 0.5;
			}
			else
			{
				mouseY3 /= Math.Abs(mouseY3);
				mouseX3 = 0.0;
			}
			mouseX3 *= 16.0;
			mouseY3 *= 16.0;
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.lastMousePositionBeforeFade = Game1.getMousePosition();
			lastClickX = Game1.getOldMouseX() + Game1.viewport.X;
			lastClickY = Game1.getOldMouseY() + Game1.viewport.Y;
			updateAimPos();
			return true;
		}

		private void updateAimPos()
		{
			if (lastUser != null && lastUser.IsLocalPlayer)
			{
				Point mousePos = Game1.getMousePosition();
				if (Game1.options.gamepadControls)
				{
					mousePos = Utility.Vector2ToPoint(lastUser.getStandingPosition() + new Vector2(Game1.oldPadState.ThumbSticks.Left.X, 0f - Game1.oldPadState.ThumbSticks.Left.Y) * 64f * 4f);
					mousePos.X -= Game1.viewport.X;
					mousePos.Y -= Game1.viewport.Y;
				}
				int mouseX = mousePos.X + Game1.viewport.X;
				int mouseY = mousePos.Y + Game1.viewport.Y;
				aimPos.X = mouseX;
				aimPos.Y = mouseY;
			}
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			finishEvent.Poll();
			if (!who.usingSlingshot)
			{
				return;
			}
			if (who.IsLocalPlayer)
			{
				updateAimPos();
				int mouseX = aimPos.X;
				int mouseY = aimPos.Y;
				Game1.debugOutput = "playerPos: " + who.getStandingPosition().ToString() + ", mousePos: " + mouseX + ", " + mouseY;
				mouseDragAmount++;
				who.faceGeneralDirection(new Vector2(mouseX, mouseY), 0, opposite: true);
				if (canPlaySound && (Math.Abs(mouseX - lastClickX) > 8 || Math.Abs(mouseY - lastClickY) > 8) && mouseDragAmount > 4)
				{
					who.currentLocation.playSound("slingshot");
					canPlaySound = false;
				}
				lastClickX = mouseX;
				lastClickY = mouseY;
				Game1.mouseCursor = -1;
			}
			int offset = (who.FacingDirection == 3 || who.FacingDirection == 1) ? 1 : ((who.FacingDirection == 0) ? 2 : 0);
			who.FarmerSprite.setCurrentFrame(42 + offset);
		}

		public override void drawAttachments(SpriteBatch b, int x, int y)
		{
			if (attachments[0] == null)
			{
				b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 43), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
				return;
			}
			b.Draw(Game1.menuTexture, new Vector2(x, y), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.86f);
			attachments[0].drawInMenu(b, new Vector2(x, y), 1f);
		}

		public override void draw(SpriteBatch b)
		{
			if (lastUser.usingSlingshot && lastUser.IsLocalPlayer)
			{
				int mouseX = aimPos.X;
				int mouseY = aimPos.Y;
				Vector2 v = Utility.getVelocityTowardPoint(new Point(lastUser.getStandingX(), lastUser.getStandingY() + 32), new Vector2(mouseX, mouseY), 256f);
				if (Math.Abs(v.X) < 1f)
				{
					_ = mouseDragAmount;
					_ = 100;
				}
				double distanceBetweenRadiusAndSquare = Math.Sqrt(v.X * v.X + v.Y * v.Y) - 181.0;
				double xPercent = v.X / 256f;
				double yPercent = v.Y / 256f;
				int x = (int)((double)v.X - distanceBetweenRadiusAndSquare * xPercent);
				int y = (int)((double)v.Y - distanceBetweenRadiusAndSquare * yPercent);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(lastUser.getStandingX() - x, lastUser.getStandingY() - 64 - 8 - y)), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 43), Color.White, 0f, new Vector2(32f, 32f), 1f, SpriteEffects.None, 0.999999f);
			}
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			if (base.IndexOfMenuItemView == 0 || base.IndexOfMenuItemView == 21 || base.IndexOfMenuItemView == 47 || base.CurrentParentTileIndex == 47)
			{
				switch (base.BaseName)
				{
				case "Slingshot":
					base.CurrentParentTileIndex = 32;
					break;
				case "Master Slingshot":
					base.CurrentParentTileIndex = 33;
					break;
				case "Galaxy Slingshot":
					base.CurrentParentTileIndex = 34;
					break;
				}
				base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			}
			spriteBatch.Draw(Tool.weaponsTexture, location + new Vector2(32f, 29f), Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, base.IndexOfMenuItemView, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), scaleSize * 4f, SpriteEffects.None, layerDepth);
			if (drawStackNumber != 0 && attachments != null && attachments[0] != null)
			{
				Utility.drawTinyDigits(attachments[0].Stack, spriteBatch, location + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(attachments[0].Stack, 3f * scaleSize)) + 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, Color.White);
			}
		}
	}
}
