using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StardewValley.Characters
{
	public class JunimoHarvester : NPC
	{
		private float alpha = 1f;

		private float alphaChange;

		private Vector2 motion = Vector2.Zero;

		private new Rectangle nextPosition;

		private readonly NetColor color = new NetColor();

		private bool destroy;

		private Item lastItemHarvested;

		private Task backgroundTask;

		public int whichJunimoFromThisHut;

		public readonly NetBool isPrismatic = new NetBool(value: false);

		private readonly NetGuid netHome = new NetGuid();

		private readonly NetEvent1Field<int, NetInt> netAnimationEvent = new NetEvent1Field<int, NetInt>();

		private int harvestTimer;

		private JunimoHut home
		{
			get
			{
				return Game1.getFarm().buildings[netHome.Value] as JunimoHut;
			}
			set
			{
				netHome.Value = Game1.getFarm().buildings.GuidOf(value);
			}
		}

		public JunimoHarvester()
		{
		}

		public JunimoHarvester(Vector2 position, JunimoHut myHome, int whichJunimoNumberFromThisHut, Color? c)
			: base(new AnimatedSprite("Characters\\Junimo", 0, 16, 16), position, 2, "Junimo")
		{
			home = myHome;
			whichJunimoFromThisHut = whichJunimoNumberFromThisHut;
			if (!c.HasValue)
			{
				pickColor();
			}
			else
			{
				color.Value = c.Value;
			}
			nextPosition = GetBoundingBox();
			base.Breather = false;
			base.speed = 3;
			forceUpdateTimer = 9999;
			collidesWithOtherCharacters.Value = true;
			ignoreMovementAnimation = true;
			farmerPassesThrough = true;
			base.Scale = 0.75f;
			base.willDestroyObjectsUnderfoot = false;
			base.currentLocation = Game1.getFarm();
			Vector2 tileToPathfindTo = Vector2.Zero;
			switch (whichJunimoNumberFromThisHut)
			{
			case 0:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)home.tileX + 1, (int)home.tileY + (int)home.tilesHigh + 1), 30);
				break;
			case 1:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)home.tileX - 1, (int)home.tileY), 30);
				break;
			case 2:
				tileToPathfindTo = Utility.recursiveFindOpenTileForCharacter(this, base.currentLocation, new Vector2((int)home.tileX + (int)home.tilesWide, (int)home.tileY), 30);
				break;
			}
			if (tileToPathfindTo != Vector2.Zero)
			{
				controller = new PathFindController(this, base.currentLocation, Utility.Vector2ToPoint(tileToPathfindTo), -1, reachFirstDestinationFromHut, 100);
			}
			if ((controller == null || controller.pathToEndPoint == null) && Game1.IsMasterGame)
			{
				pathfindToRandomSpotAroundHut();
				if (controller == null || controller.pathToEndPoint == null)
				{
					destroy = true;
				}
			}
			collidesWithOtherCharacters.Value = false;
		}

		private void pickColor()
		{
			Random r = new Random((int)home.tileX + (int)home.tileY * 777 + whichJunimoFromThisHut);
			if (r.NextDouble() < 0.25)
			{
				switch (r.Next(8))
				{
				case 0:
					color.Value = Color.Red;
					break;
				case 1:
					color.Value = Color.Goldenrod;
					break;
				case 2:
					color.Value = Color.Yellow;
					break;
				case 3:
					color.Value = Color.Lime;
					break;
				case 4:
					color.Value = new Color(0, 255, 180);
					break;
				case 5:
					color.Value = new Color(0, 100, 255);
					break;
				case 6:
					color.Value = Color.MediumPurple;
					break;
				case 7:
					color.Value = Color.Salmon;
					break;
				}
				if (r.NextDouble() < 0.01)
				{
					color.Value = Color.White;
				}
			}
			else
			{
				switch (r.Next(8))
				{
				case 0:
					color.Value = Color.LimeGreen;
					break;
				case 1:
					color.Value = Color.Orange;
					break;
				case 2:
					color.Value = Color.LightGreen;
					break;
				case 3:
					color.Value = Color.Tan;
					break;
				case 4:
					color.Value = Color.GreenYellow;
					break;
				case 5:
					color.Value = Color.LawnGreen;
					break;
				case 6:
					color.Value = Color.PaleGreen;
					break;
				case 7:
					color.Value = Color.Turquoise;
					break;
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(color, netHome.NetFields, netAnimationEvent, isPrismatic);
			netAnimationEvent.onEvent += doAnimationEvent;
		}

		protected virtual void doAnimationEvent(int animId)
		{
			switch (animId)
			{
			case 1:
				break;
			case 0:
				Sprite.CurrentAnimation = null;
				break;
			case 2:
				Sprite.currentFrame = 0;
				break;
			case 3:
				Sprite.currentFrame = 1;
				break;
			case 4:
				Sprite.currentFrame = 2;
				break;
			case 5:
				Sprite.currentFrame = 44;
				break;
			case 6:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(12, 200),
					new FarmerSprite.AnimationFrame(13, 200),
					new FarmerSprite.AnimationFrame(14, 200),
					new FarmerSprite.AnimationFrame(15, 200)
				});
				break;
			case 7:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(44, 200),
					new FarmerSprite.AnimationFrame(45, 200),
					new FarmerSprite.AnimationFrame(46, 200),
					new FarmerSprite.AnimationFrame(47, 200)
				});
				break;
			case 8:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(28, 100),
					new FarmerSprite.AnimationFrame(29, 100),
					new FarmerSprite.AnimationFrame(30, 100),
					new FarmerSprite.AnimationFrame(31, 100)
				});
				break;
			}
		}

		public void reachFirstDestinationFromHut(Character c, GameLocation l)
		{
			tryToHarvestHere();
		}

		public void tryToHarvestHere()
		{
			if (base.currentLocation != null)
			{
				if (isHarvestable())
				{
					harvestTimer = 2000;
				}
				else
				{
					pokeToHarvest();
				}
			}
		}

		public void pokeToHarvest()
		{
			if (!home.isTilePassable(getTileLocation()) && Game1.IsMasterGame)
			{
				destroy = true;
			}
			else if (harvestTimer <= 0 && Game1.random.NextDouble() < 0.7)
			{
				pathfindToNewCrop();
			}
		}

		public override bool shouldCollideWithBuildingLayer(GameLocation location)
		{
			return true;
		}

		public void setMoving(int xSpeed, int ySpeed)
		{
			motion.X = xSpeed;
			motion.Y = ySpeed;
		}

		public void setMoving(Vector2 motion)
		{
			this.motion = motion;
		}

		public override void Halt()
		{
			base.Halt();
			motion = Vector2.Zero;
		}

		public override bool canTalk()
		{
			return false;
		}

		public void junimoReachedHut(Character c, GameLocation l)
		{
			controller = null;
			motion.X = 0f;
			motion.Y = -1f;
			destroy = true;
		}

		public bool foundCropEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
		{
			if (location.isCropAtTile(currentNode.x, currentNode.y) && (location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] as HoeDirt).readyForHarvest())
			{
				return true;
			}
			if (location.terrainFeatures.ContainsKey(new Vector2(currentNode.x, currentNode.y)) && location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] is Bush && (int)(location.terrainFeatures[new Vector2(currentNode.x, currentNode.y)] as Bush).tileSheetOffset == 1)
			{
				return true;
			}
			return false;
		}

		public void pathFindToNewCrop_doWork()
		{
			if (Game1.timeOfDay > 1900)
			{
				if (controller == null)
				{
					returnToJunimoHut(base.currentLocation);
				}
				return;
			}
			if (Game1.random.NextDouble() < 0.035 || (bool)home.noHarvest)
			{
				pathfindToRandomSpotAroundHut();
				return;
			}
			controller = new PathFindController(this, base.currentLocation, foundCropEndFunction, -1, eraseOldPathController: false, reachFirstDestinationFromHut, 100, Point.Zero);
			if (controller.pathToEndPoint == null || Math.Abs(controller.pathToEndPoint.Last().X - ((int)home.tileX + 1)) > 8 || Math.Abs(controller.pathToEndPoint.Last().Y - ((int)home.tileY + 1)) > 8)
			{
				if (Game1.random.NextDouble() < 0.5 && !home.lastKnownCropLocation.Equals(Point.Zero))
				{
					controller = new PathFindController(this, base.currentLocation, home.lastKnownCropLocation, -1, reachFirstDestinationFromHut, 100);
				}
				else if (Game1.random.NextDouble() < 0.25)
				{
					netAnimationEvent.Fire(0);
					returnToJunimoHut(base.currentLocation);
				}
				else
				{
					pathfindToRandomSpotAroundHut();
				}
			}
			else
			{
				netAnimationEvent.Fire(0);
			}
		}

		public void pathfindToNewCrop()
		{
			if (backgroundTask == null || backgroundTask.IsCompleted)
			{
				pathFindToNewCrop_doWork();
			}
		}

		public void returnToJunimoHut(GameLocation location)
		{
			if (Utility.isOnScreen(Utility.Vector2ToPoint(position.Value / 64f), 64, base.currentLocation))
			{
				jump();
			}
			collidesWithOtherCharacters.Value = false;
			if (Game1.IsMasterGame)
			{
				controller = new PathFindController(this, location, new Point((int)home.tileX + 1, (int)home.tileY + 1), 0, junimoReachedHut);
				if (controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0 || location.isCollidingPosition(nextPosition, Game1.viewport, isFarmer: false, 0, glider: false, this))
				{
					destroy = true;
				}
			}
			if (Utility.isOnScreen(Utility.Vector2ToPoint(position.Value / 64f), 64, base.currentLocation))
			{
				location.playSound("junimoMeep1");
			}
		}

		public override void faceDirection(int direction)
		{
		}

		protected override void updateSlaveAnimation(GameTime time)
		{
		}

		private bool isHarvestable()
		{
			if (base.currentLocation.terrainFeatures.ContainsKey(getTileLocation()) && base.currentLocation.terrainFeatures[getTileLocation()] is HoeDirt && (base.currentLocation.terrainFeatures[getTileLocation()] as HoeDirt).readyForHarvest())
			{
				return true;
			}
			if (base.currentLocation.terrainFeatures.ContainsKey(getTileLocation()) && base.currentLocation.terrainFeatures[getTileLocation()] is Bush && (int)(base.currentLocation.terrainFeatures[getTileLocation()] as Bush).tileSheetOffset == 1)
			{
				return true;
			}
			return false;
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (backgroundTask != null && !backgroundTask.IsCompleted && Game1.IsMasterGame)
			{
				Sprite.Animate(time, 8, 4, 100f);
				return;
			}
			netAnimationEvent.Poll();
			base.update(time, location);
			if (isPrismatic.Value)
			{
				color.Value = Utility.GetPrismaticColor(whichJunimoFromThisHut);
			}
			forceUpdateTimer = 99999;
			if (eventActor)
			{
				return;
			}
			if (destroy)
			{
				alphaChange = -0.05f;
			}
			alpha += alphaChange;
			if (alpha > 1f)
			{
				alpha = 1f;
			}
			else if (alpha < 0f)
			{
				alpha = 0f;
				if (destroy && Game1.IsMasterGame)
				{
					location.characters.Remove(this);
					home.myJunimos.Remove(this);
				}
			}
			if (Game1.IsMasterGame)
			{
				if (harvestTimer > 0)
				{
					int oldTimer = harvestTimer;
					harvestTimer -= time.ElapsedGameTime.Milliseconds;
					if (harvestTimer > 1800)
					{
						netAnimationEvent.Fire(2);
					}
					else if (harvestTimer > 1600)
					{
						netAnimationEvent.Fire(3);
					}
					else if (harvestTimer > 1000)
					{
						netAnimationEvent.Fire(4);
						shake(50);
					}
					else if (oldTimer >= 1000 && harvestTimer < 1000)
					{
						netAnimationEvent.Fire(2);
						if (base.currentLocation != null && !home.noHarvest && isHarvestable())
						{
							netAnimationEvent.Fire(5);
							lastItemHarvested = null;
							if (base.currentLocation.terrainFeatures[getTileLocation()] is Bush && (int)(base.currentLocation.terrainFeatures[getTileLocation()] as Bush).tileSheetOffset == 1)
							{
								tryToAddItemToHut(new Object(815, 1));
								(base.currentLocation.terrainFeatures[getTileLocation()] as Bush).tileSheetOffset.Value = 0;
								(base.currentLocation.terrainFeatures[getTileLocation()] as Bush).setUpSourceRect();
								if (Utility.isOnScreen(getTileLocationPoint(), 64, base.currentLocation))
								{
									(base.currentLocation.terrainFeatures[getTileLocation()] as Bush).performUseAction(getTileLocation(), base.currentLocation);
								}
								if (Utility.isOnScreen(getTileLocationPoint(), 64, base.currentLocation))
								{
									DelayedAction.playSoundAfterDelay("coin", 260, base.currentLocation);
								}
							}
							else if ((base.currentLocation.terrainFeatures[getTileLocation()] as HoeDirt).crop.harvest(getTileX(), getTileY(), base.currentLocation.terrainFeatures[getTileLocation()] as HoeDirt, this))
							{
								(base.currentLocation.terrainFeatures[getTileLocation()] as HoeDirt).destroyCrop(getTileLocation(), base.currentLocation.farmers.Any(), base.currentLocation);
							}
							if (lastItemHarvested != null && base.currentLocation.farmers.Any())
							{
								Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, lastItemHarvested.parentSheetIndex, 16, 16), 1000f, 1, 0, position + new Vector2(0f, -40f), flicker: false, flipped: false, (float)getStandingY() / 10000f + 0.01f, 0.02f, Color.White, 4f, -0.01f, 0f, 0f)
								{
									motion = new Vector2(0.08f, -0.25f)
								});
								if (lastItemHarvested is ColoredObject)
								{
									Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (int)lastItemHarvested.parentSheetIndex + 1, 16, 16), 1000f, 1, 0, position + new Vector2(0f, -40f), flicker: false, flipped: false, (float)getStandingY() / 10000f + 0.015f, 0.02f, (lastItemHarvested as ColoredObject).color, 4f, -0.01f, 0f, 0f)
									{
										motion = new Vector2(0.08f, -0.25f)
									});
								}
							}
						}
					}
					else if (harvestTimer <= 0)
					{
						pokeToHarvest();
					}
				}
				else if (alpha > 0f && controller == null)
				{
					if ((base.addedSpeed > 0 || base.speed > 2 || isCharging) && Game1.IsMasterGame)
					{
						destroy = true;
					}
					nextPosition = GetBoundingBox();
					nextPosition.X += (int)motion.X;
					bool sparkle = false;
					if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
					{
						position.X += (int)motion.X;
						sparkle = true;
					}
					nextPosition.X -= (int)motion.X;
					nextPosition.Y += (int)motion.Y;
					if (!location.isCollidingPosition(nextPosition, Game1.viewport, this))
					{
						position.Y += (int)motion.Y;
						sparkle = true;
					}
					if (!motion.Equals(Vector2.Zero) && sparkle && Game1.random.NextDouble() < 0.005)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite((Game1.random.NextDouble() < 0.5) ? 10 : 11, position, color)
						{
							motion = motion / 4f,
							alphaFade = 0.01f,
							layerDepth = 0.8f,
							scale = 0.75f,
							alpha = 0.75f
						});
					}
					if (Game1.random.NextDouble() < 0.002)
					{
						switch (Game1.random.Next(6))
						{
						case 0:
							netAnimationEvent.Fire(6);
							break;
						case 1:
							netAnimationEvent.Fire(7);
							break;
						case 2:
							netAnimationEvent.Fire(0);
							break;
						case 3:
							jumpWithoutSound();
							yJumpVelocity /= 2f;
							netAnimationEvent.Fire(0);
							break;
						case 4:
							if (!home.noHarvest)
							{
								pathfindToNewCrop();
							}
							break;
						case 5:
							netAnimationEvent.Fire(8);
							break;
						}
					}
				}
			}
			bool moveRight2 = moveRight;
			bool moveLeft2 = moveLeft;
			bool moveUp2 = moveUp;
			bool moveDown2 = moveDown;
			if (Game1.IsMasterGame)
			{
				if (controller == null && motion.Equals(Vector2.Zero))
				{
					return;
				}
				moveRight2 |= (Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X > 0f);
				moveLeft2 |= (Math.Abs(motion.X) > Math.Abs(motion.Y) && motion.X < 0f);
				moveUp2 |= (Math.Abs(motion.Y) > Math.Abs(motion.X) && motion.Y < 0f);
				moveDown2 |= (Math.Abs(motion.Y) > Math.Abs(motion.X) && motion.Y > 0f);
			}
			else
			{
				moveLeft2 = (IsRemoteMoving() && FacingDirection == 3);
				moveRight2 = (IsRemoteMoving() && FacingDirection == 1);
				moveUp2 = (IsRemoteMoving() && FacingDirection == 0);
				moveDown2 = (IsRemoteMoving() && FacingDirection == 2);
				if (!moveRight2 && !moveLeft2 && !moveUp2 && !moveDown2)
				{
					return;
				}
			}
			Sprite.CurrentAnimation = null;
			if (moveRight2)
			{
				flip = false;
				if (Sprite.Animate(time, 16, 8, 50f))
				{
					Sprite.currentFrame = 16;
				}
			}
			else if (moveLeft2)
			{
				if (Sprite.Animate(time, 16, 8, 50f))
				{
					Sprite.currentFrame = 16;
				}
				flip = true;
			}
			else if (moveUp2)
			{
				if (Sprite.Animate(time, 32, 8, 50f))
				{
					Sprite.currentFrame = 32;
				}
			}
			else if (moveDown2)
			{
				Sprite.Animate(time, 0, 8, 50f);
			}
		}

		public void pathfindToRandomSpotAroundHut()
		{
			controller = new PathFindController(endPoint: Utility.Vector2ToPoint(new Vector2((int)home.tileX + 1 + Game1.random.Next(-8, 9), (int)home.tileY + 1 + Game1.random.Next(-8, 9))), c: this, location: base.currentLocation, finalFacingDirection: -1, endBehaviorFunction: reachFirstDestinationFromHut, limit: 100);
		}

		public void tryToAddItemToHut(Item i)
		{
			lastItemHarvested = i;
			Item result = home.output.Value.addItem(i);
			if (result != null && i is Object)
			{
				for (int j = 0; j < result.Stack; j++)
				{
					Game1.createObjectDebris(i.parentSheetIndex, getTileX(), getTileY(), -1, (i as Object).quality, 1f, base.currentLocation);
				}
			}
		}

		public override void draw(SpriteBatch b, float alpha = 1f)
		{
			if (this.alpha > 0f)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(Sprite.SpriteWidth * 4 / 2, (float)Sprite.SpriteHeight * 3f / 4f * 4f / (float)Math.Pow(Sprite.SpriteHeight / 16, 2.0) + (float)yJumpOffset - 8f) + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), Sprite.SourceRect, color.Value * this.alpha, rotation, new Vector2(Sprite.SpriteWidth * 4 / 2, (float)(Sprite.SpriteHeight * 4) * 3f / 4f) / 4f, Math.Max(0.2f, scale) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)getStandingY() / 10000f)));
				if (!swimming)
				{
					b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2((float)(Sprite.SpriteWidth * 4) / 2f, 44f)), Game1.shadowTexture.Bounds, color.Value * this.alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), (4f + (float)yJumpOffset / 40f) * (float)scale, SpriteEffects.None, Math.Max(0f, (float)getStandingY() / 10000f) - 1E-06f);
				}
			}
		}
	}
}
