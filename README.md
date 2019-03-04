# ServiceFabric-MultiPortsDemo
This example demonstrates the following concepts in Service Fabric
a) Host both http and tcp end points on a stateful service. The ports are chosen randomly [even the http port is random].
b) Demonstrates how a .net client can use "ServiceProxy" object to directly communicate with a particular partition on the tcp end point.
This example cna be expanded further to accommodate other .net clients like wpf or winforms