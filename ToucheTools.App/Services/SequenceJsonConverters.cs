using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace ToucheTools.App.Services;

//from: https://stackoverflow.com/questions/48715635/deserialize-string-with-tuple-key-in-c-sharp
internal class ValueTupleConverter<T1, T2>
    : TypeConverter 
    where T1: struct where T2: struct
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, 
        Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context,
        CultureInfo? culture, object value)
    {
        var elements = ((String)value).Split(new[] { '(', ',', ')'},
            StringSplitOptions.RemoveEmptyEntries);

        return (
            JsonConvert.DeserializeObject<T1>(elements.First()), 
            JsonConvert.DeserializeObject<T2>(elements.Last()));
    }
}

internal class ValueTupleConverter<T1, T2, T3>
    : TypeConverter 
    where T1: struct where T2: struct where T3: struct
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, 
        Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context,
        CultureInfo? culture, object value)
    {
        var elements = ((String)value).Split(new[] { '(', ',', ')'},
            StringSplitOptions.RemoveEmptyEntries);

        return (
            JsonConvert.DeserializeObject<T1>(elements[0]), 
            JsonConvert.DeserializeObject<T2>(elements[1]), 
            JsonConvert.DeserializeObject<T3>(elements[2])
        );
    }
}

internal class ValueTupleConverter<T1, T2, T3, T4>
    : TypeConverter 
    where T1: struct where T2: struct where T3: struct where T4: struct
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, 
        Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext? context,
        CultureInfo? culture, object value)
    {
        var elements = ((String)value).Split(new[] { '(', ',', ')'},
            StringSplitOptions.RemoveEmptyEntries);

        return (
            JsonConvert.DeserializeObject<T1>(elements[0]), 
            JsonConvert.DeserializeObject<T2>(elements[1]), 
            JsonConvert.DeserializeObject<T3>(elements[2]), 
            JsonConvert.DeserializeObject<T4>(elements[3])
        );
    }
}

// public class SequenceFrameMappingJsonConverter : JsonConverter
// {
//     public override (int, int, int) Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//     {
//         var s = reader.GetString();
//         if (s == null || !s.StartsWith("(") || !s.EndsWith(")"))
//         {
//             throw new Exception("Invalid value, missing parens");
//         }
//
//         if (s.Count(c => c == ',') != 2)
//         {
//             throw new Exception("Invalid value, wrong number of separators");
//         }
//
//         var parts = s.Split(',').Select(x => x.Trim()).Select(int.Parse).ToList();
//         if (parts.Count != 3)
//         {
//             throw new Exception("Invalid value, wrong number of parts");
//         }
//
//         return (parts[0], parts[1], parts[2]);
//     }
//
//     public override void Write(Utf8JsonWriter writer, (int, int, int) value, JsonSerializerOptions options)
//     {
//         writer.WriteStringValue($"({value.Item1}, {value.Item2}, {value.Item3})");
//     }
// }