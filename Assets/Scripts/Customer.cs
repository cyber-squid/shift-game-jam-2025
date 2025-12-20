using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Customer : MonoBehaviour
{
    // literally just a state machine.
    // the states should be separate classes and not a bunch of if statements but it's a game jam, it's not that seriosu

    // must begin in a "waiting to be seated" state
    // if they are at the front of the queue, they are interactable and their patience ticks down
    // when interacted with, if there are empty seats they will move to an available one and change status to "waitforordertaken" 


    // "interaction" should be called automatically when the player moves to a customer's saved location. 

    CustomerState state;

    [SerializeField] float patienceLeft;
    float totalPatience = 20;

    float mealSelectionTime = 7f;
    float mealEatingTime = 15;



    float moveSpeed = 0.1f;

    bool isInteractable = false;
    bool wasInteractedWith = false;
    public Location currentSeat;
    Food chosenFood;


    [SerializeField] GameObject thoughtBubble;
    [SerializeField] SpriteRenderer foodImage;


    private void Awake()
    {
        patienceLeft = totalPatience;
        state = CustomerState.WaitToBeSeated;
        isInteractable = true;

        // Try to set a starting seat if GameManager provides one; guard against missing GameManager
        if (GameManager.Instance != null && GameManager.Instance.startingLocation != null)
        {
            currentSeat = GameManager.Instance.startingLocation;
            GameManager.Instance.startingLocation.containsCustomer = true;
        }
        else
        {
            currentSeat = null;
        }
    }

    private void Update()
    {
        UpdateAction(state);

        if (patienceLeft <= 0)
        {
            if (currentSeat != null) { currentSeat.containsCustomer = false; }
            StartCoroutine(MoveCharacter(GameManager.Instance.exitLocation));
            // leave the restaurant angry. add loss points and clear customer data from the location
        }
    }


    private void UpdateAction(CustomerState state)
    {
        if (state == CustomerState.WaitToBeSeated)
        {
            if (isInteractable)
            {
                patienceLeft -= Time.deltaTime;

                // if interacted with
                if (wasInteractedWith)
                {
                    Location seat = null;
                    if (Location.instance != null)
                        seat = Location.instance.CalculateFreeSeat();

                    if (seat != null)
                    {
                        isInteractable = false;
                        patienceLeft += 999;
                        if (currentSeat != null)
                        {
                            currentSeat.containsCustomer = false;
                        }
                        StartCoroutine(MoveCharacter(seat));
                    }

                    wasInteractedWith = false;
                }
            }
        }



        if (state == CustomerState.SelectMeal)
        {
            mealSelectionTime -= Time.deltaTime;

            if (mealSelectionTime <= 0)
            {
                if (GameManager.Instance != null)
                {
                    chosenFood = GameManager.Instance.SelectFood();
                }

                if (chosenFood != null && foodImage != null)
                {
                    foodImage.sprite = chosenFood.foodPicture;
                }

                if (thoughtBubble != null)
                {
                    thoughtBubble.SetActive(true);
                }

                UpdateState(CustomerState.WaitForOrderTaken);
                isInteractable = true;
            }
        }



        if (state == CustomerState.WaitForOrderTaken)
        {
            patienceLeft -= Time.deltaTime;

            // if interacted with
            if (wasInteractedWith)
            {
                if (chosenFood != null)
                {
                    Kitchen.AddOrderToQueue(chosenFood);
                    UpdateState(CustomerState.WaitForFood);
                }

                wasInteractedWith = false;
            }
        }



        if (state == CustomerState.WaitForFood)
        {
            patienceLeft -= Time.deltaTime;

            if (wasInteractedWith)
            {
                if (PlayerController.instance != null && PlayerController.instance.foodsThatCanBeCarried != null)
                {
                    for (int i = 0; i < PlayerController.instance.foodsThatCanBeCarried.Length; i++)
                    {
                        var slot = PlayerController.instance.foodsThatCanBeCarried[i];
                        if (slot != null && slot.storedFood != null && chosenFood != null)
                        {
                            if (slot.storedFood == chosenFood)
                            {
                                // food image should be copied onto the location in front of the customer. food should be deleted from the player's hand
                                if (thoughtBubble != null)
                                    thoughtBubble.SetActive(true);

                                var sr = thoughtBubble != null ? thoughtBubble.GetComponent<SpriteRenderer>() : null;
                                if (sr != null) sr.sprite = null;

                                if (currentSeat != null && currentSeat.foodPlateOffset != null && foodImage != null)
                                {
                                    foodImage.transform.position = currentSeat.foodPlateOffset.position;
                                }

                                if (slot.storedFoodSprite != null)
                                    slot.storedFoodSprite.sprite = null;

                                slot.storedFood = null;

                                UpdateState(CustomerState.EatFood);
                                isInteractable = false;

                                break;
                            }
                        }
                    }
                }

                wasInteractedWith = false;
            }
        }



        if (state == CustomerState.EatFood)
        {
            // just play food eating anim.

            mealEatingTime -= Time.deltaTime;

            if (mealEatingTime <= 0)
            {
                if (thoughtBubble != null) thoughtBubble.SetActive(false);
                if (currentSeat != null) currentSeat.containsCustomer = false;
                if (GameManager.Instance != null && GameManager.Instance.exitLocation != null)
                {
                    StartCoroutine(MoveCharacter(GameManager.Instance.exitLocation));
                }
                // leave the restaurant happy. clear customer data from the location
            }
        }
    }


    void UpdateState(CustomerState newState)
    {
        state = newState;

        if (state == CustomerState.WaitForFood)
            patienceLeft = totalPatience * 2;
        else
            patienceLeft = totalPatience;
    }


    public IEnumerator MoveCharacter(Location locationToMoveTo)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, locationToMoveTo.transform.position, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, locationToMoveTo.transform.position) < 0.05f)
        {
            currentSeat = locationToMoveTo;
            currentSeat.containsCustomer = true;

            if (state == CustomerState.WaitToBeSeated)
            {
                // after finishing move, register the seat as their new interaction location
                UpdateState(CustomerState.SelectMeal);
                isInteractable = false;
            }

            yield break;
        }

        StartCoroutine(MoveCharacter(locationToMoveTo));
    }


    public void OnInteract()
    {
        switch (state)
        {
            case CustomerState.WaitToBeSeated:
                {
                    if (isInteractable) { wasInteractedWith = true; }
                }
                break;
            case CustomerState.WaitForOrderTaken:
                {
                    wasInteractedWith = true;
                }
                break;
            case CustomerState.WaitForFood:
                {
                    wasInteractedWith = true;
                }
                break;
            default:
                break;
        }

    }
}

enum CustomerState
{
    WaitToBeSeated,
    SelectMeal,
    WaitForOrderTaken,
    WaitForFood,
    EatFood
}