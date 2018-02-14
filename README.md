# m3u8
m3u8 file downloader library and chrome-extension

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

Chrome-Extension
-----
For using chrome-extension need:
1) a build the project '**m3u8/m3u8.downloader/m3u8.downloader.csproj**'.
2) run '**m3u8/m3u8-chrome-extension/m3u8-downloader-host/install_host.bat**' for register host application for chrome.
3) create in chrome extension directly by path '**m3u8/m3u8-chrome-extension/m3u8-chrome-extension/**' <strike>or create '.crx'-file and register him</strike>.
