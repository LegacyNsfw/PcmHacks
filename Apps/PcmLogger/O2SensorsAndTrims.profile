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
                        "Name": "Percent",
                        "Expression": "x * 0.0234"
                    },
                    "Name": "Volumetric Efficiency",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x13B0"
                },
            ],
            "TotalBytes": 6
        },
        {
            "Dpid": "0xFD",
            "Parameters": [
                {
                    "Conversion": {
                        "Name": "TBD",
                        "Expression": "x"
                    },
                    "Name": "Left Front O2 Sensor Voltage",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x14"
                },

                {
                    "Conversion": {
                        "Name": "TBD",
                        "Expression": "x"
                    },
                    "Name": "Right Front O2 Sensor Voltage",
                    "DefineBy": 1,
                    "ByteCount": 2,
                    "Address": "0x18"
                },

                {
                    "Conversion": {
                        "Name": "Degrees",
                        "Expression": "x*0.390625"
                    },
                    "Name": "Throttle Position",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x12B1"
                },

                {
                    "Conversion": {
                        "Name": "Degrees",
                        "Expression": "(x\/2.0) - 64"
                    },
                    "Name": "Ignition Timing",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xE"
                },
			]
			"TotalBytes": 6
		},
		{
            "Dpid": "0xFC",
            "Parameters": [
			{
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
                    "Name": "Left Short Term Fuel Trim",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0x6"
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
                        "Name": "C",
                        "Expression": "x-40"
                    },
                    "Name": "Intake Air Temperature",
                    "DefineBy": 1,
                    "ByteCount": 1,
                    "Address": "0xF"
                },
			]
			"TotalBytes": 6
        }
    ]
}