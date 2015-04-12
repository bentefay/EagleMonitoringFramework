using System;
using System.Collections.Generic;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{

    public abstract class Trigger
    {
        protected Check Check;
        protected object[] Input;
        protected LinkedList<Action> Actions = new LinkedList<Action>();

        abstract public bool Test(object value);

        public abstract Type GetValueType();

        public void AddAction(Action action)
        {
            var myNode = new LinkedListNode<Action>(action);
            Actions.AddLast(myNode);

            action.SetTrigger(this);
        }

        public void SetCheck(Check check)
        {
            Check = check;
        }

        public Check GetCheck()
        {
            return Check;
        }
    }
}
