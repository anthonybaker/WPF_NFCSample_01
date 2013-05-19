using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF_NFCSample_01
{
    public class NFCMessageEventArgs : EventArgs
    {
        #region properties

        public string Data { get; internal set; }
        public NFCEventType EventType { get; internal set; }

        #endregion

        #region constructor

        public NFCMessageEventArgs(string data, NFCEventType eventType)
        {
            this.Data = data;
            this.EventType = eventType;
        }

        #endregion
    }
}
