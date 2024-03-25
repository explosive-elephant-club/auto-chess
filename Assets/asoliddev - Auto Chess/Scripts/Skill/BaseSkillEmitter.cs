using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSkillEmitter : MonoBehaviour
{
    public bool isContinuousByDuration;
    public int effectCount;
    public float duration;
    public float interval;
    public int emitCount;
    protected List<Transform> emitPoints;
    public Skill skill;

    protected int intervalCount = 0;
    protected float intervalTimer = 0;
    protected float totalTimer = 0;
    protected int curEmitPointIndex = 0;
    // Start is called before the first frame update
    public virtual void Init(Skill _skill)
    {
        skill = _skill;
        emitPoints = new List<Transform>(emitCount);
        if (!isContinuousByDuration)
        {
            duration = interval * effectCount;
        }
        Emit();
        intervalCount++;
    }

    public void Update()
    {
        Move();
        if (intervalCount < emitCount)
        {
            if (intervalTimer > interval)
            {
                intervalTimer = 0;
                intervalCount++;
                Emit();
            }
            else
            {
                intervalTimer += Time.deltaTime;
            }
        }
        if (totalTimer > duration)
        {
            Finish();
        }
        else
        {
            totalTimer += Time.deltaTime;
        }


    }

    public virtual SkillEffect Emit()
    {
        GameObject effectInstance = GameObject.Instantiate(skill.effectPrefab);
        effectInstance.transform.parent = emitPoints[curEmitPointIndex];
        effectInstance.transform.localPosition = Vector3.zero; //GetCastPoint().position;
        effectInstance.transform.localRotation = Quaternion.Euler(Vector3.zero); //GetCastPoint().rotation;
        SkillEffect effectScript = effectInstance.GetComponent<SkillEffect>();
        effectScript.Init(skill);
        return effectScript;
    }

    public void Finish()
    {

    }

    public virtual void Move()
    {

    }
}
