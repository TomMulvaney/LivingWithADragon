using UnityEngine;
using System.Collections;

public class StoryPage : MonoBehaviour 
{
	// Images are kept in separate variables instead of an array so that the people creating the prefabs are less likely to make mistakes
	[SerializeField]
	private Texture2D m_foreground;
	[SerializeField]
	private Texture2D m_midground;
	[SerializeField]
	private Texture2D m_background;
	[SerializeField]
	private AudioClip m_audio;
	[SerializeField]
	private string m_text;
	[SerializeField]
	private bool m_isLastPage;

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

	public AudioClip GetAudio()
	{
		return m_audio;
	}

	public string GetText()
	{
		return m_text;
	}

	public bool IsEmpty()
	{
		return !HasImages() && m_audio == null && System.String.IsNullOrEmpty(m_text);
	}

	public bool HasImages()
	{
		return m_foreground != null && m_midground != null && m_background != null;
	}

	public bool IsLastPage()
	{
		return m_isLastPage;
	}
}
