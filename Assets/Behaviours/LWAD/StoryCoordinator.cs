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
	private UIGrid m_pageCountGrid;
	[SerializeField]
	private UISprite[] m_pageCounters;

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

	[SerializeField]
	private bool m_imageScaleTweens;
	[SerializeField]
	private bool m_textScaleTweens;

	[SerializeField]
	private GameObject m_transitionScreen;

	int m_currentPage = 1;
	int m_currentChapterStart = 1;
	int m_nextEmotionChoice;

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

	void PlayFun()
	{
		Debug.Log ("PlayFun()");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_SCARY_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_CUTE_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_FUN");
	}

	void PlayScary()
	{
		Debug.Log ("PlayScary()");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_FUN_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_CUTE_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_SCARY");
	}

	void PlayCute()
	{
		Debug.Log ("PlayCute()");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_SCARY_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_FUN_STOP");
		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_CUTE");
	}

#if UNITY_EDITOR
	void Update()
	{
		if (Input.GetKeyDown (KeyCode.F)) 
		{
			PlayFun();
		}

		if (Input.GetKeyDown (KeyCode.S)) 
		{
			PlayScary();
		}

		if (Input.GetKeyDown (KeyCode.C)) 
		{
			PlayCute();
		}
	}
#endif

	void Awake()
	{
		m_pageCountGrid.Reposition ();

		//m_transitionScreen.SetActive (true);

		m_goToEmotionButton.Off (true);

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

	IEnumerator Start()
	{
		yield return StartCoroutine (StoryInfo.WaitForInstance());
		
		//Debug.Log (String.Format("{0}_{1}_{2}", StoryInfo.Instance.GetTitle(), m_currentPage, m_emotion.ToString()));
		
		PlayCute ();
		
		StartCoroutine(ChangeBoth (FindStoryPage ()));

		yield return new WaitForSeconds(0.75f);

		Hashtable tweenArgs = new Hashtable ();
		//tweenArgs.Add ("amount", new Vector3(20, 0, 0));
		tweenArgs.Add ("position", new Vector3 (2200, 0, 0));
		tweenArgs.Add ("islocal", true);
		tweenArgs.Add ("time", 1.2f);

		iTween.MoveTo (m_transitionScreen, tweenArgs);
		//iTween.MoveBy (m_transitionScreen, tweenArgs);
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

	void RefreshPageCounters()
	{
		SetNextEmotionChoice (); 

		Array.Sort (m_pageCounters, CollectionHelpers.CompareLocalPosX);

		int numPageCounters = m_nextEmotionChoice - m_currentChapterStart;
		int numCompleted = m_currentPage - m_currentChapterStart + 1;

		Debug.Log ("numPageCounters: " + numPageCounters);
		Debug.Log ("numCompleted: " + numCompleted);

		for (int i = 0; i < m_pageCounters.Length; ++i) 
		{
			m_pageCounters[i].gameObject.SetActive(i < numPageCounters);
			m_pageCounters[i].spriteName = i < numCompleted ? "button_navigation_selected_24" : "button_navigation_nonselected_23";
		}

		float gridLocalPosX = -numPageCounters * m_pageCountGrid.cellWidth / 2;
		if (numPageCounters % 2 == 0) 
		{
			gridLocalPosX += m_pageCountGrid.cellWidth / 2;
		}


		m_pageCountGrid.transform.localPosition = new Vector3 (gridLocalPosX, m_pageCountGrid.transform.localPosition.y, m_pageCountGrid.transform.localPosition.z);

		m_pageCountGrid.Reposition ();
	}

	IEnumerator ChangeBoth(StoryPage page)
	{
		StartCoroutine(ChangeText (page));
		yield return StartCoroutine(ChangeImage (page));
	}

	IEnumerator ChangeText(StoryPage page)
	{
		Debug.Log("page: " + page);

		if (page != null) 
		{
			m_audioSource.clip = page.GetAudio();

			bool enableAudioButton = m_audioSource.clip != null;

			m_audioButton.EnableCollider(enableAudioButton);

			if(m_textScaleTweens)
			{
				Vector3 audioButtonScale = enableAudioButton ? Vector3.one : Vector3.zero;

				if(!enableAudioButton)
				{
					TweenScale.Begin(m_audioButton.gameObject, StoryInfo.scaleDuration, audioButtonScale);
				}

				TweenScale.Begin(m_textLabel.gameObject, StoryInfo.scaleDuration, Vector3.zero);

				yield return new WaitForSeconds(StoryInfo.scaleDuration + m_fadeInDelay);

				if(enableAudioButton)
				{
					TweenScale.Begin(m_audioButton.gameObject, StoryInfo.scaleDuration, audioButtonScale);
				}
			}
			else
			{
				float audioPanelAlpha = enableAudioButton ? 1 : 0;

				if(!enableAudioButton)
				{
					TweenAlpha.Begin(m_audioPanel.gameObject, StoryInfo.fadeDuration, audioPanelAlpha);
				}
				
				TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 0);

				yield return new WaitForSeconds(StoryInfo.fadeDuration + m_fadeInDelay);

				if(enableAudioButton)
				{
					TweenAlpha.Begin(m_audioPanel.gameObject, StoryInfo.fadeDuration, audioPanelAlpha);
				}
			}
			
			m_textLabel.text = page.GetText ();

			if(m_textScaleTweens)
			{
				TweenScale.Begin(m_textLabel.gameObject, StoryInfo.fadeDuration, Vector3.one);
			}
			else
			{
				TweenAlpha.Begin(m_textPanel.gameObject, StoryInfo.fadeDuration, 1);
			}
		}

		yield break;
	}

	void SetNextEmotionChoice()
	{
		int nextEmotionChoice = m_currentPage + 1;
		while (FindStoryPage(nextEmotionChoice) != null) 
		{
			++nextEmotionChoice;
		}
		
		m_nextEmotionChoice = nextEmotionChoice;
	}

	void ScaleImages(Vector3 scale)
	{
		TweenScale.Begin (m_foreground.gameObject, StoryInfo.scaleDuration, scale);
		TweenScale.Begin (m_midground.gameObject, StoryInfo.scaleDuration, scale);
		TweenScale.Begin (m_background.gameObject, StoryInfo.scaleDuration, scale);
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

					if(m_imageScaleTweens)
					{
						ScaleImages(Vector3.zero);
						yield return new WaitForSeconds(StoryInfo.scaleDuration + m_fadeInDelay);
					}
					else
					{
						TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 0);
						yield return new WaitForSeconds(StoryInfo.fadeDuration + m_fadeInDelay);
					}
				}

				m_foreground.mainTexture = newForegroundTex;
				m_midground.mainTexture = page.GetMidground ();
				m_background.mainTexture = page.GetBackground ();

				if(hasFaded)
				{
					if(m_imageScaleTweens)
					{
						ScaleImages(Vector3.one);
					}
					else
					{
						TweenAlpha.Begin(m_imagePanel.gameObject, StoryInfo.fadeDuration, 1);
					}
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

		RefreshPageCounters();

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
		TweenAlpha.Begin(m_audioPanel.gameObject, StoryInfo.fadeDuration, 0);
		m_goToEmotionButton.Off (false);

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

		StartCoroutine (OnPressBackCo ());
	}

	bool m_hasEndedChapter;
	int m_firstChapterEnd;

	IEnumerator OnPressBackCo()
	{
		if (m_currentPage == m_firstChapterEnd) 
		{
			m_emotion = Emotion.Fun;
			PlayFun();
		}

		--m_currentPage;
		
		StoryPage page = FindStoryPage ();
		
		StartCoroutine(ChangeText (page));
		
		int imagePage = m_currentPage;
		while (!page.HasImages() && imagePage > 0)
		{
			--imagePage;
			page = FindStoryPage(imagePage);
		}
		
		yield return StartCoroutine(ChangeImage (page));
		
		StartCoroutine (GoStoryPage ());
	}
	
	void OnChooseEmotion(MyButton button)
	{
		m_emotion = (Emotion)button.GetInt();

		switch (m_emotion)
		{
		case Emotion.Fun:
			PlayFun();
			break;
		case Emotion.Scary:
			PlayScary();
			break;
		case Emotion.Cute:
			PlayCute();
			break;
		}

		StartCoroutine (OnChooseEmotionCo ());
	}

	IEnumerator OnChooseEmotionCo()
	{
		++m_currentPage;

		TweenAlpha.Begin (m_emotionPanel.gameObject, StoryInfo.fadeDuration, 0);
		
		//yield return new WaitForSeconds (StoryInfo.fadeDuration);
		
		yield return StartCoroutine(ChangeBoth (FindStoryPage ()));
		
		StartCoroutine (GoStoryPage ());
	}

	IEnumerator GoStoryPage()
	{
		if (m_currentChapterStart > 1) 
		{
			m_goToEmotionButton.On ();
		}

		EmotionCam (false);
		
		TweenAlpha.Begin (m_emotionPanel.gameObject, StoryInfo.fadeDuration, 0);
		
		yield return new WaitForSeconds (StoryInfo.fadeDuration);
		
		//TweenAlpha.Begin (m_textPanel.gameObject, StoryInfo.fadeDuration, 1);
		
		//yield return new WaitForSeconds (StoryInfo.fadeDuration);
		
		TextCam (true);
	}

	void OnSwipeLeft(TurnSwipeDetect swipeDetect)
	{
		StopAudio ();

		if (!IsLastPage ()) 
		{
			++m_currentPage;
			StartCoroutine (TurnPage ());
		}
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
			if(!m_hasEndedChapter)
			{
				m_hasEndedChapter = true;
				m_firstChapterEnd = m_currentPage;
			}
			StartCoroutine (GoEmotionSelect ());
		}
		else 
		{
			StartCoroutine(ChangeBoth(page));
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

	bool IsLastPage()
	{
		return FindStoryPage(m_currentPage + 1) == null && FindStoryPage(m_currentPage + 2) == null;
	}


	void OnGUI()
	{
		GUILayout.Label ("Page: " + m_currentPage);
		GUILayout.Label ("ChapterStart: " + m_currentChapterStart);
		GUILayout.Label ("NextEmotionChoice: " + m_nextEmotionChoice);
	}

}
