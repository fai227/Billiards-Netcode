using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CueController : NetworkBehaviour
{
    [Header("NetworkTransform")]
    private NetworkVariable<Vector3> pos = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Variables")]
    private Rigidbody rig;
    private PlayerController playerController;
    private GameObject mainBall;
    private static float mouseSensitivity = 2f;

    void Start()
    {
        mainBall = GameObject.FindGameObjectWithTag("MainBall");
        rig = GetComponent<Rigidbody>();
        playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        SetPosition();
    }

    void Update()
    {
        //�I�[�i�[�̂ݑ���\
        if (IsOwner)
        {
            //�}�E�X�ړ����ړ��ɂ���
            rig.velocity = transform.forward * -Input.GetAxis("Mouse Y") * mouseSensitivity;
            //����
            if (Input.GetMouseButtonDown(0))
            {
                //playerController.
            }
        }
        

        //����
        if (IsOwner)
        {
            SetPosition();
        }
        else
        {
            transform.position = pos.Value;
        }

        if (!Input.GetMouseButton(0))
        {
            //���ʂɌ�����
            transform.LookAt(mainBall.transform);
        }
    }

    private void SetPosition()
    {
        pos.Value = transform.position;
    }
}
