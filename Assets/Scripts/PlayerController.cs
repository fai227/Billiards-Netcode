using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using DG.Tweening;

public class PlayerController : NetworkBehaviour
{
    #region Variables
    [Header("Player Setting")]
    public NetworkVariable<FixedString32Bytes> playerName = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<bool> isReady;
    public NetworkVariable<int> score;

    [Header("Camera")]
    private GameObject ballCameraBase;
    private Camera ballCamera;
    private bool canMoveCamera;
    private static Vector3 cameraFarPosition = new Vector3(0f, 1.5f, -1.5f);
    private static Vector3 cameraNearPosition = new Vector3(0f, 0.25f, -0.5f);
    private static float cameraModeDuration = 2f;

    public static bool settingMainBall;
    public static bool finishedShot;

    #endregion

    #region Unity Methods
    //コールバックセット＆プレイヤー名セット
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //名前のコールバック
        playerName.OnValueChanged += SetPlayerName;

        //スコアのコールバック
        score.OnValueChanged += ScoreCallback;

        if (IsOwner)
        {
            //準備完了のコールバック
            isReady.OnValueChanged += UIManager.Instance.SetReadyUI;

            //名前を反映
            playerName.Value = UIManager.Instance.GetPlayerName();

            //ゲームの種類とモードを設定
            UIManager.Instance.SetGameTypeUI(GameManager.GameType.Fifteen_Ball, GameManager.Instance.gameType.Value);
            UIManager.Instance.SetModeButton(GameManager.Instance.hardMode.Value);
        }
    }

    //コールバック解除
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //名前のコールバック
        playerName.OnValueChanged -= SetPlayerName;
        UIManager.Instance.SetPlayerUI(OwnerClientId, false);   //プレイヤーの名前を消す

        //スコアのコールバック
        score.OnValueChanged -= ScoreCallback;

        if (IsOwner)
        {
            //準備完了のコールバック
            isReady.OnValueChanged -= UIManager.Instance.SetReadyUI;
        }
    }

    private void Start()
    {
        //初期設定
        ballCameraBase = GameObject.Find("BallCameraBase");
        ballCamera = GameObject.Find("BallCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        //カメラ移動
        if (IsOwner)
        {
            //カメラを動かすことが出来るときは動かす
            if (canMoveCamera)
            {
                if (!Input.GetMouseButton(0))
                {
                    Vector3 prev = ballCameraBase.transform.rotation.eulerAngles;
                    ballCameraBase.transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
                }
            }
        }

        //メインボールを設置する場所を表示する
        if (settingMainBall)
        {


            //白球が置かれたとき
            if (Input.GetMouseButtonDown(0))
            {
                settingMainBall = false;
                SetCameraPosition(true);
            }
        }
    }
    #endregion

    #region Methods
    //プレイヤーの名前をセット
    private void SetPlayerName(FixedString32Bytes prev, FixedString32Bytes next)
    {
        gameObject.name = next.ToString();
        UIManager.Instance.SetPlayerUI(OwnerClientId, true, playerName.Value.ToString());    //プレイヤーの名前表示
    }

    //準備状態をサーバーに変更依頼
    [ServerRpc(RequireOwnership = false)] public void SetReadyStateServerRpc(bool flag)
    {
        //準備状態をセット
        isReady.Value = flag;
        
        //開始を判定
        if(flag)
        {
            GameManager.Instance.Ready();
        }
    }

    //スコアセットコールバック
    private void ScoreCallback(int _, int next)
    {
        UIManager.Instance.SetScore(OwnerClientId, next);
    }

    //自分のターンかをセットする
    public void SetMyTurn(bool flag, bool isFoul)
    {
        //自身のターンで、ファールでなければカメラを追尾モードに設定
        SetCameraPosition(flag && !isFoul);

        //白球を生成する場合
        if(flag && isFoul)
        {
            settingMainBall = true;
        }
    }

    //カメラの位置を設定する関数
    public void SetCameraPosition(bool follow)
    {
        //カメラを動かせなくする
        canMoveCamera = false;

        //白玉を追うとき
        if (follow)
        {
            Vector3 mainBallPosition = GameObject.FindGameObjectWithTag("MainBall").transform.position;
            ballCameraBase.transform.DOLocalMove(mainBallPosition, cameraModeDuration);

            float centerRotationY = Quaternion.LookRotation(-mainBallPosition).eulerAngles.y;
            ballCameraBase.transform.DOLocalRotate(new Vector3(0f, centerRotationY, 0f), cameraModeDuration);

            ballCamera.transform.DOLocalMove(cameraNearPosition, cameraModeDuration);
            float angle = Mathf.Atan(-cameraNearPosition.y / cameraNearPosition.z) * Mathf.Rad2Deg;
            ballCamera.transform.DOLocalRotate(new Vector3(angle, 0f, 0f), cameraModeDuration);
        }
        //カメラをリセットするとき
        else
        {
            ballCameraBase.transform.DOLocalMove(Vector3.zero, cameraModeDuration);
            ballCameraBase.transform.DOLocalRotateQuaternion(Quaternion.identity, cameraModeDuration);
            ballCamera.transform.DOLocalMove(cameraFarPosition, cameraModeDuration);
            float angle = Mathf.Atan(-cameraFarPosition.y / cameraFarPosition.z) * Mathf.Rad2Deg;
            ballCamera.transform.DOLocalRotate(new Vector3(angle, 0f, 0f), cameraModeDuration);
        }

        //カメラを時間をおいて動かせるようにする
        DOVirtual.DelayedCall(cameraModeDuration, () =>
        {
            canMoveCamera = true;

            //キューを動かせるようにする
            if (follow)
            {
                GameObject.FindGameObjectWithTag("Cue").GetComponent<CueController>().canMove = true;
            }
        }, false);
    }
    #endregion
}
