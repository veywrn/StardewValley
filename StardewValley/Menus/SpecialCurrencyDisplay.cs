using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	public class SpecialCurrencyDisplay
	{
		public class CurrencyDisplayType
		{
			public string key;

			public NetIntDelta netIntDelta;

			public Action<int> playSound;

			public Action<SpriteBatch, Vector2> drawSprite;
		}

		protected MoneyDial _moneyDial;

		protected float currentPosition;

		protected CurrencyDisplayType _currentDisplayedCurrency;

		public Dictionary<string, CurrencyDisplayType> registeredCurrencyDisplays;

		public float timeToLive;

		public Action<SpriteBatch, Vector2> drawSprite;

		public CurrencyDisplayType forcedOnscreenCurrencyType;

		public SpecialCurrencyDisplay()
		{
			_moneyDial = new MoneyDial(3);
			_moneyDial.onPlaySound = null;
			drawSprite = null;
			registeredCurrencyDisplays = new Dictionary<string, CurrencyDisplayType>();
		}

		public virtual void Register(string key, NetIntDelta net_int_delta, Action<int> sound_function = null, Action<SpriteBatch, Vector2> draw_function = null)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				Unregister(key);
			}
			CurrencyDisplayType currency_type = new CurrencyDisplayType();
			currency_type.key = key;
			currency_type.netIntDelta = net_int_delta;
			currency_type.playSound = sound_function;
			currency_type.drawSprite = draw_function;
			registeredCurrencyDisplays[key] = currency_type;
			registeredCurrencyDisplays[key].netIntDelta.fieldChangeVisibleEvent += OnCurrencyChange;
		}

		public virtual void ShowCurrency(string currency_type)
		{
			if (currency_type == null || !registeredCurrencyDisplays.ContainsKey(currency_type))
			{
				forcedOnscreenCurrencyType = null;
				return;
			}
			forcedOnscreenCurrencyType = registeredCurrencyDisplays[currency_type];
			SetDisplayedCurrency(forcedOnscreenCurrencyType);
		}

		public virtual void OnCurrencyChange(NetIntDelta field, int old_value, int new_value)
		{
			if (Game1.gameMode != 3)
			{
				return;
			}
			string currency_key = null;
			foreach (string key in registeredCurrencyDisplays.Keys)
			{
				if (registeredCurrencyDisplays[key].netIntDelta == field)
				{
					currency_key = key;
					break;
				}
			}
			if (currency_key == null)
			{
				return;
			}
			SetDisplayedCurrency(currency_key);
			if (_currentDisplayedCurrency != null)
			{
				_moneyDial.currentValue = old_value;
				if (_moneyDial.onPlaySound != null)
				{
					_moneyDial.onPlaySound(new_value - old_value);
				}
			}
			timeToLive = 5f;
		}

		public virtual void SetDisplayedCurrency(CurrencyDisplayType currency_type)
		{
			if (currency_type == _currentDisplayedCurrency || (forcedOnscreenCurrencyType != null && forcedOnscreenCurrencyType != currency_type))
			{
				return;
			}
			_moneyDial.onPlaySound = null;
			drawSprite = null;
			_currentDisplayedCurrency = currency_type;
			if (currency_type != null)
			{
				_moneyDial.currentValue = _currentDisplayedCurrency.netIntDelta.Value;
				_moneyDial.previousTargetValue = _moneyDial.currentValue;
				if (currency_type.playSound != null)
				{
					_moneyDial.onPlaySound = currency_type.playSound;
				}
				else
				{
					_moneyDial.onPlaySound = DefaultPlaySound;
				}
				if (currency_type.drawSprite != null)
				{
					drawSprite = currency_type.drawSprite;
				}
				else
				{
					drawSprite = DefaultDrawSprite;
				}
			}
		}

		public virtual void SetDisplayedCurrency(string key)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				CurrencyDisplayType currency_type = registeredCurrencyDisplays[key];
				SetDisplayedCurrency(currency_type);
			}
		}

		public virtual void Unregister(string key)
		{
			if (registeredCurrencyDisplays.ContainsKey(key))
			{
				if (_currentDisplayedCurrency == registeredCurrencyDisplays[key])
				{
					SetDisplayedCurrency((CurrencyDisplayType)null);
				}
				registeredCurrencyDisplays[key].netIntDelta.fieldChangeVisibleEvent -= OnCurrencyChange;
				registeredCurrencyDisplays.Remove(key);
			}
		}

		public virtual void Cleanup()
		{
			foreach (string key in new List<string>(registeredCurrencyDisplays.Keys))
			{
				Unregister(key);
			}
		}

		public virtual void DefaultDrawSprite(SpriteBatch b, Vector2 position)
		{
			if (_currentDisplayedCurrency != null)
			{
				if (_currentDisplayedCurrency.key == "walnuts")
				{
					b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
				else if (_currentDisplayedCurrency.key == "qiGems")
				{
					b.Draw(Game1.objectSpriteSheet, position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 858, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
			}
		}

		public virtual void DefaultPlaySound(int direction)
		{
			if (_currentDisplayedCurrency != null)
			{
				if (direction < 0 && _currentDisplayedCurrency.key == "walnuts")
				{
					Game1.playSound("goldenWalnut");
				}
				if (direction > 0 && _currentDisplayedCurrency.key == "walnuts")
				{
					Game1.playSound("goldenWalnut");
				}
			}
		}

		public virtual void Update(GameTime time)
		{
			if (timeToLive > 0f)
			{
				timeToLive -= (float)time.ElapsedGameTime.TotalSeconds;
				if (timeToLive < 0f)
				{
					timeToLive = 0f;
				}
			}
			if (timeToLive > 0f || forcedOnscreenCurrencyType != null)
			{
				currentPosition += (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
			}
			else
			{
				currentPosition -= (float)time.ElapsedGameTime.TotalSeconds / 0.5f;
			}
			currentPosition = Utility.Clamp(currentPosition, 0f, 1f);
		}

		public Vector2 GetUpperLeft()
		{
			return new Vector2(16f, (int)Utility.Lerp(-26f, 0f, currentPosition) * 4);
		}

		public virtual void Draw(SpriteBatch b)
		{
			if (_currentDisplayedCurrency != null && !(currentPosition <= 0f))
			{
				Vector2 draw_position = GetUpperLeft();
				b.Draw(Game1.mouseCursors2, draw_position, new Rectangle(48, 176, 52, 26), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				int displayed_value = _currentDisplayedCurrency.netIntDelta.Value;
				if (currentPosition < 0.5f)
				{
					displayed_value = _moneyDial.previousTargetValue;
				}
				_moneyDial.draw(b, draw_position + new Vector2(108f, 40f), displayed_value);
				if (drawSprite != null)
				{
					drawSprite(b, draw_position + new Vector2(4f, 6f) * 4f);
				}
			}
		}
	}
}
