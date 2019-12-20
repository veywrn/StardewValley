using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Network;
using System;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Chunk : INetObject<NetFields>
	{
		[XmlElement("position")]
		public NetPosition position = new NetPosition();

		[XmlIgnore]
		public readonly NetFloat xVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

		[XmlIgnore]
		public readonly NetFloat yVelocity = new NetFloat().Interpolated(interpolate: true, wait: true);

		[XmlIgnore]
		public readonly NetBool hasPassedRestingLineOnce = new NetBool(value: false);

		[XmlIgnore]
		public int bounces;

		private readonly NetInt netDebrisType = new NetInt();

		[XmlIgnore]
		public bool hitWall;

		[XmlElement("xSpriteSheet")]
		public readonly NetInt xSpriteSheet = new NetInt();

		[XmlElement("ySpriteSheet")]
		public readonly NetInt ySpriteSheet = new NetInt();

		[XmlIgnore]
		public float rotation;

		[XmlIgnore]
		public float rotationVelocity;

		private readonly NetFloat netScale = new NetFloat().Interpolated(interpolate: true, wait: true);

		private readonly NetFloat netAlpha = new NetFloat();

		public int debrisType
		{
			get
			{
				return netDebrisType;
			}
			set
			{
				netDebrisType.Value = value;
			}
		}

		public float scale
		{
			get
			{
				return netScale;
			}
			set
			{
				netScale.Value = value;
			}
		}

		public float alpha
		{
			get
			{
				return netAlpha;
			}
			set
			{
				netAlpha.Value = value;
			}
		}

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public Chunk()
		{
			NetFields.AddFields(position.NetFields, xVelocity, yVelocity, netDebrisType, xSpriteSheet, ySpriteSheet, netScale, netAlpha, hasPassedRestingLineOnce);
			NetFields.DeltaAggregateTicks = 30;
		}

		public Chunk(Vector2 position, float xVelocity, float yVelocity, int debrisType)
			: this()
		{
			this.position.Value = position;
			this.xVelocity.Value = xVelocity;
			this.yVelocity.Value = yVelocity;
			this.debrisType = debrisType;
			alpha = 1f;
		}

		public float getSpeed()
		{
			return (float)Math.Sqrt((float)xVelocity * (float)xVelocity + (float)yVelocity * (float)yVelocity);
		}
	}
}
