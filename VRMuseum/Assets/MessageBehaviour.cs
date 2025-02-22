using FMOD.Studio;
using FMODUnity;
using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace vrm
{
    public class MessageBehaviour : MonoBehaviour
    {
        [SerializeField] private EventReference m_TypeSound;

        private Animator m_Animator;
        private TMPro.TextMeshProUGUI m_Text;

        public string TargetText = "New Text";
        public float LetterTickSeconds = 0.05f;
        public float PersistenceSeconds = 3f;
        private bool m_Written = false;

        public readonly System.Action FinishedWriting;
        public bool Written { get { return m_Written; } }

        private void Awake()
        {
            if (!TryGetComponent(out m_Animator))
                throw new System.Exception("Animator not found");
            if (!TryGetComponent(out m_Text))
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
            m_Written = false;
            List<EventInstance> instance = new();
            foreach (var letter in TargetText)
            {
                var text = m_Text.text;
                text += letter;
                m_Text.text = text;
                if (instance.Count > 0)
                {
                    AudioManager.Instance.StopSound(instance.Last());
                    instance.RemoveAt(instance.Count - 1);
                }
                instance.Add(AudioManager.Instance.PlaySound2D(m_TypeSound));
                instance.Last().setPitch(0.1f / LetterTickSeconds * 2f);
                yield return new WaitForSeconds(LetterTickSeconds);
            }
            m_Written = true;
            FinishedWriting?.Invoke();
            yield return new WaitForSeconds(PersistenceSeconds);
        }

        private void OnDisappearComplete()
        {
            Destroy(gameObject);
        }
    }
}
