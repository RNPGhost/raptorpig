﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusStack<T> {
  private T _default_status;
  private T _current_status; // Most recently assigned status
  private List<T> _status_stack = new List<T>();

  public StatusStack(T default_status)
  {
    _default_status = default_status;
    _current_status = _default_status;
  }

  public T GetValue() {
    return _current_status;
  }

  public void SetValue (T new_status) {
    _status_stack.Add(new_status);
    _current_status = new_status;
	}
	

  public void UnsetValue(T old_status)
  {
    _status_stack.Remove(old_status);
    int stack_size = _status_stack.Count;
    _current_status = stack_size > 0 ? _status_stack[stack_size - 1] : _default_status;
  }
}
