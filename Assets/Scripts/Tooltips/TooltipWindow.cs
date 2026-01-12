using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TooltipWindow : MonoBehaviour, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] TextMeshProUGUI tooltipTextUI;
    [SerializeField] TooltipManager tooltipManager;
    [SerializeField] TooltipManager.Tooltip tooltip;

    [Header("Settings")]
    [SerializeField] float tooltipWindowHeight;
    [SerializeField] float tooltipWindowExtraHeight;
	[SerializeField] float tooltipWindowWidth;

	[Header("Settings")]
    [SerializeField] bool showTitle = true;
    [SerializeField] int titleTextSize = 125;
    [SerializeField] int typeTextSize = 110;

    [Header("Locked Borders")]
    [SerializeField] List<Image> lockedBorders;
	public Color defaultBorderColor;
	public Color lockedBorderColor;
	public bool locked;

	public void SetReferenceTooltip(TooltipManager.Tooltip tooltipRef)
	{
		tooltip = tooltipRef;
	}

    public void SetTooltipText(string tooltipText, KeywordHandler keywordHandler, KeywordScriptableObject keyword)
	{
		tooltipTextUI.text = "";
        if (keyword != null && showTitle == true && keyword.keywordName.Length > 0)
		{
			tooltipTextUI.text += "<b><size=" + titleTextSize + "%>" + keyword.keywordName + "<size=100%></b>\n";
            if (keyword.keywordType != KeywordHandler.KeywordType.None)
            {
                tooltipTextUI.text += "<size=" + typeTextSize + "%>" + keyword.keywordType + "<size=100%>\n";
            }
        }
        tooltipTextUI.text += keywordHandler.AddKeywordsToTextFromWordingList(tooltipText, true, keyword);
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
		for (int i = 0; i < lockedBorders.Count; i++) {
			if (locked)
			{
				lockedBorders[i].color = lockedBorderColor;
			}
			else
			{
                lockedBorders[i].color = defaultBorderColor;
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
		tooltipManager.RemoveTooltip(tooltip, true);
	}
}
