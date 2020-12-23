using Netcode;
using System;
using System.Collections.Generic;

namespace StardewValley
{
	public class MailReward : OrderReward
	{
		public NetBool noLetter = new NetBool(value: true);

		public NetStringList grantedMails = new NetStringList();

		public NetBool host = new NetBool(value: false);

		public override void InitializeNetFields()
		{
			base.InitializeNetFields();
			base.NetFields.AddFields(noLetter, grantedMails, host);
		}

		public override void Load(SpecialOrder order, Dictionary<string, string> data)
		{
			string[] array = order.Parse(data["MailReceived"]).Split(' ');
			foreach (string s in array)
			{
				grantedMails.Add(s);
			}
			if (data.ContainsKey("NoLetter"))
			{
				noLetter.Value = Convert.ToBoolean(order.Parse(data["NoLetter"]));
			}
			if (data.ContainsKey("Host"))
			{
				host.Value = Convert.ToBoolean(order.Parse(data["Host"]));
			}
		}

		public override void Grant()
		{
			foreach (string mail in grantedMails)
			{
				if (host.Value)
				{
					if (Game1.IsMasterGame)
					{
						if (Game1.newDaySync != null)
						{
							Game1.addMail(mail, noLetter.Value, sendToEveryone: true);
						}
						else
						{
							string actualMail2 = mail;
							if (actualMail2 == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
							{
								Game1.player.mailReceived.Remove("ClintReward2");
								actualMail2 = "ClintReward2";
							}
							Game1.addMailForTomorrow(actualMail2, noLetter.Value, sendToEveryone: true);
						}
					}
				}
				else if (Game1.newDaySync != null)
				{
					Game1.addMail(mail, noLetter.Value, sendToEveryone: true);
				}
				else
				{
					string actualMail = mail;
					if (actualMail == "ClintReward" && Game1.player.mailReceived.Contains("ClintReward"))
					{
						Game1.player.mailReceived.Remove("ClintReward2");
						actualMail = "ClintReward2";
					}
					Game1.addMailForTomorrow(actualMail, noLetter.Value, sendToEveryone: true);
				}
			}
		}
	}
}
