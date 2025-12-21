using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] public KitchenFoodSlot[] foodsThatCanBeCarried;
    Location currentLocation;

    // selection can include groups (pairs) of customers
    List<Customer> selectedCustomers = new List<Customer>();

    [SerializeField] float moveSpeed = 1f;

    private void Start()
    {
        instance = this;
        
        // Set player to always render in front
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 100;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            print("mouse down");
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                print("object: " + hit.collider.name);
                Location newLocation = hit.collider.GetComponent<Location>();
                if (newLocation)
                {
                    // If customers are selected for group seating, seat them at the clicked free seat
                    if (selectedCustomers != null && selectedCustomers.Count > 0 && newLocation.IsCustomerSeat() && !newLocation.containsCustomer)
                    {
                        // mark location as containing customers immediately to avoid races
                        newLocation.containsCustomer = true;

                        // move each selected customer to their grouped offset
                        for (int i = 0; i < selectedCustomers.Count; i++)
                        {
                            var cust = selectedCustomers[i];
                            if (cust == null) continue;

                            // compute grouped offset for each customer
                            Vector3 seatPos = newLocation.GetGroupedSeatPosition(i, selectedCustomers.Count);

                            cust.StopAllCoroutines();
                            cust.StartCoroutine(cust.MoveCharacter(newLocation, seatPos));
                        }

                        // clear selection
                        foreach (var c in selectedCustomers) if (c != null) c.Deselect();
                        selectedCustomers.Clear();
                        
                        // Don't move player when seating customers
                        return;
                    }
                    
                    // When clicking a table with customers, trigger interaction
                    if (newLocation.containsCustomer)
                    {
                        Customer[] allCustomers = FindObjectsOfType<Customer>();
                        foreach (var cust in allCustomers)
                        {
                            if (cust != null && cust.currentSeat == newLocation)
                            {
                                cust.OnInteract(); // Take the order
                            }
                        }
                        
                        // Move player to the table
                        StopAllCoroutines();
                        if (currentLocation != null) { currentLocation.containsPlayer = false; }
                        StartCoroutine(MoveCharacter(newLocation));
                        return;
                    }

                    // Only move player to empty tables if no other action was taken
                    StopAllCoroutines();
                    if (currentLocation != null) { currentLocation.containsPlayer = false; }
                    StartCoroutine(MoveCharacter(newLocation));
                }

                KitchenFoodSlot kitchenFood = hit.collider.GetComponent<KitchenFoodSlot>();
                if (kitchenFood)
                {
                    // Find a parent Location (the kitchen bar) and move to its approach point if set
                    Location kitchenLoc = kitchenFood.GetComponentInParent<Location>();
                    if (kitchenLoc != null && kitchenLoc.isKitchenBar)
                    {
                        Vector3 approach = (kitchenLoc.kitchenApproachPoint != null) ? kitchenLoc.kitchenApproachPoint.position : kitchenLoc.transform.position;
                        StartCoroutine(MoveToPositionAndTryPickup(approach, kitchenFood));
                    }
                    else
                    {
                        // fallback: move to the clicked object's position
                        StartCoroutine(MoveToPositionAndTryPickup(kitchenFood.transform.position, kitchenFood));
                    }
                }

                Customer customer = hit.collider.GetComponent<Customer>();
                if (customer)
                {
                    // SELECT A GROUP: deselect previous
                    foreach (var c in selectedCustomers) if (c != null) c.Deselect();
                    selectedCustomers.Clear();

                    // Add clicked customer (if not already seated)
                    if (!customer.IsSeated())
                    {
                        selectedCustomers.Add(customer);

                        // If they have a paired sibling that's not seated, add them too
                        if (customer.pairedCustomer != null && !customer.pairedCustomer.IsSeated())
                        {
                            selectedCustomers.Add(customer.pairedCustomer);
                        }

                        // Show selection indicators
                        foreach (var c in selectedCustomers) if (c != null) c.Select();
                    }
                }
            }
        }
    }


    public IEnumerator MoveCharacter(Location locationToMoveTo)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, locationToMoveTo.transform.position, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, locationToMoveTo.transform.position) < 0.05f)
        {
            if (currentLocation != null) currentLocation.containsPlayer = false;
            currentLocation = locationToMoveTo;
            currentLocation.containsPlayer = true;
            yield break;
        }

        StartCoroutine(MoveCharacter(locationToMoveTo));
    }

    public IEnumerator MoveCharacter(Vector3 spotToMoveTo, Customer customer, bool triggerOnArrival = true)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, spotToMoveTo, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, spotToMoveTo) < 0.05f)
        {
            if (currentLocation != null) currentLocation.containsPlayer = false;
            currentLocation = customer.currentSeat;
            if (currentLocation != null) currentLocation.containsPlayer = true;
            if (triggerOnArrival)
            {
                customer.OnInteract();
            }
            yield break;
        }

        StartCoroutine(MoveCharacter(spotToMoveTo, customer, triggerOnArrival));
    }

    // Move to a location position and then try to pick up the given kitchen slot
    public IEnumerator MoveToAndPickUp(Location kitchenLocation, KitchenFoodSlot slot)
    {
        if (kitchenLocation == null || slot == null) yield break;

        Vector3 approach = (kitchenLocation.kitchenApproachPoint != null) ? kitchenLocation.kitchenApproachPoint.position : kitchenLocation.transform.position;

        // Move to approach
        while (Vector2.Distance(this.transform.position, approach) > 0.05f)
        {
            this.transform.position = Vector2.MoveTowards(this.transform.position, approach, moveSpeed / 3);
            yield return null;
        }

        // set current location to the kitchen so TryPickUpFood will succeed
        currentLocation = kitchenLocation;
        currentLocation.containsPlayer = true;

        TryPickUpFood(slot);
    }

    public IEnumerator MoveToPositionAndTryPickup(Vector3 position, KitchenFoodSlot slot)
    {
        if (slot == null) yield break;

        while (Vector2.Distance(this.transform.position, position) > 0.05f)
        {
            this.transform.position = Vector2.MoveTowards(this.transform.position, position, moveSpeed / 3);
            yield return null;
        }

        // Set current location to the kitchen so TryPickUpFood will work
        Location kitchenLoc = slot.GetComponentInParent<Location>();
        if (kitchenLoc != null && kitchenLoc.isKitchenBar)
        {
            if (currentLocation != null) currentLocation.containsPlayer = false;
            currentLocation = kitchenLoc;
            currentLocation.containsPlayer = true;
        }

        TryPickUpFood(slot);
    }

    void TryPickUpFood(KitchenFoodSlot kitchenFoodSlot)
    {
        if (currentLocation == null || !currentLocation.isKitchenBar)
            return;

        if (kitchenFoodSlot.storedFood != null)
        {
            for (int i = 0; i < foodsThatCanBeCarried.Length; i++)
            {
                if (foodsThatCanBeCarried[i].storedFood == null)
                {
                    print("got foods");
                    foodsThatCanBeCarried[i].storedFood = kitchenFoodSlot.storedFood;
                    foodsThatCanBeCarried[i].storedFoodSprite.sprite = foodsThatCanBeCarried[i].storedFood.foodPicture;

                    Kitchen.ClearSlot(kitchenFoodSlot);
                    break;
                }
            }
        }
    }
}

