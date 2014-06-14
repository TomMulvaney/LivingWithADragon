using UnityEngine;
using System.Collections;

public class StoryPage : MonoBehaviour 
{
	[SerializeField]
	private Texture2D m_foreground;
	[SerializeField]
	private Texture2D m_midground;
	[SerializeField]
	private Texture2D m_background;
	[SerializeField]
	private string m_text;

	public Texture2D GetForeground()
	{
		return m_foreground;
	}

	public Texture2D GetMidground()
	{
		return m_midground;
	}

	public Texture2D GetBackground()
	{
		return m_background;
	}

	public string GetText()
	{
		return m_text;
	}
}
