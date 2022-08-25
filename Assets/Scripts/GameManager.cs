using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using Unity.Netcode.Transports.UNET;

public class GameManager : NetworkBehaviour
{
    #region Variables
    //インスタンス
    public static GameManager Instance;

    //ゲームの種類
    public enum GameType
    {
        Fifteen_Ball = 0,
        Eight_Ball = 1,
        Night_Ball = 2,
        Snooker = 3
    }

    //ゲーム設定
    public NetworkVariable<bool> isPlaying;
    public NetworkVariable<GameType> gameType;

    //ターン管理
    public NetworkVariable<ulong> whoseTurn;

    #endregion

    #region Unity Methods
    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //プレイ開始・終了を認識するコールバック
        isPlaying.OnValueChanged += UIManager.Instance.SetGameUI;

        //ターン変更コールバック
        whoseTurn.OnValueChanged += ChangeTurnCallBack;

        //ゲームの種類変更コールバック
        gameType.OnValueChanged += UIManager.Instance.SetGameTypeUI;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //プレイ開始・終了を認識するコールバック
        isPlaying.OnValueChanged -= UIManager.Instance.SetGameUI;

        //ターン変更コールバック
        whoseTurn.OnValueChanged -= ChangeTurnCallBack;

        //ゲームの種類変更コールバック
        gameType.OnValueChanged -= UIManager.Instance.SetGameTypeUI;
    }

    #endregion

    #region Methods
    public void StartHost()
    {
        if (UIManager.Instance.GetPlayerName() == "")
        {
            UIManager.Instance.ShakePlayerName();
            return;
        }

        NetworkManager.Singleton.StartHost();
        UIManager.Instance.FadeTitlePanel();
    }

    public void StartClient()
    {
        if(UIManager.Instance.GetPlayerName() == "")
        {
            UIManager.Instance.ShakePlayerName();
            return;
        }

        NetworkManager.Singleton.StartClient();
        UIManager.Instance.FadeTitlePanel();
    }

    //プレイヤーが準備完了の際に呼ばれる
    public void Ready()
    {
        if (isEveryoneReady())
        {
            StartGame();
        }
    }

    //全員が準備完了かを調べる
    private bool isEveryoneReady()
    {
        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (!player.PlayerObject.GetComponent<PlayerController>().isReady.Value)
            {
                return false;
            }
        }
        return true;
    }

    //ゲームスタート関数（サーバーサイドのみで実行される）
    private void StartGame()
    {
        isPlaying.Value = true;
        NetworkManager.Singleton.GetComponent<UNetTransport>().MaxConnections = NetworkManager.Singleton.ConnectedClientsIds.Count;

    }

    //ゲーム終了関数（サーバー再度のみで実行される）
    private void EndGame()
    {
        //初期状態に戻す
        isPlaying.Value = true;
        NetworkManager.Singleton.GetComponent<UNetTransport>().MaxConnections = 8;
        //プレイヤーの待機状態を戻す
        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.GetComponent<PlayerController>().isReady.Value = false;
        }
    }

    //ターン交代コールバック
    private void ChangeTurnCallBack(ulong prev, ulong next)
    {


        //自分のターンか変更
        BallController.myTurn = next == OwnerClientId;
    }
    #endregion
}
