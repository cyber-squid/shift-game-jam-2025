using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] Food[] allFoods;
    public Location exitLocation;
    [Tooltip("Optional explicit exit point transform for customers who leave early")]
    public Transform exitPoint;
    public Location startingLocation;

    int currentPoints;
    [SerializeField] TextMeshProUGUI pointsText;

    float gameTimer;
    [SerializeField] float startingTimeLeft;
    [SerializeField] TextMeshProUGUI timeText;

    [SerializeField] GameObject endScreen;
    [SerializeField] TextMeshProUGUI endText;

    float timescale;

    private void Awake()
    {
        Instance = this;
        gameTimer = startingTimeLeft;
    }

    private void Update()
    {
        gameTimer -= Time.deltaTime;
        timeText.text = ((int)gameTimer).ToString();

        if (gameTimer < 0) 
        {
            timescale = Time.timeScale;
            Time.timeScale = 0;
            endScreen.SetActive(true);
            endText.text = "Total points: " + currentPoints.ToString();
        }
    }

    public Food SelectFood()
    {
        return allFoods[Random.Range(0, allFoods.Length)];
    }

    public void AdjustPoints(int pointsToGive)
    {
        currentPoints += pointsToGive;
        pointsText.text = currentPoints.ToString();
    }

    public void Restart()
    {
        Time.timeScale = timescale;
        SceneManager.LoadScene("MainGame");
    }
}
