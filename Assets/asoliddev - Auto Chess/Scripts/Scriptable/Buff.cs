using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffActiveMode { PerSecond, BeforeAttack, AfterAttack, BeforeHit, AfterHit }
public enum BuffStackMode { Time, Layer }

[CreateAssetMenu(fileName = "DefaultBuff", menuName = "AutoChess/Buff", order = 3)]
public class Buff : ScriptableObject
{
    public string displayName;
    public float duration;
    public int layer = 1;

    public BuffActiveMode activeMode;
    public BuffStackMode stackMode;

}
