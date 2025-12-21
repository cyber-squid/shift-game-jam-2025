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

    // Expose whether this Location is a customer seat so other systems can query it safely
    public bool IsCustomerSeat()
    {
        return isACustomerSeat;
    }

    public Transform foodPlateOffset;
    [Tooltip("Optional approach point used when player moves to the kitchen bar")]
    public Transform kitchenApproachPoint;

    [Tooltip("Optional explicit seat transform for the left seat (used when seating pairs)")]
    public Transform leftSeatOffset;
    [Tooltip("Optional explicit seat transform for the right seat (used when seating pairs)")]
    public Transform rightSeatOffset;

    public Vector3 GetGroupedSeatPosition(int index, int total)
    {
        // Single customer: they sit at the LEFT seat (preferred) and reserve the table
        if (total <= 1)
        {
            if (leftSeatOffset != null) return leftSeatOffset.position;
            return this.transform.position;
        }

        // Pair seating: prefer explicit left/right offsets when both exist
        if (total == 2)
        {
            if (leftSeatOffset != null && rightSeatOffset != null)
            {
                return (index == 0) ? leftSeatOffset.position : rightSeatOffset.position;
            }

            // if only left is provided, place the second seat to the right of left by a small gap
            if (leftSeatOffset != null && rightSeatOffset == null)
            {
                float pairGap = 0.5f;
                return (index == 0) ? leftSeatOffset.position : leftSeatOffset.position + new Vector3(pairGap, 0f, 0f);
            }

            // if only right is provided, place the first seat to the left of right by a small gap
            if (rightSeatOffset != null && leftSeatOffset == null)
            {
                float pairGap = 0.5f;
                return (index == 0) ? rightSeatOffset.position + new Vector3(-pairGap, 0f, 0f) : rightSeatOffset.position;
            }

            // fallback: symmetric around the location transform
            float fallbackGap = 0.5f;
            return (index == 0) ? this.transform.position + new Vector3(-fallbackGap / 2f, 0f, 0f) : this.transform.position + new Vector3(fallbackGap / 2f, 0f, 0f);
        }

        // default fallback
        return this.transform.position;
    }

    private void Awake()
    {
        if (allSeatingLocations == null) { allSeatingLocations = new List<Location>(); }

        if (isACustomerSeat)
        {
            allSeatingLocations.Add(this);
        }

        if (isStaticInstance) {instance = this;}

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
            return freeSeats[Random.Range(0, freeSeats.Count)];
        }
        else return null;
    }
}
