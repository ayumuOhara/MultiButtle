using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    List<Transform> spawnList = new List<Transform>();

    [SerializeField]
    List<GameObject> playerObjList = new List<GameObject>();

    [SerializeField]
    TextMeshProUGUI playerCntText;

    bool hasSpawned = false;
    public bool isStart = false;

    const int PLAYER_LIMIT_CNT = 2;     // 参加するプレイヤーの最大値

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    void Start()
    {
        //PhotonNetwork.OfflineMode = true; // オフラインモードON
        //PhotonNetwork.CreateRoom("OfflineRoom"); // ダミールーム作成

        TrySpawnPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitPlayers());
        }
    }

    void TrySpawnPlayer()
    {
        if (!hasSpawned && PhotonNetwork.InRoom)
        {
            GameObject obj = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
            obj.transform.position = spawnList[0].transform.position;
            PhotonView playerPhoton = obj.GetComponent<PhotonView>();
            photonView.RPC(nameof(AddPlayerList), RpcTarget.MasterClient, playerPhoton.ViewID);
            Debug.Log("プレイヤー生成：" + obj);
            hasSpawned = true;
        }
    }

    [PunRPC]
    void AddPlayerList(int viewID)
    {
        PhotonView view = PhotonView.Find(viewID);
        if (view != null)
        {
            GameObject player = view.gameObject;
            if (!playerObjList.Contains(player))
            {
                playerObjList.Add(player);
                Debug.Log($"プレイヤーをリストに追加: {player.name} (ViewID: {viewID})");
            }
        }
        else
        {
            Debug.LogWarning($"ViewID {viewID} のオブジェクトが見つかりませんでした。");
        }
    }


    // プレイヤーの生存人数
    public int GetPlayerCnt()
    {
        int playerCnt = 0;

        foreach (var p in PhotonNetwork.PlayerList)
        {
            bool dead = p.GetDead();
            if (!dead)
            {
                playerCnt++;
            }
        }

        return playerCnt;
    }

    // プレイヤーが揃うまで待機
    IEnumerator WaitPlayers()
    {
        while (true)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PLAYER_LIMIT_CNT)
            {                
                photonView.RPC(nameof(SetSpawn), RpcTarget.All);
                photonView.RPC(nameof(GameStart), RpcTarget.All);
                photonView.RPC(nameof(PlayerCntText), RpcTarget.All, false);
                yield break;
            }
            else
            {
                photonView.RPC(nameof(PlayerCntText), RpcTarget.All, true);
                yield return null;
            }
        }
    }

    // ゲームを開始する
    [PunRPC]
    void GameStart()
    {
        isStart = true;
    }

    // 現在のプレイヤー数を表示
    [PunRPC]
    void PlayerCntText(bool state)
    {
        playerCntText.gameObject.SetActive(state);
        playerCntText.text = $"現在のプレイヤー数\n{PhotonNetwork.CurrentRoom.PlayerCount} / {PLAYER_LIMIT_CNT}";
    }

    [PunRPC]
    // プレイヤーのスポーン地点を設定
    void SetSpawn()
    {
        int[] rnd = { 0, 1, 2, 3 };
        Shuffle(rnd); // シャッフル処理を呼び出す

        for (int i = 0; i < playerObjList.Count; i++)
        {
            GameObject p = playerObjList[i];
            PhotonView playerPhoton = p.GetComponent<PhotonView>();

            int spawnPoint = rnd[i]; // 重複なしのインデックスを使用
            p.transform.position = spawnList[spawnPoint].transform.position;
        }
    }

    // 配列の要素をシャッフル
    void Shuffle(int[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

}
