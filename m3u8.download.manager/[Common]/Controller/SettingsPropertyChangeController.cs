using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

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

        public SettingsPropertyChangeController()
        {
            this.Settings = _Settings_.Default;

            var props = typeof(_Settings_).GetProperties();
            _PD = new Dictionary< string, object >( props.Length );
            foreach ( var prop in props )
            {
                if ( prop.PropertyType.IsValueType )
                {
                    Call_APV_With_Reflection_Cast_2_struct( prop );
                }                
            }
            #region comm. prev.
            /*
            APV( nameof(_Settings_.AttemptRequestCountByPart)             , this.Settings.AttemptRequestCountByPart );
            APV( nameof(_Settings_.RequestTimeoutByPart)                  , this.Settings.RequestTimeoutByPart      );
            APV( nameof(_Settings_.MaxDegreeOfParallelism)                , this.Settings.MaxDegreeOfParallelism    );
            APV( nameof(_Settings_.UseCrossDownloadInstanceParallelism)   , this.Settings.UseCrossDownloadInstanceParallelism );
            APV( nameof(_Settings_.MaxCrossDownloadInstance)              , this.Settings.MaxCrossDownloadInstance );
            APV( nameof(_Settings_.ShowDownloadStatisticsInMainFormTitle) , this.Settings.ShowDownloadStatisticsInMainFormTitle );
            APV( nameof(_Settings_.ShowOnlyRequestRowsWithErrors)         , this.Settings.ShowOnlyRequestRowsWithErrors );
            APV( nameof(_Settings_.ShowAllDownloadsCompleted_Notification), this.Settings.ShowAllDownloadsCompleted_Notification );
            APV( nameof(_Settings_.ScrollToLastRow)                       , this.Settings.ScrollToLastRow );
            APV( nameof(_Settings_.MaxSpeedThresholdInMbps)               , this.Settings.MaxSpeedThresholdInMbps );
            */
            #endregion

            this.Settings.PropertyChanged += Settings_PropertyChanged;
        }
        public void Dispose() => this.Settings.PropertyChanged -= Settings_PropertyChanged;

        public static _Settings_ SettingsDefault { [M(O.AggressiveInlining)] get => _Settings_.Default; }
        public _Settings_ Settings { [M(O.AggressiveInlining)] get; }

        [M(O.AggressiveInlining)] public void SaveNoThrow() => Settings.SaveNoThrow();
        public (TimeSpan timeout, int attemptRequestCountByPart) GetCreateM3u8ClientParams() => (Settings.RequestTimeoutByPart, Settings.AttemptRequestCountByPart);

        public IEnumerable< string > NameCleanerExcludesWords { [M(O.AggressiveInlining)] get => Settings.GetNameCleanerExcludesWords(); }
        public bool    ShowOnlyRequestRowsWithErrors { [M(O.AggressiveInlining)] get => Settings.ShowOnlyRequestRowsWithErrors; }
        public string  OutputFileDirectory           { [M(O.AggressiveInlining)] get => Settings.OutputFileDirectory; }
        public bool    UniqueUrlsOnly                { [M(O.AggressiveInlining)] get => Settings.UniqueUrlsOnly; }
        public string  MainFormPositionJson          { [M(O.AggressiveInlining)] get => Settings.MainFormPositionJson; [M(O.AggressiveInlining)] set => Settings.MainFormPositionJson = value; }
        public string  DownloadRowsJson              { [M(O.AggressiveInlining)] get => Settings.DownloadRowsJson; [M(O.AggressiveInlining)] set => Settings.DownloadRowsJson = value; }
        public bool    UseCrossDownloadInstanceParallelism { [M(O.AggressiveInlining)] get => Settings.UseCrossDownloadInstanceParallelism; }
        public int     MaxDegreeOfParallelism              { [M(O.AggressiveInlining)] get => Settings.MaxDegreeOfParallelism;   [M(O.AggressiveInlining)] set => Settings.MaxDegreeOfParallelism   = value; }
        public int?    MaxCrossDownloadInstance            { [M(O.AggressiveInlining)] get => Settings.MaxCrossDownloadInstance; [M(O.AggressiveInlining)] set => Settings.MaxCrossDownloadInstance = value; }
        public double? MaxSpeedThresholdInMbps             { [M(O.AggressiveInlining)] get => Settings.MaxSpeedThresholdInMbps;  [M(O.AggressiveInlining)] set => Settings.MaxSpeedThresholdInMbps  = value; }
        public bool    ShowLog { [M(O.AggressiveInlining)] get => Settings.ShowLog; [M(O.AggressiveInlining)] set => Settings.ShowLog = value; }

        private void APV< T >( string propertyName, T value ) where T : struct => _PD[ propertyName ] = value;
        private void APV< T >( string propertyName, T? value ) where T : struct => _PD[ propertyName ] = value;
        private void PPC< T >( string propertyName, T settingsValue ) where T : struct
        {
            if ( _PD.TryGetValue( propertyName, out var obj ) && !((T) obj).Equals( settingsValue ) )
            {
                _PD[ propertyName ] = settingsValue;
                SettingsPropertyChanged?.Invoke( this.Settings, propertyName );
            }
        }
        private void PPC< T >( string propertyName, T? settingsValue ) where T : struct
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

        private void Call_APV_With_Reflection_Cast_2_struct( PropertyInfo prop )
        {
            var val = prop.GetValue( this.Settings );
            switch ( prop.PropertyType )
            {
                case Type t when t == typeof(bool)    : APV( prop.Name, (bool) val ); break;
                case Type t when t == typeof(bool?)   : APV( prop.Name, (bool?) val ); break;

                case Type t when t == typeof(char)    : APV( prop.Name, (char) val ); break;
                case Type t when t == typeof(char?)   : APV( prop.Name, (char?) val ); break;

                case Type t when t == typeof(sbyte)   : APV( prop.Name, (sbyte) val ); break;
                case Type t when t == typeof(sbyte?)  : APV( prop.Name, (sbyte?) val ); break;
                case Type t when t == typeof(byte)    : APV( prop.Name, (byte) val ); break;
                case Type t when t == typeof(byte?)   : APV( prop.Name, (byte?) val ); break;
                        
                case Type t when t == typeof(short)   : APV( prop.Name, (short) val ); break;
                case Type t when t == typeof(short?)  : APV( prop.Name, (short?) val ); break;
                case Type t when t == typeof(ushort)  : APV( prop.Name, (ushort) val ); break;
                case Type t when t == typeof(ushort?) : APV( prop.Name, (ushort?) val ); break;

                case Type t when t == typeof(int)     : APV( prop.Name, (int) val ); break;
                case Type t when t == typeof(int?)    : APV( prop.Name, (int?) val ); break;
                case Type t when t == typeof(uint)    : APV( prop.Name, (uint) val ); break;
                case Type t when t == typeof(uint?)   : APV( prop.Name, (uint?) val ); break;

                case Type t when t == typeof(long)    : APV( prop.Name, (long) val ); break;
                case Type t when t == typeof(long?)   : APV( prop.Name, (long?) val ); break;
                case Type t when t == typeof(ulong)   : APV( prop.Name, (ulong) val ); break;
                case Type t when t == typeof(ulong?)  : APV( prop.Name, (ulong?) val ); break;

                case Type t when t == typeof(float)   : APV( prop.Name, (float) val ); break;
                case Type t when t == typeof(float?)  : APV( prop.Name, (float?) val ); break;
                case Type t when t == typeof(double)  : APV( prop.Name, (double) val ); break;
                case Type t when t == typeof(double?) : APV( prop.Name, (double?) val ); break;

                case Type t when t == typeof(decimal) : APV( prop.Name, (decimal) val ); break;
                case Type t when t == typeof(decimal?): APV( prop.Name, (decimal?) val ); break;

                case Type t when t == typeof(TimeSpan) : APV( prop.Name, (TimeSpan) val ); break;
                case Type t when t == typeof(TimeSpan?): APV( prop.Name, (TimeSpan?) val ); break;

                default: throw (new InvalidCastException( prop.ToString() ));
            }
        }
        private void Call_PPC_With_Reflection_Cast_2_struct( PropertyChangedEventArgs e )
        {
            var prop = typeof(_Settings_).GetProperty( e.PropertyName );
            if ( prop == null )
            {
                Debug.WriteLine( $"Unknown prop: '{e.PropertyName}'" );
                return;
            }

            if ( !prop.PropertyType.IsValueType )
            {
                Debug.WriteLine( $"NOT struct: {prop.ToString()}" );
                return;
            }

            var val = prop.GetValue( this.Settings );
            switch ( prop.PropertyType )
            {
                case Type t when t == typeof(bool)    : PPC( prop.Name, (bool) val ); break;
                case Type t when t == typeof(bool?)   : PPC( prop.Name, (bool?) val ); break;

                case Type t when t == typeof(char)    : PPC( prop.Name, (char) val ); break;
                case Type t when t == typeof(char?)   : PPC( prop.Name, (char?) val ); break;

                case Type t when t == typeof(sbyte)   : PPC( prop.Name, (sbyte) val ); break;
                case Type t when t == typeof(sbyte?)  : PPC( prop.Name, (sbyte?) val ); break;
                case Type t when t == typeof(byte)    : PPC( prop.Name, (byte) val ); break;
                case Type t when t == typeof(byte?)   : PPC( prop.Name, (byte?) val ); break;
                        
                case Type t when t == typeof(short)   : PPC( prop.Name, (short) val ); break;
                case Type t when t == typeof(short?)  : PPC( prop.Name, (short?) val ); break;
                case Type t when t == typeof(ushort)  : PPC( prop.Name, (ushort) val ); break;
                case Type t when t == typeof(ushort?) : PPC( prop.Name, (ushort?) val ); break;

                case Type t when t == typeof(int)     : PPC( prop.Name, (int) val ); break;
                case Type t when t == typeof(int?)    : PPC( prop.Name, (int?) val ); break;
                case Type t when t == typeof(uint)    : PPC( prop.Name, (uint) val ); break;
                case Type t when t == typeof(uint?)   : PPC( prop.Name, (uint?) val ); break;

                case Type t when t == typeof(long)    : PPC( prop.Name, (long) val ); break;
                case Type t when t == typeof(long?)   : PPC( prop.Name, (long?) val ); break;
                case Type t when t == typeof(ulong)   : PPC( prop.Name, (ulong) val ); break;
                case Type t when t == typeof(ulong?)  : PPC( prop.Name, (ulong?) val ); break;

                case Type t when t == typeof(float)   : PPC( prop.Name, (float) val ); break;
                case Type t when t == typeof(float?)  : PPC( prop.Name, (float?) val ); break;
                case Type t when t == typeof(double)  : PPC( prop.Name, (double) val ); break;
                case Type t when t == typeof(double?) : PPC( prop.Name, (double?) val ); break;

                case Type t when t == typeof(decimal) : PPC( prop.Name, (decimal) val ); break;
                case Type t when t == typeof(decimal?): PPC( prop.Name, (decimal?) val ); break;

                case Type t when t == typeof(TimeSpan) : PPC( prop.Name, (TimeSpan) val ); break;
                case Type t when t == typeof(TimeSpan?): PPC( prop.Name, (TimeSpan?) val ); break;

                default: throw (new InvalidCastException( prop.ToString() ));
            }
        }

        private void Settings_PropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            Call_PPC_With_Reflection_Cast_2_struct( e );

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
