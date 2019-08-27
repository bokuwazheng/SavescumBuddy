# <img src="https://user-images.githubusercontent.com/46743297/61076153-3cf55600-a424-11e9-8daf-c6332aac6f4d.png" alt="cat_icon" width="36" height="36"> SavescumBuddy ([download](https://github.com/bokuwazheng/SavescumBuddy/releases/download/v1.1/Setup.msi))
Helps keep your game savefile backups organized. Works with any game that has only a single savefile.

### New in v1.1: 
* Improved performance.
* Google Drive integration.
* and [more...](https://github.com/bokuwazheng/SavescumBuddy/releases/tag/v1.1)

![main](https://user-images.githubusercontent.com/46743297/61174768-73121180-a5ad-11e9-8fde-8b8f8e1623c9.jpg)

![settings](https://user-images.githubusercontent.com/46743297/61174772-7c02e300-a5ad-11e9-908d-89d9d54fed17.jpg)

## Why use Savescum Buddy?
For save scumming purposes obviously. Take a look at the screenshots above, all the features are pretty much self-explanatory.
Also, you might want to sync your backup folder with a write-only cloud storage to make sure you won't ever lose your progress.

### How does it work?
* It creates a copy of savefile and puts it in a folder within selected backup folder paired with a screenshot of your main screen.
* Other data like notes and paths is stored in %APPDATA%\bokuwazheng\DB.db.
* Settings and sorting options are stored in %USERPROFILE%\AppData\Local\SavescumBuddy\SavescumBuddy.exe_someRandomChars\user.config.

### How do I use it?
1. Add a Game to the list,
   * Specify the savefile path (e.g. C:\Users\User\Documents\nbgi\darksouls\drak0005.sl2),
   * Specify the backup folder,
   * Add a title, it is used to sort backups (press 'Current Only' in sorting panel to show backup of current Game),
   * Click 'Update' and 'Set as current' (only one Game can be set as current), 
   * Note: Deleting the Game from the list doesn't affect associated backups,
2. Specify desired Autobackup and Hotkeys options,
3. Use the '+' button to create new backup, 
   * Or press quicksave hotkey (successful action will be followed by a sound cue),
   * Add a note or hit the '♥' button to mark a backup,
4. While in game main menu press the 'Restore' button to load a backup, 
   * Or press quickload hotkey (successful action will be followed by a sound cue),
   * Note: quickload hotkey restores the latest backup (but not autobackup) of current Game,
5. Click the "trash can" icon (or press Delete on your keyboard) to delete selected backup,
   * Deleted backups are moved to Recycle Bin,
6. Use sorting buttons or search by note to find desired backups,
7. To import existing backups:
   * Do steps 1 and 2,
   * Click 'import' (imported backups must be located in separate folders).

## Installation
Note: installer also will install Segoe MDL2 Assets (version 1.75).
* Download [Setup.msi](https://github.com/bokuwazheng/SavescumBuddy/releases/download/v1.1/Setup.msi).
* Run the Setup.msi and follow the instructions.

## How to build
* Install [Visual Studio 2019](https://visualstudio.microsoft.com/) and run it.
* [Clone](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio#clone-from-another-git-provider) the source repository from Github. 
    ````
    https://github.com/bokuwazheng/SavescumBuddy.git
    ````
* In Visual Studio open Solution Explorer.
* Double-click the .sln file.
* Go to Build and click 'Build Solution' or just press F6 on your keyboard.
* Use [DB Browser for SQLite](https://sqlitebrowser.org/) to manage DB.db.

## Credit
* [*GlobalKeyboardHook.cs*](https://github.com/bokuwazheng/SavescumBuddy/blob/master/SavescumBuddy/GlobalKeyboardHook.cs) 
by Johannes Nestler, 2009.
* [*cat_icon_64px.ico*](https://github.com/bokuwazheng/SavescumBuddy/blob/master/SavescumBuddy/cat_icon_64px.ico)
from Segoe UI Emoji.
* Sound cues from [낙원](https://music.apple.com/ca/album/paradise/1384386163?i=1384386840) by BTS (cropped and denoised with Adobe Premiere Pro).
