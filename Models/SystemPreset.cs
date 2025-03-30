using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace WinFlux.Models
{
    [Serializable]
    [XmlRoot("SystemPreset")]
    public class SystemPreset
    {
        public SystemPreset()
        {
            // Initialize all settings to avoid null reference exceptions
            PrivacySettings = new PrivacySettings();
            TelemetrySettings = new TelemetrySettings();
            PerformanceSettings = new PerformanceSettings();
            GameSettings = new GameSettings();
            AppSettings = new AppSettings();
            AppInstallerSettings = new AppInstallerSettings();
        }

        [XmlElement("PrivacySettings")]
        public PrivacySettings PrivacySettings { get; set; }
        
        [XmlElement("TelemetrySettings")]
        public TelemetrySettings TelemetrySettings { get; set; }
        
        [XmlElement("PerformanceSettings")]
        public PerformanceSettings PerformanceSettings { get; set; }
        
        [XmlElement("GameSettings")]
        public GameSettings GameSettings { get; set; }
        
        [XmlElement("AppSettings")]
        public AppSettings AppSettings { get; set; }
        
        [XmlElement("AppInstallerSettings")]
        public AppInstallerSettings AppInstallerSettings { get; set; }
    }

    [Serializable]
    [XmlType("PrivacySettings")]
    public class PrivacySettings
    {
        public PrivacySettings()
        {
            ToggleSettings = new SerializableDictionary<string, bool>();
        }

        [XmlElement("ToggleSettings")]
        public SerializableDictionary<string, bool> ToggleSettings { get; set; }
    }

    [Serializable]
    [XmlType("TelemetrySettings")]
    public class TelemetrySettings
    {
        public TelemetrySettings()
        {
            ToggleSettings = new SerializableDictionary<string, bool>();
            BlockedHosts = new List<string>();
        }

        [XmlElement("ToggleSettings")]
        public SerializableDictionary<string, bool> ToggleSettings { get; set; }
        
        [XmlArray("BlockedHosts")]
        [XmlArrayItem("Host")]
        public List<string> BlockedHosts { get; set; }
    }

    [Serializable]
    [XmlType("PerformanceSettings")]
    public class PerformanceSettings
    {
        public PerformanceSettings()
        {
            ToggleSettings = new SerializableDictionary<string, bool>();
            SliderValues = new SerializableDictionary<string, double>();
        }

        [XmlElement("ToggleSettings")]
        public SerializableDictionary<string, bool> ToggleSettings { get; set; }
        
        [XmlElement("SliderValues")]
        public SerializableDictionary<string, double> SliderValues { get; set; }
    }

    [Serializable]
    [XmlType("GameSettings")]
    public class GameSettings
    {
        public GameSettings()
        {
            ToggleSettings = new SerializableDictionary<string, bool>();
        }

        [XmlElement("ToggleSettings")]
        public SerializableDictionary<string, bool> ToggleSettings { get; set; }
    }

    [Serializable]
    [XmlType("AppSettings")]
    public class AppSettings
    {
        public AppSettings()
        {
            AppsToRemove = new List<string>();
        }

        [XmlArray("AppsToRemove")]
        [XmlArrayItem("App")]
        public List<string> AppsToRemove { get; set; }
    }

    [Serializable]
    [XmlType("AppInstallerSettings")]
    public class AppInstallerSettings
    {
        public AppInstallerSettings()
        {
            AppsToInstall = new SerializableDictionary<string, bool>();
            PreferredPackageManager = 0; // 0 = Winget, 1 = Chocolatey
        }

        [XmlElement("AppsToInstall")]
        public SerializableDictionary<string, bool> AppsToInstall { get; set; }
        
        [XmlElement("PreferredPackageManager")]
        public int PreferredPackageManager { get; set; }
    }

    [Serializable]
    [XmlRoot("Dictionary")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        public SerializableDictionary() : base() { }
        
        public SerializableDictionary(Dictionary<TKey, TValue> dictionary) : base(dictionary) { }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Item");
                
                reader.ReadStartElement("Key");
                TKey key = (TKey)Convert.ChangeType(reader.ReadContentAsString(), typeof(TKey));
                reader.ReadEndElement();
                
                reader.ReadStartElement("Value");
                TValue value = (TValue)Convert.ChangeType(reader.ReadContentAsString(), typeof(TValue));
                reader.ReadEndElement();
                
                Add(key, value);
                
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            
            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (var key in Keys)
            {
                writer.WriteStartElement("Item");
                
                writer.WriteStartElement("Key");
                writer.WriteString(key.ToString());
                writer.WriteEndElement();
                
                writer.WriteStartElement("Value");
                writer.WriteString(this[key].ToString());
                writer.WriteEndElement();
                
                writer.WriteEndElement();
            }
        }
    }
} 