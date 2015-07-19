using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;

using NisAnim.Conversion;

namespace NisAnim.IO
{
    public static class FileHelpers
    {
        public static Type IdentifyFile(EndianBinaryReader reader, string fileName)
        {
            long position = reader.BaseStream.Position;
            List<Tuple<Type, string, int>> matchedTypes = new List<Tuple<Type, string, int>>();

            Type baseType = typeof(BaseFile);
            foreach (Type t in baseType.Assembly.GetTypes().Where(x => x.IsSubclassOf(baseType)))
            {
                FieldInfo fi = t.GetField("MagicNumber", BindingFlags.Public | BindingFlags.Static);
                if (fi != null)
                {
                    string magic = (fi.GetValue(null) as string);
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);
                    if (magic == Encoding.ASCII.GetString(reader.ReadBytes(magic.Length))) matchedTypes.Add(new Tuple<Type, string, int>(t, null, int.MaxValue));
                }

                FileNamePatternAttribute attrib = (FileNamePatternAttribute)t.GetCustomAttributes(typeof(FileNamePatternAttribute), false).FirstOrDefault();
                if (attrib != null)
                {
                    Regex reg = new Regex(attrib.Pattern);
                    if (reg.IsMatch(Path.GetFileName(fileName)))
                        matchedTypes.Add(new Tuple<Type, string, int>(t, attrib.Pattern, attrib.Pattern.Length));
                }
            }

            /* Assume longer Regex pattern means more precise match (i.e. "*.pac" vs "map*.pac") */
            Tuple<Type, string, int> bestMatch = matchedTypes.OrderByDescending(x => x.Item3).FirstOrDefault();
            return (bestMatch == null ? null : bestMatch.Item1);
        }

        public static TreeNode TraverseObject(TreeNode parentNode, string name, object obj, bool recurse = false)
        {
            if (obj == null) return null;

            DisplayNameAttribute objDisplayName = (obj.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(DisplayNameAttribute)) as DisplayNameAttribute);
            string objFilename = string.Empty;
            PropertyInfo pi = obj.GetType().GetProperty("Filename", BindingFlags.Instance | BindingFlags.Public);
            if (pi != null) objFilename = (pi.GetValue(obj, null) as string);

            TreeNode node = new TreeNode(
                objDisplayName != null ? string.Format("{0} [{1}]", (objFilename != string.Empty ? objFilename : name), objDisplayName.DisplayName) : name
                ) { Tag = obj };

            if (!(obj is System.Collections.ICollection))
            {
                var props = obj.GetType()
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    .Where(x => x.PropertyType.IsArray ||
                        (x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) ||
                        x.GetCustomAttributes(false).FirstOrDefault(y => (y is EditorBrowsableAttribute) && (y as EditorBrowsableAttribute).State != EditorBrowsableState.Never) != null);

                foreach (var prop in props)
                {
                    var val = prop.GetValue(obj, null);
                    if (val == null) continue;

                    DisplayNameAttribute propDisplayName = (prop.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(DisplayNameAttribute)) as DisplayNameAttribute);
                    if (((val is System.Collections.ICollection) &&
                        (!(val as System.Collections.ICollection).AsQueryable().ElementType.IsValueType) && (val as System.Collections.ICollection).Count > 0) ||
                        (!(val is System.Collections.ICollection)))
                    {
                        if (recurse)
                            node.Nodes.Add(TraverseObject(node, propDisplayName != null ? propDisplayName.DisplayName : prop.Name, val, recurse));
                        else
                            node.Nodes.Add(new TreeNode(propDisplayName != null ? propDisplayName.DisplayName : prop.Name) { Tag = val });
                    }
                }
            }
            else
            {
                foreach (var e in (obj as IEnumerable<object>).Select((Value, Index) => new { Index, Value }))
                {
                    if (recurse)
                        node.Nodes.Add(TraverseObject(node, string.Format("{0}", e.Index), e.Value, recurse));
                    else
                        node.Nodes.Add(new TreeNode(string.Format("{0}", e.Index)) { Tag = e });
                }
            }

            return node;
        }
    }
}
