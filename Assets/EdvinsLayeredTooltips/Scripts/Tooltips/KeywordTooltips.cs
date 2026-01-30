using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
public class KeywordTooltips : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    [Header("Values")]
    private int lastLinkIndex = -1;

    [Header("Local References")]
    [SerializeField] TextMeshProUGUI textMeshPro;
    [SerializeField] TooltipTrigger trigger;

    TooltipManager tooltipManager;

    [Tooltip("Only if you want this the text here to a specific TooltipStyle")]
    [SerializeField] TooltipWindowStyle specificTooltipStyle;

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

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMeshPro, trigger.GetMousePosition(), null);
        if (linkIndex != -1 && linkIndex != lastLinkIndex)
        {
            lastLinkIndex = linkIndex;
            TMP_LinkInfo linkInfo = textMeshPro.textInfo.linkInfo[linkIndex];
            string linkedWord = linkInfo.GetLinkID();
            Debug.Log($"Hovered keyword: {linkedWord}");
            if (linkedWord != null)
            {
                KeywordScriptableObject keyword = GetKeyword(linkedWord);
                if (keyword != null)
                {
                    trigger.CreateTooltip(keyword.tooltipText, trigger.GetMousePosition(), keyword);
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

    private KeywordScriptableObject GetKeyword(string linkedWord)
    {
        List<KeywordScriptableObject> keywords = tooltipManager.GetKeywords();

        for (int i = 0; i < keywords.Count; i++)
        {
            if (keywords[i].tooltipText != null)
            {
                if (keywords[i].keywordName == linkedWord)
                {
                    return keywords[i];
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
