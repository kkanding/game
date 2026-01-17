using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [Header("References")]
    public Slider healthSlider;
    
    [Header("Settings")]
    public float smoothSpeed = 5f; // 부드러운 전환 속도
    
    private float targetValue;
    
    void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }
    
    void Start()
    {
        if (healthSlider != null)
        {
            targetValue = healthSlider.value;
        }
    }
    
    void Update()
    {
        // 부드럽게 감소
        if (healthSlider != null && Mathf.Abs(healthSlider.value - targetValue) > 0.001f)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * smoothSpeed);
        }
    }
    
    // 체력바 설정 (0~1 비율)
    public void SetHealth(float ratio)
    {
        targetValue = Mathf.Clamp01(ratio);
    }
    
    // 즉시 설정 (애니메이션 없이)
    public void SetHealthImmediate(float ratio)
    {
        targetValue = Mathf.Clamp01(ratio);
        if (healthSlider != null)
        {
            healthSlider.value = targetValue;
        }
    }
}