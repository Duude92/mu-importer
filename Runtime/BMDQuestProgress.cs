using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDQuestProgress : MonoBehaviour
    {
        public SQuestProgress[] QuestProgress;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SQuestProgress
        {
            public byte m_byUIType;
            public int m_nNPCWords;
            public int m_nPlayerWords;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = QM_MAX_ANSWER)]
            public int[] m_anAnswer;
            public int m_nSubject;
            public int m_nSummary;
        }

    }
}