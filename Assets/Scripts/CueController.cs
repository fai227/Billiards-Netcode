using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CueController : NetworkBehaviour
{
    [Header("NetworkTransform")]
    private NetworkVariable<Vector3> pos = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Variables")]
    private PlayerController playerController;
    private GameObject mainBall;

    void Start()
    {
        mainBall = GameObject.FindGameObjectWithTag("MainBall");
        playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        SetPosition();
    }

    void Update()
    {
        //�I�[�i�[�̂ݑ���\
        if (IsOwner)
        {
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

        //���ʂɌ�����
        transform.LookAt(mainBall.transform);
    }

    private void SetPosition()
    {
        pos.Value = transform.position;
    }
}
