using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerInputActions PlayerInputActions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerInputActions = new PlayerInputActions();
        PlayerInputActions.Enable();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            PlayerInputActions?.Disable();
        }
    }
}