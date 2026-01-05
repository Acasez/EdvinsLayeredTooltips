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

	[SerializeField] Vector2 mousePos;


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
		StopAllCoroutines();
		DestroyPreviousTooltip();
	}

	public void DestroyPreviousTooltip()
	{
		tooltipManager.RemoveCallersTooltip(this);
	}

	private IEnumerator StartTimer()
	{
		yield return new WaitForSeconds(0.5f);
		DestroyPreviousTooltip();
		CreateTooltip(null, Input.mousePosition);
	}

	public void CreateTooltip(string message, Vector3 location)
	{
		mousePos = Input.mousePosition;
		if (message == null)
		{
			message = tooltipText;
		}

		tooltipManager.SetupTooltipWindow(this, message, location);
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
