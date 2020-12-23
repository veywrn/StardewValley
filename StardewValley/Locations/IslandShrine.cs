using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Locations
{
	public class IslandShrine : IslandForestLocation
	{
		[XmlIgnore]
		public ItemPedestal northPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal southPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal eastPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public ItemPedestal westPedestal = new NetRef<ItemPedestal>();

		[XmlIgnore]
		public NetEvent0 puzzleFinishedEvent = new NetEvent0();

		[XmlElement("puzzleFinished")]
		public NetBool puzzleFinished = new NetBool();

		public IslandShrine()
		{
		}

		public IslandShrine(string map, string name)
			: base(map, name)
		{
			AddMissingPedestals();
		}

		public override List<Vector2> GetAdditionalWalnutBushes()
		{
			return new List<Vector2>
			{
				new Vector2(23f, 34f)
			};
		}

		public virtual void AddMissingPedestals()
		{
			Vector2 position = new Vector2(0f, 0f);
			position.X = 21f;
			position.Y = 27f;
			IslandGemBird.GemBirdType bird_type5 = IslandGemBird.GemBirdType.Amethyst;
			Object existing_pedestal4 = getObjectAtTile((int)position.X, (int)position.Y);
			bird_type5 = IslandGemBird.GetBirdTypeForLocation("IslandWest");
			if (existing_pedestal4 == null)
			{
				westPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
				objects.Add(position, westPedestal);
				westPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(bird_type5), 1);
				westPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (existing_pedestal4 is ItemPedestal)
			{
				ItemPedestal pedestal4 = existing_pedestal4 as ItemPedestal;
				int item_index4 = IslandGemBird.GetItemIndex(bird_type5);
				if (pedestal4.requiredItem.Value == null || pedestal4.requiredItem.Value.ParentSheetIndex != item_index4)
				{
					pedestal4.requiredItem.Value = new Object(Vector2.Zero, item_index4, 1);
					if (pedestal4.heldObject.Value != null && pedestal4.heldObject.Value.ParentSheetIndex != item_index4)
					{
						pedestal4.heldObject.Value = null;
					}
				}
			}
			position.X = 27f;
			position.Y = 27f;
			existing_pedestal4 = getObjectAtTile((int)position.X, (int)position.Y);
			bird_type5 = IslandGemBird.GetBirdTypeForLocation("IslandEast");
			if (existing_pedestal4 == null)
			{
				eastPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
				objects.Add(position, eastPedestal);
				eastPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(bird_type5), 1);
				eastPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (existing_pedestal4 is ItemPedestal)
			{
				ItemPedestal pedestal3 = existing_pedestal4 as ItemPedestal;
				int item_index3 = IslandGemBird.GetItemIndex(bird_type5);
				if (pedestal3.requiredItem.Value == null || pedestal3.requiredItem.Value.ParentSheetIndex != item_index3)
				{
					pedestal3.requiredItem.Value = new Object(Vector2.Zero, item_index3, 1);
					if (pedestal3.heldObject.Value != null && pedestal3.heldObject.Value.ParentSheetIndex != item_index3)
					{
						pedestal3.heldObject.Value = null;
					}
				}
			}
			position.X = 24f;
			position.Y = 28f;
			existing_pedestal4 = getObjectAtTile((int)position.X, (int)position.Y);
			bird_type5 = IslandGemBird.GetBirdTypeForLocation("IslandSouth");
			if (existing_pedestal4 == null)
			{
				southPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
				objects.Add(position, southPedestal);
				southPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(bird_type5), 1);
				southPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else if (existing_pedestal4 is ItemPedestal)
			{
				ItemPedestal pedestal2 = existing_pedestal4 as ItemPedestal;
				int item_index2 = IslandGemBird.GetItemIndex(bird_type5);
				if (pedestal2.requiredItem.Value == null || pedestal2.requiredItem.Value.ParentSheetIndex != item_index2)
				{
					pedestal2.requiredItem.Value = new Object(Vector2.Zero, item_index2, 1);
					if (pedestal2.heldObject.Value != null && pedestal2.heldObject.Value.ParentSheetIndex != item_index2)
					{
						pedestal2.heldObject.Value = null;
					}
				}
			}
			position.X = 24f;
			position.Y = 25f;
			existing_pedestal4 = getObjectAtTile((int)position.X, (int)position.Y);
			bird_type5 = IslandGemBird.GetBirdTypeForLocation("IslandNorth");
			if (existing_pedestal4 == null)
			{
				northPedestal = new ItemPedestal(position, null, lock_on_success: false, Color.White);
				objects.Add(position, northPedestal);
				northPedestal.requiredItem.Value = new Object(Vector2.Zero, IslandGemBird.GetItemIndex(bird_type5), 1);
				northPedestal.successColor.Value = new Color(0, 0, 0, 0);
			}
			else
			{
				if (!(existing_pedestal4 is ItemPedestal))
				{
					return;
				}
				ItemPedestal pedestal = existing_pedestal4 as ItemPedestal;
				int item_index = IslandGemBird.GetItemIndex(bird_type5);
				if (pedestal.requiredItem.Value == null || pedestal.requiredItem.Value.ParentSheetIndex != item_index)
				{
					pedestal.requiredItem.Value = new Object(Vector2.Zero, item_index, 1);
					if (pedestal.heldObject.Value != null && pedestal.heldObject.Value.ParentSheetIndex != item_index)
					{
						pedestal.heldObject.Value = null;
					}
				}
			}
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(puzzleFinished, puzzleFinishedEvent);
			puzzleFinishedEvent.onEvent += OnPuzzleFinish;
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			if (puzzleFinished.Value)
			{
				ApplyFinishedTiles();
			}
			if (Game1.IsMasterGame)
			{
				AddMissingPedestals();
			}
		}

		public override void TransferDataFromSavedLocation(GameLocation l)
		{
			base.TransferDataFromSavedLocation(l);
			if (l is IslandShrine)
			{
				IslandShrine shrine = l as IslandShrine;
				northPedestal = (shrine.getObjectAtTile((int)northPedestal.TileLocation.X, (int)northPedestal.TileLocation.Y) as ItemPedestal);
				southPedestal = (shrine.getObjectAtTile((int)southPedestal.TileLocation.X, (int)southPedestal.TileLocation.Y) as ItemPedestal);
				eastPedestal = (shrine.getObjectAtTile((int)eastPedestal.TileLocation.X, (int)eastPedestal.TileLocation.Y) as ItemPedestal);
				westPedestal = (shrine.getObjectAtTile((int)westPedestal.TileLocation.X, (int)westPedestal.TileLocation.Y) as ItemPedestal);
				puzzleFinished.Value = shrine.puzzleFinished.Value;
			}
		}

		public void OnPuzzleFinish()
		{
			if (Game1.IsMasterGame)
			{
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
				Game1.createItemDebris(new Object(73, 1), new Vector2(24f, 19f) * 64f, -1, this);
			}
			if (Game1.currentLocation == this)
			{
				Game1.playSound("boulderBreak");
				Game1.playSound("secret1");
				Game1.flashAlpha = 1f;
				ApplyFinishedTiles();
			}
		}

		public virtual void ApplyFinishedTiles()
		{
			setMapTileIndex(23, 19, 142, "AlwaysFront", 2);
			setMapTileIndex(24, 19, 143, "AlwaysFront", 2);
			setMapTileIndex(25, 19, 144, "AlwaysFront", 2);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (Game1.IsMasterGame && !puzzleFinished.Value && northPedestal.match.Value && southPedestal.match.Value && eastPedestal.match.Value && westPedestal.match.Value)
			{
				Game1.player.team.MarkCollectedNut("IslandShrinePuzzle");
				puzzleFinishedEvent.Fire();
				puzzleFinished.Value = true;
				northPedestal.locked.Value = true;
				northPedestal.heldObject.Value = null;
				southPedestal.locked.Value = true;
				southPedestal.heldObject.Value = null;
				eastPedestal.locked.Value = true;
				eastPedestal.heldObject.Value = null;
				westPedestal.locked.Value = true;
				westPedestal.heldObject.Value = null;
			}
		}
	}
}
