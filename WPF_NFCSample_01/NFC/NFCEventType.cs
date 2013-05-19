using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_NFCSample_01
{
    public enum NFCEventType
    {
        UNKNOWN = 0,
        NFC_NDEF_FOUND = 1,
        NFC_DEVICE_CHANGED = 2,
        NFC_UNKNOWN_SERVICE = 3,
        NFC_CONNECTED = 4,
        NFC_DISCONNECTED = 5,
        NFC_IDLE = 6,
    }
}
