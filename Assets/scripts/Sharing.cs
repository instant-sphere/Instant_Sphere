using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using LitJson;
using System;

/**
 * This class handles communication with the API
 * For uploading photo, authenticate tablet, send email
 **/
public sealed class Sharing : MonoBehaviour
{
    string mTabletToken;
    string mImageToken;
    bool mIsAuthenticated;

    // static string URL = "https://server.instant-sphere.com";
    static string URL = "http://127.0.0.1:3333";

    /**
     * Uploads a photo to the server
     **/
    public void SendToServer(byte[] img)
    {
        StartCoroutine(Upload(img));
    }

    /**
     * Asks the server to send a email
     **/
    public void SendToServerMail(string mail)
    {
        StartCoroutine(Email(mail));
    }

    /**
     * Communicates the tablet Android ID to the server
     **/
    public void SendToServerAuthentication(string androidID)
    {
        StartCoroutine(Authentication(androidID));
    }

    /**
     * Asks the server our token
     **/
    public void SendToServerAskToken(string androidID)
    {
        StartCoroutine(DemandeToken(androidID));
    }

    /**
     * Coroutine that uploads a photo on the server and retrieves the corresponding token
     **/
    private IEnumerator Upload(byte[] img)
    {
        mImageToken = null;
        WWWForm form = new WWWForm();

        form.AddBinaryData("imgUploader", img, "photo.jpg", "image/jpeg");

        UnityWebRequest www = UnityWebRequest.Post(URL + "/api/Upload", form);
        www.SetRequestHeader("x-access-token", mTabletToken);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error + www.downloadHandler.text);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            try
            {
                JsonData response = HttpRequest.JSONStringToDictionary(www.downloadHandler.text);
                mImageToken = response["code"].ToString();
            }
            catch (Exception e)
            {
                Debug.Log("Error uploading photo :" + e.Message);
                throw;
            }
        }
    }

    /**
     * Coroutine that asks the server to send an email
     **/
    private IEnumerator Email(string mail)
    {
        WWWForm form = new WWWForm();

        form.AddField("mail", mail);
        form.AddField("token_img", mImageToken);

        UnityWebRequest www = UnityWebRequest.Post(URL + "/api/email", form);
        www.SetRequestHeader("x-access-token", mTabletToken);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
            Debug.Log(www.downloadHandler.text);
    }

    /**
     * Returns the photo token
     **/
    public string GetTokenImg()
    {
        return mImageToken;
    }

    /**
     * Coroutine that registers the tablet on the server
     **/
    private IEnumerator Authentication(string idTablet)
    {
        WWWForm form = new WWWForm();

        form.AddField("id_tablette", idTablet);

        UnityWebRequest www = UnityWebRequest.Post(URL + "/enregistrement", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
        {
            Debug.Log("Auth :" + www.downloadHandler.text);
            try
            {
                JsonData response = HttpRequest.JSONStringToDictionary(www.downloadHandler.text);
                if (response["success"].ToString() == "True")
                    mIsAuthenticated = true;
                else
                    mIsAuthenticated = false;
            }
            catch (Exception e)
            {
                mTabletToken = null;
                Debug.Log("Error when authenticate :" + e.Message);
            }
        }
    }

    /**
     * Returns true if the tablet is authenticated
     **/
    public bool GetAuth()
    {
        return mIsAuthenticated;
    }

    /**
     * Coroutine that asks the token to the server
     **/
    private IEnumerator DemandeToken(string idTablette)
    {
        WWWForm form = new WWWForm();

        form.AddField("id_tablette", idTablette);

        UnityWebRequest www = UnityWebRequest.Post(URL + "/api/demandetoken", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
            Debug.Log(www.error);
        else
        {
            Debug.Log("Token :" + www.downloadHandler.text);
            try
            {
                JsonData response = HttpRequest.JSONStringToDictionary(www.downloadHandler.text);
                if (response["success"].ToString() == "True")
                    mTabletToken = response["token"].ToString();
                else
                    mTabletToken = null;
            }
            catch(Exception e)
            {
                mTabletToken = null;
                Debug.Log("Error when getting token :" + e.Message);
            }
        }
    }

    /**
     * Returns the tablet token
     **/
    public string GetToken()
    {
        return mTabletToken;
    }

    /**
     * Sets the tablet token
     **/
    public void SetToken(string token)
    {
        mTabletToken = token;
    }
}
