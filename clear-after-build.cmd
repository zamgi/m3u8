rd ".vs" /S /Q

rem for /f "usebackq" %%f in (`"dir /ad/b/s bin"`) do rd "%%f" /S/Q
for /f "usebackq" %%f in (`"dir /ad/b/s obj"`) do rd "%%f" /S/Q
for /f "usebackq" %%f in (`"dir /ad/b/s .vs"`) do rd "%%f" /S/Q

del "*.csproj.user" /S/Q

rd "m3u8.client\bin" /S /Q
rd "m3u8.client\obj" /S /Q

rd "m3u8.client.tests\bin" /S /Q
rd "m3u8.client.tests\obj" /S /Q

rd "m3u8.console.app\bin" /S /Q
rd "m3u8.console.app\obj" /S /Q

rd "m3u8.console.app_2\bin" /S /Q
rd "m3u8.console.app_2\obj" /S /Q

rd "prev\m3u8.downloader\bin" /S /Q
rd "prev\m3u8.downloader\obj" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.downloader.host\bin\*.pdb" /Q

rd "m3u8.download.manager\WinForms\bin" /S /Q
rd "m3u8.download.manager\WinForms\obj" /S /Q
rd "m3u8.download.manager\WinForms\.vs" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.download.manager.host\bin\*.pdb" /Q

rd "m3u8.download.manager\Avalonia\bin" /S /Q
rd "m3u8.download.manager\Avalonia\obj" /S /Q
rd "m3u8.download.manager\Avalonia\.vs" /S /Q

rd "m3u8.download.manager\WinForms.NET_CORE\bin" /S /Q
rd "m3u8.download.manager\WinForms.NET_CORE\obj" /S /Q
rd "m3u8.download.manager\WinForms.NET_CORE\.vs" /S /Q

rd "m3u8_live_stream_downloader\bin" /S /Q
rd "m3u8_live_stream_downloader\obj" /S /Q
rd "m3u8_live_stream_downloader\.vs" /S /Q

rem pause