using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    List<Transform> spawnList = new List<Transform>(); // スポーン位置（Transformで管理）

    [SerializeField]
    TextMeshProUGUI playerCntText;

    bool hasSpawned = false;

    void Update()
    {
        if (!hasSpawned && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            GameObject obj = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            Debug.Log("プレイヤー生成：" + obj);
            hasSpawned = true;
            playerCntText.gameObject.SetActive(false);
        }
        else
        {
            playerCntText.text = $"現在のプレイヤー数\n{PhotonNetwork.CurrentRoom.PlayerCount} / 2";
        }
    }
}
