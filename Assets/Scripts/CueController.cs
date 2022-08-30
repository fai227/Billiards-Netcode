using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using DG.Tweening;

public class CueController : NetworkBehaviour
{
    [Header("NetworkTransform")]
    private NetworkVariable<Vector3> pos = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> rot = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    [Header("Component")]
    private Rigidbody rig;
    private PlayerController playerController;

    [Header("Variables")]
    private GameObject mainBall;
    private Vector3 originPosition;
    public bool canMove;
    private bool isFixed;

    [Header("Constants")]
    private static float mouseSensitivity = 1f;
    private static float fadeDuration = 0.5f;
    private static float maxSpeed = 10f;

    void Start()
    {
        mainBall = GameObject.FindGameObjectWithTag("MainBall");
        rig = GetComponent<Rigidbody>();
        playerController = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerController>();

        //使用者じゃなければ当たり判定を消す
        if (!IsOwner)
        {
            GetComponentInChildren<BoxCollider>().isTrigger = true;
        }
    }

    void Update()
    {
        //オーナーのみ操作可能
        if (IsOwner)
        {
            //操作可能の時操作可能
            if (canMove)
            {
                //操作
                if (Input.GetMouseButtonDown(0))
                {
                    originPosition = transform.position;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    transform.position = originPosition;
                }

                //押しているときだけ移動
                if (Input.GetMouseButton(0))
                {
                    rig.velocity = transform.forward * Input.GetAxis("Mouse Y") * mouseSensitivity;
                }
                else
                {
                    rig.velocity = Vector3.zero;

                    //回転
                    transform.Rotate(0f, Input.GetAxis("Mouse X"), 0f);
                }
            }

            pos.Value = transform.position;
            rot.Value = (int)transform.rotation.eulerAngles.y;
        }
        else
        {
            transform.position = pos.Value;
            transform.rotation = Quaternion.Euler(0f, rot.Value, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("MainBall"))
        {
            //フェードアウトを実行、サーバーに依頼
            FadeOut();  FadeOutServerRpc();
        }
    }

    #region Fade Out Methods
    [ServerRpc]
    private void FadeOutServerRpc()
    {
        FadeOutClientRpc();
    }

    [ClientRpc]
    private void FadeOutClientRpc()
    {
        if (!IsOwner)
        {
            FadeOut();
        }
    }

    private void FadeOut()
    {
        canMove = false;

        foreach(var material in GetComponentInChildren<Renderer>().materials)
        {
            DOVirtual.Float(1f, 0f, fadeDuration, value =>
            {
                material.color = new Color(material.color.r, material.color.g, material.color.b, value);
            }).OnComplete(() =>
            {
                DestroyServerRpc();
            });
        }

        DOVirtual.DelayedCall(fadeDuration / 2f, () =>
        {
            GetComponentInChildren<BoxCollider>().isTrigger = true;
        }, false);
    }

    [ServerRpc] private void DestroyServerRpc() => GetComponent<NetworkObject>().Despawn();
    #endregion
}
