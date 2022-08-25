using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    //�����̔Ԃ��ǂ���
    public static bool myTurn;

    //�l�b�g���[�N��œ�������g�����X�t�H�[��
    public NetworkVariable<Vector3> networkPos;
    public NetworkVariable<Quaternion> networkRot;

    private void Update()
    {
        transform.position = networkPos.Value;
        transform.rotation = networkRot.Value;
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
}
