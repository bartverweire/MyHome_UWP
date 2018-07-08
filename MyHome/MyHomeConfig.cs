using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Json;
using Windows.Storage;

namespace MyHome
{
    public class MyHomeConfig : INotifyPropertyChanged, IOWNEventListener
    {
        private String configPath = "config.json";

        private OWNMonitor _monitor;
        private OWNCommand _command;
        public OWNComponent[] components = new OWNComponent[100];

        public event PropertyChangedEventHandler PropertyChanged;

        public LightGroup lights { get; set; } = new LightGroup();
        public ShutterGroup shutters { get; set; } = new ShutterGroup();

        public MyHomeConfig()
        {
            _monitor = new OWNMonitor("192.168.0.103", 20000, this);
            _command = new OWNCommand("192.168.0.103", 20000, this);
            _monitor.start();
        }

        public async Task InitializeAsync()
        {
            await LoadConfig();
            getStatus(null);
        }

        public async Task LoadConfig()
        {
            string path = Path.Combine(Package.Current.InstalledLocation.Path, configPath);
            StorageFile configFile = await StorageFile.GetFileFromPathAsync(path);

            string config = await FileIO.ReadTextAsync(configFile);

            JsonObject jsonConfig = JsonObject.Parse(config);
            
            foreach (JsonValue jsonLight in jsonConfig.GetNamedArray("lights"))
            {
                int id = (int)jsonLight.GetObject().GetNamedNumber("id");
                string name = jsonLight.GetObject().GetNamedString("name");
                bool dimmable = jsonLight.GetObject().GetNamedBoolean("dimmable");

                Light light = new Light(id, name, dimmable);
                lights.Add(light);
                components[light.id] = light;
            }

            foreach (JsonValue jsonShutter in jsonConfig.GetNamedArray("shutters"))
            {
                int id = (int)jsonShutter.GetObject().GetNamedNumber("id");
                string name = jsonShutter.GetObject().GetNamedString("name");

                Shutter shutter = new Shutter(id, name);
                shutters.Add(shutter);
                components[shutter.id] = shutter;
            }

        }

        public async void setStatus(OWNComponent component, int status)
        {
            await _command.Send(new string[] { component.getCommand(status) });
        }

        public async void getStatus(OWNComponent component)
        {
            string[] statusMessages;

            if (component != null)
            {
                statusMessages = new string[] { component.getStatusCommand() };
            }
            else
            {
                statusMessages = new string[lights.components.Count + shutters.components.Count];

                for(int i = 0, j = 0; i < components.Length; i++)
                {
                    OWNComponent item = components[i];
                    if (item != null)
                    {
                        statusMessages[j++] = item.getStatusCommand();
                    }
                }
            }

            await _command.Send(statusMessages);
        }

        public void randomStatus()
        {
            foreach(Light light in lights.components)
            {
                light.Status = (light.Status + 1) % 2;
            }

            NotifyPropertyChanged();
        }

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        void IOWNEventListener.handleEvent(string message)
        {
            string[] msgSeparator = new string[] { "##" };
            string msgPattern = @"^\*(\d+)\*(\d+)\*(\d+)$";
            
            string[] messages = message.Split(msgSeparator, StringSplitOptions.None);
            foreach (string singleMessage in messages)
            {
                //Debug.WriteLine("Single message" + singleMessage);
                MatchCollection matches = Regex.Matches(singleMessage, msgPattern);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count == 4)
                    {
                        int who      = int.Parse(match.Groups[1].Value);
                        int what     = int.Parse(match.Groups[2].Value);
                        int where    = int.Parse(match.Groups[3].Value);

                        try
                        {
                            OWNComponent component = components[where];
                            if (component != null)
                            {
                                Debug.WriteLine("===> Received event for existing component " + where);
                                component.Status = what;
                            }
                        }
                        catch(Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                        }
                        
                    }
                }
            }
        }
    }


}
