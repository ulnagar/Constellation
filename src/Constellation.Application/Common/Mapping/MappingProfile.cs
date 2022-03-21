using AutoMapper;
using System;
using System.Linq;
using System.Reflection;

namespace Constellation.Application.Common.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            ApplyMappingsFromAssembly(Assembly.GetExecutingAssembly());
        }

        private void ApplyMappingsFromAssembly(Assembly assembly)
        {
            var types = assembly.GetExportedTypes()
                .Where(t => t.GetInterfaces().Any(i =>
                    i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMapFrom<>)))
                .ToList();

            foreach (var type in types)
            {
                var instance = Activator.CreateInstance(type);

                // Local Mapping method
                var methodInfo = type.GetMethod("Mapping");
                if (methodInfo != null)
                {
                    methodInfo.Invoke(instance, new object[] { this });
                    continue;
                }

                // Interface Mapping method
                var interfaces = type.GetInterfaces().Where(iface => iface.Name == "IMapFrom`1").ToList();
                foreach (var iface in interfaces)
                {
                    iface.GetMethod("Mapping")?.Invoke(instance, new object[] { this });
                }
            }
        }
    }
}
