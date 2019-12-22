using System;
using System.Collections.Generic;
using System.Text;

namespace VPNControl
{
    interface StatusListener
    {
        void OnConnected();
        void OnDisconnected();
    }
}
