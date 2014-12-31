using UnityEngine;
using System.Collections;

public class PickupCoconut : MonoBehaviour {

	void OnTriggerEnter(Collider other) 
	{
				
		SendMessageUpwards("PickupCoconut", transform.parent.gameObject);
		
	}

}
