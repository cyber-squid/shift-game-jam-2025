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

    public Transform seatOffset;  // used for player moving to customers 
    public Transform foodPlateOffset;

    [Tooltip("Vertical spacing (in world units) between grouped seats")]
    public float seatPairSpacing = 0.6f;

    public Vector3 GetGroupedSeatPosition(int index, int total)
    {
        Vector3 basePos = (seatOffset != null) ? seatOffset.position : this.transform.position;
        if (total <= 1) return basePos;
        float start = -((total - 1) * seatPairSpacing) / 2f;
        float offset = start + index * seatPairSpacing;
        return basePos + new Vector3(0f, offset, 0f);
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
