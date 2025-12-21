using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitchen : MonoBehaviour
{
    static Queue<Food> foodQueue;
    static Kitchen instance;

    [SerializeField] KitchenFoodSlot[] slots;

    float timeToNextFoodAppearing;
    [SerializeField] float foodTimer = 10;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        foodQueue = new Queue<Food>();
        timeToNextFoodAppearing = foodTimer;
    }

    // Update is called once per frame
    void Update()
    {
        // Guard against missing queue or empty queue or missing slots
        if (foodQueue == null || foodQueue.Count == 0 || slots == null || slots.Length == 0)
            return;

        timeToNextFoodAppearing -= Time.deltaTime;

        if (timeToNextFoodAppearing < 0)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null) continue;

                if (slot.storedFood == null && foodQueue.Count > 0)
                {
                    // Dequeue first to avoid race conditions and then assign safely
                    var next = foodQueue.Dequeue();
                    slot.storedFood = next;
                    if (slot.storedFoodSprite != null && next != null)
                    {
                        slot.storedFoodSprite.sprite = next.foodPicture;
                        slot.storedFoodSprite.enabled = true;
                        slot.storedFoodSprite.gameObject.SetActive(true);
                    }
                    timeToNextFoodAppearing = foodTimer;
                    break;
                }
            }
        }
    }

    public static void AddOrderToQueue(Food foodToAdd)
    {
        if (foodQueue == null) foodQueue = new Queue<Food>();
        if (foodToAdd == null) return;
        
        foodQueue.Enqueue(foodToAdd);
    }

    public static void ClearSlot(KitchenFoodSlot foodToRemove)
    {
        if (foodToRemove == null) return;
        if (foodToRemove.storedFoodSprite != null)
            foodToRemove.storedFoodSprite.sprite = null;
        foodToRemove.storedFood = null;
    }
}
