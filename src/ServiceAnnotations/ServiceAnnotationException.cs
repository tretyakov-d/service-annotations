using System;

namespace ServiceAnnotations
{
    public class ServiceAnnotationException : Exception
    {
        public string Code { get; }
        public string Description { get; }
        public string Tip { get; }
        
        public ServiceAnnotationException(string errorCode, string description, string tip = "", Exception innerException = null)
            : base(string.Concat(errorCode, ":", description), innerException)
        {
            Code = errorCode;
            Description = description;
            Tip = tip;
        }
    }
}