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
    #endregion

    #region Unity Methods
    private void Start()
    {
        ChangeBallMaterial();
        UpdateTransform();
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

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{transform.name}と{other.transform.name}に接触");
    }
    #endregion

    #region Methods
    //位置の設定関数（サーバー側）
    [ServerRpc]
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
    #endregion
}
