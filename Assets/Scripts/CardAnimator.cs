using UnityEngine;
using System.Collections;

public class CardAnimator : MonoBehaviour
{
    [Header("Draw Animation")]
    public float drawDuration = 0.5f;
    public Vector2 drawStartOffset = new Vector2(0, -300);
    
    [Header("Discard Animation")]
    public float discardDuration = 0.3f;
    public Vector2 discardTargetOffset = new Vector2(-500, -200);
    
    [Header("Hover Animation")]
    public float hoverHeight = 30f;
    public float hoverScale = 1.1f;
    public float hoverSpeed = 10f; // ← 더 빠르게
    
    private RectTransform rectTransform;
    private Vector2 layoutPosition;
    private Vector3 originalScale;
    private CanvasGroup canvasGroup;
    private bool isHovering = false;
    private bool isAnimating = false;
    private bool hasRecordedLayoutPosition = false;
    
    private Vector2 targetPosition; // ← 목표 위치 저장
    private Vector3 targetScale; // ← 목표 스케일 저장
    private bool hoverComplete = false; // ← 호버 애니메이션 완료 여부
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        originalScale = transform.localScale;
    }
    
    void LateUpdate()
	{
		// ← Layout 감지 부분 완전 삭제!
		// 대신 드로우 애니메이션에서 한 번만 저장
		
		// Layout이 적용된 후 위치 저장 (한 번만)
		if (!hasRecordedLayoutPosition && !isAnimating)
		{
			layoutPosition = rectTransform.anchoredPosition;
			hasRecordedLayoutPosition = true;
		}
		
		// 호버 애니메이션
		if (!isAnimating && hasRecordedLayoutPosition)
		{
			if (isHovering)
			{
				// 호버 시작 시 목표 위치 설정 (한 번만!)
				if (!hoverComplete)
				{
					targetPosition = layoutPosition + Vector2.up * hoverHeight;
					targetScale = originalScale * hoverScale;
				}
				
				// 목표 위치로 이동
				rectTransform.anchoredPosition = Vector2.Lerp(
					rectTransform.anchoredPosition,
					targetPosition,
					Time.deltaTime * hoverSpeed
				);
				
				transform.localScale = Vector3.Lerp(
					transform.localScale,
					targetScale,
					Time.deltaTime * hoverSpeed
				);
				
				// 목표에 거의 도달했으면 완료
				if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 0.1f)
				{
					hoverComplete = true;
					rectTransform.anchoredPosition = targetPosition;
					transform.localScale = targetScale;
				}
			}
			else
			{
				hoverComplete = false;
				
				// 원래 위치로 복귀
				rectTransform.anchoredPosition = Vector2.Lerp(
					rectTransform.anchoredPosition,
					layoutPosition,
					Time.deltaTime * hoverSpeed
				);
				
				transform.localScale = Vector3.Lerp(
					transform.localScale,
					originalScale,
					Time.deltaTime * hoverSpeed
				);
				
				// 원위치 도달 시 정확히 고정
				if (Vector2.Distance(rectTransform.anchoredPosition, layoutPosition) < 0.1f)
				{
					rectTransform.anchoredPosition = layoutPosition; // ← 정확히 원위치 고정!
					transform.localScale = originalScale;
				}
			}
		}
	}
    
    // 드로우 애니메이션
    public void PlayDrawAnimation()
    {
        StartCoroutine(DrawAnimation());
    }
    
    IEnumerator DrawAnimation()
    {
        isAnimating = true;
        
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Layout이 적용될 때까지 대기
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Layout 위치 저장
        layoutPosition = rectTransform.anchoredPosition;
        hasRecordedLayoutPosition = true;
        
        // 시작 위치/상태
        Vector2 startPos = layoutPosition + drawStartOffset;
        rectTransform.anchoredPosition = startPos;
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        
        float elapsed = 0f;
        
        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / drawDuration;
            
            float easeT = EaseOutBack(t);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, layoutPosition, easeT);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, easeT);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            
            yield return null;
        }
        
        // 최종 상태
        rectTransform.anchoredPosition = layoutPosition;
        transform.localScale = originalScale;
        canvasGroup.alpha = 1f;
        
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        isAnimating = false;
    }
    
    // 버리기 애니메이션
    public void PlayDiscardAnimation(System.Action onComplete = null)
    {
        StartCoroutine(DiscardAnimation(onComplete));
    }
    
    IEnumerator DiscardAnimation(System.Action onComplete)
    {
        isAnimating = true;
        
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 targetPos = startPos + discardTargetOffset;
        
        float elapsed = 0f;
        
        while (elapsed < discardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / discardDuration;
            
            float easeT = EaseInBack(t);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            transform.rotation = Quaternion.Euler(0, 0, t * 90f);
            
            yield return null;
        }
        
        onComplete?.Invoke();
    }
    
    public void OnHoverEnter()
    {
        if (!isAnimating)
        {
            isHovering = true;
            hoverComplete = false; // ← 호버 시작 시 리셋
        }
    }
    
    public void OnHoverExit()
    {
        isHovering = false;
		hoverComplete = false;
    }
    
    float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
    
    float EaseInBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return c3 * t * t * t - c1 * t * t;
    }
	
	// 외부에서 layoutPosition 리셋
	public void ResetLayoutPosition()
	{
		layoutPosition = rectTransform.anchoredPosition;
		hasRecordedLayoutPosition = true;
		hoverComplete = false;
	}
}