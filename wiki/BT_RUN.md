# 宝塔运行

### 1. 下载release对应平台的包，解压到指定目录
### 2. 重命名`appsettings.Example.json`为`appsettings.json`，并修改配置文件，若没有此文件，可自行新建
> `appsettings.json`说明参见：[appsettings.json](appsettings.md)
### 3. 为二进制文件`Telegram.CoinConvertBot`增加可执行权限
### 4. `宝塔应用管理器`或`Supervisor管理器`添加应用
> 应用名称：CoinConvertBot  
> 应用环境：无 （`Supervisor管理器`无此项）  
> 执行目录：/xxx (你解压文件的目录)  
> 启动文件：/xxx/Telegram.CoinConvertBot  
> 如有其他选项保持默认  