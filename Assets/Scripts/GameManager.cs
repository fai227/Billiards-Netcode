using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;
using Unity.Netcode.Transports.UNET;

public class GameManager : NetworkBehaviour
{
    #region Variables
    //�C���X�^���X
    public static GameManager Instance;

    //�Q�[���̎��
    public enum GameType
    {
        Fifteen_Ball = 0,
        Eight_Ball = 1,
        Night_Ball = 2,
        Snooker = 3
    }

    //�Q�[���ݒ�
    public NetworkVariable<bool> isPlaying;
    public NetworkVariable<GameType> gameType;

    //�^�[���Ǘ�
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

        //�v���C�J�n�E�I����F������R�[���o�b�N
        isPlaying.OnValueChanged += UIManager.Instance.SetGameUI;

        //�^�[���ύX�R�[���o�b�N
        whoseTurn.OnValueChanged += ChangeTurnCallBack;

        //�Q�[���̎�ޕύX�R�[���o�b�N
        gameType.OnValueChanged += UIManager.Instance.SetGameTypeUI;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //�v���C�J�n�E�I����F������R�[���o�b�N
        isPlaying.OnValueChanged -= UIManager.Instance.SetGameUI;

        //�^�[���ύX�R�[���o�b�N
        whoseTurn.OnValueChanged -= ChangeTurnCallBack;

        //�Q�[���̎�ޕύX�R�[���o�b�N
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

    //�v���C���[�����������̍ۂɌĂ΂��
    public void Ready()
    {
        if (isEveryoneReady())
        {
            StartGame();
        }
    }

    //�S���������������𒲂ׂ�
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

    //�Q�[���X�^�[�g�֐��i�T�[�o�[�T�C�h�݂̂Ŏ��s�����j
    private void StartGame()
    {
        isPlaying.Value = true;
        NetworkManager.Singleton.GetComponent<UNetTransport>().MaxConnections = NetworkManager.Singleton.ConnectedClientsIds.Count;

    }

    //�Q�[���I���֐��i�T�[�o�[�ēx�݂̂Ŏ��s�����j
    private void EndGame()
    {
        //������Ԃɖ߂�
        isPlaying.Value = true;
        NetworkManager.Singleton.GetComponent<UNetTransport>().MaxConnections = 8;
        //�v���C���[�̑ҋ@��Ԃ�߂�
        foreach(var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            player.PlayerObject.GetComponent<PlayerController>().isReady.Value = false;
        }
    }

    //�^�[�����R�[���o�b�N
    private void ChangeTurnCallBack(ulong prev, ulong next)
    {


        //�����̃^�[�����ύX
        BallController.myTurn = next == OwnerClientId;
    }
    #endregion
}
