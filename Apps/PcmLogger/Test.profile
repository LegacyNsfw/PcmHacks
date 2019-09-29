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
                        "Name": "kpa",
                        "Expression": "x"
                    },
                    "Name": "Manifold Absolute Pressure",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xB"
                },
                {
                    "Conversion": {
                        "Name": "%",
                        "Expression": "x\/2.56"
                    },
                    "Name": "Throttle Position Sensor",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x11"
                }
            ],
            "TotalBytes": 6
        },
        {
            "Dpid": "0xFD",
            "Parameters": [
                {
                    "Conversion": {
                        "Name": "C",
                        "Expression": "x-40"
                    },
                    "Name": "Intake Air Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xF"
                },
                {
                    "Conversion": {
                        "Name": "C",
                        "Expression": "x-40"
                    },
                    "Name": "Engine Coolant Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x5"
                },
                {
                    "Conversion": {
                        "Name": "%",
                        "Expression": "(x-128)\/1.28"
                    },
                    "Name": "Left Long Term Fuel Trim",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x7"
                },
                {
                    "Conversion": {
                        "Name": "%",
                        "Expression": "(x-128)\/1.28"
                    },
                    "Name": "Right Long Term Fuel Trim",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x9"
                },
                {
                    "Conversion": {
                        "Name": "Degrees",
                        "Expression": "(x*256)\/22.5"
                    },
                    "Name": "Knock Retard",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x11A6"
                },
                {
                    "Conversion": {
                        "Name": "AFR",
                        "Expression": "x*10"
                    },
                    "Name": "Target AFR",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x119E"
                }
            ],
            "TotalBytes": 6
        }
    ]
}