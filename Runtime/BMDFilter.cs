using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDFilter : MonoBehaviour
    {
        public FILTER[] Filters;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct FILTER
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] AbuseFilter;
        }
    }
}