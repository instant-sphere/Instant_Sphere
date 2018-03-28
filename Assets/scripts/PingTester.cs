using System.Collections;
using System.Net;
using UnityEngine;


public class PingTester : MonoBehaviour
{
    const string SERVER_ADDRESS = "server.instant-sphere.com";
    bool mIsServerReachable;
    string mServerIP;

    // Use this for initialization
    private void Start()
    {
        ResolveServerIP();
        mIsServerReachable = false;
        StartCoroutine(TestServer());
    }

    /**
     * Performs a DNS resolution of the server name SERVER_ADDRESS and store the IP address
     **/
    private void ResolveServerIP()
    {
        try
        {
            mServerIP = Dns.GetHostAddresses(SERVER_ADDRESS)[0].ToString();
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
            mServerIP = null;
        }
    }

    private void Update()
    {
        if (mServerIP == null)
            ResolveServerIP();
    }

    IEnumerator TestServer()
    {
        while (true)
        {
            mIsServerReachable = false;
            Ping ping = new Ping(mServerIP);

            // Suspends the coroutine execution for the given amount of seconds using scaled time.
            yield return new WaitForSeconds(5.5f);

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
