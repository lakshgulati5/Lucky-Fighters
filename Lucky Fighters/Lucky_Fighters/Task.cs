using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucky_Fighters
{
    /// <summary>
    /// A task that needs to be completed after a delay<br/>
    /// TaskAction is a function () => void<br/>
    /// Usage: Player.AddTask(new Task(delay, () => { do stuff here }));<br />
    /// Chain tasks with Task.Then()
    /// </summary>
    class Task
    {
        private float toWait;
        public TaskAction OnCompleted;
        private LinkedList<Task> taskList;

        public delegate void TaskAction();

        public bool IsCompleted => toWait < 0f;

        public Task(float delay, TaskAction action)
        {
            toWait = delay;
            OnCompleted = action;
            taskList = new LinkedList<Task>();
        }

        // Run another task after this one is finished
        public Task Then(float delay, TaskAction action)
        {
            taskList.AddLast(new Task(delay, action));
            return this;
        }

        public void Update(float elapsed)
        {
            toWait -= elapsed;
        }

        public void WhenCompleted()
        {
            OnCompleted();
            LinkedListNode<Task> first = taskList.First;
            if (first != null)
            {
                Task next = taskList.First.Value;
                toWait += next.toWait;
                OnCompleted = next.OnCompleted;
                taskList.RemoveFirst();
            }
        }
    }
}
