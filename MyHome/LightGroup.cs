using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public class LightGroup : OWNGroup<Light>
    {
        public bool IsOn
        {
            get
            {
                bool on = false;

                foreach (Light light in components)
                {
                    on = on || light.IsOn;
                }

                return on;
            }
        }

        public LightGroup(string name) : base(name)
        {
        }
    }
}
