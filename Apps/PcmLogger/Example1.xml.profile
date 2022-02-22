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
    <Parameter Name="PID 1105; 128=DFCO" DefineBy="Pid" ByteCount="1" Address="0x1105">
      <Conversion Name="Raw" Expression="x" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFD">
    <Parameter Name="Desired Idle Airflow" DefineBy="Pid" ByteCount="2" Address="0x1617">
      <Conversion Name="g/s" Expression="x*0.00009765625" />
    </Parameter>
    <Parameter Name="Target throttle position" DefineBy="Pid" ByteCount="2" Address="0x131E">
      <Conversion Name="%" Expression="x*0.00390625" />
    </Parameter>
    <Parameter Name="Left Long Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x7">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
    <Parameter Name="Right Long Term Fuel Trim" DefineBy="Pid" ByteCount="1" Address="0x9">
      <Conversion Name="%" Expression="(x-128)/1.28" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFC">
    <Parameter Name="Fueling Mode" DefineBy="Pid" ByteCount="2" Address="0x3">
      <Conversion Name="Mode" Expression="x" />
    </Parameter>
    <Parameter Name="Intake Air Temperature" DefineBy="Pid" ByteCount="1" Address="0xF">
      <Conversion Name="F" Expression="((x-40)*1.8)+32" />
    </Parameter>
    <Parameter Name="Engine Coolant Temperature" DefineBy="Pid" ByteCount="1" Address="0x5">
      <Conversion Name="F" Expression="((x-40)*1.8)+32" />
    </Parameter>
    <Parameter Name="Knock Retard" DefineBy="Pid" ByteCount="1" Address="0x11A6">
      <Conversion Name="Degrees" Expression="(x/256)*22.5" />
    </Parameter>
    <Parameter Name="Target AFR" DefineBy="Pid" ByteCount="1" Address="0x119E">
      <Conversion Name="AFR" Expression="x/10" />
    </Parameter>
  </ParameterGroup>
</LogProfile>