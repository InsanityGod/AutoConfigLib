using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using YamlDotNet.RepresentationModel;

namespace AutoConfigLib.Auto.Rendering
{
    public static class XmlDocumentationHelper
    {
        private static XmlDocument _document;
        private static Assembly _assembly;
        public static void LoadAssembly(Assembly assembly)
        {
            //TODO: look into embedded xml files
            _document = null;
            _assembly = assembly;
            var xmlPath = Path.Combine(
                Path.GetDirectoryName(_assembly.Location),
                Path.GetFileNameWithoutExtension(_assembly.Location) + ".xml"
            );

            try
            {
                var doc = new XmlDocument();
                doc.Load(xmlPath);
                _document = doc;
            }
            catch
            {
                //Failed to load Xml
            }
        }

        public static bool TryGetSummary(MemberInfo member, out string summary)
        {
            summary = null;
            if(_assembly != member.DeclaringType.Assembly) LoadAssembly(member.DeclaringType.Assembly);
            if(_document == null) return false;
            var memberName = GetMemberXmlName(member);
            if(memberName == null) return false;

            XmlNode memberNode = _document.SelectSingleNode($"/doc/members/member[@name='{memberName}']");
            if(memberNode == null) return false;

            var summaryNode = memberNode.SelectSingleNode("summary");
            if(summaryNode == null) return false;

            summary = StringTools.CleanWhiteSpaces(summaryNode.InnerText);
            return true;
        }

        private static string GetMemberXmlName(MemberInfo member)
        {
            if (member is Type type)
            {
                return $"T:{type.FullName}";
            }
            else if (member is MethodInfo method)
            {
                string parameters = string.Join(",", method.GetParameters().Select(p => p.ParameterType.FullName));
                return $"M:{method.DeclaringType.FullName}.{method.Name}({parameters})";
            }
            else if (member is PropertyInfo property)
            {
                return $"P:{property.DeclaringType.FullName}.{property.Name}";
            }
            else if (member is FieldInfo field)
            {
                return$"F:{field.DeclaringType.FullName}.{field.Name}";
            }
            else if (member is EventInfo eventInfo)
            {
                return $"E:{eventInfo.DeclaringType.FullName}.{eventInfo.Name}";
            }
            return null;
        }
    }
}
