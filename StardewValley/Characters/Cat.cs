using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace StardewValley.Characters
{
	public class Cat : Pet
	{
		public const int behavior_StandUp = 54;

		public const int behavior_Flop = 55;

		public const int behavior_Leap = 56;

		public Cat()
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
			if (animation_event == "blink")
			{
				bool blink = Game1.random.NextDouble() < 0.45;
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(19, blink ? 200 : Game1.random.Next(1000, 9000)),
					new FarmerSprite.AnimationFrame(18, 1, secondaryArm: false, flip: false, base.hold)
				});
				Sprite.loop = false;
				if (blink && Game1.random.NextDouble() < 0.2)
				{
					playContentSound();
					shake(200);
				}
			}
			else if (animation_event == "lick")
			{
				List<FarmerSprite.AnimationFrame> licks = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(19, 300),
					new FarmerSprite.AnimationFrame(20, 200),
					new FarmerSprite.AnimationFrame(21, 200),
					new FarmerSprite.AnimationFrame(22, 200, secondaryArm: false, flip: false, lickSound),
					new FarmerSprite.AnimationFrame(23, 200)
				};
				int extraLicks = Game1.random.Next(1, 6);
				for (int i = 0; i < extraLicks; i++)
				{
					licks.Add(new FarmerSprite.AnimationFrame(21, 150));
					licks.Add(new FarmerSprite.AnimationFrame(22, 150, secondaryArm: false, flip: false, lickSound));
					licks.Add(new FarmerSprite.AnimationFrame(23, 150));
				}
				licks.Add(new FarmerSprite.AnimationFrame(18, 1, secondaryArm: false, flip: false, base.hold));
				Sprite.loop = false;
				Sprite.setCurrentAnimation(licks);
			}
		}

		public Cat(int xTile, int yTile, int breed)
		{
			base.Name = "Cat";
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
			return "Animals\\cat" + ((whichBreed.Value == 0) ? "" : string.Concat(whichBreed.Value));
		}

		public override void OnNewBehavior()
		{
			base.OnNewBehavior();
			switch (base.CurrentBehavior)
			{
			case 54:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(17, 200),
					new FarmerSprite.AnimationFrame(16, 200),
					new FarmerSprite.AnimationFrame(0, 200)
				});
				Sprite.loop = false;
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
			case 55:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(24, 100),
					new FarmerSprite.AnimationFrame(25, 100),
					new FarmerSprite.AnimationFrame(26, 100),
					new FarmerSprite.AnimationFrame(27, Game1.random.Next(8000, 30000), secondaryArm: false, flip: false, flopSound)
				});
				Sprite.loop = false;
				break;
			case 56:
				Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(30, 300),
					new FarmerSprite.AnimationFrame(31, 300),
					new FarmerSprite.AnimationFrame(30, 300),
					new FarmerSprite.AnimationFrame(31, 300),
					new FarmerSprite.AnimationFrame(30, 300),
					new FarmerSprite.AnimationFrame(31, 500),
					new FarmerSprite.AnimationFrame(24, 800, secondaryArm: false, flip: false, leap),
					new FarmerSprite.AnimationFrame(4, 1)
				});
				Sprite.loop = false;
				break;
			}
		}

		public override void RunState(GameTime time)
		{
			base.RunState(time);
			if (base.CurrentBehavior == 1)
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
			else if (base.CurrentBehavior == 2)
			{
				if (Sprite.currentFrame != 18 && Sprite.CurrentAnimation == null)
				{
					Sprite.currentFrame = 18;
				}
				else if (Sprite.currentFrame == 18 && Game1.IsMasterGame && Game1.random.NextDouble() < 0.01)
				{
					switch (Game1.random.Next(6))
					{
					case 0:
					case 1:
						faceDirection(2);
						base.CurrentBehavior = 54;
						break;
					case 2:
					case 3:
						petAnimationEvent.Fire("lick");
						break;
					default:
						petAnimationEvent.Fire("blink");
						break;
					}
				}
			}
			if (base.CurrentBehavior == 54)
			{
				if (Sprite.CurrentAnimation == null)
				{
					base.CurrentBehavior = 0;
				}
			}
			else if (base.CurrentBehavior == 0)
			{
				if (!Game1.IsMasterGame || Sprite.CurrentAnimation != null || !(Game1.random.NextDouble() < 0.01))
				{
					return;
				}
				int num = Game1.random.Next(4);
				if ((uint)num <= 2u || num != 3)
				{
					return;
				}
				switch (base.FacingDirection)
				{
				case 0:
				case 2:
					faceDirection(2);
					base.CurrentBehavior = 2;
					break;
				case 1:
					if (Game1.random.NextDouble() < 0.85)
					{
						base.CurrentBehavior = 55;
					}
					else
					{
						base.CurrentBehavior = 56;
					}
					break;
				case 3:
					if (Game1.random.NextDouble() < 0.85)
					{
						base.CurrentBehavior = 55;
					}
					else
					{
						base.CurrentBehavior = 56;
					}
					break;
				}
			}
			else
			{
				if ((base.CurrentBehavior != 55 && base.CurrentBehavior != 56) || !Game1.IsMasterGame)
				{
					return;
				}
				if (base.CurrentBehavior == 56 && yJumpOffset != 0)
				{
					if (base.FacingDirection == 1)
					{
						xVelocity = 4f;
					}
					else if (base.FacingDirection == 3)
					{
						xVelocity = -4f;
					}
					MovePosition(time, Game1.viewport, base.currentLocation);
				}
				if (Sprite.CurrentAnimation == null)
				{
					base.CurrentBehavior = 0;
				}
			}
		}

		public void lickSound(Farmer who)
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 128, base.currentLocation))
			{
				Game1.playSound("Cowboy_Footstep");
			}
		}

		public void leap(Farmer who)
		{
			if (base.currentLocation.Equals(Game1.currentLocation))
			{
				jump();
			}
		}

		public void flopSound(Farmer who)
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 128, base.currentLocation))
			{
				Game1.playSound("thudStep");
			}
			if (!Game1.IsMasterGame)
			{
				hold(who);
			}
		}

		public override void playContentSound()
		{
			if (Utility.isOnScreen(getTileLocationPoint(), 128, base.currentLocation))
			{
				Game1.playSound("cat");
			}
		}
	}
}
