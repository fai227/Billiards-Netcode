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
    //�C���X�^���X
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
        //�p�l�������ݒ�
        titlePanel.SetActive(true);
        lobbyPanel.SetActive(false);
        gamePanel.SetActive(false);
    }
    #endregion

    #region Methods
    //�^�C�g���p�l���t�F�[�h�A�E�g�p
    public void FadeTitlePanel()
    {
        titlePanel.SetActive(false);
        lobbyPanel.SetActive(true);
    } 

    //�v���C���[���擾�֐�
    public string GetPlayerName()
    {
        return playerNameInputField.text;
    }

    //�v���C���[�������͂���Ă��Ȃ��Ƃ��ɗh�炷�֐�
    public void ShakePlayerName()
    {
        //�F��Ԃɕς���
        Image playerNameInputFieldImage =  playerNameInputField.GetComponent<Image>();
        playerNameInputFieldImage.DOColor(Color.red, fadeColorDuration);
        DOVirtual.DelayedCall(shakeDuration - fadeColorDuration, () =>
        {
            playerNameInputFieldImage.DOColor(Color.black, fadeColorDuration);
        });

        //�h�炷
        playerNameInputField.GetComponent<RectTransform>().DOShakePosition(duration:shakeDuration, strength:5) ;
    }

    //���������{�^��������
    public void SetReady()
    {
        PlayerController mainPlayerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        mainPlayerController.SetReadyStateServerRpc(!mainPlayerController.isReady.Value);
    }

    //�����������ύX���ꂽ�Ƃ���UI��ύX����R�[���o�b�N
    public void SetReadyUI(bool _, bool next)
    {
        //��������
        if(next)
        {
            //�����ƃ{�^���F�ύX
            readyButton.GetComponent<Image>().DOColor(Color.red, buttonDuration);
            
            Text tmpText = readyButton.GetComponentInChildren<Text>();
            tmpText.text = "Cancel";
            tmpText.DOColor(Color.red, buttonDuration);
        }
        else
        {
            //�����ƃ{�^���F�ύX
            readyButton.GetComponent<Image>().DOColor(Color.black, buttonDuration);

            Text tmpText = readyButton.GetComponentInChildren<Text>();
            tmpText.text = "Ready";
            tmpText.DOColor(Color.black, buttonDuration);
        }
    }

    //�Q�[���J�n�E�I�����ꂽ�Ƃ��ɌĂяo�����R�[���o�b�N
    public void SetGameUI(bool _, bool next)
    {

    }

    //�Q�[���̎�ނ�ύX����R�[���o�b�N
    public void SetGameTypeUI(GameManager.GameType prev, GameManager.GameType next)
    {
        //�I���������̂�Ԃɂ���
        gameTypeButtons[(int)next].GetComponent<Image>().DOColor(Color.red, buttonDuration);
        gameTypeButtons[(int)next].GetComponentInChildren<Text>().DOColor(Color.red, buttonDuration);

        //�I���������ꂽ���̂����ɂ���
        gameTypeButtons[(int)prev].GetComponent<Image>().DOColor(Color.black, buttonDuration);
        gameTypeButtons[(int)prev].GetComponentInChildren<Text>().DOColor(Color.black, buttonDuration);
    }

    //�Q�[���̕ύX���s���֐�
    [ServerRpc(RequireOwnership = false)] public void SetGameTypeServerRpc(int typeNum)
    {
        //�������̎��͕ύX�ł��Ȃ�
        if (NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().isReady.Value)
        {
            return;
        }

        GameManager.Instance.gameType.Value = (GameManager.GameType)Enum.ToObject(typeof(GameManager.GameType), typeNum);
    }

    //�v���C���[��UI���Z�b�g����֐�
    public void SetPlayerUI(ulong id)
    {

    }

    //���݂̃v���C���[��UI��ݒ肷��֐�
    public void SetNowPlayerUI(ulong id)
    {

    }
    #endregion
}
