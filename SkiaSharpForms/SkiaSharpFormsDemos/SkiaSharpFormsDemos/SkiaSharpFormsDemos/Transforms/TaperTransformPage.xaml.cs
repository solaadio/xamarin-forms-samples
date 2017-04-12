using System;
using System.IO;
using System.Reflection;

using Xamarin.Forms;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace SkiaSharpFormsDemos.Transforms
{
    enum TaperSide { Left, Top, Right, Bottom }

    enum TaperCorner { LeftOrTop, RightOrBottom, Both }

    public partial class TaperTransformPage : ContentPage
    {
        SKBitmap bitmap;

        MatrixDisplay matrixDisplay = new MatrixDisplay
        {
            PerspectiveFormat = "F5"
        };

        public TaperTransformPage()
        {
            InitializeComponent();

            string resourceID = "SkiaSharpFormsDemos.Media.FacePalm.jpg";
            Assembly assembly = GetType().GetTypeInfo().Assembly;

            using (Stream stream = assembly.GetManifestResourceStream(resourceID))
            using (SKManagedStream skStream = new SKManagedStream(stream))
            {
                bitmap = SKBitmap.Decode(skStream);
            }
        }

        void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (canvasView != null)
            {
                canvasView.InvalidateSurface();
            }
        }

        void OnPickerSelectedIndexChanged(object sender, EventArgs args)
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

            TaperSide taperSide = (TaperSide)taperSidePicker.SelectedIndex;
            TaperCorner taperCorner = (TaperCorner)taperCornerPicker.SelectedIndex;
            float taperFraction = (float)taperFractionSlider.Value;

            SKMatrix taperMatrix = 
                CalculateTaperTransform(new SKSize(bitmap.Width, bitmap.Height),
                                        taperSide, taperCorner, taperFraction);

            // Display the matrix in the lower-right corner
            SKSize matrixSize = matrixDisplay.Measure(taperMatrix);

            matrixDisplay.Paint(canvas, taperMatrix,
                new SKPoint(info.Width - matrixSize.Width,
                            info.Height - matrixSize.Height));

            // Center bitmap on canvas
            float x = (info.Width - bitmap.Width) / 2;
            float y = (info.Height - bitmap.Height) / 2;

            //       SKMatrix matrix = Multiply(Multiply(SKMatrix.MakeTranslation(-x, -y), taperMatrix), SKMatrix.MakeTranslation(x, y));

            SKMatrix matrix = SKMatrix.MakeTranslation(-x, -y);
            SKMatrix.PostConcat(ref matrix, taperMatrix);
            SKMatrix.PostConcat(ref matrix, SKMatrix.MakeTranslation(x, y));


            canvas.SetMatrix(matrix);
            canvas.DrawBitmap(bitmap, x, y);
        }

        SKMatrix CalculateTaperTransform(SKSize size, TaperSide taperSide, TaperCorner taperCorner, float taperFraction)
        {
            SKMatrix matrix = SKMatrix.MakeIdentity();

            switch (taperSide)
            {
                case TaperSide.Left:
                    matrix.ScaleX = taperFraction;
                    matrix.ScaleY = taperFraction;
                    matrix.Persp0 = (taperFraction - 1) / size.Width;

                    switch (taperCorner)
                    {
                        case TaperCorner.RightOrBottom:
                            break;

                        case TaperCorner.LeftOrTop:
                            matrix.SkewY = size.Height * matrix.Persp0;
                            matrix.TransY = size.Height * (1 - taperFraction);
                            break;

                        case TaperCorner.Both:
                            matrix.SkewY = (size.Height / 2) * matrix.Persp0;
                            matrix.TransY = size.Height * (1 - taperFraction) / 2;
                            break;
                    }
                    break;

                case TaperSide.Top:
                    matrix.ScaleX = taperFraction;
                    matrix.ScaleY = taperFraction;
                    matrix.Persp1 = (taperFraction - 1) / size.Height;

                    switch (taperCorner)
                    {
                        case TaperCorner.RightOrBottom:
                            break;

                        case TaperCorner.LeftOrTop:
                            matrix.SkewX = size.Width * matrix.Persp1;
                            matrix.TransX = size.Width * (1 - taperFraction);
                            break;

                        case TaperCorner.Both:
                            matrix.SkewX = (size.Width / 2) * matrix.Persp1;
                            matrix.TransX = size.Width * (1 - taperFraction) / 2;
                            break;
                    }
                    break;

                case TaperSide.Right:
                    matrix.ScaleX = 1 / taperFraction;
                    matrix.Persp0 = (1 - taperFraction) / (size.Width * taperFraction);

                    switch (taperCorner)
                    {
                        case TaperCorner.RightOrBottom:
                            break;

                        case TaperCorner.LeftOrTop:
                            matrix.SkewY = size.Height * matrix.Persp0;
                            break;

                        case TaperCorner.Both:
                            matrix.SkewY = (size.Height / 2) * matrix.Persp0;
                            break;
                    }
                    break;

                case TaperSide.Bottom:
                    matrix.ScaleY = 1 / taperFraction;
                    matrix.Persp1 = (1 - taperFraction) / (size.Height * taperFraction);

                    switch (taperCorner)
                    {
                        case TaperCorner.RightOrBottom:
                            break;

                        case TaperCorner.LeftOrTop:
                            matrix.SkewX = size.Width * matrix.Persp1;
                            break;

                        case TaperCorner.Both:
                            matrix.SkewX = (size.Width / 2) * matrix.Persp1;
                            break;
                    }
                    break;
            }
            return matrix;
        }






        SKMatrix Multiply(SKMatrix A, SKMatrix B)
        {
            SKMatrix result;
            SKMatrix.Concat(ref result, B, A);
            return result;
        }
    }
}
