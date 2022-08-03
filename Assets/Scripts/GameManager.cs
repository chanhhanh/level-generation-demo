using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject player;
    #region Singleton
    public static GameManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
    }
  
    #endregion
    public void SpawnPlayer(Vector3 pos)
    {
        Instantiate(player, pos, Quaternion.identity);
    }
}
