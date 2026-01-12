using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public enum TooltipType
	{
		UI,
		Keyword, //Add more types if you want
	}

	[SerializeField] TooltipManager tooltipManager;
	[SerializeField] string tooltipText;
	[SerializeField] TooltipType tooltipType;
    [SerializeField] TooltipWindow tooltipWindow;

    private void Start()
	{
		tooltipManager = FindAnyObjectByType<TooltipManager>();
		if (tooltipManager == null)
		{
			Debug.LogWarning("No tooltipManager!");
		}
	}
	public void OnPointerEnter(PointerEventData eventData)
	{
		if (tooltipType == TooltipType.Keyword)
		{
			return; //Triggered by the KeywordTooltip script
		}
		if (tooltipManager.GetCallersTooltip(this) != null)
		{
			return; // Don't create a tooltip if this already
		}
		StopAllCoroutines();
		StartCoroutine(StartTimer());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
        if (IsPointerOverTooltip())
        {
            return;
        }

        StopAllCoroutines();
		DestroyPreviousTooltip();
	}

	public void DestroyPreviousTooltip()
	{
		tooltipManager.RemoveCallersTooltip(this);
	}

    public Vector3 GetMousePosition()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.mousePosition;
#else
    return Mouse.current.position.ReadValue();
#endif
    }

    private IEnumerator StartTimer()
	{
		yield return new WaitForSeconds(0.5f);
		DestroyPreviousTooltip();
		CreateTooltip(null, GetMousePosition(), null);
	}

	public bool IsPointerOverTooltip()
	{
        // Get the tooltip's RectTransform
        TooltipWindow tooltipWindow = tooltipManager.GetCallersTooltip(this)?.tooltipWindow;
        if (tooltipWindow == null)
        {
            return false; // No tooltip exists
        }

        RectTransform tooltipRect = tooltipWindow.GetComponent<RectTransform>();
        if (tooltipRect == null)
        {
            return false; // No RectTransform found
        }

        // Convert mouse position to screen point
        Vector2 mouseScreenPos = GetMousePosition();

        // Check if the mouse is over the tooltip
        return RectTransformUtility.RectangleContainsScreenPoint(tooltipRect, mouseScreenPos);
    }

    public void CreateTooltip(string message, Vector3 location, KeywordScriptableObject keyword)
    {
        if (message == null)
        {
            message = tooltipText;
        }
        tooltipManager.SetupTooltipWindow(this, message, location, keyword);
    }

    public bool HasToolTip()
	{
		if (tooltipManager != null)
		{
			if (tooltipManager.GetCallersTooltip(this) != null)
			{
				return true;
			}
		}
		else
		{
			Debug.Log("Tooltip Manager not set");
		}

		return false;
	}
}
