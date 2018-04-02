using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

/**
 * This class creates a request to download the video flux coming from the OSC Camera
 * It uses a thread to read each image then puts it in a public shared buffer
 **/
public class StreamingRequest
{
    HttpWebRequest mWebRequest;
    Thread mThread;
    ReaderWriterLockSlim mAccessImage;  //mutex to protect the buffer
    BinaryReader mReader;
    byte[] mLastFullImage;  //last fully downloaded image
    bool mError = false;

    /**
     * Constructor, URL is the url of the camera streaming endpoint and jsonData are the POST data
     * Sends the request and starts the downloading thread
     **/
    public StreamingRequest(string URL, string jsonData)
    {
        mAccessImage = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        mWebRequest = WebRequest.Create(URL) as HttpWebRequest;

        mWebRequest.Method = "POST";
        mWebRequest.Timeout = 1000 * 3600;
        mWebRequest.ContentType = "application/json;charset=utf-8";

        byte[] postBytes = Encoding.Default.GetBytes(jsonData);
        mWebRequest.ContentLength = postBytes.Length;

        try
        {
            Stream reqStream = mWebRequest.GetRequestStream();
            reqStream.Write(postBytes, 0, postBytes.Length);
            reqStream.Close();
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            mError = true;
            return;
        }

        mThread = new Thread(Run);
        mThread.Start();
        Debug.Log("Starting streaming thread " + mThread.ManagedThreadId);
    }

    /**
     * Abort the downloading thread, the webrequest and close everything
     **/
    public void Abort()
    {
        if (mThread != null)
        {
            Debug.Log("Terminating thread " + mThread.ManagedThreadId);
            mThread.Abort();
        }
        if (mReader != null)
            mReader.Close();
        if(mWebRequest != null)
            mWebRequest.Abort();
    }

    /**
     * Returns the last fully downloaded image or null if there is no new image since last call
     **/
    public byte[] GetLastReceivedImage()
    {
        mAccessImage.EnterWriteLock();
        byte[] ret = mLastFullImage;
        mLastFullImage = null;
        mAccessImage.ExitWriteLock();
        return ret;
    }

    /**
     * Returns true if the stream has encountered an error, false otherwise
     **/
    public bool IsStreamOnError()
    {
        return mError;
    }

    /**
     * Reads the stream until the beginning of the data
     * Return the size of the data in bytes to be read
     **/
    private int SkipHeaderAndGetSize()
    {
        char c;
        char[] cs = new char[256];
        int i = 0;
        do
        {
            c = mReader.ReadChar();
        } while (c != 'h');

        mReader.ReadChar();

        while (c != '\n')
        {
            c = mReader.ReadChar();
            cs[i++] = c;
        }

        mReader.ReadChar();
        mReader.ReadChar();

        string s = new string(cs);
        return int.Parse(s);
    }

    /**
     * Thread function
     * Read the stream indefinitely and extract each image from the stream to the buffer
     **/
    void Run()
    {
        try
        {
            Stream stream = mWebRequest.GetResponse().GetResponseStream();
            stream.ReadTimeout = 30 * 1000;
            mReader = new BinaryReader(new BufferedStream(stream), new ASCIIEncoding());

            while (true)
            {
                int blockSize = SkipHeaderAndGetSize();

                byte[] data = new byte[blockSize];
                int readByte = 0;
                while (readByte != blockSize)
                {
                    readByte += mReader.Read(data, readByte, blockSize - readByte);
                }

                mAccessImage.EnterWriteLock();
                mLastFullImage = data;
                mAccessImage.ExitWriteLock();
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            mError = true;
            return;
        }
    }
}
