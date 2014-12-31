using UnityEngine;
using System.Collections;

public class DestroyAllNuts : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		transform.parent.gameObject.BroadcastMessage("DestroyAllNuts");
	}
	

}
