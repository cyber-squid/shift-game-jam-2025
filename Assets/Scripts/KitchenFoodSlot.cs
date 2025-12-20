using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple container representing a food slot at the kitchen/bar
public class KitchenFoodSlot : MonoBehaviour
{
    [Tooltip("The Food currently stored in this kitchen slot (can be null)")]
    public Food storedFood;
}
