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
    <Parameter Name="Engine Coolant Temperature" DefineBy="Pid" ByteCount="1" Address="0x5">
      <Conversion Name="F" Expression="((x-40)*1.8)+32" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFD">
    <Parameter Name="Load" DefineBy="Address" ByteCount="2" Address="0xFFAAEA">
      <Conversion Name="g/cyl" Expression="x/2048.0" />
    </Parameter>
    <Parameter Name="Throttle position" DefineBy="Pid" ByteCount="1" Address="0x11">
      <Conversion Name="%" Expression="(x*100)/255" />
    </Parameter>
    <Parameter Name="Fueling Mode" DefineBy="Pid" ByteCount="2" Address="0x3">
      <Conversion Name="Mode" Expression="x" />
    </Parameter>
    <Parameter Name="Ignition Timing" DefineBy="Pid" ByteCount="1" Address="0xE">
      <Conversion Name="degrees" Expression="(x/2)-64" />
    </Parameter>
  </ParameterGroup>
</LogProfile>