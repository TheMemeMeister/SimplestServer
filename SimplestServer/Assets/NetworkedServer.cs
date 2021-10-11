using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class NetworkedServer : MonoBehaviour
{
    const int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    const int socketPort = 5491;

    LinkedList<PlayerAccount> playerAccounts;

    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init(); //initializing the connection bridge
        ConnectionConfig config = new ConnectionConfig(); //configuring the connection
        reliableChannelID = config.AddChannel(QosType.Reliable); //quality of service. Reliable has msg garenteed to be delivered, order not garenteed.
        unreliableChannelID = config.AddChannel(QosType.Unreliable); //nothing garenteed
        HostTopology topology = new HostTopology(config, maxConnections); //last step in creating a connection, letting the servert know what configuration will be used(how many default connections, what special connections)
        hostID = NetworkTransport.AddHost(topology, socketPort, null); //Adds host based on Networking.HostTopology. Returns the Host ID.

        playerAccounts = new LinkedList<PlayerAccount>();
        //read in player accounts
        
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
                //byte firstByte = recBuffer[0];
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
        string[] csv = msg.Split(',');
        int signfier = int.Parse(csv[0]);

        if (signfier == ClientToServerSignifiers.CreateAccount)
        {
            Debug.Log("create account");
            ///check if player account name already exits
            ///
            string n = csv[1];
            string p = csv[2];
            bool nameIsInUse = false;
            foreach (PlayerAccount pa in playerAccounts)
            {
                if(pa.name == n)
                {
                    nameIsInUse = true;
                }
            }
            if (nameIsInUse)
            {
                SendMessageToClient(ServerToClientSignifiers.AccountCreationFailed + "", id);
            }
            else
            {
                PlayerAccount newPlayerAccount = new PlayerAccount(n, p);
                playerAccounts.AddLast(newPlayerAccount);
                SendMessageToClient(ServerToClientSignifiers.AccountCreationComplete + "", id);
                ///save list to HD
            }



        }
        else if (signfier == ClientToServerSignifiers.LoginAccount)
        {
            Debug.Log("create login");
            ///check if player account name already exits,
            ///sent to client success/failure
        }
    }
    public class PlayerAccount
    {
        public string name, password;

        public PlayerAccount(string Name, string Password)
        {
            name = Name;
            password = Password;
        }
    }






    public static class ClientToServerSignifiers
    {
        public const int CreateAccount = 1;
        public const int LoginAccount = 2;

    }
    public static class ServerToClientSignifiers
    {
        public const int LoginComplete = 1;
        public const int LoginFailed = 2;

        public const int AccountCreationComplete = 3;
        public const int AccountCreationFailed = 4;

    }
}
