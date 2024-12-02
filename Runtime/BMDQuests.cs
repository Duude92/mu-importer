using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static MUImporter.Enums;

namespace MUImporter
{
    public class BMDQuests : MonoBehaviour
    {
        public QUEST_ATTRIBUTE[] Quests;
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct QUEST_ATTRIBUTE
        {
            public short shQuestConditionNum;
            public short shQuestRequestNum;
            public short wNpcType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] strQuestName;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_QUEST_CONDITION)]
            public QUEST_CLASS_ACT[] QuestAct;
            // public int someshort;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_QUEST_REQUEST)]

            public QUEST_CLASS_REQUEST[] QuestRequest;
        }
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct QUEST_CLASS_ACT
        {
            public byte chLive;
            public byte byQuestType;
            public short wItemType;
            public byte byItemSubType;
            public byte byItemLevel;
            public byte byItemNum;
            public byte byRequestType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_CLASS)]
            public byte[] byRequestClass;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public short[] shQuestStartText;
        };
        [Serializable, StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct QUEST_CLASS_REQUEST
        {
            public byte byLive;
            public byte byType;
            public short wCompleteQuestIndex;
            public short wLevelMin;
            public short wLevelMax;
            public short wRequestStrength;
            public int dwZen;
            public short shErrorText;
        };

    }
}