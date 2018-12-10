# SCE
SCE系统是一款基于ASP.Net Core的在线考评、投票管理系统

## 开发

SCE系统最初为东北大学软件学院开发。在开发结束后，考虑到项目具有较高的可重用性，且代码简明整洁，具有较高的可扩展开发性，特对其开源。

## 如何运行

运行代码前需要安装下列组件：  

    git
    dotnet core sdk

下载项目后，在./src/SCE目录依次执行：  

    dotnet restore
    dotnet ef database update
    dotnet run

这将会下载后端依赖项、更新数据库、并运行项目。