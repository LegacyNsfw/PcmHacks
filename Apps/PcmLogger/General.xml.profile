<?xml version="1.0"?>
<LogProfile xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <ParameterGroup Dpid="0xFE">
    <Parameter Name="Engine Speed" DefineBy="Pid" ByteCount="2" Address="0xC">
      <Conversion Name="RPM" Expression="x*.25" />
    </Parameter>
    <Parameter Name="Mass Air Flow" DefineBy="Pid" ByteCount="2" Address="0x10">
      <Conversion Name="g/s" Expression="x/100" />
    </Parameter>
    <Parameter Name="Manifold Absolute Pressure" DefineBy="Pid" ByteCount="1" Address="0xB">
      <Conversion Name="kpa" Expression="x" />
    </Parameter>
    <Parameter Name="Throttle Position Sensor" DefineBy="Pid" ByteCount="1" Address="0x11">
      <Conversion Name="%" Expression="x/2.56" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFD">
    <Parameter Name="Intake Air Temperature" DefineBy="Pid" ByteCount="1" Address="0xF">
      <Conversion Name="C" Expression="x-40" />
    </Parameter>
    <Parameter Name="Engine Coolant Temperature" DefineBy="Pid" ByteCount="1" Address="0x5">
      <Conversion Name="C" Expression="x-40" />
    </Parameter>
    <Parameter Name="Left Long Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x7">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
    <Parameter Name="Right Long Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x9">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
    <Parameter Name="Knock Retard" DefineBy="Pid" ByteCount="1" Address="0x11A6">
      <Conversion Name="Degrees" Expression="(x*256)/22.5" />
    </Parameter>
    <Parameter Name="Target AFR" DefineBy="Pid" ByteCount="1" Address="0x119E">
      <Conversion Name="AFR" Expression="x*10" />
    </Parameter>
  </ParameterGroup>
</LogProfile>