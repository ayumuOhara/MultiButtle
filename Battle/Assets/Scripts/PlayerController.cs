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

    [SerializeField] GameObject coolTimeBar;    // クールタイムバーのプレファブ
    [SerializeField] GameObject bulletPrefab;   // 弾のプレファブ
    [SerializeField] Transform bulletSpawn;     // 弾が生成される場所
    [SerializeField] float rotateSpeed = 180f;  // 回転速度（1秒間に回転する角度の最大値）

    public float moveSpeed { get; private set; }    // 移動速度
    float screenWidth = 0f;                         // 画面の横幅
    float screenHeight = 0f;                        // 画面の縦幅
    Vector3 screenSize = Vector3.zero;              // スクリーンの面積
    Vector3 minScreenPos = Vector3.zero;            // スクリーンの左下の座標
    Vector3 maxScreenPos = Vector3.zero;            // スクリーンの右上の座標
    Vector3 margin = new Vector3(0.6f, 0.6f);

    float time = 0f;
    float coolTime = 5.0f;  // 次の弾が発射されるまでの時間

    bool hasBullet = true;  // 弾が発射できるか

    void OnEnable()
    {
        photonView.RPC(nameof(SetPropaties), RpcTarget.All);
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

            // 生成位置と回転は適当に指定（Canvas内UIならVector3.zero, Quaternion.identityでOK）
            GameObject obj = Instantiate(coolTimeBar, Vector3.zero, Quaternion.identity);
            FollowTransform ftf = obj.GetComponent<FollowTransform>();
            coolTimeFill = obj.GetComponent<CoolTimeFill>();
            ftf.SetTarget(transform);

            // CanvasのTransformを取得（例えば、Canvasに"Canvas"タグをつけているなら）
            Transform canvasTransform = GameObject.FindGameObjectWithTag("Canvas").transform;

            // 生成したオブジェクトの親をCanvasに設定（UIのRectTransformなら必ずこうする）
            obj.transform.SetParent(canvasTransform, false);

            SetStatus();
        }
    }

    void Update()
    {
        // マウスポインタのスクリーン座標をワールド座標に変換
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // 2DゲームならZ座標を0にする

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

    // 変数の初期化
    void SetStatus()
    {
        moveSpeed = 2.0f;      // 移動速度の初期化

        // スクリーンの横幅、縦幅を取得
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        // 取得した横幅、縦幅からスクリーンの座標の最小、最大値を設定
        maxScreenPos = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, 0));
        minScreenPos = Camera.main.ScreenToWorldPoint(Vector3.zero);
    }

    [PunRPC]
    // プロパティの初期化
    void SetPropaties()
    {
        PhotonNetwork.LocalPlayer.SetDead(false);
        PhotonNetwork.LocalPlayer.SetRank(0);
    }

    [PunRPC]
    // ダメージ処理
    public void TakeDamage()
    {
        Debug.Log("ダメージを確認");

        Mathf.Clamp(hp, 0, maxHp);
        hp--;

        if (hp <= 0)
        {
            Debug.Log("HPが０になった");

            photonView.RPC(nameof(Dead), RpcTarget.All);
        }
    }

    [PunRPC]
    // 死亡処理
    void Dead()
    {
        PhotonNetwork.LocalPlayer.SetDead(true);
        SetNowRank();
        //RankScore(PhotonNetwork.LocalPlayer.GetRank());
        transform.position = gameManager.farstSpawn.transform.position;
    }

    // プレイヤーの移動
    void Moving()
    {
        // WASDで移動する向きを取得
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.W)) y = 1.0f;
        if (Input.GetKey(KeyCode.S)) y = -1.0f;
        if (Input.GetKey(KeyCode.D)) x = 1.0f;
        if (Input.GetKey(KeyCode.A)) x = -1.0f;

        // 現在地から取得した向きにmoveSpeed分移動する
        Vector3 moveVelocity = new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;
        Vector3 newPosition = transform.position + moveVelocity;    // 内部的に移動先の座標を先に計算

        // 画面外に自機が出ないように移動範囲を制限
        newPosition.x = Mathf.Clamp(newPosition.x, minScreenPos.x + margin.x, maxScreenPos.x - margin.x);
        newPosition.y = Mathf.Clamp(newPosition.y, minScreenPos.y + margin.y, maxScreenPos.y - margin.y);

        transform.position = newPosition;   // 現在地を移動した座標に更新
    }

    // マウスの方向に自機を向ける
    void RotateToMouse(Vector3 mousePos)
    {
        Vector3 direction = mousePos - transform.position;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.AngleAxis(targetAngle, Vector3.forward);

        // 現在の向きから目標の向きへ、一定速度で回転
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    // 弾を生成し、発射
    void Shooting()
    {
        object[] instantiationData = new object[] { photonView.ViewID };
        GameObject bullet = PhotonNetwork.Instantiate("Bullet", bulletSpawn.position, transform.rotation, 0, instantiationData);
        hasBullet = false;
    }

    // リロード
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

    // 死亡時の順位を設定
    void SetNowRank()
    {
        GameManager game = GameObject.Find("GameManager").GetComponent<GameManager>();
        PhotonNetwork.LocalPlayer.SetRank(game.GetPlayerCnt());
    }

    // 順位に応じてスコアを設定
    void RankScore(int rank)
    {
        int[] rankScores = { 0, 4, 3, 2, 1 };
        if (rank > 1 || rank < 4)
        {
            Debug.LogError("不正な順位を検知");
            return;
        }
        else
        {
            PhotonNetwork.LocalPlayer.AddScore(rankScores[rank]);
        }
    }
}
