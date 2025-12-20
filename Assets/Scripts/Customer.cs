using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Customer : MonoBehaviour
{
    // literally just a state machine.
    // the states should be separate classes and not a bunch of if statements but it's a game jam, it's not that serious

    // must begin in a "waiting to be seated" state
    // if they are at the front of the queue, they are interactable and their patience ticks down
    // when interacted with, if there are empty seats they will move to an available one and change status to "waitforordertaken" 


    // "interaction" should be called automatically when the player moves to a customer's saved location. 

    CustomerState state;

    [SerializeField] float patienceLeft;
    float totalPatience = 20;

    float mealSelectionTime = 7f;
    float mealEatingTime = 15;



    public float moveSpeed = 0.1f;

    bool isInteractable = false;
    Location currentSeat;
    Food chosenFood;


    [SerializeField] GameObject thoughtBubble;
    [SerializeField] SpriteRenderer foodImage;
    [SerializeField] GameObject selectionIndicator;


    private void Awake()
    {
        patienceLeft = totalPatience;
        state = new CustomerState();
        state = CustomerState.WaitToBeSeated;
        isInteractable=true;
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
    }

    public bool IsSeated()
    {
        return currentSeat != null;
    }

    public void Select()
    {
        if (selectionIndicator != null) selectionIndicator.SetActive(true);
    }

    public void Deselect()
    {
        if (selectionIndicator != null) selectionIndicator.SetActive(false);
    }

    private void Update()
    {
        UpdateAction(state);

        if (patienceLeft <= 0)
        {
            if (currentSeat != null) {currentSeat.containsCustomer = false;}
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
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Location seat = Location.instance.CalculateFreeSeat();
                    patienceLeft += 999; 
                    
                    if (seat != null)
                    {
                        StartCoroutine(MoveCharacter(seat));
                    }
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (chosenFood != null)
                {
                    Kitchen.AddOrderToQueue(chosenFood);
                    UpdateState(CustomerState.WaitForFood);
                }
            }
        }

        if (state == CustomerState.WaitForFood)
        {
            patienceLeft -= Time.deltaTime;

            // if interacted with
            {
                // if you are holding the right food
                {
                    // food image should be copied onto the location in front of the customer. food should be deleted from the player's hand
                    UpdateState(CustomerState.EatFood);
                    if (thoughtBubble != null) thoughtBubble.SetActive(false);
                    isInteractable = false;
                }
            }
        }

        if (state == CustomerState.EatFood)
        {
            // just play food eating anim

            mealEatingTime -= Time.deltaTime;

            if(mealEatingTime <= 0)
            {
                if (currentSeat != null)
                {
                    currentSeat.containsCustomer = false;
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

}

enum CustomerState
{
    WaitToBeSeated,
    SelectMeal,
    WaitForOrderTaken,
    WaitForFood,
    EatFood
}