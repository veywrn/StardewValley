namespace Microsoft.Xna.Framework.Graphics
{
	public static class ViewportExtensions
	{
		public static Rectangle GetTitleSafeArea(this Viewport vp)
		{
			return vp.TitleSafeArea;
		}
	}
}
