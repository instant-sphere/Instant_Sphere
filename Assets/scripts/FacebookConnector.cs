using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using System;

/**
 * This class encapsulates calls to the Facebook API
 * You can connect, disconnect and share an Url/photo
 **/
public class FacebookConnector
{
    byte[] mPhotoToShare;
    Action<bool> mShareResultCallback;

    /**
     * Initiate a new connection with facebook in order to share the photo passed in parameter
     * Initialize facebook SDK if it is not otherwise call Connection()
     **/
    public void StartConnection(byte[] photo, Action<bool> shareResultCallback)
    {
        mShareResultCallback = shareResultCallback;
        mPhotoToShare = photo;
        if (!FB.IsInitialized)
            InitSDK();
        else
            Connection();
    }

    /**
     * Destroy the local copy of the photo and logout any connected user
     **/
    public void Disconnection()
    {
        mPhotoToShare = null;
        if (FB.IsInitialized && FB.IsLoggedIn)
            FB.LogOut();
    }

    /**
     * Initialize the facebook SDK and call Connection() after
     **/
    private void InitSDK()
    {
        if (!FB.IsInitialized)
            FB.Init(Connection);
    }

    /**
     * Prompt user for login into facebook with publish permissions
     * publish permissions will not be granted if app has not been validated by facebook
     **/
    private void Connection()
    {
        FB.ActivateApp();
        FB.LogInWithPublishPermissions(new List<string>() { "publish_actions" }, AuthCallback);
    }

    /**
     * Callback for authentication
     * Share a photo on user's journal
     **/
    private void AuthCallback(ILoginResult res)
    {
        if (FB.IsLoggedIn)
        {
            WWWForm data = new WWWForm();
            data.AddBinaryData("image", mPhotoToShare, "photo.jpg", "image/jpeg");
            data.AddField("allow_spherical_photo", "true");
            FB.API("me/photos", HttpMethod.POST, ShareCallback, data);
        }
        else
        {
            Debug.Log("Authentication canceled");
        }
    }

    /**
     * Callback for uploading the photo
     * Check if uploading was successful then disconnect the user
     **/
    private void ShareCallback(IGraphResult res)
    {
        if(res.Cancelled || !string.IsNullOrEmpty(res.Error))
            mShareResultCallback(false);
        else
            mShareResultCallback(true);
        Disconnection();
    }
}
