using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome
{
    public interface IOWNEventListener
    {
        void handleEvent(string message);
    }
}
