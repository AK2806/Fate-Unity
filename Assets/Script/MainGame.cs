using System.Collections;
using System.Collections.Generic;
using GameCore.Network;
using GameService.ServerProxy;
using ClientComponentImpl;
using ClientComponentImpl.StoryScene;
using UnityEngine;

public sealed class MainGame : MonoBehaviour {
	private ResourceLoader _resLoader;
	private StorySceneController _storySceneController;

	public Camera mainCamera;

	// Use this for initialization
	private void Start () {
		_resLoader = new ResourceLoader();
		_storySceneController = new StorySceneController(mainCamera);
		GameServerProxy.Instance.StoryScene.SetController(_storySceneController);
	}
	
	// Update is called once per frame
	private void Update () {
		GameServerProxy.Instance.ExecuteServerCommands();
		_storySceneController.Update(Time.deltaTime);
	}
}
