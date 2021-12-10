using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Metaballs
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        public WriteableBitmap Bitmap;

        public Main()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(ImageContainer, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(ImageContainer, EdgeMode.Aliased);

            Bitmap = new(250, 250, 96, 96, PixelFormats.Bgr32, null);

            ImageContainer.Source = Bitmap;
        }
    }
}
