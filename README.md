# <img src="https://user-images.githubusercontent.com/46743297/61076153-3cf55600-a424-11e9-8daf-c6332aac6f4d.png" alt="cat_icon" width="36" height="36"> Savescum Buddy
Backup and restore savefiles with hotkeys. Configurable backup scheduler will make backups for you. Upload and recover your backups via Google Drive. Suits any game that has only a single savefile.

![backups](https://user-images.githubusercontent.com/46743297/106933138-a2f1ae80-6729-11eb-879a-f9216fceb120.png)
![tooltip](https://user-images.githubusercontent.com/46743297/106932875-4f7f6080-6729-11eb-8863-592f5fac059d.png)
![games](https://user-images.githubusercontent.com/46743297/106932870-4ee6ca00-6729-11eb-80cc-83756ad21e26.png)
![settings](https://user-images.githubusercontent.com/46743297/106932873-4ee6ca00-6729-11eb-80e8-d077a4ce298d.png)

## How to build
* Install [Visual Studio 2019](https://visualstudio.microsoft.com/) and run it.
* [Clone](https://docs.microsoft.com/en-us/azure/devops/repos/git/clone?view=azure-devops&tabs=visual-studio#clone-from-another-git-provider) the source repository from Github. 
    ````
    https://github.com/bokuwazheng/SavescumBuddy.git
    ````
* In Visual Studio open Solution Explorer.
* Double-click the .sln file.
* Go to Build and click 'Build Solution' or just press F6 on your keyboard.
* Use [DB Browser for SQLite](https://sqlitebrowser.org/) to manage the database.
* [Sqlite script](https://github.com/bokuwazheng/SavescumBuddy/wiki/Sqlite-script) can be found in the wiki.

## Credit
* [*GlobalKeyboardHook.cs*](https://github.com/bokuwazheng/SavescumBuddy/blob/e3da799978c848f3e020c4d2beffcd2952cb6af2/SavescumBuddy.Wpf/Services/GlobalKeyboardHook.cs) 
by Johannes Nestler, 2009.
* [*Icon*](https://github.com/bokuwazheng/SavescumBuddy/blob/master/SavescumBuddy/Resources/icon.ico)
from Segoe UI Emoji.
* Sound cues from [낙원](https://music.apple.com/ca/album/paradise/1384386163?i=1384386840) by BTS (cropped and denoised with Adobe Premiere Pro).
