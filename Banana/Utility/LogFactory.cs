using log4net;
using log4net.Config;
using log4net.Repository;

using log4net.Core;
using System;
using System.IO;

namespace Banana.Utility
{
    public sealed class LogFactory
    {
        private readonly log4net.ILog logInstance = null;
#if NETSTANDARD2_0
        public static ILoggerRepository Repository
        {
            get; private set;
        }
#endif

        static LogFactory()
        {
            FileInfo configFile = new FileInfo($"{AppDomain.CurrentDomain.BaseDirectory}/config/log4net.config");
            if (!configFile.Exists)
            {
                //throw new FileNotFoundException($"未找到配置文件：/config/log4net.config");
                return;
            }
#if NETSTANDARD2_0
            Repository = LogManager.CreateRepository("Banana_NETCore_Log4net_Repository");
            XmlConfigurator.Configure(Repository, configFile);
#else
            log4net.Config.XmlConfigurator.Configure(configFile);
#endif
        }

        private LogFactory(ILog log)
        {
            logInstance = log;
        }

        /// <summary>
        /// [推荐使用]
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LogFactory GetLogger(Type type)
        {
#if NETSTANDARD2_0
            return new LogFactory(LogManager.GetLogger(Repository.Name, type));
#else
            return new LogFactory(log4net.LogManager.GetLogger(type: type));
#endif
        }

        public static LogFactory GetLogger(string name)
        {
#if NETSTANDARD2_0
            return new LogFactory(log4net.LogManager.GetLogger(Repository.Name, name));
#else
            return new LogFactory(log4net.LogManager.GetLogger(name));
#endif
        }
        /// <summary>
        /// 建议记录Debug/Info/Warn等级别信息，[customLogger]
        /// </summary>
        /// <returns></returns>
        public static LogFactory GetLogger()
        {
            return GetLogger(name: "customLogger");
        }

        /// <summary>
        /// 致命错误
        /// </summary>
        public LogFactory Fatal(Exception exception, object message = null)
        {
            if (logInstance.IsFatalEnabled)
                logInstance.Fatal(message, exception);

            return this;
        }

        /// <summary>
        /// 一般错误
        /// </summary>
        public LogFactory Error(Exception exception, object message = null)
        {
            if (logInstance.IsErrorEnabled)
                logInstance.Error(message, exception);

            return this;
        }

        /// <summary>
        /// 警告
        /// </summary>
        public LogFactory Warn(object message, Exception exception = null)
        {
            if (logInstance.IsWarnEnabled)
                logInstance.Warn(message, exception);

            return this;
        }

        /// <summary>
        /// 一般信息
        /// </summary>
        public LogFactory Info(object message, Exception exception = null)
        {
            if (logInstance.IsInfoEnabled)
                logInstance.Info(message, exception);

            return this;
        }

        /// <summary>
        /// 调试信息
        /// </summary>
        public LogFactory Debug(object message, Exception exception = null)
        {
            if (logInstance.IsDebugEnabled)
                logInstance.Debug(message, exception);

            return this;
        }
    }
}
