using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public class Light : OWNComponent
    {
        public bool dimmable = false;
        public bool IsOn
        {
            get
            {
                return this.Status > 0;
            }
        }

        public Light(int id, string name, bool dimmable = false, int status = 0)
        {
            this.id         = id;
            this.type       = 1;
            this.name       = name;
            this.dimmable   = dimmable;
            this.Status     = status;
        }

        public override int Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
                OnPropertyChanged();
                OnPropertyChanged("IsOn");
                Debug.WriteLine("===> Setting status for light " + id + " to " + value);
            }
        }


        public override string ToString()
        {
            Debug.WriteLine("Outputting " + this.name);
            return this.name;
        }
    }
}
