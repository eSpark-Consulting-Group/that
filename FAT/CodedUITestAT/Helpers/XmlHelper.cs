using System.IO;
using System.Xml;

namespace CodedUITestFAT
{
    /// <summary> Contains methods for work with xml-files. </summary>
    internal static class XmlHelper
    {
        /// <summary> Sets an existing setting value to the application configuration file. </summary>
        /// <param name="pathToApp">Set a path to the application.</param>
        /// <param name="settingNameAttributeValue">Set a setting name attribute value.</param>
        /// <param name="newSettingValue">Set a new setting value.</param>
        internal static void SetConfigSetting(string pathToApp, string settingNameAttributeValue, string newSettingValue)
        {
            string configXmlPath = Path.Combine(pathToApp, DataHelper.ProductExeName + ".config");
            string formatedNewSettingValue = string.Format(@"<{0}>{1}</{2}>", Settings.Configuration.Default.ConfigSettingElementValue, newSettingValue,
                                                                                                        Settings.Configuration.Default.ConfigSettingElementValue);
            XmlDocument configXml = new XmlDocument();
            configXml.Load(configXmlPath);

            XmlNode settings = configXml.SelectSingleNode(Settings.Configuration.Default.ConfigSettingsXPath);
            XmlNodeList settingsList = settings.ChildNodes;
            foreach (XmlNode setting in settingsList)
                if (setting.Attributes[Settings.Configuration.Default.ConfigSettingAttributeName].Value == settingNameAttributeValue)
                                                                                                                            setting.InnerXml = formatedNewSettingValue;
            configXml.Save(configXmlPath);
        }

        /// <summary> Adds a new setting value to the product configuration file. </summary>
        /// <param name="pathToConfig">Set a path to the product config file.</param>
        /// <param name="settingNameAttributeValue">Set a setting name attribute value.</param>
        /// <param name="settingValue">Set a new setting value.</param>
        internal static void AddConfigSetting(string pathToConfig, string settingNameAttributeValue, string settingValue)
        {
            string configXmlPath = Path.Combine(pathToConfig, DataHelper.ProductExeName + ".config");
            XmlDocument configToolXml = new XmlDocument();
            configToolXml.Load(configXmlPath);

            XmlNode settings = configToolXml.SelectSingleNode(Settings.Configuration.Default.ConfigSettingsXPath);

            XmlElement newSetting = configToolXml.CreateElement(Settings.Configuration.Default.ConfigSettingElementSetting);
            newSetting.SetAttribute(Settings.Configuration.Default.ConfigSettingAttributeName, settingNameAttributeValue);
            newSetting.SetAttribute(Settings.Configuration.Default.ConfigSettingAttributeSerialize, "String");
            settings.AppendChild(newSetting);

            XmlElement newSettingInnerValue = configToolXml.CreateElement(Settings.Configuration.Default.ConfigSettingElementValue);
            newSettingInnerValue.InnerText = settingValue;
            newSetting.AppendChild(newSettingInnerValue);

            configToolXml.Save(configXmlPath);
        }
    }
}
