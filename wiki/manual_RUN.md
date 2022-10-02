## 写在前面

本教程理论上适用于Windows、Linux、MacOS，仅在win11机器上测试过

# Windows版本教程
## 1. 下载源代码
下载`Release`中与您的设备相符的版本

## 2. 修改配置文件
修改配置文件`appsettings.json`
若没有此文件，可新建
并按照配置文件内的说明修改配置
> `appsettings.json`说明参见：[appsettings.json](appsettings.md)

## 3. 启动
双击`exe`文件启动即可

# Linux Or MacOS 版本教程

## 一、通过docker启动
>注意：以下命令皆以`root`用户身份编写，如使用的非`root`用户，请使用`sudo su`切换为`root`身份。
### 1. 首先安装docker
`海外机`直接使用以下命令安装
```
curl -fsSL https://get.docker.com | bash
```
`国内机`使用`阿里云镜像仓库`安装docker
```
curl -fsSL https://get.docker.com | bash -s docker --mirror Aliyun
```
`国内机`使用`AZ镜像仓库`安装docker
```
curl -fsSL https://get.docker.com | bash -s docker --mirror AzureChinaCloud
```
### 2. 拉取源代码
拉取`指定版本`源代码
```
cd ~
mkdir code
cd code
git clone -b v1.0.0 https://github.com/LightCountry/CoinConvertBot.git
```
或者拉取`最新版本`源代码
```
git clone https://github.com/LightCountry/CoinConvertBot.git
```
### 3. 修改配置文件
```
cd ~/code/CoinConvertBot/src/Telegram.CoinConvertBot
#重命名配置文件
mv appsettings.Example.json appsettings.json
# 可使用vi命令修改配置文件
# 或者通过ftp工具修改
# 或者在宝塔面板的文件功能内找到配置文件修改
vi appsettings.json
```
### 3. 打包docker镜像
```
cd ~/code/CoinConvertBot/src
docker build -t coin-convert-bot:latest .
```
### 4. 启动docker容器
```
docker run -itd --name coin-convert-bot coin-convert-bot:latest
```
如果需要查看日志，可以使用以下命令
```
docker logs coin-convert-bot -f
```
### 5. 其他命令
```
# 停止容器
docker stop coin-convert-bot
# 删除容器（需要先执行停止容器才能执行删除）
docker rm coin-convert-bot
```

## 二、通过docker-compose启动

### 1-3步骤与`通过docker启动`的教程一致

### 4. 安装docker-compose
可通过以下教程安装`docker-compose`
 1. [菜鸟教程 docker-compose 安装](https://www.runoob.com/docker/docker-compose.html)
 2. [Github docker-compose 开源仓库](https://github.com/docker/compose/releases)

### 5. 启动
```
cd ~/code/CoinConvertBot/src
docker-compose up -d
```
如果修改了配置文件，需要重新编译再启动
```
docker-compose up -d --build
```
如果需要查看日志，可以使用以下命令
```
docker-compose logs -f app
```
### 6. 其他命令
```
# 停止
docker-compose stop
# 删除（未停止也可以直接删除）
docker-compose down
```
