using Photon.Pun;
using UnityEngine;

public class BulletController : MonoBehaviourPunCallbacks
{
    float moveSpeed = 2.0f;
    Vector3 moveVector = Vector3.zero;
    int ownerViewID = -1;

    bool isFake = false;    // フェイク弾か

    void Awake()
    {
        // InstantiationData から ownerViewID を取得
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
            Invoke(nameof(DestroyBullet), 5f); // ローカルで破棄タイマーを開始
        }
    }

    void Update()
    {
        Move();
    }

    [PunRPC]
    // フェイク属性付与
    public void SetFake(bool fake)
    {
        isFake = fake;
    }

    // 弾を消滅させる
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
                Debug.Log("自身に当たったので何もおこらない");
            }
        }
    }

    // GameManagerのisStopを呼び出す
    void RPCStop(bool stop)
    {
        PhotonView gameView = GameObject.Find("GameManager").GetComponent<PhotonView>();
        gameView.RPC("SetStop", RpcTarget.All, stop);
    }

    // 生成されたときのプレイヤーの向きに移動
    void Move()
    {
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }
}
