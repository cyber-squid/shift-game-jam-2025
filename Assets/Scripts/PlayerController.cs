using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    Food[] foodsThatCanBeCarried = new Food[2];
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
                    currentLocation.containsPlayer = false;
                    StartCoroutine(MoveCharacter(newLocation));
                }

                KitchenFoodSlot kitchenFood = new KitchenFoodSlot();
                if (kitchenFood)
                {
                    TryPickUpFood(kitchenFood);
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


    void TryPickUpFood(KitchenFoodSlot kitchenFood)
    {
        if (currentLocation.isKitchenBar)
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

