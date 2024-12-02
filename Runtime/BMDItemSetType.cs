using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDItemSetType : MonoBehaviour
    {
        public ITEM_SET_TYPE[] ItemSetType;

        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ITEM_SET_TYPE
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] byOption;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] byMixItemLevel;
        }

    }
}