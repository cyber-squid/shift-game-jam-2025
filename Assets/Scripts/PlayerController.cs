using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    [SerializeField] public KitchenFoodSlot[] foodsThatCanBeCarried;
    Location currentLocation;

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
                    StopAllCoroutines();
                    if (currentLocation != null) { currentLocation.containsPlayer = false; }
                    StartCoroutine(MoveCharacter(newLocation));
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

                    // Safely determine move target: prefer customer's seat offset if available, fallback to seat transform, then to customer transform
                    Vector3 targetPos;
                    if (customer.currentSeat != null)
                    {
                        if (customer.currentSeat.seatOffset != null)
                            targetPos = customer.currentSeat.seatOffset.position;
                        else
                            targetPos = customer.currentSeat.transform.position;
                    }
                    else
                    {
                        targetPos = customer.transform.position;
                    }

                    StartCoroutine(MoveCharacter(targetPos, customer));
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

