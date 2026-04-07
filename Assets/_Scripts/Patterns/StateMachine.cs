using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace Assets._Scripts.Patterns
{
    public class StateMachine<T> where T : Enum
    // public abstract class StateMachine<T> : MonoBehaviour where T : Enum
    {
        private Dictionary<T, AState<T>> _stateDict = new();

        public AState<T> CurrentState {get; private set;}
        private AState<T> _defaultState;

        public StateMachine()
        {
        }

        public StateMachine(AState<T> defaultState, params AState<T>[] states)
        {
            _defaultState = defaultState;
            AddStates(states);
        }

        // public abstract void Init(); // Setup dict and change to default state

        public bool TryGetState(T key, out AState<T> state)
        {
            return _stateDict.TryGetValue(key, out state);
        }

        public void SetDefaultState(T key)
        {
            _stateDict.TryGetValue(key, out _defaultState);
        }

        public void AddState(AState<T> state)
        {
            // if (_stateDict.ContainsKey(state.Key)) Debug.Log($"Overrided state {state.Key}");
            // else Debug.Log($"Add new state {state.Key}");
            _stateDict[state.Key] = state;
        }

        public void AddStates(params AState<T>[] states)
        {
            foreach (var state in states)
            {
                AddState(state);
            }
        }

        public bool TryRemoveState(T key)
        {
            if (!_stateDict.ContainsKey(key)) return false;

            _stateDict.Remove(key);
            return true;
        }

        public void ChangeState(T key)
        {
            if(!TryGetState(key, out var next) || next == null || next == CurrentState) return;

            CurrentState?.Exit();
            CurrentState = next;
            CurrentState.Enter();
            Debug.Log($"Changed to {CurrentState.Key} state");
        }

        public void ChangeToDefault()
        {
            if (_defaultState != null)
            {
                ChangeState(_defaultState.Key);
                return;
            }

            if (_stateDict.Count == 0) 
            {
                // Debug.Log($"State machine has no state");
                // Debug.Log($"State machine {name} has no state");
                return;
            }

            ChangeState(_stateDict.First().Key);
            // Debug.Log($"No default state, fallback to {CurrentState.Key} state");
        }

        public void DoState()
        // void Update()
        {
            if (!CurrentState.GetNextState().Equals(CurrentState.Key))
                ChangeState(CurrentState.GetNextState());

            if (_stateDict.Count > 0 || CurrentState != null)
                CurrentState.Do();
        }
    }
}