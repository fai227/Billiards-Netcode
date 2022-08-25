using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PlayerController : NetworkBehaviour
{
    #region Variables
    //�v���C���[��
    public NetworkVariable<FixedString32Bytes> playerName = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //�����p
    public NetworkVariable<bool> isReady;

    //�X�R�A
    public NetworkVariable<int> score;

    #endregion

    #region Unity Methods
    //�R�[���o�b�N�Z�b�g���v���C���[���Z�b�g
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        //���O�̃R�[���o�b�N
        playerName.OnValueChanged += SetPlayerName;

        if (IsOwner)
        {
            //���������̃R�[���o�b�N
            isReady.OnValueChanged += UIManager.Instance.SetReadyUI;

            //���O�𔽉f
            playerName.Value = UIManager.Instance.GetPlayerName();
        }
    }

    //�R�[���o�b�N����
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        //���O�̃R�[���o�b�N
        playerName.OnValueChanged -= SetPlayerName;

        if(IsOwner)
        {
            //���������̃R�[���o�b�N
            isReady.OnValueChanged -= UIManager.Instance.SetReadyUI;
        }
    }
    #endregion

    #region Methods
    //�v���C���[�̖��O���Z�b�g
    private void SetPlayerName(FixedString32Bytes prev, FixedString32Bytes next)
    {
        gameObject.name = next.ToString();
    }

    //������Ԃ��T�[�o�[�ɕύX�˗�
    [ServerRpc(RequireOwnership = false)] public void SetReadyStateServerRpc(bool flag)
    {
        //������Ԃ��Z�b�g
        isReady.Value = flag;
        
        //�J�n�𔻒�
        if(flag)
        {
            GameManager.Instance.Ready();
        }
    }
    #endregion
}
