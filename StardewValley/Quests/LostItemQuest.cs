using Microsoft.Xna.Framework;
using Netcode;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace StardewValley.Quests
{
	public class LostItemQuest : Quest
	{
		[XmlElement("npcName")]
		public readonly NetString npcName = new NetString();

		[XmlElement("locationOfItem")]
		public readonly NetString locationOfItem = new NetString();

		[XmlElement("itemIndex")]
		public readonly NetInt itemIndex = new NetInt();

		[XmlElement("tileX")]
		public readonly NetInt tileX = new NetInt();

		[XmlElement("tileY")]
		public readonly NetInt tileY = new NetInt();

		[XmlElement("itemFound")]
		public readonly NetBool itemFound = new NetBool();

		[XmlElement("objective")]
		public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

		public LostItemQuest()
		{
		}

		public LostItemQuest(string npcName, string locationOfItem, int itemIndex, int tileX, int tileY)
		{
			this.npcName.Value = npcName;
			this.locationOfItem.Value = locationOfItem;
			this.itemIndex.Value = itemIndex;
			this.tileX.Value = tileX;
			this.tileY.Value = tileY;
			questType.Value = 9;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(objective, npcName, locationOfItem, itemIndex, tileX, tileY, itemFound);
		}

		public override void adjustGameLocation(GameLocation location)
		{
			if (!itemFound && location.name.Equals(locationOfItem.Value))
			{
				Vector2 position = new Vector2((int)tileX, (int)tileY);
				if (location.overlayObjects.ContainsKey(position))
				{
					location.overlayObjects.Remove(position);
				}
				Object o = new Object(position, itemIndex, 1);
				o.questItem.Value = true;
				o.questId.Value = id;
				o.isSpawnedObject.Value = true;
				location.overlayObjects.Add(position, o);
			}
		}

		public new void reloadObjective()
		{
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}

		public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -2, Item item = null, string str = null)
		{
			if ((bool)completed)
			{
				return false;
			}
			if (item != null && item is Object && (item as Object).parentSheetIndex.Value == itemIndex.Value && !itemFound)
			{
				itemFound.Value = true;
				string npcDisplayName = npcName;
				NPC namedNpc = Game1.getCharacterFromName(npcName);
				if (namedNpc != null)
				{
					npcDisplayName = namedNpc.displayName;
				}
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Quests:MessageFoundLostItem", item.DisplayName, npcDisplayName));
				objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", namedNpc);
				Game1.playSound("jingle1");
			}
			else if (n != null && n.Name.Equals(npcName.Value) && n.isVillager() && (bool)itemFound && Game1.player.hasItemInInventory(itemIndex, 1))
			{
				questComplete();
				Dictionary<int, string> questData = Game1.temporaryContent.Load<Dictionary<int, string>>("Data\\Quests");
				string thankYou = (questData[id].Length > 9) ? questData[id].Split('/')[9] : Game1.content.LoadString("Data\\ExtraDialogue:LostItemQuest_DefaultThankYou");
				n.setNewDialogue(thankYou);
				Game1.drawDialogue(n);
				Game1.player.changeFriendship(250, n);
				Game1.player.removeFirstOfThisItemFromInventory(itemIndex);
				return true;
			}
			return false;
		}
	}
}
