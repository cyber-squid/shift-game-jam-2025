using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kitchen : MonoBehaviour
{
    static Queue<Food> foodQueue;

    [SerializeField] KitchenFoodSlot[] slots;

    float timeToNextFoodAppearing;
    [SerializeField] float foodTimer = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (foodQueue.Count > 0)
        {
            timeToNextFoodAppearing -= Time.deltaTime;

            if (timeToNextFoodAppearing < 0)
            {
                for (int i = 0; i < slots.Length; i++) 
                {
                    if (slots[i].storedFood == null) 
                    { 
                        slots[i].storedFood = foodQueue.Dequeue();
                        timeToNextFoodAppearing = foodTimer;
                    }
                }
            }
        }
    }

    public static void AddOrderToQueue(Food foodToAdd)
    {
        foodQueue.Enqueue(foodToAdd);
    }
}
