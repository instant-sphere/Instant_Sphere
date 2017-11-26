using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using System;

/**
 * This class encapsulates calls to the Facebook API
 * You can connect, disconnect and share an Url
 **/
public class facebook
{
    private void InitSDK()
    {
        if (!FB.IsInitialized)
            FB.Init(Connection);
        else
            FB.ActivateApp();
    }

    public void Disconnection()
    {
        if (FB.IsInitialized && FB.IsLoggedIn)
            FB.LogOut();
    }

    public void StartConnection()
    {
        if (!FB.IsInitialized)
            InitSDK();
        else
            Connection();
    }

    private void Connection()
    {
        FB.ActivateApp();
        FB.LogInWithPublishPermissions(new List<string>() { "publish_actions" }, AuthCallback);
    }

    private void AuthCallback(ILoginResult res)
    {
        if(FB.IsLoggedIn)
        {
            FB.ShareLink(contentURL: new Uri("http://www.instant-sphere.com/"));
        }
        else
        {
            Debug.Log("Authentification canceled");
        }
    }
}
