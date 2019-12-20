using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class AdventureGuild : GameLocation
	{
		private NPC Gil = new NPC(null, new Vector2(-1000f, -1000f), "AdventureGuild", 2, "Gil", datable: false, null, Game1.content.Load<Texture2D>("Portraits\\Gil"));

		private bool talkedToGil;

		public AdventureGuild()
		{
		}

		public AdventureGuild(string mapPath, string name)
			: base(mapPath, name)
		{
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			switch ((map.GetLayer("Buildings").Tiles[tileLocation] != null) ? map.GetLayer("Buildings").Tiles[tileLocation].TileIndex : (-1))
			{
			case 1306:
				showMonsterKillList();
				return true;
			case 1291:
			case 1292:
			case 1355:
			case 1356:
			case 1357:
			case 1358:
				gil();
				return true;
			default:
				return base.checkAction(tileLocation, viewport, who);
			}
		}

		protected override void resetLocalState()
		{
			base.resetLocalState();
			talkedToGil = false;
			if (!Game1.player.mailReceived.Contains("guildMember"))
			{
				Game1.player.mailReceived.Add("guildMember");
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
			{
				float yOffset = 4f * (float)Math.Round(Math.Sin(DateTime.UtcNow.TimeOfDay.TotalMilliseconds / 250.0), 2);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(504f, 464f + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.064801f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 504f + yOffset)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.06481f);
			}
		}

		private string killListLine(string monsterType, int killCount, int target)
		{
			string monsterNamePlural = Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_" + monsterType);
			if (killCount == 0)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_None", killCount, target, monsterNamePlural) + "^";
			}
			if (killCount >= target)
			{
				return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_OverTarget", killCount, target, monsterNamePlural) + "^";
			}
			return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat", killCount, target, monsterNamePlural) + "^";
		}

		public void showMonsterKillList()
		{
			if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
			{
				Game1.player.mailReceived.Add("checkedMonsterBoard");
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Header").Replace('\n', '^') + "^");
			int slimesKilled = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge");
			int shadowsKilled = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute");
			int skeletonsKilled = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int crabsKilled = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int caveInsectsKilled = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int batsKilled = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat");
			int duggyKilled = Game1.stats.getMonstersKilled("Duggy");
			int dustSpiritKilled = Game1.stats.getMonstersKilled("Dust Spirit");
			int mummiesKilled = Game1.stats.getMonstersKilled("Mummy");
			int dinosKilled = Game1.stats.getMonstersKilled("Pepper Rex");
			int serpentsKilled = Game1.stats.getMonstersKilled("Serpent");
			stringBuilder.Append(killListLine("Slimes", slimesKilled, 1000));
			stringBuilder.Append(killListLine("VoidSpirits", shadowsKilled, 150));
			stringBuilder.Append(killListLine("Bats", batsKilled, 200));
			stringBuilder.Append(killListLine("Skeletons", skeletonsKilled, 50));
			stringBuilder.Append(killListLine("CaveInsects", caveInsectsKilled, 125));
			stringBuilder.Append(killListLine("Duggies", duggyKilled, 30));
			stringBuilder.Append(killListLine("DustSprites", dustSpiritKilled, 500));
			stringBuilder.Append(killListLine("RockCrabs", crabsKilled, 60));
			stringBuilder.Append(killListLine("Mummies", mummiesKilled, 100));
			stringBuilder.Append(killListLine("PepperRex", dinosKilled, 50));
			stringBuilder.Append(killListLine("Serpent", serpentsKilled, 250));
			stringBuilder.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
			Game1.drawLetterMessage(stringBuilder.ToString());
		}

		public override void cleanupBeforePlayerExit()
		{
			base.cleanupBeforePlayerExit();
			Game1.changeMusicTrack("none");
		}

		public static bool areAllMonsterSlayerQuestsComplete()
		{
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge");
			int shadowsKilled = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute");
			int skeletonsKilled = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int crabsKilled = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int caveInsectsKilled = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int batsKilled = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat");
			int duggyKilled = Game1.stats.getMonstersKilled("Duggy");
			Game1.stats.getMonstersKilled("Metal Head");
			Game1.stats.getMonstersKilled("Stone Golem");
			int dustSpiritKilled = Game1.stats.getMonstersKilled("Dust Spirit");
			int mummiesKilled = Game1.stats.getMonstersKilled("Mummy");
			int dinosKilled = Game1.stats.getMonstersKilled("Pepper Rex");
			int serpentsKilled = Game1.stats.getMonstersKilled("Serpent");
			if (num < 1000)
			{
				return false;
			}
			if (shadowsKilled < 150)
			{
				return false;
			}
			if (skeletonsKilled < 50)
			{
				return false;
			}
			if (caveInsectsKilled < 125)
			{
				return false;
			}
			if (batsKilled < 200)
			{
				return false;
			}
			if (duggyKilled < 30)
			{
				return false;
			}
			if (dustSpiritKilled < 500)
			{
				return false;
			}
			if (crabsKilled < 60)
			{
				return false;
			}
			if (mummiesKilled < 100)
			{
				return false;
			}
			if (dinosKilled < 50)
			{
				return false;
			}
			if (serpentsKilled < 250)
			{
				return false;
			}
			return true;
		}

		public static bool willThisKillCompleteAMonsterSlayerQuest(string nameOfMonster)
		{
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge");
			int shadowsKilled = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute");
			int skeletonsKilled = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int crabsKilled = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int caveInsectsKilled = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int batsKilled = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat");
			int duggyKilled = Game1.stats.getMonstersKilled("Duggy");
			int metalHeadKilled = Game1.stats.getMonstersKilled("Metal Head");
			int golemKilled = Game1.stats.getMonstersKilled("Stone Golem");
			int dustSpiritKilled = Game1.stats.getMonstersKilled("Dust Spirit");
			int mummiesKilled = Game1.stats.getMonstersKilled("Mummy");
			int dinosKilled = Game1.stats.getMonstersKilled("Pepper Rex");
			int serpentsKilled = Game1.stats.getMonstersKilled("Serpent");
			int slimesKilledNew = num + ((nameOfMonster.Equals("Green Slime") || nameOfMonster.Equals("Frost Jelly") || nameOfMonster.Equals("Sludge")) ? 1 : 0);
			int shadowsKilledNew = shadowsKilled + ((nameOfMonster.Equals("Shadow Guy") || nameOfMonster.Equals("Shadow Shaman") || nameOfMonster.Equals("Shadow Brute")) ? 1 : 0);
			int skeletonsKilledNew = skeletonsKilled + ((nameOfMonster.Equals("Skeleton") || nameOfMonster.Equals("Skeleton Mage")) ? 1 : 0);
			int crabsKilledNew = crabsKilled + ((nameOfMonster.Equals("Rock Crab") || nameOfMonster.Equals("Lava Crab") || nameOfMonster.Equals("Iridium Crab")) ? 1 : 0);
			int caveInsectsKilledNew = caveInsectsKilled + ((nameOfMonster.Equals("Grub") || nameOfMonster.Equals("Fly") || nameOfMonster.Equals("Bug")) ? 1 : 0);
			int batsKilledNew = batsKilled + ((nameOfMonster.Equals("Bat") || nameOfMonster.Equals("Frost Bat") || nameOfMonster.Equals("Lava Bat")) ? 1 : 0);
			int duggyKilledNew = duggyKilled + (nameOfMonster.Equals("Duggy") ? 1 : 0);
			nameOfMonster.Equals("Metal Head");
			nameOfMonster.Equals("Stone Golem");
			int dustSpiritKilledNew = dustSpiritKilled + (nameOfMonster.Equals("Dust Spirit") ? 1 : 0);
			int mummiesKilledNew = mummiesKilled + (nameOfMonster.Equals("Mummy") ? 1 : 0);
			int dinosKilledNew = dinosKilled + (nameOfMonster.Equals("Pepper Rex") ? 1 : 0);
			int serpentsKilledNew = serpentsKilled + (nameOfMonster.Equals("Serpent") ? 1 : 0);
			if (num < 1000 && slimesKilledNew >= 1000 && !Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				return true;
			}
			if (shadowsKilled < 150 && shadowsKilledNew >= 150 && !Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				return true;
			}
			if (skeletonsKilled < 50 && skeletonsKilledNew >= 50 && !Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				return true;
			}
			if (caveInsectsKilled < 125 && caveInsectsKilledNew >= 125 && !Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				return true;
			}
			if (batsKilled < 200 && batsKilledNew >= 200 && !Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				return true;
			}
			if (duggyKilled < 30 && duggyKilledNew >= 30 && !Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				return true;
			}
			if (dustSpiritKilled < 500 && dustSpiritKilledNew >= 500 && !Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				return true;
			}
			if (crabsKilled < 60 && crabsKilledNew >= 60 && !Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				return true;
			}
			if (mummiesKilled < 100 && mummiesKilledNew >= 100 && !Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				return true;
			}
			if (dinosKilled < 50 && dinosKilledNew >= 50 && !Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				return true;
			}
			if (serpentsKilled < 250 && serpentsKilledNew >= 250 && !Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				return true;
			}
			return false;
		}

		public void onRewardCollected(Item item, Farmer who)
		{
			if (item != null && !who.hasOrWillReceiveMail("Gil_" + item.Name))
			{
				who.mailReceived.Add("Gil_" + item.Name);
			}
		}

		private void gil()
		{
			List<Item> rewards = new List<Item>();
			int num = Game1.stats.getMonstersKilled("Green Slime") + Game1.stats.getMonstersKilled("Frost Jelly") + Game1.stats.getMonstersKilled("Sludge");
			int shadowsKilled = Game1.stats.getMonstersKilled("Shadow Guy") + Game1.stats.getMonstersKilled("Shadow Shaman") + Game1.stats.getMonstersKilled("Shadow Brute");
			int skeletonsKilled = Game1.stats.getMonstersKilled("Skeleton") + Game1.stats.getMonstersKilled("Skeleton Mage");
			int goblinsKilled = Game1.stats.getMonstersKilled("Goblin Warrior") + Game1.stats.getMonstersKilled("Goblin Wizard");
			int crabsKilled = Game1.stats.getMonstersKilled("Rock Crab") + Game1.stats.getMonstersKilled("Lava Crab") + Game1.stats.getMonstersKilled("Iridium Crab");
			int caveInsectsKilled = Game1.stats.getMonstersKilled("Grub") + Game1.stats.getMonstersKilled("Fly") + Game1.stats.getMonstersKilled("Bug");
			int batsKilled = Game1.stats.getMonstersKilled("Bat") + Game1.stats.getMonstersKilled("Frost Bat") + Game1.stats.getMonstersKilled("Lava Bat");
			int duggyKilled = Game1.stats.getMonstersKilled("Duggy");
			int metalHeadKilled = Game1.stats.getMonstersKilled("Metal Head");
			int golemKilled = Game1.stats.getMonstersKilled("Stone Golem");
			int dustSpiritKilled = Game1.stats.getMonstersKilled("Dust Spirit");
			int mummiesKilled = Game1.stats.getMonstersKilled("Mummy");
			int dinosKilled = Game1.stats.getMonstersKilled("Pepper Rex");
			int serpentsKilled = Game1.stats.getMonstersKilled("Serpent");
			if (num >= 1000 && !Game1.player.mailReceived.Contains("Gil_Slime Charmer Ring"))
			{
				rewards.Add(new Ring(520));
			}
			if (shadowsKilled >= 150 && !Game1.player.mailReceived.Contains("Gil_Savage Ring"))
			{
				rewards.Add(new Ring(523));
			}
			if (skeletonsKilled >= 50 && !Game1.player.mailReceived.Contains("Gil_Skeleton Mask"))
			{
				rewards.Add(new Hat(8));
			}
			if (goblinsKilled >= 50)
			{
				Game1.player.specialItems.Contains(9);
			}
			if (crabsKilled >= 60 && !Game1.player.mailReceived.Contains("Gil_Crabshell Ring"))
			{
				rewards.Add(new Ring(810));
			}
			if (caveInsectsKilled >= 125 && !Game1.player.mailReceived.Contains("Gil_Insect Head"))
			{
				rewards.Add(new MeleeWeapon(13));
			}
			if (batsKilled >= 200 && !Game1.player.mailReceived.Contains("Gil_Vampire Ring"))
			{
				rewards.Add(new Ring(522));
			}
			if (duggyKilled >= 30 && !Game1.player.mailReceived.Contains("Gil_Hard Hat"))
			{
				rewards.Add(new Hat(27));
			}
			if (metalHeadKilled >= 50)
			{
				Game1.player.specialItems.Contains(519);
			}
			if (golemKilled >= 50)
			{
				Game1.player.specialItems.Contains(517);
			}
			if (dustSpiritKilled >= 500 && !Game1.player.mailReceived.Contains("Gil_Burglar's Ring"))
			{
				rewards.Add(new Ring(526));
			}
			if (mummiesKilled >= 100 && !Game1.player.mailReceived.Contains("Gil_Arcane Hat"))
			{
				rewards.Add(new Hat(60));
			}
			if (dinosKilled >= 50 && !Game1.player.mailReceived.Contains("Gil_Knight's Helmet"))
			{
				rewards.Add(new Hat(50));
			}
			if (serpentsKilled >= 250 && !Game1.player.mailReceived.Contains("Gil_Napalm Ring"))
			{
				rewards.Add(new Ring(811));
			}
			foreach (Item i in rewards)
			{
				if (i is Object)
				{
					(i as Object).specialItem = true;
				}
			}
			if (rewards.Count > 0)
			{
				Game1.activeClickableMenu = new ItemGrabMenu(rewards, this)
				{
					behaviorOnItemGrab = onRewardCollected
				};
				return;
			}
			if (talkedToGil)
			{
				Game1.drawDialogue(Gil, Game1.content.LoadString("Characters\\Dialogue\\Gil:Snoring"));
			}
			else
			{
				Game1.drawDialogue(Gil, Game1.content.LoadString("Characters\\Dialogue\\Gil:ComeBackLater"));
			}
			talkedToGil = true;
		}
	}
}
