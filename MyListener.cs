using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;

public class MyListener : MonoBehaviour
{
    Thread thread;
    public int connectionPort = 25001;
    TcpListener server;
    TcpClient client;
    bool running;
    public bool hasStarted { private set; get; } = false;
    public float yaw { private set; get; } = 0f;

    void Start()
    {
        // Receive on a separate thread so Unity doesn't freeze waiting for data
        ThreadStart ts = new ThreadStart(GetData);
        thread = new Thread(ts);
        thread.Start();

    }

    void GetData()
    {
        // Create the server
        server = new TcpListener(IPAddress.Any, connectionPort);
        server.Start();

        // Create a client to get the data stream
        client = server.AcceptTcpClient();

        // Start listening
        running = true;
        while (running)
        {
            Connection();
        }
        server.Stop();
    }

    void Connection()
    {
        // Read data from the network stream
        NetworkStream nwStream = client.GetStream();
        byte[] buffer = new byte[client.ReceiveBufferSize];
        int bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);

        // Decode the bytes into a string
        string dataReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        // Make sure we're not getting an empty string
        if (dataReceived != null && dataReceived != "")
        {
            // Convert the received string of data to the format we are using
            yaw = ParseData(dataReceived).z;
            position = new Vector3(0,0,yaw);
            //limit
            

            nwStream.Write(buffer, 0, bytesRead);
        }
    }

    // Use-case specific function, need to re-write this to interpret whatever data is being sent
    public Vector3 ParseData(string dataString)
    {
        Debug.Log(dataString);

        // Extract values from the string
        string[] values = dataString.Split(new char[] { ':', ',', ' ', '|', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        if (values.Length >= 6)
        {
            // Parse values to floats
            float roll = float.Parse(values[1]);
            float pitch = float.Parse(values[3]);
            float yaw = float.Parse(values[5]);

            if (values[1] != "" && values[3] != "" && values[5] != "")
            {
                hasStarted = true;
            }


            // Create and return a Vector3
            Vector3 result = new Vector3(roll, pitch, yaw);
            return result;
        }
        else
        {
            Debug.LogError("Invalid data format received");
            return position;
        }
    }

    // Position is the data being received in this example
    Vector3 position = Vector3.zero;

    void Update()
    {
        // Set this object's position in the scene according to the position received
        transform.eulerAngles = position;
    }

    public bool starter()
    {
        return hasStarted;
    }



}