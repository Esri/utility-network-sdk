# ArcGIS Pro 2.1 Utility Network Pre-release SDK

The utility network in ArcGIS provides organizations that manage networks – like electric, telecom, gas, sewer, and water lines – an extensible solution that focuses on performance, scalability, interoperability, and data integrity. 

This repository describes the ArcGIS Pro SDK to use and extend the utility network.  The wiki contains documentation.  Code samples show how to author add-ins for ArcGIS Pro.

The SDK described here will evolve based upon user feedback and the evolution of the platform.

### Table of Contents

* [Samples](#samples)
* [Documentation](#documentation)
* [Requirements](#requirements)
* [Installing ArcGIS Pro SDK for .NET](#installing-arcgis-pro-sdk-for-net)
* [Resources](#resources)
* [Licensing](#licensing)


-------------------------
## Samples
This repository contains C# add-in samples for ArcGIS Pro 2.1.  These samples demonstrate the key functions of the utility network.

* [Electric Distribution Load Report (C#)](https://github.com/esri/utility-network-sdk/tree/master/LoadReportSample) 

*To compile ArcGIS Pro SDK samples using ArcGIS Pro 2.1 you must set the Target framework in your Visual Studio project properties to ".NET Framework 4.6.1."*

## Documentation
This repository contains two important documents.

- [Utility Network SDK Overview](https://github.com/esri/utility-network-sdk/blob/master/Utility%20Network%20SDK%20Overview.pdf).  This document describes the major classes inside the Utility Network SDK.  While the SDK is still evolving, this document is up-to-date with the SDK. 
- [Utility Network SDK.chm](https://github.com/esri/utility-network-sdk/blob/master/Utility%20Network%20SDK.chm).  This contains an API reference for this pre-release.  Utility network classes are contained within the `ArcGIS.Core.Internal.Data.UtilityNetwork` namespace (post-ship, they will move to `ArcGIS.Core.Data.UtilityNetwork`).  This API reference is not yet complete, and will evolve over time.  For classes outside of the utility network, we recommend you continue to use the [official documentation](http://pro.arcgis.com/en/pro-app/sdk/api-reference/#topic1.html).

In addition, the [wiki page](https://github.com/esri/utility-network-sdk/wiki) contains two additional documents:
- [ProConcepts: Utility Network](https://github.com/esri/utility-network-sdk/wiki/ProConcepts-Utility-Network).  This is the draft version of the conceptual documentation that will ship with the first release.
- [SDK Changes](https://github.com/esri/utility-network-sdk/wiki/SDK%20Changes).  For those partners who were part of our alpha or beta programs, this document describes what classes and methods changed with each pre-release.

## Requirements
The requirements and supported software needed to develop utility network add-ins.

#### ArcGIS Pro 
* ArcGIS Pro 2.1 Alpha 3
* ArcGIS Pro SDK for .Net

#### ArcGIS Enterprise
* ArcGIS Enterprise 10.6 Pre-release

#### Supported platforms

* Windows 10 (Home, Pro, Enterprise) (64 bit [EM64T])
* Windows 8.1 Professional, and Enterprise (64 bit [EM64T]) 
* Windows 7 SP1 Ultimate, Enterprise and Professional (64 bit [EM64T]) 

#### Supported .NET framework

* Microsoft .NET Framework 4.6.1 Developer Pack

#### Supported IDEs

* Visual Studio 2017 (Professional, Enterprise, and Community Editions)
* Visual Studio 2015 (Professional, Enterprise, and Community Editions) 

Note: [ArcGIS Pro system requirements](http://pro.arcgis.com/en/pro-app/get-started/arcgis-pro-system-requirements.htm) 

## Installing ArcGIS Pro SDK for .NET

ArcGIS Pro SDK for .NET can be downloaded and installed from within Visual Studio.

Read the [ProGuide: Installation and Upgrade](http://github.com/esri/arcgis-pro-sdk/wiki/ProGuide-Installation-and-Upgrade) for detailed installation instructions.

## Resources
* [Utility Network Early Adopter site](https://earlyadopter.esri.com/project/home.html?cap=2578B1991F9E43C7B114BD1BB37462C9)
* [Building add-ins for ArcGIS Pro](https://github.com/Esri/arcgis-pro-sdk/wiki/ProGuide-Build-your-first-add-in)
* [ArcGIS Pro 2.0 API Reference Guide](http://pro.arcgis.com/en/pro-app/sdk/api-reference/)

## Licensing
Copyright 2017 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at:

   http://www.apache.org/licenses/LICENSE-2.0.

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](./License.txt) file.





