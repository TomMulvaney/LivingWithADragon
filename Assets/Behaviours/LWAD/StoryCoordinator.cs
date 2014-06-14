using UnityEngine;
using System.Collections;

public class StoryCoordinator : Singleton<StoryCoordinator>
{
	[SerializeField]
	private UIPanel m_imagePanel;
	[SerializeField]
	private UITexture m_foreground;
	[SerializeField]
	private UITexture m_midground;
	[SerializeField]
	private UITexture m_background;
	[SerializeField]
	private UIPanel m_textPanel;
	[SerializeField]
	private UILabel m_texLabel;
	[SerializeField]
	private UIPanel m_buttonPanel;
	[SerializeField]
	private AudioSource m_audioSource;

	int m_currentPage = 0;

	void Start()
	{
	}
}
