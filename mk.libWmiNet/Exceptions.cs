using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace mk.libWmiNet
{
    [Serializable]
    public class WmiException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WmiException(string message = null, Exception inner = null)
            : base(message, inner)
        {
        }

        protected WmiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }


        private string GetAdditionalMessage()
        {
            if (null != InnerException)
            {
                if (InnerException is ManagementException)
                {
                    ManagementException x = (ManagementException)InnerException;
                    return x.ErrorCode + " : " + x.ErrorInformation;
                }
                else if (InnerException is COMException)
                {
                    COMException x = (COMException)InnerException;
                    return "Encountered COM Error 0x" + x.ErrorCode.ToString("X8") + x.Message;
                }
            }

            return string.Empty;
        }


        public override string ToString()
        {
            string x = GetAdditionalMessage();
            
            if (string.IsNullOrWhiteSpace(x))
            {
                return base.ToString();
            }

            return x + Environment.NewLine + base.ToString();
        }


        public string UnderlyingErrorMessage { get { return GetAdditionalMessage(); } }
    }


    [Serializable]
    public class WmiConnectionException : WmiException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public WmiConnectionException(uint code, string message = null, Exception inner = null)
            : this(message, inner)
        {
            m_errorCode = code;
        }

        public WmiConnectionException(string message = null, Exception inner = null)
            : base(message, inner)
        {
        }

        protected WmiConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public uint ErrorCode { get { return m_errorCode; } }


        private readonly uint m_errorCode = 0;
    }
}
