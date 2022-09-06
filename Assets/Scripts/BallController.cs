using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallController : NetworkBehaviour
{
    #region Variables
    //�����̔Ԃ��ǂ���
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
        //�����̃^�[������Ȃ��Ƃ��͈ʒu���X�V
        if (!myTurn)
        {
            UpdateTransform();
        }
    }

    private void FixedUpdate()
    {
        //�����̃^�[���̎��͈ʒu�𑗂�
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
    //�ʒu�̐ݒ�֐��i�T�[�o�[���j
    [ServerRpc(RequireOwnership = false)]
    private void SynchronizeTransformServerRpc(Vector3 pos, Quaternion rot)
    {
        networkPos.Value = pos;
        networkRot.Value = rot;
    }

    //�{�[���̌����ڕύX�֐�
    private void ChangeBallMaterial()
    {
        if (ballNumber.Value < 0)
        {
            Debug.Log("�{�[��������������Ă��܂���");
            return;
        }
        GetComponent<MeshRenderer>().material.mainTexture = ballTextures[ballNumber.Value];

        //���̃{�[���͕ʐݒ��
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
