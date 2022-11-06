using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace m3u8.download.manager
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Color2ColorTransition
    {
        public static IEnumerable< Color > GetTransitions( Color startColor, Color endColor, int stepCount )
        {
            var r = startColor.R * 1.0f; var e_r = endColor.R;
            var g = startColor.G * 1.0f; var e_g = endColor.G;
            var b = startColor.B * 1.0f; var e_b = endColor.B;

            var r_i = (e_r - r) / stepCount;
            var g_i = (e_g - g) / stepCount;
            var b_i = (e_b - b) / stepCount;

            for ( ; 0 < stepCount; stepCount-- )
            {
                r += r_i; g += g_i; b += b_i;
                var color = Color.FromArgb( (int) r, (int) g, (int) b );
                yield return (color);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public abstract class ProcessorBase : IDisposable
        {
            private bool _Running_ShowDisappearingMessage;
            private bool _Break_ShowDisappearingMessage;
            protected ProcessorBase() { }
            public void Dispose() => _Break_ShowDisappearingMessage = true;

            protected abstract void StartAction( string message, Color startColor );
            protected abstract void ProgressAction( Color color );
            protected abstract void EndAction();

            public async void Run( string message, Color startColor, Color endColor, int millisecondsDelay = 3 * 1_000, int stepCount = 100 )
            {
                #region [.waiting for already running.]
                if ( _Running_ShowDisappearingMessage )
                {
                    const int WAIT_millisecondsDelay = 10;

                    _Break_ShowDisappearingMessage = true;
                    for ( var x = (2 * millisecondsDelay / WAIT_millisecondsDelay); _Running_ShowDisappearingMessage && (0 < x); x-- )
                    {
                        await Task.Delay( WAIT_millisecondsDelay );
                    }
                }
                #endregion

                _Break_ShowDisappearingMessage   = false;
                _Running_ShowDisappearingMessage = true;
                StartAction( message, startColor );
                {
                    var stepMillisecondsDelay = millisecondsDelay / stepCount;

                    foreach ( var color in GetTransitions( startColor, endColor, stepCount ) )
                    {
                        await Task.Delay( stepMillisecondsDelay );

                        ProgressAction( color );

                        if ( _Break_ShowDisappearingMessage )
                        {
                            break;
                        }
                    }
                }
                EndAction();
                _Running_ShowDisappearingMessage = false;
            }
        }
    }
}
