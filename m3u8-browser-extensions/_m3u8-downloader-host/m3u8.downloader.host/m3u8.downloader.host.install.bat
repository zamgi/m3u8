:: Copyright 2014 The Chromium Authors. All rights reserved.
:: Use of this source code is governed by a BSD-style license that can be
:: found in the LICENSE file.
 
:: Change HKCU to HKLM if you want to install globally.
:: %~dp0 is the directory containing this bat script and ends with a backslash.

REG ADD "HKCU\Software\Google\Chrome\NativeMessagingHosts\m3u8.downloader.host" /ve /t REG_SZ /d "%~dp0m3u8.downloader.host.google-chrome.json" /f
REG ADD "HKCU\Software\Mozilla\NativeMessagingHosts\m3u8.downloader.host" /ve /t REG_SZ /d "%~dp0m3u8.downloader.host.firefox.json" /f
