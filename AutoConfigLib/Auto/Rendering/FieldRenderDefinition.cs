using System;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace AutoConfigLib.Auto.Rendering
{
    public class FieldRenderDefinition
    {
        public string Name { get; set; }

        public string Category { get; set; }
        
        public string Description { get; set; }

        public object DefaultValue { get; set; }

        public string SubId { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsVisible { get; set; } = true;
        //TODO use static int to see if still valid!

        private IRenderer cachedRenderer;
        public IRenderer ValueRenderer
        {
            get
            {
                //Lazy initialisation
                if(cachedRenderer == null)
                {
                    if (PropertyInfo != null)
                    {
                        cachedRenderer = Renderer.GetOrCreateRenderForType(PropertyInfo.PropertyType);
                    }
                    else if (FieldInfo != null)
                    {
                        cachedRenderer = Renderer.GetOrCreateRenderForType(FieldInfo.FieldType);
                    }
                    else if (MethodInfo != null)
                    {
                        cachedRenderer = Renderer.GetOrCreateRenderForType(typeof(MethodInfo));
                    }
                }

                return cachedRenderer;
            }
        }

        public PropertyInfo PropertyInfo { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public object GetValue(object instance)
        {
            if (PropertyInfo != null) return PropertyInfo.GetValue(instance);
            if (FieldInfo != null) return FieldInfo.GetValue(instance);
            if (MethodInfo != null) return MethodInfo;

            return null;
        }

        public void SetValue(object instance, object value)
        {
            if (IsReadOnly) return;
            try
            {
                PropertyInfo?.SetValue(instance, value);
                FieldInfo?.SetValue(instance, value);
            }
            catch
            {
                Console.WriteLine($"AutoConfigLib: detected unsettable field/method '{Name}' on '{instance.GetType()}' that was not correctly estimated as ReadOnly");
                IsReadOnly = true;
            }
        }

        public void Initialize(MemberInfo memberInfo)
        {
            PropertyInfo = memberInfo as PropertyInfo;
            FieldInfo = memberInfo as FieldInfo;
            MethodInfo = memberInfo as MethodInfo;

            var nameAttr = memberInfo.GetCustomAttribute<DisplayNameAttribute>();
            Name = string.IsNullOrEmpty(nameAttr?.DisplayName) ? Renderer.GetHumanReadable(memberInfo.Name) : nameAttr.DisplayName;
            
            Category = memberInfo.GetCustomAttribute<CategoryAttribute>()?.Category ?? string.Empty;
            Description = memberInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;
            DefaultValue = memberInfo.GetCustomAttribute<DefaultValueAttribute>()?.Value;

            var readonlyAttr = memberInfo.GetCustomAttribute<ReadOnlyAttribute>();
            if (readonlyAttr != null) IsReadOnly = readonlyAttr.IsReadOnly;

            var browseAble = memberInfo.GetCustomAttribute<BrowsableAttribute>();
            IsVisible = browseAble?.Browsable ?? (MethodInfo == null);

            SubId = memberInfo.Name;

            if (PropertyInfo != null)
            {
                if (!PropertyInfo.CanWrite) IsReadOnly = true;
                if (!PropertyInfo.CanRead || PropertyInfo.GetGetMethod(true).IsStatic) IsVisible = false;
            }
            else if (FieldInfo != null)
            {
                if (FieldInfo.IsInitOnly) IsReadOnly = true;
                if (FieldInfo.IsStatic) IsVisible = false;
            }
            else if (MethodInfo != null)
            {
                //TODO: arguments
            }
            else
            {
                IsVisible = false;
            }

            //TODO button renderer
        }

        public static FieldRenderDefinition Create(MemberInfo info)
        {
            var newInstance = new FieldRenderDefinition();
            newInstance.Initialize(info);
            return newInstance;
        }

        public T GetCustomAttribute<T>() where T : Attribute => ((MemberInfo)PropertyInfo ?? FieldInfo).GetCustomAttribute<T>();
    }
}