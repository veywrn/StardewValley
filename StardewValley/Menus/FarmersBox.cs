using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace StardewValley.Menus
{
	internal class FarmersBox : IClickableMenu
	{
		private readonly List<Farmer> _farmers = new List<Farmer>();

		private readonly Texture2D _iconTex;

		public float _updateTimer;

		private readonly List<FarmerBoxButton> _profileButtons;

		private readonly List<FarmerBoxButton> _muteButtons;

		public FarmersBox()
			: base(0, 200, 528, 400)
		{
			_muteButtons = new List<FarmerBoxButton>();
			_profileButtons = new List<FarmerBoxButton>();
		}

		private void UpdateFarmers(List<ClickableComponent> parentComponents)
		{
			if (!(_updateTimer > 0f))
			{
				_farmers.Clear();
				foreach (Farmer farmer in Game1.getOnlineFarmers())
				{
					_farmers.Add(farmer);
				}
				_updateTimer = 1f;
			}
		}

		public override void receiveLeftClick(int x, int y, bool playSound = true)
		{
		}

		public override void update(GameTime time)
		{
			_updateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
		}

		public void draw(SpriteBatch b, int left, int bottom, ClickableComponent current, List<ClickableComponent> parentComponents)
		{
			UpdateFarmers(parentComponents);
			if (_farmers.Count == 0)
			{
				return;
			}
			int sizeY = 100;
			height = sizeY * _farmers.Count;
			xPositionOnScreen = left;
			yPositionOnScreen = bottom - height;
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(301, 288, 15, 15), xPositionOnScreen, yPositionOnScreen, width, height, Color.White, 4f, drawShadow: false);
			b.End();
			b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
			Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
			int x = xPositionOnScreen + 16;
			int y = yPositionOnScreen;
			for (int i = 0; i < _farmers.Count; i++)
			{
				Farmer farmer = _farmers[i];
				Rectangle newClip = origClip;
				newClip.X = x;
				newClip.Y = y;
				newClip.Height = sizeY - 8;
				newClip.Width = 200;
				b.GraphicsDevice.ScissorRectangle = newClip;
				FarmerRenderer.isDrawingForUI = true;
				farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes ? 108 : 0, 0, secondaryArm: false, flip: false), farmer.bathingClothes ? 108 : 0, new Rectangle(0, farmer.bathingClothes ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
				FarmerRenderer.isDrawingForUI = false;
				b.GraphicsDevice.ScissorRectangle = origClip;
				int textX = x + 80;
				int textY2 = y + 12;
				string farmerName = ChatBox.formattedUserName(farmer);
				b.DrawString(Game1.dialogueFont, farmerName, new Vector2(textX, textY2), Color.White);
				string platformUserName = Game1.multiplayer.getUserName(farmer.UniqueMultiplayerID);
				if (!string.IsNullOrEmpty(platformUserName))
				{
					textY2 += Game1.dialogueFont.LineSpacing + 4;
					string userName = "(" + platformUserName + ")";
					b.DrawString(Game1.smallFont, userName, new Vector2(textX, textY2), Color.White);
				}
				y += sizeY;
			}
			b.GraphicsDevice.ScissorRectangle = origClip;
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
		}
	}
}
