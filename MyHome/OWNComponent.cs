using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public class OWNComponent : INotifyPropertyChanged
    {
        public int      id      { get; set; }
        public int      type    { get; set; }
        public string   name    { get; set; }
        public int      _status;

        public virtual int Status
        {
            get
            {
                return this._status;
            }
            set
            {
                this._status = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        protected OWNComponent()
        {
            
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string getCommand(int status)
        {
            return "*" + type + "*" + status + "*" + id + "##";
        }

        public string getStatusCommand()
        {
            return "*#" + type + "*" + id + "##";
        }
    }
}
