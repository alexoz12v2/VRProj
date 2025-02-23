using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// https://gamedevbeginner.com/events-and-delegates-in-unity/#event_based_systems

namespace vrm {
    // event which requests a GUI to be displayed with the given canvas data, at a given position with a given orientation
    public class GUIEvent : UnityEvent<CanvasData, Vector3, Quaternion> { }

    // TODO derive base class Singleton
    public class GUIEvents : MonoBehaviour
    {
        private static GUIEvents _instance;
        public GUIEvent OnGUIDisplayRequest = new();

        [SerializeField] private GameObject _canvasPrefab; // Assigned in inspector
        [SerializeField] private GameObject _paragraphPrefab; // Assigned in inspector

        private TextMeshProUGUI _titleTextComponent;
        private GameObject _uiContentObject;

        private const string UICanvas = "UICanvas";
        private const string UIContent = "UIContent";
        private const string UIParagraphButton = "UIParagraphButton";
        private const string UIParagraphContent = "UIParagraphContent";
        private const string UIParagraphTitle = "UIParagraphTitle";

        public static GUIEvents Instance { get { return _instance; } }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogError("You tried to instantiate the Singleton GUIEvents more than once");
                Destroy(gameObject);
            }
            else
            {
                _instance = this;

                // Validate the Canvas Prefab
                if (_canvasPrefab != null)
                {
                    GameObject canvas = Methods.FindChildWithTag(_canvasPrefab, UICanvas);
                    if (canvas == null)
                        Debug.LogError("Couldn't find canvas game object (tagged with UICanvas)");
                    Canvas canvasComponent = canvas.GetComponent<Canvas>();
                    canvasComponent.worldCamera = Camera.main;
                    GraphicRaycaster raycasterComponent = canvas.GetComponent<GraphicRaycaster>();

                    if (canvasComponent == null || raycasterComponent == null)
                        Debug.LogError("Canvas Prefab must have both a Canvas and a Graphic Raycaster component.");

                    _uiContentObject = Methods.FindChildWithTag(_canvasPrefab, UIContent);
                    if (_uiContentObject == null)
                        Debug.LogError("Canvas Prefab must contain a GameObject with the tag 'UIContent'.");

                    _titleTextComponent = TitlebarFrom(_canvasPrefab);
                }
                else
                    Debug.LogError("Canvas Prefab is not assigned in the inspector.");

                // Validate the Paragraph Prefab
                if (_paragraphPrefab != null)
                {
                    RectTransform[] rectTransforms = _paragraphPrefab.GetComponentsInChildren<RectTransform>(true);
                    if (rectTransforms.Length == 0)
                        Debug.LogError("Paragraph Prefab must have all its objects with RectTransform components.");
                }
                else
                    Debug.LogError("Paragraph Prefab is not assigned in the inspector.");

                OnGUIDisplayRequest.AddListener(GuiRequest);
                Debug.Log("Successfully Instanciated a GUIEvents Object");
            }
        }

        private TextMeshProUGUI TitlebarFrom(GameObject canvas)
        {
            GameObject titleBar = Methods.FindChildByName(canvas, "TitleBar");
            if (titleBar != null)
            {
                CanvasRenderer renderer = titleBar.GetComponent<CanvasRenderer>();
                if (renderer == null)
                    Debug.LogError("TitleBar must have a Canvas Renderer component.");

                GameObject titleText = Methods.FindChildByName(titleBar, "TitleText");
                if (titleText != null)
                {
                    var titleTextComponent = titleText.GetComponent<TextMeshProUGUI>();
                    if (titleTextComponent == null)
                        Debug.LogError("TitleText must have a TextMeshProUGUI component.");
                    return titleTextComponent;
                }
                else
                    Debug.LogError("TitleBar must contain a child GameObject named 'TitleText'.");
            }
            else
                Debug.LogError("Canvas Prefab must contain a GameObject named 'TitleBar'.");

            return null;
        }

        private void GuiRequest(CanvasData canvasData, Vector3 position, Quaternion orientation)
        {
            GameObject obj = Instantiate(_canvasPrefab, position, orientation);
            obj.transform.SetParent(null);
            TextMeshProUGUI text = TitlebarFrom(obj);
            if (text == null)
            {
                Debug.LogError("Unexpected Error: GUIEvent prefab didn't have the titlebar");
                return;
            }

            if (text.text.Length > 0)
                text.text.Remove(0);
            text.text = canvasData.Title;

            GameObject contentParent = Methods.FindChildWithTag(obj, UIContent);
            if (contentParent == null)
            {
                Debug.LogError("Unexpected Error: GUIEvent prefab didn't have the UIContent Tagged GameObject");
                return;
            }

            foreach (var data in canvasData.Paragraphs)
            {
                GameObject paragraph = Instantiate(_paragraphPrefab, contentParent.transform);
                GameObject o = Methods.FindChildWithTag(paragraph, UIParagraphTitle);
                if (o == null)
                {
                    Debug.LogErrorFormat("Couldn't find title gameobject with tag UIParagraphTitle in gameobject {}", obj);
                    return;
                }
                else
                {
                    var parTitle = o.GetComponent<TextMeshProUGUI>();
                    if (parTitle == null)
                    {
                        Debug.LogError("Object Tagged With UIParagraphTitle didn't have TextMeshProUGUI Component");
                        return;
                    }

                    parTitle.text = data.Title != null ? data.Title : "ERROR EMPTY TITLE";
                }

                o = Methods.FindChildWithTag(paragraph, UIParagraphButton);
                if (o == null)
                {
                    Debug.LogError("Doundn't filnd any object with UIParagrphButton");
                    return;
                }
                else
                { // TODO
                }

                o = Methods.FindChildWithTag(paragraph, UIParagraphContent);
                if (o == null)
                {
                    Debug.LogError("Couldn't find any object with UIParagraphContent");
                    return;
                }
                else
                {
                    var parCorput = o.GetComponent<TextMeshProUGUI>();
                    if (parCorput == null)
                    {
                        Debug.LogError("Object Tagged With UIParagraphContent didn't have TextMeshProUGUI component");
                        return;
                    }

                    parCorput.text = data.Corpus; // guaranteed to be nonnull
                }
            }
        }

        private void OnDestroy()
        {
            if (this == _instance)
                _instance = null;
        }
    }

}
