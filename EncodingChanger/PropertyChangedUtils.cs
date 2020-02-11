using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EncodingChanger
{
	static class PropertyChangedUtils
	{
		public static void SetProperty<T, TProperty>(this T @this, ref TProperty storage, TProperty value, [CallerMemberName]string propertyName = "") where T : INotifyPropertyChanged
		{
			if (EqualityComparer<TProperty>.Default.Equals(storage, value)) return;
			storage = value;
			RaisePropertyChanged(@this, propertyName);
		}

		public static void SetProperty<TProperty>(object @this, ref TProperty storage, TProperty value, PropertyChangedEventHandler handler, [CallerMemberName]string propertyName = "")
		{
			if (EqualityComparer<TProperty>.Default.Equals(storage, value)) return;
			storage = value;
			RaisePropertyChanged(@this, handler, propertyName);
		}

		public static void RaisePropertyChanged<T>(this T @this, [CallerMemberName]string propertyName = "") where T : INotifyPropertyChanged => RaisePropertyChanged(PropertyChangedUtilsHelper<T>.FindHandler(@this), propertyName);

		public static void RaisePropertyChanged(object @this, PropertyChangedEventHandler handler, [CallerMemberName]string propertyName = "") => RaisePropertyChanged(x => handler(@this, x), propertyName);

		public static void RaisePropertyChanged(Action<PropertyChangedEventArgs>? handler, [CallerMemberName]string propertyName = "") => handler?.Invoke(new PropertyChangedEventArgs(propertyName));
	}

	static class PropertyChangedUtilsHelper<T> where T : INotifyPropertyChanged
	{
		static readonly Func<T, Action<PropertyChangedEventArgs>?> s_HandlerFinder = GetHandlerFinder();

		static Func<T, Action<PropertyChangedEventArgs>?> GetHandlerFinder()
		{
			var methods = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var methodAndParams = methods
				.Where(x => (x.Name == "OnPropertyChanged" || x.Name == "RaisePropertyChanged") && x.ReturnType == typeof(void))
				.Select(method => (method, parameters: method.GetParameters()))
				.Where(x => x.parameters.Length == 1)
				.Select(x => (x.method, param: x.parameters[0]))
				.OrderBy(x => x.method.Name); // Grant priority for "OnPropertyChanged"
			foreach (var (method, param) in methodAndParams)
			{
				if (param.ParameterType == typeof(PropertyChangedEventArgs))
				{
					// C#:
					//   instance => instance.${method}
					// IL:
					//   ldarg.0
					//   #if (is_virtual_method(${method}))
					//     dup
					//     ldvirtftn ${method}
					//   #else
					//     ldftn ${method}
					//   newobj instance void Action<PropertyChangedEventArgs>::.ctor(object, native int)
					//   ret
					var dynamicMethod = new DynamicMethod("FindHandlerFromMethod", typeof(Action<PropertyChangedEventArgs>), new[] { typeof(T) }, true);
					dynamicMethod.DefineParameter(1, ParameterAttributes.In, "instance");
					var ilGenerator = dynamicMethod.GetILGenerator();
					ilGenerator.Emit(OpCodes.Ldarg_0);
					if (method.IsVirtual)
					{
						ilGenerator.Emit(OpCodes.Dup);
						ilGenerator.Emit(OpCodes.Ldvirtftn, method);
					}
					else
					{
						ilGenerator.Emit(OpCodes.Ldftn, method);
					}
					ilGenerator.Emit(OpCodes.Newobj, typeof(Action<PropertyChangedEventArgs>).GetConstructor(new[] { typeof(object), typeof(IntPtr) })!);
					ilGenerator.Emit(OpCodes.Ret);
					return (Func<T, Action<PropertyChangedEventArgs>>)dynamicMethod.CreateDelegate(typeof(Func<T, Action<PropertyChangedEventArgs>>));
				}
				else if (param.ParameterType == typeof(string))
				{
					// instance => e => instance.${method}(e.PropertyName);
					var instance = Expression.Parameter(typeof(T));
					var e = Expression.Parameter(typeof(PropertyChangedEventArgs));
					return Expression.Lambda<Func<T, Action<PropertyChangedEventArgs>>>(
						Expression.Lambda<Action<PropertyChangedEventArgs>>(
							Expression.Call(instance, method, Expression.PropertyOrField(e, nameof(PropertyChangedEventArgs.PropertyName))),
							e
						),
						instance
					).Compile();
				}
			}
			var field = typeof(T).GetField(nameof(INotifyPropertyChanged.PropertyChanged), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && field.FieldType == typeof(PropertyChangedEventHandler))
			{
				// instance => e => instance.${field}(instance, e);
				var instance = Expression.Parameter(typeof(T));
				var e = Expression.Parameter(typeof(PropertyChangedEventArgs));
				return Expression.Lambda<Func<T, Action<PropertyChangedEventArgs>>>(
					Expression.Lambda<Action<PropertyChangedEventArgs>>(
						Expression.Invoke(Expression.Field(instance, field), instance, e),
						e
					),
					instance
				).Compile();
			}
			return _ => null;
		}

		public static Action<PropertyChangedEventArgs>? FindHandler(T instance) => s_HandlerFinder(instance);
	}
}
