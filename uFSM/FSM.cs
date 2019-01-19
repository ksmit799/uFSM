using System;
using System.Collections.Generic;
using System.Reflection;

namespace uFSM
{
    public class FSM<T> where T : class, new()
    {
        private static T m_Instance = new T();
        private static object m_FSMLock = new object();

        private List<KeyValuePair<string, object[]>> m_TransitionQueue = new List<KeyValuePair<string, object[]>>();
        private string m_State = "Off";

        public void Request(string name, object[] args = null)
        {
            lock (m_FSMLock)
            {
                // If we are already processing a transition, queue it.
                if (m_State == null)
                    m_TransitionQueue.Add(new KeyValuePair<string, object[]>(name, args));

                // Otherwise, process the transition.
                else
                    ProcessTransition(name, args);
            }
        }

        private void ProcessTransition(string name, object[] args)
        {
            string oldState = m_State;
            string newState = name;
            m_State = null; // Because we are transitioning, we don't have an active state.

            Type classType = m_Instance.GetType();

            try
            {
                // Attempt to execute an exit method if it exists.
                if (oldState != null)
                {
                    MethodInfo exitMethod = classType.GetMethod("Exit" + oldState);
                    if (exitMethod != null)
                        // Invoke our exit method.
                        exitMethod.Invoke(this, new object[] { });
                }

                // Get our enter method.
                MethodInfo enterMethod = classType.GetMethod("Enter" + newState);
                if (enterMethod == null)
                    throw new FSMError($"Error transitioning to state '{newState}', no such state exists...");

                // Call the enter method of our new state.
                enterMethod.Invoke(this, args);
            }
            catch (Exception fsmError)
            {
                m_State = "Off";
                throw fsmError;
            }

            // Update our current state.
            m_State = newState;

            // If we have transitions queued up, process them.
            if (m_TransitionQueue.Count >= 1)
            {
                KeyValuePair<string, object[]> transition = m_TransitionQueue[0];
                m_TransitionQueue.RemoveAt(0);
                ProcessTransition(transition.Key, transition.Value);
            }
        }
    }
}