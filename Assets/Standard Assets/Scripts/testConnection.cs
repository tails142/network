using UnityEngine;
using System.Collections;

public class testConnection : MonoBehaviour {

	// Use this for initialization
	void Start() {
		MasterServer.ipAddress = "forums.gullcatlock.com";
		Network.natFacilitatorIP = MasterServer.ipAddress;
	}

	void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
		Debug.Log("Could not connect to master server: " + info);

		WWW  www = new WWW("http://pvlgrvm.gullcatlock.com/checkstatus.php");

	}



	void OnServerInitialized()
	{
				MasterServer.RegisterHost ("com.pvlgrm", "Dereks Game", "Test");
	}

	void OnMasterServerEvent(MasterServerEvent msEvent)
	{
		Debug.Log(msEvent);
	}

	void OnGUI () {

		if (GUILayout.Button ("Register"))
		{
						bool useNat = !Network.HavePublicAddress ();
						Network.InitializeServer (32, 25002, useNat);
		}

		if (GUILayout.Button("Unregister"))
		{
			Network.Disconnect();
			MasterServer.UnregisterHost();
		}
	}
}
