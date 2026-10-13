using UnityEngine;
using Verse;

namespace MakeYourBulk
{
    public class Searchbox
    {
        private string m_Buffer = "";

        private bool m_Changed = true;
        public bool LastCheckChanged
        {
            get
            {
                if (m_Changed)
                {
                    m_Changed = false;
                    return true;
                }

                return m_Changed;
            }
        }

        public bool IsContained(string str) => m_Buffer.NullOrEmpty() || str.ToLower().Contains(m_Buffer.ToLower());

        public void Show(Rect searchRect, Rect labelRect = new Rect(), string labelStr = "")
        {
            int length = m_Buffer.Length;
            m_Buffer = Widgets.TextField(searchRect, m_Buffer);
            if (length != m_Buffer.Length)
                m_Changed = true;

            if (!labelStr.NullOrEmpty())
                Widgets.Label(labelRect, labelStr);
        }
    }
}
