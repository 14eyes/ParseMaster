using System.Globalization;
using Newtonsoft.Json;

namespace ParseMaster;

public class Parser
{
    private readonly Dictionary<string, Dictionary<uint, string>> _typeIndex;
    private readonly Dictionary<string, Dictionary<string, string>> _typeMap = new();
    private readonly Dictionary<string, Dictionary<string, string>> _enumMap = new();
    private readonly Dictionary<string, string> _baseMap = new();
    private readonly List<string> _excelNames = new();
    private readonly List<string> _noZig = new();
    public const string ConfigNamespace = "MoleMole.Config";

    public Parser(string configFile, string enumFile, string typeIndexFile)
    {
        _typeIndex =
            JsonConvert.DeserializeObject<Dictionary<string, Dictionary<uint, string>>>(File.ReadAllText(typeIndexFile))
            !;
        var configInputFile = File.ReadAllLines(configFile);
        var enumInputFile = File.ReadAllLines(enumFile);

        var tmp = new KeyValuePair<string, Dictionary<string, string>>();
        foreach (var lineStr in configInputFile)
        {
            var line = lineStr.Trim();

            if (line.StartsWith("nozig "))
            {
                line = line[6..];
                _noZig.Add(line.Split(' ')[2]);
            }

            if (line.StartsWith("excel "))
            {
                line = line[6..];
                _excelNames.Add(line.Split(' ')[1]);
            }

            if (line.StartsWith("class"))
            {
                if (line.Contains(':'))
                {
                    var className = line.Split(' ')[1];
                    var baseClassName = line.Split(':')[1].Split(' ')[1];
                    _baseMap.Add(className, baseClassName);
                }

                tmp = new(line.Split(' ')[1], new());
                continue;
            }

            if (line.StartsWith("}"))
            {
                _typeMap.Add(tmp.Key, tmp.Value);
                continue;
            }

            var key = line.Split(" ")[1];
            var value = line.Split(" ")[0];
            tmp.Value.Add(key, value);
        }

        foreach (var lineStr in enumInputFile)
        {
            var line = lineStr.Trim();
            if (line.StartsWith("excel enum"))
            {
                line = line[6..];
                _excelNames.Add(line.Split(' ')[1]);
            }

            if (line.StartsWith("enum"))
            {
                tmp = new(line.Split(' ')[1], new());
                continue;
            }

            if (line.StartsWith("}"))
            {
                _enumMap.Add(tmp.Key, tmp.Value);
                continue;
            }

            var key = line.Split(" = ")[1];
            var value = line.Split(" = ")[0];
            tmp.Value.Add(key, value);
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Loaded {_typeMap.Count} Config types & {_enumMap.Count} enums");
    }

    internal string ParseFile(string filename, string classname, FileType mode, bool derivation)
    {
        var reader = new DeReader(filename);
        var multiple = !mode.Equals(FileType.Single);
        classname = $"{ConfigNamespace}.{classname}";
        if (!multiple)
            return ParseClass(classname, reader, derivation);

        ulong length = mode switch
        {
            FileType.List => reader.ReadVarUInt(),
            FileType.ListDictionary => reader.ReadVarUInt(),
            FileType.Packed => (ulong)reader.ReadVarInt(),
            _ => 1
        };
        Logger.WriteLine($"Number of elements in file: {length}");

        var items = mode switch
        {
            FileType.ListDictionary => Enumerable.Range(0, (int)length).Select(_ => ParseDictionary(reader, "string", classname)).ToList(),
            FileType.List => Enumerable.Range(0, (int)length).Select(_ => ParseClass(classname, reader, derivation)).ToList(),
            FileType.Dictionary => new() { ParseDictionary(reader, "string", classname) },
            FileType.DictionaryList => new() { ParseDictionary(reader, "string", $"{classname}[]") },
            FileType.DictionaryVuit => new() { ParseDictionary(reader, "vuint", classname) },
            FileType.DictionaryVuitVuit => new() { ParseDictionary(reader, "vuint", "vuint") },
            FileType.Packed => Enumerable.Range(0, (int)length).Select(_ => $"{{{string.Join(",", ParseClassInt(classname, reader, _baseMap.GetValueOrDefault(classname)))}}}").ToList(),
            _ => throw new InvalidDataException($"Invalid mode {mode}")
        };
        var len = reader.LenToEof();
        if (len is not 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{filename} not fully read {len} bytes left!!");
        }

        return $"[{string.Join(",", items)}]";
    }

    private string ParseDictionary(DeReader reader, string keyType, string valueType)
    {
        var size = reader.ReadVarUInt();
        Logger.WriteLine($"Dict size: {size}");

        var items = new List<string>();
        for (var i = 0u; i < size; i++)
        {
            var key = ParseFieldType(keyType, reader);
            Logger.WriteLine($"Parsing key {key} with type {valueType}");

            if (!key.Contains('"'))
            {
                key = $"\"{key}\"";
            }

            items.Add($"{key}: {ParseFieldType(valueType, reader)}");
        }

        return $"{{{string.Join(",", items)}}}";
    }

    private string ParseClass(string classname, DeReader reader, bool derivation)
    {
        var output = new List<string>();
        string? derivedClass = null;
        var rootClassname = classname;

        if (derivation && HasDerivedClasses(classname))
        {
            rootClassname = GetBasestBase(classname);
            if (_typeIndex.TryGetValue(rootClassname, out var typeIndex))
            {
                var classId = reader.ReadVarUInt();
                if (!typeIndex.TryGetValue((uint)classId, out derivedClass))
                {
                    throw new MissingMemberException($"Derived class for {classname} (id {classId}) not found!");
                }

                Logger.WriteLine($"Deriving class {classId} ({derivedClass})");
            }
        }

        if (derivedClass is null)
        {
            if (!classname.EndsWith(".Vector") && !_excelNames.Contains($"{classname}"))
            {
                output.Add($"\"$type\": \"{classname[(ConfigNamespace.Length + 1)..]}\"");
            }

            rootClassname = GetBasestBase(classname);
            output.AddRange(ParseClassInt(classname, reader, rootClassname));
        }
        else
        {
            output.Add($"\"$type\": \"{derivedClass[(ConfigNamespace.Length + 1)..]}\"");
            output.AddRange(ParseClassInt(derivedClass, reader, rootClassname));
        }

        return $"{{{string.Join(",", output)}}}";
    }

    private bool HasDerivedClasses(string classname)
    {
        return _baseMap.Any(x => x.Value.Equals(classname));
    }

    public string GetBasestBase(string className)
    {
        var baseClass = className;

        while (_baseMap.TryGetValue(baseClass, out var nextBaseClass))
        {
            baseClass = nextBaseClass;
        }

        return baseClass;
    }

    private Dictionary<string, string> MergeFields(Dictionary<string, string> fields, string className)
    {
        if (!_baseMap.TryGetValue(className, out var baseType))
        {
            return fields;
        }

        if (!_typeMap.TryGetValue(baseType, out var baseFields))
        {
            throw new InvalidDataException($"Base class {baseType} not found!");
        }

        // recursively merge base class fields
        baseFields = MergeFields(baseFields, baseType);

        var mergedFields = new Dictionary<string, string>(fields);
        foreach (var (fieldName, fieldType) in baseFields)
        {
            if (!fields.ContainsKey(fieldName))
            {
                mergedFields.Add(fieldName, fieldType);
            }
        }

        return mergedFields;
    }

    private IEnumerable<string> ParseClassInt(string classname, DeReader reader, string? baseClassName)
    {
        var output = new List<string>();

        if (!_typeMap.TryGetValue($"{classname}", out var fields))
        {
            throw new InvalidDataException($"Class {classname} not found!");
        }

        var hasBase = baseClassName is not null && !classname.Equals(baseClassName);
        var isExcel = _excelNames.Contains(classname);

        if (hasBase && !isExcel && _baseMap.TryGetValue(classname, out var baseType))
        {
            output.AddRange(ParseClassInt(baseType, reader, baseClassName));
        }

        if (hasBase && isExcel)
        {
            fields = MergeFields(fields, classname!);
        }

        Logger.WriteLine($"Parsing Class {classname} ({fields.Count} fields)");

        if (fields.Count > 0)
        {
            var bm = _excelNames.Contains(classname)
                ? new(reader)
                : new BitMask(reader, fields.Count <= 8);

            var j = 0;
            foreach (var (fieldName, fieldType) in fields)
            {
                if (bm.TestBit(j))
                {
                    Logger.Write($"Field (#{j}) {fieldName}, type {fieldType} = ");
                    var ret = ParseFieldType(fieldType, reader);
                    Logger.WriteLine("");

                    output.Add($"\"{fieldName}\": {ret}");
                }
                else
                {
                    Logger.WriteLine($"Skipping field (#{j}) {fieldName}");
                }

                // HACK: two hash fields are treated like one
                if (!fieldName.EndsWith("HashSuffix"))
                {
                    j++;
                }
            }
        }

        return output;
    }

    private string ParseFieldType(string fieldType, DeReader reader)
    {
        switch (fieldType)
        {
            case { } when fieldType.EndsWith("[]"):
                var items = new List<string>();
                fieldType = fieldType[..^2];
                var length = _excelNames.Contains(fieldType) && !_noZig.Contains(fieldType)
                    ? (ulong)reader.ReadVarInt()
                    : reader.ReadVarUInt();
                Logger.Write($"({length}) [");
                for (uint i = 0; i < length; i++)
                {
                    items.Add(ParseFieldType(fieldType, reader));
                    Logger.Write(" ");
                }

                Logger.Write("]");
                return $"[{string.Join(",", items)}]";
            case { } when _enumMap.ContainsKey(fieldType):
                var value = IsEnumSigned(fieldType) ? reader.ReadVarInt() : (long)reader.ReadVarUInt();
                var sValue = _enumMap[fieldType][value.ToString()];
                Logger.Write(sValue);
                return $"\"{sValue}\"";
            case "string":
                var stringValue = reader.ReadString();
                Logger.Write(stringValue);
                return $"\"{stringValue}\"";
            case "vuint":
                var vuintValue = reader.ReadVarUInt();
                Logger.Write(vuintValue);
                return vuintValue.ToString();
            case "vint":
                var vintValue = reader.ReadVarInt();
                Logger.Write(vintValue);
                return vintValue.ToString();
            case "byte":
                var byteValue = reader.ReadU8();
                Logger.Write(byteValue);
                return byteValue.ToString();
            case "bool":
                var boolValue = reader.ReadBool();
                Logger.Write(boolValue);
                return boolValue.ToString().ToLower();
            case "float":
                var f32Value = reader.ReadF32();
                Logger.Write(f32Value);
                return FormatFloat(f32Value);
            case "double":
                var f64Value = reader.ReadF64();
                Logger.Write(f64Value);
                return FormatFloat(f64Value);
            case $"{ConfigNamespace}.DynamicFloat":
                var dynamicFloatValue = ReadDynamicFloat(reader);
                Logger.Write(dynamicFloatValue);
                return dynamicFloatValue;
            case $"{ConfigNamespace}.DynamicInt":
                var dynamicIntValue = ReadDynamicInt(reader);
                Logger.Write(dynamicIntValue);
                return dynamicIntValue;
            case $"{ConfigNamespace}.DynamicArgument":
                var dynamicArgumentValue = ReadDynamicArgument(reader);
                Logger.Write(dynamicArgumentValue);
                return dynamicArgumentValue;
            case $"{ConfigNamespace}.DynamicString":
                var isDynamic = reader.ReadBool();
                if (isDynamic)
                {
                }

                var dynamicStringValue = reader.ReadString();
                Logger.Write(dynamicStringValue);
                return $"\"{dynamicStringValue}\"";
            case { } when fieldType.StartsWith($"{ConfigNamespace}."):
                return ParseClass(fieldType, reader, true);
            case { } when fieldType.StartsWith("map<"):
                var typeArgs = fieldType
                    .Substring(fieldType.IndexOf('<') + 1, fieldType.LastIndexOf('>') - fieldType.IndexOf('<') - 1)
                    .Split(',');

                var keyType = typeArgs[0];
                var valueType = typeArgs[1];
                if (typeArgs.Length == 3)
                {
                    valueType += $",{typeArgs[2]}";
                }

                return ParseDictionary(reader, keyType, valueType);
            default:
                Logger.WriteLine($"Fucked up type of {fieldType}");
                throw new InvalidOperationException($"Type {fieldType} is not supported");
        }
    }

    private static string ReadDynamicArgument(DeReader reader)
    {
        // Credit goes to Raz
        var typeIndex = reader.ReadVarUInt();
        return typeIndex switch
        {
            1 => reader.ReadS8().ToString(),
            2 => reader.ReadU8().ToString(),
            3 => reader.ReadS16().ToString(),
            4 => reader.ReadU16().ToString(),
            5 => reader.ReadS32().ToString(),
            6 => reader.ReadU32().ToString(),
            7 => FormatFloat(reader.ReadF32()),
            8 => FormatFloat(reader.ReadF64()),
            9 => reader.ReadBool().ToString().ToLower(),
            10 => $"\"{reader.ReadString()}\"",
            _ => throw new InvalidDataException($"Unhandled DynamicArgument type {typeIndex}!")
        };
    }

    private static string ReadDynamicInt(DeReader reader)
    {
        // Credit goes to Raz
        var isString = reader.ReadBool();
        return isString ? $"\"{reader.ReadString()}\"" : reader.ReadVarInt().ToString();
    }

    private static string ReadDynamicFloat(DeReader reader)
    {
        // Credit goes to Raz
        var isFormula = reader.ReadBool();
        if (isFormula)
        {
            var count = reader.ReadVarInt();
            var components = new List<string>();

            for (var i = 0; i < count; i++)
            {
                var isOperator = reader.ReadBool();

                if (isOperator)
                {
                    var op = reader.ReadVarInt();
                    var sOp = op switch
                    {
                        0 => "Add",
                        1 => "Sub",
                        11 => "Mul",
                        12 => "Div",
                        _ => op.ToString()
                    };
                    components.Add($"\" {sOp} \"");
                }
                else
                {
                    var isString = reader.ReadBool();
                    components.Add(isString
                        ? $"\"{reader.ReadString()}\""
                        : reader.ReadF32().ToString(CultureInfo.InvariantCulture));
                }
            }

            return $"[{string.Join(",", components)}]";
        }

        {
            var isString = reader.ReadBool();
            return isString ? $"\"{reader.ReadString()}\"" : reader.ReadF32().ToString(CultureInfo.InvariantCulture);
        }
    }

    private static string FormatFloat(object value) => $"{value:0.0############}";
    private bool IsEnumSigned(string s) => bool.Parse(_enumMap[s].Single(x => x.Value.Equals("__signed")).Key);
}