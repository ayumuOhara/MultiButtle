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
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnAllPlayers();
            }
            hasSpawned = true;
        }
    }

    void SpawnAllPlayers()
    {
        // スポーン位置をシャッフル
        List<Transform> shuffledSpawnPoints = new List<Transform>(spawnList);
        Shuffle(shuffledSpawnPoints);

        int index = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object[] instantiationData = new object[] { index }; // インデックスを渡して各クライアントが自分で使う
            PhotonNetwork.Instantiate("Player", shuffledSpawnPoints[index].position, Quaternion.identity, 0, instantiationData);
            index++;
        }
    }

    // リストをシャッフルするユーティリティ
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rnd = Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }
    }
}
