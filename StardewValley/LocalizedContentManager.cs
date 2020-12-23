using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace StardewValley
{
	public class LocalizedContentManager : ContentManager
	{
		public delegate void LanguageChangedHandler(LanguageCode code);

		public enum LanguageCode
		{
			en,
			ja,
			ru,
			zh,
			pt,
			es,
			de,
			th,
			fr,
			ko,
			it,
			tr,
			hu
		}

		public const string genderDialogueSplitCharacter = "¦";

		private static LanguageCode _currentLangCode = GetDefaultLanguageCode();

		public CultureInfo CurrentCulture;

		public static readonly Dictionary<string, string> localizedAssetNames = new Dictionary<string, string>();

		public static LanguageCode CurrentLanguageCode
		{
			get
			{
				return _currentLangCode;
			}
			set
			{
				if (_currentLangCode != value)
				{
					LanguageCode prev = _currentLangCode;
					_currentLangCode = value;
					Console.WriteLine("LocalizedContentManager.CurrentLanguageCode CHANGING from '{0}' to '{1}'", prev, _currentLangCode);
					LocalizedContentManager.OnLanguageChange?.Invoke(_currentLangCode);
					Console.WriteLine("LocalizedContentManager.CurrentLanguageCode CHANGED from '{0}' to '{1}'", prev, _currentLangCode);
				}
			}
		}

		public static bool CurrentLanguageLatin
		{
			get
			{
				if (CurrentLanguageCode != 0 && CurrentLanguageCode != LanguageCode.es && CurrentLanguageCode != LanguageCode.de && CurrentLanguageCode != LanguageCode.pt && CurrentLanguageCode != LanguageCode.fr && CurrentLanguageCode != LanguageCode.it && CurrentLanguageCode != LanguageCode.tr)
				{
					return CurrentLanguageCode == LanguageCode.hu;
				}
				return true;
			}
		}

		public static event LanguageChangedHandler OnLanguageChange;

		private static LanguageCode GetDefaultLanguageCode()
		{
			return LanguageCode.en;
		}

		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory, CultureInfo currentCulture)
			: base(serviceProvider, rootDirectory)
		{
			CurrentCulture = currentCulture;
		}

		public LocalizedContentManager(IServiceProvider serviceProvider, string rootDirectory)
			: this(serviceProvider, rootDirectory, Thread.CurrentThread.CurrentUICulture)
		{
		}

		public virtual T LoadBase<T>(string assetName)
		{
			return base.Load<T>(assetName);
		}

		public override T Load<T>(string assetName)
		{
			return Load<T>(assetName, CurrentLanguageCode);
		}

		public virtual T Load<T>(string assetName, LanguageCode language)
		{
			if (language != 0)
			{
				if (!localizedAssetNames.TryGetValue(assetName, out string _))
				{
					string localizedAssetName2 = assetName + "." + LanguageCodeString(language);
					try
					{
						base.Load<T>(localizedAssetName2);
						localizedAssetNames[assetName] = localizedAssetName2;
					}
					catch (ContentLoadException)
					{
						localizedAssetName2 = assetName + "_international";
						try
						{
							base.Load<T>(localizedAssetName2);
							localizedAssetNames[assetName] = localizedAssetName2;
						}
						catch (ContentLoadException)
						{
							localizedAssetNames[assetName] = assetName;
						}
					}
				}
				return base.Load<T>(localizedAssetNames[assetName]);
			}
			return base.Load<T>(assetName);
		}

		public string LanguageCodeString(LanguageCode code)
		{
			string retVal = "";
			switch (code)
			{
			case LanguageCode.ja:
				retVal = "ja-JP";
				break;
			case LanguageCode.ru:
				retVal = "ru-RU";
				break;
			case LanguageCode.zh:
				retVal = "zh-CN";
				break;
			case LanguageCode.pt:
				retVal = "pt-BR";
				break;
			case LanguageCode.es:
				retVal = "es-ES";
				break;
			case LanguageCode.de:
				retVal = "de-DE";
				break;
			case LanguageCode.th:
				retVal = "th-TH";
				break;
			case LanguageCode.fr:
				retVal = "fr-FR";
				break;
			case LanguageCode.ko:
				retVal = "ko-KR";
				break;
			case LanguageCode.it:
				retVal = "it-IT";
				break;
			case LanguageCode.tr:
				retVal = "tr-TR";
				break;
			case LanguageCode.hu:
				retVal = "hu-HU";
				break;
			}
			return retVal;
		}

		public LanguageCode GetCurrentLanguage()
		{
			return CurrentLanguageCode;
		}

		private string GetString(Dictionary<string, string> strings, string key)
		{
			if (strings.TryGetValue(key + ".desktop", out string result))
			{
				return result;
			}
			return strings[key];
		}

		public virtual string LoadStringReturnNullIfNotFound(string path)
		{
			string result = LoadString(path);
			if (!result.Equals(path))
			{
				return result;
			}
			return null;
		}

		public virtual string LoadString(string path)
		{
			parseStringPath(path, out string assetName, out string key);
			Dictionary<string, string> strings = Load<Dictionary<string, string>>(assetName);
			if (strings != null && strings.ContainsKey(key))
			{
				string sentence = GetString(strings, key);
				if (sentence.Contains("¦"))
				{
					sentence = ((!Game1.player.IsMale) ? sentence.Substring(sentence.IndexOf("¦") + 1) : sentence.Substring(0, sentence.IndexOf("¦")));
				}
				return sentence;
			}
			return LoadBaseString(path);
		}

		public virtual string LoadString(string path, object sub1)
		{
			string sentence = LoadString(path);
			try
			{
				return string.Format(sentence, sub1);
			}
			catch (Exception)
			{
				return sentence;
			}
		}

		public virtual string LoadString(string path, object sub1, object sub2)
		{
			string sentence = LoadString(path);
			try
			{
				return string.Format(sentence, sub1, sub2);
			}
			catch (Exception)
			{
				return sentence;
			}
		}

		public virtual string LoadString(string path, object sub1, object sub2, object sub3)
		{
			string sentence = LoadString(path);
			try
			{
				return string.Format(sentence, sub1, sub2, sub3);
			}
			catch (Exception)
			{
				return sentence;
			}
		}

		public virtual string LoadString(string path, params object[] substitutions)
		{
			string sentence = LoadString(path);
			if (substitutions.Length != 0)
			{
				try
				{
					return string.Format(sentence, substitutions);
				}
				catch (Exception)
				{
					return sentence;
				}
			}
			return sentence;
		}

		public virtual string LoadBaseString(string path)
		{
			parseStringPath(path, out string assetName, out string key);
			Dictionary<string, string> strings = base.Load<Dictionary<string, string>>(assetName);
			if (strings != null && strings.ContainsKey(key))
			{
				return GetString(strings, key);
			}
			return path;
		}

		private void parseStringPath(string path, out string assetName, out string key)
		{
			int i = path.IndexOf(':');
			if (i == -1)
			{
				throw new ContentLoadException("Unable to parse string path: " + path);
			}
			assetName = path.Substring(0, i);
			key = path.Substring(i + 1, path.Length - i - 1);
		}

		public virtual LocalizedContentManager CreateTemporary()
		{
			return new LocalizedContentManager(base.ServiceProvider, base.RootDirectory, CurrentCulture);
		}
	}
}
