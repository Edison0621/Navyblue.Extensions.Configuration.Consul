// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Navyblue.Extension.Configuration.Consul
{
    internal class JsonConfigurationFileParser
    {
        private readonly Stack<string> _context = new Stack<string>();

        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private string _currentPath;

        private JsonConfigurationFileParser()
        {
        }

        public static IDictionary<string, string> Parse(string serviceKey, Stream input)
            => new JsonConfigurationFileParser().ParseStream(serviceKey, input);

        private void EnterContext(string context)
        {
            this._context.Push(context);
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse());
        }

        private void ExitContext()
        {
            this._context.Pop();
            this._currentPath = ConfigurationPath.Combine(this._context.Reverse());
        }

        private IDictionary<string, string> ParseStream(string serviceKey, Stream input)
        {
            this._data.Clear();

            using (StreamReader reader = new StreamReader(input))
            using (JsonDocument doc = JsonDocument.Parse(reader.ReadToEnd(), new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }))
            {
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    throw new FormatException(Resources.FormatError_UnsupportedJSONToken(doc.RootElement.ValueKind));
                }

                this.VisitElement(serviceKey, doc.RootElement);
            }

            return this._data;
        }

        private void VisitElement(string serviceKey, JsonElement element)
        {
            foreach (JsonProperty property in element.EnumerateObject())
            {
                this.EnterContext(property.Name);
                this.VisitValue(serviceKey, property.Value);
                this.ExitContext();
            }
        }

        private void VisitValue(string serviceKey, JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Object:
                    this.VisitElement(serviceKey, value);
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach (JsonElement arrayElement in value.EnumerateArray())
                    {
                        this.EnterContext(index.ToString());
                        this.VisitValue(serviceKey, arrayElement);
                        this.ExitContext();
                        index++;
                    }

                    break;

                case JsonValueKind.Number:
                case JsonValueKind.String:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string key = serviceKey + ":" + this._currentPath;
                    if (this._data.ContainsKey(key))
                    {
                        throw new FormatException(Resources.FormatError_KeyIsDuplicated(key));
                    }

                    this._data[key] = value.ToString();
                    break;

                default:
                    throw new FormatException(Resources.FormatError_UnsupportedJSONToken(value.ValueKind));
            }
        }
    }
}