using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    Food[] foodsThatCanBeCarried = new Food[2];

    [SerializeField] float moveSpeed = 1f;

    private void Start()
    {
        ///sefsgfgsfg
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
                    StartCoroutine(MoveCharacter(newLocation));
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

            yield break;
        }

        StartCoroutine(MoveCharacter(locationToMoveTo));
    }
}

