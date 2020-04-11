<?xml version="1.0"?>
<LogProfile xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <ParameterGroup Dpid="0xFE">
    <Parameter Name="Engine Speed" DefineBy="Pid" ByteCount="2" Address="0xC">
      <Conversion Name="RPM" Expression="x*.25" />
    </Parameter>
    <Parameter Name="Mass Air Flow" DefineBy="Pid" ByteCount="2" Address="0x10">
      <Conversion Name="g/s" Expression="x/100" />
    </Parameter>
    <Parameter Name="Throttle position" DefineBy="Pid" ByteCount="1" Address="0x11">
      <Conversion Name="%" Expression="(x*100)/255" />
    </Parameter>
    <Parameter Name="Ignition Timing" DefineBy="Pid" ByteCount="1" Address="0xE">
      <Conversion Name="degrees" Expression="(x/2)-64" />
    </Parameter>
  </ParameterGroup>
  <ParameterGroup Dpid="0xFD">
    <Parameter Name="Idle P term" DefineBy="Address" ByteCount="2" Address="0xFFA2A2">
      <Conversion Name="g/cyl" Expression="x/16.0" />
    </Parameter>
    <Parameter Name="Idle I term" DefineBy="Address" ByteCount="2" Address="0xFFA296">
      <Conversion Name="g/cyl" Expression="x/16.0" />
    </Parameter>
    <Parameter Name="Idle D term" DefineBy="Address" ByteCount="2" Address="0xFFA28C">
      <Conversion Name="g/cyl" Expression="(x-2000.0)/1000.0" />
    </Parameter>

  </ParameterGroup>
</LogProfile>