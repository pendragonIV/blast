using System;
using UnityEngine;

public abstract class TutorialStep : ScriptableObject
{
    public event Action<TutorialStep> OnStepCompleted;
    protected TutorialManager _tutorialManager;

    public virtual void Execute(TutorialManager tutorialManager)
    {
        _tutorialManager = tutorialManager;
    }

    protected void EndStep()
    {
        OnStepCompleted?.Invoke(this);
    }
}
