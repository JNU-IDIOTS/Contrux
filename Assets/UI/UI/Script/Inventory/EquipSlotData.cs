using UnityEngine;
using UnityEngine.UIElements;
public class EquipSlotData
{
    public VisualElement slotElement;
    public WeaponPrefabData weaponData;

    public EquipSlotData(VisualElement slotElement)
    {
        this.slotElement = slotElement;
    }

    public void SetItem(WeaponPrefabData data)
    {
        weaponData = data;
        slotElement.userData = data;
        UpdateIcon();
    }

    public void Clear(bool includeData = true)
    {
        if (includeData)
            weaponData = null;

        slotElement.userData = null;

        slotElement.schedule.Execute(() =>
        {
            slotElement.Clear();
        });
    }

    private void UpdateIcon()
    {
        //Debug.Log("[UpdateIcon] È£ÃâµÊ");
        slotElement.schedule.Execute(() =>
        {
            slotElement.Clear();
            if (weaponData != null && weaponData.icon != null)
            {
                var icon = new VisualElement();
                icon.style.backgroundImage = new StyleBackground(weaponData.icon);
                icon.style.width = 120;
                icon.style.height = 142;
                icon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                slotElement.Add(icon);
            }
        });
    }
}
