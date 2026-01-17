using UnityEngine;
using UnityEngine.UI;

public class NodeButtonTest : MonoBehaviour
{
    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => {
                Debug.Log("테스트 버튼 클릭됨!");
            });
            Debug.Log("테스트 리스너 추가 완료");
        }
    }
}