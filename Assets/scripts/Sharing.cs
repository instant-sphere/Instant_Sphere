using UnityEngine;
using System.Collections;
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
		form.AddField("somefield", "somedata");

		form.AddBinaryData ("imaaage", img, "immg", "multipart/form-data" );
		Debug.Log ("********************************** SendToServe*******");
		UnityWebRequest www = UnityWebRequest.Post("https://server.instant-sphere.com:333/api/Upload", form);
		yield return www.SendWebRequest();

		if(www.isNetworkError || www.isHttpError) {
			Debug.Log(www.error);
		}
		else {
			Debug.Log(www.downloadHandler.text);
		}
	}
		 
}
