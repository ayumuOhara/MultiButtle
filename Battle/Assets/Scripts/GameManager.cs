using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform farstSpawn;

    [SerializeField]
    List<Transform> spawnList = new List<Transform>();

    [SerializeField]
    TextMeshProUGUI playerCntText;

    bool hasSpawned = false;
    public bool isStart = false;
    public bool isStop { get; private set; } = false;

    const int PLAYER_LIMIT_CNT = 2;

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    void Start()
    {
        TrySpawnPlayer();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(WaitPlayers());
        }
    }

    // プレイヤーを初期位置にスポーン
    void TrySpawnPlayer()
    {
        if (!hasSpawned && PhotonNetwork.InRoom)
        {
            GameObject obj = PhotonNetwork.Instantiate("Player", farstSpawn.transform.position, Quaternion.identity);
            hasSpawned = true;
        }
    }


    // プレイヤーの生存数を取得
    public int GetPlayerCnt()
    {
        int playerCnt = 0;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            // カスタムプロパティ "Dead" が存在するか確認
            if (p.CustomProperties.ContainsKey("Dead"))
            {
                bool dead = p.GetDead();
                if (!dead)
                {
                    playerCnt++;
                }
            }
            else
            {
                Debug.LogWarning($"[Dead] プロパティが未設定のプレイヤー: {p.NickName}");
            }
        }
        return playerCnt;
    }


    // プレイヤーが揃うまで待機
    IEnumerator WaitPlayers()
    {
        Debug.Log("プレイヤー待機コルーチンを開始");

        while (true)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PLAYER_LIMIT_CNT)
            {
                Debug.Log("プレイヤーが揃った");

                photonView.RPC(nameof(SetSpawn), RpcTarget.MasterClient);
                photonView.RPC(nameof(PlayerCntText), RpcTarget.All, false);
                yield return new WaitForSeconds(1.0f);
                StartCoroutine(InGame());
                yield break;
            }
            else
            {
                photonView.RPC(nameof(PlayerCntText), RpcTarget.All, true);
                yield return null;
            }
        }
    }

    // ゲームを開始
    [PunRPC]
    void StartGame()
    {
        Debug.Log("ゲームスタート");
        isStart = true;
    }

    // ゲームを終了
    [PunRPC]
    void EndGame()
    {
        Debug.Log("ゲームエンド");
        isStart = false;
    }

    // ゲーム処理
    IEnumerator InGame()
    {
        photonView.RPC(nameof(StartGame), RpcTarget.All);

        while (GetPlayerCnt() > 1)
        {
            yield return null;
        }

        photonView.RPC(nameof(EndGame), RpcTarget.All);
        yield break;
    }

    // プレイヤーの動きを停止させる
    [PunRPC]
    void SetStop(bool stop)
    {
        isStop = stop;
    }

    // プレイヤーの人数表示
    [PunRPC]
    void PlayerCntText(bool state)
    {
        playerCntText.gameObject.SetActive(state);
        playerCntText.text = $"現在のプレイヤー数\n{PhotonNetwork.CurrentRoom.PlayerCount} / {PLAYER_LIMIT_CNT}";
    }

    // 全プレイヤーのスポーン位置を設定
    [PunRPC]
    void SetSpawn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 添え字用の配列の要素をシャッフル
        int[] indices = new int[spawnList.Count];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;
        Shuffle(indices);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int assigned = 0;

        // 全プレイヤー分のスポーン位置を割り振る
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null)
            {
                int spawnIndex = indices[assigned % spawnList.Count];
                // 対象のプレイヤーにスポーン位置を送信
                photonView.RPC(nameof(AssignSpawnPosition), RpcTarget.All, view.ViewID, spawnIndex);
                assigned++;
            }
        }
    }

    // プレイヤーのスポーン位置割り振り
    [PunRPC]
    void AssignSpawnPosition(int viewID, int spawnIndex)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null && view.ViewID == viewID)
            {
                player.transform.position = spawnList[spawnIndex].position;
                break;
            }
        }
    }

    // プレイヤーのプロパティをセット
    [PunRPC]
    void SetAllPlayerProp()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null)
            {
                view.RPC("SetPropaties", RpcTarget.All);
                break;
            }
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
