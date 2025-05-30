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

    // �v���C���[�������ʒu�ɃX�|�[��
    void TrySpawnPlayer()
    {
        if (!hasSpawned && PhotonNetwork.InRoom)
        {
            GameObject obj = PhotonNetwork.Instantiate("Player", farstSpawn.transform.position, Quaternion.identity);
            hasSpawned = true;
        }
    }


    // �v���C���[�̐��������擾
    public int GetPlayerCnt()
    {
        int playerCnt = 0;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            // �J�X�^���v���p�e�B "Dead" �����݂��邩�m�F
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
                Debug.LogWarning($"[Dead] �v���p�e�B�����ݒ�̃v���C���[: {p.NickName}");
            }
        }
        return playerCnt;
    }


    // �v���C���[�������܂őҋ@
    IEnumerator WaitPlayers()
    {
        Debug.Log("�v���C���[�ҋ@�R���[�`�����J�n");

        while (true)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == PLAYER_LIMIT_CNT)
            {
                Debug.Log("�v���C���[��������");

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


    // ���E���h���J�n
    [PunRPC]
    void RoundStart()
    {
        Debug.Log("���E���h�X�^�[�g");
        isStart = true;
    }

    // ���E���h���I��
    [PunRPC]
    void RoundEnd()
    {
        Debug.Log("���E���h�G���h");
        isStart = false;
    }

    // ���E���h����
    IEnumerator RoundGame()
    {
        Debug.Log("���E���h���[�v�R���[�`���J�n");

        photonView.RPC(nameof(RoundStart), RpcTarget.All);

        int roundCnt = 1;
        photonView.RPC(nameof(RoundCntText), RpcTarget.All, roundCnt);

        while (roundCnt <= 15)
        {
            if (GetPlayerCnt() <= 1)
            {
                roundCnt++;
                Debug.Log($"���E���h�F{roundCnt}");

                photonView.RPC(nameof(RoundEnd), RpcTarget.All);
                photonView.RPC(nameof(SetAllPlayerProp), RpcTarget.All);
                photonView.RPC(nameof(SetSpawn), RpcTarget.MasterClient);
                photonView.RPC(nameof(RoundCntText), RpcTarget.All, roundCnt);
                photonView.RPC(nameof(RoundStart), RpcTarget.All);

                yield return null;
            }

            yield return null;
        }

        Debug.Log("�Q�[���G���h");
        yield break;
    }

    // ���E���h���\��
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

    // �v���C���[�̐l���\��
    [PunRPC]
    void PlayerCntText(bool state)
    {
        playerCntText.gameObject.SetActive(state);
        playerCntText.text = $"���݂̃v���C���[��\n{PhotonNetwork.CurrentRoom.PlayerCount} / {PLAYER_LIMIT_CNT}";
    }

    // �S�v���C���[�̃X�|�[���ʒu��ݒ�
    [PunRPC]
    void SetSpawn()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // �Y�����p�̔z��̗v�f���V���b�t��
        int[] indices = new int[spawnList.Count];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;
        Shuffle(indices);

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        int assigned = 0;

        // �S�v���C���[���̃X�|�[���ʒu������U��
        foreach (GameObject player in players)
        {
            PhotonView view = player.GetComponent<PhotonView>();
            if (view != null)
            {
                int spawnIndex = indices[assigned % spawnList.Count];
                // �Ώۂ̃v���C���[�ɃX�|�[���ʒu�𑗐M
                photonView.RPC(nameof(AssignSpawnPosition), RpcTarget.All, view.ViewID, spawnIndex);
                assigned++;
            }
        }
    }

    // �v���C���[�̃X�|�[���ʒu����U��
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

    // �v���C���[�̃X�|�[���ʒu����U��
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

    // �z��̗v�f���V���b�t��
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
