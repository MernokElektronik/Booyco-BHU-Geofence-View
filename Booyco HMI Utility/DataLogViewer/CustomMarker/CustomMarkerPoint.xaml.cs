using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System;
using System.ComponentModel;


namespace Booyco_HMI_Utility.CustomMarkers
{
    /// <summary>
    /// Interaction logic for CustomMarkerDemo.xaml
    /// </summary>
    public partial class CustomMarkerPoint
   {
    
        GMapMarker Marker;
    
        GMapControl mapControl;

            public CustomMarkerPoint(GMapControl _mapControl, MarkerEntry marker)
            {
                this.InitializeComponent();
                this.mapControl = _mapControl;
                this.Marker = marker.MapMarker;
         

            if (marker.VehicleInfo.Zone == 1)
            {
                PointOfIntersection.Fill = Brushes.Blue;
            }
            else if (marker.VehicleInfo.Zone == 2)
            {
                PointOfIntersection.Fill = Brushes.Yellow;
            }
            else if (marker.VehicleInfo.Zone == 3)
            {
                PointOfIntersection.Fill = Brushes.Red;
            }
            else
            {
                PointOfIntersection.Fill = Brushes.Transparent;
            }
            Label_PopupInfo.Content = marker.title;
            this.SizeChanged += new SizeChangedEventHandler(CustomMarkerPoint_SizeChanged);
            this.MouseEnter += new MouseEventHandler(CustomMarkerPoint_MouseEnter);
            this.MouseLeave += new MouseEventHandler(CustomMarkerPoint_MouseLeave);
         
        }

        void CustomMarkerPoint_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void CustomMarkerPointr_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
            {
                Point p = e.GetPosition(mapControl);
                Marker.Position = mapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }

        void CustomMarkerPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsMouseCaptured)
            {
                Mouse.Capture(this);
            }
        }

        void CustomMarkerPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseCaptured)
            {
                Mouse.Capture(null);
            }
        }

        void CustomMarkerPoint_MouseLeave(object sender, MouseEventArgs e)
        {
         //   Marker.ZIndex -= 10000;
 
        }

        void CustomMarkerPoint_MouseEnter(object sender, MouseEventArgs e)
        {
           // Marker.ZIndex += 10000;
         
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        private void PointOfIntersection_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Label_PopupInfo.IsVisible)
            {
                Label_PopupInfo.Visibility = Visibility.Visible;
            }
            else
            {
                Label_PopupInfo.Visibility = Visibility.Collapsed;

            }
        }
    }
}