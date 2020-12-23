using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace StardewValley.Objects
{
	public class TankFish
	{
		public enum FishType
		{
			Normal,
			Eel,
			Cephalopod,
			Float,
			Ground,
			Crawl,
			Static
		}

		protected FishTankFurniture _tank;

		public Vector2 position;

		public float zPosition;

		public bool facingLeft;

		public Vector2 velocity = Vector2.Zero;

		protected Texture2D _texture;

		public float nextSwim;

		public int fishIndex;

		public int currentFrame;

		public int numberOfDarts;

		public FishType fishType;

		public float minimumVelocity;

		public float fishScale = 1f;

		public List<int> currentAnimation;

		public List<int> idleAnimation;

		public List<int> dartStartAnimation;

		public List<int> dartHoldAnimation;

		public List<int> dartEndAnimation;

		public int currentAnimationFrame;

		public float currentFrameTime;

		public float nextBubble;

		public TankFish(FishTankFurniture tank, Item item)
		{
			_tank = tank;
			_texture = _tank.GetAquariumTexture();
			string[] aquarium_fish_split = _tank.GetAquariumData()[item.ParentSheetIndex].Split('/');
			fishIndex = int.Parse(aquarium_fish_split[0]);
			currentFrame = fishIndex;
			zPosition = Utility.RandomFloat(4f, 10f);
			Dictionary<int, string> fish_sheet_data = Game1.content.Load<Dictionary<int, string>>("Data\\Fish");
			fishScale = 0.75f;
			if (fish_sheet_data.ContainsKey(item.ParentSheetIndex))
			{
				string[] fish_split = fish_sheet_data[item.ParentSheetIndex].Split('/');
				if (!(fish_split[1] == "trap"))
				{
					minimumVelocity = Utility.RandomFloat(0.25f, 0.35f);
					if (fish_split[2] == "smooth")
					{
						minimumVelocity = Utility.RandomFloat(0.5f, 0.6f);
					}
					if (fish_split[2] == "dart")
					{
						minimumVelocity = 0f;
					}
				}
			}
			if (aquarium_fish_split.Length > 1)
			{
				string fish_type_string = aquarium_fish_split[1];
				if (fish_type_string == "eel")
				{
					fishType = FishType.Eel;
					minimumVelocity = Utility.Clamp(fishScale, 0.3f, 0.4f);
				}
				else if (fish_type_string == "cephalopod")
				{
					fishType = FishType.Cephalopod;
					minimumVelocity = 0f;
				}
				else if (fish_type_string == "ground")
				{
					fishType = FishType.Ground;
					zPosition = 4f;
					minimumVelocity = 0f;
				}
				else if (fish_type_string == "static")
				{
					fishType = FishType.Static;
				}
				else if (fish_type_string == "crawl")
				{
					fishType = FishType.Crawl;
					minimumVelocity = 0f;
				}
				else if (fish_type_string == "front_crawl")
				{
					fishType = FishType.Crawl;
					zPosition = 3f;
					minimumVelocity = 0f;
				}
				else if (fish_type_string == "float")
				{
					fishType = FishType.Float;
				}
			}
			if (aquarium_fish_split.Length > 2)
			{
				string[] array = aquarium_fish_split[2].Split(' ');
				idleAnimation = new List<int>();
				string[] array2 = array;
				foreach (string frame4 in array2)
				{
					idleAnimation.Add(int.Parse(frame4));
				}
				SetAnimation(idleAnimation);
			}
			if (aquarium_fish_split.Length > 3)
			{
				string animation_string3 = aquarium_fish_split[3];
				string[] animation_split3 = animation_string3.Split(' ');
				dartStartAnimation = new List<int>();
				if (animation_string3 != "")
				{
					string[] array2 = animation_split3;
					foreach (string frame3 in array2)
					{
						dartStartAnimation.Add(int.Parse(frame3));
					}
				}
			}
			if (aquarium_fish_split.Length > 4)
			{
				string animation_string2 = aquarium_fish_split[4];
				string[] animation_split2 = animation_string2.Split(' ');
				dartHoldAnimation = new List<int>();
				if (animation_string2 != "")
				{
					string[] array2 = animation_split2;
					foreach (string frame2 in array2)
					{
						dartHoldAnimation.Add(int.Parse(frame2));
					}
				}
			}
			if (aquarium_fish_split.Length > 5)
			{
				string animation_string = aquarium_fish_split[5];
				string[] animation_split = animation_string.Split(' ');
				dartEndAnimation = new List<int>();
				if (animation_string != "")
				{
					string[] array2 = animation_split;
					foreach (string frame in array2)
					{
						dartEndAnimation.Add(int.Parse(frame));
					}
				}
			}
			Rectangle tank_bounds_local = _tank.GetTankBounds();
			tank_bounds_local.X = 0;
			tank_bounds_local.Y = 0;
			position = Vector2.Zero;
			position = Utility.getRandomPositionInThisRectangle(tank_bounds_local, Game1.random);
			nextSwim = Utility.RandomFloat(0.1f, 10f);
			nextBubble = Utility.RandomFloat(0.1f, 10f);
			facingLeft = (Game1.random.Next(2) == 1);
			if (facingLeft)
			{
				velocity = new Vector2(-1f, 0f);
			}
			else
			{
				velocity = new Vector2(1f, 0f);
			}
			velocity *= minimumVelocity;
			if (fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				position.Y = 0f;
			}
			ConstrainToTank();
		}

		public void SetAnimation(List<int> frames)
		{
			if (currentAnimation != frames)
			{
				currentAnimation = frames;
				currentAnimationFrame = 0;
				currentFrameTime = 0f;
				if (currentAnimation != null && currentAnimation.Count > 0)
				{
					currentFrame = frames[0];
				}
			}
		}

		public virtual void Draw(SpriteBatch b, float alpha, float draw_layer)
		{
			SpriteEffects sprite_effects = SpriteEffects.None;
			int draw_offset = -12;
			int slice_size = 8;
			if (fishType == FishType.Eel)
			{
				slice_size = 4;
			}
			int slice_offset = slice_size;
			if (facingLeft)
			{
				sprite_effects = SpriteEffects.FlipHorizontally;
				slice_offset *= -1;
				draw_offset = -draw_offset - slice_size + 1;
			}
			float bob = (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 1.25 + (double)(position.X / 32f)) * 2f;
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				bob = 0f;
			}
			float scale = GetScale();
			int cols = _texture.Width / 24;
			int sprite_sheet_x = currentFrame % cols * 24;
			int sprite_sheet_y = currentFrame / cols * 48;
			int wiggle_start_pixels = 10;
			float wiggle_amount = 1f;
			if (fishType == FishType.Eel)
			{
				wiggle_start_pixels = 20;
				bob *= 0f;
			}
			if (fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				float angle = 0f;
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
			}
			else if (fishType == FishType.Cephalopod || fishType == FishType.Float)
			{
				float angle2 = Utility.Clamp(velocity.X, -0.5f, 0.5f);
				b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(0f, bob) * 4f * scale), new Rectangle(sprite_sheet_x, sprite_sheet_y, 24, 24), Color.White * alpha, angle2, new Vector2(12f, 12f), 4f * scale, sprite_effects, draw_layer);
			}
			else
			{
				for (int slice = 0; slice < 24 / slice_size; slice++)
				{
					float multiplier3 = (float)(slice * slice_size) / (float)wiggle_start_pixels;
					multiplier3 = 1f - multiplier3;
					float velocity_multiplier2 = velocity.Length() / 1f;
					float time_multiplier = 1f;
					float position_multiplier = 0f;
					velocity_multiplier2 = Utility.Clamp(velocity_multiplier2, 0.2f, 1f);
					multiplier3 = Utility.Clamp(multiplier3, 0f, 1f);
					if (fishType == FishType.Eel)
					{
						multiplier3 = 1f;
						velocity_multiplier2 = 1f;
						time_multiplier = 0.1f;
						position_multiplier = 4f;
					}
					if (facingLeft)
					{
						position_multiplier *= -1f;
					}
					b.Draw(_texture, Game1.GlobalToLocal(GetWorldPosition() + new Vector2(draw_offset + slice * slice_offset, bob + (float)(Math.Sin((double)(slice * 20) + Game1.currentGameTime.TotalGameTime.TotalSeconds * 25.0 * (double)time_multiplier + (double)(position_multiplier * position.X / 16f)) * (double)wiggle_amount * (double)multiplier3 * (double)velocity_multiplier2)) * 4f * scale), new Rectangle(sprite_sheet_x + slice * slice_size, sprite_sheet_y, slice_size, 24), Color.White * alpha, 0f, new Vector2(0f, 12f), 4f * scale, sprite_effects, draw_layer);
				}
			}
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(new Vector2(GetWorldPosition().X, (float)_tank.GetTankBounds().Bottom - zPosition * 4f)), null, Color.White * alpha * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f * scale, 1f), SpriteEffects.None, _tank.GetFishSortRegion().X - 1E-07f);
		}

		public Vector2 GetWorldPosition()
		{
			return new Vector2((float)_tank.GetTankBounds().X + position.X, (float)_tank.GetTankBounds().Bottom - position.Y - zPosition * 4f);
		}

		public void ConstrainToTank()
		{
			Rectangle tank_bounds = _tank.GetTankBounds();
			Rectangle bounds3 = GetBounds();
			tank_bounds.X = 0;
			tank_bounds.Y = 0;
			if (bounds3.X < tank_bounds.X)
			{
				position.X += tank_bounds.X - bounds3.X;
				bounds3 = GetBounds();
			}
			if (bounds3.Y < tank_bounds.Y)
			{
				position.Y -= tank_bounds.Y - bounds3.Y;
				bounds3 = GetBounds();
			}
			if (bounds3.Right > tank_bounds.Right)
			{
				position.X += tank_bounds.Right - bounds3.Right;
				bounds3 = GetBounds();
			}
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				if (position.Y > (float)tank_bounds.Bottom)
				{
					position.Y -= (float)tank_bounds.Bottom - position.Y;
					bounds3 = GetBounds();
				}
			}
			else if (bounds3.Bottom > tank_bounds.Bottom)
			{
				position.Y -= tank_bounds.Bottom - bounds3.Bottom;
				bounds3 = GetBounds();
			}
		}

		public virtual float GetScale()
		{
			return fishScale;
		}

		public Rectangle GetBounds()
		{
			Vector2 dimensions = new Vector2(24f, 18f);
			dimensions *= 4f * GetScale();
			if (fishType == FishType.Crawl || fishType == FishType.Ground || fishType == FishType.Static)
			{
				return new Rectangle((int)(position.X - dimensions.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - dimensions.Y), (int)dimensions.X, (int)dimensions.Y);
			}
			return new Rectangle((int)(position.X - dimensions.X / 2f), (int)((float)_tank.GetTankBounds().Height - position.Y - dimensions.Y / 2f), (int)dimensions.X, (int)dimensions.Y);
		}

		public virtual void Update(GameTime time)
		{
			if (currentAnimation != null && currentAnimation.Count > 0)
			{
				currentFrameTime += (float)time.ElapsedGameTime.TotalSeconds;
				float seconds_per_frame = 0.125f;
				if (currentFrameTime > seconds_per_frame)
				{
					currentAnimationFrame += (int)(currentFrameTime / seconds_per_frame);
					currentFrameTime %= seconds_per_frame;
					if (currentAnimationFrame >= currentAnimation.Count)
					{
						if (currentAnimation == idleAnimation)
						{
							currentAnimationFrame %= currentAnimation.Count;
							currentFrame = currentAnimation[currentAnimationFrame];
						}
						else if (currentAnimation == dartStartAnimation)
						{
							if (dartHoldAnimation != null)
							{
								SetAnimation(dartHoldAnimation);
							}
							else
							{
								SetAnimation(idleAnimation);
							}
						}
						else if (currentAnimation == dartHoldAnimation)
						{
							currentAnimationFrame %= currentAnimation.Count;
							currentFrame = currentAnimation[currentAnimationFrame];
						}
						else if (currentAnimation == dartEndAnimation)
						{
							SetAnimation(idleAnimation);
						}
					}
					else
					{
						currentFrame = currentAnimation[currentAnimationFrame];
					}
				}
			}
			if (fishType != FishType.Static)
			{
				Rectangle local_tank_bounds = _tank.GetTankBounds();
				local_tank_bounds.X = 0;
				local_tank_bounds.Y = 0;
				float velocity_x = velocity.X;
				if (fishType == FishType.Crawl)
				{
					velocity_x = Utility.Clamp(velocity_x, -0.5f, 0.5f);
				}
				position.X += velocity_x;
				Rectangle bounds3 = GetBounds();
				if (bounds3.Left < local_tank_bounds.Left || bounds3.Right > local_tank_bounds.Right)
				{
					ConstrainToTank();
					bounds3 = GetBounds();
					velocity.X *= -1f;
					facingLeft = !facingLeft;
				}
				position.Y += velocity.Y;
				bounds3 = GetBounds();
				if (bounds3.Top < local_tank_bounds.Top || bounds3.Bottom > local_tank_bounds.Bottom)
				{
					ConstrainToTank();
					velocity.Y *= 0f;
				}
				float move_magnitude2 = velocity.Length();
				if (move_magnitude2 > minimumVelocity)
				{
					float deceleration = 0.015f;
					if (fishType == FishType.Crawl || fishType == FishType.Ground)
					{
						deceleration = 0.03f;
					}
					move_magnitude2 = Utility.Lerp(move_magnitude2, minimumVelocity, deceleration);
					if (move_magnitude2 < 0.0001f)
					{
						move_magnitude2 = 0f;
					}
					velocity.Normalize();
					velocity *= move_magnitude2;
					if (currentAnimation == dartHoldAnimation && move_magnitude2 <= minimumVelocity + 0.5f)
					{
						if (dartEndAnimation != null && dartEndAnimation.Count > 0)
						{
							SetAnimation(dartEndAnimation);
						}
						else if (idleAnimation != null && idleAnimation.Count > 0)
						{
							SetAnimation(idleAnimation);
						}
					}
				}
				nextSwim -= (float)time.ElapsedGameTime.TotalSeconds;
				if (nextSwim <= 0f)
				{
					if (numberOfDarts == 0)
					{
						numberOfDarts = Game1.random.Next(1, 4);
						nextSwim = Utility.RandomFloat(6f, 12f);
						if (fishType == FishType.Cephalopod)
						{
							nextSwim = Utility.RandomFloat(2f, 5f);
						}
						if (Game1.random.NextDouble() < 0.30000001192092896)
						{
							facingLeft = !facingLeft;
						}
					}
					else
					{
						nextSwim = Utility.RandomFloat(0.1f, 0.5f);
						numberOfDarts--;
						if (Game1.random.NextDouble() < 0.05000000074505806)
						{
							facingLeft = !facingLeft;
						}
					}
					if (dartStartAnimation != null && dartStartAnimation.Count > 0)
					{
						SetAnimation(dartStartAnimation);
					}
					else if (dartHoldAnimation != null && dartHoldAnimation.Count > 0)
					{
						SetAnimation(dartHoldAnimation);
					}
					velocity.X = 1.5f;
					if (_tank.getTilesWide() <= 2)
					{
						velocity.X *= 0.5f;
					}
					if (facingLeft)
					{
						velocity.X *= -1f;
					}
					if (fishType == FishType.Cephalopod)
					{
						velocity.Y = Utility.RandomFloat(0.5f, 0.75f);
					}
					else if (fishType == FishType.Ground)
					{
						velocity.X *= 0.5f;
						velocity.Y = Utility.RandomFloat(0.5f, 0.25f);
					}
					else
					{
						velocity.Y = Utility.RandomFloat(-0.5f, 0.5f);
					}
					if (fishType == FishType.Crawl)
					{
						velocity.Y = 0f;
					}
				}
			}
			if (fishType == FishType.Cephalopod || fishType == FishType.Ground || fishType == FishType.Crawl || fishType == FishType.Static)
			{
				float fall_speed = 0.2f;
				if (fishType == FishType.Static)
				{
					fall_speed = 0.6f;
				}
				if (position.Y > 0f)
				{
					position.Y -= fall_speed;
				}
			}
			nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
			if (nextBubble <= 0f)
			{
				nextBubble = Utility.RandomFloat(1f, 10f);
				float x_offset2 = 0f;
				if (fishType == FishType.Ground || fishType == FishType.Normal || fishType == FishType.Eel)
				{
					x_offset2 = 32f;
				}
				if (facingLeft)
				{
					x_offset2 *= -1f;
				}
				x_offset2 *= fishScale;
				_tank.bubbles.Add(new Vector4(position.X + x_offset2, position.Y + zPosition, zPosition, 0.25f));
			}
			ConstrainToTank();
		}
	}
}
