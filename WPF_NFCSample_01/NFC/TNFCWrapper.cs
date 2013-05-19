//////////////////////////////////////////////////////////////////////////////////
//  Copyright (C) 2012 Identive GmbH                                            //
//                                                                              //
//     This source code is provided 'as-is', without any express or implied     //
//     warranty. In no event will Identive be held liable for any damages       //
//     arising from the use of this software.                                   //
//                                                                              //
//     Identive does not warrant, that the source code will be free from        //
//     defects in design or workmanship or that operation of the source code    //
//     will be error-free. No implied or statutory warranty of merchantability  //
//     or fitness for a particulat purpose shall apply.                         //
//     The entire risk of quality and performance is with the user of this      //
//     source code.                                                             //
//                                                                              //
//     Permission is granted to anyone to use this software for any purpose,    //
//     including commercial applications, and to alter it and redistribute it   //
//     freely, subject to the following restrictions:                           //
//                                                                              //
//     1. The origin of this source code must not be misrepresented; you must   //
//        not claim that you wrote the original source code. If you use this    //
//        source code in a product, an acknowledgment in the product            //
//        documentation would be appreciated but is not required.               //
//                                                                              //
//     2. Altered source versions must be plainly marked as such, and must not  //
//        be misrepresented as being the original source code.                  //
//                                                                              //
//     3. This notice may not be removed or altered from any source             //
//        distribution.                                                         //
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace WPF_NFCSample_01
{
    [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Ansi)]
    public class TSystemTime
    {
        [FieldOffset(0)]
        public ushort wYear;
        [FieldOffset(2)]
        public ushort wMonth;
        [FieldOffset(4)]
        public ushort wDayOfWeek;
        [FieldOffset(6)]
        public ushort wDay;
        [FieldOffset(8)]
        public ushort wHour;
        [FieldOffset(10)]
        public ushort wMinute;
        [FieldOffset(12)]
        public ushort wSecond;
        [FieldOffset(14)]
        public ushort wMilliseconds;
    }

    [StructLayout(LayoutKind.Sequential, Size = 12)]
    public struct TNFCAddress
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Address;
    }

    [StructLayout(LayoutKind.Sequential, Size = 84)]
    public struct TMessageInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string ReaderName;
        public UInt32 CardType;
        public TSystemTime TimeStamp;
    }

    class TNFCWrapper
    {

        #region constants

        // Error Codes
        public const Int32 ERR_SUCCESS = 0;
        public const Int32 ERR_NOT_INITIALIZED = 1;
        public const Int32 ERR_INVALID_PARAMETER = 2;
        public const Int32 ERR_BUFFER_TOO_SMALL = 3;
        public const Int32 ERR_NO_MESSAGE = 4;
        public const Int32 ERR_TIMEOUT = 5;
        public const Int32 ERR_WRITE_ERROR = 6;
        public const Int32 ERR_NDEF_TOO_LARGE = 7;
        public const Int32 ERR_TAG_NOT_SUPPORTED = 8;
        public const Int32 ERR_TAG_NOT_FORMATTED = 9;
        public const Int32 ERR_TAG_NOT_EMPTY = 10;
        public const Int32 ERR_TAG_READONLY = 11;
        public const Int32 ERR_CANCELLED = 12;
        public const Int32 ERR_INVALID_NDEF = 13;
        public const Int32 ERR_NOT_LISTENING = 14;

        public const Int32 ERR_DLL_NOT_VALID = 20;

        //Messages
        public const Int32 WM_USER = 0x0400;
        public const Int32 WM_NFC_NOTIFY = WM_USER + 1;

        public const Int32 NFC_NDEF_FOUND = 1;
        public const Int32 NFC_DEVICE_CHANGED = 2;
        public const Int32 NFC_UNKNOWN_SERVICE = 3;
        public const Int32 NFC_CONNECTED = 4;
        public const Int32 NFC_DISCONNECTED = 5;
        public const Int32 NFC_IDLE = 6;

        //NFC tags and devices
        public const Int32 TAG_UNKNOWN = 0x0;
        public const Int32 TAG_MIFARE_STD_1K = 0x1;
        public const Int32 TAG_MIFARE_STD_4K = 0x2;
        public const Int32 TAG_MIFARE_UL = 0x4;
        public const Int32 TAG_MIFARE_DESFIRE = 0x8;
        public const Int32 TAG_FELICA = 0x10;
        public const Int32 TAG_TOPAZ = 0x20;
        public const Int32 TAG_SMARTMX = 0x40;
        public const Int32 TAG_MIFARE_DESFIRE_EV1 = 0x80;
        public const Int32 TAG_MIFARE_PLUS = 0x100;
        public const Int32 TAG_MIFARE_UL_C = 0x200;

        public const UInt32 DEVICE_GENERAL = 0x80000000;

        public const UInt32 NFC_TAG_TYPE1 = TAG_TOPAZ;
        public const UInt32 NFC_TAG_TYPE2 = TAG_MIFARE_UL | TAG_MIFARE_UL_C;
        public const UInt32 NFC_TAG_TYPE3 = TAG_FELICA;
        public const UInt32 NFC_TAG_TYPE4 = TAG_MIFARE_DESFIRE | TAG_SMARTMX | TAG_MIFARE_DESFIRE_EV1;
        public const UInt32 NFC_FORMATTED_TAG = TAG_MIFARE_STD_1K | TAG_MIFARE_STD_4K | TAG_MIFARE_PLUS;
        public const UInt32 NFC_DEVICE = DEVICE_GENERAL;

        public const UInt32 NFC_TAGS = NFC_TAG_TYPE1 | NFC_TAG_TYPE2 | NFC_TAG_TYPE3 | NFC_TAG_TYPE4 | NFC_FORMATTED_TAG;
        public const UInt32 NFC_TAGS_AND_DEVICES = NFC_TAGS | NFC_DEVICE;

        #endregion

        #region imported functions 
        // imported functions 
        [DllImport("SCM_NFC.dll", EntryPoint = "Initialize", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Initialize(UInt32 hWnd);

        [DllImport("SCM_NFC.dll", EntryPoint = "StartListening", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 StartListening();

        [DllImport("SCM_NFC.dll", EntryPoint = "StopListening", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 StopListening();

        [DllImport("SCM_NFC.dll", EntryPoint = "Rescan", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 Rescan();

        [DllImport("SCM_NFC.dll", EntryPoint = "GetNDEFQueueInfo", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 GetNDEFQueueInfo(ref UInt32 DeviceCount, ref UInt32 MessageCount, ref UInt32 NextMessageSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "ClearNDEFQueue", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ClearNDEFQueue(ref TNFCAddress Sender);

        [DllImport("SCM_NFC.dll", EntryPoint = "ReadNDEF", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ReadNDEF(ref TNFCAddress Sender, ref TMessageInfo MessageInfo, ref byte NDEF, ref  UInt32 NDEFSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "WriteNDEF", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 WriteNDEF(ref TNFCAddress Sender, ref TMessageInfo MessageInfo, ref byte NDEF, ref UInt32 NDEFSize, bool DontOverwrite, bool FormatCard, UInt32 TimeOut);

        [DllImport("SCM_NFC.dll", EntryPoint = "NDEF2XML", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 _NDEF2XML(ref byte NDEF, UInt32 NDEFSize, StringBuilder XML, ref UInt32 XMLSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "CreateNDEFSp", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 CreateNDEFSp(string URI, string Comment, string Language, ref byte Action, ref UInt32 Size, string TargetType, ref byte NDEF, ref UInt32 NDEFSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "CreateNDEFURI", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 CreateNDEFURI(string URI, ref byte NDEF, ref UInt32 NDEFSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "CreateNDEFText", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 CreateNDEFText(string Text, string Language, ref byte NDEF, ref UInt32 NDEFSize);

        [DllImport("SCM_NFC.dll", EntryPoint = "CreateNDEFvCard", ExactSpelling = false, CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 CreateNDEFvCard(string VCard, ref byte NDEF, ref UInt32 NDEFSize);

        #endregion

        #region constructor

        public TNFCWrapper()
        {
        }

        #endregion

        #region public methods

        public string NFCWrapperErrorToString(UInt32 ErrorCode)
        {
            switch (ErrorCode)
            {
                case ERR_SUCCESS: return "OK";
                case ERR_NOT_INITIALIZED: return "ERR_NOT_INITIALIZED";
                case ERR_INVALID_PARAMETER: return "ERR_INVALID_PARAMETER";
                case ERR_BUFFER_TOO_SMALL: return "ERR_BUFFER_TOO_SMALL";
                case ERR_NO_MESSAGE: return "ERR_NO_MESSAGE";
                case ERR_TIMEOUT: return "ERR_TIMEOUT";
                case ERR_WRITE_ERROR: return "ERR_WRITE_ERROR";
                case ERR_NDEF_TOO_LARGE: return "ERR_NDEF_TOO_LARGE";
                case ERR_TAG_NOT_SUPPORTED: return "ERR_TAG_NOT_SUPPORTED";
                case ERR_TAG_NOT_FORMATTED: return "ERR_TAG_NOT_FORMATTED";
                case ERR_TAG_NOT_EMPTY: return "ERR_TAG_NOT_EMPTY";
                case ERR_TAG_READONLY: return "ERR_TAG_READONLY";
                case ERR_CANCELLED: return "ERR_CANCELLED";
                case ERR_INVALID_NDEF: return "ERR_INVALID_NDEF";
                case ERR_DLL_NOT_VALID: return "ERR_DLL_NOT_VALID";
                case ERR_NOT_LISTENING: return "ERR_NOT_LISTENING";
                default: return "UNKNOWN";
            }
        }

        public string NFCTagTypeToString(UInt32 TagType)
        {
            switch (TagType)
            {
                case TAG_MIFARE_STD_1K: return "MIFARE Classic 1K";
                case TAG_MIFARE_STD_4K: return "MIFARE Classic 4K";
                case TAG_MIFARE_PLUS: return "MIFARE Plus";
                case TAG_MIFARE_UL: return "MIFARE Ultralight";
                case TAG_MIFARE_UL_C: return "MIFARE Ultralight C";
                case TAG_MIFARE_DESFIRE: return "MIFARE DESFire";
                case TAG_MIFARE_DESFIRE_EV1: return "MIFARE DESFire-EV1";
                case TAG_FELICA: return "Felica";
                case TAG_TOPAZ: return "Topaz";
                case DEVICE_GENERAL: return "NFC Device";
                default: return "UNKNOWN";
            }
        }

        public UInt32 NDEF2XML(ref byte NDEF, UInt32 NDEFSize, ref string XML)
        {
            StringBuilder sbXML = new StringBuilder("");
            UInt32 XMLSize = 0;
            UInt32 Result;
            XML = "";
            Result = _NDEF2XML(ref NDEF, NDEFSize, sbXML, ref XMLSize);
            if (Result == ERR_BUFFER_TOO_SMALL)
            {
                sbXML.EnsureCapacity((int)XMLSize);
                Result = _NDEF2XML(ref NDEF, NDEFSize, sbXML, ref XMLSize);
                XML = sbXML.ToString();
            }
            return Result;
        }

        #endregion

    }
}
