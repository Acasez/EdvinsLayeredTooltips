using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.FilePathAttribute;

public class TooltipManager : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] List<Tooltip> tooltips;
	[SerializeField] bool mobile;

    [Header("Canvas")]
    [SerializeField] Canvas canvas;
	[SerializeField] RectTransform canvasRectTransform;

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

    private void Start()
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
			RemoveTooltip(tooltip);	
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


    public void SetupTooltipWindow(TooltipTrigger caller, string message, Vector3 screenPosition)
    {
        Tooltip thisTooltip = GetFirstAvaibleTooltip();
        if (thisTooltip == null) { return; }

        thisTooltip.active = true;
        thisTooltip.trigger = caller;

        thisTooltip.tooltipWindow.gameObject.SetActive(true);
        thisTooltip.tooltipWindow.SetTooltipText(message, keywordHandler);

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
        Vector2 desiredPosition = new Vector2(localPosition.x, localPosition.y - tipDownDistance);

        // Get canvas bounds (already in local space)
        Vector2 canvasMin = canvasRectTransform.rect.min;
        Vector2 canvasMax = canvasRectTransform.rect.max;

        // Calculate min/max allowed positions (accounting for tooltip size and pivot)
        float minX = canvasMin.x + tooltipWidth * tooltipRect.pivot.x;
        float maxX = canvasMax.x - tooltipWidth * (1 - tooltipRect.pivot.x);
        float minY = canvasMin.y + tooltipHeight * tooltipRect.pivot.y;
        float maxY = canvasMax.y - tooltipHeight * (1 - tooltipRect.pivot.y);

        // Clamp
        float clampedX = Mathf.Clamp(desiredPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(desiredPosition.y, minY, maxY);
		/*
        Debug.Log("Screen Position: " + screenPosition);
        Debug.Log("Local Position: " + localPosition);
        Debug.Log("Desired Position: " + desiredPosition);
        Debug.Log("Clamped Position: " + new Vector2(clampedX, clampedY));
		*/
        return new Vector2(clampedX, clampedY);
    }



    public void RemoveCallersTooltip(TooltipTrigger caller)
	{
		Tooltip thisTooltip = GetCallersTooltip(caller);

		RemoveTooltip(thisTooltip);
	}

	public void RemoveTooltip(Tooltip thisTooltip)
	{
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
