using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

public class BinaryFormatter
{
    private readonly HashSet<object> _serializedObjects = new HashSet<object>();
    /// <summary>
    /// Serializes an object and writes it to the provided stream.
    /// </summary>
    public void Serialize(Stream stream, object obj)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (obj == null)
            throw new ArgumentNullException(nameof(obj));

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
        SerializeObject(writer, obj);
    }

    /// <summary>
    /// Deserializes an object of the specified generic type from the provided stream.
    /// </summary>
    public T Deserialize<T>(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        return (T)DeserializeObject(reader, typeof(T));
    }
    public void SerializeObject(BinaryWriter writer, object obj)
    {
        try
        {
            // If object is null, write a null marker and return
            if (obj == null)
            {
                writer.Write(false); // Null marker
                return;
            }

            // If we've already serialized this object, avoid circular reference
            if (_serializedObjects.Contains(obj))
            {
                writer.Write(false); // Indicate circular reference
                return;
            }

            // Mark the object as serialized
            _serializedObjects.Add(obj);

            writer.Write(true); // Non-null marker
            var type = obj.GetType();
            writer.Write(type.AssemblyQualifiedName ?? throw new InvalidOperationException("Type name is null."));

            // Handle primitives, string, decimal, and enums
            if (type.IsPrimitive || obj is string || obj is decimal)
            {
                writer.Write(obj.ToString());
            }
            else if (type.IsEnum)
            {
                writer.Write(obj.ToString());
            }
            // Handle arrays
            else if (obj is Array array)
            {
                writer.Write(array.Length);
                foreach (var item in array)
                {
                    SerializeObject(writer, item);
                }
            }
            // Handle collections like lists
            else if (obj is IList list)
            {
                writer.Write(list.Count);
                foreach (var item in list)
                {
                    SerializeObject(writer, item);
                }
            }
            // Handle custom objects (complex types)
            else
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (property.CanRead && property.GetIndexParameters().Length == 0)
                    {
                        writer.Write(property.Name);
                        try
                        {
                            var value = property.GetValue(obj);
                            SerializeObject(writer, value);
                        }
                        catch (Exception ex)
                        {
                            // Log serialization error for property
                            Console.WriteLine($"Error serializing property {property.Name}: {ex.Message}");
                            SerializeObject(writer, null); // Serialize as null if error occurs
                        }
                    }
                }
                writer.Write("END"); // End of object marker
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during serialization: {ex.Message}");
            throw; // Rethrow the exception for the caller to handle
        }
    }
    private readonly Dictionary<long, object> _deserializedObjects = new Dictionary<long, object>();

    public object DeserializeObject(BinaryReader reader, Type targetType)
    {
        try
        {
            if (!reader.ReadBoolean()) // Null marker
                return null;

            var typeName = reader.ReadString();
            var type = Type.GetType(typeName) ?? targetType;

            // Check if we've already deserialized this object to avoid circular references
            if (_deserializedObjects.ContainsKey(reader.BaseStream.Position))
            {
                return _deserializedObjects[reader.BaseStream.Position];
            }

            object obj = null;

            // Handle primitives, string, decimal, and enums
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                var value = reader.ReadString();
                obj = Convert.ChangeType(value, type);
            }
            else if (type.IsEnum)
            {
                var value = reader.ReadString();
                obj = Enum.Parse(type, value);
            }
            else if (type.IsArray)
            {
                var elementType = type.GetElementType() ?? throw new InvalidOperationException("Array element type is null.");
                var length = reader.ReadInt32();
                var array = Array.CreateInstance(elementType, length);
                for (int i = 0; i < length; i++)
                {
                    array.SetValue(DeserializeObject(reader, elementType), i);
                }
                obj = array;
            }
            else if (typeof(IList).IsAssignableFrom(type))
            {
                var list = (IList)Activator.CreateInstance(type);
                var length = reader.ReadInt32();
                for (int i = 0; i < length; i++)
                {
                    list.Add(DeserializeObject(reader, type.GetGenericArguments()[0]));
                }
                obj = list;
            }
            else
            {
                // Handle complex types (classes)
                obj = FormatterServices.GetUninitializedObject(type);
                _deserializedObjects[reader.BaseStream.Position] = obj; // Track this object to avoid circular references

                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                while (true)
                {
                    if (reader.BaseStream.Position >= reader.BaseStream.Length)
                    {
                        break;
                    }
                    try
                    {
                        var propertyName = reader.ReadString();
                        if (propertyName == "END") break;

                        var property = Array.Find(properties, p => p.Name == propertyName);
                        if (property != null && property.CanWrite)
                        {
                            var value = DeserializeObject(reader, property.PropertyType);
                            property.SetValue(obj, value);
                        }

                    }
                    catch (EndOfStreamException ex){
                        break;                   
                    }
                }
            }

            return obj;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during deserialization: {ex.Message}");
            throw; // Rethrow the exception for the caller to handle
        }
    }
}
