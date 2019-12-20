using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley
{
	public class LightSource : INetObject<NetFields>
	{
		public enum LightContext
		{
			None,
			MapLight,
			WindowLight
		}

		public const int lantern = 1;

		public const int windowLight = 2;

		public const int sconceLight = 4;

		public const int cauldronLight = 5;

		public const int indoorWindowLight = 6;

		public const int projectorLight = 7;

		public const int maxLightsOnScreenBeforeReduction = 8;

		public const float reductionPerExtraLightSource = 0.03f;

		public const int playerLantern = -85736;

		public readonly NetInt textureIndex = new NetInt().Interpolated(interpolate: false, wait: false);

		public Texture2D lightTexture;

		public readonly NetVector2 position = new NetVector2().Interpolated(interpolate: true, wait: true);

		public readonly NetColor color = new NetColor();

		public readonly NetFloat radius = new NetFloat();

		public readonly NetInt identifier = new NetInt();

		public readonly NetEnum<LightContext> lightContext = new NetEnum<LightContext>();

		public readonly NetLong playerID = new NetLong(0L).Interpolated(interpolate: false, wait: false);

		public int Identifier
		{
			get
			{
				return identifier.Value;
			}
			set
			{
				identifier.Value = value;
			}
		}

		public long PlayerID
		{
			get
			{
				return playerID.Value;
			}
			set
			{
				playerID.Value = value;
			}
		}

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public LightSource()
		{
			NetFields.AddFields(textureIndex, position, color, radius, identifier, lightContext, playerID);
			textureIndex.fieldChangeEvent += delegate(NetInt field, int oldValue, int newValue)
			{
				loadTextureFromConstantValue(newValue);
			};
		}

		public LightSource(int textureIndex, Vector2 position, float radius, Color color, LightContext light_context = LightContext.None, long playerID = 0L)
			: this()
		{
			this.textureIndex.Value = textureIndex;
			this.position.Value = position;
			this.radius.Value = radius;
			this.color.Value = color;
			lightContext.Value = light_context;
			this.playerID.Value = playerID;
		}

		public LightSource(int textureIndex, Vector2 position, float radius, Color color, int identifier, LightContext light_context = LightContext.None, long playerID = 0L)
			: this()
		{
			this.textureIndex.Value = textureIndex;
			this.position.Value = position;
			this.radius.Value = radius;
			this.color.Value = color;
			this.identifier.Value = identifier;
			lightContext.Value = light_context;
			this.playerID.Value = playerID;
		}

		public LightSource(int textureIndex, Vector2 position, float radius, LightContext light_context = LightContext.None, long playerID = 0L)
			: this()
		{
			this.textureIndex.Value = textureIndex;
			this.position.Value = position;
			this.radius.Value = radius;
			color.Value = Color.Black;
			lightContext.Value = light_context;
			this.playerID.Value = playerID;
		}

		private void loadTextureFromConstantValue(int value)
		{
			switch (value)
			{
			case 3:
				break;
			case 1:
				lightTexture = Game1.lantern;
				break;
			case 2:
				lightTexture = Game1.windowLight;
				break;
			case 4:
				lightTexture = Game1.sconceLight;
				break;
			case 5:
				lightTexture = Game1.cauldronLight;
				break;
			case 6:
				lightTexture = Game1.indoorWindowLight;
				break;
			case 7:
				lightTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Lighting\\projectorLight");
				break;
			}
		}

		public LightSource Clone()
		{
			return new LightSource(textureIndex, position, radius, color, identifier, lightContext.Value, playerID.Value);
		}
	}
}
