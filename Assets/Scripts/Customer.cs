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

    float patienceLeft;
    float totalPatience = 10;

    float mealSelectionTime = 2.5f;
    float mealEatingTime = 5;

    bool isInteractable = false;

    private void Awake()
    {
        patienceLeft = totalPatience;
    }

    private void Update()
    {
        UpdateAction(state);

        if (patienceLeft <= 0)
        {
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
                {
                    //calculate free seats

                    // if there are seats free
                    {
                        // select a free seat and move to it
                        // after finishing move, register the seat as their new interaction location
                        // update status to selectmeal
                        isInteractable = false;
                    }
                }
            }
        }

        if (state == CustomerState.SelectMeal)
        {
            mealSelectionTime -= Time.deltaTime;

            if (mealSelectionTime <= 0) 
            {
                // pick random food and set "thought bubble" for the customer active, and set the food image in the bubble to the chosen random food.
                UpdateState(CustomerState.WaitForOrderTaken);
                isInteractable = true;
            }
        }

        if (state == CustomerState.WaitForOrderTaken)
        {
            patienceLeft -= Time.deltaTime;

            // if interacted with
            {
                // set kitchen to spawn the corresponding food item
                //update status to waitforfood
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
                    //update status to eatfood
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

}

enum CustomerState
{
    WaitToBeSeated,
    SelectMeal,
    WaitForOrderTaken,
    WaitForFood,
    EatFood
}