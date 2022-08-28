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
        //�J�������w��ʒu�Ɉړ�
        ballCamera.transform.LookAt(ballCameraBase.transform);
        float magnitude = ballCamera.transform.position.magnitude;
        if(magnitude != cameraDistance)
        {
            ballCamera.transform.position = ballCamera.transform.position / magnitude * cameraDistance;
        }

        //�J�����𓮂������Ƃ��o����Ƃ��͓�����
        if (canMoveCamera)
        {
            Debug.Log("�J�����𓮂����܂�");
            ballCameraBase.transform.Rotate(Input.GetAxis("MouseY"), -Input.GetAxis("Mouse X"), 0f);
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
        if(flag && !isFoul)
        {
            canMoveCamera = false;
            ballCameraBase.transform.DOMove(GameObject.FindGameObjectWithTag("MainBall").transform.position, cameraModeDuration);
            //�J�����̋��������X�ɋ߂Â��A�Ō�ɃJ�����𓮂�����悤�ɂ���
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

    //�J������߂��֐�
    public void SetCameraFollowBall()
    {
        canMoveCamera = false;
        //ballCameraBase.transform.DOMove(Vector3.zero, cameraModeDuration);
        //ballCamera.transform.DOMove(ballCameraOriginPosition, cameraModeDuration);
    }

    //�J�����̈ʒu��ݒ肷��֐�
    public void SetCameraPosition(bool reset = false)
    {
        //�J�������Z�b�g�֐�
        if (reset)
        {

        }
    }
    #endregion
}
