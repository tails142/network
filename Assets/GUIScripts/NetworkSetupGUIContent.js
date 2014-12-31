#pragma strict

import System;

var dialogName : String = "Network Settings";
var networkLevelToLoad : String = "MainScene";

// Objects in scene that should be destroyed when MainScene is loaded
var destroyOnLoad : Transform[];	

// Objects in scene that should be activated when MainScene is loaded
var activateOnLoad : Transform[];	

// Objects in scene that should be reparented to the Main Camera when MainScene is loaded
var parentToMainCameraOnLoad : Transform[];	


var BTS_FailedConnectionCount : int = 0;
var BTS_FailedConnectionCountMax : int = 2;
var BTS_MasterServerIP : String = "bzn.gullcatlock.com";
var BTS_MasterServerRestartURLPath : String = "/checkstatus.php";
var BTS_isHosting : boolean = false;

var masterServerAutoRefreshSeconds : float = 30.0;
var masterServerTimeoutSeconds : float = 10.0;
var nextMasterServerRefreshTime : DateTime;
var nextMasterServerRefreshTimeout : DateTime;

var textLabelColor : Color = Color.green;
var guiSelectColor : Color = Color.yellow;
var guiErrorColor : Color = Color.red;
var guiOKColor : Color = Color.green;

var currentMenu : e_menu = e_menu.root;

var leftMarginPercent : float = 50.0;
var topMargin : float = 0.1;

var scrollPosition : Vector2 = Vector2.zero;
var buttonHeight : float= 20;
var listSpacing : float;

var gameName : String;
var gamePassword : String;
var gameComment : String;
var gamePort : String;
var maxPlayers : String;

var registerOnLan : boolean;
var registerOnInternet : boolean;
var lastRegisterOnLan : boolean;
var lastRegisterOnInternet : boolean;

var buttonBar : String[];


var selectedComponents : Hashtable;

// Game hosting console
var consoleLog : String;


//An array that contains the hashtables for the currently displayed list
var currentListHash = new Array();

//Back button
private var previousMenu = new Array();
private var previousCancelButton = new Array();
private var previousReadyButton = new Array();
private var previousButtonBar = new Array();

private var hostDataBuffer : HostData[] = null;

private var waitingForMasterServer : int = 0;

private var didStartGame : boolean = false;

function Awake() 
{

	if (0 == buttonBar.length)
	{
		buttonBar = new String[4];
		buttonBar[0] = "Cancel";
		buttonBar[1] = "Ready";
	}
	
	
	
	previousMenu.Clear();
	
	didStartGame = false;			
}

function OnEnable()
{

	didStartGame = false;
}


function rendercontent (boxSize : Vector2) 
 {
    	
  	// Some basic scroll view variables
 	var l_svpr : Rect; // Scroll View Position Rect
	var l_svvr : Rect; // Scroll View View Rect
	var l_gridr : Rect; // Grid Rect
	var l_svvrHeight : int; // Height of the View Rect MUST be greater or equal to than height of position rect.
	var l_gridHeight : int; // Height of the View Grid MUST be Min(lineHeight * numLines, l_svvrHeight).

	
	// Some basic GUI variables
	var l_box_x : int = 0;
	var l_box_y : int = 0;
	var l_style : String;
	var l_lineHeight : int = 32;
	var l_lineSpace : int = 3;
	
	var l_savedContentColor : Color = GUI.contentColor;

	// Draw the dialog box with the correct title
//	GUI.Box (Rect (l_box_x, l_box_y, boxSize.x, boxSize.y), GUIContent(dialogName));
	
	//Top Bar
	
	if (didStartGame)
	{
		GUI.enabled = false;
	}
	
	if (buttonBar.Length > 0)
	{
		if ("" != buttonBar[0])
		{
			if (GUI.Button(Rect(l_lineHeight / 2,l_lineHeight / 2, boxSize.x / 4 - l_lineHeight / 2 ,l_lineHeight - l_lineSpace), buttonBar[0]))
			{
				
				if (e_menu.root == currentMenu)
				{
					didStartGame = true;
   	 				Invoke("LoadTheMainScene", 1);
				}
				else if (e_menu.hosting == currentMenu)
				{
					StopHosting();
    				currentMenu = previousMenu[previousMenu.length - 1];
    				previousMenu.Pop();
    				buttonBar = previousButtonBar[previousButtonBar.length - 1];
    				previousButtonBar.Pop();
				}
				else
				{
    				currentMenu = previousMenu[previousMenu.length - 1];
    				previousMenu.Pop();
    				buttonBar = previousButtonBar[previousButtonBar.length - 1];
    				previousButtonBar.Pop();
      			}
      		}
      	}
    }
 
    
	if (buttonBar.Length > 1)
	{
		if ("" != buttonBar[1])
		{
			if (GUI.Button (Rect(boxSize.x / 4 + l_lineHeight / 2 - l_lineSpace, l_lineHeight / 2, boxSize.x / 4 - l_lineHeight / 2,l_lineHeight - l_lineSpace), buttonBar[1]))
    		{

   	 			if (e_menu.root == currentMenu)
    			{
   		
   		 			NextMenu(e_menu.network);
   		 			buttonBar = new String[3];
   		 			buttonBar[0] = "<Back";
    		 		buttonBar[1] = "Join Game";
    		 		buttonBar[2] = "Host Game";
  		 	   		   			
    			}
    			else if ((e_menu.network == currentMenu) && (0 == waitingForMasterServer))
    			{
    				BTS_isHosting = false;
					BTSMasterServer.ipAddress = BTS_MasterServerIP;
					Network.natFacilitatorIP = BTS_MasterServerIP;
					
    				BTSUDPServer.RegisterForEvents(gameObject);

  					BTSMasterServer.ClearHostList();
   					BTSUDPServer.ClearHostList();
   					
   					waitingForMasterServer = 2;
   					BTSMasterServer.RequestHostList(Literals.lGameType);
    				BTSUDPServer.RequestHostList(Literals.lGameType);
   					
   					nextMasterServerRefreshTime = System.DateTime.Now.AddSeconds(masterServerAutoRefreshSeconds);
   					nextMasterServerRefreshTimeout = System.DateTime.Now.AddSeconds(masterServerTimeoutSeconds);
    		 		NextMenu(e_menu.join);
   		 			buttonBar = new String[2];
   		 			buttonBar[0] = "<Back";
    		 		buttonBar[1] = "Join Game";   		 	   		   			
    			}
    			else if (e_menu.host == currentMenu)
    			{
    				StartHosting();
    		 		NextMenu(e_menu.hosting);
   		 			buttonBar = new String[1];
   		 			buttonBar[0] = "Cancel";
   		 			
    			}
    		}
    	}   	
     }
     
 	if (buttonBar.Length > 2)
	{
		if ("" != buttonBar[2])
		{
			if (GUI.Button (Rect(boxSize.x / 2 + 2 * l_lineSpace, l_lineHeight / 2, boxSize.x / 4  - l_lineHeight / 2 ,l_lineHeight - l_lineSpace), buttonBar[2]))
    		{

   	 			if (e_menu.network == currentMenu)
    			{
   					ReadServerSettings();
   		 			NextMenu(e_menu.host);
   		 			buttonBar = new String[4];
   		 			buttonBar[0] = "<Back";
    		 		buttonBar[1] = "Start Game";
    		 		buttonBar[2] = "";
    		 		buttonBar[3] = "Defaults";
  		 	   		   			
    			}
    		}
    	}   	
     }
    
 	if (buttonBar.Length > 3)
	{
		if ("" != buttonBar[3])
		{
			if (GUI.Button (Rect(boxSize.x / 4 * 3, l_lineHeight / 2, boxSize.x / 4  - l_lineHeight / 2 ,l_lineHeight - l_lineSpace), buttonBar[3]))
    		{
   	 			if (e_menu.host == currentMenu)
    			{
					gameName = Literals.lDefaultGameName;
					gamePassword = Utilities.RandomDigits(6);
					gameComment = Literals.lDefaultGameComment;
					gamePort = Literals.lDefaultGamePort;
					maxPlayers = "" + Literals.lDefaultConnections;
					registerOnLan = Literals.lDefaultRegisterOnLAN;
					registerOnInternet = Literals.lDefaultRegisterOnInternet;
					lastRegisterOnLan = Literals.lDefaultRegisterOnLAN;
					lastRegisterOnInternet = Literals.lDefaultRegisterOnInternet;
    			}

  			}
    	}   	
     }
    
    GUI.enabled = true;
    
	var l_groupRect : Rect; 
					
   	var l_line : int = 0;
 
	switch(currentMenu)
    {
    	case e_menu.root:
    	    			
    		break;
    		
    	case e_menu.network:
    	    			
    		break;
    		
     	case e_menu.join:
     	
     		l_groupRect = Rect(l_lineHeight / 2, 2 * l_lineHeight,boxSize.x - l_lineHeight , boxSize.y - 2 * l_lineHeight - l_lineSpace);
   			GUI.Box (l_groupRect, "");
    		ListAvailableHosts(boxSize);
    	    			
    		break;
    		
    	case e_menu.host:
    		
    		l_groupRect = Rect(l_lineHeight / 2, 2 * l_lineHeight,boxSize.x - l_lineHeight , boxSize.y - 2 * l_lineHeight - l_lineSpace);
   			GUI.Box (l_groupRect, "");

    		l_groupRect = Rect(l_lineHeight, l_lineHeight + l_lineSpace,boxSize.x - 2 * l_lineHeight , 4 * l_lineHeight);
    		
    		GUI.BeginGroup(l_groupRect);
	   		GUI.contentColor = textLabelColor;
      		GUI.Label(Rect(0, l_lineHeight, l_groupRect.width / 2 - l_lineSpace, l_lineHeight), "Game Name");
      		GUI.Label(Rect(l_groupRect.width / 2, l_lineHeight, l_groupRect.width / 4 - l_lineSpace, l_lineHeight), "Password");
       		GUI.Label(Rect(l_groupRect.width / 4 * 3, l_lineHeight, l_groupRect.width / 4 - l_lineSpace, l_lineHeight), "Max Players");
			GUI.contentColor = l_savedContentColor;
    		gameName = GUI.TextField(Rect(0, 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 2 - l_lineSpace, l_lineHeight), gameName);
    		gamePassword = GUI.TextField(Rect(l_groupRect.width / 2, 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 4 - l_lineSpace, l_lineHeight), gamePassword);
    		maxPlayers = GUI.TextField(Rect(l_groupRect.width / 4 * 3, 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 4 - l_lineSpace, l_lineHeight), maxPlayers);
       		GUI.EndGroup();
    		
    		l_groupRect = Rect(l_lineHeight, boxSize.y / 4, boxSize.x - 2 * l_lineHeight , 4 * l_lineHeight);
    		GUI.BeginGroup(l_groupRect);
	   		GUI.contentColor = textLabelColor;
      		GUI.Label(Rect(0, l_lineHeight, l_groupRect.width / 2 - l_lineSpace, l_lineHeight), "IP Address");
      		GUI.Label(Rect(l_groupRect.width / 4, l_lineHeight, l_groupRect.width / 4 - l_lineSpace, l_lineHeight), "Port");
       		GUI.Label(Rect(l_groupRect.width / 2, l_lineHeight, l_groupRect.width / 2 - l_lineSpace, l_lineHeight), "Network Registration Options");
			GUI.contentColor = l_savedContentColor;
      		GUI.Label(Rect(0, 1.5 * l_lineHeight, l_groupRect.width / 4  - l_lineSpace, l_lineHeight), Network.player.ipAddress + " : ");
      		gamePort = GUI.TextField(Rect(l_groupRect.width / 4, 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 4  - l_lineSpace, l_lineHeight), gamePort);
    		registerOnLan = GUI.Toggle(Rect(l_groupRect.width / 2 , 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 6, l_lineHeight), registerOnLan, "LAN");
    		if (registerOnLan != lastRegisterOnLan)
    		{
    			if (false == registerOnInternet)
    			{
    				registerOnLan = true;
    			}
    			else
    			{
    				registerOnInternet = !registerOnLan;
    			}
    		}
    				
    		registerOnInternet = GUI.Toggle(Rect(l_groupRect.width - l_groupRect.width / 4 , 1.5 * l_lineHeight + l_lineSpace, l_groupRect.width / 6, l_lineHeight), registerOnInternet, "Internet");
     		if (registerOnInternet != lastRegisterOnInternet)
    		{
    			if (false == registerOnLan)
    			{
    				registerOnInternet = true;
    			}
    			else
    			{
    				registerOnLan = !registerOnInternet;
    			}
    		}
    		
    		lastRegisterOnLan = registerOnLan;
    		lastRegisterOnInternet = registerOnInternet;
   		
       		GUI.EndGroup();

       		l_groupRect = Rect(l_lineHeight, boxSize.y - 4.5 * l_lineHeight,boxSize.x - 2 * l_lineHeight , 4 * l_lineHeight);
    		GUI.BeginGroup(l_groupRect);
	   		GUI.contentColor = textLabelColor;
      		GUI.Label(Rect(0, 2 * l_lineHeight + 3 * l_lineSpace, l_groupRect.width / 2 - l_lineSpace, l_lineHeight), "Game Comment");
 			GUI.contentColor = l_savedContentColor;
    		gameComment = GUI.TextField(Rect(0, 3 * l_lineHeight - l_lineSpace, l_groupRect.width, l_lineHeight), gameComment);
       		GUI.EndGroup();
    		break;
 
    	case e_menu.hosting:
    		l_groupRect = Rect(l_lineHeight / 2, 2 * l_lineHeight,boxSize.x - l_lineHeight , boxSize.y - 2 * l_lineHeight - l_lineSpace);
   			
   			GUI.enabled = false;
   			GUI.TextArea (l_groupRect, consoleLog);
   			GUI.TextArea (l_groupRect, consoleLog);
   			GUI.enabled = true;

    		break;
   		
    		
    	default:
    		var stringToEdit : String = "Error: Failed to Locate menu. Please report via email to supportburningthumb.com\n Press the back button to return to where you were.";
    		stringToEdit = GUI.TextArea (Rect (0,0,l_groupRect.width ,l_groupRect.height), stringToEdit, 200);
    	
    		break;
    }
  
    		    	  		    	
			
}

function NextMenu(item :e_menu)
{


//	This is where an array of previous items should be set
//	The currentMenu.length is used to determine the menu depth
	previousMenu.Push(currentMenu);
	previousButtonBar.Push(buttonBar);
	currentMenu = item;	
}

function PlayerDidConfirmHosting()
{

	
	BTSPreferences.SetStringForKey(gameName, Literals.kGameName);
	BTSPreferences.SetStringForKey(gamePassword, Literals.kServerPassword);
	BTSPreferences.SetStringForKey(gameComment, Literals.kGameComment);
	BTSPreferences.SetStringForKey(gamePort, Literals.kPort);
	BTSPreferences.SetStringForKey(maxPlayers, Literals.kConnections);
	BTSPreferences.SetBoolForKey(registerOnLan, Literals.kRegisterOnLan);
	BTSPreferences.SetBoolForKey(registerOnInternet, Literals.kRegisterOnInternet);

	BTSPreferences.Synchronize();

}

function PlayerDidConfirm()
{

	didStartGame = true;
	 
	BTSPreferences.Synchronize();

	SendMessageUpwards("ok", SendMessageOptions.RequireReceiver);
}

function PlayerDidCancel()
{
   	 SendMessageUpwards("cancel", SendMessageOptions.RequireReceiver);
}

function ListAvailableHosts(boxSize : Vector2)
{
	// Some basic GUI variables
	var l_box_x : int = 0;
	var l_box_y : int = 0;
	var l_style : String;
	var l_lineHeight : int = 32;
	var l_lineSpace : int = 3;

	var data1 : Array = new Array(BTSMasterServer.PollHostList());
	var data2 : Array = new Array(BTSUDPServer.PollHostList());
	var data3 : Array = data1.Concat(data2);
	
	var data : HostData[] = data3.ToBuiltin(HostData);
	
	if (0 != waitingForMasterServer)
	{
		if (null != hostDataBuffer)
		{
			if (0 != hostDataBuffer.Length)
			{
				data = hostDataBuffer;
			}
		}
	}
	else
	{
		hostDataBuffer = data;
	}
	
	var _cnt : int = 0;
	
	var l_svpr : Rect = Rect (l_lineHeight, l_lineHeight * 4 , boxSize.x - l_lineHeight * 2 , boxSize.y / 2);
	var l_svvr : Rect = Rect (0, 0, boxSize.x - l_lineHeight * 3, boxSize.y / 2);
	scrollPosition = GUI.BeginScrollView (l_svpr, scrollPosition,l_svvr, false, true );
	
	if (0 == data.Length)
	{
		GUI.enabled = false;
	
		GUI.Button(new Rect(0,0,boxSize.x - 2.5 * l_lineHeight - 5,l_lineHeight - 2),"No games found - yet");
	
		GUI.enabled = true;
	}
	
	var x : int = 0;
	for (var element in data)
	{

			var name : String = element.gameName;
			
			name = name + " " + element.connectedPlayers + " / " + element.playerLimit;
			
			/*
			var hostInfo;
			hostInfo = "[";
			// Here we display all IP addresses, there can be multiple in cases where
			// internal LAN connections are being attempted. In the GUI we could just display
			// the first one in order not confuse the end user, but internally Unity will
			// do a connection check on all IP addresses in the element.ip list, and connect to the
			// first valid one.
			for (var host in element.ip)
			{
				hostInfo = hostInfo + host + ":" + element.port + " ";
			}
			hostInfo = hostInfo + "]";
			*/
			
			if (element.connectedPlayers == element.playerLimit)
			{
				GUI.enabled = false;
				name = name + " (Full)";
			}

			if (GUI.Button(new Rect(0,(_cnt*l_lineHeight),boxSize.x - 2.5 * l_lineHeight - 5,l_lineHeight - 2),name))
			{

				Network.Connect(element);	
			}
			
			GUI.enabled = true;
			
			_cnt++;
	}

	GUI.EndScrollView();
	
	var l_diff : System.TimeSpan;
	
	if (0 != waitingForMasterServer)
	{
		GUI.enabled = false;
		GUI.Button(Rect(l_lineHeight / 2 + 2 * l_lineSpace, boxSize.y - l_lineHeight, 3.5 * l_lineHeight - 2 * l_lineSpace,l_lineHeight - 3 * l_lineSpace), "Refreshing");
		nextMasterServerRefreshTime = System.DateTime.Now.AddSeconds(masterServerAutoRefreshSeconds);
		GUI.enabled = true;
		
		if (System.DateTime.Now > nextMasterServerRefreshTimeout)
		{
			waitingForMasterServer = 0;
		}
		
	}
	else if ((System.DateTime.Now > nextMasterServerRefreshTime) ||
		(GUI.Button(Rect(l_lineHeight / 2 + 2 * l_lineSpace, boxSize.y - l_lineHeight, 3.5 * l_lineHeight - 2 * l_lineSpace,l_lineHeight - 3 * l_lineSpace), "Refresh Now")))
	{
		waitingForMasterServer = 2;
   		BTSMasterServer.RequestHostList(Literals.lGameType);
    	BTSUDPServer.RequestHostList(Literals.lGameType);
   		nextMasterServerRefreshTime = System.DateTime.Now.AddSeconds(masterServerAutoRefreshSeconds);
   		nextMasterServerRefreshTimeout = System.DateTime.Now.AddSeconds(masterServerTimeoutSeconds);
	}

	if (0 != waitingForMasterServer)
	{
		GUI.enabled = false;
	}
	
	l_diff = nextMasterServerRefreshTime - System.DateTime.Now;
	GUI.Label(Rect(4 * l_lineHeight + l_lineSpace, boxSize.y - l_lineHeight + l_lineSpace, boxSize.x - 2.5 * l_lineHeight - 5,l_lineHeight - l_lineSpace), "Auto refresh game list in " + l_diff.Seconds + " seconds");
	GUI.enabled = true;
	
}


function OnConnectedToServer() 
{
    Debug.Log("Connected to server");
    
    Invoke("LoadTheMainScene", 1);
}



function ReadServerSettings()
{

	gameName = BTSPreferences.GetStringForKey(Literals.kGameName);
	
	if ("" == gameName)
	{
		gameName = Literals.lDefaultGameName;
	}

	
	gamePassword = Utilities.RandomDigits(6);

	if (BTSPreferences.HasKey(Literals.kServerPassword))
	{
		gamePassword = BTSPreferences.GetStringForKey(Literals.kServerPassword);
	}
	
	gameComment = BTSPreferences.GetStringForKey(Literals.kGameComment);
	
	if ("" == gameComment)
	{
		gameComment = Literals.lDefaultGameComment;
	}
	
	gamePort = BTSPreferences.GetStringForKey(Literals.kPort);
	
	if ("" == gamePort)
	{
		gamePort = Literals.lDefaultGamePort;
	}
	
	maxPlayers = BTSPreferences.GetStringForKey(Literals.kConnections);
	
	if ("" == maxPlayers)
	{
		maxPlayers = "" + Literals.lDefaultConnections;
	}
	
	registerOnLan = Literals.lDefaultRegisterOnLAN;
	registerOnInternet = Literals.lDefaultRegisterOnInternet;
	
	if (BTSPreferences.HasKey(Literals.kRegisterOnLan))
	{
		registerOnLan = BTSPreferences.GetBoolForKey(Literals.kRegisterOnLan);
	}
	
	if (BTSPreferences.HasKey(Literals.kRegisterOnInternet))
	{
		registerOnInternet = BTSPreferences.GetBoolForKey(Literals.kRegisterOnInternet);
	}

	if ((false == registerOnLan) && (false == registerOnInternet))
	{
		registerOnLan = true;
	}

}

function StopHosting()
{

	BTSUDPServer.UnregisterHost();
	BTSMasterServer.UnregisterHost();

}

function StartHosting()
{
	consoleLog = "";
	
	if ((false == registerOnLan) && (false == registerOnInternet))
	{
		registerOnLan = true;
	}
	
	Network.incomingPassword = gamePassword;

	if ("" == gamePassword)
	{
		consoleLog += "This game is not password protected\n";
	}
	else
	{
		consoleLog += "Setting the incoming Password to " + gamePassword +"\n";
	}
	

	if (registerOnLan)
	{
		BTSUDPServer.RegisterForEvents(gameObject);
		consoleLog += "Registered with UDP Server to receive notifications\n";
	}
	else
	{
		BTSUDPServer.DeregisterForEvents(gameObject);
		consoleLog += "Deregistered from UDP Server\n";
	}

    var l_useNat = !Network.HavePublicAddress();
    
    if (Network.peerType == NetworkPeerType.Disconnected)
    {
    	Network.InitializeSecurity();
    	consoleLog += "Initialized network security\n";
    }
    
    Network.InitializeServer(System.Convert.ToInt32(maxPlayers) - 1, System.Convert.ToInt32(gamePort), l_useNat);
    consoleLog += "Initialized server with " + (System.Convert.ToInt32(maxPlayers) - 1) + " players on port " + gamePort + ". NAT=" + l_useNat +"\n";
    
    /*
     * You must, absoluely must, set the password and call Network.InitializeServer   
     * before calling MasterServer.RegisterHost.
     *
     * And if you change the password or initialize the server again, you must register 
     * the host again - this needs to be verified
     */
    
    BTS_isHosting = true;
    
	if (BTS_FailedConnectionCount <= BTS_FailedConnectionCountMax)
	{
		BTSMasterServer.ipAddress = BTS_MasterServerIP;
		Network.natFacilitatorIP = BTS_MasterServerIP;
	}
     
	if (registerOnLan)
	{
    	consoleLog += "Trying to registered host on LAN using UDP port " + BTSUDPServer.port +"\n";
    	BTSUDPServer.RegisterHost(Literals.lGameType, gameName , gameComment);
    }

	if (registerOnInternet)
	{
    	consoleLog += "Trying to register game using MasterServer\n";
    	BTSMasterServer.RegisterHost(Literals.lGameType, gameName , gameComment);
	}
}


function OnBTSUDPServerEvent(busEvent: BTSUDPServer.BTSUDPServerEvent) 
{
	
	Debug.Log("BTSUDPServerEvent: " + busEvent + " " + BTSUDPServer.PollHostList().Length);
	
    if (BTS_isHosting)
    {
		consoleLog += "BTSUDPServerEvent: " + busEvent + "\n";
	}
	
	///
	//	If we succeeded, request the host list.
	//	This too is debugging code and it can be safely removed
	//
		
    if (busEvent == BTSUDPServer.BTSUDPServerEvent.RegistrationSucceeded) 
    {
    	if (BTS_isHosting)
    	{
   	 		consoleLog += "The server was successfully registered on the LAN\n";
//   	 		PlayerDidConfirm();
   	 		Invoke("LoadTheMainScene", 1);
   	 		Invoke("DestroyTheObjects", 1);

   	 	}
    }
    else if (busEvent == BTSUDPServer.BTSUDPServerEvent.RegistrationFailedNoServer) 
    {
    	if (BTS_isHosting)
    	{
			consoleLog += "Probably port " + gamePort + "or port " + BTSUDPServer.port + " is in use.  Try another port\n";
		}
    }
    else if (busEvent == BTSUDPServer.BTSUDPServerEvent.HostListReceived)
    {
    	hostDataBuffer = null;
    	
    	if (0 != waitingForMasterServer)
    	{
    		waitingForMasterServer--;
    	}
    	
  		if (BTSUDPServer.PollHostList().Length != 0) 
  		{
			Debug.LogWarning("Testing code executed");
 			var hostData : HostData[];
			var i : int;

           	hostData = BTSUDPServer.PollHostList();
        	for (i = 0; i < hostData.Length; i++)
        	{
            	Debug.Log("BTSUDPServer - Game name: " + hostData[i].gameName + " " + hostData[i].guid);
        	}
                
    	}
 	}
}


function OnMasterServerEvent(msEvent: MasterServerEvent) 
{

	Debug.Log("MasterServerEvent: " + msEvent + " " + BTSMasterServer.PollHostList().Length);
	
	
    if (BTS_isHosting)
    {
		consoleLog += "MasterServerEvent: " + msEvent + "\n";
	}
	
	/*
		If we succeeded, request the host list.
		This too is debugging code and it can be safely removed
	*/
		
    if (msEvent == MasterServerEvent.RegistrationSucceeded) 
    {
    	BTS_FailedConnectionCount = 0;
    	if (BTS_isHosting)
    	{
   	 		consoleLog += "The server was successfully registered on the Internet\n";
   	 		Invoke("LoadTheMainScene", 1);
   	 	}
    }
    else if (msEvent == MasterServerEvent.HostListReceived)
    {
    	BTS_FailedConnectionCount = 0;
    	if (0 != waitingForMasterServer)
    	{
    		waitingForMasterServer--;
    	}
    	
 		 if (BTSMasterServer.PollHostList().Length != 0) 
 		 {
			Debug.LogWarning("Testing code executed");
			var hostData : HostData[];
			var i : int;

       	 	hostData = BTSMasterServer.PollHostList();
       	 	for (i = 0; i < hostData.Length; i++)
        	{
            	Debug.Log("BTSMasterServer - Game name: " + hostData[i].gameName + " " + hostData[i].guid);
        	}
                
    	}
 	}
}

function OnFailedToConnectToMasterServer(info : NetworkConnectionError) 
{
	// Count the failures
	BTS_FailedConnectionCount++;
	waitingForMasterServer--;
	
	Debug.Log(BTS_FailedConnectionCount + ": " + NetworkConnectionError);
	
	if (BTS_isHosting)
	{
	    consoleLog += "Could not connect to master server: " + BTS_FailedConnectionCount + " / " + info + "\n";

		// Try again
		if (BTS_FailedConnectionCount < BTS_FailedConnectionCountMax)
		{
			RetryBTSMasterServer();
		}
		else
		{
			consoleLog += "MasterServer is not available.\nPlease host your game on the LAN.\n";
		}
		
	}

}

function RetryBTSMasterServer()
{

	BTSMasterServer.ipAddress = BTS_MasterServerIP;
	Network.natFacilitatorIP = BTS_MasterServerIP;

	consoleLog += "Retrying MasterServer\n";
	
	BTSMasterServer.serverRestartScript = BTS_MasterServerRestartURLPath;
	BTSMasterServer.RestartMasterServer(Literals.lGameType, gameName , gameComment);
	
}

function LoadTheMainScene()
{

		if (Network.peerType == NetworkPeerType.Server)
		{
			Network.RemoveRPCsInGroup(0);
			Network.RemoveRPCsInGroup(1);
		}

		if ("" != networkLevelToLoad)
		{
			Application.LoadLevelAdditiveAsync(networkLevelToLoad);
		}
		ActivateTheObjects();
		DestroyTheObjects();
		
}

function NetworkLoadTheMainScene()
{

		Network.RemoveRPCsInGroup(0);
		Network.RemoveRPCsInGroup(1);
		
		if ("" != networkLevelToLoad)
		{
			BTSNetworkLoadLevel.LoadLevelAdditiveAsync(networkLevelToLoad);
		}
		
		ActivateTheObjects();
		DestroyTheObjects();
		
}

function DestroyTheObjects()
{

	Debug.Log("DestroyTheObjects()");
	
	// Wait for the player object
	while ( !GameObject.FindGameObjectWithTag("MainCharacter") )
		yield WaitForFixedUpdate();
		
	// Remove any objects that need to be reparented
	for (var t in parentToMainCameraOnLoad)
	{
		t.parent = null;
	}

	// Destroy objects that are no longer needed
	for ( var t in destroyOnLoad )
	{
		// Remove any tags from objects that are being destroyed
		// This is most important to remove the MainCamera tag
		t.gameObject.tag = "";
		
		// Destroy the objects, this will happen later so that is
		// why we removed the tags
		Destroy( t.gameObject );
	}
	
	
	// Find the main camera
	var l_mainCamera : GameObject = GameObject.FindGameObjectWithTag("MainCamera");
	
	if (null != l_mainCamera)
	{
		// Reparent the objects to the Main Camera
		for (var t in parentToMainCameraOnLoad)
		{
			t.parent = l_mainCamera.transform;
		}
	}

	PlayerDidConfirm();

}

function ActivateTheObjects()
{

	Debug.Log("ActivateTheObjects()");
			
	// Activate objects that are needed
	for ( var t in activateOnLoad )
	{	
		// Activate the object
		t.gameObject.SetActive(true);
	}
	
}
