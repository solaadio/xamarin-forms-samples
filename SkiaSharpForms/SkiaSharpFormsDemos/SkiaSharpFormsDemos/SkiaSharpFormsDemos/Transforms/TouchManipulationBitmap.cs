using System;
using System.Collections.Generic;

using SkiaSharp;

using TouchTracking;

namespace SkiaSharpFormsDemos.Transforms
{
    class TouchManipulationBitmap
    {
        SKBitmap bitmap;
        Dictionary<long, TouchManipulationInfo> touchDictionary =
            new Dictionary<long, TouchManipulationInfo>();

        public TouchManipulationBitmap(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
            Matrix = SKMatrix.MakeIdentity();
        }

        public SKMatrix Matrix { set; get; }

        public void Paint(SKCanvas canvas)
        {
            canvas.Save();
            SKMatrix matrix = Matrix;
            canvas.Concat(ref matrix);
            canvas.DrawBitmap(bitmap, 0, 0);
            canvas.Restore();
        }

        public bool HitTest(SKPoint location)
        {
            SKRect rect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            rect = Matrix.MapRect(rect);
            return rect.Contains(location);
        }

        public void ProcessTouchEvent(long id, TouchActionType type, SKPoint location)
        {
            switch (type)
            {
                case TouchActionType.Pressed:
                    touchDictionary.Add(id, new TouchManipulationInfo
                    {
                        PreviousPoint = location,
                        NewPoint = location
                    });
                    break;

                case TouchActionType.Moved:
                    TouchManipulationInfo info = touchDictionary[id];
                    info.NewPoint = location;
                    Manipulate();
                    info.PreviousPoint = info.NewPoint;
                    break;

                case TouchActionType.Released:
                    touchDictionary[id].NewPoint = location;
                    Manipulate();
                    touchDictionary.Remove(id);
                    break;

                case TouchActionType.Cancelled:
                    touchDictionary.Remove(id);
                    break;
            }
        }


        // TouchManipulationManager
        // ------------------------

        public bool EnableAnisotropicScale { set; get; } = true;

        public bool EnableOneFingerRotate { set; get; } = true;

        public bool EnableTwoFingerRotate { set; get; } // = true;         // Implies Isotropic scaling


        void Manipulate()
        {
            TouchManipulationInfo[] infos = new TouchManipulationInfo[touchDictionary.Count];
            touchDictionary.Values.CopyTo(infos, 0);

            if (infos.Length == 0)
            {
                return;
            }

            SKMatrix touchMatrix = SKMatrix.MakeIdentity();

            if (infos.Length == 1)
            {
                SKPoint prevPoint = infos[0].PreviousPoint;
                SKPoint newPoint = infos[0].NewPoint;
                SKPoint pivotPoint = Matrix.MapPoint(bitmap.Width / 2, bitmap.Height / 2);

                touchMatrix = OneFingerManipulate(prevPoint, newPoint, pivotPoint);
            }
            else if (infos.Length >= 2)
            {
                int pivotIndex = infos[0].NewPoint == infos[0].PreviousPoint ? 0 : 1;
                SKPoint pivotPoint = infos[pivotIndex].NewPoint;
                SKPoint newPoint = infos[1 - pivotIndex].NewPoint;
                SKPoint prevPoint = infos[1 - pivotIndex].PreviousPoint;

                touchMatrix = TwoFingerManipulate(prevPoint, newPoint, pivotPoint);
            }

            SKMatrix matrix = Matrix;
            SKMatrix.PostConcat(ref matrix, touchMatrix);
            Matrix = matrix;
        }

        SKMatrix OneFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
        {
            SKMatrix touchMatrix = SKMatrix.MakeIdentity();
            SKPoint delta = newPoint - prevPoint;

            if (EnableOneFingerRotate)
            {
                SKPoint oldVector = prevPoint - pivotPoint;
                SKPoint newVector = newPoint - pivotPoint;

                // Avoid rotation if fingers are close to center
                if (Magnitude(newVector) > 25 && Magnitude(oldVector) > 25)
                {
                    float prevAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                    float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                    // Calculate rotation matrix
                    float angle = newAngle - prevAngle;
                    touchMatrix = SKMatrix.MakeRotation(angle, pivotPoint.X, pivotPoint.Y);

                    // Effectively rotate the old vector
                    float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                    oldVector.X = magnitudeRatio * newVector.X;
                    oldVector.Y = magnitudeRatio * newVector.Y;

                    // Recalculate delta
                    delta = newVector - oldVector;
                }
            }

            // Multiply the rotation matrix by a translation matrix
            SKMatrix.PostConcat(ref touchMatrix, SKMatrix.MakeTranslation(delta.X, delta.Y));

            return touchMatrix;
        }

        SKMatrix TwoFingerManipulate(SKPoint prevPoint, SKPoint newPoint, SKPoint pivotPoint)
        {
            SKMatrix touchMatrix = SKMatrix.MakeIdentity();
            SKPoint oldVector = prevPoint - pivotPoint;
            SKPoint newVector = newPoint - pivotPoint;

            if (EnableTwoFingerRotate)
            {
                // Find angles from pivot point to touch points
                float oldAngle = (float)Math.Atan2(oldVector.Y, oldVector.X);
                float newAngle = (float)Math.Atan2(newVector.Y, newVector.X);

                // Calculate rotation matrix
                float angle = newAngle - oldAngle;
                touchMatrix = SKMatrix.MakeRotation(angle, pivotPoint.X, pivotPoint.Y);

                // Effectively rotate the old vector
                float magnitudeRatio = Magnitude(oldVector) / Magnitude(newVector);
                oldVector.X = magnitudeRatio * newVector.X;
                oldVector.Y = magnitudeRatio * newVector.Y;
            }

            float scaleX = 0;
            float scaleY = 0;

            if (EnableAnisotropicScale)
            {
                //scaleX = (newPoint.X - pivotPoint.X) / (prevPoint.X - pivotPoint.X);
                //scaleY = (newPoint.Y - pivotPoint.Y) / (prevPoint.Y - pivotPoint.Y);

                scaleX = newVector.X / oldVector.X;
                scaleY = newVector.Y / oldVector.Y;

            }
            else
            {
                // Determine dominant scaling direction
      //          if (Math.Abs(newVector.X - oldVector.X) > Math.Abs(newVector.Y - oldVector.Y))
     //           {
     //               scaleX = scaleY = newVector.X / oldVector.X;
      //          }
       //         else
        ////        {
          //          scaleX = scaleY = newVector.Y / oldVector.Y;
           //     }


                scaleX = scaleY = Magnitude(newVector) / Magnitude(oldVector); //  newPoint - pivotPoint) / Magnitude(prevPoint - pivotPoint);
            }

            if (!float.IsNaN(scaleX) && !float.IsInfinity(scaleX) &&
                !float.IsNaN(scaleY) && !float.IsInfinity(scaleY))
            { 
                SKMatrix.PostConcat(ref touchMatrix, SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y));
            }

            return touchMatrix;

            //    return SKMatrix.MakeScale(scaleX, scaleY, pivotPoint.X, pivotPoint.Y);
        }

        float Magnitude(SKPoint point)
        {
            return (float)Math.Sqrt(Math.Pow(point.X, 2) + Math.Pow(point.Y, 2));
        }
    }
}
