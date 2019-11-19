{
    "ParameterGroups": [
        {
            "Dpid": "0xFE",
            "Parameters": [
                {
                    "Conversion": {
                        "Name": "RPM",
                        "Expression": "x*.25"
                    },
                    "Name": "Engine Speed",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0xC"
                },
                {
                    "Conversion": {
                        "Name": "g\/s",
                        "Expression": "x\/100"
                    },
                    "Name": "Mass Air Flow",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x10"
                },
                {
                    "Conversion": {
                        "Name": "C",
                        "Expression": "x-40"
                    },
                    "Name": "Coolant Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x5"
                },
                {
                    "Conversion": {
                        "Name": "C",
                        "Expression": "x-40"
                    },
                    "Name": "Intake Air Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xF"
                }
            ],
            "TotalBytes": 6
        },
        {
            "Dpid": "0xFD",
            "Parameters": [
                {
                    "Conversion": {
                        "Name": "psi",
                        "Expression": "x"
                    },
                    "Name": "Engine Oil Pressure",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x115C"
                },
                {
                    "Conversion": {
                        "Name": "RPM",
                        "Expression": "x*12.5"
                    },
                    "Name": "Target idle speed",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1192"
                },
                {
                    "Conversion": {
                        "Name": "percent",
                        "Expression": "x/2.56"
                    },
                    "Name": "target throttle position - fail",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1464"
                },
                {
                    "Conversion": {
                        "Name": "%",
                        "Expression": "x/2.56"
                    },
                    "Name": "EGR DC",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1172"
                },
                {
                    "Conversion": {
                        "Name": "Seconds",
                        "Expression": "x"
                    },
                    "Name": "Engine Off Time - fail",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x13B5"
                },
                {
                    "Conversion": {
                        "Name": "C",
                        "Expression": "(x+40)*(256/192)"
                    },
                    "Name": "Transmission Oil Temp",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1949"
                },
            ],
            "TotalBytes": 6
        }
    ]
}