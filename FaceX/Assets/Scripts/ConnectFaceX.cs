using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

public class ConnectFaceX : MonoBehaviour
{
    public GameObject Head = null;

    private Thread _thead = null;

    private Dictionary<string, float> _values = new Dictionary<string, float>();

    private void RunNetwork()
    {
        // the client
        TcpClient client = null;

        try
        {
            while (true)
            {
                // connect with retry
                try
                {
                    client = new TcpClient("127.0.0.1", 6699);
                }
                catch (SocketException)
                {
                    Thread.Sleep(1000);
                    continue;
                }

                // create the reader
                var reader = new StreamReader(client.GetStream());

                // read-loop
                while (true)
                {
                    var line = "";
                    try
                    {
                        line = reader.ReadLine();
                    }
                    catch (IOException)
                    {
                        break;
                    }

                    // split fields
                    var fields = line.Split(';');

                    // save fields
                    foreach (string field in fields)
                    {
                        var kvs = field.Split(':');
                        if (kvs.Length != 2)
                        {
                            continue;
                        }
                        _values[kvs[0].ToLower()] = float.Parse(kvs[1]);
                    }
                }

                // break on exception, retry
                client.Close();
                client = null;

                Thread.Sleep(1000);
            }
        }
        catch (ThreadAbortException)
        {
        }
        finally
        {
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }
    }

    void Start()
    {
        // fill data
        _values["r1"] = 0;
        _values["r2"] = 0;

        // start network thread
        if (_thead == null)
        {
            _thead = new Thread(RunNetwork);
            _thead.Start();
        }
    }

    void Update()
    {
        float ry = Mathf.Min(24, Mathf.Max(-24, _values["r2"] * 20));
        float rz = Mathf.Min(24, Mathf.Max(-24, _values["r1"] * 20));
        Head.transform.localEulerAngles = new Vector3(0, ry, rz);
    }

    private void OnApplicationQuit()
    {
        if (this._thead != null)
        {
            this._thead.Abort();
            this._thead.Join();
            this._thead = null;
        }
    }
}
