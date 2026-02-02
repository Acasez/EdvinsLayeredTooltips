using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LayeredTooltips
{
    public class TooltipManager : MonoBehaviour
    {
        public enum MouseButton { Left, Right, Middle, SideBack, SideForward }

        [Header("Settings")]
        [Tooltip("How many tooltips there should be. I would suggest like 5, but any amount is fine. You could go beyond 25 in code if you want")]
        [SerializeField][Range(1, 25)] int tooltipAmount = 5;
        [SerializeField] bool mobile;
        [SerializeField] TooltipWindow tooltipWindowPrefab;

        [Header("Canvas")]
        Canvas canvas;
        RectTransform canvasRectTransform;

        [Header("Keywords")]
        [SerializeField] KeywordHandler keywordHandler;

        [Header("Tooltip Window Settings")]
        [Tooltip("Margin from the edge of the screen")]
        [SerializeField] int edgeOfScreenMargin = 15;
        [Tooltip("How far down the tooltip is from mouse position")]
        [SerializeField] float tipNudge = 100f;
        [Tooltip("How far down the tooltip is from mouse position, based on the tooltips height")]
        [SerializeField] float tooltipSizeDownwardFactor = 0.5f;

        [Header("Decoration")]
        [SerializeField] TooltipWindowStyle tooltipWindowStyle;

        [Header("Ingame Tooltips")]
        [SerializeField] List<Tooltip> tooltips;

        [SerializeField] private MouseButton activeButton = MouseButton.Middle;

        [Serializable]
        public class Tooltip
        {
            public TooltipWindow tooltipWindow;
            public TooltipTrigger trigger;
            public bool active;
            public bool locked;
        }

        private void Awake()
        {
            canvas = GetComponentInChildren<Canvas>();
            canvasRectTransform = canvas.GetComponent<RectTransform>();
            if (Application.isMobilePlatform)
            {
                mobile = true;
            }
            else
            {
                mobile = false;
            }
            for (int i = 0; i < tooltipAmount; i++)
            {
                Tooltip currentTooltip = new()
                {
                    tooltipWindow = Instantiate(tooltipWindowPrefab, canvas.transform)
                };
                currentTooltip.tooltipWindow.SetReferences(this, currentTooltip);
                currentTooltip.trigger = currentTooltip.tooltipWindow.GetComponentInChildren<TooltipTrigger>();
                tooltips.Add(currentTooltip);
                RemoveTooltip(currentTooltip, true);
            }
        }

        private void Update()
        {
            bool isButtonPressed = false;

            switch (activeButton)
            {
                case MouseButton.Left:
                    isButtonPressed = GetMouseButtonDown(0);
                    break;
                case MouseButton.Right:
                    isButtonPressed = GetMouseButtonDown(1);
                    break;
                case MouseButton.Middle:
                    isButtonPressed = GetMouseButtonDown(2);
                    break;
                case MouseButton.SideBack:
                    isButtonPressed = GetMouseButtonDown(3);
                    break;
                case MouseButton.SideForward:
                    isButtonPressed = GetMouseButtonDown(4);
                    break;
            }

            if (isButtonPressed)
            {
                Tooltip topTooltip = GetTopTooltip();
                if (topTooltip != null)
                {
                    LockUnlockTooltip(topTooltip);
                }
            }
        }

        private bool GetMouseButtonDown(int button)
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(button);
#else
        return Mouse.current != null && GetInputSystemButton(Mouse.current, button);
#endif
        }

#if ENABLE_LEGACY_INPUT_MANAGER
        private bool GetInputSystemButton(Mouse mouse, int button)
        {
            switch (button)
            {
                case 0: return mouse.leftButton.wasPressedThisFrame;
                case 1: return mouse.rightButton.wasPressedThisFrame;
                case 2: return mouse.middleButton.wasPressedThisFrame;
                case 3: return mouse.backButton.wasPressedThisFrame;
                case 4: return mouse.forwardButton.wasPressedThisFrame;
                default: return false;
            }
        }
#endif
        public List<KeywordScriptableObject> GetKeywords()
        {
            return keywordHandler.keywords;
        }

        public KeywordHandler GetKeywordHandler()
        {
            return keywordHandler;
        }

        public TooltipWindowStyle GetTooltipDecoration()
        {
            return tooltipWindowStyle;
        }

        public void LockUnlockTooltip(Tooltip tooltip)
        {
            if (tooltip.locked)
            {
                tooltip.locked = false;
                tooltip.tooltipWindow.DisplayLockTooltip(false);
                if (!tooltip.trigger.IsPointerOverTooltip())
                { //Remove tooltip if the mouse isn't there currently
                    RemoveTooltip(tooltip, true);
                }
            }
            else
            {
                tooltip.locked = true;
                tooltip.tooltipWindow.DisplayLockTooltip(true);
            }
        }

        public Tooltip GetTopTooltip()
        {
            for (int i = tooltips.Count - 1; i > -1; i--)
            {
                if (tooltips[i] != null)
                {
                    if (tooltips[i].active == true)
                    {
                        return tooltips[i];
                    }
                }
            }

            Debug.Log("No available tooltips");
            return null;
        }

        public Tooltip GetFirstAvaibleTooltip()
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i] != null)
                {
                    if (tooltips[i].active == false)
                    {
                        return tooltips[i];
                    }
                }
            }

            Debug.Log("No available tooltips");
            return null;
        }


        public void SetupTooltipWindow(TooltipTrigger caller, string message, Vector3 screenPosition, KeywordScriptableObject keyword)
        {
            if (GetCallersTooltip(caller) != null)
            {
                Debug.Log("Caller already has tooltip");
                return;
            }

            Tooltip thisTooltip = GetFirstAvaibleTooltip();
            if (thisTooltip == null) { return; }

            thisTooltip.active = true;
            thisTooltip.trigger = caller;

            thisTooltip.tooltipWindow.gameObject.SetActive(true);
            thisTooltip.tooltipWindow.SetReferences(this, thisTooltip);
            thisTooltip.tooltipWindow.SetTooltipText(message, keywordHandler, keyword);

            RectTransform tooltipRect = thisTooltip.tooltipWindow.GetComponent<RectTransform>();
            Vector2 clampedPos = GetPositionInsideCanvas(tooltipRect, screenPosition);
            Debug.Log("Clamped Pos: " + clampedPos);
            thisTooltip.tooltipWindow.transform.localPosition = clampedPos;
        }

        private Vector2 GetPositionInsideCanvas(RectTransform tooltipRect, Vector2 screenPosition)
        {
            // Convert screen position to local position in canvas
            Vector2 localPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                null, // Use null for overlay canvas, or specify your camera if using Screen Space - Camera
                out localPosition
            );

            // Calculate tooltip size
            float tooltipWidth = tooltipRect.rect.width;
            float tooltipHeight = tooltipRect.rect.height;

            // Calculate the desired position (adjust for tooltip pivot and offset)
            float tipDownDistance = (tooltipHeight * tooltipSizeDownwardFactor) + tipNudge;
            if (mobile) tipDownDistance *= -1f;
            Vector2 desiredPosition = new(localPosition.x, localPosition.y - tipDownDistance);

            // Get canvas bounds (already in local space)
            Vector2 canvasMin = canvasRectTransform.rect.min;
            Vector2 canvasMax = canvasRectTransform.rect.max;

            // Calculate min/max allowed positions (accounting for tooltip size and pivot)
            float minX = canvasMin.x + edgeOfScreenMargin + tooltipWidth * tooltipRect.pivot.x;
            float maxX = canvasMax.x - edgeOfScreenMargin - tooltipWidth * (1 - tooltipRect.pivot.x);
            float minY = canvasMin.y + edgeOfScreenMargin + tooltipHeight * tooltipRect.pivot.y;
            float maxY = canvasMax.y - edgeOfScreenMargin - tooltipHeight * (1 - tooltipRect.pivot.y);

            // Clamp
            float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }

        public void RemoveCallersTooltip(TooltipTrigger caller)
        {
            Tooltip thisTooltip = GetCallersTooltip(caller);

            RemoveTooltip(thisTooltip, false);
        }

        public void RemoveTooltip(Tooltip thisTooltip, bool trueRemove)
        {
            if (thisTooltip == null || thisTooltip.tooltipWindow == null) { return; };

            if (thisTooltip.locked && trueRemove == false)
            {
                return;
            }
            RemoveTooltipsAboveThis(thisTooltip);

            thisTooltip.active = false;
            thisTooltip.trigger = null;
            thisTooltip.tooltipWindow.gameObject.SetActive(false);
        }

        public void RemoveTooltipsAboveThis(Tooltip thisTooltip)
        {
            for (int i = tooltips.Count - 1; i > -1; i--)
            {
                if (tooltips[i] == thisTooltip)
                {
                    return;
                }
                if (tooltips[i].active)
                {
                    Debug.Log("Removing tooltip " + i);
                    RemoveTooltip(tooltips[i], true);
                }
            }
        }

        public Tooltip GetCallersTooltip(TooltipTrigger caller)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i] != null)
                {
                    if (tooltips[i].trigger == caller)
                    {
                        return tooltips[i];
                    }
                }
            }

            return null;
        }
    }
}

