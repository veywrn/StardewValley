using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using System;
using System.Xml.Serialization;

namespace StardewValley.Objects
{
	public class ItemPedestal : Object
	{
		[XmlIgnore]
		public NetMutex itemModifyMutex = new NetMutex();

		[XmlElement("pedestalType")]
		public NetInt pedestalType = new NetInt(0);

		[XmlElement("requiredItem")]
		public NetRef<Object> requiredItem = new NetRef<Object>();

		[XmlElement("successColor")]
		public NetColor successColor = new NetColor();

		[XmlElement("lockOnSuccess")]
		public NetBool lockOnSuccess = new NetBool();

		[XmlElement("locked")]
		public NetBool locked = new NetBool();

		[XmlElement("match")]
		public NetBool match = new NetBool();

		[XmlIgnore]
		public Texture2D texture;

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(itemModifyMutex.NetFields, pedestalType, requiredItem, successColor, lockOnSuccess, locked, match);
			heldObject.InterpolationWait = false;
		}

		public ItemPedestal()
		{
		}

		public ItemPedestal(Vector2 tile, Object required_item, bool lock_on_success, Color success_color, int pedestal_type = 221)
			: base(tile, 0)
		{
			pedestalType.Value = pedestal_type;
			requiredItem.Value = required_item;
			lockOnSuccess.Value = lock_on_success;
			successColor.Value = success_color;
		}

		public override bool performObjectDropInAction(Item drop_in_item, bool probe, Farmer who)
		{
			if (locked.Value)
			{
				return false;
			}
			if (!drop_in_item.canBeTrashed())
			{
				return false;
			}
			if (heldObject.Value != null)
			{
				DropObject(who);
				return false;
			}
			if (drop_in_item.GetType() == typeof(Object))
			{
				if (probe)
				{
					return true;
				}
				Object placed_object = drop_in_item.getOne() as Object;
				itemModifyMutex.RequestLock(delegate
				{
					who.currentLocation.playSound("woodyStep");
					heldObject.Value = placed_object;
					UpdateItemMatch();
					itemModifyMutex.ReleaseLock();
				}, delegate
				{
					if (placed_object != heldObject.Value)
					{
						Game1.createItemDebris(placed_object, (base.TileLocation + new Vector2(0.5f, 0.5f)) * 64f, -1, who.currentLocation);
					}
				});
				return true;
			}
			return false;
		}

		public virtual void UpdateItemMatch()
		{
			bool success = false;
			if (heldObject.Value != null && requiredItem.Value != null && Utility.getStandardDescriptionFromItem(heldObject.Value, 1) == Utility.getStandardDescriptionFromItem(requiredItem.Value, 1))
			{
				success = true;
			}
			if (success != match.Value)
			{
				match.Value = success;
				if (match.Value && lockOnSuccess.Value)
				{
					locked.Value = true;
				}
			}
		}

		public override bool checkForAction(Farmer who, bool checking_for_activity = false)
		{
			if (locked.Value)
			{
				return false;
			}
			if (checking_for_activity)
			{
				return true;
			}
			if (DropObject(who))
			{
				return true;
			}
			return false;
		}

		public bool DropObject(Farmer who)
		{
			if (heldObject.Value != null)
			{
				itemModifyMutex.RequestLock(delegate
				{
					Object value = heldObject.Value;
					heldObject.Value = null;
					if (who.addItemToInventoryBool(value))
					{
						value.performRemoveAction(tileLocation, who.currentLocation);
						Game1.playSound("coin");
					}
					else
					{
						heldObject.Value = value;
					}
					UpdateItemMatch();
					itemModifyMutex.ReleaseLock();
				}, delegate
				{
				});
				return true;
			}
			return false;
		}

		public override bool performToolAction(Tool t, GameLocation location)
		{
			return false;
		}

		public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
		{
			itemModifyMutex.Update(environment);
		}

		public override bool onExplosion(Farmer who, GameLocation location)
		{
			return false;
		}

		public override void DayUpdate(GameLocation location)
		{
			base.DayUpdate(location);
			itemModifyMutex.ReleaseLock();
		}

		public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
		{
			Vector2 position = new Vector2(x * 64, y * 64);
			b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), Object.getSourceRectForBigCraftable(pedestalType.Value), Color.White, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 2f) / 10000f));
			if (match.Value)
			{
				b.Draw(Game1.bigCraftableSpriteSheet, Game1.GlobalToLocal(Game1.viewport, position), Object.getSourceRectForBigCraftable(pedestalType.Value + 1), successColor.Value, 0f, new Vector2(0f, 16f), 4f, SpriteEffects.None, Math.Max(0f, (position.Y - 1f) / 10000f));
			}
			if (heldObject.Value != null)
			{
				Vector2 draw_position = new Vector2(x, y);
				if (heldObject.Value.bigCraftable.Value)
				{
					draw_position.Y -= 1f;
				}
				heldObject.Value.draw(b, (int)draw_position.X * 64, (int)((draw_position.Y - 0.2f) * 64f) - 64, position.Y / 10000f, 1f);
			}
		}
	}
}
