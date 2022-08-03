﻿using System;
using System.Collections.Generic;
using Avalonia.Data;
using Avalonia.PropertyStore;
using Avalonia.Styling.Activators;

namespace Avalonia.Styling
{
    /// <summary>
    /// Stores state for a <see cref="Style"/> that has been instanced on a control.
    /// </summary>
    /// <remarks>
    /// <see cref="StyleInstance"/> is based on <see cref="ValueFrame"/> meaning that it is 
    /// injected directly into the value store of an <see cref="AvaloniaObject"/>. Depending on
    /// the setters present on the style, it may be possible to share a single style instance
    /// among all controls that the style is applied to, meaning that a single style instance can
    /// apply to multiple controls.
    /// </remarks>
    internal class StyleInstance : ValueFrame, IStyleInstance, IStyleActivatorSink, IDisposable
    {
        private readonly IStyleActivator? _activator;
        private List<ISetterInstance>? _setters;

        public StyleInstance(IStyle style, IStyleActivator? activator)
        {
            _activator = activator;
            Priority = activator is object ? BindingPriority.StyleTrigger : BindingPriority.Style;
            Source = style;
        }

        public bool HasActivator => _activator is object;

        public override bool IsActive
        {
            get
            {
                if (_activator?.IsSubscribed == false)
                    _activator.Subscribe(this);
                return _activator?.IsActive ?? true;
            }
        }

        public IStyle Source { get; }

        public void Add(ISetterInstance instance)
        {
            if (instance is IValueEntry valueEntry)
            {
                if (Contains(valueEntry.Property))
                    throw new InvalidOperationException(
                        $"Duplicate setter encountered for property '{valueEntry.Property}' in '{Source}'.");
                Add(valueEntry);
            }
            else
                (_setters ??= new()).Add(instance);
        }

        public override void Dispose()
        {
            base.Dispose();
            _activator?.Dispose();
        }

        public new void MakeShared() => base.MakeShared();

        void IStyleActivatorSink.OnNext(bool value, int tag) => Owner?.OnFrameActivationChanged(this);
    }
}