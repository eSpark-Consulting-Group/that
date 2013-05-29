using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace APTAT.Core.Tasks
{
    public class ActionFactoryInstall 
    {
    }

    public class ActionFactoryAgentControl
    {
        [Serializable]
        public class DataInput 
        {
            public enum ControlAction
            {
                Update,
                Uninstall
            };

            private ControlAction _controlAction = ControlAction.Update;
            private bool _forceUpdate;

            [NonSerialized]
            private FileStream _agentMsiFileStream;

            public ControlAction AgentControlAction
            {
                get { return _controlAction; }
                set { _controlAction = value; }
            }

            public bool ForceUpdate
            {
                get { return _forceUpdate; }
                set { _forceUpdate = value; }
            }
        }
    }
}
