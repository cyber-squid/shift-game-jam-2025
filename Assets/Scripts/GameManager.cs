using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] Food[] allFoods;
    public Location exitLocation;
    [Tooltip("Optional explicit exit point transform for customers who leave early")]
    public Transform exitPoint;
    public Location startingLocation;

    private void Awake()
    {
        Instance = this;
    }
    public Food SelectFood()
    {
        return allFoods[Random.Range(0, allFoods.Length)];
    }
}
