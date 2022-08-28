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
        //�p�l�������ݒ�
        FadePanel(titlePanel, true, 0f);
        FadePanel(lobbyPanel, false, 0f);
        FadePanel(gamePanel, false, 0f);
    }
    #endregion

    #region Methods
    //�^�C�g���p�l���t�F�[�h�A�E�g�p
    public void EnterLobby()
    {
        FadePanel(playerInfPanel, true);
        FadePanel(titlePanel, false);
        FadePanel(lobbyPanel, true);
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
        //�Q�[���J�n
        if (next)
        {
            //�p�l���ݒ�
            FadePanel(gamePanel, true);
            FadePanel(lobbyPanel, false);

        }
        //�Q�[���I��
        else
        {
            //�p�l���ݒ�
            FadePanel(gamePanel, false);
            FadePanel(lobbyPanel, true);


        }
    }

    //�Q�[���̎�ނ�ύX����R�[���o�b�N
    public void SetGameTypeUI(GameManager.GameType _, GameManager.GameType next)
    {
        //�S�č��ɂ���
        for(int i = 0; i < 4; i++)
        {
            if((int)next != i)
            {
                gameTypeButtons[i].GetComponent<Image>().DOColor(Color.black, buttonDuration);
                gameTypeButtons[i].GetComponentInChildren<Text>().DOColor(Color.black, buttonDuration);
            }
        }

        //�I���������̂�Ԃɂ���
        gameTypeButtons[(int)next].GetComponent<Image>().DOColor(Color.red, buttonDuration);
        gameTypeButtons[(int)next].GetComponentInChildren<Text>().DOColor(Color.red, buttonDuration);
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
    public void SetPlayerUI(ulong id, bool flag, string name = "")
    {
        //�v���C���[�ڑ�
        if(flag)
        {
            //�v���C���[UI�쐬
            GameObject playerNameUI = Instantiate(playerPrefab, playersPanel.transform);
            playerNameUI.name = id.ToString();

            //���O���f
            playerNameUI.GetComponentInChildren<Text>().text = name;
        }
        //�v���C���[�ؒf
        else
        {
            GameObject playerNameUI = playersPanel.transform.Find(id.ToString()).gameObject;
            if(playerNameUI != null)
            {
                Destroy(playerNameUI);
            }
        }
    }

    //���݂̃v���C���[��UI��ݒ肷��֐�
    public void SetNowPlayerUI(ulong id, bool flag = true)
    {
        //�S�Ă�UI�����ɖ߂�
        for(int i = 0; i < playersPanel.transform.childCount; i++)
        {
            playersPanel.transform.GetChild(i).Find("Circle").GetComponent<Image>().sprite = noneSprite;
        }

        //���݂̃^�[���̃v���C���[UI�̐ݒ�
        if(flag)
        {
            playersPanel.transform.Find(id.ToString()).Find("Circle").GetComponent<Image>().sprite = rightSprite;
        }
    }

    //�p�l���̃t�F�[�h�֐�
    public void FadePanel(GameObject panel, bool flag, float time = 0.5f)
    {
        CanvasGroup panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if(panelCanvasGroup == null)
        {
            Debug.LogError($"Canvas Group is not attached to {panel.name}", this);
        }

        //�t�F�[�h�C��
        if (flag)
        {
            //�����ݒ�
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 0f;
            panel.SetActive(true);

            //�t�F�[�h�C���J�n
            panelCanvasGroup.DOFade(1f, time).OnComplete(() =>
            {
                //������ɐG���悤�ɂ���
                panelCanvasGroup.interactable = true;
            });
        }
        //�t�F�[�h�A�E�g
        else
        {
            //�����ݒ�
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.alpha = 1f;

            //�t�F�[�h�A�E�g�J�n
            panelCanvasGroup.DOFade(0f, time).OnComplete(() =>
            {
                //������ݒ�
                panel.SetActive(false);
            });
        }
    }

    //�X�R�A�Z�b�g�֐�
    public void SetScore(ulong id, int score)
    {
        playersPanel.transform.Find(id.ToString()).Find("Score").GetComponentInChildren<Text>().text = score.ToString();
    }

    //���[�h�ύX�֐�
    [ServerRpc(RequireOwnership = false)]public void SetModeServerRpc()
    {
        GameManager.Instance.hardMode.Value = !GameManager.Instance.hardMode.Value;
    }

    //���[�h���{�^���ɃZ�b�g�֐�
    public void SetModeButton(bool flag)
    {
        hardModeButton.GetComponentInChildren<Text>().text = flag ? "Hard" : "Easy";
    }
    #endregion
}
