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
1) a build the project '**m3u8/m3u8.download.manager/m3u8.download.manager.csproj**'.
2) 
- for Windows:
   - run 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.host/**m3u8.download.manager.host.install.bat**' 
   - build in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.host/bin' execute file named '**m3u8.download.manager.exe**'   
   - or 
   'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host/google-chrome/**m3u8.download.manager.avalonia.host.install.bat**'   
   - build in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host' execute file named '**m3u8.download.manager.avalonia.exe**'
- for Linux:   
   - run 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host/google-chrome/**m3u8.download.manager.avalonia.host.install.sh**'
   - build or unzip ('m3u8.download.manager.avalonia.zip') in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host' single execute file named '**m3u8.download.manager.avalonia**' and put him execute rights 
for register host application for chrome.
3) create in chrome extension directly by path 'm3u8/m3u8-browser-extensions/m3u8-chrome-extension/' <strike>or create '.crx'-file and register him</strike>.

FireFox-Extension/AddOn
-----
For using firefox-extension/addOn need:
1) a build the project '**m3u8/m3u8.download.manager/m3u8.download.manager.csproj**'.
2) 
- for Windows:
   - run 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.host/**m3u8.download.manager.host.install.bat**' 
   - build in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.host/bin' execute file named '**m3u8.download.manager.exe**'    
   - or 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host/firefox/**m3u8.download.manager.avalonia.host.install.bat**'
   - build in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host' execute file named '**m3u8.download.manager.avalonia.exe**'   
- for Linux:   
   - run 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host/firefox/**m3u8.download.manager.avalonia.host.install.sh**'
   - build or unzip ('m3u8.download.manager.avalonia.zip') in directory 'm3u8/m3u8-browser-extensions/_m3u8-downloader-host/m3u8.download.manager.avalonia.host' single execute file named '**m3u8.download.manager.avalonia**' and put him execute rights    
for register host application for firefox.
3) create in firefox extension from 'm3u8/m3u8-browser-extensions/m3u8-firefox-extension/xpi/**m3u8_file_downloader-1.3-fx.xpi**' or create directly by path 'm3u8/m3u8-browser-extensions/m3u8-firefox-extension/'.

FireFox ESR-Extension/AddOn
-----
For using in FireFox ESR (including Top-Browser) - using like for FireFox-for-Windows <strike>(need a remove [install.rdf]-file from this folder & remove [install.rdf]-file from .xpi-file)</strike> (and possible turn-off some xpi-settings (like 'xpinstall.signatures.required') in about:config tab)

-----
real usage example:
![alt tag](https://github.com/zamgi/m3u8/blob/master/%5Bm3u8%5D.gif)
