using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley.Characters
{
	public class Dog : Pet
	{
		public const int behavior_SitSide = 50;

		public const int behavior_Sprint = 51;

		public const int behavior_StandUp = 54;

		public const int behavior_StandUpRight = 55;

		protected int sprintTimer;

		private bool wagging;

		public Dog()
		{
			Sprite = new AnimatedSprite(getPetTextureName(), 0, 32, 32);
			base.HideShadow = true;
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
		}

		public override void OnPetAnimationEvent(string animation_event)
		{
			if (base.CurrentBehavior == 1)
			{
				return;
			}
			if (animation_event == "bark")
			{
				if (Utility.isOnScreen(getTileLocationPoint(), 640, base.currentLocation))
				{
					Game1.playSound("dog_bark");
					shake(500);
				}
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(26, 500, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(23, 1, secondaryArm: false, flip: false, base.hold)
				});
			}
			else if (animation_event == "close_eyes")
			{
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(27, (Game1.random.NextDouble() < 0.3) ? 500 : Game1.random.Next(2000, 15000)),
					new FarmerSprite.AnimationFrame(18, 1, secondaryArm: false, flip: false, base.hold)
				});
				Sprite.loop = false;
			}
			else if (animation_event == "sit_animation")
			{
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(23, Game1.random.Next(2000, 6000), secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(23, 1, secondaryArm: false, flip: false, base.hold)
				});
			}
			else if (animation_event == "wag")
			{
				wag();
			}
			else
			{
				if (!(animation_event == "pant"))
				{
					return;
				}
				if (base.CurrentBehavior == 50)
				{
					List<FarmerSprite.AnimationFrame> panting = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(24, 200, secondaryArm: false, flip: false, pantSound),
						new FarmerSprite.AnimationFrame(25, 200, secondaryArm: false, flip: false)
					};
					int pantings = Game1.random.Next(5, 15);
					for (int j = 0; j < pantings; j++)
					{
						panting.Add(new FarmerSprite.AnimationFrame(24, 200, secondaryArm: false, flip: false, pantSound));
						panting.Add(new FarmerSprite.AnimationFrame(25, 200, secondaryArm: false, flip: false));
					}
					Sprite.setCurrentAnimation(panting);
					Sprite.loop = false;
				}
				else if (base.CurrentBehavior == 2)
				{
					List<FarmerSprite.AnimationFrame> pant = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(18, 200, secondaryArm: false, flip: false, pantSound),
						new FarmerSprite.AnimationFrame(19, 200)
					};
					int pants = Game1.random.Next(7, 20);
					for (int i = 0; i < pants; i++)
					{
						pant.Add(new FarmerSprite.AnimationFrame(18, 200, secondaryArm: false, flip: false, pantSound));
						pant.Add(new FarmerSprite.AnimationFrame(19, 200));
					}
					Sprite.setCurrentAnimation(pant);
					Sprite.loop = false;
				}
			}
		}

		public Dog(int xTile, int yTile, int breed)
		{
			base.Name = "Dog";
			base.displayName = name;
			whichBreed.Value = breed;
			Sprite = new AnimatedSprite(getPetTextureName(), 0, 32, 32);
			base.Position = new Vector2(xTile, yTile) * 64f;
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
			base.currentLocation = Game1.currentLocation;
			base.HideShadow = true;
		}

		public override string getPetTextureName()
		{
			return "Animals\\dog" + ((whichBreed.Value == 0) ? "" : string.Concat(whichBreed.Value));
		}

		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			sprintTimer = 0;
		}

		public override void RunState(GameTime time)
		{
			base.RunState(time);
			if (Sprite.CurrentAnimation == null)
			{
				wagging = false;
			}
			if (base.CurrentBehavior == 51)
			{
				if (Game1.IsMasterGame && sprintTimer > 0)
				{
					sprintTimer -= time.ElapsedGameTime.Milliseconds;
					base.speed = 6;
					tryToMoveInDirection(base.FacingDirection, isFarmer: false, -1, glider: false);
					if (sprintTimer <= 0)
					{
						base.speed = 2;
						base.CurrentBehavior = 0;
					}
				}
			}
			else if (base.CurrentBehavior == 55 || base.CurrentBehavior == 54)
			{
				if (Sprite.CurrentAnimation == null)
				{
					base.CurrentBehavior = 0;
				}
			}
			else if (base.CurrentBehavior == 1)
			{
				if (Game1.IsMasterGame && Game1.timeOfDay < 2000 && Game1.random.NextDouble() < 0.001)
				{
					base.CurrentBehavior = 0;
				}
				if (Game1.random.NextDouble() < 0.002)
				{
					doEmote(24);
				}
			}
			else if (base.CurrentBehavior == 50)
			{
				if (withinPlayerThreshold(2))
				{
					if (!wagging)
					{
						wag();
					}
				}
				else if (Sprite.CurrentFrame != 23 && Sprite.CurrentAnimation == null)
				{
					Sprite.CurrentFrame = 23;
				}
				else if (Sprite.CurrentFrame == 23 && Game1.IsMasterGame && Game1.random.NextDouble() < 0.01)
				{
					switch (Game1.random.Next(7))
					{
					case 0:
						base.CurrentBehavior = 55;
						break;
					case 1:
						petAnimationEvent.Fire("bark");
						break;
					case 2:
						petAnimationEvent.Fire("wag");
						break;
					case 3:
					case 4:
						petAnimationEvent.Fire("sit_animation");
						break;
					default:
						petAnimationEvent.Fire("pant");
						break;
					}
				}
			}
			else if (base.CurrentBehavior == 2)
			{
				if (Sprite.currentFrame != 18 && Sprite.CurrentAnimation == null)
				{
					Sprite.currentFrame = 18;
				}
				else if (Sprite.currentFrame == 18 && Game1.IsMasterGame && Game1.random.NextDouble() < 0.01)
				{
					switch (Game1.random.Next(4))
					{
					case 0:
					case 1:
						faceDirection(2);
						base.CurrentBehavior = 54;
						break;
					case 2:
						petAnimationEvent.Fire("pant");
						break;
					case 3:
						petAnimationEvent.Fire("close_eyes");
						break;
					}
				}
			}
			else
			{
				if (base.CurrentBehavior != 0 || !Game1.IsMasterGame || Sprite.CurrentAnimation != null || !(Game1.random.NextDouble() < 0.01))
				{
					return;
				}
				switch (Game1.random.Next(7 + ((base.currentLocation is Farm) ? 1 : 0)))
				{
				case 0:
				case 1:
				case 2:
				case 3:
					break;
				case 4:
				case 5:
					switch (base.FacingDirection)
					{
					case 2:
						faceDirection(2);
						base.CurrentBehavior = 2;
						break;
					case 0:
					case 1:
					case 3:
						if (base.FacingDirection == 0)
						{
							base.FacingDirection = ((!(Game1.random.NextDouble() < 0.5)) ? 1 : 3);
						}
						faceDirection(base.FacingDirection);
						base.CurrentBehavior = 50;
						break;
					}
					break;
				case 6:
				case 7:
					Halt();
					base.CurrentBehavior = 51;
					break;
				}
			}
		}

		public override void OnNewBehavior()
		{
			base.OnNewBehavior();
			sprintTimer = 0;
			switch (base.CurrentBehavior)
			{
			case 50:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(20, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: false, base.hold)
				});
				break;
			case 51:
				if (Game1.IsMasterGame)
				{
					faceDirection((!(Game1.random.NextDouble() < 0.5)) ? 1 : 3);
					sprintTimer = Game1.random.Next(1000, 3500);
				}
				if (Utility.isOnScreen(getTileLocationPoint(), 64, base.currentLocation))
				{
					Game1.playSound("dog_bark");
				}
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(32, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(33, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(34, 100, secondaryArm: false, flip: false, hitGround),
					new FarmerSprite.AnimationFrame(33, 100, secondaryArm: false, flip: false)
				});
				Sprite.loop = true;
				break;
			case 2:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(16, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(17, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(18, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(19, 100, secondaryArm: false, flip: false, base.hold)
				});
				break;
			case 54:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(17, 200),
					new FarmerSprite.AnimationFrame(16, 200),
					new FarmerSprite.AnimationFrame(0, 200)
				});
				Sprite.loop = false;
				break;
			case 55:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(20, 100, secondaryArm: false, flip: false, base.hold)
				});
				Sprite.loop = false;
				break;
			}
		}

		public void wag()
		{
			int delay = withinPlayerThreshold(2) ? 120 : 200;
			wagging = true;
			Sprite.loop = false;
			List<FarmerSprite.AnimationFrame> wag = new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(31, delay, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(23, delay, secondaryArm: false, flip: false, hitGround)
			};
			int wags = Game1.random.Next(2, 6);
			for (int i = 0; i < wags; i++)
			{
				wag.Add(new FarmerSprite.AnimationFrame(31, delay, secondaryArm: false, flip: false));
				wag.Add(new FarmerSprite.AnimationFrame(23, delay, secondaryArm: false, flip: false, hitGround));
			}
			wag.Add(new FarmerSprite.AnimationFrame(23, 2, secondaryArm: false, flip: false, doneWagging));
			Sprite.setCurrentAnimation(wag);
		}

		public void doneWagging(Farmer who)
		{
			wagging = false;
		}

		public void hitGround(Farmer who)
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 128, base.currentLocation))
			{
				base.currentLocation.playTerrainSound(getTileLocation(), this, showTerrainDisturbAnimation: false);
			}
		}

		public void pantSound(Farmer who)
		{
			if (withinPlayerThreshold(5))
			{
				base.currentLocation.localSound("dog_pant");
			}
		}

		public void thumpSound(Farmer who)
		{
			if (withinPlayerThreshold(4))
			{
				base.currentLocation.localSound("thudStep");
			}
		}

		public override void playContentSound()
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 128, base.currentLocation))
			{
				Game1.playSound("dog_pant");
				DelayedAction.playSoundAfterDelay("dog_pant", 400);
			}
		}
	}
}
