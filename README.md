# m3u8
m3u8 file downloader library

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
