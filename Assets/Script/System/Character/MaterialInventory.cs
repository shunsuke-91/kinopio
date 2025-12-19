using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MaterialInventory
{
    public const int MaxCountPerMaterial = 99;

    [SerializeField]
    private List<MaterialStack> items = new List<MaterialStack>();

    public int GetCount(string id)
    {
        if (string.IsNullOrEmpty(id) || items == null) return 0;
        var stack = items.Find(s => s != null && s.materialId == id);
        return stack != null ? Mathf.Clamp(stack.count, 0, MaxCountPerMaterial) : 0;
    }

    public void Add(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return;
        if (items == null) items = new List<MaterialStack>();

        var stack = items.Find(s => s != null && s.materialId == id);
        if (stack == null)
        {
            stack = new MaterialStack { materialId = id, count = 0 };
            items.Add(stack);
        }

        int newCount = Mathf.Clamp(stack.count + amount, 0, MaxCountPerMaterial);
        stack.count = newCount;
    }

    public bool TryConsume(string id, int amount)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0 || items == null) return false;
        var stack = items.Find(s => s != null && s.materialId == id);
        if (stack == null || stack.count < amount) return false;

        stack.count = Mathf.Clamp(stack.count - amount, 0, MaxCountPerMaterial);
        return true;
    }

    public List<MaterialStack> ToSerializableList()
    {
        if (items == null) return new List<MaterialStack>();
        var copy = new List<MaterialStack>(items.Count);
        foreach (var item in items)
        {
            if (item == null) continue;
            copy.Add(new MaterialStack
            {
                materialId = item.materialId,
                count = Mathf.Clamp(item.count, 0, MaxCountPerMaterial)
            });
        }
        return copy;
    }

    public void LoadFromList(List<MaterialStack> source)
    {
        items = new List<MaterialStack>();
        if (source == null) return;

        foreach (var stack in source)
        {
            if (stack == null || string.IsNullOrEmpty(stack.materialId)) continue;
            int count = Mathf.Clamp(stack.count, 0, MaxCountPerMaterial);
            items.Add(new MaterialStack { materialId = stack.materialId, count = count });
        }
    }
}

[Serializable]
public class MaterialStack
{
    public string materialId;
    public int count;
}
