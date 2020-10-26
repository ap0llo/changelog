using System;
using System.Collections.Generic;
using System.Text;
using Grynwald.ChangeLog.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace schema.Test
{
    public class JsonSchemaBuilderTest
    {
        private const string s_SchemaNamespace = "http://json-schema.org/draft-04/schema#";

        private void AssertEqual(JToken? expected, JToken? actual)
        {
            if (!JToken.DeepEquals(expected, actual))
            {
                var messageBuilder = new StringBuilder();
                messageBuilder.AppendLine("JToken comparison failure.");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("Expected:");
                messageBuilder.AppendLine(expected?.ToString(Formatting.Indented) ?? "<null>");
                messageBuilder.AppendLine();
                messageBuilder.AppendLine("Actual:");
                messageBuilder.AppendLine(actual?.ToString(Formatting.Indented) ?? "<null>");

                throw new XunitException(messageBuilder.ToString());
            }
        }

        private class Class1
        { }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_empty_class()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object""
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class1>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        private class Class2
        {
            [JsonSchemaDefaultValue]
            public string Property1 { get; set; } = "";

            [JsonSchemaDefaultValue]
            public int Property2 { get; set; }

            [JsonSchemaDefaultValue]
            public int? Property3 { get; set; }

            public bool Property4 { get; set; }

            public bool? Property5 { get; set; }
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_properties_of_primitive_types()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string""
                    }},
                    ""property2"" : {{
                        ""type"" : ""integer""
                    }},
                    ""property3"" : {{
                        ""type"" : ""integer""
                    }},
                    ""property4"" : {{
                        ""type"" : ""boolean""
                    }},
                    ""property5"" : {{
                        ""type"" : ""boolean""
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class2>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_includes_default_values_for_primitive_properties_with_a_JsonSchemaDefaultValue_attribute()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string"",
                        ""default"" : ""some-string""
                    }},
                    ""property2"" : {{
                        ""type"" : ""integer"",
                        ""default"": 23
                    }},
                    ""property3"" : {{
                        ""type"" : ""integer"",
                        ""default"" : 42
                    }},
                    ""property4"" : {{
                        ""type"" : ""boolean""
                    }},
                    ""property5"" : {{
                        ""type"" : ""boolean""
                    }}
                }}
            }}");

            var modelInstance = new Class2()
            {
                Property1 = "some-string",
                Property2 = 23,
                Property3 = 42
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }

        private class Class3
        {
            [JsonSchemaDefaultValue]
            public string[] Property1 { get; set; } = Array.Empty<string>();

            public int[] Property2 { get; set; } = Array.Empty<int>();
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_arrays()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"" : ""string""
                        }}
                    }},
                    ""property2"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"" : ""integer""
                        }}
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class3>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_includes_default_values_for_arrays()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"" : ""string""
                        }},
                        ""default"" : [""abc"", ""def""]
                    }},
                    ""property2"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"" : ""integer""
                        }}
                    }}
                }}
            }}");

            var modelInstance = new Class3()
            {
                Property1 = new[] { "abc", "def" }
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }


        private class Class4
        {
            [JsonSchemaDefaultValue]
            public Class5? Property { get; set; }
        }

        private class Class5
        {
            public string Property1 { get; set; } = "";
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_nested_classes()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property"" : {{
                        ""type"" : ""object"",
                        ""properties"" : {{
                            ""property1"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class4>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_includes_default_values_for_objects()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property"" : {{
                        ""type"" : ""object"",
                        ""properties"" : {{
                            ""property1"" : {{
                                ""type"" : ""string""
                            }}
                        }},
                        ""default"" : {{
                            ""property1"" : ""some-string""
                        }}
                    }}
                }}
            }}");

            var modelInstance = new Class4()
            {
                Property = new Class5()
                {
                    Property1 = "some-string"
                }
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }



        public class Class6
        {
            [JsonSchemaDefaultValue]
            public Dictionary<string, string> Property1 { get; set; } = new Dictionary<string, string>();

            public Dictionary<string, string> Property2 { get; set; } = new Dictionary<string, string>();
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_dictionary_properties_01()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""object"",
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }},
                    ""property2"" : {{
                        ""type"" : ""object"",
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class6>();

            // ASSERT
            AssertEqual(expected, schema);
        }


        [Fact]
        public void SchemaBuilder_includes_individual_default_values_for_dictionary_properties_01()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""object"",                        
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }},
                        ""properties"" : {{
                            ""key1"" : {{
                                ""type"" : ""string"",
                                ""default"" : ""value1""
                            }},
                            ""key2"" : {{
                                ""type"" : ""string"" ,
                                ""default"" : ""value2""
                            }}
                        }}
                    }},
                    ""property2"" : {{
                        ""type"" : ""object"",                        
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }}
                }}
            }}");

            var modelInstance = new Class6()
            {
                Property1 = new Dictionary<string, string>()
                {
                    { "key1", "value1" },
                    { "key2", "value2" }
                },
                // Property2's value should not be included in the schema, because it has no JsonSchemaDefaultValue attribute
                Property2 = new Dictionary<string, string>()
                {
                    { "key1", "value1" },
                }
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_does_not_include_default_value_for_empty_dictionaries()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""object"",                        
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }},
                    ""property2"" : {{
                        ""type"" : ""object"",                        
                        ""patternProperties"" : {{
                            "".*"" : {{
                                ""type"" : ""string""
                            }}
                        }}
                    }}
                }}
            }}");

            var modelInstance = new Class6()
            {
                Property1 = new Dictionary<string, string>()
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }


        public class Class7
        {
            public Class8? Property1 { get; set; }

        }
        public class Class8
        {
            [JsonSchemaDefaultValue]
            public Dictionary<string, Class9> Property1 { get; set; } = new Dictionary<string, Class9>();

            public Dictionary<string, Class9> Property2 { get; set; } = new Dictionary<string, Class9>();
        }

        public class Class9
        {
            public string Property1 { get; set; } = "";
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_dictionary_properties_02()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""object"",
                        ""properties"" : {{
                            ""property1"" : {{
                                ""type"" : ""object"",
                                ""patternProperties"" : {{
                                    "".*"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }}
                                    }}
                                }}
                            }},
                            ""property2"" : {{
                                ""type"" : ""object"",
                                ""patternProperties"" : {{
                                    "".*"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class7>();

            // ASSERT
            AssertEqual(expected, schema);
        }


        [Fact]
        public void SchemaBuilder_includes_individual_default_values_for_dictionary_properties_02()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""object"",
                        ""properties"" : {{
                            ""property1"" : {{
                                ""type"" : ""object"",
                                ""patternProperties"" : {{
                                    "".*"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }}
                                    }}
                                }},
                                ""properties"" : {{
                                    ""key1"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }},
                                        ""default"" : {{
                                            ""property1"" : ""value1""
                                        }}
                                    }},
                                    ""key2"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }},
                                        ""default"" : {{
                                            ""property1"" : ""value2""
                                        }}
                                    }}
                                }}
                            }},
                            ""property2"" : {{
                                ""type"" : ""object"",
                                ""patternProperties"" : {{
                                    "".*"" : {{
                                        ""type"" : ""object"",
                                        ""properties"" : {{
                                            ""property1"" : {{
                                                ""type"" : ""string""
                                            }}
                                        }}
                                    }}
                                }}                                
                            }}
                        }}
                    }}
                }}
            }}");

            var modelInstance = new Class7()
            {
                Property1 = new Class8()
                {
                    Property1 = new Dictionary<string, Class9>()
                    {
                        { "key1", new Class9() { Property1 = "value1" } },
                        { "key2", new Class9() { Property1 = "value2" } }
                    },
                    // Property2's value should not be included in the schema, because it has no JsonSchemaDefaultValue attribute
                    Property2 = new Dictionary<string, Class9>()
                    {
                        { "key1", new Class9() { Property1 = "value1" } }
                    }
                }
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }


        public class Class10
        {
            [JsonSchemaDefaultValue]
            public Enum1 Property1 { get; set; }
        }

        public enum Enum1
        {
            Value1,
            Value2
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_enum_properties()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            ""Value1"",
                            ""Value2""
                        ]
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class10>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_includes_default_value_for_enum_properties()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            ""Value1"",
                            ""Value2""
                        ],
                        ""default"" : ""Value2""
                    }}
                }}
            }}");

            var modelInstance = new Class10()
            {
                Property1 = Enum1.Value2
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }


        public class Class11
        {
            [JsonSchemaDefaultValue]
            public Enum1? Property1 { get; set; }

            [JsonSchemaDefaultValue]
            public Enum1? Property2 { get; set; }
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_nullable_enum_properties()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            null,
                            ""Value1"",
                            ""Value2""
                        ]
                    }},
                    ""property2"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            null,
                            ""Value1"",
                            ""Value2""
                        ]
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class11>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        [Fact]
        public void SchemaBuilder_includes_default_value_for_nullable_enum_properties()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            null,
                            ""Value1"",
                            ""Value2""
                        ],
                        ""default"" : null
                    }},
                    ""property2"" : {{
                        ""type"" : ""string"",
                        ""enum"" : [
                            null,
                            ""Value1"",
                            ""Value2""
                        ],
                        ""default"" : ""Value1""
                    }}
                }}
            }}");

            var modelInstance = new Class11()
            {
                Property1 = null,
                Property2 = Enum1.Value1
            };

            // ACT
            var schema = JsonSchemaBuilder.GetSchema(modelInstance);

            // ASSERT
            AssertEqual(expected, schema);
        }


        public class Class12
        {
            [JsonSchemaUniqueItems]
            public string[] Property1 { get; set; } = Array.Empty<string>();

            [JsonSchemaUniqueItems]
            public int[] Property2 { get; set; } = Array.Empty<int>();

            // When property has JsonSchemaUniqueItems, the schema will *not* include a "uniqueItems" setting
            public int[] Property3 { get; set; } = Array.Empty<int>();

            [JsonSchemaUniqueItems] // Attribute will be ignored when property type is not an array
            public int Property4 { get; set; }
        }

        [Fact]
        public void SchemaBuilder_includes_unique_items_setting_if_a_property_has_a_JsonSchemaUniqueItems_attribute()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"": ""string""
                        }},
                        ""uniqueItems"" : true
                    }},
                    ""property2"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"": ""integer""
                        }},
                        ""uniqueItems"" : true
                    }},
                    ""property3"" : {{
                        ""type"" : ""array"",
                        ""items"" : {{
                            ""type"": ""integer""
                        }}
                    }},
                    ""property4"" : {{
                        ""type"" : ""integer""
                    }}
                }}
            }}");


            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class12>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        private class Class13
        {
            public string Property1 { get; set; } = "";

            [JsonSchemaPropertyName("someName")]
            public string Property2 { get; set; } = "";
        }

        [Fact]
        public void SchemaBuilder_uses_property_name_from_JsonSchemaPropertyName_attribute()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string""
                    }},
                    ""someName"" : {{
                        ""type"" : ""string""
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class13>();

            // ASSERT
            AssertEqual(expected, schema);
        }

        private class Class14
        {
            public string Property1 { get; set; } = "";

            [JsonSchemaIgnore]
            public string Property2 { get; set; } = "";
        }

        [Fact]
        public void SchemaBuilder_ignores_properties_with_a_JsonSchemaIgnore_attribute()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property1"" : {{
                        ""type"" : ""string""
                    }}
                }}
            }}");

            // ACT
            var schema = JsonSchemaBuilder.GetSchema<Class14>();

            // ASSERT
            AssertEqual(expected, schema);
        }
    }
}
