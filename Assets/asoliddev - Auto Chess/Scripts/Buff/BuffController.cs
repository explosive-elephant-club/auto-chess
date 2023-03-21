using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    public List<Buff> buffList = new List<Buff>();


    public void AddBuff(BaseBuffData buffData, GameObject _caster = null)
    {
        Buff buff = new Buff(buffData, gameObject, _caster);
        buffList.Add(buff);
        buff.onBuffStart();
    }
}
