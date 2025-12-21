using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Food", menuName = "ScriptableObjects/Food")]
public class Food : ScriptableObject
{
    int foodID;
    public Sprite foodPicture;
}
