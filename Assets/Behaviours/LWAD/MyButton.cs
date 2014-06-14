using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyButton : MonoBehaviour 
{
	public delegate void PressEventHandler (MyButton button);
    public event PressEventHandler Pressing;
    public event PressEventHandler Unpressing;
    public event PressEventHandler Unpressed;
    
    
    // Shared Variables
    [SerializeField]
    private bool m_autoReset = true;
    [SerializeField]
    private int m_int;
    [SerializeField]
    private string m_string;
    [SerializeField]
    private UISprite m_pressableButton;
    [SerializeField]
    private float m_pressDuration = 0.3f;
    [SerializeField]
    private List<string> m_pressedAudio = new List<string>();
    [SerializeField]
    private List<string> m_unpressedAudio = new List<string>();
    [SerializeField]
    private UIWidget[] m_additionalColoredWidgets;
    [SerializeField]
    private SimpleSpriteAnim m_sheenAnimation;
    [SerializeField]
    private SimpleSpriteAnim m_pressAnimation;

    bool m_isTransitioning;

    
    // PositionChange Variables and Method
    [SerializeField]
    private bool m_changePosition;
    [SerializeField]
    private Transform m_pressedLocation;
    [SerializeField]
    private Transform m_positionFollower;
    [SerializeField]
    private Transform m_followerLocation;
    [SerializeField]
    private GameObject m_emptyPrefab;
    [SerializeField]
    private iTween.EaseType m_positionEaseType = iTween.EaseType.linear;
    
    Transform m_unpressedLocation;
    Hashtable m_positionTweenArgs = new Hashtable();

    IEnumerator UpdateFollowerPosition()
    {
        m_positionFollower.position = m_followerLocation.position;
        yield return null;
        StartCoroutine("UpdateFollowerPosition");
    }
    
	// ScaleChange Variables
	[SerializeField]
	private bool m_changeScale;
    
    // SpriteChange Variables and Method
    [SerializeField]
    private bool m_changeSprite;
    [SerializeField]
    private string m_pressedSpriteName;
    
    string m_unpressedSpriteName;

    bool m_hasSetSpriteNames = false; // Defensive: It is possible for a menu Coordinator to call ChangeSprite before Start method has set m_pressedSpriteName

    public void ChangeSprite(bool toStateB)
    {
        if (!m_hasSetSpriteNames) // Defensive: It is possible for a menu Coordinator to call ChangeSprite before Start method has set m_pressedSpriteName
        {
            m_hasSetSpriteNames = true;

            m_unpressedSpriteName = m_pressableButton.spriteName;
            
            if (System.String.IsNullOrEmpty(m_pressedSpriteName))
            {
				m_pressedSpriteName = m_unpressedSpriteName.Substring(0, m_unpressedSpriteName.Length - 1);
				m_pressedSpriteName += "b";
            }
        } 

        m_pressableButton.spriteName = toStateB ? m_pressedSpriteName : m_unpressedSpriteName;
    }
    

    // Shared Methods
	float m_fadeTweenDuration = 0.2f;

	public void Off(bool instant)
	{
		collider.enabled = false;

		if (instant) 
		{
			m_pressableButton.color = new Color(m_pressableButton.color.r, m_pressableButton.color.g, m_pressableButton.color.b, 0);
		} 
		else 
		{
			TweenAlpha.Begin(gameObject, StoryInfo.fadeDuration, 0);
		}
	}

	public void On()
	{
		collider.enabled = true;
		TweenAlpha.Begin(gameObject, StoryInfo.fadeDuration, 1);
	}

    public void AddPressedAudio(string audioEvent)
    {
        m_pressedAudio.Add(audioEvent);
    }

    public void AddUnpressedAudio(string audioEvent)
    {
        m_unpressedAudio.Add(audioEvent);
    }

    public int GetInt()
    {
        return m_int;
    }

    public void SetInt(int newInt)
    {
        m_int = newInt;
    }

    public string GetString()
    {
        return m_string;
    }

    public void SetString(string newString)
    {
        m_string = newString;
    }

    public void SetAutoReset(bool newAutoReset)
    {
        m_autoReset = newAutoReset;
    }

    void Start()
    {
        if (m_pressableButton != null)
        {
            if (m_changePosition)
            {
                GameObject unpressedLocationGo = Wingrove.SpawningHelpers.InstantiateUnderWithIdentityTransforms(m_emptyPrefab, transform);
                m_unpressedLocation = unpressedLocationGo.transform;
                m_unpressedLocation.position = m_pressableButton.transform.position;
                
                m_positionTweenArgs.Add("time", m_pressDuration);
                m_positionTweenArgs.Add("easetype", m_positionEaseType);
            }
            
            if (m_changeSprite && !m_hasSetSpriteNames)
            {
                m_hasSetSpriteNames = true;

                m_unpressedSpriteName = m_pressableButton.spriteName;
                
                if (System.String.IsNullOrEmpty(m_pressedSpriteName))
                {
                    m_pressedSpriteName = m_unpressedSpriteName.Substring(0, m_unpressedSpriteName.Length - 1);
					m_pressedSpriteName += "b";
                }
            }
            
            if (m_sheenAnimation != null)
            {
                m_sheenAnimation.OnAnimFinish += OnSheenFinish;
                m_sheenAnimation.PlayAnimation("ON");
                StartCoroutine(PlaySheen());
            }
        }
    }
    
    void OnSheenFinish(string animName)
    {
        StartCoroutine(PlaySheen());
    }
    
    IEnumerator PlaySheen()
    {
        yield return new WaitForSeconds(Random.Range(3f, 9f));
        m_sheenAnimation.PlayAnimation("ON");
    }
    
    void OnPress(bool pressed)
    {
        if (pressed)
        {
            StartCoroutine(Press());
        } 
        else if(m_autoReset)
        {
            StartCoroutine(Unpress());
        }
    }
    
    IEnumerator Press()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Pressing != null)
        {
            Pressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;

        foreach (string audioEvent in m_pressedAudio)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
        
        if (m_changePosition)
        {
            if (m_positionFollower != null)
            {
                StartCoroutine("UpdateFollowerPosition");
            }

            m_positionTweenArgs ["position"] = m_pressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }

		if (m_changeScale) 
		{
			iTween.ScaleTo(gameObject, Vector3.one * 0.8f, m_pressDuration);
		}
        
        if (m_changeSprite)
        {
            m_pressableButton.spriteName = m_pressedSpriteName;
        }
        
        if (m_pressAnimation != null)
        {
            m_pressAnimation.PlayAnimation("ON");
        }
        
        yield return new WaitForSeconds(m_pressDuration);
        
        collider.enabled = true;
        m_isTransitioning = false;
    }
    
    public IEnumerator Unpress()
    {
        while (m_isTransitioning)
        {
            yield return null;
        }
        
        if (Unpressing != null)
        {
            Unpressing(this);
        }
        
        m_isTransitioning = true;
        collider.enabled = false;

        foreach(string audioEvent in m_unpressedAudio)
        {
            WingroveAudio.WingroveRoot.Instance.PostEvent(audioEvent);
        }
        
        if (m_changePosition)
        {
            m_positionTweenArgs ["position"] = m_unpressedLocation;
            iTween.MoveTo(m_pressableButton.gameObject, m_positionTweenArgs);
        }

		if (m_changeScale) 
		{
			iTween.ScaleTo(gameObject, Vector3.one, m_pressDuration);
		}
        
        yield return new WaitForSeconds(m_pressDuration);
        
        if (Unpressed != null)
        {
            Unpressed(this);
        }

        if (m_changeSprite)
        {
            m_pressableButton.spriteName = m_unpressedSpriteName;
        }

        if (m_changePosition && m_positionFollower != null)
        {
            StopCoroutine("UpdateFollowerPosition");
        }
        
        collider.enabled = true;
        m_isTransitioning = false;
    }

    public void EnableCollider(bool enable)
    {
        collider.enabled = enable;
    }
}