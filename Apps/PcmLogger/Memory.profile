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
                    "Conversion" : {
                        "Name": "Raw",
                        "Expression": "0x"
                    },
                    "Name": "Address FF8800",
                    "DefineBy": 2,
                    "ByteCount": 2,
                    "Address": "0xFF8800"
                },
                {
                    "Conversion": {
                        "Name": "Raw",
                        "Expression": "0x"
                    },
                    "Name": "Address FF8802",
                    "DefineBy": 2,
                    "ByteCount": 2,
                    "Address": "0xFF8802"
                },
                {
                    "Conversion": {
                        "Name": "Raw",
                        "Expression": "0x"
                    },
                    "Name": "Ignition Advance Multiplier",
                    "DefineBy": 2,
                    "ByteCount": 2,
                    "Address": "0xFF8250"
                }
            ],
            "TotalBytes": 6
        }
    ]
}