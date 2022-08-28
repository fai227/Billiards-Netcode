using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    #region Variables
    //�����̔Ԃ��ǂ���
    public static bool myTurn;

    [Header("Network Transform")]
    public NetworkVariable<Vector3> networkPos;
    public NetworkVariable<Quaternion> networkRot;

    [Header("Number of Ball")]
    public NetworkVariable<int> ballNumber;

    [Header("Ball Sprite")]
    [SerializeField] private Texture[] ballTextures;
    #endregion

    #region Unity Methods
    private void Start()
    {
        ChangeBallMaterial();
        UpdateTransform();
    }

    private void Update()
    {
        //�����̃^�[������Ȃ��Ƃ��͈ʒu���X�V
        if (!myTurn)
        {
            UpdateTransform();
        }
    }

    private void FixedUpdate()
    {
        //�����̃^�[���̎��͈ʒu�𑗂�
        if (myTurn)
        {
            SynchronizeTransformServerRpc(transform.position, transform.rotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{transform.name}��{other.transform.name}�ɐڐG");
    }
    #endregion

    #region Methods
    //�ʒu�̐ݒ�֐��i�T�[�o�[���j
    [ServerRpc]
    private void SynchronizeTransformServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPos.Value = pos;
        networkRot.Value = rot;
    }

    //�{�[���̌����ڕύX�֐�
    private void ChangeBallMaterial()
    {
        if (ballNumber.Value < 0)
        {
            Debug.Log("�{�[��������������Ă��܂���");
            return;
        }
        GetComponent<MeshRenderer>().material.mainTexture = ballTextures[ballNumber.Value];

        //���̃{�[���͕ʐݒ��
        if(ballNumber.Value == 0)
        {
            string mainBall = "MainBall";
            gameObject.tag = mainBall;
            gameObject.layer = LayerMask.NameToLayer(mainBall);
        }
    }

    private void UpdateTransform()
    {
        transform.position = networkPos.Value;
        transform.rotation = networkRot.Value;
    } 
    #endregion
}
