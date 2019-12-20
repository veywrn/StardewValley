using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using System;

namespace StardewValley.Buildings
{
	public class Stable : Building
	{
		private readonly NetGuid horseId = new NetGuid();

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

		public Stable()
		{
		}

		public Stable(Guid horseId, BluePrint b, Vector2 tileLocation)
			: base(b, tileLocation)
		{
			HorseId = horseId;
		}

		protected override void initNetFields()
		{
			base.initNetFields();
			base.NetFields.AddField(horseId);
		}

		public override Rectangle getSourceRectForMenu()
		{
			return new Rectangle(0, 0, texture.Value.Bounds.Width, texture.Value.Bounds.Height);
		}

		public Horse getStableHorse()
		{
			return Utility.findHorse(HorseId);
		}

		public virtual void grabHorse()
		{
			if ((int)daysOfConstructionLeft <= 0)
			{
				Horse horse = Utility.findHorse(HorseId);
				if (horse == null)
				{
					horse = new Horse(HorseId, (int)tileX + 1, (int)tileY + 1);
					Game1.getFarm().characters.Add(horse);
				}
				else
				{
					Game1.warpCharacter(horse, "Farm", new Point((int)tileX + 1, (int)tileY + 1));
				}
				horse.ownerId.Value = owner.Value;
			}
		}

		public virtual void updateHorseOwnership()
		{
			if ((int)daysOfConstructionLeft > 0)
			{
				return;
			}
			Horse horse = Utility.findHorse(HorseId);
			if (horse == null)
			{
				return;
			}
			horse.ownerId.Value = owner.Value;
			if (horse.getOwner() != null)
			{
				if (horse.getOwner().horseName.Value != null)
				{
					horse.name.Value = horse.getOwner().horseName.Value;
					horse.displayName = horse.getOwner().horseName.Value;
				}
				else
				{
					horse.name.Value = "";
					horse.displayName = "";
				}
			}
		}

		public override void dayUpdate(int dayOfMonth)
		{
			base.dayUpdate(dayOfMonth);
			grabHorse();
		}

		public override bool intersects(Rectangle boundingBox)
		{
			if ((int)daysOfConstructionLeft > 0)
			{
				return base.intersects(boundingBox);
			}
			if (base.intersects(boundingBox))
			{
				if (boundingBox.X >= ((int)tileX + 1) * 64 && boundingBox.Right < ((int)tileX + 3) * 64)
				{
					return boundingBox.Y <= ((int)tileY + 1) * 64;
				}
				return true;
			}
			return false;
		}

		public override void performActionOnDemolition(GameLocation location)
		{
			Horse horse = getStableHorse();
			if (horse != null && horse.currentLocation != null)
			{
				horse.currentLocation.characters.Remove(horse);
			}
			Game1.player.team.demolishStableEvent.Fire(HorseId);
			base.performActionOnDemolition(location);
		}

		public override void Update(GameTime time)
		{
			base.Update(time);
		}

		public override void draw(SpriteBatch b)
		{
			if (!base.isMoving)
			{
				if ((int)daysOfConstructionLeft > 0)
				{
					drawInConstruction(b);
					return;
				}
				drawShadow(b);
				b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64)), texture.Value.Bounds, color.Value * alpha, 0f, new Vector2(0f, texture.Value.Bounds.Height), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh - 1) * 64) / 10000f);
			}
		}
	}
}
