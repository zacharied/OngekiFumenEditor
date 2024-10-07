﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using OngekiFumenEditor.Properties;
using OngekiFumenEditor.Utils;

namespace OngekiFumenEditor.Kernel.ArgProcesser.Attributes
{
    public abstract class OptionBindingAttrbuteBase : Attribute
    {
        public OptionBindingAttrbuteBase(string name, string description, object defaultValue, Type type)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
            Type = type;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string HelpArgName { get; set; }
        public object DefaultValue { get; set; }
        public Type Type { get; }
        public bool Require { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class OptionBindingAttrbute<T> : OptionBindingAttrbuteBase
    {
        public OptionBindingAttrbute(string name, string description, T defaultValue) : base(name, description, defaultValue, typeof(T))
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LocalizableOptionBindingAttribute<T> : OptionBindingAttrbuteBase
    {
        public LocalizableOptionBindingAttribute(string name, string resourceKey, T defaultValue, bool require = false)
            : base(name, Resources.ResourceManager.GetString(resourceKey) ?? string.Empty, defaultValue, typeof(T))
        {
            Require = require;
#if DEBUG
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (description is string.Empty) {
                Log.LogDebug($"Invalid resource key '{resourceKey}' for option '{name}'");     
            }
#endif
        }
    }
}