using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class streamingRequest
{
    HttpWebRequest mWebRequest;
    Thread mThread;
    ReaderWriterLockSlim mAccessImage;
    byte[] mLastFullImage;
    string mData;

    public streamingRequest(string URL, string jsonData)
    {
        mAccessImage = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        mWebRequest = WebRequest.Create(URL) as HttpWebRequest;

        mWebRequest.Method = "POST";
        mWebRequest.Timeout = (int)(30 * 10000f);
        mWebRequest.ContentType = "application/json;charset=utf-8";

        byte[] postBytes = Encoding.Default.GetBytes(jsonData);
        mWebRequest.ContentLength = postBytes.Length;

        Stream reqStream = mWebRequest.GetRequestStream();
        reqStream.Write(postBytes, 0, postBytes.Length);
        reqStream.Close();

        mThread = new Thread(Run);
        mThread.Start();
    }

    public void Abort()
    {
        mThread.Abort();
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

    /**
     * Read the stream until the beginning of the data
     * Return the size of the data in octet to be read
     **/
    private int SkipHeaderAndGetSize(BinaryReader binReader)
    {
        return 0;
    }

    void Run()
    {
        Stream stream = mWebRequest.GetResponse().GetResponseStream();

        BinaryReader reader = new BinaryReader(new BufferedStream(stream), new ASCIIEncoding());

        while (true)
        {
            char c;
            char[] cs = new char[256];
            int i = 0;
            do
            {
                c = reader.ReadChar();
            } while (c != 'h');

            reader.ReadChar();

            while (c != '\n')
            {
                c = reader.ReadChar();
                cs[i++] = c;
            }

            reader.ReadChar();
            reader.ReadChar();

            string s = new string(cs);
            int blockSize = int.Parse(s);

            byte[] data = new byte[blockSize];
            int readByte = 0;
            while (readByte != blockSize)
            {
                readByte += reader.Read(data, readByte, blockSize - readByte);
            }

            mAccessImage.EnterWriteLock();
            mLastFullImage = data;
            mAccessImage.ExitWriteLock();
        }
    }
}
