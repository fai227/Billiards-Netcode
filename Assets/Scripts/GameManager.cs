using System;
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
        Nine_Ball = 2,
        Snooker = 3,
    }

    [Header("Game Setting")]
    public NetworkVariable<bool> isPlaying;
    public NetworkVariable<GameType> gameType;
    public NetworkVariable<ulong> whoseTurn;
    private int startPlayerNum;
    private int turnNum;
    public NetworkVariable<bool> hardMode;

    [Header("Ball")]
    [SerializeField] private GameObject ballPrefab;

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

        //モード変更コールバック
        hardMode.OnValueChanged += ModeCallback;
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
        UIManager.Instance.EnterLobby();
    }

    public void StartClient()
    {
        if(UIManager.Instance.GetPlayerName() == "")
        {
            UIManager.Instance.ShakePlayerName();
            return;
        }

        NetworkManager.Singleton.StartClient();
        UIManager.Instance.EnterLobby();
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


        //ボール設置

        //ボール準備
        int[] ballNumbers = new int[15];
        switch (gameType.Value)
        {
            case GameType.Nine_Ball:
                //ボール場所指定
                Array.Copy(new int[] { 1, 5, 6, 3, 9, 4, -1, 7, 8, -1, -1, -1, 2, -1, -1 }, ballNumbers,ballNumbers.Length);
                break;

            case GameType.Fifteen_Ball:
                //ボール場所指定
                Array.Copy(new int[] { 1, 7, 8, 11, 12, 13, 9, 14, 15, 10, 2, 4, 5, 6, 3 }, ballNumbers, ballNumbers.Length);
                break;
            case GameType.Eight_Ball:
                //ボールランダム
                int[] tmpNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                ArrayMethods.Shuffle(tmpNumbers);

                //8を真ん中に
                int eightPlace = Array.IndexOf(tmpNumbers, 8);
                ArrayMethods.Replace(tmpNumbers, eightPlace, 4);

                //15を左手前に
                int fifteenPlace = Array.IndexOf(tmpNumbers, 15);
                ArrayMethods.Replace(tmpNumbers, fifteenPlace, 10);

                //1を右手前に
                int onePlace = Array.IndexOf(tmpNumbers, 1);
                ArrayMethods.Replace(tmpNumbers, onePlace, 14);

                //ボール場所指定
                Array.Copy(tmpNumbers, ballNumbers, ballNumbers.Length);

                    break;
            case GameType.Snooker:
                //ボール場所指定
                Array.Copy(new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }, ballNumbers, ballNumbers.Length);
                break;
        }

        //ボール設置
        //変数準備
        float ballRadius = ballPrefab.transform.localScale.x / 2f;
        float rootThree = Mathf.Sqrt(3);
        Vector3 originPosition = new Vector3(0.68f, 0f, 0f);
        Quaternion rot = Quaternion.Euler(90f, 90f, 0f);

        int ballNumber = 0;
        for(int i = 0; i < 5; i++)
        {
            float x = originPosition.x + rootThree * i * ballRadius;
            for (int s = 0; s < i + 1; s++)
            {
                if (ballNumbers[ballNumber] > 0)
                {
                    float z = originPosition.z + 2f * s * ballRadius - i * ballRadius;
                    GenerateBall(ballNumbers[ballNumber], new Vector3(x, originPosition.y, z), rot);
                }
                ballNumber++;
            }
        }

        //白球設置
        GenerateBall(0, new Vector3(-0.68f, 0f, 0f), Quaternion.Euler(90f, 90f, 0f));

        //ゲーム開始（ターン初期化）
        startPlayerNum = UnityEngine.Random.Range(0, NetworkManager.ConnectedClientsIds.Count);
        NextTurn(); 
    }

    public void NextTurn()
    {
        turnNum++;
        whoseTurn.Value = NetworkManager.ConnectedClientsIds[(startPlayerNum + turnNum) % NetworkManager.ConnectedClientsIds.Count];
    }

    //ゲーム終了関数（サーバーサイドのみで実行される）
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
        if(next >= 0)
        {
            //ターンのUIを変更
            UIManager.Instance.SetNowPlayerUI(next);
        }

        //自分のターンか変更
        BallController.myTurn = next == OwnerClientId;
    }

    //モード変更コールバック
    private void ModeCallback(bool _, bool next)
    {
        UIManager.Instance.SetModeButton(next);
    }

    //ボール設置関数（サーバーサイドのみで実行される）
    private void GenerateBall(int ballNumber, Vector3 pos, Quaternion rot)
    {
        GameObject ballObject = Instantiate(ballPrefab);
        BallController ballController = ballObject.GetComponent<BallController>();

        ballController.ballNumber.Value = ballNumber;
        ballController.networkPos.Value = pos;
        ballController.networkRot.Value = rot;

        ballObject.GetComponent<NetworkObject>().Spawn();
    }

    
    #endregion
}

/// <summary>
/// 自作便利配列クラス
/// </summary>
public static class ArrayMethods
{
    /// <summary>
    /// 配列シャッフル関数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    public static void Shuffle<T>(T[] array) 
    {
        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            var r = UnityEngine.Random.Range(0, i + 1);
            Replace(array, i, r);
        }
    }

    /// <summary>
    /// 配列入れ替え関数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="n"></param>
    /// <param name="m"></param>
    public static void Replace<T>(T[] array, int n, int m)
    {
        //番号が同じときは行わない
        if(n == m)
        {
            return;
        }

        var tmp = array[n];
        array[n] = array[m];
        array[m] = tmp;
    }
}
