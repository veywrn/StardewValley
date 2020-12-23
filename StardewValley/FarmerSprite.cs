using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValley
{
	public class FarmerSprite : AnimatedSprite
	{
		public struct AnimationFrame
		{
			public int frame;

			public int milliseconds;

			public int positionOffset;

			public int xOffset;

			public bool secondaryArm;

			public bool flip;

			public endOfAnimationBehavior frameStartBehavior;

			public endOfAnimationBehavior frameEndBehavior;

			public AnimationFrame(int frame, int milliseconds, int position_offset, bool secondary_arm, bool flip, endOfAnimationBehavior frame_start_behavior, endOfAnimationBehavior frame_end_behavior, int x_offset)
			{
				this.frame = frame;
				this.milliseconds = milliseconds;
				positionOffset = position_offset;
				secondaryArm = secondary_arm;
				this.flip = flip;
				frameStartBehavior = frame_start_behavior;
				frameEndBehavior = frame_end_behavior;
				xOffset = x_offset;
			}

			public AnimationFrame(int frame, int milliseconds, int positionOffset, bool secondaryArm, bool flip, endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false, int xOffset = 0)
			{
				this = new AnimationFrame(frame, milliseconds, positionOffset, secondaryArm, flip, null, null, xOffset);
				if (!behaviorAtEndOfFrame)
				{
					frameStartBehavior = frameBehavior;
				}
				else
				{
					frameEndBehavior = frameBehavior;
				}
			}

			public AnimationFrame(int frame, int milliseconds, bool secondaryArm, bool flip, endOfAnimationBehavior frameBehavior = null, bool behaviorAtEndOfFrame = false)
			{
				this = new AnimationFrame(frame, milliseconds, 0, secondaryArm, flip, frameBehavior, behaviorAtEndOfFrame);
			}

			public AnimationFrame(int frame, int milliseconds)
			{
				this = new AnimationFrame(frame, milliseconds, secondaryArm: false, flip: false);
			}

			public AnimationFrame AddFrameAction(endOfAnimationBehavior callback)
			{
				frameStartBehavior = (endOfAnimationBehavior)Delegate.Combine(frameStartBehavior, callback);
				return this;
			}

			public AnimationFrame AddFrameEndAction(endOfAnimationBehavior callback)
			{
				frameEndBehavior = (endOfAnimationBehavior)Delegate.Combine(frameEndBehavior, callback);
				return this;
			}
		}

		public const int walkDown = 0;

		public const int walkRight = 8;

		public const int walkUp = 16;

		public const int walkLeft = 24;

		public const int runDown = 32;

		public const int runRight = 40;

		public const int runUp = 48;

		public const int runLeft = 56;

		public const int grabDown = 64;

		public const int grabRight = 72;

		public const int grabUp = 80;

		public const int grabLeft = 88;

		public const int carryWalkDown = 96;

		public const int carryWalkRight = 104;

		public const int carryWalkUp = 112;

		public const int carryWalkLeft = 120;

		public const int carryRunDown = 128;

		public const int carryRunRight = 136;

		public const int carryRunUp = 144;

		public const int carryRunLeft = 152;

		public const int toolDown = 160;

		public const int toolRight = 168;

		public const int toolUp = 176;

		public const int toolLeft = 184;

		public const int toolChooseDown = 192;

		public const int toolChooseRight = 194;

		public const int toolChooseUp = 196;

		public const int toolChooseLeft = 198;

		public const int seedThrowDown = 200;

		public const int seedThrowRight = 204;

		public const int seedThrowUp = 208;

		public const int seedThrowLeft = 212;

		public const int eat = 216;

		public const int sick = 224;

		public const int swordswipeDown = 232;

		public const int swordswipeRight = 240;

		public const int swordswipeUp = 248;

		public const int swordswipeLeft = 256;

		public const int punchDown = 272;

		public const int punchRight = 274;

		public const int punchUp = 276;

		public const int punchLeft = 278;

		public const int harvestItemUp = 279;

		public const int harvestItemRight = 280;

		public const int harvestItemDown = 281;

		public const int harvestItemLeft = 282;

		public const int shearUp = 283;

		public const int shearRight = 284;

		public const int shearDown = 285;

		public const int shearLeft = 286;

		public const int milkUp = 287;

		public const int milkRight = 288;

		public const int milkDown = 289;

		public const int milkLeft = 290;

		public const int tired = 291;

		public const int tired2 = 292;

		public const int passOutTired = 293;

		public const int drink = 294;

		public const int fishingUp = 295;

		public const int fishingRight = 296;

		public const int fishingDown = 297;

		public const int fishingLeft = 298;

		public const int fishingDoneUp = 299;

		public const int fishingDoneRight = 300;

		public const int fishingDoneDown = 301;

		public const int fishingDoneLeft = 302;

		public const int pan = 303;

		public const int showHoldingEdible = 304;

		private int currentToolIndex;

		private float oldInterval;

		public bool pauseForSingleAnimation;

		public bool animateBackwards;

		public bool loopThisAnimation;

		public bool freezeUntilDialogueIsOver;

		private int currentSingleAnimation = -1;

		private int currentAnimationFrames;

		public float currentSingleAnimationInterval = 200f;

		public float intervalModifier = 1f;

		public string currentStep = "sandyStep";

		private Farmer owner;

		public bool animatingBackwards;

		public const int cheer = 97;

		public AnimationFrame CurrentAnimationFrame
		{
			get
			{
				if (base.CurrentAnimation == null)
				{
					return new AnimationFrame(0, 100, 0, secondaryArm: false, flip: false);
				}
				return base.CurrentAnimation[currentAnimationIndex % base.CurrentAnimation.Count];
			}
		}

		public int CurrentSingleAnimation
		{
			get
			{
				if (base.CurrentAnimation != null)
				{
					return base.CurrentAnimation[0].frame;
				}
				return -1;
			}
		}

		public override int CurrentFrame
		{
			get
			{
				return currentFrame;
			}
			set
			{
				if (currentFrame != value && !freezeUntilDialogueIsOver)
				{
					currentFrame = value;
					UpdateSourceRect();
				}
				if (value > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
				{
					currentFrame = 0;
				}
			}
		}

		public bool PauseForSingleAnimation
		{
			get
			{
				return pauseForSingleAnimation;
			}
			set
			{
				pauseForSingleAnimation = value;
			}
		}

		public int CurrentToolIndex
		{
			get
			{
				return currentToolIndex;
			}
			set
			{
				currentToolIndex = value;
			}
		}

		public void setOwner(Farmer owner)
		{
			this.owner = owner;
		}

		public void setCurrentAnimation(AnimationFrame[] animation)
		{
			currentSingleAnimation = -1;
			currentAnimation.Clear();
			currentAnimation.AddRange(animation);
			oldFrame = CurrentFrame;
			currentAnimationIndex = 0;
			if (base.CurrentAnimation.Count > 0)
			{
				interval = base.CurrentAnimation[0].milliseconds;
				CurrentFrame = base.CurrentAnimation[0].frame;
				currentAnimationFrames = base.CurrentAnimation.Count;
			}
		}

		public override void faceDirection(int direction)
		{
			bool carrying = false;
			if (owner != null)
			{
				carrying = owner.IsCarrying();
			}
			if (!IsPlayingBasicAnimation(direction, carrying))
			{
				switch (direction)
				{
				case 0:
					setCurrentFrame(12, 1, 100, 1, flip: false, carrying);
					break;
				case 1:
					setCurrentFrame(6, 1, 100, 1, flip: false, carrying);
					break;
				case 2:
					setCurrentFrame(0, 1, 100, 1, flip: false, carrying);
					break;
				case 3:
					setCurrentFrame(6, 1, 100, 1, flip: true, carrying);
					break;
				}
				UpdateSourceRect();
			}
		}

		public virtual bool IsPlayingBasicAnimation(int direction, bool carrying)
		{
			bool moving = false;
			if (owner != null && owner.CanMove && owner.isMoving())
			{
				moving = true;
			}
			switch (direction)
			{
			case 0:
				if (carrying)
				{
					if (!moving)
					{
						if (CurrentFrame == 113)
						{
							return true;
						}
						return false;
					}
					if (currentSingleAnimation == 112 || currentSingleAnimation == 144)
					{
						return true;
					}
					break;
				}
				if (!moving)
				{
					if (CurrentFrame == 17)
					{
						return true;
					}
					return false;
				}
				if (currentSingleAnimation == 16 || currentSingleAnimation == 48)
				{
					return true;
				}
				break;
			case 2:
				if (carrying)
				{
					if (!moving)
					{
						if (CurrentFrame == 97)
						{
							return true;
						}
						return false;
					}
					if (currentSingleAnimation == 96 || currentSingleAnimation == 128)
					{
						return true;
					}
					break;
				}
				if (!moving)
				{
					if (CurrentFrame == 1)
					{
						return true;
					}
					return false;
				}
				if (currentSingleAnimation == 0 || currentSingleAnimation == 32)
				{
					return true;
				}
				break;
			case 3:
				if (carrying)
				{
					if (!moving)
					{
						if (CurrentFrame == 121)
						{
							return true;
						}
						return false;
					}
					if (currentSingleAnimation == 120 || currentSingleAnimation == 152)
					{
						return true;
					}
					break;
				}
				if (!moving)
				{
					if (CurrentFrame == 25)
					{
						return true;
					}
					return false;
				}
				if (currentSingleAnimation == 24 || currentSingleAnimation == 56)
				{
					return true;
				}
				break;
			case 1:
				if (carrying)
				{
					if (!moving)
					{
						if (CurrentFrame == 105)
						{
							return true;
						}
						return false;
					}
					if (currentSingleAnimation == 104 || currentSingleAnimation == 136)
					{
						return true;
					}
					break;
				}
				if (!moving)
				{
					if (CurrentFrame == 9)
					{
						return true;
					}
					return false;
				}
				if (currentSingleAnimation == 8 || currentSingleAnimation == 40)
				{
					return true;
				}
				break;
			}
			return false;
		}

		public void setCurrentSingleFrame(int which, short interval = 32000, bool secondaryArm = false, bool flip = false)
		{
			loopThisAnimation = false;
			currentAnimation.Clear();
			currentAnimation.Add(new AnimationFrame((short)which, interval, secondaryArm, flip));
			CurrentFrame = base.CurrentAnimation[0].frame;
		}

		public void setCurrentFrame(int which)
		{
			setCurrentFrame(which, 0);
		}

		public void setCurrentFrame(int which, int offset)
		{
			setCurrentFrame(which, offset, 100, 1, flip: false, secondaryArm: false);
		}

		public void setCurrentFrameBackwards(int which, int offset, int interval, int numFrames, bool secondaryArm, bool flip)
		{
			getAnimationFromIndex(which, this, interval, numFrames, secondaryArm, flip);
			base.CurrentAnimation.Reverse();
			CurrentFrame = base.CurrentAnimation[Math.Min(base.CurrentAnimation.Count - 1, offset)].frame;
		}

		public void setCurrentFrame(int which, int offset, int interval, int numFrames, bool flip, bool secondaryArm)
		{
			getAnimationFromIndex(which, this, interval, numFrames, flip, secondaryArm);
			currentAnimationIndex = Math.Min(base.CurrentAnimation.Count - 1, offset);
			CurrentFrame = base.CurrentAnimation[currentAnimationIndex].frame;
			base.interval = CurrentAnimationFrame.milliseconds;
			timer = 0f;
		}

		public FarmerSprite()
		{
			interval /= 2f;
			base.SpriteWidth = 16;
			base.SpriteHeight = 32;
			UpdateSourceRect();
		}

		public FarmerSprite(string texture)
			: base(texture)
		{
			interval /= 2f;
			base.SpriteWidth = 16;
			base.SpriteHeight = 32;
			UpdateSourceRect();
		}

		public void animate(int whichAnimation, GameTime time)
		{
			animate(whichAnimation, time.ElapsedGameTime.Milliseconds);
		}

		public void animate(int whichAnimation, int milliseconds)
		{
			if (!PauseForSingleAnimation)
			{
				if (whichAnimation != currentSingleAnimation || base.CurrentAnimation == null || base.CurrentAnimation.Count <= 1)
				{
					float oldtimer = timer;
					int oldIndex = currentAnimationIndex;
					currentSingleAnimation = whichAnimation;
					setCurrentFrame(whichAnimation);
					timer = oldtimer;
					CurrentFrame = base.CurrentAnimation[Math.Min(oldIndex, base.CurrentAnimation.Count - 1)].frame;
					currentAnimationIndex = oldIndex % base.CurrentAnimation.Count;
					UpdateSourceRect();
				}
				animate(milliseconds);
			}
		}

		public void checkForSingleAnimation(GameTime time)
		{
			if (PauseForSingleAnimation)
			{
				if (!animateBackwards)
				{
					animateOnce(time);
				}
				else
				{
					animateBackwardsOnce(time);
				}
			}
		}

		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames)
		{
			animateOnce(whichAnimation, animationInterval, numberOfFrames, null);
		}

		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction)
		{
			animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, flip: false, secondaryArm: false);
		}

		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm)
		{
			animateOnce(whichAnimation, animationInterval, numberOfFrames, endOfBehaviorFunction, flip, secondaryArm, backwards: false);
		}

		public void animateOnce(AnimationFrame[] animation, endOfAnimationBehavior endOfBehaviorFunction = null)
		{
			if (!PauseForSingleAnimation)
			{
				currentSingleAnimation = -1;
				CurrentFrame = currentSingleAnimation;
				PauseForSingleAnimation = true;
				oldFrame = CurrentFrame;
				oldInterval = interval;
				currentSingleAnimationInterval = 100f;
				timer = 0f;
				currentAnimation.Clear();
				currentAnimation.AddRange(animation);
				CurrentFrame = base.CurrentAnimation[0].frame;
				currentAnimationFrames = base.CurrentAnimation.Count;
				currentAnimationIndex = 0;
				interval = CurrentAnimationFrame.milliseconds;
				loopThisAnimation = false;
				endOfAnimationFunction = endOfBehaviorFunction;
				if (currentAnimationFrames > 0 && base.CurrentAnimation[0].frameStartBehavior != null)
				{
					base.CurrentAnimation[0].frameStartBehavior(owner);
				}
			}
		}

		public void showFrameUntilDialogueOver(int whichFrame)
		{
			freezeUntilDialogueIsOver = true;
			setCurrentFrame(whichFrame);
			UpdateSourceRect();
		}

		public void animateOnce(int whichAnimation, float animationInterval, int numberOfFrames, endOfAnimationBehavior endOfBehaviorFunction, bool flip, bool secondaryArm, bool backwards)
		{
			if (PauseForSingleAnimation || freezeUntilDialogueIsOver)
			{
				return;
			}
			currentSingleAnimation = whichAnimation;
			CurrentFrame = currentSingleAnimation;
			PauseForSingleAnimation = true;
			oldFrame = CurrentFrame;
			oldInterval = interval;
			currentSingleAnimationInterval = animationInterval;
			endOfAnimationFunction = endOfBehaviorFunction;
			timer = 0f;
			animatingBackwards = false;
			if (backwards)
			{
				animatingBackwards = true;
				setCurrentFrameBackwards(currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
			}
			else
			{
				setCurrentFrame(currentSingleAnimation, 0, (int)animationInterval, numberOfFrames, secondaryArm, flip);
			}
			if (base.CurrentAnimation[0].frameStartBehavior != null)
			{
				base.CurrentAnimation[0].frameStartBehavior(owner);
			}
			if (owner.Stamina <= 0f && (bool)owner.usingTool)
			{
				for (int i = 0; i < base.CurrentAnimation.Count; i++)
				{
					base.CurrentAnimation[i] = new AnimationFrame(base.CurrentAnimation[i].frame, base.CurrentAnimation[i].milliseconds * 2, base.CurrentAnimation[i].positionOffset, base.CurrentAnimation[i].secondaryArm, base.CurrentAnimation[i].flip, base.CurrentAnimation[i].frameStartBehavior, base.CurrentAnimation[i].frameEndBehavior, base.CurrentAnimation[i].xOffset);
				}
			}
			currentAnimationFrames = base.CurrentAnimation.Count;
			currentAnimationIndex = 0;
			interval = CurrentAnimationFrame.milliseconds;
			if (!owner.UsingTool || owner.CurrentTool == null)
			{
				return;
			}
			CurrentToolIndex = owner.CurrentTool.CurrentParentTileIndex;
			if (owner.CurrentTool is FishingRod)
			{
				if (owner.FacingDirection == 3 || owner.FacingDirection == 1)
				{
					CurrentToolIndex = 55;
				}
				else
				{
					CurrentToolIndex = 48;
				}
			}
		}

		public void animateBackwardsOnce(int whichAnimation, float animationInterval)
		{
			animateOnce(whichAnimation, animationInterval, 6, null, flip: false, secondaryArm: false, backwards: true);
		}

		public bool isUsingWeapon()
		{
			if (PauseForSingleAnimation)
			{
				if (currentSingleAnimation < 232 || currentSingleAnimation >= 264)
				{
					if (currentSingleAnimation >= 272)
					{
						return currentSingleAnimation < 280;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public int getWeaponTypeFromAnimation()
		{
			if (currentSingleAnimation >= 272 && currentSingleAnimation < 280)
			{
				return 1;
			}
			if (currentSingleAnimation >= 232 && currentSingleAnimation < 264)
			{
				return 3;
			}
			return -1;
		}

		public bool isOnToolAnimation()
		{
			if (PauseForSingleAnimation || owner.UsingTool)
			{
				if ((currentSingleAnimation < 160 || currentSingleAnimation >= 192) && (currentSingleAnimation < 232 || currentSingleAnimation >= 264))
				{
					if (currentSingleAnimation >= 272)
					{
						return currentSingleAnimation < 280;
					}
					return false;
				}
				return true;
			}
			return false;
		}

		public bool isPassingOut()
		{
			if (PauseForSingleAnimation)
			{
				if (currentSingleAnimation != 293)
				{
					return CurrentFrame == 5;
				}
				return true;
			}
			return false;
		}

		private void doneWithAnimation()
		{
			CurrentFrame--;
			interval = oldInterval;
			if (!Game1.eventUp)
			{
				owner.CanMove = true;
				owner.Halt();
			}
			PauseForSingleAnimation = false;
			animatingBackwards = false;
		}

		private void currentAnimationTick()
		{
			if (currentAnimationIndex < base.CurrentAnimation.Count)
			{
				if (base.CurrentAnimation[currentAnimationIndex].frameEndBehavior != null)
				{
					base.CurrentAnimation[currentAnimationIndex].frameEndBehavior(owner);
				}
				currentAnimationIndex++;
				if (loopThisAnimation)
				{
					currentAnimationIndex %= base.CurrentAnimation.Count;
				}
				else if (currentAnimationIndex >= base.CurrentAnimation.Count)
				{
					loopThisAnimation = false;
					return;
				}
				if (base.CurrentAnimation[currentAnimationIndex].frameStartBehavior != null)
				{
					base.CurrentAnimation[currentAnimationIndex].frameStartBehavior(owner);
				}
				if (base.CurrentAnimation != null && currentAnimationIndex < base.CurrentAnimation.Count)
				{
					currentSingleAnimationInterval = base.CurrentAnimation[currentAnimationIndex].milliseconds;
					CurrentFrame = base.CurrentAnimation[currentAnimationIndex].frame;
					interval = base.CurrentAnimation[currentAnimationIndex].milliseconds;
				}
				else
				{
					owner.completelyStopAnimatingOrDoingAction();
					owner.forceCanMove();
				}
			}
		}

		public override void UpdateSourceRect()
		{
			base.SourceRect = new Rectangle(CurrentFrame * base.SpriteWidth % 96, CurrentFrame * base.SpriteWidth / 96 * base.SpriteHeight, base.SpriteWidth, base.SpriteHeight);
		}

		private new void animateOnce(GameTime time)
		{
			if (freezeUntilDialogueIsOver || owner == null)
			{
				return;
			}
			timer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (timer > interval * intervalModifier)
			{
				currentAnimationTick();
				timer = 0f;
				if (currentAnimationIndex > currentAnimationFrames - 1)
				{
					if (CurrentAnimationFrame.frameEndBehavior != null)
					{
						CurrentAnimationFrame.frameEndBehavior(owner);
					}
					if (base.endOfAnimationFunction != null)
					{
						endOfAnimationBehavior endOfAnimationFunction = base.endOfAnimationFunction;
						base.endOfAnimationFunction = null;
						endOfAnimationFunction(owner);
						if (owner.UsingTool && owner.CurrentTool.Name.Equals("Fishing Rod"))
						{
							PauseForSingleAnimation = false;
							interval = oldInterval;
							owner.CanMove = false;
						}
						else if (!(owner.CurrentTool is MeleeWeapon) || (int)(owner.CurrentTool as MeleeWeapon).type != 1)
						{
							doneWithAnimation();
						}
						return;
					}
					doneWithAnimation();
					if (owner.isEating)
					{
						owner.doneEating();
					}
				}
				switch (currentSingleAnimation)
				{
				case 160:
				case 161:
				case 165:
					if (owner.CurrentTool != null)
					{
						owner.CurrentTool.Update(2, currentAnimationIndex, owner);
					}
					break;
				case 176:
				case 180:
				case 181:
					if (owner.CurrentTool != null)
					{
						owner.CurrentTool.Update(0, currentAnimationIndex, owner);
					}
					break;
				}
				if (CurrentFrame == 109 && owner.ShouldHandleAnimationSound())
				{
					owner.currentLocation.localSound("eat");
				}
				if (isOnToolAnimation() && !isUsingWeapon() && currentAnimationIndex == 4 && currentToolIndex % 2 == 0 && !(owner.CurrentTool is FishingRod))
				{
					currentToolIndex++;
				}
			}
			UpdateSourceRect();
		}

		private void checkForFootstep()
		{
			if (Game1.player.isRidingHorse() || owner == null || owner.currentLocation != Game1.currentLocation)
			{
				return;
			}
			Vector2 tileLocationOfPlayer = (owner != null) ? owner.getTileLocation() : Game1.player.getTileLocation();
			if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.ToLower().Contains("mine") || Game1.currentLocation.Name.ToLower().Contains("cave") || Game1.currentLocation.IsGreenhouse)
			{
				string stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Buildings");
				if (stepType == null || stepType.Length < 1)
				{
					stepType = Game1.currentLocation.doesTileHaveProperty((int)tileLocationOfPlayer.X, (int)tileLocationOfPlayer.Y, "Type", "Back");
				}
				if (stepType != null)
				{
					if (!(stepType == "Dirt"))
					{
						if (!(stepType == "Stone"))
						{
							if (!(stepType == "Grass"))
							{
								if (stepType == "Wood")
								{
									currentStep = "woodyStep";
								}
							}
							else
							{
								currentStep = (Game1.currentLocation.GetSeasonForLocation().Equals("winter") ? "snowyStep" : "grassyStep");
							}
						}
						else
						{
							currentStep = "stoneStep";
						}
					}
					else
					{
						currentStep = "sandyStep";
					}
				}
			}
			else
			{
				currentStep = "thudStep";
			}
			if (((currentSingleAnimation >= 32 && currentSingleAnimation <= 56) || (currentSingleAnimation >= 128 && currentSingleAnimation <= 152)) && currentAnimationIndex % 4 == 0)
			{
				string played_step2 = currentStep;
				played_step2 = owner.currentLocation.getFootstepSoundReplacement(played_step2);
				if (owner.onBridge.Value)
				{
					if (owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(owner.Position, 384))
					{
						played_step2 = "thudStep";
					}
					if (owner.bridge != null)
					{
						owner.bridge.OnFootstep(owner.Position);
					}
				}
				if (Game1.currentLocation.terrainFeatures.ContainsKey(tileLocationOfPlayer) && Game1.currentLocation.terrainFeatures[tileLocationOfPlayer] is Flooring)
				{
					played_step2 = ((Flooring)Game1.currentLocation.terrainFeatures[tileLocationOfPlayer]).getFootstepSound();
				}
				Vector2 owner_position = owner.position;
				if (owner.shouldShadowBeOffset)
				{
					owner_position += owner.drawOffset.Value;
				}
				if (played_step2.Equals("sandyStep"))
				{
					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-8, 8), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), flicker: false, Game1.random.NextDouble() < 0.5, owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.75f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f));
					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(128, 2948, 64, 64), 80f, 8, 0, new Vector2(owner_position.X + 16f + (float)Game1.random.Next(-4, 4), owner_position.Y + (float)(Game1.random.Next(-3, -1) * 4)), flicker: false, Game1.random.NextDouble() < 0.5, owner_position.Y / 10000f, 0.03f, Color.Khaki * 0.45f, 0.55f + (float)Game1.random.Next(-3, 4) * 0.05f, 0f, 0f, 0f)
					{
						delayBeforeAnimationStart = 20
					});
				}
				else if (played_step2.Equals("snowyStep"))
				{
					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(247, 407, 6, 6), 2000f, 1, 10000, new Vector2(owner_position.X + 24f + (float)(Game1.random.Next(-4, 4) * 4), owner_position.Y + 8f + (float)(Game1.random.Next(-4, 4) * 4)), flicker: false, flipped: false, owner_position.Y / 1E+07f, 0.01f, Color.White, 3f + (float)Game1.random.NextDouble(), 0f, ((int)owner.facingDirection == 1 || (int)owner.facingDirection == 3) ? (-(float)Math.PI / 4f) : 0f, 0f));
				}
				if (played_step2 != null && owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(owner.Position, 384) && (owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(is_local_only: true)))
				{
					Game1.playSound(played_step2);
				}
				Game1.stats.takeStep();
			}
			else
			{
				if ((currentSingleAnimation < 0 || currentSingleAnimation > 24) && (currentSingleAnimation < 96 || currentSingleAnimation > 120))
				{
					return;
				}
				if (owner.onBridge.Value && currentAnimationIndex % 2 == 0)
				{
					if (owner.currentLocation == Game1.currentLocation && Utility.isOnScreen(owner.Position, 384) && (owner == Game1.player || !LocalMultiplayer.IsLocalMultiplayer(is_local_only: true)))
					{
						Game1.playSound("thudStep");
					}
					if (owner.bridge != null)
					{
						owner.bridge.OnFootstep(owner.Position);
					}
				}
				if (currentAnimationIndex == 0)
				{
					Game1.stats.takeStep();
				}
			}
		}

		private void animateBackwardsOnce(GameTime time)
		{
			timer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (timer > currentSingleAnimationInterval)
			{
				CurrentFrame--;
				timer = 0f;
				if (currentAnimationIndex > currentAnimationFrames - 1)
				{
					if (CurrentFrame < 63 || CurrentFrame > 96)
					{
						CurrentFrame = oldFrame;
					}
					else
					{
						CurrentFrame = CurrentFrame % 16 + 8;
					}
					interval = oldInterval;
					PauseForSingleAnimation = false;
					animatingBackwards = false;
					if (!Game1.eventUp)
					{
						owner.CanMove = true;
					}
					if (owner.CurrentTool != null && owner.CurrentTool.Name.Equals("Fishing Rod"))
					{
						owner.UsingTool = false;
					}
					owner.Halt();
					if ((CurrentSingleAnimation >= 160 && CurrentSingleAnimation < 192) || (CurrentSingleAnimation >= 200 && CurrentSingleAnimation < 216) || (CurrentSingleAnimation >= 232 && CurrentSingleAnimation < 264))
					{
						Game1.toolAnimationDone(owner);
					}
				}
				if (owner.UsingTool && owner.CurrentTool != null && owner.CurrentTool.Name.Equals("Fishing Rod"))
				{
					switch (CurrentFrame)
					{
					case 164:
						owner.CurrentTool.Update(2, 0, owner);
						break;
					case 168:
						owner.CurrentTool.Update(1, 0, owner);
						break;
					case 180:
						owner.CurrentTool.Update(0, 0, owner);
						break;
					case 184:
						owner.CurrentTool.Update(3, 0, owner);
						break;
					}
				}
			}
			UpdateSourceRect();
		}

		public int frameOfCurrentSingleAnimation()
		{
			if (PauseForSingleAnimation)
			{
				return CurrentFrame - (currentSingleAnimation - currentSingleAnimation % 8);
			}
			if (!Game1.pickingTool && owner.CurrentTool != null && owner.CurrentTool.Name.Equals("Watering Can"))
			{
				return 4;
			}
			if (!Game1.pickingTool && owner.UsingTool && ((currentToolIndex >= 48 && currentToolIndex <= 55) || (owner.CurrentTool != null && owner.CurrentTool.Name.Equals("Fishing Rod"))))
			{
				return 6;
			}
			return 0;
		}

		public void setCurrentSingleAnimation(int which)
		{
			CurrentFrame = which;
			currentSingleAnimation = which;
			getAnimationFromIndex(which, this, 100, 1, flip: false, secondaryArm: false);
			if (base.CurrentAnimation != null && base.CurrentAnimation.Count > 0)
			{
				currentAnimationFrames = base.CurrentAnimation.Count;
				interval = base.CurrentAnimation.First().milliseconds;
				CurrentFrame = base.CurrentAnimation.First().frame;
			}
			if (interval <= 50f)
			{
				interval = 800f;
			}
			UpdateSourceRect();
		}

		private void animate(int Milliseconds, int firstFrame, int lastFrame)
		{
			if (CurrentFrame > lastFrame || CurrentFrame < firstFrame)
			{
				CurrentFrame = firstFrame;
			}
			timer += Milliseconds;
			if (timer > interval * intervalModifier)
			{
				CurrentFrame++;
				timer = 0f;
				if (CurrentFrame > lastFrame)
				{
					CurrentFrame = firstFrame;
				}
				checkForFootstep();
			}
			UpdateSourceRect();
		}

		private void animate(int Milliseconds)
		{
			timer += Milliseconds;
			if (timer > interval * intervalModifier)
			{
				currentAnimationTick();
				timer = 0f;
				checkForFootstep();
			}
			UpdateSourceRect();
		}

		public override void StopAnimation()
		{
			bool animation_dirty = false;
			if (pauseForSingleAnimation)
			{
				return;
			}
			interval = 0f;
			if (CurrentFrame >= 64 && CurrentFrame <= 155 && owner != null && !owner.bathingClothes)
			{
				switch (owner.FacingDirection)
				{
				case 0:
					CurrentFrame = 12;
					break;
				case 1:
					CurrentFrame = 6;
					break;
				case 2:
					CurrentFrame = 0;
					break;
				case 3:
					CurrentFrame = 6;
					break;
				}
				animation_dirty = true;
			}
			else if (!Game1.pickingTool && owner != null)
			{
				bool carrying = owner.ActiveObject != null && Game1.eventUp;
				if (!IsPlayingBasicAnimation(owner.facingDirection, carrying))
				{
					animation_dirty = true;
					switch (owner.FacingDirection)
					{
					case 0:
						if (owner.ActiveObject != null && !Game1.eventUp)
						{
							setCurrentFrame(112, 1);
						}
						else
						{
							setCurrentFrame(16, 1);
						}
						break;
					case 1:
						if (owner.ActiveObject != null && !Game1.eventUp)
						{
							setCurrentFrame(104, 1);
						}
						else
						{
							setCurrentFrame(8, 1);
						}
						break;
					case 2:
						if (owner.ActiveObject != null && !Game1.eventUp)
						{
							setCurrentFrame(96, 1);
						}
						else
						{
							setCurrentFrame(0, 1);
						}
						break;
					case 3:
						if (owner.ActiveObject != null && !Game1.eventUp)
						{
							setCurrentFrame(120, 1);
						}
						else
						{
							setCurrentFrame(24, 1);
						}
						break;
					}
					currentSingleAnimation = -1;
				}
			}
			if (animation_dirty)
			{
				currentAnimationIndex = 0;
				UpdateSourceRect();
			}
		}

		public static void getAnimationFromIndex(int index, FarmerSprite requester, int interval, int numberOfFrames, bool flip, bool secondaryArm)
		{
			bool showCarryingArm = (index >= 96 && index < 160) || index == 232 || index == 248;
			if (requester.owner != null && requester.owner.ActiveObject != null && requester.owner.ActiveObject is Furniture)
			{
				showCarryingArm = false;
			}
			requester.loopThisAnimation = true;
			int frameOffset = 0;
			if (requester.owner != null && (bool)requester.owner.bathingClothes)
			{
				frameOffset += 108;
			}
			List<AnimationFrame> outFrames = requester.currentAnimation;
			outFrames.Clear();
			float toolSpeedModifier = 1f;
			if (requester.owner != null && requester.owner.CurrentTool != null)
			{
				toolSpeedModifier = requester.owner.CurrentTool.AnimationSpeedModifier;
			}
			requester.currentSingleAnimation = index;
			switch (index)
			{
			case -1:
				outFrames.Add(new AnimationFrame(0, 100, showCarryingArm, flip: false));
				return;
			case 0:
			case 96:
				outFrames.Add(new AnimationFrame(1 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(2 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(frameOffset, 200, showCarryingArm, flip: false));
				return;
			case 8:
			case 104:
				outFrames.Add(new AnimationFrame(7 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(8 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: false));
				return;
			case 16:
			case 112:
				outFrames.Add(new AnimationFrame(13 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(12 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(14 + frameOffset, 200, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(12 + frameOffset, 200, showCarryingArm, flip: false));
				return;
			case 24:
			case 120:
				outFrames.Add(new AnimationFrame(7 + frameOffset, 200, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(8 + frameOffset, 200, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(6 + frameOffset, 200, showCarryingArm, flip: true));
				return;
			case 32:
			case 128:
				outFrames.Add(new AnimationFrame(0, 90, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(1, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(18, 120, -4, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(1, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(0, 90, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(2, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(19, 120, -4, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(2, 60, -2, showCarryingArm, flip: false));
				return;
			case 40:
			case 136:
				outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(20, 140, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(11, 100, 0, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(21, 140, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(17, 100, 0, showCarryingArm, flip: false));
				return;
			case 48:
			case 144:
				outFrames.Add(new AnimationFrame(12, 90, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(13, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(22, 120, -3, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(13, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(12, 90, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(14, 60, -2, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(23, 120, -3, showCarryingArm, flip: false));
				outFrames.Add(new AnimationFrame(14, 60, -2, showCarryingArm, flip: false));
				return;
			case 56:
			case 152:
				outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(20, 140, -2, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(11, 100, 0, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(6, 80, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(6, 10, -1, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(21, 140, -2, showCarryingArm, flip: true));
				outFrames.Add(new AnimationFrame(17, 100, 0, showCarryingArm, flip: true));
				return;
			case 232:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(24, 55, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(25, 45, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(26, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(27, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(28, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(29, (short)interval * 2, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(29, 0, showCarryingArm, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 160:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(66, (int)(150f * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(67, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(68, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
				outFrames.Add(new AnimationFrame(69, (short)((float)(170 + requester.owner.toolPower * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(70, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 297:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(66, 100, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(67, 40, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(68, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(69, 80, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(70, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
				return;
			case 301:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(74, 5000, secondaryArm: false, flip: false));
				return;
			case 240:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(30, 55, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(31, 45, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(32, 25, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, 25, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(34, 25, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(35, (short)interval * 2, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(35, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 168:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(48, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(49, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(50, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
				outFrames.Add(new AnimationFrame(51, (short)((float)(220 + requester.owner.toolPower * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(52, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 296:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(48, 100, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(49, 40, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(50, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(51, 80, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(52, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
				return;
			case 300:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(72, 5000, secondaryArm: false, flip: false));
				return;
			case 248:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(36, 55, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(37, 45, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(38, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(39, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(40, 25, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(41, (short)interval * 2, showCarryingArm, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(41, 0, showCarryingArm, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 176:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(36, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(37, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(38, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.useTool));
				outFrames.Add(new AnimationFrame(63, (short)((float)(220 + requester.owner.toolPower * 30) * toolSpeedModifier), secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(62, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 295:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(76, 100, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(38, 40, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(63, 40, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(62, 80, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(63, 200, secondaryArm: false, flip: false, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
				return;
			case 299:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(76, 5000, secondaryArm: false, flip: false));
				return;
			case 256:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(30, 55, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(31, 45, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(32, 25, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, 25, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(34, 25, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(35, (short)interval * 2, secondaryArm: true, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(35, 0, secondaryArm: true, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 184:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(48, (int)(100f * toolSpeedModifier), secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(49, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(50, (int)(40f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.useTool));
				outFrames.Add(new AnimationFrame(51, (short)((float)(220 + requester.owner.toolPower * 30) * toolSpeedModifier), secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(52, (int)(75f * toolSpeedModifier), secondaryArm: false, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 298:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(48, 100, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(49, 40, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(50, 40, secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(51, 80, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(52, 200, secondaryArm: false, flip: true, FishingRod.doneWithCastingAnimation, behaviorAtEndOfFrame: true));
				return;
			case 302:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(72, 5000, secondaryArm: false, flip: true));
				return;
			case 234:
				index = 28;
				secondaryArm = true;
				break;
			case 258:
			case 259:
				index = 34;
				flip = true;
				break;
			case 242:
			case 243:
				index = 34;
				break;
			case 252:
				index = 40;
				secondaryArm = true;
				break;
			case 272:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(25, (short)interval, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(27, (short)interval, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(27, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 274:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(34, (short)interval, secondaryArm: false, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, (short)interval, secondaryArm: false, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, 0, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 276:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(40, (short)interval, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(38, (short)interval, secondaryArm: true, flip: false, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(38, 0, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 278:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(34, (short)interval, secondaryArm: false, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, (short)interval, secondaryArm: false, flip: true, Farmer.showSwordSwipe));
				outFrames.Add(new AnimationFrame(33, 0, secondaryArm: false, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 192:
				index = 3;
				interval = 500;
				break;
			case 194:
				index = 9;
				interval = 500;
				break;
			case 196:
				index = 15;
				interval = 500;
				break;
			case 198:
				index = 9;
				flip = true;
				interval = 500;
				break;
			case 180:
			case 182:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(62, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(62, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(63, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(46, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 172:
			case 174:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(58, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(45, 500, secondaryArm: true, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 164:
			case 166:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(54, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(54, 75, secondaryArm: false, flip: false, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(55, 100, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(25, 500, secondaryArm: false, flip: false, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 188:
			case 190:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(58, 75, secondaryArm: false, flip: true, Farmer.showToolSwipeEffect));
				outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(45, 500, secondaryArm: true, flip: true, Farmer.canMoveNow, behaviorAtEndOfFrame: true));
				return;
			case 80:
			case 87:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(12, 0, secondaryArm: false, flip: false));
				return;
			case 72:
			case 79:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(6, 0, secondaryArm: false, flip: false));
				return;
			case 64:
			case 71:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(0, 0, secondaryArm: false, flip: false));
				return;
			case 88:
			case 95:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(6, 0, secondaryArm: false, flip: true));
				return;
			case 281:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(54, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(54, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(55, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(56, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(57, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(57, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
				return;
			case 280:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(58, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(60, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(61, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(61, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
				return;
			case 279:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(62, 0, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(62, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(63, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(64, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(65, 100, secondaryArm: false, flip: false, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(65, 0, secondaryArm: false, flip: false, Farmer.showItemIntake));
				return;
			case 282:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 0, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(58, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(59, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(60, 100, secondaryArm: false, flip: true, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(61, 200, secondaryArm: false, flip: true, Farmer.showItemIntake));
				outFrames.Add(new AnimationFrame(61, 0, secondaryArm: false, flip: true, Farmer.showItemIntake));
				return;
			case 283:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(82, 400));
				outFrames.Add(new AnimationFrame(83, 400, secondaryArm: false, flip: false, Shears.playSnip));
				outFrames.Add(new AnimationFrame(82, 400));
				outFrames.Add(new AnimationFrame(83, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 284:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(80, 400));
				outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: false, Shears.playSnip));
				outFrames.Add(new AnimationFrame(80, 400));
				outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 285:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(78, 400));
				outFrames.Add(new AnimationFrame(79, 400, secondaryArm: false, flip: false, Shears.playSnip));
				outFrames.Add(new AnimationFrame(78, 400));
				outFrames.Add(new AnimationFrame(79, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 286:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(80, 400, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: true, Shears.playSnip));
				outFrames.Add(new AnimationFrame(80, 400, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(81, 400, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 287:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(62, 400));
				outFrames.Add(new AnimationFrame(63, 400));
				outFrames.Add(new AnimationFrame(62, 400));
				outFrames.Add(new AnimationFrame(63, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 288:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 400));
				outFrames.Add(new AnimationFrame(59, 400));
				outFrames.Add(new AnimationFrame(58, 400));
				outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 289:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(54, 400));
				outFrames.Add(new AnimationFrame(55, 400));
				outFrames.Add(new AnimationFrame(54, 400));
				outFrames.Add(new AnimationFrame(55, 400, secondaryArm: false, flip: false, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 290:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(58, 400, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(58, 400, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(59, 400, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 216:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(0, 0));
				outFrames.Add(new AnimationFrame(84, (requester.owner.mostRecentlyGrabbedItem != null && requester.owner.mostRecentlyGrabbedItem is Object && (requester.owner.mostRecentlyGrabbedItem as Object).ParentSheetIndex == 434) ? 1000 : 250, secondaryArm: false, flip: false, Farmer.showEatingItem));
				outFrames.Add(new AnimationFrame(85, 400, secondaryArm: false, flip: false, Farmer.showEatingItem));
				outFrames.Add(new AnimationFrame(86, 1, secondaryArm: false, flip: false, Farmer.showEatingItem, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(86, 400, secondaryArm: false, flip: false, Farmer.showEatingItem, behaviorAtEndOfFrame: true));
				outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(88, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(88, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(87, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(0, 250, secondaryArm: false, flip: false, Farmer.showEatingItem));
				return;
			case 304:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(84, 99999999, secondaryArm: false, flip: false, Farmer.showEatingItem));
				return;
			case 294:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(0, 1));
				outFrames.Add(new AnimationFrame(90, 250));
				outFrames.Add(new AnimationFrame(91, 150));
				outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
				outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
				outFrames.Add(new AnimationFrame(92, 250, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(93, 200, secondaryArm: false, flip: false, Farmer.drinkGlug));
				outFrames.Add(new AnimationFrame(91, 250));
				outFrames.Add(new AnimationFrame(90, 50));
				return;
			case 224:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(104, 350, secondaryArm: false, flip: false));
				outFrames.Add(new AnimationFrame(105, 350, secondaryArm: false, flip: false));
				return;
			case 83:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(0, 0, secondaryArm: false, flip: false));
				return;
			case 43:
				flip = (requester.owner.FacingDirection == 3);
				break;
			case 999996:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(96, 800, secondaryArm: false, flip: false));
				return;
			case 97:
				requester.loopThisAnimation = false;
				flip = (requester.owner.FacingDirection == 3);
				outFrames.Add(new AnimationFrame(97, 800, secondaryArm: false, flip));
				return;
			case 303:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(124, 150, secondaryArm: false, flip: true, Pan.playSlosh));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(125, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(124, 150, secondaryArm: false, flip: true, Pan.playSlosh));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(125, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(124, 150, secondaryArm: false, flip: true, Pan.playSlosh));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(125, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(123, 150, secondaryArm: false, flip: true));
				outFrames.Add(new AnimationFrame(124, 150, secondaryArm: false, flip: true, Pan.playSlosh));
				outFrames.Add(new AnimationFrame(123, 500, secondaryArm: false, flip: true, Farmer.useTool, behaviorAtEndOfFrame: true));
				return;
			case 291:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(16, 1500));
				outFrames.Add(new AnimationFrame(16, 1, secondaryArm: false, flip: false, Farmer.completelyStopAnimating));
				return;
			case 292:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(16, 500));
				outFrames.Add(new AnimationFrame(0, 500));
				outFrames.Add(new AnimationFrame(16, 500));
				outFrames.Add(new AnimationFrame(0, 500));
				outFrames.Add(new AnimationFrame(0, 1, secondaryArm: false, flip: false, Farmer.completelyStopAnimating));
				return;
			case 293:
				requester.loopThisAnimation = false;
				outFrames.Add(new AnimationFrame(16, 1000));
				outFrames.Add(new AnimationFrame(0, 500));
				outFrames.Add(new AnimationFrame(16, 1000));
				outFrames.Add(new AnimationFrame(4, 200));
				outFrames.Add(new AnimationFrame(5, 2000, secondaryArm: false, flip: false, Farmer.doSleepEmote));
				outFrames.Add(new AnimationFrame(5, 2000, secondaryArm: false, flip: false, Farmer.passOutFromTired));
				outFrames.Add(new AnimationFrame(5, 2000));
				return;
			}
			if (index > FarmerRenderer.featureYOffsetPerFrame.Length - 1)
			{
				index = 0;
			}
			requester.loopThisAnimation = false;
			for (int i = 0; i < numberOfFrames; i++)
			{
				outFrames.Add(new AnimationFrame((short)(i + index), (short)interval, secondaryArm, flip));
			}
		}
	}
}
