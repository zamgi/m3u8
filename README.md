# m3u8
m3u8 file downloader library and chrome & firefox extensions/addOns

Usage
-----
Download and save m3u8 file:

```C#
var p = new m3u8_processor.DownloadFileAndSaveInputParams()
{    
    m3u8FileUrl    = <M3U8_FILE_URL>,
    OutputFileName = @"C:\abc.avi",
};
await m3u8_processor.DownloadFileAndSave_Async( p ); 
```

Chrome-Extension/AddOn
-----
For using chrome-extension/addOn need:
1) a build the project '**m3u8/m3u8.downloader/m3u8.downloader.csproj**'.
2) run '**m3u8/m3u8-browser-extensions/_m3u8-downloader-host/install_host.bat**' for register host application for chrome.
3) create in chrome extension directly by path '**m3u8/m3u8-browser-extensions/m3u8-chrome-extension/**' <strike>or create '.crx'-file and register him</strike>.

FireFox-Extension/AddOn
-----
For using firefox-extension/addOn need:
1) a build the project '**m3u8/m3u8.downloader/m3u8.downloader.csproj**'.
2) run '**m3u8/m3u8-browser-extensions/_m3u8-downloader-host/install_host.bat**' for register host application for firefox.
3) create in firefox extension from '**m3u8/m3u8-browser-extensions/m3u8-firefox-extension/xpi/m3u8_file_downloader-1.0-fx.xpi**' or create directly by path '**m3u8/m3u8-browser-extensions/m3u8-firefox-extension/**'.

FireFox ESR-Extension/AddOn
-----
For using in FireFox ESR (including Top-Browser) need a remove [install.rdf]-file from this folder & remove [install.rdf]-file from .xpi-file (and possible turn-off some xpi-settings in about--config tab)

-----
real usage example:
![alt tag](https://github.com/zamgi/m3u8/blob/master/%5Bm3u8%5D.gif)
