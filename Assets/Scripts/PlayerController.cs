using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    Food[] foodsThatCanBeCarried = new Food[2];
    Location currentLocation;
    Customer selectedCustomer;

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
            if (Camera.main == null) return;
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                print("object: " + hit.collider.name);
                Location newLocation = hit.collider.GetComponent<Location>();
                if (newLocation != null)
                {

                    StopAllCoroutines();
                    if (currentLocation != null)
                    {
                        currentLocation.containsPlayer = false;
                    }
                    StartCoroutine(MoveCharacter(newLocation));

                    // If a customer is selected, seat them at the clicked free seat and move the player there too
                    if (selectedCustomer != null && !newLocation.containsCustomer && !selectedCustomer.IsSeated())
                    {
                        // move customer
                        selectedCustomer.StopAllCoroutines();
                        selectedCustomer.StartCoroutine(selectedCustomer.MoveCharacter(newLocation));
                        // move player to same location
                        StopAllCoroutines();
                        if (currentLocation != null)
                        {
                            currentLocation.containsPlayer = false;
                        }
                        StartCoroutine(MoveCharacter(newLocation));
                        // clear selection and hide highlight
                        selectedCustomer.Deselect();
                        selectedCustomer = null;
                    }
                    else
                    {
                        StopAllCoroutines();
                        if (currentLocation != null)
                        {
                            currentLocation.containsPlayer = false;
                        }
                        StartCoroutine(MoveCharacter(newLocation));
                    }
                }

                KitchenFoodSlot kitchenFood = hit.collider.GetComponent<KitchenFoodSlot>();
                if (kitchenFood != null)
                {
                    TryPickUpFood(kitchenFood);
                }

                Customer clickedCustomer = hit.collider.GetComponent<Customer>();
                if (clickedCustomer != null)
                {
                    // deselect previous selection
                    if (selectedCustomer != null && selectedCustomer != clickedCustomer)
                    {
                        selectedCustomer.Deselect();
                        selectedCustomer = null;
                    }

                    // If customer is already seated, don't select them for seating; just move player to them
                    if (clickedCustomer.IsSeated())
                    {
                        StopAllCoroutines();
                        if (currentLocation != null)
                        {
                            currentLocation.containsPlayer = false;
                        }
                        StartCoroutine(MoveCharacter(clickedCustomer.transform.position));
                    }
                    else
                    {
                        // select the customer so the next location click will seat them
                        selectedCustomer = clickedCustomer;
                        selectedCustomer.Select();
                        StopAllCoroutines();
                        if (currentLocation != null)
                        {
                            currentLocation.containsPlayer = false;
                        }
                        StartCoroutine(MoveCharacter(clickedCustomer.transform.position));
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

    public IEnumerator MoveCharacter(Vector3 positionToMoveTo)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, positionToMoveTo, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, positionToMoveTo) < 0.05f)
        {
            // not a Location-based move, so clear currentLocation
            currentLocation = null;
            yield break;
        }

        StartCoroutine(MoveCharacter(positionToMoveTo));
    }


    void TryPickUpFood(KitchenFoodSlot kitchenFood)
    {
        if (currentLocation != null && currentLocation.isKitchenBar)
        {
            for (int i = 0; i < foodsThatCanBeCarried.Length; i++)
            {
                if (foodsThatCanBeCarried[i] == null)
                {
                    foodsThatCanBeCarried[i] = kitchenFood.storedFood;
                    kitchenFood.storedFood = null;
                }
            }
        }
    }
}

