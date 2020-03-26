# Auto Arknights
`咕咕咕`  
用于自动化明日方舟部分流程的程序，使用OpenCV进行图像识别、定位（识别按钮位置等），使用百度OCR API进行文字识别（获取理智和其他信息），使用ADB与明日方舟客户端交互。
## 运行
### 必须运行要求
* [.NET 5.0 Preview 1](https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-desktop-5.0.0-preview.1-windows-x64-installer)或以上的.NET版本。
* CPU支持`AVX2`指令集且支持`x64`架构。
* 系统为`Windows`，版本为7及以上，目标架构为`x64`。
### [下载](https://github.com/CCRcmcpe/Auto-Arknights/releases/latest) //如果你没看必须运行要求建议再看一次。
## 构建
注意：`dev`分支不能构建实属正常。
### 对于`Auto Arknights CLI`
运行其项目根目录下的`build.bat`。
## 功能
尚处于开发阶段，GUI目前半残。
+ [x] 重复单关卡任意次
+ [ ] 重复单关卡直到理智耗尽
+ [ ] 重复单关卡，在理智耗尽时等待恢复理智，恢复完毕后继续

## 关于
开发者并没有很多经验，如果有问题欢迎提出`issue`。
