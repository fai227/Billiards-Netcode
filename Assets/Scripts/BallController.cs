using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    //�����̔Ԃ��ǂ���
    public static bool myTurn;

    [Header("Network Transform")]
    public NetworkVariable<Vector3> networkPos;
    public NetworkVariable<Quaternion> networkRot;

    [Header("Number of Ball")]
    public NetworkVariable<int> ballNumber;

    [Header("Ball Sprite")]
    [SerializeField] private Texture[] ballTextures;

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

    //�ʒu�̐ݒ�֐��i�T�[�o�[���j
    [ServerRpc] private void SynchronizeTransformServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPos.Value = pos;
        networkRot.Value = rot;
    }

    //�{�[���̌����ڕύX�֐�
    private void ChangeBallMaterial()
    {
        if(ballNumber.Value < 0)
        {
            Debug.Log("�{�[��������������Ă��܂���");
            return;
        }
        GetComponent<MeshRenderer>().material.mainTexture = ballTextures[ballNumber.Value];
    }

    private void UpdateTransform()
    {
        transform.position = networkPos.Value;
        transform.rotation = networkRot.Value;
    }
}
