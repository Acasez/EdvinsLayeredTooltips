using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipWindow : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI tooltipTextUI;
	public float tooltipWindowHeight;
	public float tooltipWindowExtraHeight;

    [SerializeField] List<Image> lockedBorders;
	public Color defaultBorderColor;
	public Color lockedBorderColor;
	public bool locked;
    public void SetTooltipText(string tooltipText, KeywordHandler keywordHandler)
	{
		keywordHandler.AddKeywordsToTextFromWordingList(tooltipText, tooltipTextUI, true);
		//Debug.Log("Set up tooltip with : " + tooltipText);
		SetTooltipSize();
	}

	void SetTooltipSize()
	{
		tooltipWindowHeight = tooltipTextUI.preferredHeight + tooltipWindowExtraHeight;
		transform.GetComponent<RectTransform>().sizeDelta = new Vector2(600, tooltipWindowHeight);
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
}
