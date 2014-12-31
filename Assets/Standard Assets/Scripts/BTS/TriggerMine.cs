using UnityEngine;
using System.Collections;

public class TriggerMine : MonoBehaviour {

	void OnTriggerEnter(Collider other) 
	{
				
		SendMessageUpwards("TriggerMine", transform.parent.gameObject);
		
	}
}
