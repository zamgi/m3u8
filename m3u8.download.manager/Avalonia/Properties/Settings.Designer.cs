﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace m3u8.download.manager.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool _IsUpgradedInThisVersion {
            get {
                return ((bool)(this["_IsUpgradedInThisVersion"]));
            }
            set {
                this["_IsUpgradedInThisVersion"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("E:\\")]
        public string OutputFileDirectory {
            get {
                return ((string)(this["OutputFileDirectory"]));
            }
            set {
                this["OutputFileDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".avi")]
        public string OutputFileExtension {
            get {
                return ((string)(this["OutputFileExtension"]));
            }
            set {
                this["OutputFileExtension"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseCrossDownloadInstanceParallelism {
            get {
                return ((bool)(this["UseCrossDownloadInstanceParallelism"]));
            }
            set {
                this["UseCrossDownloadInstanceParallelism"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8")]
        public int MaxDegreeOfParallelism {
            get {
                return ((int)(this["MaxDegreeOfParallelism"]));
            }
            set {
                this["MaxDegreeOfParallelism"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public global::System.Nullable<System.Int32> MaxCrossDownloadInstance {
            get {
                return ((global::System.Nullable<System.Int32>)(this["MaxCrossDownloadInstance"]));
            }
            set {
                this["MaxCrossDownloadInstance"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("2")]
        public int MaxCrossDownloadInstanceSaved {
            get {
                return ((int)(this["MaxCrossDownloadInstanceSaved"]));
            }
            set {
                this["MaxCrossDownloadInstanceSaved"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Nullable<System.Double> MaxSpeedThresholdInMbps {
            get {
                return ((global::System.Nullable<System.Double>)(this["MaxSpeedThresholdInMbps"]));
            }
            set {
                this["MaxSpeedThresholdInMbps"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>1080</string>
  <string>1080p</string>
  <string>1O8O</string>
  <string>1O8Op</string>
  <string>360</string>
  <string>480</string>
  <string>720</string>
  <string>720p</string>
  <string>baibako</string>
  <string>cb</string>
  <string>coldfilm</string>
  <string>content</string>
  <string>DL</string>
  <string>eng</string>
  <string>films</string>
  <string>hd1080p</string>
  <string>hdtvrip</string>
  <string>hls</string>
  <string>index</string>
  <string>ivs</string>
  <string>lostfilm</string>
  <string>m3u8</string>
  <string>manifest</string>
  <string>mp4</string>
  <string>playlist</string>
  <string>rus</string>
  <string>sec</string>
  <string>stream</string>
  <string>track</string>
  <string>tracks</string>
  <string>tv</string>
  <string>video</string>
  <string>WEB</string>
  <string>webdl</string>
  <string>webrip</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection NameCleanerExcludesWords {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["NameCleanerExcludesWords"]));
            }
            set {
                this["NameCleanerExcludesWords"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DownloadRowsJson {
            get {
                return ((string)(this["DownloadRowsJson"]));
            }
            set {
                this["DownloadRowsJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3")]
        public int AttemptRequestCountByPart {
            get {
                return ((int)(this["AttemptRequestCountByPart"]));
            }
            set {
                this["AttemptRequestCountByPart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:00:45")]
        public global::System.TimeSpan RequestTimeoutByPart {
            get {
                return ((global::System.TimeSpan)(this["RequestTimeoutByPart"]));
            }
            set {
                this["RequestTimeoutByPart"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowOnlyRequestRowsWithErrors {
            get {
                return ((bool)(this["ShowOnlyRequestRowsWithErrors"]));
            }
            set {
                this["ShowOnlyRequestRowsWithErrors"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowDownloadStatisticsInMainFormTitle {
            get {
                return ((bool)(this["ShowDownloadStatisticsInMainFormTitle"]));
            }
            set {
                this["ShowDownloadStatisticsInMainFormTitle"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UniqueUrlsOnly {
            get {
                return ((bool)(this["UniqueUrlsOnly"]));
            }
            set {
                this["UniqueUrlsOnly"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AddNewDownloadFormPositionJson {
            get {
                return ((string)(this["AddNewDownloadFormPositionJson"]));
            }
            set {
                this["AddNewDownloadFormPositionJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string AddNewDownloadFormPositionLogVisibleJson {
            get {
                return ((string)(this["AddNewDownloadFormPositionLogVisibleJson"]));
            }
            set {
                this["AddNewDownloadFormPositionLogVisibleJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MainFormPositionJson {
            get {
                return ((string)(this["MainFormPositionJson"]));
            }
            set {
                this["MainFormPositionJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ChangeOutputFileFormPositionJson {
            get {
                return ((string)(this["ChangeOutputFileFormPositionJson"]));
            }
            set {
                this["ChangeOutputFileFormPositionJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string FileNameExcludesWordsEditorPositionJson {
            get {
                return ((string)(this["FileNameExcludesWordsEditorPositionJson"]));
            }
            set {
                this["FileNameExcludesWordsEditorPositionJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastSortInfoJson {
            get {
                return ((string)(this["LastSortInfoJson"]));
            }
            set {
                this["LastSortInfoJson"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ShowLog {
            get {
                return ((bool)(this["ShowLog"]));
            }
            set {
                this["ShowLog"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DownloadListColumnsWidthJson {
            get {
                return ((string)(this["DownloadListColumnsWidthJson"]));
            }
            set {
                this["DownloadListColumnsWidthJson"] = value;
            }
        }
    }
}
