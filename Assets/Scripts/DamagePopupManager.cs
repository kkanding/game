using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;
    
    [Header("Prefab")]
    public GameObject damagePopupPrefab;
    
    [Header("Canvas")]
    public Canvas canvas;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    // 데미지 팝업 생성
    public void ShowDamage(int damage, Vector3 worldPosition)
    {
        if (damagePopupPrefab == null || canvas == null)
        {
            Debug.LogWarning("DamagePopup Prefab 또는 Canvas가 없습니다!");
            return;
        }
        
        // 팝업 생성
        GameObject popupObj = Instantiate(damagePopupPrefab, canvas.transform);
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        
        if (popup != null)
        {
            popup.Setup(damage, worldPosition);
        }
    }
    
    // 힐 팝업 생성
    public void ShowHeal(int heal, Vector3 worldPosition)
    {
        if (damagePopupPrefab == null || canvas == null)
        {
            Debug.LogWarning("DamagePopup Prefab 또는 Canvas가 없습니다!");
            return;
        }
        
        GameObject popupObj = Instantiate(damagePopupPrefab, canvas.transform);
        DamagePopup popup = popupObj.GetComponent<DamagePopup>();
        
        if (popup != null)
        {
            popup.SetupHeal(heal, worldPosition);
        }
    }
}