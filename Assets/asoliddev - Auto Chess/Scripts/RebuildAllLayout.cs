using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class RebuildAllLayout : BaseControllerUI
{
    public List<RectTransform> allSizeFitterRects = new List<RectTransform>();

    public override void Awake()
    {
        base.Awake();
        Init();
        Register(transform);

    }

    void Register(Transform root)
    {
        List<Transform> stack = new List<Transform>();
        stack.Add(root);
        while (stack.Count > 0)
        {
            if (stack[0].GetComponent<ContentSizeFitter>() != null)
            {
                allSizeFitterRects.Add(stack[0].GetComponent<RectTransform>());
            }
            foreach (Transform child in stack[0])
            {
                stack.Add(child);
            }
            stack.RemoveAt(0);
        }
    }

    public IEnumerator RebuildAllSizeFitterRects()
    {
        SetUIActive(false);
        for (int i = allSizeFitterRects.Count - 1; i >= 0; i--)
        {
            if (allSizeFitterRects[i].gameObject.activeSelf)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(allSizeFitterRects[i]);
                yield return new WaitForEndOfFrame();
            }
        }
        SetUIActive(true);
    }

    public void RebuildAll()
    {
        StartCoroutine(RebuildAllSizeFitterRects());
    }
}
