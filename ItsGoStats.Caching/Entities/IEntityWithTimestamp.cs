using System;

namespace ItsGoStats.Caching.Entities
{
    public interface IEntityWithTimestamp : IEntity
    {
        DateTime Time { get; set; }
    }
}
