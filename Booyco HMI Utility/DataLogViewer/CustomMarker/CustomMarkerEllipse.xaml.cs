using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GMap.NET.WindowsPresentation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;


namespace Booyco_HMI_Utility.CustomMarkers
{
    /// <summary>
    /// Interaction logic for Cross.xaml
    /// </summary>
    public partial class CustomMarkerEllipse
    {
        
        GMapMarker Marker;    
        GMapControl mapControl;

       public CustomMarkerEllipse(GMapControl _mapControl, MarkerEntry marker)
        {
                this.InitializeComponent();
                this.mapControl = _mapControl;
                this.Marker = marker.MapMarker;

              
      
            this.SizeChanged += new SizeChangedEventHandler(MarkerEllipseControl_SizeChanged);
            this.MouseEnter += new MouseEventHandler(MarkerellipseControl_MouseEnter);
            this.MouseLeave += new MouseEventHandler(MarkerEllipseControl_MouseLeave);

            //ScaleTransform EllipseTransfrom = new ScaleTransform(14,14);
            //EllipseMarker.RenderTransform = EllipseTransfrom;
            Label_PopupInfo.Content = marker.title;
            EllipseMarker.Width = marker.VehicleInfo.Width/marker.Scale;
            EllipseMarker.Height = marker.VehicleInfo.Length/marker.Scale;

            //Popup.Placement = PlacementMode.Mouse;
            //{
            //    Label.Background = Brushes.Gray;
            //    Label.Foreground = Brushes.White;
            //    Label.BorderBrush = Brushes.Black;
            //    Label.BorderThickness = new Thickness(2);
            //    Label.Padding = new Thickness(5);
            //    Label.FontFamily = new FontFamily("Segoe UI");
            //    Label.FontSize = 10;
            //    Label.Content = marker.title;
            //}
        
            //Popup.Child = Label;
        }

        void MarkerEllipseControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Marker.Offset = new Point(-e.NewSize.Width / 2, -e.NewSize.Height / 2);
        }

        void MarkerEllipseControl_MouseLeave(object sender, MouseEventArgs e)
        {
            //  Marker.ZIndex -= 10000;
            EllipseMarker.Opacity = 1;
           
        }

        void MarkerellipseControl_MouseEnter(object sender, MouseEventArgs e)
        {
            EllipseMarker.Opacity = 0.5;
            //  Marker.ZIndex += 10000;
        
        }

        void EllipseMarker_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
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

        private void EllipseMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
           
        }
              
    }
}
