using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDSocketItem : MonoBehaviour
    {
        public SOCKET_OPTION_INFO[] SocketItem;


        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SOCKET_OPTION_INFO
        {
            public int m_iOptionID;
            public int m_iOptionCategory;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_SOCKET_OPTION_NAME_LENGTH)]
            public char[] m_szOptionName;
            public char m_bOptionType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public int[] m_iOptionValue;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] m_bySocketCheckInfo;
        }

    }
}