using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using UnityEngine.UI;

public class UIManager : NetworkBehaviour
{
    #region Variables
    //インスタンス
    public static UIManager Instance;

    [Header("Title UI")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private InputField playerNameInputField;
    private static float shakeDuration = 0.5f;
    private static float fadeColorDuration = 0.1f;

    [Header("Player Name UI")]
    [SerializeField] private GameObject playersPanel;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Lobby UI")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button readyButton;
    private static float buttonDuration = 0.5f;
    [SerializeField] private Button[] gameTypeButtons;

    [Header("Game UI")]
    [SerializeField] private GameObject gamePanel;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //パネル初期設定
        titlePanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
    }
    #endregion

    #region Methods
    //タイトルパネルフェードアウト用
    public void FadeTitlePanel()
    {
        titlePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    } 

    //プレイヤー名取得関数
    public string GetPlayerName()
    {
        return playerNameInputField.text;
    }

    //プレイヤー名が入力されていないときに揺らす関数
    public void ShakePlayerName()
    {
        //色を赤に変える
        Image playerNameInputFieldImage =  playerNameInputField.GetComponent<Image>();
        playerNameInputFieldImage.DOColor(Color.red, fadeColorDuration);
        DOVirtual.DelayedCall(shakeDuration - fadeColorDuration, () =>
        {
            playerNameInputFieldImage.DOColor(Color.black, fadeColorDuration);
        });

        //揺らす
        playerNameInputField.GetComponent<RectTransform>().DOShakePosition(duration:shakeDuration, strength:5) ;
    }

    //準備完了ボタンを押す
    public void SetReady()
    {
        PlayerController mainPlayerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        mainPlayerController.SetReadyStateServerRpc(!mainPlayerController.isReady.Value);
    }

    //準備完了が変更されたときにUIを変更するコールバック
    public void SetReadyUI(bool _, bool next)
    {
        //準備完了
        if(next)
        {
            //文字とボタン色変更
            readyButton.GetComponent<Image>().DOColor(Color.red, buttonDuration);
            
            Text tmpText = readyButton.GetComponentInChildren<Text>();
            tmpText.text = "Cancel";
            tmpText.DOColor(Color.red, buttonDuration);
        }
        else
        {
            //文字とボタン色変更
            readyButton.GetComponent<Image>().DOColor(Color.black, buttonDuration);

            Text tmpText = readyButton.GetComponentInChildren<Text>();
            tmpText.text = "Ready";
            tmpText.DOColor(Color.black, buttonDuration);
        }
    }

    //ゲーム開始・終了されたときに呼び出されるコールバック
    public void SetGameUI(bool _, bool next)
    {

    }

    //ゲームの種類を変更するコールバック
    public void SetGameTypeUI(GameManager.GameType prev, GameManager.GameType next)
    {
        //選択したものを赤にする
        gameTypeButtons[(int)next].GetComponent<Image>().DOColor(Color.red, buttonDuration);
        gameTypeButtons[(int)next].GetComponentInChildren<Text>().DOColor(Color.red, buttonDuration);

        //選択解除されたものを黒にする
        gameTypeButtons[(int)prev].GetComponent<Image>().DOColor(Color.black, buttonDuration);
        gameTypeButtons[(int)prev].GetComponentInChildren<Text>().DOColor(Color.black, buttonDuration);
    }

    //ゲームの変更を行う関数
    [ServerRpc(RequireOwnership = false)] public void SetGameTypeServerRpc(int typeNum)
    {
        //準備中の時は変更できない
        if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().isReady.Value)
        {
            return;
        }

        GameManager.Instance.gameType.Value = (GameManager.GameType)Enum.ToObject(typeof(GameManager.GameType), typeNum);
    }

    //プレイヤーのUIをセットする関数
    public void SetPlayerUI(ulong id)
    {

    }

    //現在のプレイヤーのUIを設定する関数
    public void SetNowPlayerUI(ulong id)
    {

    }
    #endregion
}
