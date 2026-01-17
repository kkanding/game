using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 50f;
    public float fadeSpeed = 2f;
    public float lifetime = 1f;
    
    private TextMeshProUGUI textMesh;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float timer = 0f;
    
    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }
    
    void Update()
    {
        // 위로 이동
        rectTransform.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
        
        // 페이드 아웃
        timer += Time.deltaTime;
        canvasGroup.alpha = 1f - (timer / lifetime);
        
        // 수명 다하면 삭제
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
    
    // 데미지 설정
	public void Setup(int damage, Vector3 worldPosition)
	{
		if (textMesh != null)
		{
			textMesh.text = "-" + damage.ToString();
		}
		
		// 이미 스크린 좌표라면 그대로 사용, 아니면 변환
		Vector2 screenPos;
		
		// Canvas 내부 좌표인지 확인 (x, y가 작으면 월드 좌표)
		if (Mathf.Abs(worldPosition.x) < 10 && Mathf.Abs(worldPosition.y) < 10)
		{
			// 월드 좌표 → 스크린 좌표 변환
			screenPos = Camera.main.WorldToScreenPoint(worldPosition);
		}
		else
		{
			// 이미 스크린/Canvas 좌표
			screenPos = worldPosition;
		}
		
		rectTransform.position = screenPos;
	}
    
    // 힐 설정 (초록색)
    public void SetupHeal(int heal, Vector3 worldPosition)
    {
        if (textMesh != null)
        {
            textMesh.text = "+" + heal.ToString();
            textMesh.color = new Color(0.3f, 1f, 0.3f); // 초록색
        }
        
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        rectTransform.position = screenPos;
    }
}