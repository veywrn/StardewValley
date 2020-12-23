using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Minigames;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using xTile.Dimensions;

namespace StardewValley.Locations
{
	public class BoatTunnel : GameLocation
	{
		public enum TunnelAnimationState
		{
			Idle,
			MoveWillyToGate,
			OpenGate,
			MoveWillyToCockpit,
			MoveFarmer,
			MovePlank,
			CloseGate,
			MoveBoat
		}

		private Texture2D boatTexture;

		private Vector2 boatPosition;

		public Microsoft.Xna.Framework.Rectangle gateRect = new Microsoft.Xna.Framework.Rectangle(0, 120, 32, 40);

		protected int _gateFrame;

		protected int _gateDirection;

		protected float _gateFrameTimer;

		public const float GATE_SECONDS_PER_FRAME = 0.1f;

		public const int GATE_FRAMES = 5;

		protected int _boatOffset;

		protected int _boatDirection;

		public const int PLANK_MAX_OFFSET = 16;

		public float _plankPosition;

		public float _plankDirection;

		protected Farmer _farmerActor;

		protected Event _boatEvent;

		protected bool _playerPathing;

		protected int nonBlockingPause;

		protected float _nextBubble;

		protected float _nextSlosh;

		protected float _nextSmoke;

		protected float _plankShake;

		protected int forceWarpTimer;

		protected bool _boatAnimating;

		public TunnelAnimationState animationState;

		public BoatTunnel()
		{
		}

		public BoatTunnel(string map, string name)
			: base(map, name)
		{
		}

		public virtual bool GateFinishedAnimating()
		{
			if (_gateDirection < 0)
			{
				return _gateFrame <= 0;
			}
			if (_gateDirection > 0)
			{
				return _gateFrame >= 5;
			}
			return true;
		}

		public virtual bool PlankFinishedAnimating()
		{
			if (_plankDirection < 0f)
			{
				return _plankPosition <= 0f;
			}
			if (_plankDirection > 0f)
			{
				return _plankPosition >= 16f;
			}
			return true;
		}

		public virtual void SetCurrentState(TunnelAnimationState animation_state)
		{
			if (animationState != animation_state)
			{
				animationState = animation_state;
				_ = 1;
			}
		}

		public virtual void UpdateGateTileProperty()
		{
			if (_gateFrame == 0)
			{
				setTileProperty(6, 8, "Back", "TemporaryBarrier", "T");
			}
			else
			{
				removeTileProperty(6, 8, "Back", "TemporaryBarrier");
			}
		}

		public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
		{
			string action = doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings");
			if (action != null && action == "BoatTicket")
			{
				if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine"))
				{
					if (who.hasItemInInventory(787, 5))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteries"), createYesNoResponses(), "WillyBoatDonateBatteries");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateBatteriesHint"));
					}
				}
				else if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
				{
					if (Game1.player.isRidingHorse() && Game1.player.mount != null)
					{
						Game1.player.mount.checkAction(Game1.player, this);
					}
					else if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.es)
					{
						createQuestionDialogueWithCustomWidth(Game1.content.LoadString("Strings\\Locations:BoatTunnel_BuyTicket", GetTicketPrice()), createYesNoResponses(), "Boat");
					}
					else
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_BuyTicket", GetTicketPrice()), createYesNoResponses(), "Boat");
					}
				}
				return true;
			}
			if (!Game1.MasterPlayer.mailReceived.Contains("willyBoatFixed"))
			{
				if (tileLocation.X == 6 && tileLocation.Y == 8 && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull"))
				{
					if (who.hasItemInInventory(709, 200))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwood"), createYesNoResponses(), "WillyBoatDonateHardwood");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateHardwoodHint"));
					}
					return true;
				}
				if (tileLocation.X == 8 && tileLocation.Y == 10 && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor"))
				{
					if (who.hasItemInInventory(337, 5))
					{
						createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridium"), createYesNoResponses(), "WillyBoatDonateIridium");
					}
					else
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_DonateIridiumHint"));
					}
					return true;
				}
			}
			return base.checkAction(tileLocation, viewport, who);
		}

		public override bool isActionableTile(int xTile, int yTile, Farmer who)
		{
			if (!Game1.MasterPlayer.mailReceived.Contains("willyBoatFixed"))
			{
				if (xTile == 6 && yTile == 8)
				{
					return true;
				}
				if (xTile == 8 && yTile == 10)
				{
					return true;
				}
			}
			return base.isActionableTile(xTile, yTile, who);
		}

		public int GetTicketPrice()
		{
			return 1000;
		}

		public override bool answerDialogue(Response answer)
		{
			if (lastQuestionKey != null && afterQuestion == null)
			{
				string questionAndAnswer = lastQuestionKey.Split(' ')[0] + "_" + answer.responseKey;
				int ticket_price = GetTicketPrice();
				switch (questionAndAnswer)
				{
				case "Boat_Yes":
					if (Game1.player.Money >= ticket_price)
					{
						Game1.player.Money -= ticket_price;
						StartDeparture();
					}
					else if (Game1.player.Money < ticket_price)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
					}
					return true;
				case "WillyBoatDonateBatteries_Yes":
					Game1.multiplayer.globalChatInfoMessage("RepairBoatMachine", Game1.player.Name);
					Game1.player.removeItemsFromInventory(787, 5);
					DelayedAction.playSoundAfterDelay("openBox", 600);
					Game1.addMailForTomorrow("willyBoatTicketMachine", noLetter: true, sendToEveryone: true);
					checkForBoatComplete();
					return true;
				case "WillyBoatDonateHardwood_Yes":
					Game1.multiplayer.globalChatInfoMessage("RepairBoatHull", Game1.player.Name);
					Game1.player.removeItemsFromInventory(709, 200);
					DelayedAction.playSoundAfterDelay("Ship", 600);
					Game1.addMailForTomorrow("willyBoatHull", noLetter: true, sendToEveryone: true);
					checkForBoatComplete();
					return true;
				case "WillyBoatDonateIridium_Yes":
					Game1.multiplayer.globalChatInfoMessage("RepairBoatAnchor", Game1.player.Name);
					Game1.player.removeItemsFromInventory(337, 5);
					DelayedAction.playSoundAfterDelay("clank", 600);
					DelayedAction.playSoundAfterDelay("clank", 1200);
					DelayedAction.playSoundAfterDelay("clank", 1800);
					Game1.addMailForTomorrow("willyBoatAnchor", noLetter: true, sendToEveryone: true);
					checkForBoatComplete();
					return true;
				}
			}
			return base.answerDialogue(answer);
		}

		private void checkForBoatComplete()
		{
			if (Game1.player.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.player.hasOrWillReceiveMail("willyBoatHull") && Game1.player.hasOrWillReceiveMail("willyBoatAnchor"))
			{
				Game1.player.freezePause = 1500;
				DelayedAction.functionAfterDelay(delegate
				{
					Game1.multiplayer.globalChatInfoMessage("RepairBoat");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BoatTunnel_boatcomplete"));
				}, 1500);
			}
		}

		public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
		{
			if (p.Y <= 8f)
			{
				return true;
			}
			if (p.Y <= 10f && p.X >= 4f && p.X <= 8f)
			{
				return true;
			}
			return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
		}

		public virtual void StartDeparture()
		{
			xTile.Dimensions.Rectangle viewport = Game1.viewport;
			Vector2 player_position = Game1.player.Position;
			int player_direction = Game1.player.facingDirection;
			StringBuilder event_string2 = new StringBuilder();
			event_string2.Append("/0 0/farmer 0 0 0 Willy 6 12 0/playMusic none/skippable");
			if (Game1.stats.getStat("boatRidesToIsland") == 0)
			{
				event_string2.Append("/textAboveHead Willy \"" + Game1.content.LoadString("Strings\\Locations:BoatTunnel_willyText_firstRide") + "\"");
			}
			else if (Game1.random.NextDouble() < 0.2)
			{
				event_string2.Append("/textAboveHead Willy \"" + Game1.content.LoadString("Strings\\Locations:BoatTunnel_willyText_random" + Game1.random.Next(2)) + "\"");
			}
			event_string2.Append("/move Willy 0 -3 0/pause 500/locationSpecificCommand open_gate/viewport move 0 -1 1000/pause 500/move Willy 0 -2 3/move Willy -1 0 1/locationSpecificCommand path_player 6 5 2/move Willy 1 0 2/move Willy 0 1 2/pause 250/playSound clubhit/animate Willy false false 500 27/locationSpecificCommand retract_plank/jump Willy 4/pause 750/move Willy 0 -1 0/locationSpecificCommand close_gate/pause 200/move Willy 3 0 1/locationSpecificCommand offset_willy/move Willy 1 0 1");
			event_string2.Append("/locationSpecificCommand non_blocking_pause 1000/playerControl boatRide/playSound furnace/locationSpecificCommand animate_boat_start/locationSpecificCommand non_blocking_pause 1000/locationSpecificCommand boat_depart/locationSpecificCommand animate_boat_move/fade/viewport -5000 -5000/end tunnelDepart");
			_boatEvent = new Event(event_string2.ToString(), -78765, Game1.player)
			{
				showWorldCharacters = true,
				showGroundObjects = true,
				ignoreObjectCollisions = false
			};
			event_string2 = null;
			Event boatEvent = _boatEvent;
			boatEvent.onEventFinished = (Action)Delegate.Combine(boatEvent.onEventFinished, new Action(OnBoatEventEnd));
			currentEvent = _boatEvent;
			_boatEvent.checkForNextCommand(this, Game1.currentGameTime);
			Game1.eventUp = true;
			Game1.viewport = viewport;
			_farmerActor = (currentEvent.getCharacterByName("farmer") as Farmer);
			_farmerActor.Position = player_position;
			_farmerActor.faceDirection(player_direction);
			(currentEvent.getCharacterByName("Willy") as NPC).IsInvisible = false;
			Game1.stats.incrementStat("boatRidesToIsland", 1);
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			if (_boatDirection != 0)
			{
				_boatOffset += _boatDirection;
				if (currentEvent != null)
				{
					foreach (NPC actor in currentEvent.actors)
					{
						actor.shouldShadowBeOffset = true;
						actor.drawOffset.X = _boatOffset;
					}
					foreach (Farmer farmerActor in currentEvent.farmerActors)
					{
						farmerActor.shouldShadowBeOffset = true;
						farmerActor.drawOffset.X = _boatOffset;
					}
				}
			}
			if (!PlankFinishedAnimating())
			{
				_plankPosition += _plankDirection;
				if (PlankFinishedAnimating())
				{
					_plankDirection = 0f;
				}
			}
			if (!GateFinishedAnimating())
			{
				_gateFrameTimer += (float)time.ElapsedGameTime.TotalSeconds;
				if (_gateFrameTimer >= 0.1f)
				{
					_gateFrameTimer -= 0.1f;
					_gateFrame += _gateDirection;
				}
			}
			else
			{
				_gateFrameTimer = 0f;
			}
			if (_plankShake > 0f)
			{
				_plankShake -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_plankShake < 0f)
				{
					_plankShake = 0f;
				}
			}
			Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(24, 188, 16, 220);
			back_rectangle.X += (int)GetBoatPosition().X;
			back_rectangle.Y += (int)GetBoatPosition().Y;
			if ((float)_boatDirection != 0f)
			{
				if (_nextBubble > 0f)
				{
					_nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					Vector2 position2 = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
					TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position2, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
					sprite2.acceleration = new Vector2(-0.25f * (float)Math.Sign(_boatDirection), 0f);
					temporarySprites.Add(sprite2);
					_nextBubble = 0.01f;
				}
				if (_nextSlosh > 0f)
				{
					_nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
				}
				else
				{
					Game1.playSound("waterSlosh");
					_nextSlosh = 0.5f;
				}
			}
			if (_boatAnimating)
			{
				if (_nextSmoke > 0f)
				{
					_nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
					return;
				}
				Vector2 position = new Vector2(80f, -32f) * 4f + GetBoatPosition();
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position, flicker: false, flipped: false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f);
				sprite.acceleration = new Vector2(-0.25f, -0.15f);
				temporarySprites.Add(sprite);
				_nextSmoke = 0.2f;
			}
		}

		public virtual void OnBoatEventEnd()
		{
			if (_boatEvent != null)
			{
				foreach (NPC actor in _boatEvent.actors)
				{
					actor.shouldShadowBeOffset = false;
					actor.drawOffset.X = 0f;
				}
				foreach (Farmer farmerActor in _boatEvent.farmerActors)
				{
					farmerActor.shouldShadowBeOffset = false;
					farmerActor.drawOffset.X = 0f;
				}
				ResetBoat();
				_boatEvent = null;
				if (!Game1.player.hasOrWillReceiveMail("seenBoatJourney"))
				{
					Game1.addMailForTomorrow("seenBoatJourney", noLetter: true);
					Game1.currentMinigame = new BoatJourney();
				}
			}
		}

		public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
		{
			if (command_string == "open_gate")
			{
				if (first_run)
				{
					Game1.playSound("openChest");
				}
				_gateDirection = 1;
				if (GateFinishedAnimating())
				{
					UpdateGateTileProperty();
				}
				return GateFinishedAnimating();
			}
			if (command_string == "close_gate")
			{
				_gateDirection = -1;
				if (GateFinishedAnimating())
				{
					UpdateGateTileProperty();
				}
				return GateFinishedAnimating();
			}
			if (command_string == "non_blocking_pause")
			{
				if (first_run)
				{
					int delay = 0;
					if (args.Length < 0 || !int.TryParse(args[0], out delay))
					{
						delay = 0;
					}
					nonBlockingPause = delay;
					return false;
				}
				nonBlockingPause -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (nonBlockingPause < 0)
				{
					nonBlockingPause = 0;
					return true;
				}
				return false;
			}
			if (command_string == "path_player")
			{
				int x = 0;
				int y = 0;
				int direction = 2;
				if (args.Length < 0 || !int.TryParse(args[0], out x))
				{
					x = 0;
				}
				if (args.Length < 1 || !int.TryParse(args[1], out y))
				{
					y = 0;
				}
				if (args.Length < 2 || !int.TryParse(args[2], out direction))
				{
					direction = 2;
				}
				if (first_run)
				{
					_playerPathing = true;
					Game1.player.controller = new PathFindController(Game1.player, this, new Point(x, y), direction, OnReachedBoatDeck)
					{
						allowPlayerPathingInEvent = true
					};
					Game1.player.canOnlyWalk = false;
					Game1.player.setRunning(isRunning: true, force: true);
					if (Game1.player.mount != null)
					{
						Game1.player.mount.farmerPassesThrough = true;
					}
					forceWarpTimer = 8000;
				}
				if (forceWarpTimer > 0)
				{
					forceWarpTimer -= (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
					if (forceWarpTimer <= 0)
					{
						forceWarpTimer = 0;
						Game1.player.controller = null;
						Game1.player.setTileLocation(new Vector2(x, y));
						Game1.player.faceDirection(direction);
						OnReachedBoatDeck(Game1.player, this);
					}
				}
				return !_playerPathing;
			}
			if (command_string == "animate_boat_start")
			{
				if (first_run)
				{
					_boatAnimating = true;
					Game1.player.canOnlyWalk = false;
				}
				return true;
			}
			if (command_string == "boat_depart")
			{
				if (first_run)
				{
					_boatDirection = 1;
				}
				if (_boatOffset >= 100)
				{
					return true;
				}
				return false;
			}
			if (command_string == "retract_plank")
			{
				if (first_run)
				{
					_plankDirection = 0.25f;
				}
				return true;
			}
			if (command_string == "extend_plank")
			{
				if (first_run)
				{
					_plankDirection = -0.25f;
				}
				return true;
			}
			if (command_string == "offset_willy" && first_run)
			{
				_boatEvent.getActorByName("Willy").drawOffset.Y = -24f;
			}
			return base.RunLocationSpecificEventCommand(current_event, command_string, first_run, args);
		}

		public virtual void OnReachedBoatDeck(Character character, GameLocation location)
		{
			_playerPathing = false;
			Game1.player.controller = null;
			Game1.player.canOnlyWalk = true;
			forceWarpTimer = 0;
		}

		public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
		{
			return true;
		}

		public override Object getFish(float millisecondsAfterNibble, int bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
		{
			if (Game1.random.NextDouble() < 0.2)
			{
				return new Furniture(2418, Vector2.Zero);
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, locationName);
		}

		protected override void resetLocalState()
		{
			critters = new List<Critter>();
			base.resetLocalState();
			boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				addCritter(new CrabCritter(new Vector2(128f, 640f)));
			}
			if (Game1.random.NextDouble() < 0.10000000149011612)
			{
				addCritter(new CrabCritter(new Vector2(576f, 672f)));
			}
			ResetBoat();
		}

		public virtual void ResetBoat()
		{
			_nextSmoke = 0f;
			_nextBubble = 0f;
			_boatAnimating = false;
			boatPosition = new Vector2(52f, 36f) * 4f;
			_gateFrameTimer = 0f;
			_gateDirection = 0;
			_gateFrame = 0;
			_boatOffset = 0;
			_boatDirection = 0;
			_plankPosition = 0f;
			_plankDirection = 0f;
			UpdateGateTileProperty();
		}

		public Vector2 GetBoatPosition()
		{
			return boatPosition + new Vector2(_boatOffset, 0f);
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			Vector2 boat_position = GetBoatPosition();
			if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed") && Game1.farmEvent == null)
			{
				b.Draw(boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(4, 0, 156, 118), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, boatPosition.Y / 10000f);
				b.Draw(boatTexture, Game1.GlobalToLocal(boat_position + new Vector2(8f, 0f) * 4f), new Microsoft.Xna.Framework.Rectangle(0, 160, 128, 96), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 408f) / 10000f);
				Vector2 plank_shake = Vector2.Zero;
				if (!PlankFinishedAnimating() || _plankShake > 0f)
				{
					plank_shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(6f, 9f) * 64f + new Vector2(0f, (int)_plankPosition) * 4f + plank_shake), new Microsoft.Xna.Framework.Rectangle(128, 176, 17, 33), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (512f + _plankPosition * 4f) / 10000f);
				Microsoft.Xna.Framework.Rectangle gate_draw_rect = gateRect;
				gate_draw_rect.X = _gateFrame * gateRect.Width;
				b.Draw(boatTexture, Game1.GlobalToLocal(boat_position + new Vector2(35f, 81f) * 4f), gate_draw_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (boatPosition.Y + 428f) / 10000f);
			}
			else
			{
				b.Draw(boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(4, 259, 156, 122), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, boatPosition.Y / 10000f);
				b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(6f, 9f) * 64f + new Vector2(0f, (int)_plankPosition) * 4f), new Microsoft.Xna.Framework.Rectangle(128, 176, 17, 33), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (512f + _plankPosition * 4f) / 10000f);
				float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
				if (!Game1.eventUp)
				{
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(416f, 456f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(288f, 520f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
					if (!Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor"))
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 520f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 4f), SpriteEffects.None, 1f);
					}
				}
			}
			b.Draw(boatTexture, Game1.GlobalToLocal(new Vector2(4f, 8f) * 64f), new Microsoft.Xna.Framework.Rectangle(160, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0512f);
		}
	}
}
