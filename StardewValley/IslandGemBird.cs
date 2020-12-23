using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	public class IslandGemBird : INetObject<NetFields>
	{
		public enum GemBirdType
		{
			Emerald,
			Aquamarine,
			Ruby,
			Amethyst,
			Topaz,
			MAX
		}

		[XmlIgnore]
		public Texture2D texture;

		[XmlElement("position")]
		public NetVector2 position = new NetVector2();

		[XmlIgnore]
		protected float _destroyTimer;

		[XmlElement("height")]
		public NetFloat height = new NetFloat();

		[XmlIgnore]
		public int[] idleAnimation = new int[1];

		[XmlIgnore]
		public int[] lookBackAnimation = new int[17]
		{
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1,
			1
		};

		[XmlIgnore]
		public int[] scratchAnimation = new int[19]
		{
			0,
			1,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2,
			3,
			2
		};

		[XmlIgnore]
		public int[] flyAnimation = new int[11]
		{
			4,
			5,
			6,
			7,
			7,
			6,
			6,
			5,
			5,
			4,
			4
		};

		[XmlIgnore]
		public int[] currentAnimation;

		[XmlIgnore]
		public float frameTimer;

		[XmlIgnore]
		public int currentFrameIndex;

		[XmlIgnore]
		public float idleAnimationTime;

		[XmlElement("alpha")]
		public NetFloat alpha = new NetFloat(1f);

		[XmlElement("flying")]
		public NetBool flying = new NetBool();

		[XmlElement("color")]
		public NetColor color = new NetColor();

		[XmlElement("itemIndex")]
		public NetInt itemIndex = new NetInt();

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public IslandGemBird()
		{
			texture = Game1.content.Load<Texture2D>("LooseSprites\\GemBird");
			InitNetFields();
		}

		public IslandGemBird(Vector2 tile_position, GemBirdType bird_type)
			: this()
		{
			position.Value = (tile_position + new Vector2(0.5f, 0.5f)) * 64f;
			color.Value = GetColor(bird_type);
			itemIndex.Value = GetItemIndex(bird_type);
		}

		public static Color GetColor(GemBirdType bird_type)
		{
			switch (bird_type)
			{
			case GemBirdType.Emerald:
				return new Color(67, 255, 83);
			case GemBirdType.Aquamarine:
				return new Color(74, 243, 255);
			case GemBirdType.Ruby:
				return new Color(255, 38, 38);
			case GemBirdType.Amethyst:
				return new Color(255, 67, 251);
			case GemBirdType.Topaz:
				return new Color(255, 156, 33);
			default:
				return Color.White;
			}
		}

		public static int GetItemIndex(GemBirdType bird_type)
		{
			switch (bird_type)
			{
			case GemBirdType.Emerald:
				return 60;
			case GemBirdType.Aquamarine:
				return 62;
			case GemBirdType.Ruby:
				return 64;
			case GemBirdType.Amethyst:
				return 66;
			case GemBirdType.Topaz:
				return 68;
			default:
				return 0;
			}
		}

		public static GemBirdType GetBirdTypeForLocation(string location)
		{
			List<string> island_locations = new List<string>();
			island_locations.Add("IslandNorth");
			island_locations.Add("IslandSouth");
			island_locations.Add("IslandEast");
			island_locations.Add("IslandWest");
			if (!island_locations.Contains(location))
			{
				return GemBirdType.Aquamarine;
			}
			Random r = new Random((int)Game1.uniqueIDForThisGame);
			List<GemBirdType> types = new List<GemBirdType>();
			for (int i = 0; i < 5; i++)
			{
				types.Add((GemBirdType)i);
			}
			Utility.Shuffle(r, types);
			return types[island_locations.IndexOf(location)];
		}

		public void Draw(SpriteBatch b)
		{
			if (currentAnimation != null)
			{
				int frame = currentAnimation[Math.Min(currentFrameIndex, currentAnimation.Length - 1)];
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position.Value + new Vector2(0f, 0f - height.Value)), new Rectangle(frame * 32, 0, 32, 32), Color.White * alpha, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, (position.Value.Y - 1f) / 10000f);
				b.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position.Value + new Vector2(0f, 0f - height.Value)), new Rectangle(frame * 32, 32, 32, 32), color.Value * alpha, 0f, new Vector2(16f, 32f), 4f, SpriteEffects.None, position.Value.Y / 10000f);
				b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position.Value), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (position.Y - 2f) / 10000f);
			}
		}

		public void InitNetFields()
		{
			NetFields.AddFields(position, flying, height, color, alpha, itemIndex);
			position.Interpolated(interpolate: true, wait: true);
			height.Interpolated(interpolate: true, wait: true);
			alpha.Interpolated(interpolate: true, wait: true);
		}

		public bool Update(GameTime time, GameLocation location)
		{
			if (currentAnimation == null)
			{
				currentAnimation = idleAnimation;
			}
			frameTimer += (float)time.ElapsedGameTime.TotalSeconds;
			float frame_time = 0.15f;
			if ((bool)flying)
			{
				frame_time = 0.05f;
			}
			if (frameTimer >= frame_time)
			{
				frameTimer = 0f;
				currentFrameIndex++;
				if (currentFrameIndex >= currentAnimation.Length)
				{
					currentFrameIndex = 0;
					if (currentAnimation == flyAnimation && location == Game1.currentLocation && Utility.isOnScreen(position.Value + new Vector2(0f, 0f - height.Value), 64))
					{
						Game1.playSound("batFlap");
					}
					if (currentAnimation == lookBackAnimation || currentAnimation == scratchAnimation)
					{
						currentAnimation = idleAnimation;
					}
				}
			}
			if (flying.Value)
			{
				currentAnimation = flyAnimation;
				if (Game1.IsMasterGame)
				{
					height.Value += 4f;
					position.X -= 3f;
					if (alpha.Value > 0f && (float)height >= 300f)
					{
						alpha.Value -= 0.01f;
						if (alpha.Value < 0f)
						{
							alpha.Value = 0f;
						}
					}
				}
			}
			else
			{
				if (currentAnimation == idleAnimation)
				{
					idleAnimationTime -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				if (idleAnimationTime <= 0f)
				{
					currentFrameIndex = 0;
					if (Game1.random.NextDouble() < 0.75)
					{
						currentAnimation = lookBackAnimation;
					}
					else
					{
						currentAnimation = scratchAnimation;
					}
					idleAnimationTime = Utility.RandomFloat(1f, 3f);
				}
			}
			if (Game1.IsMasterGame && !flying.Value)
			{
				foreach (Farmer farmer in location.farmers)
				{
					Vector2 offset = farmer.Position - position.Value;
					if (Math.Abs(offset.X) <= 128f && Math.Abs(offset.Y) <= 128f)
					{
						flying.Value = true;
						location.playSound("parrot");
						Game1.createObjectDebris(itemIndex.Value, (int)(position.X / 64f), (int)(position.Y / 64f), location);
					}
				}
			}
			if (alpha.Value <= 0f)
			{
				if (_destroyTimer == 0f)
				{
					_destroyTimer = 3f;
				}
				else if (_destroyTimer >= 0f)
				{
					_destroyTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (_destroyTimer <= 0f)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
