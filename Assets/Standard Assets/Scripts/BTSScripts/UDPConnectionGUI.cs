using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPConnectionGUI : MonoBehaviour 
{
	
	public int datagramBroadcastInterval = 100;
	public float serverDownSeconds = 1.0f;
    public string port = "6768";
    public int listenPort = 25002;
    public string server_name = "";
	private UdpClient server;
	private UdpClient client;
  	private IPEndPoint receivePoint;
//     private string ip = "0.0.0.0";
     private string ip_broadcast = "255.255.255.255";
     private bool isServer = false;
     private bool connected = false;
//	private int clear_list = 0;
	
	private System.DateTime lastDatagramTime;
	
	private byte[] sendData;
	
	public void Update() 
	{
		if (System.DateTime.Now > lastDatagramTime.AddSeconds(serverDownSeconds))
		{
			server_name = "";
		}
	}	
	
	public void Start() 
	{
		Debug.Log("Start");
		
		// There is no server
		server_name = "";
		
		// Everyone is listening
		LoadClient();
	}

	public void LoadClient() 
	{
		// Create a new UDP Client on the Mini-Golf UDP Port
		// Probably the user should get to specify a different port
		client = new UdpClient(System.Convert.ToInt32(port));
		
		// The receivePoint will be returned when a datagram is 
		// received so it can start out as null
		receivePoint = null;
		
		// Since we will use client.Receive, which blocks, we run it
		// on a thread so that the program does not block
		Thread clientThread = new Thread(new ThreadStart(datagram_receive_client));
   		clientThread.Start();
   	}
	
	public void datagram_receive_client() 
	{
      	bool continueLoop = true;
      
     	try
      	{
          	while (continueLoop)
          	{
				// receivePoint tell us which host sent the datagram
				// This call blocks
          		byte[] recData = client.Receive(ref receivePoint);
          		
          		System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
          		
          		server_name = encode.GetString(recData);
				lastDatagramTime = System.DateTime.Now; 
          		
          		if (connected)
          		{
          			server_name = "";
  	  				if (null != client)
					{
          				client.Close();
						client = null;
					}
					
          			break;
          		}
          	}
			
      	} catch {}
    }
    
    public void start_server() 
	{
    	try
    	{
        	while (true)
        	{
          		server.Send(sendData,sendData.Length,ip_broadcast,System.Convert.ToInt32(port));
          		Thread.Sleep(datagramBroadcastInterval);
        	}
			
    	} catch {}
    }
    
	void OnGUI() 
	{

		if (!isServer)
		{
			if(GUI.Button(new Rect(10,10,120,30),"Start Server"))
			{
      			isServer = true;
      			Network.InitializeServer(32, listenPort, true);
      			string ipaddress = Network.player.ipAddress.ToString();	    	
//  	  			ip = ipaddress;
				
				System.Text.ASCIIEncoding encode = new System.Text.ASCIIEncoding();
          		sendData = encode.GetBytes(Network.player.ipAddress.ToString());
				
  	  			if (null != client)
				{
		    		client.Close();
					client = null;
				}
      			server = new UdpClient(System.Convert.ToInt32(port));
      			receivePoint = new IPEndPoint(IPAddress.Parse(ipaddress),System.Convert.ToInt32(port));
      			Thread startServer = new Thread(new ThreadStart(start_server));
      			startServer.Start();

			}
			
			if (server_name != "")
			{
				if(GUI.Button(new Rect(20,100,200,50),server_name))
				{
					connected = true;
					Network.Connect(server_name, listenPort);	
				}
			}
		}
		else
		{
			if (GUI.Button(new Rect(10,10,120,30),"Disconnect"))
			{
				Network.Disconnect();
				isServer = false;
				
				if (null != server)
				{
					server.Close();
					server = null;
				}
				
				LoadClient();
			}
		}
	}
}
