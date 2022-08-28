using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CueController : NetworkBehaviour
{
    PlayerController playerController;

    void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerController.
        }
    }
}
