rd ".vs" /S /Q

rd "m3u8.client\bin" /S /Q
rd "m3u8.client\obj" /S /Q

rd "m3u8.console.app\bin" /S /Q
rd "m3u8.console.app\obj" /S /Q

rd "m3u8.downloader\bin" /S /Q
rd "m3u8.downloader\obj" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.downloader.host\bin\*.pdb" /Q

rd "m3u8.download.manager\bin" /S /Q
rd "m3u8.download.manager\obj" /S /Q
del "m3u8-browser-extensions\_m3u8-downloader-host\m3u8.download.manager.host\bin\*.pdb" /Q

rem pause
