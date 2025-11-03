using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 인벤토리 아이템을 마우스로 끌고 다니는 기능만 담당하는 클래스
/// </summary>
public class InventoryDrag_Drop
{
    // UI 전체 화면
    private VisualElement root;
    // 마우스 따라다니는 아이템 이미지
    private VisualElement dragIcon;

    // 드래그 중인지 아닌지 알려주는 변수
    public bool isDragging { get; private set; } = false;
    // 드래그 중인 아이템의 정보
    public WeaponPrefabData currentItem;
    // 아이템을 끌기 시작한 슬롯 번호
    public int fromSlotIndex;

    /// <summary>
    /// 클래스를 만들자마자 하는 일들을 모아둔 생성자 함수
    /// </summary>
    /// <param name="root"></param>
    public InventoryDrag_Drop(VisualElement root)
    {
        this.root = root;

        dragIcon = new VisualElement();
        dragIcon.style.position = Position.Absolute;
        dragIcon.style.width = 120;
        dragIcon.style.height = 142;
        dragIcon.style.visibility = Visibility.Hidden;

        root.Add(dragIcon);
        root.RegisterCallback<MouseMoveEvent>(OnMouseMove);
    }

    /// <summary>
    /// 마우스를 움직일 때마다 실행되는 함수
    /// </summary>
    /// <param name="evt"></param>
    void OnMouseMove(MouseMoveEvent evt)
    {
        if (isDragging)
        {
            // 아이콘이 마우스 포인터 중심 근처에 오도록 위치 조정
            dragIcon.style.left = evt.mousePosition.x - 32;
            dragIcon.style.top = evt.mousePosition.y - 32;
        }
    }

    /// <summary>
    /// 드래그 시작할 때 호출하는 함수
    /// </summary>
    /// <param name="item"></param>
    /// <param name="fromIndex"></param>
    public void BeginDrag(WeaponPrefabData data, int fromIndex)
    {
        if (data == null) return;
        Debug.Log("드래그 시작됨");
        isDragging = true;
        currentItem = data;
        fromSlotIndex = fromIndex;
        dragIcon.style.backgroundImage = new StyleBackground(data.icon);
        dragIcon.style.visibility = Visibility.Visible;
    }

    /// <summary>
    /// 드래그 종료 시 호출하는 함수
    /// </summary>
    public void EndDrag()
    {
        Debug.Log("드래그 종료됨");
        isDragging = false;
        dragIcon.style.visibility = Visibility.Hidden;
        currentItem = null;
    }
}
