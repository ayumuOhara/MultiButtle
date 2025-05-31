using Photon.Pun;
using UnityEngine;

public class BulletController : MonoBehaviourPunCallbacks
{
    float moveSpeed = 2.0f;
    Vector3 moveVector = Vector3.zero;
    int ownerViewID = -1;

    bool isFake = false;    // �t�F�C�N�e��

    void Awake()
    {
        // InstantiationData ���� ownerViewID ���擾
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            ownerViewID = (int)photonView.InstantiationData[0];
        }

        RPCStop(true);
    }

    void OnEnable()
    {
        moveVector = transform.up.normalized;

        if (photonView.IsMine)
        {
            Invoke(nameof(DestroyBullet), 5f); // ���[�J���Ŕj���^�C�}�[���J�n
        }
    }

    void Update()
    {
        Move();
    }

    [PunRPC]
    // �t�F�C�N�����t�^
    public void SetFake(bool fake)
    {
        isFake = fake;
    }

    // �e�����ł�����
    void DestroyBullet()
    {
        RPCStop(false);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView photonPlayer = other.gameObject.GetComponent<PhotonView>();
        if (photonPlayer != null)
        {
            Debug.Log($"[Bullet] Hit ViewID: {photonPlayer.ViewID}, Owner ViewID: {ownerViewID}, Object: {other.gameObject.name}");

            if (photonPlayer.ViewID != ownerViewID)
            {
                if (isFake)
                {
                    DestroyBullet();
                }
                else
                {
                    photonPlayer.RPC("TakeDamage", RpcTarget.All);
                    DestroyBullet();
                }                
            }
            else
            {
                Debug.Log("���g�ɓ��������̂ŉ���������Ȃ�");
            }
        }
    }

    // GameManager��isStop���Ăяo��
    void RPCStop(bool stop)
    {
        PhotonView gameView = GameObject.Find("GameManager").GetComponent<PhotonView>();
        gameView.RPC("SetStop", RpcTarget.All, stop);
    }

    // �������ꂽ�Ƃ��̃v���C���[�̌����Ɉړ�
    void Move()
    {
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }
}
