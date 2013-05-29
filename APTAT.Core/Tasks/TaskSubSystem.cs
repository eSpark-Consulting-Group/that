using System;
using System.IO;
using System.Text;

namespace APTAT.Core.Tasks
{
    public interface ITasksSubSystem
    {
        event EventHandler<ActionCompletedEventArgs> ActionCompleted;
        event EventHandler<ActionStartedEventArgs> ActionChanged;
        event EventHandler<ActionDeferredEventArgs> ActionDeferred;
    }

    public class TaskSubSystem : ITasksSubSystem
    {

        public event EventHandler<ActionCompletedEventArgs> ActionCompleted;

        public event EventHandler<ActionStartedEventArgs> ActionChanged;

        public event EventHandler<ActionDeferredEventArgs> ActionDeferred;
    }

    public interface IInternalBusinessObject : IBusinessObject
    {
        bool IsInternal { get; set; }
    }

    public interface ITask : IInternalBusinessObject
    {
        //string Name { get; set; }
        //string Description { get; set; }

        //DateTime Created { get; }
        //DateTime Modified { get; }

        ITaskTarget Target { get; set; }
        //IActuator Actuator { get; }
        //ISequence Sequence { get; set; }

        //bool IsInvalid { get; }
        //bool IsMarkedAsDeleted { get; }

        //bool Visible { get; set; }

        //void Execute();
    }

    public interface ITaskTarget : ICloneable
    {
        IComputer[] GetTargets();
        bool isValid { get; }
    }

    public interface IActionHandle
    {
        void CompleteAction(int result, FormattedText textMessage);
    }

    public interface IActionInput : ICloneable
    {
        Stream[] GetIn(ISubSystemCatalog catalog, IComputer pc);
        void ExecutionBegin(IComputer targetComputer, ISubSystemCatalog catalog, SerializationWriter writer);
        void ExecutionEnd(int code, FormattedText message, IComputer targetComputer, ISubSystemCatalog catalog);

        bool Initialize(object parameterValue);
        bool ExecuteServerSideAction(IActionHandle actionHandle, IComputer targetComputer, ISubSystemCatalog catalog);
    }

    public interface IActionOutput
    {
        Stream GetOut(ISubSystemCatalog catalog);
        void ExecutionEnd(ISubSystemCatalog catalog, IActionResult actionResult, SerializationReader reader);
        bool RequiresExecutionEndOnError { get; }
    }

    public interface IActionFactory : ICloneable
    {
        Guid Uid { get; }
        string Name { get; }
        bool AgentSideAction { get; }

        IActionInput CreateInputInstance();
        IActionOutput CreateOutputProcessor();
        bool IsSuccessCode(int resultCode);
        void OnActionComplete(IActionResult result);
    }

    public interface IAction : ICloneable
    {
        IActionFactory Factory { get; }
        IActionInput Input { get; }
    }

    public enum ResultStatus : short
    {
        Pending = 0,
        Executing = 1,
        Succeeded = 2,
        Failed = 3
    }

    public interface IResult : IInternalBusinessObject
    {
        ResultStatus Status { get; }

        DateTime Started { get; }

        DateTime Completed { get; }
    }

    public interface IActionResult : IResult
    {
        IAction Action { get; }

        ISequenceResult Owner { get; }

        int ResultCode { get; }

        FormattedText ErrorMessage { get; }

        int CompletionProgress { get; }
        FormattedText CompletionMessage { get; }

        object Output { get; }
    }

    public interface ISequence : IInternalBusinessObject
    {
        string Name { get; }
        string Description { get; }
        IAction[] Actions { get; }
        void AddAction(IAction action);
        void RemoveAt(int index);
        void RemoveAllActions();
        bool IsMarkedAsDeleted { get; }

        ISequence CreateNew();
    }

    public interface ISequenceResult : IResult
    {
        IComputer Target { get; }

        ISequence Sequence { get; }

        IActionResult[] ActionResults { get; }
        int CompletionProgress { get; }

        ITaskResult Owner { get; }
    }

    public interface ITaskResult : IResult
    {
        ITask Task { get; }

        string ActualTaskName
        {
            get;
        }

        int CompletionProgress { get; }
        ISequenceResult[] SequenceResults
        {
            get;
        }
    }

    public class ActionCompletedEventArgs : EventArgs
    {
        private readonly IActionResult _actionResult;

        public ActionCompletedEventArgs(IActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public IActionResult ActionResult
        {
            get { return _actionResult; }
        }
    }

    public class ActionStartedEventArgs : EventArgs
    {
        private readonly IActionResult _actionResult;

        public ActionStartedEventArgs(IActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public IActionResult ActionResult
        {
            get { return _actionResult; }
        }
    }

    public class ActionDeferredEventArgs : EventArgs
    {
        private readonly IActionResult _actionResult;

        public ActionDeferredEventArgs(IActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public IActionResult ActionResult
        {
            get { return _actionResult; }
        }
    }

    public class ActionProgressChangedEventArgs : EventArgs
    {
        private readonly IActionResult _actionResult;

        public ActionProgressChangedEventArgs(IActionResult actionResult)
        {
            _actionResult = actionResult;
        }

        public IActionResult ActionResult
        {
            get { return _actionResult; }
        }
    }

    public class ProductRegisteredEventArgs : EventArgs
    {
        private readonly IComputer _computer;
        private readonly Guid _pluginUid;

        public ProductRegisteredEventArgs(IComputer computer, Guid pluginUid)
        {
            _computer = computer;
            _pluginUid = pluginUid;
        }

        public IComputer Computer
        {
            get { return _computer; }
        }

        public Guid PluginUid
        {
            get { return _pluginUid; }
        }
    }

    /// <summary> SerializationWriter.  Extends BinaryWriter to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationWriter : BinaryWriter
    {
        private static readonly long s_DateTimeOffset = (new DateTime(1601, 01, 01)).Ticks;
        private readonly bool m_closeStreamOnDispose;

        public SerializationWriter(Stream output, bool closeStreamOnDispose) :
            base(output, Encoding.Unicode)
        {
            m_closeStreamOnDispose = closeStreamOnDispose;
        }
        public SerializationWriter(Stream output)
            : this(output, false)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (m_closeStreamOnDispose)
            {
                base.Dispose(disposing);
            }
        }


        public void WriteLength(int length)
        {
            Write7BitEncodedInt(length);
        }

        /// <summary> Writes a byte array to the buffer.  Overrides the base implementation to
        /// send the length of the array which is needed when it is retrieved </summary>
        public void WriteVarByteArray(byte[] b)
        {
            if (null == b || 0 >= b.Length)
            {
                WriteLength(0);
                return;
            }
            int len = b.Length;
            WriteLength(len);
            base.Write(b);
        }

        public void WriteVarByteArray(byte[] b, int length)
        {
            if (null == b || 0 >= length)
            {
                WriteLength(0);
                return;
            }
            int len = length;
            WriteLength(len);
            base.Write(b, 0, length);
        }

        public void WriteVarCharArray(char[] arr)
        {
            if (null == arr || 0 >= arr.Length)
            {
                WriteLength(0);
                return;
            }
            WriteLength(arr.Length);
            base.Write(arr);
        }

        /// <summary> Writes a string to the buffer.  Overrides the base implementation so it can cope with nulls </summary>
        public void WriteString(string str)
        {
            if (str == null)
            {
                WriteVarCharArray(null);
                return;
            }
            WriteVarCharArray(str.ToCharArray());
        }

        /// <summary> Writes a DateTime to the buffer. </summary>
        public void WriteDateTime(DateTime dt)
        {
            Write(dt.Ticks - s_DateTimeOffset);
        }

        internal void Write(Guid guid)
        {
            byte[] buffer = guid.ToByteArray();
            Write(buffer);
        }
    } // SerializationWriter


    /// <summary> SerializationReader.  Extends BinaryReader to add additional data types,
    /// handle null strings and simplify use with ISerializable. </summary>
    public class SerializationReader : BinaryReader
    {
        private static readonly long s_DateTimeOffset = (new DateTime(1601, 01, 01)).Ticks;
        private readonly bool m_closeStreamOnDispose;


        public SerializationReader(Stream input, bool closeStreamOnDispose)
            : base(input, Encoding.Unicode)
        {
            m_closeStreamOnDispose = closeStreamOnDispose;
        }

        public SerializationReader(Stream input)
            : this(input, false)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (m_closeStreamOnDispose)
            {
                base.Dispose(disposing);
            }
        }

        public int ReadLength()
        {
            return Read7BitEncodedInt();
        }

        /// <summary> Reads a byte array from the buffer, handling nulls and the array length. </summary>
        public byte[] ReadVarByteArray()
        {
            int len = ReadLength();
            if (len > 0)
                return ReadBytes(len);
            return new byte[0];
        }

        /// <summary> Reads a char array from the buffer, handling nulls and the array length. </summary>
        public char[] ReadVarCharArray()
        {
            int len = ReadLength();
            if (len > 0)
                return ReadChars(len);
            return new char[0];
        }

        /// <summary> Reads a string from the buffer.  Overrides the base implementation so it can cope with nulls. </summary>
        public override string ReadString()
        {
            int l = ReadLength();
            if (0 >= l)
            {
                return string.Empty;
            }
            return new string(base.ReadChars(l));
        }

        /// <summary> Reads a DateTime from the buffer. </summary>
        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64() + s_DateTimeOffset);
        }

        /// <summary>
        /// Reads a Guid from the buffer.
        /// </summary>
        public Guid ReadGuid()
        {
            byte[] guidBytes = ReadBytes(16);
            return new Guid(guidBytes);
        }

    } // SerializationReader

    public static class TaskVersionValidator
    {
        public enum AgentOperation
        {
            ActionControlAgent, // Control agent (uninstall, update...)
            DiscoveryMsiProductsList,
            TunnelingCommandPrompt,
            TunnelingRemoteDesktopConnection,
            EnhancedPowerActions,
            IncompatibleProductsAction,
            IncompatibleProductsDiscovery,
            RemoteAgentLog,
            FileOperations,
        };

        /// <summary>
        /// Check if agent on the target computer supports the operation
        /// and compatible to the current server
        /// </summary>
        /// <param name="pc">Target computer</param>
        /// <param name="operation">Operation</param>
        /// <param name="throwException">Throw exception if does not support</param>
        /// <returns></returns>
        public static bool CheckAgentSupportsOperation(IComputer pc, AgentOperation operation, bool throwException)
        {
            Version requiredVersion = null;
            switch (operation)
            {
                case AgentOperation.ActionControlAgent:
                case AgentOperation.TunnelingCommandPrompt:
                case AgentOperation.TunnelingRemoteDesktopConnection:
                case AgentOperation.EnhancedPowerActions:
                case AgentOperation.IncompatibleProductsAction:
                case AgentOperation.IncompatibleProductsDiscovery:
                    requiredVersion = new Version(1, 5);
                    break;
                case AgentOperation.DiscoveryMsiProductsList:
                    requiredVersion = new Version(1, 4);
                    break;
                case AgentOperation.RemoteAgentLog:
                    requiredVersion = new Version(1, 6);
                    break;
                case AgentOperation.FileOperations:
                    // TODO: Leaving it 2.1 for now. After Agent version is changed to 3.x the code below should also be changed accordingly.
                    requiredVersion = new Version(2, 1);
                    break;
                default:
                    break;
            }

            if (requiredVersion != null && (pc.AgentVersion == null || pc.AgentVersion < requiredVersion))
            {
                if (throwException)
                    throw new InvalidOperationException("Incompatible Agent version");

                return false;
            }

            return true;
        }


        /// <summary>
        /// Check if agent on the target computers support the operation
        /// and compatible to the current server
        /// </summary>
        /// <param name="computers">Target computers</param>
        /// <param name="operation">Operation</param>
        /// <param name="throwException">Throw exception if does not support</param>
        /// <returns></returns>
        public static bool CheckAgentSupportsOperation(IComputer[] computers, AgentOperation operation,
            bool throwException)
        {
            foreach (IComputer pc in computers)
            {
                if (!CheckAgentSupportsOperation(pc, operation, throwException))
                    return false;
            }
            return true;
        }


    }
}
