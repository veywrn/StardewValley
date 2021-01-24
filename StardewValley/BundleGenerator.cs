using Microsoft.Xna.Framework;
using StardewValley.GameData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StardewValley
{
	public class BundleGenerator
	{
		public List<RandomBundleData> randomBundleData;

		public Dictionary<string, string> bundleData;

		public Dictionary<string, int> itemNameLookup;

		public Random random;

		public Dictionary<string, string> Generate(string bundle_data_path, Random rng)
		{
			random = rng;
			randomBundleData = Game1.content.Load<List<RandomBundleData>>(bundle_data_path);
			bundleData = new Dictionary<string, string>();
			Dictionary<string, string> base_data = Game1.content.LoadBase<Dictionary<string, string>>("Data\\Bundles");
			foreach (string key2 in base_data.Keys)
			{
				bundleData[key2] = base_data[key2];
			}
			foreach (RandomBundleData area_data in randomBundleData)
			{
				List<int> index_lookups = new List<int>();
				string[] array = area_data.Keys.Trim().Split(' ');
				Dictionary<int, BundleData> selected_bundles = new Dictionary<int, BundleData>();
				string[] array2 = array;
				foreach (string index_string in array2)
				{
					index_lookups.Add(int.Parse(index_string));
				}
				BundleSetData bundle_set = Utility.GetRandom(area_data.BundleSets, random);
				if (bundle_set != null)
				{
					foreach (BundleData bundle_data4 in bundle_set.Bundles)
					{
						selected_bundles[bundle_data4.Index] = bundle_data4;
					}
				}
				List<BundleData> random_bundle_pool = new List<BundleData>();
				foreach (BundleData bundle_data3 in area_data.Bundles)
				{
					random_bundle_pool.Add(bundle_data3);
				}
				for (int i = 0; i < index_lookups.Count; i++)
				{
					if (!selected_bundles.ContainsKey(i))
					{
						List<BundleData> index_bundles = new List<BundleData>();
						foreach (BundleData bundle_data2 in random_bundle_pool)
						{
							if (bundle_data2.Index == i)
							{
								index_bundles.Add(bundle_data2);
							}
						}
						if (index_bundles.Count > 0)
						{
							BundleData selected_bundle2 = Utility.GetRandom(index_bundles, random);
							random_bundle_pool.Remove(selected_bundle2);
							selected_bundles[i] = selected_bundle2;
						}
						else
						{
							foreach (BundleData bundle_data in random_bundle_pool)
							{
								if (bundle_data.Index == -1)
								{
									index_bundles.Add(bundle_data);
								}
							}
							if (index_bundles.Count > 0)
							{
								BundleData selected_bundle = Utility.GetRandom(index_bundles, random);
								random_bundle_pool.Remove(selected_bundle);
								selected_bundles[i] = selected_bundle;
							}
						}
					}
				}
				foreach (int key in selected_bundles.Keys)
				{
					BundleData data = selected_bundles[key];
					StringBuilder string_data = new StringBuilder();
					string_data.Append(data.Name);
					string_data.Append("/");
					string reward_string = data.Reward;
					if (reward_string.Length > 0)
					{
						try
						{
							if (char.IsDigit(reward_string[0]))
							{
								string[] reward_split = reward_string.Split(' ');
								int count = int.Parse(reward_split[0]);
								Item reward = Utility.fuzzyItemSearch(string.Join(" ", reward_split, 1, reward_split.Length - 1), count);
								if (reward != null)
								{
									reward_string = Utility.getStandardDescriptionFromItem(reward, reward.Stack);
								}
							}
						}
						catch (Exception)
						{
							Console.WriteLine("ERROR: Malformed reward string in bundle: " + reward_string);
							reward_string = data.Reward;
						}
					}
					string_data.Append(reward_string);
					string_data.Append("/");
					int color = 0;
					if (data.Color == "Red")
					{
						color = 4;
					}
					else if (data.Color == "Blue")
					{
						color = 5;
					}
					else if (data.Color == "Green")
					{
						color = 0;
					}
					else if (data.Color == "Orange")
					{
						color = 2;
					}
					else if (data.Color == "Purple")
					{
						color = 1;
					}
					else if (data.Color == "Teal")
					{
						color = 6;
					}
					else if (data.Color == "Yellow")
					{
						color = 3;
					}
					ParseItemList(string_data, data.Items, data.Pick, data.RequiredItems, color);
					string_data.Append("/");
					string_data.Append(data.Sprite);
					bundleData[area_data.AreaName + "/" + index_lookups[key]] = string_data.ToString();
				}
			}
			return bundleData;
		}

		public string ParseRandomTags(string data)
		{
			int open_index2 = 0;
			do
			{
				open_index2 = data.LastIndexOf('[');
				if (open_index2 >= 0)
				{
					int close_index = data.IndexOf(']', open_index2);
					if (close_index == -1)
					{
						return data;
					}
					string value = Utility.GetRandom(new List<string>(data.Substring(open_index2 + 1, close_index - open_index2 - 1).Split('|')), random);
					data = data.Remove(open_index2, close_index - open_index2 + 1);
					data = data.Insert(open_index2, value);
				}
			}
			while (open_index2 >= 0);
			return data;
		}

		public Item ParseItemString(string item_string)
		{
			string[] parts = item_string.Trim().Split(' ');
			int index2 = 0;
			int count = int.Parse(parts[index2]);
			index2++;
			int quality = 0;
			if (parts[index2] == "NQ")
			{
				quality = 0;
				index2++;
			}
			else if (parts[index2] == "SQ")
			{
				quality = 1;
				index2++;
			}
			else if (parts[index2] == "GQ")
			{
				quality = 2;
				index2++;
			}
			else if (parts[index2] == "IQ")
			{
				quality = 3;
				index2++;
			}
			string item_name = string.Join(" ", parts, index2, parts.Length - index2);
			if (char.IsDigit(item_name[0]))
			{
				Object @object = new Object(int.Parse(item_name), count);
				(@object as Object).Quality = quality;
				return @object;
			}
			Item found_item = null;
			if (item_name.ToLowerInvariant().EndsWith("category"))
			{
				try
				{
					FieldInfo field = typeof(Object).GetField(item_name);
					if (field != null)
					{
						int category_index = (int)field.GetValue(null);
						found_item = new Object(Vector2.Zero, category_index, 1);
					}
				}
				catch (Exception)
				{
				}
			}
			if (found_item == null)
			{
				found_item = Utility.fuzzyItemSearch(item_name);
				if (found_item is Object)
				{
					(found_item as Object).Quality = quality;
				}
			}
			if (found_item == null)
			{
				throw new Exception("Invalid item name '" + item_name + "' encountered while generating a bundle.");
			}
			found_item.Stack = count;
			return found_item;
		}

		public void ParseItemList(StringBuilder builder, string item_list, int pick_count, int required_items, int color)
		{
			item_list = ParseRandomTags(item_list);
			string[] items = item_list.Split(',');
			List<string> item_strings = new List<string>();
			for (int j = 0; j < items.Length; j++)
			{
				Item item = ParseItemString(items[j]);
				item_strings.Add(item.ParentSheetIndex + " " + item.Stack + " " + (item as Object).Quality);
			}
			if (pick_count < 0)
			{
				pick_count = item_strings.Count;
			}
			if (required_items < 0)
			{
				required_items = pick_count;
			}
			while (item_strings.Count > pick_count)
			{
				int index_to_remove = random.Next(item_strings.Count);
				item_strings.RemoveAt(index_to_remove);
			}
			for (int i = 0; i < item_strings.Count; i++)
			{
				builder.Append(item_strings[i]);
				if (i < item_strings.Count - 1)
				{
					builder.Append(" ");
				}
			}
			builder.Append("/");
			builder.Append(color);
			builder.Append("/");
			builder.Append(required_items);
		}
	}
}
