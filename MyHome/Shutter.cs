using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public class Shutter : OWNComponent
    {
        public bool IsStopped
        {
            get
            {
                return this.Status == 0;
            }
        }

        public bool IsDown
        {
            get
            {
                return this.Status == 2;
            }
        }

        public bool IsUp
        {
            get
            {
                return this.Status == 1;
            }
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
                OnPropertyChanged("IsDown");
                OnPropertyChanged("IsUp");
                OnPropertyChanged("IsStopped");
                Debug.WriteLine("===> Setting status for shutter " + id + " to " + value);
            }
        }

        public Shutter(int id, string name)
        {
            this.id     = id;
            this.type   = 2;
            this.name   = name;
            this.Status = 0;
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
