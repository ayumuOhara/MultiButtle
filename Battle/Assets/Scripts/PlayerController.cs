using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    GameManager gameManager;

    int hp = 1;
    int maxHp = 1;

    CoolTimeFill coolTimeFill;

    [SerializeField] GameObject coolTimeBar;    // �N�[���^�C���o�[�̃v���t�@�u
    [SerializeField] GameObject bulletPrefab;   // �e�̃v���t�@�u
    [SerializeField] Transform bulletSpawn;     // �e�����������ꏊ
    [SerializeField] float rotateSpeed = 180f;  // ��]���x�i1�b�Ԃɉ�]����p�x�̍ő�l�j

    public float moveSpeed { get; private set; }    // �ړ����x
    float screenWidth = 0f;                         // ��ʂ̉���
    float screenHeight = 0f;                        // ��ʂ̏c��
    Vector3 screenSize = Vector3.zero;              // �X�N���[���̖ʐ�
    Vector3 minScreenPos = Vector3.zero;            // �X�N���[���̍����̍��W
    Vector3 maxScreenPos = Vector3.zero;            // �X�N���[���̉E��̍��W
    Vector3 margin = new Vector3(0.6f, 0.6f);

    float time = 0f;
    float coolTime = 5.0f;  // ���̒e�����˂����܂ł̎���

    bool hasBullet = true;  // �e�����˂ł��邩

    void OnEnable()
    {
        photonView.RPC(nameof(SetPropaties), RpcTarget.All);
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            // �����ʒu�Ɖ�]�͓K���Ɏw��iCanvas��UI�Ȃ�Vector3.zero, Quaternion.identity��OK�j
            GameObject obj = Instantiate(coolTimeBar, Vector3.zero, Quaternion.identity);
            FollowTransform ftf = obj.GetComponent<FollowTransform>();
            coolTimeFill = obj.GetComponent<CoolTimeFill>();
            ftf.SetTarget(transform);

            // Canvas��Transform���擾�i�Ⴆ�΁ACanvas��"Canvas"�^�O�����Ă���Ȃ�j
            Transform canvasTransform = GameObject.FindGameObjectWithTag("Canvas").transform;

            // ���������I�u�W�F�N�g�̐e��Canvas�ɐݒ�iUI��RectTransform�Ȃ�K����������j
            obj.transform.SetParent(canvasTransform, false);

            SetStatus();
        }
    }

    void Update()
    {
        // �}�E�X�|�C���^�̃X�N���[�����W�����[���h���W�ɕϊ�
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2D�Q�[���Ȃ�Z���W��0�ɂ���

        if (photonView.IsMine && gameManager.isStart && !PhotonNetwork.LocalPlayer.GetDead())
        {
            Moving();
            RotateToMouse(mousePos);
            Reloading();

            if (Input.GetMouseButtonDown(0) && hasBullet)
            {
                Shooting();
            }
        }
    }

    // �ϐ��̏�����
    void SetStatus()
    {
        moveSpeed = 2.0f;      // �ړ����x�̏�����

        // �X�N���[���̉����A�c�����擾
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // �擾���������A�c������X�N���[���̍��W�̍ŏ��A�ő�l��ݒ�
        maxScreenPos = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, 0));
        minScreenPos = Camera.main.ScreenToWorldPoint(Vector3.zero);
    }

    [PunRPC]
    // �v���p�e�B�̏�����
    void SetPropaties()
    {
        PhotonNetwork.LocalPlayer.SetDead(false);
        PhotonNetwork.LocalPlayer.SetRank(0);
    }

    [PunRPC]
    // �_���[�W����
    public void TakeDamage()
    {
        Debug.Log("�_���[�W���m�F");

        Mathf.Clamp(hp, 0, maxHp);
        hp--;

        if (hp <= 0)
        {
            Debug.Log("HP���O�ɂȂ���");

            photonView.RPC(nameof(Dead), RpcTarget.All);
        }
    }

    [PunRPC]
    // ���S����
    void Dead()
    {
        PhotonNetwork.LocalPlayer.SetDead(true);
        SetNowRank();
        //RankScore(PhotonNetwork.LocalPlayer.GetRank());
        transform.position = gameManager.farstSpawn.transform.position;
    }

    // �v���C���[�̈ړ�
    void Moving()
    {
        // WASD�ňړ�����������擾
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.W)) y = 1.0f;
        if (Input.GetKey(KeyCode.S)) y = -1.0f;
        if (Input.GetKey(KeyCode.D)) x = 1.0f;
        if (Input.GetKey(KeyCode.A)) x = -1.0f;

        // ���ݒn����擾����������moveSpeed���ړ�����
        Vector3 moveVelocity = new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + moveVelocity;    // �����I�Ɉړ���̍��W���Ɍv�Z

        // ��ʊO�Ɏ��@���o�Ȃ��悤�Ɉړ��͈͂𐧌�
        newPosition.x = Mathf.Clamp(newPosition.x, minScreenPos.x + margin.x, maxScreenPos.x - margin.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minScreenPos.y + margin.y, maxScreenPos.y - margin.y);

        transform.position = newPosition;   // ���ݒn���ړ��������W�ɍX�V
    }

    // �}�E�X�̕����Ɏ��@��������
    void RotateToMouse(Vector3 mousePos)
    {
        Vector3 direction = mousePos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        // ���݂̌�������ڕW�̌����ցA��葬�x�ŉ�]
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    // �e�𐶐����A����
    void Shooting()
    {
        object[] instantiationData = new object[] { photonView.ViewID };
        GameObject bullet = PhotonNetwork.Instantiate("Bullet", bulletSpawn.position, transform.rotation, 0, instantiationData);
        hasBullet = false;
    }

    // �����[�h
    void Reloading()
    {
        if (!hasBullet)
        {
            time += Time.deltaTime;
            coolTimeFill.FillCoolTime(time, coolTime);

            if (time >= coolTime)
            {
                hasBullet = true;
                time = 0;
            }
        }
    }

    // ���S���̏��ʂ�ݒ�
    void SetNowRank()
    {
        GameManager game = GameObject.Find("GameManager").GetComponent<GameManager>();
        PhotonNetwork.LocalPlayer.SetRank(game.GetPlayerCnt());
    }

    // ���ʂɉ����ăX�R�A��ݒ�
    void RankScore(int rank)
    {
        int[] rankScores = { 0, 4, 3, 2, 1 };
        if (rank > 1 || rank < 4)
        {
            Debug.LogError("�s���ȏ��ʂ����m");
            return;
        }
        else
        {
            PhotonNetwork.LocalPlayer.AddScore(rankScores[rank]);
        }
    }
}
