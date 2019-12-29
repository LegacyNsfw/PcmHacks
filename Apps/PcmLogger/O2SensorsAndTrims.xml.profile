<?xml version="1.0"?>
<LogProfile xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <ParameterGroup Dpid="0xFE">
    <Parameter Name="Engine Speed" DefineBy="Pid" ByteCount="2" Address="0xC">
      <Conversion Name="RPM" Expression="x*.25" />
    </Parameter>
    <Parameter Name="Mass Air Flow" DefineBy="Pid" ByteCount="2" Address="0x10">
      <Conversion Name="g/s" Expression="x/100" />
    </Parameter>
    <Parameter Name="Fueling Mode" DefineBy="Pid" ByteCount="2" Address="0x3">
      <Conversion Name="Mode" Expression="x" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFD">
    <Parameter Name="Left Front O2 Sensor Voltage" DefineBy="Pid" ByteCount="2" Address="0x14">
      <Conversion Name="TBD" Expression="x" />
    </Parameter>
    <Parameter Name="Right Front O2 Sensor Voltage" DefineBy="Pid" ByteCount="2" Address="0x16">
      <Conversion Name="TBD" Expression="x" />
    </Parameter>
    <Parameter Name="Throttle Position" DefineBy="Pid" ByteCount="1" Address="0x11">
      <Conversion Name="Degrees" Expression="x*(100.0/255.0)" />
    </Parameter>
    <Parameter Name="Ignition Timing" DefineBy="Pid" ByteCount="1" Address="0xE">
      <Conversion Name="Degrees" Expression="(x/2.0) - 64" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFC">
    <Parameter Name="Left Long Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x7">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
    <Parameter Name="Left Short Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x6">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
    <Parameter Name="Engine Coolant Temperature" DefineBy="Pid" ByteCount="1" Address="0x5">
      <Conversion Name="C" Expression="x-40" />
    </Parameter>
    <Parameter Name="Intake Air Temperature" DefineBy="Pid" ByteCount="1" Address="0xF">
      <Conversion Name="C" Expression="x-40" />
    </Parameter>
  </ParameterGroup>
</LogProfile>