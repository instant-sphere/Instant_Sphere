using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine.Networking;
public class Sharing : MonoBehaviour{
	    UnityWebRequestAsyncOperation mWebRequest;  //object representing current connection for regular HTTP requests

	

	public void SendToServer(byte[] img) {
		StartCoroutine(Upload(img));
	}
	public IEnumerator Upload(byte[] img){
		WWWForm form = new WWWForm();
		byte[] tab = new byte[3];
		form.AddBinaryData("imgUploader", img, "photo.jpg", "image/jpeg");




		Debug.Log ("********************************** SendToServe*******");
		UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/Upload", form);
        
		yield return www.SendWebRequest();

		if(www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		}
		else {
			Debug.Log(www.downloadHandler.text);
		}
	}
		 
}
