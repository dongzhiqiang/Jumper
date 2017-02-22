using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class UIGroup : MonoBehaviour
{
    List<GameObject> m_items = new List<GameObject>();

    bool m_cached = false;

    public int Count { get { Cache(); return m_items.Count != 1 ? m_items.Count : ((m_items[0].activeSelf) ? 1 : 0); } }
    public GameObject this[int index] { get { Cache(); return m_items[index]; } }


    public IEnumerator GetEnumerator()
    {
        Cache();
        return this.m_items.GetEnumerator();
    }


    void Cache()//有时候别的函数的执行可能先于Start()函数，这时候Cache()下就相当于先执行了Start()
    {
        if (m_cached)
            return;

        Transform t = this.transform;
        for (int i = 0; i < t.childCount; ++i)
        {
            GameObject go = t.GetChild(i).gameObject;
            go.name = "item" + i;
            m_items.Add(go);
        }


        m_cached = true;
    }

    //提供设置大小的功能
    public void SetCount(int count)
    {
        Cache();
        if (count == 0)
        {
            this.gameObject.SetActive(false);
            return;
        }
        else
            this.gameObject.SetActive(true);


        //多退少补
        int curCount = m_items.Count;
        GameObject s;
        if (count < curCount)
        {
            for (int i = count; i < curCount; ++i)
            {
                s = m_items[i];
                //这里不要把m_itemTemplate对应的对象销毁了
                //if (m_itemTemplate == s)
                //    s.gameObject.SetActive(false);
                //else
                UnityEngine.Object.Destroy(s);
            }
            m_items.RemoveRange(count, curCount - count);
        }
        else if (count > curCount)
        {
            GameObject go;
            Transform t;
            for (int i = curCount; i < count; ++i)
            {
                GameObject template = m_items[m_items.Count - 1];//m_itemTemplate != null ? m_itemTemplate : 
                go = GameObject.Instantiate(template.gameObject) as GameObject;
                go.gameObject.name = "item" + i;
                t = go.transform;
                t.SetParent(this.transform, false);
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = m_items[m_items.Count - 1].gameObject.transform.localScale;
                if (go.layer != this.gameObject.layer) go.layer = this.gameObject.layer;
                if (go.activeSelf == false) go.SetActive(true);
                m_items.Add(go);
            }
        }
    }

    public GameObject Get(int idx)
    {
        Cache();
        if (idx >= m_items.Count || idx < 0)
            return null;
        return m_items[idx];
    }

    public T Get<T>(int idx) where T : Component
    {
        Cache();
        if (idx >= m_items.Count || idx < 0)
            return null;
        return m_items[idx].GetComponent<T>();
    }


}
