using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeywordTooltips : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
	public TextMeshProUGUI textMeshPro;
	public TooltipTrigger trigger;
	private int lastLinkIndex = -1;

	[SerializeField] public List<KeywordScriptableObject> keywords;

	void Awake()
	{
		textMeshPro = GetComponent<TextMeshProUGUI>();
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
		else if (linkIndex == -1 && lastLinkIndex != -1)
		{
			lastLinkIndex = -1;
			trigger.DestroyPreviousTooltip();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		trigger.DestroyPreviousTooltip();
		lastLinkIndex = -1;
	}

	private string GetKeywordDescription(string linkedWord)
	{
		for (int i = 0; i < keywords.Count; i++)
		{
			if (keywords[i].tooltipText != null)
			{
				if (keywords[i].keyword == linkedWord)
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
