using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.HomeRenovations;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class HouseRenovation : ISalable
	{
		public enum AnimationType
		{
			Build,
			Destroy
		}

		protected string _displayName;

		protected string _name;

		protected string _description;

		public AnimationType animationType;

		public List<List<Rectangle>> renovationBounds = new List<List<Rectangle>>();

		public string placementText = "";

		public GameLocation location;

		public bool requireClearance = true;

		public Action<HouseRenovation, int> onRenovation;

		public Func<HouseRenovation, int, bool> validate;

		public string DisplayName => _displayName;

		public string Name => _name;

		public int Stack
		{
			get
			{
				return 1;
			}
			set
			{
			}
		}

		public bool ShouldDrawIcon()
		{
			return false;
		}

		public void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
		{
		}

		public string getDescription()
		{
			return _description;
		}

		public int maximumStackSize()
		{
			return 1;
		}

		public int addToStack(Item stack)
		{
			return 0;
		}

		public int salePrice()
		{
			return 0;
		}

		public bool actionWhenPurchased()
		{
			return false;
		}

		public bool canStackWith(ISalable other)
		{
			return false;
		}

		public bool CanBuyItem(Farmer farmer)
		{
			return true;
		}

		public bool IsInfiniteStock()
		{
			return true;
		}

		public ISalable GetSalableInstance()
		{
			return this;
		}

		public static void ShowRenovationMenu()
		{
			Game1.activeClickableMenu = new ShopMenu(GetAvailableRenovations(), 0, null, OnPurchaseRenovation);
		}

		public static List<ISalable> GetAvailableRenovations()
		{
			FarmHouse farmhouse = Game1.getLocationFromName(Game1.player.homeLocation) as FarmHouse;
			List<ISalable> available_renovations = new List<ISalable>();
			HouseRenovation renovation2 = null;
			Dictionary<string, HomeRenovation> data = Game1.content.Load<Dictionary<string, HomeRenovation>>("Data\\HomeRenovations");
			foreach (string key in data.Keys)
			{
				HomeRenovation renovation_data = data[key];
				bool valid = true;
				foreach (RenovationValue requirement_data in renovation_data.Requirements)
				{
					if (requirement_data.Type == "Value")
					{
						string requirement_value = requirement_data.Value;
						bool match = true;
						if (requirement_value.Length > 0 && requirement_value[0] == '!')
						{
							requirement_value = requirement_value.Substring(1);
							match = false;
						}
						int value = int.Parse(requirement_value);
						try
						{
							NetInt field2 = (NetInt)farmhouse.GetType().GetField(requirement_data.Key).GetValue(farmhouse);
							if (field2 == null)
							{
								valid = false;
							}
							else
							{
								if (field2.Value == value == match)
								{
									continue;
								}
								valid = false;
							}
						}
						catch (Exception)
						{
							valid = false;
						}
						break;
					}
					if (requirement_data.Type == "Mail")
					{
						_ = requirement_data.Value;
						if (Game1.player.hasOrWillReceiveMail(requirement_data.Key) != (requirement_data.Value == "1"))
						{
							valid = false;
							break;
						}
					}
				}
				if (valid)
				{
					renovation2 = new HouseRenovation();
					renovation2.location = farmhouse;
					renovation2._name = key;
					string[] split = Game1.content.LoadString(renovation_data.TextStrings).Split('/');
					try
					{
						renovation2._displayName = split[0];
						renovation2._description = split[1];
						renovation2.placementText = split[2];
					}
					catch (Exception)
					{
						renovation2._displayName = "?";
						renovation2._description = "?";
						renovation2.placementText = "?";
					}
					if (renovation_data.CheckForObstructions)
					{
						HouseRenovation houseRenovation = renovation2;
						houseRenovation.validate = (Func<HouseRenovation, int, bool>)Delegate.Combine(houseRenovation.validate, new Func<HouseRenovation, int, bool>(EnsureNoObstructions));
					}
					if (renovation_data.AnimationType == "destroy")
					{
						renovation2.animationType = AnimationType.Destroy;
					}
					else
					{
						renovation2.animationType = AnimationType.Build;
					}
					if (renovation_data.SpecialRect != null && renovation_data.SpecialRect != "")
					{
						if (renovation_data.SpecialRect == "crib")
						{
							Rectangle? crib_bounds = farmhouse.GetCribBounds();
							if (!farmhouse.CanModifyCrib() || !crib_bounds.HasValue)
							{
								continue;
							}
							renovation2.AddRenovationBound(crib_bounds.Value);
						}
					}
					else
					{
						foreach (RectGroup rectGroup in renovation_data.RectGroups)
						{
							List<Rectangle> rectangles = new List<Rectangle>();
							foreach (Rect rect in rectGroup.Rects)
							{
								Rectangle rectangle = default(Rectangle);
								rectangle.X = rect.X;
								rectangle.Y = rect.Y;
								rectangle.Width = rect.Width;
								rectangle.Height = rect.Height;
								rectangles.Add(rectangle);
							}
							renovation2.AddRenovationBound(rectangles);
						}
					}
					foreach (RenovationValue action_data in renovation_data.RenovateActions)
					{
						if (action_data.Type == "Value")
						{
							try
							{
								NetInt field = (NetInt)farmhouse.GetType().GetField(action_data.Key).GetValue(farmhouse);
								if (!(field == null))
								{
									Action<HouseRenovation, int> action2 = delegate(HouseRenovation selected_renovation, int index)
									{
										if (action_data.Value == "selected")
										{
											field.Value = index;
										}
										else
										{
											int value2 = int.Parse(action_data.Value);
											field.Value = value2;
										}
									};
									HouseRenovation houseRenovation2 = renovation2;
									houseRenovation2.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation2.onRenovation, action2);
									continue;
								}
								valid = false;
							}
							catch (Exception)
							{
								valid = false;
							}
							break;
						}
						if (action_data.Type == "Mail")
						{
							Action<HouseRenovation, int> action = delegate
							{
								if (action_data.Value == "0")
								{
									Game1.player.mailReceived.Remove(action_data.Key);
								}
								else
								{
									Game1.player.mailReceived.Add(action_data.Key);
								}
							};
							HouseRenovation houseRenovation3 = renovation2;
							houseRenovation3.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation3.onRenovation, action);
						}
					}
					if (valid)
					{
						HouseRenovation houseRenovation4 = renovation2;
						houseRenovation4.onRenovation = (Action<HouseRenovation, int>)Delegate.Combine(houseRenovation4.onRenovation, (Action<HouseRenovation, int>)delegate
						{
							farmhouse.UpdateForRenovation();
						});
						available_renovations.Add(renovation2);
					}
				}
			}
			return available_renovations;
		}

		public static bool EnsureNoObstructions(HouseRenovation renovation, int selected_index)
		{
			if (renovation.location != null)
			{
				foreach (Rectangle rectangle in renovation.renovationBounds[selected_index])
				{
					for (int x = rectangle.Left; x < rectangle.Right; x++)
					{
						for (int y = rectangle.Top; y < rectangle.Bottom; y++)
						{
							if (renovation.location.isTileOccupiedByFarmer(new Vector2(x, y)) != null)
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
								return false;
							}
							if (renovation.location.isTileOccupied(new Vector2(x, y)))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
								return false;
							}
						}
					}
					Rectangle world_box = new Rectangle(rectangle.X * 64, rectangle.Y * 64, rectangle.Width * 64, rectangle.Height * 64);
					DecoratableLocation decoratable_location;
					if ((decoratable_location = (renovation.location as DecoratableLocation)) != null)
					{
						foreach (Furniture item in decoratable_location.furniture)
						{
							if (item.getBoundingBox(item.tileLocation).Intersects(world_box))
							{
								Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:RenovationBlocked"));
								return false;
							}
						}
					}
				}
				return true;
			}
			return false;
		}

		public static void BuildCrib(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				farm_house.cribStyle.Value = 1;
			}
		}

		public static void RemoveCrib(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				farm_house.cribStyle.Value = 0;
			}
		}

		public static void OpenBedroom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Add("renovation_bedroom_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static void CloseBedroom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Remove("renovation_bedroom_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static void OpenSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Add("renovation_southern_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static void CloseSouthernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Remove("renovation_southern_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static void OpenCornernRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Add("renovation_corner_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static void CloseCornerRoom(HouseRenovation renovation, int selected_index)
		{
			FarmHouse farm_house;
			if ((farm_house = (renovation.location as FarmHouse)) != null)
			{
				Game1.player.mailReceived.Remove("renovation_corner_open");
				farm_house.UpdateForRenovation();
			}
		}

		public static bool OnPurchaseRenovation(ISalable salable, Farmer who, int amount)
		{
			HouseRenovation renovation;
			if ((renovation = (salable as HouseRenovation)) != null)
			{
				Game1.activeClickableMenu = new RenovateMenu(renovation);
				return true;
			}
			return false;
		}

		public virtual void AddRenovationBound(Rectangle bound)
		{
			List<Rectangle> bounds = new List<Rectangle>();
			bounds.Add(bound);
			renovationBounds.Add(bounds);
		}

		public virtual void AddRenovationBound(List<Rectangle> bounds)
		{
			renovationBounds.Add(bounds);
		}
	}
}
