using Microsoft.Xna.Framework.Audio;
using System;

namespace StardewValley
{
	public interface IAudioEngine : IDisposable
	{
		bool IsDisposed
		{
			get;
		}

		AudioEngine Engine
		{
			get;
		}

		void Update();

		IAudioCategory GetCategory(string name);
	}
}
