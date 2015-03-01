# UnitySXS
A helper utility for running side-by-side installations of Unity. It edits the registry entry all versions of Unity read to determine the last opened project.

This means you can run say, 4.5 and 4.6 side-by-side, and UnitySXS will remember which project was open in which version of the editor and dynamically alter the registry to avoid opening the wrong project in the wrong version.

## How it works
There isn't anything special going on -- we just keep a dictionary of Unity versions in "x.x.x" format (for example, "4.6.1") and check the executable's internal version data to track which version is using which project. The program stays active while Unity is running and when it detects that the editor has terminated, saves out the registry setting.

The limitation here is that patches (4.6.1p1, 4.6.1p2, etc) are considered the same version by UnitySXS, though more often than not tracking major, minor and build versions is granular enough.

## Compiling
UnitySXS is written in C#, using Visual Studio 2013. It should compile just fine in the various flavours and versions of Visual Studio, including Express and Community editions, though I haven't personally tested this.

UnitySXS targets the .NET 3.5 framework and this is a required install on any machine that wants to run the application. That said, Windows XP SP3 and higher should be good to go without further interaction.

## Usage
Create a shortcut to UnitySXS.exe and supply as an arugment the full path to the Unity editor you want to open:

<pre>UnitySXS.exe "C:\Program Files (x86)\Unity_4.6.1p2\Editor\Unity.exe"</pre>

You can also add arguments that should be fed to Unity upon execution:

<pre>UnitySXS.exe "C:\Program Files (x86)\Unity_4.6.1p2\Editor\Unity.exe" -force-opengl</pre>

## Binaries?
The most recent precompiled version can always be found here: http://theplaywrite.com/files/UnitySXS.zip

## What else have you done?
If you want to check out Unity games made by us, head over to Steam!

**Deadnaut** - http://store.steampowered.com/app/337040 (made with Unity)

**Zafehouse: Diaries** - http://store.steampowered.com/app/249360 (written from scratch in Visual Basic .NET, C#, with help from SlimDX)
