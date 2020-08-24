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

                ushort[] pix16 = null;

                using (BinaryReader br = new BinaryReader(File.OpenRead(aFilePath)))
                {
                    if (_bitsPerPixel > 16)
                    {
                        int pixNum = (int)(br.BaseStream.Length / sizeof(float));
                        float[] pix32 = new float[pixNum];

                        float max = 0;
                        for (var i = 0; i < pixNum; ++i)
                        {
                            float num = br.ReadSingle();
                            pix32[i] = num > 0 ? num : 0;
                            max = Math.Max(pix32[i], max);
                        }

                        pix16 = new ushort[pixNum];
                        for (var i = 0; i < pixNum; ++i)
                        {
                            pix16[i] = (ushort)(ushort.MaxValue * pix32[i] / max); 
                        }
                    }
                    else
                    {
                        int pixNum = (int)(br.BaseStream.Length / sizeof(ushort));
                        pix16 = new ushort[pixNum];

                        ushort pixShort;
                        for (var i = 0; i < pixNum; ++i)
                        {
                            pixShort = (ushort)(br.ReadUInt16() * Math.Pow(2, 16 - _bitsPerPixel));
                            pix16[i] = pixShort;
                        }
                    }
                }

                int bitsPerPixel = 16;
                int stride = (_width * bitsPerPixel + 7) / 8;

                _original = BitmapSource.Create(_width, _height, 96, 96, PixelFormats.Gray16, null, pix16, stride);
                _image.Source = _original;

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
