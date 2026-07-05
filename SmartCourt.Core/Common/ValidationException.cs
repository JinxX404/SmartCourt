using System;
using System.Collections.Generic;

namespace SmartCourt.Core.Common;

public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<KeyValuePair<string, string[]>> failures)
        : this()
    {
        foreach (var failure in failures)
        {
            Errors.Add(failure.Key, failure.Value);
        }
    }

    public ValidationException(string propertyName, string errorMessage)
        : this()
    {
        Errors.Add(propertyName, new[] { errorMessage });
    }
}
