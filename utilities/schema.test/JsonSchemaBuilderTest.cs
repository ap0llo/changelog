using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Sdk;

namespace schema.test
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
            public string Property1 { get; set; } = "";

            public int Property2 { get; set; }

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

        private class Class3
        {
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

        private class Class4
        {
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


        public class Class6
        {
            public Dictionary<string, string> Property { get; set; } = new Dictionary<string, string>();
        }

        [Fact]
        public void SchemaBuilder_returns_expected_schema_for_dictionary_properties_01()
        {
            // ARRANGE
            var expected = JObject.Parse($@"{{
                ""$schema"" : ""{s_SchemaNamespace}"",
                ""type"" : ""object"",
                ""properties"" : {{
                    ""property"" : {{
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


        public class Class7
        {
            public Class8? Property1 { get; set; }

        }
        public class Class8
        {
            public Dictionary<string, Class9> Property { get; set; } = new Dictionary<string, Class9>();
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
                            ""property"" : {{
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


        public class Class10
        {
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

        public class Class11
        {
            public Enum1? Property1 { get; set; }
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
    }
}
