﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace REVUnit.AutoArknights.CLI.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("REVUnit.AutoArknights.CLI.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 所有任务已结束，请按任意键退出程序. . . .
        /// </summary>
        internal static string App_AllTasksCompleted {
            get {
                return ResourceManager.GetString("App_AllTasksCompleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CPU不支持AVX2指令集，无法运行本程序.
        /// </summary>
        internal static string App_NotSupported {
            get {
                return ResourceManager.GetString("App_NotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;模式&gt;[次数][后续动作].
        /// </summary>
        internal static string App_ParamsHint {
            get {
                return ResourceManager.GetString("App_ParamsHint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 即将执行以上任务，请按任意键继续. . . .
        /// </summary>
        internal static string App_ReadyToExecute {
            get {
                return ResourceManager.GetString("App_ReadyToExecute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 启动成功.
        /// </summary>
        internal static string App_Started {
            get {
                return ResourceManager.GetString("App_Started", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 正在初始化设备抽象层.
        /// </summary>
        internal static string App_Starting {
            get {
                return ResourceManager.GetString("App_Starting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 任务[{TaskId}]：开始.
        /// </summary>
        internal static string App_TaskBegin {
            get {
                return ResourceManager.GetString("App_TaskBegin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 任务[{TaskId}]完成：{Message}.
        /// </summary>
        internal static string App_TaskComplete {
            get {
                return ResourceManager.GetString("App_TaskComplete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 任务[{TaskId}]出现错误：{Message}.
        /// </summary>
        internal static string App_TaskFaulted {
            get {
                return ResourceManager.GetString("App_TaskFaulted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--------------------------------------------------
        ///.
        /// </summary>
        internal static string App_TaskListFooter {
            get {
                return ResourceManager.GetString("App_TaskListFooter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///-[任务列表]---------------------------------------
        ///
        ///.
        /// </summary>
        internal static string App_TaskListHeader {
            get {
                return ResourceManager.GetString("App_TaskListHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无法解析配置值 {0}.
        /// </summary>
        internal static string Config_Exception_CannotParse {
            get {
                return ResourceManager.GetString("Config_Exception_CannotParse", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 配置文件无效，请检查语法.
        /// </summary>
        internal static string Config_Exception_InvalidConfig {
            get {
                return ResourceManager.GetString("Config_Exception_InvalidConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无法解析 {0} 的值.
        /// </summary>
        internal static string Config_Exception_InvalidKey {
            get {
                return ResourceManager.GetString("Config_Exception_InvalidKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 配置文件需填写 {0}.
        /// </summary>
        internal static string Config_Exception_RequirementsUnmet {
            get {
                return ResourceManager.GetString("Config_Exception_RequirementsUnmet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] DefaultConfig {
            get {
                object obj = ResourceManager.GetObject("DefaultConfig", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 出现致命错误.
        /// </summary>
        internal static string Entry_FatalException {
            get {
                return ResourceManager.GetString("Entry_FatalException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无效的模式，请输入 &quot;help&quot; 来获取快速帮助.
        /// </summary>
        internal static string Plan_Exception_InvalidMode {
            get {
                return ResourceManager.GetString("Plan_Exception_InvalidMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无效的刷关次数值，请输入 &quot;help&quot; 来获取快速帮助.
        /// </summary>
        internal static string Plan_Exception_InvalidTimes {
            get {
                return ResourceManager.GetString("Plan_Exception_InvalidTimes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无效的计划，请输入 &quot;help&quot; 来获取快速帮助.
        /// </summary>
        internal static string Plan_Exception_Parsing {
            get {
                return ResourceManager.GetString("Plan_Exception_Parsing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///02    刷关2次
        ///011    刷关11次
        ///15    刷关5次，理智耗尽时等待恢复
        ///2    刷关到理智耗尽
        ///2es    刷关到理智耗尽，随后关闭模拟器，使计算机睡眠
        ///3    刷关到理智耗尽，无限等待恢复继续刷关
        ///
        ///后续动作 [c]关机 [r]重启 [s]睡眠 [h]休眠 [e]关闭远端
        ///.
        /// </summary>
        internal static string QuickHelpMessage {
            get {
                return ResourceManager.GetString("QuickHelpMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to                                                                                                     
        ///            @                      @@@#%%                                   #/                      
        ///            @@@###    @            @@@/ %%******=-                 @@@.    ##                       
        ///            @@@  @@@  @@@===       @@@/        (@@@                       @@@ @@                    
        ///            @@@  @@@  @@@  =#@@@   @@@/        (@@@        @(             @@@    (@*                
        ///   [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string StartupLogo {
            get {
                return ResourceManager.GetString("StartupLogo", resourceCulture);
            }
        }
    }
}
