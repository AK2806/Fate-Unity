using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameService.Util;
using FateUnity.Components.UI;
using FateUnity.Components.UI.Chat;

public class PlayerStatus : MonoBehaviour {
	public Image avatar;
	public Image avatarGrayMask;
	public Image lostConnectionIcon;
	public Image progressBar;
	public Text progressText;
	public ChatBubble chatBubble;
	public float chatWordLifetime = 0.5f;
	public float chatLeastLifetime = 3.0f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void DisplayLostConnection(bool visible) {
		avatarGrayMask.gameObject.SetActive(visible);
		lostConnectionIcon.gameObject.SetActive(visible);
	}

	public void DisplayProgress(bool visible) {
		avatarGrayMask.gameObject.SetActive(visible);
		progressBar.gameObject.SetActive(visible);
		progressText.gameObject.SetActive(visible);
	}

	public void SetProgress(float percent) {
		progressBar.fillAmount = percent;
		progressText.text = (int)(percent * 100) + "%";
	}

	public void SetAvatar(Sprite sprite, Vector2 pos, Vector2 scale) {
		avatar.sprite = sprite;
		avatar.SetNativeSize();
		var transform = avatar.GetComponent<RectTransform>();
		transform.anchoredPosition = pos;
		transform.localScale = scale;
	}

	public void ShowChatText(string text) {
		chatBubble.Show();
		chatBubble.SetChatText(text);
		StartCoroutine(AutoHideChatText(text.Length));
	}

	private IEnumerator AutoHideChatText(int wordCount) {
		yield return new WaitForSeconds(Mathf.Max(chatLeastLifetime, chatWordLifetime * wordCount));
		chatBubble.Hide();
	}
}
