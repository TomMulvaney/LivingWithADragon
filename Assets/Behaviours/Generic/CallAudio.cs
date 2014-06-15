using UnityEngine;
using System.Collections;

public class CallAudio : MonoBehaviour 
{
	IEnumerator Start () 
	{
		yield return StartCoroutine (WingroveAudio.WingroveRoot.WaitForInstance());

		WingroveAudio.WingroveRoot.Instance.PostEvent ("MUSIC_CUTE");
	}
}
