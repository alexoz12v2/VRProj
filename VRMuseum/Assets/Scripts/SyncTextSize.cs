using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SyncParagraphSize : MonoBehaviour
{
    public TextMeshProUGUI titleText;  // Reference to the title TextMeshPro component
    public TextMeshProUGUI corpusText; // Reference to the corpus TextMeshPro component
    private RectTransform paragraphRectTransform;
    private LayoutGroup layoutGroup; // Reference to the LayoutGroup (e.g., VerticalLayoutGroup)
 
    void Awake()
    {
        paragraphRectTransform = GetComponent<RectTransform>();
        layoutGroup = GetComponentInParent<LayoutGroup>();  // Assuming the parent has a LayoutGroup
    }

    void Update()
    {
        // Update the RectTransform based on the corpus height
        UpdateParagraphSize();
    }

    void UpdateParagraphSize()
    {
        if (titleText == null || corpusText == null)
        {
            Debug.LogError($"you didn't set either titleText {titleText} or corpusText {corpusText}");
            return;
        }

        // Get the preferred height of the title and corpus
        float titleHeight = titleText.preferredHeight;
        float corpusHeight = corpusText.preferredHeight;

        // Calculate the total height of the paragraph (title + corpus)
        float totalHeight = titleHeight + corpusHeight;

        // Update the paragraph container's RectTransform height
        // Set the height of the RectTransform to fit the total height (title + corpus)
        paragraphRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        // Optionally, adjust the position of the paragraph container if needed
        // For example, we can update the bottom boundary of the paragraph container
        Vector3 currentPos = paragraphRectTransform.localPosition;
        paragraphRectTransform.localPosition = new Vector3(currentPos.x, currentPos.y, currentPos.z);

        // Trigger the Layout Group to recalculate the layout
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(paragraphRectTransform);
        }
    }
}
