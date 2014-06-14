using UnityEngine;
using System.Collections;

public class LoadSceneOnClick : MonoBehaviour 
{
	[SerializeField]
	private string m_sceneName;

	void OnClick()
	{
		Application.LoadLevel (m_sceneName);
	}
}
