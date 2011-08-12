using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Windows;

namespace Sleuth.InjectedViewer
{
    public static class Reflector
    {
        public static void OpenType(Type typeToOpen)
        {
            string commandLineArguments = BuildReflectorArgumentsForType(typeToOpen);
            OpenInReflector(commandLineArguments);
        }

        public static void OpenMember(MemberInfo memberToOpen)
        {
            string commandLineArguments = BuildReflectorArgumentsForMember(memberToOpen);
            OpenInReflector(commandLineArguments);
        }

        #region Private Helpers

        // Points to a registry value similar to: "c:\Users\Josh\Tools\reflector\Reflector.exe" /share /select:"%1"
        const string REFLECTOR_REGKEY = @"HKEY_CURRENT_USER\Software\Classes\code\shell\open\command";

        static void OpenInReflector(string commandLineArguments)
        {
            bool success = false;
            string commandText = Microsoft.Win32.Registry.GetValue(REFLECTOR_REGKEY, String.Empty, String.Empty) as string;
            if (!string.IsNullOrEmpty(commandText))
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = commandText.Split('/')[0].Trim();
                info.Arguments = commandLineArguments;

                try
                {
                    Process.Start(info);
                    success = true;
                }
                catch (Exception ex)
                {
                    Debug.Fail(ex.ToString());
                }
            }

            if (!success)
            {
                MessageBox.Show(
                    "Red Gate's .NET Reflector was not found on your computer.",
                    "Cannot Find Reflector",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        static string BuildReflectorArgumentsForType(Type typeToOpen)
        {
            string typeName = FormatTypeName(typeToOpen);
            Assembly assembly = typeToOpen.Assembly;
            AssemblyName assemblyName = assembly.GetName();
            string publicKeyToken = GetPublicKeyToken(assembly, assemblyName);

            string argument = String.Format("/select:\"code://{0}:{1}{2}/{3}\" \"{4}\"",
                assemblyName.Name,
                assemblyName.Version.ToString(),
                String.IsNullOrEmpty(publicKeyToken) ? String.Empty : (":" + publicKeyToken),
                typeName,
                assembly.Location);

            return argument;
        }

        static string BuildReflectorArgumentsForMember(MemberInfo memberToOpen)
        {
            Assembly assembly = memberToOpen.DeclaringType.Assembly;
            AssemblyName assemblyName = assembly.GetName();
            string publicKeyToken = GetPublicKeyToken(assembly, assemblyName);
            string declaringTypeName = FormatTypeName(memberToOpen.DeclaringType);
            string memberTypeName = GetMemberTypeName(memberToOpen);

            // Do not pass the /share argument because Reflector does not navigate
            // to the property/field if you use that flag twice in a row.
            string argument = String.Format("/select:\"code://{0}:{1}{2}/{3}/{4}{5}\" \"{6}\"",
                assemblyName.Name,
                assemblyName.Version.ToString(),
                String.IsNullOrEmpty(publicKeyToken) ? String.Empty : (":" + publicKeyToken),
                declaringTypeName,
                memberToOpen is PropertyInfo ? ("property:" + memberToOpen.Name) : memberToOpen.Name,
                String.IsNullOrEmpty(memberTypeName) ? String.Empty : (":" + memberTypeName),
                assembly.Location);

            return argument;
        }

        static string GetPublicKeyToken(Assembly assembly, AssemblyName assemblyName)
        {
            string publicKeyToken = null;
            if (assembly.GlobalAssemblyCache)
            {
                byte[] token = assemblyName.GetPublicKeyToken();
                string tokenString = new SoapHexBinary(token).ToString();
                publicKeyToken = tokenString.ToLowerInvariant();
            }
            return publicKeyToken;
        }

        static string GetMemberTypeName(MemberInfo memberToOpen)
        {
            // Reflector does not expect a type name for the member if it is an Enum value.
            if (memberToOpen.DeclaringType.IsEnum)
                return String.Empty;

            Type memberType;
            bool isProperty = memberToOpen is PropertyInfo;
            if (isProperty)
            {
                PropertyInfo prop = memberToOpen as PropertyInfo;
                memberType = prop.PropertyType;
            }
            else
            {
                FieldInfo field = memberToOpen as FieldInfo;
                if (field == null)
                    throw new ArgumentException("'memberToOpen' is not a PropertyInfo or a FieldInfo.");

                memberType = field.FieldType;
            }

            // Reflector does not expect a type name for a field if it is the 
            // same as the declaring type.  This only applies to fields, not properties.
            if (memberToOpen is FieldInfo && memberType == memberToOpen.DeclaringType)
                return String.Empty;

            Type[] genericTypeArguments = memberToOpen.DeclaringType.GetGenericArguments();
            string memberTypeName = GetMemberTypeNameHelper(memberType, genericTypeArguments);

            // Reflector does not allow a nested type to be separated from the
            // containing type with a '+' symbol, instead it requires a '.' symbol.
            return memberTypeName.Replace('+', '.');
        }

        static string GetMemberTypeNameHelper(Type current, Type[] genericDeclaringTypeArguments)
        {
            string output = String.Empty;

            if (current.IsGenericType)
            {
                #region Process Generic Type

                // Append the current type name.
                if (current.DeclaringType != null)
                {
                    output = FormatTypeName(current.DeclaringType);
                    output += ".";
                    output += current.Name;
                }
                else
                {
                    output += String.Format("{0}.{1}", current.Namespace, current.Name);
                }

                // Remove generic type info from the type name.
                int idx = output.IndexOf('`');
                if (-1 < idx)
                    output = output.Remove(idx);

                // Append the type parameters.
                output += "<";
                Type[] currentTypeArgs = current.GetGenericArguments();
                for (int i = 0; i < currentTypeArgs.Length; ++i)
                {
                    Type arg = currentTypeArgs[i];
                    output += GetMemberTypeNameHelper(arg, genericDeclaringTypeArguments);
                    if (i < currentTypeArgs.Length - 1)
                        output += ",";
                }
                output += ">";

                #endregion // Process Generic Type
            }
            else if (-1 < Array.IndexOf(genericDeclaringTypeArguments, current))
            {
                #region Process Generic Type Argument

                output = String.Format("<!{0}>", Array.IndexOf(genericDeclaringTypeArguments, current));

                #endregion // Process Generic Type Argument
            }
            else
            {
                #region Process Non-Generic Type

                if (Utilities.IsPrimitiveType(current))
                {
                    output = current.Name;
                }
                else if (current.IsArray)
                {
                    output = IsPrimitiveArray(current) ? current.Name : current.FullName;

                    int rank = current.GetArrayRank();
                    if (rank > 1)
                    {
                        int idx = output.IndexOf('[');
                        if (-1 < idx)
                            output = output.Remove(idx);

                        output += "%5b";
                        for (int i = 0; i < rank; ++i)
                        {
                            output += "0:";
                            if (i < (rank - 1))
                                output += ",";
                        }
                        output += "%5d";
                    }
                }
                else
                {
                    output = current.FullName;
                }

                #endregion // Process Non-Generic Type
            }

            return output;
        }

        static bool IsPrimitiveArray(Type type)
        {
            if (!type.IsArray)
                return false;

            Type current = type;
            while (current.IsArray)
                current = current.GetElementType();

            return Utilities.IsPrimitiveType(current);
        }

        static string FormatTypeName(Type type)
        {
            // To discover what Reflector requires as a command-line argument for a certain member of a type,
            // select that member in Reflector and press Ctrl + Alt + C to copy the value to the clipboard.
            //
            // PROPERTY   -- code://WindowsBase:3.0.0.0:31bf3856ad364e35/System.Windows.Freezable/property:HasHandlers:Boolean
            // FIELD      -- code://Sleuth.WinFormsTestApp:1.0.0.0/Sleuth.WinFormsTestApp.Form1/button1:System.Windows.Forms.Button
            // ENUM VALUE -- code://mscorlib:2.0.0.0:b77a5c561934e089/System.Diagnostics.SymbolStore.SymAddressKind/NativeOffset

            if (type.FullName == null)
                return String.Empty;

            string typeName = type.FullName.Replace('+', '.');

            int idxAssemblyInfo = typeName.IndexOf("[[");
            if (-1 < idxAssemblyInfo)
            {
                Debug.Assert(typeName.EndsWith("]]"), "Unexpected type name format.");

                // If the type name has assembly information embedded in it, such as:
                // System.Collections.ObjectModel.Collection<>[[System.Windows.ResourceDictionary, PresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35]]
                // we need to remove it now.
                typeName = typeName.Remove(idxAssemblyInfo);
            }

            // Look for the character used to indicate the number of type parameters a generic class/struct contains.
            int idxTick = typeName.IndexOf('`');

            // We add up the number of type parameters because Reflector expects a nested generic type
            // to include the number of type parameters owned by the containing generic type.
            int typeParameterCount = 0;

            while (-1 < idxTick && idxTick < typeName.Length)
            {
                // Note: This code assumes a type has no more than 9 generic parameters
                // because it grabs only the next character and treats it as the argument count.
                typeParameterCount += Int32.Parse(typeName[idxTick + 1].ToString());

                int idxDot = typeName.IndexOf('.', idxTick);

                // If we are not in a nested type, remove the tick and everything else that follows it.
                if (idxDot < 0)
                    typeName = typeName.Remove(idxTick);
                else
                    typeName = typeName.Remove(idxTick, 2);

                string typeParams = "<";
                for (int i = 1; i < typeParameterCount; ++i)
                    typeParams += ",";
                typeParams += ">";

                typeName = typeName.Insert(idxTick, typeParams);

                idxTick = typeName.IndexOf('`', idxTick + 1);
            }

            // If a declaring type is generic but the nested type is not,
            // we must append <> so that Reflector can interpret it properly.
            if (0 < typeParameterCount && !typeName.EndsWith(">"))
                typeName += "<>";

            return typeName;
        }

        #endregion // Private Helpers
    }
}