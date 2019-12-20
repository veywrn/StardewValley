using Microsoft.Xna.Framework.Audio;
using System;

namespace StardewValley
{
	internal class DummyAudioEngine : IAudioEngine, IDisposable
	{
		private IAudioCategory category = new DummyAudioCategory();

		public bool IsDisposed => true;

		public AudioEngine Engine => null;

		public void Update()
		{
		}

		public IAudioCategory GetCategory(string name)
		{
			return category;
		}

		public void Dispose()
		{
		}
	}
}
