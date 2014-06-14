using UnityEngine;
using System.Collections;

public class TurnSwipeDetect : MonoBehaviour 
{
	public delegate void SwipeEventHandler (TurnSwipeDetect swipeDetect);
	public event SwipeEventHandler SwipedLeft;
	public event SwipeEventHandler SwipedRight;

    double m_dragStartTime;
    Vector2 m_totalDrag;

	void OnDrag(Vector2 delta)
	{
		m_totalDrag += delta;
	}

    void OnPress(bool press)
    {
		Debug.Log ("TurnSwipeDetect.OnPress(" + press + ")");
        if (press)
        {
            m_dragStartTime = AudioSettings.dspTime;
            m_totalDrag = Vector2.zero;
        }
        else
        {
            double dragDuration = AudioSettings.dspTime - m_dragStartTime;
            if (dragDuration < 2.0f && dragDuration > 0.05f)
            {
                if (Mathf.Abs(Vector2.Dot(m_totalDrag, Vector2.right)) > 0.3f)
                {
                    if (m_totalDrag.x > 32)
                    {
						Debug.Log("SWIPED RIGHT");
						if(SwipedRight != null)
						{
							SwipedRight(this);
						}
                    }
                    else if (m_totalDrag.x < -32)
                    {
						Debug.Log("SWIPED LEFT");
                        if(SwipedLeft != null)
						{
							SwipedLeft(this);
						}
                    }
                }
            }
        }
    }
}
