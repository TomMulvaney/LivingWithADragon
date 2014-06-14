using UnityEngine;
using System.Collections;

public class wait_for_menu : MonoBehaviour {

	// Use this for initialization
	void Start () {
		StartCoroutine(Wait());
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	IEnumerator Wait()
	{
		yield return new WaitForSeconds(4.0f);
		Debug.Log("tette");
		Application.LoadLevel("MainMenu");

	}
}	