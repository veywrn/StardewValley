using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using xTile;

namespace StardewValley.Locations
{
	public class IslandForestLocation : IslandLocation
	{
		protected Color _ambientLightColor = Color.White;

		private List<Wisp> _wisps;

		private List<WeatherDebris> weatherDebris;

		protected Texture2D _rayTexture;

		protected Random _rayRandom;

		protected int _raySeed;

		public IslandForestLocation()
		{
		}

		public IslandForestLocation(string map, string name)
			: base(map, name)
		{
		}

		public override void seasonUpdate(string season, bool onLoad = false)
		{
		}

		public override void updateSeasonalTileSheets(Map map = null)
		{
		}

		public override void tryToAddCritters(bool onlyIfOnScreen = false)
		{
		}

		protected override void resetLocalState()
		{
			_raySeed = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
			_rayTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LightRays");
			_ambientLightColor = new Color(150, 120, 50);
			ignoreOutdoorLighting.Value = false;
			base.resetLocalState();
			_updateWoodsLighting();
			new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2);
			_wisps = new List<Wisp>();
			for (int i = 0; i < 30; i++)
			{
				Wisp wisp = new Wisp(i);
				_wisps.Add(wisp);
			}
			weatherDebris = new List<WeatherDebris>();
			int spacing = 192;
			int leafType = 3;
			for (int j = 0; j < 10; j++)
			{
				weatherDebris.Add(new WeatherDebris(new Vector2(j * spacing % Game1.graphics.GraphicsDevice.Viewport.Width + Game1.random.Next(spacing), j * spacing / Game1.graphics.GraphicsDevice.Viewport.Width * spacing % Game1.graphics.GraphicsDevice.Viewport.Height + Game1.random.Next(spacing)), leafType, (float)Game1.random.Next(15) / 500f, (float)Game1.random.Next(-10, 0) / 50f, (float)Game1.random.Next(10) / 50f));
			}
		}

		public override void cleanupBeforePlayerExit()
		{
			if (_wisps != null)
			{
				_wisps.Clear();
			}
			if (weatherDebris != null)
			{
				weatherDebris.Clear();
			}
			if (Game1.locationRequest.Location != null && !(Game1.locationRequest.Location is IslandForestLocation) && !(Game1.locationRequest.Location is IslandHut))
			{
				Game1.changeMusicTrack("none");
			}
			base.cleanupBeforePlayerExit();
		}

		public override void UpdateWhenCurrentLocation(GameTime time)
		{
			base.UpdateWhenCurrentLocation(time);
			_updateWoodsLighting();
			if (_wisps != null)
			{
				for (int i = 0; i < _wisps.Count; i++)
				{
					_wisps[i].Update(time);
				}
			}
			if (weatherDebris != null)
			{
				foreach (WeatherDebris weatherDebri in weatherDebris)
				{
					weatherDebri.update();
				}
				Game1.updateDebrisWeatherForMovement(weatherDebris);
			}
		}

		protected void _updateWoodsLighting()
		{
			if (Game1.currentLocation == this)
			{
				int fade_start_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime()) - 60;
				int fade_end_time = Utility.ConvertTimeToMinutes(Game1.getTrulyDarkTime());
				int light_fade_start_time = Utility.ConvertTimeToMinutes(Game1.getStartingToGetDarkTime());
				int light_fade_end_time = Utility.ConvertTimeToMinutes(Game1.getModeratelyDarkTime());
				float num = (float)Utility.ConvertTimeToMinutes(Game1.timeOfDay) + (float)Game1.gameTimeInterval / 7000f * 10f;
				float lerp = Utility.Clamp((num - (float)fade_start_time) / (float)(fade_end_time - fade_start_time), 0f, 1f);
				float light_lerp = Utility.Clamp((num - (float)light_fade_start_time) / (float)(light_fade_end_time - light_fade_start_time), 0f, 1f);
				Game1.ambientLight.R = (byte)Utility.Lerp((int)_ambientLightColor.R, (int)Game1.eveningColor.R, lerp);
				Game1.ambientLight.G = (byte)Utility.Lerp((int)_ambientLightColor.G, (int)Game1.eveningColor.G, lerp);
				Game1.ambientLight.B = (byte)Utility.Lerp((int)_ambientLightColor.B, (int)Game1.eveningColor.B, lerp);
				Game1.ambientLight.A = (byte)Utility.Lerp((int)_ambientLightColor.A, (int)Game1.eveningColor.A, lerp);
				Color light_color = Color.Black;
				light_color.A = (byte)Utility.Lerp(255f, 0f, light_lerp);
				foreach (LightSource light in Game1.currentLightSources)
				{
					if (light.lightContext.Value == LightSource.LightContext.MapLight)
					{
						light.color.Value = light_color;
					}
				}
			}
		}

		public override void draw(SpriteBatch b)
		{
			base.draw(b);
			if (_wisps != null)
			{
				for (int i = 0; i < _wisps.Count; i++)
				{
					_wisps[i].Draw(b);
				}
			}
		}

		public virtual void DrawRays(SpriteBatch b)
		{
			Random random = new Random(_raySeed);
			float zoom = (float)Game1.graphics.GraphicsDevice.Viewport.Height * 0.6f / 128f;
			int num = -(int)(128f / zoom);
			int max = Game1.graphics.GraphicsDevice.Viewport.Width / (int)(32f * zoom);
			for (int i = num; i < max; i++)
			{
				Color color = Color.White;
				float deg2 = (float)Game1.viewport.X * Utility.RandomFloat(0.75f, 1f, random) + (float)Game1.viewport.Y * Utility.RandomFloat(0.2f, 0.5f, random) + (float)Game1.currentGameTime.TotalGameTime.TotalSeconds * 20f;
				_ = deg2 / 360f;
				deg2 %= 360f;
				float rad = deg2 * ((float)Math.PI / 180f);
				color *= Utility.Clamp((float)Math.Sin(rad), 0f, 1f) * Utility.RandomFloat(0.15f, 0.4f, random);
				float offset = Utility.Lerp(0f - Utility.RandomFloat(24f, 32f, random), 0f, deg2 / 360f);
				b.Draw(_rayTexture, new Vector2(((float)(i * 32) - offset) * zoom, Utility.RandomFloat(0f, -32f * zoom, random)), new Rectangle(128 * random.Next(0, 2), 0, 128, 128), color, 0f, Vector2.Zero, zoom, SpriteEffects.None, 1f);
			}
		}

		public override void drawWater(SpriteBatch b)
		{
			base.drawWater(b);
		}

		public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
		{
			base.drawAboveAlwaysFrontLayer(b);
			DrawRays(b);
		}
	}
}
