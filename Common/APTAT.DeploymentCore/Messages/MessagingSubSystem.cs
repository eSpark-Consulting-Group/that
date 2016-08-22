using System.Collections.Generic;
using System.Globalization;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Messages
{
    /// <summary>
    /// Contains translators of messages from different message sources. 
    /// </summary>
    public interface IMessageTranslatorCollection
    {
        /// <summary>
        /// Gets  a IMessageTranslator instance by its message source name
        /// </summary>
        /// <param name="messageSourceName">message source name</param>
        /// <returns>a IMessageTranslator instance or null</returns>
        IMessageTranslator this[string messageSourceName] { get; }

        ///// <summary>
        ///// Gets translated text by resource id of the message source.
        ///// </summary>
        ///// <param name="messageSource">message source name</param>
        ///// <param name="resourceId">resource identificator of the message source</param>
        ///// <returns>translated text or null.</returns>
        //string Translate(string messageSource, string resourceId);
    }

    /// <summary>
    /// Provides methods which translates messages received from the message source 
    /// to the language of current UI culture   
    /// </summary>
    public interface IMessageTranslator
    {
        /// <summary>
        /// Gets a name of the message source
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// Gets translated text of current UI culture by resource id of the message source.
        /// </summary>
        /// <param name="resourceId">resource identificator of the message source</param>
        /// <returns>translated text or null.</returns>
        string Translate(string resourceId);

        string Translate(string resourceId, CultureInfo cultureInfo);
    }

    public class MessageTranslatorCollection : IMessageTranslatorCollection
    {

        private readonly IDictionary<string, IMessageTranslator> _messageTranslators =
            new Dictionary<string, IMessageTranslator>();

        private static readonly MessageTranslatorCollection _instance = new MessageTranslatorCollection();

        private MessageTranslatorCollection()
        {
            this.Add(new ServerMessageTranslator());
        }


        public static MessageTranslatorCollection Instance
        {
            get { return _instance; }
        }

        #region IMessageTranslatorCollection Members

        public IMessageTranslator this[string messageSourceName]
        {
            get
            {
                IMessageTranslator translator = null;
                _messageTranslators.TryGetValue(messageSourceName, out translator);
                return translator;
            }
        }

        #endregion

        public void Add(IMessageTranslator messageTranslator)
        {
            _messageTranslators.Add(messageTranslator.SourceName, messageTranslator);
        }

        public void Clear()
        {
            _messageTranslators.Clear();
        }
    }

    internal class ServerMessageTranslator : IMessageTranslator
    {
        #region IMessageTranslator Members

        public string SourceName
        {
            get { return ServerSideMessagesResources.SourceName; }
        }

        public string Translate(string resourceId)
        {
            return ServerSideMessagesResources.GetString(resourceId);
        }

        public string Translate(string resourceId, CultureInfo cultureInfo)
        {
            return ServerSideMessagesResources.GetString(resourceId, cultureInfo);
        }

        #endregion
    }
}
