﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace OngekiFumenEditor.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.2.0.0")]
    public sealed partial class LogSetting : global::System.Configuration.ApplicationSettingsBase {
        
        private static LogSetting defaultInstance = ((LogSetting)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new LogSetting())));
        
        public static LogSetting Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\Logs")]
        public string LogFileDirPath {
            get {
                return ((string)(this["LogFileDirPath"]));
            }
            set {
                this["LogFileDirPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(".\\Dumps")]
        public string DumpFileDirPath {
            get {
                return ((string)(this["DumpFileDirPath"]));
            }
            set {
                this["DumpFileDirPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IsFullDump {
            get {
                return ((bool)(this["IsFullDump"]));
            }
            set {
                this["IsFullDump"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool IsNotifyUserCrash {
            get {
                return ((bool)(this["IsNotifyUserCrash"]));
            }
            set {
                this["IsNotifyUserCrash"] = value;
            }
        }
    }
}
