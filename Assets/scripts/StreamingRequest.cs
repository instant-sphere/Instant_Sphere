using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class StreamingRequest
{
    HttpWebRequest mWebRequest;
    Thread mThread;
    ReaderWriterLockSlim mAccessImage;
    BinaryReader mReader;
    byte[] mLastFullImage;
    string mData;
    bool mError = false;

    public StreamingRequest(string URL, string jsonData)
    {
        mAccessImage = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        mWebRequest = WebRequest.Create(URL) as HttpWebRequest;

        mWebRequest.Method = "POST";
        mWebRequest.Timeout = (int)(30 * 10000f);
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
        Debug.Log("Starting streaming thread " + mThread.ToString());
    }

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

    public byte[] GetLastReceivedImage()
    {
        mAccessImage.EnterWriteLock();
        byte[] ret = mLastFullImage;
        mLastFullImage = null;
        mAccessImage.ExitWriteLock();
        return ret;
    }

    public bool IsStreamOnError()
    {
        return mError;
    }

    /**
     * Read the stream until the beginning of the data
     * Return the size of the data in octet to be read
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

    void Run()
    {
        try
        {
            Stream stream = mWebRequest.GetResponse().GetResponseStream();
            stream.ReadTimeout = 3 * 1000;
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
