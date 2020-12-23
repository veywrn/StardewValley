using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace StardewValley.Locations
{
	public class DwarfGate : INetObject<NetFields>
	{
		public NetPoint tilePosition = new NetPoint();

		public NetLocationRef locationRef = new NetLocationRef();

		public bool triggeredOpen;

		public NetPointDictionary<bool, NetBool> switches = new NetPointDictionary<bool, NetBool>();

		public Dictionary<Point, bool> localSwitches = new Dictionary<Point, bool>();

		public NetBool opened = new NetBool(value: false);

		public bool localOpened;

		public NetInt pressedSwitches = new NetInt(0);

		public int localPressedSwitches;

		public NetInt gateIndex = new NetInt(0);

		public NetEvent0 openEvent = new NetEvent0();

		public NetEvent1Field<Point, NetPoint> pressEvent = new NetEvent1Field<Point, NetPoint>();

		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public DwarfGate()
		{
			InitNetFields();
		}

		public DwarfGate(VolcanoDungeon location, int gate_index, int x, int y, int seed)
			: this()
		{
			locationRef.Value = location;
			tilePosition.X = x;
			tilePosition.Y = y;
			gateIndex.Value = gate_index;
			Random r = new Random(seed);
			if (location.possibleSwitchPositions.ContainsKey(gate_index))
			{
				int max_points = Math.Min(location.possibleSwitchPositions[gate_index].Count, 3);
				if (gate_index > 0)
				{
					max_points = 1;
				}
				List<Point> points = new List<Point>(location.possibleSwitchPositions[gate_index]);
				Utility.Shuffle(r, points);
				int points_to_choose2 = r.Next(1, Math.Max(1, max_points));
				points_to_choose2 = Math.Min(points_to_choose2, max_points);
				if (location.isMonsterLevel())
				{
					points_to_choose2 = max_points;
				}
				for (int i = 0; i < points_to_choose2; i++)
				{
					switches[points[i]] = false;
				}
			}
			UpdateLocalStates();
			ApplyTiles();
		}

		public virtual void InitNetFields()
		{
			NetFields.AddFields(tilePosition, locationRef.NetFields, switches, pressedSwitches, openEvent.NetFields, opened, pressEvent.NetFields, gateIndex);
			pressEvent.onEvent += OnPress;
			openEvent.onEvent += OpenGate;
			switches.InterpolationWait = false;
			pressedSwitches.InterpolationWait = false;
			pressEvent.InterpolationWait = false;
		}

		public virtual void OnPress(Point point)
		{
			if (Game1.IsMasterGame && switches.ContainsKey(point) && !switches[point])
			{
				switches[point] = true;
				pressedSwitches.Value++;
			}
			if (Game1.currentLocation == locationRef.Value)
			{
				Game1.playSound("openBox");
			}
			localSwitches[point] = true;
			ApplyTiles();
		}

		public virtual void OpenGate()
		{
			if (Game1.currentLocation == locationRef.Value)
			{
				Game1.playSound("cowboy_gunload");
			}
			if (Game1.IsMasterGame)
			{
				if (gateIndex.Value == -1 && !Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
				{
					Game1.addMailForTomorrow("volcanoShortcutUnlocked", noLetter: true);
				}
				opened.Value = true;
			}
			localOpened = true;
			ApplyTiles();
		}

		public virtual void ResetLocalState()
		{
			UpdateLocalStates();
			ApplyTiles();
		}

		public virtual void UpdateLocalStates()
		{
			localOpened = opened.Value;
			localPressedSwitches = pressedSwitches.Value;
			foreach (Point key in switches.Keys)
			{
				localSwitches[key] = switches[key];
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (!localOpened)
			{
				b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(tilePosition.X, tilePosition.Y) * 64f + new Vector2(1f, -5f) * 4f), new Rectangle(178, 189, 14, 34), Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, (float)((tilePosition.Y + 2) * 64) / 10000f);
			}
		}

		public virtual void UpdateWhenCurrentLocation(GameTime time, GameLocation location)
		{
			openEvent.Poll();
			pressEvent.Poll();
			if (localPressedSwitches != pressedSwitches.Value)
			{
				localPressedSwitches = pressedSwitches.Value;
				ApplyTiles();
			}
			if (!localOpened && opened.Value)
			{
				localOpened = true;
				ApplyTiles();
			}
			foreach (Point key in switches.Keys)
			{
				if (switches[key] && !localSwitches[key])
				{
					localSwitches[key] = true;
					ApplyTiles();
				}
			}
		}

		public virtual void ApplyTiles()
		{
			int total_switches = 0;
			int local_pressed_switches = 0;
			int pressed_switches = 0;
			foreach (Point point in localSwitches.Keys)
			{
				total_switches++;
				if (switches[point])
				{
					pressed_switches++;
				}
				if (localSwitches[point])
				{
					local_pressed_switches++;
					locationRef.Value.setMapTileIndex(point.X, point.Y, VolcanoDungeon.GetTileIndex(1, 31), "Back");
					locationRef.Value.removeTileProperty(point.X, point.Y, "Back", "TouchAction");
				}
				else
				{
					locationRef.Value.setMapTileIndex(point.X, point.Y, VolcanoDungeon.GetTileIndex(0, 31), "Back");
					locationRef.Value.setTileProperty(point.X, point.Y, "Back", "TouchAction", "DwarfSwitch");
				}
			}
			switch (total_switches)
			{
			case 1:
				locationRef.Value.setMapTileIndex(tilePosition.X - 1, tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 23), "Buildings");
				break;
			case 2:
				locationRef.Value.setMapTileIndex(tilePosition.X - 1, tilePosition.Y, VolcanoDungeon.GetTileIndex(12 + local_pressed_switches, 23), "Buildings");
				break;
			case 3:
				locationRef.Value.setMapTileIndex(tilePosition.X - 1, tilePosition.Y, VolcanoDungeon.GetTileIndex(10 + local_pressed_switches, 22), "Buildings");
				break;
			}
			if (!triggeredOpen && pressed_switches >= total_switches)
			{
				triggeredOpen = true;
				if (Game1.IsMasterGame)
				{
					DelayedAction.functionAfterDelay(delegate
					{
						openEvent.Fire();
					}, 500);
				}
			}
			if (localOpened)
			{
				locationRef.Value.removeTile(tilePosition.X, tilePosition.Y + 1, "Buildings");
			}
			else
			{
				locationRef.Value.setMapTileIndex(tilePosition.X, tilePosition.Y + 1, 0, "Buildings");
			}
		}
	}
}
