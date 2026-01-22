using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class KeywordHandler : MonoBehaviour
{
    public enum HighlightStyleEnum
    {
        Underline,
        Bold,
        Italic,
        UnderlineBold,
        CK3Style,
        BG3Style
    }
    public enum KeywordType
    {
        None,
        Action,
        Resource,
        Mechanic,
        Condition,
        CardType,
        Location
    }

    [Header("Lists")]
    [Tooltip("The keywords used in the the game. Needs to be updated when you add new keywords.")]
    [SerializeField] public List<KeywordScriptableObject> keywords;
    [SerializeField] public List<Wording> allWordings;
    [SerializeField] public List<Wording> activeKeywords;

    TooltipWindowStyle tooltipWindowStyle;

    [Header("References")]
    [SerializeField] TooltipManager tooltipManager;

    [Serializable]
    public class Wording
    {
		public string wording;
		public KeywordScriptableObject keyword;
    }

    private void Start()
    {
        MakeWordingList();
        tooltipManager = FindAnyObjectByType<TooltipManager>();
        if (tooltipManager == null)
        {
            Debug.LogWarning("No tooltipManager!");
        }
        tooltipWindowStyle = tooltipManager.GetTooltipDecoration();
    }

    public void MakeWordingList()
	{
		allWordings.Clear();
		for (int keywordIndex = 0; keywordIndex < keywords.Count; keywordIndex++)
		{
			KeywordScriptableObject currentKeyword = keywords[keywordIndex];
			for (int wordIndex = 0; wordIndex < currentKeyword.wordings.Count; wordIndex++)
			{
				Wording newWording = new Wording();
				newWording.wording = currentKeyword.wordings[wordIndex];
				newWording.keyword = currentKeyword;
				allWordings.Add(newWording);
            }
		}

        // Sort allWordings by the number of spaces in the wording (descending). Using 0 for null wording strings.
        allWordings = allWordings.OrderByDescending(w => (w?.wording?.Count(c => c == ' ')) ?? 0).ToList();
    }

    public string AddKeywordsToText(string text, bool createTooltips, KeywordScriptableObject keyword)
    {
        if (allWordings.Count == 0)
        {
            Debug.LogWarning("No wordings, you might have missed to add them");
            return text;
        }
        List<KeywordScriptableObject> activeKeywords = new List<KeywordScriptableObject>();
        if (keyword != null)
        {
            activeKeywords.Add(keyword);
        }

        for (int wordingIndex = 0; wordingIndex < allWordings.Count; wordingIndex++)
        {
            Wording currentWording = allWordings[wordingIndex];
            //Debug.Log("Current wording " + currentWording.wording + " with keyword " + currentWording.keyword.keywordName);
            if (activeKeywords.Contains(currentWording.keyword))
            {
                Debug.Log("Keyword already in tooltip");
                continue;
            }
            //Split Up text into segments, ingore the parts in <> brackets

            //Debug.Log("Current Text is " + text);
            var segments = SplitTextIntoSegments(text); //Split up text into segments split by <> brackets
            var processedSegments = new List<string>();

            //Go through those parts and pattern match
            foreach (var (segment, isInBrackets) in segments)
            {
                string currentSegment = segment;
                //Debug.Log("Prossecing segment: " + currentSegment);
                if (string.IsNullOrWhiteSpace(currentSegment))
                {
                    //Debug.Log("Segment: " + currentSegment + " is empty, ignoring");
                    continue;
                }
                if (isInBrackets)
                {
                    //Debug.Log("Segment: " + currentSegment + " is already in <> brackets, ignoring");
                    processedSegments.Add(currentSegment); //Add part to bracket, skip checking it
                    continue;
                }

                string pattern = $@"(?:-\d+\s+)?\b({Regex.Escape(currentWording.wording)})\b";
                var match = Regex.Match(currentSegment, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string matchedText = match.Groups[1].Value;
                    string linkId = currentWording.keyword.keywordName;
                    activeKeywords.Add(currentWording.keyword);

                    // Check if the matched text is preceded by a negative number
                    bool isNegative = match.Index > 0 && currentSegment[match.Index - 1] == '-' && char.IsDigit(currentSegment[match.Index - 2]);

                    string icon = "";
                    if (isNegative && !string.IsNullOrWhiteSpace(currentWording.keyword.negativeIcon))
                    {
                        icon = "<sprite name=\"" + currentWording.keyword.negativeIcon + "\">";
                    }
                    else if (!string.IsNullOrWhiteSpace(currentWording.keyword.icon))
                    {
                        icon = "<sprite name=\"" + currentWording.keyword.icon + "\">";
                    }

                    if (createTooltips)
                    {
                        currentSegment = currentSegment[..match.Index] + "<link=\"" + linkId + "\">" + tooltipWindowStyle.openingText + matchedText + tooltipWindowStyle.closingText + "</link>" + icon + currentSegment[(match.Index + match.Length)..];
                    }
                    else
                    {
                        currentSegment = currentSegment[..match.Index] + tooltipWindowStyle.openingText + matchedText + tooltipWindowStyle.closingText + icon + currentSegment[(match.Index + match.Length)..];
                    }
                }
                processedSegments.Add(currentSegment);
            }
            text = string.Concat(processedSegments);
            //Debug.Log("Text is now " + text + " from segments");
        }

        return text;
    }

    public List<(string text, bool isInBrackets)> SplitTextIntoSegments(string text)
    {
        var segments = new List<(string, bool)>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return segments;
        }

        int start = 0;
        bool inBrackets = false;
        var stack = new Stack<string>(); // Tracks the type of opening tag

        for (int i = 0; i < text.Length; i++)
        {
            // Check for the current highlight style's opening tag
            if (text.Substring(i).StartsWith(tooltipWindowStyle.openingText))
            {
                // Push current segment if not empty
                if (i > start)
                {
                    segments.Add((text.Substring(start, i - start), inBrackets));
                }
                stack.Push(tooltipWindowStyle.openingText);
                start = i;
                inBrackets = true;
                i += tooltipWindowStyle.openingText.Length - 1; // Skip past openingText
            }
            else if (text.Substring(i).StartsWith(tooltipWindowStyle.closingText) && stack.Count > 0 && stack.Peek() == tooltipWindowStyle.openingText)
            {
                stack.Pop();
                segments.Add((text.Substring(start, i - start + tooltipWindowStyle.closingText.Length), true));
                start = i + tooltipWindowStyle.closingText.Length;
                inBrackets = (stack.Count > 0);
                i += tooltipWindowStyle.closingText.Length - 1; // Skip past closingText
            }
            // Handle generic < and > tags 
            else if (text[i] == '<' && !text.Substring(i).StartsWith(tooltipWindowStyle.openingText))
            {
                if (i > start)
                {
                    segments.Add((text.Substring(start, i - start), inBrackets));
                }
                stack.Push("<");
                start = i;
                inBrackets = true;
            }
            else if (text[i] == '>' && stack.Count > 0 && stack.Peek() == "<")
            {
                stack.Pop();
                segments.Add((text.Substring(start, i - start + 1), true));
                start = i + 1;
                inBrackets = (stack.Count > 0);
            }
        }

        // Add the remaining text
        if (start < text.Length)
        {
            segments.Add((text.Substring(start), inBrackets));
        }

        return segments;
    }
}
