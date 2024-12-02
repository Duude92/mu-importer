using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MUImporter
{
    public class BMDMoveReq : MonoBehaviour
    {
        public MOVEINFODATA[] InfoData;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MOVEREQINFO
        {
            public int index;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szMainMapName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szSubMapName;
            public int iReqLevel;
            public int m_iReqMaxLevel;
            public int iReqZen;
            public int iGateNum;
        };
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct MOVEINFODATA
        {
            public MOVEREQINFO _ReqInfo;
        
            public bool _bCanMove;
            public bool _bStrife;
            public bool _bSelected;

            //bool operator ==(const int& iIndex) const {int iTempIndex = iIndex; return _ReqInfo.index==iTempIndex;};
            public static bool operator ==(MOVEINFODATA thisData, int index) => index == thisData._ReqInfo.index;
            public static bool operator !=(MOVEINFODATA thisData, int index) => index != thisData._ReqInfo.index;

        };

    }
}