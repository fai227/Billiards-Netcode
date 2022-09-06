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
    //�R�[���o�b�N�Z�b�g���v���C���[���Z�b�g
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //���O�̃R�[���o�b�N
        playerName.OnValueChanged += SetPlayerName;

        //�X�R�A�̃R�[���o�b�N
        score.OnValueChanged += ScoreCallback;

        if (IsOwner)
        {
            //���������̃R�[���o�b�N
            isReady.OnValueChanged += UIManager.Instance.SetReadyUI;

            //���O�𔽉f
            playerName.Value = UIManager.Instance.GetPlayerName();

            //�Q�[���̎�ނƃ��[�h��ݒ�
            UIManager.Instance.SetGameTypeUI(GameManager.GameType.Fifteen_Ball, GameManager.Instance.gameType.Value);
            UIManager.Instance.SetModeButton(GameManager.Instance.hardMode.Value);
        }
    }

    //�R�[���o�b�N����
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //���O�̃R�[���o�b�N
        playerName.OnValueChanged -= SetPlayerName;
        UIManager.Instance.SetPlayerUI(OwnerClientId, false);   //�v���C���[�̖��O������

        //�X�R�A�̃R�[���o�b�N
        score.OnValueChanged -= ScoreCallback;

        if (IsOwner)
        {
            //���������̃R�[���o�b�N
            isReady.OnValueChanged -= UIManager.Instance.SetReadyUI;
        }
    }

    private void Start()
    {
        //�����ݒ�
        ballCameraBase = GameObject.Find("BallCameraBase");
        ballCamera = GameObject.Find("BallCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        //�J�����ړ�
        if (IsOwner)
        {
            //�J�����𓮂������Ƃ��o����Ƃ��͓�����
            if (canMoveCamera)
            {
                if (!Input.GetMouseButton(0))
                {
                    Vector3 prev = ballCameraBase.transform.rotation.eulerAngles;
                    ballCameraBase.transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
                }
            }
        }

        //���C���{�[����ݒu����ꏊ��\������
        if (settingMainBall)
        {


            //�������u���ꂽ�Ƃ�
            if (Input.GetMouseButtonDown(0))
            {
                settingMainBall = false;
                SetCameraPosition(true);
            }
        }
    }
    #endregion

    #region Methods
    //�v���C���[�̖��O���Z�b�g
    private void SetPlayerName(FixedString32Bytes prev, FixedString32Bytes next)
    {
        gameObject.name = next.ToString();
        UIManager.Instance.SetPlayerUI(OwnerClientId, true, playerName.Value.ToString());    //�v���C���[�̖��O�\��
    }

    //������Ԃ��T�[�o�[�ɕύX�˗�
    [ServerRpc(RequireOwnership = false)] public void SetReadyStateServerRpc(bool flag)
    {
        //������Ԃ��Z�b�g
        isReady.Value = flag;
        
        //�J�n�𔻒�
        if(flag)
        {
            GameManager.Instance.Ready();
        }
    }

    //�X�R�A�Z�b�g�R�[���o�b�N
    private void ScoreCallback(int _, int next)
    {
        UIManager.Instance.SetScore(OwnerClientId, next);
    }

    //�����̃^�[�������Z�b�g����
    public void SetMyTurn(bool flag, bool isFoul)
    {
        //���g�̃^�[���ŁA�t�@�[���łȂ���΃J������ǔ����[�h�ɐݒ�
        SetCameraPosition(flag && !isFoul);

        //�����𐶐�����ꍇ
        if(flag && isFoul)
        {
            settingMainBall = true;
        }
    }

    //�J�����̈ʒu��ݒ肷��֐�
    public void SetCameraPosition(bool follow)
    {
        //�J�����𓮂����Ȃ�����
        canMoveCamera = false;

        //���ʂ�ǂ��Ƃ�
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
        //�J���������Z�b�g����Ƃ�
        else
        {
            ballCameraBase.transform.DOLocalMove(Vector3.zero, cameraModeDuration);
            ballCameraBase.transform.DOLocalRotateQuaternion(Quaternion.identity, cameraModeDuration);
            ballCamera.transform.DOLocalMove(cameraFarPosition, cameraModeDuration);
            float angle = Mathf.Atan(-cameraFarPosition.y / cameraFarPosition.z) * Mathf.Rad2Deg;
            ballCamera.transform.DOLocalRotate(new Vector3(angle, 0f, 0f), cameraModeDuration);
        }

        //�J���������Ԃ������ē�������悤�ɂ���
        DOVirtual.DelayedCall(cameraModeDuration, () =>
        {
            canMoveCamera = true;

            //�L���[�𓮂�����悤�ɂ���
            if (follow)
            {
                GameObject.FindGameObjectWithTag("Cue").GetComponent<CueController>().canMove = true;
            }
        }, false);
    }
    #endregion
}
