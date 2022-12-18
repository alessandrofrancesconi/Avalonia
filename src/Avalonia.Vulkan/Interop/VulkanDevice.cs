using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using System.Threading;
using Avalonia.Platform;
using Avalonia.Vulkan.Interop;

namespace Avalonia.Vulkan;



internal partial class VulkanDevice : IVulkanDevice
{
    private readonly IntPtr _handle;
    private readonly IntPtr _physicalDeviceHandle;
    private readonly IntPtr _mainQueue;
    private readonly uint _graphicsQueueIndex;
    private readonly Dictionary<Type, object> _features;
    private readonly object _lock = new();
    private Thread? _lockedByThread;

    private VulkanDevice(IVulkanInstance instance, IntPtr handle, IntPtr physicalDeviceHandle,
        IntPtr mainQueue, uint graphicsQueueIndex, Dictionary<Type, object> features)
    {
        _handle = handle;
        _physicalDeviceHandle = physicalDeviceHandle;
        _mainQueue = mainQueue;
        _graphicsQueueIndex = graphicsQueueIndex;
        _features = features;
        Instance = instance;
    }

    T CheckAccess<T>(T f)
    {
        if (_lockedByThread != Thread.CurrentThread)
            throw new InvalidOperationException("This class is only usable when locked");
        return f;
    }

    public IDisposable Lock()
    {
        Monitor.Enter(_lock);
        var oldLockedBy = _lockedByThread;
        _lockedByThread = Thread.CurrentThread;
        return Disposable.Create(() =>
        {
            _lockedByThread = null;
            Monitor.Exit(_lock);
        });
    }

    public bool IsLost => false;
    public IntPtr Handle => CheckAccess(_handle);
    public IntPtr PhysicalDeviceHandle => CheckAccess(_physicalDeviceHandle);
    public IntPtr MainQueueHandle => CheckAccess(_mainQueue);
    public uint GraphicsQueueFamilyIndex => CheckAccess(_graphicsQueueIndex);
    public IVulkanInstance Instance { get; }
    public void Dispose()
    {
        // TODO
    }

    public object? TryGetFeature(Type featureType) => null;
}