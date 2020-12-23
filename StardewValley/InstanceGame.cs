using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace StardewValley
{
	public class InstanceGame
	{
		public object staticVarHolder;

		public bool IsMainInstance
		{
			get
			{
				if (GameRunner.instance.gameInstances.Count != 0)
				{
					return GameRunner.instance.gameInstances[0] == this;
				}
				return true;
			}
		}

		public GraphicsDevice GraphicsDevice => GameRunner.instance.GraphicsDevice;

		public ContentManager Content => GameRunner.instance.Content;

		public GameComponentCollection Components => GameRunner.instance.Components;

		public GameWindow Window => GameRunner.instance.Window;

		public bool IsFixedTimeStep
		{
			get
			{
				return GameRunner.instance.IsFixedTimeStep;
			}
			set
			{
				GameRunner.instance.IsFixedTimeStep = value;
			}
		}

		public bool IsActive => GameRunner.instance.IsActive;

		public bool IsMouseVisible
		{
			get
			{
				return GameRunner.instance.IsMouseVisible;
			}
			set
			{
				GameRunner.instance.IsMouseVisible = value;
			}
		}

		public TimeSpan TargetElapsedTime
		{
			get
			{
				return GameRunner.instance.TargetElapsedTime;
			}
			set
			{
				GameRunner.instance.TargetElapsedTime = value;
			}
		}

		protected virtual void Initialize()
		{
		}

		protected virtual void LoadContent()
		{
		}

		protected virtual void UnloadContent()
		{
		}

		protected virtual void Update(GameTime game_time)
		{
		}

		protected virtual void OnActivated(object sender, EventArgs args)
		{
		}

		protected virtual void Draw(GameTime game_time)
		{
		}

		protected virtual void BeginDraw()
		{
		}

		protected virtual void EndDraw()
		{
		}

		public void Exit()
		{
			GameRunner.instance.Exit();
		}
	}
}
