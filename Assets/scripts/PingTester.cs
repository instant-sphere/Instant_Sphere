using System.Collections;
using System.Net;
using UnityEngine;

/**
 * This class continuously pings the server and keep the server reachability up to date
 **/
public class PingTester : MonoBehaviour
{
    const string SERVER_ADDRESS = "server.instant-sphere.com";
    bool mIsServerReachable;
    string mServerIP;

    // Use this for initialization
    private void Start()
    {
        mIsServerReachable = false;
        ResolveServerIP();
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

    /**
     * Try a DNS resolution if the server IP isn't known
     **/
    private void Update()
    {
        if (mServerIP == null)
            ResolveServerIP();
    }

    /**
     * Pings the server IP every 30sec and update the server reachability accordingly
     **/
    private IEnumerator TestServer()
    {
        while (true)
        {
            mIsServerReachable = false;
            if (mServerIP != null)
            {
                Ping ping = new Ping(mServerIP);

                yield return new WaitForSeconds(1.5f);  // allow 1.5sec to complete ping

                if (ping.isDone)
                    mIsServerReachable = true;
            }
            yield return new WaitForSeconds(30.0f);
        }
    }

    public bool CheckServer()
    {
        return mIsServerReachable;
    }
}
