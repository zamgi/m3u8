rd ".vs" /S /Q

rd "m3u8.client\bin" /S /Q
rd "m3u8.client\obj" /S /Q

rd "m3u8.client.tests\bin" /S /Q
rd "m3u8.client.tests\obj" /S /Q

rd "m3u8.console.app\bin" /S /Q
rd "m3u8.console.app\obj" /S /Q

rd "m3u8.downloader\bin" /S /Q
rd "m3u8.downloader\obj" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.downloader.host\bin\*.pdb" /Q

rd "m3u8.download.manager\WinForms\bin" /S /Q
rd "m3u8.download.manager\WinForms\obj" /S /Q
rd "m3u8.download.manager\WinForms\.vs" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.download.manager.host\bin\*.pdb" /Q

rd "m3u8.download.manager\Avalonia\bin" /S /Q
rd "m3u8.download.manager\Avalonia\obj" /S /Q
rd "m3u8.download.manager\Avalonia\.vs" /S /Q

rem pause
