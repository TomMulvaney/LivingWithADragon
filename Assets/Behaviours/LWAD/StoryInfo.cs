using UnityEngine;
using System.Collections;

public class StoryInfo : Singleton<StoryInfo> 
{
	string m_title = "Dragon";

	public void SetTitle(string title)
	{
		m_title = title;
	}

	public string GetTitle()
	{
		return m_title;
	}

	public static float fadeDuration
	{
		get
		{
			return 0.25f;
		}
	}
}
