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
    //�C���X�^���X
    public static GameManager Instance;

    //�Q�[���̎��
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
    public ulong whoseTurn;
    private int startPlayerNum;
    private int turnNum;
    public NetworkVariable<bool> hardMode;

    [Header("Ball")]
    [SerializeField] private GameObject ballPrefab;
    private List<int> ballList = new();

    [Header("Cue")]
    [SerializeField] private GameObject cuePrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    [SerializeField] private AudioClip startAudio;

    #endregion

    #region Unity Methods
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //�v���C�J�n�E�I����F������R�[���o�b�N
        isPlaying.OnValueChanged += UIManager.Instance.SetGameUI;

        //�Q�[���̎�ޕύX�R�[���o�b�N
        gameType.OnValueChanged += UIManager.Instance.SetGameTypeUI;

        //���[�h�ύX�R�[���o�b�N
        hardMode.OnValueChanged += ModeCallback;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //�v���C�J�n�E�I����F������R�[���o�b�N
        isPlaying.OnValueChanged -= UIManager.Instance.SetGameUI;

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


        //�{�[���ݒu


        //�{�[������
        int[] ballNumbers = new int[15];
        switch (gameType.Value)
        {
            case GameType.Nine_Ball:
                //�{�[���ꏊ�w��
                Array.Copy(new int[] { 1, 5, 6, 3, 9, 4, -1, 7, 8, -1, -1, -1, 2, -1, -1 }, ballNumbers,ballNumbers.Length);
                break;

            case GameType.Fifteen_Ball:
                //�{�[���ꏊ�w��
                Array.Copy(new int[] { 1, 7, 8, 11, 12, 13, 9, 14, 15, 10, 2, 4, 5, 6, 3 }, ballNumbers, ballNumbers.Length);
                break;
            case GameType.Eight_Ball:
                //�{�[�������_��
                int[] tmpNumbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };
                ArrayMethods.Shuffle(tmpNumbers);

                //8��^�񒆂�
                int eightPlace = Array.IndexOf(tmpNumbers, 8);
                ArrayMethods.Replace(tmpNumbers, eightPlace, 4);

                //15������O��
                int fifteenPlace = Array.IndexOf(tmpNumbers, 15);
                ArrayMethods.Replace(tmpNumbers, fifteenPlace, 10);

                //1���E��O��
                int onePlace = Array.IndexOf(tmpNumbers, 1);
                ArrayMethods.Replace(tmpNumbers, onePlace, 14);

                //�{�[���ꏊ�w��
                Array.Copy(tmpNumbers, ballNumbers, ballNumbers.Length);

                    break;
            case GameType.Snooker:
                //�{�[���ꏊ�w��
                Array.Copy(new int[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 }, ballNumbers, ballNumbers.Length);
                break;
        }

        //�{�[���ݒu
        //�ϐ�����
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

        //�����ݒu
        GenerateBall(0, new Vector3(-0.68f, 0f, 0f), Quaternion.Euler(90f, 90f, 0f));
        
        //�Q�[���J�n�i�^�[���������j
        startPlayerNum = UnityEngine.Random.Range(0, NetworkManager.ConnectedClientsIds.Count);
        NextTurn(); 
    }

    #region Next Turn
    //���̃^�[���ւ̈ڍs���T�[�o�[�Ɉ˗�����֐��֐�
    [ServerRpc(RequireOwnership = false)]
    private void NextTurnServerRpc()
    {
        NextTurn();
    }
    //���̃^�[���ֈڍs����֐��i�T�[�o�[�T�C�h�Ŏ��s�j
    public void NextTurn(int firstBallNum = 0)
    {
        //�{�[���������Ă��邩�`�F�b�N
        bool isFoul = false;
        bool sameClientTurn = false;
        //--------------------

        //���̃^�[���ɐݒ�
        if (sameClientTurn)
        {
            turnNum++;
        }
        whoseTurn = NetworkManager.ConnectedClientsIds[(startPlayerNum + turnNum) % NetworkManager.ConnectedClientsIds.Count];

        //�t�@�[���łȂ��Ƃ��͔��ʂ̈ʒu�ɃL���[�𐶐�
        if (!isFoul)
        {
            SetCue(whoseTurn);
        }

        //�ݒ�ύX
        ballList.Clear();

        //���̃v���C���[�Ɉȍ~
        NextTurnClientRpc(whoseTurn, isFoul);
    }

    //���̃^�[���ֈȍ~����N���A���g���̊֐�
    [ClientRpc]
    public void NextTurnClientRpc(ulong nextPlayerID, bool isFoul = false)
    {
        //�^�[����UI��ύX
        UIManager.Instance.SetNowPlayerUI(nextPlayerID);

        //�ݒ�
        bool myTurn = nextPlayerID == NetworkManager.Singleton.LocalClientId;
        BallController.myTurn = myTurn;
        NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>().SetMyTurn(myTurn, isFoul);
        if (myTurn)
        {
            StartCoroutine(MyTurnCoroutine());
        }
    } 
    #endregion

    private IEnumerator MyTurnCoroutine()
    {
        //�������u�����܂ő҂�
        while (PlayerController.settingMainBall)
        {
            yield return new WaitForSeconds(1f);
        }

        //�������܂ő҂�
        while(!PlayerController.finishedShot)
        {
            yield return new WaitForSeconds(1f);
        }
        PlayerController.finishedShot = false;
        yield return new WaitForSeconds(1f);

        //�S�Ă̋��̃I�u�W�F�N�g���擾
        List<Rigidbody> ballRigidbodies = new();
        foreach(var ball in GameObject.FindGameObjectsWithTag("Ball"))
        {
            ballRigidbodies.Add(ball.GetComponent<Rigidbody>());
        }
        ballRigidbodies.Add(GameObject.FindGameObjectWithTag("MainBall").GetComponent<Rigidbody>());

        //�S�Ď~�܂�܂ő҂�
        while (true)
        {
            float speed = 0f;
            foreach(var ballRigidbody in ballRigidbodies)
            {
                if(ballRigidbody != null)
                {
                    speed += ballRigidbody.velocity.magnitude;
                }
            }
            
            if(speed <= 0f)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
        }

        NextTurnServerRpc();
    }

    //�Q�[���I���֐��i�T�[�o�[�T�C�h�݂̂Ŏ��s�����j
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

    //�L���[�ݒu�֐�
    public void SetCue(ulong id)
    {
        GameObject mainBall = GameObject.FindGameObjectWithTag("MainBall");
        GameObject cue = Instantiate(cuePrefab);
        cue.transform.position = mainBall.transform.position;
        cue.transform.rotation = Quaternion.Euler(0f, Quaternion.LookRotation(-mainBall.transform.position).eulerAngles.y, 0f);
        cue.GetComponent<NetworkObject>().SpawnWithOwnership(id);
    }

    //�L���[�ݒu�T�[�o�[�T�C�h�֐�
    [ServerRpc(RequireOwnership = false)] public void SetCueServerRpc(ulong id)
    {
        SetCue(id);
    }

    //���[�h�ύX�R�[���o�b�N
    private void ModeCallback(bool _, bool next)
    {
        UIManager.Instance.SetModeButton(next);
    }


    #region Ball Generaion Methods
    //�{�[���ݒu�֐��T�[�o�[�Ɉ˗�
    [ServerRpc(RequireOwnership = false)]
    public void GenerateMainBallServerRpc(Vector3 pos)
    {
        GenerateBall(0, pos, Quaternion.identity);
    }
    //�{�[���ݒu�֐��i�T�[�o�[�T�C�h�݂̂Ŏ��s�����j
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

    //�{�[�������������Ɏ��s�����֐��i�T�[�o�[�T�C�h�݂̂Ŏ��s�����j
    public void CupIn(int ballNum)
    {
        ballList.Add(ballNum);
    }
    #endregion
}

/// <summary>
/// ����֗��z��N���X
/// </summary>
public static class ArrayMethods
{
    /// <summary>
    /// �z��V���b�t���֐�
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
    /// �z�����ւ��֐�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="array"></param>
    /// <param name="n"></param>
    /// <param name="m"></param>
    public static void Replace<T>(T[] array, int n, int m)
    {
        //�ԍ��������Ƃ��͍s��Ȃ�
        if(n == m)
        {
            return;
        }

        var tmp = array[n];
        array[n] = array[m];
        array[m] = tmp;
    }
}
