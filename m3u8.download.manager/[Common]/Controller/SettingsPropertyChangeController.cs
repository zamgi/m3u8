using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

using m3u8.download.manager.models;

using _Settings_ = m3u8.download.manager.Properties.Settings;
using M = System.Runtime.CompilerServices.MethodImplAttribute;
using O = System.Runtime.CompilerServices.MethodImplOptions;

namespace m3u8.download.manager.controllers
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed class SettingsPropertyChangeController : IDisposable
    {
        public const int MAX_DEGREE_OF_PARALLELISM = 1024;

        /// <summary>
        /// 
        /// </summary>
        public delegate void SettingsPropertyChangedEventHandler( _Settings_ settings, string propertyName );


        public event SettingsPropertyChangedEventHandler SettingsPropertyChanged;
        private Dictionary< string, object > _PD;
        private string _AllJson;

        public SettingsPropertyChangeController()
        {
            this.Settings = _Settings_.Default;
            _AllJson = ObjAsDict_JsonSerializer.ToJSON( Settings );

            var props = typeof(_Settings_).GetProperties();
            _PD = new Dictionary< string, object >( props.Length );
            foreach ( var prop in props )
            {
                if ( prop.PropertyType.IsValueType )
                {
                    Call_AddProp4Track_With_Reflection_Cast_2_struct( prop );
                }
            }

            this.Settings.PropertyChanged += Settings_PropertyChanged;
        }
        public void Dispose() => this.Settings.PropertyChanged -= Settings_PropertyChanged;

        public static _Settings_ SettingsDefault { [M(O.AggressiveInlining)] get => _Settings_.Default; }
        public _Settings_ Settings { [M(O.AggressiveInlining)] get; }

        public (TimeSpan timeout, int attemptRequestCountByPart) GetCreateM3u8ClientParams() => (Settings.RequestTimeoutByPart, Settings.AttemptRequestCountByPart);

        public IEnumerable< string > NameCleanerExcludesWords { [M(O.AggressiveInlining)] get => Settings.GetNameCleanerExcludesWords(); }
        public bool     ShowOnlyRequestRowsWithErrors       { [M(O.AggressiveInlining)] get => Settings.ShowOnlyRequestRowsWithErrors; }
        public string   OutputFileDirectory                 { [M(O.AggressiveInlining)] get => Settings.OutputFileDirectory; }
        public bool     UniqueUrlsOnly                      { [M(O.AggressiveInlining)] get => Settings.UniqueUrlsOnly; }
        public string   MainFormPositionJson                { [M(O.AggressiveInlining)] get => Settings.MainFormPositionJson; [M(O.AggressiveInlining)] set => Settings.MainFormPositionJson = value; }
        public bool     UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => Settings.UseCrossDownloadInstanceParallelism; }
        public int      MaxDegreeOfParallelism              { [M(O.AggressiveInlining)] get => Settings.MaxDegreeOfParallelism;   [M(O.AggressiveInlining)] set => Settings.MaxDegreeOfParallelism   = value; }
        public int      MaxDegreeOfParallelismSaved         { [M(O.AggressiveInlining)] get => Settings.MaxDegreeOfParallelismSaved;   [M(O.AggressiveInlining)] set => Settings.MaxDegreeOfParallelismSaved = value; }
        public int?     MaxCrossDownloadInstance            { [M(O.AggressiveInlining)] get => Settings.MaxCrossDownloadInstance; [M(O.AggressiveInlining)] set => Settings.MaxCrossDownloadInstance = value; }
        public decimal? MaxSpeedThresholdInMbps             { [M(O.AggressiveInlining)] get => Settings.MaxSpeedThresholdInMbps;  [M(O.AggressiveInlining)] set => Settings.MaxSpeedThresholdInMbps  = value; }
        public decimal  MaxSpeedThresholdInMbpsSaved        { [M(O.AggressiveInlining)] get => Settings.MaxSpeedThresholdInMbpsSaved;  [M(O.AggressiveInlining)] set => Settings.MaxSpeedThresholdInMbpsSaved = value; }
        public bool     ShowLog                             { [M(O.AggressiveInlining)] get => Settings.ShowLog; [M(O.AggressiveInlining)] set => Settings.ShowLog = value; }
        
        public IEnumerable<
            (DateTime CreatedOrStartedDateTime,
             string Url,
             IDictionary< string, string > RequestHeaders,
             string OutputFileName,
             string OutputDirectory,
             DownloadStatus Status,
             bool IsLiveStream,
             long LiveStreamMaxFileSizeInBytes)
            > GetDownloadRows() => DownloadRowsSerializer.FromJSON( Settings.DownloadRowsJson );
        public void SetDownloadRows_WithSaveIfChanged( IEnumerable< DownloadRow > rows )
        {
            var json = DownloadRowsSerializer.ToJSON( rows );
            if ( Settings.DownloadRowsJson != json )
            {
                Settings.DownloadRowsJson = json;
                Settings.SaveNoThrow();
                _AllJson = ObjAsDict_JsonSerializer.ToJSON( Settings );
            }
        }
        public void SetDownloadRows( IEnumerable< DownloadRow > rows ) => Settings.DownloadRowsJson = DownloadRowsSerializer.ToJSON( rows );
        public void SaveNoThrow_IfAnyChanged()
        {
            var json = ObjAsDict_JsonSerializer.ToJSON( Settings );
            if ( _AllJson != json )
            {
                Settings.SaveNoThrow();
                _AllJson = json;
            }
        }

        private void AddProp4Track< T >( string propertyName, T value ) where T : struct => _PD[ propertyName ] = value;
        private void AddProp4Track< T >( string propertyName, T? value ) where T : struct => _PD[ propertyName ] = value;
        private void PropChangedAction< T >( string propertyName, T settingsValue ) where T : struct
        {
            if ( _PD.TryGetValue( propertyName, out var obj ) && !((T) obj).Equals( settingsValue ) )
            {
                _PD[ propertyName ] = settingsValue;
                SettingsPropertyChanged?.Invoke( this.Settings, propertyName );
            }
        }
        private void PropChangedAction< T >( string propertyName, T? settingsValue ) where T : struct
        {
            if ( _PD.TryGetValue( propertyName, out var obj ) )
            {
                var t = (T?) obj;
                if ( (t.HasValue && !t.Equals( settingsValue )) || (!t.HasValue && settingsValue.HasValue) )
                {
                    _PD[ propertyName ] = settingsValue;
                    SettingsPropertyChanged?.Invoke( this.Settings, propertyName );
                }
            }
        }

        private void Call_AddProp4Track_With_Reflection_Cast_2_struct( PropertyInfo prop )
        {
            var val = prop.GetValue( this.Settings );
            switch ( prop.PropertyType )
            {
                case Type t when t == typeof(bool)    : AddProp4Track( prop.Name, (bool) val ); break;
                case Type t when t == typeof(bool?)   : AddProp4Track( prop.Name, (bool?) val ); break;

                case Type t when t == typeof(char)    : AddProp4Track( prop.Name, (char) val ); break;
                case Type t when t == typeof(char?)   : AddProp4Track( prop.Name, (char?) val ); break;

                case Type t when t == typeof(sbyte)   : AddProp4Track( prop.Name, (sbyte) val ); break;
                case Type t when t == typeof(sbyte?)  : AddProp4Track( prop.Name, (sbyte?) val ); break;
                case Type t when t == typeof(byte)    : AddProp4Track( prop.Name, (byte) val ); break;
                case Type t when t == typeof(byte?)   : AddProp4Track( prop.Name, (byte?) val ); break;
                        
                case Type t when t == typeof(short)   : AddProp4Track( prop.Name, (short) val ); break;
                case Type t when t == typeof(short?)  : AddProp4Track( prop.Name, (short?) val ); break;
                case Type t when t == typeof(ushort)  : AddProp4Track( prop.Name, (ushort) val ); break;
                case Type t when t == typeof(ushort?) : AddProp4Track( prop.Name, (ushort?) val ); break;

                case Type t when t == typeof(int)     : AddProp4Track( prop.Name, (int) val ); break;
                case Type t when t == typeof(int?)    : AddProp4Track( prop.Name, (int?) val ); break;
                case Type t when t == typeof(uint)    : AddProp4Track( prop.Name, (uint) val ); break;
                case Type t when t == typeof(uint?)   : AddProp4Track( prop.Name, (uint?) val ); break;

                case Type t when t == typeof(long)    : AddProp4Track( prop.Name, (long) val ); break;
                case Type t when t == typeof(long?)   : AddProp4Track( prop.Name, (long?) val ); break;
                case Type t when t == typeof(ulong)   : AddProp4Track( prop.Name, (ulong) val ); break;
                case Type t when t == typeof(ulong?)  : AddProp4Track( prop.Name, (ulong?) val ); break;

                case Type t when t == typeof(float)   : AddProp4Track( prop.Name, (float) val ); break;
                case Type t when t == typeof(float?)  : AddProp4Track( prop.Name, (float?) val ); break;
                case Type t when t == typeof(double)  : AddProp4Track( prop.Name, (double) val ); break;
                case Type t when t == typeof(double?) : AddProp4Track( prop.Name, (double?) val ); break;

                case Type t when t == typeof(decimal) : AddProp4Track( prop.Name, (decimal) val ); break;
                case Type t when t == typeof(decimal?): AddProp4Track( prop.Name, (decimal?) val ); break;

                case Type t when t == typeof(TimeSpan) : AddProp4Track( prop.Name, (TimeSpan) val ); break;
                case Type t when t == typeof(TimeSpan?): AddProp4Track( prop.Name, (TimeSpan?) val ); break;

                default: throw (new InvalidCastException( prop.ToString() ));
            }
        }
        private void Call_PropChangedAction_With_Reflection_Cast_2_struct( PropertyChangedEventArgs e )
        {
            var prop = typeof(_Settings_).GetProperty( e.PropertyName );
            if ( prop == null )
            {
                Debug.WriteLine( $"Unknown prop: '{e.PropertyName}'" );
                return;
            }

            if ( !prop.PropertyType.IsValueType )
            {
                Debug.WriteLine( $"NOT struct: {prop}" );
                return;
            }

            var val = prop.GetValue( this.Settings );
            switch ( prop.PropertyType )
            {
                case Type t when t == typeof(bool)    : PropChangedAction( prop.Name, (bool) val ); break;
                case Type t when t == typeof(bool?)   : PropChangedAction( prop.Name, (bool?) val ); break;

                case Type t when t == typeof(char)    : PropChangedAction( prop.Name, (char) val ); break;
                case Type t when t == typeof(char?)   : PropChangedAction( prop.Name, (char?) val ); break;

                case Type t when t == typeof(sbyte)   : PropChangedAction( prop.Name, (sbyte) val ); break;
                case Type t when t == typeof(sbyte?)  : PropChangedAction( prop.Name, (sbyte?) val ); break;
                case Type t when t == typeof(byte)    : PropChangedAction( prop.Name, (byte) val ); break;
                case Type t when t == typeof(byte?)   : PropChangedAction( prop.Name, (byte?) val ); break;
                        
                case Type t when t == typeof(short)   : PropChangedAction( prop.Name, (short) val ); break;
                case Type t when t == typeof(short?)  : PropChangedAction( prop.Name, (short?) val ); break;
                case Type t when t == typeof(ushort)  : PropChangedAction( prop.Name, (ushort) val ); break;
                case Type t when t == typeof(ushort?) : PropChangedAction( prop.Name, (ushort?) val ); break;

                case Type t when t == typeof(int)     : PropChangedAction( prop.Name, (int) val ); break;
                case Type t when t == typeof(int?)    : PropChangedAction( prop.Name, (int?) val ); break;
                case Type t when t == typeof(uint)    : PropChangedAction( prop.Name, (uint) val ); break;
                case Type t when t == typeof(uint?)   : PropChangedAction( prop.Name, (uint?) val ); break;

                case Type t when t == typeof(long)    : PropChangedAction( prop.Name, (long) val ); break;
                case Type t when t == typeof(long?)   : PropChangedAction( prop.Name, (long?) val ); break;
                case Type t when t == typeof(ulong)   : PropChangedAction( prop.Name, (ulong) val ); break;
                case Type t when t == typeof(ulong?)  : PropChangedAction( prop.Name, (ulong?) val ); break;

                case Type t when t == typeof(float)   : PropChangedAction( prop.Name, (float) val ); break;
                case Type t when t == typeof(float?)  : PropChangedAction( prop.Name, (float?) val ); break;
                case Type t when t == typeof(double)  : PropChangedAction( prop.Name, (double) val ); break;
                case Type t when t == typeof(double?) : PropChangedAction( prop.Name, (double?) val ); break;

                case Type t when t == typeof(decimal) : PropChangedAction( prop.Name, (decimal) val ); break;
                case Type t when t == typeof(decimal?): PropChangedAction( prop.Name, (decimal?) val ); break;

                case Type t when t == typeof(TimeSpan) : PropChangedAction( prop.Name, (TimeSpan) val ); break;
                case Type t when t == typeof(TimeSpan?): PropChangedAction( prop.Name, (TimeSpan?) val ); break;

                default: throw (new InvalidCastException( prop.ToString() ));
            }
        }

        private void Settings_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Call_PropChangedAction_With_Reflection_Cast_2_struct( e );

            #region comm. prev.
            /*
            switch ( e.PropertyName )
            {
                case nameof(_Settings_.AttemptRequestCountByPart):
                    PPC( e.PropertyName, this.Settings.AttemptRequestCountByPart );
                break;
                case nameof(_Settings_.RequestTimeoutByPart):
                    PPC( e.PropertyName, this.Settings.RequestTimeoutByPart );
                break;

                case nameof(_Settings_.UseCrossDownloadInstanceParallelism):
                    PPC( e.PropertyName, this.Settings.UseCrossDownloadInstanceParallelism );
                break;

                case nameof(_Settings_.MaxDegreeOfParallelism):
                    PPC( e.PropertyName, this.Settings.MaxDegreeOfParallelism );
                break;

                case nameof(_Settings_.MaxCrossDownloadInstance):
                    PPC( e.PropertyName, this.Settings.MaxCrossDownloadInstance );
                break;

                case nameof(_Settings_.ShowDownloadStatisticsInMainFormTitle):
                    PPC( e.PropertyName, this.Settings.ShowDownloadStatisticsInMainFormTitle );
                break;

                case nameof(_Settings_.ShowOnlyRequestRowsWithErrors):
                    PPC( e.PropertyName, this.Settings.ShowOnlyRequestRowsWithErrors );
                break;

                case nameof(_Settings_.ShowAllDownloadsCompleted_Notification):
                    PPC( e.PropertyName, this.Settings.ShowAllDownloadsCompleted_Notification );
                break;

                case nameof(_Settings_.ScrollToLastRow):
                    PPC( e.PropertyName, this.Settings.ScrollToLastRow );
                break;

                case nameof(_Settings_.MaxSpeedThresholdInMbps):
                    PPC( e.PropertyName, this.Settings.MaxSpeedThresholdInMbps );
                break;
            }
            */
            #endregion
        }
    }
}
