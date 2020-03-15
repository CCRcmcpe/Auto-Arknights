# Auto Arknights
用于自动化明日方舟部分流程的程序，使用OpenCV进行图像识别、定位（识别按钮位置等），使用百度OCR API进行文字识别（获取理智和其他信息），使用ADB与明日方舟客户端交互。
## 构建
注意：目前尚处于开发阶段，`clone`下来不能构建实属正常。
### 对于`Auto Arknights Console`
#### Windows
运行其项目根目录下的`build.bat`。
#### Linux
在项目根目录下运行：  
`dotnet publish -c Release -r [自己的Runtime]`  
[自己的Runtime]: 请查阅 https://docs.microsoft.com/en-us/dotnet/core/rid-catalog
## 功能
已实现:
* 重复单关卡任意次

暂未实现：
* 重复单关卡直到理智耗尽
* 重复单关卡，在理智耗尽时等待恢复理智，恢复完毕后继续

## 关于
开发者并没有很多经验，如果有问题欢迎提出`issue`。