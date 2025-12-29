using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TooltipWindow : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI tooltipTextUI;
	public float tooltipWindowHeight;
	public float tooltipWindowExtraHeight;

	[SerializeField] GameObject lockedBorders;
	public void SetTooltipText(string tooltipText, KeywordHandler keywordHandler)
	{
		keywordHandler.AddKeywordsToText(tooltipText, tooltipTextUI, true);
		Debug.Log("Set up tooltip with : " + tooltipText);
		SetTooltipSize();
	}

	void SetTooltipSize()
	{
		tooltipWindowHeight = tooltipTextUI.preferredHeight + tooltipWindowExtraHeight;
		transform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, tooltipWindowHeight);
	}

	public void DisplayLockTooltip(bool locked)
	{
		lockedBorders.SetActive(locked);
	}
}
