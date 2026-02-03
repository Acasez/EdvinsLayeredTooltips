using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LayeredTooltips
{
    public class TooltipWindow : MonoBehaviour, IPointerExitHandler
    {
        [Header("Local References")]
        [SerializeField] TextMeshProUGUI tooltipTextUI;
        [SerializeField] Image tooltipBackground;

        [Header("Set References")]
        TooltipManager tooltipManager;
        TooltipManager.Tooltip tooltip;
        TooltipWindowStyle tooltipWindowStyle;

        [Header("Settings")]
        float tooltipWindowHeight;
        [SerializeField] float tooltipWindowExtraHeight;
        [SerializeField] float tooltipWindowWidth;

        [Header("Locked Borders")]
        [SerializeField] List<Image> lockedBorders;
        public bool locked;

        public void SetReferences(TooltipManager tooltipManagerRef, TooltipManager.Tooltip tooltipRef)
        {
            tooltipManager = tooltipManagerRef;
            tooltip = tooltipRef;
            tooltipWindowStyle = tooltipManager.GetTooltipDecoration();
        }

        public void SetTooltipText(string tooltipText, KeywordHandler keywordHandler, KeywordScriptableObject keyword)
        {
            tooltipTextUI.text = "";

            TooltipWindowStyle thisTooltipStyle;
            if (keyword != null && keyword.specificTooltipStyle != null)
            {
                thisTooltipStyle = keyword.specificTooltipStyle;
                tooltipBackground.color = thisTooltipStyle.backgroundColor;
                Debug.Log("Using specific tooltip style");
            }
            else
            {
                thisTooltipStyle = tooltipWindowStyle;
                tooltipBackground.color = thisTooltipStyle.backgroundColor;
            }

            if (keyword != null && thisTooltipStyle.showTitle == true && !string.IsNullOrWhiteSpace(keyword.keywordName))
            {
                tooltipTextUI.text += "<b><size=" + thisTooltipStyle.titleTextSize + "%>" + keyword.keywordName;
                if (thisTooltipStyle.keywordIconNextToTitle == true && !string.IsNullOrWhiteSpace(keyword.icon))
                {
                    tooltipTextUI.text += "<sprite name=\"" + keyword.icon + "\">";
                }
                tooltipTextUI.text += "<size=100%></b>\n";
                if (keyword.keywordType != KeywordHandler.KeywordType.None)
                {
                    tooltipTextUI.text += "<size=" + thisTooltipStyle.typeTextSize + "%>" + keyword.keywordType + "<size=100%>\n";
                }
            }
            tooltipTextUI.text += keywordHandler.AddKeywordsToText(tooltipText, true, keyword, thisTooltipStyle);
            tooltipTextUI.color = thisTooltipStyle.textColor;
            SetTooltipSize();
        }

        void SetTooltipSize()
        {
            tooltipWindowHeight = tooltipTextUI.preferredHeight + tooltipWindowExtraHeight;
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(tooltipWindowWidth, tooltipWindowHeight);
        }

        public void DisplayLockTooltip(bool setLocked)
        {
            locked = setLocked;
            for (int i = 0; i < lockedBorders.Count; i++)
            {
                if (locked)
                {
                    lockedBorders[i].color = tooltipManager.GetTooltipDecoration().lockedBorderColor;
                }
                else
                {
                    lockedBorders[i].color = tooltipManager.GetTooltipDecoration().defaultBorderColor;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tooltipManager.RemoveTooltip(tooltip, false);
        }

        private void OnDestroy()
        {
            RemoveTooltip();
        }

        public void RemoveTooltip()
        {
            if (tooltipManager == null) return;
            tooltipManager.RemoveTooltip(tooltip, true);
        }
    }
}