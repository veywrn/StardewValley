using Microsoft.Xna.Framework.Audio;
using System;

namespace StardewValley
{
	public interface ISoundBank : IDisposable
	{
		bool IsInUse
		{
			get;
		}

		bool IsDisposed
		{
			get;
		}

		ICue GetCue(string name);

		void PlayCue(string name);

		void PlayCue(string name, AudioListener listener, AudioEmitter emitter);
	}
}
