
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
        public Brush Fill = new SolidBrush(Color.FromArgb(255, Color.Black));

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

                Size = new Size((int)(14 * _scale), (int)(14 * _scale));
                Offset = new Point(-Size.Width / 2, (int)(-Size.Height / 1.4));
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

                    g.FillEllipse(Fill, new RectangleF(-Size.Width / 2, -Size.Height / 2, Size.Width, Size.Height));
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

            if (Fill != null)
            {
                Fill.Dispose();
                Fill = null;
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
