# <img src="https://user-images.githubusercontent.com/46743297/61076153-3cf55600-a424-11e9-8daf-c6332aac6f4d.png" alt="cat_icon" width="36" height="36"> Savescum Buddy
Backup and restore savefiles with hotkeys. Configurable backup scheduler will make backups for you. Upload and recover your backups via Google Drive. Suits any game that has only a single savefile.

![backups](https://user-images.githubusercontent.com/46743297/106929367-55734280-6725-11eb-9e43-001bb7a5a6e1.png)
![tooltip](https://user-images.githubusercontent.com/46743297/106929377-57d59c80-6725-11eb-913c-e984fce29f52.png)
![games](https://user-images.githubusercontent.com/46743297/106929374-573d0600-6725-11eb-88b5-c6bdf98ceffd.png)
![settings](https://user-images.githubusercontent.com/46743297/106929375-573d0600-6725-11eb-96e1-1c0f9480caa4.png)

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
* [*cat_icon_64px.ico*](https://github.com/bokuwazheng/SavescumBuddy/blob/master/SavescumBuddy/Resources/icon.ico)
from Segoe UI Emoji.
* Sound cues from [낙원](https://music.apple.com/ca/album/paradise/1384386163?i=1384386840) by BTS (cropped and denoised with Adobe Premiere Pro).
