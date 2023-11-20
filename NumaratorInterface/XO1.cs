using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;


namespace XO1
{
    public static class TransformationOperations
    {
        public static void SaveCanvastoFile(Canvas canvas, string filepath, int width, int height)
        {
            Rect rect = new Rect(canvas.RenderSize);
            RenderTargetBitmap rtb = new RenderTargetBitmap(width,height, 96d/2d, 96d/2d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(canvas);
            //encode as PNG
            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));
            //save to memory stream
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            pngEncoder.Save(ms);
            ms.Close();
            System.IO.File.WriteAllBytes(filepath, ms.ToArray());
        
        }
        public static void MoveUIElement(UIElement UI, double X, double Y)
        {
            if (UI.RenderTransform is TransformGroup)
            {
                if (((TransformGroup)(UI.RenderTransform)).Children.OfType<TranslateTransform>().Count<TranslateTransform>() > 0)
                {
                    TranslateTransform TS = ((TransformGroup)(UI.RenderTransform)).Children.OfType<TranslateTransform>().First<TranslateTransform>();
                    TS.X = TS.X + X;
                    TS.Y = TS.Y + Y;
                }
                else
                {
                    TranslateTransform TS = new TranslateTransform();
                    TS.X = X;
                    TS.Y = Y;
                    ((TransformGroup)(UI.RenderTransform)).Children.Add(TS);
                }
            }
            else
            {
                TransformGroup TG = new TransformGroup();
                TranslateTransform TS = new TranslateTransform();
                TS.X = X;
                TS.Y = Y;
                TG.Children.Add(TS);
                UI.RenderTransform = TG;
            }
        }
        public static void ScaleUIElement(UIElement UI, bool expand, Point Center)
        {
            if (UI.RenderTransform is TransformGroup)
            {
                if (((TransformGroup)(UI.RenderTransform)).Children.OfType<ScaleTransform>().Count<ScaleTransform>() > 0)
                {
                    ScaleTransform ST = ((TransformGroup)(UI.RenderTransform)).Children.OfType<ScaleTransform>().First<ScaleTransform>();
                    if (expand)
                    {
                        ST.ScaleX = ST.ScaleX * 1.1;
                        ST.ScaleY = ST.ScaleY * 1.1;
                        MoveUIElement(UI, (Mouse.GetPosition(UI).X - Center.X) * ST.ScaleX, (Mouse.GetPosition(UI).Y - Center.Y) * ST.ScaleY);
                    }
                    if (!expand)
                    {
                        ST.ScaleX = ST.ScaleX * 0.9;
                        ST.ScaleY = ST.ScaleY * 0.9;
                        MoveUIElement(UI, (Mouse.GetPosition(UI).X - Center.X) * ST.ScaleX, (Mouse.GetPosition(UI).Y - Center.Y) * ST.ScaleY);
                    }
                }
                else
                {
                    ScaleTransform ST = new ScaleTransform();
                    if (expand)
                    {
                        ST.ScaleX = 1.1;
                        ST.ScaleY = 1.1;
                        MoveUIElement(UI, (Mouse.GetPosition(UI).X - Center.X) * ST.ScaleX, (Mouse.GetPosition(UI).Y - Center.Y) * ST.ScaleY);
                    }
                    if (!expand)
                    {
                        ST.ScaleX = 1;
                        ST.ScaleY = 1;
                        MoveUIElement(UI, (Mouse.GetPosition(UI).X - Center.X) * ST.ScaleX, (Mouse.GetPosition(UI).Y - Center.Y) * ST.ScaleY);
                    }
                    ((TransformGroup)(UI.RenderTransform)).Children.Insert(0, ST);
                }
            }
            else
            {
                TransformGroup TG = new TransformGroup();
                ScaleTransform ST = new ScaleTransform();
                if (expand)
                {
                    ST.ScaleX = 1.1;
                    ST.ScaleY = 1.1;
                }
                if (!expand)
                {
                    ST.ScaleX = 1;
                    ST.ScaleY = 1;
                }
                TG.Children.Insert(0, ST);
                UI.RenderTransform = TG;
            }

        }
    }
}