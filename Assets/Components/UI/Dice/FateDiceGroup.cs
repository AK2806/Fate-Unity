using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateUnity.Components.UI.Dice {
	public class FateDiceGroup : MonoBehaviour {
		public FateDice[] dices = new FateDice[4];

		// Use this for initialization
		void Start () {
			
		}
		
		// Update is called once per frame
		void Update () {
			
		}

		public void DisplayPoints(int[] dicePoints) {
			int length = Mathf.Min(dices.Length, dicePoints.Length);
			if (length < dices.Length) {
				for (int i = length; i < dices.Length; ++i) {
					dices[i].gameObject.SetActive(false);
				}
			}
			for (int i = 0; i < length; ++i) {
				dices[i].gameObject.SetActive(true);
				dices[i].DisplayPoint(dicePoints[i]);
			}
		}
	}
}
