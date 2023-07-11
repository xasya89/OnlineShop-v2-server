using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnlineShop2.RMK.ViewModels
{
    public class ViewModelBase : ReactiveObject, IDisposable
    {
        public IServiceScope ServiceScope { get; private set; }

        static public T Create<T>() where T : ViewModelBase
        {
            Type vmType = typeof(T);

            IServiceScope serviceScope = Program.Host.Services.CreateScope();
            IServiceProvider serviceProvider = serviceScope.ServiceProvider;

            ConstructorInfo vmConstructor = vmType
                .GetConstructors()
                .Single();

            List<object> services = new();

            foreach (ParameterInfo info in vmConstructor.GetParameters())
            {
                services.Add(serviceProvider.GetRequiredService(info.ParameterType));
            }

            T vm = (T)Activator.CreateInstance(vmType, services.ToArray());

            typeof(ViewModelBase)
                .GetProperty(nameof(ServiceScope), BindingFlags.Public | BindingFlags.Instance)
                .GetSetMethod(nonPublic: true)
                .Invoke(vm, new[] { serviceScope });

            return vm;
        }
        public void Dispose()
        {
            ServiceScope?.Dispose();
        }
    }
}