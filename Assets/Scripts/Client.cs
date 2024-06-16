using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Client : MonoBehaviour {

    [SerializeField] private GameObject playerModel;

    // socket configuration
    private int socketPort = 0;
    private int maxConnections = 1;

    // stores file descriptors
    private int channelId;
    private int socketId;
    private int connectionId;

    // sets the error byte
    private byte error;

    // true if we are connected to a server
    public bool isConnected;

    // a dictonary of players, with a connection id as key and player value
    private Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

    // initialize the socket
    public void Start()
    {
        // here we initialize the transport layer
        NetworkTransport.Init();
        // create configuration class for the connection
        ConnectionConfig config = new ConnectionConfig();
        // creates a reliable channel
        channelId = config.AddChannel(QosType.Reliable);
        // sets max connections allowed on socket
        HostTopology topology = new HostTopology(config, maxConnections);
        // opens socket and returns the socket file descriptor
        socketId = NetworkTransport.AddHost(topology, socketPort);
    }

    // method for connecting to another socket
    public void Connect(string address, int port)
    {
        // connects to socket
        connectionId = NetworkTransport.Connect(socketId, address, port, 0, out error);
    }

    // method for sending data to another socket
    public void Send(object[] command)
    {
        // creates buffer to hold serialized data
        byte[] buffer = new byte[1024];
        // creates memory stream
        Stream stream = new MemoryStream(buffer);
        // creates encoder
        BinaryFormatter formatter = new BinaryFormatter();
        // serializes data into buffer
        formatter.Serialize(stream, command);
        // buffer size is used for argument
        int bufferSize = 1024;
        // sends buffer to socket
        NetworkTransport.Send(socketId, connectionId, channelId, buffer, bufferSize, out error);
    }

    // method for disconnecting from current socket
    public void Disconnect()
    {
        // disconnects from server
        NetworkTransport.Disconnect(socketId, connectionId, out error);
    }

    public void Update()
    {
        // stores the recieved host id
        int recHostId;
        // stores the recieved connection id
        int recConnectionId;
        // stores the recieved channel id
        int recChannelId;
        // stores the recieved bytes
        byte[] recBuffer = new byte[1024];
        // keep tratc of buffer size
        int bufferSize = 1024;
        // stores amonunt of bytes recieved
        int dataSize;
        // waits to recieve message
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out recConnectionId, out recChannelId, recBuffer, bufferSize, out dataSize, out error);
        // checks the data type that comes through
        switch (recData)
        {
            case NetworkEventType.Nothing:
                // if nothing is sent then we do nothing
                break;
            case NetworkEventType.ConnectEvent:
                // if the connection id is ours
                // then we have successfully connected
                if (connectionId == recConnectionId)
                {
                    // we are now connected to a server
                    isConnected = true;
                }
                // breaks out of switch
                break;
            case NetworkEventType.DataEvent:
                // if the packet we recieve contains bytes of data
                // we want to extract this data and use it to tell us what to do
                // deserializes bytes
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                object[] command = formatter.Deserialize(stream) as object[];
                // splits string for parsing
                // the first item in the array tell us what to do
                // for instance "MOVE", "SPAWN" and "DESTROY" are some examples
                // parses data
                switch (command[0] as string)
                {
                    case "SPAWN":
                        SpawnPlayer(command);
                        break;
                    case "MOVE":
                        Move(command);
                        break;
                }
                break;
            case NetworkEventType.DisconnectEvent:
                break;
        }
    }

    // method for moving the player given in the third parameter
    public void Move(object[] data)
    {
        // creates player position
        Vector3 position = new Vector3((float)data[2], (float)data[3], (float)data[4]);
        // creates player rotation
        Quaternion rotation = new Quaternion((float)data[5], (float)data[6], (float)data[7], (float)data[8]);

        // sets the position of the player on the server
        players[(int)data[1]].transform.position = position;
        players[(int)data[1]].transform.rotation = rotation;
    }

    public void SpawnPlayer(object[] data)
    {
        // creates player position
        Vector3 position = new Vector3((float)data[2], (float)data[3], (float)data[4]);
        // creates player rotation
        Quaternion rotation = new Quaternion((float)data[5], (float)data[6], (float)data[7], (float)data[8]);
        // checks if this is our player
        if (data[1] as string == "MAIN")
        {
            GameObject player = Instantiate(playerModel, position, rotation);
        }
        else
        {
            // spawns player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.transform.position = position;
            player.transform.rotation = rotation;
            // adds player to dictonary
            players.Add((int)data[1], player);
        }
    }

    public void OnGUI()
    {
        // tempory connection area
        string serverAddress = "127.0.0.1";
        string serverPort = "47777";
        GUI.TextField(new Rect(10, 10, 200, 20), serverAddress, 25);
        GUI.TextField(new Rect(10, 35, 200, 20), serverPort, 25);
        if (GUI.Button(new Rect(10, 60, 200, 20), "Connect"))
        {
            Connect(serverAddress, int.Parse(serverPort));
        }
        if (GUI.Button(new Rect(10, 85, 200, 20), "Disconnect"))
        {
            Disconnect();
        }
    }
}
