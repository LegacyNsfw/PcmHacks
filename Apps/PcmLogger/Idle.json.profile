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
                        "Name": "Raw",
                        "Expression": "x"
                    },
                    "Name": "PID 1105; 128=DFCO",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x1105"
                },
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
			],
            "TotalBytes": 6
		},
        { 
            "Dpid": "0xFC",
            "Parameters": [
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
                        "Name": "F",
                        "Expression": "((x-40)*1.8)+32"
                    },
                    "Name": "Intake Air Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xF"
                },
                {
                    "Conversion": {
                        "Name": "F",
                        "Expression": "((x-40)*1.8)+32"
                    },
                    "Name": "Engine Coolant Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x5"
                },
				{
                    "Conversion": {
                        "Name": "Degrees",
                        "Expression": "(x\/256)*22.5"
                    },
                    "Name": "Knock Retard",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x11A6"
                },
                {
                    "Conversion": {
                        "Name": "AFR",
                        "Expression": "x\/10"
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