﻿using System.Collections.Generic;
using Avalonia.Data;

#nullable enable

namespace Avalonia.Controls.Generators
{
    /// <summary>
    /// Creates <see cref="ContentControl"/> containers for an <see cref="ItemsControl"/>.
    /// </summary>
    public class ItemContainerGenerator : IItemContainerGenerator
    {
        private Stack<IControl>? _recyclePool;

        public ItemContainerGenerator(ItemsControl owner) => Owner = owner;

        public ItemsControl Owner { get; }

        public IControl Realize(IControl parent, int index, object? item)
        {
            IControl result;

            if (_recyclePool?.Count > 0)
            {
                result = _recyclePool.Pop();
                result.DataContext = item;
                result.IsVisible = true;
                return result;
            }
            else
            {
                result = CreateContainer(parent, index, item);
            }

            return result;
        }

        public void Unrealize(IControl container, int index, object? item)
        {
            container.IsVisible = false;
            container.DataContext = null;
            (_recyclePool ??= new())?.Push((ContentControl)container);
        }

        protected virtual IControl CreateContainer(IControl parent, int index, object? item)
        {
            var result = new ContentControl 
            { 
                DataContext = item,
                ContentTemplate = Owner.ItemTemplate,
            };

            result.Bind(
                ContentControl.ContentProperty,
                result.GetBindingObservable(StyledElement.DataContextProperty),
                BindingPriority.TemplatedParent);
            return result;
        }
    }
}