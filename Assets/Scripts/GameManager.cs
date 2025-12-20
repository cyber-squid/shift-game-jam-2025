using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] Food[] allFoods;
    public Location exitLocation;
    public Location startingLocation;

    private void Awake()
    {
        Instance = this;
    }
    public Food SelectFood()
    {
        return allFoods[Random.Range(0, allFoods.Length - 1)];
    }
}
