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
    [SerializeField] GameObject playerInfPanel;
    [SerializeField] private GameObject playersPanel;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Sprite noneSprite;
    [SerializeField] private Sprite rightSprite;

    [Header("Lobby UI")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private Button readyButton;
    private static float buttonDuration = 0.5f;
    [SerializeField] private Button[] gameTypeButtons;
    [SerializeField] private Button hardModeButton;

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
        FadePanel(titlePanel, true, 0f);
        FadePanel(lobbyPanel, false, 0f);
        FadePanel(gamePanel, false, 0f);
    }
    #endregion

    #region Methods
    //タイトルパネルフェードアウト用
    public void EnterLobby()
    {
        FadePanel(playerInfPanel, true);
        FadePanel(titlePanel, false);
        FadePanel(lobbyPanel, true);
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
        //ゲーム開始
        if (next)
        {
            //パネル設定
            FadePanel(gamePanel, true);
            FadePanel(lobbyPanel, false);

        }
        //ゲーム終了
        else
        {
            //パネル設定
            FadePanel(gamePanel, false);
            FadePanel(lobbyPanel, true);


        }
    }

    //ゲームの種類を変更するコールバック
    public void SetGameTypeUI(GameManager.GameType _, GameManager.GameType next)
    {
        //全て黒にする
        for(int i = 0; i < 4; i++)
        {
            if((int)next != i)
            {
                gameTypeButtons[i].GetComponent<Image>().DOColor(Color.black, buttonDuration);
                gameTypeButtons[i].GetComponentInChildren<Text>().DOColor(Color.black, buttonDuration);
            }
        }

        //選択したものを赤にする
        gameTypeButtons[(int)next].GetComponent<Image>().DOColor(Color.red, buttonDuration);
        gameTypeButtons[(int)next].GetComponentInChildren<Text>().DOColor(Color.red, buttonDuration);
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
    public void SetPlayerUI(ulong id, bool flag, string name = "")
    {
        //プレイヤー接続
        if(flag)
        {
            //プレイヤーUI作成
            GameObject playerNameUI = Instantiate(playerPrefab, playersPanel.transform);
            playerNameUI.name = id.ToString();

            //名前反映
            playerNameUI.GetComponentInChildren<Text>().text = name;
        }
        //プレイヤー切断
        else
        {
            GameObject playerNameUI = playersPanel.transform.Find(id.ToString()).gameObject;
            if(playerNameUI != null)
            {
                Destroy(playerNameUI);
            }
        }
    }

    //現在のプレイヤーのUIを設定する関数
    public void SetNowPlayerUI(ulong id, bool flag = true)
    {
        //全てのUIを元に戻す
        for(int i = 0; i < playersPanel.transform.childCount; i++)
        {
            playersPanel.transform.GetChild(i).Find("Circle").GetComponent<Image>().sprite = noneSprite;
        }

        //現在のターンのプレイヤーUIの設定
        if(flag)
        {
            playersPanel.transform.Find(id.ToString()).Find("Circle").GetComponent<Image>().sprite = rightSprite;
        }
    }

    //パネルのフェード関数
    public void FadePanel(GameObject panel, bool flag, float time = 0.5f)
    {
        CanvasGroup panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if(panelCanvasGroup == null)
        {
            Debug.LogError($"Canvas Group is not attached to {panel.name}", this);
        }

        //フェードイン
        if (flag)
        {
            //初期設定
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 0f;
            panel.SetActive(true);

            //フェードイン開始
            panelCanvasGroup.DOFade(1f, time).OnComplete(() =>
            {
                //完了後に触れるようにする
                panelCanvasGroup.interactable = true;
            });
        }
        //フェードアウト
        else
        {
            //初期設定
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 1f;

            //フェードアウト開始
            panelCanvasGroup.DOFade(0f, time).OnComplete(() =>
            {
                //完了後設定
                panel.SetActive(false);
            });
        }
    }

    //スコアセット関数
    public void SetScore(ulong id, int score)
    {
        playersPanel.transform.Find(id.ToString()).Find("Score").GetComponentInChildren<Text>().text = score.ToString();
    }

    //モード変更関数
    [ServerRpc(RequireOwnership = false)]public void SetModeServerRpc()
    {
        GameManager.Instance.hardMode.Value = !GameManager.Instance.hardMode.Value;
    }

    //モードをボタンにセット関数
    public void SetModeButton(bool flag)
    {
        hardModeButton.GetComponentInChildren<Text>().text = flag ? "Hard" : "Easy";
    }
    #endregion
}
