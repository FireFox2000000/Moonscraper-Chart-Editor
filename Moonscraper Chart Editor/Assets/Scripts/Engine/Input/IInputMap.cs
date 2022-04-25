// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

namespace MoonscraperEngine.Input
{
    public interface IInputMap
    {
        string GetInputStr();
        bool HasConflict(IInputMap other, InputAction.Properties properties);
        bool IsEmpty { get; }
        IInputMap Clone();
        bool SetFrom(IInputMap that);
        void SetEmpty();
        bool IsCompatibleWithDevice(IInputDevice device);
    }
}
