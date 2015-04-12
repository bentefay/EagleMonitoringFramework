using System;
using System.Collections.Generic;
using Action = Eagle.Server.Framework.Entities.Actions.Action;

namespace Eagle.Server.Framework.Entities.Triggers
{

    public abstract class Trigger
    {
        protected Check check;

        protected object[] input;
        protected LinkedList<Action> actions = 
            new LinkedList<Action>();

        abstract public bool Test(object value);

        public abstract Type GetValueType();

        public void AddAction(Action action)
        {
            LinkedListNode<Action> myNode =
                new LinkedListNode<Action>(action);
            actions.AddLast(myNode);

            action.setTrigger(this);
        }

        public void setCheck(Check check)
        {
            this.check = check;
        }

        public Check getCheck()
        {
            return check;
        }
    }
}
