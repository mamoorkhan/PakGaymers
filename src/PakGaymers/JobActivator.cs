using Microsoft.Azure.WebJobs.Host;
using System;

namespace PakGaymers
{
    public class JobActivator : IJobActivator
    {
        private readonly IServiceProvider _service;

        public JobActivator(IServiceProvider service)
        {
            _service = service;
        }

        public T CreateInstance<T>()
        {
            return (T)_service.GetService(typeof(T));
        }
    }
}