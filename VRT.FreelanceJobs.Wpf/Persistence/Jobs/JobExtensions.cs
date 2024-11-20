using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;

namespace VRT.FreelanceJobs.Wpf.Persistence.Jobs;

internal static class JobExtensions
{
    private static readonly IReadOnlyDictionary<string, PropertyInfo> JobProperties = typeof(Job)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty)
        .ToFrozenDictionary(k => k.Name, v => v);
    public static bool Update<T>(this Job job, Expression<Func<Job, T>> property, T newValue)
    {
        var body = (MemberExpression)property.Body;
        var propertyName = body.Member.Name;

        if (JobProperties.TryGetValue(propertyName, out var propInfo) is false)
        {
            return false;
        }

        var currentValue = (T?)propInfo.GetValue(job);
        if ((currentValue is null && newValue is null) || Equals(currentValue, newValue))
        {
            return false;
        }
        propInfo.SetValue(job, newValue);
        return true;
    }
    public static bool Update<T>(this Job job, Expression<Func<Job, T[]>> property, T[] newValue)
    {
        var body = (MemberExpression)property.Body;
        var propertyName = body.Member.Name;

        if (JobProperties.TryGetValue(propertyName, out var propInfo) is false)
        {
            return false;
        }

        var currentValue = (T[]?)propInfo.GetValue(job);
        if ((currentValue is null && newValue is null) || Enumerable.SequenceEqual(currentValue!, newValue))
        {
            return false;
        }
        propInfo.SetValue(job, newValue);
        return true;
    }
}
