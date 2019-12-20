using Netcode;
using System.Collections.Generic;
using System.IO;

namespace StardewValley
{
	public class StartMovieEvent : NetEventArg
	{
		public long uid;

		public List<List<Character>> playerGroups;

		public List<List<Character>> npcGroups;

		public StartMovieEvent()
		{
		}

		public StartMovieEvent(long farmer_uid, List<List<Character>> player_groups, List<List<Character>> npc_groups)
		{
			uid = farmer_uid;
			playerGroups = player_groups;
			npcGroups = npc_groups;
		}

		public void Read(BinaryReader reader)
		{
			uid = reader.ReadInt64();
			playerGroups = ReadCharacterList(reader);
			npcGroups = ReadCharacterList(reader);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(uid);
			WriteCharacterList(writer, playerGroups);
			WriteCharacterList(writer, npcGroups);
		}

		public List<List<Character>> ReadCharacterList(BinaryReader reader)
		{
			List<List<Character>> group_list = new List<List<Character>>();
			int group_list_count = reader.ReadInt32();
			for (int j = 0; j < group_list_count; j++)
			{
				List<Character> group = new List<Character>();
				int group_count = reader.ReadInt32();
				for (int i = 0; i < group_count; i++)
				{
					Character character2 = null;
					character2 = (Character)((reader.ReadInt32() != 1) ? ((object)Game1.getCharacterFromName(reader.ReadString())) : ((object)Game1.getFarmer(reader.ReadInt64())));
					group.Add(character2);
				}
				group_list.Add(group);
			}
			return group_list;
		}

		public void WriteCharacterList(BinaryWriter writer, List<List<Character>> group_list)
		{
			writer.Write(group_list.Count);
			foreach (List<Character> group in group_list)
			{
				writer.Write(group.Count);
				foreach (Character character in group)
				{
					if (character is Farmer)
					{
						writer.Write(1);
						writer.Write((character as Farmer).UniqueMultiplayerID);
					}
					else
					{
						writer.Write(0);
						writer.Write(character.Name);
					}
				}
			}
		}
	}
}
