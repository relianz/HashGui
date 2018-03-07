/*

The MIT License

Copyright 2018, Dr.-Ing. Markus A. Stulle, München (markus@stulle.zone)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
and associated documentation files (the "Software"), to deal in the Software without restriction, 
including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies 
or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/

using System;                         // BitConverter
using System.IO;                      // FileStream, File, Path
using System.Windows;                 // DataFormats
using System.Diagnostics;             // Stopwatch
using System.Security.Cryptography;   // SHA256Managed
using System.Windows.Media.Effects;   // BlurEffect
using System.Windows.Media.Animation; // DoubleAnimation

namespace HashGui
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void HashPanel_Drop( object sender, DragEventArgs e )
        {
            if (e.Data.GetDataPresent( DataFormats.FileDrop ))
            {
                // Der Bediener kann mehr als eine Datei ablegen:
                string[] files = (string[])e.Data.GetData( DataFormats.FileDrop );
                
                // BlurElementExtension.BlurApply( dropTarget, 10.0, TimeSpan.FromMilliseconds( 3 ), new TimeSpan() );

                Stopwatch watch = Stopwatch.StartNew();
                string hash = GetChecksum( files[ 0 ] );
                watch.Stop();

                if (hash == null)
                    return;

                fileName.Text = files[ 0 ];
                duration.Text = "";

                long nanosecPerTick = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
                long microsec = (watch.ElapsedTicks * nanosecPerTick) / 1000;
                                
                duration.Text = microsec.ToString();
                // BlurElementExtension.BlurDisable( dropTarget, TimeSpan.FromMilliseconds( 3 ), new TimeSpan() );

                fileName.Text = files[ 0 ];
                hashValue_last.Text = hashValue_new.Text;
                hashValue_new.Text  = hash;

            } // DataFormats.FileDrop

        } // HashPanel_Drop

        private string GetChecksum( string path )
        {
            FileStream stream = null;

            try {
                stream = File.OpenRead( path );
            }
            catch( Exception x )
            {
                string s = x.ToString();

                string[] pathComponents = path.Split( Path.DirectorySeparatorChar );
                string   fileName = pathComponents[ pathComponents.Length - 1 ];

                MessageBoxResult result = MessageBox.Show( "Kann Datei <" + fileName + "> nicht öffnen!", "Fehler beim Öffnen der Datei", 
                                                           MessageBoxButton.OK, MessageBoxImage.Error );
                return null;
            }

            using (stream)
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash( stream );
                return BitConverter.ToString( checksum ).Replace( "-", " " );
            }

        } // GetChecksum

        private string GetChecksumBuffered( Stream stream )
        {
            using (var bufferedStream = new BufferedStream( stream, 1024 * 32 ))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash( bufferedStream );
                return BitConverter.ToString( checksum ).Replace( "-", " " );
            }

        } // GetChecksumBuffere

    } // class MainWindow

    public static class BlurElementExtension
    {        
        public static void BlurApply( this UIElement element,
                                      double blurRadius, TimeSpan duration, TimeSpan beginTime )
        {
            BlurEffect blur = new BlurEffect() {
                Radius = blurRadius
            };

            DoubleAnimation blurEnable = new DoubleAnimation( 0, blurRadius, duration ) {
                BeginTime = beginTime
            };

            element.Effect = blur;
            blur.BeginAnimation( BlurEffect.RadiusProperty, blurEnable );

        } // BlurApply

        /// <summary>
        /// Turning blur off
        /// </summary>
        /// <param name="element">bluring element</param>
        /// <param name="duration">blur animation duration</param>
        /// <param name="beginTime">blur animation delay</param>
        public static void BlurDisable( this UIElement element, TimeSpan duration, TimeSpan beginTime )
        {
            BlurEffect blur = element.Effect as BlurEffect;
            if (blur == null || blur.Radius == 0) {
                return;
            }

            DoubleAnimation blurDisable = new DoubleAnimation( blur.Radius, 0, duration ) {
                BeginTime = beginTime
            };

            blur.BeginAnimation( BlurEffect.RadiusProperty, blurDisable );

        } // BlurDisable

    } // class BlurElementExtension

} // namespace HashGui
