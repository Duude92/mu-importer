using System.Collections.Generic;
using UnityEngine;

namespace MUImporter
{
    public class BMDQuestWords : MonoBehaviour
    {
        public Dictionary<int, string> QuestWords;

        public struct SQuestWordsHeader
        {
            public int m_nIndex;
            public short m_nWordsLen;
        }
    }
}