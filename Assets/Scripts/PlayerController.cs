using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    #region Variables
    //プレイヤー名
    public NetworkVariable<FixedString32Bytes> playerName = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //準備用
    public NetworkVariable<bool> isReady;

    //スコア
    public NetworkVariable<int> score;

    #endregion

    #region Unity Methods
    //コールバックセット＆プレイヤー名セット
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //名前のコールバック
        playerName.OnValueChanged += SetPlayerName;

        //スコアのコールバック
        score.OnValueChanged += ScoreCallback;

        if (IsOwner)
        {
            //準備完了のコールバック
            isReady.OnValueChanged += UIManager.Instance.SetReadyUI;

            //名前を反映
            playerName.Value = UIManager.Instance.GetPlayerName();
        }
    }

    //コールバック解除
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //名前のコールバック
        playerName.OnValueChanged -= SetPlayerName;
        UIManager.Instance.SetPlayerUI(OwnerClientId, false);   //プレイヤーの名前を消す

        //スコアのコールバック
        score.OnValueChanged -= ScoreCallback;

        if (IsOwner)
        {
            //準備完了のコールバック
            isReady.OnValueChanged -= UIManager.Instance.SetReadyUI;
        }
    }
    #endregion

    #region Methods
    //プレイヤーの名前をセット
    private void SetPlayerName(FixedString32Bytes prev, FixedString32Bytes next)
    {
        gameObject.name = next.ToString();
        UIManager.Instance.SetPlayerUI(OwnerClientId, true, playerName.Value.ToString());    //プレイヤーの名前表示
    }

    //準備状態をサーバーに変更依頼
    [ServerRpc(RequireOwnership = false)] public void SetReadyStateServerRpc(bool flag)
    {
        //準備状態をセット
        isReady.Value = flag;
        
        //開始を判定
        if(flag)
        {
            GameManager.Instance.Ready();
        }
    }

    //スコアセットコールバック
    private void ScoreCallback(int _, int next)
    {
        UIManager.Instance.SetScore(OwnerClientId, next);
    }
    #endregion
}
