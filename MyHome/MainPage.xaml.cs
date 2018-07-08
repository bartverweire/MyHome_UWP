using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyHome
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool _pageLoaded = false;

        public MyHomeConfig myHome { get; }
        public string debug { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            myHome = new MyHomeConfig();
            myHome.InitializeAsync();
        }

        private void LightStatus_Toggled(object sender, RoutedEventArgs e)
        {
            if (_pageLoaded)
            {
                ToggleSwitch lightSwitch = sender as ToggleSwitch;
                Light light = lightSwitch.DataContext as Light;

                Debug.WriteLine("Light " + light.name + " toggled");
                Debug.WriteLine("Light Status " + light.IsOn);
                Debug.WriteLine("Light Switch Status " + lightSwitch.IsOn);

                // check if lightSwitch status differs from light status (using xor operator)
                if (lightSwitch.IsOn ^ light.IsOn)
                {
                    Debug.WriteLine("Toggled by user, sending command");
                    myHome.setStatus(light, lightSwitch.IsOn ? 1 : 0);
                }
                else
                {
                    Debug.WriteLine("Toggled by background, not sending any command");
                }
            }
            else
            {
                debug = "Still Initializing";
            }
            
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = true;
        }

        private void ShutterDown_Click(object sender, RoutedEventArgs e)
        {
            Button shutterDownButton = sender as Button;
            Shutter shutter = shutterDownButton.DataContext as Shutter;

            Debug.WriteLine("Bringing shutter " + shutter.name + " down");
            myHome.setStatus(shutter, 2);
        }

        private void ShutterStop_Click(object sender, RoutedEventArgs e)
        {
            Button shutterDownButton = sender as Button;
            Shutter shutter = shutterDownButton.DataContext as Shutter;

            Debug.WriteLine("Stopping shutter " + shutter.name);
            myHome.setStatus(shutter, 0);
        }

        private void ShutterUp_Click(object sender, RoutedEventArgs e)
        {
            Button shutterDownButton = sender as Button;
            Shutter shutter = shutterDownButton.DataContext as Shutter;

            Debug.WriteLine("Bringing shutter " + shutter.name + " up");
            myHome.setStatus(shutter, 1);
        }
    }
}
