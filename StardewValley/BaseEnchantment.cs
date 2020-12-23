using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley
{
	[XmlInclude(typeof(BaseEnchantment))]
	[XmlInclude(typeof(BaseWeaponEnchantment))]
	[XmlInclude(typeof(ArtfulEnchantment))]
	[XmlInclude(typeof(BugKillerEnchantment))]
	[XmlInclude(typeof(HaymakerEnchantment))]
	[XmlInclude(typeof(MagicEnchantment))]
	[XmlInclude(typeof(VampiricEnchantment))]
	[XmlInclude(typeof(CrusaderEnchantment))]
	[XmlInclude(typeof(ShearsEnchantment))]
	[XmlInclude(typeof(MilkPailEnchantment))]
	[XmlInclude(typeof(PanEnchantment))]
	[XmlInclude(typeof(WateringCanEnchantment))]
	[XmlInclude(typeof(AxeEnchantment))]
	[XmlInclude(typeof(HoeEnchantment))]
	[XmlInclude(typeof(PickaxeEnchantment))]
	[XmlInclude(typeof(SwiftToolEnchantment))]
	[XmlInclude(typeof(ReachingToolEnchantment))]
	[XmlInclude(typeof(BottomlessEnchantment))]
	[XmlInclude(typeof(ShavingEnchantment))]
	[XmlInclude(typeof(ArchaeologistEnchantment))]
	[XmlInclude(typeof(EfficientToolEnchantment))]
	[XmlInclude(typeof(PowerfulEnchantment))]
	[XmlInclude(typeof(GenerousEnchantment))]
	[XmlInclude(typeof(MasterEnchantment))]
	[XmlInclude(typeof(AutoHookEnchantment))]
	[XmlInclude(typeof(PreservingEnchantment))]
	[XmlInclude(typeof(AmethystEnchantment))]
	[XmlInclude(typeof(TopazEnchantment))]
	[XmlInclude(typeof(AquamarineEnchantment))]
	[XmlInclude(typeof(JadeEnchantment))]
	[XmlInclude(typeof(EmeraldEnchantment))]
	[XmlInclude(typeof(RubyEnchantment))]
	[XmlInclude(typeof(DiamondEnchantment))]
	[XmlInclude(typeof(GalaxySoulEnchantment))]
	public class BaseEnchantment : INetObject<NetFields>
	{
		[XmlIgnore]
		protected string _displayName;

		[XmlIgnore]
		protected bool _applied;

		[XmlIgnore]
		[InstancedStatic]
		public static bool hideEnchantmentName;

		protected static List<BaseEnchantment> _enchantments;

		protected readonly NetInt level = new NetInt(1);

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		[XmlElement("level")]
		public int Level
		{
			get
			{
				return level.Value;
			}
			set
			{
				level.Value = value;
			}
		}

		public BaseEnchantment()
		{
			InitializeNetFields();
		}

		public static BaseEnchantment GetEnchantmentFromItem(Item base_item, Item item)
		{
			if (base_item == null || (base_item is MeleeWeapon && !(base_item as MeleeWeapon).isScythe()))
			{
				if (base_item != null && base_item is MeleeWeapon && (base_item as MeleeWeapon).isGalaxyWeapon() && Utility.IsNormalObjectAtParentSheetIndex(item, 896))
				{
					return new GalaxySoulEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 60))
				{
					return new EmeraldEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 62))
				{
					return new AquamarineEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 64))
				{
					return new RubyEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 66))
				{
					return new AmethystEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 68))
				{
					return new TopazEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 70))
				{
					return new JadeEnchantment();
				}
				if (Utility.IsNormalObjectAtParentSheetIndex(item, 72))
				{
					return new DiamondEnchantment();
				}
			}
			if (Utility.IsNormalObjectAtParentSheetIndex(item, 74))
			{
				Random enchantmentRandom = new Random((int)Game1.stats.getStat("timesEnchanted") + (int)Game1.uniqueIDForThisGame);
				return Utility.GetRandom(GetAvailableEnchantmentsForItem(base_item as Tool), enchantmentRandom);
			}
			return null;
		}

		public static List<BaseEnchantment> GetAvailableEnchantmentsForItem(Tool item)
		{
			List<BaseEnchantment> item_enchantments = new List<BaseEnchantment>();
			if (item == null)
			{
				return GetAvailableEnchantments();
			}
			List<BaseEnchantment> enchantments = GetAvailableEnchantments();
			HashSet<Type> applied_enchantments = new HashSet<Type>();
			foreach (BaseEnchantment enchantment2 in item.enchantments)
			{
				applied_enchantments.Add(enchantment2.GetType());
			}
			foreach (BaseEnchantment enchantment in enchantments)
			{
				if (enchantment.CanApplyTo(item) && !applied_enchantments.Contains(enchantment.GetType()))
				{
					item_enchantments.Add(enchantment);
				}
			}
			return item_enchantments;
		}

		public static List<BaseEnchantment> GetAvailableEnchantments()
		{
			if (_enchantments == null)
			{
				_enchantments = new List<BaseEnchantment>();
				_enchantments.Add(new ArtfulEnchantment());
				_enchantments.Add(new BugKillerEnchantment());
				_enchantments.Add(new VampiricEnchantment());
				_enchantments.Add(new CrusaderEnchantment());
				_enchantments.Add(new HaymakerEnchantment());
				_enchantments.Add(new PowerfulEnchantment());
				_enchantments.Add(new ReachingToolEnchantment());
				_enchantments.Add(new ShavingEnchantment());
				_enchantments.Add(new BottomlessEnchantment());
				_enchantments.Add(new GenerousEnchantment());
				_enchantments.Add(new ArchaeologistEnchantment());
				_enchantments.Add(new MasterEnchantment());
				_enchantments.Add(new AutoHookEnchantment());
				_enchantments.Add(new PreservingEnchantment());
				_enchantments.Add(new EfficientToolEnchantment());
				_enchantments.Add(new SwiftToolEnchantment());
			}
			return _enchantments;
		}

		public virtual bool IsForge()
		{
			return false;
		}

		public virtual bool IsSecondaryEnchantment()
		{
			return false;
		}

		public virtual void InitializeNetFields()
		{
			NetFields.AddFields(level);
		}

		public void OnEquip(Farmer farmer)
		{
			if (!_applied)
			{
				farmer.enchantments.Add(this);
				_applied = true;
				_OnEquip(farmer);
			}
		}

		public void OnUnequip(Farmer farmer)
		{
			if (_applied)
			{
				farmer.enchantments.Remove(this);
				_applied = false;
				_OnUnequip(farmer);
			}
		}

		protected virtual void _OnEquip(Farmer who)
		{
		}

		protected virtual void _OnUnequip(Farmer who)
		{
		}

		public void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
			_OnDealDamage(monster, location, who, ref amount);
		}

		public void OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
			_OnDealDamage(monster, location, who, ref amount);
		}

		protected virtual void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
		{
		}

		public void OnMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
			_OnMonsterSlay(m, location, who);
		}

		protected virtual void _OnMonsterSlay(Monster m, GameLocation location, Farmer who)
		{
		}

		public void OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
			_OnCutWeed(tile_location, location, who);
		}

		protected virtual void _OnCutWeed(Vector2 tile_location, GameLocation location, Farmer who)
		{
		}

		public virtual BaseEnchantment GetOne()
		{
			BaseEnchantment obj = Activator.CreateInstance(GetType()) as BaseEnchantment;
			obj.level.Value = level.Value;
			return obj;
		}

		public int GetLevel()
		{
			return level.Value;
		}

		public void SetLevel(Item item, int new_level)
		{
			if (new_level < 1)
			{
				new_level = 1;
			}
			else if (GetMaximumLevel() >= 0 && new_level > GetMaximumLevel())
			{
				new_level = GetMaximumLevel();
			}
			if (level.Value != new_level)
			{
				UnapplyTo(item);
				level.Value = new_level;
				ApplyTo(item);
			}
		}

		public virtual int GetMaximumLevel()
		{
			return -1;
		}

		public void ApplyTo(Item item, Farmer farmer = null)
		{
			_ApplyTo(item);
			if (IsItemCurrentlyEquipped(item, farmer))
			{
				OnEquip(farmer);
			}
		}

		protected virtual void _ApplyTo(Item item)
		{
		}

		public bool IsItemCurrentlyEquipped(Item item, Farmer farmer)
		{
			if (farmer == null)
			{
				return false;
			}
			return _IsCurrentlyEquipped(item, farmer);
		}

		protected virtual bool _IsCurrentlyEquipped(Item item, Farmer farmer)
		{
			return farmer.CurrentTool == item;
		}

		public void UnapplyTo(Item item, Farmer farmer = null)
		{
			_UnapplyTo(item);
			if (IsItemCurrentlyEquipped(item, farmer))
			{
				OnUnequip(farmer);
			}
		}

		protected virtual void _UnapplyTo(Item item)
		{
		}

		public virtual bool CanApplyTo(Item item)
		{
			return true;
		}

		public string GetDisplayName()
		{
			if (_displayName == null)
			{
				_displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\EnchantmentNames:" + GetName());
				if (_displayName == null)
				{
					_displayName = GetName();
				}
			}
			return _displayName;
		}

		public virtual string GetName()
		{
			return "Unknown Enchantment";
		}

		public virtual bool ShouldBeDisplayed()
		{
			return true;
		}
	}
}
