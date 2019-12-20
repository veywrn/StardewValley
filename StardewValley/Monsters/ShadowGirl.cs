using Microsoft.Xna.Framework;
using System;

namespace StardewValley.Monsters
{
	public class ShadowGirl : Monster
	{
		public const int blockTimeBeforePathfinding = 500;

		private new Vector2 lastPosition = Vector2.Zero;

		private int howLongOnThisPosition;

		public ShadowGirl()
		{
		}

		public ShadowGirl(Vector2 position)
			: base("Shadow Girl", position)
		{
			base.IsWalkingTowardPlayer = false;
			moveTowardPlayerThreshold.Value = 8;
			if (Game1.MasterPlayer.friendshipData.ContainsKey("???") && Game1.MasterPlayer.friendshipData["???"].Points >= 1250)
			{
				base.DamageToFarmer = 0;
			}
		}

		public override void reloadSprite()
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Girl");
		}

		public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
		{
			int actualDamage = Math.Max(1, damage - (int)resilience);
			if (Game1.random.NextDouble() < (double)missChance - (double)missChance * addedPrecision)
			{
				actualDamage = -1;
			}
			else
			{
				if (base.Player.CurrentTool.Name.Equals("Holy Sword") && !isBomb)
				{
					base.Health -= damage * 3 / 4;
					base.currentLocation.debris.Add(new Debris(string.Concat(damage * 3 / 4), 1, new Vector2(getStandingX(), getStandingY()), Color.LightBlue, 1f, 0f));
				}
				base.Health -= actualDamage;
				setTrajectory(xTrajectory, yTrajectory);
				if (base.Health <= 0)
				{
					deathAnimation();
				}
			}
			return actualDamage;
		}

		protected override void localDeathAnimation()
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10));
		}

		protected override void sharedDeathAnimation()
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 64, 21), 64, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X + 10, Sprite.SourceRect.Y + 21, 64, 21), 42, getStandingX(), getStandingY() - 32, 1, getStandingY() / 64, Color.White);
		}

		public override void update(GameTime time, GameLocation location)
		{
			if (!location.farmers.Any())
			{
				return;
			}
			if (!base.Player.isRafting || !withinPlayerThreshold(4))
			{
				updateGlow();
				updateEmote(time);
				if (controller == null)
				{
					updateMovement(location, time);
				}
				if (controller != null && controller.update(time))
				{
					controller = null;
				}
			}
			behaviorAtGameTick(time);
			if (base.Position.X < 0f || base.Position.X > (float)(location.map.GetLayer("Back").LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(location.map.GetLayer("Back").LayerHeight * 64))
			{
				location.characters.Remove(this);
			}
		}

		public override void behaviorAtGameTick(GameTime time)
		{
			base.behaviorAtGameTick(time);
			base.addedSpeed = 0;
			base.speed = 3;
			if (howLongOnThisPosition > 500 && controller == null)
			{
				base.IsWalkingTowardPlayer = false;
				controller = new PathFindController(this, base.currentLocation, new Point((int)base.Player.getTileLocation().X, (int)base.Player.getTileLocation().Y), Game1.random.Next(4), null, 300);
				timeBeforeAIMovementAgain = 2000f;
				howLongOnThisPosition = 0;
			}
			else if (controller == null)
			{
				base.IsWalkingTowardPlayer = true;
			}
			if (base.Position.Equals(lastPosition))
			{
				howLongOnThisPosition += time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				howLongOnThisPosition = 0;
			}
			lastPosition = base.Position;
		}
	}
}
