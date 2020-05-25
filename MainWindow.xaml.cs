using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RawViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapSource _original;
        const int _width = 5120;
        const int _height = 5120;
        int _bitsPerPixel = 10;

        public MainWindow()
        {
            InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Properties.Settings.Default.InitialDirectory;
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.InitialDirectory = openFileDialog.InitialDirectory;
                    DrawPicture(openFileDialog.FileName);                    
                }
            }
        }
        private void DrawPicture(string aFilePath)
        {
            try
            {
                if (!File.Exists(aFilePath))
                {
                    return;
                }

                using (BinaryReader br = new BinaryReader(File.Open(aFilePath, FileMode.Open)))
                {
                    if (_bitsPerPixel > 16)
                    {
                        int iNumberOfPixels = (int)(br.BaseStream.Length / sizeof(float));
                        float[] pixels = new float[iNumberOfPixels];

                        for (var i = 0; i < iNumberOfPixels; ++i)
                        {
                            pixels[i] = br.ReadSingle();
                        }

                        int bitsPerPixel = 32;
                        int bytesPerPixel = (bitsPerPixel + 7) / 8;
                        int stride = 4 * ((_width * bytesPerPixel + 3) / 4);

                        _original = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray32Float, null, pixels, stride);
                        _image.Source = _original;
                    }
                    else
                    {
                        int iNumberOfPixels = (int)(br.BaseStream.Length / 2);
                        ushort[] pix16 = new ushort[iNumberOfPixels];

                        ushort pixShort;
                        for (var i = 0; i < iNumberOfPixels; ++i)
                        {
                            pixShort = (ushort)(br.ReadUInt16() * Math.Pow(2, 16 - _bitsPerPixel));
                            pix16[i] = pixShort;
                        }

                        int bitsPerPixel = 16;
                        int stride = (_width * bitsPerPixel + 7) / 8;

                        _original = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray16, null, pix16, stride);
                        _image.Source = _original;
                    }
                }

                Properties.Settings.Default.LastOpenedFilePath = aFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnSlider_ValueChanged(object sender, RoutedEventArgs e)
        {
            _image.Source = BitmapUtils.AdjustBitmap(_original, _sliderBrightness.Value, _sliderContrast.Value, _sliderGamma.Value);
        }
        private void ZoomBorder_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                DrawPicture(files[0]);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ImageType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBoxType.SelectedIndex)
            {
                case 0:
                    _bitsPerPixel = 8;
                    break;
                case 1:
                    _bitsPerPixel = 10;
                    break;
                case 2:
                    _bitsPerPixel = 12;
                    break;
                case 3:
                    _bitsPerPixel = 14;
                    break;
                case 4:
                    _bitsPerPixel = 16;
                    break;
                case 5:
                    _bitsPerPixel = 32;
                    break;
            }

            DrawPicture(Properties.Settings.Default.LastOpenedFilePath);
        }
    }
}
