using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Net.NetworkInformation;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using WpfApp.Markers;

namespace WpfApp
{
    public partial class MainWindow : Window
    {
        GMapMarker currentMarker;

        #region Ctor
        public MainWindow()
        {
            InitializeComponent();

            // set cache mode if no internet available
            if (!Stuff.PingNetwork("pingtest.com"))
            {
                MainMap.Manager.Mode = AccessMode.CacheOnly;
                MessageBox.Show("No internet connection available, going to CacheOnly mode.", "GMap.NET - Demo.WindowsPresentation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // config map
            MainMap.MapProvider = GMapProviders.OpenStreetMap;
            MainMap.Position = new PointLatLng(40.75, -74);

            // map events
            MainMap.MouseMove += new MouseEventHandler(MainMap_MouseMove);
            MainMap.MouseLeftButtonDown += new MouseButtonEventHandler(MainMap_MouseLeftButtonDown);
            MainMap.MouseEnter += new MouseEventHandler(MainMap_MouseEnter);

            // set current marker
            currentMarker = new GMapMarker(MainMap.Position);
            {
                currentMarker.Shape = new CustomMarkerRed(this, currentMarker, "current position marker");
                currentMarker.Offset = new Point(-15, -15);
                currentMarker.ZIndex = int.MaxValue;
                MainMap.Markers.Add(currentMarker);
            }

            // add first location for demo
            string msg = "Welcome to NY";
            PointLatLng city = new PointLatLng(40.713, -74);
            GMapMarker ny = new GMapMarker(city);
            {
                ny.ZIndex = 0;
                ny.Shape = new CustomMarkerDemo(this, ny, msg);
            }
            MainMap.Markers.Add(ny);

            if (MainMap.Markers.Count > 1)
            {
                MainMap.ZoomAndCenterMarkers(null);
            }
        }
        #endregion

        #region Marker
        // add marker
        private void addMarker_Click(object sender, RoutedEventArgs e)
        {
            GMapMarker marker = new GMapMarker(currentMarker.Position);
            {
                Placemark? p = null;
                if (checkBoxPlace.IsChecked.Value)
                {
                    GeoCoderStatusCode status;
                    var plret = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out status);
                    if (status == GeoCoderStatusCode.G_GEO_SUCCESS && plret != null)
                    {
                        p = plret;
                    }
                }

                string ToolTipText;
                if (p != null)
                {
                    ToolTipText = p.Value.Address;
                }
                else
                {
                    ToolTipText = currentMarker.Position.ToString();
                }

                marker.Shape = new CustomMarkerDemo(this, marker, ToolTipText);
                marker.ZIndex = combobox.SelectedIndex;
            }
            MainMap.Markers.Add(marker);
        }

        private void clearAll_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Markers.Clear();
            MainMap.Markers.Add(currentMarker);
        }

        // zoom to fit & center markers
        private void zoomCenter_Click(object sender, RoutedEventArgs e)
        {
            MainMap.ZoomAndCenterMarkers(null);
        }
        #endregion

        #region Overlay
        // change overlays
        private void combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            ComboBox combobox = sender as ComboBox;
            if (combobox != null && MainMap != null)
            {
                if (combobox.SelectedIndex == combobox.Items.Count - 1)
                {
                    foreach(GMapMarker marker in MainMap.Markers)
                        marker.Shape.Visibility = Visibility.Visible;
                }
                else
                {
                    foreach (GMapMarker marker in MainMap.Markers)
                    {
                        if(marker.ZIndex == combobox.SelectedIndex)
                            marker.Shape.Visibility = Visibility.Visible;
                        else
                            marker.Shape.Visibility = Visibility.Collapsed;
                    }
                }
                currentMarker.Shape.Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Mouse
        void MainMap_MouseEnter(object sender, MouseEventArgs e)
        {
            MainMap.Focus();
        }

        void MainMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(MainMap);
            currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
        }

        // move current marker with left holding
        void MainMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(MainMap);
                currentMarker.Position = MainMap.FromLocalToLatLng((int)p.X, (int)p.Y);
            }
        }
        #endregion

        #region Stuff
        public class Stuff
        {
            #if !PocketPC
            public static bool PingNetwork(string hostNameOrAddress)
            {
                bool pingStatus = false;

                using (Ping p = new Ping())
                {
                    byte[] buffer = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
                    int timeout = 4444; // 4s

                    try
                    {
                        PingReply reply = p.Send(hostNameOrAddress, timeout, buffer);
                        pingStatus = (reply.Status == IPStatus.Success);
                    }
                    catch (Exception)
                    {
                        pingStatus = false;
                    }
                }

                return pingStatus;
            }
            #endif
        }
        #endregion
    }
}



