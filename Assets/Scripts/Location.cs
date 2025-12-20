using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Location : MonoBehaviour
{
    // need to keep track of whether a customer is at them or not. 
    // if a player enters, this 
    public static Location instance; 
    public static List<Location> allSeatingLocations;

    public bool containsCustomer;
    public bool containsPlayer;

    [SerializeField] bool isACustomerSeat;
    public bool isKitchenBar;
    [SerializeField] bool isStaticInstance;

    public Transform seatOffset;  // used for player moving to customers 
    public Transform foodPlateOffset;

    private void Awake()
    {
        if (allSeatingLocations == null) { allSeatingLocations = new List<Location>(); }

        if (isACustomerSeat)
        {
            allSeatingLocations.Add(this);
        }

        if (isStaticInstance) {instance = this;}

        // Ensure offsets have safe fallbacks so missing inspector assignments don't crash runtime
        if (seatOffset == null)
        {
            // fallback to the location's own transform
            seatOffset = this.transform;
        }

        if (foodPlateOffset == null)
        {
            // create a child transform to act as a food plate offset (local zero)
            GameObject plate = new GameObject("FoodPlateOffset");
            plate.transform.SetParent(this.transform, false);
            plate.transform.localPosition = Vector3.zero;
            foodPlateOffset = plate.transform;
        }

    }

    void SetContainState(Customer customer, bool doesContainACustomer)
    {

    }

    public void MovePlayerCharacter()
    {
        StartCoroutine(PlayerController.instance.MoveCharacter(this));
    }

    public Location CalculateFreeSeat()
    {
        List<Location> freeSeats = new List<Location>();

        for (int i = 0; i < allSeatingLocations.Count; i++)
        {
            if (allSeatingLocations[i].containsCustomer == false)
            {
                freeSeats.Add(allSeatingLocations[i]);
            }
        }

        if (freeSeats.Count > 0)
        {
            return freeSeats[Random.Range(0, freeSeats.Count - 1)];
        }
        else return null;
    }
}
