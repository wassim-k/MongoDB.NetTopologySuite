# MongoDB.NetTopologySuite.Serialization

## Overview
Serialize NetTopologySuite geospatial models to BSON and deserialize from BSON for MongoDB .NET C# Driver.

## Installation

### Install package
```bash
PM> Install-Package MongoDB.NetTopologySuite.Serialization
```

### Register serializers
Make sure serializers are registered before any MongoDB code is executed.
```csharp
BsonNetTopologySuiteSerializers.Register();
```

## Motivation
In projects that follow Onion Architecture or any other Domain centric architecture, the domain layer sits at the center of the architecture and has no dependencies on any of the other layers.
In such setup, the domain layer is agnostic to the persistance technology used and would not have a dependency on MongoDB C# Driver, instead it's the Infrastructure/Persistence layer that has a dependency on the Domain layer.  
In this scenario NetTopologySuite can be used in the Domain layer for defining geospatial types without violating Onion Architecture principles.  
This library allows for that decoupling by providing the required BSON serializers/deserializers.

[NetTopologySuite has also been chosen by EntityFramework as the standard for defining spatial types](https://learn.microsoft.com/en-us/ef/core/modeling/spatial)