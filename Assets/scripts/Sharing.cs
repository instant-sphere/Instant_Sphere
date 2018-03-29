using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Sharing : MonoBehaviour
{
    private string token;

    public void SendToServer(byte[] img)
    {
        StartCoroutine(Upload(img));
    }

    public void SendToServerMail(string mail)
    {
        StartCoroutine(Email(mail));
    }

    private IEnumerator Upload(byte[] img)
    {
        WWWForm form = new WWWForm();
        form.AddBinaryData("imgUploader", img, "photo.jpg", "image/jpeg");

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/Upload", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            token = null;
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            token = www.downloadHandler.text.Substring(20, 5);
        }
    }

    private IEnumerator Email(string mail)
    {
        WWWForm form = new WWWForm();
        //
        //form.AddBinaryData("Mail", System.Text.Encoding.UTF8.GetBytes(mail), "mail", "text/plain");
//        form.Add(new MultipartFormDataSection("mail", mail));

        form.AddField("mail", mail);

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/email", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
            Debug.Log(www.downloadHandler.text);
    }

    public string GetToken()
    {
        return token;
    }
}
