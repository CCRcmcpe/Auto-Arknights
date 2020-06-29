# Auto Arknights
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FCCRcmcpe%2FAuto-Arknights.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FCCRcmcpe%2FAuto-Arknights?ref=badge_shield)  
*用于自动化明日方舟刷关的程序。*
## 运行
### 必须运行要求
* 最新版的 `.NET 5.0 Preview`。([官方下载](https://dotnet.microsoft.com/download/dotnet-core/5.0))
* CPU支持`AVX2`指令集且支持`x64`架构。
* 系统为`Windows`，版本为7及以上，目标架构为`x64`。

确保你的电脑满足运行要求后，[下载](https://github.com/CCRcmcpe/Auto-Arknights/releases/latest)并运行
### 如何使用
0. （第一次运行）本程序需要连接目标设备的`ADB`，运行前编辑`Config.json`更改`ADB:Remote`至你想要的地址即可。  
各大模拟器的`ADB`地址不同，请自行搜索。此外，也可以连接开启了局域网调试的手机等设备。
1. 打开作战界面（带作战开始按钮的），并确保你勾选了代理作战。
2. 选择模式然后运行，各个模式的功能在下面有介绍。
## 功能
### 注意事项
1. **目前暂不支持剿灭作战！**
2. 如果刷关过程中博士等级升级了会卡住
### 重复单关卡
功能|说明
-|-
任意次|`Mode.SpecifiedTimes`，请注意如果理智不够会卡住
任意次，当理智耗尽时等待恢复|`Mode.SpecTimesWithWait`，功能同上且不会出现上列问题
直到理智耗尽|`Mode.UntilNoSanity`
当理智耗尽时等待恢复|`Mode.WaitWhileNoSanity`
## 构建
1. 切换到项目根目录。  
2. 运行`build-winx64.bat`，构建成果在`artifact/`。
## 关于
开发者并没有很多经验，如果有问题欢迎提出`issue`。
