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
                    if (selectedCustomers != null && selectedCustomers.Count > 0 && newLocation.isACustomerSeat && !newLocation.containsCustomer)
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

                        // move player to the table as well
                        StopAllCoroutines();
                        if (currentLocation != null) { currentLocation.containsPlayer = false; }
                        StartCoroutine(MoveCharacter(newLocation));

                        // clear selection
                        foreach (var c in selectedCustomers) if (c != null) c.Deselect();
                        selectedCustomers.Clear();
                    }
                    else
                    {
                        StopAllCoroutines();
                        if (currentLocation != null) { currentLocation.containsPlayer = false; }
                        StartCoroutine(MoveCharacter(newLocation));
                    }
                }

                KitchenFoodSlot kitchenFood = hit.collider.GetComponent<KitchenFoodSlot>();
                if (kitchenFood)
                {
                    print("gettiong food");
                    TryPickUpFood(kitchenFood);
                }

                Customer customer = hit.collider.GetComponent<Customer>();
                if (customer)
                {
                    StopAllCoroutines();
                    if (currentLocation != null) { currentLocation.containsPlayer = false; }

                    // If the customer is seated at an actual customer seat, move directly to the seat offset
                    if (customer.IsSeated() && customer.currentSeat != null)
                    {
                        Vector3 targetPos = (customer.currentSeat.seatOffset != null) ? customer.currentSeat.seatOffset.position : customer.currentSeat.transform.position;
                        StartCoroutine(MoveCharacter(targetPos, customer));
                        // don't change selection when clicking a seated customer
                        return;
                    }

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

                        // Move player to the clicked customer's position (the queue position)
                        StartCoroutine(MoveCharacter(customer.transform.position, customer));
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
            currentLocation = locationToMoveTo;
            currentLocation.containsPlayer = true;
            yield break;
        }

        StartCoroutine(MoveCharacter(locationToMoveTo));
    }

    public IEnumerator MoveCharacter(Vector3 spotToMoveTo, Customer customer)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, spotToMoveTo, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, spotToMoveTo) < 0.05f)
        {
            currentLocation = customer.currentSeat;
            currentLocation.containsPlayer = true;
            customer.OnInteract();
            yield break;
        }

        StartCoroutine(MoveCharacter(spotToMoveTo, customer));
    }


    void TryPickUpFood(KitchenFoodSlot kitchenFoodSlot)
    {
        if (currentLocation.isKitchenBar)
        {
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
}

