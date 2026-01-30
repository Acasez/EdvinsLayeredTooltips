using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LayeredTooltips
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public enum TooltipType
        {
            Static,
            TextWithTooltips,
            LinkedKeyword
        }
        TooltipManager tooltipManager;
        [Header("Tooltip Type")]
        [SerializeField] TooltipType tooltipType;

        [Header("Tooltip Text")]
        [TextArea][SerializeField] string tooltipText;

        [Header("Linked Keyword")]
        [SerializeField] KeywordScriptableObject linkedKeyword;

        [Header("Variables")]
        [Tooltip("How long until tooltip appears")]
        [SerializeField] float tooltipTriggerTime = 0.5f;

        private void Start()
        {
            tooltipManager = FindAnyObjectByType<TooltipManager>();
            if (tooltipManager == null)
            {
                Debug.LogWarning("No tooltipManager!");
            }
            if (tooltipType == TooltipType.LinkedKeyword && linkedKeyword != null)
            {
                tooltipText = linkedKeyword.tooltipText;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipType == TooltipType.TextWithTooltips)
            {
                return; //Triggered by the KeywordTooltip script
            }
            if (tooltipManager.GetCallersTooltip(this) != null)
            {
                return; // Don't create a tooltip if this already
            }
            StopAllCoroutines();
            StartCoroutine(StartTimer());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (IsPointerOverTooltip())
            {
                return;
            }

            StopAllCoroutines();
            DestroyPreviousTooltip();
        }

        public void DestroyPreviousTooltip()
        {
            tooltipManager.RemoveCallersTooltip(this);
        }

        public Vector3 GetMousePosition()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.mousePosition;
#else
    return Mouse.current.position.ReadValue();
#endif
        }

        private IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(tooltipTriggerTime);
            DestroyPreviousTooltip();
            CreateTooltip(null, GetMousePosition(), null);
        }

        public bool IsPointerOverTooltip()
        {
            // Get the tooltip's RectTransform
            TooltipWindow tooltipWindow = tooltipManager.GetCallersTooltip(this)?.tooltipWindow;
            if (tooltipWindow == null)
            {
                return false; // No tooltip exists
            }

            if (!tooltipWindow.TryGetComponent<RectTransform>(out var tooltipRect))
            {
                return false; // No RectTransform found
            }

            // Convert mouse position to screen point
            Vector2 mouseScreenPos = GetMousePosition();

            // Check if the mouse is over the tooltip
            return RectTransformUtility.RectangleContainsScreenPoint(tooltipRect, mouseScreenPos);
        }

        public void CreateTooltip(string message, Vector3 location, KeywordScriptableObject keyword)
        {
            if (message == null)
            {
                message = tooltipText;
            }
            tooltipManager.SetupTooltipWindow(this, message, location, keyword);
        }

        public bool HasToolTip()
        {
            if (tooltipManager != null)
            {
                if (tooltipManager.GetCallersTooltip(this) != null)
                {
                    return true;
                }
            }
            else
            {
                Debug.Log("Tooltip Manager not set");
            }

            return false;
        }

        public void SetTooltipText(string text)
        {
            tooltipText = text;
        }
    }
}


