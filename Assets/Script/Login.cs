using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour {
    private static readonly byte[][] verificationCode = new byte[][] {
		new byte[] { 0x00, 0x10, 0x20, 0xAB },
		new byte[] { 0x00, 0x10, 0x20, 0x3B },
		new byte[] { 0x00, 0x10, 0x20, 0xC5 },
		new byte[] { 0x00, 0x11, 0x33, 0xFA }
	};

	private Thread loginThread = null;
	private volatile bool loginResult = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		/*
		if (loginThread != null) {
			if (!loginThread.IsAlive) {
				if (loginResult) { // success
					SceneManager.LoadSceneAsync("MainScene");
				} else { // failure
					Debug.Log("Error login code.");
				}
				loginThread = null;
			}
			
		}
		 */
	}

	public void LoginAs(Dropdown dropdown) {
		switch (dropdown.value) {
			case 0:
				GameCore.Logger.WriteLine("DM");
				ConnectServer(0);
				break;
			case 1:
				GameCore.Logger.WriteLine("Player1");
				ConnectServer(1);
				break;
			case 2:
				GameCore.Logger.WriteLine("Player2");
				ConnectServer(2);
				break;
			case 3:
				GameCore.Logger.WriteLine("Player3");
				ConnectServer(3);
				break;
		}
	}

	private void ConnectServer(int userIndex) {
		var server = GameService.ServerProxy.GameServerProxy.Instance;
		bool result = server.EstablishConnection("127.0.0.1", verificationCode[userIndex]);
		if (result) SceneManager.LoadScene("MainScene");
		else Debug.Log("Error login code.");
		/*
		loginThread = new Thread(() => {
			var server = Main.Instance.Server;
			loginResult = server.EstablishConnection("127.0.0.1", verificationCode[userIndex]);
		});
		loginThread.Start();
		 */
	}
}
