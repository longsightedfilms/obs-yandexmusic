# OBS Yandex Music
##### Console helper to your streams
Primarily i wrote this app for myself, but maybe someone wants this app too. This app get currently playing song artist and song name from Yandex Music (opened in browser) and write to text file. Also it is logging to console all played songs.
### Features
- Supports Chrome, Opera, IE and Edge.
- Writes current song to file
- Can swap artist name and song name
- Shorten long artist names

### Installing
You can get compiled binaries here:
### Compile from sources
First, you need to clone repository:
`git clone "https://github.com/longsightedfilms/obs-yandexmusic/"`
Second, you need to install .Net Core 2.0 SDK
[https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.200](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.200 "https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.200")
Then in cmd write:
`dotnet restore`
`dotnet publish -c Release -r win-x86 /p:TrimUnusedDependencies=true`
### How to run
1. Open command prompt in app folder
2. Type `obs-yandexmusic.exe // some args`
3. Open Yandex Music in browser
4. In app folder you get `yandex-music.txt`
5. In OBS set checkbox `Read from file` and set file path to `yandex-music.txt`

### Command line arguments
You can set, which browser runs Yandex Music:
`-- chrome` for Google Chrome
`-- opera` for Opera (Chromium Based)
`-- ie` for Internet Explorer
`-- edge` for Microsoft Edge
If browser is not set, by default it uses Google Chrome. Currently in Firefox Win32 API can't properly get window title.
`-- reverse` - swaps artist name and song title.
For example:
Without `-- reverse`: `Save Yourself - Breaking Benjamin`
With `-- reverse`: `Breaking Benjamin - Save Yourself`
### Shorten
With 0.1.2 version app automatically changes long artist name to short name. It very helpful in case of enumeration all peoples in band as artist name. For example:
`Breaking Benjamin, Jasen Rauch, Keith Wallen, Benjamin Burnley, Shaun Foist, Aaron Bruch`
shortens to:
`Breaking Benjamin`
### Changelog
0.1 - Initial release
0.1.1 - Fixed some bugs, added support for several browsers
0.1.2 - Another fix bugs, add reverse and shorten

### Known bugs
- Win32 API detect only current tab name, so it is better to run music in different browser.
- Maybe bad recognizing window title
- Currently app detect Yandex Music in brute-force way, so when you try serfing internet, it can write inappropriate name.

### License
OBS Yandex Music is licensed under the MIT license