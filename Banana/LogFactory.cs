using System;
using System.IO;

using log4net;
using log4net.Config;
using log4net.Repository;

namespace Banana
{
    // 日志解耦参考 https://github.com/fengjb/Logging

    public sealed class LogFactory
    {
        private readonly ILog logInstance = null;

#if NETSTANDARD2_0
        public static ILoggerRepository logRepository = null;
#endif

        static LogFactory()
        {
            FileInfo configFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config\\log4net.config"));
            if (!configFile.Exists)
            {
                if (Config.Debug)
                {
                    throw new FileNotFoundException($"未找到配置文件：/config/log4net.config");
                }

                return;
            }
#if NETSTANDARD2_0
            logRepository = LogManager.CreateRepository("BANANA_NETCORE_LOGREPOSITORY");
            XmlConfigurator.Configure(logRepository, configFile);
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
            if (logRepository == null)
            {
                if (Config.Debug)
                {
                    throw new NullReferenceException("Repository不能为空");
                }

                return new LogFactory(null);
            }

            return new LogFactory(LogManager.GetLogger(logRepository.Name, type));
#else
                return new LogFactory(log4net.LogManager.GetLogger(type: type));
#endif
        }

        public static LogFactory GetLogger(string name)
        {
#if NETSTANDARD2_0
            if (logRepository == null)
            {
                throw new NullReferenceException("Repository不能为空");
            }

            return new LogFactory(log4net.LogManager.GetLogger(logRepository.Name, name));
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
            if (logInstance != null && logInstance.IsFatalEnabled)
            {
                logInstance.Fatal(message, exception);
            }

            return this;
        }

        /// <summary>
        /// 一般错误
        /// </summary>
        public LogFactory Error(Exception exception, object message = null)
        {
            if (logInstance != null && logInstance.IsErrorEnabled)
            {
                logInstance.Error(message, exception);
            }

            return this;
        }

        /// <summary>
        /// 警告
        /// </summary>
        public LogFactory Warn(object message, Exception exception = null)
        {
            if (logInstance != null && logInstance.IsWarnEnabled)
            {
                logInstance.Warn(message, exception);
            }

            return this;
        }

        /// <summary>
        /// 一般信息
        /// </summary>
        public LogFactory Info(object message, Exception exception = null)
        {
            if (logInstance != null && logInstance.IsInfoEnabled)
            {
                logInstance.Info(message, exception);
            }

            return this;
        }

        /// <summary>
        /// 调试信息
        /// </summary>
        public LogFactory Debug(object message, Exception exception = null)
        {
            if (logInstance != null && logInstance.IsDebugEnabled)
            {
                logInstance.Debug(message, exception);
            }

            return this;
        }
    }

}
