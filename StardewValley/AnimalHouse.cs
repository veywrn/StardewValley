using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.Network;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Xml.Serialization;
using xTile.Dimensions;

namespace StardewValley
{
	public class AnimalHouse : GameLocation, IAnimalLocation
	{
		[XmlElement("animals")]
		public readonly NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> animals = new NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>();

		[XmlElement("animalLimit")]
		public readonly NetInt animalLimit = new NetInt(4);

		public readonly NetLongList animalsThatLiveHere = new NetLongList();

		[XmlElement("incubatingEgg")]
		public readonly NetPoint incubatingEgg = new NetPoint();

		private readonly List<KeyValuePair<long, FarmAnimal>> _tempAnimals = new List<KeyValuePair<long, FarmAnimal>>();

		public NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>> Animals => animals;

		public AnimalHouse()
		{
		}

		public AnimalHouse(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(animals, animalLimit, animalsThatLiveHere, incubatingEgg);
		}

		public void updateWhenNotCurrentLocation(Building parentBuilding, GameTime time)
		{
			if (!Game1.currentLocation.Equals(this))
			{
				for (int i = animals.Count() - 1; i >= 0; i--)
				{
					animals.Pairs.ElementAt(i).Value.updateWhenNotCurrentLocation(parentBuilding, time, this);
				}
			}
		}

		public void incubator()
		{
			if (incubatingEgg.Y <= 0 && Game1.player.ActiveObject != null && Game1.player.ActiveObject.Category == -5)
			{
				incubatingEgg.X = 2;
				incubatingEgg.Y = Game1.player.ActiveObject.ParentSheetIndex;
				map.GetLayer("Front").Tiles[1, 2].TileIndex += ((Game1.player.ActiveObject.ParentSheetIndex != 180 && Game1.player.ActiveObject.ParentSheetIndex != 182) ? 1 : 2);
				Game1.throwActiveObjectDown();
			}
			else if (Game1.player.ActiveObject == null && incubatingEgg.Y > 0)
			{
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_RemoveEgg_Question"), createYesNoResponses(), "RemoveIncubatingEgg");
			}
		}

		public bool isFull()
		{
			return animalsThatLiveHere.Count >= (int)animalLimit;
		}

		public bool CheckPetAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (!kvp.Value.wasPet && kvp.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckPetAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (!kvp.Value.wasPet && kvp.Value.GetBoundingBox().Intersects(rect))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Vector2 position, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if ((bool)kvp.Value.wasPet && kvp.Value.GetCursorPetBoundingBox().Contains((int)position.X, (int)position.Y))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public bool CheckInspectAnimal(Microsoft.Xna.Framework.Rectangle rect, Farmer who)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if ((bool)kvp.Value.wasPet && kvp.Value.GetBoundingBox().Intersects(rect))
				{
					kvp.Value.pet(who);
					return true;
				}
			}
			return false;
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileLocation.X * 64, tileLocation.Y * 64, 64, 64);
			if (who.ActiveObject != null && who.ActiveObject.Name.Equals("Hay") && doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Trough", "Back") != null && !objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
			{
				objects.Add(new Vector2(tileLocation.X, tileLocation.Y), (Object)who.ActiveObject.getOne());
				who.reduceActiveItemByOne();
				who.currentLocation.playSound("coin");
				return true;
			}
			bool b = base.checkAction(tileLocation, viewport, who);
			if (!b)
			{
				if (CheckPetAnimal(tileRect, who))
				{
					return true;
				}
				if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true) && CheckInspectAnimal(tileRect, who))
				{
					return true;
				}
			}
			return b;
		}

		public override bool isTileOccupiedForPlacement(Vector2 tileLocation, Object toPlace = null)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				if (pair.Value.getTileLocation().Equals(tileLocation))
				{
					return true;
				}
			}
			return base.isTileOccupiedForPlacement(tileLocation, toPlace);
		}

		protected override void resetSharedState()
		{
			resetPositionsOfAllAnimals();
			foreach (Object o in objects.Values)
			{
				if ((bool)o.bigCraftable && o.Name.Contains("Incubator") && o.heldObject.Value != null && (int)o.minutesUntilReady <= 0 && !isFull())
				{
					string whatHatched = "??";
					switch (o.heldObject.Value.ParentSheetIndex)
					{
					case 305:
						whatHatched = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_VoidEgg");
						break;
					case 174:
					case 176:
					case 180:
					case 182:
						whatHatched = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_RegularEgg");
						break;
					case 442:
						whatHatched = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_DuckEgg");
						break;
					case 107:
						whatHatched = Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_Hatch_DinosaurEgg");
						break;
					}
					currentEvent = new Event("none/-1000 -1000/farmer 2 9 0/pause 250/message \"" + whatHatched + "\"/pause 500/animalNaming/pause 500/end");
					break;
				}
			}
			base.resetSharedState();
		}

		public Building getBuilding()
		{
			foreach (Building b in Game1.getFarm().buildings)
			{
				if (b.indoors.Value != null && b.indoors.Value.Equals(this))
				{
					return b;
				}
			}
			return null;
		}

		public void addNewHatchedAnimal(string name)
		{
			if (getBuilding() is Coop)
			{
				foreach (Object o in objects.Values)
				{
					if ((bool)o.bigCraftable && o.Name.Contains("Incubator") && o.heldObject.Value != null && (int)o.minutesUntilReady <= 0 && !isFull())
					{
						string animalName = "??";
						if (o.heldObject.Value == null)
						{
							animalName = "White Chicken";
						}
						else
						{
							switch (o.heldObject.Value.ParentSheetIndex)
							{
							case 305:
								animalName = "Void Chicken";
								break;
							case 174:
							case 176:
								animalName = "White Chicken";
								break;
							case 180:
							case 182:
								animalName = "Brown Chicken";
								break;
							case 442:
								animalName = "Duck";
								break;
							case 107:
								animalName = "Dinosaur";
								break;
							}
						}
						FarmAnimal a2 = new FarmAnimal(animalName, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
						a2.Name = name;
						a2.displayName = name;
						Building newAnimalHome2 = a2.home = getBuilding();
						a2.homeLocation.Value = new Vector2((int)newAnimalHome2.tileX, (int)newAnimalHome2.tileY);
						a2.setRandomPosition(a2.home.indoors);
						(newAnimalHome2.indoors.Value as AnimalHouse).animals.Add(a2.myID, a2);
						(newAnimalHome2.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(a2.myID);
						o.heldObject.Value = null;
						o.ParentSheetIndex = 101;
						break;
					}
				}
			}
			else if (Game1.farmEvent != null && Game1.farmEvent is QuestionEvent)
			{
				FarmAnimal a = new FarmAnimal((Game1.farmEvent as QuestionEvent).animal.type.Value, Game1.multiplayer.getNewID(), Game1.player.uniqueMultiplayerID);
				a.Name = name;
				a.displayName = name;
				a.parentId.Value = (Game1.farmEvent as QuestionEvent).animal.myID;
				Building newAnimalHome = a.home = getBuilding();
				a.homeLocation.Value = new Vector2((int)newAnimalHome.tileX, (int)newAnimalHome.tileY);
				(Game1.farmEvent as QuestionEvent).forceProceed = true;
				a.setRandomPosition(a.home.indoors);
				(newAnimalHome.indoors.Value as AnimalHouse).animals.Add(a.myID, a);
				(newAnimalHome.indoors.Value as AnimalHouse).animalsThatLiveHere.Add(a.myID);
			}
			Game1.exitActiveMenu();
		}

		public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false)
		{
			foreach (KeyValuePair<long, FarmAnimal> kvp in animals.Pairs)
			{
				if (character != null && !character.Equals(kvp.Value) && position.Intersects(kvp.Value.GetBoundingBox()) && (!isFarmer || !Game1.player.GetBoundingBox().Intersects(kvp.Value.GetBoundingBox())))
				{
					if (isFarmer && (character as Farmer).TemporaryPassableTiles.Intersects(position))
					{
						break;
					}
					kvp.Value.farmerPushing();
					return true;
				}
			}
			return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			foreach (KeyValuePair<long, FarmAnimal> kvp2 in animals.Pairs)
			{
				_tempAnimals.Add(kvp2);
			}
			foreach (KeyValuePair<long, FarmAnimal> kvp in _tempAnimals)
			{
				if (kvp.Value.updateWhenCurrentLocation(time, this))
				{
					animals.Remove(kvp.Key);
				}
			}
			_tempAnimals.Clear();
		}

		public void resetPositionsOfAllAnimals()
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.setRandomPosition(this);
			}
		}

		public override bool dropObject(Object obj, Vector2 location, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
		{
			Vector2 tileLocation = new Vector2((int)(location.X / 64f), (int)(location.Y / 64f));
			if (obj.Name.Equals("Hay") && doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Trough", "Back") != null)
			{
				if (!objects.ContainsKey(tileLocation))
				{
					objects.Add(tileLocation, obj);
					return true;
				}
				return false;
			}
			return base.dropObject(obj, location, viewport, initialPlacement);
		}

		public void feedAllAnimals()
		{
			int fed = 0;
			for (int x = 0; x < map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					if (doesTileHaveProperty(x, y, "Trough", "Back") != null)
					{
						Vector2 tileLocation = new Vector2(x, y);
						if (!objects.ContainsKey(tileLocation) && (int)Game1.getFarm().piecesOfHay > 0)
						{
							objects.Add(tileLocation, new Object(178, 1));
							fed++;
							Game1.getFarm().piecesOfHay.Value--;
						}
						if (fed >= (int)animalLimit)
						{
							return;
						}
					}
				}
			}
		}

		public override void DayUpdate(int dayOfMonth)
		{
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.dayUpdate(this);
			}
			base.DayUpdate(dayOfMonth);
		}

		public override bool performToolAction(Tool t, int tileX, int tileY)
		{
			if (t is MeleeWeapon)
			{
				foreach (FarmAnimal a in animals.Values)
				{
					if (a.GetBoundingBox().Intersects((t as MeleeWeapon).mostRecentArea))
					{
						a.hitWithWeapon(t as MeleeWeapon);
					}
				}
			}
			return false;
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
			{
				pair.Value.draw(b);
			}
		}
	}
}
