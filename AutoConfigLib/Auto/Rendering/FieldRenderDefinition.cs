using HarmonyLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AutoConfigLib.Auto.Rendering
{
    public class FieldRenderDefinition
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Category { get; set; }

        public string SubId { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsVisible { get; set; } = true;

        public IRenderer ValueRenderer { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public FieldInfo FieldInfo { get; set; }

        public MethodInfo MethodInfo { get; set; }

        public object GetValue(object instance)
        {
            if(PropertyInfo != null) return PropertyInfo.GetValue(instance);
            if(FieldInfo != null) return FieldInfo.GetValue(instance);
            if(MethodInfo != null) return MethodInfo;

            return null;
        }

        public void SetValue(object instance, object value)
        {
            if(IsReadOnly) return;
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
            Name = nameAttr != null ? nameAttr.DisplayName : Renderer.GetHumanReadable(memberInfo.Name);

            var descriptionAttr = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            Description = descriptionAttr?.Description;

            var categoryAttr = memberInfo.GetCustomAttribute<CategoryAttribute>();
            Category = categoryAttr?.Category ?? string.Empty;

            var readonlyAttr = memberInfo.GetCustomAttribute<ReadOnlyAttribute>();
            if(readonlyAttr != null) IsReadOnly = readonlyAttr.IsReadOnly;

            var browseAble = memberInfo.GetCustomAttribute<BrowsableAttribute>();
            IsVisible = browseAble?.Browsable ?? (MethodInfo == null);

            SubId = memberInfo.Name;

            if(PropertyInfo != null)
            {
                ValueRenderer = Renderer.GetOrCreateRenderForType(PropertyInfo.PropertyType);
                if (!PropertyInfo.CanWrite) IsReadOnly = true;
                if(!PropertyInfo.CanRead || PropertyInfo.GetGetMethod(true).IsStatic) IsVisible = false;
            }
            else if(FieldInfo != null)
            {
                ValueRenderer = Renderer.GetOrCreateRenderForType(FieldInfo.FieldType);
                if(FieldInfo.IsInitOnly) IsReadOnly = true;
                if(FieldInfo.IsStatic) IsVisible = false;
            }
            else if(MethodInfo != null)
            {
                ValueRenderer = Renderer.GetOrCreateRenderForType(typeof(MethodInfo));
                //TODO: arguments
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
