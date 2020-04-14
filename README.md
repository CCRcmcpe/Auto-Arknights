# Auto Arknights
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FCCRcmcpe%2FAuto-Arknights.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FCCRcmcpe%2FAuto-Arknights?ref=badge_shield)  
用于自动化明日方舟部分流程的程序，基于OpenCV。  
~~感谢某不知名马姓人士提供的OCR服务~~
## 运行
### 必须运行要求
* .NET 5.0 Preview 1或以上的.NET版本。([官方下载](https://dotnet.microsoft.com/download/dotnet-core/5.0))
* CPU支持`AVX2`指令集且支持`x64`架构。
* 系统为`Windows`，版本为7及以上，目标架构为`x64`。
### [下载](https://github.com/CCRcmcpe/Auto-Arknights/releases/latest) //如果你没看必须运行要求建议再看一次。
## 构建
### 对于`Auto Arknights CLI`
1. 定位到项目根目录。  
2. 运行`build-winx64.bat`，构建成果在<code>bin\Release\\<i>[.NET版本]</i>\win-x64\publish</code>。
## 功能
尚处于开发阶段，GUI目前半残。
### 重复单关卡
功能|状态|注释
-|-|-
任意次|已实现|Mode.SpecifiedTimes
直到理智耗尽|已实现|Mode.UntilNoSanity
当理智耗尽时等待恢复|已实现|Mode.WaitWhileNoSanity
当理智耗尽时自动嗑药|计划中
当理智耗尽时自动碎石|计划中
当理智耗尽时自动嗑药或碎石|计划中
## 关于
开发者并没有很多经验，如果有问题欢迎提出`issue`。  
#### 本程序使用..
* `Feature Matching`进行精准图像识别、定位
* `OCR`进行文字识别
* `ADB`与明日方舟客户端交互