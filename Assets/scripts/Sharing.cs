using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Sharing : MonoBehaviour
{
    public void SendToServer(byte[] img)
    {
        StartCoroutine(Upload(img));
    }

    private IEnumerator Upload(byte[] img)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("imgUploader", img, "photo.jpg", "image/jpeg");

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/Upload", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
            Debug.Log(www.downloadHandler.text);
    }

}
