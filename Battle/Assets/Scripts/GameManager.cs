using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    List<Transform> spawnList = new List<Transform>();

    [SerializeField]
    TextMeshProUGUI playerCntText;

    bool hasSpawned = false;

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    void Start()
    {
        TrySpawnPlayer();
    }

    void TrySpawnPlayer()
    {
        if (!hasSpawned && PhotonNetwork.InRoom)
        {
            GameObject obj = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            Debug.Log("�v���C���[�����F" + obj);
            hasSpawned = true;
        }
    }

    void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                playerCntText.gameObject.SetActive(false);
            }
            else
            {
                playerCntText.text = $"���݂̃v���C���[��\n{PhotonNetwork.CurrentRoom.PlayerCount} / 2";
            }
        }
    }
}
