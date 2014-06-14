using UnityEngine;
using System.Collections;
using System;

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
	private UICamera m_textCamera;
	[SerializeField]
	private UIPanel m_textPanel;
	[SerializeField]
	private UILabel m_textLabel;
	[SerializeField]
	private TurnSwipeDetect m_swipeDetect;

	[SerializeField]
	private UICamera m_emotionCamera;
	[SerializeField]
	private UIPanel m_emotionPanel;
	[SerializeField]
	private MyButton[] m_emotionButtons;

	[SerializeField]
	private AudioSource m_audioSource;

	int m_currentPage = 1;
	int m_currentChapterStart = 1;

	bool m_isChoosingEmotion = false;

	enum Emotion
	{
		Fun,
		Scary,
		Cute
	}

	Emotion m_emotion = Emotion.Fun;

#if UNITY_EDITOR
	[SerializeField]
	private bool m_startOnEmotions;
#endif

	void Awake()
	{
		m_swipeDetect.SwipedLeft += OnSwipeLeft;
		m_swipeDetect.SwipedRight += OnSwipeRight;

		m_emotionPanel.alpha = 0;
		EmotionCam (false);

		m_textPanel.alpha = 1;
		TextCam (true);

#if UNITY_EDITOR
		if(m_startOnEmotions)
		{
			m_emotionPanel.alpha = 1;
			EmotionCam (true);
			
			m_textPanel.alpha = 0;
			TextCam (false);
		}
#endif
	}

	void EmotionCam(bool enable)
	{
		m_emotionCamera.enabled = enable;
	}

	void TextCam(bool enable)
	{
		m_textCamera.enabled = enable;
	}

	IEnumerator Start()
	{
		yield return StartCoroutine (StoryInfo.WaitForInstance());

		//Debug.Log (String.Format("{0}_{1}_{2}", StoryInfo.Instance.GetTitle(), m_currentPage, m_emotion.ToString()));
		ChangePage (FindStoryPage ());
	}

	void ChangePage(StoryPage page)
	{
		Debug.Log ("ChangePage(): " + page);
		if (page != null) 
		{
			if (page.HasImages ()) 
			{
				m_foreground.mainTexture = page.GetForeground ();
				m_midground.mainTexture = page.GetMidground ();
				m_background.mainTexture = page.GetBackground ();

				m_currentChapterStart = m_currentPage;
			}
				
			m_textLabel.text = page.GetText ();
		}
	}

	IEnumerator GoEmotionSelect()
	{
		TextCam (false);
		TweenAlpha.Begin (m_textPanel.gameObject, StoryInfo.fadeDuration, 0);

		yield return new WaitForSeconds (StoryInfo.fadeDuration);

		TweenAlpha.Begin (m_emotionPanel.gameObject, StoryInfo.fadeDuration, 1);

		yield return new WaitForSeconds (StoryInfo.fadeDuration);

		EmotionCam (true);
	}
	
	void OnChooseEmotion(MyButton button)
	{
		m_emotion = (Emotion)button.GetInt();
	}

	void OnSwipeLeft(TurnSwipeDetect swipeDetect)
	{
		--m_currentPage;
		StartCoroutine (TurnPage ());
	}

	void OnSwipeRight(TurnSwipeDetect swipeDetect)
	{
		++m_currentPage;
		StartCoroutine (TurnPage ());
	}

	IEnumerator TurnPage()
	{
		StoryPage page = FindStoryPage ();

		if (page != null) 
		{
			if (page.IsEmpty ()) 
			{
				++m_currentPage;
				StartCoroutine (GoEmotionSelect ());
			} 
			else 
			{
				ChangePage (page);
			}
		}

		yield break;
	}

	StoryPage FindStoryPage()
	{
		string goName = String.Format ("{0}_{1}_{2}", StoryInfo.Instance.GetTitle (), m_currentPage, m_emotion.ToString ()).ToLower ();
		Debug.Log ("Finding: " + goName);
		GameObject go = Resources.Load<GameObject>(goName);
		return go != null ? (StoryPage)go.GetComponent<StoryPage>() : null;
	}
}
