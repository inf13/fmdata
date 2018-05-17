namespace FMData.Tests
{
    public static partial class DataApiResponses
    {
        public static string SuccessfulFind() => $@"{{
    ""response"": {{
        ""data"": [
            {{
                ""fieldData"": {{
                    ""Id"": ""4"",
                    ""Name"": ""fuzzzerd"",
                    ""Created"": ""03/29/2018 15:22:09"",
                    ""Modified"": ""03/29/2018 15:22:12""
                }},
                ""portalData"": {{}},
                ""recordId"": ""4"",
                ""modId"": ""0""
            }},
            {{
                ""fieldData"": {{
                    ""Id"": ""1"",
                    ""Name"": ""Fuzzzerd Buzz"",
                    ""Created"": ""03/07/2018 16:54:34"",
                    ""Modified"": ""04/05/2018 21:34:55""
                }},
                ""portalData"": {{}},
                ""recordId"": ""1"",
                ""modId"": ""12""
            }}
        ]
    }},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulCreate(int createdId = 254) => $@"{{
    ""response"": {{""recordId"":{createdId}}},
    ""messages"":[{{""code"":""0"",""message"":""OK""}}]
}}";

        public static string SuccessfulEdit() => @"{
    ""response"": {},
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";

        public static string SuccessfulDelete() => @"{
    ""response"": {},
    ""messages"":[{""code"":""0"",""message"":""OK""}]
}";
    }
}