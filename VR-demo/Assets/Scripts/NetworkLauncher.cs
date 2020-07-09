using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkLauncher : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = false;
        PhotonNetwork.ConnectUsingSettings(); //using unity settings
    }

    //connect to server
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Welcome");

        PhotonNetwork.JoinOrCreateRoom("VR Room", new Photon.Realtime.RoomOptions() { MaxPlayers = 4 }, default);
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        PhotonNetwork.Instantiate("Player", new Vector3(0, 5, -10), Quaternion.identity, 0);
    }
}
