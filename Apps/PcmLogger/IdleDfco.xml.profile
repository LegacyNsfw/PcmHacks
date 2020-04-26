<?xml version="1.0"?>
<LogProfile xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <ParameterGroup Dpid="0xFE">
    <Parameter Name="Native RPM" DefineBy="Address" ByteCount="2" Address="0xFFA0C0">
      <Conversion Name="Raw" Expression="x/5.12" />
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
    <Parameter Name="Throttle position" DefineBy="Pid" ByteCount="1" Address="0x11">
      <Conversion Name="%" Expression="(x*100)/255" />
    </Parameter>
    <Parameter Name="DFCO Air Mode" DefineBy="Address" ByteCount="1" Address="0xFF9820">
      <Conversion Name="Mode" Expression="x" />
    </Parameter>
    <Parameter Name="DFCO Air" DefineBy="Address" ByteCount="2" Address="0xFF97E6">
      <Conversion Name="TBD" Expression="x/1024.0" />
    </Parameter>
    <Parameter Name="DFCO Airflow Disable" DefineBy="Address" ByteCount="1" Address="0xFFA4BF">
      <Conversion Name="Raw" Expression="x" />
    </Parameter>
    <Parameter Name="TBD" DefineBy="Address" ByteCount="1" Address="0xFFA0C0">
      <Conversion Name="Raw" Expression="x*0.1953125" />
    </Parameter>
  </ParameterGroup>
</LogProfile>