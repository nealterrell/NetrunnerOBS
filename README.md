# NetrunnerOBS

A simple OBS (https://obsproject.com/) plugin to show Android Netrunner cards as a video Source.

To install:

1. Install CLR Browser Source Plugin: https://obsproject.com/forum/resources/clr-browser-source-plugin.22/
2. Download current NetrunnerOBS prototype release: https://drive.google.com/open?id=0B-_Wwk51wKI0R0RsbUg5RTFiMWs&authuser=0
3. Place the file NetrunnerOBS.dll in the "CLRHostPlugin" folder of your OBS installation's "plugins" folder.
  1. This should be C:\Program Files\OBS\plugins\CLRHostPlugin; or in C:\Program Files (x86)\OBS\plugins\CLRHostPlugin
  2. If you do not have a CLRHostPlugin folder, see step 1.
4. Start OBS.
5. Add a new Source of type Netrunner Card. If you do not see this option, then the plugin was not installed correctly.
6. A window will pop up in the background (temporary issue). Activate it from the taskbar or by Alt-Tab. 

Using the plugin:

1. The first time the plugin loads, it will contact Netrunnderdb.com to download its public list of available cards. You can check to make sure this worked by finding this folder: %USERPROFILE%\AppData\Roaming\OBS\pluginData\Netrunner . 
2. The Netrunner Card Source window lets you type in card names to show them on stream. You can change how long it takes for a card to disappear.
3. To keep the download small, no card images come pre-loaded with the plugin. Each time you type a new card name in, the plugin will download an image for that card from Netrunnerdb.com, which takes a second. If you want to pre-fetch all card images, use the "Advanced..." button on the Netrunner Card Source window. You can also resynch the card list from this menu (when new cards are released, if things get corrupted, etc.).
4. You can position the card image using the Edit Scene button; drag and resize as needed.

KNOWN ISSUES:
1. Cards will be sized incorrectly the first time you add a Netrunner Card Image to your scene. To fix, close the Card Selection window and then double-click the Netrunner Card Image item in your Sources to reopen the window. The next card you show will be correctly sized.


