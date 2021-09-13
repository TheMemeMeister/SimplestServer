using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init(); //initializing the connection bridge
        ConnectionConfig config = new ConnectionConfig(); //configuring the connection
        reliableChannelID = config.AddChannel(QosType.Reliable); //quality of service. Reliable has msg garenteed to be delivered, order not garenteed.
        unreliableChannelID = config.AddChannel(QosType.Unreliable); //nothing garenteed
        HostTopology topology = new HostTopology(config, maxConnections); //last step in creating a connection, letting the servert know what configuration will be used(how many default connections, what special connections)
        hostID = NetworkTransport.AddHost(topology, socketPort, null); //Adds host based on Networking.HostTopology. Returns the Host ID.
        
    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0; //no rerrors yet.

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);  //Part 3 out param -> The out keyword is what is being returned
        //by the function.The reason we use out keyword parameters, moded with int values but not a referance to a class instance is that one is checking value by value, while another is comparing referances.
      
        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);
    }

}
