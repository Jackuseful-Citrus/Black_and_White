using UnityEngine;

public class EnhancedMouseMove : MonoBehaviour
{
    private Transform backgroundTransform; 

    [Header("移动速度/强度")]
    [Range(0.01f, 5f)] 
    public float strengthX = 1.0f; 

    [Range(0.01f, 5f)]
    public float strengthY = 1.0f; 

    [Header("平滑度")]
    [Tooltip("移动的平滑系数 (值越小越平滑)")]
    public float smoothing = 4f; 
    private Vector3 initialPosition;
    private Vector3 targetPosition;


    void Start()
    {
        backgroundTransform = transform;
        initialPosition = backgroundTransform.position;
    }

    void Update()
    {
        float normalizedMouseX = Input.mousePosition.x / Screen.width;

        float centeredX = normalizedMouseX - 0.5f; 
        
        float normalizedMouseY = Input.mousePosition.y / Screen.height;

        float centeredY = normalizedMouseY - 0.5f; 

      
        float xOffset = centeredX * -strengthX;
        float yOffset = centeredY * -strengthY;
        targetPosition = new Vector3(
            initialPosition.x + xOffset, 
            initialPosition.y + yOffset, 
            initialPosition.z
        );     
        backgroundTransform.position = Vector3.Lerp(
            backgroundTransform.position, 
            targetPosition, 
            Time.deltaTime * smoothing
        );
    }
}