using Photon.Pun;
using UnityEngine;

public class BulletController : MonoBehaviourPunCallbacks
{
    float moveSpeed = 10.0f;
    Vector3 moveVector = Vector3.zero;

    void OnEnable()
    {
        // �e�̈ړ��������v�Z
        moveVector = transform.up.normalized;
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Move();
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        player.TakeDamage();
    }

    void Move()
    {
        // �v�Z���������Ɍ������Ĉړ�
        transform.position += moveVector * moveSpeed * Time.deltaTime;
    }
}
