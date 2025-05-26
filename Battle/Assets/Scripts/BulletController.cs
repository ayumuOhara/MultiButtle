using Photon.Pun;
using UnityEngine;

public class BulletController : MonoBehaviourPunCallbacks
{
    float moveSpeed = 10.0f;
    Vector3 moveVector = Vector3.zero;
    int ownerViewID = -1;

    void Awake()
    {
        // InstantiationData から ownerViewID を取得
        if (photonView.InstantiationData != null && photonView.InstantiationData.Length > 0)
        {
            ownerViewID = (int)photonView.InstantiationData[0];
        }
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

    void DestroyBullet()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView photonPlayer = other.gameObject.GetComponent<PhotonView>();
        if (photonPlayer != null)
        {
            Debug.Log($"[Bullet] Hit ViewID: {photonPlayer.ViewID}, Owner ViewID: {ownerViewID}, Object: {other.gameObject.name}");

            if (photonPlayer.ViewID != ownerViewID)
            {
                Debug.Log("[Bullet] Damage applied");
                photonPlayer.RPC("TakeDamage", RpcTarget.All);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("[Bullet] Hit owner, no damage");
            }
        }
    }

    void Move()
    {
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }
}
