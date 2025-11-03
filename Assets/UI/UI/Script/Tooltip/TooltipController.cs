//using UnityEngine;
//using UnityEngine.UIElements;

//public class TooltipController
//{
//    private VisualElement tooltipRoot;
//    private Label tooltipName;
//    private Label gradeType;
//    private Label level;
//    private Label description;
//    private Label tooltipReqStat;
//    private Label tooltipAtk;
//    private Label price;

//    public void Initialize(VisualElement root, VisualTreeAsset tooltipUXML)
//    {
//        tooltipRoot = tooltipUXML.CloneTree();
//        tooltipRoot.style.display = DisplayStyle.None;
//        tooltipRoot.style.position = Position.Absolute;

//        tooltipName = tooltipRoot.Q<Label>("Name");
//        gradeType = tooltipRoot.Q<Label>("GradeType");
//        level = tooltipRoot.Q<Label>("Level");
//        description = tooltipRoot.Q<Label>("description");
//        tooltipReqStat = tooltipRoot.Q<Label>("reqStat");
//        tooltipAtk = tooltipRoot.Q<Label>("Atk");
//        price = tooltipRoot.Q<Label>("price");

//        root.Add(tooltipRoot);
//    }

//    public void Show(WeaponPrefabData weapon, Vector2 position)
//    {
//        if (weapon == null)
//        {
//            tooltipRoot.style.display = DisplayStyle.None;
//            return;
//        }

//        tooltipName.text = string.IsNullOrEmpty(weapon.weaponName)
//            ? $"{weapon.grade} {weapon.type}"
//            : weapon.weaponName;

//        gradeType.text = $"{weapon.grade}, {weapon.type}";
//        level.text = $"레벨: {weapon.level}";
//        description.text = weapon.description;

//        tooltipReqStat.text =
//            $"체력 - {weapon.requiredHp} 힘 -  {weapon.requiredStr} 민첩 - {weapon.requiredDex} 지력 - {weapon.requiredInt} 운 - {weapon.requiredluk}";


//        tooltipAtk.text =
//            $"공격력 : {weapon.attackPower}     " +
//            $"공격속도 : {weapon.attackSpeed}\n\n" +
//            $"치명타 확률 : {weapon.critChance}%   치명타 데미지 : {weapon.critDamage}%\n\n" +
//            $"사거리 : {weapon.range}";

//        price.text = $"판매가: {weapon.price} G";

//        //tooltipRoot.style.left = position.x + 10;
//        //tooltipRoot.style.top = position.y + 10;
//        tooltipRoot.style.display = DisplayStyle.Flex;

//        tooltipRoot.schedule.Execute(() =>
//        {
//            float width = tooltipRoot.resolvedStyle.width;
//            float height = tooltipRoot.resolvedStyle.height;

//            var rootBound = tooltipRoot.parent.worldBound;

//            float newX = position.x + 10;
//            float newY = position.y + 10;

//            if (newX + width > rootBound.xMax)
//            {
//                Debug.Log("[툴팁 반전] 오른쪽 넘침 → 왼쪽으로 이동");
//                newX = position.x - width - 10;
//            }

//            if (newY + height > rootBound.yMax)
//            {
//                Debug.Log("[툴팁 반전] 아래쪽 넘침 → 위쪽으로 이동");
//                newY = position.y - height - 10;
//            }

//            tooltipRoot.style.left = newX;
//            tooltipRoot.style.top = newY;
//        });

//    }

//    public void Hide()
//    {
//        tooltipRoot.style.display = DisplayStyle.None;
//    }
//}
