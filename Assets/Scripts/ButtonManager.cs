using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public enum ButtonType { Play, Credits, Exit, Back }
    
    public ButtonType buttonType;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Color hoverColor = Color.yellow;
    
    [Header("Credits/Back Button Settings")]
    public GameObject creditsPanel;
    public GameObject playButton;
    public GameObject creditsButton;
    public GameObject sign;
    public GameObject exitButton;
    public GameObject backButton;
    
    [Header("Background Sprites")]
    public GameObject startBackground;
    public Sprite menuSprite;
    public Sprite creditsSprite;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnMouseDown()
    {
        switch (buttonType)
        {
            case ButtonType.Play:
                SceneManager.LoadScene("MainGame");
                break;
                
            case ButtonType.Credits:
                if (creditsPanel != null) creditsPanel.SetActive(true);
                if (playButton != null) playButton.SetActive(false);
                if (creditsButton != null) creditsButton.SetActive(false);
                if (sign != null) sign.SetActive(false);
                if (exitButton != null) exitButton.SetActive(false);
                if (backButton != null)
                {
                    backButton.SetActive(true);
                    var sr = backButton.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.white;
                }
                if (startBackground != null && creditsSprite != null)
                {
                    var bgSprite = startBackground.GetComponent<SpriteRenderer>();
                    if (bgSprite != null) bgSprite.sprite = creditsSprite;
                }
                break;
                
            case ButtonType.Exit:
                Application.Quit();
                break;
                
            case ButtonType.Back:
                if (creditsPanel != null) creditsPanel.SetActive(false);
                if (playButton != null) playButton.SetActive(true);
                if (creditsButton != null)
                {
                    creditsButton.SetActive(true);
                    var sr = creditsButton.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = Color.white;
                }
                if (sign != null) sign.SetActive(true);
                if (exitButton != null) exitButton.SetActive(true);
                if (backButton != null) backButton.SetActive(false);
                if (startBackground != null && menuSprite != null)
                {
                    var bgSprite = startBackground.GetComponent<SpriteRenderer>();
                    if (bgSprite != null) bgSprite.sprite = menuSprite;
                }
                break;
        }
    }
}
