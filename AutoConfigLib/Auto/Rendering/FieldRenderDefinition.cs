using Microsoft.VisualBasic.FileIO;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Vintagestory.GameContent;

namespace AutoConfigLib.Auto.Rendering
{
    public class FieldRenderDefinition
    {
        public string Name { get; set; }

        public string Category { get; set; }
        
        public string Description { get; set; }

        public object DefaultValue { get; set; }

        public bool HasDefaultValue { get; set; }

        public string FormatString { get; set; }

        public bool IsPercentage { get; set; }

        public string SubId { get; set; }

        public bool IsReadOnly { get; set; }

        public bool IsNullableValueType { get; set; }

        public uint MaxStringLength { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool UseSlider { get; set; }

        public object RangeMin { get; set; }

        public object RangeMax { get; set; }

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

        public Type ValueType { get; set; }

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
            var descriptionAttr = memberInfo.GetCustomAttribute<DescriptionAttribute>();
            if(descriptionAttr != null)
            {
                Description = descriptionAttr.Description;
            }
            else if(XmlDocumentationHelper.TryGetSummary(memberInfo, out string xmlSummary))
            {
                Description = xmlSummary;
            }

            var defaultAttr = memberInfo.GetCustomAttribute<DefaultValueAttribute>();
            if (defaultAttr != null)
            {
                HasDefaultValue = true;
                DefaultValue = defaultAttr.Value;
            }

            var readonlyAttr = memberInfo.GetCustomAttribute<ReadOnlyAttribute>();
            if (readonlyAttr != null) IsReadOnly = readonlyAttr.IsReadOnly;

            var browseAble = memberInfo.GetCustomAttribute<BrowsableAttribute>();
            IsVisible = browseAble?.Browsable ?? (MethodInfo == null);

            SubId = memberInfo.Name;

            if (PropertyInfo != null)
            {
                if (!PropertyInfo.CanWrite) IsReadOnly = true;
                if (!PropertyInfo.CanRead || PropertyInfo.GetGetMethod(true).IsStatic) IsVisible = false;

                ValueType = PropertyInfo.PropertyType;
            }
            else if (FieldInfo != null)
            {
                if (FieldInfo.IsInitOnly) IsReadOnly = true;
                if (FieldInfo.IsStatic) IsVisible = false;

                ValueType = FieldInfo.FieldType;
            }
            else if (MethodInfo != null)
            {
                //TODO: arguments
            }
            else
            {
                IsVisible = false;
            }
            
            if(ValueType != null) 
            {
                IsNullableValueType = Nullable.GetUnderlyingType(ValueType) != null;
            }

            var rangeAttr = memberInfo.GetCustomAttribute<RangeAttribute>();
            if(ValueType != null && rangeAttr != null)
            {
                var type = Nullable.GetUnderlyingType(typeof(ValueType)) ?? ValueType;

                try
                {
                    RangeMin = Convert.ChangeType(rangeAttr.Minimum, type);
                    RangeMax = Convert.ChangeType(rangeAttr.Maximum, type);

                    UseSlider = true;
                    //Disable slider if we are using max values (since this will screw up interface)
                    if(rangeAttr.Minimum is double minDouble && double.IsInfinity(minDouble)) UseSlider = false;
                    else if(rangeAttr.Maximum is double maxDouble && double.IsInfinity(maxDouble)) UseSlider = false;
                    else if(rangeAttr.Minimum is int minInt && Math.Abs(minInt) == int.MaxValue) UseSlider = false;
                    else if(rangeAttr.Maximum is int maxInt && Math.Abs(maxInt) == int.MaxValue) UseSlider = false;
                }
                catch
                {
                    UseSlider = false;
                }
            }

            var maxLengthAttribute = memberInfo.GetCustomAttribute<MaxLengthAttribute>();
            MaxStringLength = (uint)(maxLengthAttribute?.Length ?? AutoConfigLibModSystem.Config.DefaultMaxStringLength);

            var formatAttr = memberInfo.GetCustomAttribute<DisplayFormatAttribute>();
            FormatString = formatAttr?.DataFormatString;

            IsPercentage = FormatString != null && FormatString.ToLower() == "p" && (Nullable.GetUnderlyingType(ValueType) ?? ValueType) == typeof(float);
            if (IsPercentage) FormatString = "%.2f%%";
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