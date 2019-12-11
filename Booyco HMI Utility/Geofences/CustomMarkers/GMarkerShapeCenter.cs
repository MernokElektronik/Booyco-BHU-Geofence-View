
namespace Booyco_HMI_Utility.Geofences
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using GMap.NET;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;

    [Serializable]
    public class GMarkerShapeCenter : GMapMarker, ISerializable
    {
        [NonSerialized]
        public Brush FillBackground = new SolidBrush(Color.FromArgb(255, Color.Green));
        public Brush FillForeground = new SolidBrush(Color.FromArgb(255, Color.White));

        public float Bearing = 0;
        private float _scale = 1;
        private bool selected = false;

        static readonly Point[] Arrow = new[] { new Point(-7, 7), new Point(0, -22), new Point(7, 7), new Point(0, 2) };

        public float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;

                Size = new Size((int)(55 * _scale), (int)(55 * _scale));
                Offset = new Point(-Size.Width / 2, (int)(-Size.Height / 2));
            }
        }

        public GMarkerShapeCenter(PointLatLng p)
           : base(p)
        {
            Scale = 1;
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            if (this.selected)
            {
                this.FillBackground = new SolidBrush(Color.Blue);
            }
            else
            {
                this.FillBackground = new SolidBrush(Color.Green);
            }

        }

        public static explicit operator GMarkerGoogle(GMarkerShapeCenter v)
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

                    g.FillEllipse(FillBackground, new RectangleF(-Size.Width / 2, -Size.Height / 2, Size.Width, Size.Height));
                    g.FillPolygon(FillForeground, Arrow);
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

        protected GMarkerShapeCenter(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

        internal void SetBearing(int bearing)
        {
            this.Bearing = bearing;
        }

        #endregion
    }
}
