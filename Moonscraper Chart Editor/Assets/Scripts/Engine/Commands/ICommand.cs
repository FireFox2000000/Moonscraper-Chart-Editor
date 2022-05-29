// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

namespace MoonscraperEngine
{
    public interface ICommand
    {
        void Invoke();
        void Revoke();
    }
}
