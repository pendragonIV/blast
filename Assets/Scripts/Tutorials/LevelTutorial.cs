using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/LevelTutorial")]
public class LevelTutorial : ScriptableObject
{
    [field: SerializeField]
    public int Level { get; set; }
    [SerializeField]
    private List<TutorialStep> _tutorialSteps = new();
    private int _stepIndex = 0;
    private TutorialManager _tutorialManager;

    public void Begin(TutorialManager tutorialManager)
    {
        _stepIndex = 0;
        _tutorialManager = tutorialManager;
        if (_tutorialSteps.Count == 0)
            return;

        ExecuteTutorialStep();
    }

    private void ExecuteTutorialStep()
    {
        _tutorialSteps[_stepIndex].OnStepCompleted += OnStepCompleted;
        _tutorialSteps[_stepIndex].Execute(_tutorialManager);
    }

    private void OnStepCompleted(TutorialStep tutorialStep)
    {
        tutorialStep.OnStepCompleted -= OnStepCompleted;
        _stepIndex++;

        if (_stepIndex >= _tutorialSteps.Count)
        {
            _tutorialManager.EndTutorial();
            return;
        }

        ExecuteTutorialStep();
    }
}
