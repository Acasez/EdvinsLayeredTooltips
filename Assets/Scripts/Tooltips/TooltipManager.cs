using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipManager : MonoBehaviour 
{
	[SerializeField] List<Tooltip> tooltips;

	[Header("Keywords")]
	[SerializeField] KeywordHandler keywordHandler;

	[Header("Tooltip Text Settings")]
	/* Might wanna keep these for later adjustments
	[SerializeField] int lineWidth = 200; // Maximum width of the tooltip text
	[SerializeField] int borderSize = 8; // Size of the border around the tooltip
	[SerializeField] int edgeOfScreenMargin = 15; // Margin from the edge of the screen */
	[SerializeField] float tipNudge = 100f;
    [SerializeField] float tooltipSizeDownwardFactor = 0.3f;

	[Serializable]
	public class Tooltip
	{
		public TooltipWindow tooltipWindow;
		public TooltipTrigger trigger;
		public bool active;
		public bool locked;
	}

    public List<KeywordScriptableObject> GetKeywords()
    {
        return keywordHandler.keywords;
    }

    private void Update()
	{
		if (Input.GetMouseButtonDown(2))
		{
			Tooltip topTooltip = GetTopTooltip();
			if (topTooltip != null)
			{
				LockUnlockTooltip(topTooltip);
			}
		}
	}

	public void LockUnlockTooltip(Tooltip tooltip)
	{
		if (tooltip.locked)
		{
			tooltip.locked = false;
			tooltip.tooltipWindow.DisplayLockTooltip(false);
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


	public void SetupTooltipWindow(TooltipTrigger caller, string message, Vector3 location)
	{
		Tooltip thisTooltip = GetFirstAvaibleTooltip();
		if (thisTooltip == null) { return; }

		thisTooltip.active = true;
		thisTooltip.trigger = caller;

		thisTooltip.tooltipWindow.gameObject.SetActive(true);
		thisTooltip.tooltipWindow.SetTooltipText(message, keywordHandler);

		float tipDownDistance = (thisTooltip.tooltipWindow.tooltipWindowHeight * tooltipSizeDownwardFactor) + tipNudge;
		Debug.Log("Tip Down Distance: " + tipDownDistance);
		if (Application.isMobilePlatform)
		{
			thisTooltip.tooltipWindow.transform.position = new Vector2(location.x, location.y + tipDownDistance);
		}
		else
		{
			thisTooltip.tooltipWindow.transform.position = new Vector2(location.x, location.y - tipDownDistance);
		}
        Debug.Log("Tooltip Position: " + thisTooltip.tooltipWindow.transform.position.y + (thisTooltip.tooltipWindow.tooltipWindowHeight * tooltipSizeDownwardFactor)); 
	}

	public void RemoveCallersTooltip(TooltipTrigger caller)
	{
		Tooltip thisTooltip = GetCallersTooltip(caller);

		if (thisTooltip == null) { return; };
		if (thisTooltip.locked)
		{
			Debug.Log("Tooltip locked");
			return;
		}

		thisTooltip.active = false;
		thisTooltip.trigger = null;
		thisTooltip.tooltipWindow.gameObject.SetActive(false);
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
