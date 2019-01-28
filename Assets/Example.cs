using UnityEngine;
using UnityEngine.UI;
using Futilef;
using System.Collections;

using ImgAttr = Futilef.GpController.ImgAttr;

public class Example : MonoBehaviour {
	const int frameCounterSize = 500;
	int frameCounter;
	float lastCounterTime;

	GpController gpc;

	public Text text;

	IEnumerator TextAnimChange() {
		int i = 9;
		while (true) {
			if (++i >= 10) {
				i = 0;
				text.text = "";
			}
			text.text += i;
			yield return new WaitForSeconds(0.01f);
		}
	}

	void OnEnable() {
		Res.LoadAtlases(0, 1, 2);
		Res.LoadAtlases(1000, 1001, 1002);
		Res.LoadAtlases(2000, 2001, 2002, 2003);
		
		gpc = new GpController(Camera.main);

		gpc.AddImg(0, 1000);
		gpc.AddImg(1, 1001);
		gpc.AddImg(2, 1002);
		gpc.AddImg(3, 1003);
		gpc.AddImg(4, 1006);
		gpc.AddImg(5, 1007);
		gpc.AddImg(6, 1008);
		gpc.AddImg(7, 1009);
		gpc.AddImg(8, 1004);
		gpc.AddImg(9, 1005);
		gpc.AddImg(10, 1004);
		gpc.AddImg(11, 1005);
		gpc.AddImg(12, 1004);
		gpc.AddImg(13, 1005);

		gpc.SetImgAttr(0, ImgAttr.Position, 960f, 540f, 0f);
		gpc.SetImgAttr(0, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(1, ImgAttr.Position, 960f, 540f, -1f);
		gpc.SetImgAttr(1, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(2, ImgAttr.Position, 1336f, 301f, -2f);
		gpc.SetImgAttr(2, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(3, ImgAttr.Position, 1336f, 301f, -3f);
		gpc.SetImgAttr(3, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(4, ImgAttr.Position, 1678f, 866f, -1f);
		gpc.SetImgAttr(4, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(5, ImgAttr.Position, 1651f, 119f, -6f);
		gpc.SetImgAttr(5, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(6, ImgAttr.Position, 1651f, 119f, -7f);
		gpc.SetImgAttr(6, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(7, ImgAttr.Position, 688f, 401f, -3f);
		gpc.SetImgAttr(7, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(8, ImgAttr.Position, 167f, 183f, -4f);
		gpc.SetImgAttr(8, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(9, ImgAttr.Position, 167f, 183f, -4f);
		gpc.SetImgAttr(9, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(10, ImgAttr.Position, 714f, 189f, -4f);
		gpc.SetImgAttr(10, ImgAttr.Scale, 0.96f, 0.96f);
		gpc.SetImgAttr(10, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(11, ImgAttr.Position, 714f, 189f, -4f);
		gpc.SetImgAttr(11, ImgAttr.Scale, 0.96f, 0.96f);
		gpc.SetImgAttr(11, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(12, ImgAttr.Position, 1671f, 216f, -4f);
		gpc.SetImgAttr(12, ImgAttr.Scale, 0.78f, 0.78f);
		gpc.SetImgAttr(12, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(13, ImgAttr.Position, 1671f, 216f, -4f);
		gpc.SetImgAttr(13, ImgAttr.Scale, 0.78f, 0.78f);
		gpc.SetImgAttr(13, ImgAttr.Alpha, 1f);

		gpc.Wait(3f);
		gpc.SetImgAttrEased(1, ImgAttr.Tint, 0.5f, EsType.CubicIn, 1.0f, 0.4f, 0f);
		gpc.SetImgAttrEased(3, ImgAttr.Tint, 0.5f, EsType.CubicIn, 1.0f, 0.4f, 0f);
		gpc.SetImgAttrEased(6, ImgAttr.Tint, 0.5f, EsType.CubicIn, 1.0f, 0.4f, 0f);
		gpc.SetImgAttrEased(4, ImgAttr.Tint, 0.5f, EsType.CubicIn, 1.0f, 0.4f, 0f);

		StartCoroutine(TextAnimChange());
		/*
		gpc.Wait(0.8f);
		gpc.SetImgAttrEased(1, ImgAttr.Tint, 0.5f, EsType.CubicOut, 1.0f, 1.0f, 1.0f);
		gpc.SetImgAttrEased(3, ImgAttr.Tint, 0.5f, EsType.CubicOut, 1.0f, 1.0f, 1.0f);
		gpc.SetImgAttrEased(6, ImgAttr.Tint, 0.5f, EsType.CubicOut, 1.0f, 1.0f, 1.0f);
		gpc.SetImgAttrEased(4, ImgAttr.Tint, 0.5f, EsType.CubicOut, 1.0f, 1.0f, 1.0f);
		 */
		/*
		gpc.AddImg(0, 2000);
		gpc.AddImg(1, 2001);
		gpc.AddImg(2, 2002);
		gpc.AddImg(3, 2003);
		gpc.AddImg(4, 2004);
		gpc.AddImg(5, 2005);
		gpc.AddImg(6, 2005);
		gpc.AddImg(7, 2005);
		gpc.AddImg(8, 2005);
		gpc.AddImg(9, 2005);
		gpc.AddImg(10, 2006);

		gpc.SetImgAttr(0, ImgAttr.Position, 640f, 540f, 0f);
		gpc.SetImgAttr(0, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(1, ImgAttr.Position, 1920f, 540f, 0f);
		gpc.SetImgAttr(1, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(2, ImgAttr.Position, 3200f, 540f, 0f);
		gpc.SetImgAttr(2, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(3, ImgAttr.Position, 1809f, 363f, -1f);
		gpc.SetImgAttr(3, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(4, ImgAttr.Position, 1809f, 363f, -1f);
		gpc.SetImgAttr(5, ImgAttr.Position, 615f, 364f, -1f);
		gpc.SetImgAttr(5, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(6, ImgAttr.Position, 986f, 364f, -1f);
		gpc.SetImgAttr(6, ImgAttr.Scale, 0.7f, 0.7f);
		gpc.SetImgAttr(6, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(7, ImgAttr.Position, 1291f, 364f, -1f);
		gpc.SetImgAttr(7, ImgAttr.Scale, 0.46f, 0.46f);
		gpc.SetImgAttr(7, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(8, ImgAttr.Position, 2716f, 364f, -1f);
		gpc.SetImgAttr(8, ImgAttr.Scale, -0.7f, 0.7f);
		gpc.SetImgAttr(8, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(9, ImgAttr.Position, 3089f, 364f, -1f);
		gpc.SetImgAttr(9, ImgAttr.Scale, -1f, 1f);
		gpc.SetImgAttr(9, ImgAttr.Alpha, 1f);
		gpc.SetImgAttr(10, ImgAttr.Position, 1749f, 552f, -2f);
		
		gpc.Wait(3);
		gpc.SetImgAttrEased(10, ImgAttr.Alpha, 1.0f, EsType.CubicIn, 1.0f);
		gpc.SetImgAttrEased(4, ImgAttr.Alpha, 1.0f, EsType.CubicIn, 1.0f);
		*/

		/*
		for (int i = 0; i < 1; i += 1) {
			gpc.AddImg(1, 4);
			
			gpc.SetImgAttr(1, ImgAttr.Position, 0f, 0f, 0f);
			gpc.SetImgAttr(1, ImgAttr.Rotation, 0f);·
			gpc.SetImgAttr(1, ImgAttr.Alpha, 1f);
			gpc.SetImgAttrEased(1, ImgAttr.Scale, 1f, EsType.ElasticOut, 0.01f, 0.01f);
			gpc.Wait(.5f);
			gpc.SetImgAttrEased(1, ImgAttr.Tint, 1.5f, EsType.ElasticOut, 1f, 0.5f, 1f);
			gpc.SetImgAttrEased(1, ImgAttr.Position, 2f, EsType.ElasticOut, 2f, -1f, 0f);
			gpc.Wait();
			gpc.SetImgAttrEased(1, ImgAttr.Tint, 1.5f, EsType.ElasticOut, 1f, 1f, 1f);
			gpc.SetImgAttrEased(1, ImgAttr.Position, 2f, EsType.ElasticOut, -2f, 2f, 0f);
			gpc.SetImgAttrEased(1, ImgAttr.Rotation, 1.5f, EsType.ElasticOut, Mathf.PI * 2.5f);

			gpc.Wait(.5f);
			gpc.AddImg(2, 1);
			gpc.SetImgAttr(2, ImgAttr.Position, 0f, 0f, -5f);
			gpc.SetImgAttr(2, ImgAttr.Rotation, 0f);
			gpc.SetImgAttr(2, ImgAttr.Scale, 0.1f, 0.1f);
			gpc.SetImgAttr(2, ImgAttr.Alpha, 1f);
			gpc.SetImgAttrEased(2, ImgAttr.Scale, 1f, EsType.ElasticOut, 0.006f, 0.006f);
			gpc.SetImgAttrEased(2, ImgAttr.Position, 4f, EsType.ElasticOut, -2f, 2f, 0f);

			gpc.Wait();
			gpc.SetImgAttrEased(1, ImgAttr.Tint, 1f, EsType.ElasticOut, 1.5f, 1.5f, 1.5f);

		}
 		*/
	}

	void Update() {
		if (gpc != null) gpc.Update(Time.deltaTime);
	}
	 
	void OnDisable() {
		Debug.Log("Clean up ");
		gpc.Dispose();
	}
}
