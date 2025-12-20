using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Location : MonoBehaviour
{
    // need to keep track of whether a customer is at them or not. 
    // if a player enters, this 
    static List<Location> allSeatingLocations;

    bool containsCustomer;
    bool containsPlayer;

    public bool isACustomerSeat;

    void SetContainState(Customer customer, bool doesContainACustomer)
    {

    }

    public void MovePlayerCharacter()
    {
        StartCoroutine(PlayerController.instance.MoveCharacter(this));
    }
}
