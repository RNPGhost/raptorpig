﻿using UnityEngine;

public class Player : MonoBehaviour {
  [SerializeField]
  private string _id;
  public string Id {
    get {
      return _id;
    }
  }
}