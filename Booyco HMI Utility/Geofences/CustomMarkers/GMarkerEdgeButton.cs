
namespace Booyco_HMI_Utility.Geofences
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using GMap.NET;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;

    [Serializable]
    public class GMarkerEdgeButton : GMapMarker, ISerializable
    {
        [NonSerialized]
        public Brush FillBackground = new SolidBrush(Color.FromArgb(180, Color.Gray));
        public Brush FillForeground = new SolidBrush(Color.FromArgb(255, Color.White));

        //[NonSerialized]
        //public Pen Pen = new Pen(Brushes.Blue, 5);

        static readonly Point[] Plus = new[] { new Point(-7, 7), new Point(0, -22), new Point(7, 7), new Point(0, 2) };

        public float Bearing = 0;
        private float _scale = 1;

        public float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;

                Size = new Size((int)(35 * _scale), (int)(35 * _scale));
                Offset = new Point(-Size.Width / 2, (int)(-Size.Height / 2));
            }
        }

        public GMarkerEdgeButton(PointLatLng p) : base(p)
        {
            Scale = 1;
            ToolTipMode = MarkerTooltipMode.OnMouseOver;
            ToolTipText = "Click to add a vertex";
        }

        public static explicit operator GMarkerGoogle(GMarkerEdgeButton v)
        {
            throw new NotImplementedException();
        }

        public override void OnRender(Graphics g)
        {
            //g.DrawRectangle(Pen, new System.Drawing.Rectangle(LocalPosition.X, LocalPosition.Y, Size.Width, Size.Height));
            {
                g.TranslateTransform(ToolTipPosition.X, ToolTipPosition.Y);
                var c = g.BeginContainer();
                {
                    g.RotateTransform(Bearing - Overlay.Control.Bearing);
                    g.ScaleTransform(Scale, Scale);
                    // draw bg
                    g.FillEllipse(FillBackground, new RectangleF(-Size.Width / 2, -Size.Height / 2, Size.Width, Size.Height));
                    // draw plus
                    float smallEdge = 0.1f;
                    float bigEdge = 0.4f;
                    float widePart = 1 - (smallEdge * 2);
                    float thinPart = 1 - (bigEdge * 2);
                    g.FillRectangle(FillForeground, new RectangleF(
                        (-Size.Width / 2) + (Size.Width * smallEdge), 
                        (-Size.Height / 2) + (Size.Height * bigEdge), 
                        Size.Width * widePart, 
                        Size.Height * thinPart
                    )); // horizontal dash
                    g.FillRectangle(FillForeground, new RectangleF(
                        (-Size.Width / 2) + (Size.Width * bigEdge),
                        (-Size.Height / 2) + (Size.Height * smallEdge),
                        Size.Width * thinPart,
                        Size.Height * widePart
                    )); // vertical dash
                }
                g.EndContainer(c);
                g.TranslateTransform(-ToolTipPosition.X, -ToolTipPosition.Y);
            }
        }

        public override void Dispose()
        {
            //if(Pen != null)
            //{
            //   Pen.Dispose();
            //   Pen = null;
            //}

            if (FillBackground != null)
            {
                FillBackground.Dispose();
                FillBackground = null;
            }

            base.Dispose();
        }

        #region ISerializable Members

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        protected GMarkerEdgeButton(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

        #endregion
    }
}
