using UnityEngine;
using System.Collections;

public class WaitforClick : MonoBehaviour {

/*	public MonoBehaviour followScript;
	public GameObject mainCharacterObject;
	public GUITexture mainTitle;
	public GameObject cameraPath;
*/
	public GameObject testConnection;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (0)) 
		{
			testConnection.SetActive(true);
		}
	
	}
}
