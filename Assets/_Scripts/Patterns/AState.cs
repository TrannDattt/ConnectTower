using System;

namespace Assets._Scripts.Patterns
{
    public abstract class AState<T> where T : Enum
    {
        public T Key {get; private set;}
        public bool IsFinished {get; private set;}

        public AState(T key)
        {
            Key = key;
            IsFinished = false;
        }

        protected void FinishState() => IsFinished = true;

        public virtual void Enter()
        {
            IsFinished = false;
        }

        public virtual void Do()
        {
            if (IsFinished) return;
        }

        public virtual void Exit()
        {
            IsFinished = true;
        }
    }
}