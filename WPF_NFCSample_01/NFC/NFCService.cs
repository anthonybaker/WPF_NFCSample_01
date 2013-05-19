using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Xml.Linq;

namespace WPF_NFCSample_01
{
    public class NFCService
    {
        #region fields

        TNFCWrapper NFCWrapper;

        #endregion

        #region imported functions

        [System.Runtime.InteropServices.DllImport("kernel32.dll", EntryPoint = "LoadLibraryA")]
        static extern int LoadLibrary(string lpLibFileName);

        #endregion

        #region events

        public delegate void NFCMessageHandler(object sender, NFCMessageEventArgs args);
        public event NFCMessageHandler MessageReceived;

        #endregion

        #region constructor

        public NFCService()
        {
        }

        #endregion

        #region event handlers

        /// <summary>
        /// Event handler to manage the NFC event messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnMessageReceived(object sender, NFCMessageEventArgs args)
        {
            if (MessageReceived != null)
            {
                MessageReceived(this, args);
            }
        }

        #endregion

        #region methods

        /// <summary>
        /// Initializes the NFC Service. Loads the SCM_NFC.dll library, 
        /// initializes the wrapper and starts listening to NFC event messages.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public bool Initialize(IntPtr handle)
        {
            // Verify that the NFC Wrapper is present
            if (LoadLibrary("SCM_NFC.dll") == 0)
            {
                Debug.WriteLine("NFC Wrapper not found");
                Debug.WriteLine("Be sure that the file SCM_NFC.dll is present in the search path.");
                return false;
            }

            Debug.WriteLine("SCM_NFC.DLL successfully loaded.");

            // sets the source and handle for the Win32 process
            HwndSource source = HwndSource.FromHwnd(handle);
            source.AddHook(new HwndSourceHook(WndProc));

            //Init NFC Wrapper
            NFCWrapper = new TNFCWrapper();
            TNFCWrapper.Initialize((UInt32)handle.ToInt32());

            //Tell the NFC Wrapper to start listening for NFC devices or tags
            TNFCWrapper.StartListening();
            Debug.WriteLine("Please put an NFC Tag on the device");

            return true;
        }

        /// <summary>
        /// Handles the Win32 API messages and parses the TNFCWrapper messages.
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Handle messages Win32 process messages
            Debug.WriteLine("msg: " + msg + " wParam: " + wParam + " lParam: " + lParam);
            
            if (msg == TNFCWrapper.WM_NFC_NOTIFY)
            {
                string s = "";
                string payload = "";
                Int32 param = wParam.ToInt32();

                switch (param)
                {
                    case TNFCWrapper.NFC_NDEF_FOUND:
                        s = "NFC_NDEF_FOUND  Size = " + lParam.ToString();
                        payload = ReadNDEF();
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_NDEF_FOUND));
                        break;
                    case TNFCWrapper.NFC_DEVICE_CHANGED:
                        s = "NFC_DEVICE_CHANGED";
                        //payload = ReadNDEF();
                        payload = "No NFC Message Data found";
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_DEVICE_CHANGED));
                        break;
                    case TNFCWrapper.NFC_UNKNOWN_SERVICE:
                        s = "NFC_UNKNOWN_SERVICE";
                        //payload = ReadNDEF();
                        payload = "No NFC Message Data found";
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_UNKNOWN_SERVICE));
                        break;
                    case TNFCWrapper.NFC_CONNECTED:
                        s = "NFC_CONNECTED";
                        //payload = ReadNDEF();
                        payload = "NFC Tag Detected";
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_CONNECTED));
                        break;
                    case TNFCWrapper.NFC_DISCONNECTED:
                        s = "NFC_DISCONNECTED";
                        //payload = ReadNDEF();
                        payload = "No NFC Tag found";
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_DISCONNECTED));
                        break;
                    case TNFCWrapper.NFC_IDLE:
                        s = "NFC_IDLE";
                        //payload = ReadNDEF();
                        payload = "NFC Idle";
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.NFC_IDLE));
                        break;
                    default:
                        s = "UNKNOWN";
                        //payload = ReadNDEF();
                        OnMessageReceived(this, new NFCMessageEventArgs(payload, NFCEventType.UNKNOWN));
                        break;
                }
                Debug.WriteLine(s);
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Reads the NFC message data
        /// </summary>
        /// <returns></returns>
        private string ReadNDEF()
        {
            UInt32 DeviceCount = 0;
            UInt32 MessageCount = 0;
            UInt32 NextMessageSize = 0;
            UInt32 Result;

            // check to make sure the NCFWrappr is valid.
            if (NFCWrapper == null) 
                return null;

            // Get information about the message queue
            Result = TNFCWrapper.GetNDEFQueueInfo(ref DeviceCount, ref MessageCount, ref NextMessageSize);
            Debug.WriteLine("GetNDEFQueueInfo: " + NFCWrapper.NFCWrapperErrorToString(Result));

            // Quit if the queue's information could not be retrieved
            if (Result != TNFCWrapper.ERR_SUCCESS) 
                return null;

            Debug.WriteLine("  DeviceCount     = " + DeviceCount.ToString());
            Debug.WriteLine("  MessageCount    = " + MessageCount.ToString());
            Debug.WriteLine("  NextMessageSize = " + NextMessageSize.ToString());

            //Resize the NDEF buffer accordingly to the site of the next message in the queue
            byte[] NDEF = new byte[NextMessageSize];
            UInt32 NDEFSize = NextMessageSize;
            TNFCAddress NFCAddress = new TNFCAddress();
            TMessageInfo MessageInfo = new TMessageInfo();

            //Read the NDEF message from the message queue
            Result = TNFCWrapper.ReadNDEF(ref NFCAddress, ref MessageInfo, ref NDEF[0], ref NDEFSize);
            Debug.WriteLine("ReadNDEF: " + NFCWrapper.NFCWrapperErrorToString(Result));

            // Quit if the message queue reading was not successful.
            if (Result != TNFCWrapper.ERR_SUCCESS) 
                return null;

            //Display the message details
            string sAddress = "";
            for (int i = 0; i < 12; i++) sAddress = sAddress + NFCAddress.Address[i].ToString("X2") + " ";

            Debug.WriteLine("  Sender ID   = " + sAddress);
            Debug.WriteLine("  Sender Type = " + NFCWrapper.NFCTagTypeToString(MessageInfo.CardType));
            Debug.WriteLine("  Device Name = " + MessageInfo.ReaderName);
            Debug.WriteLine("  Received At = " + SystemTimeToString(MessageInfo.TimeStamp));
            Debug.WriteLine("  NDEF Size   = " + NDEFSize.ToString());
            Debug.WriteLine(" ");

            // print NDEF as Hex dump
            PrintHexDump(NDEF, NDEFSize);

            // convert NDEF into XML
            string XML = "";
            Result = NFCWrapper.NDEF2XML(ref NDEF[0], NDEFSize, ref XML);
            Debug.WriteLine("NDEF2XML: " + NFCWrapper.NFCWrapperErrorToString(Result));
            
            // Quit if the message could not be converted to XML
            if (Result != TNFCWrapper.ERR_SUCCESS) 
                return null;

            // print NDEF as XML
            Debug.WriteLine(XML);
            Debug.WriteLine(" ");

            // Parse XML Message
            XDocument xml = XDocument.Parse(XML);
            var typeElement = xml.Descendants("Type");
            if (typeElement != null)
            {
                var type = typeElement.First().Value;
                XNamespace NDEF_Text = "";
                string payload = "No NFC Message Data";
                switch (type)
                {
                    case "urn:nfc:wkt:U":
                        //NDEF_Text = "urn:nfc:wkt:U";
                        //payload = xml.Descendants(NDEF_Text + "Text").First().Value;
                        payload = xml.ToString();
                        return payload;
                    case "text/x-vCard":
                        //NDEF_Text = "text/x-vCard";
                        //payload = xml.Descendants(NDEF_Text + "Text").First().Value;
                        payload = xml.ToString();
                        return payload;
                    case "urn:nfc:wkt:T":
                        //NDEF_Text = "urn:nfc:wkt:T";
                        //payload = xml.Descendants(NDEF_Text + "Text").First().Value;
                        payload = xml.ToString();
                        return payload;
                    default:
                        payload = xml.ToString();
                        return payload;

                }
            }
            return null;
        }

        private void WriteNFCMessage()
        {
            // Validate NFCWrapper.
            if (NFCWrapper == null) 
                return;

            UInt32 Result;

            //Create Smart Poster NDEF Message
            string URI = "http://www.anthonybaker.co";
            string Comment = "Anthony Baker's Blog";
            string Language = "en-US";
            string TargetType = "";
            UInt32 Size = 0;
            byte Action = 0;

            UInt32 NDEFSize = 1000;
            byte[] NDEF = new byte[NDEFSize];

            Result = TNFCWrapper.CreateNDEFSp(URI, Comment, Language, ref Action, ref Size, TargetType, ref NDEF[0], ref NDEFSize);
            Debug.WriteLine("CreateNDEFSp: " + NFCWrapper.NFCWrapperErrorToString(Result));
            if (Result != TNFCWrapper.ERR_SUCCESS) 
                return;

            // print NDEF as Hex dump
            PrintHexDump(NDEF, NDEFSize);

            // convert NDEF into XML
            string XML = "";
            Result = NFCWrapper.NDEF2XML(ref NDEF[0], NDEFSize, ref XML);
            Debug.WriteLine("NDEF2XML: " + NFCWrapper.NFCWrapperErrorToString(Result));
            if (Result != TNFCWrapper.ERR_SUCCESS) 
                return;

            // print NDEF as XML
            Debug.WriteLine(XML);
            Debug.WriteLine(" ");

            // now write the newly created NDEF message on an NFC tag 
            Debug.WriteLine("Put an NFC Tag on the reader within the next 5 seconds ...");

            TNFCAddress NFCAddress = new TNFCAddress();
            TMessageInfo MessageInfo = new TMessageInfo();
            Result = TNFCWrapper.WriteNDEF(ref NFCAddress, ref MessageInfo, ref NDEF[0], ref NDEFSize, false, true, 5);
            Debug.WriteLine("NDEF2XML: " + NFCWrapper.NFCWrapperErrorToString(Result));
            
            if (Result == TNFCWrapper.ERR_SUCCESS)
            {
                //Display the message details
                string sAddress = "";
                for (int i = 0; i < 12; i++) sAddress = sAddress + NFCAddress.Address[i].ToString("X2") + " ";

                Debug.WriteLine("  Receiver ID   = " + sAddress);
                Debug.WriteLine("  Receiver Type = " + NFCWrapper.NFCTagTypeToString(MessageInfo.CardType));
                Debug.WriteLine("  Device Name   = " + MessageInfo.ReaderName);
                Debug.WriteLine("  Sent At       = " + SystemTimeToString(MessageInfo.TimeStamp));
                Debug.WriteLine("  NDEF Size     = " + NDEFSize.ToString());
            }
            else if (Result == TNFCWrapper.ERR_NDEF_TOO_LARGE)
            {
                // the NDEF message does not fit on the NFC tag
                Debug.WriteLine("  Max. NDEF Size = " + NDEFSize.ToString());
            }
            Debug.WriteLine("");
        }

        // this function displays the NDEF as hex dump
        private void PrintHexDump(byte[] NDEF, UInt32 NDEFSize)
        {
            int Index = 0;
            string sHex = "";
            string sASCII = "";
            UInt32 MaxChars = 16;
            byte Value = 0;
            string sAddress = "";

            while (NDEFSize > 0)
            {
                sAddress = Index.ToString("X4") + ": ";
                sHex = "";
                sASCII = "";
                MaxChars = 16;

                if (MaxChars > NDEFSize) MaxChars = NDEFSize;
                for (int i = 0; i < MaxChars; i++)
                {
                    Value = NDEF[Index + i];
                    sHex = sHex + Value.ToString("X2") + " ";
                    if (Value > 31) sASCII = sASCII + (char)Value;
                    else sASCII = sASCII + ".";
                }
                Index += 16;
                if (NDEFSize >= 16) NDEFSize -= 16;
                else NDEFSize = 0;

                while (sASCII.Length < 16)
                {
                    sHex = sHex + "   ";
                    sASCII = sASCII + " ";
                }
                Debug.WriteLine("  " + sAddress + "  " + sHex + "  " + sASCII);
            }
            Debug.WriteLine(" ");
        }

        private string SystemTimeToString(TSystemTime SystemTime)
        {
            return SystemTime.wDay.ToString("D2") + "." +
                   SystemTime.wMonth.ToString("D2") + "." +
                   SystemTime.wYear.ToString("D4") + " - " +
                   SystemTime.wHour.ToString("D2") + ":" +
                   SystemTime.wMinute.ToString("D2") + ":" +
                   SystemTime.wSecond.ToString("D2");
        }

        #endregion
    }
}
