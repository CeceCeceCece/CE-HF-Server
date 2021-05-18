using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    //public GameObject playerPrefab;
    public GameObject fireballPrefab;

    public GameObject magePrefab;
    public GameObject warriorPrefab;
    public GameObject ninjaPrefab;
    public GameObject priestPrefab;
    public GameObject hunterPrefab;



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

    public RigidbodyPlayer InstantiateRigidbodyPlayer(int _class, out string classText)
    {
        GameObject prefabToBe;
        switch ((ClassCode)_class)
        {
            case ClassCode.Mage:
                prefabToBe = magePrefab;
                classText = ClassCode.Mage.ToString();
                break;
            case ClassCode.Warrior:
                prefabToBe = warriorPrefab;
                classText = ClassCode.Warrior.ToString();
                break;
            case ClassCode.Hunter:
                prefabToBe = hunterPrefab;
                classText = ClassCode.Hunter.ToString();
                break;
            case ClassCode.Priest:
                prefabToBe = priestPrefab;
                classText = ClassCode.Priest.ToString();
                break;
            case ClassCode.Ninja:
                prefabToBe = ninjaPrefab;
                classText = ClassCode.Ninja.ToString();
                break;
            default:
                Console.WriteLine("Invalid class value!");
                prefabToBe = magePrefab;
                classText = ClassCode.Mage.ToString();
                break;
        }
       
        return Instantiate(prefabToBe, new Vector3(0f, 25f, 0f), Quaternion.identity).GetComponent<RigidbodyPlayer>();
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

    public void BlastWaveCasted(Vector3 position)
    {
        Debug.Log("Blast Wave casted!");
        ServerSend.BlastWaveCasted(position);
    }

    public void IceBlockCasted(int playerID)
    {
        Debug.Log($"Ice Block Casted by {playerID}!");
        ServerSend.IceBlockCasted(playerID);
    }

    public void IceBlockEnded(int playerID)
    {
        Debug.Log($"Ice Block Ended on {playerID}!");
        ServerSend.IceBlockEnded(playerID);
    }
}