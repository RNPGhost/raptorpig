﻿using System;
using UnityEngine;

public class Character : MonoBehaviour {
  [SerializeField]
  private Player _owning_player;
  public Player OwningPlayer {
    get {
      return _owning_player;
    }
  }
  [SerializeField]
  private string _name;
  public string Name {
    get {
      return _name;
    }
  }
  [SerializeField]
  private GameObject _centre_position;
  public GameObject CentrePosition
  {
    get
    {
      return _centre_position;
    }
  }
  [SerializeField]
  private float _vitality; // 0 <= vitality <= 10
  [SerializeField]
  private float _endurance; // 0 <= endurance <= 10
  [SerializeField]
  private float _power; // 0 <= power <= 10
  [SerializeField]
  private float _speed; // 0 <= speed <= 10
  [SerializeField]
  private float _base_health;
  [SerializeField]
  private float _base_energy;
    
  private float _health;
  public float Health
  {
    get
    {
      return _health;
    }
  }
  private float _max_health;
  public float MaxHealth
  {
    get
    {
      return _max_health;
    }
  }
  private float _energy;
  public float Energy
  {
    get
    {
      return _energy;
    }
  }
  private float _max_energy;
  public float MaxEnergy
  {
    get
    {
      return _max_energy;
    }
  }



  private Ability _active_ability;
  private StatusStack<bool> _targetable = new StatusStack<bool>(true);
  private StatusStack<bool> _damage_immune = new StatusStack<bool>(false);
  private StatusStack<Character> _targetable_character;
  private StatusStack<Character> _direction_target = new StatusStack<Character>(null);
  private Quaternion _initial_rotation;

  private float _silence_end = 0;
  private float _silence_duration = 0;

  public bool Targetable {
    get {
      return _targetable.GetValue();
    }
  }

  public void SetUntargetable() {
    _targetable.SetValue(false);
  }

  public void UnsetUntargetable() {
    _targetable.UnsetValue(false);
  }

  public void SetDamageImmune() {
    _damage_immune.SetValue(true);
  }

  public void UnsetDamageImmune() {
    _damage_immune.UnsetValue(true);
  }

  public void SetTargetableCharacter(Character character) {
    _targetable_character.SetValue(character);
  }

  public void UnsetTargetableCharacter(Character character) {
    _targetable_character.UnsetValue(character);
  }

  public Character AcquireAsTargetBy(Character targeter) {
    if (Targetable) {
      return _targetable_character.GetValue();
    }

    return null;
  }

  public void SetDirectionTarget(Character target)
  {
    _direction_target.SetValue(target);
  }

  public void UnsetDirectionTarget(Character target)
  {
    _direction_target.UnsetValue(target);
  }

  public void Interrupt() {
    Debug.Log("Interuption attempt");
    if (_active_ability != null)
    {
      _active_ability.Interrupt();
    }
  }

  public void Silence(float duration)
  {
    float new_silence_end = Time.time + duration;
    if (new_silence_end > _silence_end)
    {
      _silence_end = new_silence_end;
      _silence_duration = duration;
    }
  }

  public void SetActiveAbility(Ability ability) {
    _active_ability = ability;
  }

  public bool UnsetActiveAbility(Ability ability) {
    if (_active_ability == ability)
    {
      _active_ability = null;
      return true;
    }
    return false;
  }

  public bool AbilityInProgress() {
    return _active_ability != null;
  }

  public bool IsReadyToActivateAbility(Ability ability) {
    return !AbilityInProgress() && _energy >= ability.GetEnergyCost() && !IsSilenced();
  }

  public float GetProgressTillReadyToActivateAbility()
  {
    float active_ability_progress = 1f;
    if (_active_ability != null)
    {
      active_ability_progress = _active_ability.Progress();
    }
    return Mathf.Min(active_ability_progress, GetSilenceProgress());
  }

  public float GetAbilitySpeedMultiplier()
  {
    return GetAttributeMultiplier(_speed);
  }

  public float GetAbilityPowerMultiplier()
  {
    return GetAttributeMultiplier(_power);
  }

  public void TakeDamage(float damage) {
    if (!_damage_immune.GetValue()) {
      Debug.Log(_name + " took " + damage + " damage");
      _health = Mathf.Max(_health - damage, 0);
    }
  }

  public void UseEnergy(float energy_cost)
  {
    _energy = Math.Max(_energy - energy_cost, 0f);
  }
  
  private void Start()
  {
    _targetable_character = new StatusStack<Character>(this);
    _initial_rotation = transform.rotation;
    _max_health = _base_health * GetAttributeMultiplier(_vitality);
    _health = _max_health;
    _max_energy = _base_energy * GetAttributeMultiplier(_endurance);
    _energy = _max_energy;
  }

  private void Update()
  {
    LookTowardsTarget();
    RegenerateEnergy();
  }

  private void LookTowardsTarget()
  {
    Character target = _direction_target.GetValue();
    if (target != null)
    {
      transform.LookAt(new Vector3(target.gameObject.transform.position.x, transform.position.y, target.gameObject.transform.position.z));
    }
    else if (transform.rotation != _initial_rotation)
    {
      transform.rotation = _initial_rotation;
    }
  }

  private void RegenerateEnergy()
  {
    float regeneration_percentage = 0.05f;
    float current_energy_multiplier = 0.8f + 0.4f * _energy / _max_energy;
    float regenerated_energy = regeneration_percentage * _max_energy * current_energy_multiplier * Time.deltaTime;
    _energy = Math.Min(_energy + regenerated_energy, _max_energy);
  }

  private float GetAttributeMultiplier(float attribute)
  {
    return 0.8f + attribute * 0.04f;
  }

  private bool IsSilenced()
  {
    return Time.time < _silence_end;
  }

  private float GetSilenceProgress()
  {
    return Mathf.Min((_silence_duration - (_silence_end - Time.time)) / _silence_duration, 1f);
  }
}
