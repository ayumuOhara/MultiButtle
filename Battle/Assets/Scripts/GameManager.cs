using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    List<Transform> spawnList = new List<Transform>(); // スポーン位置（Transformで管理）

    bool hasSpawned = false;

    void Update()
    {
        if (!hasSpawned && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == 4)
        {
            PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            hasSpawned = true;
        }
    }
}
