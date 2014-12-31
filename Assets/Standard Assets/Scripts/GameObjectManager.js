static var managedObjects : Hashtable;

static function Keep(a_gameObject : GameObject, a_isNetwork : int)
{

	Debug.Log(a_gameObject);

	if (null == managedObjects)
	{
		managedObjects = new Hashtable();
	}
	
	var l_uid : int = a_gameObject.GetInstanceID();
	
	var l_objectHash : Hashtable = new Hashtable();
	l_objectHash.Add("gameobject", a_gameObject);
	l_objectHash.Add("isnetwork", a_isNetwork);
	
	
	if (managedObjects.Contains(l_uid))
	{
		Debug.Log("Tried to keep the same object twice: " + a_gameObject);
		return;
	}

	DontDestroyOnLoad(a_gameObject);
	managedObjects.Add(l_uid, l_objectHash);

}

static function DestroyAll ()
{

	var l_gameObject : GameObject;
	var l_isNetwork : int;
	
	for (var l_objectHash : Hashtable in managedObjects.Values)
	{
	
		l_gameObject = l_objectHash["gameobject"];
		l_isNetwork = l_objectHash["isnetwork"];
		
		if (l_gameObject)
		{
		
			var l_networkView : NetworkView = l_gameObject.networkView;
				
			if (l_isNetwork && (l_networkView))
			{
				if (l_networkView.isMine)
				{
//					Network.RemoveRPCs(l_networkView.viewID);
					Network.Destroy(l_gameObject);
				}
			}
			else
			{
				Destroy(l_gameObject);
			}
		}
	}

	managedObjects.Clear();

}