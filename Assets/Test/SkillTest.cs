using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTest : MonoBehaviour
{
    public GameObject skillPrefab;
    public SkillHelper.MoveLogic moveLogic;
    public SkillHelper.DamageLogic damageLogic;
    public GameObject Target;
    
    public float duration = 10f;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var obj = GameObject.Instantiate(skillPrefab);
            // obj.transform.SkillMove(moveLogic, null, Target.transform, out var isPathMove);
            Destroy(obj, duration);
        }
    }
}
