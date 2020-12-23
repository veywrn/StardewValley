using Netcode;
using System;
using System.Xml.Serialization;

namespace StardewValley
{
	public class BuildingPaintColor : INetObject<NetFields>
	{
		public NetString ColorName = new NetString();

		public NetBool Color1Default = new NetBool(value: true);

		public NetInt Color1Hue = new NetInt();

		public NetInt Color1Saturation = new NetInt();

		public NetInt Color1Lightness = new NetInt();

		public NetBool Color2Default = new NetBool(value: true);

		public NetInt Color2Hue = new NetInt();

		public NetInt Color2Saturation = new NetInt();

		public NetInt Color2Lightness = new NetInt();

		public NetBool Color3Default = new NetBool(value: true);

		public NetInt Color3Hue = new NetInt();

		public NetInt Color3Saturation = new NetInt();

		public NetInt Color3Lightness = new NetInt();

		protected bool _dirty;

		[XmlIgnore]
		public NetFields NetFields
		{
			get;
		} = new NetFields();


		public BuildingPaintColor()
		{
			NetFields.AddFields(ColorName, Color1Default, Color2Default, Color3Default, Color1Hue, Color1Saturation, Color1Lightness, Color2Hue, Color2Saturation, Color2Lightness, Color3Hue, Color3Saturation, Color3Lightness);
			Color1Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
			Color2Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
			Color3Default.fieldChangeVisibleEvent += OnDefaultFlagChanged;
			Color1Hue.fieldChangeVisibleEvent += OnColorChanged;
			Color1Saturation.fieldChangeVisibleEvent += OnColorChanged;
			Color1Lightness.fieldChangeVisibleEvent += OnColorChanged;
			Color2Hue.fieldChangeVisibleEvent += OnColorChanged;
			Color2Saturation.fieldChangeVisibleEvent += OnColorChanged;
			Color2Lightness.fieldChangeVisibleEvent += OnColorChanged;
			Color3Hue.fieldChangeVisibleEvent += OnColorChanged;
			Color3Saturation.fieldChangeVisibleEvent += OnColorChanged;
			Color3Lightness.fieldChangeVisibleEvent += OnColorChanged;
		}

		public virtual void OnDefaultFlagChanged(NetBool field, bool old_value, bool new_value)
		{
			_dirty = true;
		}

		public virtual void OnColorChanged(NetInt field, int old_value, int new_value)
		{
			_dirty = true;
		}

		public virtual void Poll(Action apply)
		{
			if (_dirty)
			{
				apply?.Invoke();
				_dirty = false;
			}
		}

		public bool IsDirty()
		{
			return _dirty;
		}

		public bool RequiresRecolor()
		{
			if (Color1Default.Value && Color2Default.Value)
			{
				return !Color3Default.Value;
			}
			return true;
		}
	}
}
