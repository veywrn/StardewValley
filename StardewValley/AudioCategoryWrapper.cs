using Microsoft.Xna.Framework.Audio;

namespace StardewValley
{
	public class AudioCategoryWrapper : IAudioCategory
	{
		private AudioCategory audioCategory;

		public AudioCategoryWrapper(AudioCategory category)
		{
			audioCategory = category;
		}

		public void SetVolume(float volume)
		{
			audioCategory.SetVolume(volume);
		}
	}
}
