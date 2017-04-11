using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    public partial class TestPerspectivePage : ContentPage
    {
        SKBitmap bitmap;

        public TestPerspectivePage()
        {
            InitializeComponent();

            string resourceID = "SkiaSharpFormsDemos.Media.SeatedMonkey.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                bitmap = SKBitmap.Decode(skStream);
            }

            persp2Slider.Value = 1;
        }

        void sliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            SKMatrix matrix = SKMatrix.MakeIdentity();
            matrix.TransX = 0;
            matrix.TransY = 0;

            matrix.Persp0 = (float)persp0Slider.Value;
            matrix.Persp0 = (float)persp1Slider.Value;
            matrix.Persp0 = (float)persp2Slider.Value;

            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, 0, 0);
        }
    }
}