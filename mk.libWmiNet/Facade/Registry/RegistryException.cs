using System;
using System.Runtime.Serialization;


namespace mk.libWmiNet.Facade.Registry
{
    [Serializable]
    public class RegistryException : WmiException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RegistryException(uint code, string message, Exception inner = null) : base(message, inner)
        {
            ErrorCode = code;
            switch (code)
            {
                case 2:
                    ErrorDescription = "Key not found.";
                    break;
                case 5:
                    ErrorDescription = "Permission Denied.";
                    break;
            }
        }

        protected RegistryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public uint ErrorCode { get; private set; }
        public string ErrorDescription { get ; private set; }
    }
}
