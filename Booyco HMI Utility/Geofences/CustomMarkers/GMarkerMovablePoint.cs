
namespace Booyco_HMI_Utility.Geofences
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using GMap.NET;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;

    [Serializable]
    public class GMarkerMovablePoint : GMapMarker, ISerializable
    {
        [NonSerialized]
        public Brush Fill = new SolidBrush(Color.FromArgb(180, Color.Black));

        public float Bearing = 0;
        private float _scale = 1;
        private bool selected = false;

        public float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;

                Size = new Size((int)(40 * _scale), (int)(40 * _scale));
                Offset = new Point(-Size.Width / 2, (int)(-Size.Height / 2));
            }
        }

        public GMarkerMovablePoint(PointLatLng p)
           : base(p)
        {
            Scale = 1;
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            if (this.selected)
            {
                this.Fill = new SolidBrush(Color.FromArgb(180, Color.Blue));
            }
            else
            {
                this.Fill = new SolidBrush(Color.FromArgb(180, Color.Black));
            }
        }

        public static explicit operator GMarkerGoogle(GMarkerMovablePoint v)
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

        protected GMarkerMovablePoint(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
        }

        #endregion
    }
}
