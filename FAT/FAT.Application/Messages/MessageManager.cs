using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Threading;
using System.Windows;

namespace FAT.Application.Messages
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MessageReceiverAttribute : Attribute
    {
    }
    
    public class MessageManager 
    {
		class MessageContract : IEquatable<MessageContract>
        {
            readonly Type[] _parametersTypes;

            public MessageContract(Type[] parametersTypes)
            {
                _parametersTypes = parametersTypes;
            }

            public bool Equals(MessageContract other)
            {
                return other.GetHashCode() == GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (!(obj is MessageContract))
                {
                    return false;
                }
                return Equals(obj as MessageContract);
            }

            public override int GetHashCode()
            {
                string concat = _parametersTypes.Aggregate(string.Empty, (current, t) => string.Concat(current, t.FullName));

                return concat.GetHashCode();
            }
        }

        private object _sync = new object();

        static readonly Dictionary<Type, IList<WeakReference>> _typeAndVmMap;
        static readonly Dictionary<MessageContract, IList<Tuple<Type, MethodInfo>>> _messageContractAndTypeMethodMap;
        static readonly List<Type> _analyzedTypes;

        public static MessageManager Instance { get;  private set; }

        MessageManager()
        {
        }

        static MessageManager() 
        {
            _typeAndVmMap = new Dictionary<Type, IList<WeakReference>>();
            _messageContractAndTypeMethodMap = new Dictionary<MessageContract, IList<Tuple<Type, MethodInfo>>>();
            _analyzedTypes = new List<Type>();

            Instance = new MessageManager();
        }

	    public void RegisterListener(object obj) 
        {
			Type viewModelType = obj.GetType();

			lock (_sync)
            {
				if (!_analyzedTypes.Contains(viewModelType)) 
                {
					var methodAndContractMap = CollectMessagingContracts(viewModelType);
					if (methodAndContractMap.Count == 0)
						return;

					foreach (var info in methodAndContractMap.Keys) 
                    {
						var contract = methodAndContractMap[info];
						if (!_messageContractAndTypeMethodMap.ContainsKey(contract)) 
                            _messageContractAndTypeMethodMap[contract] = new List<Tuple<Type, MethodInfo>>();

						_messageContractAndTypeMethodMap[contract].Add(Tuple.Create(viewModelType, info));
					}
				}
				if (!_typeAndVmMap.ContainsKey(viewModelType)) 
					_typeAndVmMap[viewModelType] = new List<WeakReference>();

				_typeAndVmMap[viewModelType].Add(new WeakReference(obj));
				_analyzedTypes.Add(viewModelType);
			}
		}

		IDictionary<MethodInfo, MessageContract> CollectMessagingContracts(Type viewModelSourceType) 
        {
			var result = new Dictionary<MethodInfo, MessageContract>();

			var applicableMethods = viewModelSourceType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).
				Where(e => e.GetCustomAttributes(true).OfType<MessageReceiverAttribute>().Any() && e.ReturnType == typeof(void) && e.GetParameters().Length > 0).ToList();

			applicableMethods.ForEach(e => result.Add(e, new MessageContract(e.GetParameters().Select(p => p.ParameterType).ToArray())));

			return result;
		}

		public void PublishMessage(params object[] messageParameters) 
        {
			var inputContract = new MessageContract(messageParameters.Select(e => e.GetType()).ToArray());

			if (!_messageContractAndTypeMethodMap.ContainsKey(inputContract)) 
				return;

			foreach (var tuple in _messageContractAndTypeMethodMap[inputContract])
			    foreach (WeakReference viewModelReference in _typeAndVmMap[tuple.Item1])
			    {
			        if (viewModelReference.IsAlive && viewModelReference.Target != null)
			        {
			            var viewModel = viewModelReference.Target;

			            Action action = () => tuple.Item2.Invoke(viewModel, messageParameters);

			            if (viewModel is UIElement)
			                ((UIElement) viewModel).Dispatcher.BeginInvoke(action);
			            else ThreadPool.QueueUserWorkItem(e => action());
			        }
			    }
        }
	}
}