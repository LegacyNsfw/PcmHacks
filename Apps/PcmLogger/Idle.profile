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
                    "Name": "PID 1105, 128=DFCO",
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
                        "Name": "g/s",
                        "Expression": "x*0.00009765625"
                    },
                    "Name": "Desired Idle Airflow",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x1617"
                },
                {
                    "Conversion": {
                        "Name": "%",
                        "Expression": "x*0.00390625"
                    },
                    "Name": "Target throttle position",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x131E"
                },
                {
                    "Conversion": {
                        "Name": "bogons",
                        "Expression": "x/655.0"
                    },
                    "Name": "Target throttle area (worthless)",
                    "DefineBy": 2,
                    "ByteCount": 2,
                    "Address": "0xFC40"
                },           ],
            "TotalBytes": 6
        }
    ]
}