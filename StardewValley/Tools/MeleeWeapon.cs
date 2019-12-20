using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class MeleeWeapon : Tool
	{
		public const int defenseCooldownTime = 1500;

		public const int attackSwordCooldownTime = 2000;

		public const int daggerCooldownTime = 6000;

		public const int clubCooldownTime = 4000;

		public const int millisecondsPerSpeedPoint = 40;

		public const int defaultSpeed = 400;

		public const int stabbingSword = 0;

		public const int dagger = 1;

		public const int club = 2;

		public const int defenseSword = 3;

		public const int baseClubSpeed = -8;

		public const int scythe = 47;

		public const int goldenScythe = 53;

		[XmlElement("type")]
		public readonly NetInt type = new NetInt();

		[XmlElement("minDamage")]
		public readonly NetInt minDamage = new NetInt();

		[XmlElement("maxDamage")]
		public readonly NetInt maxDamage = new NetInt();

		[XmlElement("speed")]
		public readonly NetInt speed = new NetInt();

		[XmlElement("addedPrecision")]
		public readonly NetInt addedPrecision = new NetInt();

		[XmlElement("addedDefense")]
		public readonly NetInt addedDefense = new NetInt();

		[XmlElement("addedAreaOfEffect")]
		public readonly NetInt addedAreaOfEffect = new NetInt();

		[XmlElement("knockback")]
		public readonly NetFloat knockback = new NetFloat();

		[XmlElement("critChance")]
		public readonly NetFloat critChance = new NetFloat();

		[XmlElement("critMultiplier")]
		public readonly NetFloat critMultiplier = new NetFloat();

		public bool isOnSpecial;

		public static int defenseCooldown;

		public static int attackSwordCooldown;

		public static int daggerCooldown;

		public static int clubCooldown;

		public static int daggerHitsLeft;

		public static int timedHitTimer;

		private static float addedSwordScale = 0f;

		private static float addedClubScale = 0f;

		private static float addedDaggerScale = 0f;

		private bool hasBegunWeaponEndPause;

		private float swipeSpeed;

		[XmlIgnore]
		public Rectangle mostRecentArea;

		[XmlIgnore]
		public List<Monster> monstersHitThisSwing = new List<Monster>();

		[XmlIgnore]
		private readonly NetEvent0 animateSpecialMoveEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 defenseSwordEvent = new NetEvent0();

		[XmlIgnore]
		private readonly NetEvent0 daggerEvent = new NetEvent0();

		private bool anotherClick;

		private static Vector2 center = new Vector2(1f, 15f);

		public MeleeWeapon()
		{
			base.NetFields.AddFields(type, minDamage, maxDamage, speed, addedPrecision, addedDefense, addedAreaOfEffect, knockback, critChance, critMultiplier);
			base.Category = -98;
		}

		public MeleeWeapon(int spriteIndex)
			: this()
		{
			base.Category = -98;
			int index = (spriteIndex > -10000) ? spriteIndex : (Math.Abs(spriteIndex) - -10000);
			Dictionary<int, string> weaponData = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			if (weaponData.ContainsKey(index))
			{
				string[] split = weaponData[index].Split('/');
				base.BaseName = split[0];
				minDamage.Value = Convert.ToInt32(split[2]);
				maxDamage.Value = Convert.ToInt32(split[3]);
				knockback.Value = (float)Convert.ToDouble(split[4], CultureInfo.InvariantCulture);
				speed.Value = Convert.ToInt32(split[5]);
				addedPrecision.Value = Convert.ToInt32(split[6]);
				addedDefense.Value = Convert.ToInt32(split[7]);
				type.Set(Convert.ToInt32(split[8]));
				if ((int)type == 0)
				{
					type.Set(3);
				}
				addedAreaOfEffect.Value = Convert.ToInt32(split[11]);
				critChance.Value = (float)Convert.ToDouble(split[12], CultureInfo.InvariantCulture);
				critMultiplier.Value = (float)Convert.ToDouble(split[13], CultureInfo.InvariantCulture);
			}
			Stack = 1;
			base.InitialParentTileIndex = index;
			base.CurrentParentTileIndex = base.InitialParentTileIndex;
			base.IndexOfMenuItemView = base.CurrentParentTileIndex;
			if (isScythe(spriteIndex))
			{
				base.Category = -99;
			}
		}

		public override Item getOne()
		{
			return new MeleeWeapon(base.InitialParentTileIndex);
		}

		protected override string loadDisplayName()
		{
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
			{
				return Name;
			}
			string[] array = Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/');
			return array[array.Length - 1];
		}

		protected override string loadDescription()
		{
			return Game1.content.Load<Dictionary<int, string>>("Data\\weapons")[initialParentTileIndex].Split('/')[1];
		}

		private void OnLanguageChange(LocalizedContentManager.LanguageCode code)
		{
			Dictionary<int, string> weaponData = Game1.content.Load<Dictionary<int, string>>("Data\\weapons");
			if (weaponData.ContainsKey(initialParentTileIndex))
			{
				weaponData[initialParentTileIndex].Split('/');
				base.description = loadDescription();
				DisplayName = loadDisplayName();
			}
		}

		public MeleeWeapon(int spriteIndex, int type)
			: this()
		{
			this.type.Set(type);
			base.BaseName = "";
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animateSpecialMoveEvent, defenseSwordEvent, daggerEvent);
			animateSpecialMoveEvent.onEvent += doAnimateSpecialMove;
			defenseSwordEvent.onEvent += doDefenseSwordFunction;
			daggerEvent.onEvent += doDaggerFunction;
		}

		public override string checkForSpecialItemHoldUpMeessage()
		{
			if (base.InitialParentTileIndex == 4)
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14122");
			}
			return null;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			float coolDownLevel = 0f;
			float addedScale = 0f;
			switch ((int)type)
			{
			case 0:
			case 3:
				if (defenseCooldown > 0)
				{
					coolDownLevel = (float)defenseCooldown / 1500f;
				}
				addedScale = addedSwordScale;
				break;
			case 2:
				if (clubCooldown > 0)
				{
					coolDownLevel = (float)clubCooldown / 4000f;
				}
				addedScale = addedClubScale;
				break;
			case 1:
				if (daggerCooldown > 0)
				{
					coolDownLevel = (float)daggerCooldown / 6000f;
				}
				addedScale = addedDaggerScale;
				break;
			}
			bool drawing_as_debris = drawShadow && drawStackNumber == StackDrawType.Hide;
			if (!drawShadow | drawing_as_debris)
			{
				addedScale = 0f;
			}
			spriteBatch.Draw(Tool.weaponsTexture, location + (((int)type == 1) ? new Vector2(42f, 21f) : new Vector2(32f, 32f)), Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, base.IndexOfMenuItemView, 16, 16), color * transparency, 0f, new Vector2(8f, 8f), 4f * (scaleSize + addedScale), SpriteEffects.None, layerDepth);
			if (coolDownLevel > 0f && drawShadow && !drawing_as_debris)
			{
				spriteBatch.Draw(Game1.staminaRect, new Rectangle((int)location.X, (int)location.Y + (64 - (int)(coolDownLevel * 64f)), 64, (int)(coolDownLevel * 64f)), Color.Red * 0.66f);
			}
		}

		public override int maximumStackSize()
		{
			return 1;
		}

		public override int salePrice()
		{
			return getItemLevel() * 100;
		}

		public static void weaponsTypeUpdate(GameTime time)
		{
			if (addedSwordScale > 0f)
			{
				addedSwordScale -= 0.01f;
			}
			if (addedClubScale > 0f)
			{
				addedClubScale -= 0.01f;
			}
			if (addedDaggerScale > 0f)
			{
				addedDaggerScale -= 0.01f;
			}
			if ((float)timedHitTimer > 0f)
			{
				timedHitTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (defenseCooldown > 0)
			{
				defenseCooldown -= time.ElapsedGameTime.Milliseconds;
				if (defenseCooldown <= 0)
				{
					addedSwordScale = 0.5f;
					Game1.playSound("objectiveComplete");
				}
			}
			if (attackSwordCooldown > 0)
			{
				attackSwordCooldown -= time.ElapsedGameTime.Milliseconds;
				if (attackSwordCooldown <= 0)
				{
					addedSwordScale = 0.5f;
					Game1.playSound("objectiveComplete");
				}
			}
			if (daggerCooldown > 0)
			{
				daggerCooldown -= time.ElapsedGameTime.Milliseconds;
				if (daggerCooldown <= 0)
				{
					addedDaggerScale = 0.5f;
					Game1.playSound("objectiveComplete");
				}
			}
			if (clubCooldown > 0)
			{
				clubCooldown -= time.ElapsedGameTime.Milliseconds;
				if (clubCooldown <= 0)
				{
					addedClubScale = 0.5f;
					Game1.playSound("objectiveComplete");
				}
			}
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			base.tickUpdate(time, who);
			animateSpecialMoveEvent.Poll();
			defenseSwordEvent.Poll();
			daggerEvent.Poll();
			if (isOnSpecial && (int)type == 1 && daggerHitsLeft > 0 && !who.UsingTool)
			{
				quickStab(who);
				triggerDaggerFunction(who);
			}
			if (anotherClick)
			{
				leftClick(who);
			}
		}

		public override bool doesShowTileLocationMarker()
		{
			return false;
		}

		public int getNumberOfDescriptionCategories()
		{
			int number = 1;
			if ((int)speed != (((int)type == 2) ? (-8) : 0))
			{
				number++;
			}
			if ((int)addedDefense > 0)
			{
				number++;
			}
			if ((double)(float)critChance / 0.02 >= 2.0)
			{
				number++;
			}
			if ((double)((float)critMultiplier - 3f) / 0.02 >= 1.0)
			{
				number++;
			}
			if ((float)knockback != defaultKnockBackForThisType(type))
			{
				number++;
			}
			return number;
		}

		public override void leftClick(Farmer who)
		{
			if (who.health > 0 && Game1.activeClickableMenu == null && Game1.farmEvent == null && !Game1.eventUp && !who.swimming.Value && !who.bathingClothes.Value)
			{
				if (!isScythe() && who.FarmerSprite.currentAnimationIndex > (((int)type == 2) ? 5 : (((int)type != 1) ? 5 : 0)))
				{
					who.completelyStopAnimatingOrDoingAction();
					who.CanMove = false;
					who.UsingTool = true;
					who.canReleaseTool = true;
					setFarmerAnimating(who);
				}
				else if (!isScythe() && who.FarmerSprite.currentAnimationIndex > (((int)type == 2) ? 3 : (((int)type != 1) ? 3 : 0)))
				{
					anotherClick = true;
				}
			}
		}

		public bool isScythe(int index = -1)
		{
			if (index == -1)
			{
				index = base.InitialParentTileIndex;
			}
			if (base.InitialParentTileIndex != 47)
			{
				return base.InitialParentTileIndex == 53;
			}
			return true;
		}

		public int getItemLevel()
		{
			int weaponPoints3 = 0;
			weaponPoints3 += (int)((double)(((int)maxDamage + (int)minDamage) / 2) * (1.0 + 0.1 * (double)(Math.Max(0, speed) + (((int)type == 1) ? 15 : 0))));
			weaponPoints3 += (int)((double)((int)addedPrecision / 2 + (int)addedDefense) + ((double)(float)critChance - 0.02) * 100.0 + (double)(((float)critMultiplier - 3f) * 20f));
			if ((int)type == 2)
			{
				weaponPoints3 /= 2;
			}
			return weaponPoints3 / 5 + 1;
		}

		public override string getDescription()
		{
			if (!isScythe(base.IndexOfMenuItemView))
			{
				StringBuilder b = new StringBuilder();
				b.AppendLine(Game1.parseText(base.description, Game1.smallFont, getDescriptionWidth()));
				b.AppendLine();
				b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14132", minDamage, maxDamage));
				if ((int)speed != 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14134", ((int)speed > 0) ? "+" : "-", Math.Abs(speed)));
				}
				if ((int)addedAreaOfEffect > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14136", addedAreaOfEffect));
				}
				if ((int)addedPrecision > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14138", addedPrecision));
				}
				if ((int)addedDefense > 0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", addedDefense));
				}
				if ((double)(float)critChance / 0.02 >= 2.0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14142", (int)((double)(float)critChance / 0.02)));
				}
				if ((double)((float)critMultiplier - 3f) / 0.02 >= 1.0)
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14144", (int)((double)((float)critMultiplier - 3f) / 0.02)));
				}
				if ((float)knockback != defaultKnockBackForThisType(type))
				{
					b.AppendLine(Game1.content.LoadString("Strings\\StringsFromCSFiles:MeleeWeapon.cs.14140", ((float)knockback > defaultKnockBackForThisType(type)) ? "+" : "", (int)Math.Ceiling(Math.Abs((float)knockback - defaultKnockBackForThisType(type)) * 10f)));
				}
				return b.ToString();
			}
			return Game1.parseText(base.description, Game1.smallFont, getDescriptionWidth());
		}

		public float defaultKnockBackForThisType(int type)
		{
			switch (type)
			{
			case 1:
				return 0.5f;
			case 0:
			case 3:
				return 1f;
			case 2:
				return 1.5f;
			default:
				return -1f;
			}
		}

		public virtual Rectangle getAreaOfEffect(int x, int y, int facingDirection, ref Vector2 tileLocation1, ref Vector2 tileLocation2, Rectangle wielderBoundingBox, int indexInCurrentAnimation)
		{
			Rectangle areaOfEffect = Rectangle.Empty;
			int horizontalYOffset2 = 0;
			int upHeightOffset2 = 0;
			int num = type;
			int width;
			int height;
			if (num == 1)
			{
				width = 74;
				height = 48;
				upHeightOffset2 = 42;
				horizontalYOffset2 = -32;
			}
			else
			{
				width = 64;
				height = 64;
				horizontalYOffset2 = -32;
				upHeightOffset2 = 0;
			}
			if ((int)type == 1)
			{
				switch (facingDirection)
				{
				case 0:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset2, width / 2, height + upHeightOffset2);
					tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
					areaOfEffect.Offset(20, -16);
					areaOfEffect.Height += 16;
					areaOfEffect.Width += 20;
					break;
				case 1:
					areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset2, height, width);
					tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
					areaOfEffect.Offset(-4, 0);
					areaOfEffect.Width += 16;
					break;
				case 2:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, height);
					tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
					areaOfEffect.Offset(12, -8);
					areaOfEffect.Width -= 21;
					break;
				case 3:
					areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset2, height, width);
					tileLocation1 = new Vector2(areaOfEffect.Left / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
					tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
					areaOfEffect.Offset(-12, 0);
					areaOfEffect.Width += 16;
					break;
				}
			}
			else
			{
				switch (facingDirection)
				{
				case 0:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Y - height - upHeightOffset2, width, height + upHeightOffset2);
					tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Top / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Top / 64);
					switch (indexInCurrentAnimation)
					{
					case 5:
						areaOfEffect.Offset(76, -32);
						break;
					case 4:
						areaOfEffect.Offset(56, -32);
						areaOfEffect.Height += 32;
						break;
					case 3:
						areaOfEffect.Offset(40, -60);
						areaOfEffect.Height += 48;
						break;
					case 2:
						areaOfEffect.Offset(-12, -68);
						areaOfEffect.Height += 48;
						break;
					case 1:
						areaOfEffect.Offset(-48, -56);
						areaOfEffect.Height += 32;
						break;
					case 0:
						areaOfEffect.Offset(-60, -12);
						break;
					}
					break;
				case 2:
					areaOfEffect = new Rectangle(x - width / 2, wielderBoundingBox.Bottom, width, height);
					tileLocation1 = new Vector2(((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Left : areaOfEffect.Right) / 64, areaOfEffect.Center.Y / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(72, -92);
						break;
					case 1:
						areaOfEffect.Offset(56, -32);
						break;
					case 2:
						areaOfEffect.Offset(40, -28);
						break;
					case 3:
						areaOfEffect.Offset(-12, -8);
						break;
					case 4:
						areaOfEffect.Offset(-80, -24);
						areaOfEffect.Width += 32;
						break;
					case 5:
						areaOfEffect.Offset(-68, -44);
						break;
					}
					break;
				case 1:
					areaOfEffect = new Rectangle(wielderBoundingBox.Right, y - height / 2 + horizontalYOffset2, height, width);
					tileLocation1 = new Vector2(areaOfEffect.Center.X / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
					tileLocation2 = new Vector2(areaOfEffect.Center.X / 64, areaOfEffect.Center.Y / 64);
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(-44, -84);
						break;
					case 1:
						areaOfEffect.Offset(4, -44);
						break;
					case 2:
						areaOfEffect.Offset(12, -4);
						break;
					case 3:
						areaOfEffect.Offset(12, 37);
						break;
					case 4:
						areaOfEffect.Offset(-28, 60);
						break;
					case 5:
						areaOfEffect.Offset(-60, 72);
						break;
					}
					break;
				case 3:
					areaOfEffect = new Rectangle(wielderBoundingBox.Left - height, y - height / 2 + horizontalYOffset2, height, width);
					tileLocation1 = new Vector2(areaOfEffect.Left / 64, ((Game1.random.NextDouble() < 0.5) ? areaOfEffect.Top : areaOfEffect.Bottom) / 64);
					tileLocation2 = new Vector2(areaOfEffect.Left / 64, areaOfEffect.Center.Y / 64);
					switch (indexInCurrentAnimation)
					{
					case 0:
						areaOfEffect.Offset(56, -76);
						break;
					case 1:
						areaOfEffect.Offset(-8, -56);
						break;
					case 2:
						areaOfEffect.Offset(-16, -4);
						break;
					case 3:
						areaOfEffect.Offset(0, 37);
						break;
					case 4:
						areaOfEffect.Offset(24, 60);
						break;
					case 5:
						areaOfEffect.Offset(64, 64);
						break;
					}
					break;
				}
			}
			areaOfEffect.Inflate(addedAreaOfEffect, addedAreaOfEffect);
			return areaOfEffect;
		}

		public void triggerDefenseSwordFunction(Farmer who)
		{
			defenseSwordEvent.Fire();
		}

		private void doDefenseSwordFunction()
		{
			isOnSpecial = false;
			lastUser.UsingTool = false;
			lastUser.CanMove = true;
			lastUser.FarmerSprite.PauseForSingleAnimation = false;
		}

		public void doStabbingSwordFunction(Farmer who)
		{
			isOnSpecial = false;
			who.UsingTool = false;
			who.xVelocity = 0f;
			who.yVelocity = 0f;
		}

		public void triggerDaggerFunction(Farmer who)
		{
			daggerEvent.Fire();
		}

		private void doDaggerFunction()
		{
			Vector2 v = lastUser.getUniformPositionAwayFromBox(lastUser.FacingDirection, 48);
			float tmpKnockBack = knockback;
			knockback.Value = 0.1f;
			DoDamage(Game1.currentLocation, (int)v.X, (int)v.Y, lastUser.FacingDirection, 1, lastUser);
			knockback.Value = tmpKnockBack;
			daggerHitsLeft--;
			isOnSpecial = false;
			lastUser.UsingTool = false;
			lastUser.CanMove = true;
			lastUser.FarmerSprite.PauseForSingleAnimation = false;
			if (daggerHitsLeft > 0)
			{
				quickStab(lastUser);
			}
		}

		public void triggerClubFunction(Farmer who)
		{
			who.currentLocation.playSound("clubSmash");
			who.currentLocation.damageMonster(new Rectangle((int)lastUser.Position.X - 192, lastUser.GetBoundingBox().Y - 192, 384, 384), minDamage, maxDamage, isBomb: false, 1.5f, 100, 0f, 1f, triggerMonsterInvincibleTimer: false, lastUser);
			Game1.viewport.Y -= 21;
			Game1.viewport.X += Game1.random.Next(-32, 32);
			Vector2 v = lastUser.getUniformPositionAwayFromBox(lastUser.FacingDirection, 64);
			switch (lastUser.FacingDirection)
			{
			case 0:
			case 2:
				v.X -= 32f;
				v.Y -= 32f;
				break;
			case 1:
				v.X -= 42f;
				v.Y -= 32f;
				break;
			case 3:
				v.Y -= 32f;
				break;
			}
			Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, v, flicker: false, lastUser.FacingDirection == 1));
			lastUser.jitterStrength = 2f;
		}

		private void beginSpecialMove(Farmer who)
		{
			if (!Game1.fadeToBlack)
			{
				isOnSpecial = true;
				who.UsingTool = true;
				who.CanMove = false;
			}
		}

		private void quickStab(Farmer who)
		{
			AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = triggerDaggerFunction;
			if (!lastUser.IsLocalPlayer)
			{
				endOfAnimFunc = null;
			}
			switch (who.FacingDirection)
			{
			case 0:
				((FarmerSprite)who.Sprite).animateOnce(276, 15f, 2, endOfAnimFunc);
				Update(0, 0, who);
				break;
			case 1:
				((FarmerSprite)who.Sprite).animateOnce(274, 15f, 2, endOfAnimFunc);
				Update(1, 0, who);
				break;
			case 2:
				((FarmerSprite)who.Sprite).animateOnce(272, 15f, 2, endOfAnimFunc);
				Update(2, 0, who);
				break;
			case 3:
				((FarmerSprite)who.Sprite).animateOnce(278, 15f, 2, endOfAnimFunc);
				Update(3, 0, who);
				break;
			}
			beginSpecialMove(who);
			who.currentLocation.localSound("daggerswipe");
		}

		private int specialCooldown()
		{
			if ((int)type == 3)
			{
				return defenseCooldown;
			}
			if ((int)type == 1)
			{
				return daggerCooldown;
			}
			if ((int)type == 2)
			{
				return clubCooldown;
			}
			if ((int)type == 0)
			{
				return attackSwordCooldown;
			}
			return 0;
		}

		public void animateSpecialMove(Farmer who)
		{
			lastUser = who;
			if (((int)type != 3 || (!base.BaseName.Contains("Scythe") && !isScythe())) && !Game1.fadeToBlack && specialCooldown() <= 0)
			{
				animateSpecialMoveEvent.Fire();
			}
		}

		private void doAnimateSpecialMove()
		{
			if (lastUser == null || lastUser.CurrentTool != this)
			{
				return;
			}
			if (lastUser.isEmoteAnimating)
			{
				lastUser.EndEmoteAnimation();
			}
			if ((int)type == 3)
			{
				AnimatedSprite.endOfAnimationBehavior endOfAnimFunc2 = triggerDefenseSwordFunction;
				if (!lastUser.IsLocalPlayer)
				{
					endOfAnimFunc2 = null;
				}
				switch (lastUser.FacingDirection)
				{
				case 0:
					((FarmerSprite)lastUser.Sprite).animateOnce(252, 500f, 1, endOfAnimFunc2);
					Update(0, 0, lastUser);
					break;
				case 1:
					((FarmerSprite)lastUser.Sprite).animateOnce(243, 500f, 1, endOfAnimFunc2);
					Update(1, 0, lastUser);
					break;
				case 2:
					((FarmerSprite)lastUser.Sprite).animateOnce(234, 500f, 1, endOfAnimFunc2);
					Update(2, 0, lastUser);
					break;
				case 3:
					((FarmerSprite)lastUser.Sprite).animateOnce(259, 500f, 1, endOfAnimFunc2);
					Update(3, 0, lastUser);
					break;
				}
				lastUser.currentLocation.localSound("batFlap");
				beginSpecialMove(lastUser);
				if (lastUser.IsLocalPlayer)
				{
					defenseCooldown = 1500;
				}
				if (lastUser.professions.Contains(28))
				{
					defenseCooldown /= 2;
				}
			}
			else if ((int)type == 2)
			{
				AnimatedSprite.endOfAnimationBehavior endOfAnimFunc = triggerClubFunction;
				if (!lastUser.IsLocalPlayer)
				{
					endOfAnimFunc = null;
				}
				lastUser.currentLocation.localSound("clubswipe");
				switch (lastUser.FacingDirection)
				{
				case 0:
					((FarmerSprite)lastUser.Sprite).animateOnce(176, 40f, 8, endOfAnimFunc);
					Update(0, 0, lastUser);
					break;
				case 1:
					((FarmerSprite)lastUser.Sprite).animateOnce(168, 40f, 8, endOfAnimFunc);
					Update(1, 0, lastUser);
					break;
				case 2:
					((FarmerSprite)lastUser.Sprite).animateOnce(160, 40f, 8, endOfAnimFunc);
					Update(2, 0, lastUser);
					break;
				case 3:
					((FarmerSprite)lastUser.Sprite).animateOnce(184, 40f, 8, endOfAnimFunc);
					Update(3, 0, lastUser);
					break;
				}
				beginSpecialMove(lastUser);
				if (lastUser.IsLocalPlayer)
				{
					clubCooldown = 4000;
				}
				if (lastUser.professions.Contains(28))
				{
					clubCooldown /= 2;
				}
			}
			else if ((int)type == 1)
			{
				daggerHitsLeft = 4;
				quickStab(lastUser);
				if (lastUser.IsLocalPlayer)
				{
					daggerCooldown = 6000;
				}
				if (lastUser.professions.Contains(28))
				{
					daggerCooldown /= 2;
				}
			}
		}

		public void doSwipe(int type, Vector2 position, int facingDirection, float swipeSpeed, Farmer f)
		{
			if (f == null || f.CurrentTool != this)
			{
				return;
			}
			if (f.IsLocalPlayer)
			{
				f.TemporaryPassableTiles.Clear();
				f.currentLocation.lastTouchActionLocation = Vector2.Zero;
			}
			swipeSpeed *= 1.3f;
			switch (type)
			{
			case 3:
				if (f.CurrentTool == this)
				{
					switch (f.FacingDirection)
					{
					case 0:
						((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 6);
						Update(0, 0, f);
						break;
					case 1:
						((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 6);
						Update(1, 0, f);
						break;
					case 2:
						((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 6);
						Update(2, 0, f);
						break;
					case 3:
						((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 6);
						Update(3, 0, f);
						break;
					}
				}
				else if (f.FacingDirection != 0)
				{
					_ = f.FacingDirection;
					_ = 2;
				}
				f.currentLocation.localSound("swordswipe");
				break;
			case 2:
				if (f.CurrentTool == this)
				{
					switch (f.FacingDirection)
					{
					case 0:
						((FarmerSprite)f.Sprite).animateOnce(248, swipeSpeed, 8);
						Update(0, 0, f);
						break;
					case 1:
						((FarmerSprite)f.Sprite).animateOnce(240, swipeSpeed, 8);
						Update(1, 0, f);
						break;
					case 2:
						((FarmerSprite)f.Sprite).animateOnce(232, swipeSpeed, 8);
						Update(2, 0, f);
						break;
					case 3:
						((FarmerSprite)f.Sprite).animateOnce(256, swipeSpeed, 8);
						Update(3, 0, f);
						break;
					}
				}
				f.currentLocation.localSound("clubswipe");
				break;
			}
		}

		public void setFarmerAnimating(Farmer who)
		{
			anotherClick = false;
			who.FarmerSprite.PauseForSingleAnimation = false;
			who.FarmerSprite.StopAnimation();
			hasBegunWeaponEndPause = false;
			swipeSpeed = 400 - (int)speed * 40 - who.addedSpeed * 40;
			swipeSpeed *= 1f - who.weaponSpeedModifier;
			if ((int)type != 1)
			{
				doSwipe(type, who.Position, who.FacingDirection, swipeSpeed / (float)(((int)type == 2) ? 5 : 8), who);
				who.lastClick = Vector2.Zero;
				Vector2 actionTile2 = who.GetToolLocation(ignoreClick: true);
				DoDamage(who.currentLocation, (int)actionTile2.X, (int)actionTile2.Y, who.FacingDirection, 1, who);
			}
			else
			{
				who.currentLocation.playSound("daggerswipe");
				swipeSpeed /= 4f;
				switch (who.FacingDirection)
				{
				case 0:
					((FarmerSprite)who.Sprite).animateOnce(276, swipeSpeed, 2);
					Update(0, 0, who);
					break;
				case 1:
					((FarmerSprite)who.Sprite).animateOnce(274, swipeSpeed, 2);
					Update(1, 0, who);
					break;
				case 2:
					((FarmerSprite)who.Sprite).animateOnce(272, swipeSpeed, 2);
					Update(2, 0, who);
					break;
				case 3:
					((FarmerSprite)who.Sprite).animateOnce(278, swipeSpeed, 2);
					Update(3, 0, who);
					break;
				}
				Vector2 actionTile = who.GetToolLocation(ignoreClick: true);
				DoDamage(who.currentLocation, (int)actionTile.X, (int)actionTile.Y, who.FacingDirection, 1, who);
			}
			if (who.CurrentTool == null)
			{
				who.completelyStopAnimatingOrDoingAction();
				who.forceCanMove();
			}
		}

		public override void actionWhenStopBeingHeld(Farmer who)
		{
			who.UsingTool = false;
			anotherClick = false;
			base.actionWhenStopBeingHeld(who);
		}

		public override void endUsing(GameLocation location, Farmer who)
		{
			base.endUsing(location, who);
		}

		public void DoDamage(GameLocation location, int x, int y, int facingDirection, int power, Farmer who)
		{
			if (who.IsLocalPlayer)
			{
				isOnSpecial = false;
				if ((int)type != 2)
				{
					base.DoFunction(location, x, y, power, who);
				}
				lastUser = who;
				Vector2 tileLocation3 = Vector2.Zero;
				Vector2 tileLocation2 = Vector2.Zero;
				Rectangle areaOfEffect = getAreaOfEffect(x, y, facingDirection, ref tileLocation3, ref tileLocation2, who.GetBoundingBox(), who.FarmerSprite.currentAnimationIndex);
				mostRecentArea = areaOfEffect;
				if (location.damageMonster(areaOfEffect, (int)((float)(int)minDamage * (1f + who.attackIncreaseModifier)), (int)((float)(int)maxDamage * (1f + who.attackIncreaseModifier)), isBomb: false, (float)knockback * (1f + who.knockbackModifier), (int)((float)(int)addedPrecision * (1f + who.weaponPrecisionModifier)), (float)critChance * (1f + who.critChanceModifier), (float)critMultiplier * (1f + who.critPowerModifier), (int)type != 1 || !isOnSpecial, lastUser) && (int)type == 2)
				{
					location.playSound("clubhit");
				}
				string soundToPlay = "";
				location.projectiles.Filter(delegate(Projectile projectile)
				{
					if (areaOfEffect.Intersects(projectile.getBoundingBox()))
					{
						projectile.behaviorOnCollisionWithOther(location);
					}
					return !projectile.destroyMe;
				});
				foreach (Vector2 v in Utility.removeDuplicates(Utility.getListOfTileLocationsForBordersOfNonTileRectangle(areaOfEffect)))
				{
					if (location.terrainFeatures.ContainsKey(v) && location.terrainFeatures[v].performToolAction(this, 0, v, location))
					{
						location.terrainFeatures.Remove(v);
					}
					if (location.objects.ContainsKey(v) && location.objects[v].performToolAction(this, location))
					{
						location.objects.Remove(v);
					}
					if (location.performToolAction(this, (int)v.X, (int)v.Y))
					{
						break;
					}
				}
				if (!soundToPlay.Equals(""))
				{
					Game1.playSound(soundToPlay);
				}
				base.CurrentParentTileIndex = base.IndexOfMenuItemView;
				if (who != null && who.isRidingHorse())
				{
					who.completelyStopAnimatingOrDoingAction();
				}
			}
		}

		public static Rectangle getSourceRect(int index)
		{
			return Game1.getSourceRectForStandardTileSheet(Tool.weaponsTexture, index, 16, 16);
		}

		public override void drawTooltip(SpriteBatch spriteBatch, ref int x, ref int y, SpriteFont font, float alpha, string overrideText)
		{
			Utility.drawTextWithShadow(spriteBatch, Game1.parseText(base.description, Game1.smallFont, getDescriptionWidth()), font, new Vector2(x + 16, y + 16 + 4), Game1.textColor);
			y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, getDescriptionWidth())).Y;
			if (!isScythe(base.IndexOfMenuItemView))
			{
				Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(120, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
				Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Damage", minDamage, maxDamage), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
				y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				if ((int)speed != (((int)type == 2) ? (-8) : 0))
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(130, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					bool negativeSpeed = ((int)type == 2 && (int)speed < -8) || ((int)type != 2 && (int)speed < 0);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Speed", (((((int)type == 2) ? ((int)speed - -8) : ((int)speed)) > 0) ? "+" : "") + (((int)type == 2) ? ((int)speed - -8) : ((int)speed)) / 2), font, new Vector2(x + 16 + 52, y + 16 + 12), negativeSpeed ? Color.DarkRed : (Game1.textColor * 0.9f * alpha));
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((int)addedDefense > 0)
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(110, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", addedDefense), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((double)(float)critChance / 0.02 >= 2.0)
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(40, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", (int)((double)(float)critChance / 0.02)), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((double)((float)critMultiplier - 3f) / 0.02 >= 1.0)
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16, y + 16 + 4), new Rectangle(160, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", (int)((double)((float)critMultiplier - 3f) / 0.02)), font, new Vector2(x + 16 + 44, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
				if ((float)knockback != defaultKnockBackForThisType(type))
				{
					Utility.drawWithShadow(spriteBatch, Game1.mouseCursors, new Vector2(x + 16 + 4, y + 16 + 4), new Rectangle(70, 428, 10, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					Utility.drawTextWithShadow(spriteBatch, Game1.content.LoadString("Strings\\UI:ItemHover_Weight", (((float)(int)Math.Ceiling(Math.Abs((float)knockback - defaultKnockBackForThisType(type)) * 10f) > defaultKnockBackForThisType(type)) ? "+" : "") + (int)Math.Ceiling(Math.Abs((float)knockback - defaultKnockBackForThisType(type)) * 10f)), font, new Vector2(x + 16 + 52, y + 16 + 12), Game1.textColor * 0.9f * alpha);
					y += (int)Math.Max(font.MeasureString("TT").Y, 48f);
				}
			}
		}

		public override Point getExtraSpaceNeededForTooltipSpecialIcons(SpriteFont font, int minWidth, int horizontalBuffer, int startingHeight, string descriptionText, string boldTitleText, int moneyAmountToDisplayAtBottom)
		{
			int maxStat = 9999;
			Point dimensions = new Point(0, 0);
			dimensions.Y += Math.Max(60, (int)((boldTitleText != null) ? (Game1.dialogueFont.MeasureString(boldTitleText).Y + 16f) : 0f) + 32) + (int)font.MeasureString("T").Y + (int)((moneyAmountToDisplayAtBottom > -1) ? (font.MeasureString(string.Concat(moneyAmountToDisplayAtBottom)).Y + 4f) : 0f);
			dimensions.Y += ((!isScythe()) ? (getNumberOfDescriptionCategories() * 4 * 12) : 0);
			dimensions.Y += (int)font.MeasureString(Game1.parseText(base.description, Game1.smallFont, getDescriptionWidth())).Y;
			dimensions.X = (int)Math.Max(minWidth, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Damage", maxStat, maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Speed", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_DefenseBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritChanceBonus", maxStat)).X + (float)horizontalBuffer, Math.Max(font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_CritPowerBonus", maxStat)).X + (float)horizontalBuffer, font.MeasureString(Game1.content.LoadString("Strings\\UI:ItemHover_Weight", maxStat)).X + (float)horizontalBuffer))))));
			return dimensions;
		}

		public void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f)
		{
			drawDuringUse(frameOfFarmerAnimation, facingDirection, spriteBatch, playerPosition, f, getSourceRect(base.InitialParentTileIndex), type, isOnSpecial);
		}

		public static void drawDuringUse(int frameOfFarmerAnimation, int facingDirection, SpriteBatch spriteBatch, Vector2 playerPosition, Farmer f, Rectangle sourceRect, int type, bool isOnSpecial)
		{
			if (type != 1)
			{
				if (isOnSpecial)
				{
					switch (type)
					{
					case 3:
						switch (f.FacingDirection)
						{
						case 0:
							spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 44f), sourceRect, Color.White, (float)Math.PI * -9f / 16f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
							break;
						case 1:
							spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * -3f / 16f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 1) / 10000f));
							break;
						case 2:
							spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 52f, playerPosition.Y + 4f), sourceRect, Color.White, -5.105088f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 2) / 10000f));
							break;
						case 3:
							spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 56f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI * -5f / 16f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 1) / 10000f));
							break;
						}
						break;
					case 2:
						switch (facingDirection)
						{
						case 1:
							switch (frameOfFarmerAnimation)
							{
							case 0:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 32f - 12f, playerPosition.Y - 80f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 1:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 64f - 48f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 2:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 128f - 16f, playerPosition.Y - 64f - 12f), sourceRect, Color.White, (float)Math.PI * 3f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 3:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 72f, playerPosition.Y - 64f + 16f - 32f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 4:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 96f, playerPosition.Y - 64f + 16f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 5:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 96f - 12f, playerPosition.Y - 64f + 16f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 6:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 96f - 16f, playerPosition.Y - 64f + 40f - 8f), sourceRect, Color.White, (float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 7:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 96f - 8f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * 5f / 16f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							}
							break;
						case 3:
							switch (frameOfFarmerAnimation)
							{
							case 0:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 4f + 8f, playerPosition.Y - 56f - 64f), sourceRect, Color.White, (float)Math.PI / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 1:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 2:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 12f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -7f / 8f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 3:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 32f - 4f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 4:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f - 24f, playerPosition.Y + 64f + 12f - 64f), sourceRect, Color.White, 4.31969f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 5:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 20f, playerPosition.Y + 64f + 40f - 64f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 6:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y + 64f + 56f), sourceRect, Color.White, 3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							case 7:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 8f, playerPosition.Y + 64f + 64f), sourceRect, Color.White, 3.73064137f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
								break;
							}
							break;
						default:
							switch (frameOfFarmerAnimation)
							{
							case 0:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 24f, playerPosition.Y - 21f - 8f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								break;
							case 1:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f - 64f + 4f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								break;
							case 2:
								spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 20f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								break;
							case 3:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								else
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 21f + 32f - 64f), sourceRect, Color.White, -(float)Math.PI / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								break;
							case 4:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f + 8f, playerPosition.Y + 32f), sourceRect, Color.White, -3.926991f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								break;
							case 5:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f - 20f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								break;
							case 6:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 54f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								break;
							case 7:
								if (facingDirection == 2)
								{
									spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f + 12f, playerPosition.Y + 64f + 58f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
								}
								break;
							}
							if (f.FacingDirection == 0)
							{
								f.FarmerRenderer.draw(spriteBatch, f.FarmerSprite, f.FarmerSprite.SourceRect, f.getLocalPosition(Game1.viewport), new Vector2(0f, (f.yOffset + 128f - (float)(f.GetBoundingBox().Height / 2)) / 4f + 4f), Math.Max(0f, (float)f.getStandingY() / 10000f + 0.0099f), Color.White, 0f, f);
							}
							break;
						}
						break;
					}
					return;
				}
				switch (facingDirection)
				{
				case 1:
					switch (frameOfFarmerAnimation)
					{
					case 0:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 40f, playerPosition.Y - 64f + 8f), sourceRect, Color.White, -(float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 64f + 28f), sourceRect, Color.White, 0f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 4f), sourceRect, Color.White, (float)Math.PI / 2f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 28f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 5f / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 48f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 7:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y + 64f + 12f), sourceRect, Color.White, 1.96349537f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					}
					break;
				case 3:
					switch (frameOfFarmerAnimation)
					{
					case 0:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 16f, playerPosition.Y - 64f - 16f), sourceRect, Color.White, (float)Math.PI / 4f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 48f, playerPosition.Y - 64f + 20f), sourceRect, Color.White, 0f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 64f + 32f, playerPosition.Y + 16f), sourceRect, Color.White, -(float)Math.PI / 4f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() - 1) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 4f, playerPosition.Y + 44f), sourceRect, Color.White, -(float)Math.PI / 2f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 52f), sourceRect, Color.White, (float)Math.PI * -5f / 8f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 80f, playerPosition.Y + 40f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, center, 4f, SpriteEffects.FlipHorizontally, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					case 7:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X - 44f, playerPosition.Y + 96f), sourceRect, Color.White, -5.105088f, center, 4f, SpriteEffects.FlipVertically, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
						break;
					}
					break;
				case 0:
					switch (frameOfFarmerAnimation)
					{
					case 0:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 32f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 2f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, (float)Math.PI * -3f / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 48f, playerPosition.Y - 52f), sourceRect, Color.White, -(float)Math.PI / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 40f), sourceRect, Color.White, 0f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f, playerPosition.Y - 40f), sourceRect, Color.White, (float)Math.PI / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					case 7:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 44f, playerPosition.Y + 64f), sourceRect, Color.White, -1.96349537f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32 - 8) / 10000f));
						break;
					}
					break;
				case 2:
					switch (frameOfFarmerAnimation)
					{
					case 0:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 56f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 8f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 1:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 52f, playerPosition.Y - 8f), sourceRect, Color.White, (float)Math.PI / 2f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 2:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 40f, playerPosition.Y), sourceRect, Color.White, (float)Math.PI / 2f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 3:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 16f, playerPosition.Y + 4f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 4:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 8f, playerPosition.Y + 8f), sourceRect, Color.White, (float)Math.PI, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 5:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.53429174f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 6:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 12f, playerPosition.Y), sourceRect, Color.White, 3.53429174f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					case 7:
						spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 44f, playerPosition.Y + 64f), sourceRect, Color.White, -5.105088f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
						break;
					}
					break;
				}
				return;
			}
			frameOfFarmerAnimation %= 2;
			switch (facingDirection)
			{
			case 1:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
					break;
				case 1:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
					break;
				}
				break;
			case 3:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 16f, playerPosition.Y - 16f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
					break;
				case 1:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 8f, playerPosition.Y - 24f), sourceRect, Color.White, (float)Math.PI * -3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 64) / 10000f));
					break;
				}
				break;
			case 0:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 4f, playerPosition.Y - 40f), sourceRect, Color.White, -(float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32) / 10000f));
					break;
				case 1:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 64f - 16f, playerPosition.Y - 48f), sourceRect, Color.White, -(float)Math.PI / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() - 32) / 10000f));
					break;
				}
				break;
			case 2:
				switch (frameOfFarmerAnimation)
				{
				case 0:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 32f, playerPosition.Y - 12f), sourceRect, Color.White, (float)Math.PI * 3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
					break;
				case 1:
					spriteBatch.Draw(Tool.weaponsTexture, new Vector2(playerPosition.X + 21f, playerPosition.Y), sourceRect, Color.White, (float)Math.PI * 3f / 4f, center, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.getStandingY() + 32) / 10000f));
					break;
				}
				break;
			}
		}
	}
}
