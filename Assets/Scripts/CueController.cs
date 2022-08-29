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
        //オーナーのみ操作可能
        if (IsOwner)
        {
            //マウス移動を移動にする
            rig.velocity = transform.forward * -Input.GetAxis("Mouse Y") * mouseSensitivity;
            //操作
            if (Input.GetMouseButtonDown(0))
            {
                //playerController.
            }
        }
        

        //同期
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
            //白玉に向ける
            transform.LookAt(mainBall.transform);
        }
    }

    private void SetPosition()
    {
        pos.Value = transform.position;
    }
}
