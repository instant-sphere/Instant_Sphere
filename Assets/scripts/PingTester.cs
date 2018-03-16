using System.Collections;
using System.Net;
using UnityEngine;

public class PingTester : MonoBehaviour
{
    bool mIsServerReachable;
    string mServerIP;

    // Use this for initialization
    private void Start()
    {
        ResolveServerIP();
        mIsServerReachable = false;
        StartCoroutine(TestServer());
    }

    private void ResolveServerIP()
    {
        try
        {
            mServerIP = Dns.GetHostAddresses("server.instant-sphere.com")[0].ToString();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            mServerIP = "";
        }
    }

    private void Update()
    {
        if (mServerIP == null || mServerIP == "")
            ResolveServerIP();
    }

    IEnumerator TestServer()
    {
        while (true)
        {
            mIsServerReachable = false;
            Ping ping = new Ping(mServerIP);

            // Suspends the coroutine execution for the given amount of seconds using scaled time.
            yield return new WaitForSeconds(0.5f);

            if(ping.isDone)
                mIsServerReachable = true;
            yield return new WaitForSeconds(30.0f);
        }
    }

    public bool CheckServer()
    {
        return mIsServerReachable;
    }
}
