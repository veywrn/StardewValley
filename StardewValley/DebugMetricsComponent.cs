using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace StardewValley
{
	public class DebugMetricsComponent : DrawableGameComponent
	{
		private readonly Game _game;

		private SpriteFont _font;

		private SpriteBatch _spriteBatch;

		private int _drawX;

		private int _drawY;

		private double _fps;

		private double _mspf;

		private int _lastCollection;

		private float _lastBaseMB;

		private bool _runningSlowly;

		private StringBuilder _stringBuilder = new StringBuilder(512);

		private Texture2D _opaqueWhite;

		public int XOffset = 10;

		public int YOffset = 10;

		private IBandwidthMonitor bandwidthMonitor;

		private BarGraph bandwidthUpGraph;

		private BarGraph bandwidthDownGraph;

		public SpriteFont Font
		{
			get
			{
				return _font;
			}
			set
			{
				_font = value;
			}
		}

		public DebugMetricsComponent(Game game)
			: base(game)
		{
			_game = game;
			base.DrawOrder = int.MaxValue;
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(base.GraphicsDevice);
			int w = 2;
			int h = 2;
			_opaqueWhite = new Texture2D(base.GraphicsDevice, w, h, mipMap: false, SurfaceFormat.Color);
			Color[] data = new Color[w * h];
			_opaqueWhite.GetData(data);
			for (int i = 0; i < w * h; i++)
			{
				data[i] = Color.White;
			}
			_opaqueWhite.SetData(data);
			base.LoadContent();
		}

		public override void Update(GameTime gameTime)
		{
			if (Game1.IsServer)
			{
				bandwidthMonitor = Game1.server;
			}
			else if (Game1.IsClient)
			{
				bandwidthMonitor = Game1.client;
			}
			else
			{
				bandwidthMonitor = null;
			}
			if (bandwidthMonitor == null || !bandwidthMonitor.LogBandwidth)
			{
				bandwidthDownGraph = null;
				bandwidthUpGraph = null;
			}
			if (bandwidthMonitor != null && bandwidthMonitor.LogBandwidth && (bandwidthDownGraph == null || bandwidthUpGraph == null))
			{
				int barGraphWidth = 200;
				int barGraphHeight = 150;
				int buffer = 50;
				bandwidthUpGraph = new BarGraph(bandwidthMonitor.BandwidthLogger.LoggedAvgBitsUp, Game1.viewport.Width - barGraphWidth - buffer, buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Yellow * 0.8f, _opaqueWhite);
				bandwidthDownGraph = new BarGraph(bandwidthMonitor.BandwidthLogger.LoggedAvgBitsDown, Game1.viewport.Width - barGraphWidth - buffer, buffer + barGraphHeight + buffer, barGraphWidth, barGraphHeight, 2, BarGraph.DYNAMIC_SCALE_MAX, Color.Cyan * 0.8f, _opaqueWhite);
			}
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Game1.displayHUD || !Game1.debugMode)
			{
				return;
			}
			if (gameTime.ElapsedGameTime.TotalSeconds > 0.0)
			{
				_fps = 1.0 / gameTime.ElapsedGameTime.TotalSeconds;
				_mspf = gameTime.ElapsedGameTime.TotalSeconds * 1000.0;
			}
			if (gameTime.IsRunningSlowly)
			{
				_runningSlowly = true;
			}
			if (_font == null)
			{
				return;
			}
			_spriteBatch.Begin();
			_drawX = XOffset;
			_drawY = YOffset;
			StringBuilder sb = _stringBuilder;
			Utility.makeSafe(ref _drawX, ref _drawY, 0, 0);
			int collection = GC.CollectionCount(0);
			float memory = (float)GC.GetTotalMemory(forceFullCollection: false) / 1048576f;
			if (_lastCollection != collection)
			{
				_lastCollection = collection;
				_lastBaseMB = memory;
			}
			float diff = memory - _lastBaseMB;
			sb.AppendFormatEx("FPS: {0,3}   GC: {1,3}   {2:0.00}MB   +{3:0.00}MB", (int)Math.Round(_fps), _lastCollection % 1000, _lastBaseMB, diff);
			Color col = Color.Yellow;
			if (_runningSlowly)
			{
				sb.Append("   [IsRunningSlowly]");
				_runningSlowly = false;
				col = Color.Red;
			}
			DrawLine(col, sb, _drawX);
			if (Game1.IsMultiplayer)
			{
				col = Color.Yellow;
				if (Game1.IsServer)
				{
					foreach (KeyValuePair<long, Farmer> farmer in Game1.otherFarmers)
					{
						sb.AppendFormat("Ping({0}): {1:0.0}ms", farmer.Value.Name, Game1.server.getPingToClient(farmer.Key));
						DrawLine(col, sb, _drawX);
					}
				}
				else
				{
					sb.AppendFormat("Ping: {0:0.0}ms", Game1.client.GetPingToHost());
					DrawLine(col, sb, _drawX);
				}
			}
			if (bandwidthMonitor != null && bandwidthMonitor.LogBandwidth)
			{
				sb.AppendFormat("Up - b/s: {0}  Avg b/s: {1}", (int)bandwidthMonitor.BandwidthLogger.BitsUpPerSecond, (int)bandwidthMonitor.BandwidthLogger.AvgBitsUpPerSecond);
				DrawLine(col, sb, _drawX);
				sb.AppendFormat("Down - b/s: {0}  Avg b/s: {1}", (int)bandwidthMonitor.BandwidthLogger.BitsDownPerSecond, (int)bandwidthMonitor.BandwidthLogger.AvgBitsDownPerSecond);
				DrawLine(col, sb, _drawX);
				sb.AppendFormat("Total MB Up: {0:0.00}  Total MB Down: {1:0.00}  Total Seconds: {2:0.00}", (float)bandwidthMonitor.BandwidthLogger.TotalBitsUp / 8f / 1000f / 1000f, (float)bandwidthMonitor.BandwidthLogger.TotalBitsDown / 8f / 1000f / 1000f, (float)bandwidthMonitor.BandwidthLogger.TotalMs / 1000f);
				DrawLine(col, sb, _drawX);
				if (bandwidthUpGraph != null && bandwidthDownGraph != null)
				{
					bandwidthUpGraph.Draw(_spriteBatch);
					bandwidthDownGraph.Draw(_spriteBatch);
				}
			}
			_spriteBatch.End();
		}

		private void DrawLine(Color color, StringBuilder sb, int x)
		{
			if (sb != null)
			{
				Vector2 size = _font.MeasureString(sb);
				int y = _drawY;
				int yoffset2 = _font.LineSpacing;
				yoffset2 -= yoffset2 / 10;
				_spriteBatch.Draw(_opaqueWhite, new Rectangle(x - 1, y, (int)size.X + 2, yoffset2), null, Color.Black * 0.5f);
				_spriteBatch.DrawString(_font, sb, new Vector2(x, y), color);
				_drawY += yoffset2;
				sb.Clear();
			}
		}
	}
}
