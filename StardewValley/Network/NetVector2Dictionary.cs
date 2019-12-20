using Microsoft.Xna.Framework;
using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley.Network
{
	public sealed class NetVector2Dictionary<T, TField> : NetFieldDictionary<Vector2, T, TField, SerializableDictionary<Vector2, T>, NetVector2Dictionary<T, TField>> where TField : NetField<T, TField>, new()
	{
		public NetVector2Dictionary()
		{
		}

		public NetVector2Dictionary(IEnumerable<KeyValuePair<Vector2, T>> dict)
			: base(dict)
		{
		}

		protected override Vector2 ReadKey(BinaryReader reader)
		{
			float x = reader.ReadSingle();
			float y = reader.ReadSingle();
			return new Vector2(x, y);
		}

		protected override void WriteKey(BinaryWriter writer, Vector2 key)
		{
			writer.Write(key.X);
			writer.Write(key.Y);
		}
	}
}
