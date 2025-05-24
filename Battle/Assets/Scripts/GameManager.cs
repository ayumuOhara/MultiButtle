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

    const int PLAYER_LIMIT_CNT = 2;     // �Q������v���C���[�̍ő�l

    public override void OnJoinedRoom()
    {
        TrySpawnPlayer();
    }

    void Start()
    {
        //PhotonNetwork.OfflineMode = true; // �I�t���C�����[�hON
        //PhotonNetwork.CreateRoom("OfflineRoom"); // �_�~�[���[���쐬

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
            Debug.Log("�v���C���[�����F" + obj);
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
                Debug.Log($"�v���C���[�����X�g�ɒǉ�: {player.name} (ViewID: {viewID})");
            }
        }
        else
        {
            Debug.LogWarning($"ViewID {viewID} �̃I�u�W�F�N�g��������܂���ł����B");
        }
    }


    // �v���C���[�̐����l��
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

    // �v���C���[�������܂őҋ@
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

    // �Q�[�����J�n����
    [PunRPC]
    void GameStart()
    {
        isStart = true;
    }

    // ���݂̃v���C���[����\��
    [PunRPC]
    void PlayerCntText(bool state)
    {
        playerCntText.gameObject.SetActive(state);
        playerCntText.text = $"���݂̃v���C���[��\n{PhotonNetwork.CurrentRoom.PlayerCount} / {PLAYER_LIMIT_CNT}";
    }

    [PunRPC]
    // �v���C���[�̃X�|�[���n�_��ݒ�
    void SetSpawn()
    {
        int[] rnd = { 0, 1, 2, 3 };
        Shuffle(rnd); // �V���b�t���������Ăяo��

        for (int i = 0; i < playerObjList.Count; i++)
        {
            GameObject p = playerObjList[i];
            PhotonView playerPhoton = p.GetComponent<PhotonView>();

            int spawnPoint = rnd[i]; // �d���Ȃ��̃C���f�b�N�X���g�p
            p.transform.position = spawnList[spawnPoint].transform.position;
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
