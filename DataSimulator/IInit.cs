using System;

namespace DataSimulator
{
    public interface IInit
    {
        void Initialize(Random rnd, Guid machineId, Guid sessionKey);
    }
}
