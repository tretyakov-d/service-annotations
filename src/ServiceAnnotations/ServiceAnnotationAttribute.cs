using System;

namespace ServiceAnnotations
{
    public abstract class ServiceAnnotationAttribute : Attribute
    {
        internal abstract void Apply(Type implementationType, IAnnotationContext context);
    }
}