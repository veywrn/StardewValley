using Microsoft.Xna.Framework;
using Netcode;
using System;

namespace StardewValley.Tools
{
	public class Pickaxe : Tool
	{
		public const int hitMargin = 8;

		public const int BoulderStrength = 4;

		private int boulderTileX;

		private int boulderTileY;

		private int hitsToBoulder;

		public NetInt additionalPower = new NetInt(0);

		public Pickaxe()
			: base("Pickaxe", 0, 105, 131, stackable: false)
		{
			base.UpgradeLevel = 0;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(additionalPower);
		}

		public override Item getOne()
		{
			Pickaxe result = new Pickaxe();
			result.UpgradeLevel = base.UpgradeLevel;
			result.additionalPower.Value = additionalPower.Value;
			CopyEnchantments(this, result);
			result._GetOneFrom(this);
			return result;
		}

		protected override string loadDisplayName()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14184");
		}

		protected override string loadDescription()
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14185");
		}

		public override bool beginUsing(GameLocation location, int x, int y, Farmer who)
		{
			Update(who.FacingDirection, 0, who);
			who.EndUsingTool();
			return true;
		}

		public override void DoFunction(GameLocation location, int x, int y, int power, Farmer who)
		{
			base.DoFunction(location, x, y, power, who);
			power = who.toolPower;
			if (!isEfficient)
			{
				who.Stamina -= (float)(2 * (power + 1)) - (float)who.MiningLevel * 0.1f;
			}
			Utility.clampToTile(new Vector2(x, y));
			int tileX = x / 64;
			int tileY = y / 64;
			Vector2 tile2 = new Vector2(tileX, tileY);
			if (location.performToolAction(this, tileX, tileY))
			{
				return;
			}
			Object o = null;
			location.Objects.TryGetValue(tile2, out o);
			if (o == null)
			{
				if (who.FacingDirection == 0 || who.FacingDirection == 2)
				{
					tileX = (x - 8) / 64;
					location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
					if (o == null)
					{
						tileX = (x + 8) / 64;
						location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
					}
				}
				else
				{
					tileY = (y + 8) / 64;
					location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
					if (o == null)
					{
						tileY = (y - 8) / 64;
						location.Objects.TryGetValue(new Vector2(tileX, tileY), out o);
					}
				}
				x = tileX * 64;
				y = tileY * 64;
				if (location.terrainFeatures.ContainsKey(tile2) && location.terrainFeatures[tile2].performToolAction(this, 0, tile2, location))
				{
					location.terrainFeatures.Remove(tile2);
				}
			}
			tile2 = new Vector2(tileX, tileY);
			if (o != null)
			{
				if (o.Name.Equals("Stone"))
				{
					location.playSound("hammer");
					if ((int)o.minutesUntilReady > 0)
					{
						int damage = Math.Max(1, (int)upgradeLevel + 1) + additionalPower.Value;
						o.minutesUntilReady.Value -= damage;
						o.shakeTimer = 200;
						if ((int)o.minutesUntilReady > 0)
						{
							Game1.createRadialDebris(Game1.currentLocation, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
							return;
						}
					}
					if (o.ParentSheetIndex < 200 && !Game1.objectInformation.ContainsKey(o.ParentSheetIndex + 1) && (int)o.parentSheetIndex != 25)
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(o.ParentSheetIndex + 1, 300f, 1, 2, new Vector2(x - x % 64, y - y % 64), flicker: true, o.flipped)
						{
							alphaFade = 0.01f
						});
					}
					else
					{
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(47, new Vector2(tileX * 64, tileY * 64), Color.Gray, 10, flipped: false, 80f));
					}
					Game1.createRadialDebris(location, 14, tileX, tileY, Game1.random.Next(2, 5), resource: false);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(46, new Vector2(tileX * 64, tileY * 64), Color.White, 10, flipped: false, 80f)
					{
						motion = new Vector2(0f, -0.6f),
						acceleration = new Vector2(0f, 0.002f),
						alphaFade = 0.015f
					});
					location.OnStoneDestroyed(o.parentSheetIndex, tileX, tileY, getLastFarmerToUse());
					if ((int)o.minutesUntilReady <= 0)
					{
						o.performRemoveAction(new Vector2(tileX, tileY), location);
						location.Objects.Remove(new Vector2(tileX, tileY));
						location.playSound("stoneCrack");
						Game1.stats.RocksCrushed++;
					}
				}
				else if (o.Name.Contains("Boulder"))
				{
					location.playSound("hammer");
					if (base.UpgradeLevel < 2)
					{
						Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:Pickaxe.cs.14194")));
						return;
					}
					if (tileX == boulderTileX && tileY == boulderTileY)
					{
						hitsToBoulder += power + 1;
						o.shakeTimer = 190;
					}
					else
					{
						hitsToBoulder = 0;
						boulderTileX = tileX;
						boulderTileY = tileY;
					}
					if (hitsToBoulder >= 4)
					{
						location.removeObject(tile2, showDestroyedObject: false);
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile2.X - 32f, 64f * (tile2.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 0
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile2.X + 32f, 64f * (tile2.Y - 1f)), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 200
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile2.X, 64f * (tile2.Y - 1f) - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 400
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(5, new Vector2(64f * tile2.X, 64f * tile2.Y - 32f), Color.Gray, 8, Game1.random.NextDouble() < 0.5, 50f)
						{
							delayBeforeAnimationStart = 600
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile2.X, 64f * tile2.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128));
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile2.X + 32f, 64f * tile2.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
						{
							delayBeforeAnimationStart = 250
						});
						Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(25, new Vector2(64f * tile2.X - 32f, 64f * tile2.Y), Color.White, 8, Game1.random.NextDouble() < 0.5, 50f, 0, -1, -1f, 128)
						{
							delayBeforeAnimationStart = 500
						});
						location.playSound("boulderBreak");
						Game1.stats.BouldersCracked++;
					}
				}
				else if (o.performToolAction(this, location))
				{
					o.performRemoveAction(tile2, location);
					if (o.type.Equals("Crafting") && (int)o.fragility != 2)
					{
						Game1.currentLocation.debris.Add(new Debris(o.bigCraftable ? (-o.ParentSheetIndex) : o.ParentSheetIndex, who.GetToolLocation(), new Vector2(who.GetBoundingBox().Center.X, who.GetBoundingBox().Center.Y)));
					}
					Game1.currentLocation.Objects.Remove(tile2);
				}
			}
			else
			{
				location.playSound("woodyHit");
				if (location.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
				{
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(tileX * 64, tileY * 64), Color.White, 8, flipped: false, 80f)
					{
						alphaFade = 0.015f
					});
				}
			}
		}
	}
}
