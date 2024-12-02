using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDDialog : MonoBehaviour
    {

        public DIALOG_SCRIPT[] dialogs;

        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DIALOG_SCRIPT
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_LENGTH_DIALOG)]
            public char[] m_lpszText;
            public int m_iNumAnswer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANSWER_FOR_DIALOG)]
            public int[] m_iLinkForAnswer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANSWER_FOR_DIALOG)]
            public int[] m_iReturnForAnswer;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ANSWER_FOR_DIALOG * MAX_LENGTH_ANSWER)]
            public char[] m_lpszAnswer;//[MAX_ANSWER_FOR_DIALOG][MAX_LENGTH_ANSWER];
        };
    }
}