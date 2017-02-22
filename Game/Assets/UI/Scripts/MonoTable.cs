using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.Collections;

public class MonoTable : MonoBehaviour
{
    [System.Serializable]
    public class Param
    {
        public string name; // ½Å±¾Ãû
        public GameObject obj;
    }

    [SerializeField]
    Param[] ps;


    public GameObject getv(string valueName)
    {
        foreach (var p in ps)
        {
            if (p.name == valueName)
                return p.obj;
        }

        return null;
    }
}
