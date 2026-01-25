using UnityEngine;

public class DesignModeSwitcher : MonoBehaviour

//Craft画面でCraftUIとUpgradeUIを切り替えるためのスクリプト
{
    [Header("Roots")]
    [SerializeField] private GameObject craftRoot;
    [SerializeField] private GameObject upgradeRoot;

    private void Start()
    {
        ShowCraft();
    }

    public void ShowCraft()
    {
        if (craftRoot != null) craftRoot.SetActive(true);
        if (upgradeRoot != null) upgradeRoot.SetActive(false);
    }

    public void ShowUpgrade()
    {
        if (craftRoot != null) craftRoot.SetActive(false);
        if (upgradeRoot != null) upgradeRoot.SetActive(true);
    }
}