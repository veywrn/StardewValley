using Netcode;
using System.Xml.Serialization;

namespace StardewValley
{
	public class Warp : INetObject<NetFields>
	{
		[XmlElement("x")]
		private readonly NetInt x = new NetInt();

		[XmlElement("y")]
		private readonly NetInt y = new NetInt();

		[XmlElement("targetX")]
		private readonly NetInt targetX = new NetInt();

		[XmlElement("targetY")]
		private readonly NetInt targetY = new NetInt();

		[XmlElement("flipFarmer")]
		public readonly NetBool flipFarmer = new NetBool();

		[XmlElement("targetName")]
		private readonly NetString targetName = new NetString();

		[XmlElement("npcOnly")]
		public readonly NetBool npcOnly = new NetBool();

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public int X => x;

		public int Y => y;

		public int TargetX
		{
			get
			{
				return targetX;
			}
			set
			{
				targetX.Value = value;
			}
		}

		public int TargetY
		{
			get
			{
				return targetY;
			}
			set
			{
				targetY.Value = value;
			}
		}

		public string TargetName
		{
			get
			{
				return targetName;
			}
			set
			{
				targetName.Value = value;
			}
		}

		public Warp()
		{
			NetFields.AddFields(x, y, targetX, targetY, targetName, flipFarmer, npcOnly);
		}

		public Warp(int x, int y, string targetName, int targetX, int targetY, bool flipFarmer, bool npcOnly = false)
			: this()
		{
			this.x.Value = x;
			this.y.Value = y;
			this.targetX.Value = targetX;
			this.targetY.Value = targetY;
			this.targetName.Value = targetName;
			this.flipFarmer.Value = flipFarmer;
			this.npcOnly.Value = npcOnly;
		}
	}
}
