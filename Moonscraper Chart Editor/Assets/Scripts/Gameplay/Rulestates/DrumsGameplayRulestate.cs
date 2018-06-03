using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumsGameplayRulestate : BaseGameplayRulestate {

    public DrumsGameplayRulestate(MissFeedback missFeedback) : base(missFeedback)
    {

    }

    public void Update(float time, HitWindow<DrumsNoteHitKnowledge> hitWindow, GamepadInput drumsInput)
    {
    }

    public override void Reset()
    {

    }
}
