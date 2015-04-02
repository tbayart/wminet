using log4net;


namespace Log4NetTests
{
    class Log4NetTests
    {
        // http://stackoverflow.com/questions/308436/log4net-programmatically-specify-multiple-loggers-with-multiple-file-appenders

        static void Main(string[] args)
        {
            GlobalContext.Properties["EnvironmentId"] = "blah";
            ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log4net.Config.XmlConfigurator.Configure();
        }
    }
}
