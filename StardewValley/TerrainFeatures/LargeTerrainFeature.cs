using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System.Xml.Serialization;

namespace StardewValley.TerrainFeatures
{
	[XmlInclude(typeof(Bush))]
	public abstract class LargeTerrainFeature : TerrainFeature
	{
		[XmlElement("tilePosition")]
		public readonly NetVector2 tilePosition = new NetVector2();

		protected LargeTerrainFeature(bool needsTick)
			: base(needsTick)
		{
			base.NetFields.AddField(tilePosition);
		}

		public Rectangle getBoundingBox()
		{
			return getBoundingBox(tilePosition);
		}

		public void dayUpdate(GameLocation l)
		{
			dayUpdate(l, tilePosition);
		}

		public bool tickUpdate(GameTime time, GameLocation location)
		{
			return tickUpdate(time, tilePosition, location);
		}

		public void draw(SpriteBatch b)
		{
			draw(b, tilePosition);
		}
	}
}
