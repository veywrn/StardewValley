using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace StardewValley
{
	public static class KeyboardInput
	{
		private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		private static bool initialized;

		private static IntPtr prevWndProc;

		private static WndProc hookProcDelegate;

		private static IntPtr hIMC;

		private const int GWL_WNDPROC = -4;

		private const int WM_KEYDOWN = 256;

		private const int WM_KEYUP = 257;

		private const int WM_CHAR = 258;

		private const int WM_IME_SETCONTEXT = 641;

		private const int WM_INPUTLANGCHANGE = 81;

		private const int WM_GETDLGCODE = 135;

		private const int WM_IME_COMPOSITION = 271;

		private const int DLGC_WANTALLKEYS = 4;

		public static event CharEnteredHandler CharEntered;

		public static event KeyEventHandler KeyDown;

		public static event KeyEventHandler KeyUp;

		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr ImmGetContext(IntPtr hWnd);

		[DllImport("Imm32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr ImmAssociateContext(IntPtr hWnd, IntPtr hIMC);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		public static void Initialize(GameWindow window)
		{
			if (initialized)
			{
				throw new InvalidOperationException("TextInput.Initialize can only be called once!");
			}
			hookProcDelegate = HookProc;
			prevWndProc = (IntPtr)SetWindowLong(window.Handle, -4, (int)Marshal.GetFunctionPointerForDelegate((Delegate)hookProcDelegate));
			hIMC = ImmGetContext(window.Handle);
			initialized = true;
		}

		private static IntPtr HookProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			IntPtr returnCode = CallWindowProc(prevWndProc, hWnd, msg, wParam, lParam);
			switch (msg)
			{
			case 135u:
				returnCode = (IntPtr)(returnCode.ToInt32() | 4);
				break;
			case 256u:
				if (KeyboardInput.KeyDown != null)
				{
					KeyboardInput.KeyDown(null, new KeyEventArgs((Keys)(int)wParam));
				}
				break;
			case 257u:
				if (KeyboardInput.KeyUp != null)
				{
					KeyboardInput.KeyUp(null, new KeyEventArgs((Keys)(int)wParam));
				}
				break;
			case 258u:
				if (KeyboardInput.CharEntered != null)
				{
					KeyboardInput.CharEntered(null, new CharacterEventArgs((char)(int)wParam, lParam.ToInt32()));
				}
				break;
			case 641u:
				if (wParam.ToInt32() == 1)
				{
					ImmAssociateContext(hWnd, hIMC);
				}
				break;
			case 81u:
				ImmAssociateContext(hWnd, hIMC);
				returnCode = (IntPtr)1;
				break;
			}
			return returnCode;
		}
	}
}
