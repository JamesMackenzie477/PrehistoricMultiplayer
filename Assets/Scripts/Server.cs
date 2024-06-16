using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Server : MonoBehaviour {

    // socket configuration
    [SerializeField] private int socketPort = 47777;
    [SerializeField] private int maxConnections = 10;
    [SerializeField] private GameObject playerModel;

    // stores file descriptors
    private int channelId;
    private int socketId;

    // sets the error byte
    private byte error;

    // a dictonary of players, with a connection id as key and player value
    // this dictonary will keep track of all the current players and their game object on the server
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
        // validates socket
        if (socketId < 0) Debug.Log("There was a problem starting the server");
        else Debug.Log("Server started on port: " + socketPort);
    }

    // this method is called every frame update
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
                // invokes a function to spawn the player on the server
                // on the clients and spawn all the current players on their game
                AddPlayerToServer(recConnectionId);
                // breaks out of switch
                break;
            case NetworkEventType.DataEvent:
                // if the packet we recieve contains bytes of data
                // we want to extract this data and use it to tell us what to do
                // deserializes bytes
                Stream stream = new MemoryStream(recBuffer);
                BinaryFormatter formatter = new BinaryFormatter();
                object[] command = formatter.Deserialize(stream) as object[];
                // invokes function that will parse the data and find what the client wants to do
                ParseServerCommand(recConnectionId, command);
                // breaks out of switch
                break;
            case NetworkEventType.DisconnectEvent:
                // invokes function that will remove the player from the server
                // clean up any items left by the player and remove the player from current clients games
                RemovePlayerFromServer(recConnectionId);
                // breaks out of switch
                break;
        }
    }


    // this will take a string sent from a client and decide what the client would like to do
    public void ParseServerCommand(int connectionId, object[] data)
    {
        // splits data for parsing
        // the first item in the array tell us what to do
        // for instance "MOVE", "SPAWN" and "DESTROY" are some examples
        // parses data
        switch (data[0] as string)
        {
            // if the instruction is MOVE
            // we want to move the requesting client on the server
            // and tell all clients that the player has moved
            case "MOVE":
                // invokes move function that repositions the player on the server
                // creates player position
                Vector3 position = new Vector3((float)data[1], (float)data[2], (float)data[3]);
                // creates player rotation
                Quaternion rotation = new Quaternion((float)data[4], (float)data[5], (float)data[6], (float)data[7]);
                // chanage player location
                Move(players[connectionId], position, rotation);
                // iterate through current players
                foreach (var item in players)
                {
                    // if the connetion id is not the requestion one
                    // we want to tell that client that the player has moved
                    // and provide the new co-ordinates
                    if (connectionId != item.Key)
                    {
                        // OK there was a major problem I had here that took me over 24 hours to figure out
                        // I pinpointed the error to here so don't touch this
                        // creates a new message containing the id of the player to move
                        object[] movePlayer = { "MOVE", connectionId, players[connectionId].transform.position.x, players[connectionId].transform.position.y, players[connectionId].transform.position.z, players[connectionId].transform.rotation.x, players[connectionId].transform.rotation.y, players[connectionId].transform.rotation.z, players[connectionId].transform.rotation.w };
                        // sends message to the id of the player
                        Send(item.Key, movePlayer);
                    }
                }
                // breaks out of switch
                break;
        }
    }

    // method that will spawn the player on the server
    // on clients and add current players to the players game
    public void AddPlayerToServer(int connectionId)
    {
        // if a new player connects this is called
        // spawns player on server
        GameObject newPlayer =  GameObject.CreatePrimitive(PrimitiveType.Capsule);
        newPlayer.transform.position = new Vector3(100, 1, -50);
        newPlayer.transform.rotation = Quaternion.identity;
        // GameObject newPlayer = Instantiate(playerModel, new Vector3(100, 1, -50), Quaternion.identity);
        // adds player to players dictonary to keep track of the gameobject
        players.Add(connectionId, newPlayer);
        // iterates through server players
        // this is used to spawn the current players
        // and spawn the new player on the current players game
        foreach (var item in players)
        {
            // if the player we are spawning is ours
            // notifiy the player that the player is theirs
            // we send the connetion id of the latest player to the current players
            // this acts as an identifier for when we want to tell the clients to move that player
            if (connectionId == item.Key)
            {
                // tell the requesting player where to spawn itself and tell the player this is their player
                // MAIN tells the client that this is their character to control
                object[] spawnPlayer = { "SPAWN", "MAIN", item.Value.transform.position.x, item.Value.transform.position.y, item.Value.transform.position.z, item.Value.transform.rotation.x, item.Value.transform.rotation.y, item.Value.transform.rotation.z, item.Value.transform.rotation.w };
                // sends spawn message to new requesting player
                Send(connectionId, spawnPlayer);
            }
            else
            {
                // tell the player where to place the new player
                object[] newPlayerSpawn = { "SPAWN", connectionId, newPlayer.transform.position.x, newPlayer.transform.position.y, newPlayer.transform.position.z, newPlayer.transform.rotation.x, newPlayer.transform.rotation.y, newPlayer.transform.rotation.z, newPlayer.transform.rotation.w };
                // tell the requesting player where to place the current players
                object[] spawnPlayer = { "SPAWN", item.Key, item.Value.transform.position.x, item.Value.transform.position.y, item.Value.transform.position.z, item.Value.transform.rotation.x, item.Value.transform.rotation.y, item.Value.transform.rotation.z, item.Value.transform.rotation.w };
                // sends spawn message to current players
                Send(item.Key, newPlayerSpawn);
                // sends spawn message to new player
                Send(connectionId, spawnPlayer);
            }
        }
        // logs to console
        Debug.Log("Added player " + connectionId + " to the game");
    }


    public void RemovePlayerFromServer(int connectionID)
    {
        // removes player from game
        // this will destory their player in game
        Object.Destroy(players[connectionID]);
        // removes player from players dictonary
        players.Remove(connectionID);
    }

    // method for moving the player given in the third parameter
    public void Move(GameObject player, Vector3 position, Quaternion rotation)
    {
        // sets the position of the player on the server
        player.transform.position = position;
        player.transform.rotation = rotation;
    }

    // method for sending data to another socket
    public void Send(int connectionId, object[] command)
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
}
