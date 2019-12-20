using Microsoft.Xna.Framework;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StardewValley.Events
{
	public class MovieTheaterScreeningEvent
	{
		public int currentResponse;

		public List<List<Character>> playerAndGuestAudienceGroups;

		public Dictionary<int, Character> _responseOrder = new Dictionary<int, Character>();

		protected Dictionary<Character, Character> _whiteListDependencyLookup;

		protected Dictionary<Character, string> _characterResponses;

		public MovieData movieData;

		protected List<Farmer> _farmers;

		protected Dictionary<Character, MovieConcession> _concessionsData;

		public Event getMovieEvent(string movieID, List<List<Character>> player_and_guest_audience_groups, List<List<Character>> npcOnlyAudienceGroups, Dictionary<Character, MovieConcession> concessions_data = null)
		{
			_concessionsData = concessions_data;
			_responseOrder = new Dictionary<int, Character>();
			_whiteListDependencyLookup = new Dictionary<Character, Character>();
			_characterResponses = new Dictionary<Character, string>();
			movieData = MovieTheater.GetMovieData()[movieID];
			playerAndGuestAudienceGroups = player_and_guest_audience_groups;
			currentResponse = 0;
			StringBuilder sb = new StringBuilder();
			Random theaterRandom = new Random((int)(Game1.stats.DaysPlayed + Game1.uniqueIDForThisGame / 2uL));
			sb.Append("movieScreenAmbience/-2000 -2000/");
			string playerCharacterEventName = "farmer" + Utility.getFarmerNumberFromFarmer(Game1.player);
			string playerCharacterGuestName = "";
			foreach (List<Character> list in playerAndGuestAudienceGroups)
			{
				if (list.Contains(Game1.player))
				{
					for (int i9 = 0; i9 < list.Count; i9++)
					{
						if (!(list[i9] is Farmer))
						{
							playerCharacterGuestName = list[i9].name;
						}
					}
				}
			}
			_farmers = new List<Farmer>();
			foreach (List<Character> playerAndGuestAudienceGroup in playerAndGuestAudienceGroups)
			{
				foreach (Character character3 in playerAndGuestAudienceGroup)
				{
					if (character3 is Farmer && !_farmers.Contains(character3))
					{
						_farmers.Add(character3 as Farmer);
					}
				}
			}
			List<Character> allAudience = playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).ToList();
			allAudience.AddRange(npcOnlyAudienceGroups.SelectMany((List<Character> x) => x).ToList());
			bool first = true;
			foreach (Character c2 in allAudience)
			{
				if (c2 != null)
				{
					if (!first)
					{
						sb.Append(" ");
					}
					if (c2 is Farmer)
					{
						Farmer f = c2 as Farmer;
						sb.Append("farmer" + Utility.getFarmerNumberFromFarmer(f));
					}
					else if ((string)c2.name == "Krobus")
					{
						sb.Append("Krobus_Trenchcoat");
					}
					else
					{
						sb.Append(c2.name);
					}
					sb.Append(" -1000 -1000 0");
					first = false;
				}
			}
			sb.Append("/changeToTemporaryMap MovieTheaterScreen false/specificTemporarySprite movieTheater_setup/ambientLight 0 0 0/");
			string[] backRow = new string[8];
			playerAndGuestAudienceGroups = playerAndGuestAudienceGroups.OrderBy((List<Character> x) => theaterRandom.Next()).ToList();
			int startingSeat = theaterRandom.Next(8 - playerAndGuestAudienceGroups.SelectMany((List<Character> x) => x).Count() + 1);
			int whichGroup = 0;
			for (int i8 = 0; i8 < 8; i8++)
			{
				int seat8 = (i8 + startingSeat) % 8;
				if (playerAndGuestAudienceGroups[whichGroup].Count == 2 && (seat8 == 3 || seat8 == 7))
				{
					i8++;
					seat8++;
					seat8 %= 8;
				}
				for (int j3 = 0; j3 < playerAndGuestAudienceGroups[whichGroup].Count && seat8 + j3 < backRow.Length; j3++)
				{
					backRow[seat8 + j3] = ((playerAndGuestAudienceGroups[whichGroup][j3] is Farmer) ? ("farmer" + Utility.getFarmerNumberFromFarmer(playerAndGuestAudienceGroups[whichGroup][j3] as Farmer)) : ((string)playerAndGuestAudienceGroups[whichGroup][j3].name));
					if (j3 > 0)
					{
						i8++;
					}
				}
				whichGroup++;
				if (whichGroup >= playerAndGuestAudienceGroups.Count)
				{
					break;
				}
			}
			string[] midRow = new string[6];
			for (int j2 = 0; j2 < npcOnlyAudienceGroups.Count; j2++)
			{
				int seat = theaterRandom.Next(3 - npcOnlyAudienceGroups[j2].Count + 1) + j2 * 3;
				for (int i = 0; i < npcOnlyAudienceGroups[j2].Count; i++)
				{
					midRow[seat + i] = npcOnlyAudienceGroups[j2][i].name;
				}
			}
			int soFar4 = 0;
			int sittingTogetherCount2 = 0;
			for (int i7 = 0; i7 < backRow.Count(); i7++)
			{
				if (backRow[i7] == null || !(backRow[i7] != "") || !(backRow[i7] != playerCharacterEventName) || !(backRow[i7] != playerCharacterGuestName))
				{
					continue;
				}
				soFar4++;
				if (soFar4 < 2)
				{
					continue;
				}
				sittingTogetherCount2++;
				Point seat2 = getBackRowSeatTileFromIndex(i7);
				sb.Append("warp ").Append(backRow[i7]).Append(" ")
					.Append(seat2.X)
					.Append(" ")
					.Append(seat2.Y)
					.Append("/positionOffset ")
					.Append(backRow[i7])
					.Append(" 0 -10/");
				if (sittingTogetherCount2 == 2)
				{
					sittingTogetherCount2 = 0;
					if (theaterRandom.NextDouble() < 0.5 && backRow[i7] != playerCharacterGuestName && backRow[i7 - 1] != playerCharacterGuestName)
					{
						sb.Append("faceDirection " + backRow[i7] + " 3 true/");
						sb.Append("faceDirection " + backRow[i7 - 1] + " 1 true/");
					}
				}
			}
			soFar4 = 0;
			sittingTogetherCount2 = 0;
			for (int i6 = 0; i6 < midRow.Count(); i6++)
			{
				if (midRow[i6] == null || !(midRow[i6] != ""))
				{
					continue;
				}
				soFar4++;
				if (soFar4 < 2)
				{
					continue;
				}
				sittingTogetherCount2++;
				Point seat3 = getMidRowSeatTileFromIndex(i6);
				sb.Append("warp ").Append(midRow[i6]).Append(" ")
					.Append(seat3.X)
					.Append(" ")
					.Append(seat3.Y)
					.Append("/positionOffset ")
					.Append(midRow[i6])
					.Append(" 0 -10/");
				if (sittingTogetherCount2 == 2)
				{
					sittingTogetherCount2 = 0;
					if (i6 != 3 && theaterRandom.NextDouble() < 0.5)
					{
						sb.Append("faceDirection " + midRow[i6] + " 3 true/");
						sb.Append("faceDirection " + midRow[i6 - 1] + " 1 true/");
					}
				}
			}
			Point warpPoint = new Point(1, 15);
			soFar4 = 0;
			for (int i5 = 0; i5 < backRow.Count(); i5++)
			{
				if (backRow[i5] != null && backRow[i5] != "" && backRow[i5] != playerCharacterEventName && backRow[i5] != playerCharacterGuestName)
				{
					Point seat6 = getBackRowSeatTileFromIndex(i5);
					if (soFar4 == 1)
					{
						sb.Append("warp ").Append(backRow[i5]).Append(" ")
							.Append(seat6.X - 1)
							.Append(" 10")
							.Append("/advancedMove ")
							.Append(backRow[i5])
							.Append(" false 1 " + 200 + " 1 0 4 1000/")
							.Append("positionOffset ")
							.Append(backRow[i5])
							.Append(" 0 -10/");
					}
					else
					{
						sb.Append("warp ").Append(backRow[i5]).Append(" 1 12")
							.Append("/advancedMove ")
							.Append(backRow[i5])
							.Append(" false 1 200 ")
							.Append("0 -2 ")
							.Append(seat6.X - 1)
							.Append(" 0 4 1000/")
							.Append("positionOffset ")
							.Append(backRow[i5])
							.Append(" 0 -10/");
					}
					soFar4++;
				}
				if (soFar4 >= 2)
				{
					break;
				}
			}
			soFar4 = 0;
			for (int i4 = 0; i4 < midRow.Count(); i4++)
			{
				if (midRow[i4] != null && midRow[i4] != "")
				{
					Point seat5 = getMidRowSeatTileFromIndex(i4);
					if (soFar4 == 1)
					{
						sb.Append("warp ").Append(midRow[i4]).Append(" ")
							.Append(seat5.X - 1)
							.Append(" 8")
							.Append("/advancedMove ")
							.Append(midRow[i4])
							.Append(" false 1 " + 400 + " 1 0 4 1000/");
					}
					else
					{
						sb.Append("warp ").Append(midRow[i4]).Append(" 2 9")
							.Append("/advancedMove ")
							.Append(midRow[i4])
							.Append(" false 1 300 ")
							.Append("0 -1 ")
							.Append(seat5.X - 2)
							.Append(" 0 4 1000/");
					}
					soFar4++;
				}
				if (soFar4 >= 2)
				{
					break;
				}
			}
			sb.Append("viewport 6 8 true/pause 500/");
			for (int i3 = 0; i3 < backRow.Count(); i3++)
			{
				if (backRow[i3] != null && backRow[i3] != "")
				{
					Point seat4 = getBackRowSeatTileFromIndex(i3);
					if (backRow[i3] == playerCharacterEventName || backRow[i3] == playerCharacterGuestName)
					{
						sb.Append("warp ").Append(backRow[i3]).Append(" ")
							.Append(warpPoint.X)
							.Append(" ")
							.Append(warpPoint.Y)
							.Append("/advancedMove ")
							.Append(backRow[i3])
							.Append(" false 0 -5 ")
							.Append(seat4.X - warpPoint.X)
							.Append(" 0 4 1000/")
							.Append("pause ")
							.Append(1000)
							.Append("/");
					}
				}
			}
			sb.Append("pause 3000/proceedPosition ").Append(playerCharacterGuestName).Append("/pause 1000");
			if (playerCharacterGuestName.Equals(""))
			{
				sb.Append("/proceedPosition farmer");
			}
			sb.Append("/waitForAllStationary/pause 100");
			foreach (Character c in allAudience)
			{
				if (getEventName(c) != playerCharacterEventName && getEventName(c) != playerCharacterGuestName)
				{
					if (c is Farmer)
					{
						sb.Append("/faceDirection ").Append(getEventName(c)).Append(" 0 true/positionOffset ")
							.Append(getEventName(c))
							.Append(" 0 42 true");
					}
					else
					{
						sb.Append("/faceDirection ").Append(getEventName(c)).Append(" 0 true/positionOffset ")
							.Append(getEventName(c))
							.Append(" 0 12 true");
					}
					if (theaterRandom.NextDouble() < 0.2)
					{
						sb.Append("/pause 100");
					}
				}
			}
			sb.Append("/positionOffset ").Append(playerCharacterEventName).Append(" 0 32/positionOffset ")
				.Append(playerCharacterGuestName)
				.Append(" 0 8/ambientLight 210 210 120 true/pause 500/viewport move 0 -1 4000/pause 5000");
			List<Character> responding_characters2 = new List<Character>();
			foreach (List<Character> playerAndGuestAudienceGroup2 in playerAndGuestAudienceGroups)
			{
				foreach (Character character2 in playerAndGuestAudienceGroup2)
				{
					if (!(character2 is Farmer) && !responding_characters2.Contains(character2))
					{
						responding_characters2.Add(character2);
					}
				}
			}
			for (int i2 = 0; i2 < responding_characters2.Count; i2++)
			{
				int index = theaterRandom.Next(responding_characters2.Count);
				Character character = responding_characters2[i2];
				responding_characters2[i2] = responding_characters2[index];
				responding_characters2[index] = character;
			}
			int current_response_index2 = 0;
			foreach (MovieScene scene2 in movieData.Scenes)
			{
				if (scene2.ResponsePoint != null)
				{
					bool found_reaction = false;
					for (int n = 0; n < responding_characters2.Count; n++)
					{
						MovieCharacterReaction reaction2 = MovieTheater.GetReactionsForCharacter(responding_characters2[n] as NPC);
						if (reaction2 != null)
						{
							foreach (MovieReaction movie_reaction2 in reaction2.Reactions)
							{
								if (movie_reaction2.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters2[n] as NPC)) && movie_reaction2.SpecialResponses != null && movie_reaction2.SpecialResponses.DuringMovie != null && (movie_reaction2.SpecialResponses.DuringMovie.ResponsePoint == scene2.ResponsePoint || movie_reaction2.Whitelist.Count > 0))
								{
									if (!_whiteListDependencyLookup.ContainsKey(responding_characters2[n]))
									{
										_responseOrder[current_response_index2] = responding_characters2[n];
										if (movie_reaction2.Whitelist != null)
										{
											for (int m = 0; m < movie_reaction2.Whitelist.Count; m++)
											{
												Character white_list_character2 = Game1.getCharacterFromName(movie_reaction2.Whitelist[m]);
												if (white_list_character2 != null)
												{
													_whiteListDependencyLookup[white_list_character2] = responding_characters2[n];
													foreach (int key2 in _responseOrder.Keys)
													{
														if (_responseOrder[key2] == white_list_character2)
														{
															_responseOrder.Remove(key2);
														}
													}
												}
											}
										}
									}
									responding_characters2.RemoveAt(n);
									n--;
									found_reaction = true;
									break;
								}
							}
							if (found_reaction)
							{
								break;
							}
						}
					}
					if (!found_reaction)
					{
						for (int l = 0; l < responding_characters2.Count; l++)
						{
							MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_characters2[l] as NPC);
							if (reaction != null)
							{
								foreach (MovieReaction movie_reaction in reaction.Reactions)
								{
									if (movie_reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_characters2[l] as NPC)) && movie_reaction.SpecialResponses != null && movie_reaction.SpecialResponses.DuringMovie != null && movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == current_response_index2.ToString())
									{
										if (!_whiteListDependencyLookup.ContainsKey(responding_characters2[l]))
										{
											_responseOrder[current_response_index2] = responding_characters2[l];
											if (movie_reaction.Whitelist != null)
											{
												for (int k = 0; k < movie_reaction.Whitelist.Count; k++)
												{
													Character white_list_character = Game1.getCharacterFromName(movie_reaction.Whitelist[k]);
													if (white_list_character != null)
													{
														_whiteListDependencyLookup[white_list_character] = responding_characters2[l];
														foreach (int key in _responseOrder.Keys)
														{
															if (_responseOrder[key] == white_list_character)
															{
																_responseOrder.Remove(key);
															}
														}
													}
												}
											}
										}
										responding_characters2.RemoveAt(l);
										l--;
										found_reaction = true;
										break;
									}
								}
								if (found_reaction)
								{
									break;
								}
							}
						}
					}
					current_response_index2++;
				}
			}
			current_response_index2 = 0;
			for (int j = 0; j < responding_characters2.Count; j++)
			{
				if (!_whiteListDependencyLookup.ContainsKey(responding_characters2[j]))
				{
					for (; _responseOrder.ContainsKey(current_response_index2); current_response_index2++)
					{
					}
					_responseOrder[current_response_index2] = responding_characters2[j];
					current_response_index2++;
				}
			}
			responding_characters2 = null;
			foreach (MovieScene scene in movieData.Scenes)
			{
				_ParseScene(sb, scene);
			}
			while (currentResponse < _responseOrder.Count)
			{
				_ParseResponse(sb);
			}
			sb.Append("/stopMusic");
			sb.Append("/fade/viewport -1000 -1000");
			sb.Append("/pause 500/message \"" + Game1.content.LoadString("Strings\\Locations:Theater_MovieEnd") + "\"/pause 500");
			sb.Append("/requestMovieEnd");
			Console.WriteLine(sb.ToString());
			return new Event(sb.ToString());
		}

		protected void _ParseScene(StringBuilder sb, MovieScene scene)
		{
			if (scene.Sound != "")
			{
				sb.Append("/playSound " + scene.Sound);
			}
			if (scene.Music != "")
			{
				sb.Append("/playMusic " + scene.Music);
			}
			if (scene.MessageDelay > 0)
			{
				sb.Append("/pause " + scene.MessageDelay);
			}
			if (scene.Image >= 0)
			{
				sb.Append("/specificTemporarySprite movieTheater_screen " + movieData.SheetIndex + " " + scene.Image + " " + scene.Shake.ToString());
			}
			if (scene.Script != "")
			{
				sb.Append(scene.Script);
			}
			if (scene.Text != "")
			{
				sb.Append("/message \"" + scene.Text + "\"");
			}
			if (scene.ResponsePoint != null)
			{
				_ParseResponse(sb, scene);
			}
		}

		protected void _ParseResponse(StringBuilder sb, MovieScene scene = null)
		{
			if (_responseOrder.ContainsKey(currentResponse))
			{
				sb.Append("/pause 500");
				Character responding_character = _responseOrder[currentResponse];
				bool hadUniqueScript = false;
				if (!_whiteListDependencyLookup.ContainsKey(responding_character))
				{
					MovieCharacterReaction reaction = MovieTheater.GetReactionsForCharacter(responding_character as NPC);
					if (reaction != null)
					{
						foreach (MovieReaction movie_reaction in reaction.Reactions)
						{
							if (movie_reaction.ShouldApplyToMovie(movieData, MovieTheater.GetPatronNames(), MovieTheater.GetResponseForMovie(responding_character as NPC)) && movie_reaction.SpecialResponses != null && movie_reaction.SpecialResponses.DuringMovie != null && (movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == null || movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == "" || (scene != null && movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == scene.ResponsePoint) || movie_reaction.SpecialResponses.DuringMovie.ResponsePoint == currentResponse.ToString() || movie_reaction.Whitelist.Count > 0))
							{
								if (movie_reaction.SpecialResponses.DuringMovie.Script != "")
								{
									sb.Append(movie_reaction.SpecialResponses.DuringMovie.Script);
									hadUniqueScript = true;
								}
								if (movie_reaction.SpecialResponses.DuringMovie.Text != "")
								{
									sb.Append("/speak " + responding_character.name + " \"" + movie_reaction.SpecialResponses.DuringMovie.Text + "\"");
								}
								break;
							}
						}
					}
				}
				_ParseCharacterResponse(sb, responding_character, hadUniqueScript);
				foreach (Character key in _whiteListDependencyLookup.Keys)
				{
					if (_whiteListDependencyLookup[key] == responding_character)
					{
						_ParseCharacterResponse(sb, key);
					}
				}
			}
			currentResponse++;
		}

		protected void _ParseCharacterResponse(StringBuilder sb, Character responding_character, bool ignoreScript = false)
		{
			string response = MovieTheater.GetResponseForMovie(responding_character as NPC);
			if (_whiteListDependencyLookup.ContainsKey(responding_character))
			{
				response = MovieTheater.GetResponseForMovie(_whiteListDependencyLookup[responding_character] as NPC);
			}
			if (!(response == "love"))
			{
				if (!(response == "like"))
				{
					if (response == "dislike")
					{
						sb.Append("/friendship " + responding_character.Name + " " + 0);
						if (!ignoreScript)
						{
							sb.Append("/playSound newArtifact/emote " + (string)responding_character.name + " " + 24 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeMovie", responding_character.displayName) + "\"");
						}
					}
				}
				else
				{
					sb.Append("/friendship " + responding_character.Name + " " + 100);
					if (!ignoreScript)
					{
						sb.Append("/playSound give_gift/emote " + (string)responding_character.name + " " + 56 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeMovie", responding_character.displayName) + "\"");
					}
				}
			}
			else
			{
				sb.Append("/friendship " + responding_character.Name + " " + 200);
				if (!ignoreScript)
				{
					sb.Append("/playSound reward/emote " + (string)responding_character.name + " " + 20 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveMovie", responding_character.displayName) + "\"");
				}
			}
			if (_concessionsData != null && _concessionsData.ContainsKey(responding_character))
			{
				MovieConcession concession = _concessionsData[responding_character];
				string concession_response = MovieTheater.GetConcessionTasteForCharacter(responding_character, concession);
				string gender_tag = "";
				Dictionary<string, string> NPCDispositions = Game1.content.Load<Dictionary<string, string>>("Data\\NPCDispositions");
				if (NPCDispositions.ContainsKey(responding_character.name))
				{
					string[] disposition = NPCDispositions[responding_character.name].Split('/');
					if (disposition[4] == "female")
					{
						gender_tag = "_Female";
					}
					else if (disposition[4] == "male")
					{
						gender_tag = "_Male";
					}
				}
				string sound = "eat";
				if (concession.tags != null && concession.tags.Contains("Drink"))
				{
					sound = "gulp";
				}
				if (!(concession_response == "love"))
				{
					if (!(concession_response == "like"))
					{
						if (concession_response == "dislike")
						{
							sb.Append("/friendship " + responding_character.Name + " " + 0);
							sb.Append("/playSound croak/pause 1000");
							sb.Append("/playSound newArtifact/emote " + (string)responding_character.name + " " + 40 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_DislikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName) + "\"");
						}
					}
					else
					{
						sb.Append("/friendship " + responding_character.Name + " " + 25);
						sb.Append("/tossConcession " + responding_character.Name + " " + concession.id + "/pause 1000");
						sb.Append("/playSound " + sound + "/shake " + responding_character.Name + " 500/pause 1000");
						sb.Append("/playSound give_gift/emote " + (string)responding_character.name + " " + 56 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_LikeConcession" + gender_tag, responding_character.displayName, concession.DisplayName) + "\"");
					}
				}
				else
				{
					sb.Append("/friendship " + responding_character.Name + " " + 50);
					sb.Append("/tossConcession " + responding_character.Name + " " + concession.id + "/pause 1000");
					sb.Append("/playSound " + sound + "/shake " + responding_character.Name + " 500/pause 1000");
					sb.Append("/playSound reward/emote " + (string)responding_character.name + " " + 20 + "/message \"" + Game1.content.LoadString("Strings\\Characters:MovieTheater_LoveConcession" + gender_tag, responding_character.displayName, concession.DisplayName) + "\"");
				}
			}
			_characterResponses[responding_character] = response;
		}

		public Dictionary<Character, string> GetCharacterResponses()
		{
			return _characterResponses;
		}

		private static string getEventName(Character c)
		{
			if (c is Farmer)
			{
				return "farmer" + Utility.getFarmerNumberFromFarmer(c as Farmer);
			}
			return c.name;
		}

		private Point getBackRowSeatTileFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Point(2, 10);
			case 1:
				return new Point(3, 10);
			case 2:
				return new Point(4, 10);
			case 3:
				return new Point(5, 10);
			case 4:
				return new Point(8, 10);
			case 5:
				return new Point(9, 10);
			case 6:
				return new Point(10, 10);
			case 7:
				return new Point(11, 10);
			default:
				return new Point(4, 12);
			}
		}

		private Point getMidRowSeatTileFromIndex(int index)
		{
			switch (index)
			{
			case 0:
				return new Point(3, 8);
			case 1:
				return new Point(4, 8);
			case 2:
				return new Point(5, 8);
			case 3:
				return new Point(8, 8);
			case 4:
				return new Point(9, 8);
			case 5:
				return new Point(10, 8);
			default:
				return new Point(4, 12);
			}
		}
	}
}
