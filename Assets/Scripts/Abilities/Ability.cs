﻿using UnityEngine;

public abstract class Ability : MonoBehaviour {
  [SerializeField]
  private Character _owning_character;
  public Character OwningCharacter {
    get {
      return _owning_character;
    }
  }
  [SerializeField]
  private Animator _animator;
  protected Animator Animator
  {
    get
    {
      return _animator;
    }
  }

  private PhaseLoop _phases;
  private PhaseName _current_phase_name;
  private float _next_phase_change;
  private bool _phase_transition_allowed = false;
  private float _speed_multiplier;

  // returns the ability name
  public abstract string GetName();

  // returns the ability energy cost
  public abstract float GetEnergyCost();

  // returns whether this ability was activated
  public bool Activate()
  {
    if (IsReadyToBeActivated()) {
      _speed_multiplier = OwningCharacter.GetAbilitySpeedMultiplier();
      UnpausePhaseTransition();
      return true;
    }

    return false;
  }

  // provides a target for the ability
  public virtual void SetTarget(Character character) {}

  // resets the ability if it is not currently activated
  public virtual void Reset() {}

  // returns whether this ability was interrupted
  public virtual bool Interrupt() {
    if (Interruptable()) {
      PausePhaseTransition();
      OwningCharacter.Silence(2.5f);
      SetToNextReadyPhase();
      PhaseUpdate(_phases.Current());
      StartInterruptAnimation();
      return true;
    }

    return false;
  }

  // returns whether an ability is ready to be set up to be activated
  public virtual bool IsInProgress()
  {
    return !(_current_phase_name == PhaseName.Ready);
  }

  public float GetRemainingTime()
  {
    if (IsInProgress())
    {
      return _next_phase_change - Time.time + _phases.GetTimeTillNext(PhaseName.Ready);
    }
    else
    {
      return 0f;
    }
  }

  public float GetTotalAbilityTime()
  {
    return _phases.GetTimeSinceLastReady() + GetRemainingTime();
  }

  // returns a number between 0 and 1 that represents the ability's progress to its next ready state
  public float Progress()
  {
    float total_ability_time = GetTotalAbilityTime();
    return Mathf.Min((total_ability_time - GetRemainingTime()) / total_ability_time, 1f);

  }

  // 
  protected virtual bool IsReadyToBeActivated()
  {
    return !IsInProgress() && OwningCharacter.IsReadyToActivateAbility(this);
  }

  // carries out any actions that are specific to the ability and to the phase the ability is in
  protected abstract void AbilitySpecificPhaseUpdate(Phase phase);

  // sets up the phases of the ability, and how long it should spend in each phase
  protected void SetPhases(Phase[] phases) {
    _phases = new PhaseLoop(phases);
  }

  // returns whether it is currently possible to interrupt the ability
  protected virtual bool Interruptable() {
    return (!(GetCurrentPhaseName() == PhaseName.Ready));
  }

  // resets the ability back to the ready phase
  protected virtual void SetToNextReadyPhase() {
    _next_phase_change = Time.time;
    GoToNextPhase(PhaseName.Ready);
  }

  // start the ability progressing through its phases
  protected void UnpausePhaseTransition() {
    _next_phase_change = Time.time;
    _phase_transition_allowed = true;
  }

  // stops the ability progressing through its phases
  protected void PausePhaseTransition() {
    _phase_transition_allowed = false;
  }

  // gets the name of the phase the ability is currently in
  protected PhaseName GetCurrentPhaseName() {
    return _current_phase_name;
  }

  protected float GetSpeedMultiplier()
  {
    return _speed_multiplier;
  }

  private void Update() {
    while (_phase_transition_allowed && Time.time >= _next_phase_change) {
      GoToNextPhase();
    }
  }

  private void GoToNextPhase() {
    _phases.GoToNext();
    PhaseUpdate(_phases.Current());
  }

  private void GoToNextPhase(PhaseName name) {
    _phases.GoToNext(name);
    PhaseUpdate(_phases.Current());
  }

  private void PhaseUpdate(Phase phase) {
    UpdatePhaseVariables(phase);
    GeneralAbilityPhaseUpdate(phase);
    AbilitySpecificPhaseUpdate(phase);
  }

  private void GeneralAbilityPhaseUpdate(Phase phase)
  {
    switch (phase.Name)
    {
      case PhaseName.Ready:
        OwningCharacter.UnsetActiveAbility(this);
        Reset();
        PausePhaseTransition();
        break;
      case PhaseName.Preparation:
        OwningCharacter.UseEnergy(GetEnergyCost());
        OwningCharacter.SetActiveAbility(this);
        StartAnimation();
        break;
      default:
        break;
    }
  }

  private void StartAnimation()
  {
    Animator.SetFloat(GetName() + "Speed", _speed_multiplier);
    Animator.SetTrigger(GetName() + "Trigger");
  }

  private void StartInterruptAnimation()
  {
    Animator.SetTrigger("InterruptedTrigger");
  }

  private void UpdatePhaseVariables(Phase phase) {
    _current_phase_name = phase.Name;
    _next_phase_change += (phase.Duration / _speed_multiplier);
  }
}
