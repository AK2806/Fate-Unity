using System.Collections;
using System.Collections.Generic;
using GameCore.Network;
using GameService.ServerProxy;
using FateUnity.ClientComponentImpl;
using FateUnity.ClientComponentImpl.StoryScene;
using UnityEngine;

public sealed class MainGame : MonoBehaviour {

	// Use this for initialization
	private void Start () {
		
	}
	
	// Update is called once per frame
	private void Update () {
		GameServerProxy.Instance.ExecuteServerCommands();
	}
}
