using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Tools
{
	public class Pan : Tool
	{
		[XmlIgnore]
		private readonly NetEvent0 finishEvent = new NetEvent0();

		public Pan()
			: base("Copper Pan", -1, 12, 12, stackable: false)
		{
		}

		public override Item getOne()
		{
			return new Pan();
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pan.cs.14180");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pan.cs.14181");
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(finishEvent);
			finishEvent.onEvent += doFinish;
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			bool overrideCheck = false;
			Rectangle orePanRect = new Rectangle(location.orePanPoint.X * 64 - 64, location.orePanPoint.Y * 64 - 64, 256, 256);
			if (orePanRect.Contains(x, y) && Utility.distance(who.getStandingX(), orePanRect.Center.X, who.getStandingY(), orePanRect.Center.Y) <= 192f)
			{
				overrideCheck = true;
			}
			who.lastClick = Vector2.Zero;
			x = (int)who.GetToolLocation().X;
			y = (int)who.GetToolLocation().Y;
			who.lastClick = new Vector2(x, y);
			if (location.orePanPoint != null && !location.orePanPoint.Equals(Point.Zero))
			{
				Rectangle panRect = who.GetBoundingBox();
				if (overrideCheck || panRect.Intersects(orePanRect))
				{
					who.faceDirection(2);
					who.FarmerSprite.animateOnce(303, 50f, 4);
					return true;
				}
			}
			who.forceCanMove();
			return true;
		}

		public static void playSlosh(Farmer who)
		{
			who.currentLocation.localSound("slosh");
		}

		public override void tickUpdate(GameTime time, Farmer who)
		{
			lastUser = who;
			base.tickUpdate(time, who);
			finishEvent.Poll();
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			x = (int)who.GetToolLocation().X;
			y = (int)who.GetToolLocation().Y;
			base.CurrentParentTileIndex = 12;
			base.IndexOfMenuItemView = 12;
			location.localSound("coin");
			who.addItemsByMenuIfNecessary(getPanItems(location, who));
			location.orePanPoint.Value = Point.Zero;
			finish();
		}

		private void finish()
		{
			finishEvent.Fire();
		}

		private void doFinish()
		{
			lastUser.CanMove = true;
			lastUser.UsingTool = false;
			lastUser.canReleaseTool = true;
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.IndexOfMenuItemView = 12;
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public List<Item> getPanItems(GameLocation location, Farmer who)
		{
			List<Item> items = new List<Item>();
			int whichOre = 378;
			int whichExtra = -1;
			Random r = new Random(location.orePanPoint.X + location.orePanPoint.Y * 1000 + (int)Game1.stats.DaysPlayed);
			double roll3 = r.NextDouble() - (double)(int)who.luckLevel * 0.001 - who.DailyLuck;
			if (roll3 < 0.01)
			{
				whichOre = 386;
			}
			else if (roll3 < 0.241)
			{
				whichOre = 384;
			}
			else if (roll3 < 0.6)
			{
				whichOre = 380;
			}
			int orePieces = r.Next(5) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f) + who.DailyLuck) * 2.0);
			int extraPieces = r.Next(5) + 1 + (int)((r.NextDouble() + 0.1 + (double)((float)(int)who.luckLevel / 10f)) * 2.0);
			roll3 = r.NextDouble() - who.DailyLuck;
			if (roll3 < 0.4 + (double)who.LuckLevel * 0.04)
			{
				roll3 = r.NextDouble() - who.DailyLuck;
				whichExtra = 382;
				if (roll3 < 0.02 + (double)who.LuckLevel * 0.002)
				{
					whichExtra = 72;
					extraPieces = 1;
				}
				else if (roll3 < 0.1)
				{
					whichExtra = 60 + r.Next(5) * 2;
					extraPieces = 1;
				}
				else if (roll3 < 0.36)
				{
					whichExtra = 749;
					extraPieces = Math.Max(1, extraPieces / 2);
				}
				else if (roll3 < 0.5)
				{
					whichExtra = ((r.NextDouble() < 0.3) ? 82 : ((r.NextDouble() < 0.5) ? 84 : 86));
					extraPieces = 1;
				}
			}
			items.Add(new Object(whichOre, orePieces));
			if (whichExtra != -1)
			{
				items.Add(new Object(whichExtra, extraPieces));
			}
			return items;
		}
	}
}
