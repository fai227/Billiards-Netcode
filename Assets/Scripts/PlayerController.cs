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
    private float cameraDistance = cameraFarDistance;
    private static float cameraFarDistance = 5f;
    private static float cameraNearDistance = 1f;
    private static float cameraModeDuration = 1f;

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
        //カメラを指定位置に移動
        ballCamera.transform.LookAt(ballCameraBase.transform);
        float magnitude = ballCamera.transform.position.magnitude;
        if(magnitude != cameraDistance)
        {
            ballCamera.transform.position = ballCamera.transform.position / magnitude * cameraDistance;
        }

        //カメラを動かすことが出来るときは動かす
        if (canMoveCamera)
        {
            Debug.Log("カメラを動かせます");
            ballCameraBase.transform.Rotate(Input.GetAxis("MouseY"), -Input.GetAxis("Mouse X"), 0f);
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
        if(flag && !isFoul)
        {
            canMoveCamera = false;
            ballCameraBase.transform.DOMove(GameObject.FindGameObjectWithTag("MainBall").transform.position, cameraModeDuration);
            //カメラの距離を徐々に近づけ、最後にカメラを動かせるようにする
            DOTween.To(
                () => cameraDistance,
                (val) => cameraDistance = val,
                cameraNearDistance,
                cameraModeDuration
                ).OnComplete(() => canMoveCamera = true);

        }
        else
        {
            SetCameraFollowBall();
        }
    }

    //カメラを戻す関数
    public void SetCameraFollowBall()
    {
        canMoveCamera = false;
        //ballCameraBase.transform.DOMove(Vector3.zero, cameraModeDuration);
        //ballCamera.transform.DOMove(ballCameraOriginPosition, cameraModeDuration);
    }

    //カメラの位置を設定する関数
    public void SetCameraPosition(bool reset = false)
    {
        //カメラリセット関数
        if (reset)
        {

        }
    }
    #endregion
}
