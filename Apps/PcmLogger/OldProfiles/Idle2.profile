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
                        "Name": "Raw",
                        "Expression": "x"
                    },
                    "Name": "PID 1105; 128=DFCO",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1105"
                },
                {
                    "Conversion": {
                        "Name": "RPM",
                        "Expression": "x*12.5"
                    },
                    "Name": "Desired Idle Speed",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1192"
                }
            ],
            "TotalBytes": 6
        },
        { 
            "Dpid": "0xFD",
            "Parameters": [
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
                        "Expression": "(x-128)\/1.28"
                    },
                    "Name": "Left Long Term Fuel Trim",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x7"
                },
                {
                    "Conversion": {
                        "Name": "Mode",
                        "Expression": "x"
                    },
                    "Name": "Fueling Mode",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x3"
                },
                {
                    "Conversion": {
                        "Name": "Degrees",
                        "Expression": "x*(100.0/255.0)"
                    },
                    "Name": "Throttle Position",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x11"
                },
            ],
            "TotalBytes": 6
        }
    ]
}