using UnityEngine;
using System.Collections;
using System;
using System.Text.RegularExpressions;

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
	private MyButton m_goToEmotionButton;

	[SerializeField]
	private UIPanel m_audioPanel;
	[SerializeField]
	private MyButton m_audioButton;
	[SerializeField]
	private UISlider m_audioSlider;

	[SerializeField]
	private UICamera m_emotionCamera;
	[SerializeField]
	private UIPanel m_emotionPanel;
	[SerializeField]
	private MyButton[] m_emotionButtons;
	[SerializeField]
	private MyButton m_backButton;

	[SerializeField]
	private AudioSource m_audioSource;

	int m_currentPage = 1;
	int m_currentChapterStart = 1;

	bool m_isChoosingEmotion = false;

	float m_fadeInDelay = 0.2f;

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

		m_backButton.Unpressing += OnPressBack;

		m_goToEmotionButton.Unpressing += OnClickGoToEmotion;

		m_audioButton.Unpressing += OnPressAudio;

		foreach (MyButton button in m_emotionButtons) 
		{
			button.Unpressing += OnChooseEmotion;
		}

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

	void OnPressAudio(MyButton button)
	{
		if (m_audioSource.isPlaying) 
		{
			StopAudio();
		}
		else if (m_audioSource.clip != null) 
		{
			m_audioSource.Play();
			m_audioProgress = 0;
			StartCoroutine("UpdateAudioBar");
		}
	}

	void StopAudio()
	{
		m_audioSource.Stop();
		StopCoroutine ("UpdateAudioBar");
		m_audioProgress = 0;
		m_audioSlider.value = 0;
	}

	float m_audioProgress = 0;

	IEnumerator UpdateAudioBar()
	{
		m_audioSlider.value = m_audioProgress / m_audioSource.clip.length;
		m_audioProgress += Time.deltaTime;
		yield return null;
		StartCoroutine ("UpdateAudioBar");
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
		ChangeBoth (FindStoryPage ());
	}

	void ChangeBoth(StoryPage page)
	{
		StartCoroutine(ChangeImage (page));
		StartCoroutine(ChangeText (page));
	}

	IEnumerator ChangeText(StoryPage page)
	{
		Debug.Log("page: " + page);

		if (page != null) 
		{
			m_audioSource.clip = page.GetAudio();

			float audioPanelAlpha = m_audioSource.clip != null ? 1 : 0;
			TweenAlpha.Begin(m_audioPanel.gameObject, StoryInfo.fadeDuration, audioPanelAlpha);
			m_audioButton.EnableCollider(Mathf.Approximately(audioPanelAlpha, 1));

			TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 0);
			
			yield return new WaitForSeconds(StoryInfo.fadeDuration + m_fadeInDelay);

			m_textLabel.text = page.GetText ();

			TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 1);
		}

		yield break;
	}

	IEnumerator ChangeImage(StoryPage page)
	{
		if (page != null)
		{
			if (page.HasImages ())
			{
				bool hasFaded = false;

				Texture2D newForegroundTex = page.GetForeground();

				if(m_foreground.mainTexture != newForegroundTex && m_foreground.mainTexture != null)
				{
					hasFaded = true;
					TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 0);
					yield return new WaitForSeconds(StoryInfo.fadeDuration + m_fadeInDelay);
				}

				m_foreground.mainTexture = newForegroundTex;
				m_midground.mainTexture = page.GetMidground ();
				m_background.mainTexture = page.GetBackground ();

				if(hasFaded)
				{
					TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 1);
				}

				string pageNum = Regex.Match(page.gameObject.name, @"\d+").Value;

				try
				{
					m_currentChapterStart = Convert.ToInt32(pageNum);
				}
				catch
				{
					m_currentChapterStart = 1;
				}
			}
		}

		yield break;
	}

	void OnClickGoToEmotion(MyButton button)
	{
		StopAudio ();

		m_currentPage = m_currentChapterStart - 1;
		StartCoroutine(GoEmotionSelect ());
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

	void OnPressBack(MyButton button)
	{
		Debug.Log ("PRESSED BACK");

		--m_currentPage;

		StoryPage page = FindStoryPage ();

		StartCoroutine(ChangeText (page));

		int imagePage = m_currentPage;
		while (!page.HasImages() && imagePage > 0)
		{
			--imagePage;
			page = FindStoryPage(imagePage);
		}

		StartCoroutine(ChangeImage (page));

		StartCoroutine (GoStoryPage ());
	}
	
	void OnChooseEmotion(MyButton button)
	{
		m_emotion = (Emotion)button.GetInt();

		++m_currentPage;

		ChangeBoth (FindStoryPage ());

		StartCoroutine (GoStoryPage ());
	}

	IEnumerator GoStoryPage()
	{
		EmotionCam (false);
		
		TweenAlpha.Begin (m_emotionPanel.gameObject, StoryInfo.fadeDuration, 0);
		
		yield return new WaitForSeconds (StoryInfo.fadeDuration);
		
		TweenAlpha.Begin (m_textPanel.gameObject, StoryInfo.fadeDuration, 1);
		
		yield return new WaitForSeconds (StoryInfo.fadeDuration);
		
		TextCam (true);
	}

	void OnSwipeLeft(TurnSwipeDetect swipeDetect)
	{
		StopAudio ();

		++m_currentPage;
		StartCoroutine (TurnPage ());
	}

	void OnSwipeRight(TurnSwipeDetect swipeDetect)
	{
		StopAudio ();

		--m_currentPage;

		if (m_currentPage < 1) 
		{
			m_currentPage = 1;
		} 
		else 
		{
			StartCoroutine (TurnPage ());
		}
	}

	IEnumerator TurnPage()
	{
		StoryPage page = FindStoryPage ();

		if(page == null)
		{
			StartCoroutine (GoEmotionSelect ());
		}
		else 
		{
			ChangeBoth(page);
		}

		yield break;
	}

	StoryPage FindStoryPage(int index = -1)
	{
		if (index == -1) 
		{
			index = m_currentPage;
		}

		string goName = String.Format ("{0}_{1}_{2}", StoryInfo.Instance.GetTitle (), index, m_emotion.ToString ()).ToLower ();
		Debug.Log ("Finding: " + goName);
		GameObject go = Resources.Load<GameObject>(goName);

		if (go == null) 
		{
			go = Resources.Load<GameObject>(String.Format ("{0}_{1}", StoryInfo.Instance.GetTitle (), index).ToLower());
		}

		return go != null ? (StoryPage)go.GetComponent<StoryPage>() : null;
	}

	void OnGUI()
	{
		GUILayout.Label ("Page: " + m_currentPage);
		GUILayout.Label ("ChapterStart: " + m_currentChapterStart);
	}

	/*
	IEnumerator ChangePage(StoryPage page)
	{
		Debug.Log ("ChangePage(): " + page);
		if (page != null) 
		{
			if (page.HasImages ()) 
			{
				if(m_foreground.mainTexture != null)
				{
					TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 0);
				}

				m_currentChapterStart = m_currentPage;
			}
				
			TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 0);

			yield return new WaitForSeconds(StoryInfo.fadeDuration + 0.2f);

			m_textLabel.text = page.GetText ();

			if(page.HasImages())
			{
				m_foreground.mainTexture = page.GetForeground ();
				m_midground.mainTexture = page.GetMidground ();
				m_background.mainTexture = page.GetBackground ();
			}

			TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 1);
			TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 1);
		}

		yield break;
	}
	*/
}
