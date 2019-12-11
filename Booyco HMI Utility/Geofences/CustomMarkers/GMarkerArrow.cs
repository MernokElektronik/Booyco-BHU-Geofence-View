﻿
namespace Booyco_HMI_Utility.Geofences
{
    using System;
    using System.Drawing;
    using System.Runtime.Serialization;
    using GMap.NET;
    using GMap.NET.WindowsForms;
    using GMap.NET.WindowsForms.Markers;

    [Serializable]
   public class GMarkerArrow : GMapMarker, ISerializable
   {
      [NonSerialized]
      public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

      //[NonSerialized]
      //public Pen Pen = new Pen(Brushes.Blue, 5);

      static readonly Point[] Arrow = new[] { new Point(-7, 7), new Point(0, -22), new Point(7, 7), new Point(0, 2) };

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

            Size = new Size((int)(40* _scale), (int)(40*_scale));
            Offset = new Point(-Size.Width / 2, (int)(-Size.Height / 2));
         }
      } 

      public GMarkerArrow(PointLatLng p)
         : base(p)
      {
         Scale = 1;
      }

        public static explicit operator GMarkerGoogle(GMarkerArrow v)
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
               
               g.FillPolygon(Fill, Arrow);               
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

         if(Fill != null)
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

      protected GMarkerArrow(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }

      #endregion
   }
}
