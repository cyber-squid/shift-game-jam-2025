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
    public Customer pairedCustomer;
    Food chosenFood;

    [SerializeField] GameObject selectionIndicator;

    public void Select()
    {
        // Tint the primary sprite renderer(s) yellow to indicate selection
        if (_spriteRenderers == null) return;
        foreach (var sr in _spriteRenderers)
        {
            if (sr == null || sr == _thoughtBubbleRenderer || sr == foodImage) continue;
            sr.color = Color.yellow;
        }
    }

    public void Deselect()
    {
        // Reset sprite renderer color back to white
        if (_spriteRenderers == null) return;
        foreach (var sr in _spriteRenderers)
        {
            if (sr == null || sr == _thoughtBubbleRenderer || sr == foodImage) continue;
            sr.color = Color.white;
        }
    }

    public bool IsSeated()
    {
        // Customer is only considered seated if they're at an actual customer seat (not the waiting area)
        if (currentSeat == null) return false;
        if (!currentSeat.IsCustomerSeat()) return false;
        
        // Exclude the starting location (waiting area)
        if (GameManager.Instance != null && currentSeat == GameManager.Instance.startingLocation)
            return false;
            
        return true;
    }

    // Flip or reset the customer's sprite(s) horizontally when they occupy the right seat
    public void SetFacingRightSeat(bool isRight)
    {
        if (_spriteRenderers == null) return;
        foreach (var sr in _spriteRenderers)
        {
            if (sr == null) continue;
            // don't flip UI or thought bubble sprites
            if (sr == _thoughtBubbleRenderer || sr == foodImage) continue;
            sr.flipX = isRight;
        }
        
        // Position thought bubble: left-seated keeps original position, right-seated uses offset
        if (thoughtBubble != null)
        {
            if (isRight && thoughtBubbleOffset != null)
            {
                thoughtBubble.transform.position = thoughtBubbleOffset.position;
            }
            // else: left-seated customer keeps the thoughtBubble at its original position (no change)
        }
    }

    public void HideThoughtBubble()
    {
        if (thoughtBubble != null && thoughtBubble.activeSelf)
        {
            thoughtBubble.SetActive(false);
        }
    }


    [SerializeField] GameObject thoughtBubble;
    [SerializeField] Transform thoughtBubbleOffset;
    [SerializeField] SpriteRenderer foodImage;

    // cache sprite renderers so we can flip only the visible sprites (safer than scaling)
    SpriteRenderer[] _spriteRenderers;
    SpriteRenderer _thoughtBubbleRenderer;

    private void Awake()
    {
        patienceLeft = totalPatience;
        state = CustomerState.WaitToBeSeated;
        isInteractable = true;

        // cache sprite renderers and the thought bubble sprite renderer (if present)
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        _thoughtBubbleRenderer = (thoughtBubble != null) ? thoughtBubble.GetComponent<SpriteRenderer>() : null;

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

            // Move to an editable exit point if provided by GameManager, otherwise use exit Location
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.exitPoint != null)
                {
                    StartCoroutine(MoveCharacter(GameManager.Instance.exitPoint.position));
                }
                else if (GameManager.Instance.exitLocation != null)
                {
                    StartCoroutine(MoveCharacter(GameManager.Instance.exitLocation));
                }
            }

            // leave the restaurant angry. add loss points and clear customer data from the location
        }    }


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

                                HideThoughtBubble();
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
                Debug.Log($"SUCCESS! Customer finished eating and is leaving satisfied!");
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

            // Ensure a single customer arriving via this path faces the left seat (not flipped)
            SetFacingRightSeat(false);

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

    // Move to a specific point (used for group seating offsets)
    public IEnumerator MoveCharacter(Location locationToMoveTo, Vector3 targetPosition)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, targetPosition, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, targetPosition) < 0.05f)
        {
            currentSeat = locationToMoveTo;
            if (currentSeat != null) currentSeat.containsCustomer = true;

            // Determine whether this arrived position is left or right and flip accordingly
            bool isRight = false;
            Vector3 leftPos = (locationToMoveTo.leftSeatOffset != null) ? locationToMoveTo.leftSeatOffset.position : locationToMoveTo.transform.position + new Vector3(-0.25f, 0f, 0f);
            Vector3 rightPos = (locationToMoveTo.rightSeatOffset != null) ? locationToMoveTo.rightSeatOffset.position : locationToMoveTo.transform.position + new Vector3(0.25f, 0f, 0f);
            if (Vector3.Distance(this.transform.position, rightPos) < Vector3.Distance(this.transform.position, leftPos))
                isRight = true;

            SetFacingRightSeat(isRight);

            if (state == CustomerState.WaitToBeSeated)
            {
                UpdateState(CustomerState.SelectMeal);
                isInteractable = false;
            }

            yield break;
        }

        StartCoroutine(MoveCharacter(locationToMoveTo, targetPosition));
    }

    // Move to an arbitrary position (used for exits and special points)
    public IEnumerator MoveCharacter(Vector3 targetPosition)
    {
        this.transform.position = Vector2.MoveTowards(this.transform.position, targetPosition, moveSpeed / 3);
        yield return null;

        if (Vector2.Distance(this.transform.position, targetPosition) < 0.05f)
        {
            // arrived at arbitrary target; clear seat association
            if (currentSeat != null)
            {
                currentSeat.containsCustomer = false;
                currentSeat = null;
            }
            yield break;
        }

        StartCoroutine(MoveCharacter(targetPosition));
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