using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class SwitchFloor : Object
	{
		public static Color successColor = Color.LightBlue;

		[XmlElement("onColor")]
		public readonly NetColor onColor = new NetColor();

		[XmlElement("offColor")]
		public readonly NetColor offColor = new NetColor();

		[XmlElement("readyToflip")]
		private readonly NetBool readyToflip = new NetBool(value: false);

		[XmlElement("finished")]
		public readonly NetBool finished = new NetBool(value: false);

		private int ticksToSuccess = -1;

		[XmlElement("glow")]
		private readonly NetFloat glow = new NetFloat(0f);

		public SwitchFloor()
		{
			base.NetFields.AddFields(onColor, offColor, readyToflip, finished, glow);
		}

		public SwitchFloor(Vector2 tileLocation, Color onColor, Color offColor, bool on)
			: this()
		{
			base.tileLocation.Value = tileLocation;
			this.onColor.Value = onColor;
			this.offColor.Value = offColor;
			isOn.Value = on;
			fragility.Value = 2;
			base.name = "Switch Floor";
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:SwitchFloor.cs.13097");
		}

		public void flip(GameLocation environment)
		{
			isOn.Value = !isOn;
			glow.Value = 0.65f;
			foreach (Vector2 v in Utility.getAdjacentTileLocations(tileLocation))
			{
				if (environment.objects.ContainsKey(v) && environment.objects[v] is SwitchFloor)
				{
					environment.objects[v].isOn.Value = !environment.objects[v].isOn;
					(environment.objects[v] as SwitchFloor).glow.Value = 0.3f;
				}
			}
			Game1.playSound("shiny4");
		}

		public void setSuccessCountdown(int ticks)
		{
			ticksToSuccess = ticks;
			glow.Value = 0.5f;
		}

		public void checkForCompleteness()
		{
			Queue<Vector2> openList = new Queue<Vector2>();
			HashSet<Vector2> closedList = new HashSet<Vector2>();
			openList.Enqueue(tileLocation);
			Vector2 current2 = default(Vector2);
			List<Vector2> adjacent2 = new List<Vector2>();
			while (openList.Count > 0)
			{
				current2 = openList.Dequeue();
				if (Game1.currentLocation.objects.ContainsKey(current2) && Game1.currentLocation.objects[current2] is SwitchFloor && (Game1.currentLocation.objects[current2] as SwitchFloor).isOn != isOn)
				{
					return;
				}
				closedList.Add(current2);
				adjacent2 = Utility.getAdjacentTileLocations(current2);
				for (int i = 0; i < adjacent2.Count; i++)
				{
					if (!closedList.Contains(adjacent2[i]) && Game1.currentLocation.objects.ContainsKey(current2) && Game1.currentLocation.objects[current2] is SwitchFloor)
					{
						openList.Enqueue(adjacent2[i]);
					}
				}
				adjacent2.Clear();
			}
			int successTicks = 5;
			foreach (Vector2 v in closedList)
			{
				if (Game1.currentLocation.objects.ContainsKey(v) && Game1.currentLocation.objects[v] is SwitchFloor)
				{
					(Game1.currentLocation.objects[v] as SwitchFloor).setSuccessCountdown(successTicks);
				}
				successTicks += 2;
			}
			int coins = (int)Math.Sqrt(closedList.Count) * 2;
			Vector2 treasurePosition = closedList.Last();
			while (Game1.currentLocation.isTileOccupiedByFarmer(treasurePosition) != null)
			{
				closedList.Remove(treasurePosition);
				if (closedList.Count > 0)
				{
					treasurePosition = closedList.Last();
				}
			}
			Game1.currentLocation.objects[treasurePosition] = new Chest(coins, null, treasurePosition);
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, treasurePosition * 64f, flicker: false, flipped: false));
			Game1.playSound("coin");
		}

		public override bool isPassable()
		{
			return true;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			spriteBatch.Draw(Flooring.floorsTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), new Rectangle(0, 1280, 64, 64), finished ? successColor : ((Color)(isOn ? onColor : offColor)), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
			if ((float)glow > 0f)
			{
				spriteBatch.Draw(Flooring.floorsTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), new Rectangle(0, 1280, 64, 64), Color.White * glow, 0f, Vector2.Zero, 4f, SpriteEffects.None, 2E-08f);
			}
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if ((float)glow > 0f)
			{
				glow.Value -= 0.04f;
			}
			if (ticksToSuccess > 0)
			{
				ticksToSuccess--;
				if (ticksToSuccess == 0)
				{
					finished.Value = true;
					glow.Value += 0.2f;
					Game1.playSound("boulderCrack");
				}
			}
			else if (!finished)
			{
				foreach (Farmer farmer in Game1.currentLocation.farmers)
				{
					if (farmer.getTileLocation().Equals(tileLocation))
					{
						if ((bool)readyToflip)
						{
							flip(Game1.currentLocation);
							checkForCompleteness();
						}
						readyToflip.Value = false;
						return;
					}
				}
				readyToflip.Value = true;
			}
		}
	}
}
