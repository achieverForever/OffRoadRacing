using UnityEngine;
using System.Collections;

public class NetworkMgr : MonoBehaviour {

	public GameObject playerPrefabYellow;
	public GameObject playerPrefabBlack;
	public GameObject playerPrefabWhite;
	public GameObject playerPrefabRed;
	public Transform spawnPoint;

	private float groupX;
	private float groupY;
	private float groupWidth = 400.0f;
	private float groupHeight = Screen.height;
	private float btnWidth = 300.0f;
	private float btnHeight = 50.0f;
	private float labelWidth = 120.0f;
	private float labelHeight = 20.0f;
	private float spacing = 10.0f;
	
	private bool isJoin, isHost, isQuit, showMainMenu, showConnInfo, showMsg, isPauseMenuEnabled;
	private string connIP = "127.0.0.1";
	private int connPort = 26500;
	private int maxConnetions = 8;
	private int hostPort = 26500;
	private bool useNAT = true;
	private int selectedColor;
	private string message;
	private PauseMgr pauseMgr = null;


	// Use this for initialization
	void Start () {
		groupX = Screen.width * 0.5f - btnWidth * 0.5f;
		groupY = 50.0f;
		useNAT = false;
		showMainMenu = true;
		pauseMgr = GameObject.FindWithTag("PauseManager").GetComponent<PauseMgr>();
		if(!pauseMgr)
		{
			Debug.LogError("PauseManager Not Found");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI()
	{		
		GUI.BeginGroup(new Rect(groupX, groupY, groupWidth, groupHeight));

		// Main Menu
		if(showMainMenu && !(isJoin || isHost || isQuit)) 
		{
			if(GUI.Button(new Rect(0.0f, 0.0f, btnWidth, btnHeight), "Join Game"))
			{
				isJoin = true;
				isHost = isQuit = false;
			}
			if(GUI.Button(new Rect(0.0f, btnHeight + spacing, btnWidth, btnHeight), "Host Game"))
			{
				isHost = true;
				isJoin = isQuit = false;
			}
			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 2.0f, btnWidth, btnHeight), "Quit"))
			{
				isQuit = true;
				isJoin = isHost = false;
				Application.Quit();
			}
		}

		// Join Game Menu 
		if(isJoin)
		{
			showMainMenu = false;	// Don't show Main Menu

			GUI.Label(new Rect(0.0f, 0.0f, labelWidth, labelHeight), "Server IP");
			connIP = GUI.TextField(new Rect(labelWidth + spacing, 0.0f, 
				btnWidth, btnHeight), connIP);

			GUI.Label(new Rect(0.0f, (btnHeight + spacing), labelWidth, labelHeight), "Port");
			try
			{
				connPort = int.Parse( GUI.TextField(new Rect(labelWidth + spacing, btnHeight + spacing, 
					btnWidth, btnHeight), connPort.ToString()) );
			}
			catch(System.Exception ex)
			{
				message = "Please Enter A Valid Port Number (1025 - 65535)";
				connPort = 26500;
				StartCoroutine(ShowMsgForSeconds(5.0f));
			}

			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Yellow"))
			{
				selectedColor = 0;
			}

			if(GUI.Button(new Rect(btnWidth * 0.3f + 8.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Black"))
			{
				selectedColor = 1;
			}

			if(GUI.Button(new Rect((btnWidth * 0.3f + 8.0f)*2.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "White"))
			{
				selectedColor = 2;
			}

			if(GUI.Button(new Rect((btnWidth * 0.3f + 8.0f)*3.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Red"))
			{
				selectedColor = 3;
			}

			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 3.0f, 
				btnWidth, btnHeight), "Connect"))
			{
				// Connect to a server
				Network.Connect(connIP, connPort);
				message = "Connecting to Server";
				StartCoroutine(ShowMsgForSeconds(5.0f));
				isJoin = false;	// Hide the Join Game Menu
				showConnInfo = true;	// Show connection information
				pauseMgr.enabled = true;
			}

			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 4.0f, 
				btnWidth, btnHeight), "Back"))
			{
				// Go back to main menu
				showMainMenu = true;
				isJoin = isHost = isQuit = false;
			}
		}

		// Host Game Menu
		if(isHost)
		{
			showMainMenu = false;

			GUI.Label(new Rect(0.0f, 0.0f, labelWidth, labelHeight), "Maximum Player Number");
			try
			{
				maxConnetions = int.Parse( GUI.TextField(new Rect(labelWidth + spacing, 0.0f,
					btnWidth, btnHeight), maxConnetions.ToString()) );
			}
			catch(System.Exception ex)
			{
				message = "Please Enter A Valid Maximum Player Number (1 - 32)";
				maxConnetions = 8;
				StartCoroutine(ShowMsgForSeconds(5.0f));
			}

			GUI.Label(new Rect(0.0f, (btnHeight + spacing), labelWidth, labelHeight), "Listen Port");
			try
			{
				hostPort = int.Parse( GUI.TextField(new Rect(labelWidth + spacing, btnHeight + spacing, 
					btnWidth, btnHeight), hostPort.ToString()) );
			}
			catch(System.Exception ex)
			{
				message = "Please Enter A Valid Port Number To Listen (1025 - 65535)";
				hostPort = 26500;
				StartCoroutine(ShowMsgForSeconds(5.0f));
			}


			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Yellow"))
			{
				selectedColor = 0;
			}

			if(GUI.Button(new Rect(btnWidth * 0.3f + 8.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Black"))
			{
				selectedColor = 1;
			}

			if(GUI.Button(new Rect((btnWidth * 0.3f + 8.0f)*2.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "White"))
			{
				selectedColor = 2;
			}

			if(GUI.Button(new Rect((btnWidth * 0.3f + 8.0f)*3.0f, (btnHeight + spacing) * 2.0f,
				btnWidth * 0.3f, btnHeight), "Red"))
			{
				selectedColor = 3;
			}


			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 3.0f,
				btnWidth, btnHeight), "Start Up Server"))
			{
				// Initialize a server so that other players can connect to.
				Network.InitializeServer(maxConnetions, hostPort, useNAT);
				isHost = false;	// Hide Host Game Menu
				showConnInfo = true;	// Show connected players' information
				pauseMgr.enabled = true;
			}

			if(GUI.Button(new Rect(0.0f, (btnHeight + spacing) * 4.0f, 
				btnWidth, btnHeight), "Back"))
			{
				// Go back to main menu
				showMainMenu = true;
				isJoin = isHost = isQuit = false;
			}
		}

		GUI.EndGroup();

		if(showConnInfo)
		{
			if(Network.isServer)
			{
				GUI.Label(new Rect(10.0f, 10.0f, 200.0f, 20.0f), "Running Status: Server");

				GUI.Label(new Rect(10.0f, 30.0f, 200.0f, 20.0f), "Connected Players: " + Network.connections.Length);
			
				if(Network.connections.Length > 0)
				{
					GUI.Label(new Rect(10.0f, 50.0f, 200.0f, 25.0f), "Ping to First Player: "
						+ Network.GetAveragePing(Network.connections[0]) + " ms");					
				}

			}else if(Network.isClient)
			{
				GUI.Label(new Rect(10.0f, 10.0f, 200.0f, 20.0f), "Running Status: Client");

				if(Network.connections.Length > 0)
				{
					GUI.Label(new Rect(10.0f, 30.0f, 200.0f, 25.0f), "Ping to Server: "
						+ Network.GetAveragePing(Network.connections[0]) + " ms");					
				}
			}
		}

		// Display notifications.
		if(showMsg && message != "")
		{
			GUI.Label(new Rect(10.0f, 80.0f, 400.0f, 20.0f), message);
		}

	}


	// Events when running as a server
	void OnServerInitialized()
	{
		message = "Server has been Initialized";
		StartCoroutine(ShowMsgForSeconds(5.0f));
		Debug.Log(message);
		StartCoroutine(SpawnInSeconds(0.5f));	

	}

	void OnPlayerConnected(NetworkPlayer player)
	{
		message = "Player from " + player.ipAddress + ":" + player.port + " Connected";
		StartCoroutine(ShowMsgForSeconds(5.0f));
		Debug.Log(message);
	}

	void OnPlayerDisconnected(NetworkPlayer player)
	{
		message = "Player from " + player.ipAddress + ":" + player.port + " Disconnected";
		StartCoroutine(ShowMsgForSeconds(5.0f));
		Debug.Log(message);
		Debug.Log("Clean up that player: " + player);
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}


	// Events when running as a client
	void OnConnectedToServer()
	{
		message = "Connected to Server Successfully";
		StartCoroutine(ShowMsgForSeconds(5.0f));
		Debug.Log(message);
		StartCoroutine(SpawnInSeconds(0.5f));	
	}

	void OnFailedToConnect(NetworkConnectionError err)
	{
		message = "Failed to Connect to the Server: " + err;
		StartCoroutine(ShowMsgForSeconds(8.0f));
		Debug.Log(message);
	}

	// Note: This event is called not only on client but also on the server
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
		if(Network.isClient)
		{	// Are we a client?
			message = "Lost Connection to the Server, the Server may have Shut down!";
		}
		else
		{	// Are we a server?
			message = "Disconnected all Players to this Server";
		}
		StartCoroutine(ShowMsgForSeconds(5.0f));
		Debug.Log(message);
		//Network.RemoveRPCs(Network.player);
		// Network.DestroyPlayerObjects(Network.player);
	}

	void SpawnAndSetupPlayer()
	{
		Vector3 spawnPos = new Vector3(spawnPoint.position.x + Random.Range(-8.0f, 8.0f), spawnPoint.position.y, spawnPoint.position.z);
		GameObject newPlayer = null;
		switch(selectedColor)
		{
			case 0:
				newPlayer = (GameObject) Network.Instantiate(playerPrefabYellow, spawnPos, spawnPoint.rotation, 0);			
				break;
			case 1:
				newPlayer = (GameObject) Network.Instantiate(playerPrefabBlack, spawnPos, spawnPoint.rotation, 0);			
				break;
			case 2:
				newPlayer = (GameObject) Network.Instantiate(playerPrefabWhite, spawnPos, spawnPoint.rotation, 0);			
				break;
			case 3:
				newPlayer = (GameObject) Network.Instantiate(playerPrefabRed, spawnPos, spawnPoint.rotation, 0);			
				break;
			default:
				break;
		}

		if(newPlayer.GetComponent<NetworkView>().isMine)
		{
			GameObject camMgr = GameObject.FindWithTag("CameraManager");
			GameObject hud = GameObject.FindWithTag("HUD");
			GameObject skidmark = GameObject.FindWithTag("Skidmark");

			// Setup cameras
			if(camMgr)
			{
				camMgr.GetComponent<SetupCameras>().isNewTarget = true;
				camMgr.GetComponent<SetupCameras>().target = newPlayer.transform;
			}
			else
			{
				Debug.LogWarning("Camera Manager Not Found");
			}

			// Setup HUDs
			if(hud)
			{
				hud.GetComponent<SetupHUD>().isNewTarget = true;
				hud.GetComponent<SetupHUD>().target = newPlayer;
			}
			else
			{
				Debug.LogWarning("HUD Not Found");
			}

			// Setup skidmark and skidsmoke
			if(skidmark)
			{
				newPlayer.GetComponent<PlayerCar>().skidmark = skidmark.GetComponent<Skidmarks>();
				newPlayer.GetComponent<PlayerCar>().skidSmoke = skidmark.transform.GetChild(0).GetComponent<ParticleEmitter>();
			}
			else
			{
				Debug.LogWarning("Skidmark Not Found");			
			}

		}
	}

	IEnumerator SpawnInSeconds(float time)
	{
		yield return new WaitForSeconds(time);
		SpawnAndSetupPlayer();
		Debug.Log("A new player has been spawned");
	}

	IEnumerator ShowMsgForSeconds(float time)
	{
		showMsg = true;
		yield return new WaitForSeconds(time);
		showMsg = false;
	}
}
