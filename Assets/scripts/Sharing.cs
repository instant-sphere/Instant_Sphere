using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Sharing : MonoBehaviour
{
    private string token;
    private string token_img;
    private bool auth;

    public void SendToServer(byte[] img)
    {
        StartCoroutine(Upload(img));
    }

    public void SendToServerMail(string mail)
    {
        StartCoroutine(Email(mail));
    }

    public void SendToServerAuthentification(string android_id)
    {
        StartCoroutine(Authentification(android_id));
    }

    public void SendToServerDemandeToken(string android_id)
    {
        StartCoroutine(DemandeToken(android_id));
    }


    private IEnumerator Upload(byte[] img)
    {
        WWWForm form = new WWWForm();

        form.AddBinaryData("imgUploader", img, "photo.jpg", "image/jpeg");

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/Upload", form);
        www.SetRequestHeader("x-access-token", token);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            token_img = null;
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            token_img = www.downloadHandler.text.Substring(20, 5);
        }
    }

    private IEnumerator Email(string mail)
    {
        WWWForm form = new WWWForm();

        form.AddField("mail", mail);
        form.AddField("token_img", token_img);

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/email", form);
        www.SetRequestHeader("x-access-token", token);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
            Debug.Log(www.downloadHandler.text);
    }

    public string GetToken_img()
    {
        return token_img;
    }

    private IEnumerator Authentification(string id_tablette)
    {
        WWWForm form = new WWWForm();

        form.AddField("id_tablette", id_tablette);

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/enregistrement", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
        {
            Debug.Log(www.downloadHandler.text);
            if (www.downloadHandler.text.Substring(11, 4) == "true")
            {
                auth = true;
            }
            else
            {
                auth = false;
            }
        }
    }

    public bool GetAuth()
    {
        return auth;
    }

    private IEnumerator DemandeToken(string id_tablette)
    {
        WWWForm form = new WWWForm();

        form.AddField("id_tablette", id_tablette);

        UnityWebRequest www = UnityWebRequest.Post("http://server.instant-sphere.com:333/api/demandetoken", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
        {
            Debug.Log(www.downloadHandler.text);
            if (www.downloadHandler.text.Substring(11, 4) == "true")
            {
                token = www.downloadHandler.text.Substring(59, 139);
                Debug.Log(token);
            }
            else
            {
                token = null;
            }
        }
    }

    public string GetToken()
    {
        return token;
    }

    public void SetToken(string _token)
    {
        token = _token;
    }

}
