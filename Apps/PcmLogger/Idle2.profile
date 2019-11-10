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
<<<<<<< HEAD
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
=======
                        "Name": "Raw",
                        "Expression": "x"
                    },
                    "Name": "PID 1105, 128=DFCO",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xF"
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
>>>>>>> e60630d549e7f1d3361d9c6a14104d95915d57c0
                }
            ],
            "TotalBytes": 6
        },
<<<<<<< HEAD
        {
=======
        { 
>>>>>>> e60630d549e7f1d3361d9c6a14104d95915d57c0
            "Dpid": "0xFD",
            "Parameters": [
                {
                    "Conversion": {
<<<<<<< HEAD
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
=======
                        "Name": "g/s",
                        "Expression": "x*0.00009765625"
                    },
                    "Name": "Desired Idle Airflow",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x1617"
>>>>>>> e60630d549e7f1d3361d9c6a14104d95915d57c0
                },
                {
                    "Conversion": {
                        "Name": "%",
<<<<<<< HEAD
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
                    "ByteCount": 1,
                    "Address": "0x3"
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
                        "Name": "AFR",
                        "Expression": "x*10"
                    },
                    "Name": "Target AFR",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x119E"
                }
            ],
=======
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
                    "Name": "Target throttle area",
                    "DefineBy": 2,
                    "ByteCount": 2,
                    "Address": "0xFC40"
                },           ],
>>>>>>> e60630d549e7f1d3361d9c6a14104d95915d57c0
            "TotalBytes": 6
        }
    ]
}