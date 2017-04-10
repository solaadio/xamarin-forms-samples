using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using TouchTracking;

namespace SkiaSharpFormsDemos.Transforms
{


    // Use the TouchManipulationBitmp collection for a scatter display

        // Convert this to testing out options


    public partial class TouchManipulationPage : ContentPage
    {
        List<TouchManipulationBitmap> bitmaps = 
            new List<TouchManipulationBitmap>();

        Dictionary<long, TouchManipulationBitmap> bitmapDictionary = 
            new Dictionary<long, TouchManipulationBitmap>();

        MatrixDisplay matrixDisplay = new MatrixDisplay();

        public TouchManipulationPage()
        {
            InitializeComponent();

            string resourceID = "SkiaSharpFormsDemos.Media.MountainClimbers.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                SKBitmap bitmap = SKBitmap.Decode(skStream);
                bitmaps.Add(new TouchManipulationBitmap(bitmap));
            }
        }

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            SKPoint point = ConvertToPixel(args.Location);

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    foreach (TouchManipulationBitmap bitmap in bitmaps)
                    {
                        if (bitmap.HitTest(point))
                        {
                            bitmapDictionary.Add(args.Id, bitmap);
                            bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                            break;
                        }
                    }
                    break;

                case TouchActionType.Moved:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (bitmapDictionary.ContainsKey(args.Id))
                    {
                        TouchManipulationBitmap bitmap = bitmapDictionary[args.Id];
                        bitmap.ProcessTouchEvent(args.Id, args.Type, point);
                        bitmapDictionary.Remove(args.Id);
                        canvasView.InvalidateSurface();
                    }
                    break;
            }
        }

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();




            foreach (TouchManipulationBitmap bitmap in bitmaps)
            {
                SKPaint matrixPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 48,
                    StrokeWidth = 2
                };

                bitmap.Paint(canvas);

            // Display the matrix in the lower-right corner
            SKSize matrixSize = matrixDisplay.Measure(bitmap.Matrix);

            matrixDisplay.Paint(canvas, bitmap.Matrix,
                new SKPoint(info.Width - matrixSize.Width,
                            info.Height - matrixSize.Height));

            }

        }

        SKPoint ConvertToPixel(Point pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / canvasView.Height));
        }
    }
}
