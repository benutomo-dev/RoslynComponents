﻿#nullable enable

using System;

namespace Benutomo
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class UncancelableAttribute : Attribute
    {
    }
}
