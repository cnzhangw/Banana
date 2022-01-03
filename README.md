# Banana  
Banana micro-orm framework,based on petapoco &amp; dapper

使用时，程序根目录需放置配置文件 config/banana.json ，必须的配置项如下：
  
{
  "debug": true,
  "log": true,
  "connection": "server=数据库连接",
  "provider": "mysql", // mysql|oracle|sqlite|sqlserver
}
