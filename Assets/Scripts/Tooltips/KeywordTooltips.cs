using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeywordTooltips : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [Header("Values")]
    private int lastLinkIndex = -1;

    [Header("Local References")]
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] TooltipTrigger trigger;

    [Header("References")]
    [SerializeField] TooltipManager tooltipManager;

    private void Start()
    {
        tooltipManager = FindAnyObjectByType<TooltipManager>();
        if (tooltipManager == null)
        {
            Debug.LogWarning("No tooltipManager!");
        }
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (trigger.HasToolTip())
        {
            return;
        }

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, Input.mousePosition, null);
        if (linkIndex != -1 && linkIndex != lastLinkIndex)
        {
            lastLinkIndex = linkIndex;
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkedWord = linkInfo.GetLinkID();
            Debug.Log($"Hovered keyword: {linkedWord}");
            if (linkedWord != null)
            {
                string description = GetKeywordDescription(linkedWord);
                if (!string.IsNullOrEmpty(description))
                {
                    trigger.CreateTooltip(description, Input.mousePosition);
                }
            }
        }
        else if (linkIndex == -1 && lastLinkIndex != -1 && !trigger.IsPointerOverTooltip())
        {
            lastLinkIndex = -1;
            trigger.DestroyPreviousTooltip();
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        if (!trigger.IsPointerOverTooltip())
        {
            trigger.DestroyPreviousTooltip();
            lastLinkIndex = -1;
        }
    }

    private string GetKeywordDescription(string linkedWord)
	{
        List<KeywordScriptableObject> keywords = tooltipManager.GetKeywords();

        for (int i = 0; i < keywords.Count; i++)
		{
			if (keywords[i].tooltipText != null)
			{
				if (keywords[i].keywordName == linkedWord)
				{
					return keywords[i].tooltipText;
				}
			}
			else
			{
				Debug.Log($"Keyword: " + keywords[i] + " has no tooltipText");
			}
		}
		Debug.Log($"Didn't find a match for : " + linkedWord);
		return null;
	}
}
