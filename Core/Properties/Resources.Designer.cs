﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace REVUnit.AutoArknights.Core.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("REVUnit.AutoArknights.Core.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to 连接设备成功.
        /// </summary>
        internal static string Adb_Connected {
            get {
                return ResourceManager.GetString("Adb_Connected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 正在连接到 ADB 设备 {target}.
        /// </summary>
        internal static string Adb_Connecting {
            get {
                return ResourceManager.GetString("Adb_Connecting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 检测到设备状态: &quot;{state}&quot;.
        /// </summary>
        internal static string Adb_DeviceState {
            get {
                return ResourceManager.GetString("Adb_DeviceState", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 连接失败，重新启动 ADB 服务器后重试（第 {0}/{1} 次）.
        /// </summary>
        internal static string Adb_Exception_Connect {
            get {
                return ResourceManager.GetString("Adb_Exception_Connect", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 重试失败，无法连接.
        /// </summary>
        internal static string Adb_Exception_ConnectFailed {
            get {
                return ResourceManager.GetString("Adb_Exception_ConnectFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 发生错误，标准错误流输出：{0}.
        /// </summary>
        internal static string Adb_Exception_Execute {
            get {
                return ResourceManager.GetString("Adb_Exception_Execute", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无法获取设备分辨率信息.
        /// </summary>
        internal static string Adb_Exception_GetResolution {
            get {
                return ResourceManager.GetString("Adb_Exception_GetResolution", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 截图失败，正在重试（第 {0}/{1} 次）.
        /// </summary>
        internal static string Adb_Exception_GetScreenshot {
            get {
                return ResourceManager.GetString("Adb_Exception_GetScreenshot", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 截图失败.
        /// </summary>
        internal static string Adb_Exception_GetScreenshotFailed {
            get {
                return ResourceManager.GetString("Adb_Exception_GetScreenshotFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ADB 服务器未能正常启动.
        /// </summary>
        internal static string Adb_Exception_StartServer {
            get {
                return ResourceManager.GetString("Adb_Exception_StartServer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 正在执行 ADB 指令：{$param}.
        /// </summary>
        internal static string Adb_ExecutingCommand {
            get {
                return ResourceManager.GetString("Adb_ExecutingCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 正在启动 ADB 服务器.
        /// </summary>
        internal static string Adb_StartingServer {
            get {
                return ResourceManager.GetString("Adb_StartingServer", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无法载入资源: {0}.
        /// </summary>
        internal static string AssetsLoadException {
            get {
                return ResourceManager.GetString("AssetsLoadException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 任务结束.
        /// </summary>
        internal static string ExecuteResult_Ended {
            get {
                return ResourceManager.GetString("ExecuteResult_Ended", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 尝试了{0}次也未能达成{1}操作.
        /// </summary>
        internal static string ExecuteResult_MaxRetryReached {
            get {
                return ResourceManager.GetString("ExecuteResult_MaxRetryReached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 任务完成.
        /// </summary>
        internal static string ExecuteResult_Success {
            get {
                return ResourceManager.GetString("ExecuteResult_Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 当前理智[{CurrentSanity}]，需要理智[{RequiredSanity}].
        /// </summary>
        internal static string FarmLevel_CurrentSanity {
            get {
                return ResourceManager.GetString("FarmLevel_CurrentSanity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 必须提供缓存保存文件夹才能使用缓存.
        /// </summary>
        internal static string FeatureDetector_Exception_NoCachePath {
            get {
                return ResourceManager.GetString("FeatureDetector_Exception_NoCachePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}和{1}的类型不匹配.
        /// </summary>
        internal static string FeatureMatcher_Exception_FeatureTypesMismatch {
            get {
                return ResourceManager.GetString("FeatureMatcher_Exception_FeatureTypesMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 未检测到代理指挥正常运行迹象！.
        /// </summary>
        internal static string LevelFarming_Exception_AutoDeploy {
            get {
                return ResourceManager.GetString("LevelFarming_Exception_AutoDeploy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 请检查是否在正常代理作战，如果正常，请增加检测代理正常前等待的时间（现在为{WaitTime}s），以避免假警告出现.
        /// </summary>
        internal static string LevelFarming_Exception_AutoDeployHint {
            get {
                return ResourceManager.GetString("LevelFarming_Exception_AutoDeployHint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 刷关{0}次.
        /// </summary>
        internal static string LevelFarming_Mode_SpecifiedTimes {
            get {
                return ResourceManager.GetString("LevelFarming_Mode_SpecifiedTimes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 刷关{0}次，当理智不足以完成时等待恢复.
        /// </summary>
        internal static string LevelFarming_Mode_SpecTimesWithWait {
            get {
                return ResourceManager.GetString("LevelFarming_Mode_SpecTimesWithWait", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 刷关直到理智耗尽.
        /// </summary>
        internal static string LevelFarming_Mode_UntilNoSanity {
            get {
                return ResourceManager.GetString("LevelFarming_Mode_UntilNoSanity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 刷关直到手动结束程序.
        /// </summary>
        internal static string LevelFarming_Mode_WaitWhileNoSanity {
            get {
                return ResourceManager.GetString("LevelFarming_Mode_WaitWhileNoSanity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 检测到此关卡需要[{RequiredSanity}]理智.
        /// </summary>
        internal static string LevelFarming_RequiredSanity {
            get {
                return ResourceManager.GetString("LevelFarming_RequiredSanity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ...理智恢复完成.
        /// </summary>
        internal static string LevelFarming_SanityRecovered {
            get {
                return ResourceManager.GetString("LevelFarming_SanityRecovered", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 开始第[{CurrentTimes}/{times}]次刷关.
        /// </summary>
        internal static string LevelFarming_SpecifiedTimes_Begin {
            get {
                return ResourceManager.GetString("LevelFarming_SpecifiedTimes_Begin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 关卡完成，目前已刷关[{currentTimes}/{Times}]次.
        /// </summary>
        internal static string LevelFarming_SpecifiedTimes_Complete {
            get {
                return ResourceManager.GetString("LevelFarming_SpecifiedTimes_Complete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 成功刷关{0}次.
        /// </summary>
        internal static string LevelFarming_Success {
            get {
                return ResourceManager.GetString("LevelFarming_Success", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 开始第{CurrentTimes}次刷关.
        /// </summary>
        internal static string LevelFarming_Unlimited_Begin {
            get {
                return ResourceManager.GetString("LevelFarming_Unlimited_Begin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 关卡完成，目前已刷关{CurrentTimes}次.
        /// </summary>
        internal static string LevelFarming_Unlimited_Complete {
            get {
                return ResourceManager.GetString("LevelFarming_Unlimited_Complete", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 正在等待理智恢复....
        /// </summary>
        internal static string LevelFarming_WaitingForSanityRecovery {
            get {
                return ResourceManager.GetString("LevelFarming_WaitingForSanityRecovery", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无效的后续操作标识符 &quot;{0}&quot;.
        /// </summary>
        internal static string PostAction_Exception_ParseFailed {
            get {
                return ResourceManager.GetString("PostAction_Exception_ParseFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 执行指令：{0}....
        /// </summary>
        internal static string PostAction_ExecuteCommand {
            get {
                return ResourceManager.GetString("PostAction_ExecuteCommand", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 无法启动cmd.
        /// </summary>
        internal static string PostAction_ExecuteCommand_CannotStartCmd {
            get {
                return ResourceManager.GetString("PostAction_ExecuteCommand_CannotStartCmd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指令已执行.
        /// </summary>
        internal static string PostAction_ExecuteCommand_Completed {
            get {
                return ResourceManager.GetString("PostAction_ExecuteCommand_Completed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指令超时.
        /// </summary>
        internal static string PostAction_ExecuteCommand_Exception_Timeout {
            get {
                return ResourceManager.GetString("PostAction_ExecuteCommand_Exception_Timeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 重启.
        /// </summary>
        internal static string PostAction_Reboot {
            get {
                return ResourceManager.GetString("PostAction_Reboot", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 关机.
        /// </summary>
        internal static string PostAction_Shutdown {
            get {
                return ResourceManager.GetString("PostAction_Shutdown", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 系统未开启或不支持休眠.
        /// </summary>
        internal static string PostAction_Suspend_Exception_HibernateNotSupported {
            get {
                return ResourceManager.GetString("PostAction_Suspend_Exception_HibernateNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 强制.
        /// </summary>
        internal static string PostAction_Suspend_Forced {
            get {
                return ResourceManager.GetString("PostAction_Suspend_Forced", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 休眠.
        /// </summary>
        internal static string PostAction_Suspend_Hibernate {
            get {
                return ResourceManager.GetString("PostAction_Suspend_Hibernate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 睡眠.
        /// </summary>
        internal static string PostAction_Suspend_Sleep {
            get {
                return ResourceManager.GetString("PostAction_Suspend_Sleep", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 指令不能为空.
        /// </summary>
        internal static string PostActions_ExecuteCommand_Exception_EmptyCommand {
            get {
                return ResourceManager.GetString("PostActions_ExecuteCommand_Exception_EmptyCommand", resourceCulture);
            }
        }
    }
}
