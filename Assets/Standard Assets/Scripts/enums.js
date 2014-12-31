//Enums are declared in this file to prevent namespace conflicts
//To expose an enum dropdown in the inspector create a serialized static var of type enum_name
//Note: Unity preprocessor does not currently handle custom #if #endif
//if they ever start supporting it these values canbe used to reduce the binary size, otherwise do runtime checks
/*
public enum BTDebug
{
	controls = false,
	GUI = false,
	localMessages = false,
	networkMessages = false,
	weapons = false,
	damage = false,
	targeting = false,
}
*/
static public class BTDebug
{
	 var controls = false;
	 var GUI = false;
	 var localMessages = false;
	 var networkMessages = false;
	 var weapons = false;
	 var damage = false;
	 var targeting = false;
}

public enum BTMessageStatus
{
	success,
	unknown,
	
	nonExistantListener,
	failedLocally,
	partiallyFailedLocally,
	
	clientUnreachable,
	someClientsUnrechable,
}	

public enum GameState
{
	Deathmatch,
	CTF,
	KOH,
	Tug,	
}

enum e_menu{root,network, join, host, hosting}
enum e_garagemenu{equip,arm,buy}

// This is a variable that helps us
// identify if we are running in the 
// editor or on a real device
enum touchtexturePlatform { Mobile, MacOSX, Windows }

// These are required for the Store Kit interface
enum SKPaymentTransactionState {Purchasing, Purchased, Failed, Restored};

