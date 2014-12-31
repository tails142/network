using UnityEngine;
using System.Collections;

public class CharacterSelector : MonoBehaviour 
{
	public GUISkin customGuiSkin;
	public Material[] characterMaterial;
	public Mesh[] characterMesh;
	
	public  bool c_isGhost = false;
	
	public  bool isGhost
    {
        get { return c_isGhost; }
		set { c_isGhost = value; }
	}

	private static GUISkin c_guiSkin = null;
	
	public static GUISkin guiSkin
    {
        get { return c_guiSkin; }
		set { c_guiSkin = value; }
	}

	private static Rect windowRect0  = new Rect (20, 10, 136, 136);
	private static Rect windowRect1  = new Rect (180, 10, 136, 136);

	private static bool c_selectChar = false;

	public static bool selectChar
    {
        get { return c_selectChar; }
		set { c_selectChar = value; }
	}


	private static int c_mainCharacterSkin = 0;

	public static int mainCharacterSkin
    {
        get { return c_mainCharacterSkin; }
		set { c_mainCharacterSkin = value; }
	}

	private static int c_mainCharacterMesh = 0;

	public static int mainCharacterMesh
    {
        get { return c_mainCharacterMesh; }
		set { c_mainCharacterMesh = value; }
	}

	private static bool c_useTool = false;

	public static bool useTool
    {
        get { return c_useTool; }
		set { c_useTool = value; }
	}

	
	// Use this for initialization
	void Start () 
	{
		if (null != customGuiSkin)
		{
			guiSkin = customGuiSkin;
		}
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		if (c_isGhost)
		{
			return;
		}
		
		// The BAD magic text "backspace" should be added to the Literals class
		if (Input.GetKeyDown("backspace"))
		{
			if(selectChar)
			{
				selectChar = false;
			}
			else
			{
				selectChar = true;
			}
		}
		
	}
	
	void OnGUI() 
	{

		if (c_isGhost)
		{
			return;
		}
		
		if (!selectChar) 
		{
			return;
		}
		
		GUI.skin = guiSkin;
		
		// The BAD magic text "Gloves" should be added to the Literals class
		windowRect0 = GUI.Window (0, windowRect0, ShowWindowSkin, "Gloves");

		// The BAD magic text "Mesh" should be added to the Literals class
		windowRect1 = GUI.Window (1, windowRect1, ShowWindowMesh, "Mesh");

		
	}
	
	void ShowWindowSkin(int a_windowID)
	{
	
		if (GUI.Button (new Rect (18,32,100,20), "Yellow"))
		{
    	    mainCharacterSkin = 0;
    	    ChangeMaterial();
     	}
		
   	 	if (GUI.Button (new Rect (18,54,100,20), "Red"))
		{
      	    mainCharacterSkin = 1;
    	    ChangeMaterial();
		}
		
   	 	if (GUI.Button (new Rect (18,76,100,20), "Green"))
		{
    	    mainCharacterSkin = 2;
    	    ChangeMaterial();
        }
		
    	if (GUI.Button (new Rect (18,98,100,20), "Blue"))
		{
    	    mainCharacterSkin = 3;
    	    ChangeMaterial();
    	}
	
		GUI.DragWindow (new Rect(0,0,10000,10000));
	}

	void ShowWindowMesh(int a_windowID)
	{
	
		if (GUI.Button (new Rect (18,32,100,20), "Hat"))
		{
 			mainCharacterMesh = 0;
			ChangeMesh();
    	}
		
   	 	if (GUI.Button (new Rect (18,54,100,20), "No hat"))
		{
			mainCharacterMesh = 1;
			ChangeMesh();
 		}
		
   	 	if (GUI.Button (new Rect (18,76,100,20), "Tool"))
		{
			useTool = true;
			ChangeTool();
        }
		
    	if (GUI.Button (new Rect (18,98,100,20), "No Tool"))
		{
			useTool = false;
			ChangeTool();
     	}
	
		GUI.DragWindow (new Rect(0,0,10000,10000));
	}

	void ChangeTool()
	{

		ApplyTool();
	
		if (null != networkView)
		{
			if ((Network.peerType == NetworkPeerType.Client) ||
				(Network.peerType == NetworkPeerType.Server))
			{
				var l_group = networkView.group;
				networkView.group = 3;
				Network.RemoveRPCsInGroup(3);
				networkView.RPC("ChangeToolRPC", RPCMode.OthersBuffered, useTool);
				networkView.group = l_group;
			}
		}
	}

	void ChangeMesh()
	{

		ApplyMesh();
	
		if (null != networkView)
		{
			if ((Network.peerType == NetworkPeerType.Client) ||
				(Network.peerType == NetworkPeerType.Server))
			{
				var l_group = networkView.group;
				networkView.group = 2;
				Network.RemoveRPCsInGroup(2);
				networkView.RPC("ChangeMeshRPC", RPCMode.OthersBuffered, mainCharacterMesh);
				networkView.group = l_group;
			}
		}
	}

	void ChangeMaterial()
	{
		
		ApplyMaterial();
	
		if (null != networkView)
		{
			if ((Network.peerType == NetworkPeerType.Client) ||
				(Network.peerType == NetworkPeerType.Server))
			{
				var l_group = networkView.group;
				networkView.group = 1;
				Network.RemoveRPCsInGroup(1);
				networkView.RPC("ChangeMaterialRPC", RPCMode.OthersBuffered, mainCharacterSkin);
				networkView.group = l_group;
			}
		}
	}
	
	[RPC]
	void ChangeMaterialRPC(int a_mainCharacterSkin)
	{

		mainCharacterSkin = a_mainCharacterSkin;
	
		ApplyMaterial();

	}
	
	void ApplyMaterial() 
	{
		
		GetComponentInChildren<SkinnedMeshRenderer>().material = characterMaterial[mainCharacterSkin];
	
	}
	
	[RPC]
	void ChangeMeshRPC(int a_mainCharacterMesh)
	{

		mainCharacterMesh = a_mainCharacterMesh;
	
		ApplyMesh();

	}
	
	void ApplyMesh() 
	{
		
		GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = characterMesh[mainCharacterMesh];
	
	}
	
	[RPC]
	void ChangeToolRPC(bool a_useTool)
	{

		useTool = a_useTool;
	
		ApplyTool();

	}
	
	void ApplyTool() 
	{
		
		GetComponentInChildren<MeshFilter>().renderer.enabled = useTool;
	
	}
	

}
