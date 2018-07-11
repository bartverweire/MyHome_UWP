using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public class OWNGroup<OWNComponent>
    {
        public string name { get; set; }
        public ObservableCollection<OWNComponent> components { get; set; }

        public OWNGroup(string name) 
        {
            this.name = name;
            components = new ObservableCollection<OWNComponent>();
        }

        public void Add(OWNComponent component)
        {
            components.Add(component);
        }
    }
}
