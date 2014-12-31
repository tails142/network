using UnityEngine;
using System.Collections;

public class NetworkPlayerSetup : MonoBehaviour 
{
	
	void OnNetworkInstantiate(NetworkMessageInfo info) 
	{
		if (!networkView.isMine)
		{
			gameObject.tag = Literals.kGhostCharacterTag;
			gameObject.layer = Literals.kGhostLayer;
				
			Component l_ThirdPersonMecanimCamera = gameObject.GetComponent("ThirdPersonMecanimCamera");
//			Component l_ThirdPersonMecanimController = gameObject.GetComponent("ThirdPersonMecanimController");
			Component l_ThirdPersonCamera = gameObject.GetComponent("ThirdPersonCamera");
//			Component l_ThirdPersonController = gameObject.GetComponent("ThirdPersonController");

			CharacterSelector l_CharacterSelector = gameObject.GetComponent("CharacterSelector") as CharacterSelector;

			if (l_ThirdPersonMecanimCamera)
			{
				Component.Destroy(l_ThirdPersonMecanimCamera);
			}
			
//			if (l_ThirdPersonMecanimController)
//			{
//				Component.Destroy(l_ThirdPersonMecanimController);
//			}
			
			
			if (l_ThirdPersonCamera)
			{
				Component.Destroy(l_ThirdPersonCamera);
			}

//			if (l_ThirdPersonController)
//			{
//				Component.Destroy(l_ThirdPersonController);
//			}

			if (l_CharacterSelector)
			{
				l_CharacterSelector.isGhost = true;
			}

		}

	}
}
