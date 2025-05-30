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

    [SerializeField]
    TextMeshProUGUI roundCntText;

    [SerializeField]
    GameObject roundCntTextObj;

    bool hasSpawned = false;
    public bool isStart = false;

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
                StartCoroutine(RoundGame());
                yield break;
            }
            else
            {
                photonView.RPC(nameof(PlayerCntText), RpcTarget.All, true);
                yield return null;
            }
        }
    }


    // ラウンドを開始
    [PunRPC]
    void RoundStart()
    {
        Debug.Log("ラウンドスタート");
        isStart = true;
    }

    // ラウンドを終了
    [PunRPC]
    void RoundEnd()
    {
        Debug.Log("ラウンドエンド");
        isStart = false;
    }

    // ラウンド処理
    IEnumerator RoundGame()
    {
        Debug.Log("ラウンドループコルーチン開始");

        photonView.RPC(nameof(RoundStart), RpcTarget.All);

        int roundCnt = 1;
        photonView.RPC(nameof(RoundCntText), RpcTarget.All, roundCnt);

        while (roundCnt <= 15)
        {
            if (GetPlayerCnt() <= 1)
            {
                roundCnt++;
                Debug.Log($"ラウンド：{roundCnt}");

                photonView.RPC(nameof(RoundEnd), RpcTarget.All);
                photonView.RPC(nameof(SetAllPlayerProp), RpcTarget.All);
                photonView.RPC(nameof(SetSpawn), RpcTarget.MasterClient);
                photonView.RPC(nameof(RoundCntText), RpcTarget.All, roundCnt);
                photonView.RPC(nameof(RoundStart), RpcTarget.All);

                yield return null;
            }

            yield return null;
        }

        Debug.Log("ゲームエンド");
        yield break;
    }

    // ラウンド数表示
    [PunRPC]
    IEnumerator RoundCntText(int cnt)
    {
        roundCntTextObj.SetActive(true);
        roundCntText.text = cnt.ToString();
        yield return new WaitForSeconds(3.0f);
        roundCntText.text = $"start";
        yield return new WaitForSeconds(0.5f);
        roundCntTextObj.SetActive(false);
        yield break;
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

    // プレイヤーのスポーン位置割り振り
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
