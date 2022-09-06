using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    #region Variables
    //自分の番かどうか
    public static bool myTurn;

    [Header("Network Transform")]
    public NetworkVariable<Vector3> networkPos;
    public NetworkVariable<Quaternion> networkRot;

    [Header("Number of Ball")]
    public NetworkVariable<int> ballNumber;

    [Header("Ball Sprite")]
    [SerializeField] private Texture[] ballTextures;

    [Header("Audio")]
    private AudioSource audioSource;
    [SerializeField] private AudioClip ballHitClip;
    [SerializeField] private AudioClip pocketInClip;
    #endregion

    #region Unity Methods
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ChangeBallMaterial();
        UpdateTransform();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        GameManager.Instance.audioSource.PlayOneShot(pocketInClip);
    }

    private void Update()
    {
        //自分のターンじゃないときは位置を更新
        if (!myTurn)
        {
            UpdateTransform();
        }
    }

    private void FixedUpdate()
    {
        //自分のターンの時は位置を送る
        if (myTurn)
        {
            SynchronizeTransformServerRpc(transform.position, transform.rotation);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.CompareTag("Ball") || collision.transform.CompareTag("MainBall"))
        {
            audioSource.PlayOneShot(ballHitClip);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!myTurn)
        {
            return;
        }

        if (other.transform.CompareTag("Pocket"))
        {
            CupInServerRpc();
        }
    }
    #endregion

    #region Methods
    //位置の設定関数（サーバー側）
    [ServerRpc(RequireOwnership = false)]
    private void SynchronizeTransformServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPos.Value = pos;
        networkRot.Value = rot;
    }

    //ボールの見た目変更関数
    private void ChangeBallMaterial()
    {
        if (ballNumber.Value < 0)
        {
            Debug.Log("ボールが初期化されていません");
            return;
        }
        GetComponent<MeshRenderer>().material.mainTexture = ballTextures[ballNumber.Value];

        //白のボールは別設定に
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

    [ServerRpc(RequireOwnership = false)] private void CupInServerRpc()
    {
        GetComponent<NetworkObject>()?.Despawn();
        GameManager.Instance.CupIn(ballNumber.Value);
    }
    #endregion
}
