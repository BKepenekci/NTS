using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;


namespace XamlEditor
{
    public static class TreeViewEditor
    {
        public static void TreeViewOfCanvas(Canvas x, TreeView TV)
        {
            int i = 0;
            TV.Items.Clear();
            TreeViewItem TVI = new TreeViewItem();
            TVI.Header = "Main_Canvas";
            TVI.Name = "A";
            TVI.IsExpanded = true;
            x.Children.OfType<UIElement>().ToList<UIElement>().ForEach(U =>
            {
                TreeViewItem tvı = new TreeViewItem();
                TVI.Items.Add(tvı);
                tvı.Header = Convert.ToString(U.GetType());
                tvı.Name = ((TreeViewItem)tvı.Parent).Name + "_" + Convert.ToString(i);
                tvı.IsExpanded = true;
                if (Convert.ToString(U.GetType()) == "System.Windows.Controls.Canvas")
                {
                    AddItem(tvı, U);
                }
                ++i;
            });
            TV.Items.Add(TVI);
            //return TV;
        }
        public static void AddItem(TreeViewItem TV, UIElement x)
        {
            int i = 0;
            ((Canvas)x).Children.OfType<UIElement>().ToList<UIElement>().ForEach(U =>
            {
                TreeViewItem tvı = new TreeViewItem();
                TV.Items.Add(tvı);
                tvı.Header = Convert.ToString(U.GetType());
                tvı.Name = ((TreeViewItem)tvı.Parent).Name + "_" + Convert.ToString(i);
                tvı.IsExpanded = true;
                if (Convert.ToString(U.GetType()) == "System.Windows.Controls.Canvas")
                {
                    AddItem(tvı, U);
                }
                ++i;
            });
            //return TV;
        }
    }

    public class ResizingAdorner : Adorner
    {
        public delegate void heightwidthdelegate();
        public event heightwidthdelegate heightwidthchanged; 
        Thumb topLeft, topRight, bottomLeft, bottomRight;
        int TN = 0;
        int CN = 0;
        int ThumbNumber = 0;
        List<Thumb> PathPoints = new List<Thumb>();
        Point CurrentPosition;
        Point? InitialPosition = null;
        double ScaleXC = 1;
        double ScaleYC = 1;
        double SetLeft = 0;
        double SetTop = 0;
        double ScaleX = 1;
        double ScaleY = 1;
        // To store and manage the adorner's visual children.
        VisualCollection visualChildren;
        bool Firstpivot = false;

        // Initialize the ResizingAdorner.
        public ResizingAdorner(UIElement adornedElement,bool firstPivot)
            : base(adornedElement)
        {
            this.Firstpivot = firstPivot;
            visualChildren = new VisualCollection(this);
            //Control Path.Data & acording to Type of Data Add Thumb to PathPoints
            //Use Helper Method to Control Path Data
            Path adorned = AdornedElement as Path;
            PathPointsThumbHandler(adorned);
            //
            //Call a Helper Method to initialize the PathPointsThumb
            // with a cusomized cursor
            BuildAdornerPathPoints(PathPoints);
            // Call a helper method to initialize the Thumbs
            // with a customized cursors.
            BuildAdornerCorner(ref topLeft, Cursors.SizeNWSE);
            BuildAdornerCorner(ref topRight, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomLeft, Cursors.SizeNESW);
            BuildAdornerCorner(ref bottomRight, Cursors.SizeNWSE);
            //Width = ((Path)adornedElement).Data.Bounds.Right;
            //Height = ((Path)adornedElement).Data.Bounds.Bottom;

            // Add handlers for resizing.
            bottomLeft.DragDelta += new DragDeltaEventHandler(HandleBottomLeft);
            bottomRight.DragDelta += new DragDeltaEventHandler(HandleBottomRight);
            bottomRight.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(bottomRight_PreviewMouseLeftButtonDown);
            topLeft.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(topLeft_PreviewMouseLeftButtonDown);
            topLeft.DragDelta += new DragDeltaEventHandler(HandleTopLeft);
            topRight.DragDelta += new DragDeltaEventHandler(HandleTopRight);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            Path adorner = AdornedElement as Path;
            // Some arbitrary drawing implements.
            SolidColorBrush RectBrush = new SolidColorBrush(Colors.Green);
            RectBrush.Opacity = 0.3;
            Pen renderPen = new Pen(RectBrush, 1.5);
            // Draw Rectangle
            drawingContext.DrawRectangle(null, renderPen, new Rect(adorner.Data.Bounds.TopLeft, adorner.Data.Bounds.BottomRight));
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, renderRadius, renderRadius);
            //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, renderRadius, renderRadius);
        }
        void Position_MouseMove()
        {
            CurrentPosition = Mouse.GetPosition((Canvas)((Path)AdornedElement).Parent);
        }
        void PathPoints_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InitialPosition = e.GetPosition((Canvas)((Path)AdornedElement).Parent);
        }
        void PathPoints_DragDelta(object sender, DragDeltaEventArgs e)
        {
            CN = 0;
            Position_MouseMove();
            Thumb c = sender as Thumb;
            Path a = AdornedElement as Path;
            TN = Convert.ToInt32(c.Name.Replace("A", ""));
            if (a.Data is LineGeometry)
            {
                HandleLineGeometry((LineGeometry)(a.Data));
            }
            else if (a.Data is RectangleGeometry)
            {
                HandleRectangleGeometry((RectangleGeometry)(a.Data));
            }
            else if (a.Data is EllipseGeometry)
            {
                HandleEllipseGeometry((EllipseGeometry)(a.Data));
            }
            else if (a.Data is GeometryGroup)
            {
                HandleGeometryGroup((GeometryGroup)a.Data);
            }
            else if (a.Data is PathGeometry)
            {
                HandlePathGeometry((PathGeometry)a.Data);
            }
            else if (a.Data is CombinedGeometry)
            {
                HandleCombinedGeometry((CombinedGeometry)a.Data);
            }
            InvalidateVisual();
        }
        void HandleCombinedGeometry(CombinedGeometry CG)
        {
            List<Geometry> GL = new List<Geometry>();
            GL.Add(CG.Geometry1);
            GL.Add(CG.Geometry2);
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    HandleLineGeometry((LineGeometry)(G));
                }
                else if (G is RectangleGeometry)
                {
                    HandleRectangleGeometry((RectangleGeometry)G);
                }
                else if (G is EllipseGeometry)
                {
                    HandleEllipseGeometry((EllipseGeometry)G);
                }
                else if (G is PathGeometry)
                {
                    HandlePathGeometry((PathGeometry)G);
                }
                else if (G is GeometryGroup)
                {
                    HandleGeometryGroup((GeometryGroup)G);
                }
                else if (G is CombinedGeometry)
                {
                    HandleCombinedGeometry((CombinedGeometry)G);
                }
            }
        }
        void HandleGeometryGroup(GeometryGroup GG)
        {
            List<Geometry> GL = GG.Children.ToList<Geometry>();
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    HandleLineGeometry((LineGeometry)(G));
                }
                else if (G is RectangleGeometry)
                {
                    HandleRectangleGeometry((RectangleGeometry)G);
                }
                else if (G is EllipseGeometry)
                {
                    HandleEllipseGeometry((EllipseGeometry)G);
                }
                else if (G is PathGeometry)
                {
                    HandlePathGeometry((PathGeometry)G);
                }
                else if (G is GeometryGroup)
                {
                    HandleGeometryGroup((GeometryGroup)G);
                }
                else if (G is CombinedGeometry)
                {
                    HandleCombinedGeometry((CombinedGeometry)G);
                }
            }
        }
        void HandlePathGeometry(PathGeometry PG)
        {
            InvalidateVisual();
            foreach (PathFigure PF in (PG).Figures.ToList<PathFigure>())
            {
                if (CN == TN)
                {
                    HandlePathFigureThumb(PF);
                    return;
                }
                else
                {
                    CN = CN + 1;
                    foreach (PathSegment PS in PF.Segments.ToList<PathSegment>())
                    {
                        if (PS is LineSegment)
                        {
                            if (CN == TN)
                            {
                                HandleLineSegmentThumb((LineSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + 1;
                            }
                        }
                        else if (PS is ArcSegment)
                        {
                            if (CN == TN)//if you add one more thumb (Controlnumber==ThumbNumber||ControlNumber+1==ThumbNumber)
                            {
                                HandleArcSegmentThumb((ArcSegment)PS);//if you add one more thumb you have to send thumb number & control number to check which thumb is aktivated 
                                return;
                            }
                            else
                            {
                                CN = CN + 1;
                            }
                        }
                        else if (PS is PolyLineSegment)
                        {

                            if (CN == TN || CN + ((PolyLineSegment)PS).Points.Count > TN)
                            {
                                HandlePolyLineSegment((PolyLineSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + ((PolyLineSegment)PS).Points.Count;
                            }
                        }
                        else if (PS is BezierSegment)
                        {
                            if (CN == TN || CN + 3 > TN)
                            {
                                HandleBezierSegment((BezierSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + 3;
                            }
                        }
                        else if (PS is QuadraticBezierSegment)
                        {
                            if (CN == TN || CN + 2 > TN)
                            {
                                HandleQuadraticBezierSegment((QuadraticBezierSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + 2;
                            }
                        }
                        else if (PS is PolyQuadraticBezierSegment)
                        {
                            if (CN == TN || CN + ((PolyQuadraticBezierSegment)PS).Points.Count > TN)
                            {
                                HandlePolyQuadraticBezierSegment((PolyQuadraticBezierSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + ((PolyQuadraticBezierSegment)PS).Points.Count;
                            }
                        }
                        else if (PS is PolyBezierSegment)
                        {
                            if (CN == TN || CN + ((PolyBezierSegment)PS).Points.Count > TN)
                            {
                                HandlePolyBezierSegment((PolyBezierSegment)PS);
                                return;
                            }
                            else
                            {
                                CN = CN + ((PolyBezierSegment)PS).Points.Count;
                            }
                        }

                    }
                }

            }
        }
        void HandleQuadraticBezierSegment(QuadraticBezierSegment QBS)
        {
            if ((TN - CN) == 0)
            {
                QBS.Point1 = new Point(QBS.Point1.X + CurrentPosition.X - InitialPosition.Value.X,
                                                        QBS.Point1.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                InitialPosition = CurrentPosition;
                InvalidateVisual();
                return;
            }
            if ((TN - CN) == 1)
            {
                QBS.Point2 = new Point(QBS.Point2.X + CurrentPosition.X - InitialPosition.Value.X,
                                       QBS.Point2.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                InitialPosition = CurrentPosition;
                InvalidateVisual();

                return;
            }
        }
        void HandleBezierSegment(BezierSegment BS)
        {
            if ((TN - CN) == 0)
            {
                BS.Point1 = new Point(BS.Point1.X + CurrentPosition.X - InitialPosition.Value.X,
                                                        BS.Point1.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                InitialPosition = CurrentPosition;
                InvalidateVisual();
                return;
            }
            if ((TN - CN) == 1)
            {
                BS.Point2 = new Point(BS.Point2.X + CurrentPosition.X - InitialPosition.Value.X,
                                       BS.Point2.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                InitialPosition = CurrentPosition;
                InvalidateVisual();

                return;
            }
            if ((TN - CN) == 2)
            {
                BS.Point3 = new Point(BS.Point3.X + CurrentPosition.X - InitialPosition.Value.X,
                      BS.Point3.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                InitialPosition = CurrentPosition;
                InvalidateVisual();
                return;
            }
        }
        void HandlePolyBezierSegment(PolyBezierSegment PBS)
        {
            PBS.Points[TN - CN] = new Point(PBS.Points[TN - CN].X + CurrentPosition.X - InitialPosition.Value.X,
                                               PBS.Points[TN - CN].Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;
            UpdateLayout();
            InvalidateVisual();
        }
        void HandlePolyQuadraticBezierSegment(PolyQuadraticBezierSegment PQBS)
        {
            PQBS.Points[TN - CN] = new Point(PQBS.Points[TN - CN].X + CurrentPosition.X - InitialPosition.Value.X,
                                               PQBS.Points[TN - CN].Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;
            UpdateLayout();
            InvalidateVisual();
        }
        void HandlePolyLineSegment(PolyLineSegment PLS)
        {
            PLS.Points[TN - CN] = new Point(PLS.Points[TN - CN].X + CurrentPosition.X - InitialPosition.Value.X,
                                                        PLS.Points[TN - CN].Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;
            UpdateLayout();
            InvalidateVisual();
        }
        void HandleArcSegmentThumb(ArcSegment AS)
        {
            AS.Point = new Point(AS.Point.X + CurrentPosition.X - InitialPosition.Value.X,
                                                    AS.Point.Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;
        }
        void HandleLineSegmentThumb(LineSegment LS)
        {
            LS.Point = new Point(LS.Point.X + CurrentPosition.X - InitialPosition.Value.X,
                                                LS.Point.Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;

        }
        void HandlePathFigureThumb(PathFigure PF)
        {
            PF.StartPoint = new Point(PF.StartPoint.X + CurrentPosition.X - InitialPosition.Value.X,
                                            PF.StartPoint.Y + CurrentPosition.Y - InitialPosition.Value.Y);
            InitialPosition = CurrentPosition;
        }
        void HandleLineGeometry(LineGeometry LG)
        {
            if (CN + 2 > TN)
            {
                if ((CN + 2 - TN) == 2)
                {
                    LG.StartPoint = new Point(LG.StartPoint.X + CurrentPosition.X - InitialPosition.Value.X,
                                              LG.StartPoint.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                }
                if ((CN + 2 - TN) == 1)
                {
                    LG.EndPoint = new Point(LG.EndPoint.X + CurrentPosition.X - InitialPosition.Value.X,
                                                  LG.EndPoint.Y + CurrentPosition.Y - InitialPosition.Value.Y);
                }
                InitialPosition = CurrentPosition;
            }
            else
            {
                CN = CN + 2;
            }
        }
        void HandleEllipseGeometry(EllipseGeometry EG)
        {
            if (CN + 3 > TN)
            {
                if ((TN - CN) == 0)
                {
                    EG.RadiusY = EG.RadiusY - CurrentPosition.Y + InitialPosition.Value.Y;
                }
                else if ((TN - CN) == 1)
                {
                    EG.RadiusX = EG.RadiusX + CurrentPosition.X - InitialPosition.Value.X;
                }
                else if ((TN - CN) == 2)
                {
                    double x = EG.Center.X + CurrentPosition.X - InitialPosition.Value.X;
                    double y = EG.Center.Y + CurrentPosition.Y - InitialPosition.Value.Y;
                    EG.Center = new Point(x, y);
                }
                InitialPosition = CurrentPosition;

            }
            else
            {
                CN = CN + 3;
            }
        }
        void HandleRectangleGeometry(RectangleGeometry RG)
        {
            if (CN + 2 > TN)
            {
                if ((TN - CN) == 0)
                {
                    RG.Rect = new Rect(new Point(RG.Rect.TopLeft.X + CurrentPosition.X - InitialPosition.Value.X, RG.Rect.TopLeft.Y + CurrentPosition.Y - InitialPosition.Value.Y), new Point(RG.Rect.BottomRight.X, RG.Rect.BottomRight.Y));
                }
                else if ((TN - CN) == 1)
                {
                    RG.Rect = new Rect(new Point(RG.Rect.TopLeft.X, RG.Rect.TopLeft.Y), new Point(RG.Rect.BottomRight.X + CurrentPosition.X - InitialPosition.Value.X, RG.Rect.BottomRight.Y + CurrentPosition.Y - InitialPosition.Value.Y));
                }
                InitialPosition = CurrentPosition;
            }
            else
            {
                CN = CN + 2;
            }
            if (heightwidthchanged != null)
                heightwidthchanged();
        }
        void topLeft_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InitialPosition = e.GetPosition((Canvas)((Path)AdornedElement).Parent);
            if (double.IsNaN(Canvas.GetLeft(AdornedElement)))
                SetLeft = 0;
            else
                SetLeft = Canvas.GetLeft(AdornedElement);
            if (double.IsNaN(Canvas.GetTop(AdornedElement)))
                SetTop = 0;
            else
                SetTop = Canvas.GetTop(AdornedElement);
        }
        void HandleTopLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            Position_MouseMove();
            Canvas.SetLeft(AdornedElement, SetLeft + (CurrentPosition.X - InitialPosition.Value.X));
            Canvas.SetTop(AdornedElement, SetTop + (CurrentPosition.Y - InitialPosition.Value.Y));
        }
        void bottomRight_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Path adornedElement = this.AdornedElement as Path;
            InitialPosition = e.GetPosition((Canvas)((Path)AdornedElement).Parent);
            if (adornedElement.RenderTransform is TransformGroup)
                if (((TransformGroup)adornedElement.RenderTransform).Children.OfType<ScaleTransform>().Count<ScaleTransform>() > 0)
                {
                    ScaleTransform SC = ((TransformGroup)adornedElement.RenderTransform).Children.OfType<ScaleTransform>().First<ScaleTransform>();
                    ScaleX = SC.ScaleX;
                    ScaleY = SC.ScaleY;
                }
                else
                {
                    ScaleX = 1;
                    ScaleY = 1;
                }
            else
            {
                ScaleX = 1;
                ScaleY = 1;
            }

        }
        void HandleBottomRight(object sender, DragDeltaEventArgs args)
        {

            Path adornedElement = this.AdornedElement as Path;
            Thumb hitThumb = sender as Thumb;

            if (adornedElement == null || hitThumb == null) return;
            Canvas parentElement = adornedElement.Parent as Canvas;

            Position_MouseMove();

            if (adornedElement.RenderTransform is TransformGroup)
                if (((TransformGroup)adornedElement.RenderTransform).Children.OfType<ScaleTransform>().Count<ScaleTransform>() > 0)
                {
                    ScaleTransform SC = ((TransformGroup)adornedElement.RenderTransform).Children.OfType<ScaleTransform>().First<ScaleTransform>();
                    SC.ScaleX = ScaleX * (((adornedElement.Data.Bounds.BottomRight.X - adornedElement.Data.Bounds.TopLeft.X) + (CurrentPosition.X - InitialPosition.Value.X) / ScaleX) / (adornedElement.Data.Bounds.BottomRight.X - adornedElement.Data.Bounds.TopLeft.X));
                    SC.ScaleY = ScaleY * ((adornedElement.Data.Bounds.BottomRight.Y - adornedElement.Data.Bounds.TopLeft.Y) + (CurrentPosition.Y - InitialPosition.Value.Y) / ScaleY) / (adornedElement.Data.Bounds.BottomRight.Y - adornedElement.Data.Bounds.TopLeft.Y);
                    SC.CenterX = adornedElement.Data.Bounds.TopLeft.X;
                    SC.CenterY = adornedElement.Data.Bounds.TopLeft.Y;
                    ScaleXC = SC.ScaleX;
                    ScaleYC = SC.ScaleY;
                    InvalidateVisual();
                }
                else
                {
                    ScaleTransform SC = new ScaleTransform();
                    SC.ScaleX = ((adornedElement.Data.Bounds.Right + (CurrentPosition.X - InitialPosition.Value.X)) / adornedElement.Data.Bounds.Right);
                    SC.ScaleY = ((adornedElement.Data.Bounds.Bottom + (CurrentPosition.Y - InitialPosition.Value.Y)) / adornedElement.Data.Bounds.Bottom);
                    ((TransformGroup)adornedElement.RenderTransform).Children.Add(SC);
                }
            else
            {
                TransformGroup TG = new TransformGroup();
                ScaleTransform SC = new ScaleTransform();
                SC.ScaleX = ((adornedElement.Data.Bounds.Right + (CurrentPosition.X - InitialPosition.Value.X)) / adornedElement.Data.Bounds.Right);
                SC.ScaleY = ((adornedElement.Data.Bounds.Bottom + (CurrentPosition.Y - InitialPosition.Value.Y)) / adornedElement.Data.Bounds.Bottom);
                TG.Children.Add(SC);
                adornedElement.RenderTransform = TG;
            }
        }

        // Handler for resizing from the top-right.
        void HandleTopRight(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = this.AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;
        }

        // Handler for resizing from the bottom-left.
        void HandleBottomLeft(object sender, DragDeltaEventArgs args)
        {
            FrameworkElement adornedElement = AdornedElement as FrameworkElement;
            Thumb hitThumb = sender as Thumb;
        }

        // Arrange the Adorners.
        protected override Size ArrangeOverride(Size finalSize)
        {
            Path adorned = AdornedElement as Path;

            // desiredWidth and desiredHeight are the width and height of the element that's being adorned.  
            // These will be used to place the ResizingAdorner at the corners of the adorned element.  
            double desiredWidth = adorned.Data.Bounds.BottomRight.X;
            double desiredHeight = adorned.Data.Bounds.BottomRight.Y;
            // adornerWidth & adornerHeight are used for placement as well.
            double adornerWidth = adorned.Data.Bounds.TopLeft.X;
            double adornerHeight = adorned.Data.Bounds.TopLeft.Y;

            /*if(adorned.Data is LineGeometry)
            {
                PathPoints[0].Arrange(new Rect(new Point(0, 0), new Point(((LineGeometry)(adorned.Data)).StartPoint.X*2, ((LineGeometry)(adorned.Data)).StartPoint.Y*2)));
            }*/
            //Arrange PathPoints with the helper Methods
            if(this.Firstpivot)
                ArrangePathPointsThumbs(PathPoints);
            topLeft.Arrange(new Rect(new Point(adornerWidth, adornerHeight), new Point(desiredWidth, desiredHeight)));
            //topRight.Arrange(new Rect(new Point(0, 0), new Point(desiredWidth * 2, adornerHeight * 2)));
            //bottomLeft.Arrange(new Rect(new Point(0, 0), new Point(adornerWidth * 2, desiredHeight * 2)));
            //bottomRight.Arrange(new Rect(new Point(0, 0), new Point(desiredWidth * 2, desiredHeight * 2)));
            // Return the final size.
            return finalSize;
        }
        void ArrangePathPointsThumbs(List<Thumb> a)
        {
            Path adorned = AdornedElement as Path;
            if (adorned.Data is LineGeometry)
            {
                ThumbNumber = 0;
                ArrangeLineGeometryThumbs((LineGeometry)(adorned.Data), ThumbNumber);
            }
            else if (adorned.Data is RectangleGeometry)
            {
                ThumbNumber = 0;
                ArrangeRectangleGeometryThumbs((RectangleGeometry)(adorned.Data), ThumbNumber);
            }
            else if (adorned.Data is EllipseGeometry)
            {
                ThumbNumber = 0;
                ArrangeEllipseGeometryThumbs((EllipseGeometry)(adorned.Data), ThumbNumber);
            }
            else if (adorned.Data is GeometryGroup)
            {
                ThumbNumber = 0;
                ArrangeGeometryGroupThumbs((GeometryGroup)(adorned.Data));
            }
            else if (adorned.Data is PathGeometry)
            {
                ThumbNumber = 0;
                ArrangePathGeometryThumbs((PathGeometry)adorned.Data);
            }
            else if (adorned.Data is CombinedGeometry)
            {
                ThumbNumber = 0;
                ArrangeCombinedGeometryThumbs((CombinedGeometry)adorned.Data);
            }
        }
        void ArrangeCombinedGeometryThumbs(CombinedGeometry CG)
        {
            List<Geometry> GL = new List<Geometry>();
            GL.Add(CG.Geometry1);
            GL.Add(CG.Geometry2);
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    ArrangeLineGeometryThumbs((LineGeometry)G, ThumbNumber);
                }
                else if (G is PathGeometry)
                {
                    ArrangePathGeometryThumbs((PathGeometry)G);
                }
                else if (G is RectangleGeometry)
                {
                    ArrangeRectangleGeometryThumbs((RectangleGeometry)G, ThumbNumber);
                }
                else if (G is EllipseGeometry)
                {
                    ArrangeEllipseGeometryThumbs((EllipseGeometry)G, ThumbNumber);
                }
                else if (G is GeometryGroup)
                {
                    ArrangeGeometryGroupThumbs((GeometryGroup)G);
                }
                else if (G is CombinedGeometry)
                {
                    ArrangeCombinedGeometryThumbs((CombinedGeometry)G);
                }
            }

        }
        void ArrangeEllipseGeometryThumbs(EllipseGeometry EG, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point((EG.Bounds.TopLeft.X + Math.Abs(EG.RadiusX)) * 2 + 40, EG.Bounds.Top * 2 + 40)));
            PathPoints[i + 1].Arrange(new Rect(new Point(-40, -40), new Point(EG.Bounds.Right * 2 + 40, (EG.Bounds.Top + Math.Abs(EG.RadiusY)) * 2 + 40)));
            PathPoints[i + 2].Arrange(new Rect(new Point(-40, -40), new Point((EG.Bounds.TopLeft.X + EG.Bounds.BottomRight.X) + 40, (EG.Bounds.TopLeft.Y + EG.Bounds.BottomRight.Y) + 40)));
            ThumbNumber = ThumbNumber + 3;
        }
        void ArrangeRectangleGeometryThumbs(RectangleGeometry RG, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(RG.Rect.TopLeft.X * 2 + 40, RG.Rect.TopLeft.Y * 2 + 40)));
            PathPoints[i + 1].Arrange(new Rect(new Point(-40, -40), new Point(RG.Rect.BottomRight.X * 2 + 40, RG.Rect.BottomRight.Y * 2 + 40)));
            ThumbNumber = ThumbNumber + 2;
        }
        void ArrangeLineGeometryThumbs(LineGeometry a, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point((a).StartPoint.X * 2 + 40, (a).StartPoint.Y * 2 + 40)));
            PathPoints[i + 1].Arrange(new Rect(new Point(-40, -40), new Point((a).EndPoint.X * 2 + 40, (a).EndPoint.Y * 2 + 40)));
            ThumbNumber = ThumbNumber + 2;
        }
        void ArrangePathGeometryThumbs(PathGeometry PG)
        {
            List<PathFigure> PFL = PG.Figures.ToList();
            foreach (PathFigure PF in PFL)
            {
                ArrangePathFigureThumbs(PF, ThumbNumber);
                ThumbNumber = ThumbNumber + 1;
            }
        }
        void ArrangePathFigureThumbs(PathFigure PF, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(PF.StartPoint.X * 2 + 40, PF.StartPoint.Y * 2 + 40)));
            List<PathSegment> PSL = PF.Segments.ToList<PathSegment>();
            foreach (PathSegment PS in PSL)
            {
                if (PS is LineSegment)
                {
                    ThumbNumber = ThumbNumber + 1;
                    ArrangeLineSegmentThumbs((LineSegment)PS, ThumbNumber);
                }
                else if (PS is ArcSegment)
                {
                    ThumbNumber = ThumbNumber + 1;
                    ArrangeArcSegmentThumbs((ArcSegment)PS, ThumbNumber);

                }
                else if (PS is PolyLineSegment)
                {
                    ThumbNumber = ThumbNumber + ((PolyLineSegment)PS).Points.Count;
                    ArrangePolyLineSegment((PolyLineSegment)PS, ThumbNumber);
                }
                else if (PS is BezierSegment)
                {
                    ThumbNumber = ThumbNumber + 3;
                    ArrangeBezierSegmentThumbs((BezierSegment)PS, ThumbNumber);
                }
                else if (PS is QuadraticBezierSegment)
                {
                    ThumbNumber = ThumbNumber + 2;
                    ArrangeQuadraticBezierSegmentThumbs((QuadraticBezierSegment)PS, ThumbNumber);
                }
                else if (PS is PolyBezierSegment)
                {
                    ThumbNumber = ThumbNumber + ((PolyBezierSegment)PS).Points.Count;
                    ArrangePolyBezierSegmentThumbs((PolyBezierSegment)PS, ThumbNumber);
                }
                else if (PS is PolyQuadraticBezierSegment)
                {
                    ThumbNumber = ThumbNumber + ((PolyBezierSegment)PS).Points.Count;
                    ArrangePolyQuadraticBezierSegmentThumbs((PolyQuadraticBezierSegment)PS, ThumbNumber);
                }
            }
        }
        void ArrangeQuadraticBezierSegmentThumbs(QuadraticBezierSegment QBS, int i)
        {
            PathPoints[i].Arrange((new Rect(new Point(-40, -40), new Point(QBS.Point2.X * 2 + 40, QBS.Point2.Y * 2 + 40))));
            PathPoints[i - 1].Arrange((new Rect(new Point(-40, -40), new Point(QBS.Point1.X * 2 + 40, QBS.Point1.Y * 2 + 40))));
        }
        void ArrangePolyBezierSegmentThumbs(PolyBezierSegment PBS, int i)
        {
            for (int j = 0; j < PBS.Points.Count; ++j)
            {
                PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(PBS.Points[PBS.Points.Count - 1 - j].X * 2 + 40, PBS.Points[PBS.Points.Count - 1 - j].Y * 2 + 40)));
                --i;
            }
        }
        void ArrangePolyQuadraticBezierSegmentThumbs(PolyQuadraticBezierSegment PQBS, int i)
        {
            for (int j = 0; j < PQBS.Points.Count; ++j)
            {
                PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(PQBS.Points[PQBS.Points.Count - 1 - j].X * 2 + 40, PQBS.Points[PQBS.Points.Count - 1 - j].Y * 2 + 40)));
                --i;
            }
        }
        void ArrangePolyLineSegment(PolyLineSegment PLS, int i)
        {
            for (int j = 0; j < PLS.Points.Count; ++j)
            {
                PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(PLS.Points[PLS.Points.Count - 1 - j].X * 2 + 40, PLS.Points[PLS.Points.Count - 1 - j].Y * 2 + 40)));
                --i;
            }
        }
        void ArrangeArcSegmentThumbs(ArcSegment AS, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(AS.Point.X * 2 + 40, AS.Point.Y * 2 + 40)));
        }
        void ArrangeLineSegmentThumbs(LineSegment LS, int i)
        {
            PathPoints[i].Arrange(new Rect(new Point(-40, -40), new Point(LS.Point.X * 2 + 40, LS.Point.Y * 2 + 40)));
        }
        void ArrangeBezierSegmentThumbs(BezierSegment BS, int i)
        {
            PathPoints[i].Arrange((new Rect(new Point(-40, -40), new Point(BS.Point3.X * 2 + 40, BS.Point3.Y * 2 + 40))));
            PathPoints[i - 1].Arrange((new Rect(new Point(-40, -40), new Point(BS.Point2.X * 2 + 40, BS.Point2.Y * 2 + 40))));
            PathPoints[i - 2].Arrange((new Rect(new Point(-40, -40), new Point(BS.Point1.X * 2 + 40, BS.Point1.Y * 2 + 40))));
        }
        void ArrangeGeometryGroupThumbs(GeometryGroup GG)
        {
            List<Geometry> GL = GG.Children.ToList<Geometry>();
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    ArrangeLineGeometryThumbs((LineGeometry)G, ThumbNumber);
                }
                else if (G is PathGeometry)
                {
                    ArrangePathGeometryThumbs((PathGeometry)G);
                }
                else if (G is RectangleGeometry)
                {
                    ArrangeRectangleGeometryThumbs((RectangleGeometry)G, ThumbNumber);
                }
                else if (G is EllipseGeometry)
                {
                    ArrangeEllipseGeometryThumbs((EllipseGeometry)G, ThumbNumber);
                }
                else if (G is GeometryGroup)
                {
                    ArrangeGeometryGroupThumbs((GeometryGroup)G);
                }
                else if (G is CombinedGeometry)
                {
                    ArrangeCombinedGeometryThumbs((CombinedGeometry)G);
                }
            }

        }
        // Helper method to instantiate the corner Thumbs, set the Cursor property, 
        // set some appearance properties, and add the elements to the visual tree.
        void BuildAdornerCorner(ref Thumb cornerThumb, Cursor customizedCursor)
        {
            Path adornered = AdornedElement as Path;
            if (cornerThumb != null) return;
            cornerThumb = new Thumb();
            // Set some arbitrary visual characteristics.
            cornerThumb.Cursor = customizedCursor;
            cornerThumb.Height = cornerThumb.Width = 25;
            cornerThumb.Opacity = 1;
            cornerThumb.Background = new SolidColorBrush(Colors.Black);
            visualChildren.Add(cornerThumb);
        }
        void BuildAdornerPathPoints(List<Thumb> a)
        {
            foreach (Thumb b in a)
            {
                b.Cursor = Cursors.Pen;
                b.Style = (Style)FindResource("ThumbStyle");
                visualChildren.Add(b);
            }
        }
        // UnScale Adorners
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {

            Path Adorned = AdornedElement as Path;

            if (this.visualChildren != null)
            {
                foreach (var thumb in this.visualChildren.OfType<Thumb>())
                {
                    thumb.RenderTransform
                        = new ScaleTransform(1 / ScaleXC, 1 / ScaleYC);
                    thumb.RenderTransformOrigin = new Point(0.5, 0.5);
                }
            }
            return base.GetDesiredTransform(transform);
        }
        // Override the VisualChildrenCount and GetVisualChild properties to interface with 
        // the adorner's visual collection.
        protected override int VisualChildrenCount { get { return visualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return visualChildren[index]; }
        protected void PathPointsThumbHandler(Path a)
        {
            if (a.Data is LineGeometry)
            {
                LineGeometryThumbHandler((LineGeometry)a.Data);
            }
            else if (a.Data is RectangleGeometry)
            {
                RectangleGeometryThumbHandler((RectangleGeometry)a.Data);
            }
            else if (a.Data is EllipseGeometry)
            {
                EllipseGeometryThumbHandler((EllipseGeometry)a.Data);
            }
            else if (a.Data is GeometryGroup)
            {
                GeometryGroupThumbHandler((GeometryGroup)a.Data);
            }
            else if (a.Data is PathGeometry)
            {
                PathGeometryThumbHandler((PathGeometry)a.Data);
            }
            else if (a.Data is CombinedGeometry)
            {
                CombinedGeometryThumbHandler((CombinedGeometry)a.Data);
            }
        }
        protected void CombinedGeometryThumbHandler(CombinedGeometry CG)
        {
            Geometry G1 = CG.Geometry1;
            Geometry G2 = CG.Geometry2;
            List<Geometry> GL = new List<Geometry>();
            GL.Add(G1);
            GL.Add(G2);
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    LineGeometryThumbHandler((LineGeometry)G);
                }
                else if (G is RectangleGeometry)
                {
                    RectangleGeometryThumbHandler((RectangleGeometry)G);
                }
                else if (G is EllipseGeometry)
                {
                    EllipseGeometryThumbHandler((EllipseGeometry)G);
                }
                else if (G is GeometryGroup)
                {
                    GeometryGroupThumbHandler((GeometryGroup)G);
                }
                else if (G is PathGeometry)
                {
                    PathGeometryThumbHandler((PathGeometry)G);
                }
                else if (G is CombinedGeometry)
                {
                    CombinedGeometryThumbHandler((CombinedGeometry)G);
                }
            }
        }
        protected void EllipseGeometryThumbHandler(EllipseGeometry EG)
        {
            PathPoints.Add(new Thumb());
            PathPoints.Add(new Thumb());
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 3].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 3].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 2].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 2].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 3].Name = "A" + Convert.ToString(PathPoints.Count - 3);
            PathPoints[PathPoints.Count - 2].Name = "A" + Convert.ToString(PathPoints.Count - 2);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
        }
        protected void RectangleGeometryThumbHandler(RectangleGeometry RG)
        {
            PathPoints.Add(new Thumb());
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 2].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 2].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 2].Name = "A" + Convert.ToString(PathPoints.Count - 2);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
        }
        protected void LineGeometryThumbHandler(LineGeometry LG)
        {
            PathPoints.Add(new Thumb());
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 2].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 2].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 2].Name = "A" + Convert.ToString(PathPoints.Count - 2);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
        }
        protected void LineSegmentThumbHandler(LineSegment LS)
        {
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
        }
        protected void PolyLineSegmentThumbHandler(PolyLineSegment PLS)
        {
            foreach (Point p in PLS.Points.ToList<Point>())
            {
                PathPoints.Add(new Thumb());
                PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
                PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
                PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            }
        }
        protected void PolyBezierSegmentThumbHandler(PolyBezierSegment PBS)
        {
            foreach (Point p in PBS.Points.ToList<Point>())
            {
                PathPoints.Add(new Thumb());
                PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
                PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
                PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            }
        }
        protected void PolyQuadraticBezierSegmentThumbHandler(PolyQuadraticBezierSegment PQBS)
        {
            foreach (Point p in PQBS.Points.ToList<Point>())
            {
                PathPoints.Add(new Thumb());
                PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
                PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
                PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            }
        }
        protected void ArcSegmentThumbHandler(ArcSegment AS)
        {
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);

        }
        protected void BezierSegmentThumbHandler(BezierSegment BS)
        {
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);

        }
        protected void QuadraticBezierSegmentThumbHandler(QuadraticBezierSegment QBS)
        {
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
        }
        protected void PathGeometryThumbHandler(PathGeometry PG)
        {
            List<PathFigure> PFL = PG.Figures.ToList<PathFigure>();
            foreach (PathFigure PF in PFL)
            {
                PathFigureThumbHandler(PF);
            }
        }
        protected void PathFigureThumbHandler(PathFigure PF)
        {
            PathPoints.Add(new Thumb());
            PathPoints[PathPoints.Count - 1].PreviewMouseLeftButtonDown += new MouseButtonEventHandler(PathPoints_PreviewMouseLeftButtonDown);
            PathPoints[PathPoints.Count - 1].DragDelta += new DragDeltaEventHandler(PathPoints_DragDelta);
            PathPoints[PathPoints.Count - 1].Name = "A" + Convert.ToString(PathPoints.Count - 1);
            List<PathSegment> PSL = PF.Segments.ToList<PathSegment>();
            foreach (PathSegment PS in PSL)
            {
                if (PS is LineSegment)
                {
                    LineSegmentThumbHandler((LineSegment)PS);
                }
                else if (PS is ArcSegment)
                {
                    ArcSegmentThumbHandler((ArcSegment)PS);
                }
                else if (PS is PolyLineSegment)
                {
                    PolyLineSegmentThumbHandler((PolyLineSegment)PS);
                }
                else if (PS is BezierSegment)
                {
                    BezierSegmentThumbHandler((BezierSegment)PS);
                }
                else if (PS is QuadraticBezierSegment)
                {
                    QuadraticBezierSegmentThumbHandler((QuadraticBezierSegment)PS);
                }
                else if (PS is PolyBezierSegment)
                {
                    PolyBezierSegmentThumbHandler((PolyBezierSegment)PS);
                }
                else if (PS is PolyQuadraticBezierSegment)
                {
                    PolyQuadraticBezierSegmentThumbHandler((PolyQuadraticBezierSegment)PS);
                }
            }
        }
        protected void GeometryGroupThumbHandler(GeometryGroup GG)
        {
            List<Geometry> GL = GG.Children.ToList<Geometry>();
            foreach (Geometry G in GL)
            {
                if (G is LineGeometry)
                {
                    LineGeometryThumbHandler((LineGeometry)G);
                }
                else if (G is RectangleGeometry)
                {
                    RectangleGeometryThumbHandler((RectangleGeometry)G);
                }
                else if (G is EllipseGeometry)
                {
                    EllipseGeometryThumbHandler((EllipseGeometry)G);
                }
                else if (G is PathGeometry)
                {
                    PathGeometryThumbHandler((PathGeometry)G);
                }
                else if (G is GeometryGroup)
                {
                    GeometryGroupThumbHandler((GeometryGroup)G);
                }
                else if (G is CombinedGeometry)
                {
                    CombinedGeometryThumbHandler((CombinedGeometry)G);
                }
            }

        }
    }

    public class TextBoxAdorner : Adorner
    {
        Thumb TopLeft, Right, Bottom, BottomRight, Rotate;
        VisualCollection VisualChildren;

        public TextBoxAdorner(TextBox AdornedElement)
            : base(AdornedElement)
        {
            VisualChildren = new VisualCollection(this);
            TopLeft = new Thumb();
            Right = new Thumb();
            Bottom = new Thumb();
            BottomRight = new Thumb();
            Rotate = new Thumb();
            BuildAdorners();
            ThumbHandler();
        }
        private void ThumbHandler()
        { 
            this.TopLeft.DragDelta+=new DragDeltaEventHandler(TopLeftDragDelta);
            this.Right.DragDelta+=new DragDeltaEventHandler(RightDragDelta);
            this.Bottom.DragDelta+=new DragDeltaEventHandler(BottomDragDelta);
            this.BottomRight.DragDelta+=new DragDeltaEventHandler(BottomRightDragDelta);
            this.Rotate.DragDelta+=new DragDeltaEventHandler(RotateDragDelta);
        }
        private void BuildAdorners()
        {
            BuildAdorner(this.TopLeft,1);
            BuildAdorner(this.Right,2);
            BuildAdorner(this.Bottom,3); 
            BuildAdorner(this.BottomRight,4);
            BuildAdorner(this.Rotate,5);
        }
        private void BuildAdorner(Thumb T,int i)
        {
            T.Height = T.Width = 10;
            T.Opacity = 0.8;
            T.Background = new SolidColorBrush(Colors.Black);
            if (i == 1)
            {
                T.Style = (Style)FindResource("TopLeft");
            }
            else if (i == 2)
            {
                T.Style = (Style)FindResource("Right");
            }
            else if (i == 3)
            {
                T.Style = (Style)FindResource("Bottom");
            }
            else if (i == 4)
            {
                T.Style = (Style)FindResource("BottomRight");
                T.Width = T.Height = 7;
            }
            else if(i==5)
            {
                T.Style = (Style)FindResource("Rotate");
            }
            VisualChildren.Add(T);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect AdornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            TextBox adorner = AdornedElement as TextBox;
            // Some arbitrary drawing implements.
            SolidColorBrush RectBrush = new SolidColorBrush(Colors.Green);
            RectBrush.Opacity = 0.3;
            Pen renderPen = new Pen(RectBrush, 1.5);

            // Draw Rectangle
            drawingContext.DrawRectangle(null, renderPen, new Rect(new Point(0,0), new Point(adorner.Width,adorner.Height)));
            drawingContext.DrawLine(renderPen, new Point(adorner.Width / 2, 0), new Point(adorner.Width / 2, -10));
            drawingContext.DrawEllipse(Brushes.Green, renderPen, new Point(adorner.Width / 2, -9.5), 1.5, 1.5);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            TextBox adorned = AdornedElement as TextBox;
            TopLeft.Arrange(new Rect(new Point(-20, -20), new Point(20, 20)));
            Right.Arrange(new Rect(new Point(-20,-20), new Point(adorned.Width * 2+20, adorned.Height+20)));
            Bottom.Arrange(new Rect(new Point(-20, -20), new Point(adorned.Width+20, adorned.Height * 2+20)));
            BottomRight.Arrange(new Rect(new Point(-20, -20), new Point(adorned.Width*2+20, adorned.Height*2+20)));
            Rotate.Arrange(new Rect(new Point(-20,-20),new Point(adorned.Width+20,0)));
            return finalSize;
        }
        protected override int VisualChildrenCount { get { return VisualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return VisualChildren[index]; }

        //Thumbs DragDeltaEvents
        private void TopLeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            TextBox T=AdornedElement as TextBox;
            Point p=Mouse.GetPosition((UIElement)T.Parent);
            if (!(T.RenderTransform is RotateTransform))
                T.RenderTransform = new RotateTransform();    
            double Angle = Vector.AngleBetween(new Vector(-1,0), new Vector(-T.Width,-T.Height));
            double Angle1 = ((RotateTransform)(T.RenderTransform)).Angle;
            double d = Math.Sqrt(Math.Pow(T.Width / 2, 2) + Math.Pow(T.Height / 2, 2));
            double dx = d * Math.Cos((Angle + Angle1)*Math.PI/180) - d * Math.Cos(Angle*Math.PI/180);
            double dy = d * Math.Sin((Angle + Angle1)*Math.PI/180) - d * Math.Sin(Angle*Math.PI/180);
            Canvas.SetLeft(T, p.X+dx);
            Canvas.SetTop(T, p.Y+dy);
        }
        private void RightDragDelta(object sender, DragDeltaEventArgs e)
        {
            TextBox T = AdornedElement as TextBox;
            if (!(T.RenderTransform is RotateTransform))
                T.RenderTransform = new RotateTransform();

            Point TL = Mouse.GetPosition(TopLeft);
            T.Width = Math.Max(TL.X,0.1);
        }
        private void BottomDragDelta(object sender, DragDeltaEventArgs e)
        {
            TextBox T = AdornedElement as TextBox;

            Point p = Mouse.GetPosition(TopLeft);
            T.Height = Math.Max(p.Y, 0.1);
        }
        private void BottomRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            TextBox T = AdornedElement as TextBox;
            Point p = Mouse.GetPosition(TopLeft);
            T.Width = Math.Max(p.X ,0.1);
            T.Height = Math.Max(p.Y, 0.1);
        }
        private void RotateDragDelta(object sender, DragDeltaEventArgs e)
        {
            
            TextBox T = AdornedElement as TextBox;
            Point p = Mouse.GetPosition((UIElement)T.Parent);
            Point M = new Point(Canvas.GetLeft(T) + T.Width / 2, Canvas.GetTop(T) + T.Height / 2);
            double Angle = Vector.AngleBetween(new Vector(0,-1), new Vector(p.X-M.X,p.Y-M.Y));
            T.RenderTransform = new RotateTransform(Angle,T.Width/2,T.Height/2);
        }
    }

    public class ImageAdorner : Adorner
    {
        Thumb TopLeft, BottomRight;
        VisualCollection VisualChildren;

        public ImageAdorner(Image AdornedElement)
            : base(AdornedElement)
        {
            VisualChildren = new VisualCollection(this);
            TopLeft = new Thumb();
            BottomRight = new Thumb();
            BuildAdorners();
            ThumbHandler();
        }
        private void BuildAdorners()
        {
            TopLeft.Height = TopLeft.Width = 10;
            TopLeft.Opacity = 0.8;
            TopLeft.Background = new SolidColorBrush(Colors.Black);
            TopLeft.Style = (Style)FindResource("TopLeft");
            VisualChildren.Add(TopLeft);

            BottomRight.Height = BottomRight.Width = 10;
            BottomRight.Opacity = 0.8;
            BottomRight.Background = new SolidColorBrush(Colors.Black);
            BottomRight.Style = (Style)FindResource("BottomRight");
            VisualChildren.Add(BottomRight);
        }
        private void ThumbHandler()
        {
            TopLeft.DragDelta+=new DragDeltaEventHandler(TopLeftDragDelta);
            BottomRight.DragDelta+=new DragDeltaEventHandler(BottomRightDragDelta);
        }
        private void TopLeftDragDelta(object sender, DragDeltaEventArgs e)
        {
            Image adorned = AdornedElement as Image;
            Point p = Mouse.GetPosition((UIElement)adorned.Parent);
            Canvas.SetLeft(adorned, p.X);
            Canvas.SetTop(adorned, p.Y);
        }
        private void BottomRightDragDelta(object sender, DragDeltaEventArgs e)
        {
            Image adorned = AdornedElement as Image;
            Point p = Mouse.GetPosition((UIElement)adorned.Parent);
            double x = adorned.Source.Width;
            double y = adorned.Source.Height;
            ScaleTransform ST = new ScaleTransform((p.X-Canvas.GetLeft(adorned))/x,(p.Y-Canvas.GetTop(adorned))/y);
            adorned.RenderTransform = ST;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {

            Image Adorned = AdornedElement as Image;

            if (this.VisualChildren != null)
            {
                foreach (var thumb in this.VisualChildren.OfType<Thumb>())
                {
                    if(Adorned.RenderTransform is ScaleTransform)
                    thumb.RenderTransform
                        = new ScaleTransform(1 / ((ScaleTransform)(Adorned.RenderTransform)).ScaleX, 1 / ((ScaleTransform)(Adorned.RenderTransform)).ScaleY);
                    thumb.RenderTransformOrigin = new Point(0.5, 0.5);
                }
            }
            return base.GetDesiredTransform(transform);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            Rect AdornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            Image adorner = AdornedElement as Image;
            // Some arbitrary drawing implements.
            SolidColorBrush RectBrush = new SolidColorBrush(Colors.Green);
            RectBrush.Opacity = 0.3;
            Pen renderPen = new Pen(RectBrush, 1.5);

            // Draw Rectangle
            drawingContext.DrawRectangle(null, renderPen, new Rect(new Point(0, 0), new Point(adorner.Width, adorner.Height)));
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            Image adorned = AdornedElement as Image;
            TopLeft.Arrange(new Rect(new Point(-20, -20), new Point(20, 20)));
            BottomRight.Arrange(new Rect(new Point(-20, -20), new Point(adorned.Source.Width * 2 + 20, adorned.Source.Height * 2 + 20)));
            return finalSize;
        }
        protected override int VisualChildrenCount { get { return VisualChildren.Count; } }
        protected override Visual GetVisualChild(int index) { return VisualChildren[index]; }
    }
    public class PathCreator
    {
        public static Path LineCreater(Point a, Point b)
        {
            Path x = new Path();
            LineGeometry LG = new LineGeometry(a, b);
            x.Data = LG;
            return x;
        }
        public static Path PolyLineCreater(List<Point> a)
        {
            int i = 0;
            Path y = new Path();
            PathGeometry PG = new PathGeometry();
            PathFigure PF = new PathFigure();
            PolyLineSegment LS = new PolyLineSegment();
            foreach (Point x in a)
            {
                if (i != 0)
                {
                    LS = new PolyLineSegment();
                    foreach (Point b in a)
                        LS.Points.Add(b);
                }
                ++i;
            }
            PF.Segments.Add(LS);
            PG.Figures.Add(PF);
            PF.StartPoint = a[0];
            y.Data = PG;
            return y;
        }
        public static Path ArcCreater(List<Point> c)
        {
            Path x = new Path();
            double m1, m2, n1, n2, Cx, Cy, r;
            bool direction;
            SweepDirection s = new SweepDirection();
            double Angle = 0;
            m1 = (c[0].X - c[1].X) / (c[1].Y - c[0].Y);
            m2 = (c[2].X - c[1].X) / (c[1].Y - c[2].Y);
            n1 = ((c[0].Y + c[1].Y) / 2) -
                ((Math.Pow(c[0].X, 2) - Math.Pow(c[1].X, 2)) / (2 * c[1].Y - 2 * c[0].Y));
            n2 = ((c[1].Y + c[2].Y) / 2) -
                ((Math.Pow(c[1].X, 2) - Math.Pow(c[2].X, 2)) / (2 * c[2].Y - 2 * c[1].Y));
            Cx = (n2 - n1) / (m1 - m2);
            Cy = (m2 * Cx + n2);
            r = Math.Sqrt(Math.Pow((c[1].X - Cx), 2) + Math.Pow((c[1].Y - Cy), 2));
            direction = (c[0].Y - c[1].Y) / (c[1].X - c[0].X) > (c[0].Y - c[2].Y) / (c[2].X - c[0].X);
            if (direction)
            {
                s = SweepDirection.Clockwise;
                Angle = Vector.AngleBetween(new Vector(c[0].X - Cx, c[0].Y - Cy), new Vector(c[2].X - Cx, c[2].Y - Cy));
            }
            else
            {
                s = SweepDirection.Counterclockwise;
                Angle = -Vector.AngleBetween(new Vector(c[2].X - Cx, Cy - c[2].Y), new Vector(c[0].X - Cx, Cy - c[0].Y));
            }

            ArcSegment AS = new ArcSegment(new Point(c[2].X, c[2].Y), new Size(r, r), 0, Angle < 0, s, true);
            PathGeometry PG = new PathGeometry();
            PathFigure PF = new PathFigure();
            PF.Segments.Add(AS);
            PG.Figures.Add(PF);
            PF.StartPoint = c[0];
            x.Data = PG;

            return x;
        }
        public static Path BezierCreater(List<Point> b)
        {
            Path x = new Path();
            PathGeometry PG = new PathGeometry();
            PathFigure PF = new PathFigure();
            BezierSegment BS = new BezierSegment();
            PG.Figures.Add(PF);
            PF.Segments.Add(BS);
            x.Data = PG;
            PF.StartPoint = b[0];
            BS.Point1 = b[1];
            BS.Point2 = b[2];
            BS.Point3 = b[3];
            return x;
        }
        public static Path QuadraticBezierCreater(List<Point> p)
        {
            Path x = new Path();
            PathGeometry PG = new PathGeometry();
            PathFigure PF = new PathFigure();
            QuadraticBezierSegment QBS = new QuadraticBezierSegment();
            PG.Figures.Add(PF);
            PF.Segments.Add(QBS);
            x.Data = PG;
            PF.StartPoint = p[0];
            QBS.Point1 = p[1];
            QBS.Point2 = p[2];
            return x;
        }
        public static Path SplineCreater(List<Point> s)
        {
            PathFigure PF = new PathFigure();
            PathGeometry PG = new PathGeometry();
            PathSegmentCollection PSC = new PathSegmentCollection();
            Path x = new Path();
            if (s.Count == 2)
            {
                x = LineCreater(s[0], s[1]);
                return x;
            }
            else if (s.Count == 3)
            {
                x = QuadraticBezierCreater(s);
                return x;
            }
            else if (s.Count == 4)
            {
                x = BezierCreater(s);
                return x;
            }
            else if (s.Count > 4)
            {
                int BezierCount = Convert.ToInt32((s.Count - 4) / 2);
                BezierCount += 1;
                int QuadraticBezierCount = (s.Count) % 2;
                for (int j = 0; j < BezierCount; ++j)
                {
                    if (j == 0)
                    {
                        List<Point> a = new List<Point>();
                        for (int k = 0; k < 4; ++k)
                        {
                            a.Add(s[k]);
                        }
                        PSC.Add((BezierSegment)(((PathGeometry)(BezierCreater(a).Data)).Figures[0].Segments[0]));
                    }
                    else
                    {
                        int pp = j * 2 + 2 - 1;
                        List<Point> a = new List<Point>();
                        a.Add(new Point(0, 0));
                        a.Add(new Point(s[pp].X + s[pp].X - s[pp - 1].X, s[pp].Y + s[pp].Y - s[pp - 1].Y));
                        a.Add(s[pp + 1]);
                        a.Add(s[pp + 2]);
                        PSC.Add((BezierSegment)(((PathGeometry)(BezierCreater(a).Data)).Figures[0].Segments[0]));
                    }
                }
                if (QuadraticBezierCount == 1)
                {
                    int pp = s.Count - 2;
                    List<Point> a = new List<Point>();
                    a.Add(new Point(0, 0));
                    a.Add(new Point(s[pp].X + s[pp].X - s[pp - 1].X, s[pp].Y + s[pp].Y - s[pp - 1].Y));
                    a.Add(s[s.Count - 1]);
                    PSC.Add((QuadraticBezierSegment)(((PathGeometry)(QuadraticBezierCreater(a).Data)).Figures[0].Segments[0]));
                }
            }
            PF.StartPoint = s[0];
            PF.Segments = PSC;
            PG.Figures.Add(PF);
            x.Data = PG;
            return x;
        }
        public static Path RectangleCreater(List<Point> r)
        {
            Path x = new Path();
            RectangleGeometry RG = new RectangleGeometry();
            RG.Rect = new Rect(r[0], r[1]);
            x.Data = RG;
            return x;
        }
        public static Path EllipseCreater(List<Point> e)
        {
            Path x = new Path();
            EllipseGeometry EG = new EllipseGeometry();
            EG.RadiusX = (e[1].X - e[0].X) / 2;
            EG.RadiusY = (e[1].Y - e[0].Y) / 2;
            //Canvas.SetLeft(x, e[0].X+EG.RadiusX);
            //Canvas.SetTop(x, e[0].Y+EG.RadiusY);
            double x1 = e[0].X + EG.RadiusX;
            double y1 = e[0].Y + EG.RadiusY;
            EG.Center = new Point(x1, y1);
            x.Data = EG;
            return x;
        }
    }

    public static class CloneUIElement
    {
        public static UIElement cloneElement(UIElement orig)
        {
            if (orig == null)

                return (null);

            string s = XamlWriter.Save(orig);

            System.IO.StringReader stringReader = new System.IO.StringReader(s);

            XmlReader xmlReader = XmlTextReader.Create(stringReader, new XmlReaderSettings());

            return (UIElement)XamlReader.Load(xmlReader);
        }
    }
}

