using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using vrm;

public class GUIWriter : MonoBehaviour
{
    public string msg = "dfjsalfjdslkafjll";
    private Text TextComp;

    void OnGuiRequest(Vector3 position, string guiKey)
    {
        CanvasData data = GUIDictionary.Instance.GetCanvasDataByKey(guiKey);
        StartCoroutine(data.Paragraphs[0].Corpus);
    }

    private void Awake()
    {
        TextComp = GetComponent<Text>();
    }

    public IEnumerator type()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < msg.Length; i++)
        {
            TextComp.text = msg.Substring(0, i);
            yield return new WaitForSeconds(1);
        }
    }
}
