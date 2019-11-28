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
using Booyco_HMI_Utility;


namespace Booyco_HMI_Utility.CustomMarkers
{
   /// <summary>
   /// Interaction logic for CustomMarkerDemo.xaml
   /// </summary>
   public partial class CustomMarkerVehicle
    {
      Popup Popup;
      Label Label;
      GMapMarker Marker;
      MainWindow MainWindow;
        GMapControl mapControl;
        bool IsBrakeZoneVisible = false;
        public MarkerEntry CurrentMarker;

        public CustomMarkerVehicle(GMapControl _mapControl, MarkerEntry marker)
      {
        this.InitializeComponent();
        this.mapControl = _mapControl;
        this.Marker = marker.MapMarker;

            CurrentMarker = new MarkerEntry();

            CurrentMarker.VehicleInfo.Heading = marker.VehicleInfo.Heading;
            CurrentMarker.Scale = marker.Scale;
            CurrentMarker.VehicleInfo.PresenceZoneSize = marker.VehicleInfo.PresenceZoneSize;
            CurrentMarker.VehicleInfo.WarningZoneSize = marker.VehicleInfo.WarningZoneSize;
            CurrentMarker.VehicleInfo.CriticalZoneSize = marker.VehicleInfo.CriticalZoneSize;
            CurrentMarker.VehicleInfo.Zone = marker.VehicleInfo.Zone;
            CurrentMarker.title = marker.title;
            CurrentMarker.titleSpeed = marker.titleSpeed;
            CurrentMarker.VehicleInfo.Accuracy = marker.VehicleInfo.Accuracy;
            CurrentMarker.VehicleInfo.BrakeDistance = marker.VehicleInfo.BrakeDistance;

            RotateTransform IndicatorTransfrom = new RotateTransform(CurrentMarker.VehicleInfo.Heading);
            PathVehicle.LayoutTransform = IndicatorTransfrom;        
            Rectangle_ProhibitZone.LayoutTransform = IndicatorTransfrom;
            Rectangle_Vehicle.LayoutTransform = IndicatorTransfrom;

            if (CurrentMarker.VehicleInfo.Zone == 1)
            {
                PathVehicle.Fill = Brushes.Green;

            }
            else if (CurrentMarker.VehicleInfo.Zone == 2)
            {
                PathVehicle.Fill = Brushes.Yellow;
            }
            else if (CurrentMarker.VehicleInfo.Zone == 3)
            {
                PathVehicle.Fill = Brushes.Red;
            }
            else if (CurrentMarker.VehicleInfo.Zone == 10)
            {
                PathVehicle.Fill = Brushes.Black;
            }
            else if (CurrentMarker.VehicleInfo.Zone == 20)
            {
                //PathVehicle.Fill = Brushes.Green;
            }
            else
            {
                PathVehicle.Fill = Brushes.White;
            }
              
            

            //CustomMarkerAngle = (Double) (Heading + 180);

        this.SizeChanged += new SizeChangedEventHandler(CustomMarkerIndicator_SizeChanged);
            //this.MouseEnter += new MouseEventHandler(CustomMarkerIndicator_MouseEnter);
            //this.MouseLeave += new MouseEventHandler(CustomMarkerIndicator_MouseLeave);

            //Popup.Placement = PlacementMode.Mouse;
            //{
            //    Label.Background = Brushes.Gray;
            //    Label.Foreground = Brushes.White;
            //    Label.BorderBrush = Brushes.Black;
            //    Label.BorderThickness = new Thickness(2);
            //    Label.Padding = new Thickness(5);
            //    Label.FontSize = 12;
            //    Label.Content = marker.title;
            //}
            UpdateSizes();

      }

        double VehicleWidth = 2;
        double VehicleHeight = 4;
        double ProhibitWidth = 1;
        double ProhibitHeight = 3;
        //double VehicleWidth = 3;
        //double VehicleHeight = 4;
        //double ProhibitWidth = 8;
        double WidthFactor = 0.5;
        //double ProhibitHeight = 10;
        public void UpdateSizes()
        {
            Rectangle_ProhibitZone.Width = (ProhibitWidth+ VehicleWidth/2+CurrentMarker.VehicleInfo.Accuracy) / CurrentMarker.Scale;
            Rectangle_ProhibitZone.Height = (ProhibitHeight+VehicleHeight/2) / CurrentMarker.Scale;

            Rectangle_Vehicle.Width = (VehicleWidth / 2) / CurrentMarker.Scale;
            Rectangle_Vehicle.Height = (VehicleHeight / 2) / CurrentMarker.Scale;
            if (CurrentMarker.VehicleInfo.Zone == 10)
            {
                Rectangle_CriticalZone.Height = (CurrentMarker.VehicleInfo.BrakeDistance / 2+ ProhibitWidth + VehicleWidth / 2) / CurrentMarker.Scale;
                Rectangle_CriticalZone.Width = (ProhibitWidth + VehicleWidth / 2 + CurrentMarker.VehicleInfo.Accuracy ) / CurrentMarker.Scale;
                Rectangle_PresenceZone.Width = 0;
                Rectangle_PresenceZone.Height = 0;

                Rectangle_WarningZone.Height = 0;
                Rectangle_WarningZone.Width = 0;
            }
            else
            {

            Rectangle_PresenceZone.Width = (ProhibitWidth + VehicleWidth / 2 + CurrentMarker.VehicleInfo.Accuracy + WidthFactor) / CurrentMarker.Scale;
            Rectangle_PresenceZone.Height = (CurrentMarker.VehicleInfo.PresenceZoneSize / 2) / CurrentMarker.Scale;

            Rectangle_WarningZone.Height = (CurrentMarker.VehicleInfo.WarningZoneSize / 2) / CurrentMarker.Scale;
            Rectangle_WarningZone.Width = (ProhibitWidth + VehicleWidth / 2 + CurrentMarker.VehicleInfo.Accuracy + WidthFactor) / CurrentMarker.Scale;
            Rectangle_CriticalZone.Height = (CurrentMarker.VehicleInfo.CriticalZoneSize / 2) / CurrentMarker.Scale;
            Rectangle_CriticalZone.Width = (ProhibitWidth + VehicleWidth / 2 + CurrentMarker.VehicleInfo.Accuracy+ WidthFactor) / CurrentMarker.Scale;
            }
            Label_PopupUID.Content = CurrentMarker.title;
            Label_PopupSpeed.Content = CurrentMarker.titleSpeed;

            RectangleTransform_Crtical.Y = -Rectangle_CriticalZone.Height / 2;
            RectangleTransform_Warning.Y = -Rectangle_WarningZone.Height / 2;
            RectangleTransform_Presence.Y = -Rectangle_PresenceZone.Height / 2;
            //Rectangle_PresenceZone.LayoutTransform = IndicatorTransfrom;
            //Rectangle_WarningZone.LayoutTransform = IndicatorTransfrom;
            //Rectangle_CriticalZone.LayoutTransform = IndicatorTransfrom;
            Rectangle_RotateTransform_Warning.Angle = CurrentMarker.VehicleInfo.Heading;
            Rectangle_RotateTransform_Presence.Angle = CurrentMarker.VehicleInfo.Heading;
            Rectangle_RotateTransform_Critical.Angle = CurrentMarker.VehicleInfo.Heading;
            Ellipse_PresenceZone.Width = CurrentMarker.VehicleInfo.PresenceZoneSize / CurrentMarker.Scale;
            Ellipse_PresenceZone.Height = CurrentMarker.VehicleInfo.PresenceZoneSize / CurrentMarker.Scale;

            Ellipse_WarningZone.Width = CurrentMarker.VehicleInfo.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZone.Height = CurrentMarker.VehicleInfo.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZoneBackground.Width = CurrentMarker.VehicleInfo.WarningZoneSize / CurrentMarker.Scale;
            Ellipse_WarningZoneBackground.Height = CurrentMarker.VehicleInfo.WarningZoneSize / CurrentMarker.Scale;

            Ellipse_CriticalZone.Width = CurrentMarker.VehicleInfo.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZone.Height = CurrentMarker.VehicleInfo.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZoneBackground.Width = CurrentMarker.VehicleInfo.CriticalZoneSize / CurrentMarker.Scale;
            Ellipse_CriticalZoneBackground.Height = CurrentMarker.VehicleInfo.CriticalZoneSize / CurrentMarker.Scale;
        }
        private Double _CustomMarkerAngle;
        public Double CustomMarkerAngle
        {
            get { return _CustomMarkerAngle; }
            set
            {
                _CustomMarkerAngle = value;
                OnPropertyChanged("CustomMarkerAngle");
            }
        }

      void CustomMarkerIndicator_SizeChanged(object sender, SizeChangedEventArgs e)
      {
         Marker.Offset = new Point(-e.NewSize.Width/2, -e.NewSize.Height/2);
      }

      void CustomMarkerIndicator_MouseMove(object sender, MouseEventArgs e)
      {
         if(e.LeftButton == MouseButtonState.Pressed && IsMouseCaptured)
         {
            Point p = e.GetPosition(mapControl);
            Marker.Position = mapControl.FromLocalToLatLng((int) p.X, (int) p.Y);
         }
      }

      //void CustomMarkerIndicator_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
      //{
      //      //if(!IsMouseCaptured)
      //      //{
      //      //   Mouse.Capture(this);
      //      //}
      //      if (!Label_PopupInfo.IsVisible)
      //      {
      //          Label_PopupInfo.Visibility = Visibility.Visible;
      //      }
      //      else
      //      {
      //          Label_PopupInfo.Visibility = Visibility.Collapsed;
               
      //      }

      //}

      //void CustomMarkerIndicator_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
      //{
      //      //if(IsMouseCaptured)
      //      //{
      //      //   Mouse.Capture(null);
      //      //}
      //    //  Popup.IsOpen = false;

      //  }

      //  void CustomMarkerIndicator_MouseLeave(object sender, MouseEventArgs e)
      //{
      //      PathIndicator.Opacity = 1;
      //  // Marker.ZIndex -= 10000;
      
      //      Ellipse_CriticalZone.Visibility = Visibility.Collapsed;
      //      Ellipse_WarningZoneBackground.Visibility = Visibility.Collapsed;
      //      Ellipse_WarningZone.Visibility = Visibility.Collapsed;
      //      Ellipse_CriticalZoneBackground.Visibility = Visibility.Collapsed;
      //      Ellipse_PresenceZone.Visibility = Visibility.Collapsed;
      //      if (!IsBrakeZoneVisible)
      //      {
      //          Rectangle_CriticalZone.Visibility = Visibility.Collapsed;
      //          Rectangle_WarningZone.Visibility = Visibility.Collapsed;
      //          Rectangle_PresenceZone.Visibility = Visibility.Collapsed;
      //      }
    

      //  }

      //void CustomMarkerIndicator_MouseEnter(object sender, MouseEventArgs e)
      //{
      //      PathIndicator.Opacity = 0.5;
      //      //Marker.ZIndex += 10000;
        
      //      Ellipse_CriticalZone.Visibility = Visibility.Visible;
      //      Ellipse_WarningZoneBackground.Visibility = Visibility.Visible;
      //      Ellipse_WarningZone.Visibility = Visibility.Visible;
      //      Ellipse_CriticalZoneBackground.Visibility = Visibility.Visible;
      //      Ellipse_PresenceZone.Visibility = Visibility.Visible;
        
      //      Rectangle_CriticalZone.Visibility = Visibility.Visible;
      //      Rectangle_WarningZone.Visibility = Visibility.Visible;
      //      Rectangle_PresenceZone.Visibility = Visibility.Visible;

      //  }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

   

        private void PathIndicator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsBrakeZoneVisible)
            {
                IsBrakeZoneVisible = true;
            }
            else
            {
                IsBrakeZoneVisible = false;
            }
               
        }
    }
}