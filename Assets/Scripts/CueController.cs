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
        //オーナーのみ操作可能
        if (IsOwner)
        {
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

        //白玉に向ける
        transform.LookAt(mainBall.transform);
    }

    private void SetPosition()
    {
        pos.Value = transform.position;
    }
}
