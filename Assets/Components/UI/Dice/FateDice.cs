using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FateUnity.Components.UI.Dice {
	public class FateDice : MonoBehaviour {
		public Sprite plusImage;
		public Sprite minusImage;
		public Sprite zeroImage;

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void DisplayPoint(int value) {
			var imageComp = GetComponent<Image>();
			if (value < 0) imageComp.sprite = minusImage;
			else if (value > 0) imageComp.sprite = plusImage;
			else imageComp.sprite = zeroImage;
		}
	}

}
