using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    //自分の番かどうか
    public static bool myTurn;

    //ネットワーク上で同期するトランスフォーム
    public NetworkVariable<Vector3> networkPos;
    public NetworkVariable<Quaternion> networkRot;

    private void Update()
    {
        transform.position = networkPos.Value;
        transform.rotation = networkRot.Value;
    }

    private void FixedUpdate()
    {
        //自分のターンの時は位置を送る
        if (myTurn)
        {
            SynchronizeTransformServerRpc(transform.position, transform.rotation);
        }
    }

    //位置の設定関数（サーバー側）
    [ServerRpc] private void SynchronizeTransformServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPos.Value = pos;
        networkRot.Value = rot;
    }
}
