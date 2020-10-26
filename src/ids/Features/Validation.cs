using System;
using System.ComponentModel.DataAnnotations;

namespace Ids
{
    public class GuidAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is string s && Guid.TryParse(s, out var guid);
        }
    }
}