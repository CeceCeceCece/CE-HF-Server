using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public GameObject playerPrefab;
    public GameObject fireballPrefab;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 100;

        Server.Start(50, 26950);
    }
    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    /*public Player InstantiatePlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 25f, 0f), Quaternion.identity).GetComponent<Player>();
    }*/

    public RigidbodyPlayer InstantiateRigidbodyPlayer()
    {
        return Instantiate(playerPrefab, new Vector3(0f, 25f, 0f), Quaternion.identity).GetComponent<RigidbodyPlayer>();
    }

    public Fireball FireballInit(Transform _shootOrigin)
    {
        Debug.Log("Fireball shot!");
        return Instantiate(fireballPrefab, _shootOrigin.position + _shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<Fireball>();
    }

    public BasicAttackProjectile BasicAttackInit(Transform _shootOrigin, GameObject basicAttackPrefab)
    {
        Debug.Log("Fireball shot!");
        return Instantiate(basicAttackPrefab, _shootOrigin.position + _shootOrigin.forward * 0.7f, Quaternion.identity).GetComponent<BasicAttackProjectile>();
    }
}