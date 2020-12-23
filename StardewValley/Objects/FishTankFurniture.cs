using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class FishTankFurniture : StorageFurniture
	{
		public enum FishTankCategories
		{
			None,
			Swim,
			Ground,
			Decoration
		}

		public const int TANK_DEPTH = 10;

		public const int FLOOR_DECORATION_OFFSET = 4;

		public const int TANK_SORT_REGION = 20;

		[XmlIgnore]
		public List<Vector4> bubbles = new List<Vector4>();

		[XmlIgnore]
		public List<TankFish> tankFish = new List<TankFish>();

		[XmlIgnore]
		public NetEvent0 refreshFishEvent = new NetEvent0();

		[XmlIgnore]
		public bool fishDirty = true;

		[XmlIgnore]
		private Texture2D _aquariumTexture;

		[XmlIgnore]
		public List<KeyValuePair<Rectangle, Vector2>?> floorDecorations = new List<KeyValuePair<Rectangle, Vector2>?>();

		[XmlIgnore]
		public List<Vector2> decorationSlots = new List<Vector2>();

		[XmlIgnore]
		public List<int> floorDecorationIndices = new List<int>();

		public NetInt generationSeed = new NetInt();

		[XmlIgnore]
		public Item localDepositedItem;

		[XmlIgnore]
		protected int _currentDecorationIndex;

		protected Dictionary<Item, TankFish> _fishLookup = new Dictionary<Item, TankFish>();

		public FishTankFurniture()
		{
			generationSeed.Value = Game1.random.Next();
		}

		public FishTankFurniture(int which, Vector2 tile, int initialRotations)
			: base(which, tile, initialRotations)
		{
			generationSeed.Value = Game1.random.Next();
		}

		public FishTankFurniture(int which, Vector2 tile)
			: base(which, tile)
		{
			generationSeed.Value = Game1.random.Next();
		}

		public override void resetOnPlayerEntry(GameLocation environment, bool dropDown = false)
		{
			base.resetOnPlayerEntry(environment, dropDown);
			ResetFish();
			UpdateFish();
		}

		public virtual void ResetFish()
		{
			bubbles.Clear();
			tankFish.Clear();
			_fishLookup.Clear();
			UpdateFish();
		}

		public Texture2D GetAquariumTexture()
		{
			if (_aquariumTexture == null)
			{
				_aquariumTexture = Game1.content.Load<Texture2D>("LooseSprites\\AquariumFish");
			}
			return _aquariumTexture;
		}

		protected override void initNetFields()
		{
			base.NetFields.AddFields(generationSeed, refreshFishEvent);
			refreshFishEvent.onEvent += UpdateDecorAndFish;
			base.initNetFields();
		}

		public override Item getOne()
		{
			FishTankFurniture fishTankFurniture = new FishTankFurniture(parentSheetIndex, tileLocation);
			fishTankFurniture.drawPosition.Value = drawPosition;
			fishTankFurniture.defaultBoundingBox.Value = defaultBoundingBox;
			fishTankFurniture.boundingBox.Value = boundingBox;
			fishTankFurniture.currentRotation.Value = (int)currentRotation - 1;
			fishTankFurniture.isOn.Value = false;
			fishTankFurniture.rotations.Value = rotations;
			fishTankFurniture.rotate();
			fishTankFurniture._GetOneFrom(this);
			return fishTankFurniture;
		}

		public int GetCapacityForCategory(FishTankCategories category)
		{
			int tiles_wide = getTilesWide();
			switch (category)
			{
			case FishTankCategories.Swim:
				return tiles_wide - 1;
			case FishTankCategories.Ground:
				return tiles_wide - 1;
			case FishTankCategories.Decoration:
				if (tiles_wide <= 2)
				{
					return 1;
				}
				return -1;
			default:
				return 0;
			}
		}

		public FishTankCategories GetCategoryFromItem(Item item)
		{
			Dictionary<int, string> aquarium_data = GetAquariumData();
			if (!CanBeDeposited(item))
			{
				return FishTankCategories.None;
			}
			if (aquarium_data.ContainsKey(item.ParentSheetIndex))
			{
				switch (aquarium_data[item.ParentSheetIndex].Split('/')[1])
				{
				case "crawl":
				case "ground":
				case "front_crawl":
				case "static":
					return FishTankCategories.Ground;
				default:
					return FishTankCategories.Swim;
				}
			}
			return FishTankCategories.Decoration;
		}

		public bool HasRoomForThisItem(Item item)
		{
			if (!CanBeDeposited(item))
			{
				return false;
			}
			FishTankCategories category = GetCategoryFromItem(item);
			int capacity = GetCapacityForCategory(category);
			if (item is Hat)
			{
				capacity = 999;
			}
			if (capacity < 0)
			{
				foreach (Item held_item2 in heldItems)
				{
					if (held_item2 != null && held_item2.ParentSheetIndex == item.ParentSheetIndex)
					{
						return false;
					}
				}
				return true;
			}
			int current_count = 0;
			foreach (Item held_item in heldItems)
			{
				if (held_item != null)
				{
					if (GetCategoryFromItem(held_item) == category)
					{
						current_count++;
					}
					if (current_count >= capacity)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override string GetShopMenuContext()
		{
			return "FishTank";
		}

		public override void ShowMenu()
		{
			ShowShopMenu();
		}

		public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (mutex.IsLocked())
			{
				return true;
			}
			if ((who.ActiveObject != null || (who.CurrentItem != null && who.CurrentItem is Hat)) && localDepositedItem == null && CanBeDeposited(who.CurrentItem))
			{
				if (!HasRoomForThisItem(who.CurrentItem))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishTank_Full"));
					return true;
				}
				GameLocation location = who.currentLocation;
				localDepositedItem = who.CurrentItem.getOne();
				who.CurrentItem.Stack--;
				if (who.CurrentItem.Stack <= 0 || who.CurrentItem is Hat)
				{
					who.removeItemFromInventory(who.CurrentItem);
					who.showNotCarrying();
				}
				mutex.RequestLock(delegate
				{
					location.playSound("dropItemInWater");
					heldItems.Add(localDepositedItem);
					localDepositedItem = null;
					refreshFishEvent.Fire();
					mutex.ReleaseLock();
				}, delegate
				{
					localDepositedItem = who.addItemToInventory(localDepositedItem);
					if (localDepositedItem != null)
					{
						Game1.createItemDebris(localDepositedItem, new Vector2(base.TileLocation.X + (float)getTilesWide() / 2f + 0.5f, base.TileLocation.Y + 0.5f) * 64f, -1, location);
					}
					localDepositedItem = null;
				});
				return true;
			}
			mutex.RequestLock(delegate
			{
				ShowMenu();
			});
			return true;
		}

		public virtual bool CanBeDeposited(Item item)
		{
			if (item == null)
			{
				return false;
			}
			if (!(item is Hat) && !Utility.IsNormalObjectAtParentSheetIndex(item, item.ParentSheetIndex))
			{
				return false;
			}
			if (item.ParentSheetIndex == 152 || item.ParentSheetIndex == 393 || item.ParentSheetIndex == 390)
			{
				return true;
			}
			if (item is Hat)
			{
				int numUrchins = 0;
				int numHats = 0;
				foreach (Item i in heldItems)
				{
					if (i is Hat)
					{
						numHats++;
					}
					else if (i is Object && (int)i.parentSheetIndex == 397)
					{
						numUrchins++;
					}
				}
				if (numHats < numUrchins)
				{
					return true;
				}
				return false;
			}
			if (!GetAquariumData().ContainsKey(item.ParentSheetIndex))
			{
				return false;
			}
			return true;
		}

		public override void DayUpdate(GameLocation location)
		{
			ResetFish();
			base.DayUpdate(location);
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			if (Game1.currentLocation == environment)
			{
				if (fishDirty)
				{
					fishDirty = false;
					UpdateDecorAndFish();
				}
				foreach (TankFish item in tankFish)
				{
					item.Update(time);
				}
				for (int i = 0; i < bubbles.Count; i++)
				{
					Vector4 bubble = bubbles[i];
					bubble.W += 0.05f;
					if (bubble.W > 1f)
					{
						bubble.W = 1f;
					}
					bubble.Y += bubble.W;
					bubbles[i] = bubble;
					if (bubble.Y >= (float)GetTankBounds().Height)
					{
						bubbles.RemoveAt(i);
						i--;
					}
				}
			}
			base.updateWhenCurrentLocation(time, environment);
			refreshFishEvent.Poll();
		}

		public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
		{
			generationSeed.Value = Game1.random.Next();
			fishDirty = true;
			return base.placementAction(location, x, y, who);
		}

		public Dictionary<int, string> GetAquariumData()
		{
			return Game1.content.Load<Dictionary<int, string>>("Data\\AquariumFish");
		}

		public override bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
		{
			bool result = base.onDresserItemWithdrawn(salable, who, amount);
			refreshFishEvent.Fire();
			return result;
		}

		public virtual void UpdateFish()
		{
			List<Item> fish_items = new List<Item>();
			Dictionary<int, string> aquarium_data = GetAquariumData();
			foreach (Item item2 in heldItems)
			{
				if (item2 != null && Utility.IsNormalObjectAtParentSheetIndex(item2, item2.ParentSheetIndex) && aquarium_data.ContainsKey(item2.ParentSheetIndex))
				{
					fish_items.Add(item2);
				}
			}
			List<Item> items_to_remove = new List<Item>();
			foreach (Item key in _fishLookup.Keys)
			{
				if (!heldItems.Contains(key))
				{
					items_to_remove.Add(key);
				}
			}
			for (int i = 0; i < fish_items.Count; i++)
			{
				Item item = fish_items[i];
				if (!_fishLookup.ContainsKey(item))
				{
					TankFish fish = new TankFish(this, item);
					tankFish.Add(fish);
					_fishLookup[item] = fish;
				}
			}
			foreach (Item removed_item in items_to_remove)
			{
				tankFish.Remove(_fishLookup[removed_item]);
				heldItems.Remove(removed_item);
			}
		}

		public virtual void UpdateDecorAndFish()
		{
			Random r = new Random(generationSeed.Value);
			UpdateFish();
			decorationSlots.Clear();
			for (int y = 0; y < 3; y++)
			{
				for (int x = 0; x < getTilesWide(); x++)
				{
					Vector2 slot_position = default(Vector2);
					if (y % 2 == 0)
					{
						if (x == getTilesWide() - 1)
						{
							continue;
						}
						slot_position.X = 16 + x * 16;
					}
					else
					{
						slot_position.X = 8 + x * 16;
					}
					slot_position.Y = 4f;
					slot_position.Y += 3.33333325f * (float)y;
					decorationSlots.Add(slot_position);
				}
			}
			floorDecorationIndices.Clear();
			floorDecorations.Clear();
			_currentDecorationIndex = 0;
			for (int l = 0; l < decorationSlots.Count; l++)
			{
				floorDecorationIndices.Add(l);
				floorDecorations.Add(null);
			}
			Utility.Shuffle(r, floorDecorationIndices);
			Random decoration_random3 = new Random(r.Next());
			bool add_decoration3 = GetItemCount(393) > 0;
			for (int k = 0; k < 1; k++)
			{
				if (add_decoration3)
				{
					AddFloorDecoration(new Rectangle(16 * decoration_random3.Next(0, 5), 256, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
			decoration_random3 = new Random(r.Next());
			bool add_decoration2 = GetItemCount(152) > 0;
			for (int j = 0; j < 4; j++)
			{
				if (add_decoration2)
				{
					AddFloorDecoration(new Rectangle(16 * decoration_random3.Next(0, 3), 288, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
			decoration_random3 = new Random(r.Next());
			bool add_decoration = GetItemCount(390) > 0;
			for (int i = 0; i < 2; i++)
			{
				if (add_decoration)
				{
					AddFloorDecoration(new Rectangle(16 * decoration_random3.Next(0, 3), 272, 16, 16));
				}
				else
				{
					_AdvanceDecorationIndex();
				}
			}
		}

		public virtual void AddFloorDecoration(Rectangle source_rect)
		{
			if (_currentDecorationIndex != -1)
			{
				int index = floorDecorationIndices[_currentDecorationIndex];
				_AdvanceDecorationIndex();
				int center_x = (int)decorationSlots[index].X;
				int center_y = (int)decorationSlots[index].Y;
				if (center_x < source_rect.Width / 2)
				{
					center_x = source_rect.Width / 2;
				}
				if (center_x > GetTankBounds().Width / 4 - source_rect.Width / 2)
				{
					center_x = GetTankBounds().Width / 4 - source_rect.Width / 2;
				}
				KeyValuePair<Rectangle, Vector2> decoration = new KeyValuePair<Rectangle, Vector2>(source_rect, new Vector2(center_x, center_y));
				floorDecorations[index] = decoration;
			}
		}

		protected virtual void _AdvanceDecorationIndex()
		{
			for (int i = 0; i < decorationSlots.Count; i++)
			{
				_currentDecorationIndex++;
				if (_currentDecorationIndex >= decorationSlots.Count)
				{
					_currentDecorationIndex = 0;
				}
				if (!floorDecorations[floorDecorationIndices[_currentDecorationIndex]].HasValue)
				{
					return;
				}
			}
			_currentDecorationIndex = 1;
		}

		public override void OnMenuClose()
		{
			refreshFishEvent.Fire();
			base.OnMenuClose();
		}

		public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
			base.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
		}

		public Vector2 GetFishSortRegion()
		{
			return new Vector2(GetBaseDrawLayer() + 1E-06f, GetGlassDrawLayer() - 1E-06f);
		}

		public float GetGlassDrawLayer()
		{
			return GetBaseDrawLayer() + 0.0001f;
		}

		public float GetBaseDrawLayer()
		{
			if ((int)furniture_type != 12)
			{
				return (float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 13) ? 48 : 8)) / 10000f;
			}
			return 2E-09f;
		}

		public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
		{
			Vector2 shake = Vector2.Zero;
			if (isTemporarilyInvisible)
			{
				return;
			}
			Vector2 draw_position = drawPosition.Value;
			if (!Furniture.isDrawingLocationFurniture)
			{
				draw_position = new Vector2(x, y) * 64f;
				draw_position.Y -= sourceRect.Height * 4 - boundingBox.Height;
			}
			if (shakeTimer > 0)
			{
				shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			spriteBatch.Draw(Furniture.furnitureTexture, Game1.GlobalToLocal(Game1.viewport, draw_position + shake), new Rectangle(sourceRect.Value.X + sourceRect.Value.Width, sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetGlassDrawLayer());
			if (Furniture.isDrawingLocationFurniture)
			{
				int hatsDrawn = 0;
				for (int i = 0; i < tankFish.Count; i++)
				{
					TankFish fish = tankFish[i];
					float fish_layer2 = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, fish.zPosition / 20f);
					fish_layer2 += 1E-07f * (float)i;
					fish.Draw(spriteBatch, alpha, fish_layer2);
					if (fish.fishIndex == 86)
					{
						int hatsSoFar = 0;
						foreach (Item h in heldItems)
						{
							if (h is Hat)
							{
								if (hatsSoFar == hatsDrawn)
								{
									h.drawInMenu(spriteBatch, Game1.GlobalToLocal(fish.GetWorldPosition() + new Vector2(-30 + (fish.facingLeft ? (-4) : 0), -55f)), 0.75f, 1f, fish_layer2 + 1E-08f, StackDrawType.Hide);
									hatsDrawn++;
									break;
								}
								hatsSoFar++;
							}
						}
					}
				}
				for (int j = 0; j < floorDecorations.Count; j++)
				{
					if (floorDecorations[j].HasValue)
					{
						KeyValuePair<Rectangle, Vector2> decoration = floorDecorations[j].Value;
						Vector2 decoration_position = decoration.Value;
						Rectangle decoration_source_rect = decoration.Key;
						float decoration_layer = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
						spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + decoration_position.X * 4f, (float)(GetTankBounds().Bottom - 4) - decoration_position.Y * 4f)), decoration_source_rect, Color.White * alpha, 0f, new Vector2(decoration_source_rect.Width / 2, decoration_source_rect.Height - 4), 4f, SpriteEffects.None, decoration_layer);
					}
				}
				foreach (Vector4 bubble in bubbles)
				{
					float layer = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
					spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + bubble.X, (float)(GetTankBounds().Bottom - 4) - bubble.Y - bubble.Z * 4f)), new Rectangle(0, 240, 16, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * bubble.W, SpriteEffects.None, layer);
				}
			}
			base.draw(spriteBatch, x, y, alpha);
		}

		public int GetItemCount(int parent_sheet_index)
		{
			int count = 0;
			foreach (Item item in heldItems)
			{
				if (Utility.IsNormalObjectAtParentSheetIndex(item, parent_sheet_index))
				{
					count += item.Stack;
				}
			}
			return count;
		}

		public virtual Rectangle GetTankBounds()
		{
			int height = defaultSourceRect.Value.Height / 16;
			int width = defaultSourceRect.Value.Width / 16;
			Rectangle tank_rect = new Rectangle((int)base.TileLocation.X * 64, (int)((base.TileLocation.Y - (float)getTilesHigh() - 1f) * 64f), width * 64, height * 64);
			tank_rect.X += 4;
			tank_rect.Width -= 8;
			tank_rect.Height -= 28;
			tank_rect.Y += 64;
			tank_rect.Height -= 64;
			return tank_rect;
		}
	}
}
