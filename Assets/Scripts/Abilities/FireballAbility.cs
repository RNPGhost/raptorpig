﻿using UnityEngine;

public class FireballAbility : Ability {
  [SerializeField]
  private float _damage;
  [SerializeField]
  private Transform _fireball_spawnpoint;
  [SerializeField]
  private GameObject _fireball_prefab;

  private Character _selected_character;
  private Character _target;

  public override string GetName() {
    return "Fireball";
  }

  public override float GetEnergyCost()
  {
    return 40f;
  }

  public override void SetTarget(Character character) {
    if (!IsInProgress() && IsValidTarget(character)) {
      _selected_character = character;
    }
  }

  public override void Reset() {
    if (!IsInProgress()) {
      _selected_character = null;
    }
  }

  protected override bool IsReadyToBeActivated()
  {
    return base.IsReadyToBeActivated() && _selected_character != null;
  }

  protected override void AbilitySpecificPhaseUpdate(Phase phase) {
    switch (phase.Name) {
      case PhaseName.Ready:
        OwningCharacter.UnsetDirectionTarget(_target);
        break;
      case PhaseName.Preparation:
        _target = _selected_character.AcquireAsTargetBy(OwningCharacter);
        OwningCharacter.SetDirectionTarget(_target);
        break;
      case PhaseName.Recovery:
        CreateFireball();
        break;
      default:
        break;
    }
  }

  private void Start() {
    SetPhases(new Phase[] {
      new Phase(PhaseName.Ready),
      new Phase(PhaseName.Preparation, 1.8f),
      new Phase(PhaseName.Recovery, 1.5f),
      });
  }

  private bool IsValidTarget(Character character) {
    return (character.OwningPlayer.Id != OwningCharacter.OwningPlayer.Id
            && character.Targetable);
  }

  private void CreateFireball() {
    GameObject fireball = Instantiate(_fireball_prefab, _fireball_spawnpoint);
    TargetingMover fireballMover = fireball.GetComponent<TargetingMover>();
    fireballMover.SetTarget(_target.CentrePosition);
    fireballMover.SetSpeed(10 * GetSpeedMultiplier());
    ExplodeOnContact fireballExploder = fireball.GetComponent<ExplodeOnContact>();
    fireballExploder.SetDamage(10 * OwningCharacter.GetAbilityPowerMultiplier());
    fireballExploder.SetCaster(OwningCharacter);
  }
}