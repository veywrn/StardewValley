using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace StardewValley.Characters
{
	public class Horse : NPC
	{
		private readonly NetGuid horseId = new NetGuid();

		private readonly NetFarmerRef netRider = new NetFarmerRef();

		public readonly NetLong ownerId = new NetLong();

		[XmlIgnore]
		public readonly NetBool mounting = new NetBool();

		[XmlIgnore]
		public readonly NetBool dismounting = new NetBool();

		private Vector2 dismountTile;

		private int ridingAnimationDirection;

		private bool roomForHorseAtDismountTile;

		[XmlElement("hat")]
		public readonly NetRef<Hat> hat = new NetRef<Hat>();

		private readonly NetMutex mutex = new NetMutex();

		private bool squeezingThroughGate;

		public Guid HorseId
		{
			get
			{
				return horseId.Value;
			}
			set
			{
				horseId.Value = value;
			}
		}

		[XmlIgnore]
		public Farmer rider
		{
			get
			{
				return netRider.Value;
			}
			set
			{
				netRider.Value = value;
			}
		}

		public Horse()
		{
			Sprite = new AnimatedSprite("Animals\\horse", 0, 32, 32);
			base.Breather = false;
			base.willDestroyObjectsUnderfoot = false;
			base.HideShadow = true;
			Sprite.textureUsesFlippedRightForLeft = true;
			Sprite.loop = true;
			drawOffset.Set(new Vector2(-16f, 0f));
			faceDirection(3);
		}

		public Horse(Guid horseId, int xTile, int yTile)
			: this()
		{
			base.Name = "";
			base.displayName = base.Name;
			base.Position = new Vector2(xTile, yTile) * 64f;
			base.currentLocation = Game1.currentLocation;
			HorseId = horseId;
		}

		public override bool canTalk()
		{
			return false;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddFields(horseId, netRider.NetFields, mounting, dismounting, hat, mutex.NetFields, ownerId);
			position.Field.AxisAlignedMovement = false;
		}

		public Farmer getOwner()
		{
			if (ownerId.Value == 0L)
			{
				return null;
			}
			return Game1.getFarmerMaybeOffline(ownerId.Value);
		}

		public override void reloadSprite()
		{
		}

		public override void dayUpdate(int dayOfMonth)
		{
			faceDirection(3);
		}

		public override Rectangle GetBoundingBox()
		{
			Rectangle r = base.GetBoundingBox();
			if (squeezingThroughGate && (base.FacingDirection == 0 || base.FacingDirection == 2))
			{
				r.Inflate(-36, 0);
			}
			return r;
		}

		public override bool canPassThroughActionTiles()
		{
			return false;
		}

		public void squeezeForGate()
		{
			squeezingThroughGate = true;
			if (rider != null)
			{
				rider.TemporaryPassableTiles.Add(GetBoundingBox());
			}
		}

		public override void update(GameTime time, GameLocation location)
		{
			base.currentLocation = location;
			mutex.Update(location);
			squeezingThroughGate = false;
			faceTowardFarmer = false;
			faceTowardFarmerTimer = -1;
			Sprite.loop = (rider != null && !rider.hidden);
			if (rider != null && (bool)rider.hidden)
			{
				return;
			}
			if (rider != null && rider.isAnimatingMount)
			{
				rider.showNotCarrying();
			}
			if ((bool)mounting)
			{
				if (rider == null || !rider.IsLocalPlayer)
				{
					return;
				}
				if (rider.mount != null)
				{
					mounting.Value = false;
					rider.isAnimatingMount = false;
					rider = null;
					Halt();
					farmerPassesThrough = false;
					return;
				}
				if (rider.Position.X < (float)(GetBoundingBox().X + 16 - 4))
				{
					rider.position.X += 4f;
				}
				else if (rider.Position.X > (float)(GetBoundingBox().X + 16 + 4))
				{
					rider.position.X -= 4f;
				}
				if (rider.getStandingY() < GetBoundingBox().Y - 4)
				{
					rider.position.Y += 4f;
				}
				else if (rider.getStandingY() > GetBoundingBox().Y + 4)
				{
					rider.position.Y -= 4f;
				}
				if (rider.yJumpOffset >= -8 && rider.yJumpVelocity <= 0f)
				{
					Halt();
					Sprite.loop = true;
					base.currentLocation.characters.Remove(this);
					rider.mount = this;
					rider.freezePause = -1;
					mounting.Value = false;
					rider.isAnimatingMount = false;
					rider.canMove = true;
					if (base.FacingDirection == 1)
					{
						rider.xOffset += 8f;
					}
				}
			}
			else if ((bool)dismounting)
			{
				if (rider == null || !rider.IsLocalPlayer)
				{
					Halt();
					return;
				}
				if (rider.isAnimatingMount)
				{
					rider.faceDirection(base.FacingDirection);
				}
				Vector2 targetPosition = new Vector2(dismountTile.X * 64f + 32f - (float)(rider.GetBoundingBox().Width / 2), dismountTile.Y * 64f + 4f);
				if (Math.Abs(rider.Position.X - targetPosition.X) > 4f)
				{
					if (rider.Position.X < targetPosition.X)
					{
						rider.position.X += Math.Min(4f, targetPosition.X - rider.Position.X);
					}
					else if (rider.Position.X > targetPosition.X)
					{
						rider.position.X += Math.Max(-4f, targetPosition.X - rider.Position.X);
					}
				}
				if (Math.Abs(rider.Position.Y - targetPosition.Y) > 4f)
				{
					if (rider.Position.Y < targetPosition.Y)
					{
						rider.position.Y += Math.Min(4f, targetPosition.Y - rider.Position.Y);
					}
					else if (rider.Position.Y > targetPosition.Y)
					{
						rider.position.Y += Math.Max(-4f, targetPosition.Y - rider.Position.Y);
					}
				}
				if (rider.yJumpOffset >= 0 && rider.yJumpVelocity <= 0f)
				{
					rider.position.Y += 8f;
					rider.position.X = targetPosition.X;
					int tries = 0;
					while (rider.currentLocation.isCollidingPosition(rider.GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, rider) && tries < 6)
					{
						tries++;
						rider.position.Y -= 4f;
					}
					if (tries == 6)
					{
						rider.Position = base.Position;
						dismounting.Value = false;
						rider.isAnimatingMount = false;
						rider.freezePause = -1;
						rider.canMove = true;
						return;
					}
					dismount();
				}
			}
			else if (rider == null && base.FacingDirection != 2 && Sprite.CurrentAnimation == null && Game1.random.NextDouble() < 0.002)
			{
				Sprite.loop = false;
				switch (base.FacingDirection)
				{
				case 0:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(25, Game1.random.Next(250, 750)),
						new FarmerSprite.AnimationFrame(14, 10)
					});
					break;
				case 1:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(24, 400),
						new FarmerSprite.AnimationFrame(23, 400),
						new FarmerSprite.AnimationFrame(22, 100),
						new FarmerSprite.AnimationFrame(21, 100)
					});
					break;
				case 3:
					Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(24, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(23, 400, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(22, 100, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(21, 100, secondaryArm: false, flip: true)
					});
					break;
				}
			}
			else if (rider != null)
			{
				if (base.FacingDirection != rider.FacingDirection || ridingAnimationDirection != base.FacingDirection)
				{
					Sprite.StopAnimation();
					faceDirection(rider.FacingDirection);
				}
				bool num = (rider.movementDirections.Any() && rider.CanMove) || rider.position.Field.IsInterpolating();
				SyncPositionToRider();
				if (num && Sprite.CurrentAnimation == null)
				{
					AnimatedSprite.endOfAnimationBehavior mountFootstep = delegate
					{
						if (rider != null)
						{
							string a = rider.currentLocation.doesTileHaveProperty((int)rider.getTileLocation().X, (int)rider.getTileLocation().Y, "Type", "Back");
							if (!(a == "Stone"))
							{
								if (a == "Wood")
								{
									rider.currentLocation.localSoundAt("woodyStep", getTileLocation());
									if (rider == Game1.player)
									{
										Rumble.rumble(0.1f, 50f);
									}
								}
								else
								{
									rider.currentLocation.localSoundAt("thudStep", getTileLocation());
									if (rider == Game1.player)
									{
										Rumble.rumble(0.3f, 50f);
									}
								}
							}
							else
							{
								rider.currentLocation.localSoundAt("stoneStep", getTileLocation());
								if (rider == Game1.player)
								{
									Rumble.rumble(0.1f, 50f);
								}
							}
						}
					};
					if (base.FacingDirection == 1)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70),
							new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(12, 70),
							new FarmerSprite.AnimationFrame(13, 70)
						});
					}
					else if (base.FacingDirection == 3)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(8, 70, secondaryArm: false, flip: true),
							new FarmerSprite.AnimationFrame(9, 70, secondaryArm: false, flip: true, mountFootstep),
							new FarmerSprite.AnimationFrame(10, 70, secondaryArm: false, flip: true, mountFootstep),
							new FarmerSprite.AnimationFrame(11, 70, secondaryArm: false, flip: true, mountFootstep),
							new FarmerSprite.AnimationFrame(12, 70, secondaryArm: false, flip: true),
							new FarmerSprite.AnimationFrame(13, 70, secondaryArm: false, flip: true)
						});
					}
					else if (base.FacingDirection == 0)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(15, 70),
							new FarmerSprite.AnimationFrame(16, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(17, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(18, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(19, 70),
							new FarmerSprite.AnimationFrame(20, 70)
						});
					}
					else if (base.FacingDirection == 2)
					{
						Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(1, 70),
							new FarmerSprite.AnimationFrame(2, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(3, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(4, 70, secondaryArm: false, flip: false, mountFootstep),
							new FarmerSprite.AnimationFrame(5, 70),
							new FarmerSprite.AnimationFrame(6, 70)
						});
					}
					ridingAnimationDirection = base.FacingDirection;
				}
				if (!num)
				{
					Sprite.StopAnimation();
					faceDirection(rider.FacingDirection);
				}
			}
			if (base.FacingDirection == 3)
			{
				drawOffset.Set(Vector2.Zero);
			}
			else
			{
				drawOffset.Set(new Vector2(-16f, 0f));
			}
			flip = (base.FacingDirection == 3);
			base.update(time, location);
		}

		public override void collisionWithFarmerBehavior()
		{
			base.collisionWithFarmerBehavior();
		}

		public void dismount(bool from_demolish = false)
		{
			mutex.ReleaseLock();
			rider.mount = null;
			if (base.currentLocation != null)
			{
				Stable stable = null;
				foreach (Building building in Game1.getFarm().buildings)
				{
					if (building is Stable && (building as Stable).HorseId == HorseId)
					{
						stable = (building as Stable);
						break;
					}
				}
				if (stable != null && !from_demolish && !base.currentLocation.characters.Where((NPC c) => c is Horse && (c as Horse).HorseId == HorseId).Any())
				{
					base.currentLocation.characters.Add(this);
				}
				SyncPositionToRider();
				rider.TemporaryPassableTiles.Add(new Rectangle((int)dismountTile.X * 64, (int)dismountTile.Y * 64, 64, 64));
				rider.freezePause = -1;
				dismounting.Value = false;
				rider.isAnimatingMount = false;
				rider.canMove = true;
				rider.forceCanMove();
				rider.xOffset = 0f;
				rider = null;
				Halt();
				farmerPassesThrough = false;
			}
		}

		public void nameHorse(string name)
		{
			if (name.Length > 0)
			{
				Game1.multiplayer.globalChatInfoMessage("HorseNamed", Game1.player.Name, name);
				foreach (NPC i in Utility.getAllCharacters())
				{
					if (i.isVillager() && i.Name.Equals(name))
					{
						name += " ";
					}
				}
				base.Name = name;
				base.displayName = name;
				if (Game1.player.horseName.Value == null)
				{
					Game1.player.horseName.Value = name;
				}
				Game1.exitActiveMenu();
				Game1.playSound("newArtifact");
				if (mutex.IsLockHeld())
				{
					mutex.ReleaseLock();
				}
			}
		}

		public override bool checkAction(Farmer who, GameLocation l)
		{
			if (rider == null)
			{
				mutex.RequestLock(delegate
				{
					if (who.mount != null || rider != null || who.FarmerSprite.PauseForSingleAnimation)
					{
						mutex.ReleaseLock();
					}
					else if ((getOwner() == Game1.player || (getOwner() == null && (Game1.player.horseName.Value == null || Game1.player.horseName.Value.Length == 0 || Utility.findHorseForPlayer(Game1.player.UniqueMultiplayerID) == null))) && base.Name.Length <= 0)
					{
						foreach (Building current in (Game1.getLocationFromName("Farm") as Farm).buildings)
						{
							if ((int)current.daysOfConstructionLeft <= 0 && current is Stable)
							{
								Stable stable = current as Stable;
								if (stable.getStableHorse() == this)
								{
									stable.owner.Value = who.UniqueMultiplayerID;
									stable.updateHorseOwnership();
								}
								else if ((long)stable.owner == who.UniqueMultiplayerID)
								{
									stable.owner.Value = 0L;
									stable.updateHorseOwnership();
								}
							}
						}
						if (Game1.player.horseName.Value == null || Game1.player.horseName.Value.Length == 0)
						{
							Game1.activeClickableMenu = new NamingMenu(nameHorse, Game1.content.LoadString("Strings\\Characters:NameYourHorse"), Game1.content.LoadString("Strings\\Characters:DefaultHorseName"));
						}
					}
					else if (who.items.Count > who.CurrentToolIndex && who.items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat)
					{
						if (hat.Value != null)
						{
							Game1.createItemDebris((Hat)hat, base.position, facingDirection);
							hat.Value = null;
						}
						else
						{
							Hat value = who.Items[who.CurrentToolIndex] as Hat;
							who.Items[who.CurrentToolIndex] = null;
							hat.Value = value;
							Game1.playSound("dirtyHit");
						}
						mutex.ReleaseLock();
					}
					else
					{
						rider = who;
						rider.freezePause = 5000;
						rider.synchronizedJump(6f);
						rider.Halt();
						if (rider.Position.X < base.Position.X)
						{
							rider.faceDirection(1);
						}
						l.playSound("dwop");
						mounting.Value = true;
						rider.isAnimatingMount = true;
						rider.completelyStopAnimatingOrDoingAction();
						rider.faceGeneralDirection(Utility.PointToVector2(GetBoundingBox().Center));
					}
				});
				return true;
			}
			dismounting.Value = true;
			rider.isAnimatingMount = true;
			farmerPassesThrough = false;
			rider.TemporaryPassableTiles.Clear();
			Vector2 position = Utility.recursiveFindOpenTileForCharacter(rider, rider.currentLocation, rider.getTileLocation(), 8);
			base.Position = new Vector2(position.X * 64f + 32f - (float)(GetBoundingBox().Width / 2), position.Y * 64f + 4f);
			roomForHorseAtDismountTile = !base.currentLocation.isCollidingPosition(GetBoundingBox(), Game1.viewport, isFarmer: true, 0, glider: false, this);
			base.Position = rider.Position;
			dismounting.Value = false;
			rider.isAnimatingMount = false;
			Halt();
			if (!position.Equals(Vector2.Zero) && Vector2.Distance(position, rider.getTileLocation()) < 2f)
			{
				rider.synchronizedJump(6f);
				l.playSound("dwop");
				rider.freezePause = 5000;
				rider.Halt();
				rider.xOffset = 0f;
				dismounting.Value = true;
				rider.isAnimatingMount = true;
				dismountTile = position;
				Game1.debugOutput = "dismount tile: " + position.ToString();
			}
			else
			{
				dismount();
			}
			return true;
		}

		public void SyncPositionToRider()
		{
			if (rider != null && (!dismounting || roomForHorseAtDismountTile))
			{
				base.Position = rider.position;
			}
		}

		public override void draw(SpriteBatch b)
		{
			flip = (base.FacingDirection == 3);
			Sprite.UpdateSourceRect();
			base.draw(b);
			if (base.FacingDirection == 2 && rider != null)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(48f, -24f - rider.yOffset), new Rectangle(160, 96, 9, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (base.Position.Y + 64f) / 10000f);
			}
			bool draw_hat = true;
			if (hat.Value == null)
			{
				return;
			}
			Vector2 hatOffset = Vector2.Zero;
			switch ((int)hat.Value.which)
			{
			case 14:
				if ((int)facingDirection == 0)
				{
					hatOffset.X = -100f;
				}
				break;
			case 6:
				hatOffset.Y += 2f;
				if (base.FacingDirection == 2)
				{
					hatOffset.Y -= 1f;
				}
				break;
			case 10:
				hatOffset.Y += 3f;
				if ((int)facingDirection == 0)
				{
					draw_hat = false;
				}
				break;
			case 9:
			case 32:
				if (base.FacingDirection == 0 || base.FacingDirection == 2)
				{
					hatOffset.Y += 1f;
				}
				break;
			case 31:
				hatOffset.Y += 1f;
				break;
			case 11:
			case 39:
				if (base.FacingDirection == 3 || base.FacingDirection == 1)
				{
					if (flip)
					{
						hatOffset.X += 2f;
					}
					else
					{
						hatOffset.X -= 2f;
					}
				}
				break;
			case 26:
				if (base.FacingDirection == 3 || base.FacingDirection == 1)
				{
					if (flip)
					{
						hatOffset.X += 1f;
					}
					else
					{
						hatOffset.X -= 1f;
					}
				}
				break;
			case 56:
			case 67:
				if (base.FacingDirection == 0)
				{
					draw_hat = false;
				}
				break;
			}
			hatOffset *= 4f;
			if (shakeTimer > 0)
			{
				hatOffset += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			if (hatOffset.X <= -100f)
			{
				return;
			}
			float horse_draw_layer2 = (float)GetBoundingBox().Center.Y / 10000f;
			if (rider != null)
			{
				horse_draw_layer2 = ((base.FacingDirection == 0) ? ((position.Y + 64f - 32f) / 10000f) : ((base.FacingDirection != 2) ? ((position.Y + 64f - 1f) / 10000f) : ((position.Y + 64f + (float)((rider != null) ? 1 : 1)) / 10000f)));
			}
			if (!draw_hat)
			{
				return;
			}
			horse_draw_layer2 += 1E-07f;
			switch (Sprite.CurrentFrame)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
				hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f - ((rider != null) ? rider.yOffset : 0f))), 1.33333337f, 1f, horse_draw_layer2, 2);
				break;
			case 7:
			case 11:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -74f), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -74f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 8:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -74f), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -74f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 9:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -70f), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -70f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 10:
				if (flip)
				{
					hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -70f), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -70f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 12:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -78f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -78f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 13:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -78f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -78f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 21:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-14f, -66f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(66f, -66f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 22:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -54f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -54f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 23:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 24:
				if (flip)
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(-18f, -42f)), 1.33333337f, 1f, horse_draw_layer2, 3);
				}
				else
				{
					hat.Value.draw(b, Utility.snapDrawPosition(getLocalPosition(Game1.viewport) + hatOffset + new Vector2(70f, -42f)), 1.33333337f, 1f, horse_draw_layer2, 1);
				}
				break;
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 25:
				hat.Value.draw(b, getLocalPosition(Game1.viewport) + hatOffset + new Vector2(28f, -106f - ((rider != null) ? rider.yOffset : 0f)), 1.33333337f, 1f, horse_draw_layer2, 0);
				break;
			}
		}
	}
}
