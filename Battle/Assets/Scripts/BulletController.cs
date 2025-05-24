using Photon.Pun;
using UnityEngine;

public class BulletController : MonoBehaviourPunCallbacks
{
    float moveSpeed = 10.0f;
    Vector3 moveVector = Vector3.zero;
    int ownerViewID;

    public void SetOwner(int viewID)
    {
        ownerViewID = viewID;
    }

    void OnEnable()
    {
        moveVector = transform.up.normalized;

        if (photonView.IsMine)
        {
            photonView.RPC(nameof(DestroyBullet), RpcTarget.All);
        }
    }

    void Update()
    {
        Move();
    }

    [PunRPC]
    void DestroyBullet()
    {
        Destroy(this.gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView photonPlayer = other.gameObject.GetComponent<PhotonView>();
        if (photonPlayer != null)
        {
            // 発射者と違うプレイヤーにしか当たらない
            if (photonPlayer.ViewID != ownerViewID)
            {
                Debug.Log("他プレイヤーにヒット");
                photonPlayer.RPC("TakeDamage", RpcTarget.All);
                Destroy(gameObject); // ヒット後に弾を削除
            }
            else
            {
                Debug.Log("自分の弾なので無視");
            }
        }
    }

    void Move()
    {
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }
}
