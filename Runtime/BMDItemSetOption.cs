using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDItemSetOption : MonoBehaviour
    {
        public ITEM_SET_OPTION[] ItemSetOption;


        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ITEM_SET_OPTION
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public char[] strSetName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6 * 2)]
            public byte[] byStandardOption;//[6][2];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6 * 2)]
            public byte[] byStandardOptionValue;//[6][2];
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] byExtOption;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] byExtOptionValue;
            public byte byOptionCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] byFullOption;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] byFullOptionValue;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CLASS)]
            public byte[] byRequireClass;
        }
    }
}