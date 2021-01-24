# StardewValley

Decompiled Stardew Valley. Batteries not included.

This repository is based on the Steam release of Stardew Valley for Windows and
instructions are based on building under Windows 10 with Visual Studio 2019
Community edition. Different steps may be necessary for other environments.


## Building

1.  Purchase and install [Stardew Valley](https://www.stardewvalley.net/)

1.  Create a directory named `lib` in the repository root

1.  Copy the following libraries from Stardew Valley to `lib`:
    - BmFont.dll
    - Galaxy.dll
    - GalaxyCSharp.dll
    - GalaxyCSharpGlue.dll
    - Lidgren.Network.dll
    - steam_api.dll
    - Steamworks.NET.dll
    - xTile.dll

1.  Copy the following libraries from XNA 4.0 to `lib`:
    - Microsoft.Xna.Framework.dll
    - Microsoft.Xna.Framework.Game.dll
    - Microsoft.Xna.Framework.Graphics.dll
    - Microsoft.Xna.Framework.Xact.dll

    a. If XNA is installed, the libraries are under `C:\Windows\Microsoft.NET\assembly\GAC_32\`

    b. If XNA is not installed, download and install [Microsoft XNA Redistributable 4.0](https://www.microsoft.com/en-us/download/details.aspx?id=27598) and copy from the location above

1.  Build


## Running

Stardew Valley looks for the directory `Content` adjacent to it for all assets.
Copy this from the Stardew Valley install to the build output 
directory, by default `./StardewValley/bin/x86/<Configuration>/net40/`

This directory can easily be wiped out and the content directory is large that 
duplicating for configurations is rather wasteful, so an alternative is to copy
it to the repository root and create a symlink in the build output instead. 
This can be accomplished on Windows using PowerShell in administrator mode:

```powershell
New-Item -Type SymbolicLink -Path "./StardewValley/bin/x86/Debug/net40/Content" -Target "./Content/"
New-Item -Type SymbolicLink -Path "./StardewValley/bin/x86/Release/net40/Content" -Target "./Content/"
```
