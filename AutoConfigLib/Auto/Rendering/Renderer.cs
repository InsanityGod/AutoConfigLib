using AutoConfigLib.Auto.Rendering.Renderers;
using AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes;
using AutoConfigLib.Auto.Rendering.Renderers.ComplexTypes.Enumeration;
using AutoConfigLib.Auto.Rendering.Renderers.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;

namespace AutoConfigLib.Auto.Rendering
{
    public static class Renderer
    {

        public static Dictionary<Type, IRenderer> CachedRenderesByType { get; internal set; }
        //TODO give users an easy way to register their own renderers

        private static bool LoadedStaticImplementations = false;

        public static void LoadStaticImplementations()
        {
            if (LoadedStaticImplementations) return;
            LoadedStaticImplementations = true;
            foreach (var type in typeof(Renderer).Assembly.GetTypes().Where(type => !type.IsGenericType))
            {
                var rendererInterfaceTypes = type.GetInterfaces()
                    .Where(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IRenderer<>));

                foreach (var rendererInterfaceType in rendererInterfaceTypes)
                {
                    var renderer = (IRenderer)Activator.CreateInstance(type);
                    CachedRenderesByType[rendererInterfaceType.GenericTypeArguments[0]] = renderer;
                    renderer.Initialize();
                }
            }
        }

        public static void ClearCache()
        {
            CachedRenderesByType.Clear();
            LoadedStaticImplementations = false;
        }

        public static IRenderer GetOrCreateRenderForType(Type type)
        {
            if (!LoadedStaticImplementations) LoadStaticImplementations();
            if (CachedRenderesByType.TryGetValue(type, out var result)) return result;

            if (type.IsArray) result ??= (IRenderer)Activator.CreateInstance(typeof(ArrayRenderer<>).MakeGenericType(type.GetElementType()));

            var dictInterface = type.GetFirstGenericInterface(typeof(IDictionary<,>));
            if (dictInterface != null) result ??= (IRenderer)Activator.CreateInstance(typeof(DictionaryRenderer<,,>).MakeGenericType(type, dictInterface.GenericTypeArguments[0], dictInterface.GenericTypeArguments[1]));

            var listInterface = type.GetFirstGenericInterface(typeof(IList<>));
            if (listInterface != null) result ??= (IRenderer)Activator.CreateInstance(typeof(ListRenderer<,>).MakeGenericType(type, listInterface.GenericTypeArguments[0]));

            var collectionInterface = type.GetFirstGenericInterface(typeof(ICollection<>));
            if (collectionInterface != null) result ??= (IRenderer)Activator.CreateInstance(typeof(CollectionRenderer<,>).MakeGenericType(type, collectionInterface.GenericTypeArguments[0]));

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null && nullableType.IsValueType) result ??= (IRenderer)Activator.CreateInstance(typeof(NullableValueRenderer<>).MakeGenericType(type));

            if (type.BaseType == typeof(Enum)) result ??= (IRenderer)Activator.CreateInstance(typeof(EnumRenderer<>).MakeGenericType(type));

            if (!type.IsValueType && collectionInterface == null) result ??= (IRenderer)Activator.CreateInstance(typeof(ClassRenderer<>).MakeGenericType(type));

            result ??= (IRenderer)Activator.CreateInstance(typeof(UnsupportedTypeRenderer<>).MakeGenericType(type));
            CachedRenderesByType[type] = result;
            result.Initialize();

            //TODO: custom renderers for JsonItemStack and Code/AssetLocation

            return result;
        }

        public static Type GetFirstGenericInterface(this Type typeToCheck, Type genericInterfaceType) => Array.Find(
            typeToCheck.GetInterfaces(),
            interfaceType => interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericInterfaceType
        );

        internal static readonly char[] ReadableSplitIdentifiers = new char[] { '-', '_', ':' };

        public static string GetHumanReadable(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return string.Empty;

            StringBuilder newText = new(str.Length * 2);
            newText.Append(str[0]);

            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) && str[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(str[i]);
            }

            foreach (var delimiter in ReadableSplitIdentifiers)
            {
                newText.Replace(delimiter, ' ');
            }

            return newText.ToString();
        }
    }
}