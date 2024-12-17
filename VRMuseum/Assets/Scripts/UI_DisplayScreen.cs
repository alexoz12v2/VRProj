using UnityEngine;
using UnityEngine.Events;

/// <summary>
///  Needs a MeshCollider and a properly UV mapped mesh
/// </summary>
public class UI_DisplayScreen : MonoBehaviour
{
    [SerializeField] LayerMask RaycastMask = ~0;
    [SerializeField] float RaycastDistance = 5.0f;
    [SerializeField] Vector2 MouseScrollSensitivity = Vector2.one;
    [SerializeField] bool InvertHorizontalScoll = false;
    [SerializeField] bool InvertVerticalScroll = false;

    [SerializeField] UnityEvent<Vector2, Vector2> OnCursorInput = new();

    // Update is called once per frame
    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        Vector3 MousePosition = Input.mousePosition;
        Vector2 MouseScroll = Input.mouseScrollDelta;
#else
        Vector3 MousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector2 MouseScroll = UnityEngine.InputSystem.Mouse.current.scroll.ReadValue();
#endif // ENABLE_LEGACY_INPUT_MANAGER

        // apply sensitivity and inversion
        MouseScroll.x *= MouseScrollSensitivity.x * (InvertHorizontalScoll ? -1f : 1f);
        MouseScroll.y *= MouseScrollSensitivity.y * (InvertVerticalScroll ? -1f : 1f);

        // construct our ray from the mouse position
        Ray MouseRay = Camera.main.ScreenPointToRay(MousePosition);

        // perform our raycast
        RaycastHit HitResult;
        if (Physics.Raycast(MouseRay, out HitResult, RaycastDistance, RaycastMask, QueryTriggerInteraction.Ignore))
        {
            // ignore if not us
            if (HitResult.collider.gameObject != gameObject)
                return;

            OnCursorInput.Invoke(HitResult.textureCoord, MouseScroll);
        }
    }
}