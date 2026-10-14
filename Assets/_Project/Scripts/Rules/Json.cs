using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace IronVeil.Rules
{
    public enum JsonKind
    {
        Null = 0,
        Bool = 1,
        Number = 2,
        String = 3,
        Array = 4,
        Object = 5
    }

    public sealed class JsonParseException : Exception
    {
        public int Position { get; }

        public JsonParseException(string message, int position)
            : base(message + " (at character " + position + ")")
        {
            Position = position;
        }
    }

    /// <summary>
    /// Small JSON DOM used by the save system. Hand written on purpose: the rules
    /// assembly cannot use Unity's JsonUtility, and a save format that survives
    /// version migrations needs explicit control over encoding.
    /// </summary>
    public sealed class JsonValue
    {
        private readonly List<JsonValue> _array;
        private readonly Dictionary<string, JsonValue> _object;
        // Insertion order is kept so serialisation is byte-for-byte deterministic,
        // which the save-file checksum depends on.
        private readonly List<string> _objectOrder;
        private readonly bool _boolValue;
        private readonly double _numberValue;
        private readonly string _stringValue;

        private JsonValue(JsonKind kind, bool boolValue = false, double numberValue = 0d, string stringValue = null)
        {
            Kind = kind;
            _boolValue = boolValue;
            _numberValue = numberValue;
            _stringValue = stringValue;
            if (kind == JsonKind.Array) _array = new List<JsonValue>();
            if (kind == JsonKind.Object)
            {
                _object = new Dictionary<string, JsonValue>(StringComparer.Ordinal);
                _objectOrder = new List<string>();
            }
        }

        public JsonKind Kind { get; }

        public static JsonValue Null() => new JsonValue(JsonKind.Null);

        public static JsonValue From(bool value) => new JsonValue(JsonKind.Bool, boolValue: value);

        public static JsonValue From(double value) => new JsonValue(JsonKind.Number, numberValue: value);

        public static JsonValue From(int value) => new JsonValue(JsonKind.Number, numberValue: value);

        public static JsonValue From(float value) => new JsonValue(JsonKind.Number, numberValue: value);

        public static JsonValue From(string value)
        {
            return value == null ? Null() : new JsonValue(JsonKind.String, stringValue: value);
        }

        public static JsonValue NewArray() => new JsonValue(JsonKind.Array);

        public static JsonValue NewObject() => new JsonValue(JsonKind.Object);

        public bool IsNull => Kind == JsonKind.Null;

        public int Count => Kind == JsonKind.Array ? _array.Count : Kind == JsonKind.Object ? _object.Count : 0;

        public IReadOnlyList<JsonValue> Items => _array;

        public IEnumerable<string> Keys => _objectOrder == null ? (IEnumerable<string>)Array.Empty<string>() : _objectOrder;

        public JsonValue this[int index]
        {
            get
            {
                if (_array == null || index < 0 || index >= _array.Count) return Null();
                return _array[index];
            }
        }

        public JsonValue this[string key]
        {
            get
            {
                if (_object == null || string.IsNullOrEmpty(key)) return Null();
                return _object.TryGetValue(key, out JsonValue value) ? value : Null();
            }
        }

        public bool HasKey(string key) => _object != null && key != null && _object.ContainsKey(key);

        public JsonValue Add(JsonValue value)
        {
            _array?.Add(value ?? Null());
            return this;
        }

        public JsonValue Add(string key, JsonValue value)
        {
            if (_object == null || string.IsNullOrEmpty(key)) return this;
            if (!_object.ContainsKey(key)) _objectOrder.Add(key);
            _object[key] = value ?? Null();
            return this;
        }

        public JsonValue Add(string key, string value) => Add(key, From(value));

        public JsonValue Add(string key, int value) => Add(key, From(value));

        public JsonValue Add(string key, float value) => Add(key, From(value));

        public JsonValue Add(string key, bool value) => Add(key, From(value));

        public string AsString(string fallback = "") =>
            Kind == JsonKind.String ? _stringValue : Kind == JsonKind.Number
                ? _numberValue.ToString(CultureInfo.InvariantCulture)
                : Kind == JsonKind.Bool ? (_boolValue ? "true" : "false") : fallback;

        public double AsDouble(double fallback = 0d) =>
            Kind == JsonKind.Number ? _numberValue : Kind == JsonKind.Bool ? (_boolValue ? 1d : 0d) : fallback;

        public float AsFloat(float fallback = 0f) => (float)AsDouble(fallback);

        public int AsInt(int fallback = 0)
        {
            if (Kind != JsonKind.Number) return Kind == JsonKind.Bool ? (_boolValue ? 1 : 0) : fallback;
            if (double.IsNaN(_numberValue) || double.IsInfinity(_numberValue)) return fallback;
            if (_numberValue > int.MaxValue || _numberValue < int.MinValue) return fallback;
            return (int)Math.Round(_numberValue, MidpointRounding.AwayFromZero);
        }

        public bool AsBool(bool fallback = false)
        {
            switch (Kind)
            {
                case JsonKind.Bool: return _boolValue;
                case JsonKind.Number: return Math.Abs(_numberValue) > double.Epsilon;
                case JsonKind.String: return string.Equals(_stringValue, "true", StringComparison.OrdinalIgnoreCase);
                default: return fallback;
            }
        }

        public List<JsonValue> AsList() => _array == null ? new List<JsonValue>() : new List<JsonValue>(_array);

        public List<string> AsStringList()
        {
            List<string> result = new List<string>();
            if (_array == null) return result;
            for (int i = 0; i < _array.Count; i++) result.Add(_array[i].AsString());
            return result;
        }

        public string ToJson(bool pretty = false)
        {
            StringBuilder builder = new StringBuilder(256);
            Write(builder, this, pretty, 0);
            return builder.ToString();
        }

        private static void Write(StringBuilder builder, JsonValue value, bool pretty, int depth)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            switch (value.Kind)
            {
                case JsonKind.Null:
                    builder.Append("null");
                    break;
                case JsonKind.Bool:
                    builder.Append(value._boolValue ? "true" : "false");
                    break;
                case JsonKind.Number:
                    builder.Append(FormatNumber(value._numberValue));
                    break;
                case JsonKind.String:
                    WriteString(builder, value._stringValue);
                    break;
                case JsonKind.Array:
                    WriteArray(builder, value, pretty, depth);
                    break;
                case JsonKind.Object:
                    WriteObject(builder, value, pretty, depth);
                    break;
            }
        }

        private static void WriteArray(StringBuilder builder, JsonValue value, bool pretty, int depth)
        {
            if (value._array.Count == 0)
            {
                builder.Append("[]");
                return;
            }

            builder.Append('[');
            for (int i = 0; i < value._array.Count; i++)
            {
                if (i > 0) builder.Append(',');
                if (pretty) NewLine(builder, depth + 1);
                Write(builder, value._array[i], pretty, depth + 1);
            }
            if (pretty) NewLine(builder, depth);
            builder.Append(']');
        }

        private static void WriteObject(StringBuilder builder, JsonValue value, bool pretty, int depth)
        {
            if (value._object.Count == 0)
            {
                builder.Append("{}");
                return;
            }

            builder.Append('{');
            for (int i = 0; i < value._objectOrder.Count; i++)
            {
                string key = value._objectOrder[i];
                if (i > 0) builder.Append(',');
                if (pretty) NewLine(builder, depth + 1);
                WriteString(builder, key);
                builder.Append(':');
                if (pretty) builder.Append(' ');
                Write(builder, value._object[key], pretty, depth + 1);
            }
            if (pretty) NewLine(builder, depth);
            builder.Append('}');
        }

        private static void NewLine(StringBuilder builder, int depth)
        {
            builder.Append('\n');
            for (int i = 0; i < depth; i++) builder.Append("  ");
        }

        private static string FormatNumber(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return "0";
            if (value == Math.Floor(value) && Math.Abs(value) < 1e15)
            {
                return ((long)value).ToString(CultureInfo.InvariantCulture);
            }
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        private static void WriteString(StringBuilder builder, string value)
        {
            builder.Append('"');
            if (!string.IsNullOrEmpty(value))
            {
                for (int i = 0; i < value.Length; i++)
                {
                    char c = value[i];
                    switch (c)
                    {
                        case '"': builder.Append("\\\""); break;
                        case '\\': builder.Append("\\\\"); break;
                        case '\b': builder.Append("\\b"); break;
                        case '\f': builder.Append("\\f"); break;
                        case '\n': builder.Append("\\n"); break;
                        case '\r': builder.Append("\\r"); break;
                        case '\t': builder.Append("\\t"); break;
                        default:
                            if (c < ' ')
                            {
                                builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                builder.Append(c);
                            }
                            break;
                    }
                }
            }
            builder.Append('"');
        }

        // ---------------- Parsing ----------------

        public static JsonValue Parse(string text)
        {
            if (!TryParse(text, out JsonValue value, out string error))
            {
                throw new JsonParseException(error, -1);
            }
            return value;
        }

        public static bool TryParse(string text, out JsonValue value, out string error)
        {
            value = null;
            error = null;
            if (string.IsNullOrEmpty(text))
            {
                error = "empty input";
                return false;
            }

            Parser parser = new Parser(text);
            try
            {
                parser.SkipWhitespace();
                value = parser.ReadValue();
                parser.SkipWhitespace();
                if (!parser.AtEnd)
                {
                    error = "trailing content after the root value";
                    value = null;
                    return false;
                }
            }
            catch (JsonParseException exception)
            {
                error = exception.Message;
                value = null;
                return false;
            }

            return true;
        }

        private sealed class Parser
        {
            private readonly string _text;
            private int _index;

            public Parser(string text)
            {
                _text = text;
                _index = 0;
            }

            public bool AtEnd => _index >= _text.Length;

            public void SkipWhitespace()
            {
                while (_index < _text.Length)
                {
                    char c = _text[_index];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r') _index++;
                    else break;
                }
            }

            public JsonValue ReadValue()
            {
                SkipWhitespace();
                if (AtEnd) throw new JsonParseException("unexpected end of input", _index);

                char c = _text[_index];
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return From(ReadString());
                    case 't': ExpectLiteral("true"); return From(true);
                    case 'f': ExpectLiteral("false"); return From(false);
                    case 'n': ExpectLiteral("null"); return Null();
                    default: return From(ReadNumber());
                }
            }

            private JsonValue ReadObject()
            {
                JsonValue result = NewObject();
                _index++; // {
                SkipWhitespace();
                if (!AtEnd && _text[_index] == '}')
                {
                    _index++;
                    return result;
                }

                while (true)
                {
                    SkipWhitespace();
                    if (AtEnd || _text[_index] != '"')
                    {
                        throw new JsonParseException("expected a property name", _index);
                    }
                    string key = ReadString();
                    SkipWhitespace();
                    if (AtEnd || _text[_index] != ':')
                    {
                        throw new JsonParseException("expected ':' after property name", _index);
                    }
                    _index++;
                    JsonValue value = ReadValue();
                    result.Add(key, value);

                    SkipWhitespace();
                    if (AtEnd) throw new JsonParseException("unterminated object", _index);
                    if (_text[_index] == ',')
                    {
                        _index++;
                        continue;
                    }
                    if (_text[_index] == '}')
                    {
                        _index++;
                        return result;
                    }
                    throw new JsonParseException("expected ',' or '}'", _index);
                }
            }

            private JsonValue ReadArray()
            {
                JsonValue result = NewArray();
                _index++; // [
                SkipWhitespace();
                if (!AtEnd && _text[_index] == ']')
                {
                    _index++;
                    return result;
                }

                while (true)
                {
                    JsonValue value = ReadValue();
                    result.Add(value);
                    SkipWhitespace();
                    if (AtEnd) throw new JsonParseException("unterminated array", _index);
                    if (_text[_index] == ',')
                    {
                        _index++;
                        continue;
                    }
                    if (_text[_index] == ']')
                    {
                        _index++;
                        return result;
                    }
                    throw new JsonParseException("expected ',' or ']'", _index);
                }
            }

            private string ReadString()
            {
                _index++; // opening quote
                StringBuilder builder = new StringBuilder();
                while (true)
                {
                    if (AtEnd) throw new JsonParseException("unterminated string", _index);
                    char c = _text[_index++];
                    if (c == '"') return builder.ToString();

                    if (c != '\\')
                    {
                        builder.Append(c);
                        continue;
                    }

                    if (AtEnd) throw new JsonParseException("unterminated escape sequence", _index);
                    char escape = _text[_index++];
                    switch (escape)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u':
                            if (_index + 4 > _text.Length)
                            {
                                throw new JsonParseException("truncated unicode escape", _index);
                            }
                            string hex = _text.Substring(_index, 4);
                            if (!int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                            {
                                throw new JsonParseException("invalid unicode escape '" + hex + "'", _index);
                            }
                            builder.Append((char)code);
                            _index += 4;
                            break;
                        default:
                            throw new JsonParseException("invalid escape '\\" + escape + "'", _index);
                    }
                }
            }

            private double ReadNumber()
            {
                int start = _index;
                if (!AtEnd && (_text[_index] == '-' || _text[_index] == '+')) _index++;

                bool sawDigit = false;
                while (!AtEnd && _text[_index] >= '0' && _text[_index] <= '9')
                {
                    _index++;
                    sawDigit = true;
                }
                if (!AtEnd && _text[_index] == '.')
                {
                    _index++;
                    while (!AtEnd && _text[_index] >= '0' && _text[_index] <= '9')
                    {
                        _index++;
                        sawDigit = true;
                    }
                }
                if (!AtEnd && (_text[_index] == 'e' || _text[_index] == 'E'))
                {
                    _index++;
                    if (!AtEnd && (_text[_index] == '-' || _text[_index] == '+')) _index++;
                    while (!AtEnd && _text[_index] >= '0' && _text[_index] <= '9') _index++;
                }

                if (!sawDigit)
                {
                    throw new JsonParseException("invalid number", start);
                }

                string slice = _text.Substring(start, _index - start);
                if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed))
                {
                    throw new JsonParseException("invalid number '" + slice + "'", start);
                }
                return parsed;
            }

            private void ExpectLiteral(string literal)
            {
                if (_index + literal.Length > _text.Length ||
                    string.CompareOrdinal(_text, _index, literal, 0, literal.Length) != 0)
                {
                    throw new JsonParseException("expected '" + literal + "'", _index);
                }
                _index += literal.Length;
            }
        }
    }

    /// <summary>FNV-1a 32-bit hash, used for save integrity checks.</summary>
    public static class Checksum
    {
        public static uint Fnv1a32(string text)
        {
            unchecked
            {
                const uint offset = 2166136261u;
                const uint prime = 16777619u;
                uint hash = offset;
                if (!string.IsNullOrEmpty(text))
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        char c = text[i];
                        hash = (hash ^ (byte)(c & 0xFF)) * prime;
                        hash = (hash ^ (byte)(c >> 8)) * prime;
                    }
                }
                return hash;
            }
        }

        public static string Fnv1a32Hex(string text) => Fnv1a32(text).ToString("x8", CultureInfo.InvariantCulture);
    }
}
