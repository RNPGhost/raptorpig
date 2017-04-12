﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {
  [SerializeField]
  private Player _owner;
  public Player Owner {
    get {
      return _owner;
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
  private float _health;

  private bool _targetable = true;
  public bool Targetable {
    set {
      _targetable = value;
    }
  }

  private Character _targetable_character;
  public Character Targetable_Character {
    set {
      _targetable_character = value;
    }
  }
  
  public Character AcquireTarget() {
    if (_targetable) {
      return _targetable_character;
    } else {
      return null;
    }
  }

  public void Start() {
    _targetable_character = this;
  }

  public void TakeDamage(float damage) {
    Debug.Log(_name + " took " + damage + " damage");
    _health = Mathf.Max(_health - damage, 0);
  }
}
