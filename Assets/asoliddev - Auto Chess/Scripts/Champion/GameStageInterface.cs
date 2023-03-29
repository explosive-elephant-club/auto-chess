using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface GameStageInterface
{
    void OnEnterPreparation();
    void OnUpdatePreparation();
    void OnLeavePreparation();

    void OnEnterCombat();
    void OnUpdateCombat();
    void OnLeaveCombat();

    void OnEnterLoss();
    void OnUpdateLoss();
    void OnLeaveLoss();
}