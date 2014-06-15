using UnityEngine;
using System.Collections;

public class LoadSceneOnClick : MonoBehaviour 
{
	[SerializeField]
	private string m_sceneName;

	void OnClick()
	{
		StartCoroutine(Wait());
	}

	IEnumerator Wait()
	{
		yield return new WaitForSeconds(0.6f);

		Application.LoadLevel (m_sceneName);

	}

}
