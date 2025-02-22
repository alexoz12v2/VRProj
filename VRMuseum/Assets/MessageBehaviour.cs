using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace vrm
{
    public class MessageBehaviour : MonoBehaviour
    {
        private Animator m_Animator;
        private TMPro.TextMeshProUGUI m_Text;

        public string TargetText = "New Text";
        public float LetterTickSeconds = 0.1f;
        public float PersistenceSeconds = 4f;

        private void Awake()
        {
            if (TryGetComponent(out m_Animator))
                throw new System.Exception("Animator not found");
            if (TryGetComponent(out m_Text))
                throw new System.Exception("TMPro.TextMeshProUGUI not found");
        }

        private void Start()
        {
            new Task(WriteTask()).Finished += Disappear;
        }

        private void Disappear(bool manual)
        {
            m_Animator.SetTrigger("Disappear");
        }

        private IEnumerator WriteTask()
        {
            m_Text.SetText("");
            foreach (var letter in TargetText)
            {
                var text = m_Text.text;
                text += letter;
                m_Text.text = text;
                yield return new WaitForSeconds(LetterTickSeconds);
            }
            yield return new WaitForSeconds(PersistenceSeconds);
        }

        private void OnDisappearComplete()
        {
            Destroy(gameObject);
        }
    }
}
